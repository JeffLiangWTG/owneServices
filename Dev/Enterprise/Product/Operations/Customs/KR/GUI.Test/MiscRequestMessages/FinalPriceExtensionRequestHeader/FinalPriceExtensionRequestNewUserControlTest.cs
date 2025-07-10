using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class FinalPriceExtensionRequestNewUserControlTest : TestCaseWithFactory
	{
		public void TestFinalPriceExtensionRequestNewUserControl()
		{
			using (var form = new FinalPriceExtensionRequestNewForm(new Business.FinalPriceReportByDateExtensionHeader(Factory)))
			using (var control = new FinalPriceExtensionRequestNewUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var dynamicHeaderDetailsPanel = control.FindSingle<DynamicLayoutPanel>("FinalPriceExtensionRequestHeaderPanel");
				DynamicLayoutPanelTest.AssertControlsOrder(dynamicHeaderDetailsPanel,
					nameof(FinalPriceExtensionRequestNewControlBag.MessageTypeDropEdit),
					nameof(FinalPriceExtensionRequestNewControlBag.CustomsOfficeCodeFindBox),
					nameof(FinalPriceExtensionRequestNewControlBag.BranchGuidFindBox));
			}
		}
	}
}
