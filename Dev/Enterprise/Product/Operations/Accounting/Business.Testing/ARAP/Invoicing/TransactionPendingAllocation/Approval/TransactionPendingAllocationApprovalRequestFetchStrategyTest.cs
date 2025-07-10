using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ARAP.Invoicing.TransactionPendingAllocation.Approval
{
	public class TransactionPendingAllocationApprovalRequestFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForView()
		{
			var ownColumnNames = new string[]
			{
				"XP_ApprovalDate",
				"XP_ApprovalStatus",
				"XP_GB_RequestingBranch",
				"XP_GS_NKApprovingUser1",
				"XP_ReasonDescription",
			};

			var transactionColumnNames = new string[]
			{
				"JobNumber",
				"LinkedTransaction.AH_ComplianceSubType",
				"LinkedTransaction.AH_OH",
				"LinkedTransaction.InvoiceDate",
				"LinkedTransaction.EInvoicingStatus",
				"LinkedTransaction.EInvoicingError",
			};

			var ownColumns = ownColumnNames.Select(columnName => new TableColumn("", columnName)).ToArray();

			var request = Factory.NewWithValidTestData<TransactionPendingAllocationApprovalRequest>();

			var fetchStrategy = new TransactionPendingAllocationApprovalRequestFetchStrategy(request);
			AssertEquals("Factory should not have Fetch Hints for TransactionHeader", 0, Factory.ActiveFetchHintsForTable(AutoAccTransactionHeader.Schema.TableName));

			fetchStrategy.FetchForView(ownColumns);
			AssertEquals("Factory should not have Fetch Hints for TransactionHeader for own columns", 0, Factory.ActiveFetchHintsForTable(AutoAccTransactionHeader.Schema.TableName));

			foreach (var transactionColumnName in transactionColumnNames)
			{
				var mixedColumns = ownColumns.ToList();
				mixedColumns.Add(new TableColumn("", transactionColumnName));

				fetchStrategy.FetchForView(mixedColumns.ToArray());
				AssertEquals($"{transactionColumnName} requires Fetch Hint for TransactionHeader", 1, Factory.ActiveFetchHintsForTable(AutoAccTransactionHeader.Schema.TableName));
			}
		}
	}
}
