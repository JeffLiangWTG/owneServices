using System;
using System.Globalization;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.ChangeDataCapture.Common;

namespace CargoWise.Bi.Product.Manager.Business
{
	public class CdcInformation
	{
		#region SuppressResourceStringsCheckRegion

		public void RefreshInfo()
		{
			using (Db.DisposableActionForDbConnection())
			{
				ServerName = Db.ServerName;
				DatabaseName = Db.DatabaseName;

				CdcErrors = new CdcErrorCollection();
				RetrieveCdcErrors(Db.Connection);

				IsCdcEnabled = CdcDatabase.IsEnabled(Db.Connection, DatabaseName);
				NumberOfCdcEnabledTables = GetNumberOfCdcEnabledTables();
				DateOfEarliestScannedTransaction = GetDateOfEarliestScannedTransaction();
				DateOfLastScannedTransaction = GetDateOfLastScannedTransaction();
				DateOfLastCdcScan = GetDateOfLastCdcScan();
				HasCdcErrors = GetHasCdcErrors();

				if (IsCdcEnabled)
				{
					CdcSchemaErrors = "Querying...";
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void RetrieveCdcErrors(DbConnection biConnection)
		{
			var sqlText = "SELECT * FROM sys.dm_cdc_errors";
			using (var reader = biConnection.Command(sqlText).ExecuteReader())
			{
				while (reader.Read())
				{
					var cdcError = new CdcError();

					cdcError.session_id = new ZInt(reader["session_id"]);
					cdcError.phase_number = new ZInt(reader["phase_number"]);
					cdcError.entry_time = new ZDateTime(reader["entry_time"]);
					cdcError.error_number = new ZInt(reader["error_number"]);
					cdcError.error_severity = new ZInt(reader["error_severity"]);
					cdcError.error_state = new ZInt(reader["error_state"]);
					cdcError.error_message = reader["error_message"].ToString();
					cdcError.start_lsn = reader["start_lsn"].ToString();
					cdcError.begin_lsn = reader["begin_lsn"].ToString();
					cdcError.sequence_value = reader["sequence_value"].ToString();

					CdcErrors.Add(cdcError);
				}
			}
		}

		public ZString ServerName { get; private set; }
		public ZString DatabaseName { get; private set; }
		public bool IsCdcEnabled { get; private set; }
		public ZString IsCdcEnabledText { get => IsCdcEnabled ? "Yes" : "No"; }
		public int NumberOfCdcEnabledTables { get; private set; }

		public ZDateTime? DateOfEarliestScannedTransaction { get; private set; }
		public ZString DateOfEarliestScannedTransactionText { get => DateOfEarliestScannedTransaction != null ? DateOfEarliestScannedTransaction.Value.ToBestReadableDateTimeString() : ""; }

		public ZDateTime? DateOfLastScannedTransaction { get; private set; }
		public ZString DateOfLastScannedTransactionText { get => DateOfLastScannedTransaction != null ? DateOfLastScannedTransaction.Value.ToBestReadableDateTimeString() : ""; }

		public ZDateTime? DateOfLastCdcScan { get; private set; }
		public ZString DateOfLastCdcScanText { get => DateOfLastCdcScan != null ? DateOfLastCdcScan.Value.ToBestReadableDateTimeString() : ""; }

		public bool HasCdcErrors { get; private set; }
		public ZString HasCdcErrorsText { get => HasCdcErrors ? "Yes – check CDC service task logs for details" : "No"; } // CDC Information always in English
		public ZString CdcSchemaErrors { get; private set; }
		public CdcErrorCollection CdcErrors { get; private set; }

		int GetNumberOfCdcEnabledTables()
		{
			return Convert.ToInt32(Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM sys.tables WHERE is_ms_shipped = 0 AND is_tracked_by_cdc = 1"), CultureInfo.InvariantCulture);
		}

		#region CDC scan dates

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		ZDateTime? GetDateOfLastScannedTransaction()
		{
			ZDateTime? result = null;
			if (IsCdcEnabled)
			{
				var sqlText = "SELECT TOP 1 tran_end_time FROM cdc.lsn_time_mapping WHERE tran_id <> 0x0 ORDER BY tran_end_time DESC";
				using (var reader = Db.Connection.Command(sqlText).ExecuteReader())
				{
					while (reader.Read())
					{
						var dateObj = reader["tran_end_time"];
						if (dateObj != DBNull.Value)
						{
							result = new ZDateTime(dateObj);
						}
					}
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		ZDateTime? GetDateOfLastCdcScan()
		{
			ZDateTime? result = null;
			if (IsCdcEnabled)
			{
				var sqlText = "SELECT TOP 1 tran_end_time FROM cdc.lsn_time_mapping ORDER BY tran_end_time DESC";
				using (var reader = Db.Connection.Command(sqlText).ExecuteReader())
				{
					while (reader.Read())
					{
						var dateObj = reader["tran_end_time"];
						if (dateObj != DBNull.Value)
						{
							result = new ZDateTime(dateObj);
						}
					}
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		ZDateTime? GetDateOfEarliestScannedTransaction()
		{
			ZDateTime? result = null;
			if (IsCdcEnabled)
			{
				var sqlText = "SELECT TOP 1 tran_end_time FROM cdc.lsn_time_mapping WHERE tran_id <> 0x0 ORDER BY tran_end_time ASC";
				using (var reader = Db.Connection.Command(sqlText).ExecuteReader())
				{
					while (reader.Read())
					{
						var dateObj = reader["tran_end_time"];
						if (dateObj != DBNull.Value)
						{
							result = new ZDateTime(dateObj);
						}
					}
				}
			}

			return result;
		}

		#endregion

		#region CDC Errors

		bool GetHasCdcErrors()
		{
			bool result = false;
			if (IsCdcEnabled)
			{
				var sqlText = @"
IF EXISTS (SELECT NULL FROM sys.dm_cdc_log_scan_sessions)
	SELECT TOP 1 error_count FROM sys.dm_cdc_log_scan_sessions ORDER BY session_id DESC
ELSE
	SELECT 0";

				var errorCount = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture);
				result = errorCount != 0;
			}
			return result;
		}

		#endregion

		public void PopulateCdcSchemaErrors()
		{
			using (var cdcConnection = Db.NewExtraUnrestrictedWriterConnection(Db.ServerName, Db.DatabaseName))
			{
				CdcSchemaErrors = CdcSchemaMismatchChecker.GetCdcSchemaMismatchLogs(cdcConnection);
			}
		}

		#endregion
	}
}
