using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class StevedoresUserControlTest : TestCaseWithFactory
	{
		public void TestGridColumn()
		{
			using (var userControl = new StevedoresUserControl())
			{
				var grid = userControl.StevedoresGrid;

				var index = 0;
				AssertEquals(nameof(CusPerson.PersonFullName), ((ZTextBoxColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName);
				AssertEquals(nameof(CusPerson.PersonHomePhoneNumber), ((ZTextBoxColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName);
				AssertEquals(nameof(CusPerson.PersonMobilePhoneNumber), ((ZTextBoxColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName);
			}
		}
	}
}
