using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class OwnerOfGoodsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(JobDeclaration), control.BindingSource.DataSourceType);
		}

		public void TestCaptionRenderingEnabled()
		{
			AssertEquals("CaptionRenderingEnabled", true, control.CaptionRenderingEnabled);
		}

		public void TestOwnersOfGoodsGroupBox()
		{
			var groupBox = control.OwnersOfGoodsGroupBox;
			CombineAssertions(() =>
			{
				AssertEquals("Location", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true), groupBox.Location);
				AssertEquals("Caption", "Owners of Goods", groupBox.CaptionResourceString.Caption);
			});
		}

		public void TestOwnersOfGoodsGrid()
		{
			var grid = control.OwnersOfGoodsGrid;
			CombineAssertions(() =>
			{
				AssertCollectionContains("Within OwnersOfGoodsGroupBox", grid, control.OwnersOfGoodsGroupBox.Controls);
				AssertType<ZGrid>("Type", grid);
				AssertEquals("BindTo", "CustomsEntryInstructions.OwnerOfGoodsCollection", grid.BindTo);
			});
		}

		public void TestControls_OwnersOfGoodsGrid()
		{
			var grid = control.OwnersOfGoodsGrid;
			CombineAssertions(() =>
			{
				AssertEquals("Columns", 2, grid.ColumnStyles.Count);
				AssertEquals("OwnerOrganisationPK", 100, grid.GetColumnStyle("OrganisationPK").Width);
				AssertEquals("E2_OA_Address", 300, grid.GetColumnStyle(JobDocAddress.Schema.E2_OA_Address).Width);
			});
		}

		public void TestOwnersOfGoodsGrid_ColumnsCaption()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			using (var form = new ZForm(jobDeclaration))
			{
				form.Controls.Add(control);
				form.Show();
				var grid = control.FindSingle<ZGrid>("OwnersOfGoodsGrid");
				CombineAssertions(() =>
				{
					AssertEquals("OwnerOrganisationPK - Caption", "Organization", grid.GetColumnCaption("OrganisationPK"));
					AssertEquals("E2_OA_Address - Caption", "Address", grid.GetColumnCaption("E2_OA_Address"));
				});
			}
		}

		OwnerOfGoodsUserControl control;

		protected override void SetUp()
		{
			base.SetUp();
			control = new OwnerOfGoodsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
