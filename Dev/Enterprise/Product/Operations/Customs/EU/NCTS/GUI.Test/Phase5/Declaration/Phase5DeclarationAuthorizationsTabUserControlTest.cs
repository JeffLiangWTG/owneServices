using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.NCTS.Business;
using static Enterprise.Customs.EU.Business.AutoCusAuthorizationUsage.Schema;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class Phase5DeclarationAuthorizationsTabUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsHeader), userControl.BindingSource.DataSourceType);
		}

		public void TestGridDockStyle()
		{
			AssertEquals("Dock", DockStyle.Fill, userControl.AuthorisationsGrid.Dock);
		}

		public void TestGridBindingMember()
		{
			AssertEquals("BindingMember", "MovementHeader.CusAuthorizationUsages", userControl.AuthorisationsGrid.GetBindingMember());
		}

		public void TestAvailableColumns()
		{
			AssertSequencesEqual("Columns", new[] { AGC_Code, AGC_Number, AGC_OH_Owner, nameof(CusAuthorizationUsage.CustomsCode) },
				 userControl.AuthorisationsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
		}

		public void TestColumnsWidth()
		{
			var authorisationsGrid = userControl.AuthorisationsGrid;
			CombineAssertions(() =>
			{
				AssertEquals("AGC_Code", 50, authorisationsGrid.GetColumnStyle(AGC_Code).Width);
				AssertEquals("AGC_Number", 130, authorisationsGrid.GetColumnStyle(AGC_Number).Width);
				AssertEquals("AGC_OH_Owner", 120, authorisationsGrid.GetColumnStyle(AGC_OH_Owner).Width);
				AssertEquals("CustomsCode", 110, authorisationsGrid.GetColumnStyle(nameof(CusAuthorizationUsage.CustomsCode)).Width);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new Phase5DeclarationAuthorizationsTabUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}

		Phase5DeclarationAuthorizationsTabUserControl userControl;
	}
}
