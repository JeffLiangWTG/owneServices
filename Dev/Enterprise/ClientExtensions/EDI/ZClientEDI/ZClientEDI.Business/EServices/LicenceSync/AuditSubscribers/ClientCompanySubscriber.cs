using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using CargoWise.Schema;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.EServices.LicenceSync.AuditSubscribers
{
	public class ClientCompanySubscriber : ActualDataChangesAuditSubscriber
	{
		public override string Code => "LSC";

		public override string Description => nameof(ClientCompanySubscriber);

		public override ITableSchema Table => ClientCompanySchema.Instance;

		public override IEnumerable<SchemaColumn> SpecificColumns => new SchemaColumn[]
		{
			ClientCompanySchema.PK,
			ClientCompanySchema.LCC_Code,
			ClientCompanySchema.LCC_LD,
			ClientCompanySchema.LCC_RN_NKCountryCode,
			ClientCompanySchema.LCC_CodeValidFromUtc,
		};

		public override Action<DataRow> CustomFilter => null;

		public override bool NotifyInsert => true;

		public override bool NotifyUpdate => true;

		public override bool NotifyDelete => true;

		public override bool IsRequired() => true;

		public override void ProcessChanges(ILogger logger, DataTable table)
		{
			logger.Log(LogType.Information, $"Started processing changes for '{Table.TableName}' with '{table.Rows.Count}' changed rows.");

			foreach (DataRow row in table.Rows)
			{
				switch (row.RowState)
				{
					case DataRowState.Added:
						LogRow(logger, row, new DataRowVersion[] { DataRowVersion.Current });
						break;

					case DataRowState.Modified:
						LogRow(logger, row, new DataRowVersion[] { DataRowVersion.Original, DataRowVersion.Current });
						break;

					case DataRowState.Deleted:
						LogRow(logger, row, new DataRowVersion[] { DataRowVersion.Original });
						break;

					case DataRowState.Unchanged:
					case DataRowState.Detached:
						logger.Log(LogType.Debug, $"Ignored row with state '{row.RowState}'.");
						break;

					default:
						throw new InvalidOperationException($"Invalid state '{row.RowState}' of row in table '{Table.TableName}'.");
				}
			}

			logger.Log(LogType.Information, $"Finished processing changes for '{Table.TableName}'.");
		}

		void LogRow(ILogger logger, DataRow row, DataRowVersion[] rowVersions)
		{
			var rowChange = new
			{
				Table = Table.TableName,
				RowState = row.RowState.ToString(),
				RowVersions = rowVersions.Select(rowVersion => new
				{
					RowVersion = rowVersion.ToString(),
					Columns = SpecificColumns.ToDictionary(
						column => column.Name,
						column => Convert.ToString(row[column.Name, rowVersion])),
					AuditFields = GetAuditFields(row, rowVersion)
				}).ToList()
			};

			var jsonLog = JsonSerializer.Serialize(rowChange, new JsonSerializerOptions { WriteIndented = true });

			logger.Log(LogType.Debug, jsonLog);
		}

		object GetAuditFields(DataRow row, DataRowVersion rowVersion)
		{
			return new
			{
				StartLsn = BitConverter.ToString((byte[])row[AuditFieldNames.StartLsnFieldName, rowVersion]),
				SeqVal = BitConverter.ToString((byte[])row[AuditFieldNames.SeqValFieldName, rowVersion]),
				CommandId = Convert.ToInt32(row[AuditFieldNames.CommandIdFieldName, rowVersion]),
				Operation = Convert.ToInt32(row[AuditFieldNames.OperationFieldName, rowVersion])
			};
		}
	}
}
