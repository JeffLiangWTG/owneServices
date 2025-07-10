using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class UNDangerousGoodsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsDepartureCargoDesc), control.BindingSource.DataSourceType);
		}

		public void TestUNDangerousGoodsTextBox()
		{
			var unDangerousGoodsTextBox = control.UNDangerousGoodsTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", unDangerousGoodsTextBox);
				AssertEquals("BindTo", nameof(NctsDepartureCargoDesc.UNDGsAsString), unDangerousGoodsTextBox.BindTo);

				AssertNull("CaptionResourceString", unDangerousGoodsTextBox.CaptionResourceString.Caption);
				var labelCaptionRenderProvider = new LabelCaptionRenderProvider();
				AssertEquals("UNDangerousGoodsTextBox label is visible?", false, labelCaptionRenderProvider.GetLabelCaptionVisible(unDangerousGoodsTextBox));
			});
		}

		public void TestUNDangerousGoodsButton()
		{
			AssertType<ZButton>(control.UNDangerousGoodsButton);
		}

		public void TestImplementExtendedControl()
		{
			var unDangerousGoodsUserControlExtendedControl = control as IExtendedControl;

			AssertNotNull("UNDangerousGoodsUserControl implements IExtendedControl?", unDangerousGoodsUserControlExtendedControl);

			AssertEquals("Host", unDangerousGoodsUserControlExtendedControl, unDangerousGoodsUserControlExtendedControl.Host);
			AssertNotNull("Extensions", unDangerousGoodsUserControlExtendedControl.Extensions);
		}

		public void TestImplementResourceStringBindingMember()
		{
			var unDangerousGoodsUserControlBindingMember = control as IResourceStringBindingMember;

			AssertNotNull("UNDangerousGoodsUserControl implements IResourceStringBindingMember?", unDangerousGoodsUserControlBindingMember);
			AssertEquals("ResourceStringBindingMember", "UNDGsAsString", unDangerousGoodsUserControlBindingMember.ResourceStringBindingMember);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new UNDangerousGoodsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		UNDangerousGoodsUserControl control;
	}
}
