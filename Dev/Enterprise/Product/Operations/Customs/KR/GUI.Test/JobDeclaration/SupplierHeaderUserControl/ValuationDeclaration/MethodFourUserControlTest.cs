using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class MethodFourUserControlTest : TestCaseWithFactory
	{
		public void TestMethodFourUserControl()
		{
			using (var control = new MethodFourUserControl())
			{
				var salesOfHighestQuantityGroupBox = control.FindSingle<ZGroupBox>("SalesOfHighestQuantityGroupBox");
				AssertNotNull(salesOfHighestQuantityGroupBox);
				AssertNotNull(salesOfHighestQuantityGroupBox.FindSingle<DynamicLayoutPanel>("SalesOfHighestQuantityDynamicLayoutPanel"));

				var deductionCostGroupBox = control.FindSingle<ZGroupBox>("DeductionCostGroupBox");
				AssertNotNull(deductionCostGroupBox);
				AssertNotNull(deductionCostGroupBox.FindSingle<DynamicLayoutPanel>("DeductionCostDynamicLayoutPanel"));
			}
		}
	}
}
