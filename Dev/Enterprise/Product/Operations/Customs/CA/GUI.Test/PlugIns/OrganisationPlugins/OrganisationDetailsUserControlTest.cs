using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI;

sealed class OrganisationDetailsUserControlTest : TestCaseWithFactory
{
	public void TestControlsVisibility()
	{
		using (var form = new ZForm())
		using (var userControl = new OrganisationDetailsUserControl())
		{
			form.Controls.Add(userControl);
			form.Show();
			Assert(userControl.FindSingle<ZGroupBox>("SafeFoodLicenseGroupBox").Visible);
			var grid = userControl.FindSingle<ZGrid>("SafeFoodLicensesGrid");
			Assert(grid.Visible);

			var columns = grid.ColumnStyles.Cast<ZGridColumnInfo>();
			AssertEquals(2, columns.Count());

			ColumnIsVisible(columns.First(c => c.ColumnName == AutoCusCodeData.Schema.CY_Code));
			ColumnIsVisible(columns.First(c => c.ColumnName == AutoCusCodeData.Schema.CY_Data));
		}
	}

	void ColumnIsVisible(ZGridColumnInfo column)
	{
		Assert(column is ZTextBoxColumnStyleInfo);
		Assert(column.IsVisible);
	}
}
