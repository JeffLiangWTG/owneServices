using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class TestAlreadyPostedInterCompanyInvoiceMarker : TestCaseWithFactory
	{
		public void TestGetErrorMessageForIncorrectSelection()
		{
			//For Sister Company Invoice No Message Should be displayed
			SetUpSisterCompanyData();

			InvoicingBase sisterCompanyARInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			sisterCompanyARInvoice1.AH_OH = originalBranch.OrgProxy.PK;

			InvoicingBase sisterCompanyARInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "002", TestObjectCreator.USD, 2.0m, 200.00m, 20.00m, 200.00m, 20.00m);
			sisterCompanyARInvoice2.AH_OH = originalBranch.OrgProxy.PK;
			Factory.Save();

			var marker = new AlreadyPostedInterCompanyInvoiceMarker(new List<TransactionHeader>() { sisterCompanyARInvoice1, sisterCompanyARInvoice2 });
			ZString msg = marker.GetErrorMessageForIncorrectSelection();
			Assert(msg.IsEmpty);

			//For Not Sister Company Invoice Message Should be displayed
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsDebtor = true;
			org.OH_IsCreditor = true;

			var uaInvoice = Factory.NewWithValidTestData<UAInvoice>();
			uaInvoice.AH_OH = org.PK;
			TestObjectCreator.CreateInvoiceLine(uaInvoice, uaInvoice.TransactionCurrency, uaInvoice.AH_ExchangeRate, 100m, 0m, 0m, 100m, 0m, 0m);

			var uaCreditNote = Factory.NewWithValidTestData<UACreditNote>();
			uaCreditNote.AH_OH = org.PK;
			TestObjectCreator.CreateInvoiceLine(uaCreditNote, uaCreditNote.TransactionCurrency, uaCreditNote.AH_ExchangeRate, 200m, 0m, 0m, 200m, 0m, 0m);

			Factory.Save();

			marker = new AlreadyPostedInterCompanyInvoiceMarker(new List<TransactionHeader>() { uaInvoice, uaCreditNote });
			msg = marker.GetErrorMessageForIncorrectSelection();
			var errTransactions = new List<TransactionHeader>() { uaInvoice, uaCreditNote }.OrderBy(x => x.AH_TransactionNum).ToList();
			Assert(!msg.IsEmpty);
			AssertEquals("Message should be:", string.Format(@"You can only flag intercompany transactions as already posted. 
The following transaction(s) cannot be updated:

{0}
{1}

Please re-select the required transaction(s) to be updated as already posted.", errTransactions[0].AH_TransactionNum, errTransactions[1].AH_TransactionNum), msg);

			//For a set of Invoice/credit notes which has both sister company and non sister company transactions
			marker = new AlreadyPostedInterCompanyInvoiceMarker(new List<TransactionHeader>() { uaInvoice, uaCreditNote, sisterCompanyARInvoice1, sisterCompanyARInvoice2 });
			msg = marker.GetErrorMessageForIncorrectSelection();
			Assert(!msg.IsEmpty);
			AssertEquals("Message should be:", string.Format(@"You can only flag intercompany transactions as already posted. 
The following transaction(s) cannot be updated:

{0}
{1}

Please re-select the required transaction(s) to be updated as already posted.", errTransactions[0].AH_TransactionNum, errTransactions[1].AH_TransactionNum), msg);
		}

		public void TestGetMessageForConfirmationPrompt()
		{
			//For Sister Company Invoice Message Should be displayed
			SetUpSisterCompanyData();

			var sisterCompanyARInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			sisterCompanyARInvoice1.AH_OH = originalBranch.OrgProxy.PK;

			var sisterCompanyARInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "002", TestObjectCreator.USD, 2.0m, 200.00m, 20.00m, 200.00m, 20.00m);
			sisterCompanyARInvoice2.AH_OH = originalBranch.OrgProxy.PK;
			Factory.Save();

			var marker = new AlreadyPostedInterCompanyInvoiceMarker(new List<TransactionHeader>() { sisterCompanyARInvoice1, sisterCompanyARInvoice2 });
			var sortedTransactions = new List<TransactionHeader>() { sisterCompanyARInvoice1, sisterCompanyARInvoice2 }.OrderBy(x => x.AH_TransactionNum).ToList();
			ZString msg = marker.GetMessageForConfirmationPrompt();
			Assert(!msg.IsEmpty);
			AssertEquals("Message should be:", string.Format(@"Do you wish to flag the following intercompany transactions as already posted?

{0}
{1}", sortedTransactions[0].AH_TransactionNum, sortedTransactions[1].AH_TransactionNum), msg);
		}

		public void TestMarkAsAlreadyPosted()
		{
			//For Sister Company Invoice Message Should be displayed
			SetUpSisterCompanyData();

			InvoicingBase sisterCompanyARInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			sisterCompanyARInvoice1.AH_OH = originalBranch.OrgProxy.PK;

			InvoicingBase sisterCompanyARInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "002", TestObjectCreator.USD, 2.0m, 200.00m, 20.00m, 200.00m, 20.00m);
			sisterCompanyARInvoice2.AH_OH = originalBranch.OrgProxy.PK;
			Factory.Save();

			var marker = new AlreadyPostedInterCompanyInvoiceMarker(new List<TransactionHeader>() { sisterCompanyARInvoice1, sisterCompanyARInvoice2 });
			marker.MarkAsAlreadyPosted();
			Factory.Save();

			var reloadedInvoices = Factory.Load<TransactionHeader>(new ZQuery(AccTransactionHeaderSchema.PK, new ZGuid[] { sisterCompanyARInvoice1.PK, sisterCompanyARInvoice2.PK }));
			Assert(reloadedInvoices[0].AH_PostedInternal);
			Assert(reloadedInvoices[1].AH_PostedInternal);
		}

		[ExpectException(typeof(System.ArgumentNullException))]
		public void TestWhetherEmptyTransactionSetHasBeenProvidedInConstructor()
		{
			var marker  = new AlreadyPostedInterCompanyInvoiceMarker(null);
			marker.GetErrorMessageForIncorrectSelection();
		}

		[DisableZeroExchangeRateOverriding]		// To use production behaviour and produce a zero exchange rate
		public void TestInvoiceJobGetterDoesNotCreateNewPersistentObjects_WI00285269()
		{
			SetUpSisterCompanyData();
			differentCompanyOrgProxy.CompanyData.OB_RX_NKARDDefltCurrency = "USD";

			var shipment = TestObjectCreator.CreateShipment("S1000", transportMode: "AIR");
			shipment.ConsigneePK = TestObjectCreator.AALSHI.PK;
			shipment.ConsignorPK = TestObjectCreator.ABIGAS.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			Assert("Precondition: must be export shipment", shipment.IsExport());

			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.AALSHI, 1m, null, 0m);
			var charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OSCostAmt = 100m;
			charge.JR_RX_NKCostCurrency = "USD";
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_OH_SellAccount = ZGuid.Empty;     // Required to trigger Job.ResetDebtorOnCharges()
			job.JH_OA_LocalChargesAddr = differentCompanyOrgProxy.MainAddress.PK;
			job.ExchangeRates.RemoveAndDeleteAll();     // The incident had no exchange rates listed.

			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "S1000", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m, differentCompanyOrgProxy, ZGuid.Empty);
			invoice.AH_JH = job.PK;

			AssertEquals("Precondition: no charge sell account", ZGuid.Empty, charge.JR_OH_SellAccount);
			AssertEquals("Precondition: no overseas agent", ZGuid.Empty, job.JH_OA_AgentCollectAddr);
			AssertEquals("Precondition: no exchange rates", 0, job.ExchangeRates.Count);

			// These will trigger the Job's Overseas Agent to be defaulted, see Job.SetDefaultValuesForOverseasAgent()
			TestObjectCreator.AALSHI.CompanyData.OB_EXBillAgentChargesDirect = true;
			TestObjectCreator.AALSHI.CompanyData.OB_IMBillAgentChargesDirect = true;
			TestObjectCreator.ABIGAS.CompanyData.OB_EXBillAgentChargesDirect = true;
			TestObjectCreator.ABIGAS.CompanyData.OB_IMBillAgentChargesDirect = true;
			Factory.Save();

			AssertEquals("Precondition: no charge sell account after Save", ZGuid.Empty, charge.JR_OH_SellAccount);
			AssertEquals("Precondition: no overseas agent after Save", ZGuid.Empty, job.JH_OA_AgentCollectAddr);
			AssertEquals("Precondition: no exchange rates after Save", 0, job.ExchangeRates.Count);

			// When in a sister company.
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, differentBranch.PK.ToGuid(), TestObjectCreator.MiscDepartment.PK.ToGuid()))
			{
				var sisterFactory = new BusinessObjectFactory();

				var jobExchangeRates = sisterFactory.Load<ExchangeRate>(new ZQuery(JobExRateSchema.JF_JH, job.PK));
				AssertEquals("Precondition: Job should not have any exchange rates", 0, jobExchangeRates.Length);

				var sisterExchangeRates = sisterFactory.Load<RefExchangeRate>(new ZQuery(RefExchangeRateSchema.RE_GC, differentCompany.PK));
				AssertEquals("Precondition: Sister company does not have any exchange rates (in particular, the USD rate on the JobCharge)", 0, sisterExchangeRates.Length);

				var sisterCompanyInvoice = sisterFactory.Load<ARInvoice>(invoice.PK);
				var job2 = (Job)sisterCompanyInvoice.Job;
				AssertEquals("Accessing the InvoiceingBase.Job property should not add missing job exchange rate", 0, job2.ExchangeRates.Count);

				var marker = new AlreadyPostedInterCompanyInvoiceMarker(new TransactionHeader[] { sisterCompanyInvoice });
				marker.MarkAsAlreadyPosted();
				sisterFactory.Save();

				var reloadedInvoice = Factory.Load<TransactionHeader>(sisterCompanyInvoice.PK);
				Assert("Invoice should be marked as PostedInternal", reloadedInvoice.AH_PostedInternal);
			}
		}

		void SetUpSisterCompanyData()
		{
			originalBranch = GlbBranch.CurrentBranch;

			differentCompany = TestObjectCreator.CreateNewCompany("ABC", "CN");

			differentBranch = TestObjectCreator.CreateNewBranch(differentCompany, "AB1");
			differentCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("ORGPROXYC", true, true);
			differentCompany.GC_OH_OrgProxy = differentCompanyOrgProxy.PK;

			Factory.Save();
		}

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		GlbCompany differentCompany;
		GlbBranch differentBranch;
		OrgHeader differentCompanyOrgProxy;
		GlbBranch originalBranch;
	}
}
