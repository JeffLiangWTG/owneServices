using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	class JobConsolInvoiceReverser_InnerTest : TestCaseWithFactory
	{
		public void TestIsJobConsolInvoice()
		{
			ARInvoice invoice = Factory.New<ARInvoice>();
			JobConsolCost cost = Factory.New<JobConsolCost>();
			cost.E6_AH_ARInvoice = invoice.PK;
			JobConsolInvoiceReverser reverser = new JobConsolInvoiceReverser(invoice);
			Assert("Should be a consol invoice", reverser.IsJobConsolInvoice());
		}

		public void TestReverseRelatedAPInvAndApportionment()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			TestObjectCreator creator = new TestObjectCreator(Factory);
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = creator.CC1.PK;
			cost.E6_OH_Creditor = creator.AALSHI.PK;
			cost.E6_InvoiceNum = "APINV1";
			cost.E6_InvoiceDate = ZDateTime.Now;
			cost.E6_PaymentDate = ZDateTime.Now;
			cost.E6_RX_NKCurrency = creator.USD.RX_Code;
			cost.E6_ExchangeRate = 0.78m;
			cost.E6_ApportionmentMethod = ZArchitecture.Core.AllocationMethod.Shipment;
			cost.E6_OSCostAmount = 200m;

			Factory.Save();

			JobCollection jobs = new JobCollection(Factory);
			jobs.Load();
			ConsolInvoicingPostManager postManager = new ConsolInvoicingPostManager(Factory, jobs.Cast<Job>(), consol, apps);
			TransactionCreatorHashtable transactions = postManager.CreateTransactions(JobInvoicingPostingOption.Costs);
			AssertEquals("Should have created 1 ap invoice", 1, transactions.GetAllAPInvoicesAndCreditNotes().Length);

			Factory.Save();

			AssertNotNull(cost.APInvoice);

			InvoicingBase creditorInvoice = transactions.GetAllAPInvoicesAndCreditNotes()[0];
			ARInvoice consolARInvoice = Factory.New<ARInvoice>();
			cost.E6_AH_ARInvoice = consolARInvoice.PK;

			JobConsolInvoiceReverser reverser = new JobConsolInvoiceReverser(consolARInvoice);
			reverser.ReverseRelatedAPInvAndApportionment();

			Factory.Save();

			AssertNull(cost.APInvoice);
			AssertNull(cost.ARInvoice);

			AssertEquals("AP Invoice should be cancelled", ZBool.True, creditorInvoice.AH_IsCancelled);
			AssertEquals("AP Invoice should have number with added '-C'", "APINV1-C", creditorInvoice.ReverseTransaction.TransactionNumber);
			foreach (Job job in jobs)
			{
				Assert("Should have profit share as not posted", !job.JH_IsProfitSharePosted);
				AssertEquals("Profit share invoice should be empty", ZGuid.Empty, job.JH_ProfitShareInvoice);
				AssertEquals("Apportioned charge should still have the same cost amount", 100m, job.Charges[0].JR_OSCostAmt);
			}
		}
		public void TestReverseRelatedAPInvAndApportionmentIfAPInvoiceNumberAlreadyExist()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			TestObjectCreator creator = new TestObjectCreator(Factory);
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = creator.CC1.PK;
			cost.E6_OH_Creditor = creator.AALSHI.PK;
			cost.E6_InvoiceNum = "APINV1";
			cost.E6_InvoiceDate = ZDateTime.Now;
			cost.E6_PaymentDate = ZDateTime.Now;
			cost.E6_RX_NKCurrency = creator.USD.RX_Code;
			cost.E6_ExchangeRate = 0.78m;
			cost.E6_ApportionmentMethod = ZArchitecture.Core.AllocationMethod.Shipment;
			cost.E6_OSCostAmount = 200m;

			Factory.Save();

			JobCollection jobs = new JobCollection(Factory);
			jobs.Load();
			ConsolInvoicingPostManager postManager = new ConsolInvoicingPostManager(Factory, jobs.Cast<Job>(), consol, apps);
			TransactionCreatorHashtable transactions = postManager.CreateTransactions(JobInvoicingPostingOption.Costs);
			AssertEquals("Should have created 1 ap invoice", 1, transactions.GetAllAPInvoicesAndCreditNotes().Length);

			APCreditNote creditNoteWithTheSameNumberAsReversed = Factory.NewWithValidTestData<APCreditNote>();
			creditNoteWithTheSameNumberAsReversed.AH_TransactionNum = "APINV1-C";
			creditNoteWithTheSameNumberAsReversed.AH_OH = cost.E6_OH_Creditor;
			creditNoteWithTheSameNumberAsReversed = Factory.NewWithValidTestData<APCreditNote>();
			creditNoteWithTheSameNumberAsReversed.AH_TransactionNum = "APINV1-C/A";
			creditNoteWithTheSameNumberAsReversed.AH_OH = cost.E6_OH_Creditor;

			Factory.Save();

			AssertNotNull(cost.APInvoice);

			InvoicingBase creditorInvoice = transactions.GetAllAPInvoicesAndCreditNotes()[0];
			ARInvoice consolARInvoice = Factory.New<ARInvoice>();
			cost.E6_AH_ARInvoice = consolARInvoice.PK;

			JobConsolInvoiceReverser reverser = new JobConsolInvoiceReverser(consolARInvoice);
			reverser.ReverseRelatedAPInvAndApportionment();

			Factory.Save();

			AssertNull(cost.APInvoice);
			AssertNull(cost.ARInvoice);

			AssertEquals("AP Invoice should be cancelled", ZBool.True, creditorInvoice.AH_IsCancelled);
			AssertEquals("AP Invoice should have number with added '-C/B'", "APINV1-C/B", creditorInvoice.ReverseTransaction.TransactionNumber);

			foreach (Job job in jobs)
			{
				Assert("Should have profit share as not posted", !job.JH_IsProfitSharePosted);
				AssertEquals("Profit share invoice should be empty", ZGuid.Empty, job.JH_ProfitShareInvoice);
				AssertEquals("Apportioned charge should still have the same cost amount", 100m, job.Charges[0].JR_OSCostAmt);
			}
		}

		public void TestReverseRelatedAPInvAndApportionmentIfAPInvoiceNumberAlreadyExistAndHasMaximumLength()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			TestObjectCreator creator = new TestObjectCreator(Factory);
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = creator.CC1.PK;
			cost.E6_OH_Creditor = creator.AALSHI.PK;
			cost.E6_InvoiceNum = new string('x', cost.E6_InvoiceNumInfo.MaxLength);
			cost.E6_InvoiceDate = ZDateTime.Now;
			cost.E6_PaymentDate = ZDateTime.Now;
			cost.E6_RX_NKCurrency = creator.USD.RX_Code;
			cost.E6_ExchangeRate = 0.78m;
			cost.E6_ApportionmentMethod = ZArchitecture.Core.AllocationMethod.Shipment;
			cost.E6_OSCostAmount = 200m;

			Factory.Save();

			string originalInvoiceNumber = cost.E6_InvoiceNum;

			JobCollection jobs = new JobCollection(Factory);
			jobs.Load();
			ConsolInvoicingPostManager postManager = new ConsolInvoicingPostManager(Factory, jobs.Cast<Job>(), consol, apps);
			TransactionCreatorHashtable transactions = postManager.CreateTransactions(JobInvoicingPostingOption.Costs);
			AssertEquals("Should have created 1 ap invoice", 1, transactions.GetAllAPInvoicesAndCreditNotes().Length);

			APCreditNote creditNoteWithTheSameNumberAsReversed = Factory.NewWithValidTestData<APCreditNote>();
			AssertEquals("Precondition: E6_InvoiceNum and AH_TransactionNum MaxLengths must be equal",
				cost.E6_InvoiceNumInfo.MaxLength, creditNoteWithTheSameNumberAsReversed.AH_TransactionNumInfo.MaxLength);
			creditNoteWithTheSameNumberAsReversed.AH_TransactionNum = originalInvoiceNumber.Substring(0, cost.E6_InvoiceNumInfo.MaxLength - 2) + "-C";
			creditNoteWithTheSameNumberAsReversed.AH_OH = cost.E6_OH_Creditor;
			creditNoteWithTheSameNumberAsReversed = Factory.NewWithValidTestData<APCreditNote>();
			creditNoteWithTheSameNumberAsReversed.AH_TransactionNum = originalInvoiceNumber.Substring(0, cost.E6_InvoiceNumInfo.MaxLength - 4) + "-C/A";
			creditNoteWithTheSameNumberAsReversed.AH_OH = cost.E6_OH_Creditor;

			Factory.Save();

			AssertNotNull(cost.APInvoice);

			InvoicingBase creditorInvoice = transactions.GetAllAPInvoicesAndCreditNotes()[0];
			ARInvoice consolARInvoice = Factory.New<ARInvoice>();
			cost.E6_AH_ARInvoice = consolARInvoice.PK;

			JobConsolInvoiceReverser reverser = new JobConsolInvoiceReverser(consolARInvoice);
			reverser.ReverseRelatedAPInvAndApportionment();

			Factory.Save();

			AssertNull(cost.APInvoice);
			AssertNull(cost.ARInvoice);

			AssertEquals("AP Invoice should be cancelled", ZBool.True, creditorInvoice.AH_IsCancelled);
			AssertEquals("AP Invoice should have number with added '-C/B'",
				originalInvoiceNumber.Substring(0, cost.E6_InvoiceNumInfo.MaxLength - 4) + "-C/B",
				creditorInvoice.ReverseTransaction.TransactionNumber);
		}
	}
}