using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public abstract class APApportionmentInvoicingLineBaseCollectionTest : TestCaseWithFactory
	{
		public void TestRaiseApportionedInvoiceLineModified()
		{
			EventRaised = false;
			var creator = new TestObjectCreator(Factory);
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "SGSIN";

			Factory.Save();

			consol.Shipments.AddNew();
			consol.Shipments.AddNew();

			var invoice = GetInvoicingBase(Factory);
			var testLineCollection = invoice.Lines;
			testLineCollection.ApportionedInvoiceLineModified += new ApportionedInvoiceLineModifiedEventHandler(TestCollection_ApportionedInvoiceLineModified);

			try
			{
				var cost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
				cost.E6_AC_ChargeCode = creator.CC1.PK;
				cost.E6_OSCostAmount = 20m;
				cost.E6_ApportionmentMethod = "SHP";
				cost.SetIsUsedForApportionment();

				var nonApportionedLine = (InvoicingLineBase)invoice.Lines.AddNew();
				invoice.ImportSingleCost(cost, (InvoicingLineBase)testLineCollection.AddNew());
				var apportionedLine1 = testLineCollection[1];
				Assert("Should be an apportioned charge", apportionedLine1.IsPopulatedFromImportedApportionment);
				var apportionedLine2 = testLineCollection[2];
				Assert("Should be an apportioned charge", apportionedLine2.IsPopulatedFromImportedApportionment);

				nonApportionedLine.AL_OSExTaxAmount = 200.00m;
				Assert("Shouldn't have raised event because line is not apportioned", !EventRaised);

				apportionedLine1.AL_OSExTaxAmount = 100.00m;
				Assert("Should have raised event", EventRaised);
				AssertEquals("Line value should remain the same", 10.00m, apportionedLine1.AL_OSExTaxAmount);
			}
			finally
			{
				invoice.ClearApportionmentJobMutexes();
			}
		}

		bool EventRaised;
		void TestCollection_ApportionedInvoiceLineModified(InvoicingLineBase sender, EventArgs e)
		{
			EventRaised = true;
		}

		protected abstract InvoicingBase GetInvoicingBase(BusinessObjectFactory factory);
		protected abstract InvoicingLineBase GetInvoicingLineBase(BusinessObjectFactory factory);
	}
}
