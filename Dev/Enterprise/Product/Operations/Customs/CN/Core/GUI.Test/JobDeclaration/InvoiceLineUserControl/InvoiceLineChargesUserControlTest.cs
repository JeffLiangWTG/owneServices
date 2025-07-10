using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CN.GUI.Testing
{
	class InvoiceLineChargesUserControlTest : TestCaseWithFactory
	{
		public void TestGridCaption()
		{
			using (var control = new InvoiceLineChargesUserControl())
			{
				var chargesIsDutiable = control.ChargesGrid.GetColumnStyle("J7_IsDutiable");
				AssertEquals("Add to FOB?", chargesIsDutiable.Caption);
				AssertEquals("Add to CIF?", control.ChargesGrid.GetColumnStyle("J7_IsGSTApplicable").Caption);
				AssertEquals("Add to FOB?", control.ApportionedChargesGrid.GetColumnStyle("J7_IsDutiable").Caption);
				AssertEquals("Add to CIF?", control.ApportionedChargesGrid.GetColumnStyle("J7_IsGSTApplicable").Caption);
			}
		}
	}
}
