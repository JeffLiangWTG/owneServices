using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;

namespace Enterprise.Accounting.Module.Testing
{
	sealed class ChequeTransactionFilterStripControlTest : TestCaseWithFactory
	{
		public void TestColumnsAddToGridCorrectly()
		{
			using (var filterControl = new ChequeTransactionFilterStripControl(null, new ChequeTransactionFilterStripBusinessObject()))
			{
				AssertColumnsAddedToGridCorrectly("APB_BatchNumber");
				AssertColumnsAddedToGridCorrectly("APB_PaymentType");
				AssertColumnsAddedToGridCorrectly("APB_PaymentDate");
				AssertColumnsAddedToGridCorrectly("AmountTotal");
				AssertColumnsAddedToGridCorrectly("APB_RX_NKBatchCurrency");
				AssertColumnsAddedToGridCorrectly("APB_OH_DebtorOrCreditor");
				AssertColumnsAddedToGridCorrectly("APB_AB");
				AssertColumnsAddedToGridCorrectly("APB_GB", false);
				AssertColumnsAddedToGridCorrectly("DebtorOrCreditorFullName", false);
				AssertColumnsAddedToGridCorrectly("APB_AB_FundingBankAccount", false);
				AssertColumnsAddedToGridCorrectly("APB_Description", false);
				AssertColumnsAddedToGridCorrectly("APB_ExchangeRate", false);
				AssertColumnsAddedToGridCorrectly("LocalAmountTotal", false);

				void AssertColumnsAddedToGridCorrectly(ZString columnName, bool isVisible = true)
				{
					var expectedColumn = columnName;
					var columns = filterControl.FilteredGrid.ColumnStyles.Cast<ZGridColumnInfo>();
					Assert("New column should be added.", columns.Any(x => x.ColumnName == expectedColumn));
					Assert("New column should be visible", columns.FirstOrDefault(x => x.ColumnName == expectedColumn).IsVisible == isVisible);
				}
			}
		}
	}
}
