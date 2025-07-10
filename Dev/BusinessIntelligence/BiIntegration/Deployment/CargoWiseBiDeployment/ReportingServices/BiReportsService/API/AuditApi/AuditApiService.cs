using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CargoWise.Bi.Deployment.ReportingServices
{
	// Takes DataTables from the AuditApiScriptRunner and handles converting them into DTOs
	public class AuditApiService : IAuditApiService
	{
		public AuditApiService()
		{
			scriptRunner = new AuditApiScriptRunner();
		}

		internal AuditApiService(AuditApiScriptRunner auditApiScriptRunner)
		{
			scriptRunner = auditApiScriptRunner;
		}

		readonly AuditApiScriptRunner scriptRunner;

		#region Change Summary

		public ChangeSummaryResponse GetChangedTablesList(string afterLsn)
		{
			var maxLsnBytes = scriptRunner.GetMaxLsn();
			var maxLsnHex = maxLsnBytes == null ? "0x00000000000000000000" : HexStringHelper.BytesToHexString(maxLsnBytes);

			var afterLsnBytes = HexStringHelper.HexStringToBytes(afterLsn);
			if (HexStringHelper.IsGreaterThan(afterLsnBytes, maxLsnBytes))
			{
				throw new AuditAPIException($"Parameter is invalid: The after_lsn value ({afterLsn}) provided is greater than the maxLsn value for this server ({maxLsnHex}).");
			}

			var tables = new List<ChangedTable>();
			var resultTable = scriptRunner.GetChangedTables(HexStringHelper.HexStringToBytes(afterLsn));
			foreach (DataRow row in resultTable.Rows)
			{
				tables.Add(new ChangedTable() { schemaName = (string)row[0], tableName = (string)row[1] });
			}

			return new ChangeSummaryResponse() { afterLsn = afterLsn, maxLsn = maxLsnHex, totalItems = tables.Count, items = tables.ToArray() };
		}

		#endregion

		#region Change Detail

		public ChangeDetailResponse GetChangeDetail(string schemaName, string tableName, string afterLsn, string afterSeqVal = "0xFFFFFFFFFFFFFFFFFFFF", int afterCommandId = int.MaxValue, int afterOperation = 4, string maxLsn = null, int pageSize = 1000)
		{
			maxLsn = ValidateMaxLsn(maxLsn);
			maxLsn = ValidateAfterLsnValue(afterLsn, maxLsn);
			var maxLsnBytes = HexStringHelper.HexStringToBytes(maxLsn);

			var response = new ChangeDetailResponse();
			response.schemaName = schemaName;
			response.tableName = tableName;
			response.pageSize = pageSize;

			var currentAfterLsn = HexStringHelper.HexStringToBytes(afterLsn);
			var currentAfterSeqVal = HexStringHelper.HexStringToBytes(afterSeqVal);
			var currentAfterCommandId = afterCommandId;
			var currentAfterOperation = afterOperation;
			var currentPageSize = pageSize;

			var changeDetails = new List<ChangeDetail>();

			while (currentPageSize > 0 && !HexStringHelper.IsGreaterThan(currentAfterLsn, maxLsnBytes))
			{
				var detail = GetChangeDetailAfterLsn(response, currentAfterLsn, currentAfterSeqVal, currentAfterCommandId, currentAfterOperation, currentPageSize);
				if (detail.schemaChangeCount > 0)
				{
					changeDetails.Add(detail);
					currentPageSize = currentPageSize - detail.schemaChangeCount;
					response.totalItems += detail.schemaChangeCount;

					var lastItem = detail.changes.Last();
					currentAfterLsn = HexStringHelper.HexStringToBytes(lastItem.start_lsn);
					currentAfterSeqVal = HexStringHelper.HexStringToBytes(lastItem.seqval);
					currentAfterCommandId = lastItem.command_id;
					currentAfterOperation = lastItem.operation;
				}
				else
				{
					currentAfterLsn = detail.max_lsn;
					currentAfterSeqVal = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
					currentAfterCommandId = int.MaxValue;
					currentAfterOperation = 4;

					// latest LSN has been reached
					if (HexStringHelper.BytesToHexString(currentAfterLsn) == maxLsn)
					{
						break;
					}
				}
			}

			response.lastItem = new LastItemDetail()
			{
				start_lsn = HexStringHelper.BytesToHexString(currentAfterLsn),
				seqval = HexStringHelper.BytesToHexString(currentAfterSeqVal),
				command_id = currentAfterCommandId,
				operation = currentAfterOperation
			};

			response.items = changeDetails.ToArray();
			return response;
		}

		ChangeDetail GetChangeDetailAfterLsn(ChangeDetailResponse response, byte[] afterLsnBytes, byte[] afterSeqVal, int afterCommandId, int afterOperation, int pageSize)
		{
			var changeDetail = new ChangeDetail();

#pragma warning disable CW1108 // Do Not Use DataSet
			var dataSet = scriptRunner.GetSchemaMappingDetails(response.schemaName, response.tableName, afterLsnBytes);
#pragma warning restore CW1108 // Do Not Use DataSet

			changeDetail.version = dataSet.Tables[0].Rows[0]["EffectiveSchemaVersion"].ToString();
			changeDetail.max_lsn = (byte[])dataSet.Tables[0].Rows[0]["EffectiveMaxLsn"];

			var columnDetails = new List<ColumnDetail>();
			var columnMapping = new List<string>();
			foreach (DataRow dataRow in dataSet.Tables[1].Rows)
			{
				var columnName = dataRow["ColumnName"].ToString();
				var mappedName = dataRow["MappedName"].ToString();

				columnMapping.Add(string.IsNullOrEmpty(mappedName) ? $"[{columnName}]" : $"[{mappedName}] AS [{columnName}]");

				columnDetails.Add(new ColumnDetail()
				{
					name = columnName,
					type = dataRow["DataType"].ToString(),
					maxLength = dataRow["MaxLength"] == DBNull.Value ? null : (int)dataRow["MaxLength"],
					precision = dataRow["Precision"] == DBNull.Value ? null : (int)dataRow["Precision"],
					scale = dataRow["Scale"] == DBNull.Value ? null : (int)dataRow["Scale"]
				});
			}
			changeDetail.columns = columnDetails.ToArray();

			if (!columnDetails.Any())
			{
				changeDetail.schemaChangeCount = 0;
				return changeDetail;
			}
			else
			{
				var changeData = scriptRunner.GetChangeData(response.schemaName, response.tableName, afterLsnBytes, changeDetail.max_lsn, columnMapping, afterSeqVal, afterCommandId, afterOperation, pageSize);
				changeDetail.changes = changeData.ToArray();
				changeDetail.schemaChangeCount = changeData.Count;

				return changeDetail;
			}
		}

		#endregion

		#region Validation

		string ValidateMaxLsn(string maxLsn)
		{
			var lastMaxLsnProcessed = scriptRunner.GetMaxLsn();
			if (lastMaxLsnProcessed != null)
			{
				if (string.IsNullOrEmpty(maxLsn) || HexStringHelper.IsGreaterThan(HexStringHelper.HexStringToBytes(maxLsn), lastMaxLsnProcessed))
				{
					return HexStringHelper.BytesToHexString(lastMaxLsnProcessed);
				}
				else
				{
					return maxLsn;
				}
			}
			return "0x00000000000000000000";
		}

		string ValidateAfterLsnValue(string afterLsn, string maxLsn)
		{
			var maxLsnBytes = HexStringHelper.HexStringToBytes(maxLsn);
			var afterLsnBytes = HexStringHelper.HexStringToBytes(afterLsn);
			if (HexStringHelper.IsGreaterThan(afterLsnBytes, maxLsnBytes))
			{
				throw new AuditAPIException($"Parameter is invalid: The after_lsn value ({afterLsn}) provided is greater than the maxLsn value for this server ({maxLsn}).");
			}

			return maxLsn;
		}

		#endregion

		#region Hex Comparison

		#endregion
	}
}
