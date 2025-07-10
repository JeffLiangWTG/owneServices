using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	sealed class Message818LineProviderHelperTest : TestCaseWithFactory
	{
		public void TestObservedShortageOrExcess()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Error for empty", decimal.Zero, helper.ObservedShortageOrExcess);
				emcsInvoiceLine.JI_CustomsQuantity = 5.11;
				emcsInvoiceLine.ZG_DeclaredValue = 5.11;
				AssertEquals("No difference to the Declared", decimal.Zero, helper.ObservedShortageOrExcess);
				emcsInvoiceLine.JI_CustomsQuantity = 10.12;
				emcsInvoiceLine.ZG_DeclaredValue = 5.11;
				AssertEquals("Customs Qty greater than the Declared Value", 5.01m, helper.ObservedShortageOrExcess);
				emcsInvoiceLine.JI_CustomsQuantity = 1.34;
				AssertEquals("Customs Qty less than the Declared Value", -3.77m, helper.ObservedShortageOrExcess);
				emcsInvoiceLine.JI_CustomsQuantity = 6.11;
				AssertEquals("Value is integer", "1", helper.ObservedShortageOrExcess.ToString());
				emcsInvoiceLine.JI_CustomsQuantity = 5.61;
				AssertEquals("Value is normalized", "0.5", helper.ObservedShortageOrExcess.ToString());
			});
		}

		public void TestRefusedQuantity()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Error for empty", decimal.Zero, helper.RefusedQuantity);
				emcsInvoiceLine.Outturn.C5_RejectedQuantity = 5.15;
				AssertEquals(5.15m, helper.RefusedQuantity);
				emcsInvoiceLine.Outturn.C5_RejectedQuantity = 2.00;
				AssertEquals("Value is integer", "2", helper.RefusedQuantity.ToString());
				emcsInvoiceLine.Outturn.C5_RejectedQuantity = 3.50;
				AssertEquals("Value is normalized", "3.5", helper.RefusedQuantity.ToString());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			emcsDeclaration = Factory.New<EMCSJobDeclaration>();
			emcsInvoiceLine = emcsDeclaration.InvoiceHeader.InvoiceLines.AddNew();
			helper = new Message818LineProviderHelper(emcsInvoiceLine);
		}
		EMCSJobDeclaration emcsDeclaration;
		EMCSJobComInvoiceLine emcsInvoiceLine;
		Message818LineProviderHelper helper;
	}
}
