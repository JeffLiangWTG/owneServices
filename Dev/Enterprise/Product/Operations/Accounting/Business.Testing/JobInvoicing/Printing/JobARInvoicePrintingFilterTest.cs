using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using OrgHeader = Enterprise.MasterFiles.Business.OrgHeader;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(JobARInvoicePrintingFilter))]
	public class JobARInvoicePrintingFilterTest : JobInvoicePrintingFilterTest
	{
		public void TestGetCompanyPK()
		{
			var pk1 = ZGuid.NewZGuid();
			var pk2 = GlbCompany.CurrentCompany.PK;

			var filter = new JobARInvoicePrintingFilterForTest(null, ZGuid.Empty, Factory, pk1);
			AssertEquals(pk1, filter.GetCompanyPK_Exposed());

			filter = new JobARInvoicePrintingFilterForTest(null, ZGuid.Empty);
			AssertEquals(pk2, filter.GetCompanyPK_Exposed());
		}

		protected override JobInvoicePrintingFilter GetNewBusinessObject(IBusiness hostBusinessObject, Job jobHeader)
		{
			return new JobARInvoicePrintingFilter(hostBusinessObject, jobHeader.PK);
		}

		protected override JobInvoicePrintingFilter GetNewBusinessObject(ForwardingConsol hostBusinessObject, Job[] jobHeaders)
		{
			return new JobARInvoicePrintingFilter(hostBusinessObject, jobHeaders.Select(x => x.PK).ToArray());
		}

		public void TestDontLoadTransactionsWithEmptyConsolidatedInvoiceRef()
		{
			BusinessObjectFactory dataFactory = new BusinessObjectFactory();
			ARInvoice invoiceWithNoReference = dataFactory.New<ARInvoice>();
			((ARInvoiceLine)invoiceWithNoReference.Lines.AddNew()).AL_AG = TestObjectCreator.GLHeader1.PK;

			dataFactory.Save();

			JobARInvoicePrintingFilter printingFilter = new JobARInvoicePrintingFilter(Consol, new ZGuid[] { ZGuid.NewZGuid() });
			printingFilter.RefreshInvoiceList();
			AssertEquals("Should be no items in the list of transactions", 0, printingFilter.Transactions.Count);
		}

		public void TestEnableAccTransactionHeaderIndexHints()
		{
			var printingFilter = new JobARInvoicePrintingFilter(Consol, new ZGuid[] { ZGuid.NewZGuid() });
			Assert("index hints is diabled by default", !printingFilter.EnableAccTransactionHeaderIndexHints);

			printingFilter.EnableAccTransactionHeaderIndexHints = true;
			Assert(printingFilter.EnableAccTransactionHeaderIndexHints);
		}

		public void TestLoadTransactionsViaConsolidatedInvoiceRef()
		{
			// Agent collect invoices posted at consol level are created WITHOUT any AL_JH references to jobs. therefore you need to use AH_ConsolidatedInvoiceRef instead.
			Consol.Shipments.Add(Shipment1);
			Consol.Shipments.Add(Shipment2);

			Consol.Factory.Save();
			BusinessObjectFactory dataFactory = new BusinessObjectFactory();

			ARInvoice agentCollectInvoice1 = dataFactory.New<ARInvoice>();
			agentCollectInvoice1.AH_ConsolidatedInvoiceRef = Consol.JK_UniqueConsignRef;
			agentCollectInvoice1.AH_GC = GlbCompany.CurrentCompany.PK;
			agentCollectInvoice1.Lines.AddNew();
			agentCollectInvoice1.Lines[0].AL_AG = TestObjectCreator.GLHeader1.PK;

			ARInvoice agentCollectInvoice2 = dataFactory.New<ARInvoice>();
			agentCollectInvoice2.AH_ConsolidatedInvoiceRef = Consol.JK_UniqueConsignRef;
			agentCollectInvoice2.AH_GB = TestObjectCreator.NonCurrentCompany.Branches[0].PK;
			agentCollectInvoice2.Lines.AddNew();
			agentCollectInvoice2.Lines[0].AL_GB = TestObjectCreator.NonCurrentCompany.Branches[0].PK;
			agentCollectInvoice2.Lines[0].AL_AG = TestObjectCreator.GLHeader1.PK;
			dataFactory.Save();

			JobARInvoicePrintingFilter printingFilter = new JobARInvoicePrintingFilter(Consol, new ZGuid[] { Header1.PK, Header2.PK });
			printingFilter.RefreshInvoiceList();
			AssertEquals("Should be 14 items in the list of transactions", 14, printingFilter.Transactions.Count);
			Assert("Should contain the agent invoice from the current company", printingFilter.Transactions.Contains(agentCollectInvoice1.PK));
		}

		public void TestAgentCollectInvoiceAmendments()
		{
			Consol.Shipments.Add(Shipment1);
			Consol.Shipments.Add(Shipment2);

			Factory.Save();

			ARCreditNote agentCollectCredit = Factory.New<ARCreditNote>();
			agentCollectCredit.AH_ConsolidatedInvoiceRef = Consol.JK_UniqueConsignRef;
			agentCollectCredit.AH_GC = GlbCompany.CurrentCompany.PK;

			IAmending amendmentAsCredit = ((IAmending)agentCollectCredit).GenerateAmendingTransaction(TransactionTypes.CreditNote);
			IAmending amendmentAsInvoice = ((IAmending)agentCollectCredit).GenerateAmendingTransaction(TransactionTypes.Invoice);
			Factory.Save();

			JobARInvoicePrintingFilter printingFilter = new JobARInvoicePrintingFilter(Consol, new ZGuid[] { Header1.PK, Header2.PK });
			printingFilter.RefreshInvoiceList();
			AssertEquals("Should be 16 items in the list of transactions", 16, printingFilter.Transactions.Count);
			Assert("Should contain amendmentAsCredit", printingFilter.Transactions.Contains(amendmentAsCredit.PK));
			Assert("Should contain amendmentAsInvoice", printingFilter.Transactions.Contains(amendmentAsInvoice.PK));
		}

		[StressTest()]
		public void TestADOStackOverflowWhenLoadingLotsOfInvoices()
		{
			BusinessObjectFactory dataFactory = new BusinessObjectFactory();
			TestObjectCreator creator = new TestObjectCreator(dataFactory);
			ForwardingConsol consol = dataFactory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			for (int shipmentCount = 0; shipmentCount < 50; shipmentCount++)
			{
				ForwardingShipment shipment = consol.Shipments.AddNew();
				Job job = Job.CreateWithMutex(dataFactory, shipment);
				job.PlugInData = shipment;
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.Dispose();
			}
			dataFactory.Save();

			JobCollection jobs = new JobCollection(dataFactory);
			jobs.Load();
			for (int invoiceCount = 0; invoiceCount < 50; invoiceCount++)
			{
				ARInvoice invoiceToPost = (ARInvoice)creator.CreateInvoice(typeof(ARInvoice), creator.USD, 0.75m);
				invoiceToPost.AH_OH = creator.AALSHI.PK;
				foreach (Job jobToPostInvoiceFor in jobs)
				{
					ARInvoiceLine line = (ARInvoiceLine)invoiceToPost.Lines.AddNew();
					line.AL_AC = creator.MRG100.PK;
					line.AL_JH = jobToPostInvoiceFor.PK;
					line.AL_OSExTaxAmount = 500m;
					creator.CreateJobCharge(line, jobToPostInvoiceFor, creator.MRG100, creator.USD);
				}
			}
			dataFactory.Save();

			List<ZGuid> jobPKs = new List<ZGuid>();
			foreach (Job jobToPostInvoiceFor in jobs)
			{
				jobPKs.Add(jobToPostInvoiceFor.PK);
			}

			JobInvoicePrintingFilter filter = new JobARInvoicePrintingFilter(consol, jobPKs.ToArray());
			filter.RefreshInvoiceList();
			AssertEquals("Should be 50 invoices in the list", 50, filter.Transactions.Count);
		}

		void SetupProfitShareRelationship(OrgHeader sendingAgent, OrgHeader receivingAgent, decimal sendingProfitSharePercentage, decimal receivingProfitSharePercentage, ZString origin, ZString destination, ZString transportMode)
		{
			OrgAgentRelationship agentRelationship1 = Factory.New<OrgAgentRelationship>();
			agentRelationship1.O3_OH_SendingAgent = sendingAgent.PK;
			agentRelationship1.O3_OH_ReceivingAgent = receivingAgent.PK;
			OrgProfitShareDetails profitShare1 = agentRelationship1.ProfitShareDetails.AddNew();
			profitShare1.O4_FreightMode = transportMode;
			profitShare1.O4_StartDate = ZDateTime.Today.AddDays(-3);
			profitShare1.O4_EndDate = ZDateTime.Today.AddDays(30);
			profitShare1.O4_SendingPortOrCountry = origin;
			profitShare1.O4_ReceivingPortOrCountry = destination;
			profitShare1.O4_OH_ControllingAgent = GlbCompany.CurrentCompany.OrgProxy.PK;
			OrgProfitShareParty sendParty = profitShare1.PartyDetails.AddNew();
			sendParty.PS_PartyType = "SEN";
			sendParty.PS_PartyProfitSharePercent = sendingProfitSharePercentage;

			OrgProfitShareParty rcvParty = profitShare1.PartyDetails.AddNew();
			rcvParty.PS_PartyType = "RCV";
			rcvParty.PS_PartyProfitSharePercent = receivingProfitSharePercentage;
		}

		void CreateGroupWithPostingStyle(OrgHeader org, ZString postingStyle)
		{
			OrgInvoiceRollupOrGroup group = org.CompanyData.InvoiceRollupOrGroups.AddNew();
			group.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			group.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			group.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			group.PG_InvoicePostingStyle = InvoicePostingOptionsList.Codes.FinalInvoiceOnly;
			Factory.Save();
		}

		public void TestRefreshConsolInvoiceListWithPSInvoiceWithoutJobHeader()
		{
			#region Setup
			Factory.Save();
			TestObjectCreator creator = new TestObjectCreator(Factory);
			creator.Agent.CompanyData.OB_IsCreditor = ZBool.True;
			creator.Agent.CompanyData.OB_IsDebtor = ZBool.True;
			CreateGroupWithPostingStyle(creator.AALSHI, InvoicePostingOptionsList.Codes.FinalInvoiceOnly);

			ZString transportMode = "AIR";
			ZString origin = "AUSYD";
			ZString destination = "USLAX";

			SetupProfitShareRelationship(GlbCompany.CurrentCompany.OrgProxy, creator.Agent, 60m, 40m, origin, destination, transportMode);
			SetupConsolAndShipments(transportMode, origin, destination, creator.Agent);
			Consol.JK_PrepaidCollect = Core.Constants.PaymentType.Collect;
			Consol.JK_RL_NKLoadPort = origin;
			Consol.JK_RL_NKDischargePort = destination;
			Consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);
			Consol.SetDefaultReceivingForwarderAddress(creator.Agent);

			ApportionmentListing apps = new ApportionmentListing(Factory, Consol);
			apps.IsActivated = true;
			apps.LoadChildShipmentsAndAcquireMutexesWhereRequired();
			JobConsolCost freightCost = apps.CostsCollection.TryAddNew();
			freightCost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			freightCost.E6_RX_NKCurrency = creator.USD.RX_Code;
			freightCost.E6_ExchangeRate = 0.65m;
			freightCost.E6_ApportionmentMethod = "SHP";
			freightCost.E6_OSCostAmount = 200m;
			freightCost.E6_OH_Creditor = creator.Agent.PK;
			freightCost.E6_InvoiceNum = "xawecadfa";
			freightCost.E6_InvoiceDate = ZDateTime.Now;

			Factory.Save();

			Job shipment1Job = new Job.Loader(Factory, Shipment1).Load();
			Job shipment2Job = new Job.Loader(Factory, Shipment2).Load();
			shipment1Job.JH_GE = ZGuid.Empty;
			shipment1Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			shipment2Job.JH_GE = ZGuid.Empty;
			shipment2Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			shipment1Job.PlugInData = Shipment1;
			shipment2Job.PlugInData = Shipment2;

			Charge shipment1Charge = shipment1Job.Charges.AddNew();
			Charge shipment2Charge = shipment2Job.Charges.AddNew();

			shipment1Charge.JR_AC = Env.Registry.FreightChargeCode;
			shipment1Charge.JR_IsIncludedInProfitShare = true;
			shipment1Charge.JR_OSSellAmt = 180m;
			shipment1Charge.JR_OH_SellAccount = creator.Agent.PK;
			shipment1Charge.JR_OSCostAmt = 80m;
			shipment1Charge.JR_AgentDeclaredSellAmt = 150m;
			shipment1Charge.JR_AgentDeclaredCostAmt = 100m;

			shipment2Charge.JR_AC = Env.Registry.FreightChargeCode;
			shipment2Charge.JR_IsIncludedInProfitShare = true;
			shipment2Charge.JR_OH_SellAccount = creator.Agent.PK;
			shipment2Charge.JR_OSSellAmt = 200m;
			shipment2Charge.JR_OSCostAmt = 100m;
			shipment2Charge.JR_AgentDeclaredSellAmt = 180m;
			shipment2Charge.JR_AgentDeclaredCostAmt = 120m;

			Factory.Save();
			apps.ReleaseMutexes();

			JobCollection jobs = new JobCollection(Factory);
			jobs.Load();

			#endregion

			ConsolInvoicingPostManager testPostManager = new ConsolInvoicingPostManager(Factory, jobs.Cast<Job>(), Consol, apps);
			TransactionCreatorHashtable transactions = testPostManager.CreateTransactions(JobInvoicingPostingOption.Agent);
			Factory.Save();

			JobARInvoicePrintingFilter printingFilter = new JobARInvoicePrintingFilter(Consol, new ZGuid[] { shipment1Job.PK, shipment2Job.PK });

			printingFilter.RefreshInvoiceList();
			AssertEquals("Invoice Count", 14, printingFilter.Transactions.Count);
			printingFilter.DebtorOrCreditor = creator.Agent.PK;
			printingFilter.RefreshInvoiceList();
			AssertEquals("Invoice Count", 1, printingFilter.Transactions.Count);
			AssertContains("AH_ConsolidatedInvoiceRef must contain Consol Number", Consol.JK_UniqueConsignRef, printingFilter.Transactions[0].AH_ConsolidatedInvoiceRef);
		}

		public void TestSetDefaultValues()
		{
			JobARInvoicePrintingFilter printingFilter = new JobARInvoicePrintingFilter(Consol, new ZGuid[] { Header1.PK, Header2.PK });
			AssertEquals("Debtor", ZGuid.Empty, printingFilter.DebtorOrCreditor);
			AssertEquals("Job Number", ZGuid.Empty, printingFilter.JobNumber);
			AssertEquals("Transaction Type", ZString.Empty, printingFilter.TransactionType);
		}

		public void TestRefreshInvoiceListWithAdditionalJobs()
		{
			CommonCartage parent1 = Factory.NewWithValidTestData<CommonCartage>();
			Job job1 = new Job.Loader(parent1).TryCreateWithoutMutexForTestOnly();
			Charge charge = job1.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			charge.JR_OSSellAmt = 100m;

			MockJobHeaderParentWithAdditionalJobs parent2 = Factory.NewWithValidTestData<MockJobHeaderParentWithAdditionalJobs>();
			parent2.AdditionalParent = parent1;
			Job job2 = new Job.Loader(parent2).TryCreateWithoutMutexForTestOnly();
			job2.JH_JobNum = "M00001";

			InvoicingPostManager postManager = new InvoicingPostManager(job2);
			postManager.CreateTransactions(JobInvoicingPostingOption.Revenue);
			InvoicingBaseCollection postedInvoices = postManager.Poster.PostedInvoices;

			Factory.Save();

			AssertEquals("One invoice was posted", 1, postedInvoices.Count);

			JobInvoicePrintingFilter filter = GetNewBusinessObject(parent2, job2);
			filter.RefreshInvoiceList();
			AssertEquals("Transactions should contain 1 invoice", 1, filter.Transactions.Count);
			AssertEquals("Transactions should contain warehouse invoice", true, filter.Transactions.Contains(postedInvoices[0].PK));
		}

		class MockJobHeaderParentWithAdditionalJobs : CommonCartage, IJobInvoicingPlugInAdditionalJobs
		{
			public MockJobHeaderParentWithAdditionalJobs(BusinessObjectFactory factory, System.Data.DataRow row)
				: base(factory, row)
			{
			}

			public CommonCartage AdditionalParent
			{
				get; set;
			}

			IJobInvoicingPlugIn[] IJobInvoicingPlugInAdditionalJobs.AdditionalJobsToShowChargesFor
			{
				get { return new IJobInvoicingPlugIn[] { AdditionalParent }; }
			}
		}

		protected override ZString Ledger
		{
			get { return LedgerTypes.AccountsReceivable; }
		}

		#region Get Query Overrides

		protected override JobInvoicePrintingFilter CreateInstanceForTest(IBusiness hostBusinessObject, Job jobHeader, ZDateTime from, ZDateTime to)
		{
			return new JobARInvoicePrintingFilter(hostBusinessObject, jobHeader, from, to);
		}

		[TestDate(2017, 10, 15)]
		public override void TestGetQueryForConsolWithoutJobNumber()
		{
			TestARGetQueryForConsolJobWithoutJobNumberCore();
		}

		[TestDate(2017, 10, 15)]
		public override void TestGetQueryWithJobNumber()
		{
			TestARGetQueryWithJobNumberCore();
		}

		#endregion

		class JobARInvoicePrintingFilterForTest : JobARInvoicePrintingFilter
		{
			public JobARInvoicePrintingFilterForTest(IBusiness hostBusinessObject, ZGuid jobHeaderPK)
				: base(hostBusinessObject, jobHeaderPK)
			{
			}

			public JobARInvoicePrintingFilterForTest(IBusiness hostBusinessObject, ZGuid jobHeaderPK, BusinessObjectFactory factory, ZGuid jobCompanyCampaign) : base(hostBusinessObject, jobHeaderPK, factory, jobCompanyCampaign)
			{
			}

			public ZGuid GetCompanyPK_Exposed()
			{
				return base.GetCompanyPK();
			}
		}
	}
}
