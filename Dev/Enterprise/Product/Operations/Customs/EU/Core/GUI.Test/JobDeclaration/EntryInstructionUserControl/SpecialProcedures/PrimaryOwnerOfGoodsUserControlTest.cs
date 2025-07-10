using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class PrimaryOwnerOfGoodsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(JobDeclaration), control.BindingSource.DataSourceType);
		}

		public void TestCaptionRenderingEnabled()
		{
			AssertEquals("CaptionRenderingEnabled", true, control.CaptionRenderingEnabled);
		}

		public void TestPrimaryOwnerOfGoodsGroupBox()
		{
			var groupBox = control.PrimaryOwnerOfGoodsGroupBox;
			CombineAssertions(() =>
			{
				AssertEquals("Location", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true), groupBox.Location);
				AssertEquals("Caption", "Primary Owner of Goods", groupBox.CaptionResourceString.Caption);
			});
		}

		public void TestPrimaryOwnerOfGoodsAddressControl()
		{
			var codeOrganisationFindBox = control.OwnerOfGoodsOrganisationFindBox;
			CombineAssertions(() =>
			{
				AssertCollectionContains("Within PrimaryOwnerOfGoodsGroupBox", codeOrganisationFindBox, control.PrimaryOwnerOfGoodsGroupBox.Controls);
				AssertType<ZOrganisationFindBox>("Type", codeOrganisationFindBox);
				AssertEquals("BindTo", "CustomsEntryInstructions.CEI_OH_Owner", codeOrganisationFindBox.BindTo);
			});
		}

		PrimaryOwnerOfGoodsUserControl control;

		protected override void SetUp()
		{
			base.SetUp();
			control = new PrimaryOwnerOfGoodsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
