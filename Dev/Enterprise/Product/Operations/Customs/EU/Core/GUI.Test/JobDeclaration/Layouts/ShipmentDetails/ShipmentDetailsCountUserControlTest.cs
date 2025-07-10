using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class ShipmentDetailsCountUserControlTest : TestCase
	{
		public void TestCaptionRenderingEnabled()
		{
			AssertEquals("The layout needs the caption resource strings", true, control.CaptionRenderingEnabled);
		}

		public void TestTotalNoOfPiecesCalcEdit()
		{
			AssertType<ZCalcEdit>(control.TotalNoOfPiecesCalcEdit);
		}

		public void TestTotalContainerCountCalcEdit()
		{
			AssertType<ZCalcEdit>(control.ContainerCountCalcEdit);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new ShipmentDetailsCountUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		ShipmentDetailsCountUserControl control;
	}
}
