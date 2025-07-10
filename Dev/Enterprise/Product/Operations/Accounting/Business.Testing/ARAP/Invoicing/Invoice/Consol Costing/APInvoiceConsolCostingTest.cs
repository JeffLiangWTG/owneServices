using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(APInvoiceConsolCosting))]
	class APInvoiceConsolCostingTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new APInvoiceConsolCosting(Factory, Factory.New<APInvoice>());
		}

		public void TestConsolCostsUACreditNote()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "SGSIN";
			Factory.Save();

			var creditNote = Factory.NewWithValidTestData<UACreditNote>();
			creditNote.SubmittedFromInvoicingForm = true;

			consol.Shipments.AddNew();
			consol.Shipments.AddNew();

			var cost1 = creditNote.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			cost1.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			cost1.E6_OSCostAmount = 100m;
			cost1.E6_ApportionmentMethod = "SHP";
			creditNote.ImportAllApportionmentsFromCosting();
			var amount1 = creditNote.ConsolCosting.ConsolCosts[0].E6_OSCostAmount;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();

			var converter = new UnapprovedTransactionConverter(factory2);
			var creditNote2 = factory2.Load<UACreditNote>(creditNote.PK);
			var creditNote3 = (APCreditNote)converter.ConvertToAP(creditNote2, false);

			factory2.Save();
			var amount2 = creditNote3.ConsolCosting.ConsolCosts[0].E6_OSCostAmount;
			AssertEquals("should have same signs and magnitudes", amount1, amount2);
		}

		public void TestConsolCostsUACreditNoteShouldNotAlterOSCostAmount()
		{
			// Arrange
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "SGSIN";
			var shipment = consol.Shipments.AddNew();
			shipment.FillWithValidTestData();
			shipment.JS_ActualWeight = 100;

			Factory.Save();

			var uaCreditNote = Factory.NewWithValidTestData<UACreditNote>();
			uaCreditNote.SubmittedFromInvoicingForm = true;

			try
			{
				var consolCost = uaCreditNote.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
				consolCost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
				consolCost.E6_OSCostAmount = 100m;
				consolCost.SetIsUsedForApportionment();
				consolCost.E6_ApportionmentMethod = "GWT";

				AssertEquals("Unapportioned Amount", 0m, consolCost.UnApportionedAmount);
				AssertEquals("JobCharge OS Cost Amount", 100m, consolCost.E6_OSCostAmount);
				AssertEquals("ApportionSplitCharge OS Cost Amount", 100m, consolCost.ApportionmentCharges[0].JR_OSCostAmt);

				uaCreditNote.ImportAllApportionmentsFromCosting();

				AssertEquals("Transaction Line created", 1, uaCreditNote.Lines.Count);
				AssertEquals("Transaction Line Amount", 100m, uaCreditNote.Lines[0].AL_LineAmount);

				Factory.Save();
			}
			finally
			{
				uaCreditNote.ClearApportionmentJobMutexes();
			}

			// Act
			var factory2 = new BusinessObjectFactory();
			var converter = new UnapprovedTransactionConverter(factory2);
			var uaCreditNoteReload = factory2.Load<UACreditNote>(uaCreditNote.PK);
			var apCreditNote = converter.ConvertToAP(uaCreditNoteReload, false);

			BusinessObjectFactory.SaveTogether(apCreditNote.Factory);

			// Assert
			var apConsolCost = apCreditNote.ConsolCosting.ConsolCosts[0];
			var chargeCollection = new ApportionmentSplitChargeCollection(apConsolCost);
			chargeCollection.Load();

			AssertEquals("JobCharge OS Cost Amount after transform", -100m, apConsolCost.E6_OSCostAmount);
			AssertEquals("ApportionSplitCharge OS Cost transform", -100m, chargeCollection.JR_OSCostAmtSum);
		}

		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator fTestObjectCreator;

		public void TestConsolCosts()
		{
			UAInvoice invoice = Factory.NewWithValidTestData<UAInvoice>();
			invoice.AH_Ledger = "AP";
			invoice.AH_TransactionType = "INV";

			JobConsolCost cost = Factory.NewWithValidTestData<JobConsolCost>();
			cost.E6_AH_APInvoice = invoice.PK;

			Factory.Save();

			APInvoiceConsolCosting consolCosting = new APInvoiceConsolCosting(Factory, invoice);
			AssertEquals("Consol Costing must consist one cost", 1, consolCosting.ConsolCosts.Count);
			AssertEquals("ParentAPInvoice of cost must equal to ParentAPInvoice of class", consolCosting.ConsolCosts[0].ParentAPInvoice, consolCosting.ParentAPInvoice);
		}
	}
}
