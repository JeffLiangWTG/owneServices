using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	public class PrintQueueReplacer : SaveInTransactionActionWithMainConnection
	{
		public PrintQueueReplacer(PrintQueueReplaceBizo printQueueReplaceBizo)
		{
			this.printQueueReplaceBizo = printQueueReplaceBizo;
		}

		protected override IChangedTableNames SaveInTransaction()
		{
			if (printQueueReplaceBizo != null)
			{
				ReplacePrintQueueReferenced(AccChequeBookSchema.AK_SQ);
				ReplacePrintQueueReferenced(AccComplianceSequenceSchema.XD_SQ_DocumentPrintQueue);
				ReplacePrintQueueReferenced(ProcessTaskNotificationSchema.PQ_SQ);
				ReplacePrintQueueReferenced(StmPrintJobSchema.SP_SQ);
				ReplacePrintQueueReferenced(StmPrintJobQueueSchema.SPQ_SQ_PrintQueue);
				ReplacePrintQueueReferenced(StmScheduleTaskRecipientSchema.S6_SQ);
				ReplacePrintQueueReferenced(StmDefaultPrinterSchema.SDP_SQ_Printer);
				ReplacePrintQueueReferenced(WhsLocationSchema.WL_SQ_DefaultPrintQueue);
				ReplacePrintQueueReferenced(WhsRFRegistrySchema.WRR_SQ_Printer);
			}

			return ChangedTableNames.Empty;
		}

		protected override void OnAllTransactionsCommitted(IChangedTableNames changedTableNames)
		{
			if (printQueueReplaceBizo != null)
			{
				printQueueReplaceBizo.Factory.SaveInTransactionActions.Remove(this);
			}
		}

		void ReplacePrintQueueReferenced(SchemaColumn referenceColumn)
		{
			var sql = FormattableString.Invariant($@"UPDATE {referenceColumn.TableName} SET {referenceColumn.Name} = @newPrinter,
{referenceColumn.ColumnPrefix}_SystemLastEditTimeUtc = GETUTCDATE(),
{referenceColumn.ColumnPrefix}_SystemLastEditUser = @SystemLastEditUser
WHERE {referenceColumn.Name} = @oldPrinter");
			using (var cmd = Connection.Command(sql))
			{
				cmd.AddParameter("@newPrinter", referenceColumn.SqlDbType, printQueueReplaceBizo.ReplacePrintQueuePK.ToGuid());
				cmd.AddParameter("@oldPrinter", referenceColumn.SqlDbType, printQueueReplaceBizo.PrintQueuePK.ToGuid());
				cmd.AddParameterBasedOnDbColumn("@SystemLastEditUser", GlbStaff.CurrentUser.GS_Code.ToString(), GlbStaffSchema.GS_Code);
				cmd.ExecuteNonQuery();
			}
		}

		DbConnection connection;
		DbConnection Connection => connection ?? (connection = Db.Connection);

		readonly PrintQueueReplaceBizo printQueueReplaceBizo;
	}
}
