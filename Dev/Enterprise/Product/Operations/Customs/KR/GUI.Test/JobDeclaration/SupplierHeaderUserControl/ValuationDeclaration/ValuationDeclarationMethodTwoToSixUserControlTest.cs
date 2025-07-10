using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(ValuationDeclarationMethodTwoToSixUserControl))]
	sealed class ValuationDeclarationMethodTwoToSixUserControlTest : TestCaseWithFactory
	{
		public void TestDocument()
		{
			AssertDynamicLayoutPanel("DynamicDocumentLayoutPanel");
		}
		public void TestItemUseCode()
		{
			AssertDynamicLayoutPanel("DynamicItemUseCodeLayoutPanel");
		}
		public void TestGoodsPricingBasis()
		{
			AssertDynamicLayoutPanel("DynamicGoodsPricingBasisLayoutPanel");
		}

		void AssertDynamicLayoutPanel(string panelName)
		{
			using (var userControl = new ValuationDeclarationMethodTwoToSixUserControl())
			{
				var dynamicLayoutPanel = userControl.FindSingle<DynamicLayoutPanel>(panelName);
				AssertNotNull(dynamicLayoutPanel);
			}
		}
	}
}
