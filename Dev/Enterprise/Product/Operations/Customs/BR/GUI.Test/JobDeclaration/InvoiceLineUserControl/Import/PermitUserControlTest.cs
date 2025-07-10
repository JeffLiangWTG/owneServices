using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI.Testing
{
	public class PermitUserControlTest : TestCaseWithFactory
	{
		public void TestDataSourceType()
		{
			using (var control = new PermitUserControl())
			{
				AssertEquals("DataSourceType", typeof(JobDeclaration), control.DataSourceType);
			}
		}

		public void TestColumns()
		{
			using (var control = new PermitUserControl())
			{
				var visibleColumnsCount = control.PermitGrid.ColumnStyles.Count;
				AssertEquals(3, visibleColumnsCount);

				AssertEquals("Should have CSI_ReferenceNumber column in PermitGrid", "CSI_ReferenceNumber", ((ZCodeFindBoxColumnStyleInfo)control.PermitGrid.ColumnStyles[0]).ColumnName);
				AssertEquals("Should have CSI_Quantity column in PermitGrid", "CSI_Quantity", ((ZCalcEditColumnStyleInfo)control.PermitGrid.ColumnStyles[1]).ColumnName);
				AssertEquals("Should have CSI_UnitOfQuantity column in PermitGrid", "CSI_UnitOfQuantity", ((ZDropEditColumnStyleInfo)control.PermitGrid.ColumnStyles[2]).ColumnName);
			}
		}
	}
}
