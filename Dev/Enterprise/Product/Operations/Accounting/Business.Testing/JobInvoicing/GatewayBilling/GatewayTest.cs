using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Billing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.JobInvoicing.GatewayBilling.Testing
{
	public class GatewayTest : TestCaseWithFactory
	{
		[TestDate(2020, 2, 2)]
		public void TestPostingAPToGatewayConsolStoppedIfJRJCouldNotBeCreated()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			var debtorGatewayAgent = TestObjectCreator.CreateOrgHeader("MULTIPROXY", true, true, "AUBNE");

			var appPort1 = debtorGatewayAgent.AppointedGatewayAgentPorts.AddNew();
			appPort1.O5_OA_AgentOfficeAddress = debtorGatewayAgent.MainAddress.PK;
			appPort1.O5_PortOrCountry = "AUBNE";
			appPort1.O5_AgentDirection = AgentDirectionList.Codes.Both;
			appPort1.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
			appPort1.O5_RoadAgentStatus = AgentStatusList.Codes.GatewayAgent;
			appPort1.O5_RailAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var branches = Factory.Load<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK));
			branches.ForEach(x => x.GB_OH_OrgProxy = debtorGatewayAgent.PK);

			Factory.Save();

			var gC0001 = TestObjectCreator.CreateGatewayConsol("AUMEL", "AUBNE", "C0001", receivingGatewayAgent: debtorGatewayAgent);
			gC0001.JK_PrepaidCollect = PaymentType.Collect;

			var s0001 = TestObjectCreator.CreateShipment("S0001", "AUMEL", "USLAX", gC0001, incoTerm: "FOB", housebill: "S0001");

			var gatewayJob = TestObjectCreator.CreateJob(gC0001);
			var s1Job = TestObjectCreator.CreateJob(s0001);

			Factory.Save();

			//preconditions to reproduce the bug
			var gatewaySellCharge = gatewayJob.Charges.AddNew();
			gatewaySellCharge.JR_AC = TestObjectCreator.FRT.PK;
			gatewaySellCharge.JR_OSSellAmt = 555;

			AssertEquals(gatewaySellCharge.SellAccount, debtorGatewayAgent);
			Assert(gatewaySellCharge.InternalFieldsPointToSameEntity());

			var shipmentCostCharge = s1Job.Charges.AddNew();
			shipmentCostCharge.JR_AC = TestObjectCreator.FRT.PK;
			AssertEquals(shipmentCostCharge.SellAccount, debtorGatewayAgent);

			s1Job.Charges.RemoveAndDeleteAll();
			gatewayJob.Charges.RemoveAndDeleteAll();

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var creator2 = new TestObjectCreator(factory2);

			var creditor = creator2.CreateOrgHeader("CREDIT7", true, true);

			var invoice = creator2.CreateInvoice(typeof(APInvoice), RefCurrency.LoadFromCurrencyCode(Factory, "AUD"), organisation: creditor);
			invoice.AH_InvoiceDate = ZDateTime.Now;
			invoice.AH_PostDate = ZDateTime.Now;
			invoice.SubmittedFromInvoicingForm = true;
			creator2.CreateInvoiceLine(invoice, gatewayJob, creator2.FRT, 555m);

			AssertExceptionThrown<CannotSaveAfterCriticalErrorException>("", "One to many job revenue journal cannot be created due to S0001 FRT cost charge internal job, branch, department fields (S0001 empty BRN) don't match gateway billing sell charge (C0001 BNE BRN). This can occur if the system tries to set Debtor and Creditor of the charge both organization proxies", () => factory2.Save());
		}

		#region Consume Existing Charge

		public void TestExistingCharge_WhenDebtorIsOrgProxy_CreateNew()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			//			gC0001		gC0002		C0003		C0004
			//	AUBNE	-	AUSYD	-	SGSIN	-	HKHKG	-	USLAX
			//												 \
			//													C0005
			//														\
			//															USNYC
			//				|-	-	-	-	-	S0001	-	-	-|
			//	|-	-	-	-	-	-	-	S0002	-	-	-	-	-	-|
			//	|-	-	-	S0003	-	-|
			//
			var setup = TestObjectCreator.CreateGatewayConsolsAndShipments();
			var thisCompOrgProxyPK = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			Factory.Save();

			//setup existing charge to consume
			var s1Job = new Job.Loader(setup.s0001).TryLoadOrCreateWithoutMutexForTestOnly();
			var existingCharge = s1Job.Charges.AddNew();
			existingCharge.JR_AC = TestObjectCreator.FRT.PK;
			existingCharge.JR_OSCostAmt = 1111m;
			existingCharge.JR_OSSellAmt = 1500m;
			existingCharge.JR_OH_SellAccount = thisCompOrgProxyPK;

			using (var gatewayJob = TestObjectCreator.CreateJob(setup.gC0002))
			{
				var charge = gatewayJob.Charges.AddNew();
				charge.JR_AC = TestObjectCreator.FRT.PK;
				charge.JR_OH_SellAccount = setup.senAg.PK;
				charge.JR_OSSellAmt = 3333m;
				charge.JR_RX_NKSellCurrency = "AUD";
				charge.JR_JH_InternalJob = gatewayJob.PK;
				charge.JR_GB_InternalBranch = charge.JR_GB;
				charge.JR_GE_InternalDept = charge.JR_GE;

				Assert(existingCharge.JR_OH_CostAccount.IsEmpty);
				Assert(!charge.IsRevenuePosted);

				//save and trigger gtw and jrj creation
				Factory.Save();

				Assert(charge.IsRevenuePosted);
				Assert(!charge.SellRecognition.IsEmpty);

				Assert(!charge.JR_E6_GatewaySellHeader.IsEmpty);
				var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, setup.gC0002.PK));
				AssertEquals(1, consolCosts.Length);
				AssertEquals(charge.JR_E6_GatewaySellHeader, consolCosts[0].PK);

				var consolCost = Factory.Load<JobConsolCost>(charge.JR_E6_GatewaySellHeader);
				AssertEquals(3333m, consolCost.E6_OSCostAmount);

				var s2Job = new JobHeader.Loader(setup.s0002).Load() as Job;
				var s3Job = new JobHeader.Loader(setup.s0003).Load() as Job;

				AssertEquals(2, s1Job.Charges.Count);
				AssertEquals(1, s2Job.Charges.Count);
				AssertEquals(1, s3Job.Charges.Count);

				var s1ChargeExisting = s1Job.Charges.Cast<Charge>().Single(x => x.PK == existingCharge.PK);
				var s1ChargeNew = s1Job.Charges.Cast<Charge>().Single(x => x.PK != existingCharge.PK);
				var s2Charge = s2Job.Charges[0];
				var s3Charge = s3Job.Charges[0];

				AssertEquals(1111m, s1ChargeExisting.JR_OSCostAmt);
				AssertEquals(1500m, s1ChargeExisting.JR_OSSellAmt);
				Assert(!s1ChargeExisting.JR_IsApportioned);
				Assert(!s1ChargeExisting.IsCostPosted);
				Assert(!s1ChargeExisting.IsCostPostedWithJobRevenueJournal);

				AssertEquals(1111m, s1ChargeNew.JR_OSCostAmt);
				AssertEquals(1111m, s2Charge.JR_OSCostAmt);
				AssertEquals(1111m, s3Charge.JR_OSCostAmt);

				Assert(s1ChargeNew.JR_IsApportioned);
				Assert(s1ChargeNew.IsCostPosted);
				Assert(s1ChargeNew.IsCostPostedWithJobRevenueJournal);
				Assert(!s1ChargeNew.CostRecognition.IsEmpty);

				Assert(s2Charge.JR_IsApportioned);
				Assert(s2Charge.IsCostPostedWithJobRevenueJournal);
				Assert(!s3Charge.CostRecognition.IsEmpty);

				Assert(s3Charge.JR_IsApportioned);
				Assert(s3Charge.IsCostPostedWithJobRevenueJournal);
				Assert(!s3Charge.CostRecognition.IsEmpty);

				Assert(charge.IsRevenuePostedWithAutoJobRevenueJournal);
				AssertEquals(1, gatewayJob.Charges.Count);

				//check created JRJ

				var journalsQuery = new ZQuery(AccTransactionHeaderSchema.PK, gatewayJob.Charges.Cast<JobCharge>().Select(x => x.ARLine?.AL_AH).WhereNotNull().ToArray());
				var journal = Factory.Load<JobRevenueJournal>(journalsQuery).Single();

				AssertEquals("GATEWAY SELL APPORTIONMENT C0002", journal.AH_Desc);
				Assert(!journal.AH_IsCancelled);

				var journalLines = journal.Lines.Cast<JobRevenueJournalLine>().ToArray();

				AssertEquals(1, journalLines.Where(x => x.AL_OSAmount > 0).Count());
				AssertEquals(3, journalLines.Where(x => x.AL_OSAmount < 0).Count());

				Assert(consolCost.E6_AH_APInvoice == journal.PK);
			}
		}

		public void TestExistingCharge_WhenDebtorIsSisterOrgProxy_Consume()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			//			gC0001		gC0002		C0003		C0004
			//	AUBNE	-	AUSYD	-	SGSIN	-	HKHKG	-	USLAX
			//												 \
			//													C0005
			//														\
			//															USNYC
			//				|-	-	-	-	-	S0001	-	-	-|
			//	|-	-	-	-	-	-	-	S0002	-	-	-	-	-	-|
			//	|-	-	-	S0003	-	-|
			//
			var setup = TestObjectCreator.CreateGatewayConsolsAndShipments();
			var sisterOrgProxyPK = TestObjectCreator.DebtorSisterOrgProxy.PK;
			Factory.Save();

			//setup existing charge to consume
			var s1Job = new Job.Loader(setup.s0001).TryLoadOrCreateWithoutMutexForTestOnly();
			var existingCharge = s1Job.Charges.AddNew();
			existingCharge.JR_AC = TestObjectCreator.FRT.PK;
			existingCharge.JR_OSCostAmt = 1111m;
			existingCharge.JR_OSSellAmt = 1500m;
			existingCharge.JR_OH_SellAccount = sisterOrgProxyPK;

			using (var gatewayJob = TestObjectCreator.CreateJob(setup.gC0002))
			{
				var charge = gatewayJob.Charges.AddNew();
				charge.JR_AC = TestObjectCreator.FRT.PK;
				charge.JR_OH_SellAccount = setup.senAg.PK;
				charge.JR_OSSellAmt = 3333m;
				charge.JR_RX_NKSellCurrency = "AUD";
				charge.JR_JH_InternalJob = gatewayJob.PK;
				charge.JR_GB_InternalBranch = charge.JR_GB;
				charge.JR_GE_InternalDept = charge.JR_GE;

				Assert(existingCharge.JR_OH_CostAccount.IsEmpty);
				Assert(!charge.IsRevenuePosted);

				//save and trigger gtw and jrj creation
				Factory.Save();

				Assert(charge.IsRevenuePosted);
				Assert(!charge.SellRecognition.IsEmpty);

				Assert(!charge.JR_E6_GatewaySellHeader.IsEmpty);
				var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, setup.gC0002.PK));
				AssertEquals(1, consolCosts.Length);
				AssertEquals(charge.JR_E6_GatewaySellHeader, consolCosts[0].PK);

				var consolCost = Factory.Load<JobConsolCost>(charge.JR_E6_GatewaySellHeader);
				AssertEquals(3333m, consolCost.E6_OSCostAmount);

				var s2Job = new JobHeader.Loader(setup.s0002).Load() as Job;
				var s3Job = new JobHeader.Loader(setup.s0003).Load() as Job;

				AssertEquals(1, s1Job.Charges.Count);
				AssertEquals(1, s2Job.Charges.Count);
				AssertEquals(1, s3Job.Charges.Count);

				var s1Charge = s1Job.Charges[0];
				var s2Charge = s2Job.Charges[0];
				var s3Charge = s3Job.Charges[0];

				AssertEquals(1111m, s1Charge.JR_OSCostAmt);
				AssertEquals(setup.senAg.PK, s1Charge.JR_OH_CostAccount);
				AssertEquals(1500m, s1Charge.JR_OSSellAmt);
				AssertEquals(1111m, s2Charge.JR_OSCostAmt);
				AssertEquals(1111m, s3Charge.JR_OSCostAmt);

				Assert(s1Charge.JR_IsApportioned);
				Assert(s1Charge.IsCostPosted);
				Assert(s1Charge.IsCostPostedWithJobRevenueJournal);
				Assert(!s1Charge.CostRecognition.IsEmpty);

				Assert(s2Charge.JR_IsApportioned);
				Assert(s2Charge.IsCostPostedWithJobRevenueJournal);
				Assert(!s3Charge.CostRecognition.IsEmpty);

				Assert(s3Charge.JR_IsApportioned);
				Assert(s3Charge.IsCostPostedWithJobRevenueJournal);
				Assert(!s3Charge.CostRecognition.IsEmpty);

				Assert(charge.IsRevenuePostedWithAutoJobRevenueJournal);
				AssertEquals(1, gatewayJob.Charges.Count);

				//check created JRJ

				var journalsQuery = new ZQuery(AccTransactionHeaderSchema.PK, gatewayJob.Charges.Cast<JobCharge>().Select(x => x.ARLine?.AL_AH).WhereNotNull().ToArray());
				var journal = Factory.Load<JobRevenueJournal>(journalsQuery).Single();

				AssertEquals("GATEWAY SELL APPORTIONMENT C0002", journal.AH_Desc);
				Assert(!journal.AH_IsCancelled);

				var journalLines = journal.Lines.Cast<JobRevenueJournalLine>().ToArray();

				AssertEquals(1, journalLines.Where(x => x.AL_OSAmount > 0).Count());
				AssertEquals(3, journalLines.Where(x => x.AL_OSAmount < 0).Count());

				Assert(consolCost.E6_AH_APInvoice == journal.PK);
			}
		}

		public void TestExistingCharge_WithMatchingCreditor_Consume()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			//			gC0001		gC0002		C0003		C0004
			//	AUBNE	-	AUSYD	-	SGSIN	-	HKHKG	-	USLAX
			//												 \
			//													C0005
			//														\
			//															USNYC
			//				|-	-	-	-	-	S0001	-	-	-|
			//	|-	-	-	-	-	-	-	S0002	-	-	-	-	-	-|
			//	|-	-	-	S0003	-	-|
			//
			var setup = TestObjectCreator.CreateGatewayConsolsAndShipments();
			Factory.Save();

			//setup existing charge to consume
			var s1Job = new Job.Loader(setup.s0001).TryLoadOrCreateWithoutMutexForTestOnly();
			var existingCharge = s1Job.Charges.AddNew();
			existingCharge.JR_AC = TestObjectCreator.FRT.PK;
			existingCharge.JR_OSCostAmt = 1111m;
			existingCharge.JR_OSSellAmt = 1500m;
			existingCharge.JR_OH_CostAccount = setup.senAg.PK;

			Factory.Save();

			using (var gatewayJob = TestObjectCreator.CreateJob(setup.gC0002))
			{
				var charge = gatewayJob.Charges.AddNew();
				charge.JR_AC = TestObjectCreator.FRT.PK;
				charge.JR_OH_SellAccount = setup.senAg.PK;
				charge.JR_OSSellAmt = 3333m;
				charge.JR_RX_NKSellCurrency = "AUD";
				charge.JR_JH_InternalJob = gatewayJob.PK;
				charge.JR_GB_InternalBranch = charge.JR_GB;
				charge.JR_GE_InternalDept = charge.JR_GE;

				Assert(!charge.IsRevenuePosted);

				//save and trigger gtw and jrj creation
				Factory.Save();

				Assert(charge.IsRevenuePosted);
				Assert(!charge.SellRecognition.IsEmpty);

				Assert(!charge.JR_E6_GatewaySellHeader.IsEmpty);
				var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, setup.gC0002.PK));
				AssertEquals(1, consolCosts.Length);
				AssertEquals(charge.JR_E6_GatewaySellHeader, consolCosts[0].PK);

				var consolCost = Factory.Load<JobConsolCost>(charge.JR_E6_GatewaySellHeader);
				AssertEquals(3333m, consolCost.E6_OSCostAmount);

				var s2Job = new JobHeader.Loader(setup.s0002).Load() as Job;
				var s3Job = new JobHeader.Loader(setup.s0003).Load() as Job;

				AssertEquals(1, s1Job.Charges.Count);
				AssertEquals(1, s2Job.Charges.Count);
				AssertEquals(1, s3Job.Charges.Count);

				var s1Charge = s1Job.Charges[0];
				var s2Charge = s2Job.Charges[0];
				var s3Charge = s3Job.Charges[0];

				AssertEquals(1111m, s1Charge.JR_OSCostAmt);
				AssertEquals(setup.senAg.PK, s1Charge.JR_OH_CostAccount);
				AssertEquals(1500m, s1Charge.JR_OSSellAmt);
				AssertEquals(1111m, s2Charge.JR_OSCostAmt);
				AssertEquals(1111m, s3Charge.JR_OSCostAmt);

				Assert(s1Charge.JR_IsApportioned);
				Assert(s1Charge.IsCostPosted);
				Assert(s1Charge.IsCostPostedWithJobRevenueJournal);
				Assert(!s1Charge.CostRecognition.IsEmpty);

				Assert(s2Charge.JR_IsApportioned);
				Assert(s2Charge.IsCostPostedWithJobRevenueJournal);
				Assert(!s3Charge.CostRecognition.IsEmpty);

				Assert(s3Charge.JR_IsApportioned);
				Assert(s3Charge.IsCostPostedWithJobRevenueJournal);
				Assert(!s3Charge.CostRecognition.IsEmpty);

				Assert(charge.IsRevenuePostedWithAutoJobRevenueJournal);
				AssertEquals(1, gatewayJob.Charges.Count);

				//check created JRJ

				var journalsQuery = new ZQuery(AccTransactionHeaderSchema.PK, gatewayJob.Charges.Cast<JobCharge>().Select(x => x.ARLine?.AL_AH).WhereNotNull().ToArray());
				var journal = Factory.Load<JobRevenueJournal>(journalsQuery).Single();

				AssertEquals("GATEWAY SELL APPORTIONMENT C0002", journal.AH_Desc);
				Assert(!journal.AH_IsCancelled);

				var journalLines = journal.Lines.Cast<JobRevenueJournalLine>().ToArray();

				AssertEquals(1, journalLines.Where(x => x.AL_OSAmount > 0).Count());
				AssertEquals(3, journalLines.Where(x => x.AL_OSAmount < 0).Count());

				Assert(consolCost.E6_AH_APInvoice == journal.PK);
			}
		}

		public void TestExistingCharge_WithEmptyCreditor_Consume()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			//			gC0001		gC0002		C0003		C0004
			//	AUBNE	-	AUSYD	-	SGSIN	-	HKHKG	-	USLAX
			//												 \
			//													C0005
			//														\
			//															USNYC
			//				|-	-	-	-	-	S0001	-	-	-|
			//	|-	-	-	-	-	-	-	S0002	-	-	-	-	-	-|
			//	|-	-	-	S0003	-	-|
			//
			var setup = TestObjectCreator.CreateGatewayConsolsAndShipments();
			Factory.Save();

			//setup existing charge to consume
			var s1Job = new Job.Loader(setup.s0001).TryLoadOrCreateWithoutMutexForTestOnly();
			var existingCharge = s1Job.Charges.AddNew();
			existingCharge.JR_AC = TestObjectCreator.FRT.PK;
			existingCharge.JR_OSCostAmt = 1111m;
			existingCharge.JR_OSSellAmt = 1500m;

			Factory.Save();
			Assert(existingCharge.JR_OH_CostAccount.IsEmpty);
			Assert(existingCharge.JR_OH_SellAccount.IsEmpty);

			using (var gatewayJob = TestObjectCreator.CreateJob(setup.gC0002))
			{
				var charge = gatewayJob.Charges.AddNew();
				charge.JR_AC = TestObjectCreator.FRT.PK;
				charge.JR_OH_SellAccount = setup.senAg.PK;
				charge.JR_OSSellAmt = 3333m;
				charge.JR_RX_NKSellCurrency = "AUD";
				charge.JR_JH_InternalJob = gatewayJob.PK;
				charge.JR_GB_InternalBranch = charge.JR_GB;
				charge.JR_GE_InternalDept = charge.JR_GE;

				Assert(!charge.IsRevenuePosted);

				//save and trigger gtw and jrj creation
				Factory.Save();

				Assert(charge.IsRevenuePosted);
				Assert(!charge.SellRecognition.IsEmpty);

				Assert(!charge.JR_E6_GatewaySellHeader.IsEmpty);
				var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, setup.gC0002.PK));
				AssertEquals(1, consolCosts.Length);
				AssertEquals(charge.JR_E6_GatewaySellHeader, consolCosts[0].PK);

				var consolCost = Factory.Load<JobConsolCost>(charge.JR_E6_GatewaySellHeader);
				AssertEquals(3333m, consolCost.E6_OSCostAmount);

				var s2Job = new JobHeader.Loader(setup.s0002).Load() as Job;
				var s3Job = new JobHeader.Loader(setup.s0003).Load() as Job;

				AssertEquals(1, s1Job.Charges.Count);
				AssertEquals(1, s2Job.Charges.Count);
				AssertEquals(1, s3Job.Charges.Count);

				var s1Charge = s1Job.Charges[0];
				var s2Charge = s2Job.Charges[0];
				var s3Charge = s3Job.Charges[0];

				AssertEquals(1111m, s1Charge.JR_OSCostAmt);
				AssertEquals(setup.senAg.PK, s1Charge.JR_OH_CostAccount);
				AssertEquals(1500m, s1Charge.JR_OSSellAmt);
				AssertEquals(1111m, s2Charge.JR_OSCostAmt);
				AssertEquals(1111m, s3Charge.JR_OSCostAmt);

				Assert(s1Charge.JR_IsApportioned);
				Assert(s1Charge.IsCostPosted);
				Assert(s1Charge.IsCostPostedWithJobRevenueJournal);
				Assert(!s1Charge.CostRecognition.IsEmpty);

				Assert(s2Charge.JR_IsApportioned);
				Assert(s2Charge.IsCostPostedWithJobRevenueJournal);
				Assert(!s3Charge.CostRecognition.IsEmpty);

				Assert(s3Charge.JR_IsApportioned);
				Assert(s3Charge.IsCostPostedWithJobRevenueJournal);
				Assert(!s3Charge.CostRecognition.IsEmpty);

				Assert(charge.IsRevenuePostedWithAutoJobRevenueJournal);
				AssertEquals(1, gatewayJob.Charges.Count);

				//check created JRJ

				var journalsQuery = new ZQuery(AccTransactionHeaderSchema.PK, gatewayJob.Charges.Cast<JobCharge>().Select(x => x.ARLine?.AL_AH).WhereNotNull().ToArray());
				var journal = Factory.Load<JobRevenueJournal>(journalsQuery).Single();

				AssertEquals("GATEWAY SELL APPORTIONMENT C0002", journal.AH_Desc);
				Assert(!journal.AH_IsCancelled);

				var journalLines = journal.Lines.Cast<JobRevenueJournalLine>().ToArray();

				AssertEquals(1, journalLines.Where(x => x.AL_OSAmount > 0).Count());
				AssertEquals(3, journalLines.Where(x => x.AL_OSAmount < 0).Count());

				Assert(consolCost.E6_AH_APInvoice == journal.PK);
			}
		}

		public void TestExistingCharge_WithEmptyCreditorSingleShipment_Consume()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			var prevSenAg = TestObjectCreator.DebtorSisterOrgProxy;

			var gC0002 = TestObjectCreator.CreateGatewayConsol("AUSYD", "SGSIN", "C0002", sendingGatewayCompany: GlbCompany.CurrentCompany);
			var s0001 = TestObjectCreator.CreateShipment("S0001", "AUSYD", "USLAX", gC0002, incoTerm: "CIF", housebill: "S0001");

			gC0002.JK_PrepaidCollect = PaymentType.Prepaid;

			var senAg = gC0002.SendingForwarder ?? Factory.NewWithValidTestData<OrgHeader>();
			gC0002.JK_OA_SendingForwarderAddress = senAg.MainAddress.PK;

			var recAg = gC0002.ReceivingForwarder ?? Factory.NewWithValidTestData<OrgHeader>();
			gC0002.JK_OA_ReceivingForwarderAddress = recAg.MainAddress.PK;

			Factory.Save();

			var s1Job = new Job.Loader(s0001).TryLoadOrCreateWithoutMutexForTestOnly();
			var existingCharge = s1Job.Charges.AddNew();
			existingCharge.JR_AC = TestObjectCreator.FRT.PK;
			existingCharge.JR_OSCostAmt = 1111m;
			existingCharge.JR_OSSellAmt = 1500m;

			Factory.Save();
			Assert(existingCharge.JR_OH_CostAccount.IsEmpty);
			Assert(existingCharge.JR_OH_SellAccount.IsEmpty);

			using (var gatewayJob = TestObjectCreator.CreateJob(gC0002))
			{
				var charge = gatewayJob.Charges.AddNew();
				charge.JR_AC = TestObjectCreator.FRT.PK;
				charge.JR_OH_SellAccount = senAg.PK;
				charge.JR_OSSellAmt = 1111m;
				charge.JR_RX_NKSellCurrency = "AUD";
				charge.JR_JH_InternalJob = gatewayJob.PK;
				charge.JR_GB_InternalBranch = charge.JR_GB;
				charge.JR_GE_InternalDept = charge.JR_GE;

				Assert(!charge.IsRevenuePosted);
				Assert(!existingCharge.IsCostPosted);
				Assert(!existingCharge.JR_IsApportioned);

				//save and trigger gtw and jrj creation
				Factory.Save();

				Assert(existingCharge.JR_IsApportioned);
				Assert(existingCharge.IsCostPosted);
				Assert(charge.IsRevenuePosted);
				Assert(!charge.SellRecognition.IsEmpty);

				Assert(!charge.JR_E6_GatewaySellHeader.IsEmpty);
				var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, gC0002.PK));
				AssertEquals(1, consolCosts.Length);
				AssertEquals(charge.JR_E6_GatewaySellHeader, consolCosts[0].PK);

				var consolCost = Factory.Load<JobConsolCost>(charge.JR_E6_GatewaySellHeader);
				AssertEquals(1111m, consolCost.E6_OSCostAmount);

				AssertEquals(1, s1Job.Charges.Count);

				var s1Charge = s1Job.Charges[0];

				AssertEquals(1111m, s1Charge.JR_OSCostAmt);
				AssertEquals(senAg.PK, s1Charge.JR_OH_CostAccount);
				AssertEquals(1500m, s1Charge.JR_OSSellAmt);

				Assert(s1Charge.JR_IsApportioned);
				Assert(s1Charge.IsCostPosted);
				Assert(s1Charge.IsCostPostedWithJobRevenueJournal);
				Assert(!s1Charge.CostRecognition.IsEmpty);

				Assert(charge.IsRevenuePostedWithAutoJobRevenueJournal);
				AssertEquals(1, gatewayJob.Charges.Count);

				//check created JRJ

				var journalsQuery = new ZQuery(AccTransactionHeaderSchema.PK, gatewayJob.Charges.Cast<JobCharge>().Select(x => x.ARLine?.AL_AH).WhereNotNull().ToArray());
				var journal = Factory.Load<JobRevenueJournal>(journalsQuery).Single();

				AssertEquals("GATEWAY SELL APPORTIONMENT C0002", journal.AH_Desc);
				Assert(!journal.AH_IsCancelled);

				var journalLines = journal.Lines.Cast<JobRevenueJournalLine>().ToArray();

				AssertEquals(1, journalLines.Where(x => x.AL_OSAmount > 0).Count());
				AssertEquals(1, journalLines.Where(x => x.AL_OSAmount < 0).Count());

				Assert(consolCost.E6_AH_APInvoice == journal.PK);
			}
		}

		public void TestExistingCharge_WithOrgProxyCreditorSingleShipment_Consume()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			var gC0002 = TestObjectCreator.CreateGatewayConsol("AUSYD", "SGSIN", "C0002", sendingGatewayCompany: GlbCompany.CurrentCompany);
			var s0001 = TestObjectCreator.CreateShipment("S0001", "AUSYD", "USLAX", gC0002, incoTerm: "CIF", housebill: "S0001");

			gC0002.JK_PrepaidCollect = PaymentType.Prepaid;

			var senAg = gC0002.SendingForwarder ?? Factory.NewWithValidTestData<OrgHeader>();
			gC0002.JK_OA_SendingForwarderAddress = senAg.MainAddress.PK;
			AssertEquals(1, s0001.Gateways.Count);
			AssertEquals(senAg.PK, s0001.Gateways[0].ForwarderPK);

			var recAg = gC0002.ReceivingForwarder ?? Factory.NewWithValidTestData<OrgHeader>();
			gC0002.JK_OA_ReceivingForwarderAddress = recAg.MainAddress.PK;

			//senAg is also a org proxy for a branch
			var newBranch = TestObjectCreator.CreateBranch("NB1", GlbCompany.CurrentCompany, senAg);

			Factory.Save();

			var s1Job = new Job.Loader(s0001).TryLoadOrCreateWithoutMutexForTestOnly();
			var existingCharge = s1Job.Charges.AddNew();
			existingCharge.JR_AC = TestObjectCreator.FRT.PK;
			existingCharge.JR_OSCostAmt = 1111m;
			existingCharge.JR_OSSellAmt = 1500m;
			existingCharge.JR_OH_CostAccount = senAg.PK;

			Factory.Save();
			Assert(!existingCharge.IsCostPosted);

			using (var gatewayJob = TestObjectCreator.CreateJob(gC0002))
			{
				var charge = gatewayJob.Charges.AddNew();
				charge.JR_AC = TestObjectCreator.FRT.PK;
				charge.JR_OH_SellAccount = senAg.PK;
				charge.JR_OSSellAmt = 1111m;
				charge.JR_RX_NKSellCurrency = "AUD";
				charge.JR_JH_InternalJob = gatewayJob.PK;
				charge.JR_GB_InternalBranch = charge.JR_GB;
				charge.JR_GE_InternalDept = charge.JR_GE;

				//save and trigger gtw and jrj creation
				Factory.Save();

				Assert(existingCharge.JR_IsApportioned);
				Assert(existingCharge.IsCostPosted);
				Assert(charge.IsRevenuePosted);
				Assert(!charge.SellRecognition.IsEmpty);

				Assert(!charge.JR_E6_GatewaySellHeader.IsEmpty);
				var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, gC0002.PK));
				AssertEquals(1, consolCosts.Length);
				AssertEquals(charge.JR_E6_GatewaySellHeader, consolCosts[0].PK);

				var consolCost = Factory.Load<JobConsolCost>(charge.JR_E6_GatewaySellHeader);
				AssertEquals(1111m, consolCost.E6_OSCostAmount);

				AssertEquals(1, s1Job.Charges.Count);

				var s1Charge = s1Job.Charges[0];

				AssertEquals(1111m, s1Charge.JR_OSCostAmt);
				AssertEquals(senAg.PK, s1Charge.JR_OH_CostAccount);
				AssertEquals(1500m, s1Charge.JR_OSSellAmt);

				Assert(s1Charge.JR_IsApportioned);
				Assert(s1Charge.IsCostPosted);
				Assert(s1Charge.IsCostPostedWithJobRevenueJournal);
				Assert(!s1Charge.CostRecognition.IsEmpty);

				Assert(charge.IsRevenuePostedWithAutoJobRevenueJournal);
				AssertEquals(1, gatewayJob.Charges.Count);

				//check created JRJ

				var journalsQuery = new ZQuery(AccTransactionHeaderSchema.PK, gatewayJob.Charges.Cast<JobCharge>().Select(x => x.ARLine?.AL_AH).WhereNotNull().ToArray());
				var journal = Factory.Load<JobRevenueJournal>(journalsQuery).Single();

				AssertEquals("GATEWAY SELL APPORTIONMENT C0002", journal.AH_Desc);
				Assert(!journal.AH_IsCancelled);

				var journalLines = journal.Lines.Cast<JobRevenueJournalLine>().ToArray();

				AssertEquals(1, journalLines.Where(x => x.AL_OSAmount > 0).Count());
				AssertEquals(1, journalLines.Where(x => x.AL_OSAmount < 0).Count());

				Assert(consolCost.E6_AH_APInvoice == journal.PK);
			}
		}

		#endregion

		[TestDate(2020, 2, 2)]
		public void TestApportionedUnpostedShipmentChargeDoesNotCreatGTWBatch()
		{
			var debtorGatewayAgent = TestObjectCreator.CreateOrgHeader("MULTIPROXY", true, true, "AUBNE");

			var appPort1 = debtorGatewayAgent.AppointedGatewayAgentPorts.AddNew();
			appPort1.O5_OA_AgentOfficeAddress = debtorGatewayAgent.MainAddress.PK;
			appPort1.O5_PortOrCountry = "AUBNE";
			appPort1.O5_AgentDirection = AgentDirectionList.Codes.Both;
			appPort1.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
			appPort1.O5_RoadAgentStatus = AgentStatusList.Codes.GatewayAgent;
			appPort1.O5_RailAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var branches = Factory.Load<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK));
			branches.ForEach(x => x.GB_OH_OrgProxy = debtorGatewayAgent.PK);

			Factory.Save();

			var gC0001 = TestObjectCreator.CreateGatewayConsol("AUMEL", "AUBNE", "C0001", receivingGatewayAgent: debtorGatewayAgent);
			gC0001.JK_PrepaidCollect = PaymentType.Collect;

			var s0001 = TestObjectCreator.CreateShipment("S0001", "AUMEL", "USLAX", gC0001, incoTerm: "FOB", housebill: "S0001");

			var gatewayJob = TestObjectCreator.CreateJob(gC0001);
			var s1Job = TestObjectCreator.CreateJob(s0001);

			Factory.Save();

			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			var gatewaySellCharge = gatewayJob.Charges.AddNew();
			gatewaySellCharge.JR_AC = TestObjectCreator.FRT.PK;
			gatewaySellCharge.JR_OSSellAmt = 555;

			GatewaySellToCostSynchroniser.Synchronise(gatewayJob);

			AutoJRJRegistryStatusHelper.SetAutoJRJDisabled_ForTestOnly();

			Factory.Save();

			Assert("precondition", !gatewaySellCharge.IsRevenuePosted);
			AssertEquals("precondition", 1, s1Job.Charges.Count);

			var factory2 = new BusinessObjectFactory();
			var s1JobReloaded = new Job.Loader(factory2, factory2.Load<ForwardingShipment>(s0001.PK)).Load();
			var sCharge = s1JobReloaded.Charges[0];

			sCharge.JR_JH_InternalJob = s1Job.PK;
			sCharge.JR_GE_InternalDept = s1Job.JH_GE;
			sCharge.JR_GB_InternalBranch = ZGuid.Empty;

			factory2.Save();

			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			//all the above is necessary to put the system into a state where shipment charge is apportioned but not posted with GTW sell apportionment charge present as per Issue II ESC of the incident
			Assert("precondition", sCharge.JR_IsApportioned);
			Assert("precondition", !sCharge.JR_IsCostPosted);
			Assert("precondition", !sCharge.JR_IsRevenuePosted);
			AssertEquals(sCharge.SellAccount.OH_Code, debtorGatewayAgent.OH_Code);
			AssertEquals(sCharge.CostAccount.OH_Code, debtorGatewayAgent.OH_Code);

			AssertEquals(s1Job.PK, sCharge.JR_JH_InternalJob);
			AssertEquals(ZGuid.Empty, sCharge.JR_GB_InternalBranch);
			AssertEquals(s1Job.JH_GE, sCharge.JR_GE_InternalDept);

			var factory3 = new BusinessObjectFactory();
			var creator3 = new TestObjectCreator(factory3);

			var creditor = creator3.CreateOrgHeader("CREDIT7", true, true);

			var invoice = creator3.CreateInvoice(typeof(APInvoice), RefCurrency.LoadFromCurrencyCode(Factory, "AUD"), organisation: creditor);
			invoice.AH_InvoiceDate = ZDateTime.Now;
			invoice.AH_PostDate = ZDateTime.Now;
			invoice.SubmittedFromInvoicingForm = true;
			creator3.CreateInvoiceLine(invoice, gatewayJob, creator3.CC1, 55m);

			AssertNoExceptionThrown(() => factory3.Save());
		}

		public void TestConsolGetApportionmentsCanHandleTwoTypesOfApportionmentListing()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			//			gC0001		gC0002		C0003		C0004
			//	AUBNE	-	AUSYD	-	SGSIN	-	HKHKG	-	USLAX
			//												 \
			//													C0005
			//														\
			//															USNYC
			//				|-	-	-	-	-	S0001	-	-	-|
			//	|-	-	-	-	-	-	-	S0002	-	-	-	-	-	-|
			//	|-	-	-	S0003	-	-|
			//
			var setup = TestObjectCreator.CreateGatewayConsolsAndShipments();
			Factory.Save();

			using (var gatewayJob = TestObjectCreator.CreateJob(setup.gC0002))
			{
				// Create gateway apportionment
				var charge = gatewayJob.Charges.AddNew();
				charge.JR_AC = TestObjectCreator.FRT.PK;
				charge.JR_OH_SellAccount = setup.senAg.PK;
				charge.JR_OSSellAmt = 3333m;
				charge.JR_RX_NKSellCurrency = "AUD";
				charge.JR_JH_InternalJob = gatewayJob.PK;
				charge.JR_GB_InternalBranch = charge.JR_GB;
				charge.JR_GE_InternalDept = charge.JR_GE;

				Assert(!charge.IsRevenuePosted);
				Factory.Save();
				Assert(charge.IsRevenuePosted);
				Assert(!charge.JR_E6_GatewaySellHeader.IsEmpty);

				// Create non GW apportionment
				var app = setup.gC0002.GetApportionments(false);
				var cost = app.CostsCollection.TryAddNew();
				cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
				cost.E6_GC = GlbCompany.CurrentCompany.PK;
				cost.E6_OSCostAmount = 7777m;
				cost.E6_LocalCostAmount = 7777m;
				Factory.Save();

				var allConsolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, setup.gC0002.PK));
				AssertEquals(2, allConsolCosts.Length);

				var gatewayApportionment = setup.gC0002.GetApportionments(true);
				AssertEquals(1, gatewayApportionment.CostsCollection.Count);
				cost = gatewayApportionment.CostsCollection[0];
				Assert(cost.IsGatewayConsolCost);
				AssertEquals(3333m, cost.E6_LocalCostAmount);

				var nonGatewayApportionment = setup.gC0002.GetApportionments(false);
				AssertEquals(1, nonGatewayApportionment.CostsCollection.Count);
				cost = nonGatewayApportionment.CostsCollection[0];
				Assert(!cost.IsGatewayConsolCost);
				AssertEquals(7777m, cost.E6_LocalCostAmount);
			}
		}

		public void TestGatewaySellApportionmentOneToManyJRJ()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			//			gC0001		gC0002		C0003		C0004
			//	AUBNE	-	AUSYD	-	SGSIN	-	HKHKG	-	USLAX
			//												 \
			//													C0005
			//														\
			//															USNYC
			//				|-	-	-	-	-	S0001	-	-	-|
			//	|-	-	-	-	-	-	-	S0002	-	-	-	-	-	-|
			//	|-	-	-	S0003	-	-|
			//
			var setup = TestObjectCreator.CreateGatewayConsolsAndShipments();
			Factory.Save();

			using (var gatewayJob = TestObjectCreator.CreateJob(setup.gC0002))
			{
				var charge = gatewayJob.Charges.AddNew();
				charge.JR_AC = TestObjectCreator.FRT.PK;
				charge.JR_OH_SellAccount = setup.senAg.PK;
				charge.JR_OSSellAmt = 3333m;
				charge.JR_OSCostAmt = 0m;
				charge.JR_RX_NKSellCurrency = "AUD";
				charge.JR_JH_InternalJob = gatewayJob.PK;
				charge.JR_GB_InternalBranch = charge.JR_GB;
				charge.JR_GE_InternalDept = charge.JR_GE;

				Assert(!charge.IsRevenuePosted);
				Factory.Save();

				Assert(charge.IsRevenuePosted);
				Assert(!charge.SellRecognition.IsEmpty);

				Assert(!charge.JR_E6_GatewaySellHeader.IsEmpty);
				var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, setup.gC0002.PK));
				AssertEquals(1, consolCosts.Length);
				AssertEquals(charge.JR_E6_GatewaySellHeader, consolCosts[0].PK);

				var consolCost = Factory.Load<JobConsolCost>(charge.JR_E6_GatewaySellHeader);
				AssertEquals(3333m, consolCost.E6_OSCostAmount);

				var s1Job = new JobHeader.Loader(setup.s0001).Load() as Job;
				var s2Job = new JobHeader.Loader(setup.s0002).Load() as Job;
				var s3Job = new JobHeader.Loader(setup.s0003).Load() as Job;

				AssertEquals(1, s1Job.Charges.Count);
				AssertEquals(1, s2Job.Charges.Count);
				AssertEquals(1, s3Job.Charges.Count);

				var s1Charge = s1Job.Charges[0];
				var s2Charge = s2Job.Charges[0];
				var s3Charge = s3Job.Charges[0];

				AssertEquals(1111m, s1Charge.JR_OSCostAmt);
				AssertEquals(1111m, s2Charge.JR_OSCostAmt);
				AssertEquals(1111m, s3Charge.JR_OSCostAmt);

				Assert(s1Charge.JR_IsApportioned);
				Assert(s1Charge.IsCostPosted);
				Assert(s1Charge.IsCostPostedWithJobRevenueJournal);
				Assert(!s1Charge.CostRecognition.IsEmpty);

				Assert(s2Charge.JR_IsApportioned);
				Assert(s2Charge.IsCostPostedWithJobRevenueJournal);
				Assert(!s3Charge.CostRecognition.IsEmpty);

				Assert(s3Charge.JR_IsApportioned);
				Assert(s3Charge.IsCostPostedWithJobRevenueJournal);
				Assert(!s3Charge.CostRecognition.IsEmpty);

				Assert(charge.IsRevenuePostedWithAutoJobRevenueJournal);
				AssertEquals(1, gatewayJob.Charges.Count);

				//check created JRJ

				var journalsQuery = new ZQuery(AccTransactionHeaderSchema.PK, gatewayJob.Charges.Cast<JobCharge>().Select(x => x.ARLine?.AL_AH).WhereNotNull().ToArray());
				var journal = Factory.Load<JobRevenueJournal>(journalsQuery).Single();

				AssertEquals("GATEWAY SELL APPORTIONMENT C0002", journal.AH_Desc);
				Assert(!journal.AH_IsCancelled);

				var journalLines = journal.Lines.Cast<JobRevenueJournalLine>().ToArray();

				AssertEquals(1, journalLines.Where(x => x.AL_OSAmount > 0).Count());
				AssertEquals(3, journalLines.Where(x => x.AL_OSAmount < 0).Count());

				Assert(consolCost.E6_AH_APInvoice == journal.PK);

				//reverse JRJ
				new ReversingFactory().NewReversing(journal).Reverse();
				Factory.Save();

				Assert(journal.AH_IsCancelled);

				consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, setup.gC0002.PK));
				AssertEquals(0, consolCosts.Length);

				AssertEquals(0, gatewayJob.Charges.Count);
				AssertEquals(0, s1Job.Charges.Count);
				AssertEquals(0, s2Job.Charges.Count);
				AssertEquals(0, s3Job.Charges.Count);

				var events = AccBillingEventCollector.GetInstance(Factory).GetEvents(AccBillingCodes.GatewayBilling);
				AssertEquals(0, events.Count());

				var summary = AccBillingHandler.GetBillingSummary(Factory, AccBillingCodes.GatewayBilling, setup.gC0002.PK, "JK");
				AssertEquals("BillingCounter", 3, summary.BilledItemCount);
				AssertEquals("Billed Shipments", "S0001, S0002, S0003", summary.BilliedShipmentNumbersAsCSV);
				AssertEquals("BillingHeader", 1, summary.BillingHeaders.Count());

				var header = summary.BillingHeaders.First();
				AssertEquals("Billing Code", "GSH", header.ABH_BillingCode);
				AssertEquals("Billing Counter", 3, header.ABH_BillingCounter);
				AssertEquals("Billing EventType", "APP", header.ABH_EventType);
				AssertEquals("Billing Company PK", GlbCompany.CurrentCompany.PK, header.ABH_GC_Company);
				AssertEquals("Billing Staff", GlbStaff.CurrentUser.GS_Code, header.ABH_GS_NKEventUser);
				AssertEquals("Billing Reference Number", "00001000", header.ABH_InternalReferenceNumber);
				AssertEquals("Billing Parent", setup.gC0002.PK, header.ABH_ParentId);
				AssertEquals("Billing Parent Reference Number", setup.gC0002.JK_UniqueConsignRef, header.ABH_ParentReferenceNumber);
				AssertEquals("Billing Parent Table Code", "JK", header.ABH_ParentTableCode);

				var lineItems = header.BillingItems.OfType<AccBillingItem>().OrderBy(b => b.ABI_ParentReferenceNumber).ToArray();
				AssertEquals("BillingLines", 3, lineItems.Length);

				var line = lineItems[0];
				AssertEquals("Line Parent PK", header.PK, line.ABI_ABH);
				AssertEquals("Shipment PK", setup.s0001.PK, line.ABI_ParentId);
				AssertEquals("Shipment Number", setup.s0001.JS_UniqueConsignRef, line.ABI_ParentReferenceNumber);
				AssertEquals("Shipment Table Code", "JS", line.ABI_ParentTableCode);

				line = lineItems[1];
				AssertEquals("Line Parent PK", header.PK, line.ABI_ABH);
				AssertEquals("Shipment PK", setup.s0002.PK, line.ABI_ParentId);
				AssertEquals("Shipment Number", setup.s0002.JS_UniqueConsignRef, line.ABI_ParentReferenceNumber);
				AssertEquals("Shipment Table Code", "JS", line.ABI_ParentTableCode);

				line = lineItems[2];
				AssertEquals("Line Parent PK", header.PK, line.ABI_ABH);
				AssertEquals("Shipment PK", setup.s0003.PK, line.ABI_ParentId);
				AssertEquals("Shipment Number", setup.s0003.JS_UniqueConsignRef, line.ABI_ParentReferenceNumber);
				AssertEquals("Shipment Table Code", "JS", line.ABI_ParentTableCode);
			}
		}

		public void TestJRJBillinngEventIsAdded()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();
			var setup = TestObjectCreator.CreateGatewayConsolsAndShipments();
			Factory.Save();

			TestObjectCreator.NonCurrentCompanyBranch.GB_OH_OrgProxy = ZGuid.Empty;
			TestObjectCreator.NonCurrentBranch.GB_OH_OrgProxy = setup.prevSenAg.PK;
			var sisBranchJob = TestObjectCreator.CreateJob(setup.s0001, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			sisBranchJob.JH_GB = TestObjectCreator.NonCurrentBranch.PK;
			Factory.Save();

			using (var gatewayJob = TestObjectCreator.CreateJob(setup.gC0002))
			{
				var charge = gatewayJob.Charges.AddNew();
				charge.JR_AC = TestObjectCreator.FRT.PK;
				charge.JR_OH_SellAccount = setup.prevSenAg.PK;
				charge.JR_OSSellAmt = 3333m;
				charge.JR_RX_NKSellCurrency = "AUD";
				charge.JR_JH_InternalJob = sisBranchJob.PK;
				charge.JR_GB_InternalBranch = charge.JR_GB;
				charge.JR_GE_InternalDept = charge.JR_GE;
				Factory.Save();
			}

			var events = AccBillingEventCollector.GetInstance(Factory).GetEvents(AccBillingCodes.GatewayBilling);
			AssertEquals(0, events.Count());

			var summary = AccBillingHandler.GetBillingSummary(Factory, AccBillingCodes.GatewayBilling, setup.gC0002.PK, "JK");
			AssertEquals("BillingCounter", 3, summary.BilledItemCount);
			AssertEquals("Billed Shipments", "S0001, S0002, S0003", summary.BilliedShipmentNumbersAsCSV);
			AssertEquals("BillingHeader", 1, summary.BillingHeaders.Count());

			var header = summary.BillingHeaders.First();
			AssertEquals("Billing Code", "GSH", header.ABH_BillingCode);
			AssertEquals("Billing Counter", 3, header.ABH_BillingCounter);
			AssertEquals("Billing EventType", "JRJ", header.ABH_EventType);
			AssertEquals("Billing Company PK", GlbCompany.CurrentCompany.PK, header.ABH_GC_Company);
			AssertEquals("Billing Staff", GlbStaff.CurrentUser.GS_Code, header.ABH_GS_NKEventUser);
			AssertEquals("Billing Reference Number", "00001000", header.ABH_InternalReferenceNumber);
			AssertEquals("Billing Parent", setup.gC0002.PK, header.ABH_ParentId);
			AssertEquals("Billing Parent Reference Number", setup.gC0002.JK_UniqueConsignRef, header.ABH_ParentReferenceNumber);
			AssertEquals("Billing Parent Table Code", "JK", header.ABH_ParentTableCode);

			var lineItems = header.BillingItems.OfType<AccBillingItem>().OrderBy(b => b.ABI_ParentReferenceNumber).ToArray();
			AssertEquals("BillingLines", 3, lineItems.Length);

			var line = lineItems[0];
			AssertEquals("Line Parent PK", header.PK, line.ABI_ABH);
			AssertEquals("Shipment PK", setup.s0001.PK, line.ABI_ParentId);
			AssertEquals("Shipment Number", setup.s0001.JS_UniqueConsignRef, line.ABI_ParentReferenceNumber);
			AssertEquals("Shipment Table Code", "JS", line.ABI_ParentTableCode);

			line = lineItems[1];
			AssertEquals("Line Parent PK", header.PK, line.ABI_ABH);
			AssertEquals("Shipment PK", setup.s0002.PK, line.ABI_ParentId);
			AssertEquals("Shipment Number", setup.s0002.JS_UniqueConsignRef, line.ABI_ParentReferenceNumber);
			AssertEquals("Shipment Table Code", "JS", line.ABI_ParentTableCode);

			line = lineItems[2];
			AssertEquals("Line Parent PK", header.PK, line.ABI_ABH);
			AssertEquals("Shipment PK", setup.s0003.PK, line.ABI_ParentId);
			AssertEquals("Shipment Number", setup.s0003.JS_UniqueConsignRef, line.ABI_ParentReferenceNumber);
			AssertEquals("Shipment Table Code", "JS", line.ABI_ParentTableCode);
		}

		public void TestAutoJRJDescriptionSetFromRegistry()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();
			var setup = TestObjectCreator.CreateGatewayConsolsAndShipments();
			Factory.Save();

			using (var gatewayJob = TestObjectCreator.CreateJob(setup.gC0002))
			{
				var registryValue = AccountingConfigurationRegistry.Instance.VoucherNumberOfSupportingDocumentDefaults.DefaultValue;
				var autoJRJEntry = registryValue.Cast<DefaultNumberOfSupportingDocuments>().First(x => x.Code == TransactionCategory.Codes.AutoJobRevenueJournal);
				AccountingConfigurationRegistry.Instance.VoucherNumberOfSupportingDocumentDefaults.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue);

				AssertDescription((autoJRJEntry.Description + " C0002").ToUpper());

				autoJRJEntry.Description = (NoResString)"Here is custom text";
				AccountingConfigurationRegistry.Instance.VoucherNumberOfSupportingDocumentDefaults.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue);
				AssertDescription("Here is custom text C0002".ToUpper());

				setup.gC0002.JK_UniqueConsignRef = ZString.Empty;
				AssertDescription("Here is custom text".ToUpper());

				void AssertDescription(ZString expectedDescription)
				{
					var charge = gatewayJob.Charges.AddNew();
					charge.JR_AC = TestObjectCreator.FRT.PK;
					charge.JR_OH_SellAccount = setup.senAg.PK;
					charge.JR_OSSellAmt = 3333m;
					charge.JR_RX_NKSellCurrency = "AUD";
					charge.JR_JH_InternalJob = gatewayJob.PK;
					charge.JR_GB_InternalBranch = charge.JR_GB;
					charge.JR_GE_InternalDept = charge.JR_GE;
					Factory.Save();

					Assert(!charge.JR_E6_GatewaySellHeader.IsEmpty);
					AssertNotNull(charge.ARLine);
					var journal = Factory.Load<JobRevenueJournal>(new ZQuery(AccTransactionHeaderSchema.PK, charge.ARLine.AL_AH)).Single();

					AssertEquals(expectedDescription, journal.AH_Desc);
				}
			}
		}

		public void TestInternalFieldsOnGatewayApportionSplitChargeReadOnly()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			//			gC0001		gC0002		C0003		C0004
			//	AUBNE	-	AUSYD	-	SGSIN	-	HKHKG	-	USLAX
			//												 \
			//													C0005
			//														\
			//															USNYC
			//				|-	-	-	-	-	S0001	-	-	-|
			//	|-	-	-	-	-	-	-	S0002	-	-	-	-	-	-|
			//	|-	-	-	S0003	-	-|
			//
			var setup = TestObjectCreator.CreateGatewayConsolsAndShipments();
			Factory.Save();

			using (var gatewayJob = TestObjectCreator.CreateJob(setup.gC0002))
			{
				var gtwCharge = gatewayJob.Charges.AddNew();
				gtwCharge.JR_AC = TestObjectCreator.FRT.PK;
				gtwCharge.JR_OH_SellAccount = setup.senAg.PK;
				gtwCharge.JR_OSSellAmt = 3333m;
				gtwCharge.JR_RX_NKSellCurrency = "AUD";
				gtwCharge.JR_JH_InternalJob = gatewayJob.PK;
				gtwCharge.JR_GB_InternalBranch = gtwCharge.JR_GB;
				gtwCharge.JR_GE_InternalDept = gtwCharge.JR_GE;

				var listing = setup.gC0002.GetApportionments(true);
				listing.PrepareForConsolCosting();
				var consolCost = Factory.Load<JobConsolCost>(gtwCharge.JR_E6_GatewaySellHeader);
				var apportionmentCharges = consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().ToList();

				AssertEquals(3, apportionmentCharges.Count);

				foreach (var apCharge in apportionmentCharges)
				{
					Assert(apCharge.JR_JH_InternalJobInfo.ReadOnly);
					Assert(apCharge.JR_GB_InternalBranchInfo.ReadOnly);
					Assert(apCharge.JR_GE_InternalDeptInfo.ReadOnly);
				}

				listing.ReleaseMutexes();
			}
		}

		public void TestReverseManyToManyGTWSellApportionmentWithNewOneToManyCode()
		{
			//			gC0001		gC0002		C0003		C0004
			//	AUBNE	-	AUSYD	-	SGSIN	-	HKHKG	-	USLAX
			//												 \
			//													C0005
			//														\
			//															USNYC
			//				|-	-	-	-	-	S0001	-	-	-|
			//	|-	-	-	-	-	-	-	S0002	-	-	-	-	-	-|
			//	|-	-	-	S0003	-	-|
			//
			var setup = TestObjectCreator.CreateGatewayConsolsAndShipments();
			Factory.Save();

			void createJournalAndTransactions(Charge shipmentCharge, Charge gatewayCharge)
			{
				var journal = shipmentCharge.Factory.New<JobRevenueJournal>();
				journal.AH_TransactionCategory = TransactionCategory.Codes.AutoJobRevenueJournal;
				journal.AH_InvoiceDate = ZDateTime.Now;
				journal.AH_PostDate = ZDateTime.Now;
				journal.MarkAsAlreadyTransformed();

				var firstLine = journal.JournalLines.AddNew();
				setValuesOnLineAndLinkToCharge(shipmentCharge, firstLine, true);

				var secondLine = journal.JournalLines.AddNew();
				setValuesOnLineAndLinkToCharge(gatewayCharge, secondLine, false);
			}

			void setValuesOnLineAndLinkToCharge(ChargeWithCost charge, JobRevenueJournalLine line, bool useCostValues)
			{
				line.CostRevenueType = useCostValues ? TransactionLineTypes.Cost : TransactionLineTypes.Revenue;
				line.AL_AC = charge.JR_AC;
				line.AL_JH = charge.JR_JH;
				line.AL_Desc = charge.JR_Desc;
				line.AL_GB = charge.JR_GB;
				line.AL_GE = charge.JR_GE;
				line.AL_RX_NKTransactionCurrency = useCostValues ? charge.JR_RX_NKCostCurrency : charge.JR_RX_NKSellCurrency;

				var osAmount = useCostValues ? charge.JR_OSCostAmt : charge.JR_OSSellAmt;
				var localAmount = useCostValues ? charge.JR_LocalCostAmt : charge.JR_LocalSellAmt;

				line.DebitCreditSign = (useCostValues ^ localAmount < 0) ? DebitCreditDataEntry.DR : DebitCreditDataEntry.CR;
				line.AL_ExchangeRate = Env.CurrentCompany.ExchangeRate.GetRate(localAmount, osAmount);
				line.OSUnsignedLineAmount = Math.Abs(osAmount);

				using (line.GetOSAmountCalculationSuspender())
				{
					line.LocalUnsignedLineAmount = Math.Abs(localAmount);
				}

				using (charge.Calculations.SuspendCalculations())
				{
					if (useCostValues)
					{
						var accrual = charge.Accrual;

						using (accrual.SetReverseDateBeforeUnlinkChargeErrorSuspender.GetSuspender())
						{
							charge.Accrual.AL_ReverseDate = ZDateTime.Now;
							charge.JR_AL_APLine = line.PK;
						}
					}
					else
					{
						charge.JR_AL_ARLine = line.PK;
					}
				}
			}

			Charge createAndSetupGatewayCharge(JobConsolCost consolCost, Job gatewayJob)
			{
				var gatewayCharge = gatewayJob.Charges.AddNew();
				gatewayCharge.JR_AC = TestObjectCreator.FRT.PK;
				gatewayCharge.JR_E6_GatewaySellHeader = consolCost.PK;
				gatewayCharge.JR_OH_SellAccount = setup.senAg.PK;
				gatewayCharge.JR_OSSellAmt = 1001m;
				gatewayCharge.JR_OSCostAmt = 0m;
				gatewayCharge.JR_JH_InternalJob = gatewayJob.PK;
				gatewayCharge.JR_GB_InternalBranch = gatewayCharge.JR_GB;
				gatewayCharge.JR_GE_InternalDept = gatewayCharge.JR_GE;

				return gatewayCharge;
			}

			using (var gatewayJob = TestObjectCreator.CreateJob(setup.gC0002))
			{
				var originalGTWSellCharge = gatewayJob.Charges.AddNew();
				originalGTWSellCharge.JR_AC = TestObjectCreator.FRT.PK;
				originalGTWSellCharge.JR_OH_SellAccount = setup.senAg.PK;
				originalGTWSellCharge.JR_OSSellAmt = 3003m;
				originalGTWSellCharge.JR_OSCostAmt = 0m;
				originalGTWSellCharge.JR_RX_NKSellCurrency = "AUD";
				originalGTWSellCharge.JR_JH_InternalJob = gatewayJob.PK;
				originalGTWSellCharge.JR_GB_InternalBranch = originalGTWSellCharge.JR_GB;
				originalGTWSellCharge.JR_GE_InternalDept = originalGTWSellCharge.JR_GE;

				Factory.Save();

				//Preconditions

				Assert(!originalGTWSellCharge.IsRevenuePosted);

				Assert(!originalGTWSellCharge.JR_E6_GatewaySellHeader.IsEmpty);
				var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, setup.gC0002.PK));
				AssertEquals(1, consolCosts.Length);
				AssertEquals(originalGTWSellCharge.JR_E6_GatewaySellHeader, consolCosts[0].PK);

				var consolCost = Factory.Load<JobConsolCost>(originalGTWSellCharge.JR_E6_GatewaySellHeader);
				AssertEquals(3003m, consolCost.E6_OSCostAmount);

				var s1Job = new JobHeader.Loader(setup.s0001).Load() as Job;
				var s2Job = new JobHeader.Loader(setup.s0002).Load() as Job;
				var s3Job = new JobHeader.Loader(setup.s0003).Load() as Job;

				AssertEquals(1, s1Job.Charges.Count);
				AssertEquals(1, s2Job.Charges.Count);
				AssertEquals(1, s3Job.Charges.Count);

				var s1Charge = s1Job.Charges[0];
				var s2Charge = s2Job.Charges[0];
				var s3Charge = s3Job.Charges[0];

				AssertEquals(1001m, s1Charge.JR_OSCostAmt);
				AssertEquals(1001m, s2Charge.JR_OSCostAmt);
				AssertEquals(1001m, s3Charge.JR_OSCostAmt);

				// replicating auto JRJ posting

				var gatewayCharge1 = createAndSetupGatewayCharge(consolCost, gatewayJob);
				var gatewayCharge2 = createAndSetupGatewayCharge(consolCost, gatewayJob);
				var gatewayCharge3 = createAndSetupGatewayCharge(consolCost, gatewayJob);
				createJournalAndTransactions(s1Charge, gatewayCharge1);
				createJournalAndTransactions(s2Charge, gatewayCharge2);
				createJournalAndTransactions(s3Charge, gatewayCharge3);

				originalGTWSellCharge.JR_OSSellAmt = 0m;

				Factory.Save();

				//Preconditions

				Assert(s1Charge.JR_IsApportioned);
				Assert(s2Charge.JR_IsApportioned);
				Assert(s3Charge.JR_IsApportioned);
				Assert(s1Charge.IsCostPostedWithJobRevenueJournal);
				Assert(s2Charge.IsCostPostedWithJobRevenueJournal);
				Assert(s3Charge.IsCostPostedWithJobRevenueJournal);

				Assert(!originalGTWSellCharge.IsRevenuePostedWithAutoJobRevenueJournal);

				Assert(gatewayCharge1.IsRevenuePostedWithAutoJobRevenueJournal);
				Assert(gatewayCharge2.IsRevenuePostedWithAutoJobRevenueJournal);
				Assert(gatewayCharge3.IsRevenuePostedWithAutoJobRevenueJournal);

				Assert(!originalGTWSellCharge.IsRevenuePosted);
				AssertEquals(4, gatewayJob.Charges.Count);

				var journalsQuery = new ZQuery(AccTransactionHeaderSchema.PK, gatewayJob.Charges.Cast<JobCharge>().Select(x => x.ARLine?.AL_AH).WhereNotNull().ToArray());
				var journals = Factory.Load<JobRevenueJournal>(journalsQuery);
				var journalLines = journals.SelectMany(x => x.Lines).Cast<JobRevenueJournalLine>().ToArray();

				AssertEquals(3, journalLines.Where(x => x.AL_OSAmount > 0).Count());
				AssertEquals(3, journalLines.Where(x => x.AL_OSAmount < 0).Count());

				//reverse JRJ
				var reversingFactory = new ReversingFactory();

				foreach (var journal in journals)
				{
					var reversing = reversingFactory.NewReversing(journal);
					reversing.Reverse();
				}

				Factory.Save();

				consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, setup.gC0002.PK));
				AssertEquals(0, consolCosts.Length);

				AssertEquals(1, gatewayJob.Charges.Count);
				AssertEquals(0, s1Job.Charges.Count);
				AssertEquals(0, s2Job.Charges.Count);
				AssertEquals(0, s3Job.Charges.Count);
			}
		}

		public void TestDetachingShipmentInvalidatesRelatedJobNumber()
		{
			var setup = TestObjectCreator.CreateGatewayConsolsAndShipments();
			Factory.Save();

			SetupGatewayChargeDefaultInvoiceTargetJobConfigurationRegistry();
			using (var gatewayJobHeader = TestObjectCreator.CreateJob(setup.gC0002))
			{
				var charge = gatewayJobHeader.Charges.AddNew();
				charge.JR_Calc_RelatedJobNumber = "S0002";
				charge.JR_OH_SellAccount = TestObjectCreator.DebtorSisterOrgProxy.PK;

				charge.RunPreSaveValidation();

				AssertEquals("S0002", charge.JR_Calc_RelatedJobNumber);
				AssertEquals("S0002", charge.JR_Calc_InvoiceTarget);

				AssertNoNotifications(charge.JR_Calc_RelatedJobNumberInfo);
				AssertNoNotifications(charge.JR_Calc_InvoiceTargetInfo);

				setup.gC0002.Shipments.Remove(setup.s0002);
				charge.RunPreSaveValidation();

				AssertHasError(charge.JR_Calc_RelatedJobNumberInfo, "Enter a valid Related Job Number.");
				AssertHasError(charge.JR_Calc_InvoiceTargetInfo, "Enter a valid Intercompany Invoice Target Job.");
			}
		}

		public void TestInvoiceTargetCacheStaleness()
		{
			//			gC0001		gC0002		C0003		C0004
			//	AUBNE	-	AUSYD	-	SGSIN	-	HKHKG	-	USLAX
			//												 \
			//													C0005
			//														\
			//															USNYC
			//				|-	-	-	-	-	S0001	-	-	-|
			//	|-	-	-	-	-	-	-	S0002	-	-	-	-	-	-|
			//	|-	-	-	S0003	-	-|
			//
			var setup = TestObjectCreator.CreateGatewayConsolsAndShipments();
			Factory.Save();

			using (var gatewayJobHeader = TestObjectCreator.CreateJob(setup.gC0002))
			{
				var charge = gatewayJobHeader.Charges.AddNew();
				charge.JR_Calc_RelatedJobNumber = "S0001";

				Func<(string, string)[]> actual = () => charge.Lookups.InvoiceTargetJobNumbers.Cast<ICodeDescription>().Select(x => (x.Code, x.Description)).ToArray();

				var expected = Array.Empty<(string, string)>();
				AssertArrayEqualsByElements(expected, actual());

				charge.JR_OH_SellAccount = TestObjectCreator.DebtorSisterOrgProxy.PK;

				expected = new[]
				{
					("S0001", "AUSYD - USLAX"),
					("C0002", "AUSYD - SGSIN"),
					("C0003", "SGSIN - HKHKG"),
					("C0004", "HKHKG - USLAX")
				};

				AssertArrayEqualsByElements(expected, actual());
			}
		}

		public void TestInvoiceTargetJobNumbersLookups()
		{
			//			gC0001		gC0002		C0003		C0004
			//	AUBNE	-	AUSYD	-	SGSIN	-	HKHKG	-	USLAX
			//												 \
			//													C0005
			//														\
			//															USNYC
			//				|-	-	-	-	-	S0001	-	-	-|
			//	|-	-	-	-	-	-	-	S0002	-	-	-	-	-	-|
			//	|-	-	-	S0003	-	-|
			//
			var setup = TestObjectCreator.CreateGatewayConsolsAndShipments();
			Factory.Save();

			using (var gatewayJobHeader = TestObjectCreator.CreateJob(setup.gC0002))
			{
				var charge = gatewayJobHeader.Charges.AddNew();
				charge.JR_OH_SellAccount = TestObjectCreator.DebtorSisterOrgProxy.PK;
				var charge2 = gatewayJobHeader.Charges.AddNew();
				charge2.JR_OH_SellAccount = TestObjectCreator.DebtorSisterOrgProxy.PK;

				charge.JR_Calc_RelatedJobNumber = "S0001";
				charge2.JR_Calc_RelatedJobNumber = "S0002";

				var expected = new[]
				{
					("S0001", "AUSYD - USLAX"),
					("C0002", "AUSYD - SGSIN"),
					("C0003", "SGSIN - HKHKG"),
					("C0004", "HKHKG - USLAX")
				};

				var expected2 = new[]
				{
					("S0002", "AUBNE - USNYC"),
					("C0001", "AUBNE - AUSYD"),
					("C0002", "AUSYD - SGSIN"),
					("C0003", "SGSIN - HKHKG"),
					("C0005", "HKHKG - USNYC"),
				};

				var actual = charge.Lookups.InvoiceTargetJobNumbers.Cast<ICodeDescription>().Select(x => (x.Code, x.Description)).ToArray();
				var actual2 = charge2.Lookups.InvoiceTargetJobNumbers.Cast<ICodeDescription>().Select(x => (x.Code, x.Description)).ToArray();
				AssertArrayEqualsByElements(expected, actual);
				AssertArrayEqualsByElements(expected2, actual2);

				charge.JR_Calc_RelatedJobNumber = "S0002";
				charge2.JR_Calc_RelatedJobNumber = "S0003";
				actual = charge.Lookups.InvoiceTargetJobNumbers.Cast<ICodeDescription>().Select(x => (x.Code, x.Description)).ToArray();
				actual2 = charge2.Lookups.InvoiceTargetJobNumbers.Cast<ICodeDescription>().Select(x => (x.Code, x.Description)).ToArray();

				expected = new[]
				{
					("S0002", "AUBNE - USNYC"),
					("C0001", "AUBNE - AUSYD"),
					("C0002", "AUSYD - SGSIN"),
					("C0003", "SGSIN - HKHKG"),
					("C0005", "HKHKG - USNYC"),
				};

				expected2 = new[]
				{
					("S0003", "AUBNE - SGSIN"),
					("C0001", "AUBNE - AUSYD"),
					("C0002", "AUSYD - SGSIN"),
				};

				AssertArrayEqualsByElements(expected, actual);
				AssertArrayEqualsByElements(expected2, actual2);

				charge.JR_Calc_RelatedJobNumber = "S0003";
				actual = charge.Lookups.InvoiceTargetJobNumbers.Cast<ICodeDescription>().Select(x => (x.Code, x.Description)).ToArray();

				expected = new[]
				{
					("S0003", "AUBNE - SGSIN"),
					("C0001", "AUBNE - AUSYD"),
					("C0002", "AUSYD - SGSIN"),
				};

				AssertArrayEqualsByElements(expected, actual);
			}
		}

		public void TestInvoiceTargetJobDefaultingDirectionTransportModePriorities()
		{
			//			gC0001		gC0002		C0003		C0004
			//	AUBNE	-	AUSYD	-	SGSIN	-	HKHKG	-	USLAX
			//												 \
			//													C0005
			//														\
			//															USNYC
			//				|-	-	-	-	-	S0001	-	-	-|
			//	|-	-	-	-	-	-	-	S0002	-	-	-	-	-	-|
			//	|-	-	-	S0003	-	-|
			//

			var setting3 = SetupInvoiceTargeJobDefaultingRegistry(("ALL", "ALL", "GTT", "REL"));      // 3rd priority
			var setting2 = SetupInvoiceTargeJobDefaultingRegistry(("EXP", "ALL", "GTT", "PCL"));      // 2nd priority
			var setting1 = SetupInvoiceTargeJobDefaultingRegistry(("EXP", "AIR", "GTT", "SCL"));        // 1st priority
			var dataSetup = TestObjectCreator.CreateGatewayConsolsAndShipments();
			dataSetup.gC0001.JK_SendingForwarderHandlingType = "GTT";
			Factory.Save();

			using (var job = TestObjectCreator.CreateJob(dataSetup.gC0002))
			{
				var charge = TestObjectCreator.CreateCharge(job, chargeCode: TestObjectCreator.FRT, osSellAmt: 100m, debtor: TestObjectCreator.DebtorSisterOrgProxy);
				var previousConsol = job.GetInvoicingSupporter().GetPreviousConsol("S0002");
				dataSetup.gC0001.JK_OA_SendingForwarderAddress = TestObjectCreator.DebtorSisterOrgProxy.MainAddress.PK;
				AssertEquals("C0001", previousConsol.InvoicingSupporter.ConsolNumber);
				AssertNotNull("previous consol sending agent is NOT null", dataSetup.gC0001.SendingForwarder);
				dataSetup.gC0001.JK_SendingForwarderHandlingType = "GTT";
				charge.JR_Calc_RelatedJobNumber = "S0002";
				charge.JR_OH_SellAccount = TestObjectCreator.DebtorSisterOrgProxy.PK;
				AssertEquals("C0002", charge.JR_Calc_InvoiceTarget);      //1st priority

				setting1.Dispose();
				charge.JR_OH_SellAccount = ZGuid.Empty;
				charge.JR_OH_SellAccount = TestObjectCreator.DebtorSisterOrgProxy.PK;
				AssertEquals("C0001", charge.JR_Calc_InvoiceTarget);      //2nd priority

				setting2.Dispose();
				charge.JR_OH_SellAccount = ZGuid.Empty;
				charge.JR_OH_SellAccount = TestObjectCreator.DebtorSisterOrgProxy.PK;
				AssertEquals("S0002", charge.JR_Calc_InvoiceTarget);      //3rd priority

				setting3.Dispose();
				charge.JR_OH_SellAccount = ZGuid.Empty;
				charge.JR_OH_SellAccount = TestObjectCreator.DebtorSisterOrgProxy.PK;
				Assert(charge.JR_Calc_InvoiceTarget.IsEmpty);      //registry default
			}
		}

		public void TestInvoiceTargetJobDefaulting()
		{
			//			gC0001		gC0002		C0003		C0004
			//	AUBNE	-	AUSYD	-	SGSIN	-	HKHKG	-	USLAX
			//												 \
			//													C0005
			//														\
			//															USNYC
			//				|-	-	-	-	-	S0001	-	-	-|
			//	|-	-	-	-	-	-	-	S0002	-	-	-	-	-	-|
			//	|-	-	-	S0003	-	-|
			//
			var dataSetup = TestObjectCreator.CreateGatewayConsolsAndShipments();
			Factory.Save();

			SetupGatewayChargeDefaultInvoiceTargetJobConfigurationRegistry();

			using (var job = TestObjectCreator.CreateJob(dataSetup.gC0002))
			{
				var charge = job.Charges.AddNew();
				charge.JR_AC = TestObjectCreator.FRT.PK;
				charge.JR_OSSellAmt = 100m;
				charge.JR_RX_NKSellCurrency = "AUD";
				charge.JR_Calc_RelatedJobNumber = string.Empty;
				AssertEquals(string.Empty, charge.JR_Calc_InvoiceTarget);

				charge.JR_OH_SellAccount = TestObjectCreator.CreateOrgHeader("NotAProxy", false, true, true, false, false, false, false).PK;
				charge.JR_Calc_RelatedJobNumber = "ABC";
				AssertEquals(string.Empty, charge.JR_Calc_InvoiceTarget);

				//Test 1 - invalid related job number
				var charge1 = TestObjectCreator.CreateCharge(job, chargeCode: TestObjectCreator.FRT, osSellAmt: 100m, debtor: TestObjectCreator.DebtorSisterOrgProxy);
				charge1.JR_Calc_RelatedJobNumber = "Error";
				AssertEquals(string.Empty, charge1.JR_Calc_InvoiceTarget);

				//Test 2 - previous consol exists, sending agent does not exists, NON => SCL
				var charge2 = TestObjectCreator.CreateCharge(job, chargeCode: TestObjectCreator.FRT, osSellAmt: 100m, debtor: TestObjectCreator.DebtorSisterOrgProxy);
				var previousConsol = job.GetInvoicingSupporter().GetPreviousConsol("S0002");
				AssertEquals("C0001", previousConsol.InvoicingSupporter.ConsolNumber);
				dataSetup.gC0001.JK_OA_SendingForwarderAddress = ZGuid.Empty;
				AssertNull("previous consol sending agent is null", dataSetup.gC0001.SendingForwarder);
				charge2.JR_Calc_RelatedJobNumber = "S0002";
				charge2.JR_OH_SellAccount = TestObjectCreator.DebtorSisterOrgProxy.PK;
				AssertEquals("C0002", charge2.JR_Calc_InvoiceTarget);

				//Test 3 - previous consol exists, sending agent exists AND type is blank, SGT => REL
				var charge3 = TestObjectCreator.CreateCharge(job, chargeCode: TestObjectCreator.FRT, osSellAmt: 100m, debtor: TestObjectCreator.DebtorSisterOrgProxy);
				previousConsol = job.GetInvoicingSupporter().GetPreviousConsol("S0002");
				dataSetup.gC0001.JK_OA_SendingForwarderAddress = TestObjectCreator.DebtorSisterOrgProxy.MainAddress.PK;
				AssertEquals("C0001", previousConsol.InvoicingSupporter.ConsolNumber);
				AssertNotNull("previous consol sending agent is NOT null", dataSetup.gC0001.SendingForwarder);
				AssertEquals(ZString.Empty, dataSetup.gC0001.JK_SendingForwarderHandlingType);
				charge3.JR_Calc_RelatedJobNumber = "S0002";
				charge3.JR_OH_SellAccount = TestObjectCreator.DebtorSisterOrgProxy.PK;
				AssertEquals("S0002", charge3.JR_Calc_InvoiceTarget);

				//Test 4 - previous consol exists, sending agent exists AND type is GTA, GTA => REL
				var charge4 = TestObjectCreator.CreateCharge(job, chargeCode: TestObjectCreator.FRT, osSellAmt: 100m, debtor: TestObjectCreator.DebtorSisterOrgProxy);
				previousConsol = job.GetInvoicingSupporter().GetPreviousConsol("S0002");
				dataSetup.gC0001.JK_OA_SendingForwarderAddress = TestObjectCreator.DebtorSisterOrgProxy.MainAddress.PK;
				AssertEquals("C0001", previousConsol.InvoicingSupporter.ConsolNumber);
				AssertNotNull("previous consol sending agent is NOT null", dataSetup.gC0001.SendingForwarder);
				dataSetup.gC0001.JK_SendingForwarderHandlingType = "GTA";
				charge4.JR_Calc_RelatedJobNumber = "S0002";
				charge4.JR_OH_SellAccount = TestObjectCreator.DebtorSisterOrgProxy.PK;
				AssertEquals("S0002", charge4.JR_Calc_InvoiceTarget);

				//Test 5 - previous consol exists, sending agent exists AND type is GTT, GTT => PCL
				var charge5 = TestObjectCreator.CreateCharge(job, chargeCode: TestObjectCreator.FRT, osSellAmt: 100m, debtor: TestObjectCreator.DebtorSisterOrgProxy);
				previousConsol = job.GetInvoicingSupporter().GetPreviousConsol("S0002");
				dataSetup.gC0001.JK_OA_SendingForwarderAddress = TestObjectCreator.DebtorSisterOrgProxy.MainAddress.PK;
				AssertEquals("C0001", previousConsol.InvoicingSupporter.ConsolNumber);
				AssertNotNull("previous consol sending agent is NOT null", dataSetup.gC0001.SendingForwarder);
				dataSetup.gC0001.JK_SendingForwarderHandlingType = "GTT";
				charge5.JR_Calc_RelatedJobNumber = "S0002";
				charge5.JR_OH_SellAccount = TestObjectCreator.DebtorSisterOrgProxy.PK;
				AssertEquals("C0001", charge5.JR_Calc_InvoiceTarget);

				//Test 6 - previous consol does NOT exist, NON => SCL
				var charge6 = TestObjectCreator.CreateCharge(job, chargeCode: TestObjectCreator.FRT, osSellAmt: 100m, debtor: TestObjectCreator.DebtorSisterOrgProxy);
				dataSetup.s0002.Consols.Remove(dataSetup.gC0001);
				previousConsol = job.GetInvoicingSupporter().GetPreviousConsol("S0002");
				AssertNull(previousConsol);
				charge6.JR_Calc_RelatedJobNumber = "S0002";
				charge6.JR_OH_SellAccount = TestObjectCreator.DebtorSisterOrgProxy.PK;
				AssertEquals("C0002", charge6.JR_Calc_InvoiceTarget);
			}
		}

		public void TestInvoiceTargetWhenSellAccountChange()
		{
			var consol = TestObjectCreator.CreateGatewayConsol("AUSYD", "SGSIN", "C0001", receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment = TestObjectCreator.CreateShipment("S0001");
			consol.Shipments.Add(shipment);
			Factory.Save();

			var collection = new GatewayChargeDefaultInvoiceTargetJobConfigurationCollection();
			var setting1 = collection.AddNew();
			setting1.ConsolDirection = "ALL";
			setting1.ConsolTransportMode = "ALL";
			setting1.PreviousSendingAgentType = "ALL";
			setting1.InvoiceTargetJobType = "SCL";
			AccountingMasterFilesRegistry.Instance.GatewayChargeDefaultInvoiceTargetJobConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			using (var job = TestObjectCreator.CreateJob(consol))
			{
				var charge = job.Charges.AddNew();
				charge.JR_AC = TestObjectCreator.FRT.PK;
				charge.JR_OSSellAmt = 100m;
				charge.JR_RX_NKSellCurrency = "AUD";
				charge.JR_Calc_RelatedJobNumber = shipment.JS_UniqueConsignRef;
				AssertNullOrEmpty(charge.JR_Calc_InvoiceTarget);

				charge.JR_OH_SellAccount = ZGuid.Empty;
				AssertNullOrEmpty("Expect empty invoice target since debtor is missing", charge.JR_Calc_InvoiceTarget);

				charge.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
				AssertNullOrEmpty("Expect empty invoice target since debtor is NOT proxy of another company", charge.JR_Calc_InvoiceTarget);

				charge.JR_OH_SellAccount = TestObjectCreator.DebtorSisterOrgProxy.PK;
				AssertEquals("Expect invoice target is not empty when debtor is proxy of another company", consol.JK_UniqueConsignRef, charge.JR_Calc_InvoiceTarget);
			}
		}

		public void TestTargetJobNumberResetOnDebtorOrRelatedJobChange()
		{
			var sisterOrgProxy = TestObjectCreator.DebtorSisterOrgProxy;

			//			gC0001		gC0002		C0003		C0004
			//	AUBNE	-	AUSYD	-	SGSIN	-	HKHKG	-	USLAX
			//												 \
			//													C0005
			//														\
			//															USNYC
			//				|-	-	-	-	-	S0001	-	-	-|
			//	|-	-	-	-	-	-	-	S0002	-	-	-	-	-	-|
			//	|-	-	-	S0003	-	-|
			//
			var setup = TestObjectCreator.CreateGatewayConsolsAndShipments();
			Factory.Save();

			SetupGatewayChargeDefaultInvoiceTargetJobConfigurationRegistry();

			using (var consolJobHeader = TestObjectCreator.CreateJob(setup.gC0002))
			{
				var orgCC = Factory.LoadTop1<AccChargeCode>(new ZQuery(new ZQuery(AccChargeCodeSchema.AC_Code, "OCAA"), new ZQuery(AccChargeCodeSchema.AC_GC, Env.CurrentCompanyPK)));
				setup.gC0001.JK_OA_SendingForwarderAddress = sisterOrgProxy.MainAddress.PK;

				var charge = consolJobHeader.Charges.AddNew();
				charge.JR_AC = orgCC.PK;
				charge.JR_Calc_RelatedJobNumber = "S0002";
				charge.JR_Calc_InvoiceTarget = "C0001";

				Assert(!charge.JR_Calc_InvoiceTarget.IsEmpty);

				charge.JR_Calc_RelatedJobNumber = "S0001";
				charge.JR_OH_SellAccount = sisterOrgProxy.PK;
				AssertEquals(setup.gC0002.JK_UniqueConsignRef, charge.JR_Calc_InvoiceTarget);

				charge.JR_Calc_RelatedJobNumber = "S0003";
				charge.JR_Calc_InvoiceTarget = "C0001";
				Assert(!charge.JR_Calc_InvoiceTarget.IsEmpty);

				charge.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
				Assert(charge.JR_Calc_InvoiceTarget.IsEmpty);

				charge.JR_OH_SellAccount = ZGuid.Empty;
				Assert(charge.JR_Calc_InvoiceTarget.IsEmpty);

				charge.JR_OH_SellAccount = ZGuid.Invalid;
				Assert(charge.JR_Calc_InvoiceTarget.IsEmpty);

				charge.JR_OH_SellAccount = TestObjectCreator.DebtorSisterOrgProxy.PK;
				AssertEquals("S0003", charge.JR_Calc_InvoiceTarget);
			}
		}

		public void TestInvoiceTargetJobNumberReadOnly()
		{
			SetupDebtorDefaultingRegistry(("ALL", "ALL", "ORG", "PPD", "SHP", "SGT", "PSA"));

			var sisterOrgProxy = TestObjectCreator.DebtorSisterOrgProxy;

			//			gC0001		gC0002		C0003		C0004
			//	AUBNE	-	AUSYD	-	SGSIN	-	HKHKG	-	USLAX
			//												 \
			//													C0005
			//														\
			//															USNYC
			//				|-	-	-	-	-	S0001	-	-	-|
			//	|-	-	-	-	-	-	-	S0002	-	-	-	-	-	-|
			//	|-	-	-	S0003	-	-|
			//
			var setup = TestObjectCreator.CreateGatewayConsolsAndShipments();
			var acc = TestObjectCreator.CreateChargeCode("GTB");
			acc.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			Factory.Save();

			using (var consolJobHeader = TestObjectCreator.CreateJob(setup.gC0002))
			{
				var taxRate = TestObjectCreator.CreateTaxRate("TAX1", "desc", 5);

				var charge = consolJobHeader.Charges.AddNew();
				charge.JR_AC = acc.PK;
				charge.JR_OSSellAmt = 600m;
				charge.JR_RX_NKSellCurrency = "AUD";
				charge.JR_RX_NKCostCurrency = "AUD";
				charge.JR_OH_SellAccount = sisterOrgProxy.PK;
				charge.JR_SellRatingOverride = false;

				Assert(!charge.IsRevenuePosted);
				Assert(charge.JR_Calc_InvoiceTargetInfo.ReadOnly);

				charge.JR_Calc_RelatedJobNumber = "S0002";
				Assert(!charge.JR_Calc_InvoiceTargetInfo.ReadOnly);

				charge.JR_Calc_RelatedJobNumber = "XXXX";
				Assert(charge.JR_Calc_InvoiceTargetInfo.ReadOnly);

				charge.JR_Calc_RelatedJobNumber = "S0003";
				Assert(!charge.JR_Calc_InvoiceTargetInfo.ReadOnly);

				charge.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
				Assert(charge.JR_Calc_InvoiceTargetInfo.ReadOnly);

				charge.JR_OH_SellAccount = sisterOrgProxy.PK;
				Assert(!charge.JR_Calc_InvoiceTargetInfo.ReadOnly);

				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("123", TestObjectCreator.AUD, 1m, sisterOrgProxy);
				var transactionLine = invoice.Lines.AddNew() as InvoicingLineBase;
				transactionLine.AL_AH = invoice.PK;
				transactionLine.AL_LineType = TransactionLineTypes.Revenue;
				transactionLine.AL_LineAmount = -600;
				transactionLine.AL_OSAmount = -600;
				transactionLine.AL_RX_NKTransactionCurrency = "AUD";
				transactionLine.AL_RevRecognitionType = "IMM";

				charge.JR_AL_ARLine = transactionLine.PK;
				charge.JR_AT_CostGSTRate = taxRate.PK;

				Assert(charge.IsRevenuePosted);
				Assert(charge.JR_Calc_InvoiceTargetInfo.ReadOnly);
			}
		}

		#region Internal Job Defaulting

		public void TestInternalJobDefaultingWithRegistrySetting()
		{
			//			        C0001 & S0001
			//	USCHI (GTT)	          \     
			//
			//  		     C0002 & S0002			    C0004 & S0001, S0002, S0003, S004
			//	USATL (GTA)		    -		 USLAX(GTT)                -                   AUSYD	
			//		
			//	
			//	USHOU (SGT)            /
			//			       C0003 & S0003
			//

			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			var orgProxyCHI = TestObjectCreator.CreateOrgHeader("YOUUNICHI", true, true);
			var branchCHI = TestObjectCreator.CreateBranch("CHI", "Chicago", GlbCompany.CurrentCompany, orgProxyCHI);

			var appPort1 = orgProxyCHI.AppointedGatewayAgentPorts.AddNew();
			appPort1.O5_OA_AgentOfficeAddress = orgProxyCHI.MainAddress.PK;
			appPort1.O5_PortOrCountry = "USCHI";
			appPort1.O5_AgentDirection = AgentDirectionList.Codes.Both;
			appPort1.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;
			appPort1.O5_RoadAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;
			appPort1.O5_RailAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;

			var orgProxyATL = TestObjectCreator.CreateOrgHeader("YOUUNIATL", true, true);
			var branchATL = TestObjectCreator.CreateBranch("ATL", "Atlanta", GlbCompany.CurrentCompany, orgProxyATL);

			var appPort2 = orgProxyATL.AppointedGatewayAgentPorts.AddNew();
			appPort2.O5_OA_AgentOfficeAddress = orgProxyATL.MainAddress.PK;
			appPort2.O5_PortOrCountry = "USATL";
			appPort2.O5_AgentDirection = AgentDirectionList.Codes.Both;
			appPort2.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
			appPort2.O5_RoadAgentStatus = AgentStatusList.Codes.GatewayAgent;
			appPort2.O5_RailAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var orgProxyHOU = TestObjectCreator.CreateOrgHeader("YOUUNIHOU", true, true);
			var branchHOU = TestObjectCreator.CreateBranch("HOU", "Houston", GlbCompany.CurrentCompany, orgProxyHOU);

			var orgProxyLAX = TestObjectCreator.CreateOrgHeader("YOUUNILAX", true, true);
			var branchLAX = TestObjectCreator.CreateBranch("LAX", "Los Angeles", GlbCompany.CurrentCompany, orgProxyLAX);

			var consol1 = TestObjectCreator.CreateGatewayConsol("USCHI", "USLAX", "C0001", sendingGatewayAgent: orgProxyCHI, receivingGatewayCompany: GlbCompany.CurrentCompany, receivingGatewayAgent: orgProxyLAX);
			var shipment1 = TestObjectCreator.CreateShipment("S0001", "USCHI", "AUSYD", consol1);
			var consolJob1 = TestObjectCreator.CreateJob(consol1);
			var shipmentJob1 = TestObjectCreator.CreateJob(shipment1);

			var consol2 = TestObjectCreator.CreateGatewayConsol("USATL", "USLAX", "C0002", sendingGatewayAgent: orgProxyATL, receivingGatewayCompany: GlbCompany.CurrentCompany, receivingGatewayAgent: orgProxyLAX);
			var shipment2 = TestObjectCreator.CreateShipment("S0002", "USATL", "AUSYD", consol2);
			var consolJob2 = TestObjectCreator.CreateJob(consol2);
			var shipmentJob2 = TestObjectCreator.CreateJob(shipment2);

			var consol3 = TestObjectCreator.CreateGatewayConsol("USHOU", "USLAX", "C0003", receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment3 = TestObjectCreator.CreateShipment("S0003", "USHOU", "AUSYD", consol3);
			var consolJob3 = TestObjectCreator.CreateJob(consol3);
			var shipmentJob3 = TestObjectCreator.CreateJob(shipment3);

			var consol4 = TestObjectCreator.CreateGatewayConsol("USLAX", "AUSYD", "C0004", receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment4 = TestObjectCreator.CreateShipment("S0004", "USLAX", "AUSYD", consol4);
			var shipmentJob4 = TestObjectCreator.CreateJob(shipment4);

			consol4.Shipments.Add(shipment1);
			consol4.Shipments.Add(shipment2);
			consol4.Shipments.Add(shipment3);

			Factory.Save();

			SetupGatewayChargeDefaultInvoiceTargetJobConfigurationRegistry();

			using (var job = TestObjectCreator.CreateJob(consol4))
			{
				Factory.Save();

				// Previous sending agent is GTT --> PCL
				var charge = job.Charges.AddNew();
				var previousConsol = job.GetInvoicingSupporter().GetPreviousConsol("S0001");
				consol1.JK_OA_SendingForwarderAddress = orgProxyCHI.MainAddress.PK;
				AssertEquals("C0001", previousConsol.InvoicingSupporter.ConsolNumber);
				AssertNotNull("previous consol sending agent is NOT null", consol1.SendingForwarder);
				consol1.JK_SendingForwarderHandlingType = "GTT";
				charge.JR_Calc_RelatedJobNumber = shipment1.JobNumber;
				charge.JR_OH_SellAccount = branchCHI.OrgProxy.PK;
				AssertEquals(previousConsol.InvoicingSupporter.Job.PK, charge.JR_JH_InternalJob);
				AssertEquals(previousConsol.InvoicingSupporter.Job.JH_GB, charge.JR_GB_InternalBranch);
				AssertEquals(previousConsol.InvoicingSupporter.Job.JH_GE, charge.JR_GE_InternalDept);

				// Previous sending agent is GTA --> REL
				var charge2 = job.Charges.AddNew();
				previousConsol = job.GetInvoicingSupporter().GetPreviousConsol("S0002");
				consol2.JK_OA_SendingForwarderAddress = orgProxyATL.MainAddress.PK;
				AssertEquals("C0002", previousConsol.InvoicingSupporter.ConsolNumber);
				AssertNotNull("previous consol sending agent is NOT null", consol2.SendingForwarder);
				consol2.JK_SendingForwarderHandlingType = "GTA";
				charge2.JR_Calc_RelatedJobNumber = shipment2.JobNumber;
				charge2.JR_OH_SellAccount = branchATL.OrgProxy.PK;
				AssertEquals(shipment2.Job.PK, charge2.JR_JH_InternalJob);
				AssertEquals(shipment2.Job.JH_GB, charge2.JR_GB_InternalBranch);
				AssertEquals(shipment2.Job.JH_GE, charge2.JR_GE_InternalDept);

				// Previous sending agent is SGT --> REL
				var charge3 = job.Charges.AddNew();
				previousConsol = job.GetInvoicingSupporter().GetPreviousConsol("S0003");
				consol3.JK_OA_SendingForwarderAddress = orgProxyHOU.MainAddress.PK;
				AssertEquals("C0003", previousConsol.InvoicingSupporter.ConsolNumber);
				AssertNotNull("previous consol sending agent is NOT null", consol3.SendingForwarder);
				AssertEquals(ZString.Empty, consol3.JK_SendingForwarderHandlingType);
				charge3.JR_Calc_RelatedJobNumber = shipment3.JobNumber;
				charge3.JR_OH_SellAccount = branchHOU.OrgProxy.PK;
				AssertEquals(shipment3.Job.PK, charge3.JR_JH_InternalJob);
				AssertEquals(shipment3.Job.JH_GB, charge3.JR_GB_InternalBranch);
				AssertEquals(shipment3.Job.JH_GE, charge3.JR_GE_InternalDept);

				// Previous sending agent is NON --> SCL
				var charge4 = job.Charges.AddNew();
				previousConsol = job.GetInvoicingSupporter().GetPreviousConsol("S0004");
				AssertNull(previousConsol);
				charge4.JR_Calc_RelatedJobNumber = shipment4.JobNumber;
				charge4.JR_OH_SellAccount = branchLAX.OrgProxy.PK;
				AssertEquals(consol4.InvoicingSupporter.Job.PK, charge4.JR_JH_InternalJob);
				AssertEquals(consol4.InvoicingSupporter.Job.JH_GB, charge4.JR_GB_InternalBranch);
				AssertEquals(consol4.InvoicingSupporter.Job.JH_GE, charge4.JR_GE_InternalDept);
			}
		}

		public void TestInternalJobDefaultingWhenNoRegistrySettingOrJRJDisabled()
		{
			//		 
			//			     C0001 & S0001			    C0002 & S0001, S0002
			//	USCHI (GTA)		    -		 USLAX(GTT)                -            AUSYD	
			//			

			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			var orgProxyCHI = TestObjectCreator.CreateOrgHeader("YOUUNICHI", true, true);
			var branchCHI = TestObjectCreator.CreateBranch("CHI", "Chicago", GlbCompany.CurrentCompany, orgProxyCHI);

			var appPort1 = orgProxyCHI.AppointedGatewayAgentPorts.AddNew();
			appPort1.O5_OA_AgentOfficeAddress = orgProxyCHI.MainAddress.PK;
			appPort1.O5_PortOrCountry = "USCHI";
			appPort1.O5_AgentDirection = AgentDirectionList.Codes.Both;
			appPort1.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;
			appPort1.O5_RoadAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;
			appPort1.O5_RailAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;

			var orgProxyLAX = TestObjectCreator.CreateOrgHeader("YOUUNILAX", true, true);
			var branchLAX = TestObjectCreator.CreateBranch("LAX", "Los Angeles", GlbCompany.CurrentCompany, orgProxyLAX);

			var consol1 = TestObjectCreator.CreateGatewayConsol("USCHI", "USLAX", "C0001", sendingGatewayAgent: orgProxyCHI, receivingGatewayCompany: GlbCompany.CurrentCompany, receivingGatewayAgent: orgProxyLAX);
			var shipment1 = TestObjectCreator.CreateShipment("S0001", "USCHI", "AUSYD", consol1);
			var consolJob1 = TestObjectCreator.CreateJob(consol1);
			var shipmentJob1 = TestObjectCreator.CreateJob(shipment1);

			var consol2 = TestObjectCreator.CreateGatewayConsol("USLAX", "AUSYD", "C0004", receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment2 = TestObjectCreator.CreateShipment("S0004", "USLAX", "AUSYD", consol2);
			var shipmentJob2 = TestObjectCreator.CreateJob(shipment2);

			consol2.Shipments.Add(shipment1);

			Factory.Save();

			using (var job = TestObjectCreator.CreateJob(consol2))
			{
				// There is no configuration in the Registry Setting  - Related Shipment/Existing Logic
				var charge = job.Charges.AddNew();
				charge.JR_Calc_RelatedJobNumber = shipment1.JobNumber;
				charge.JR_OH_SellAccount = branchCHI.OrgProxy.PK;
				charge.JR_AC = TestObjectCreator.FRT.PK;
				charge.JR_OSSellAmt = 3333m;
				charge.JR_RX_NKSellCurrency = "USD";
				Factory.Save();
				AssertEquals(shipment1.Job.PK, charge.JR_JH_InternalJob);
				AssertEquals(true, charge.IsRevenuePosted);

				// Internal Job Default Setting disbaled - Related Shipment/Existing Logic
				AutoJRJRegistryStatusHelper.SetAutoJRJDisabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());
				var charge2 = job.Charges.AddNew();
				charge2.JR_Calc_RelatedJobNumber = shipment1.JobNumber;
				charge2.JR_OH_SellAccount = branchCHI.OrgProxy.PK;
				charge2.JR_AC = TestObjectCreator.FRT.PK;
				charge2.JR_OSSellAmt = 3333m;
				charge2.JR_RX_NKSellCurrency = "USD";
				Factory.Save();
				AssertEquals(shipment1.Job.PK, charge.JR_JH_InternalJob);

				// Auto JRJ is disabled
				AccountingMasterFilesRegistry.Instance.GetInternalJobConfigurationSetting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				AutoJRJRegistryStatusHelper.SetAutoJRJDisabled_ForTestOnly();
				var charge3 = job.Charges.AddNew();
				charge3.JR_Calc_RelatedJobNumber = shipment1.JobNumber;
				charge3.JR_OH_SellAccount = branchCHI.OrgProxy.PK;
				charge3.JR_AC = TestObjectCreator.FRT.PK;
				charge3.JR_OSSellAmt = 3333m;
				charge3.JR_RX_NKSellCurrency = "USD";
				Factory.Save();
				AssertEquals(false, charge3.IsRevenuePosted);
			}
		}

		public void TestInternalJobDefaultingWithDefaultDebtorConfiguration()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			var sendingAgent = TestObjectCreator.CreateOrgHeader("YOUUNICHI", true, true);
			_ = TestObjectCreator.CreateBranch("CHI", "Chicago", GlbCompany.CurrentCompany, sendingAgent);

			var appPort1 = sendingAgent.AppointedGatewayAgentPorts.AddNew();
			appPort1.O5_OA_AgentOfficeAddress = sendingAgent.MainAddress.PK;
			appPort1.O5_PortOrCountry = "USCHI";
			appPort1.O5_AgentDirection = AgentDirectionList.Codes.Both;
			appPort1.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;
			appPort1.O5_RoadAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;
			appPort1.O5_RailAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;

			var receivingAgent = TestObjectCreator.CreateOrgHeader("YOUSGSIN", true, true);
			_ = TestObjectCreator.CreateBranch("SGB", "Singapore", GlbCompany.CurrentCompany, receivingAgent);
			var receivingAgentCompany = TestObjectCreator.CreateNewCompany("SGA", orgProxy: receivingAgent);

			var appPort2 = receivingAgent.AppointedGatewayAgentPorts.AddNew();
			appPort2.O5_OA_AgentOfficeAddress = receivingAgent.MainAddress.PK;
			appPort2.O5_PortOrCountry = "SGSIN";
			appPort2.O5_AgentDirection = AgentDirectionList.Codes.Both;
			appPort2.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;
			appPort2.O5_RoadAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;
			appPort2.O5_RailAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;

			var gatewayConsol = TestObjectCreator.CreateGatewayConsol("USCHI", "SGSIN", "C0001", receivingGatewayCompany: receivingAgentCompany);
			var shipment = TestObjectCreator.CreateShipment("S0001", "USCHI", "SGSIN", gatewayConsol);
			_ = TestObjectCreator.CreateJob(shipment);
			Assert("Precondition: Is gateway consol", gatewayConsol.IsGateway());

			Factory.Save();

			// ("ALL", "ALL", "DST", "ALL", "ALL", "ALL", "RGT") --> This setup already in the default config of the DebtorDefaultingRegistry

			var collection = new GatewayChargeDefaultInvoiceTargetJobConfigurationCollection();
			var setting = collection.AddNew();
			setting.ConsolDirection = "ALL";
			setting.ConsolTransportMode = "ALL";
			setting.PreviousSendingAgentType = "ALL";
			setting.InvoiceTargetJobType = "REL";
			AccountingMasterFilesRegistry.Instance.GatewayChargeDefaultInvoiceTargetJobConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			AccountingMasterFilesRegistry.Instance.GetInternalJobConfigurationSetting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			using (var job = TestObjectCreator.CreateJob(gatewayConsol))
			{
				Factory.Save();

				var dstChargeCode = TestObjectCreator.CreateChargeCode("DAWB");
				dstChargeCode.AC_ChargeGroup = "DST";

				var charge = job.Charges.AddNew();
				charge.JR_AC = dstChargeCode.PK;
				AssertEquals("Debtor should be set as Receiving Agent", receivingAgent.PK, charge.JR_OH_SellAccount);

				charge.JR_Calc_RelatedJobNumber = shipment.JobNumber;
				AssertEquals(shipment.Job.PK, charge.JR_JH_InternalJob);
				AssertEquals(shipment.Job.JH_GB, charge.JR_GB_InternalBranch);
				AssertEquals(shipment.Job.JH_GE, charge.JR_GE_InternalDept);
			}
		}

		#endregion

		#region Branch Defaulting

		public void TestGatewayBranchDefaulting()
		{
			AccChargeCode LoadCC(string ac_code)
			{
				var ccQuery = new ZQuery(AccChargeCodeSchema.AC_Code, ac_code);
				ccQuery.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);

				var cc = Factory.LoadTop1<AccChargeCode>(ccQuery);
				return cc;
			}

			void setupChargeBranchDefaulting(string ac_code, string direction, string transportMode, string rule, string branchCode = "")
			{
				var cc = LoadCC(ac_code);
				var setting = cc.BranchOverrides.AddNew();
				setting.YA_Direction = direction;
				setting.YA_TransportMode = transportMode;
				setting.YA_DefaultingRule = rule;
				setting.YA_JobType = JobInvoicingConsumerTypes.GatewayConsolCode;

				if (!string.IsNullOrEmpty(branchCode))
				{
					setting.YA_GB_SpecificBranch = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, branchCode).PK;
				}
			}

			void assertCCBranch(string ac_code, ForwardingConsol c, string expectedBranchCode)
			{
				var cc = LoadCC(ac_code);
				using (var job = new Job.Loader(c).TryLoadOrCreateWithoutMutexForTestOnly())
				{
					var charge = job.Charges.AddNew();
					charge.JR_AC = cc.PK;

					AssertEquals($"{ac_code}-{c.JK_UniqueConsignRef}-{c.TransportMode}", expectedBranchCode, charge.Branch.GB_Code);
				}
			}

			setupChargeBranchDefaulting("CAF", "ALL", "AIR", "SBA", "SYD");
			setupChargeBranchDefaulting("CAF", "IMP", "ALL", "SBA", "BNE");
			setupChargeBranchDefaulting("BAF", "EXP", "ALL", "SNA");
			setupChargeBranchDefaulting("BAF", "EXP", "ROA", "RCA");

			var agSyd = TestObjectCreator.CreateOrgHeader("SA_SYD", true, true, "AUSYD");
			var agSin = TestObjectCreator.CreateOrgHeader("AG_SIN", true, true, "SGSIN");

			var sinBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, "TES"));
			sinBranch.GB_OH_OrgProxy = agSin.PK;

			var sydBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, "SYD"));
			sydBranch.GB_OH_OrgProxy = agSyd.PK;

			var appPort1 = agSyd.AppointedGatewayAgentPorts.AddNew();
			appPort1.O5_OA_AgentOfficeAddress = agSyd.MainAddress.PK;
			appPort1.O5_PortOrCountry = "AUSYD";
			appPort1.O5_AgentDirection = AgentDirectionList.Codes.Both;
			appPort1.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
			appPort1.O5_RoadAgentStatus = AgentStatusList.Codes.GatewayAgent;
			appPort1.O5_RailAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var appPort2 = agSin.AppointedGatewayAgentPorts.AddNew();
			appPort2.O5_OA_AgentOfficeAddress = agSin.MainAddress.PK;
			appPort2.O5_PortOrCountry = "SGSIN";
			appPort2.O5_AgentDirection = AgentDirectionList.Codes.Both;
			appPort2.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
			appPort2.O5_RoadAgentStatus = AgentStatusList.Codes.GatewayAgent;
			appPort2.O5_RailAgentStatus = AgentStatusList.Codes.GatewayAgent;

			Factory.Save();

			var expAirConsol = TestObjectCreator.CreateGatewayConsol("AUSYD", "SGSIN", consolNum: "CE01", sendingGatewayAgent: agSyd, receivingGatewayCompany: sinBranch.Company, receivingGatewayAgent: agSin, transportMode: "AIR");
			var impAirConsol = TestObjectCreator.CreateGatewayConsol("SGSIN", "AUSYD", consolNum: "CI01", sendingGatewayCompany: sinBranch.Company, sendingGatewayAgent: agSin, receivingGatewayAgent: agSyd, transportMode: "AIR");
			var expRoaConsol = TestObjectCreator.CreateGatewayConsol("AUSYD", "SGSIN", consolNum: "CE02", sendingGatewayAgent: agSyd, receivingGatewayCompany: sinBranch.Company, receivingGatewayAgent: agSin, transportMode: "ROA");
			var impRaiConsol = TestObjectCreator.CreateGatewayConsol("SGSIN", "AUSYD", consolNum: "CI02", sendingGatewayCompany: sinBranch.Company, sendingGatewayAgent: agSin, receivingGatewayAgent: agSyd, transportMode: "RAI");

			assertCCBranch("CAF", expAirConsol, "SYD");
			assertCCBranch("CAF", impAirConsol, "BNE");
			assertCCBranch("CAF", expRoaConsol, "SYD");
			assertCCBranch("CAF", impRaiConsol, "BNE");
			assertCCBranch("BAF", expAirConsol, "SYD");
			assertCCBranch("BAF", impAirConsol, "SYD");
			assertCCBranch("BAF", expRoaConsol, "TES");
			assertCCBranch("BAF", impRaiConsol, "SYD");
		}

		#endregion

		#region Debtor Defaulting

		public void TestDebtorDefaultingDirectionTransportModePriorities()
		{
			var setting3 = SetupDebtorDefaultingRegistry(("ALL", "ALL", "ORG", "ALL", "SHP", "ALL", "PSA"));      // 3rd priority
			var setting2 = SetupDebtorDefaultingRegistry(("EXP", "ALL", "ORG", "ALL", "SHP", "ALL", "RGT"));      // 2nd priority
			var setting1 = SetupDebtorDefaultingRegistry(("EXP", "AIR", "ORG", "ALL", "SHP", "ALL", "SPA"));        // 1st priority
			var setup = TestObjectCreator.CreateGatewayConsolsAndShipments();
			setup.gC0001.JK_SendingForwarderHandlingType = "GTT";
			Factory.Save();
			using (var consolJobHeader = TestObjectCreator.CreateJob(setup.gC0002))
			{
				var orgCC = Factory.LoadTop1<AccChargeCode>(new ZQuery(new ZQuery(AccChargeCodeSchema.AC_Code, "OCAA"), new ZQuery(AccChargeCodeSchema.AC_GC, Env.CurrentCompanyPK)));

				var charge = consolJobHeader.Charges.AddNew();
				charge.JR_AC = orgCC.PK;

				charge.JR_Calc_RelatedJobNumber = "S0002";
				AssertEquals(setup.shPicAg.PK, charge.JR_OH_SellAccount);       //1st priority

				setting1.Dispose();
				charge.JR_OH_SellAccount = ZGuid.Empty;
				charge.JR_Calc_RelatedJobNumber = ZString.Empty;

				charge.JR_Calc_RelatedJobNumber = "S0002";
				AssertEquals(setup.recAg.PK, charge.JR_OH_SellAccount);      //2nd priority

				setting2.Dispose();
				charge.JR_OH_SellAccount = ZGuid.Empty;
				charge.JR_Calc_RelatedJobNumber = ZString.Empty;

				charge.JR_Calc_RelatedJobNumber = "S0002";
				AssertEquals(setup.prevSenAg.PK, charge.JR_OH_SellAccount);      //3rd priority

				setting3.Dispose();
				charge.JR_OH_SellAccount = ZGuid.Empty;
				charge.JR_Calc_RelatedJobNumber = ZString.Empty;

				charge.JR_Calc_RelatedJobNumber = "S0002";
				AssertEquals(setup.senAg.PK, charge.JR_OH_SellAccount);      //registry default
			}
		}

		public void TestDebtorDefaultingPaymentTermRelatedJobPriorities()
		{
			var setting3 = SetupDebtorDefaultingRegistry(("ALL", "ALL", "ORG", "ALL", "SHP", "GTT", "PSA"));      // 3rd priority
			var setting2 = SetupDebtorDefaultingRegistry(("ALL", "ALL", "ORG", "PPD", "ALL", "ALL", "RGT"));      // 2nd priority
			var setting1 = SetupDebtorDefaultingRegistry(("ALL", "ALL", "ORG", "PPD", "SHP", "ALL", "SPA"));		// 1st priority
			SetupDebtorDefaultingRegistry(("ALL", "ALL", "ORG", "ALL", "SHP", "GTA", "RGT"));						// NA (GTA)
			SetupDebtorDefaultingRegistry(("ALL", "ALL", "ORG", "CCX", "ALL", "ALL", "RGT"));						// NA (CCX)

			//			gC0001		gC0002		C0003		C0004
			//	AUBNE	-	AUSYD	-	SGSIN	-	HKHKG	-	USLAX
			//												 \
			//													C0005
			//														\
			//															USNYC
			//				|-	-	-	-	-	S0001	-	-	-|
			//	|-	-	-	-	-	-	-	S0002	-	-	-	-	-	-|
			//	|-	-	-	S0003	-	-|
			//
			var setup = TestObjectCreator.CreateGatewayConsolsAndShipments("PPD");
			setup.gC0001.JK_SendingForwarderHandlingType = "GTT";
			Factory.Save();

			using (var consolJobHeader = TestObjectCreator.CreateJob(setup.gC0002))
			{
				var orgCC = Factory.LoadTop1<AccChargeCode>(new ZQuery(new ZQuery(AccChargeCodeSchema.AC_Code, "OCAA"), new ZQuery(AccChargeCodeSchema.AC_GC, Env.CurrentCompanyPK)));

				var charge = consolJobHeader.Charges.AddNew();
				charge.JR_AC = orgCC.PK;

				charge.JR_Calc_RelatedJobNumber = "S0002";
				AssertEquals(setup.shPicAg.PK, charge.JR_OH_SellAccount);		//1st priority

				setting1.Dispose();
				charge.JR_OH_SellAccount = ZGuid.Empty;
				charge.JR_Calc_RelatedJobNumber = ZString.Empty;

				charge.JR_Calc_RelatedJobNumber = "S0002";
				AssertEquals(setup.recAg.PK, charge.JR_OH_SellAccount);      //2nd priority

				setting2.Dispose();
				charge.JR_OH_SellAccount = ZGuid.Empty;
				charge.JR_Calc_RelatedJobNumber = ZString.Empty;

				charge.JR_Calc_RelatedJobNumber = "S0002";
				AssertEquals(setup.prevSenAg.PK, charge.JR_OH_SellAccount);      //3rd priority

				setting3.Dispose();
				charge.JR_OH_SellAccount = ZGuid.Empty;
				charge.JR_Calc_RelatedJobNumber = ZString.Empty;

				charge.JR_Calc_RelatedJobNumber = "S0002";
				AssertEquals(setup.senAg.PK, charge.JR_OH_SellAccount);      //registry default
			}
		}

		public void TestDebtorDefaultingChargeGroupPaymentTermPriorities()
		{
			var setting3 = SetupDebtorDefaultingRegistry(("ALL", "ALL", "ALL", "CCX", "SHP", "GTA", "PSA"));      // NA, overridden by ORG registry default
			var setting2 = SetupDebtorDefaultingRegistry(("ALL", "ALL", "ORG", "ALL", "SHP", "GTA", "SDA"));      // 2nd priority
			var setting1 = SetupDebtorDefaultingRegistry(("ALL", "ALL", "ORG", "CCX", "SHP", "GTA", "SPA"));      // 1st priority
			SetupDebtorDefaultingRegistry(("ALL", "ALL", "ALL", "CCX", "SHP", "GTT", "RGT"));                     // NA (GTT)
			SetupDebtorDefaultingRegistry(("ALL", "ALL", "ORG", "PPD", "ALL", "ALL", "RGT"));                     // NA (PPD)

			//			gC0001		gC0002		C0003		C0004
			//	CNSHA	-	SGSIN	-	AUSYD	-	USLAX	-	USCHI
			//												 \
			//													C0005
			//														\
			//															USNYC
			//				|-	-	-	-	-	S0001	-	-	-|
			//	|-	-	-	-	-	-	-	S0002	-	-	-	-	-	-|
			//	|-	-	-	S0003	-	-|
			//
			var setup = TestObjectCreator.CreateGatewayConsolsAndShipments("CCX");
			setup.gC0001.JK_SendingForwarderHandlingType = "GTA";
			Factory.Save();

			using (var consolJobHeader = TestObjectCreator.CreateJob(setup.gC0002))
			{
				var orgCC = Factory.LoadTop1<AccChargeCode>(new ZQuery(new ZQuery(AccChargeCodeSchema.AC_Code, "OCAA"), new ZQuery(AccChargeCodeSchema.AC_GC, Env.CurrentCompanyPK)));

				var charge = consolJobHeader.Charges.AddNew();
				charge.JR_AC = orgCC.PK;

				charge.JR_Calc_RelatedJobNumber = "S0002";
				AssertEquals(setup.shPicAg.PK, charge.JR_OH_SellAccount);      //1st priority

				setting1.Dispose();
				charge.JR_OH_SellAccount = ZGuid.Empty;
				charge.JR_Calc_RelatedJobNumber = ZString.Empty;

				charge.JR_Calc_RelatedJobNumber = "S0002";
				AssertEquals(setup.shDelAg.PK, charge.JR_OH_SellAccount);      //2nd priority

				setting2.Dispose();
				charge.JR_OH_SellAccount = ZGuid.Empty;
				charge.JR_Calc_RelatedJobNumber = ZString.Empty;

				charge.JR_Calc_RelatedJobNumber = "S0002";
				AssertEquals(setup.senAg.PK, charge.JR_OH_SellAccount);      //registry default

				setting3.Dispose();
				charge.JR_OH_SellAccount = ZGuid.Empty;
				charge.JR_Calc_RelatedJobNumber = ZString.Empty;

				charge.JR_Calc_RelatedJobNumber = "S0002";
				AssertEquals(setup.senAg.PK, charge.JR_OH_SellAccount);      //registry default
			}
		}

		public void TestDebtorDefaultingRelatedJobPrevSAPriorities()
		{
			var setting3 = SetupDebtorDefaultingRegistry(("ALL", "ALL", "ORG", "PPD", "ALL", "ALL", "RGT"));      // 3rd priority
			var setting2 = SetupDebtorDefaultingRegistry(("ALL", "ALL", "ORG", "PPD", "SHP", "ALL", "SDA"));      // 2nd priority
			var setting1 = SetupDebtorDefaultingRegistry(("ALL", "ALL", "ORG", "PPD", "SHP", "SGT", "PSA"));      // 1st priority
			SetupDebtorDefaultingRegistry(("ALL", "ALL", "ORG", "PPD", "SHP", "NON", "PSA"));                     // NA (NON)
			SetupDebtorDefaultingRegistry(("ALL", "ALL", "ORG", "CCX", "SHP", "ALL", "PSA"));                     // NA (CCX)

			//			gC0001		gC0002		C0003		C0004
			//	AUBNE	-	AUSYD	-	SGSIN	-	HKHKG	-	USLAX
			//												 \
			//													C0005
			//														\
			//															USNYC
			//				|-	-	-	-	-	S0001	-	-	-|
			//	|-	-	-	-	-	-	-	S0002	-	-	-	-	-	-|
			//	|-	-	-	S0003	-	-|
			//
			var setup = TestObjectCreator.CreateGatewayConsolsAndShipments("PPD");
			Factory.Save();

			using (var consolJobHeader = TestObjectCreator.CreateJob(setup.gC0002))
			{
				var orgCC = Factory.LoadTop1<AccChargeCode>(new ZQuery(new ZQuery(AccChargeCodeSchema.AC_Code, "OCAA"), new ZQuery(AccChargeCodeSchema.AC_GC, Env.CurrentCompanyPK)));

				var charge = consolJobHeader.Charges.AddNew();
				charge.JR_AC = orgCC.PK;

				charge.JR_Calc_RelatedJobNumber = "S0002";
				AssertEquals(setup.prevSenAg.PK, charge.JR_OH_SellAccount);      //1st priority

				setting1.Dispose();
				charge.JR_OH_SellAccount = ZGuid.Empty;
				charge.JR_Calc_RelatedJobNumber = ZString.Empty;

				charge.JR_Calc_RelatedJobNumber = "S0002";
				AssertEquals(setup.shDelAg.PK, charge.JR_OH_SellAccount);      //2nd priority

				setting2.Dispose();
				charge.JR_OH_SellAccount = ZGuid.Empty;
				charge.JR_Calc_RelatedJobNumber = ZString.Empty;

				charge.JR_Calc_RelatedJobNumber = "S0002";
				AssertEquals(setup.recAg.PK, charge.JR_OH_SellAccount);      //3rd priority

				setting3.Dispose();
				charge.JR_OH_SellAccount = ZGuid.Empty;
				charge.JR_Calc_RelatedJobNumber = ZString.Empty;

				charge.JR_Calc_RelatedJobNumber = "S0002";
				AssertEquals(setup.senAg.PK, charge.JR_OH_SellAccount);      //registry default
			}
		}

		public static IDisposable SetupDebtorDefaultingRegistry((string consolDirectoin, string consolTransportMode, string chargeCodeGroup, string paymentTerm, string relatedJob, string prevSA, string debtor) config)
			=> TestObjectCreator.SetupDebtorDefaultingRegistry(config);

		public static IDisposable SetupInvoiceTargeJobDefaultingRegistry((string consolDirectoin, string consolTransportMode, string previousSendingAgentType, string invoiceTargetJobType) config)
			=> TestObjectCreator.SetupInvoiceTargeJobDefaultingRegistry(config);

		#endregion

		void SetupGatewayChargeDefaultInvoiceTargetJobConfigurationRegistry()
		{
			// Registry setting: Prev sending agent - Invoice Target Job
			// SGT - REL
			// GTA - REL
			// GTT - PCL
			// NON - SCL
			// ALL - SCL
			var collection = new GatewayChargeDefaultInvoiceTargetJobConfigurationCollection();
			var setting1 = collection.AddNew();
			setting1.ConsolDirection = "ALL";
			setting1.ConsolTransportMode = "ALL";
			setting1.PreviousSendingAgentType = "SGT";
			setting1.InvoiceTargetJobType = "REL";
			var setting2 = collection.AddNew();
			setting2.ConsolDirection = "ALL";
			setting2.ConsolTransportMode = "ALL";
			setting2.PreviousSendingAgentType = "GTA";
			setting2.InvoiceTargetJobType = "REL";
			var setting3 = collection.AddNew();
			setting3.ConsolDirection = "ALL";
			setting3.ConsolTransportMode = "ALL";
			setting3.PreviousSendingAgentType = "GTT";
			setting3.InvoiceTargetJobType = "PCL";
			var setting4 = collection.AddNew();
			setting4.ConsolDirection = "ALL";
			setting4.ConsolTransportMode = "ALL";
			setting4.PreviousSendingAgentType = "NON";
			setting4.InvoiceTargetJobType = "SCL";
			var setting5 = collection.AddNew();
			setting5.ConsolDirection = "ALL";
			setting5.ConsolTransportMode = "ALL";
			setting5.PreviousSendingAgentType = "ALL";
			setting5.InvoiceTargetJobType = "SCL";
			AccountingMasterFilesRegistry.Instance.GatewayChargeDefaultInvoiceTargetJobConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
