using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class MethodFiveToSixUserControlTest : TestCaseWithFactory
	{
		public void TestMethodFiveToSixUserControl()
		{
			using (var control = new MethodFiveToSixUserControl())
			{
				var amountAgreedUponWithCustomsGroupBox = control.FindSingle<ZGroupBox>("AmountAgreedUponWithCustomsGroupBox");
				AssertNotNull(amountAgreedUponWithCustomsGroupBox);
				AssertNotNull(amountAgreedUponWithCustomsGroupBox.FindSingle<DynamicLayoutPanel>("AmountAgreedUponWithCustomsDynamicLayoutPanel"));

				var additionalCostGroupBox = control.FindSingle<ZGroupBox>("AdditionalCostGroupBox");
				AssertNotNull(additionalCostGroupBox);
				AssertNotNull(additionalCostGroupBox.FindSingle<DynamicLayoutPanel>("AdditionalCostDynamicLayoutPanel"));
			}
		}
	}
}
