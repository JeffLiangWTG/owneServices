using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.Declaration.Testing
{
	class FRGuaranteesUserControlTest : TestCaseWithFactory
	{
		public void TestColumns()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var control = new FRGuaranteesUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var guaranteesGrid = (ZGrid)form.Controls.Find("GuaranteesGrid", true).FirstOrDefault();
				var friendlyDeltaTNamecolumn = FindColumnByName(guaranteesGrid, "CusGuarantee+CustomsGuaranteeFriendlyNameForDeltaT");
				Assert(friendlyDeltaTNamecolumn.IsVisible);
				AssertEquals("COD Detail", friendlyDeltaTNamecolumn.CaptionResourceString.Caption);
			}
		}

		ZGridColumnInfo FindColumnByName(ZGrid grid, string columnName) => grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == $"{columnName}");
	}
}

