using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AsycudaCustoms.GUI.Testing
{
	class ExportInvoiceLineChargesUserControlTest : TestCaseWithFactory
	{
		public void TestGridChanges()
		{
			using (var userControl = new ExportInvoiceLineChargesUserControl())
			{
				Assert("J7_IsGSTApplicable removed from ChargesGrid", userControl.ChargesGrid.GetColumnStyle("J7_IsGSTApplicable").IsUnavailable);
				Assert("J7_IsGSTApplicable removed from ApportionedChargesGrid", userControl.ApportionedChargesGrid.GetColumnStyle("J7_IsGSTApplicable").IsUnavailable);

				var columnInChargesGrid = userControl.ChargesGrid.GetColumnStyle("J7_IsDutiable");
				AssertEquals("Add to Customs Value", columnInChargesGrid.Caption);
				AssertEquals(120, columnInChargesGrid.Width);

				var columnInApportionedChargesGrid = userControl.ApportionedChargesGrid.GetColumnStyle("J7_IsDutiable");
				AssertEquals("Add to Customs Value", columnInApportionedChargesGrid.Caption);
				AssertEquals(120, columnInApportionedChargesGrid.Width);
			}
		}
	}
}
