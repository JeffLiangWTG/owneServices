using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.EInvoicing;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class TransactionPendingAllocationFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForView()
		{
			var ownColumns = new TableColumn[]
			{
				new TableColumn("", "AH_TransactionNum"),
				new TableColumn("", "AH_Desc"),
				new TableColumn("", "AH_DueDate"),
				new TableColumn("", "AH_InvoiceDate"),
				new TableColumn("", "AH_DocumentReceivedDate"),
				new TableColumn("", "AH_GB"),
				new TableColumn("", "AH_GE"),
				new TableColumn("", "AH_PostDate"),
				new TableColumn("", "AH_Desc"),
				new TableColumn("", "AH_TransactionNum")
			};

			var pivotColumns = new TableColumn[]
			{
				new TableColumn("", "EInvoicingStatus"),
				new TableColumn("", "EInvoicingError"),
				new TableColumn("", "EInvoicingGovernmentAllocatedNumber")
			};

			var transaction = Factory.NewWithValidTestData<TransactionPendingAllocation>();
			var fetchStrategy = new TransactionPendingAllocationFetchStrategy(transaction);

			AssertEquals("Factory should not have Fetch Hints for Pivot table", 0, Factory.ActiveFetchHintsForTable(AutoAccEInvoicingTransactionPivot.Schema.TableName));

			fetchStrategy.FetchForView(ownColumns);
			AssertEquals("Factory should not have Fetch Hints for Pivot table using own columns", 0, Factory.ActiveFetchHintsForTable(AutoAccEInvoicingTransactionPivot.Schema.TableName));

			foreach (var pivotColumn in pivotColumns)
			{
				var mixedColumns = ownColumns.ToList();
				mixedColumns.Add(pivotColumn);
				fetchStrategy.FetchForView(mixedColumns.ToArray());
				AssertEquals($"{pivotColumn.ColumnName} requires Fetch Hint for Pivot table", 1, Factory.ActiveFetchHintsForTable(AutoAccEInvoicingTransactionPivot.Schema.TableName));
			}
		}
	}
}
