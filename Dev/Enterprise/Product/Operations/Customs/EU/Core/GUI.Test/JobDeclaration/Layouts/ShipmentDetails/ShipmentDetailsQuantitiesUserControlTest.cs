using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	class ShipmentDetailsQuantitiesUserControlTest : TestCase
	{
		public void TestCaptionRenderingEnabled()
		{
			AssertEquals("The layout needs the caption resource strings", true, control.CaptionRenderingEnabled);
		}

		public void TestTotalNoOfPacksCalcDropEdit()
		{
			AssertType<ZCalcDropEdit>(control.TotalNoOfPacksCalcDropEdit);
		}

		public void TestWeightCalcDropEdit()
		{
			AssertType<ZCalcDropEdit>(control.WeightCalcDropEdit);
		}

		public void TestVolumeCalcDropEdit()
		{
			AssertType<ZCalcDropEdit>(control.VolumeCalcDropEdit);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new ShipmentDetailsQuantitiesUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		ShipmentDetailsQuantitiesUserControl control;
	}
}
