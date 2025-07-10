using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class StreetAndNumberWithAddressValidationControlTest : TestCaseWithFactory
	{
		public void TestStreetAndNumberTextBox()
		{
			var streetAndNumberTextBox = control.StreetAndNumberTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", streetAndNumberTextBox);
				AssertEquals("BindTo", nameof(CusGoodsLocation.Address) + "." + nameof(CusGoodsLocationAddress.E2_Address1AndE2_Address2), streetAndNumberTextBox.BindTo);
			});
		}

		public void TestValidateAddressButton()
		{
			var validateAddressButton = control.ValidateAddressButton;
			AssertType<ZButton>("Type", validateAddressButton);
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new StreetAndNumberWithAddressValidationControl();
		}

		StreetAndNumberWithAddressValidationControl control;
	}
}
