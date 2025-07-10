using System.Collections.Generic;
using System.Linq;

namespace CargoWise.Bi.Registration.PowerBi
{
	public enum BiReportType
	{
		Paginated,
		NonPaginated
	}
	public abstract class PowerBiItem
	{
		public abstract string Name { get; }
		public abstract string DataSourceModel { get; }

		public virtual string Id { get; set; }

		public abstract BiReportCategory Category { get; }

		public string BusinessArea
		{
			get
			{
				return GetType().Namespace.Split('.').Last();
			}
		}

		public virtual BiReportType ReportType => BiReportType.NonPaginated;

		public string ResourceName
		{
			get
			{
				if (IsDataset)
				{
					return datasetNamespace + "." + Name + Extension;
				}
				else
				{
					return powerbiNamespace + ".ReportFiles." + BusinessArea + "." + Name + Extension;
				}
			}
		}

		public virtual bool IsDataset => false;

		public virtual bool DeployToClients => false;

		public string ResourceType
		{
			get
			{
				if (IsDataset)
				{
					return WebAPIDataset;
				}
				else
				{
					switch (ReportType)
					{
						case BiReportType.Paginated:
							return Ssrs;
						default:
							return PowerBi;
					}
				}
			}
		}

		public string Extension
		{
			get
			{
				if (IsDataset)
				{
					return DatasetExtension;
				}
				else
				{
					return ReportType == BiReportType.NonPaginated ? PbixExtension : RdlExtension;
				}
			}
		}

		public virtual bool ShouldSessionBePerpetualInGLOW => false;

		public string powerbiNamespace => "CargoWise.Bi.Registration.PowerBi";
		public string datasetNamespace => "CargoWise.Bi.Registration.WebAPIDataset";

		public const string RdlExtension = ".rdl";
		public const string PbixExtension = ".pbix";
		public const string DatasetExtension = ".rsd";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "URL Report Type Constants")]
		public const string PowerBi = "powerbi";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "URL Report Type Constants")]
		public const string Ssrs = "report";
		public const string WebAPIDataset = "WebAPIDataset";

		public virtual Dictionary<string, string> GetQueryStringFilters(string companyCode, string countryCode, string branchCode)
		{
			return null;
		}
	}
}
