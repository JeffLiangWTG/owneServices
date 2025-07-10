using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;

namespace Enterprise.ComplianceRisk.Business
{
	public class SupportedCountriesHelper
	{
		const string ComplianceSupportedCountriesDbCacheKey = "ComplianceSupportedCountriesDbCache";

		readonly object _lock = new();
		readonly BorderWiseApiHelper borderWiseApiHelper;

		public SupportedCountriesHelper(BorderWiseApiHelper borderWiseApiHelper)
		{
			this.borderWiseApiHelper = borderWiseApiHelper;
		}

		public async Task<SupportedCountriesCheckResponseModel> GetSupportedCountriesCheckResponseModelAsync(CancellationToken cancellationToken, BusinessObjectFactory factory)
		{
			var modelInDb = GetSupportedCountriesStmData(factory);

			var hours = (ZDateTime.UtcNow.ToDateTime() - modelInDb.LastModifyUtcTime).TotalHours;

			if (hours <= 24 && modelInDb.SupportedCountries != null)
			{
				return modelInDb.SupportedCountries;
			}

			SupportedCountriesCheckResponseModel modelFromBW = null;

			try
			{
				modelFromBW = await borderWiseApiHelper.SupportedCountriesCheckAsync(cancellationToken);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (!cancellationToken.IsCancellationRequested)
				{
					DeniedPartyScreenerAsync.WriteMessage($"\r\nFailed to call the BorderWise API and get supported countries, exception: {ex}\r\n");

					if (ex is ComplianceCheckException && hours > 240 && !modelInDb.ErrorReported)
					{
						modelInDb.ErrorReported = true;
						SetSupportedCountries(modelInDb);

						ErrorReporter.ReportDeveloperExceptionOnce("Failed to call the BorderWise API and get supported countries", ex);
					}

					if (modelInDb.SupportedCountries == null)
					{
						throw new ComplianceCheckException("Supported Countries Are Empty.");
					}
				}
			}

			if (modelFromBW != null)
			{
				modelInDb.ErrorReported = false;
				modelInDb.SupportedCountries = modelFromBW;
				SetSupportedCountries(modelInDb);
				return modelFromBW;
			}

			return modelInDb.SupportedCountries;
		}

		internal static SupportedCountriesCheckResponseModelWithReportedFlag GetSupportedCountriesStmData(BusinessObjectFactory factory)
		{
			try
			{
				var query = new ZQuery(StmDataSchema.SD_Name, ComplianceSupportedCountriesDbCacheKey);
				var stmData = factory.LoadTop1<StmData>(query);

				if (stmData != null)
				{
					var supportedCountries = JsonConvert.DeserializeObject<SupportedCountriesCheckResponseModelWithReportedFlag>(Encoding.UTF8.GetString(stmData.SD_BinaryValue));

					if (supportedCountries != null)
					{
						return supportedCountries;
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				// Ignore
			}

			return new SupportedCountriesCheckResponseModelWithReportedFlag
			{
				ErrorReported = false,
				SupportedCountries = null,
				LastModifyUtcTime = DateTime.MinValue,
			};
		}

		protected void SetSupportedCountries(SupportedCountriesCheckResponseModelWithReportedFlag supportedCountries)
		{
			lock (_lock)
			{
				var factory = new BusinessObjectFactory();

				var query = new ZQuery(StmDataSchema.SD_Name, ComplianceSupportedCountriesDbCacheKey);
				var stmRow = factory.LoadTop1<StmData>(query);

				if (stmRow == null)
				{
					stmRow = factory.New<StmData>();
					stmRow.SD_Name = ComplianceSupportedCountriesDbCacheKey;
				}

				supportedCountries.LastModifyUtcTime = ZDateTime.UtcNow.ToDateTime();
				stmRow.SD_BinaryValue = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(supportedCountries));

				try
				{
					factory.Save();
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					// ignore
				}
			}
		}
	}
 
	public class SupportedCountriesCheckResponseModelWithReportedFlag
	{
		public SupportedCountriesCheckResponseModel SupportedCountries { get; set; }

		public DateTime LastModifyUtcTime { get; set; }

		public bool ErrorReported { get; set; }
	}
}
