using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;

namespace Enterprise.Accounting.Module.Testing
{
	public class PaymentBatchFilterStripControlTest : TestCaseWithFactory
	{
		public void TestColumnsAddToGridCorrectly()
		{
			AssertColumnsAddedToGridCorrectly("APB_BatchNumber");
			AssertColumnsAddedToGridCorrectly("APB_AB");
			AssertColumnsAddedToGridCorrectly("APB_PaymentType");
			AssertColumnsAddedToGridCorrectly("LocalAmountTotal");
			AssertColumnsAddedToGridCorrectly("APB_AK");
			AssertColumnsAddedToGridCorrectly("APB_ChequeOrReference");
			AssertColumnsAddedToGridCorrectly("APB_Status");
			AssertColumnsAddedToGridCorrectly("APB_PaymentDate");
			AssertColumnsAddedToGridCorrectly("APB_PostDate");
			AssertColumnsAddedToGridCorrectly("APB_GB");
		}

		void AssertColumnsAddedToGridCorrectly(ZString columnName)
		{
			using (var filterControl = new PaymentBatchFilterStripControl(null, new PaymentBatchFilterStripBusinessObject()))
			{
				var expectedColumn = columnName;
				var columns = filterControl.FilteredGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				Assert("New column should be added.", columns.Any(x => x.ColumnName == expectedColumn));
				Assert("New column should be visible", columns.FirstOrDefault(x => x.ColumnName == expectedColumn).IsVisible);
			}
		}
	}
}
