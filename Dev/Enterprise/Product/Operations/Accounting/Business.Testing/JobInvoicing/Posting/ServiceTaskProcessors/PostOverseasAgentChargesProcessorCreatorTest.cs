using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting.Testing
{
	public class PostOverseasAgentChargesProcessorCreatorTest : TestCaseWithFactory
	{
		public void TestCreatePostOverseasAgentChargesProcessorForShipment()
		{
			ZString transportMode = "AIR";
			ZString origin = "AUSYD";
			ZString destination = "USLAX";

			var shipment = Creator.CreateShipment("S001", origin, destination);
			shipment.JS_TransportMode = transportMode;
			shipment.JS_OH_DeliveryAgent = Creator.Agent.PK;
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;

			Creator.Agent.MiscServ.OM_FWBillCollectFeesOnSingleInvoice = ZBool.False;
			Creator.SetupProfitShareRelationship(GlbCompany.CurrentCompany.OrgProxy, Creator.Agent, 60m, 40m, origin, destination, transportMode);

			var job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_OA_AgentCollectAddr = Creator.Agent.MainAddress.PK;

			var charge = Creator.CreateCharge(job, Creator.CC3, "Desc", Creator.AUD, 100M, Creator.AALSHI, Creator.AUD, 120M, Creator.Agent);
			charge.JR_AgentDeclaredSellAmtLocal = 120M;
			charge.JR_AgentDeclaredCostAmtLocal = 100M;
			charge.JR_IsIncludedInProfitShare = true;

			job.RunPreSaveValidation();
			AssertNoErrors("Precondition: job shouldn't contain errors.", job);

			Factory.Save();

			var poster = new PostOverseasAgentChargesProcessorCreator().CreateOverseasAgentChargesPoster(shipment) as JobPostingWorkflowProcessor;
			AssertNotNull(poster);
			AssertType(typeof(JobOverseasAgentChargesPoster), poster);
		}

		public void TestCreatePostOverseasAgentChargesProcessorForConsol()
		{
			var rate = AccTaxRate.Helper.FindTaxRate(new BusinessObjectFactory(), AccTaxRate.Helper.MainFreeGSTTaxRegistryID, Env.CurrentCompanyPK);
			rate.SetRateNumerator_ForTestOnly(0);
			rate.Factory.Save();

			new AccountingPeriodTestHelper(Factory).SetupSinglePeriod(1, DateTime.Today.AddMonths(-1), DateTime.Today.AddMonths(1));

			ZString transportMode = "AIR";
			ZString origin = "AUSYD";
			ZString destination = "USLAX";

			var consol = Creator.CreateConsol(origin, destination, "C0001");
			consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;

			var shipment1 = Creator.CreateShipment("S001", consol);
			var shipment2 = Creator.CreateShipment("S002", consol);
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;

			shipment1.JS_TransportMode = transportMode;
			shipment2.JS_TransportMode = transportMode;
			shipment1.JS_OH_DeliveryAgent = Creator.Agent.PK;
			shipment2.JS_OH_DeliveryAgent = Creator.Agent.PK;
			shipment1.JS_RL_NKOrigin = origin;
			shipment2.JS_RL_NKOrigin = origin;
			shipment1.JS_RL_NKDestination = destination;
			shipment2.JS_RL_NKDestination = destination;

			Creator.Agent.MiscServ.OM_FWBillCollectFeesOnSingleInvoice = ZBool.False;
			Creator.SetupProfitShareRelationship(GlbCompany.CurrentCompany.OrgProxy, Creator.Agent, 60m, 40m, origin, destination, transportMode);

			var job1 = new Job.Loader(Factory, shipment1).TryCreateWithoutMutexForTestOnly();
			job1.JH_GE = Creator.FEADepartment.PK;
			job1.JH_OA_AgentCollectAddr = Creator.Agent.MainAddress.PK;

			var charge1 = Creator.CreateCharge(job1, Creator.CC3, "Desc", Creator.AUD, 100M, Creator.AALSHI, Creator.AUD, 120M, Creator.Agent);
			charge1.JR_AgentDeclaredSellAmtLocal = 120M;
			charge1.JR_AgentDeclaredCostAmtLocal = 100M;
			charge1.JR_IsIncludedInProfitShare = true;

			job1.RunPreSaveValidation();
			AssertNoErrors("Precondition: job shouldn't contain errors.", job1);

			var job2 = new Job.Loader(Factory, shipment2).TryCreateWithoutMutexForTestOnly();
			job2.JH_GE = Creator.FEADepartment.PK;
			job2.JH_OA_AgentCollectAddr = Creator.Agent.MainAddress.PK;

			var charge2 = Creator.CreateCharge(job2, Creator.CC3, "Desc", Creator.AUD, 200M, Creator.AALSHI, Creator.AUD, 240M, Creator.Agent);
			charge2.JR_AgentDeclaredSellAmtLocal = 240M;
			charge2.JR_AgentDeclaredCostAmtLocal = 200M;
			charge2.JR_IsIncludedInProfitShare = true;

			job2.RunPreSaveValidation();
			AssertNoErrors("Precondition: job shouldn't contain errors.", job2);

			Factory.Save();

			var poster = new PostOverseasAgentChargesProcessorCreator().CreateOverseasAgentChargesPoster(consol);
			AssertNotNull("Poster must be created", poster);
			var buffer = new NotificationBuffer();
			poster.Process(buffer);
			AssertEquals("Processed for the current company", "Started posting Agent Charges on the EDI - Eagle Datamation International\r\nFinished posting Agent Charges on the EDI - Eagle Datamation International\r\n", buffer.AsString);

			var transactionsPosted = Factory.Load<InvoicingBase>(new ZQuery());
			AssertEquals("Should be four transactions", 4, transactionsPosted.Length);
			AssertNotNull("One should be a Profit Share posted as AP Invoice", transactionsPosted.FirstOrDefault(x => x.AH_Ledger == LedgerTypes.AccountsPayable && x.AH_TransactionType == TransactionTypes.Invoice && x.AH_TransactionNum == "PS C0001"));
		}

		TestObjectCreator Creator
		{
			get { return creator_cached ?? (creator_cached = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator creator_cached;
	}
}
