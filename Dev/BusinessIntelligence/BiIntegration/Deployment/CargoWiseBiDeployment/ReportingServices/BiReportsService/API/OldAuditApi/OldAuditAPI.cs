using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Registry.Business;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Bi.Deployment.ReportingServices.API.OldAuditApi
{
	public class OldAuditAPI : BIAPI
	{
		[ThreadSafe]
		static readonly Lazy<OldAuditAPI> lazy = new Lazy<OldAuditAPI>(() => new OldAuditAPI());

		public static OldAuditAPI Instance => lazy.Value;

		OldAuditAPI()
		{
		}

		[ThreadSafe]
		public override bool IsAPIEnabled
		{
			get
			{
				using (Db.DisposableActionForDbConnection())
				{
					return SystemDataRegistry.Instance.BiAuditAPI.Value;
				}
			}
		}

		public override void VerifyAPIIsEnabled()
		{
			if (!IsAPIEnabled)
			{
				throw new AuditAPIException(Res.GetString("2baaf43d-c3bd-4e5b-a3d4-ca1d9585ea4a", "The Audit Web API has not been enabled on this system."));
			}
		}

		#region SuppressResourceStringsCheckRegion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public DataTable GetCdcHistorySummaryDetails(ZDateTime utcFromDateTime, ZDateTime utcToDateTime, int batchSize = 0)
		{
			ValidateParameters(utcFromDateTime, utcToDateTime);
			VerifyAPIIsEnabled();

			var sqlText = string.Format(CultureInfo.InvariantCulture, "{0}.usp_GetCdcHistorySummaryDetails", BIAPIServiceConstants.BIADMIN);
			using (var auditConnection = Db.NewExtraConnectionWithMainDbCredentials(AuditServer, Db.AuditDatabaseName))
			using (var cmd = auditConnection.Command(sqlText))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				if (!utcFromDateTime.IsEmpty)
				{
					cmd.AddParameter("@fromDateTime", SqlDbType.DateTime, utcFromDateTime);
					cmd.AddParameter("@fromLsnPeriod", SqlDbType.SmallInt, GetLsnPeriodByDate(utcFromDateTime));
				}
				if (!utcToDateTime.IsEmpty)
				{
					cmd.AddParameter("@toDateTime", SqlDbType.DateTime, utcToDateTime);
					cmd.AddParameter("@toLsnPeriod", SqlDbType.SmallInt, GetLsnPeriodByDate(utcToDateTime));
				}
				if (batchSize == 0)
				{
					cmd.AddParameter("@batchSize", SqlDbType.Int, APIBatchSize);
				}
				else
				{
					cmd.AddParameter("@batchSize", SqlDbType.Int, batchSize);
				}

				return DataUtils.GetDataTableFromCommand(cmd);
			}
		}

		static void ValidateParameters(ZDateTime utcFromDateTime, ZDateTime utcToDateTime)
		{
			if (utcFromDateTime >= utcToDateTime)
			{
				throw new AuditAPIException("to_time must be greater than from_time.");
			}
		}

		public int GetLsnPeriodByDate(ZDateTime inputDate)
		{
			var year = (inputDate.Year % 100).ToString("D2");
			var month = inputDate.Month.ToString("D2");
			return int.Parse(year + month);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public DataTable GetCdcHistorySummaryDetailsAfterLsn(string lsn, int batchSize = 0)
		{
			VerifyAPIIsEnabled();
			var lsnStr = "0x0";
			var startLsn = string.IsNullOrEmpty(lsn) ? HexStringToHex(lsnStr.Replace("0x", string.Empty)) : HexStringToHex(lsn.Replace("0x", string.Empty));
			var sqlText = string.Format(CultureInfo.InvariantCulture, "{0}.usp_GetCdcHistorySummaryDetailsAfterLsn", BIAPIServiceConstants.BIADMIN);

			using (var auditConnection = Db.NewExtraConnectionWithMainDbCredentials(Instance.AuditServer, Db.AuditDatabaseName))
			using (var cmd = auditConnection.Command(sqlText))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@lsn", SqlDbType.Binary, startLsn);
				if (batchSize == 0)
				{
					cmd.AddParameter("@batchSize", SqlDbType.Int, APIBatchSize);
				}
				else
				{
					cmd.AddParameter("@batchSize", SqlDbType.Int, batchSize);
				}
				return DataUtils.GetDataTableFromCommand(cmd);
			}
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "not possible to use a factory, hitting Audit DB, SQL Param")]
		public DataTable GetCdcChanges(string lsn, int batch, string schema, string tableName, int batchSize = 0)
		{
			VerifyAPIIsEnabled();
			const string lsnStr = "0x0";
			var startLsn = string.IsNullOrEmpty(lsn) ? HexStringToHex(lsnStr.Replace("0x", string.Empty)) : HexStringToHex(lsn.Replace("0x", string.Empty));
			var sqlText = string.Format(CultureInfo.InvariantCulture, "{0}.usp_GetCdcChanges", BIAPIServiceConstants.BIADMIN);
			using (var auditConnection = Db.NewExtraConnectionWithMainDbCredentials(Instance.AuditServer, Db.AuditDatabaseName))
			using (var cmd = auditConnection.Command(sqlText))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				if (batch >= 0)
				{
					cmd.AddParameter("@schema", SqlDbType.VarChar, schema);
					cmd.AddParameter("@tableName", SqlDbType.VarChar, tableName);
					cmd.AddParameter("@lsn", SqlDbType.Binary, startLsn);
					cmd.AddParameter("@batch", SqlDbType.Int, batch - 1);
					if (batchSize == 0)
					{
						cmd.AddParameter("@batchSize", SqlDbType.Int, APIBatchSize);
					}
					else
					{
						cmd.AddParameter("@batchSize", SqlDbType.Int, batchSize);
					}

					var dataTable = DataUtils.GetDataTableFromCommand(cmd);
					return dataTable;
				}
				else
				{
					return new DataTable() { Locale = CultureInfo.InvariantCulture };
				}
			}
		}

		byte[] HexStringToHex(string hexString)
		{
			var hexBytes = new byte[hexString.Length / 2];
			for (var i = 0; i < hexBytes.Length; i++)
			{
				hexBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
			}
			return hexBytes;
		}
	}
}
