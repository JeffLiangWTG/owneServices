using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	sealed class Message871LineProviderHelperTest : TestCaseWithFactory
	{
		public void TestActualQuantity()
		{
			CombineAssertions(() =>
			{
				emcsInvoiceLine.JI_CustomsQuantity = 10.12;
				AssertEquals("Actual Quantity", 10.12m, helper.ActualQuantity);

				emcsInvoiceLine.JI_CustomsQuantity = 100.200m;
				AssertEquals("Normalized Actual Quantity", "100.2", helper.ActualQuantity.ToString());

				emcsInvoiceLine.JI_CustomsQuantity = 100.000m;
				AssertEquals("Normalized Actual Quantity", "100", helper.ActualQuantity.ToString());
			});
		}

		public void TestActualQuantity_Rejected()
		{
			emcsInvoiceLine.JI_CustomsQuantity = 5.11;
			emcsInvoiceLine.Outturn.C5_RejectedQuantity = 1.34;
			AssertEquals("Actual Quantity", 3.77m, helper.ActualQuantity);
		}

		protected override void SetUp()
		{
			base.SetUp();
			emcsDeclaration = Factory.New<EMCSJobDeclaration>();
			emcsInvoiceLine = emcsDeclaration.InvoiceHeader.InvoiceLines.AddNew();
			helper = new Message871LineProviderHelper(emcsInvoiceLine);
		}
		EMCSJobDeclaration emcsDeclaration;
		EMCSJobComInvoiceLine emcsInvoiceLine;
		Message871LineProviderHelper helper;
	}
}
