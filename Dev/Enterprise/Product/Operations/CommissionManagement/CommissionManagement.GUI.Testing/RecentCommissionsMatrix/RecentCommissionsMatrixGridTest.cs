using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.CommissionManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.CommissionManagement.GUI.Testing
{
	public class RecentCommissionsMatrixGridTest : TestCaseWithFactory
	{
		public void TestRowHeaderWidth()
		{
			var lineCollection = new ViewCommissionLineCollection(Factory);
			var matrix = new RecentCommissionsMatrix(new ZDateTime(2002, 2, 2), lineCollection);

			using (var form = new ZForm())
			using (var grid = new RecentCommissionsMatrixGrid())
			{
				ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
				zCalcEditColumnStyleInfo1.ColumnName = RecentCommissionsRow.Schema.TotalCurrentMonth;
				grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
				form.Controls.Add(grid);

				grid.SetDataBinding(matrix, "");
				form.Show();

				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(70), grid.TableStyles[0].RowHeaderWidth);
			}
		}
	}
}
