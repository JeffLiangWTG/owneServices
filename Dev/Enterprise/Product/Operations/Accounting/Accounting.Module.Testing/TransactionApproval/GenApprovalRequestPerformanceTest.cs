using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module.TransactionApproval.Testing
{
	public class GenApprovalRequestPerformanceTest : TestCaseWithFactory
	{
		public void TestIndexUsedForJobNumFilter()
		{
			AssertIndexUsedForJobNumFilter(() =>
			{
				PrepareDataForJobNumFilter(2, GlbBranch.CurrentBranch.PK, Core.Constants.GenApprovalRequestApprovalType.APInvoiceCharges);
				PrepareDataForJobNumFilter(1000, GlbBranch.CurrentBranch.PK, Core.Constants.GenApprovalRequestApprovalType.ARCreditNote);
			});
		}

		void PrepareDataForJobNumFilter(int totalCount, ZGuid requestingBranch,
			string approvalType = Core.Constants.GenApprovalRequestApprovalType.APInvoiceCharges,
			string subSystem = Core.Constants.GenApprovalRequestSubSystem.Accounting)
		{
			var sqlText = $@"
DECLARE @TotalCount int = 1
DECLARE @TransactionCount int = (SELECT COUNT(*) FROM dbo.AccTransactionHeader)

WHILE @TotalCount <= {totalCount}
BEGIN

	SET @TransactionCount = @TransactionCount + 1;
	DECLARE @AH_PK uniqueidentifier = NEWID();

	INSERT INTO dbo.AccTransactionHeader
		(AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_InvoiceDate, AH_DueDate, AH_PostToGL, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_SystemLastEditTimeUtc, AH_SystemLastEditUser)
	VALUES
		(@AH_PK, 'AP', 'INV', 'INV' + CAST(@TransactionCount AS varchar(10)), GETDATE(), GETDATE(), 'Y', 100.0000, 100.0000, 'CNY', 1, GETDATE(), '{TestObjectCreator.Debtor.PK}', '{GlbCompany.CurrentCompany.PK}', '{GlbBranch.CurrentBranch.PK}', '{GlbDepartment.CurrentDepartment.PK}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

	INSERT INTO dbo.GenApprovalRequest
		(XP_PK, XP_GB_RequestingBranch, XP_ApprovalType, XP_SubSystem, XP_ApprovalStatus, XP_ApprovalDate, XP_ParentID, XP_ParentTableCode, XP_RequestID, XP_ReasonDescription, XP_SystemCreateUser, XP_SystemCreateTimeUtc)
	VALUES
		(NEWID(), '{requestingBranch}', '{approvalType}', '{subSystem}', 'REQ', GETDATE(), @AH_PK, 'AH', '', '', '~BP', GETDATE())

	SET @TotalCount = @TotalCount + 1;

END
";
			Db.Connection.ExecuteNonQuery(sqlText);
		}

		void AssertIndexUsedForJobNumFilter(Action prepareDataAction)
		{
			Factory.Load<AccTransactionHeader>(new ZQuery()).DeleteAll();
			Factory.Load<GenApprovalRequest>(new ZQuery()).DeleteAll();
			Factory.Save();

			prepareDataAction();
			Factory.Save();

			var filterBO = new InvoicingBaseApprovalFilterBusinessObject();

			var filter = (ModuleTextFilter)filterBO["Job #"];
			filter.IsActive = true;
			filter.Property = "INV1";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			var collection = new APInvoiceChargesApprovalRequestCollection(Factory.CreateNewFactory());
			collection.AdditionalFilter = filterBO.Filter;

			Db.Connection.ExecuteNonQuery($"UPDATE STATISTICS {GenApprovalRequestSchema.Constants.TableName} WITH FULLSCAN");

			using (Db.Connection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				AssertEquals(1, collection.Count);

				var queryPlan = Db.Connection.ExecutedCommandsAndQueryPlans.First(t => t.Item1.Contains(GenApprovalRequestSchema.Constants.XP_GB_RequestingBranch));
				var queryPlanAnalyzer = new QueryPlanalyzer(queryPlan.Item2.First());

				AssertEquals("Should not contain any TableScan.", false, queryPlanAnalyzer.TableScans.Any());
				AssertEquals(true, queryPlanAnalyzer.IndexSeeks.Any(x => x.IndexName == "FK_RX__XP_GB_RequestingBranch_XP_ApprovalType_XP_SubSystem"));
			}
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
