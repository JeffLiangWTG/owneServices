using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	sealed class UCC6TemporaryStorageGrossWeightWithUnitUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var control = new UCC6TemporaryStorageGrossWeightWithUnitUserControl())
			{
				AssertNotNull(control.Controls.Find("grossWeightUnitDropEdit", true));
				AssertNotNull(control.Controls.Find("grossWeightCalcEdit", true));
			}
		}
	}
}
