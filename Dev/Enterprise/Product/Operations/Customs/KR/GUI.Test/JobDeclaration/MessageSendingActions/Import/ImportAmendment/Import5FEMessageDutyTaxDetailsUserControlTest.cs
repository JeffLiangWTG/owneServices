using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class Import5FEMessageDutyTaxDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestDetailsTabPage()
		{
			using (var userControl = new Import5FEMessageDutyTaxDetailsUserControl())
			{
				using (var grid = userControl.FindSingle<ZGrid>("DutyTaxDetailsGrid"))
				{
					var index = 0;

					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index++]).ColumnName, nameof(AmendedItem.DutyTaxType));
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index++]).ColumnName, nameof(AmendedItem.DutyTaxTypeDescription));
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index++]).ColumnName, nameof(AmendedItem.BeforeAmount));
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index++]).ColumnName, nameof(AmendedItem.AfterAmount));
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index++]).ColumnName, nameof(AmendedItem.AmountDifference));
				}
			}
		}
	}
}
