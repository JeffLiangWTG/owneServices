using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	class ShipmentDetailsUnlocoIncoTermsPlaceUserControlTest : TestCase
	{
		public void TestCaptionRenderingEnabled()
		{
			AssertEquals("The layout needs the caption resource strings", true, control.CaptionRenderingEnabled);
		}

		public void TestShipmentIncoTermPlaceTextBox()
		{
			AssertType<ZTextBox>(control.ShipmentIncoTermPlaceTextBox);
		}

		public void TestAgreedPlaceCodeDropEdit()
		{
			AssertType<ZCodeFindBox>(control.AgreedPlaceCodeFindBox);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new ShipmentDetailsUnlocoIncoTermsPlaceUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		ShipmentDetailsUnlocoIncoTermsPlaceUserControl control;
	}
}
