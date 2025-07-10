using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Schema;
using Enterprise.BufferManagement.Service.Client;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.PAVE.Subscribers
{
	public class ProcessHeaderResponsiveTransferSubscriber : PAVEServiceTaskNudgingSubscriber
	{
		#region Properties

		public override string Code => "PTH";

		public override ITableSchema Table => ProcessHeaderSchema.Instance;

		public override bool IsRequired() => true;

		public override bool NotifyInsert => true;

		public override bool NotifyUpdate => true;

		public override bool NotifyDelete => false;

		public override string Description => nameof(ProcessHeaderResponsiveTransferSubscriber);

		public override IEnumerable<SchemaColumn> SpecificColumns => null;

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			if (row[ProcessHeaderSchema.Constants.FH_FC_CurrentComponent] == DBNull.Value
				|| row[ProcessHeaderSchema.FH_FH_ParentHeader.Name] == DBNull.Value
				|| (row[ProcessHeaderSchema.FH_P0_Template.Name] != DBNull.Value && Convert.ToString(row[ProcessHeaderSchema.FH_P0_Template.Name]).Length > 0))
			{
				row.Delete();
			}
		};

		#endregion

		#region ProcessChange

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log Message")]
		public override void ProcessChanges(ILogger logger, DataTable changeTable)
		{
			var isEnabled = BMSRegistry.BufferManagementEnabled && BMSRegistry.EnableResponsivePAVEDataProcessing && BMSRegistry.TransferWorkflowComponentOnChanges;

			if (!isEnabled)
			{
				logger.Log(LogType.Debug, $"{changeTable.Rows.Count} changes were skipped as the {Code} subscriber is disabled.");
				return;
			}

			if (changeTable.Rows.Count > BMSRegistry.TransferWorkflowComponentMaximumNumberOfCDCChanges) //that should be distinct?
			{
				logger.Log(LogType.Warning, $"Number of changes: {changeTable.Rows.Count} greater than {nameof(BMSRegistry.TransferWorkflowComponentMaximumNumberOfCDCChanges)}: {BMSRegistry.TransferWorkflowComponentMaximumNumberOfCDCChanges}; Workflow Transfers will not be processed responsively and will be deferred to the Buffer Management Schematic Transfer Runner service task."); // Log Message

				BMSRegistry.ProcessAllTransferRulesLinksOnNextBMSRun = true;
				MaybeNudgeServiceTask(logger,
					shouldNudge: BMSRegistry.EnableNudgingBMSServiceTaskByThePVEServiceTask,
					serviceTaskCode: "BMS",
					delay: BMSRegistry.DelayForNudgingTheBMSServiceTaskByThePVEServiceTask);
				return;
			}

			var rowTransEndTime = changeTable.Rows.Cast<DataRow>().Select(change => new
			{
				Row = change,
				TransactionEndTimeUtc = DateTime.SpecifyKind((DateTime)change["TranEndTimeUtc"], DateTimeKind.Utc)
			}).ToArray();

			var pks = rowTransEndTime
				.Where(rt => ShouldProcessChangeForTransfer(rt.Row))
				.Select(change => (Guid)change.Row[ProcessHeaderSchema.Constants.PK])
				.Distinct()
				.ToArray();
			var schematicServiceClient = ObjectFactory.Get<IPAVEServiceClientFactory>().GetSchematicServiceClient();
			const int processTransferRulesBatchSize = 1024; //TODO: Move to registry?

			try
			{
				foreach (var batch in pks.Batch(processTransferRulesBatchSize))
				{
					schematicServiceClient.ProcessTransferRules(batch, logger);
				}
			}
			catch (Exception e)
			{
				logger.Warning(string.Format(CultureInfo.InvariantCulture, "CDC updates failed for {0}, Error: {1})", Table.TableName, e.ToString()));
			}
		}

		#endregion

		#region ResponsiveTransfer

		public const string ManualTransferCode = "MAN";

		static bool ShouldProcessChangeForTransfer(DataRow row)
		{
			if (!(bool)row[ProcessHeaderSchema.Constants.FH_IsActive])
			{
				return false;
			}

			if (row.RowState != DataRowState.Modified)
			{
				return true;
			}

			var changedColumns = row.Table.Columns.Cast<DataColumn>().Where(col => HasCellChanged(row, col)).ToArray();

			if (changedColumns.Length != 0 && changedColumns.All(c => c.ColumnName == "FH_AutoVersion"))
			{
				return false;
			}

			if (changedColumns.Length == 3 && changedColumns.All(c =>
				c.ColumnName == ProcessHeaderSchema.Constants.FH_FC_DedicatedBuffer
				|| c.ColumnName == ProcessHeaderSchema.Constants.FH_SystemLastEditTimeUtc
				|| c.ColumnName == ProcessHeaderSchema.Constants.FH_SystemLastEditUser))
			{
				return false;
			}

			var originalComponent = row[ProcessHeaderSchema.Constants.FH_FC_CurrentComponent, DataRowVersion.Original];
			var currentComponent = row[ProcessHeaderSchema.Constants.FH_FC_CurrentComponent, DataRowVersion.Current];

			if (originalComponent.Equals(currentComponent))
			{
				return true;
			}

			var lastTransferType = row[ProcessHeaderSchema.Constants.FH_LastTransferType].ToString();
			if (lastTransferType == ManualTransferCode)
			{
				return true;
			}

			var excludedColumns = new HashSet<string> {
				ProcessHeaderSchema.Constants.FH_FC_CurrentComponent,
				ProcessHeaderSchema.Constants.FH_LastTransferType,
				ProcessHeaderSchema.Constants.FH_ReleaseDateTime,
				ProcessHeaderSchema.Constants.FH_SystemLastEditUser,
				"TranEndTimeUtc",
			};

			return changedColumns.Any(column => !excludedColumns.Contains(column.ColumnName));
		}

		#endregion
	}
}
