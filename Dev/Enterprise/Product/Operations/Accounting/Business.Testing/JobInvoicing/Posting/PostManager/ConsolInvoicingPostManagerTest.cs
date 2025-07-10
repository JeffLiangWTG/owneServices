using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Billing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class ConsolInvoicingPostManagerTest : TransactionCreatorBaseTest
	{
		#region Tax Branch

		public void TestWillNotTriggerCriticalValidationError_TransactionLineTaxClassNotEqualJobChargeTaxClass_WhenIsOverseasAgentChargeAndCollectFeesOnSingleInvoice()
		{
			#region Setup

			var branch1 = TestObjectCreator.CreateBranch("BR1", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("BR2", GlbCompany.CurrentCompany);
			var branch3 = TestObjectCreator.CreateBranch("BR3", GlbCompany.CurrentCompany);

			TestObjectCreator.Agent.CompanyData.OB_IsCreditor = ZBool.True;
			TestObjectCreator.Agent.CompanyData.OB_IsDebtor = ZBool.True;
			TestObjectCreator.Agent.CompanyData.SetAPTaxApplicable(ZBool.True);
			TestObjectCreator.Agent.CompanyData.SetARTaxApplicable(ZBool.True);
			Factory.Save();

			ZString transportMode = "AIR";
			ZString origin = "AUSYD";
			ZString destination = "USLAX";

			Factory.Save();

			SetupConsolAndShipments(transportMode, origin, destination, TestObjectCreator.Agent);
			Consol.JK_PrepaidCollect = Enterprise.Core.Constants.PaymentType.Collect;
			Consol.JK_RL_NKLoadPort = origin;
			Consol.JK_RL_NKDischargePort = destination;
			Consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);
			Consol.SetDefaultReceivingForwarderAddress(TestObjectCreator.Agent);
			Factory.Save();

			var shipment1Job = TestObjectCreator.CreateJob(Shipment1);
			var shipment2Job = TestObjectCreator.CreateJob(Shipment2);
			shipment1Job.JH_GB_TaxBranch = branch3.PK;
			shipment2Job.JH_GB_TaxBranch = branch2.PK;

			var shipment1Charge1 = shipment1Job.Charges.AddNew();
			shipment1Charge1.JR_AC = Env.Registry.FreightChargeCode;
			shipment1Charge1.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment1Charge1.JR_RX_NKSellCurrency = shipment1Charge1.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			shipment1Charge1.JR_OSSellExRate = shipment1Charge1.JR_OSCostExRate = 0.78m;
			shipment1Charge1.JR_OSCostAmt = 800m;
			shipment1Charge1.JR_OSSellAmt = 1000m;
			shipment1Charge1.JR_GB_SellTaxBranch = branch2.PK;
			shipment1Charge1.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;

			var shipment1Charge2 = shipment1Job.Charges.AddNew();
			shipment1Charge2.JR_AC = Env.Registry.FreightChargeCode;
			shipment1Charge2.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment1Charge2.JR_OSCostAmt = 60m;
			shipment1Charge2.JR_OSSellAmt = 75m;
			shipment1Charge2.JR_GB_SellTaxBranch = branch2.PK;

			var shipment2Charge1 = shipment2Job.Charges.AddNew();
			shipment2Charge1.JR_AC = Env.Registry.FreightChargeCode;
			shipment2Charge1.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment2Charge1.JR_RX_NKSellCurrency = shipment2Charge1.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			shipment2Charge1.JR_OSSellExRate = shipment2Charge1.JR_OSCostExRate = 0.78m;
			shipment2Charge1.JR_OSCostAmt = 600m;
			shipment2Charge1.JR_OSSellAmt = 750m;
			shipment2Charge1.JR_GB_SellTaxBranch = branch3.PK;
			shipment2Charge1.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;

			var shipment2Charge2 = shipment2Job.Charges.AddNew();
			shipment2Charge2.JR_AC = Env.Registry.FreightChargeCode;
			shipment2Charge2.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment2Charge2.JR_OSCostAmt = 80m;
			shipment2Charge2.JR_OSSellAmt = 100m;
			shipment2Charge2.JR_GB_SellTaxBranch = branch3.PK;

			Factory.Save();

			var jobs = new JobCollection(Factory);
			jobs.Load();

			#endregion

			AssertEquals("Precondition: IsOverseasAgentCharge", Consol.AgentToInvoice().PK, TestObjectCreator.Agent.PK);
			AssertEquals("Precondition: OM_FWBillCollectFeesOnSingleInvoice", true, TestObjectCreator.Agent.MiscServ.OM_FWBillCollectFeesOnSingleInvoice);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ConsolInvoicingPostManager testPostManager = new ConsolInvoicingPostManager(Factory, jobs.Cast<Job>(), Consol, null);
				testPostManager.ExportAgentPosting += new ExportAgentPostingEventHandler(Creator_ExportAgentPosting);
				testPostManager.OnCriticalPostError += new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_OnTaxBranchCriticalPostError);
				var transactions = testPostManager.CreateTransactions(JobInvoicingPostingOption.Agent);
				Factory.Save();

				var apTransactions = transactions.GetAllAPTransactions();
				var arTransactions = transactions.GetAllARTransactions();
				AssertEquals(0, apTransactions.Length);
				AssertEquals(0, arTransactions.Length);
				AssertEquals(true, IsOnCriticalPostErrorEventRaised);
			}
		}

		public void TestWillNotTriggerCriticalValidationError_TransactionLineTaxClassNotEqualJobChargeTaxClass_WhenIsAgentCharge()
		{
			#region Setup

			var branch1 = TestObjectCreator.CreateBranch("BR1", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("BR2", GlbCompany.CurrentCompany);
			var branch3 = TestObjectCreator.CreateBranch("BR3", GlbCompany.CurrentCompany);

			TestObjectCreator.Agent.CompanyData.OB_IsCreditor = ZBool.True;
			TestObjectCreator.Agent.CompanyData.OB_IsDebtor = ZBool.True;
			TestObjectCreator.Agent.CompanyData.SetAPTaxApplicable(ZBool.True);
			TestObjectCreator.Agent.CompanyData.SetARTaxApplicable(ZBool.True);
			Factory.Save();

			ZString transportMode = "AIR";
			ZString origin = "AUSYD";
			ZString destination = "USLAX";

			Factory.Save();

			SetupConsolAndShipments(transportMode, origin, destination, TestObjectCreator.Agent);
			Consol.JK_PrepaidCollect = Enterprise.Core.Constants.PaymentType.Collect;
			Consol.JK_RL_NKLoadPort = origin;
			Consol.JK_RL_NKDischargePort = destination;
			Consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);
			Factory.Save();

			var shipment1Job = TestObjectCreator.CreateJob(Shipment1);
			var shipment2Job = TestObjectCreator.CreateJob(Shipment2);
			shipment1Job.JH_GB_TaxBranch = branch3.PK;
			shipment2Job.JH_GB_TaxBranch = branch2.PK;

			var shipment1Charge1 = shipment1Job.Charges.AddNew();
			shipment1Charge1.JR_AC = Env.Registry.FreightChargeCode;
			shipment1Charge1.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment1Charge1.JR_RX_NKSellCurrency = shipment1Charge1.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			shipment1Charge1.JR_OSSellExRate = shipment1Charge1.JR_OSCostExRate = 0.78m;
			shipment1Charge1.JR_OSCostAmt = 800m;
			shipment1Charge1.JR_OSSellAmt = 1000m;
			shipment1Charge1.JR_GB_SellTaxBranch = branch2.PK;
			shipment1Charge1.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;

			var shipment1Charge2 = shipment1Job.Charges.AddNew();
			shipment1Charge2.JR_AC = Env.Registry.FreightChargeCode;
			shipment1Charge2.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment1Charge2.JR_OSCostAmt = 60m;
			shipment1Charge2.JR_OSSellAmt = 75m;
			shipment1Charge2.JR_GB_SellTaxBranch = branch2.PK;

			var shipment2Charge1 = shipment2Job.Charges.AddNew();
			shipment2Charge1.JR_AC = Env.Registry.FreightChargeCode;
			shipment2Charge1.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment2Charge1.JR_RX_NKSellCurrency = shipment2Charge1.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			shipment2Charge1.JR_OSSellExRate = shipment2Charge1.JR_OSCostExRate = 0.78m;
			shipment2Charge1.JR_OSCostAmt = 600m;
			shipment2Charge1.JR_OSSellAmt = 750m;
			shipment2Charge1.JR_GB_SellTaxBranch = branch3.PK;
			shipment2Charge1.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;

			var shipment2Charge2 = shipment2Job.Charges.AddNew();
			shipment2Charge2.JR_AC = Env.Registry.FreightChargeCode;
			shipment2Charge2.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment2Charge2.JR_OSCostAmt = 80m;
			shipment2Charge2.JR_OSSellAmt = 100m;
			shipment2Charge2.JR_GB_SellTaxBranch = branch3.PK;

			Factory.Save();

			var jobs = new JobCollection(Factory);
			jobs.Load();

			#endregion

			AssertEquals("Precondition: IsAgentCharge", true, Consol.IsAgentCharge(shipment1Charge1));
			AssertEquals("Precondition: IsAgentCharge", true, Consol.IsAgentCharge(shipment1Charge2));
			AssertEquals("Precondition: IsAgentCharge", true, Consol.IsAgentCharge(shipment2Charge1));
			AssertEquals("Precondition: IsAgentCharge", true, Consol.IsAgentCharge(shipment2Charge2));
			AssertEquals("Precondition: OM_FWBillCollectFeesOnSingleInvoice", true, TestObjectCreator.Agent.MiscServ.OM_FWBillCollectFeesOnSingleInvoice);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ConsolInvoicingPostManager testPostManager = new ConsolInvoicingPostManager(Factory, jobs.Cast<Job>(), Consol, null);
				testPostManager.ExportAgentPosting += new ExportAgentPostingEventHandler(Creator_ExportAgentPosting);
				testPostManager.OnCriticalPostError += new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_OnTaxBranchCriticalPostError);
				var transactions = testPostManager.CreateTransactions(JobInvoicingPostingOption.Agent);
				Factory.Save();

				var apTransactions = transactions.GetAllAPTransactions();
				var arTransactions = transactions.GetAllARTransactions();
				AssertEquals(0, apTransactions.Length);
				AssertEquals(4, arTransactions.Length);
				AssertEquals(false, IsOnCriticalPostErrorEventRaised);
			}
		}

		public void TestWillNotTriggerCriticalValidationError_TransactionLineTaxClassNotEqualJobChargeTaxClass_WhenCombinedShipmentChargesForBuyersConsol()
		{
			#region Setup

			var branch1 = TestObjectCreator.CreateBranch("BR1", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("BR2", GlbCompany.CurrentCompany);
			var branch3 = TestObjectCreator.CreateBranch("BR3", GlbCompany.CurrentCompany);

			TestObjectCreator.Agent.CompanyData.OB_IsCreditor = ZBool.True;
			TestObjectCreator.Agent.CompanyData.OB_IsDebtor = ZBool.True;
			TestObjectCreator.Agent.CompanyData.SetAPTaxApplicable(ZBool.True);
			TestObjectCreator.Agent.CompanyData.SetARTaxApplicable(ZBool.True);
			Factory.Save();

			ZString transportMode = "AIR";
			ZString origin = "AUSYD";
			ZString destination = "USLAX";

			Factory.Save();

			SetupConsolAndShipments(transportMode, origin, destination, TestObjectCreator.Agent);
			Consol.JK_PrepaidCollect = Enterprise.Core.Constants.PaymentType.Collect;
			Consol.JK_RL_NKLoadPort = origin;
			Consol.JK_RL_NKDischargePort = destination;
			Consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);
			Consol.SetDefaultReceivingForwarderAddress(TestObjectCreator.Agent);
			Factory.Save();

			var shipment1Job = TestObjectCreator.CreateJob(Shipment1);
			var shipment2Job = TestObjectCreator.CreateJob(Shipment2);
			shipment1Job.JH_GB_TaxBranch = branch3.PK;
			shipment2Job.JH_GB_TaxBranch = branch2.PK;

			var shipment1Charge1 = shipment1Job.Charges.AddNew();
			shipment1Charge1.JR_AC = Env.Registry.FreightChargeCode;
			shipment1Charge1.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment1Charge1.JR_RX_NKSellCurrency = shipment1Charge1.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			shipment1Charge1.JR_OSSellExRate = shipment1Charge1.JR_OSCostExRate = 0.78m;
			shipment1Charge1.JR_OSCostAmt = 800m;
			shipment1Charge1.JR_OSSellAmt = 1000m;
			shipment1Charge1.JR_GB_SellTaxBranch = branch2.PK;
			shipment1Charge1.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;

			var shipment1Charge2 = shipment1Job.Charges.AddNew();
			shipment1Charge2.JR_AC = Env.Registry.FreightChargeCode;
			shipment1Charge2.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment1Charge2.JR_OSCostAmt = 60m;
			shipment1Charge2.JR_OSSellAmt = 75m;
			shipment1Charge2.JR_GB_SellTaxBranch = branch2.PK;

			var shipment2Charge1 = shipment2Job.Charges.AddNew();
			shipment2Charge1.JR_AC = Env.Registry.FreightChargeCode;
			shipment2Charge1.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment2Charge1.JR_RX_NKSellCurrency = shipment2Charge1.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			shipment2Charge1.JR_OSSellExRate = shipment2Charge1.JR_OSCostExRate = 0.78m;
			shipment2Charge1.JR_OSCostAmt = 600m;
			shipment2Charge1.JR_OSSellAmt = 750m;
			shipment2Charge1.JR_GB_SellTaxBranch = branch3.PK;
			shipment2Charge1.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;

			var shipment2Charge2 = shipment2Job.Charges.AddNew();
			shipment2Charge2.JR_AC = Env.Registry.FreightChargeCode;
			shipment2Charge2.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment2Charge2.JR_OSCostAmt = 80m;
			shipment2Charge2.JR_OSSellAmt = 100m;
			shipment2Charge2.JR_GB_SellTaxBranch = branch3.PK;

			Factory.Save();

			var jobs = new JobCollection(Factory);
			jobs.Load();

			#endregion

			TestObjectCreator.Agent.MiscServ.OM_FWBillCollectFeesOnSingleInvoice = false;
			TestObjectCreator.Agent.CompanyData.OB_ARBuyersConsolInvoicingStyle = Constants.ConsolInvoicingStyles.ApportionInvoiceMaster;

			Shipment1.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			Shipment1.JobHeader.JH_OA_LocalChargesAddr = TestObjectCreator.Agent.AddressForSendingARDocuments.PK;
			Consol.JK_ConsolMode = Constants.ContainerModes.BuyersConsol;
			Factory.Save();

			AssertEquals("Preconditions: Consol is a buyer consol", true, Consol.IsBuyersConsol);
			AssertEquals("Precondition: IsOverseasAgentCharge", Consol.AgentToInvoice().PK, TestObjectCreator.Agent.PK);
			AssertEquals("Precondition: OM_FWBillCollectFeesOnSingleInvoice", false, TestObjectCreator.Agent.MiscServ.OM_FWBillCollectFeesOnSingleInvoice);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ConsolInvoicingPostManager testPostManager = new ConsolInvoicingPostManager(Factory, jobs.Cast<Job>(), Consol, null);
				testPostManager.ExportAgentPosting += new ExportAgentPostingEventHandler(Creator_ExportAgentPosting);
				testPostManager.OnCriticalPostError += new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_OnTaxBranchCriticalPostError);
				var transactions = testPostManager.CreateTransactions(JobInvoicingPostingOption.Agent);
				Factory.Save();

				var apTransactions = transactions.GetAllAPTransactions();
				var arTransactions = transactions.GetAllARTransactions();
				AssertEquals(0, apTransactions.Length);
				AssertEquals(0, arTransactions.Length);
				AssertEquals(true, IsOnCriticalPostErrorEventRaised);
			}
		}

		#endregion

		public void TestPreventPostingARCRD()
		{
			TestObjectCreator.Agent.CompanyData.OB_IsCreditor = ZBool.True;
			TestObjectCreator.Agent.CompanyData.OB_IsDebtor = ZBool.True;

			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("S001", consol);
			var job = TestObjectCreator.CreateJob(shipment, false);
			consol.SetDefaultReceivingForwarderAddress(TestObjectCreator.Agent);

			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT, 100m, TestObjectCreator.Agent);
			consolCost.E6_InvoiceNum = "INV1";
			consolCost.E6_InvoiceDate = ZDateTime.Now;
			consolCost.E6_IsForCollectInvoice = true;

			Factory.Save();

			ConsolInvoicingPostManager testPostManager;
			bool criticalErrorRised;

			Assert(true);
			Assert(false);

			void Assert(bool isCreditNotePrevented)
			{
				AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfCreditNotes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, isCreditNotePrevented);
				CreatePostManager();
				var transactions = testPostManager.CreateTransactions(JobInvoicingPostingOption.Agent);
				AssertEquals(nameof(testPostManager.CancelPosting), isCreditNotePrevented, testPostManager.CancelPosting);
				AssertEquals(nameof(criticalErrorRised), isCreditNotePrevented, criticalErrorRised);
			}

			void CreatePostManager()
			{
				var newFactory = new BusinessObjectFactory();
				var jobInNewFactory = newFactory.Load<Job>(job.PK);
				var consolInNewFactory = newFactory.Load<ForwardingConsol>(consol.PK);
				testPostManager = new ConsolInvoicingPostManager(newFactory, new[] { jobInNewFactory }, consolInNewFactory);
				testPostManager.OnCriticalPostError += PostManager_OnCriticalPostError_ForPreventCreditNoteTest;

				criticalErrorRised = false;
			}

			void PostManager_OnCriticalPostError_ForPreventCreditNoteTest(object sender, CriticalPostingErrorEventArgs e)
			{
				criticalErrorRised = true;

				var header = ((CriticalTransactionPostingErrorEventArgs)e).Header;
				AssertType<ARCreditNote>(header);
				AssertHasRowError(header, "Posting of Receivables Credit Notes is not permitted. The SELL Charges being posted would produce at least one Receivables Credit Note. Please review the prepared charges and correct appropriately. This is controlled by the registry setting Accounting -> Receivable Defaults -> Default Settings -> Prevent Creation of Credit Notes.");
			}
		}

		#region SIV Event Created When Invoice Posted

		public void TestSIVEventCreatedOnShipmentInvoiceCreationAndReversing()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKDestination = "JPAAM";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_UniqueConsignRef = "S1";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C1";
			consol.Shipments.Add(shipment);
			Factory.Save();

			var job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			job.Parent = shipment;

			CC1.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

			CreateCharge(job, CC1, "Charge Code 1", TestObjectCreator.AUD, 100M, Creditor1, TestObjectCreator.AUD, 150M, LocalClient);
			job.Charges[0].JR_InvoiceType = "FIN";

			Factory.Save();

			var jobs = new[] { job };

			var apps = new ApportionmentListing(Factory, consol);
			var postManager = new ConsolInvoicingPostManager(Factory, jobs, consol, apps);

			var transactions = postManager.CreateTransactions(JobInvoicingPostingOption.All);
			Factory.Save();
			AssertEquals("Payables Transaction Count", 0, transactions.APTransactionsCount);
			AssertEquals("Receivable Transactions Count", 1, transactions.ARTransactionsCount);

			InvoicingBase[] invoices = postManager.Poster.GetInvoices(TestObjectCreator.AUD, LocalClient);
			AssertEquals(1, invoices.Length);

			var filter = new ZQuery(StmALogSchema.SL_Parent, job.PK);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ServiceInvoicePosted.Code);
			StmALog[] retrievedLogs = Factory.Load<StmALog>(filter);

			AssertEquals("1 log found with FIN INV", 1, retrievedLogs.Count(x => x.SL_Reference == "FIN INV S1"));

			var reverser = new JobInvoicingReverser(job);
			reverser.ReverseAllInvoices("Incorrect Data Entry", "IDE");
			Assert("Precondition: ContinueWithSave", reverser.ContinueWithSave);
			reverser.ReversingFactory.Save();

			retrievedLogs = Factory.Load<StmALog>(filter);
			AssertEquals("1 log found with FIN CRD for reverse invoice", 1, retrievedLogs.Count(x => x.SL_Reference == "FIN CRD S1"));
		}

		public void TestSIVEventCreatedOnConsolInvoiceCreationAndReversing()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKDestination = "JPAAM";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_UniqueConsignRef = "S1";

			Consol = Factory.New<ForwardingConsol>();
			Consol.Shipments.Add(shipment);
			Consol.JK_UniqueConsignRef = "C1";

			var job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			job.Parent = shipment;

			CC1.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

			CreateCharge(job, CC1, "Charge Code 1", TestObjectCreator.AUD, 100M, Creditor1, TestObjectCreator.AUD, 150M, Agent);
			job.Charges[0].JR_InvoiceType = "FIN";

			Factory.Save();

			var jobs = new[] { job };

			var apps = new ApportionmentListing(Factory, Consol);
			var postManager = new ConsolInvoicingPostManager(Factory, jobs, Consol, apps);

			var transactions = postManager.CreateTransactions(JobInvoicingPostingOption.Agent);
			Factory.Save();
			AssertEquals("Payables Transaction Count", 0, transactions.APTransactionsCount);
			AssertEquals("Receivable Transactions Count", 1, transactions.ARTransactionsCount);

			InvoicingBase[] invoices = postManager.Poster.GetInvoices(TestObjectCreator.AUD, Agent);
			AssertEquals(1, invoices.Length);

			var filter = new ZQuery(StmALogSchema.SL_Parent, Consol.PK);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ServiceInvoicePosted.Code);
			StmALog[] retrievedLogs = Factory.Load<StmALog>(filter);

			AssertEquals("1 log found with FIN INV", 1, retrievedLogs.Count(x => x.SL_Reference == "FIN INV C1"));

			var reverser = new JobInvoicingReverser(job);
			reverser.ReverseAllInvoices("Incorrect Data Entry", "IDE");
			Assert("Precondition: ContinueWithSave", reverser.ContinueWithSave);
			reverser.ReversingFactory.Save();

			retrievedLogs = Factory.Load<StmALog>(filter);
			AssertEquals("1 log found with FIN CRD for reverse invoice", 1, retrievedLogs.Count(x => x.SL_Reference == "FIN CRD C1"));
		}

		#endregion

		#region RoundingForJapan

		public void TestRoundChargeAmountsForJapan()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Japan))
			{
				AccountingConfigurationRegistry.Instance.JapanIATAImportAirLocalClientFRTChargeGroupRounding.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Constants.RoundingRules.Codes.JapanYen);

				ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
				shipment.JS_RL_NKDestination = "JPAAM";
				shipment.JS_RL_NKOrigin = "USLAX";

				Consol = Factory.New<ForwardingConsol>();
				Consol.Shipments.Add(shipment);

				Job job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
				job.Parent = shipment;

				CC1.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

				RefCurrency jPY = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "JPY");

				for (int i = 0; i < 4; i++)
				{
					CreateCharge(job, CC1, "Charge Code 1", jPY, 100M, Creditor1, jPY, 150M, LocalClient);
				}

				job.Charges[0].JR_InvoiceType = job.Charges[1].JR_InvoiceType = "FIN";
				job.Charges[2].JR_InvoiceType = job.Charges[3].JR_InvoiceType = "ITC";

				job.Charges[0].JR_LocalSellAmt = job.Charges[2].JR_LocalSellAmt = 50;
				job.Charges[1].JR_LocalSellAmt = job.Charges[3].JR_LocalSellAmt = 17;

				Factory.Save();

				var jobs = new[] { job };

				ApportionmentListing apps = new ApportionmentListing(Factory, Consol);
				ConsolInvoicingPostManager postManager = new ConsolInvoicingPostManager(Factory, jobs, Consol, apps);

				TransactionCreatorHashtable transactions = postManager.CreateTransactions(JobInvoicingPostingOption.All);
				AssertEquals("Payables Transaction Count", 0, transactions.APTransactionsCount);
				AssertEquals("Receivable Transactions Count", 2, transactions.ARTransactionsCount);

				AssertEquals(53m, job.Charges[0].JR_LocalSellAmt);
				AssertEquals(17m, job.Charges[1].JR_LocalSellAmt);
				AssertEquals(53m, job.Charges[2].JR_LocalSellAmt);
				AssertEquals(17m, job.Charges[3].JR_LocalSellAmt);

				InvoicingBase[] invoices = postManager.Poster.GetInvoices(jPY, LocalClient);
				AssertEquals(2, invoices.Length);

				for (int i = 0; i < 2; i++)
				{
					ARInvoice invoice = (ARInvoice)invoices[i];
					AssertEquals("Invoice Line Count", 2, invoice.Lines.Count);
					AssertEquals(53m, invoice.Lines[0].AL_LocalExTaxAmount);
					AssertEquals(17m, invoice.Lines[1].AL_LocalExTaxAmount);
				}
			}
		}

		#endregion

		#region TEST: Create Consol Cost Only Invoices From Consol With Apportionment

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateConsolCostOnlyInvoicesFromConsolWithApportionment()
		{
			Job job1 = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "Y00001000";
			ExchangeRate rate1_1 = CreateExchangeRate(job1, USD, .7M);
			ExchangeRate rate1_2 = CreateExchangeRate(job1, GBP, .4M);

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = CC1.PK;
			cost.E6_OH_Creditor = Creditor3.PK;
			cost.E6_InvoiceNum = "3";
			cost.E6_InvoiceDate = Now.AddDays(10);
			cost.E6_PaymentDate = Now.AddDays(25);
			ZGuid costSplitGroup1 = cost.PK;
			Factory.Save();

			Charge charge1_1 = CreateCharge(job1, CC1, "Job 1 Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge1_8 = CreateCharge(job1, CC8, "Job 1 Charge Code 8", AUD, 300M, Creditor3, AUD, 300M, LocalClient);
			charge1_8.JR_E6 = costSplitGroup1;

			SetAPInvoiceInfo(charge1_1, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_8, "3", Now.AddDays(10), Now.AddDays(25));

			Job job2 = CreateJob("Z00001001", LocalClient, 5M, Agent, 10M);

			Charge charge2_3 = CreateCharge(job2, CC3, "Job 2 Charge Code 3", AUD, 350M, Creditor3, AUD, 400M, LocalClient);
			Charge charge2_4 = CreateCharge(job2, CC8, "Job 2 Charge Code 8", AUD, 335M, Creditor3, AUD, 500M, Agent);
			charge2_4.JR_E6 = costSplitGroup1;

			SetAPInvoiceInfo(charge2_3, "2", Now.AddDays(15), Now.AddDays(25));
			SetAPInvoiceInfo(charge2_4, "3", Now.AddDays(10), Now.AddDays(25));
			Factory.Save();

			#region WIPs and Accruals

			AccTransactionLines charge1_1WIP = charge1_1.WIP;
			AccTransactionLines charge1_8WIP = charge1_8.WIP;
			AccTransactionLines charge2_3WIP = charge2_3.WIP;
			AccTransactionLines charge2_4WIP = charge2_4.WIP;

			AccTransactionLines charge1_1Accrual = charge1_1.Accrual;
			AccTransactionLines charge1_8Accrual = charge1_8.Accrual;
			AccTransactionLines charge2_3Accrual = charge2_3.Accrual;
			AccTransactionLines charge2_4Accrual = charge2_4.Accrual;

			#endregion

			var jobs = new[] { job1, job2 };
			ConsolInvoicingPostManager creator = new ConsolInvoicingPostManager(Factory, jobs, consol, apps);

			TransactionCreatorHashtable transactions = creator.CreateTransactions(JobInvoicingPostingOption.ConsolCosts);
			AssertEquals("Invoice Count", 1, transactions.APTransactionsCount);
			AssertEquals("Receivable Transactions count", 0, transactions.ARTransactionsCount);

			AssertNull(transactions.RetrieveAPInvoice(Creditor1, "1"));
			AssertNull(transactions.RetrieveAPInvoice(Creditor3, "1"));
			AssertNull(transactions.RetrieveAPInvoice(Creditor3, "2"));

			#region Creditor 3 Invoice 3

			APInvoice creditor3Inv3 = transactions.RetrieveAPInvoice(Creditor3, "3");
			AssertEquals("Invoice Line Count", 2, creditor3Inv3.Lines.Count);

			AssertTransactionHeaderValues(creditor3Inv3, "AP", "INV", "3", "Multiple Jobs", Now.AddDays(10), Now.AddDays(25),
					-635M, -63.50M, 0M, -698.50M, AUD, 1, Now, ZBool.False, Creditor3, null,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor3Inv3);

			TransactionLine cC8Line1 = creditor3Inv3.FindTransactionLine("CST", CC8, job1.PK);
			AssertTransactionLineValues(cC8Line1, "CST", 1, "Job 1 Charge Code 8", -300M, GST1, -30M, WHTFREE1, 0M, -330M, AUD, 1, Now, Now,
					ZBool.False, creditor3Inv3, job1, CC8, CC8.CostAccount, Creditor3);
			AssertTransactionLineDefaults(cC8Line1);

			TransactionLine cC8Line2 = creditor3Inv3.FindTransactionLine("CST", CC8, job2.PK);
			AssertTransactionLineValues(cC8Line2, "CST", 2, "Job 2 Charge Code 8", -335M, GST1, -33.50M, WHTFREE1, 0M, -368.50M, AUD, 1, Now, Now,
					ZBool.False, creditor3Inv3, job2, CC8, CC8.CostAccount, Creditor3);
			AssertTransactionLineDefaults(cC8Line2);

			#endregion

			#region WIP and Accrual Reversal Assertions

			AssertEquals("Charge 1_1 WIP Reversed", false, charge1_1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_8 WIP Reversed", false, charge1_8WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_3 WIP Reversed", false, charge2_3WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_4 WIP Reversed", false, charge2_4WIP.AL_ReverseDate.IsValid);

			AssertEquals("Charge 1_1 Accrual Reversed", false, charge1_1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_8 Accrual Reversed", true, charge1_8Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_3 Accrual Reversed", false, charge2_3Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_4 Accrual Reversed", true, charge2_4Accrual.AL_ReverseDate.IsValid);

			#endregion
		}

		#endregion

		#region TEST: Create All Cost Invoices From Consol

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateAllCostInvoicesFromConsol()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Job job1 = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			consol.JK_UniqueConsignRef = "Y00001000";
			ExchangeRate rate1_1 = CreateExchangeRate(job1, USD, .7M);
			ExchangeRate rate1_2 = CreateExchangeRate(job1, GBP, .4M);

			Charge charge1_1 = CreateCharge(job1, CC1, "Job 1 Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge1_2 = CreateCharge(job1, CC2, "Job 1 Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			Charge charge1_3 = CreateCharge(job1, CC3, "Job 1 Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);
			Charge charge1_4 = CreateCharge(job1, CC4, "Job 1 Charge Code 4", null, 0M, null, USD, 500M, Agent);
			Charge charge1_5 = CreateCharge(job1, CC5, "Job 1 Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, LocalClient);
			Charge charge1_6 = CreateCharge(job1, CC6, "Job 1 Charge Code 6", USD, 200M, Creditor2, USD, 275M, Agent);
			Charge charge1_7 = CreateCharge(job1, CC7, "Job 1 Charge Code 7", AUD, 200M, Creditor1, AUD, 300M, LocalClient);

			SetAPInvoiceInfo(charge1_1, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_2, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_3, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_5, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_7, "1", Now.AddDays(10), Now.AddDays(20));

			Job job2 = CreateJob("Z00001001", LocalClient, 5M, Agent, 10M);
			ExchangeRate rate2_1 = CreateExchangeRate(job2, USD, .7M);
			ExchangeRate rate2_2 = CreateExchangeRate(job2, GBP, .4M);

			Charge charge2_1 = CreateCharge(job2, CC1, "Job 2 Charge Code 1", AUD, 150M, Creditor1, AUD, 200M, LocalClient);
			Charge charge2_2 = CreateCharge(job2, CC2, "Job 2 Charge Code 2", AUD, 250M, Creditor2, AUD, 250M, LocalClient);
			Charge charge2_3 = CreateCharge(job2, CC3, "Job 2 Charge Code 3", AUD, 350M, Creditor3, AUD, 400M, LocalClient);

			SetAPInvoiceInfo(charge2_1, "3", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge2_2, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge2_3, "2", Now.AddDays(15), Now.AddDays(25));
			Factory.Save();

			#region WIPs and Accruals

			AccTransactionLines charge1_1WIP = charge1_1.WIP;
			AccTransactionLines charge1_2WIP = charge1_2.WIP;
			AccTransactionLines charge1_3WIP = charge1_3.WIP;
			AccTransactionLines charge1_4WIP = charge1_4.WIP;
			AccTransactionLines charge1_5WIP = charge1_5.WIP;
			AccTransactionLines charge1_6WIP = charge1_6.WIP;
			AccTransactionLines charge1_7WIP = charge1_7.WIP;
			AccTransactionLines charge2_1WIP = charge2_1.WIP;
			AccTransactionLines charge2_2WIP = charge2_2.WIP;
			AccTransactionLines charge2_3WIP = charge2_3.WIP;

			AccTransactionLines charge1_1Accrual = charge1_1.Accrual;
			AccTransactionLines charge1_2Accrual = charge1_2.Accrual;
			AccTransactionLines charge1_3Accrual = charge1_3.Accrual;
			AccTransactionLines charge1_5Accrual = charge1_5.Accrual;
			AccTransactionLines charge1_6Accrual = charge1_6.Accrual;
			AccTransactionLines charge1_7Accrual = charge1_7.Accrual;
			AccTransactionLines charge2_1Accrual = charge2_1.Accrual;
			AccTransactionLines charge2_2Accrual = charge2_2.Accrual;
			AccTransactionLines charge2_3Accrual = charge2_3.Accrual;

			#endregion

			var jobs = new[] { job1, job2 };

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			ConsolInvoicingPostManager creator = new ConsolInvoicingPostManager(Factory, jobs, consol, apps);

			TransactionCreatorHashtable transactions = creator.CreateTransactions(JobInvoicingPostingOption.Costs);
			AssertEquals("Invoice Count", 7, transactions.APTransactionsCount);
			AssertEquals("Receivable Transactions count", 0, transactions.ARTransactionsCount);

			#region Creditor 1 Invoice 1

			APInvoice creditor1Inv1 = transactions.RetrieveAPInvoice(Creditor1, "1");
			AssertEquals("Invoice Line Count", 2, creditor1Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor1Inv1, "AP", "INV", "1", "Z00001000", Now.AddDays(10), Now.AddDays(20),
					-300M, -10M, 0M, -310M, AUD, 1, Now, ZBool.False, Creditor1, job1,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor1Inv1);

			TransactionLine cC1_1Line = creditor1Inv1.FindTransactionLine("CST", CC1, job1.PK);
			AssertTransactionLineValues(cC1_1Line, "CST", 1, "Job 1 Charge Code 1", -100M, GST1, -10M, WHTFREE1, 0M, -110M, AUD, 1, Now, Now,
					ZBool.False, creditor1Inv1, job1, CC1, CC1.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC1_1Line);

			TransactionLine cC1_7Line = creditor1Inv1.FindTransactionLine("CST", CC7, job1.PK);
			AssertTransactionLineValues(cC1_7Line, "CST", 2, "Job 1 Charge Code 7", -200M, GSTFREE1, 0M, WHTFREE1, 0M, -200M, AUD, 1, Now, Now,
					ZBool.False, creditor1Inv1, job1, CC7, CC7.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC1_7Line);

			#endregion

			#region Creditor 1 Invoice 2

			APInvoice creditor1Inv2 = transactions.RetrieveAPInvoice(Creditor1, "2"); // charge 5
			AssertEquals("Invoice Line Count", 1, creditor1Inv2.Lines.Count);

			AssertTransactionHeaderValues(creditor1Inv2, "AP", "INV", "2", "Z00001000", Now.AddDays(10), Now.AddDays(20),
					-250M, -25M, 0M, -110M, GBP, .4M, Now, ZBool.False, Creditor1, job1,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertOverseaAPTransactionHeaderDefaults_ForBasePostManager(creditor1Inv2);

			TransactionLine cC5Line = creditor1Inv2.FindTransactionLine("CST", CC5, job1.PK);
			AssertTransactionLineValues(cC5Line, "CST", 1, "Job 1 Charge Code 5", -250M, GST1, -25M, WHTFREE1, 0M, -110M, GBP, .4M, Now, Now,
					ZBool.False, creditor1Inv2, job1, CC5, CC5.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC5Line);

			#endregion

			#region Creditor 1 Invoice 3

			APInvoice creditor1Inv3 = transactions.RetrieveAPInvoice(Creditor1, "3");
			AssertEquals("Invoice Line Count", 1, creditor1Inv3.Lines.Count);

			AssertTransactionHeaderValues(creditor1Inv3, "AP", "INV", "3", "Z00001001", Now.AddDays(10), Now.AddDays(20),
					-150M, -15M, 0M, -165M, AUD, 1, Now, ZBool.False, Creditor1, job2,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor1Inv3);

			TransactionLine cC1Line2 = creditor1Inv3.FindTransactionLine("CST", CC1, job2.PK);
			AssertTransactionLineValues(cC1Line2, "CST", 1, "Job 2 Charge Code 1", -150M, GST1, -15M, WHTFREE1, 0M, -165M, AUD, 1, Now, Now,
					ZBool.False, creditor1Inv3, job2, CC1, CC1.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC1Line2);

			#endregion

			#region Creditor 2 Invoice 1

			APInvoice creditor2Inv1 = transactions.RetrieveAPInvoice(Creditor2, "1");
			AssertEquals("Invoice Line Count", 1, creditor2Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor2Inv1, "AP", "INV", "1", "Z00001000", Now.AddDays(10), Now.AddDays(20),
					-200M, -20M, -10M, -220M, AUD, 1, Now, ZBool.False, Creditor2, job1,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor2Inv1);

			TransactionLine cC2Line1 = creditor2Inv1.FindTransactionLine("CST", CC2, job1.PK);
			AssertTransactionLineValues(cC2Line1, "CST", 1, "Job 1 Charge Code 2", -200M, GST1, -20M, WHT1, -10M, -220M, AUD, 1, Now, Now,
					ZBool.False, creditor2Inv1, job1, CC2, CC2.CostAccount, Creditor2);
			AssertTransactionLineDefaults(cC2Line1);

			#endregion

			#region Creditor 2 Invoice 2

			APInvoice creditor2Inv2 = transactions.RetrieveAPInvoice(Creditor2, "2");
			AssertEquals("Invoice Line Count", 1, creditor2Inv2.Lines.Count);

			AssertTransactionHeaderValues(creditor2Inv2, "AP", "INV", "2", "Z00001001", Now.AddDays(10), Now.AddDays(20),
					-250M, -25M, -12.50M, -275M, AUD, 1, Now, ZBool.False, Creditor2, job2,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor2Inv2);

			TransactionLine cC2Line2 = creditor2Inv2.FindTransactionLine("CST", CC2, job2.PK);
			AssertTransactionLineValues(cC2Line2, "CST", 1, "Job 2 Charge Code 2", -250M, GST1, -25M, WHT1, -12.50M, -275M, AUD, 1, Now, Now,
					ZBool.False, creditor2Inv2, job2, CC2, CC2.CostAccount, Creditor2);
			AssertTransactionLineDefaults(cC2Line2);

			#endregion

			#region Creditor 3 Invoice 1

			APInvoice creditor3Inv1 = transactions.RetrieveAPInvoice(Creditor3, "1");
			AssertEquals("Invoice Line Count", 1, creditor3Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor3Inv1, "AP", "INV", "1", "Z00001000", Now.AddDays(10), Now.AddDays(20),
					-300M, 0M, -15M, -300M, AUD, 1, Now, ZBool.False, Creditor3, job1,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor2Inv1);

			TransactionLine cC3Line = creditor3Inv1.FindTransactionLine("CST", CC3, job1.PK);
			AssertTransactionLineValues(cC3Line, "CST", 1, "Job 1 Charge Code 3", -300M, GSTFREE1, 0M, WHT1, -15M, -300M, AUD, 1, Now, Now,
					ZBool.False, creditor3Inv1, job1, CC3, CC3.CostAccount, Creditor3);
			AssertTransactionLineDefaults(cC3Line);

			#endregion

			#region Creditor 3 Invoice 2

			APInvoice creditor3Inv2 = transactions.RetrieveAPInvoice(Creditor3, "2");
			AssertEquals("Invoice Line Count", 1, creditor3Inv2.Lines.Count);

			AssertTransactionHeaderValues(creditor3Inv2, "AP", "INV", "2", "Z00001001", Now.AddDays(15), Now.AddDays(25),
					-350, 0M, -17.50M, -350M, AUD, 1, Now, ZBool.False, Creditor3, job2,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);

			TransactionLine cC2_3Line = creditor3Inv2.FindTransactionLine("CST", CC3, job2.PK);
			AssertTransactionLineValues(cC2_3Line, "CST", 1, "Job 2 Charge Code 3", -350M, GSTFREE1, 0M, WHT1, -17.50M, -350M, AUD, 1, Now, Now,
					ZBool.False, creditor3Inv2, job2, CC3, CC3.CostAccount, Creditor3);
			AssertTransactionLineDefaults(cC2_3Line);

			#endregion

			#region WIP and Accrual Reversal Assertions

			AssertEquals("Charge 1_1 WIP Reversed", false, charge1_1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_2 WIP Reversed", false, charge1_2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_3 WIP Reversed", false, charge1_3WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_4 WIP Reversed", false, charge1_4WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_5 WIP Reversed", false, charge1_5WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_6 WIP Reversed", false, charge1_6WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_7 WIP Reversed", false, charge1_7WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_1 WIP Reversed", false, charge2_1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_2 WIP Reversed", false, charge2_2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_3 WIP Reversed", false, charge2_3WIP.AL_ReverseDate.IsValid);

			AssertEquals("Charge 1_1 Accrual Reversed", true, charge1_1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_2 Accrual Reversed", true, charge1_2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_3 Accrual Reversed", true, charge1_3Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_5 Accrual Reversed", true, charge1_5Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_6 Accrual Reversed", false, charge1_6Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_7 Accrual Reversed", true, charge1_7Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_1 Accrual Reversed", true, charge2_1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_2 Accrual Reversed", true, charge2_2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_3 Accrual Reversed", true, charge2_3Accrual.AL_ReverseDate.IsValid);

			#endregion
		}

		#endregion

		#region TEST: Create All Cost Invoices From Consol With Apportionment

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateAllCostInvoicesFromConsolWithApportionment()
		{
			Job job1 = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "Y00001000";
			ExchangeRate rate1_1 = CreateExchangeRate(job1, USD, .7M);
			ExchangeRate rate1_2 = CreateExchangeRate(job1, GBP, .4M);

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = CC1.PK;
			cost.E6_OH_Creditor = Creditor3.PK;
			cost.E6_InvoiceNum = "3";
			cost.E6_InvoiceDate = Now.AddDays(10);
			cost.E6_PaymentDate = Now.AddDays(25);
			ZGuid costSplitGroup1 = cost.PK;
			Factory.Save();

			Charge charge1_1 = CreateCharge(job1, CC1, "Job 1 Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge1_2 = CreateCharge(job1, CC2, "Job 1 Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			Charge charge1_3 = CreateCharge(job1, CC3, "Job 1 Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);
			Charge charge1_4 = CreateCharge(job1, CC4, "Job 1 Charge Code 4", null, 0M, null, USD, 500M, Agent);
			Charge charge1_5 = CreateCharge(job1, CC5, "Job 1 Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, LocalClient);
			Charge charge1_6 = CreateCharge(job1, CC6, "Job 1 Charge Code 6", USD, 200M, Creditor2, USD, 275M, Agent);
			Charge charge1_7 = CreateCharge(job1, CC7, "Job 1 Charge Code 7", AUD, 200M, Creditor1, AUD, 300M, LocalClient);
			Charge charge1_8 = CreateCharge(job1, CC8, "Job 1 Charge Code 8", AUD, 300M, Creditor3, AUD, 300M, LocalClient);
			charge1_8.JR_E6 = costSplitGroup1;

			SetAPInvoiceInfo(charge1_1, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_2, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_3, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_5, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_7, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_8, "3", Now.AddDays(10), Now.AddDays(25));

			Job job2 = CreateJob("Z00001001", LocalClient, 5M, Agent, 10M);
			ExchangeRate rate2_1 = CreateExchangeRate(job2, USD, .7M);
			ExchangeRate rate2_2 = CreateExchangeRate(job2, GBP, .4M);

			Charge charge2_1 = CreateCharge(job2, CC1, "Job 2 Charge Code 1", AUD, 150M, Creditor1, AUD, 200M, LocalClient);
			Charge charge2_2 = CreateCharge(job2, CC2, "Job 2 Charge Code 2", AUD, 250M, Creditor2, AUD, 250M, LocalClient);
			Charge charge2_3 = CreateCharge(job2, CC3, "Job 2 Charge Code 3", AUD, 350M, Creditor3, AUD, 400M, LocalClient);
			Charge charge2_4 = CreateCharge(job2, CC8, "Job 2 Charge Code 8", AUD, 335M, Creditor3, AUD, 500M, Agent);
			charge2_4.JR_E6 = costSplitGroup1;

			SetAPInvoiceInfo(charge2_1, "3", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge2_2, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge2_3, "2", Now.AddDays(15), Now.AddDays(25));
			SetAPInvoiceInfo(charge2_4, "3", Now.AddDays(10), Now.AddDays(25));
			Factory.Save();

			#region WIPs and Accruals

			AccTransactionLines charge1_1WIP = charge1_1.WIP;
			AccTransactionLines charge1_2WIP = charge1_2.WIP;
			AccTransactionLines charge1_3WIP = charge1_3.WIP;
			AccTransactionLines charge1_4WIP = charge1_4.WIP;
			AccTransactionLines charge1_5WIP = charge1_5.WIP;
			AccTransactionLines charge1_6WIP = charge1_6.WIP;
			AccTransactionLines charge1_7WIP = charge1_7.WIP;
			AccTransactionLines charge1_8WIP = charge1_8.WIP;
			AccTransactionLines charge2_1WIP = charge2_1.WIP;
			AccTransactionLines charge2_2WIP = charge2_2.WIP;
			AccTransactionLines charge2_3WIP = charge2_3.WIP;
			AccTransactionLines charge2_4WIP = charge2_4.WIP;

			AccTransactionLines charge1_1Accrual = charge1_1.Accrual;
			AccTransactionLines charge1_2Accrual = charge1_2.Accrual;
			AccTransactionLines charge1_3Accrual = charge1_3.Accrual;
			AccTransactionLines charge1_5Accrual = charge1_5.Accrual;
			AccTransactionLines charge1_6Accrual = charge1_6.Accrual;
			AccTransactionLines charge1_7Accrual = charge1_7.Accrual;
			AccTransactionLines charge1_8Accrual = charge1_8.Accrual;
			AccTransactionLines charge2_1Accrual = charge2_1.Accrual;
			AccTransactionLines charge2_2Accrual = charge2_2.Accrual;
			AccTransactionLines charge2_3Accrual = charge2_3.Accrual;
			AccTransactionLines charge2_4Accrual = charge2_4.Accrual;

			#endregion

			var jobs = new[] { job1, job2 };
			ConsolInvoicingPostManager creator = new ConsolInvoicingPostManager(Factory, jobs, consol, apps);

			TransactionCreatorHashtable transactions = creator.CreateTransactions(JobInvoicingPostingOption.Costs);
			AssertEquals("Invoice Count", 8, transactions.APTransactionsCount);
			AssertEquals("Receivable Transactions count", 0, transactions.ARTransactionsCount);

			#region Creditor 1 Invoice 1

			APInvoice creditor1Inv1 = transactions.RetrieveAPInvoice(Creditor1, "1");
			AssertEquals("Invoice Line Count", 2, creditor1Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor1Inv1, "AP", "INV", "1", "Z00001000", Now.AddDays(10), Now.AddDays(20),
					-300M, -10M, 0M, -310M, AUD, 1, Now, ZBool.False, Creditor1, job1,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor1Inv1);

			TransactionLine cC1Line1 = creditor1Inv1.FindTransactionLine("CST", CC1, job1.PK);
			AssertTransactionLineValues(cC1Line1, "CST", 1, "Job 1 Charge Code 1", -100M, GST1, -10M, WHTFREE1, 0M, -110M, AUD, 1, Now, Now,
					ZBool.False, creditor1Inv1, job1, CC1, CC1.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC1Line1);

			TransactionLine cC7Line = creditor1Inv1.FindTransactionLine("CST", CC7, job1.PK);
			AssertTransactionLineValues(cC7Line, "CST", 2, "Job 1 Charge Code 7", -200M, GSTFREE1, 0M, WHTFREE1, 0M, -200M, AUD, 1, Now, Now,
					ZBool.False, creditor1Inv1, job1, CC7, CC7.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC7Line);

			#endregion

			#region Creditor 1 Invoice 2

			APInvoice creditor1Inv2 = transactions.RetrieveAPInvoice(Creditor1, "2");
			AssertEquals("Invoice Line Count", 1, creditor1Inv2.Lines.Count);

			AssertTransactionHeaderValues(creditor1Inv2, "AP", "INV", "2", "Z00001000", Now.AddDays(10), Now.AddDays(20),
					-250M, -25M, 0M, -110M, GBP, .4M, Now, ZBool.False, Creditor1, job1,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertOverseaAPTransactionHeaderDefaults_ForBasePostManager(creditor1Inv2);

			TransactionLine cC5Line = creditor1Inv2.FindTransactionLine("CST", CC5, job1.PK);
			AssertTransactionLineValues(cC5Line, "CST", 1, "Job 1 Charge Code 5", -250M, GST1, -25M, WHTFREE1, 0M, -110M, GBP, .4M, Now, Now,
					ZBool.False, creditor1Inv2, job1, CC5, CC5.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC5Line);

			#endregion

			#region Creditor 1 Invoice 3

			APInvoice creditor1Inv3 = transactions.RetrieveAPInvoice(Creditor1, "3");
			AssertEquals("Invoice Line Count", 1, creditor1Inv3.Lines.Count);

			AssertTransactionHeaderValues(creditor1Inv3, "AP", "INV", "3", "Z00001001", Now.AddDays(10), Now.AddDays(20),
					-150M, -15M, 0M, -165M, AUD, 1, Now, ZBool.False, Creditor1, job2,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor1Inv3);

			TransactionLine cC1Line2 = creditor1Inv3.FindTransactionLine("CST", CC1, job2.PK);
			AssertTransactionLineValues(cC1Line2, "CST", 1, "Job 2 Charge Code 1", -150M, GST1, -15M, WHTFREE1, 0M, -165M, AUD, 1, Now, Now,
					ZBool.False, creditor1Inv3, job2, CC1, CC1.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC1Line2);

			#endregion

			#region Creditor 2 Invoice 1

			APInvoice creditor2Inv1 = transactions.RetrieveAPInvoice(Creditor2, "1");
			AssertEquals("Invoice Line Count", 1, creditor2Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor2Inv1, "AP", "INV", "1", "Z00001000", Now.AddDays(10), Now.AddDays(20),
					-200M, -20M, -10M, -220M, AUD, 1, Now, ZBool.False, Creditor2, job1,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor2Inv1);

			TransactionLine cC2Line1 = creditor2Inv1.FindTransactionLine("CST", CC2, job1.PK);
			AssertTransactionLineValues(cC2Line1, "CST", 1, "Job 1 Charge Code 2", -200M, GST1, -20M, WHT1, -10M, -220M, AUD, 1, Now, Now,
					ZBool.False, creditor2Inv1, job1, CC2, CC2.CostAccount, Creditor2);
			AssertTransactionLineDefaults(cC2Line1);

			#endregion

			#region Creditor 2 Invoice 2

			APInvoice creditor2Inv2 = transactions.RetrieveAPInvoice(Creditor2, "2");
			AssertEquals("Invoice Line Count", 1, creditor2Inv2.Lines.Count);

			AssertTransactionHeaderValues(creditor2Inv2, "AP", "INV", "2", "Z00001001", Now.AddDays(10), Now.AddDays(20),
					-250M, -25M, -12.50M, -275M, AUD, 1, Now, ZBool.False, Creditor2, job2,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor2Inv2);

			TransactionLine cC2Line2 = creditor2Inv2.FindTransactionLine("CST", CC2, job2.PK);
			AssertTransactionLineValues(cC2Line2, "CST", 1, "Job 2 Charge Code 2", -250M, GST1, -25M, WHT1, -12.50M, -275M, AUD, 1, Now, Now,
					ZBool.False, creditor2Inv2, job2, CC2, CC2.CostAccount, Creditor2);
			AssertTransactionLineDefaults(cC2Line2);

			#endregion

			#region Creditor 3 Invoice 1

			APInvoice creditor3Inv1 = transactions.RetrieveAPInvoice(Creditor3, "1");
			AssertEquals("Invoice Line Count", 1, creditor3Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor3Inv1, "AP", "INV", "1", "Z00001000", Now.AddDays(10), Now.AddDays(20),
					-300M, 0M, -15M, -300M, AUD, 1, Now, ZBool.False, Creditor3, job1,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor2Inv1);

			TransactionLine cC3Line = creditor3Inv1.FindTransactionLine("CST", CC3, job1.PK);
			AssertTransactionLineValues(cC3Line, "CST", 1, "Job 1 Charge Code 3", -300M, GSTFREE1, 0M, WHT1, -15M, -300M, AUD, 1, Now, Now,
					ZBool.False, creditor3Inv1, job1, CC3, CC3.CostAccount, Creditor3);
			AssertTransactionLineDefaults(cC3Line);

			#endregion

			#region Creditor 3 Invoice 2

			APInvoice creditor3Inv2 = transactions.RetrieveAPInvoice(Creditor3, "2");
			AssertEquals("Invoice Line Count", 1, creditor3Inv2.Lines.Count);

			AssertTransactionHeaderValues(creditor3Inv2, "AP", "INV", "2", "Z00001001", Now.AddDays(15), Now.AddDays(25),
					-350, 0M, -17.50M, -350M, AUD, 1, Now, ZBool.False, Creditor3, job2,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor3Inv2);

			TransactionLine cC3Line2 = creditor3Inv2.FindTransactionLine("CST", CC3, job2.PK);
			AssertTransactionLineValues(cC3Line2, "CST", 1, "Job 2 Charge Code 3", -350M, GSTFREE1, 0M, WHT1, -17.50M, -350M, AUD, 1, Now, Now,
					ZBool.False, creditor3Inv2, job2, CC3, CC3.CostAccount, Creditor3);
			AssertTransactionLineDefaults(cC3Line2);

			#endregion

			#region Creditor 3 Invoice 3

			APInvoice creditor3Inv3 = transactions.RetrieveAPInvoice(Creditor3, "3");
			AssertEquals("Invoice Line Count", 2, creditor3Inv3.Lines.Count);

			AssertTransactionHeaderValues(creditor3Inv3, "AP", "INV", "3", "Multiple Jobs", Now.AddDays(10), Now.AddDays(25),
					-635M, -63.50M, 0M, -698.50M, AUD, 1, Now, ZBool.False, Creditor3, null,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor3Inv3);

			TransactionLine cC8Line1 = creditor3Inv3.FindTransactionLine("CST", CC8, job1.PK);
			AssertTransactionLineValues(cC8Line1, "CST", 1, "Job 1 Charge Code 8", -300M, GST1, -30M, WHTFREE1, 0M, -330M, AUD, 1, Now, Now,
					ZBool.False, creditor3Inv3, job1, CC8, CC8.CostAccount, Creditor3);
			AssertTransactionLineDefaults(cC8Line1);

			TransactionLine cC8Line2 = creditor3Inv3.FindTransactionLine("CST", CC8, job2.PK);
			AssertTransactionLineValues(cC8Line2, "CST", 2, "Job 2 Charge Code 8", -335M, GST1, -33.50M, WHTFREE1, 0M, -368.50M, AUD, 1, Now, Now,
					ZBool.False, creditor3Inv3, job2, CC8, CC8.CostAccount, Creditor3);
			AssertTransactionLineDefaults(cC8Line2);

			#endregion

			#region WIP and Accrual Reversal Assertions

			AssertEquals("Charge 1_1 WIP Reversed", false, charge1_1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_2 WIP Reversed", false, charge1_2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_3 WIP Reversed", false, charge1_3WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_4 WIP Reversed", false, charge1_4WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_5 WIP Reversed", false, charge1_5WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_6 WIP Reversed", false, charge1_6WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_7 WIP Reversed", false, charge1_7WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_1 WIP Reversed", false, charge2_1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_2 WIP Reversed", false, charge2_2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_3 WIP Reversed", false, charge2_3WIP.AL_ReverseDate.IsValid);

			AssertEquals("Charge 1_1 Accrual Reversed", true, charge1_1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_2 Accrual Reversed", true, charge1_2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_3 Accrual Reversed", true, charge1_3Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_5 Accrual Reversed", true, charge1_5Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_6 Accrual Reversed", false, charge1_6Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_7 Accrual Reversed", true, charge1_7Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_1 Accrual Reversed", true, charge2_1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_2 Accrual Reversed", true, charge2_2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_3 Accrual Reversed", true, charge2_3Accrual.AL_ReverseDate.IsValid);

			#endregion
		}

		#region TestCreateInvoicesCostsOnlyAsUAInvoices

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateInvoicesCostsOnlyAsUAInvoices()
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertTestCreateInvoicesCostsOnlyAsUAInvoices(false);
		}

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateInvoicesCostsOnlyAsRequests_WhenChargeApprovalActivated()
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertTestCreateInvoicesCostsOnlyAsUAInvoices(true);
		}

		void AssertTestCreateInvoicesCostsOnlyAsUAInvoices(bool isChargeApprovalActivated)
		{
			AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			SetupCharges();
			Charge3.JR_OSCostAmt = -300;

			SetupAPInvoiceInfo();
			Factory.Save();

			InitializeWIPAccruals();

			var guiWrapperMock = new Mock<IPostingJobTransactionsApprovalGUIProvider>();
			if (isChargeApprovalActivated)
			{
				guiWrapperMock.Setup(m => m.IsForPreviewOnly).Returns(false);
				guiWrapperMock.Setup(m => m.IsBulkPosting).Returns(false);
				guiWrapperMock.Setup(m => m.ShowLoginFormForTest).Returns(true);
				var securityProviderMock = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
				securityProviderMock.Setup(m => m.ShouldApprovalRequestBeCreated).Returns(true);
				guiWrapperMock.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>())).Returns(securityProviderMock.Object);
				guiWrapperMock.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction()).Returns(Tuple.Create(Job.PK, (ZString)Job.TablePrefix));
				APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.CheckLevelSecurityRights_ForTestOnly = x => Env.Security.APInvoiceApproval_FirstApproval.IsAllowed;
				guiWrapperMock.Setup(m => m.ShowPostingConfirmationForm(It.IsAny<APInvoiceCharges[]>())).Returns((ZDialogResult)DialogResult.OK);
			}
			InvoicingPostManager creator = new InvoicingPostManager(Job, guiWrapperMock.Object);
			TransactionCreatorHashtable transactions;

			SetUpRegistryForTest();
			try
			{
				if (isChargeApprovalActivated)
				{
					Env.Security.APInvoiceApproval_FirstApproval.IsAllowed = false;
				}
				else
				{
					Env.Security.APUnapprovedInvoicesFirstApproval.IsAllowed = false;
				}
				transactions = creator.CreateTransactions(JobInvoicingPostingOption.Costs);
			}
			finally
			{
				ResetRegistryForTest();
			}

			AssertEquals("Invoice Count", isChargeApprovalActivated ? 2 : 4, transactions.Count);
			AssertEquals("AP Transactions Count", 2, transactions.APTransactionsCount);
			AssertEquals("AR Transactions Count", 0, transactions.ARTransactionsCount);
			var requests = transactions.GetAllAPInvoiceApprovalRequests();
			AssertEquals("Request Count after finalizing the operation", isChargeApprovalActivated ? 2 : 0, requests.Length);

			#region Creditor 1 Invoice 1

			if (isChargeApprovalActivated)
			{
				var request = requests.Where(x => x.PostingDetails.Creditor == Creditor1.OH_Code && x.PostingDetails.TransactionNumber == "1").FirstOrDefault();
				AssertEquals("Request lines", 2, request.PostingDetails.Charges.Count);
				var charges = request.PostingDetails.Charges.Cast<APInvoiceChargesApprovalRequestChargeDetails>();
				var charge = charges.FirstOrDefault(x => x.ChargeCode == CC1.AC_Code && x.JobNumber == Job.JH_JobNum);
				AssertNotNull("Should be created correct charge.", charge);
				charge = charges.FirstOrDefault(x => x.ChargeCode == CC7.AC_Code && x.JobNumber == Job.JH_JobNum);
				AssertNotNull("Should be created correct charge.", charge);
			}
			else
			{
				APInvoice creditor1Inv1 = transactions.RetrieveAPInvoice(Creditor1, "1");
				AssertEquals("Invoice Line Count", 2, creditor1Inv1.Lines.Count);
				AssertEquals("Should be correct type of invoice", LedgerTypes.UnapprovedPayableTransactions, creditor1Inv1.AH_Ledger);
				AssertEquals("Should be correct type of invoice", TransactionTypes.UAInvoice, creditor1Inv1.AH_TransactionType);

				TransactionLine cC1Line = creditor1Inv1.FindTransactionLine(TransactionLineTypes.UnapprovedCost, CC1, Job.PK);
				AssertNotNull("Should be correct type of line", cC1Line);

				TransactionLine cC7Line = creditor1Inv1.FindTransactionLine(TransactionLineTypes.UnapprovedCost, CC7, Job.PK);
				AssertNotNull("Should be correct type of line", cC7Line);
			}

			#endregion

			#region Creditor 2 Invoice 1

			APInvoice creditor2Inv1 = transactions.RetrieveAPInvoice(Creditor2, "1");
			AssertEquals("Invoice Line Count", 1, creditor2Inv1.Lines.Count);

			TransactionLine cC2Line = creditor2Inv1.FindTransactionLine(TransactionLineTypes.Cost, CC2, Job.PK);
			Assert("Should be APInvoiceLine", cC2Line is APInvoiceLine);

			#endregion

			#region Creditor 3 Invoice 1

			APCreditNote creditor3Inv1 = transactions.RetrieveAPCreditNote(Creditor3, "1");
			AssertEquals("Invoice Line Count", 1, creditor3Inv1.Lines.Count);
			Assert("Should be APCreditNote", creditor3Inv1 is APCreditNote);

			TransactionLine cC3Line = creditor3Inv1.FindTransactionLine(TransactionLineTypes.UnapprovedCost, CC3, Job.PK);
			AssertNull("Should be approved", cC3Line);

			#endregion

			#region Creditor 1 Invoice 2

			if (isChargeApprovalActivated)
			{
				var request = requests.Where(x => x.PostingDetails.Creditor == Creditor1.OH_Code && x.PostingDetails.TransactionNumber == "2").FirstOrDefault();
				AssertEquals("Request lines", 1, request.PostingDetails.Charges.Count);
				var charges = request.PostingDetails.Charges.Cast<APInvoiceChargesApprovalRequestChargeDetails>();
				var charge = charges.FirstOrDefault(x => x.ChargeCode == CC5.AC_Code && x.JobNumber == Job.JH_JobNum);
				AssertNotNull("Should be created correct charge.", charge);
			}
			else
			{
				APInvoice creditor1Inv2 = transactions.RetrieveAPInvoice(Creditor1, "2");
				AssertEquals("Invoice Line Count", 1, creditor1Inv2.Lines.Count);
				AssertEquals("Should be correct type of invoice", LedgerTypes.UnapprovedPayableTransactions, creditor1Inv2.AH_Ledger);
				AssertEquals("Should be correct type of invoice", TransactionTypes.UAInvoice, creditor1Inv2.AH_TransactionType);

				TransactionLine cC5Line = creditor1Inv2.FindTransactionLine(TransactionLineTypes.UnapprovedCost, CC5, Job.PK);
				AssertNotNull("Should be correct type of line", cC5Line);
			}

			#endregion

			#region WIP and Accrual Reversal Assertions

			AssertEquals("Charge 1 WIP Reversed", false, Charge1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 WIP Reversed", false, Charge2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 WIP Reversed", false, Charge3WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 4 WIP Reversed", false, Charge4WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 WIP Reversed", false, Charge5WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 WIP Reversed", false, Charge6WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 7 WIP Reversed", false, Charge7WIP.AL_ReverseDate.IsValid);

			AssertEquals("Charge 1 Accrual Reversed", !isChargeApprovalActivated, Charge1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 Accrual Reversed", true, Charge2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 hasn't Accrual", true, Charge3Accrual == null);
			AssertEquals("Charge 5 Accrual Reversed", !isChargeApprovalActivated, Charge5Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 Accrual Reversed", false, Charge6Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 7 Accrual Reversed", !isChargeApprovalActivated, Charge7Accrual.AL_ReverseDate.IsValid);

			#endregion
		}

		#endregion

		#endregion

		#region TEST: Create All Cost Invoices From Consol With Apportionment and Payments

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateAllCostInvoicesFromConsolWithApportionmentAndPayments()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();

			Job job1 = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			CreateExchangeRate(job1, USD, .7M);
			CreateExchangeRate(job1, GBP, .4M);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "Y00001000";
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			ZGuid costSplitGroup1 = cost.PK;
			cost.E6_AC_ChargeCode = CC1.PK;
			cost.E6_OH_Creditor = Creditor3.PK;
			cost.E6_InvoiceNum = "3";
			cost.E6_InvoiceDate = Now.AddDays(10);
			cost.E6_PaymentDate = Now.AddDays(25);
			cost.E6_GC = GlbCompany.CurrentCompany.PK;

			Charge charge1_1 = CreateCharge(job1, CC1, "Job 1 Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge1_2 = CreateCharge(job1, CC2, "Job 1 Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			Charge charge1_3 = CreateCharge(job1, CC3, "Job 1 Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);
			Charge charge1_4 = CreateCharge(job1, CC4, "Job 1 Charge Code 4", null, 0M, null, USD, 500M, Agent);
			Charge charge1_5 = CreateCharge(job1, CC5, "Job 1 Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, LocalClient);
			Charge charge1_6 = CreateCharge(job1, CC6, "Job 1 Charge Code 6", USD, 200M, Creditor2, USD, 275M, Agent);
			Charge charge1_7 = CreateCharge(job1, CC7, "Job 1 Charge Code 7", AUD, 200M, Creditor1, AUD, 300M, LocalClient);
			Charge charge1_8 = CreateCharge(job1, CC8, "Job 1 Charge Code 8", AUD, 300M, Creditor3, AUD, 300M, LocalClient);
			charge1_8.JR_E6 = costSplitGroup1;

			SetAPInvoiceInfo(charge1_1, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_2, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_3, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_5, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_6, "2", Now.AddDays(20), Now.AddDays(30));
			SetAPInvoiceInfo(charge1_7, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_8, "3", Now.AddDays(10), Now.AddDays(25));

			SetAPPaymentInfo(charge1_1, ReceiptTypes.Cash, AUDBankAccount, "CASH");
			SetAPPaymentInfo(charge1_6, ReceiptTypes.CreditCard, USDBankAccount, "CC");
			SetAPPaymentInfo(charge1_7, ReceiptTypes.Cash, AUDBankAccount, "CASH");
			SetAPPaymentInfo(charge1_8, ReceiptTypes.Cash, AUDBankAccount, "CASH2");

			Job job2 = CreateJob("Z00001001", LocalClient, 5M, Agent, 10M);
			ExchangeRate rate2_1 = CreateExchangeRate(job2, USD, .7M);
			ExchangeRate rate2_2 = CreateExchangeRate(job2, GBP, .4M);

			Charge charge2_1 = CreateCharge(job2, CC1, "Job 2 Charge Code 1", AUD, 150M, Creditor1, AUD, 200M, LocalClient);
			Charge charge2_2 = CreateCharge(job2, CC2, "Job 2 Charge Code 2", USD, 250M, Creditor2, USD, 250M, LocalClient);
			Charge charge2_3 = CreateCharge(job2, CC3, "Job 2 Charge Code 3", AUD, 350M, Creditor3, AUD, 400M, LocalClient);
			Charge charge2_4 = CreateCharge(job2, CC8, "Job 2 Charge Code 8", AUD, 335M, Creditor3, AUD, 500M, Agent);
			charge2_4.JR_E6 = costSplitGroup1;

			SetAPInvoiceInfo(charge2_1, "3", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge2_2, "3", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge2_3, "2", Now.AddDays(15), Now.AddDays(25));
			SetAPInvoiceInfo(charge2_4, "3", Now.AddDays(10), Now.AddDays(25));

			SetAPPaymentInfo(charge2_1, ReceiptTypes.Cash, AUDBankAccount, "CASH");
			SetAPPaymentInfo(charge2_2, ReceiptTypes.CreditCard, USDBankAccount, "CC");
			SetAPPaymentInfo(charge2_4, ReceiptTypes.Cash, AUDBankAccount, "CASH2");

			cost.E6_OSCostAmount = charge2_4.JR_OSCostAmt + charge1_8.JR_OSCostAmt;
			cost.E6_LocalCostAmount = charge2_4.JR_LocalCostAmt + charge1_8.JR_LocalCostAmt;

			Factory.Save();

			#region WIPs and Accruals

			AccTransactionLines charge1_1WIP = charge1_1.WIP;
			AccTransactionLines charge1_2WIP = charge1_2.WIP;
			AccTransactionLines charge1_3WIP = charge1_3.WIP;
			AccTransactionLines charge1_4WIP = charge1_4.WIP;
			AccTransactionLines charge1_5WIP = charge1_5.WIP;
			AccTransactionLines charge1_6WIP = charge1_6.WIP;
			AccTransactionLines charge1_7WIP = charge1_7.WIP;
			AccTransactionLines charge1_8WIP = charge1_8.WIP;
			AccTransactionLines charge2_1WIP = charge2_1.WIP;
			AccTransactionLines charge2_2WIP = charge2_2.WIP;
			AccTransactionLines charge2_3WIP = charge2_3.WIP;
			AccTransactionLines charge2_4WIP = charge2_4.WIP;

			AccTransactionLines charge1_1Accrual = charge1_1.Accrual;
			AccTransactionLines charge1_2Accrual = charge1_2.Accrual;
			AccTransactionLines charge1_3Accrual = charge1_3.Accrual;
			AccTransactionLines charge1_5Accrual = charge1_5.Accrual;
			AccTransactionLines charge1_6Accrual = charge1_6.Accrual;
			AccTransactionLines charge1_7Accrual = charge1_7.Accrual;
			AccTransactionLines charge1_8Accrual = charge1_8.Accrual;
			AccTransactionLines charge2_1Accrual = charge2_1.Accrual;
			AccTransactionLines charge2_2Accrual = charge2_2.Accrual;
			AccTransactionLines charge2_3Accrual = charge2_3.Accrual;
			AccTransactionLines charge2_4Accrual = charge2_4.Accrual;

			#endregion

			var jobs = new[] { job1, job2 };
			ConsolInvoicingPostManager creator = new ConsolInvoicingPostManager(Factory, jobs, consol, apps);
			TransactionCreatorHashtable transactions = creator.CreateTransactions(JobInvoicingPostingOption.Costs);
			Factory.Save();
			AssertEquals("Invoice Count", 14, transactions.Count + transactions.GetAllAPPaymentApprovals().Length);

			#region Creditor 1 Invoice 1

			APInvoice creditor1Inv1 = transactions.RetrieveAPInvoice(Creditor1, "1");
			AssertEquals("Invoice Line Count", 2, creditor1Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor1Inv1, "AP", "INV", "1", "Z00001000", Now.AddDays(10), Now.AddDays(20),
					-300M, -10M, 0M, -310M, AUD, 1, Now, ZBool.False, Creditor1, job1,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.True);
			AssertTransactionHeaderDefaults(creditor1Inv1);

			TransactionLine cC1Line1 = creditor1Inv1.FindTransactionLine("CST", CC1, job1.PK);
			AssertTransactionLineValues(cC1Line1, "CST", 1, "Job 1 Charge Code 1", -100M, GST1, -10M, WHTFREE1, 0M, -110M, AUD, 1, Now, Now,
					ZBool.False, creditor1Inv1, job1, CC1, CC1.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC1Line1);

			TransactionLine cC7Line = creditor1Inv1.FindTransactionLine("CST", CC7, job1.PK);
			AssertTransactionLineValues(cC7Line, "CST", 2, "Job 1 Charge Code 7", -200M, GSTFREE1, 0M, WHTFREE1, 0M, -200M, AUD, 1, Now, Now,
					ZBool.False, creditor1Inv1, job1, CC7, CC7.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC7Line);

			#endregion

			#region Creditor 1 Invoice 2

			APInvoice creditor1Inv2 = transactions.RetrieveAPInvoice(Creditor1, "2");
			AssertEquals("Invoice Line Count", 1, creditor1Inv2.Lines.Count);

			AssertTransactionHeaderValues(creditor1Inv2, "AP", "INV", "2", "Z00001000", Now.AddDays(10), Now.AddDays(20),
					-250M, -25M, 0M, -110M, GBP, .4M, Now, ZBool.False, Creditor1, job1,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertOverseaAPTransactionHeaderDefaults_ForBasePostManager(creditor1Inv2);

			TransactionLine cC5Line = creditor1Inv2.FindTransactionLine("CST", CC5, job1.PK);
			AssertTransactionLineValues(cC5Line, "CST", 1, "Job 1 Charge Code 5", -250M, GST1, -25M, WHTFREE1, 0M, -110M, GBP, .4M, Now, Now,
					ZBool.False, creditor1Inv2, job1, CC5, CC5.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC5Line);

			#endregion

			#region Creditor 1 Invoice 3

			APInvoice creditor1Inv3 = transactions.RetrieveAPInvoice(Creditor1, "3");
			AssertEquals("Invoice Line Count", 1, creditor1Inv3.Lines.Count);

			AssertTransactionHeaderValues(creditor1Inv3, "AP", "INV", "3", "Z00001001", Now.AddDays(10), Now.AddDays(20),
					-150M, -15M, 0M, -165M, AUD, 1, Now, ZBool.False, Creditor1, job2,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.True);
			AssertTransactionHeaderDefaults(creditor1Inv3);

			TransactionLine cC1Line2 = creditor1Inv3.FindTransactionLine("CST", CC1, job2.PK);
			AssertTransactionLineValues(cC1Line2, "CST", 1, "Job 2 Charge Code 1", -150M, GST1, -15M, WHTFREE1, 0M, -165M, AUD, 1, Now, Now,
					ZBool.False, creditor1Inv3, job2, CC1, CC1.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC1Line2);

			#endregion

			#region Creditor 2 Invoice 1

			APInvoice creditor2Inv1 = transactions.RetrieveAPInvoice(Creditor2, "1");
			AssertEquals("Invoice Line Count", 1, creditor2Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor2Inv1, "AP", "INV", "1", "Z00001000", Now.AddDays(10), Now.AddDays(20),
					-200M, -20M, -10M, -220M, AUD, 1, Now, ZBool.False, Creditor2, job1,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor2Inv1);

			TransactionLine cC2Line1 = creditor2Inv1.FindTransactionLine("CST", CC2, job1.PK);
			AssertTransactionLineValues(cC2Line1, "CST", 1, "Job 1 Charge Code 2", -200M, GST1, -20M, WHT1, -10M, -220M, AUD, 1, Now, Now,
					ZBool.False, creditor2Inv1, job1, CC2, CC2.CostAccount, Creditor2);
			AssertTransactionLineDefaults(cC2Line1);

			#endregion

			#region Creditor 2 Invoice 2

			APInvoice creditor2Inv2 = transactions.RetrieveAPInvoice(Creditor2, "2");
			AssertEquals("Invoice Line Count", 1, creditor2Inv2.Lines.Count);

			AssertTransactionHeaderValues(creditor2Inv2, "AP", "INV", "2", "Z00001000", Now.AddDays(20), Now.AddDays(30),
					-285.71M, -28.57M, -14.29M, -220M, USD, 0.700013M, Now, ZBool.False, Creditor2, job1,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.True);
			AssertOverseaAPTransactionHeaderDefaults_ForBasePostManager(creditor2Inv2);

			TransactionLine cC6Line = creditor2Inv2.FindTransactionLine("CST", CC6, job1.PK);
			AssertTransactionLineValues(cC6Line, "CST", 1, "Job 1 Charge Code 6", -285.71M, GST1, -28.57M, WHT1, -14.29M, -220M, USD, 0.700013M, Now, Now,
					ZBool.False, creditor2Inv2, job1, CC6, CC6.CostAccount, Creditor2);
			AssertTransactionLineDefaults(cC6Line);

			#endregion

			#region Creditor 2 Invoice 3

			APInvoice creditor2Inv3 = transactions.RetrieveAPInvoice(Creditor2, "3");
			AssertEquals("Invoice Line Count", 1, creditor2Inv3.Lines.Count);

			AssertTransactionHeaderValues(creditor2Inv3, "AP", "INV", "3", "Z00001001", Now.AddDays(10), Now.AddDays(20),
					-357.14M, -35.71M, -17.86M, -275M, USD, 0.700013M, Now, ZBool.False, Creditor2, job2,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.True);
			AssertOverseaAPTransactionHeaderDefaults_ForBasePostManager(creditor2Inv3);

			TransactionLine cC2Line2 = creditor2Inv3.FindTransactionLine("CST", CC2, job2.PK);
			AssertTransactionLineValues(cC2Line2, "CST", 1, "Job 2 Charge Code 2", -357.14M, GST1, -35.71M, WHT1, -17.86M, -275M, USD, 0.700013M, Now, Now,
					ZBool.False, creditor2Inv3, job2, CC2, CC2.CostAccount, Creditor2);
			AssertTransactionLineDefaults(cC2Line2);

			#endregion

			#region Creditor 3 Invoice 1

			APInvoice creditor3Inv1 = transactions.RetrieveAPInvoice(Creditor3, "1");
			AssertEquals("Invoice Line Count", 1, creditor3Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor3Inv1, "AP", "INV", "1", "Z00001000", Now.AddDays(10), Now.AddDays(20),
					-300M, 0M, -15M, -300M, AUD, 1, Now, ZBool.False, Creditor3, job1,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor3Inv1);

			TransactionLine cC3Line = creditor3Inv1.FindTransactionLine("CST", CC3, job1.PK);
			AssertTransactionLineValues(cC3Line, "CST", 1, "Job 1 Charge Code 3", -300M, GSTFREE1, 0M, WHT1, -15M, -300M, AUD, 1, Now, Now,
					ZBool.False, creditor3Inv1, job1, CC3, CC3.CostAccount, Creditor3);
			AssertTransactionLineDefaults(cC3Line);

			#endregion

			#region Creditor 3 Invoice 2

			APInvoice creditor3Inv2 = transactions.RetrieveAPInvoice(Creditor3, "2");
			AssertEquals("Invoice Line Count", 1, creditor3Inv2.Lines.Count);

			AssertTransactionHeaderValues(creditor3Inv2, "AP", "INV", "2", "Z00001001", Now.AddDays(15), Now.AddDays(25),
					-350, 0M, -17.50M, -350M, AUD, 1, Now, ZBool.False, Creditor3, job2,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor3Inv2);

			TransactionLine cC3Line2 = creditor3Inv2.FindTransactionLine("CST", CC3, job2.PK);
			AssertTransactionLineValues(cC3Line2, "CST", 1, "Job 2 Charge Code 3", -350M, GSTFREE1, 0M, WHT1, -17.50M, -350M, AUD, 1, Now, Now,
					ZBool.False, creditor3Inv2, job2, CC3, CC3.CostAccount, Creditor3);
			AssertTransactionLineDefaults(cC3Line2);

			#endregion

			#region Creditor 3 Invoice 3

			APInvoice creditor3Inv3 = transactions.RetrieveAPInvoice(Creditor3, "3");
			AssertEquals("Invoice Line Count", 2, creditor3Inv3.Lines.Count);

			AssertTransactionHeaderValues(creditor3Inv3, "AP", "INV", "3", "Multiple Jobs", Now.AddDays(10), Now.AddDays(25),
					-635M, -63.50M, 0M, -698.50M, AUD, 1, Now, ZBool.False, Creditor3, null,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.True);
			AssertTransactionHeaderDefaults(creditor3Inv3);

			TransactionLine cC8Line1 = creditor3Inv3.FindTransactionLine("CST", CC8, job1.PK);
			AssertTransactionLineValues(cC8Line1, "CST", 1, "Job 1 Charge Code 8", -300M, GST1, -30M, WHTFREE1, 0M, -330M, AUD, 1, Now, Now,
					ZBool.False, creditor3Inv3, job1, CC8, CC8.CostAccount, Creditor3);
			AssertTransactionLineDefaults(cC8Line1);

			TransactionLine cC8Line2 = creditor3Inv3.FindTransactionLine("CST", CC8, job2.PK);
			AssertTransactionLineValues(cC8Line2, "CST", 2, "Job 2 Charge Code 8", -335M, GST1, -33.50M, WHTFREE1, 0M, -368.50M, AUD, 1, Now, Now,
					ZBool.False, creditor3Inv3, job2, CC8, CC8.CostAccount, Creditor3);
			AssertTransactionLineDefaults(cC8Line2);

			#endregion

			#region Job1 AUD Payment

			APPayment aUDPayment1 = transactions.RetrieveAPPayment_ForTestOnly(Creditor1, AUDBankAccount, "CSH", "CASH", job1.JH_JobNum);
			AssertTransactionHeaderValues(aUDPayment1, "AP", "PAY", null, "AP Payment Z00001000", Now, ZDateTime.Empty,
					310.00M, 0M, 0M, 310M, AUD, 1, Now, ZBool.False, Creditor1, null, ZString.Empty, 0, "CASH", "CSH",
					AUDBankAccount, ZBool.True);
			AssertTransactionHeaderDefaults(aUDPayment1);

			#endregion

			#region Job2 AUD Payment

			APPayment aUDPayment2 = transactions.RetrieveAPPayment_ForTestOnly(Creditor1, AUDBankAccount, "CSH", "CASH", job2.JH_JobNum);
			AssertTransactionHeaderValues(aUDPayment2, "AP", "PAY", null, "AP Payment Z00001001", Now, ZDateTime.Empty,
					165.00M, 0M, 0M, 165M, AUD, 1, Now, ZBool.False, Creditor1, null, ZString.Empty, 0, "CASH", "CSH",
					AUDBankAccount, ZBool.True);
			AssertTransactionHeaderDefaults(aUDPayment2);

			#endregion

			#region Job1 USD Payment

			APPayment uSDPayment1 = transactions.RetrieveAPPayment_ForTestOnly(Creditor2, USDBankAccount, "CCD", "CC", job1.JH_JobNum);
			AssertTransactionHeaderValues(uSDPayment1, "AP", "PAY", null, "AP Payment Z00001000", Now, ZDateTime.Empty,
					314.28M, 0M, 0M, 220M, USD, 0.700013M, Now, ZBool.False, Creditor2, null, ZString.Empty, 0, "CC", "CCD",
					USDBankAccount, ZBool.True);
			AssertTransactionHeaderDefaults(uSDPayment1);

			#endregion

			#region Job2 USD Payment

			APPayment uSDPayment2 = transactions.RetrieveAPPayment_ForTestOnly(Creditor2, USDBankAccount, "CCD", "CC", job2.JH_JobNum);
			AssertTransactionHeaderValues(uSDPayment2, "AP", "PAY", null, "AP Payment Z00001001", Now, ZDateTime.Empty,
					392.85M, 0M, 0M, 275M, USD, 0.700013M, Now, ZBool.False, Creditor2, null, ZString.Empty, 0, "CC", "CCD",
					USDBankAccount, ZBool.True);
			AssertTransactionHeaderDefaults(uSDPayment2);

			#endregion

			#region Apportionment Payment

			APPayment apportionmentPayment = transactions.RetrieveAPPayment_ForTestOnly(Creditor3, AUDBankAccount, "CSH", "CASH2", ZString.Empty);
			AssertTransactionHeaderValues(apportionmentPayment, "AP", "PAY", null, "AP Payment Y00001000", Now, ZDateTime.Empty,
					698.50M, 0M, 0M, 698.50M, AUD, 1, Now, ZBool.False, Creditor3, null, ZString.Empty, 0, "CASH2", "CSH",
					AUDBankAccount, ZBool.True);
			AssertTransactionHeaderDefaults(apportionmentPayment);

			#endregion

			TransactionMatchLinkGroup group1 = new TransactionMatchLinkGroup(aUDPayment1.RelatedPaymentApproval.NewPaymentMatchingObject.MatchLinks);
			TransactionMatchLinkGroup group2 = new TransactionMatchLinkGroup(uSDPayment1.RelatedPaymentApproval.NewPaymentMatchingObject.MatchLinks);
			TransactionMatchLinkGroup group3 = new TransactionMatchLinkGroup(aUDPayment2.RelatedPaymentApproval.NewPaymentMatchingObject.MatchLinks);
			TransactionMatchLinkGroup group4 = new TransactionMatchLinkGroup(uSDPayment2.RelatedPaymentApproval.NewPaymentMatchingObject.MatchLinks);
			TransactionMatchLinkGroup group5 = new TransactionMatchLinkGroup(apportionmentPayment.RelatedPaymentApproval.NewPaymentMatchingObject.MatchLinks);

			#region AUDPayment1 Matching

			AssertEquals("Group 1 Match Count", 2, group1.Count);
			TransactionMatchLink payment1MatchLink = group1.FindMatchLinkByTransactionHeaderAndAmount(aUDPayment1, 310M);
			TransactionMatchLink invoice1aMatchLink = group1.FindMatchLinkByTransactionHeaderAndAmount(creditor1Inv1, -310M);

			AssertNotNull("Payment1 Match Link Found", payment1MatchLink);
			AssertNotNull("Invoice1a Match Link Found", invoice1aMatchLink);

			AssertInvoiceAndPaymentAreInSameGroup(group1, invoice1aMatchLink, payment1MatchLink);

			AssertEquals("Payment1 Match Date", Now.ToString("yyyy MMM dd"), payment1MatchLink.AP_MatchDate.ToString("yyyy MMM dd"));
			AssertEquals("Invoice1a Match Date", Now.ToString("yyyy MMM dd"), invoice1aMatchLink.AP_MatchDate.ToString("yyyy MMM dd"));

			AssertMatchLinkDefaults(payment1MatchLink);
			AssertMatchLinkDefaults(invoice1aMatchLink);

			AssertAPInvoiceShowsAsPaid(creditor1Inv1);

			#endregion

			#region USDPayment1 Matching

			AssertEquals("Group 2 Match Count", 2, group2.Count);
			TransactionMatchLink payment2MatchLink = group2.FindMatchLinkByTransactionHeaderAndAmount(uSDPayment1, 314.28M);
			TransactionMatchLink invoice2aMatchLink = group2.FindMatchLinkByTransactionHeaderAndAmount(creditor2Inv2, -314.28M);

			AssertNotNull("Payment 2 Match Link Found", payment2MatchLink);
			AssertNotNull("Invoice 2a Match Link Found", invoice2aMatchLink);

			AssertInvoiceAndPaymentAreInSameGroup(group2, invoice2aMatchLink, payment2MatchLink);

			AssertEquals("Payment2 Match Date", Now.ToString("yyyy MMM dd"), payment2MatchLink.AP_MatchDate.ToString("yyyy MMM dd"));
			AssertEquals("Invoice2a Match Date", Now.ToString("yyyy MMM dd"), invoice2aMatchLink.AP_MatchDate.ToString("yyyy MMM dd"));

			AssertMatchLinkDefaults(payment2MatchLink);
			AssertMatchLinkDefaults(invoice2aMatchLink);

			AssertAPInvoiceShowsAsPaid(creditor2Inv2);

			#endregion

			#region AUDPayment2 Matching

			AssertEquals("Group 3 Match Count", 2, group3.Count);
			TransactionMatchLink payment3MatchLink = group3.FindMatchLinkByTransactionHeaderAndAmount(aUDPayment2, 165M);
			TransactionMatchLink invoice3aMatchLink = group3.FindMatchLinkByTransactionHeaderAndAmount(creditor1Inv3, -165M);

			AssertNotNull("Payment3 Match Link Found", payment3MatchLink);
			AssertNotNull("Invoice3a Match Link Found", invoice3aMatchLink);

			AssertInvoiceAndPaymentAreInSameGroup(group3, invoice3aMatchLink, payment3MatchLink);

			AssertEquals("Payment3 Match Date", Now.ToString("yyyy MMM dd"), payment3MatchLink.AP_MatchDate.ToString("yyyy MMM dd"));
			AssertEquals("Invoice3a Match Date", Now.ToString("yyyy MMM dd"), invoice3aMatchLink.AP_MatchDate.ToString("yyyy MMM dd"));

			AssertMatchLinkDefaults(payment3MatchLink);
			AssertMatchLinkDefaults(invoice3aMatchLink);

			AssertAPInvoiceShowsAsPaid(creditor1Inv3);

			#endregion

			#region USDPayment2 Matching

			AssertEquals("Group 4 Match Count", 2, group4.Count);
			TransactionMatchLink payment4MatchLink = group4.FindMatchLinkByTransactionHeaderAndAmount(uSDPayment2, 392.85M);
			TransactionMatchLink invoice4aMatchLink = group4.FindMatchLinkByTransactionHeaderAndAmount(creditor2Inv3, -392.85M);

			AssertNotNull("Payment4 Match Link Found", payment4MatchLink);
			AssertNotNull("Invoice4a Match Link Found", invoice4aMatchLink);

			AssertInvoiceAndPaymentAreInSameGroup(group4, invoice4aMatchLink, payment4MatchLink);

			AssertEquals("Payment4 Match Date", Now.ToString("yyyy MMM dd"), payment4MatchLink.AP_MatchDate.ToString("yyyy MMM dd"));
			AssertEquals("Invoice4a Match Date", Now.ToString("yyyy MMM dd"), invoice4aMatchLink.AP_MatchDate.ToString("yyyy MMM dd"));

			AssertMatchLinkDefaults(payment4MatchLink);
			AssertMatchLinkDefaults(invoice4aMatchLink);

			AssertAPInvoiceShowsAsPaid(creditor2Inv3);

			#endregion

			#region Apportionment Matching

			AssertEquals("Group 5 Match Count", 2, group5.Count);
			TransactionMatchLink payment5MatchLink = group5.FindMatchLinkByTransactionHeaderAndAmount(apportionmentPayment, 698.50M);
			TransactionMatchLink invoice5aMatchLink = group5.FindMatchLinkByTransactionHeaderAndAmount(creditor3Inv3, -698.50M);

			AssertNotNull("Payment5 Match Link Found", payment5MatchLink);
			AssertNotNull("Invoice5a Match Link Found", invoice5aMatchLink);

			AssertInvoiceAndPaymentAreInSameGroup(group5, invoice5aMatchLink, payment5MatchLink);

			AssertEquals("Payment5 Match Date", Now.ToString("yyyy MMM dd"), payment5MatchLink.AP_MatchDate.ToString("yyyy MMM dd"));
			AssertEquals("Invoice5a Match Date", Now.ToString("yyyy MMM dd"), invoice5aMatchLink.AP_MatchDate.ToString("yyyy MMM dd"));

			AssertMatchLinkDefaults(payment5MatchLink);
			AssertMatchLinkDefaults(invoice5aMatchLink);

			AssertAPInvoiceShowsAsPaid(creditor3Inv3);

			#endregion

			#region WIP and Accrual Reversal Assertions

			AssertEquals("Charge 1_1 WIP Reversed", false, charge1_1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_2 WIP Reversed", false, charge1_2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_3 WIP Reversed", false, charge1_3WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_4 WIP Reversed", false, charge1_4WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_5 WIP Reversed", false, charge1_5WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_6 WIP Reversed", false, charge1_6WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_7 WIP Reversed", false, charge1_7WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_1 WIP Reversed", false, charge2_1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_2 WIP Reversed", false, charge2_2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_3 WIP Reversed", false, charge2_3WIP.AL_ReverseDate.IsValid);

			AssertEquals("Charge 1_1 Accrual Reversed", true, charge1_1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_2 Accrual Reversed", true, charge1_2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_3 Accrual Reversed", true, charge1_3Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_5 Accrual Reversed", true, charge1_5Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_6 Accrual Reversed", true, charge1_6Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_7 Accrual Reversed", true, charge1_7Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_1 Accrual Reversed", true, charge2_1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_2 Accrual Reversed", true, charge2_2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_3 Accrual Reversed", true, charge2_3Accrual.AL_ReverseDate.IsValid);

			#endregion
		}

		#endregion

		#region TEST: Create All Agent Invoices From Consol Bill In Local Currency

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateAllAgentInvoicesFromConsolBillInLocalCurrency()
		{
			string consolNumber = "C00001000";
			ForwardingConsol consol = CreateConsol("AUSYD", "USLAX", consolNumber);
			Job job1 = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			job1.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			CreateExchangeRate(job1, USD, .7M);
			CreateExchangeRate(job1, GBP, .4M);

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);

			Charge charge1_1 = CreateCharge(job1, CC1, "Job 1 Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge1_2 = CreateCharge(job1, CC2, "Job 1 Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			Charge charge1_3 = CreateCharge(job1, CC3, "Job 1 Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);
			charge1_3.JR_PreventInvoicePrintGrouping = ZBool.True;
			Charge charge1_4 = CreateCharge(job1, CC4, "Job 1 Charge Code 4", null, 0M, null, USD, 500M, Agent);
			Charge charge1_5 = CreateCharge(job1, CC5, "Job 1 Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, LocalClient);
			Charge charge1_6 = CreateCharge(job1, CC6, "Job 1 Charge Code 6", USD, 200M, Creditor2, USD, 275M, Agent);

			Job job2 = CreateJob("Z00001001", LocalClient, 5M, Agent, 10M);
			job2.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			CreateExchangeRate(job2, USD, .7M);
			CreateExchangeRate(job2, GBP, .4M);

			Charge charge2_1 = CreateCharge(job2, CC1, "Job 2 Charge Code 1", AUD, 300M, Creditor2, AUD, 350M, Agent);
			Charge charge2_2 = CreateCharge(job2, CC7, "Job 2 Charge Code 7", GBP, 150M, Creditor1, GBP, 200M, Agent);
			charge2_2.JR_PreventInvoicePrintGrouping = ZBool.True;
			Charge charge2_3 = CreateCharge(job2, CC5, "Job 2 Charge Code 5", USD, 200M, Creditor3, USD, 300M, LocalClient);
			Charge charge2_4 = CreateCharge(job2, CC2, "Job 2 Charge Code 2", USD, 300M, Creditor3, USD, 400M, Agent);

			Factory.Save();

			#region WIPs and Accruals

			AccTransactionLines charge1_1WIP = charge1_1.WIP;
			AccTransactionLines charge1_2WIP = charge1_2.WIP;
			AccTransactionLines charge1_3WIP = charge1_3.WIP;
			AccTransactionLines charge1_4WIP = charge1_4.WIP;
			AccTransactionLines charge1_5WIP = charge1_5.WIP;
			AccTransactionLines charge1_6WIP = charge1_6.WIP;
			AccTransactionLines charge2_1WIP = charge2_1.WIP;
			AccTransactionLines charge2_2WIP = charge2_2.WIP;
			AccTransactionLines charge2_3WIP = charge2_3.WIP;
			AccTransactionLines charge2_4WIP = charge2_4.WIP;

			AccTransactionLines charge1_1Accrual = charge1_1.Accrual;
			AccTransactionLines charge1_2Accrual = charge1_2.Accrual;
			AccTransactionLines charge1_3Accrual = charge1_3.Accrual;
			AccTransactionLines charge1_5Accrual = charge1_5.Accrual;
			AccTransactionLines charge1_6Accrual = charge1_6.Accrual;
			AccTransactionLines charge2_1Accrual = charge2_1.Accrual;
			AccTransactionLines charge2_2Accrual = charge2_2.Accrual;
			AccTransactionLines charge2_3Accrual = charge2_3.Accrual;
			AccTransactionLines charge2_4Accrual = charge2_4.Accrual;

			#endregion

			var jobs = new[] { job1, job2 };

			ConsolInvoicingPostManager creator = new ConsolInvoicingPostManager(Factory, jobs, consol, apps);
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			TransactionCreatorHashtable transactions = creator.CreateTransactions(JobInvoicingPostingOption.Agent);
			AssertEquals("Payables Trx Count", 0, transactions.APTransactionsCount);
			AssertEquals("Receivable Transactions Count", 1, transactions.ARTransactionsCount);

			#region AUD Invoice

			InvoicingBase[] aUDInvoices = creator.Poster.GetInvoices(AUD, Agent);
			AssertEquals(1, aUDInvoices.Length);
			ARInvoice aUDInvoice = (ARInvoice)aUDInvoices[0];
			AssertEquals("Invoice Line Count", 6, aUDInvoice.Lines.Count);

			AssertTransactionHeaderValues(aUDInvoice, "AR", "INV", null, "FREIGHT CONSOL INVOICE", Now, Now,
					3120.64M, 142.14M, 71.08M, 3262.78M, AUD, 1M, Now, ZBool.False, Agent, null,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(aUDInvoice, true);

			TransactionLine cC3Line = aUDInvoice.FindTransactionLine("REV", CC3, job1.PK);
			AssertTransactionLineValues(cC3Line, "REV", 3, "Job 1 Charge Code 3", 350M, GSTFREE1, 0M, WHT1, 17.50M, 350M, AUD, 1, Now,
					ZBool.True, aUDInvoice, job1, CC3, CC3.CostAccount, Agent);
			AssertTransactionLineDefaults(cC3Line);

			TransactionLine cC4Line = aUDInvoice.FindTransactionLine("REV", CC4, job1.PK);
			AssertTransactionLineValues(cC4Line, "REV", 4, "Job 1 Charge Code 4", 793.65M, GSTFREE1, 0M, WHTFREE1, 0M, 793.65M, AUD, 1, Now,
					ZBool.False, aUDInvoice, job1, CC4, CC4.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC4Line);

			TransactionLine cC6Line = aUDInvoice.FindTransactionLine("REV", CC6, job1.PK);
			AssertTransactionLineValues(cC6Line, "REV", 6, "Job 1 Charge Code 6", 436.51M, GST1, 43.65M, WHT1, 21.83M, 480.16M, AUD, 1, Now,
					ZBool.False, aUDInvoice, job1, CC6, CC6.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC6Line);

			TransactionLine cC1Line = aUDInvoice.FindTransactionLine("REV", CC1, job2.PK);
			AssertTransactionLineValues(cC1Line, "REV", 1, "Job 2 Charge Code 1", 350M, GST1, 35M, WHTFREE1, 0M, 385M, AUD, 1, Now,
					ZBool.False, aUDInvoice, job2, CC1, CC1.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC1Line);

			TransactionLine cC7Line = aUDInvoice.FindTransactionLine("REV", CC7, job2.PK);
			AssertTransactionLineValues(cC7Line, "REV", 2, "Job 2 Charge Code 7", 555.56M, GSTFREE1, 0M, WHTFREE1, 0M, 555.56M, AUD, 1, Now,
					ZBool.True, aUDInvoice, job2, CC7, CC7.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC7Line);

			TransactionLine cC2Line = aUDInvoice.FindTransactionLine("REV", CC2, job2.PK);
			AssertTransactionLineValues(cC2Line, "REV", 4, "Job 2 Charge Code 2", 634.92M, GST1, 63.49M, WHT1, 31.75M, 698.41M, AUD, 1, Now,
					ZBool.False, aUDInvoice, job2, CC2, CC2.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC2Line);

			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, consolNumber);

			#endregion

			#region WIP and Accrual Reversal Assertions

			AssertEquals("Charge 1_1 WIP Reversed", false, charge1_1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_2 WIP Reversed", false, charge1_2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_3 WIP Reversed", true, charge1_3WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_4 WIP Reversed", true, charge1_4WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_5 WIP Reversed", false, charge1_5WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_6 WIP Reversed", true, charge1_6WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_1 WIP Reversed", true, charge2_1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_2 WIP Reversed", true, charge2_2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_3 WIP Reversed", false, charge2_3WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_4 WIP Reversed", true, charge2_4WIP.AL_ReverseDate.IsValid);

			AssertEquals("Charge 1_1 Accrual Reversed", false, charge1_1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_2 Accrual Reversed", false, charge1_2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_3 Accrual Reversed", false, charge1_3Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_5 Accrual Reversed", false, charge1_5Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_6 Accrual Reversed", false, charge1_6Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_1 Accrual Reversed", false, charge2_1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_2 Accrual Reversed", false, charge2_2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_3 Accrual Reversed", false, charge2_3Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_4 Accrual Reversed", false, charge2_4Accrual.AL_ReverseDate.IsValid);

			#endregion
		}

		#endregion

		#region TEST: Create All Agent Invoices From Consol Bill In Foreign Currency

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateAllAgentInvoicesFromConsolBillInForeignCurrency()
		{
			string consolNumber = "C00001000";
			ForwardingConsol consol = CreateConsol("AUSYD", "USLAX", consolNumber);
			Job job1 = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			job1.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			CreateExchangeRate(job1, USD, .7M);
			CreateExchangeRate(job1, GBP, .4M);

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);

			Charge charge1_1 = CreateCharge(job1, CC1, "Job 1 Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge1_2 = CreateCharge(job1, CC2, "Job 1 Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			Charge charge1_3 = CreateCharge(job1, CC3, "Job 1 Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);
			charge1_3.JR_PreventInvoicePrintGrouping = ZBool.True;
			Charge charge1_4 = CreateCharge(job1, CC4, "Job 1 Charge Code 4", null, 0M, null, USD, 500M, Agent);
			charge1_4.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			Charge charge1_5 = CreateCharge(job1, CC5, "Job 1 Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, LocalClient);
			charge1_5.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			Charge charge1_6 = CreateCharge(job1, CC6, "Job 1 Charge Code 6", USD, 200M, Creditor2, USD, 275M, Agent);
			charge1_6.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			Job job2 = CreateJob("Z00001001", LocalClient, 5M, Agent, 10M);
			job2.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			CreateExchangeRate(job2, USD, .7M);
			CreateExchangeRate(job2, GBP, .4M);

			Charge charge2_1 = CreateCharge(job2, CC1, "Job 2 Charge Code 1", AUD, 300M, Creditor2, AUD, 350M, Agent);
			Charge charge2_2 = CreateCharge(job2, CC7, "Job 2 Charge Code 7", GBP, 150M, Creditor1, GBP, 200M, Agent);
			charge2_2.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			charge2_2.JR_PreventInvoicePrintGrouping = ZBool.True;
			Charge charge2_3 = CreateCharge(job2, CC5, "Job 2 Charge Code 5", USD, 200M, Creditor3, USD, 300M, LocalClient);
			charge2_3.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			Charge charge2_4 = CreateCharge(job2, CC2, "Job 2 Charge Code 2", USD, 300M, Creditor3, USD, 400M, Agent);
			charge2_4.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			charge2_4.JR_OSSellAmt = 400M;

			Factory.Save();

			#region WIPs and Accruals

			AccTransactionLines charge1_1WIP = charge1_1.WIP;
			AccTransactionLines charge1_2WIP = charge1_2.WIP;
			AccTransactionLines charge1_3WIP = charge1_3.WIP;
			AccTransactionLines charge1_4WIP = charge1_4.WIP;
			AccTransactionLines charge1_5WIP = charge1_5.WIP;
			AccTransactionLines charge1_6WIP = charge1_6.WIP;
			AccTransactionLines charge2_1WIP = charge2_1.WIP;
			AccTransactionLines charge2_2WIP = charge2_2.WIP;
			AccTransactionLines charge2_3WIP = charge2_3.WIP;
			AccTransactionLines charge2_4WIP = charge2_4.WIP;

			AccTransactionLines charge1_1Accrual = charge1_1.Accrual;
			AccTransactionLines charge1_2Accrual = charge1_2.Accrual;
			AccTransactionLines charge1_3Accrual = charge1_3.Accrual;
			AccTransactionLines charge1_5Accrual = charge1_5.Accrual;
			AccTransactionLines charge1_6Accrual = charge1_6.Accrual;
			AccTransactionLines charge2_1Accrual = charge2_1.Accrual;
			AccTransactionLines charge2_2Accrual = charge2_2.Accrual;
			AccTransactionLines charge2_3Accrual = charge2_3.Accrual;
			AccTransactionLines charge2_4Accrual = charge2_4.Accrual;

			#endregion

			var jobs = new[] { job1, job2 };

			ConsolInvoicingPostManager creator = new ConsolInvoicingPostManager(Factory, jobs, consol, apps);
			creator.ExportAgentPosting += new ExportAgentPostingEventHandler(Creator_ExportAgentPosting);

			TransactionCreatorHashtable transactions = creator.CreateTransactions(JobInvoicingPostingOption.Agent);
			AssertEquals("Payables Trx Count", 0, transactions.APTransactionsCount);
			AssertEquals("Receivable Transactions Count", 3, transactions.ARTransactionsCount);

			#region AUD Invoice

			InvoicingBase[] aUDInvoices = creator.Poster.GetInvoices(AUD, Agent);
			AssertEquals(1, aUDInvoices.Length);
			ARInvoice aUDInvoice = (ARInvoice)aUDInvoices[0];
			AssertEquals("Invoice Line Count", 2, aUDInvoice.Lines.Count);

			AssertTransactionHeaderValues(aUDInvoice, "AR", "INV", null, "FREIGHT CONSOL INVOICE", Now, Now,
					700.00M, 35M, 17.50M, 735.00M, AUD, 1M, Now, ZBool.False, Agent, null,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(aUDInvoice, true);

			TransactionLine cC3Line = aUDInvoice.FindTransactionLine("REV", CC3, job1.PK);
			AssertTransactionLineValues(cC3Line, "REV", 3, "Job 1 Charge Code 3", 350M, GSTFREE1, 0M, WHT1, 17.50M, 350.00M, AUD, 1, Now,
					ZBool.True, aUDInvoice, job1, CC3, CC3.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC3Line);

			TransactionLine cC1Line = aUDInvoice.FindTransactionLine("REV", CC1, job2.PK);
			AssertTransactionLineValues(cC1Line, "REV", 1, "Job 2 Charge Code 1", 350M, GST1, 35M, WHTFREE1, 0M, 385M, AUD, 1, Now,
					ZBool.False, aUDInvoice, job2, CC1, CC1.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC1Line);

			#endregion

			#region USD Invoice

			InvoicingBase[] uSDInvoices = creator.Poster.GetInvoices(USD, Agent);
			AssertEquals(1, uSDInvoices.Length);
			InvoicingBase uSDInvoice = uSDInvoices[0];
			AssertEquals("Invoice Line Count", 3, uSDInvoice.Lines.Count);

			AssertTransactionHeaderValues(uSDInvoice, "AR", "INV", null, "FREIGHT CONSOL INVOICE", Now, Now,
					1678.58M, 96.43M, 48.21M, 1242.50M, USD, .7M, Now, ZBool.False, Agent, null,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(uSDInvoice, true);

			TransactionLine cC4Line = uSDInvoice.FindTransactionLine("REV", CC4, job1.PK);
			AssertTransactionLineValues(cC4Line, "REV", 4, "Job 1 Charge Code 4", 714.29M, GSTFREE1, 0M, WHTFREE1, 0M, 500.00M, USD, .7M, Now,
					ZBool.False, uSDInvoice, job1, CC4, CC4.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC4Line);

			TransactionLine cC6Line = uSDInvoice.FindTransactionLine("REV", CC6, job1.PK);
			AssertTransactionLineValues(cC6Line, "REV", 6, "Job 1 Charge Code 6", 392.86M, GST1, 39.29M, WHT1, 19.64M, 302.50M, USD, .7M, Now,
					ZBool.False, uSDInvoice, job1, CC6, CC6.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC6Line);

			TransactionLine cC2Line = uSDInvoice.FindTransactionLine("REV", CC2, job2.PK);
			AssertTransactionLineValues(cC2Line, "REV", 4, "Job 2 Charge Code 2", 571.43M, GST1, 57.14M, WHT1, 28.57M, 440.00M, USD, .7M, Now,
					ZBool.False, uSDInvoice, job2, CC2, CC2.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC2Line);

			#endregion

			#region GBP Invoice

			InvoicingBase[] gBPInvoices = creator.Poster.GetInvoices(GBP, Agent);
			AssertEquals(1, gBPInvoices.Length);
			InvoicingBase gBPInvoice = gBPInvoices[0];
			AssertEquals("Invoice Line Count", 1, gBPInvoice.Lines.Count);

			AssertTransactionHeaderValues(gBPInvoice, "AR", "INV", null, "FREIGHT CONSOL INVOICE", Now, Now,
					500.00M, 0M, 0M, 200.00M, GBP, .4M, Now, ZBool.False, Agent, null,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(gBPInvoice, true);

			TransactionLine cC7Line = gBPInvoice.FindTransactionLine("REV", CC7, job2.PK);
			AssertTransactionLineValues(cC7Line, "REV", 2, "Job 2 Charge Code 7", 500M, GSTFREE1, 0M, WHTFREE1, 0M, 200M, GBP, .4M, Now,
					ZBool.True, gBPInvoice, job2, CC7, CC7.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC7Line);

			#endregion

			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, consolNumber);
			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, consolNumber + "/A");
			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, consolNumber + "/B");

			#region WIP and Accrual Reversal Assertions

			AssertEquals("Charge 1_1 WIP Reversed", false, charge1_1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_2 WIP Reversed", false, charge1_2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_3 WIP Reversed", true, charge1_3WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_4 WIP Reversed", true, charge1_4WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_5 WIP Reversed", false, charge1_5WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_6 WIP Reversed", true, charge1_6WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_1 WIP Reversed", true, charge2_1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_2 WIP Reversed", true, charge2_2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_3 WIP Reversed", false, charge2_3WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_4 WIP Reversed", true, charge2_4WIP.AL_ReverseDate.IsValid);

			AssertEquals("Charge 1_1 Accrual Reversed", false, charge1_1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_2 Accrual Reversed", false, charge1_2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_3 Accrual Reversed", false, charge1_3Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_5 Accrual Reversed", false, charge1_5Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_6 Accrual Reversed", false, charge1_6Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_1 Accrual Reversed", false, charge2_1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_2 Accrual Reversed", false, charge2_2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_3 Accrual Reversed", false, charge2_3Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_4 Accrual Reversed", false, charge2_4Accrual.AL_ReverseDate.IsValid);

			#endregion
		}

		#endregion

		#region TEST: Set Receivables Transaction Category When Post Oversea Agent Invoice

		public void TestSetReceivablesTransactionCategoryWhenPostOverseaAgentInvoice()
		{
			Agent.MiscServ.OM_FWBillCollectFeesOnSingleInvoice = true;
			string consolNumber = "C00001000";
			var consol = CreateConsol("AUSYD", "USLAX", consolNumber);
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);
			consol.SetDefaultReceivingForwarderAddress(Agent);

			var job1 = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			job1.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var job2 = CreateJob("Z00001001", LocalClient, 5M, Agent, 10M);
			job2.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var charge1_1 = CreateCharge(job1, CC1, "Job 1 Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, Agent);
			var charge2_1 = CreateCharge(job2, CC1, "Job 2 Charge Code 1", AUD, 300M, Creditor2, AUD, 350M, Agent);
			var apps = new ApportionmentListing(Factory, consol);

			Factory.Save();

			var testPostManager1 = new ConsolInvoicingPostManager(Factory, new[] { job1 }, consol, apps);
			testPostManager1.ExportAgentPosting += new ExportAgentPostingEventHandler(Creator_ExportAgentPosting);
			var transactions1 = testPostManager1.CreateTransactions(JobInvoicingPostingOption.Agent);
			var arTransactions1 = transactions1.GetAllARInvoicesAndCreditNotes();

			AssertEquals("Expect 1 AR Invoice", 1, arTransactions1.Length);
			Assert(arTransactions1[0] is ARInvoice);
			AssertEquals("Transaction Category", "CUR", arTransactions1[0].AH_TransactionCategory);

			var testPostManager2 = new ConsolInvoicingPostManager(Factory, new[] { job2 }, consol, apps);
			testPostManager2.ExportAgentPosting += new ExportAgentPostingEventHandler(TestPostManager_ExportAgentPosting_SetLocalCurrency);
			var transactions2 = testPostManager2.CreateTransactions(JobInvoicingPostingOption.Agent);
			var arTransactions2 = transactions2.GetAllARInvoicesAndCreditNotes();

			AssertEquals("Expect 1 AR Invoice", 1, arTransactions2.Length);
			Assert(arTransactions2[0] is ARInvoice);
			AssertEquals("Transaction Category", "FIN", arTransactions2[0].AH_TransactionCategory);
		}

		#endregion

		#region TEST: Community Regions Settings is being considered when creating Agent Invoices

		[TestDate(2014, 12, 30, 12, 00, 00)]
		[ExpectNoExceptions("No Critical Validation Error should be thrown")]
		public void TestAgentInvoicesCreationConsidersCommunityRegionsSettings()
		{
			var company = TestObjectCreator.CreateNewCompany("NL1", Core.Constants.CountryCodes.Netherlands);
			var branch = TestObjectCreator.CreateNewBranch(company, "BR1");
			Factory.Save();

			var countryBE = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Belgium));
			var countryDE = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Germany));
			var countryNL = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Netherlands));

			FreightDataRegistry.Instance.CommunityRegionsForDirectionCalculation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { countryBE.PK.ToGuid(), countryDE.PK.ToGuid(), countryNL.PK.ToGuid() });

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				Job job1 = CreateJob("S0001", TestObjectCreator.LocalClient, 5M, TestObjectCreator.Agent, 10M);
				job1.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				job1.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
				job1.JH_OA_AgentCollectAddr = TestObjectCreator.Agent.MainAddress.PK;
				CreateExchangeRate(job1, TestObjectCreator.USD, .75M);
				var charge1 = CreateCharge(job1, TestObjectCreator.CC3, "Job 1 Charge Code 1", TestObjectCreator.USD, 200M, TestObjectCreator.Creditor2, TestObjectCreator.USD, 275M, TestObjectCreator.Agent);
				charge1.JR_InvoiceType = "CUR";
				var charge2 = CreateCharge(job1, TestObjectCreator.CC1, "Job 1 Charge Code 2", TestObjectCreator.AUD, 100M, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 150M, TestObjectCreator.Agent);
				charge2.JR_InvoiceType = "FIN";

				Job job2 = CreateJob("S0002", TestObjectCreator.LocalClient, 10M, TestObjectCreator.Agent, 20M);
				job2.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				job2.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
				job2.JH_OA_AgentCollectAddr = TestObjectCreator.Agent.MainAddress.PK;
				CreateExchangeRate(job2, TestObjectCreator.EUR, 1.5M);
				var charge3 = CreateCharge(job2, TestObjectCreator.CC4, "Job 2 Charge Code 1", TestObjectCreator.EUR, 120M, TestObjectCreator.Creditor1, TestObjectCreator.EUR, 160M, TestObjectCreator.Agent);
				charge3.JR_InvoiceType = "CUR";
				var charge4 = CreateCharge(job2, TestObjectCreator.CC5, "Job 2 Charge Code 2", TestObjectCreator.AUD, 210M, TestObjectCreator.Creditor2, TestObjectCreator.AUD, 285M, TestObjectCreator.Agent);
				charge4.JR_InvoiceType = "FIN";

				string consolNumber = "C00001000";
				ForwardingConsol consol = CreateConsol("BEANR", "CAMTR", consolNumber);
				consol.JK_RL_NKLoadPort = "BEANR";
				consol.JK_RL_NKDischargePort = "CAMTR";
				consol.JK_OA_ReceivingForwarderAddress = TestObjectCreator.Agent.MainAddress.PK;
				consol.JK_OA_SendingForwarderAddress = TestObjectCreator.LocalClient.MainAddress.PK;
				ApportionmentListing apps = new ApportionmentListing(Factory, consol);

				Factory.Save();

				var jobs = new[] { job1, job2 };

				ConsolInvoicingPostManager creator = new ConsolInvoicingPostManager(Factory, jobs, consol, apps);
				creator.ExportAgentPosting += new ExportAgentPostingEventHandler(Creator_ExportAgentPosting);

				TransactionCreatorHashtable transactions = creator.CreateTransactions(JobInvoicingPostingOption.Agent);
				Factory.Save();

				AssertEquals("Payables Trx Count", 0, transactions.APTransactionsCount);
				AssertEquals("Receivable Transactions Count", 1, transactions.ARTransactionsCount);

				AssertEquals("Invoice Type: Charge 1_1", "CUR", charge1.JR_InvoiceType);
				AssertEquals("Invoice Type: Charge 1_2", "CUR", charge2.JR_InvoiceType);
				AssertEquals("Invoice Type: Charge 2_1", "CUR", charge3.JR_InvoiceType);
				AssertEquals("Invoice Type: Charge 2_2", "CUR", charge4.JR_InvoiceType);

				AssertEquals("Sell Currency: Charge 1_1", "USD", charge1.JR_RX_NKSellCurrency);
				AssertEquals("Sell Currency: Charge 1_2", "USD", charge2.JR_RX_NKSellCurrency);
				AssertEquals("Sell Currency: Charge 2_1", "USD", charge3.JR_RX_NKSellCurrency);
				AssertEquals("Sell Currency: Charge 2_2", "USD", charge4.JR_RX_NKSellCurrency);
			}
		}

		#endregion

		#region TEST: Create Invoices For Entire Consol With Revenue Bill In Local Currency

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateInvoicesForEntireConsolWithRevenueBillInLocalCurrency()
		{
			string consolNumber = "C00001000";

			ForwardingConsol consol = CreateConsol("AUSYD", "USLAX", consolNumber);
			ZGuid consolID = consol.PK;
			Job job1 = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			job1.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			ExchangeRate rate1_1 = CreateExchangeRate(job1, USD, .7M);
			ExchangeRate rate1_2 = CreateExchangeRate(job1, GBP, .4M);

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = CC1.PK;
			cost.E6_OH_Creditor = Creditor3.PK;
			cost.E6_InvoiceNum = "3";
			cost.E6_InvoiceDate = Now.AddDays(10);
			cost.E6_PaymentDate = Now.AddDays(25);
			ZGuid costSplitGroup1 = cost.PK;
			Factory.Save();

			Charge charge1_1 = CreateCharge(job1, CC1, "Job 1 Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge1_2 = CreateCharge(job1, CC2, "Job 1 Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			Charge charge1_3 = CreateCharge(job1, CC3, "Job 1 Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);
			Charge charge1_4 = CreateCharge(job1, CC4, "Job 1 Charge Code 4", null, 0M, null, USD, 500M, Agent);
			charge1_4.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			Charge charge1_5 = CreateCharge(job1, CC5, "Job 1 Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, Agent);
			charge1_5.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			Charge charge1_6 = CreateCharge(job1, CC6, "Job 1 Charge Code 6", USD, 200M, Creditor2, USD, 275M, Agent);
			charge1_6.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			Charge charge1_7 = CreateCharge(job1, CC7, "Job 1 Charge Code 7", AUD, 200M, Creditor1, AUD, 300M, LocalClient);
			Charge charge1_8 = CreateCharge(job1, CC8, "Job 1 Charge Code 8", AUD, 300M, Creditor3, AUD, 300M, LocalClient);
			charge1_8.JR_E6 = costSplitGroup1;

			SetAPInvoiceInfo(charge1_1, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_2, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_3, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_5, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_6, "2", Now.AddDays(20), Now.AddDays(30));
			SetAPInvoiceInfo(charge1_7, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_8, "3", Now.AddDays(10), Now.AddDays(25));

			Job job2 = CreateJob("Z00001001", LocalClient, 5M, Agent, 10M);
			job2.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			ExchangeRate rate2_1 = CreateExchangeRate(job2, USD, .7M);
			ExchangeRate rate2_2 = CreateExchangeRate(job2, GBP, .4M);

			Charge charge2_1 = CreateCharge(job2, CC1, "Job 2 Charge Code 1", AUD, 150M, Creditor1, AUD, 200M, LocalClient);
			Charge charge2_2 = CreateCharge(job2, CC2, "Job 2 Charge Code 2", USD, 250M, Creditor2, USD, 250M, Agent);
			charge2_2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			Charge charge2_3 = CreateCharge(job2, CC3, "Job 2 Charge Code 3", AUD, 350M, Creditor3, AUD, 400M, LocalClient);
			Charge charge2_4 = CreateCharge(job2, CC8, "Job 2 Charge Code 8", AUD, 335M, Creditor3, AUD, 500M, Agent);
			charge2_4.JR_E6 = costSplitGroup1;

			SetAPInvoiceInfo(charge2_1, "3", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge2_2, "3", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge2_3, "2", Now.AddDays(15), Now.AddDays(25));
			SetAPInvoiceInfo(charge2_4, "3", Now.AddDays(10), Now.AddDays(25));

			Factory.Save();

			#region WIPs and Accruals

			AccountingConfigurationRegistry.Instance.AllowForwardDatingofAPInvoiceDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			AccTransactionLines charge1_1WIP = charge1_1.WIP;
			AccTransactionLines charge1_2WIP = charge1_2.WIP;
			AccTransactionLines charge1_3WIP = charge1_3.WIP;
			AccTransactionLines charge1_4WIP = charge1_4.WIP;
			AccTransactionLines charge1_5WIP = charge1_5.WIP;
			AccTransactionLines charge1_6WIP = charge1_6.WIP;
			AccTransactionLines charge1_7WIP = charge1_7.WIP;
			AccTransactionLines charge1_8WIP = charge1_8.WIP;
			AccTransactionLines charge2_1WIP = charge2_1.WIP;
			AccTransactionLines charge2_2WIP = charge2_2.WIP;
			AccTransactionLines charge2_3WIP = charge2_3.WIP;
			AccTransactionLines charge2_4WIP = charge2_4.WIP;

			AccTransactionLines charge1_1Accrual = charge1_1.Accrual;
			AccTransactionLines charge1_2Accrual = charge1_2.Accrual;
			AccTransactionLines charge1_3Accrual = charge1_3.Accrual;
			AccTransactionLines charge1_5Accrual = charge1_5.Accrual;
			AccTransactionLines charge1_6Accrual = charge1_6.Accrual;
			AccTransactionLines charge1_7Accrual = charge1_7.Accrual;
			AccTransactionLines charge1_8Accrual = charge1_8.Accrual;
			AccTransactionLines charge2_1Accrual = charge2_1.Accrual;
			AccTransactionLines charge2_2Accrual = charge2_2.Accrual;
			AccTransactionLines charge2_3Accrual = charge2_3.Accrual;
			AccTransactionLines charge2_4Accrual = charge2_4.Accrual;

			#endregion

			var jobs = new[] { job1, job2 };
			ConsolInvoicingPostManager creator = new ConsolInvoicingPostManager(Factory, jobs, consol, apps);

			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			TransactionCreatorHashtable transactions = creator.CreateTransactions(JobInvoicingPostingOption.All);
			AssertEquals("Payables Trx Count", 9, transactions.APTransactionsCount);
			AssertEquals("Receivable Transactions Count", 3, transactions.ARTransactionsCount);

			#region Agent AUD Invoice

			InvoicingBase[] agentInvoices = creator.Poster.GetInvoices(AUD, Agent);
			AssertEquals(1, agentInvoices.Length);
			ARInvoice agentInvoice = (ARInvoice)agentInvoices[0];
			AssertEquals("Invoice Lines", 6, agentInvoice.Lines.Count);

			AssertTransactionHeaderValues(agentInvoice, "AR", "INV", null, "FREIGHT CONSOL INVOICE", Now, Now,
					2824.21M, 168.05M, 59.17M, 2992.26M, AUD, 1, Now, ZBool.False, Agent, null,
					"COD", 0, ZString.Empty, String.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(agentInvoice, true);

			TransactionLine cC3Line1 = agentInvoice.FindTransactionLine("REV", CC3, job1.PK);
			AssertTransactionLineValues(cC3Line1, "REV", 3, "Job 1 Charge Code 3", 350M, GSTFREE1, 0M, WHT1, 17.50M, 350M, AUD, 1, Now,
					ZBool.False, agentInvoice, job1, CC3, CC3.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC3Line1);

			TransactionLine cC4Line1 = agentInvoice.FindTransactionLine("REV", CC4, job1.PK);
			AssertTransactionLineValues(cC4Line1, "REV", 4, "Job 1 Charge Code 4", 793.65M, GSTFREE1, 0M, WHTFREE1, 0M, 793.65M, AUD, 1, Now,
					ZBool.False, agentInvoice, job1, CC4, CC4.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC4Line1);

			TransactionLine cC5Line1 = agentInvoice.FindTransactionLine("REV", CC5, job1.PK);
			AssertTransactionLineValues(cC5Line1, "REV", 5, "Job 1 Charge Code 5", 347.22M, GST1, 34.72M, WHTFREE1, 0M, 381.94M, AUD, 1, Now,
					ZBool.False, agentInvoice, job1, CC5, CC5.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC5Line1);

			TransactionLine cC6Line1 = agentInvoice.FindTransactionLine("REV", CC6, job1.PK);
			AssertTransactionLineValues(cC6Line1, "REV", 6, "Job 1 Charge Code 6", 436.51M, GST1, 43.65M, WHT1, 21.83M, 480.16M, AUD, 1, Now,
					ZBool.False, agentInvoice, job1, CC6, CC6.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC6Line1);

			TransactionLine cC2Line2 = agentInvoice.FindTransactionLine("REV", CC2, job2.PK);
			AssertTransactionLineValues(cC2Line2, "REV", 2, "Job 2 Charge Code 2", 396.83M, GST1, 39.68M, WHT1, 19.84M, 436.51M, AUD, 1, Now,
					ZBool.False, agentInvoice, job2, CC2, CC2.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC2Line2);

			TransactionLine cC8Line2 = agentInvoice.FindTransactionLine("REV", CC8, job2.PK);
			AssertTransactionLineValues(cC8Line2, "REV", 4, "Job 2 Charge Code 8", 500M, GST1, 50M, WHTFREE1, 0M, 550M, AUD, 1, Now,
					ZBool.False, agentInvoice, job2, CC8, CC8.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC8Line2);

			#endregion

			#region Job1 Local Client AUD Invoice

			InvoicingBase[] job1LocalClientInvoices = creator.Poster.GetInvoices(AUD, LocalClient, job1.JH_JobNum);
			AssertEquals(1, job1LocalClientInvoices.Length);
			ARInvoice job1LocalClientInvoice = (ARInvoice)job1LocalClientInvoices[0];
			AssertEquals("Invoice Lines", 4, job1LocalClientInvoice.Lines.Count);

			AssertTransactionHeaderValues(job1LocalClientInvoice, "AR", "INV", null, "Z00001000", Now, Now,
					950M, 65M, 10M, 1015M, AUD, 1, Now, ZBool.False, LocalClient, job1,
					"COD", 0, ZString.Empty, String.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(job1LocalClientInvoice);

			TransactionLine cC1Line1 = job1LocalClientInvoice.FindTransactionLine("REV", CC1, job1.PK);
			AssertTransactionLineValues(cC1Line1, "REV", 1, "Job 1 Charge Code 1", 150M, GST1, 15M, WHTFREE1, 0M, 165M, AUD, 1, Now,
					ZBool.False, job1LocalClientInvoice, job1, CC1, CC1.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC1Line1);

			TransactionLine cC2Line1 = job1LocalClientInvoice.FindTransactionLine("REV", CC2, job1.PK);
			AssertTransactionLineValues(cC2Line1, "REV", 2, "Job 1 Charge Code 2", 200M, GST1, 20M, WHT1, 10M, 220M, AUD, 1, Now,
					ZBool.False, job1LocalClientInvoice, job1, CC2, CC2.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC2Line1);

			TransactionLine cC7Line1 = job1LocalClientInvoice.FindTransactionLine("REV", CC7, job1.PK);
			AssertTransactionLineValues(cC7Line1, "REV", 7, "Job 1 Charge Code 7", 300M, GSTFREE1, 0M, WHTFREE1, 0M, 300M, AUD, 1, Now,
					ZBool.False, job1LocalClientInvoice, job1, CC7, CC7.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC7Line1);

			TransactionLine cC8Line1 = job1LocalClientInvoice.FindTransactionLine("REV", CC8, job1.PK);
			AssertTransactionLineValues(cC8Line1, "REV", 8, "Job 1 Charge Code 8", 300M, GST1, 30M, WHTFREE1, 0M, 330M, AUD, 1, Now,
					ZBool.False, job1LocalClientInvoice, job1, CC8, CC8.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC8Line1);

			#endregion

			#region Job2 Local Client AUD Invoice

			InvoicingBase[] job2LocalClientInvoices = creator.Poster.GetInvoices(AUD, LocalClient, job2.JH_JobNum);
			AssertEquals(1, job2LocalClientInvoices.Length);
			ARInvoice job2LocalClientInvoice = (ARInvoice)job2LocalClientInvoices[0];
			AssertEquals("Invoice Lines", 2, job2LocalClientInvoice.Lines.Count);

			AssertTransactionHeaderValues(job2LocalClientInvoice, "AR", "INV", null, "Z00001001", Now, Now,
					600M, 20M, 20M, 620M, AUD, 1, Now, ZBool.False, LocalClient, job2,
					"COD", 0, ZString.Empty, String.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(job2LocalClientInvoice);

			TransactionLine cC1Line2 = job2LocalClientInvoice.FindTransactionLine("REV", CC1, job2.PK);
			AssertTransactionLineValues(cC1Line2, "REV", 1, "Job 2 Charge Code 1", 200M, GST1, 20M, WHTFREE1, 0M, 220M, AUD, 1, Now,
					ZBool.False, job2LocalClientInvoice, job2, CC1, CC1.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC1Line2);

			TransactionLine cC3Line2 = job2LocalClientInvoice.FindTransactionLine("REV", CC3, job2.PK);
			AssertTransactionLineValues(cC3Line2, "REV", 3, "Job 2 Charge Code 3", 400M, GSTFREE1, 0M, WHT1, 20M, 400M, AUD, 1, Now,
					ZBool.False, job2LocalClientInvoice, job2, CC3, CC3.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC3Line2);

			#endregion

			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, consolNumber);
			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, "Z00001000");
			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, "Z00001001");

			#region Creditor 1 Invoice 1

			APInvoice creditor1Inv1 = transactions.RetrieveAPInvoice(Creditor1, "1");
			AssertEquals("Invoice Line Count", 2, creditor1Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor1Inv1, "AP", "INV", "1", "Z00001000", Now.AddDays(10), Now.AddDays(20),
					-300M, -10M, 0M, -310M, AUD, 1, Now, ZBool.False, Creditor1, job1,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor1Inv1);

			cC1Line1 = creditor1Inv1.FindTransactionLine("CST", CC1, job1.PK);
			AssertTransactionLineValues(cC1Line1, "CST", 1, "Job 1 Charge Code 1", -100M, GST1, -10M, WHTFREE1, 0M, -110M, AUD, 1, Now, Now,
					ZBool.False, creditor1Inv1, job1, CC1, CC1.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC1Line1);

			TransactionLine cC7Line = creditor1Inv1.FindTransactionLine("CST", CC7, job1.PK);
			AssertTransactionLineValues(cC7Line, "CST", 2, "Job 1 Charge Code 7", -200M, GSTFREE1, 0M, WHTFREE1, 0M, -200M, AUD, 1, Now, Now,
					ZBool.False, creditor1Inv1, job1, CC7, CC7.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC7Line);

			#endregion

			#region Creditor 1 Invoice 2

			APInvoice creditor1Inv2 = transactions.RetrieveAPInvoice(Creditor1, "2");
			AssertEquals("Invoice Line Count", 1, creditor1Inv2.Lines.Count);

			AssertTransactionHeaderValues(creditor1Inv2, "AP", "INV", "2", "Z00001000", Now.AddDays(10), Now.AddDays(20),
					-250M, -25M, 0M, -110M, GBP, .4M, Now, ZBool.False, Creditor1, job1,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertOverseaAPTransactionHeaderDefaults_ForBasePostManager(creditor1Inv2);

			TransactionLine cC5Line = creditor1Inv2.FindTransactionLine("CST", CC5, job1.PK);
			AssertTransactionLineValues(cC5Line, "CST", 1, "Job 1 Charge Code 5", -250M, GST1, -25M, WHTFREE1, 0M, -110M, GBP, .4M, Now, Now,
					ZBool.False, creditor1Inv2, job1, CC5, CC5.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC5Line);

			#endregion

			#region Creditor 1 Invoice 3

			APInvoice creditor1Inv3 = transactions.RetrieveAPInvoice(Creditor1, "3");
			AssertEquals("Invoice Line Count", 1, creditor1Inv3.Lines.Count);

			AssertTransactionHeaderValues(creditor1Inv3, "AP", "INV", "3", "Z00001001", Now.AddDays(10), Now.AddDays(20),
					-150M, -15M, 0M, -165M, AUD, 1, Now, ZBool.False, Creditor1, job2,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor1Inv3);

			cC1Line2 = creditor1Inv3.FindTransactionLine("CST", CC1, job2.PK);
			AssertTransactionLineValues(cC1Line2, "CST", 1, "Job 2 Charge Code 1", -150M, GST1, -15M, WHTFREE1, 0M, -165M, AUD, 1, Now, Now,
					ZBool.False, creditor1Inv3, job2, CC1, CC1.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC1Line2);

			#endregion

			#region Creditor 2 Invoice 1

			APInvoice creditor2Inv1 = transactions.RetrieveAPInvoice(Creditor2, "1");
			AssertEquals("Invoice Line Count", 1, creditor2Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor2Inv1, "AP", "INV", "1", "Z00001000", Now.AddDays(10), Now.AddDays(20),
					-200M, -20M, -10M, -220M, AUD, 1, Now, ZBool.False, Creditor2, job1,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor2Inv1);

			cC2Line1 = creditor2Inv1.FindTransactionLine("CST", CC2, job1.PK);
			AssertTransactionLineValues(cC2Line1, "CST", 1, "Job 1 Charge Code 2", -200M, GST1, -20M, WHT1, -10M, -220M, AUD, 1, Now, Now,
					ZBool.False, creditor2Inv1, job1, CC2, CC2.CostAccount, Creditor2);
			AssertTransactionLineDefaults(cC2Line1);

			#endregion

			#region Creditor 2 Invoice 2

			APInvoice creditor2Inv2 = transactions.RetrieveAPInvoice(Creditor2, "2");
			AssertEquals("Invoice Line Count", 1, creditor2Inv2.Lines.Count);

			AssertTransactionHeaderValues(creditor2Inv2, "AP", "INV", "2", "Z00001000", Now.AddDays(20), Now.AddDays(30),
					-285.71M, -28.57M, -14.29M, -220M, USD, 0.700013M, Now, ZBool.False, Creditor2, job1,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertOverseaAPTransactionHeaderDefaults_ForBasePostManager(creditor2Inv2);

			TransactionLine cC6Line = creditor2Inv2.FindTransactionLine("CST", CC6, job1.PK);
			AssertTransactionLineValues(cC6Line, "CST", 1, "Job 1 Charge Code 6", -285.71M, GST1, -28.57M, WHT1, -14.29M, -220M, USD, .7M, Now, Now,
					ZBool.False, creditor2Inv2, job1, CC6, CC6.CostAccount, Creditor2);
			AssertTransactionLineDefaults(cC6Line);

			#endregion

			#region Creditor 2 Invoice 3

			APInvoice creditor2Inv3 = transactions.RetrieveAPInvoice(Creditor2, "3");
			AssertEquals("Invoice Line Count", 1, creditor2Inv3.Lines.Count);

			AssertTransactionHeaderValues(creditor2Inv3, "AP", "INV", "3", "Z00001001", Now.AddDays(10), Now.AddDays(20),
					-357.14M, -35.71M, -17.86M, -275M, USD, 0.700013M, Now, ZBool.False, Creditor2, job2,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertOverseaAPTransactionHeaderDefaults_ForBasePostManager(creditor2Inv3);

			cC2Line2 = creditor2Inv3.FindTransactionLine("CST", CC2, job2.PK);
			AssertTransactionLineValues(cC2Line2, "CST", 1, "Job 2 Charge Code 2", -357.14M, GST1, -35.71M, WHT1, -17.86M, -275M, USD, .7M, Now, Now,
					ZBool.False, creditor2Inv3, job2, CC2, CC2.CostAccount, Creditor2);
			AssertTransactionLineDefaults(cC2Line2);

			#endregion

			#region Creditor 3 Invoice 1

			APInvoice creditor3Inv1 = transactions.RetrieveAPInvoice(Creditor3, "1");
			AssertEquals("Invoice Line Count", 1, creditor3Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor3Inv1, "AP", "INV", "1", "Z00001000", Now.AddDays(10), Now.AddDays(20),
					-300M, 0M, -15M, -300M, AUD, 1, Now, ZBool.False, Creditor3, job1,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor3Inv1);

			TransactionLine cC3Line = creditor3Inv1.FindTransactionLine("CST", CC3, job1.PK);
			AssertTransactionLineValues(cC3Line, "CST", 1, "Job 1 Charge Code 3", -300M, GSTFREE1, 0M, WHT1, -15M, -300M, AUD, 1, Now, Now,
					ZBool.False, creditor3Inv1, job1, CC3, CC3.CostAccount, Creditor3);
			AssertTransactionLineDefaults(cC3Line);

			#endregion

			#region Creditor 3 Invoice 2

			APInvoice creditor3Inv2 = transactions.RetrieveAPInvoice(Creditor3, "2");
			AssertEquals("Invoice Line Count", 1, creditor3Inv2.Lines.Count);

			AssertTransactionHeaderValues(creditor3Inv2, "AP", "INV", "2", "Z00001001", Now.AddDays(15), Now.AddDays(25),
					-350, 0M, -17.50M, -350M, AUD, 1, Now, ZBool.False, Creditor3, job2,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor3Inv2);

			cC3Line2 = creditor3Inv2.FindTransactionLine("CST", CC3, job2.PK);
			AssertTransactionLineValues(cC3Line2, "CST", 1, "Job 2 Charge Code 3", -350M, GSTFREE1, 0M, WHT1, -17.50M, -350M, AUD, 1, Now, Now,
					ZBool.False, creditor3Inv2, job2, CC3, CC3.CostAccount, Creditor3);
			AssertTransactionLineDefaults(cC3Line2);

			#endregion

			#region Creditor 3 Invoice 3

			APInvoice creditor3Inv3 = transactions.RetrieveAPInvoice(Creditor3, "3");
			AssertEquals("Invoice Line Count", 2, creditor3Inv3.Lines.Count);

			AssertTransactionHeaderValues(creditor3Inv3, "AP", "INV", "3", "Multiple Jobs", Now.AddDays(10), Now.AddDays(25),
					-635M, -63.50M, 0M, -698.50M, AUD, 1, Now, ZBool.False, Creditor3, null,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor3Inv3);

			cC8Line1 = creditor3Inv3.FindTransactionLine("CST", CC8, job1.PK);
			AssertTransactionLineValues(cC8Line1, "CST", 1, "Job 1 Charge Code 8", -300M, GST1, -30M, WHTFREE1, 0M, -330M, AUD, 1, Now, Now,
					ZBool.False, creditor3Inv3, job1, CC8, CC8.CostAccount, Creditor3);
			AssertTransactionLineDefaults(cC8Line1);

			cC8Line2 = creditor3Inv3.FindTransactionLine("CST", CC8, job2.PK);
			AssertTransactionLineValues(cC8Line2, "CST", 2, "Job 2 Charge Code 8", -335M, GST1, -33.50M, WHTFREE1, 0M, -368.50M, AUD, 1, Now, Now,
					ZBool.False, creditor3Inv3, job2, CC8, CC8.CostAccount, Creditor3);
			AssertTransactionLineDefaults(cC8Line2);

			#endregion

			#region WIP and Accrual Reversal Assertions

			AssertEquals("Charge 1_1 WIP Reversed", true, charge1_1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_2 WIP Reversed", true, charge1_2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_3 WIP Reversed", true, charge1_3WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_4 WIP Reversed", true, charge1_4WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_5 WIP Reversed", true, charge1_5WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_6 WIP Reversed", true, charge1_6WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_7 WIP Reversed", true, charge1_7WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_1 WIP Reversed", true, charge2_1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_2 WIP Reversed", true, charge2_2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_3 WIP Reversed", true, charge2_3WIP.AL_ReverseDate.IsValid);

			AssertEquals("Charge 1_1 Accrual Reversed", true, charge1_1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_2 Accrual Reversed", true, charge1_2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_3 Accrual Reversed", true, charge1_3Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_5 Accrual Reversed", true, charge1_5Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_6 Accrual Reversed", true, charge1_6Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_7 Accrual Reversed", true, charge1_7Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_1 Accrual Reversed", true, charge2_1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_2 Accrual Reversed", true, charge2_2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_3 Accrual Reversed", true, charge2_3Accrual.AL_ReverseDate.IsValid);

			#endregion
		}

		#endregion

		#region TEST: Create Invoices For Entire Consol With Revenue Bill In Foreign Currency

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateInvoicesForEntireConsolWithRevenueBillInForeignCurrency()
		{
			#region Setup

			AccountingConfigurationRegistry.Instance.AllowForwardDatingofAPInvoiceDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			string consolNumber = "C00001000";
			ForwardingConsol consol = CreateConsol("AUSYD", "USLAX", consolNumber);
			ZGuid consolID = consol.PK;
			Job job1 = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			job1.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			ExchangeRate rate1_1 = CreateExchangeRate(job1, USD, .7M);
			ExchangeRate rate1_2 = CreateExchangeRate(job1, GBP, .4M);

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = CC1.PK;
			cost.E6_OH_Creditor = Creditor3.PK;
			cost.E6_InvoiceNum = "3";
			cost.E6_InvoiceDate = Now.AddDays(10);
			cost.E6_PaymentDate = Now.AddDays(25);
			ZGuid costSplitGroup1 = cost.PK;
			Factory.Save();

			Charge charge1_1 = CreateCharge(job1, CC1, "Job 1 Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge1_2 = CreateCharge(job1, CC2, "Job 1 Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			Charge charge1_3 = CreateCharge(job1, CC3, "Job 1 Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);
			Charge charge1_4 = CreateCharge(job1, CC4, "Job 1 Charge Code 4", null, 0M, null, USD, 500M, Agent);
			charge1_4.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			Charge charge1_5 = CreateCharge(job1, CC5, "Job 1 Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, Agent);
			charge1_5.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			Charge charge1_6 = CreateCharge(job1, CC6, "Job 1 Charge Code 6", USD, 200M, Creditor2, USD, 275M, Agent);
			charge1_6.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			Charge charge1_7 = CreateCharge(job1, CC7, "Job 1 Charge Code 7", AUD, 200M, Creditor1, AUD, 300M, LocalClient);
			Charge charge1_8 = CreateCharge(job1, CC8, "Job 1 Charge Code 8", AUD, 300M, Creditor3, AUD, 300M, LocalClient);
			charge1_8.JR_E6 = costSplitGroup1;

			SetAPInvoiceInfo(charge1_1, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_2, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_3, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_5, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_6, "2", Now.AddDays(20), Now.AddDays(30));
			SetAPInvoiceInfo(charge1_7, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_8, "3", Now.AddDays(10), Now.AddDays(25));

			Job job2 = CreateJob("Z00001001", LocalClient, 5M, Agent, 10M);
			job2.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			ExchangeRate rate2_1 = CreateExchangeRate(job2, USD, .7M);
			ExchangeRate rate2_2 = CreateExchangeRate(job2, GBP, .4M);

			Charge charge2_1 = CreateCharge(job2, CC1, "Job 2 Charge Code 1", AUD, 150M, Creditor1, AUD, 200M, LocalClient);
			Charge charge2_2 = CreateCharge(job2, CC2, "Job 2 Charge Code 2", USD, 250M, Creditor2, USD, 250M, Agent);
			charge2_2.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			Charge charge2_3 = CreateCharge(job2, CC3, "Job 2 Charge Code 3", AUD, 350M, Creditor3, AUD, 400M, LocalClient);
			Charge charge2_4 = CreateCharge(job2, CC8, "Job 2 Charge Code 8", AUD, 335M, Creditor3, AUD, 500M, Agent);
			charge2_4.JR_E6 = costSplitGroup1;

			SetAPInvoiceInfo(charge2_1, "3", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge2_2, "3", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge2_3, "2", Now.AddDays(15), Now.AddDays(25));
			SetAPInvoiceInfo(charge2_4, "3", Now.AddDays(10), Now.AddDays(25));

			Factory.Save();

			#region WIPs and Accruals

			AccTransactionLines charge1_1WIP = charge1_1.WIP;
			AccTransactionLines charge1_2WIP = charge1_2.WIP;
			AccTransactionLines charge1_3WIP = charge1_3.WIP;
			AccTransactionLines charge1_4WIP = charge1_4.WIP;
			AccTransactionLines charge1_5WIP = charge1_5.WIP;
			AccTransactionLines charge1_6WIP = charge1_6.WIP;
			AccTransactionLines charge1_7WIP = charge1_7.WIP;
			AccTransactionLines charge1_8WIP = charge1_8.WIP;
			AccTransactionLines charge2_1WIP = charge2_1.WIP;
			AccTransactionLines charge2_2WIP = charge2_2.WIP;
			AccTransactionLines charge2_3WIP = charge2_3.WIP;
			AccTransactionLines charge2_4WIP = charge2_4.WIP;

			AccTransactionLines charge1_1Accrual = charge1_1.Accrual;
			AccTransactionLines charge1_2Accrual = charge1_2.Accrual;
			AccTransactionLines charge1_3Accrual = charge1_3.Accrual;
			AccTransactionLines charge1_5Accrual = charge1_5.Accrual;
			AccTransactionLines charge1_6Accrual = charge1_6.Accrual;
			AccTransactionLines charge1_7Accrual = charge1_7.Accrual;
			AccTransactionLines charge1_8Accrual = charge1_8.Accrual;
			AccTransactionLines charge2_1Accrual = charge2_1.Accrual;
			AccTransactionLines charge2_2Accrual = charge2_2.Accrual;
			AccTransactionLines charge2_3Accrual = charge2_3.Accrual;
			AccTransactionLines charge2_4Accrual = charge2_4.Accrual;

			#endregion

			var jobs = new[] { job1, job2 };

			#endregion

			ConsolInvoicingPostManager creator = new ConsolInvoicingPostManager(Factory, jobs, consol, apps);
			TransactionCreatorHashtable transactions = creator.CreateTransactions(JobInvoicingPostingOption.All);
			AssertEquals("Payables Transaction Count", 9, transactions.APTransactionsCount);
			AssertEquals("Receivable Transactions Count", 5, transactions.ARTransactionsCount);

			#region Receivables

			#region Agent AUD Invoice

			InvoicingBase[] agentAUDInvoices = creator.Poster.GetInvoices(AUD, Agent);
			AssertEquals(1, agentAUDInvoices.Length);
			ARInvoice agentAUDInvoice = (ARInvoice)agentAUDInvoices[0];
			AssertEquals("Invoice Lines", 2, agentAUDInvoice.Lines.Count);

			AssertTransactionHeaderValues(agentAUDInvoice, "AR", "INV", null, "FREIGHT CONSOL INVOICE", Now, Now,
					850M, 50M, 17.50M, 900M, AUD, 1, Now, ZBool.False, Agent, null,
					"COD", 0, ZString.Empty, String.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(agentAUDInvoice, true);

			TransactionLine cC3Line1 = agentAUDInvoice.FindTransactionLine("REV", CC3, job1.PK);
			AssertTransactionLineValues(cC3Line1, "REV", 3, "Job 1 Charge Code 3", 350M, GSTFREE1, 0M, WHT1, 17.50M, 350M, AUD, 1, Now, ZDateTime.Empty,
					ZBool.False, agentAUDInvoice, job1, CC3, CC3.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC3Line1);

			TransactionLine cC8Line2 = agentAUDInvoice.FindTransactionLine("REV", CC8, job2.PK);
			AssertTransactionLineValues(cC8Line2, "REV", 4, "Job 2 Charge Code 8", 500M, GST1, 50M, WHTFREE1, 0M, 550M, AUD, 1, Now, ZDateTime.Empty,
					ZBool.False, agentAUDInvoice, job2, CC8, CC8.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC8Line2);

			#endregion

			#region Agent USD Invoice

			InvoicingBase[] agentUSDInvoices = creator.Poster.GetInvoices(USD, Agent);
			AssertEquals(1, agentUSDInvoices.Length);
			ARInvoice agentUSDInvoice = (ARInvoice)agentUSDInvoices[0];
			AssertEquals("Invoice Lines", 3, agentUSDInvoice.Lines.Count);

			AssertTransactionHeaderValues(agentUSDInvoice, "AR", "INV", null, "FREIGHT CONSOL INVOICE", Now, Now,
					1464.29M, 75.00M, 37.50M, 1077.50M, USD, .7M, Now, ZBool.False, Agent, null,
					"COD", 0, ZString.Empty, String.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(agentUSDInvoice, true);

			TransactionLine cC4Line1 = agentUSDInvoice.FindTransactionLine("REV", CC4, job1.PK);
			AssertTransactionLineValues(cC4Line1, "REV", 4, "Job 1 Charge Code 4", 714.29M, GSTFREE1, 0M, WHTFREE1, 0M, 500M, USD, .7M, Now, ZDateTime.Empty,
					ZBool.False, agentUSDInvoice, job1, CC4, CC4.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC4Line1);

			TransactionLine cC6Line1 = agentUSDInvoice.FindTransactionLine("REV", CC6, job1.PK);
			AssertTransactionLineValues(cC6Line1, "REV", 6, "Job 1 Charge Code 6", 392.86M, GST1, 39.29M, WHT1, 19.64M, 302.50M, USD, .7M, Now, ZDateTime.Empty,
					ZBool.False, agentUSDInvoice, job1, CC6, CC6.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC6Line1);

			TransactionLine cC2Line2 = agentUSDInvoice.FindTransactionLine("REV", CC2, job2.PK);
			AssertTransactionLineValues(cC2Line2, "REV", 2, "Job 2 Charge Code 2", 357.14M, GST1, 35.71M, WHT1, 17.86M, 275M, USD, .7M, Now, ZDateTime.Empty,
					ZBool.False, agentUSDInvoice, job2, CC2, CC2.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC2Line2);

			#endregion

			#region Agent GBP Invoice

			InvoicingBase[] agentGBPInvoices = creator.Poster.GetInvoices(GBP, Agent);
			AssertEquals(1, agentGBPInvoices.Length);
			ARInvoice agentGBPInvoice = (ARInvoice)agentGBPInvoices[0];
			AssertEquals("Invoice Lines", 1, agentGBPInvoice.Lines.Count);

			AssertTransactionHeaderValues(agentGBPInvoice, "AR", "INV", null, "FREIGHT CONSOL INVOICE", Now, Now,
					312.50M, 31.25M, 0, 137.50M, GBP, .4M, Now, ZBool.False, Agent, null,
					"COD", 0, ZString.Empty, String.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(agentGBPInvoice, true);

			TransactionLine cC5Line1 = agentGBPInvoice.FindTransactionLine("REV", CC5, job1.PK);
			AssertTransactionLineValues(cC5Line1, "REV", 5, "Job 1 Charge Code 5", 312.50M, GST1, 31.25M, WHTFREE1, 0M, 137.50M, GBP, .4M, Now, ZDateTime.Empty,
					ZBool.False, agentGBPInvoice, job1, CC5, CC5.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC5Line1);

			#endregion

			#region Job1 Local Client AUD Invoice

			InvoicingBase[] job1LocalClientInvoices = creator.Poster.GetInvoices(AUD, LocalClient, job1.JH_JobNum);
			AssertEquals(1, job1LocalClientInvoices.Length);
			ARInvoice job1LocalClientInvoice = (ARInvoice)job1LocalClientInvoices[0];
			AssertEquals("Invoice Lines", 4, job1LocalClientInvoice.Lines.Count);

			AssertTransactionHeaderValues(job1LocalClientInvoice, "AR", "INV", null, "Z00001000", Now, Now,
					950M, 65M, 10M, 1015M, AUD, 1, Now, ZBool.False, LocalClient, job1,
					"COD", 0, ZString.Empty, String.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(job1LocalClientInvoice);

			TransactionLine cC1Line1 = job1LocalClientInvoice.FindTransactionLine("REV", CC1, job1.PK);
			AssertTransactionLineValues(cC1Line1, "REV", 1, "Job 1 Charge Code 1", 150M, GST1, 15M, WHTFREE1, 0M, 165M, AUD, 1, Now, ZDateTime.Empty,
					ZBool.False, job1LocalClientInvoice, job1, CC1, CC1.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC1Line1);

			TransactionLine cC2Line1 = job1LocalClientInvoice.FindTransactionLine("REV", CC2, job1.PK);
			AssertTransactionLineValues(cC2Line1, "REV", 2, "Job 1 Charge Code 2", 200M, GST1, 20M, WHT1, 10M, 220M, AUD, 1, Now, ZDateTime.Empty,
					ZBool.False, job1LocalClientInvoice, job1, CC2, CC2.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC2Line1);

			TransactionLine cC7Line1 = job1LocalClientInvoice.FindTransactionLine("REV", CC7, job1.PK);
			AssertTransactionLineValues(cC7Line1, "REV", 7, "Job 1 Charge Code 7", 300M, GSTFREE1, 0M, WHTFREE1, 0M, 300M, AUD, 1, Now, ZDateTime.Empty,
					ZBool.False, job1LocalClientInvoice, job1, CC7, CC7.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC7Line1);

			TransactionLine cC8Line1 = job1LocalClientInvoice.FindTransactionLine("REV", CC8, job1.PK);
			AssertTransactionLineValues(cC8Line1, "REV", 8, "Job 1 Charge Code 8", 300M, GST1, 30M, WHTFREE1, 0M, 330M, AUD, 1, Now, ZDateTime.Empty,
					ZBool.False, job1LocalClientInvoice, job1, CC8, CC8.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC8Line1);

			#endregion

			#region Job2 Local Client AUD Invoice

			InvoicingBase[] job2LocalClientInvoices = creator.Poster.GetInvoices(AUD, LocalClient, job2.JH_JobNum);
			AssertEquals(1, job2LocalClientInvoices.Length);
			ARInvoice job2LocalClientInvoice = (ARInvoice)job2LocalClientInvoices[0];
			AssertEquals("Invoice Lines", 2, job2LocalClientInvoice.Lines.Count);

			AssertTransactionHeaderValues(job2LocalClientInvoice, "AR", "INV", null, "Z00001001", Now, Now,
					600M, 20M, 20M, 620M, AUD, 1, Now, ZBool.False, LocalClient, job2,
					"COD", 0, ZString.Empty, String.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(job2LocalClientInvoice);

			TransactionLine cC1Line2 = job2LocalClientInvoice.FindTransactionLine("REV", CC1, job2.PK);
			AssertTransactionLineValues(cC1Line2, "REV", 1, "Job 2 Charge Code 1", 200M, GST1, 20M, WHTFREE1, 0M, 220M, AUD, 1, Now, ZDateTime.Empty,
					ZBool.False, job2LocalClientInvoice, job2, CC1, CC1.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC1Line2);

			TransactionLine cC3Line2 = job2LocalClientInvoice.FindTransactionLine("REV", CC3, job2.PK);
			AssertTransactionLineValues(cC3Line2, "REV", 3, "Job 2 Charge Code 3", 400M, GSTFREE1, 0M, WHT1, 20M, 400M, AUD, 1, Now, ZDateTime.Empty,
					ZBool.False, job2LocalClientInvoice, job2, CC3, CC3.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC3Line2);

			#endregion

			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, consolNumber);
			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, consolNumber + "/A");
			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, consolNumber + "/B");
			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, "Z00001000");
			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, "Z00001001");

			#endregion

			#region Payables

			#region Creditor 1 Invoice 1

			APInvoice creditor1Inv1 = transactions.RetrieveAPInvoice(Creditor1, "1");
			AssertEquals("Invoice Line Count", 2, creditor1Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor1Inv1, "AP", "INV", "1", "Z00001000", Now.AddDays(10), Now.AddDays(20),
					-300M, -10M, 0M, -310M, AUD, 1, Now, ZBool.False, Creditor1, job1,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor1Inv1);

			cC1Line1 = creditor1Inv1.FindTransactionLine("CST", CC1, job1.PK);
			AssertTransactionLineValues(cC1Line1, "CST", 1, "Job 1 Charge Code 1", -100M, GST1, -10M, WHTFREE1, 0M, -110M, AUD, 1, Now, Now,
					ZBool.False, creditor1Inv1, job1, CC1, CC1.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC1Line1);

			TransactionLine cC7Line = creditor1Inv1.FindTransactionLine("CST", CC7, job1.PK);
			AssertTransactionLineValues(cC7Line, "CST", 2, "Job 1 Charge Code 7", -200M, GSTFREE1, 0M, WHTFREE1, 0M, -200M, AUD, 1, Now, Now,
					ZBool.False, creditor1Inv1, job1, CC7, CC7.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC7Line);

			#endregion

			#region Creditor 1 Invoice 2

			APInvoice creditor1Inv2 = transactions.RetrieveAPInvoice(Creditor1, "2");
			AssertEquals("Invoice Line Count", 1, creditor1Inv2.Lines.Count);

			AssertTransactionHeaderValues(creditor1Inv2, "AP", "INV", "2", "Z00001000", Now.AddDays(10), Now.AddDays(20),
					-250M, -25M, 0M, -110M, GBP, .4M, Now, ZBool.False, Creditor1, job1,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertOverseaAPTransactionHeaderDefaults_ForBasePostManager(creditor1Inv2);

			TransactionLine cC5Line = creditor1Inv2.FindTransactionLine("CST", CC5, job1.PK);
			AssertTransactionLineValues(cC5Line, "CST", 1, "Job 1 Charge Code 5", -250M, GST1, -25M, WHTFREE1, 0M, -110M, GBP, .4M, Now, Now,
					ZBool.False, creditor1Inv2, job1, CC5, CC5.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC5Line);

			#endregion

			#region Creditor 1 Invoice 3

			APInvoice creditor1Inv3 = transactions.RetrieveAPInvoice(Creditor1, "3");
			AssertEquals("Invoice Line Count", 1, creditor1Inv3.Lines.Count);

			AssertTransactionHeaderValues(creditor1Inv3, "AP", "INV", "3", "Z00001001", Now.AddDays(10), Now.AddDays(20),
					-150M, -15M, 0M, -165M, AUD, 1, Now, ZBool.False, Creditor1, job2,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor1Inv3);

			cC1Line2 = creditor1Inv3.FindTransactionLine("CST", CC1, job2.PK);
			AssertTransactionLineValues(cC1Line2, "CST", 1, "Job 2 Charge Code 1", -150M, GST1, -15M, WHTFREE1, 0M, -165M, AUD, 1, Now, Now,
					ZBool.False, creditor1Inv3, job2, CC1, CC1.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC1Line2);

			#endregion

			#region Creditor 2 Invoice 1

			APInvoice creditor2Inv1 = transactions.RetrieveAPInvoice(Creditor2, "1");
			AssertEquals("Invoice Line Count", 1, creditor2Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor2Inv1, "AP", "INV", "1", "Z00001000", Now.AddDays(10), Now.AddDays(20),
					-200M, -20M, -10M, -220M, AUD, 1, Now, ZBool.False, Creditor2, job1,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor2Inv1);

			cC2Line1 = creditor2Inv1.FindTransactionLine("CST", CC2, job1.PK);
			AssertTransactionLineValues(cC2Line1, "CST", 1, "Job 1 Charge Code 2", -200M, GST1, -20M, WHT1, -10M, -220M, AUD, 1, Now, Now,
					ZBool.False, creditor2Inv1, job1, CC2, CC2.CostAccount, Creditor2);
			AssertTransactionLineDefaults(cC2Line1);

			#endregion

			#region Creditor 2 Invoice 2

			APInvoice creditor2Inv2 = transactions.RetrieveAPInvoice(Creditor2, "2");
			AssertEquals("Invoice Line Count", 1, creditor2Inv2.Lines.Count);

			AssertTransactionHeaderValues(creditor2Inv2, "AP", "INV", "2", "Z00001000", Now.AddDays(20), Now.AddDays(30),
					-285.71M, -28.57M, -14.29M, -220M, USD, 0.700013M, Now, ZBool.False, Creditor2, job1,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertOverseaAPTransactionHeaderDefaults_ForBasePostManager(creditor2Inv2);

			TransactionLine cC6Line = creditor2Inv2.FindTransactionLine("CST", CC6, job1.PK);
			AssertTransactionLineValues(cC6Line, "CST", 1, "Job 1 Charge Code 6", -285.71M, GST1, -28.57M, WHT1, -14.29M, -220M, USD, .7M, Now, Now,
					ZBool.False, creditor2Inv2, job1, CC6, CC6.CostAccount, Creditor2);
			AssertTransactionLineDefaults(cC6Line);

			#endregion

			#region Creditor 2 Invoice 3

			APInvoice creditor2Inv3 = transactions.RetrieveAPInvoice(Creditor2, "3");
			AssertEquals("Invoice Line Count", 1, creditor2Inv3.Lines.Count);

			AssertTransactionHeaderValues(creditor2Inv3, "AP", "INV", "3", "Z00001001", Now.AddDays(10), Now.AddDays(20),
					-357.14M, -35.71M, -17.86M, -275M, USD, 0.700013M, Now, ZBool.False, Creditor2, job2,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertOverseaAPTransactionHeaderDefaults_ForBasePostManager(creditor2Inv3);

			cC2Line2 = creditor2Inv3.FindTransactionLine("CST", CC2, job2.PK);
			AssertTransactionLineValues(cC2Line2, "CST", 1, "Job 2 Charge Code 2", -357.14M, GST1, -35.71M, WHT1, -17.86M, -275M, USD, .7M, Now, Now,
					ZBool.False, creditor2Inv3, job2, CC2, CC2.CostAccount, Creditor2);
			AssertTransactionLineDefaults(cC2Line2);

			#endregion

			#region Creditor 3 Invoice 1

			APInvoice creditor3Inv1 = transactions.RetrieveAPInvoice(Creditor3, "1");
			AssertEquals("Invoice Line Count", 1, creditor3Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor3Inv1, "AP", "INV", "1", "Z00001000", Now.AddDays(10), Now.AddDays(20),
					-300M, 0M, -15M, -300M, AUD, 1, Now, ZBool.False, Creditor3, job1,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor3Inv1);

			TransactionLine cC3Line = creditor3Inv1.FindTransactionLine("CST", CC3, job1.PK);
			AssertTransactionLineValues(cC3Line, "CST", 1, "Job 1 Charge Code 3", -300M, GSTFREE1, 0M, WHT1, -15M, -300M, AUD, 1, Now, Now,
					ZBool.False, creditor3Inv1, job1, CC3, CC3.CostAccount, Creditor3);
			AssertTransactionLineDefaults(cC3Line);

			#endregion

			#region Creditor 3 Invoice 2

			APInvoice creditor3Inv2 = transactions.RetrieveAPInvoice(Creditor3, "2");
			AssertEquals("Invoice Line Count", 1, creditor3Inv2.Lines.Count);

			AssertTransactionHeaderValues(creditor3Inv2, "AP", "INV", "2", "Z00001001", Now.AddDays(15), Now.AddDays(25),
					-350, 0M, -17.50M, -350M, AUD, 1, Now, ZBool.False, Creditor3, job2,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor3Inv2);

			cC3Line2 = creditor3Inv2.FindTransactionLine("CST", CC3, job2.PK);
			AssertTransactionLineValues(cC3Line2, "CST", 1, "Job 2 Charge Code 3", -350M, GSTFREE1, 0M, WHT1, -17.50M, -350M, AUD, 1, Now, Now,
					ZBool.False, creditor3Inv2, job2, CC3, CC3.CostAccount, Creditor3);
			AssertTransactionLineDefaults(cC3Line2);

			#endregion

			#region Creditor 3 Invoice 3

			APInvoice creditor3Inv3 = transactions.RetrieveAPInvoice(Creditor3, "3");
			AssertEquals("Invoice Line Count", 2, creditor3Inv3.Lines.Count);

			AssertTransactionHeaderValues(creditor3Inv3, "AP", "INV", "3", "Multiple Jobs", Now.AddDays(10), Now.AddDays(25),
					-635M, -63.50M, 0M, -698.50M, AUD, 1, Now, ZBool.False, Creditor3, null,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor3Inv3);

			cC8Line1 = creditor3Inv3.FindTransactionLine("CST", CC8, job1.PK);
			AssertTransactionLineValues(cC8Line1, "CST", 1, "Job 1 Charge Code 8", -300M, GST1, -30M, WHTFREE1, 0M, -330M, AUD, 1, Now, Now,
					ZBool.False, creditor3Inv3, job1, CC8, CC8.CostAccount, Creditor3);
			AssertTransactionLineDefaults(cC8Line1);

			cC8Line2 = creditor3Inv3.FindTransactionLine("CST", CC8, job2.PK);
			AssertTransactionLineValues(cC8Line2, "CST", 2, "Job 2 Charge Code 8", -335M, GST1, -33.50M, WHTFREE1, 0M, -368.50M, AUD, 1, Now, Now,
					ZBool.False, creditor3Inv3, job2, CC8, CC8.CostAccount, Creditor3);
			AssertTransactionLineDefaults(cC8Line2);

			#endregion

			#endregion

			#region WIP and Accrual Reversal Assertions

			AssertEquals("Charge 1_1 WIP Reversed", true, charge1_1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_2 WIP Reversed", true, charge1_2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_3 WIP Reversed", true, charge1_3WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_4 WIP Reversed", true, charge1_4WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_5 WIP Reversed", true, charge1_5WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_6 WIP Reversed", true, charge1_6WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_7 WIP Reversed", true, charge1_7WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_1 WIP Reversed", true, charge2_1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_2 WIP Reversed", true, charge2_2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_3 WIP Reversed", true, charge2_3WIP.AL_ReverseDate.IsValid);

			AssertEquals("Charge 1_1 Accrual Reversed", true, charge1_1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_2 Accrual Reversed", true, charge1_2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_3 Accrual Reversed", true, charge1_3Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_5 Accrual Reversed", true, charge1_5Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_6 Accrual Reversed", true, charge1_6Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_7 Accrual Reversed", true, charge1_7Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_1 Accrual Reversed", true, charge2_1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_2 Accrual Reversed", true, charge2_2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_3 Accrual Reversed", true, charge2_3Accrual.AL_ReverseDate.IsValid);

			#endregion
		}

		#endregion

		#region TEST: Create Invoices For Entire Consol With Payments

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateInvoicesForEntireConsolWithPayments()
		{
			#region Setup

			AccountingConfigurationRegistry.Instance.AllowForwardDatingofAPInvoiceDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			new AccountingPeriodTestHelper(Factory).SetupPeriods();

			ForwardingConsol consol = CreateConsol("AUSYD", "USLAX", "C00001000");

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = CC1.PK;
			cost.E6_OH_Creditor = Creditor3.PK;
			cost.E6_InvoiceNum = "3";
			cost.E6_InvoiceDate = Now.AddDays(10);
			cost.E6_PaymentDate = Now.AddDays(25);
			ZGuid costSplitGroup1 = cost.PK;

			Job job1 = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			job1.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			CreateExchangeRate(job1, USD, .7M);
			CreateExchangeRate(job1, GBP, .4M);

			Charge charge1_1 = CreateCharge(job1, CC1, "Job 1 Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge1_2 = CreateCharge(job1, CC2, "Job 1 Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			Charge charge1_3 = CreateCharge(job1, CC3, "Job 1 Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);
			Charge charge1_4 = CreateCharge(job1, CC4, "Job 1 Charge Code 4", null, 0M, null, USD, 500M, Agent);
			charge1_4.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			Charge charge1_5 = CreateCharge(job1, CC5, "Job 1 Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, Agent);
			charge1_5.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			Charge charge1_6 = CreateCharge(job1, CC6, "Job 1 Charge Code 6", USD, 200M, Creditor2, USD, 275M, Agent);
			charge1_6.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			Charge charge1_7 = CreateCharge(job1, CC7, "Job 1 Charge Code 7", AUD, 200M, Creditor1, AUD, 300M, LocalClient);
			Charge charge1_8 = CreateCharge(job1, CC8, "Job 1 Charge Code 8", AUD, 300M, Creditor3, AUD, 300M, LocalClient);
			charge1_8.JR_E6 = costSplitGroup1;

			SetAPInvoiceInfo(charge1_1, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_2, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_3, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_5, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_6, "2", Now.AddDays(20), Now.AddDays(30));
			SetAPInvoiceInfo(charge1_7, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_8, "3", Now.AddDays(10), Now.AddDays(25));

			SetAPPaymentInfo(charge1_1, ReceiptTypes.Cash, AUDBankAccount, "CASH");
			SetAPPaymentInfo(charge1_6, ReceiptTypes.CreditCard, USDBankAccount, "CC");
			SetAPPaymentInfo(charge1_7, ReceiptTypes.Cash, AUDBankAccount, "CASH");
			SetAPPaymentInfo(charge1_8, ReceiptTypes.Cash, AUDBankAccount, "CASH2");

			Job job2 = CreateJob("Z00001001", LocalClient, 5M, Agent, 10M);
			job2.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			CreateExchangeRate(job2, USD, .7M);
			CreateExchangeRate(job2, GBP, .4M);

			Charge charge2_1 = CreateCharge(job2, CC1, "Job 2 Charge Code 1", AUD, 150M, Creditor1, AUD, 200M, LocalClient);
			Charge charge2_2 = CreateCharge(job2, CC2, "Job 2 Charge Code 2", USD, 250M, Creditor2, USD, 250M, Agent);
			charge2_2.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			Charge charge2_3 = CreateCharge(job2, CC3, "Job 2 Charge Code 3", AUD, 350M, Creditor3, AUD, 400M, LocalClient);
			Charge charge2_4 = CreateCharge(job2, CC8, "Job 2 Charge Code 8", AUD, 335M, Creditor3, AUD, 500M, Agent);
			charge2_4.JR_E6 = costSplitGroup1;

			SetAPInvoiceInfo(charge2_1, "3", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge2_2, "3", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge2_3, "2", Now.AddDays(15), Now.AddDays(25));
			SetAPInvoiceInfo(charge2_4, "3", Now.AddDays(10), Now.AddDays(25));

			SetAPPaymentInfo(charge2_1, ReceiptTypes.Cash, AUDBankAccount, "CASH");
			SetAPPaymentInfo(charge2_2, ReceiptTypes.CreditCard, USDBankAccount, "CC");
			SetAPPaymentInfo(charge2_4, ReceiptTypes.Cash, AUDBankAccount, "CASH2");

			cost.E6_OSCostAmount = charge2_4.JR_OSCostAmt + charge1_8.JR_OSCostAmt;
			cost.E6_LocalCostAmount = charge2_4.JR_LocalCostAmt + charge1_8.JR_LocalCostAmt;

			Factory.Save();

			#region WIPs and Accruals

			AccTransactionLines charge1_1WIP = charge1_1.WIP;
			AccTransactionLines charge1_2WIP = charge1_2.WIP;
			AccTransactionLines charge1_3WIP = charge1_3.WIP;
			AccTransactionLines charge1_4WIP = charge1_4.WIP;
			AccTransactionLines charge1_5WIP = charge1_5.WIP;
			AccTransactionLines charge1_6WIP = charge1_6.WIP;
			AccTransactionLines charge1_7WIP = charge1_7.WIP;
			AccTransactionLines charge1_8WIP = charge1_8.WIP;
			AccTransactionLines charge2_1WIP = charge2_1.WIP;
			AccTransactionLines charge2_2WIP = charge2_2.WIP;
			AccTransactionLines charge2_3WIP = charge2_3.WIP;
			AccTransactionLines charge2_4WIP = charge2_4.WIP;

			AccTransactionLines charge1_1Accrual = charge1_1.Accrual;
			AccTransactionLines charge1_2Accrual = charge1_2.Accrual;
			AccTransactionLines charge1_3Accrual = charge1_3.Accrual;
			AccTransactionLines charge1_5Accrual = charge1_5.Accrual;
			AccTransactionLines charge1_6Accrual = charge1_6.Accrual;
			AccTransactionLines charge1_7Accrual = charge1_7.Accrual;
			AccTransactionLines charge1_8Accrual = charge1_8.Accrual;
			AccTransactionLines charge2_1Accrual = charge2_1.Accrual;
			AccTransactionLines charge2_2Accrual = charge2_2.Accrual;
			AccTransactionLines charge2_3Accrual = charge2_3.Accrual;
			AccTransactionLines charge2_4Accrual = charge2_4.Accrual;

			#endregion

			var jobs = new[] { job1, job2 };

			#endregion

			ConsolInvoicingPostManager creator = new ConsolInvoicingPostManager(Factory, jobs, consol, apps);
			TransactionCreatorHashtable transactions = creator.CreateTransactions(JobInvoicingPostingOption.All);
			Factory.Save();
			AssertEquals("Payables Trx Count", 14, transactions.APTransactionsCount + transactions.GetAllAPPaymentApprovals().Length);
			AssertEquals("Receivable Transactions Count", 5, transactions.ARTransactionsCount);

			#region Agent AUD Invoice

			InvoicingBase[] agentAUDInvoices = creator.Poster.GetInvoices(AUD, Agent, "C00001000");
			AssertEquals(1, agentAUDInvoices.Length);
			ARInvoice agentAUDInvoice = (ARInvoice)agentAUDInvoices[0];
			AssertEquals("Invoice Lines", 2, agentAUDInvoice.Lines.Count);

			AssertTransactionHeaderValues(agentAUDInvoice, "AR", "INV", null, "FREIGHT CONSOL INVOICE", Now, Now,
					850M, 50M, 17.50M, 900M, AUD, 1, Now, ZBool.False, Agent, null,
					"COD", 0, ZString.Empty, String.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(agentAUDInvoice, true);

			TransactionLine cC3Line1 = agentAUDInvoice.FindTransactionLine("REV", CC3, job1.PK);
			AssertTransactionLineValues(cC3Line1, "REV", 3, "Job 1 Charge Code 3", 350M, GSTFREE1, 0M, WHT1, 17.50M, 350M, AUD, 1, Now, Now, ZBool.False, agentAUDInvoice, job1, CC3, CC3.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC3Line1);

			TransactionLine cC8Line2 = agentAUDInvoice.FindTransactionLine("REV", CC8, job2.PK);
			AssertTransactionLineValues(cC8Line2, "REV", 4, "Job 2 Charge Code 8", 500M, GST1, 50M, WHTFREE1, 0M, 550M, AUD, 1, Now, Now, ZBool.False, agentAUDInvoice, job2, CC8, CC8.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC8Line2);

			#endregion

			#region Agent USD Invoice

			InvoicingBase[] agentUSDInvoices = creator.Poster.GetInvoices(USD, Agent);
			AssertEquals(1, agentUSDInvoices.Length);
			ARInvoice agentUSDInvoice = (ARInvoice)agentUSDInvoices[0];
			AssertEquals("Invoice Lines", 3, agentUSDInvoice.Lines.Count);

			AssertTransactionHeaderValues(agentUSDInvoice, "AR", "INV", null, "FREIGHT CONSOL INVOICE", Now, Now,
					1464.29M, 75.00M, 37.50M, 1077.50M, USD, .7M, Now, ZBool.False, Agent, null,
					"COD", 0, ZString.Empty, String.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(agentUSDInvoice, true);

			TransactionLine cC4Line1 = agentUSDInvoice.FindTransactionLine("REV", CC4, job1.PK);
			AssertTransactionLineValues(cC4Line1, "REV", 4, "Job 1 Charge Code 4", 714.29M, GSTFREE1, 0M, WHTFREE1, 0M, 500M, USD, .7M, Now, Now, ZBool.False, agentUSDInvoice, job1, CC4, CC4.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC4Line1);

			TransactionLine cC6Line1 = agentUSDInvoice.FindTransactionLine("REV", CC6, job1.PK);
			AssertTransactionLineValues(cC6Line1, "REV", 6, "Job 1 Charge Code 6", 392.86M, GST1, 39.29M, WHT1, 19.64M, 302.50M, USD, .7M, Now, Now, ZBool.False, agentUSDInvoice, job1, CC6, CC6.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC6Line1);

			TransactionLine cC2Line2 = agentUSDInvoice.FindTransactionLine("REV", CC2, job2.PK);
			AssertTransactionLineValues(cC2Line2, "REV", 2, "Job 2 Charge Code 2", 357.14M, GST1, 35.71M, WHT1, 17.86M, 275M, USD, .7M, Now, Now, ZBool.False, agentUSDInvoice, job2, CC2, CC2.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC2Line2);

			#endregion

			#region Agent GBP Invoice

			InvoicingBase[] agentGBPInvoices = creator.Poster.GetInvoices(GBP, Agent);
			AssertEquals(1, agentGBPInvoices.Length);
			ARInvoice agentGBPInvoice = (ARInvoice)agentGBPInvoices[0];
			AssertEquals("Invoice Lines", 1, agentGBPInvoice.Lines.Count);

			AssertTransactionHeaderValues(agentGBPInvoice, "AR", "INV", null, "FREIGHT CONSOL INVOICE", Now, Now,
					312.50M, 31.25M, 0, 137.50M, GBP, .4M, Now, ZBool.False, Agent, null,
					"COD", 0, ZString.Empty, String.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(agentGBPInvoice, true);

			TransactionLine cC5Line1 = agentGBPInvoice.FindTransactionLine("REV", CC5, job1.PK);
			AssertTransactionLineValues(cC5Line1, "REV", 5, "Job 1 Charge Code 5", 312.50M, GST1, 31.25M, WHTFREE1, 0M, 137.50M, GBP, .4M, Now, Now, ZBool.False, agentGBPInvoice, job1, CC5, CC5.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC5Line1);

			#endregion

			#region Job1 Local Client AUD Invoice

			InvoicingBase[] job1LocalClientInvoices = creator.Poster.GetInvoices(AUD, LocalClient, job1.JH_JobNum);
			AssertEquals(1, job1LocalClientInvoices.Length);
			ARInvoice job1LocalClientInvoice = (ARInvoice)job1LocalClientInvoices[0];
			AssertEquals("Invoice Lines", 4, job1LocalClientInvoice.Lines.Count);

			AssertTransactionHeaderValues(job1LocalClientInvoice, "AR", "INV", null, "Z00001000", Now, Now,
					950M, 65M, 10M, 1015M, AUD, 1, Now, ZBool.False, LocalClient, job1,
					"COD", 0, ZString.Empty, String.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(job1LocalClientInvoice);

			TransactionLine cC1Line1 = job1LocalClientInvoice.FindTransactionLine("REV", CC1, job1.PK);
			AssertTransactionLineValues(cC1Line1, "REV", 1, "Job 1 Charge Code 1", 150M, GST1, 15M, WHTFREE1, 0M, 165M, AUD, 1, Now, Now, ZBool.False, job1LocalClientInvoice, job1, CC1, CC1.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC1Line1);

			TransactionLine cC2Line1 = job1LocalClientInvoice.FindTransactionLine("REV", CC2, job1.PK);
			AssertTransactionLineValues(cC2Line1, "REV", 2, "Job 1 Charge Code 2", 200M, GST1, 20M, WHT1, 10M, 220M, AUD, 1, Now, Now, ZBool.False, job1LocalClientInvoice, job1, CC2, CC2.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC2Line1);

			TransactionLine cC7Line1 = job1LocalClientInvoice.FindTransactionLine("REV", CC7, job1.PK);
			AssertTransactionLineValues(cC7Line1, "REV", 7, "Job 1 Charge Code 7", 300M, GSTFREE1, 0M, WHTFREE1, 0M, 300M, AUD, 1, Now, Now, ZBool.False, job1LocalClientInvoice, job1, CC7, CC7.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC7Line1);

			TransactionLine cC8Line1 = job1LocalClientInvoice.FindTransactionLine("REV", CC8, job1.PK);
			AssertTransactionLineValues(cC8Line1, "REV", 8, "Job 1 Charge Code 8", 300M, GST1, 30M, WHTFREE1, 0M, 330M, AUD, 1, Now, Now, ZBool.False, job1LocalClientInvoice, job1, CC8, CC8.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC8Line1);

			#endregion

			#region Job2 Local Client AUD Invoice

			InvoicingBase[] job2LocalClientInvoices = creator.Poster.GetInvoices(AUD, LocalClient, job2.JH_JobNum);
			AssertEquals(1, job2LocalClientInvoices.Length);
			ARInvoice job2LocalClientInvoice = (ARInvoice)job2LocalClientInvoices[0];
			AssertEquals("Invoice Lines", 2, job2LocalClientInvoice.Lines.Count);

			AssertTransactionHeaderValues(job2LocalClientInvoice, "AR", "INV", null, "Z00001001", Now, Now,
					600M, 20M, 20M, 620M, AUD, 1, Now, ZBool.False, LocalClient, job2,
					"COD", 0, ZString.Empty, String.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(job2LocalClientInvoice);

			TransactionLine cC1Line2 = job2LocalClientInvoice.FindTransactionLine("REV", CC1, job2.PK);
			AssertTransactionLineValues(cC1Line2, "REV", 1, "Job 2 Charge Code 1", 200M, GST1, 20M, WHTFREE1, 0M, 220M, AUD, 1, Now, Now, ZBool.False, job2LocalClientInvoice, job2, CC1, CC1.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC1Line2);

			TransactionLine cC3Line2 = job2LocalClientInvoice.FindTransactionLine("REV", CC3, job2.PK);
			AssertTransactionLineValues(cC3Line2, "REV", 3, "Job 2 Charge Code 3", 400M, GSTFREE1, 0M, WHT1, 20M, 400M, AUD, 1, Now, Now, ZBool.False, job2LocalClientInvoice, job2, CC3, CC3.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC3Line2);

			#endregion

			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, "C00001000");
			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, "C00001000/A");
			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, "C00001000/B");
			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, "Z00001000");
			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, "Z00001001");

			#region Creditor 1 Invoice 1

			APInvoice creditor1Inv1 = transactions.RetrieveAPInvoice(Creditor1, "1");
			AssertEquals("Invoice Line Count", 2, creditor1Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor1Inv1, "AP", "INV", "1", "Z00001000", Now.AddDays(10), Now.AddDays(20),
					-300M, -10M, 0M, -310M, AUD, 1, Now, ZBool.False, Creditor1, job1,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.True);
			AssertTransactionHeaderDefaults(creditor1Inv1);

			cC1Line1 = creditor1Inv1.FindTransactionLine("CST", CC1, job1.PK);
			AssertTransactionLineValues(cC1Line1, "CST", 1, "Job 1 Charge Code 1", -100M, GST1, -10M, WHTFREE1, 0M, -110M, AUD, 1, Now, Now,
					ZBool.False, creditor1Inv1, job1, CC1, CC1.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC1Line1);

			TransactionLine cC7Line = creditor1Inv1.FindTransactionLine("CST", CC7, job1.PK);
			AssertTransactionLineValues(cC7Line, "CST", 2, "Job 1 Charge Code 7", -200M, GSTFREE1, 0M, WHTFREE1, 0M, -200M, AUD, 1, Now, Now,
					ZBool.False, creditor1Inv1, job1, CC7, CC7.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC7Line);

			#endregion

			#region Creditor 1 Invoice 2

			APInvoice creditor1Inv2 = transactions.RetrieveAPInvoice(Creditor1, "2");
			AssertEquals("Invoice Line Count", 1, creditor1Inv2.Lines.Count);

			AssertTransactionHeaderValues(creditor1Inv2, "AP", "INV", "2", "Z00001000", Now.AddDays(10), Now.AddDays(20),
					-250M, -25M, 0M, -110M, GBP, .4M, Now, ZBool.False, Creditor1, job1,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertOverseaAPTransactionHeaderDefaults_ForBasePostManager(creditor1Inv2);

			TransactionLine cC5Line = creditor1Inv2.FindTransactionLine("CST", CC5, job1.PK);
			AssertTransactionLineValues(cC5Line, "CST", 1, "Job 1 Charge Code 5", -250M, GST1, -25M, WHTFREE1, 0M, -110M, GBP, .4M, Now, Now,
					ZBool.False, creditor1Inv2, job1, CC5, CC5.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC5Line);

			#endregion

			#region Creditor 1 Invoice 3

			APInvoice creditor1Inv3 = transactions.RetrieveAPInvoice(Creditor1, "3");
			AssertEquals("Invoice Line Count", 1, creditor1Inv3.Lines.Count);

			AssertTransactionHeaderValues(creditor1Inv3, "AP", "INV", "3", "Z00001001", Now.AddDays(10), Now.AddDays(20),
					-150M, -15M, 0M, -165M, AUD, 1, Now, ZBool.False, Creditor1, job2,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.True);
			AssertTransactionHeaderDefaults(creditor1Inv3);

			cC1Line2 = creditor1Inv3.FindTransactionLine("CST", CC1, job2.PK);
			AssertTransactionLineValues(cC1Line2, "CST", 1, "Job 2 Charge Code 1", -150M, GST1, -15M, WHTFREE1, 0M, -165M, AUD, 1, Now, Now,
					ZBool.False, creditor1Inv3, job2, CC1, CC1.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC1Line2);

			#endregion

			#region Creditor 2 Invoice 1

			APInvoice creditor2Inv1 = transactions.RetrieveAPInvoice(Creditor2, "1");
			AssertEquals("Invoice Line Count", 1, creditor2Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor2Inv1, "AP", "INV", "1", "Z00001000", Now.AddDays(10), Now.AddDays(20),
					-200M, -20M, -10M, -220M, AUD, 1, Now, ZBool.False, Creditor2, job1,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor2Inv1);

			cC2Line1 = creditor2Inv1.FindTransactionLine("CST", CC2, job1.PK);
			AssertTransactionLineValues(cC2Line1, "CST", 1, "Job 1 Charge Code 2", -200M, GST1, -20M, WHT1, -10M, -220M, AUD, 1, Now, Now,
					ZBool.False, creditor2Inv1, job1, CC2, CC2.CostAccount, Creditor2);
			AssertTransactionLineDefaults(cC2Line1);

			#endregion

			#region Creditor 2 Invoice 2

			APInvoice creditor2Inv2 = transactions.RetrieveAPInvoice(Creditor2, "2");
			AssertEquals("Invoice Line Count", 1, creditor2Inv2.Lines.Count);

			AssertTransactionHeaderValues(creditor2Inv2, "AP", "INV", "2", "Z00001000", Now.AddDays(20), Now.AddDays(30),
					-285.71M, -28.57M, -14.29M, -220M, USD, 0.700013M, Now, ZBool.False, Creditor2, job1,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.True);
			AssertOverseaAPTransactionHeaderDefaults_ForBasePostManager(creditor2Inv2);

			TransactionLine cC6Line = creditor2Inv2.FindTransactionLine("CST", CC6, job1.PK);
			AssertTransactionLineValues(cC6Line, "CST", 1, "Job 1 Charge Code 6", -285.71M, GST1, -28.57M, WHT1, -14.29M, -220M, USD, 0.700013M, Now, Now,
					ZBool.False, creditor2Inv2, job1, CC6, CC6.CostAccount, Creditor2);
			AssertTransactionLineDefaults(cC6Line);

			#endregion

			#region Creditor 2 Invoice 3

			APInvoice creditor2Inv3 = transactions.RetrieveAPInvoice(Creditor2, "3");
			AssertEquals("Invoice Line Count", 1, creditor2Inv3.Lines.Count);

			AssertTransactionHeaderValues(creditor2Inv3, "AP", "INV", "3", "Z00001001", Now.AddDays(10), Now.AddDays(20),
					-357.14M, -35.71M, -17.86M, -275M, USD, 0.700013M, Now, ZBool.False, Creditor2, job2,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.True);
			AssertOverseaAPTransactionHeaderDefaults_ForBasePostManager(creditor2Inv3);

			cC2Line2 = creditor2Inv3.FindTransactionLine("CST", CC2, job2.PK);
			AssertTransactionLineValues(cC2Line2, "CST", 1, "Job 2 Charge Code 2", -357.14M, GST1, -35.71M, WHT1, -17.86M, -275M, USD, 0.700013M, Now, Now,
					ZBool.False, creditor2Inv3, job2, CC2, CC2.CostAccount, Creditor2);
			AssertTransactionLineDefaults(cC2Line2);

			#endregion

			#region Creditor 3 Invoice 1

			APInvoice creditor3Inv1 = transactions.RetrieveAPInvoice(Creditor3, "1");
			AssertEquals("Invoice Line Count", 1, creditor3Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor3Inv1, "AP", "INV", "1", "Z00001000", Now.AddDays(10), Now.AddDays(20),
					-300M, 0M, -15M, -300M, AUD, 1, Now, ZBool.False, Creditor3, job1,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor3Inv1);

			TransactionLine cC3Line = creditor3Inv1.FindTransactionLine("CST", CC3, job1.PK);
			AssertTransactionLineValues(cC3Line, "CST", 1, "Job 1 Charge Code 3", -300M, GSTFREE1, 0M, WHT1, -15M, -300M, AUD, 1, Now, Now,
					ZBool.False, creditor3Inv1, job1, CC3, CC3.CostAccount, Creditor3);
			AssertTransactionLineDefaults(cC3Line);

			#endregion

			#region Creditor 3 Invoice 2

			APInvoice creditor3Inv2 = transactions.RetrieveAPInvoice(Creditor3, "2");
			AssertEquals("Invoice Line Count", 1, creditor3Inv2.Lines.Count);

			AssertTransactionHeaderValues(creditor3Inv2, "AP", "INV", "2", "Z00001001", Now.AddDays(15), Now.AddDays(25),
					-350, 0M, -17.50M, -350M, AUD, 1, Now, ZBool.False, Creditor3, job2,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor3Inv2);

			cC3Line2 = creditor3Inv2.FindTransactionLine("CST", CC3, job2.PK);
			AssertTransactionLineValues(cC3Line2, "CST", 1, "Job 2 Charge Code 3", -350M, GSTFREE1, 0M, WHT1, -17.50M, -350M, AUD, 1, Now, Now,
					ZBool.False, creditor3Inv2, job2, CC3, CC3.CostAccount, Creditor3);
			AssertTransactionLineDefaults(cC3Line2);

			#endregion

			#region Creditor 3 Invoice 3

			APInvoice creditor3Inv3 = transactions.RetrieveAPInvoice(Creditor3, "3");
			AssertEquals("Invoice Line Count", 2, creditor3Inv3.Lines.Count);

			AssertTransactionHeaderValues(creditor3Inv3, "AP", "INV", "3", "Multiple Jobs", Now.AddDays(10), Now.AddDays(25),
					-635M, -63.50M, 0M, -698.50M, AUD, 1, Now, ZBool.False, Creditor3, null,
					"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.True);
			AssertTransactionHeaderDefaults(creditor3Inv3);

			cC8Line1 = creditor3Inv3.FindTransactionLine("CST", CC8, job1.PK);
			AssertTransactionLineValues(cC8Line1, "CST", 1, "Job 1 Charge Code 8", -300M, GST1, -30M, WHTFREE1, 0M, -330M, AUD, 1, Now, Now,
					ZBool.False, creditor3Inv3, job1, CC8, CC8.CostAccount, Creditor3);
			AssertTransactionLineDefaults(cC8Line1);

			cC8Line2 = creditor3Inv3.FindTransactionLine("CST", CC8, job2.PK);
			AssertTransactionLineValues(cC8Line2, "CST", 2, "Job 2 Charge Code 8", -335M, GST1, -33.50M, WHTFREE1, 0M, -368.50M, AUD, 1, Now, Now,
					ZBool.False, creditor3Inv3, job2, CC8, CC8.CostAccount, Creditor3);
			AssertTransactionLineDefaults(cC8Line2);

			#endregion

			#region Job1 AUD Payment

			APPayment aUDPayment1 = transactions.RetrieveAPPayment_ForTestOnly(Creditor1, AUDBankAccount, "CSH", "CASH", job1.JH_JobNum);
			AssertTransactionHeaderValues(aUDPayment1, "AP", "PAY", null, "AP Payment Z00001000", Now, ZDateTime.Empty,
					310.00M, 0M, 0M, 310M, AUD, 1, Now, ZBool.False, Creditor1, null, ZString.Empty, 0, "CASH", "CSH",
					AUDBankAccount, ZBool.True);
			AssertTransactionHeaderDefaults(aUDPayment1);

			#endregion

			#region Job2 AUD Payment

			APPayment aUDPayment2 = transactions.RetrieveAPPayment_ForTestOnly(Creditor1, AUDBankAccount, "CSH", "CASH", job2.JH_JobNum);
			AssertTransactionHeaderValues(aUDPayment2, "AP", "PAY", null, "AP Payment Z00001001", Now, ZDateTime.Empty,
					165.00M, 0M, 0M, 165M, AUD, 1, Now, ZBool.False, Creditor1, null, ZString.Empty, 0, "CASH", "CSH",
					AUDBankAccount, ZBool.True);
			AssertTransactionHeaderDefaults(aUDPayment2);

			#endregion

			#region Job1 USD Payment

			APPayment uSDPayment1 = transactions.RetrieveAPPayment_ForTestOnly(Creditor2, USDBankAccount, "CCD", "CC", job1.JH_JobNum);
			AssertTransactionHeaderValues(uSDPayment1, "AP", "PAY", null, "AP Payment Z00001000", Now, ZDateTime.Empty,
					314.28M, 0M, 0M, 220M, USD, 0.700013M, Now, ZBool.False, Creditor2, null, ZString.Empty, 0, "CC", "CCD",
					USDBankAccount, ZBool.True);
			AssertTransactionHeaderDefaults(uSDPayment1);

			#endregion

			#region Job2 USD Payment

			APPayment uSDPayment2 = transactions.RetrieveAPPayment_ForTestOnly(Creditor2, USDBankAccount, "CCD", "CC", job2.JH_JobNum);
			AssertTransactionHeaderValues(uSDPayment2, "AP", "PAY", null, "AP Payment Z00001001", Now, ZDateTime.Empty,
					392.85M, 0M, 0M, 275M, USD, 0.700013M, Now, ZBool.False, Creditor2, null, ZString.Empty, 0, "CC", "CCD",
					USDBankAccount, ZBool.True);
			AssertTransactionHeaderDefaults(uSDPayment2);

			#endregion

			#region Apportionment Payment

			APPayment apportionmentPayment = transactions.RetrieveAPPayment_ForTestOnly(Creditor3, AUDBankAccount, "CSH", "CASH2", ZString.Empty);
			AssertTransactionHeaderValues(apportionmentPayment, "AP", "PAY", null, "AP Payment C00001000", Now, ZDateTime.Empty,
					698.50M, 0M, 0M, 698.50M, AUD, 1, Now, ZBool.False, Creditor3, null, ZString.Empty, 0, "CASH2", "CSH",
					AUDBankAccount, ZBool.True);
			AssertTransactionHeaderDefaults(apportionmentPayment);

			#endregion

			TransactionMatchLinkGroup group1 = new TransactionMatchLinkGroup(aUDPayment1.RelatedPaymentApproval.NewPaymentMatchingObject.MatchLinks);
			TransactionMatchLinkGroup group2 = new TransactionMatchLinkGroup(uSDPayment1.RelatedPaymentApproval.NewPaymentMatchingObject.MatchLinks);
			TransactionMatchLinkGroup group3 = new TransactionMatchLinkGroup(aUDPayment2.RelatedPaymentApproval.NewPaymentMatchingObject.MatchLinks);
			TransactionMatchLinkGroup group4 = new TransactionMatchLinkGroup(uSDPayment2.RelatedPaymentApproval.NewPaymentMatchingObject.MatchLinks);
			TransactionMatchLinkGroup group5 = new TransactionMatchLinkGroup(apportionmentPayment.RelatedPaymentApproval.NewPaymentMatchingObject.MatchLinks);

			#region AUDPayment1 Matching

			AssertEquals("Group 1 Match Count", 2, group1.Count);
			TransactionMatchLink payment1MatchLink = group1.FindMatchLinkByTransactionHeaderAndAmount(aUDPayment1, 310M);
			TransactionMatchLink invoice1aMatchLink = group1.FindMatchLinkByTransactionHeaderAndAmount(creditor1Inv1, -310M);

			AssertNotNull("Payment1 Match Link Found", payment1MatchLink);
			AssertNotNull("Invoice1a Match Link Found", invoice1aMatchLink);

			AssertInvoiceAndPaymentAreInSameGroup(group1, invoice1aMatchLink, payment1MatchLink);

			AssertEquals("Payment1 Match Date", Now.ToString("yyyy MMM dd"), payment1MatchLink.AP_MatchDate.ToString("yyyy MMM dd"));
			AssertEquals("Invoice1a Match Date", Now.ToString("yyyy MMM dd"), invoice1aMatchLink.AP_MatchDate.ToString("yyyy MMM dd"));

			AssertMatchLinkDefaults(payment1MatchLink);
			AssertMatchLinkDefaults(invoice1aMatchLink);

			AssertAPInvoiceShowsAsPaid(creditor1Inv1);

			#endregion

			#region USDPayment1 Matching

			AssertEquals("Group 2 Match Count", 2, group2.Count);
			TransactionMatchLink payment2MatchLink = group2.FindMatchLinkByTransactionHeaderAndAmount(uSDPayment1, 314.28M);
			TransactionMatchLink invoice2aMatchLink = group2.FindMatchLinkByTransactionHeaderAndAmount(creditor2Inv2, -314.28M);

			AssertNotNull("Payment 2 Match Link Found", payment2MatchLink);
			AssertNotNull("Invoice 2a Match Link Found", invoice2aMatchLink);

			AssertInvoiceAndPaymentAreInSameGroup(group2, invoice2aMatchLink, payment2MatchLink);

			AssertEquals("Payment2 Match Date", Now.ToString("yyyy MMM dd"), payment2MatchLink.AP_MatchDate.ToString("yyyy MMM dd"));
			AssertEquals("Invoice2a Match Date", Now.ToString("yyyy MMM dd"), invoice2aMatchLink.AP_MatchDate.ToString("yyyy MMM dd"));

			AssertMatchLinkDefaults(payment2MatchLink);
			AssertMatchLinkDefaults(invoice2aMatchLink);

			AssertAPInvoiceShowsAsPaid(creditor2Inv2);

			#endregion

			#region AUDPayment2 Matching

			AssertEquals("Group 3 Match Count", 2, group3.Count);
			TransactionMatchLink payment3MatchLink = group3.FindMatchLinkByTransactionHeaderAndAmount(aUDPayment2, 165M);
			TransactionMatchLink invoice3aMatchLink = group3.FindMatchLinkByTransactionHeaderAndAmount(creditor1Inv3, -165M);

			AssertNotNull("Payment3 Match Link Found", payment3MatchLink);
			AssertNotNull("Invoice3a Match Link Found", invoice3aMatchLink);

			AssertInvoiceAndPaymentAreInSameGroup(group3, invoice3aMatchLink, payment3MatchLink);

			AssertEquals("Payment3 Match Date", Now.ToString("yyyy MMM dd"), payment3MatchLink.AP_MatchDate.ToString("yyyy MMM dd"));
			AssertEquals("Invoice3a Match Date", Now.ToString("yyyy MMM dd"), invoice3aMatchLink.AP_MatchDate.ToString("yyyy MMM dd"));

			AssertMatchLinkDefaults(payment3MatchLink);
			AssertMatchLinkDefaults(invoice3aMatchLink);

			AssertAPInvoiceShowsAsPaid(creditor1Inv3);

			#endregion

			#region USDPayment2 Matching

			AssertEquals("Group 4 Match Count", 2, group4.Count);
			TransactionMatchLink payment4MatchLink = group4.FindMatchLinkByTransactionHeaderAndAmount(uSDPayment2, 392.85M);
			TransactionMatchLink invoice4aMatchLink = group4.FindMatchLinkByTransactionHeaderAndAmount(creditor2Inv3, -392.85M);

			AssertNotNull("Payment4 Match Link Found", payment4MatchLink);
			AssertNotNull("Invoice4a Match Link Found", invoice4aMatchLink);

			AssertInvoiceAndPaymentAreInSameGroup(group4, invoice4aMatchLink, payment4MatchLink);

			AssertEquals("Payment4 Match Date", Now.ToString("yyyy MMM dd"), payment4MatchLink.AP_MatchDate.ToString("yyyy MMM dd"));
			AssertEquals("Invoice4a Match Date", Now.ToString("yyyy MMM dd"), invoice4aMatchLink.AP_MatchDate.ToString("yyyy MMM dd"));

			AssertMatchLinkDefaults(payment4MatchLink);
			AssertMatchLinkDefaults(invoice4aMatchLink);

			AssertAPInvoiceShowsAsPaid(creditor2Inv3);

			#endregion

			#region Apportionment Matching

			AssertEquals("Group 5 Match Count", 2, group5.Count);
			TransactionMatchLink payment5MatchLink = group5.FindMatchLinkByTransactionHeaderAndAmount(apportionmentPayment, 698.50M);
			TransactionMatchLink invoice5aMatchLink = group5.FindMatchLinkByTransactionHeaderAndAmount(creditor3Inv3, -698.50M);

			AssertNotNull("Payment5 Match Link Found", payment5MatchLink);
			AssertNotNull("Invoice5a Match Link Found", invoice5aMatchLink);

			AssertInvoiceAndPaymentAreInSameGroup(group5, invoice5aMatchLink, payment5MatchLink);

			AssertEquals("Payment5 Match Date", Now.ToString("yyyy MMM dd"), payment5MatchLink.AP_MatchDate.ToString("yyyy MMM dd"));
			AssertEquals("Invoice5a Match Date", Now.ToString("yyyy MMM dd"), invoice5aMatchLink.AP_MatchDate.ToString("yyyy MMM dd"));

			AssertMatchLinkDefaults(payment5MatchLink);
			AssertMatchLinkDefaults(invoice5aMatchLink);

			AssertAPInvoiceShowsAsPaid(creditor3Inv3);

			#endregion

			#region WIP and Accrual Reversal Assertions

			AssertEquals("Charge 1_1 WIP Reversed", true, charge1_1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_2 WIP Reversed", true, charge1_2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_3 WIP Reversed", true, charge1_3WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_4 WIP Reversed", true, charge1_4WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_5 WIP Reversed", true, charge1_5WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_6 WIP Reversed", true, charge1_6WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_7 WIP Reversed", true, charge1_7WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_1 WIP Reversed", true, charge2_1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_2 WIP Reversed", true, charge2_2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_3 WIP Reversed", true, charge2_3WIP.AL_ReverseDate.IsValid);

			AssertEquals("Charge 1_1 Accrual Reversed", true, charge1_1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_2 Accrual Reversed", true, charge1_2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_3 Accrual Reversed", true, charge1_3Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_5 Accrual Reversed", true, charge1_5Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_6 Accrual Reversed", true, charge1_6Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_7 Accrual Reversed", true, charge1_7Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_1 Accrual Reversed", true, charge2_1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_2 Accrual Reversed", true, charge2_2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_3 Accrual Reversed", true, charge2_3Accrual.AL_ReverseDate.IsValid);

			#endregion

		}

		#endregion

		#region TEST: Can't post AR invoices from a job with invoicing on hold

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCannotPostARInvoicesFromJobWithInvoicingOnHold()
		{
			AccountingConfigurationRegistry.Instance.AllowForwardDatingofAPInvoiceDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Job job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			job.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			consol.JK_UniqueConsignRef = job.JH_JobNum;
			job.JH_Status = JobHeaderStatus.InvoiceOnHold.Code;
			CreateExchangeRate(job, USD, .7M);

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);

			Charge charge1 = CreateCharge(job, CC1, "Charge code 1 tries to make an AP/AR invoice and an AP payment and a CFX line.", USD, 100M, Creditor1, USD, 800M, LocalClient);
			Charge charge2 = CreateCharge(job, CC8, "Charge code 2 tries to make an AP/AR credit note.", AUD, -50M, Creditor3, AUD, -50M, Agent);

			USDBankAccount.AB_ChequeNumDigits = 5;
			ChequeBook.AK_AB = USDBankAccount.PK;

			SetAPInvoiceInfo(charge1, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPPaymentInfo(charge1, ReceiptTypes.Cheque, USDBankAccount, "1");
			SetAPInvoiceInfo(charge2, "3", Now.AddDays(10), Now.AddDays(20));

			charge1.JR_PaymentType = "CHQ";
			charge2.JR_PaymentType = "CHQ";
			charge1.JR_AB = USDBankAccount.PK;
			charge2.JR_AB = AUDBankAccount.PK;
			charge1.JR_AK = ChequeBook.PK;
			charge2.JR_AK = AUDChequeBook.PK;
			charge1.JR_ChequeNo = "1";
			charge2.JR_ChequeNo = "101";

			AssertEquals(false, charge1.HasErrors);
			AssertEquals(false, charge2.HasErrors);
			Factory.Save();

			var jobs = new[] { job };
			ConsolInvoicingPostManager postManager = new ConsolInvoicingPostManager(Factory, jobs, consol, apps);

			TransactionCreatorHashtable transactions = postManager.CreateTransactions(JobInvoicingPostingOption.All);
			string generatedTransactions = GetContentsStringPayable(transactions);

			string message = "No invoices should have been created, because the Job is on hold. " + generatedTransactions;

			message = "On-hold status should prohibit invoices, but no other transaction type. " + generatedTransactions;
			AssertEquals(message, true, transactions.ContainsAPInvoice(Creditor1.OH_Code, charge1.JR_APInvoiceNum)); // charge 1
			AssertNotNull(message, transactions.RetrieveAPPaymentApproval(Creditor1.OH_Code, USDBankAccount.AB_Code, ReceiptTypes.Cheque, "00001", job.JH_JobNum)); // charge 1
			AssertEquals(message, true, transactions.ContainsAPCreditNote(Creditor3.OH_Code, charge2.JR_APInvoiceNum)); // charge 2
			AssertEquals("Expected exactly 2 transactions (+ 1 Payment Approval) to be created. " + generatedTransactions, 2, transactions.Count);
		}

		#endregion

		#region TEST: Can't post AR/AP invoices from a job with work on hold

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCannotPostInvoicesFromJobWithWorkOnHold()
		{
			AccountingConfigurationRegistry.Instance.AllowForwardDatingofAPInvoiceDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			ForwardingConsol consol = CreateConsol("AUSYD", "USLAX", "Y00001000");
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);

			Job jobOnHold = CreateJob("Z00001000", LocalClient, 0M, Agent, 0M);
			jobOnHold.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			jobOnHold.JH_Status = JobHeaderStatus.WorkOnHold.Code;

			Charge charge1_1 = CreateCharge(jobOnHold, CC1, "Job 1: Charge 1 tries to make an AP and AR invoice and an AP payment.", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge1_2 = CreateCharge(jobOnHold, CC8, "Job 1: Charge 2 tries to make an AP and AR credit note.", AUD, -50M, Creditor3, AUD, -50M, Agent);

			AUDBankAccount.AB_ChequeNumDigits = 5;
			SetAPInvoiceInfo(charge1_1, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPPaymentInfo(charge1_1, ReceiptTypes.Cheque, AUDBankAccount, "1");
			SetAPInvoiceInfo(charge1_2, "2", Now.AddDays(10), Now.AddDays(20));

			charge1_1.JR_PaymentType = "CHQ";
			charge1_2.JR_PaymentType = "CHQ";
			charge1_1.JR_AB = AUDBankAccount.PK;
			charge1_2.JR_AB = AUDBankAccount.PK;
			charge1_1.JR_AK = AUDChequeBook.PK;
			charge1_2.JR_AK = AUDChequeBook.PK;
			charge1_1.JR_ChequeNo = "3";
			charge1_2.JR_ChequeNo = "4";

			AssertEquals(false, charge1_1.HasErrors);
			AssertEquals(false, charge1_2.HasErrors);
			AssertEquals(false, jobOnHold.HasErrors);

			Job jobWorking = CreateJob("Z00001001", LocalClient, 0M, Agent, 0M);
			jobWorking.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			jobWorking.JH_Status = JobHeaderStatus.Working.Code;

			Charge charge2_1 = CreateCharge(jobWorking, CC1, "Job 2: Charge 1 tries to make an AP and AR invoice and an AP payment.", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge2_2 = CreateCharge(jobWorking, CC8, "Job 2: Charge 2 tries to make an AP and AR credit note.", AUD, -50M, Creditor3, AUD, -50M, Agent);

			SetAPInvoiceInfo(charge2_1, "3", Now.AddDays(10), Now.AddDays(20));
			SetAPPaymentInfo(charge2_1, ReceiptTypes.Cheque, AUDBankAccount, "2");
			SetAPInvoiceInfo(charge2_2, "4", Now.AddDays(10), Now.AddDays(20));

			charge2_1.JR_PaymentType = "CHQ";
			charge2_2.JR_PaymentType = "CHQ";
			charge2_1.JR_AB = AUDBankAccount.PK;
			charge2_2.JR_AB = AUDBankAccount.PK;
			charge2_1.JR_AK = AUDChequeBook.PK;
			charge2_2.JR_AK = AUDChequeBook.PK;
			charge2_1.JR_ChequeNo = "5";
			charge2_2.JR_ChequeNo = "6";

			AssertEquals(false, charge2_1.HasErrors);
			AssertEquals(false, charge2_2.HasErrors);
			AssertEquals(false, jobWorking.HasErrors);

			Factory.Save();

			var jobs = new[] { jobOnHold, jobWorking };
			ConsolInvoicingPostManager postManager = new ConsolInvoicingPostManager(Factory, jobs, consol, apps);

			TransactionCreatorHashtable transactions = postManager.CreateTransactions(JobInvoicingPostingOption.All);

			string message = "Working job should have posted normally. " + GetContentsStringPayable(transactions);
			AssertEquals(message, true, transactions.ContainsAPInvoice(Creditor1.OH_Code, "3"));
			AssertEquals(message, true, postManager.Poster.ContainsInvoice(AUD, LocalClient));
			AssertEquals(message, true, transactions.ContainsAPPaymentApproval(Creditor1.OH_Code, AUDBankAccount.AB_Code, ReceiptTypes.Cheque, "00005", jobWorking.JH_JobNum));
			AssertEquals(message, true, transactions.ContainsAPCreditNote(Creditor3.OH_Code, "4"));
			AssertEquals(message, true, postManager.Poster.ContainsInvoice(AUD, Agent));

			AssertEquals("2 (+ 1 Payment Approval) Payables transactions should have been created." + GetContentsStringPayable(transactions), 2, transactions.APTransactionsCount);
			AssertEquals("Receivable Transactions Count", 2, transactions.ARTransactionsCount);
		}

		#endregion

		#region TEST: Can't post AR invoices from a job with empty Profit Loss reason when it is required

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCannotPostARInvoicesFromJobWithEmptyProfitLossReason()
		{
			ForwardingConsol consol = CreateConsol("AUSYD", "USLAX", "Y00001000");
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);

			Job jobWithoutProfitLossReason = CreateJob("Z00001000", LocalClient, 0M, Agent, 0M);
			jobWithoutProfitLossReason.JH_ProfitLossReasonCode = "";
			Charge charge1 = CreateCharge(jobWithoutProfitLossReason, CC1, "Job 1: Charge 1 tries to make an AP and AR invoicet.", AUD, 100M, Creditor1, AUD, 150M, LocalClient);

			AssertEquals(false, charge1.HasErrors);
			AssertEquals(false, jobWithoutProfitLossReason.HasErrors);

			Factory.Save();

			JobProfitLossReasonCodeCollection plReasonCodes = new JobProfitLossReasonCodeCollection();
			JobProfitLossReasonCode plReasonCode = plReasonCodes.AddNew();
			plReasonCode.Code = "TST";
			plReasonCode.Description = (NoResString)"Test";
			AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, plReasonCodes);

			TestObjectCreator.SetupRegistrySetJobStatusToInvoicedWhenFirstARInvoicePosted(GlbCompany.CurrentCompany.PK.ToGuid(), null);

			var value = AccountingConfigurationRegistry.Instance.SetJobStatusToInvoicedWhenFirstARInvoicePosted.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			foreach (CodeDescriptionBool item in value)
			{
				AssertEquals("Is status changed to Invoiced on posting first AR Invoice when Job Status: " + item.Code, false, item.Bool);
			}

			JobProfitLossRequiringReasonParameters plRequiringReasonParameters = new JobProfitLossRequiringReasonParameters();
			plRequiringReasonParameters.ProfitThreshold = 10M;
			plRequiringReasonParameters.JobStatusCollection.AddNew().Code = JobHeaderStatus.JobInvoiced.Code;
			AccountingConfigurationRegistry.Instance.JobProfitLossRequiringReasonParameters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, plRequiringReasonParameters);

			var jobs = new[] { jobWithoutProfitLossReason };

			ConsolInvoicingPostManager postManager = new ConsolInvoicingPostManager(Factory, jobs, consol, apps);
			postManager.OnCriticalPostError += new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_OnCriticalPostError);

			TransactionCreatorHashtable transactions = postManager.CreateTransactions(JobInvoicingPostingOption.All);
			Assert("Posting should be cancelled", postManager.CancelPosting);
			Assert("Critical Post Error Event should be raised", IsOnCriticalPostErrorEventRaised);

			postManager.OnCriticalPostError -= new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_OnCriticalPostError);
		}

		#endregion

		#region TEST: Can't post an apportionment if any shipment in its consol is on hold

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCannotPostApportionmentIfConsolContainsShipmentOnHold()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();

			string consolNumber = "C00001000";
			// create a consol
			ForwardingConsol consol = CreateConsol("AUSYD", "USLAX", consolNumber);
			consol.JK_UniqueConsignRef = consolNumber;
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = CC1.PK;
			cost.E6_OH_Creditor = Creditor1.PK;
			cost.E6_InvoiceNum = "1";
			cost.E6_InvoiceDate = Now;
			cost.E6_PaymentDate = Now.AddDays(1);
			ZGuid costSplitGroup = cost.PK;
			Factory.Save();
			// add a shipment that is NOT on hold
			Job jobWorking = CreateJob("Z00001000", LocalClient, 0M, Agent, 0M);
			jobWorking.JH_GE = TestObjectCreator.FESDepartment.PK;
			jobWorking.JH_Status = JobHeaderStatus.Working.Code;
			Charge chargeWorking = CreateCharge(jobWorking, CC1, "Charge for working job", AUD, 1, Creditor1, AUD, 1m, LocalClient);
			chargeWorking.JR_E6 = costSplitGroup;
			SetAPInvoiceInfo(chargeWorking, "1", ZDateTime.Now, ZDateTime.Now.AddDays(1));
			SetAPPaymentInfo(chargeWorking, ReceiptTypes.Cash, AUDBankAccount, "CASH1");
			jobWorking.Charges.Add(chargeWorking);
			AssertEquals(false, chargeWorking.HasErrors);
			AssertEquals(false, jobWorking.HasErrors);

			// add a shipment that is on hold
			Job jobOnHold = CreateJob("Z00001001", LocalClient, 0M, Agent, 0M);
			jobOnHold.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			jobOnHold.JH_Status = JobHeaderStatus.WorkOnHold.Code;
			Charge chargeOnHold = CreateCharge(jobOnHold, CC2, "Charge for job on hold", AUD, 2, Creditor1, AUD, 2m, LocalClient);
			chargeOnHold.JR_E6 = costSplitGroup;
			SetAPInvoiceInfo(chargeOnHold, "1", ZDateTime.Now, ZDateTime.Now.AddDays(1));
			SetAPPaymentInfo(chargeOnHold, ReceiptTypes.Cash, AUDBankAccount, "CASH1");
			jobOnHold.Charges.Add(chargeOnHold);
			AssertEquals(false, chargeOnHold.HasErrors);
			AssertEquals(false, jobOnHold.HasErrors);

			// post and save
			var jobs = new[] { jobWorking, jobOnHold };
			ConsolInvoicingPostManager creator = new ConsolInvoicingPostManager(Factory, jobs, consol, apps);
			Factory.Save();
			TransactionCreatorHashtable transactions = creator.CreateTransactions(JobInvoicingPostingOption.All);
			Factory.Save();

			AssertEquals("Apportionment for consol containing a shipment on hold should not post. " + GetContentsStringPayable(transactions), 0, transactions.APTransactionsCount);
			AssertEquals("Receivable Transactions Count", 1, transactions.ARTransactionsCount);

			// re-post with working status
			jobOnHold.JH_Status = JobHeaderStatus.Working.Code;
			Factory.Save();
			creator = new ConsolInvoicingPostManager(Factory, jobs, consol, apps);
			transactions = creator.CreateTransactions(JobInvoicingPostingOption.All);
			AssertEquals("Should have 1 AP payment, 1 AP invoice " + GetContentsStringPayable(transactions), 2, transactions.APTransactionsCount + transactions.GetAllAPPaymentApprovals().Length);
			AssertEquals("Receivable Transactions Count", 1, transactions.ARTransactionsCount);
		}

		#endregion

		#region TEST: Apportioned charges with some jobs having invoicing on hold are AP-posted and partially AR-posted.

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestPostApportionedChargeWhenConsolContainsShipmentWithInvoicingOnHold()
		{
			// create a consol
			ForwardingConsol consol = CreateConsol("AUSYD", "USLAX", "Y00001000");

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = CC1.PK;
			cost.E6_OH_Creditor = Creditor1.PK;
			cost.E6_InvoiceNum = "1";
			cost.E6_InvoiceDate = ZDateTime.Now;
			cost.E6_PaymentDate = ZDateTime.Now.AddDays(1);
			ZGuid costSplitGroup = cost.PK;
			ZGuid consolID = consol.PK;
			Factory.Save();
			// add a shipment for which invoicing is NOT on hold
			Job jobWorking = CreateJob("Z00001000", LocalClient, 0M, Agent, 0M);
			jobWorking.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			jobWorking.JH_Status = JobHeaderStatus.Working.Code;
			Charge chargeWorking = CreateCharge(jobWorking, CC1, "Charge for working job", AUD, 1, Creditor1, AUD, 1m, LocalClient);
			chargeWorking.JR_E6 = costSplitGroup;
			SetAPInvoiceInfo(chargeWorking, "1", ZDateTime.Now, ZDateTime.Now.AddDays(1));
			jobWorking.Charges.Add(chargeWorking);
			AssertEquals(false, chargeWorking.HasErrors);
			AssertEquals(false, jobWorking.HasErrors);

			// add a shipment that has invoicing on hold
			Job jobWithInvoicingOnHold = CreateJob("Z00001001", LocalClient, 0M, Agent, 0M);
			jobWithInvoicingOnHold.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			jobWithInvoicingOnHold.JH_Status = JobHeaderStatus.InvoiceOnHold.Code;
			Charge chargeWithInvoicingOnHold = CreateCharge(jobWithInvoicingOnHold, CC2, "Charge for job on hold", AUD, 3, Creditor1, AUD, 3m, LocalClient);
			chargeWithInvoicingOnHold.JR_E6 = costSplitGroup;
			SetAPInvoiceInfo(chargeWithInvoicingOnHold, "1", ZDateTime.Now, ZDateTime.Now.AddDays(1));
			jobWithInvoicingOnHold.Charges.Add(chargeWithInvoicingOnHold);
			AssertEquals(false, chargeWithInvoicingOnHold.HasErrors);
			AssertEquals(false, jobWithInvoicingOnHold.HasErrors);

			// post and save
			var jobs = new Job[] { jobWorking, jobWithInvoicingOnHold };
			ConsolInvoicingPostManager creator = new ConsolInvoicingPostManager(Factory, jobs, consol, apps);
			Factory.Save();

			TransactionCreatorHashtable transactions = creator.CreateTransactions(JobInvoicingPostingOption.All);
			string generatedTransactions = GetContentsStringPayable(transactions);

			// assertions
			APInvoice aP = transactions.RetrieveAPInvoice(Creditor1.OH_Code, chargeWithInvoicingOnHold.JR_APInvoiceNum);
			AssertNotNull("An AP invoice should have been created. " + generatedTransactions, aP);

			InvoicingBase[] aRs = creator.Poster.GetInvoices(AUD, LocalClient, jobWorking.JH_JobNum);
			AssertEquals(1, aRs.Length);
			ARInvoice aR = (ARInvoice)aRs[0];
			AssertNotNull("An AR invoice should have been created. " + generatedTransactions, aR);

			AssertEquals(generatedTransactions, chargeWorking.JR_LocalCostAmt + chargeWithInvoicingOnHold.JR_LocalCostAmt, aP.AH_LocalExTaxAmount);
			AssertEquals(generatedTransactions, chargeWorking.JR_LocalSellAmt, aR.AH_LocalExTaxAmount);

			AssertEquals(generatedTransactions, true, aP.Lines.Contains(chargeWorking.JR_AL_APLine));
			AssertEquals(generatedTransactions, true, aP.Lines.Contains(chargeWithInvoicingOnHold.JR_AL_APLine));
			AssertEquals(generatedTransactions, true, aR.Lines.Contains(chargeWorking.JR_AL_ARLine));

			AssertEquals("Expected exactly 1 payable transaction to be created. " + generatedTransactions, 1, transactions.APTransactionsCount);
			AssertEquals("Receivable Transactions Count", 1, transactions.ARTransactionsCount);

			// re-post with working status
			jobWithInvoicingOnHold.JH_Status = JobHeaderStatus.Working.Code;

			cost.E6_OSCostAmount = 4m;
			cost.E6_LocalCostAmount = 4m;
			cost.E6_OSGSTAmount_Calc = 0.4m;

			Factory.Save();
			transactions = creator.CreateTransactions(JobInvoicingPostingOption.All);

			aRs = creator.Poster.GetInvoices(AUD, LocalClient, jobWithInvoicingOnHold.JH_JobNum);
			AssertEquals(1, aRs.Length);
			aR = (ARInvoice)aRs[0];
			AssertNotNull("An AR invoice should have been created. " + generatedTransactions, aR);

			AssertEquals("Expected no payable transaction to be created. " + GetContentsStringPayable(transactions), 0, transactions.APTransactionsCount);
			AssertEquals("Receivable Transactions Count", 1, transactions.ARTransactionsCount);
		}

		#endregion

		#region TEST: Apportioned credit notes with some jobs having invoicing on hold are AP-posted and partially AR-posted.

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestPostApportionedCreditNoteWhenConsolContainsShipmentWithInvoicingOnHold()
		{
			// create a consol
			ForwardingConsol consol = CreateConsol("AUSYD", "USLAX", "Y00001000");
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();

			Job jobWorking = new Job.Loader(Factory, shipment1).TryLoadOrCreateWithMutex();
			jobWorking.JH_GE = GlbDepartment.CurrentDepartment.PK;
			jobWorking.JH_Status = JobHeaderStatus.Working.Code;
			jobWorking.LocalChargesPK = LocalClient.PK;
			jobWorking.AgentCollectPK = Agent.PK;
			Job jobWithInvoicingOnHold = new Job.Loader(Factory, shipment2).TryLoadOrCreateWithMutex();
			jobWithInvoicingOnHold.JH_Status = JobHeaderStatus.InvoiceOnHold.Code;
			jobWithInvoicingOnHold.JH_GE = GlbDepartment.CurrentDepartment.PK;
			jobWithInvoicingOnHold.LocalChargesPK = LocalClient2.PK;
			jobWithInvoicingOnHold.AgentCollectPK = Agent.PK;

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = CC1.PK;
			ZGuid costSplitGroup = cost.PK;
			cost.E6_OSCostAmount = -100m;
			cost.E6_ApportionmentMethod = "SHP";
			cost.E6_OH_Creditor = Creditor1.PK;
			cost.E6_InvoiceNum = "x2222";
			cost.E6_InvoiceDate = ZDateTime.Now;
			cost.E6_PaymentDate = ZDateTime.Now;
			Factory.Save();

			ApportionSplitCharge chargeWorking = (ApportionSplitCharge)cost.ApportionmentCharges.Find(new ZQuery(JobChargeSchema.JR_JH, jobWorking.PK))[0];
			ApportionSplitCharge chargeWithInvoicingOnHold = (ApportionSplitCharge)cost.ApportionmentCharges.Find(new ZQuery(JobChargeSchema.JR_JH, jobWithInvoicingOnHold.PK))[0];

			AssertNotNull(chargeWorking);
			AssertNotNull(chargeWithInvoicingOnHold);

			Factory.Save();

			JobCollection jobs = new JobCollection(new BusinessObjectFactory());
			jobs.Load();

			ConsolInvoicingPostManager creator = new ConsolInvoicingPostManager(Factory, jobs.Cast<Job>(), consol, apps);

			TransactionCreatorHashtable transactions = creator.CreateTransactions(JobInvoicingPostingOption.All);
			string generatedTransactions = GetContentsStringPayable(transactions);

			// assertions
			APCreditNote aPNote = transactions.RetrieveAPCreditNote(Creditor1.OH_Code, chargeWithInvoicingOnHold.JR_APInvoiceNum);
			AssertNotNull(generatedTransactions, aPNote);
			Assert(!aPNote.IsDeleted);
			InvoicingBase[] aRNotes = creator.Poster.GetInvoices(AUD, LocalClient);
			AssertEquals(1, aRNotes.Length);
			ARCreditNote aRNote = (ARCreditNote)aRNotes[0];

			chargeWorking = jobs.Factory.Load<ApportionSplitCharge>(chargeWorking.PK);
			chargeWithInvoicingOnHold = jobs.Factory.Load<ApportionSplitCharge>(chargeWithInvoicingOnHold.PK);

			AssertEquals(generatedTransactions, -1 * (chargeWorking.JR_LocalCostAmt + chargeWithInvoicingOnHold.JR_LocalCostAmt), aPNote.AH_LocalExTaxAmount);
			AssertEquals(generatedTransactions, -1 * chargeWorking.JR_LocalSellAmt, aRNote.AH_LocalExTaxAmount);

			AssertNotNull(aPNote.Lines.Find(new ZQuery(AccTransactionLinesSchema.PK, chargeWorking.JR_AL_APLine))[0]);
			AssertNotNull(aRNote.Lines.Find(new ZQuery(AccTransactionLinesSchema.PK, chargeWorking.JR_AL_ARLine))[0]);
			AssertNotNull(aPNote.Lines.Find(new ZQuery(AccTransactionLinesSchema.PK, chargeWithInvoicingOnHold.JR_AL_APLine))[0]);
			AssertEquals(0, aRNote.Lines.Find(new ZQuery(AccTransactionLinesSchema.PK, chargeWithInvoicingOnHold.JR_AL_ARLine)).Length);

			AssertEquals("Expected exactly 1 payables transactions to be created. " + generatedTransactions, 1, transactions.APTransactionsCount);
			AssertEquals("Receivable Transactions Count", 1, transactions.ARTransactionsCount);

			Factory.Save();
		}

		#endregion

		#region TEST: mixture of branch posting group is not allowed
		public void TestMixtureOfBranchPostingGroupNotAllowed()
		{
			var branch1 = TestObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("BBB", GlbCompany.CurrentCompany);

			var settings1 = new BranchGroupSettings();
			settings1.BranchPK = branch1.PK;
			settings1.GroupNumber = 1;
			settings1.IsParentBranch = true;

			var settings2 = new BranchGroupSettings();
			settings2.BranchPK = branch2.PK;
			settings2.GroupNumber = 2;
			settings2.IsParentBranch = true;

			var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
			branchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings1);
			branchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings2);

			AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

			string consolNumber = "C00001000";
			ForwardingConsol consol = CreateConsol("AUSYD", "USLAX", consolNumber);
			Job job1 = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			job1.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);

			Charge charge1_1 = CreateCharge(job1, CC1, "Job 1 Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, Agent);
			charge1_1.JR_GB = branch1.PK;

			Job job2 = CreateJob("Z00001001", LocalClient, 5M, Agent, 10M);
			job2.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			Charge charge2_1 = CreateCharge(job2, CC1, "Job 2 Charge Code 1", AUD, 300M, Creditor2, AUD, 350M, Agent);
			charge2_1.JR_GB = branch2.PK;

			var jobs = new[] { job1, job2 };

			ConsolInvoicingPostManager testPostManager = new ConsolInvoicingPostManager(Factory, jobs, consol, apps);
			testPostManager.OnCriticalPostError += new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_OnCriticalPostError);

			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			TransactionCreatorHashtable transactions = testPostManager.CreateTransactions(JobInvoicingPostingOption.Agent);
			AssertEquals("Receivable Transactions Count", 0, transactions.ARTransactionsCount);
		}

		#endregion
		#region TEST: Profit share and export master freight collect

		public void TestProfitSharePostedForNewlyAddedShipmentsOnly()
		{
			#region Setup

			CreateGroupWithPostingStyle(TestObjectCreator.AALSHI, InvoicePostingOptionsList.Codes.FinalInvoiceOnly);

			TestObjectCreator.Agent.CompanyData.OB_IsCreditor = ZBool.True;
			TestObjectCreator.Agent.CompanyData.OB_IsDebtor = ZBool.True;
			TestObjectCreator.Agent.CompanyData.SetAPTaxApplicable(ZBool.True);
			TestObjectCreator.Agent.CompanyData.SetARTaxApplicable(ZBool.True);
			TestObjectCreator.Agent.MiscServ.OM_ARWHTApplicable = ZBool.False;
			OrgHeader agent1 = TestObjectCreator.CreateOrgHeader("AGENT1", true, true);
			OrgHeader agent2 = TestObjectCreator.CreateOrgHeader("AGENT2", true, true);
			OrgHeader agent3 = TestObjectCreator.CreateOrgHeader("AGENT3", true, true);

			Factory.Save();

			ZString transportMode = "AIR";
			ZString origin = "AUSYD";
			ZString destination = "USLAX";

			SetupProfitShareRelationship(GlbCompany.CurrentCompany.OrgProxy, agent1, 60m, 40m, origin, destination, transportMode);
			SetupProfitShareRelationship(GlbCompany.CurrentCompany.OrgProxy, agent2, 60m, 40m, origin, destination, transportMode);
			SetupProfitShareRelationship(GlbCompany.CurrentCompany.OrgProxy, agent3, 60m, 40m, origin, destination, transportMode);

			Factory.Save();

			SetupConsolAndShipments(transportMode, origin, destination, TestObjectCreator.Agent);
			Shipment1.JS_OH_DeliveryAgent = agent1.PK;
			Shipment2.JS_OH_DeliveryAgent = agent2.PK;

			Consol.JK_PrepaidCollect = Enterprise.Core.Constants.PaymentType.Collect;
			Consol.JK_RL_NKLoadPort = origin;
			Consol.JK_RL_NKDischargePort = destination;
			Consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);
			Consol.SetDefaultReceivingForwarderAddress(TestObjectCreator.Agent);

			ApportionmentListing apps = new ApportionmentListing(Factory, Consol);
			apps.IsActivated = true;
			apps.LoadChildShipmentsAndAcquireMutexesWhereRequired();
			JobConsolCost freightCost = apps.CostsCollection.TryAddNew();
			freightCost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			freightCost.E6_RX_NKCurrency = TestObjectCreator.USD.RX_Code;
			freightCost.E6_ExchangeRate = 0.65m;
			freightCost.E6_OSCostAmount = 200m;
			freightCost.E6_ApportionmentMethod = "SHP";
			freightCost.E6_OH_Creditor = TestObjectCreator.Agent.PK;
			freightCost.E6_InvoiceNum = "xawecadfa";
			freightCost.E6_InvoiceDate = ZDateTime.Now;

			Factory.Save();

			Job shipment1Job = new Job.Loader(Factory, Shipment1).Load();
			Job shipment2Job = new Job.Loader(Factory, Shipment2).Load();
			shipment1Job.JH_GE = ZGuid.Empty;
			shipment1Job.JH_GE = TestObjectCreator.FEADepartment.PK;
			shipment2Job.JH_GE = ZGuid.Empty;
			shipment2Job.JH_GE = TestObjectCreator.FEADepartment.PK;
			shipment1Job.PlugInData = Shipment1;
			shipment2Job.PlugInData = Shipment2;

			Charge shipment1Charge = shipment1Job.Charges.AddNew();
			Charge shipment2Charge = shipment2Job.Charges.AddNew();

			shipment1Charge.JR_AC = Env.Registry.FreightChargeCode;
			shipment1Charge.JR_IsIncludedInProfitShare = true;
			shipment1Charge.JR_OSSellAmt = 180m;
			shipment1Charge.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment1Charge.JR_OSCostAmt = 80m;
			shipment1Charge.JR_AgentDeclaredSellAmt = 150m;
			shipment1Charge.JR_AgentDeclaredCostAmt = 100m;

			shipment2Charge.JR_AC = Env.Registry.FreightChargeCode;
			shipment2Charge.JR_IsIncludedInProfitShare = true;
			shipment2Charge.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment2Charge.JR_OSSellAmt = 200m;
			shipment2Charge.JR_OSCostAmt = 100m;
			shipment2Charge.JR_LocalCostAmt = 100m;
			shipment2Charge.JR_AgentDeclaredSellAmt = 180m;
			shipment2Charge.JR_AgentDeclaredCostAmt = 120m;

			Factory.Save();
			apps.ReleaseMutexes();

			#endregion

			try
			{
				JobCollection jobs = new JobCollection(Factory);
				jobs.Load();

				ConsolInvoicingPostManager testPostManager = new ConsolInvoicingPostManager(Factory, jobs.Cast<Job>(), Consol, apps);
				testPostManager.ExportAgentPosting += TestPostManager_ExportAgentPosting_TestCreditProfitShareWhenPostingAgentInvoices;
				testPostManager.ProfitShareConfirmation += TestPostManager_ProfitShareConfirmation;
				testPostManager.CreateTransactions(JobInvoicingPostingOption.Agent);

				JobConsolCostCollection costCollection = new JobConsolCostCollection(Factory, Consol);
				costCollection.Load();
				AssertEquals("CostCollection.Count", 3, costCollection.Count);

				ForwardingShipment shipment3 = Consol.Shipments.AddNew();
				shipment3.JS_TransportMode = transportMode;
				shipment3.JS_RL_NKOrigin = origin;
				shipment3.JS_RL_NKDestination = destination;
				shipment3.JS_OH_DeliveryAgent = agent3.PK;

				apps = new ApportionmentListing(Factory, Consol);
				apps.IsActivated = true;
				apps.LoadChildShipmentsAndAcquireMutexesWhereRequired();
				freightCost = apps.CostsCollection.TryAddNew();
				freightCost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
				freightCost.E6_RX_NKCurrency = TestObjectCreator.USD.RX_Code;
				freightCost.E6_ExchangeRate = 0.65m;
				freightCost.E6_OSCostAmount = 200m;
				freightCost.E6_ApportionmentMethod = "SHP";
				freightCost.E6_OH_Creditor = TestObjectCreator.Agent.PK;
				freightCost.E6_InvoiceNum = "xawecad10";
				freightCost.E6_InvoiceDate = ZDateTime.Now;

				Factory.Save();

				Job shipment3Job = new Job.Loader(Factory, shipment3).Load();
				shipment3Job.JH_GE = ZGuid.Empty;
				shipment3Job.JH_GE = TestObjectCreator.FEADepartment.PK;
				shipment3Job.PlugInData = shipment3;

				Charge shipment3Charge = shipment3Job.Charges.AddNew();
				shipment3Charge.JR_AC = Env.Registry.FreightChargeCode;
				shipment3Charge.JR_IsIncludedInProfitShare = true;
				shipment3Charge.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
				shipment3Charge.JR_OSSellAmt = 200m;
				shipment3Charge.JR_OSCostAmt = 100m;
				shipment3Charge.JR_AgentDeclaredSellAmt = 180m;
				shipment3Charge.JR_AgentDeclaredCostAmt = 120m;

				Factory.Save();
				apps.ReleaseMutexes();

				TransactionCreatorHashtable transactions = testPostManager.CreateTransactions(JobInvoicingPostingOption.Agent);

				InvoicingBase[] apInvoices = transactions.GetAllAPInvoicesAndCreditNotes();
				AssertEquals("Should be 1 AP invoice", 1, apInvoices.Length);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		[DisableZeroExchangeRateOverriding]
		public void TestCreditProfitShareWhenPostingAgentInvoices()
		{
			#region Setup
			ExchangeRateReader.GetReaderInstance().ClearCache();

			TestObjectCreator.Agent.CompanyData.OB_IsCreditor = ZBool.True;
			TestObjectCreator.Agent.CompanyData.OB_IsDebtor = ZBool.True;
			TestObjectCreator.Agent.CompanyData.SetAPTaxApplicable(ZBool.True);
			TestObjectCreator.Agent.CompanyData.SetARTaxApplicable(ZBool.True);
			TestObjectCreator.Agent.MiscServ.OM_ARWHTApplicable = ZBool.False;
			CreateGroupWithPostingStyle(TestObjectCreator.AALSHI, InvoicePostingOptionsList.Codes.FinalInvoiceOnly);
			Factory.Save();

			ZString transportMode = "AIR";
			ZString origin = "AUSYD";
			ZString destination = "USLAX";

			SetupProfitShareRelationship(GlbCompany.CurrentCompany.OrgProxy, TestObjectCreator.Agent, 60m, 40m, origin, destination, transportMode);

			Factory.Save();

			SetupConsolAndShipments(transportMode, origin, destination, TestObjectCreator.Agent);
			Consol.JK_PrepaidCollect = Enterprise.Core.Constants.PaymentType.Collect;
			Consol.JK_RL_NKLoadPort = origin;
			Consol.JK_RL_NKDischargePort = destination;
			Consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);
			Consol.SetDefaultReceivingForwarderAddress(TestObjectCreator.Agent);

			ApportionmentListing apps = new ApportionmentListing(Factory, Consol);
			apps.IsActivated = true;
			apps.LoadChildShipmentsAndAcquireMutexesWhereRequired();
			JobConsolCost freightCost = apps.CostsCollection.TryAddNew();
			freightCost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			freightCost.E6_RX_NKCurrency = TestObjectCreator.USD.RX_Code;
			freightCost.E6_OH_Creditor = TestObjectCreator.Agent.PK;
			freightCost.E6_ExchangeRate = 0.65m;
			freightCost.E6_OSCostAmount = 200m;
			freightCost.E6_ApportionmentMethod = "SHP";

			Factory.Save();

			Job shipment1Job = new Job.Loader(Factory, Shipment1).Load();
			Job shipment2Job = new Job.Loader(Factory, Shipment2).Load();
			shipment1Job.JH_GE = ZGuid.Empty;
			shipment1Job.JH_GE = TestObjectCreator.FEADepartment.PK;
			shipment2Job.JH_GE = ZGuid.Empty;
			shipment2Job.JH_GE = TestObjectCreator.FEADepartment.PK;
			shipment1Job.PlugInData = Shipment1;
			shipment2Job.PlugInData = Shipment2;

			Charge shipment1Charge = shipment1Job.Charges.AddNew();
			Charge shipment2Charge = shipment2Job.Charges.AddNew();

			shipment1Charge.JR_AC = Env.Registry.FreightChargeCode;
			shipment1Charge.JR_IsIncludedInProfitShare = true;
			shipment1Charge.JR_OSSellAmt = 180m;
			shipment1Charge.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment1Charge.JR_OSCostAmt = 80m;
			shipment1Charge.JR_AgentDeclaredSellAmt = 150m;
			shipment1Charge.JR_AgentDeclaredCostAmt = 100m;

			shipment2Charge.JR_AC = Env.Registry.FreightChargeCode;
			shipment2Charge.JR_IsIncludedInProfitShare = true;
			shipment2Charge.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment2Charge.JR_OSSellAmt = 200m;
			shipment2Charge.JR_OSCostAmt = 100m;
			shipment2Charge.JR_AgentDeclaredSellAmt = 180m;
			shipment2Charge.JR_AgentDeclaredCostAmt = 120m;

			Factory.Save();

			freightCost.E6_InvoiceNum = "xawecadfa";
			freightCost.E6_InvoiceDate = ZDateTime.Now;

			Factory.Save();

			apps.ReleaseMutexes();

			JobCollection jobs = new JobCollection(Factory);
			jobs.Load();

			#endregion

			ConsolInvoicingPostManager testPostManager = new ConsolInvoicingPostManager(Factory, jobs.Cast<Job>(), Consol, apps);
			testPostManager.ExportAgentPosting += new ExportAgentPostingEventHandler(TestPostManager_ExportAgentPosting_TestCreditProfitShareWhenPostingAgentInvoices);
			testPostManager.ProfitShareConfirmation += new ProfitShareConfirmationEventHandler(TestPostManager_ProfitShareConfirmation);
			TransactionCreatorHashtable transactions = testPostManager.CreateTransactions(JobInvoicingPostingOption.Agent);

			InvoicingBase[] aPInvoices = transactions.GetAllAPInvoicesAndCreditNotes();
			AssertEquals("Should be 2 AP invoices", 2, aPInvoices.Length);
			APInvoice invoice = (APInvoice)aPInvoices[0];
			AssertEquals("Invoice should be for zero", 0m, invoice.AH_OSExTaxAmount);
			AssertEquals("Invoice should have 4 lines", 4, invoice.Lines.Count);

			APInvoice pSInvoice = (APInvoice)aPInvoices[1];
			AssertEquals("Invoice should be for zero", 0m, invoice.AH_OSExTaxAmount);
			AssertEquals("Invoice should have 4 lines", 4, invoice.Lines.Count);

			InvoicingBaseCollection postedInvoices = testPostManager.Poster.PostedInvoices;
			AssertEquals("Should have posted 1 AR invoice", 1, postedInvoices.Count);
			ARInvoice agentInvoice = (ARInvoice)postedInvoices[0];
			AssertEquals("Should have been posted for the total amount less the profit share credit", 28.31m, agentInvoice.AH_OSExTaxAmount);
			AssertEquals("Should have 6 lines on invoice", 6, agentInvoice.Lines.Count);

			Assert(shipment1Charge.IsRevenuePosted);
			Assert(shipment2Charge.IsRevenuePosted);

			ZQuery pSCostQuery = new ZQuery(JobConsolCostSchema.E6_AC_ChargeCode, AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value);
			pSCostQuery.AddToFilter(JobConsolCostSchema.E6_ParentID, Consol.PK);
			pSCostQuery.AddToFilter(JobConsolCostSchema.E6_ParentTableCode, "JK");
			JobConsolCost pSCost = Factory.LoadTop1<JobConsolCost>(pSCostQuery);
			Assert(pSCost.E6_AH_APInvoice.IsValid);
			Assert(pSCost.E6_AH_ARInvoice.IsValid);

			Factory.Save();

			BusinessObjectFactory loadFactory = new BusinessObjectFactory();
			shipment1Charge = loadFactory.Load<Charge>(shipment1Charge.PK);
			shipment2Charge = loadFactory.Load<Charge>(shipment2Charge.PK);
			Assert("Revenue should be posted", shipment1Charge.IsRevenuePosted);
			Assert("Revenue should be posted", shipment2Charge.IsRevenuePosted);
			pSCost = loadFactory.Load<JobConsolCost>(pSCost.PK);
			Assert(!pSCost.ApportionmentCharges[0].JR_IsIncludedInProfitShare);
			Assert(!pSCost.ApportionmentCharges[1].JR_IsIncludedInProfitShare);
		}

		public void TestCreditProfitShareWhenPostingAgentInvoices_AR()
		{
			#region Setup

			TestObjectCreator.Agent.CompanyData.OB_IsCreditor = ZBool.True;
			TestObjectCreator.Agent.CompanyData.OB_IsDebtor = ZBool.True;
			TestObjectCreator.Agent.CompanyData.SetAPTaxApplicable(ZBool.True);
			TestObjectCreator.Agent.CompanyData.SetARTaxApplicable(ZBool.True);
			TestObjectCreator.Agent.MiscServ.OM_ARWHTApplicable = ZBool.False;
			CreateGroupWithPostingStyle(TestObjectCreator.AALSHI, InvoicePostingOptionsList.Codes.FinalInvoiceOnly);
			Factory.Save();

			ZString transportMode = "AIR";
			ZString origin = "AUSYD";
			ZString destination = "USLAX";

			SetupProfitShareRelationship(GlbCompany.CurrentCompany.OrgProxy, TestObjectCreator.Agent, 60m, 40m, origin, destination, transportMode);

			Factory.Save();

			SetupConsolAndShipments(transportMode, origin, destination, TestObjectCreator.Agent);
			Consol.JK_PrepaidCollect = Enterprise.Core.Constants.PaymentType.Collect;
			Consol.JK_RL_NKLoadPort = origin;
			Consol.JK_RL_NKDischargePort = destination;
			Consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);
			Consol.SetDefaultReceivingForwarderAddress(TestObjectCreator.Agent);

			ApportionmentListing apps = new ApportionmentListing(Factory, Consol);
			apps.IsActivated = true;
			apps.LoadChildShipmentsAndAcquireMutexesWhereRequired();
			JobConsolCost freightCost = apps.CostsCollection.TryAddNew();
			freightCost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			freightCost.E6_RX_NKCurrency = TestObjectCreator.USD.RX_Code;
			freightCost.E6_ExchangeRate = 0.65m;
			freightCost.E6_OSCostAmount = 200m;
			freightCost.E6_ApportionmentMethod = "SHP";
			freightCost.E6_OH_Creditor = TestObjectCreator.Agent.PK;
			freightCost.E6_InvoiceNum = "xawecadfa";
			freightCost.E6_InvoiceDate = ZDateTime.Now;

			Factory.Save();

			Job shipment1Job = new Job.Loader(Factory, Shipment1).Load();
			Job shipment2Job = new Job.Loader(Factory, Shipment2).Load();
			shipment1Job.PlugInData = Shipment1;
			shipment2Job.PlugInData = Shipment2;

			Charge shipment1Charge = shipment1Job.Charges.AddNew();
			Charge shipment2Charge = shipment2Job.Charges.AddNew();

			shipment1Charge.JR_AC = Env.Registry.FreightChargeCode;
			shipment1Charge.JR_IsIncludedInProfitShare = true;
			shipment1Charge.JR_OSSellAmt = 180m;
			shipment1Charge.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment1Charge.JR_OSCostAmt = 80m;
			shipment1Charge.JR_AgentDeclaredSellAmt = 150m;
			shipment1Charge.JR_AgentDeclaredCostAmt = 100m;

			shipment2Charge.JR_AC = Env.Registry.FreightChargeCode;
			shipment2Charge.JR_IsIncludedInProfitShare = true;
			shipment2Charge.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment2Charge.JR_OSSellAmt = 200m;
			shipment2Charge.JR_OSCostAmt = 100m;
			shipment2Charge.JR_AgentDeclaredSellAmt = 180m;
			shipment2Charge.JR_AgentDeclaredCostAmt = 120m;

			Factory.Save();
			apps.ReleaseMutexes();

			JobCollection jobs = new JobCollection(Factory);
			jobs.Load();

			#endregion

			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			ConsolInvoicingPostManager testPostManager = new ConsolInvoicingPostManager(Factory, jobs.Cast<Job>(), Consol, apps);
			testPostManager.ExportAgentPosting += new ExportAgentPostingEventHandler(TestPostManager_ExportAgentPosting_TestCreditProfitShareWhenPostingAgentInvoices);
			testPostManager.ProfitShareConfirmation += new ProfitShareConfirmationEventHandler(TestPostManager_ProfitShareConfirmation);
			testPostManager.CreateTransactions(JobInvoicingPostingOption.All);

			Charge[] shipment1PSCharges = (Charge[])shipment1Job.Charges.Find(new ZQuery(JobChargeSchema.JR_AC, AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value));
			AssertEquals(1, shipment1PSCharges.Length);
			AssertEquals(true, shipment1PSCharges[0].JR_IsRevenuePosted);

			Charge[] shipment2PSCharges = (Charge[])shipment2Job.Charges.Find(new ZQuery(JobChargeSchema.JR_AC, AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value));
			AssertEquals(1, shipment2PSCharges.Length);
			AssertEquals(true, shipment2PSCharges[0].JR_IsRevenuePosted);
		}

		public void TestCreditProfitShareWhenPostingAgentInvoices_AP()
		{
			#region Setup

			AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var profitShareChargeCode = TestObjectCreator.CreateChargeCode("PST");
			AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, profitShareChargeCode.PK.ToGuid());

			TestObjectCreator.Agent.CompanyData.OB_IsCreditor = ZBool.True;
			TestObjectCreator.Agent.CompanyData.OB_IsDebtor = ZBool.True;
			TestObjectCreator.Agent.CompanyData.SetAPTaxApplicable(ZBool.True);
			TestObjectCreator.Agent.CompanyData.SetARTaxApplicable(ZBool.True);
			TestObjectCreator.Agent.MiscServ.OM_ARWHTApplicable = ZBool.False;
			CreateGroupWithPostingStyle(TestObjectCreator.AALSHI, InvoicePostingOptionsList.Codes.FinalInvoiceOnly);
			Factory.Save();

			ZString transportMode = "AIR";
			ZString origin = "AUSYD";
			ZString destination = "USLAX";

			SetupProfitShareRelationship(GlbCompany.CurrentCompany.OrgProxy, TestObjectCreator.Agent, 60m, 40m, origin, destination, transportMode);

			Factory.Save();

			SetupConsolAndShipments(transportMode, origin, destination, TestObjectCreator.Agent);
			Consol.JK_PrepaidCollect = Enterprise.Core.Constants.PaymentType.Collect;
			Consol.JK_RL_NKLoadPort = origin;
			Consol.JK_RL_NKDischargePort = destination;
			Consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);
			Consol.SetDefaultReceivingForwarderAddress(TestObjectCreator.Agent);

			ApportionmentListing apps = new ApportionmentListing(Factory, Consol);
			apps.IsActivated = true;
			apps.LoadChildShipmentsAndAcquireMutexesWhereRequired();
			JobConsolCost freightCost = apps.CostsCollection.TryAddNew();
			freightCost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			freightCost.E6_RX_NKCurrency = TestObjectCreator.USD.RX_Code;
			freightCost.E6_ExchangeRate = 0.65m;
			freightCost.E6_OSCostAmount = 200m;
			freightCost.E6_ApportionmentMethod = "SHP";
			freightCost.E6_OH_Creditor = TestObjectCreator.Agent.PK;
			freightCost.E6_InvoiceNum = "xawecadfa";
			freightCost.E6_InvoiceDate = ZDateTime.Now;

			Factory.Save();

			Job shipment1Job = new Job.Loader(Factory, Shipment1).Load();
			Job shipment2Job = new Job.Loader(Factory, Shipment2).Load();
			shipment1Job.PlugInData = Shipment1;
			shipment2Job.PlugInData = Shipment2;

			Charge shipment1Charge = shipment1Job.Charges.AddNew();
			Charge shipment2Charge = shipment2Job.Charges.AddNew();

			shipment1Charge.JR_AC = Env.Registry.FreightChargeCode;
			shipment1Charge.JR_IsIncludedInProfitShare = true;
			shipment1Charge.JR_OSSellAmt = 180m;
			shipment1Charge.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment1Charge.JR_OSCostAmt = 80m;
			shipment1Charge.JR_AgentDeclaredSellAmt = 150m;
			shipment1Charge.JR_AgentDeclaredCostAmt = 100m;

			shipment2Charge.JR_AC = Env.Registry.FreightChargeCode;
			shipment2Charge.JR_IsIncludedInProfitShare = true;
			shipment2Charge.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment2Charge.JR_OSSellAmt = 200m;
			shipment2Charge.JR_OSCostAmt = 100m;
			shipment2Charge.JR_AgentDeclaredSellAmt = 180m;
			shipment2Charge.JR_AgentDeclaredCostAmt = 120m;

			Factory.Save();
			apps.ReleaseMutexes();

			JobCollection jobs = new JobCollection(Factory);
			jobs.Load();

			#endregion

			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			ConsolInvoicingPostManager testPostManager = new ConsolInvoicingPostManager(Factory, jobs.Cast<Job>(), Consol, apps);
			testPostManager.ExportAgentPosting += new ExportAgentPostingEventHandler(TestPostManager_ExportAgentPosting_TestCreditProfitShareWhenPostingAgentInvoices);
			testPostManager.ProfitShareConfirmation += new ProfitShareConfirmationEventHandler(TestPostManager_ProfitShareConfirmation);
			testPostManager.CreateTransactions(JobInvoicingPostingOption.All);

			Charge[] shipment1PSCharges = (Charge[])shipment1Job.Charges.Find(new ZQuery(JobChargeSchema.JR_AC, AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value));
			AssertEquals(1, shipment1PSCharges.Length);
			AssertEquals("Costs should not be posted since the profit share charge code has error", false, shipment1PSCharges[0].JR_IsCostPosted);

			Charge[] shipment2PSCharges = (Charge[])shipment2Job.Charges.Find(new ZQuery(JobChargeSchema.JR_AC, AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value));
			AssertEquals(1, shipment2PSCharges.Length);
			AssertEquals("Costs should not be posted since the profit share charge code has error", false, shipment2PSCharges[0].JR_IsCostPosted);
		}

		#region Tax Branch

		public void TestCreditProfitShareWhenPostingAgentInvoices_AP_EnableTaxBranchReporting()
		{
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			TestObjectCreator.ResetSecurityCore();

			#region Setup

			var branch1 = TestObjectCreator.CreateBranch("BR1", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("BR2", GlbCompany.CurrentCompany);
			var branch3 = TestObjectCreator.CreateBranch("BR3", GlbCompany.CurrentCompany);

			TestObjectCreator.Agent.CompanyData.OB_IsCreditor = ZBool.True;
			TestObjectCreator.Agent.CompanyData.OB_IsDebtor = ZBool.True;
			TestObjectCreator.Agent.CompanyData.SetAPTaxApplicable(ZBool.True);
			TestObjectCreator.Agent.CompanyData.SetARTaxApplicable(ZBool.True);
			TestObjectCreator.Agent.MiscServ.OM_ARWHTApplicable = ZBool.False;
			CreateGroupWithPostingStyle(TestObjectCreator.AALSHI, InvoicePostingOptionsList.Codes.FinalInvoiceOnly);
			Factory.Save();

			ZString transportMode = "AIR";
			ZString origin = "AUSYD";
			ZString destination = "USLAX";

			SetupProfitShareRelationship(GlbCompany.CurrentCompany.OrgProxy, TestObjectCreator.Agent, 50m, 50m, origin, destination, transportMode);

			Factory.Save();

			SetupConsolAndShipments(transportMode, origin, destination, TestObjectCreator.Agent);
			Consol.JK_PrepaidCollect = Enterprise.Core.Constants.PaymentType.Collect;
			Consol.JK_RL_NKLoadPort = origin;
			Consol.JK_RL_NKDischargePort = destination;
			Consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);
			Consol.SetDefaultReceivingForwarderAddress(TestObjectCreator.Agent);
			Factory.Save();

			var shipment1Job = TestObjectCreator.CreateJob(Shipment1);
			var shipment2Job = TestObjectCreator.CreateJob(Shipment2);
			shipment1Job.JH_GB_TaxBranch = branch3.PK;
			shipment2Job.JH_GB_TaxBranch = branch2.PK;
			shipment1Job.JH_GE = TestObjectCreator.FEADepartment.PK;
			shipment2Job.JH_GE = TestObjectCreator.FEADepartment.PK;

			var shipment1Charge1 = shipment1Job.Charges.AddNew();
			var shipment1Charge2 = shipment1Job.Charges.AddNew();
			var shipment2Charge1 = shipment2Job.Charges.AddNew();

			shipment1Charge1.JR_AC = Env.Registry.FreightChargeCode;
			shipment1Charge1.JR_IsIncludedInProfitShare = true;
			shipment1Charge1.JR_OSSellAmt = 1000m;
			shipment1Charge1.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment1Charge1.JR_OSCostAmt = 800m;
			shipment1Charge1.JR_AgentDeclaredSellAmt = 1000m;
			shipment1Charge1.JR_AgentDeclaredCostAmt = 800m;
			shipment1Charge1.JR_GB_SellTaxBranch = branch2.PK;

			shipment1Charge2.JR_AC = Env.Registry.FreightChargeCode;
			shipment1Charge2.JR_IsIncludedInProfitShare = true;
			shipment1Charge2.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment1Charge2.JR_OSSellAmt = 1001m;
			shipment1Charge2.JR_OSCostAmt = 801m;
			shipment1Charge2.JR_AgentDeclaredSellAmt = 1001m;
			shipment1Charge2.JR_AgentDeclaredCostAmt = 801m;
			shipment1Charge2.JR_GB_SellTaxBranch = branch2.PK;

			shipment2Charge1.JR_AC = Env.Registry.FreightChargeCode;
			shipment2Charge1.JR_IsIncludedInProfitShare = true;
			shipment2Charge1.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
			shipment2Charge1.JR_OSSellAmt = 750m;
			shipment2Charge1.JR_OSCostAmt = 600m;
			shipment2Charge1.JR_AgentDeclaredSellAmt = 750m;
			shipment2Charge1.JR_AgentDeclaredCostAmt = 600m;
			shipment2Charge1.JR_GB_SellTaxBranch = branch3.PK;

			Factory.Save();

			var jobs = new JobCollection(Factory);
			jobs.Load();

			#endregion

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

				ConsolInvoicingPostManager testPostManager = new ConsolInvoicingPostManager(Factory, jobs.Cast<Job>(), Consol, null);
				testPostManager.ExportAgentPosting += new ExportAgentPostingEventHandler(TestPostManager_ExportAgentPosting_TestCreditProfitShareWhenPostingAgentInvoices);
				testPostManager.ProfitShareConfirmation += new ProfitShareConfirmationEventHandler(TestPostManager_ProfitShareConfirmation);
				var transactions = testPostManager.CreateTransactions(JobInvoicingPostingOption.All);

				Charge[] shipment1PSCharges = (Charge[])shipment1Job.Charges.Find(new ZQuery(JobChargeSchema.JR_AC, AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value));
				AssertEquals(1, shipment1PSCharges.Length);
				AssertEquals(true, shipment1PSCharges[0].JR_IsCostPosted);

				Charge[] shipment2PSCharges = (Charge[])shipment2Job.Charges.Find(new ZQuery(JobChargeSchema.JR_AC, AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value));
				AssertEquals(1, shipment2PSCharges.Length);
				AssertEquals(true, shipment2PSCharges[0].JR_IsCostPosted);

				var consolcost = Consol.GetApportionments().CostsCollection[0];
				AssertEquals(AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value, consolcost.E6_AC_ChargeCode);
				AssertEquals(true, consolcost.IsPosted);

				var apTransactions = transactions.GetAllAPTransactions();
				var arTransactions = transactions.GetAllARTransactions();
				AssertEquals(1, apTransactions.Length);
				AssertEquals(3, arTransactions.Length);
				AssertEquals(branch1.PK, apTransactions.Single(x => x.AH_InvoiceAmount == 0m).AH_GB_TaxBranch);
				AssertEquals(branch2.PK, arTransactions.Single(x => x.AH_InvoiceAmount == 2001m).AH_GB_TaxBranch);
				AssertEquals(branch3.PK, arTransactions.Single(x => x.AH_InvoiceAmount == 750m).AH_GB_TaxBranch);
				AssertEquals(branch1.PK, arTransactions.Single(x => x.AH_InvoiceAmount == -275m).AH_GB_TaxBranch);
				AssertEquals(true, apTransactions.OfType<InvoicingBase>().All(x => x.Lines.OfType<AccTransactionLines>().All(y => y.AL_GB_TaxBranch == x.AH_GB_TaxBranch)));
				AssertEquals(true, arTransactions.OfType<InvoicingBase>().All(x => x.Lines.OfType<AccTransactionLines>().All(y => y.AL_GB_TaxBranch == x.AH_GB_TaxBranch)));
			}
		}

		public void TestPostConsolCostOnCollectInvoice_TaxBranch_EnableTaxBranchReporting()
		{
			TestPostConsolCostOnCollectInvoice_TaxBranch(true);
		}

		public void TestPostConsolCostOnCollectInvoice_TaxBranch_DisableTaxBranchReporting()
		{
			TestPostConsolCostOnCollectInvoice_TaxBranch(false);
		}

		void TestPostConsolCostOnCollectInvoice_TaxBranch(bool enableTaxBranchReporting)
		{
			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableTaxBranchReporting))
			{
				TestObjectCreator.Agent.CompanyData.OB_IsCreditor = ZBool.True;
				TestObjectCreator.Agent.CompanyData.OB_IsDebtor = ZBool.True;
				TestObjectCreator.Agent.CompanyData.SetAPTaxApplicable(ZBool.True);
				TestObjectCreator.Agent.CompanyData.SetARTaxApplicable(ZBool.True);
				Factory.Save();

				SetupConsolAndShipments("AIR", "AUSYD", "USLAX", TestObjectCreator.Agent);
				Consol.JK_PrepaidCollect = Enterprise.Core.Constants.PaymentType.Collect;
				Consol.JK_RL_NKLoadPort = "AUSYD";
				Consol.JK_RL_NKDischargePort = "USLAX";
				Consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);
				Consol.SetDefaultReceivingForwarderAddress(TestObjectCreator.Agent);

				var apps = new ApportionmentListing(Factory, Consol);
				apps.IsActivated = true;
				apps.LoadChildShipmentsAndAcquireMutexesWhereRequired();
				var cost = apps.CostsCollection.TryAddNew();
				cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
				cost.E6_RX_NKCurrency = TestObjectCreator.USD.RX_Code;
				cost.E6_ExchangeRate = 0.65m;
				cost.E6_OSCostAmount = 200m;
				cost.E6_ApportionmentMethod = "SHP";
				cost.E6_OH_Creditor = TestObjectCreator.Agent.PK;
				cost.E6_InvoiceNum = "xawecadfa";
				cost.E6_InvoiceDate = ZDateTime.Now;
				cost.E6_IsForCollectInvoice = true;

				Factory.Save();

				cost.E6_GB_CostTaxBranch = TestObjectCreator.NonCurrentBranch.PK;
				cost.ApportionmentCharges.OfType<ApportionSplitCharge>().ForEach(x => x.JR_GB_CostTaxBranch = TestObjectCreator.NonCurrentBranch.PK);
				cost.ApportionmentCharges.OfType<ApportionSplitCharge>().ForEach(x => x.JR_GB_SellTaxBranch = GlbBranch.CurrentBranch.PK);

				var jobs = new JobCollection(Factory);
				jobs.Load();
				var testPostManager = new ConsolInvoicingPostManager(Factory, jobs.Cast<Job>(), Consol, apps);
				var transactions = testPostManager.CreateTransactions(JobInvoicingPostingOption.Agent);

				var expectedTaxBranch = enableTaxBranchReporting ? TestObjectCreator.NonCurrentBranch.PK : ZGuid.Empty;

				AssertEquals("Expect 1 AR CreditNote", 1, transactions.GetAllARInvoicesAndCreditNotes().Length);
				var arCreditNote = transactions.GetAllARInvoicesAndCreditNotes()[0] as ARCreditNote;
				AssertEquals(2, arCreditNote.Lines.Count);
				AssertEquals(expectedTaxBranch, arCreditNote.AH_GB_TaxBranch);
				AssertEquals(true, arCreditNote.Lines.OfType<AccTransactionLines>().All(x => x.AL_GB_TaxBranch == expectedTaxBranch));

				AssertEquals("Expect 1 AP Invoice", 1, transactions.GetAllAPInvoicesAndCreditNotes().Length);
				var apInvoice = transactions.GetAllAPInvoicesAndCreditNotes()[0] as APInvoice;
				AssertEquals(4, apInvoice.Lines.Count);
				AssertEquals(expectedTaxBranch, apInvoice.AH_GB_TaxBranch);
				AssertEquals(true, apInvoice.Lines.OfType<AccTransactionLines>().All(x => x.AL_GB_TaxBranch == expectedTaxBranch));
			}
		}

		#endregion

		public void TestPostAgentInvoiceWhenProfitShareAsARAndConsolCostOnCollectInvoice()
		{
			#region Setup

			TestObjectCreator.Agent.CompanyData.OB_IsCreditor = ZBool.True;
			TestObjectCreator.Agent.CompanyData.OB_IsDebtor = ZBool.True;
			TestObjectCreator.Agent.CompanyData.SetAPTaxApplicable(ZBool.True);
			TestObjectCreator.Agent.CompanyData.SetARTaxApplicable(ZBool.True);
			TestObjectCreator.Agent.MiscServ.OM_ARWHTApplicable = ZBool.False;
			CreateGroupWithPostingStyle(TestObjectCreator.AALSHI, InvoicePostingOptionsList.Codes.FinalInvoiceOnly);
			Factory.Save();

			ZString transportMode = "AIR";
			ZString origin = "AUSYD";
			ZString destination = "USLAX";

			SetupProfitShareRelationship(GlbCompany.CurrentCompany.OrgProxy, TestObjectCreator.Agent, 60m, 40m, origin, destination, transportMode);

			Factory.Save();

			SetupConsolAndShipments(transportMode, origin, destination, TestObjectCreator.Agent);
			Consol.JK_PrepaidCollect = Enterprise.Core.Constants.PaymentType.Collect;
			Consol.JK_RL_NKLoadPort = origin;
			Consol.JK_RL_NKDischargePort = destination;
			Consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);
			Consol.SetDefaultReceivingForwarderAddress(TestObjectCreator.Agent);

			ApportionmentListing apps = new ApportionmentListing(Factory, Consol);
			apps.IsActivated = true;
			apps.LoadChildShipmentsAndAcquireMutexesWhereRequired();
			JobConsolCost freightCost = apps.CostsCollection.TryAddNew();
			freightCost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			freightCost.E6_RX_NKCurrency = TestObjectCreator.USD.RX_Code;
			freightCost.E6_ExchangeRate = 0.65m;
			freightCost.E6_OSCostAmount = 200m;
			freightCost.E6_ApportionmentMethod = "SHP";
			freightCost.E6_OH_Creditor = TestObjectCreator.Agent.PK;
			freightCost.E6_InvoiceNum = "xawecadfa";
			freightCost.E6_InvoiceDate = ZDateTime.Now;
			freightCost.E6_IsForCollectInvoice = true;

			Factory.Save();

			Job shipment1Job = new Job.Loader(Factory, Shipment1).Load();
			Job shipment2Job = new Job.Loader(Factory, Shipment2).Load();
			shipment1Job.PlugInData = Shipment1;
			shipment2Job.PlugInData = Shipment2;

			Charge shipment1Charge = shipment1Job.Charges.AddNew();
			Charge shipment2Charge = shipment2Job.Charges.AddNew();

			shipment1Charge.JR_AC = Env.Registry.FreightChargeCode;
			shipment1Charge.JR_IsIncludedInProfitShare = true;
			shipment1Charge.JR_OSSellAmt = 180m;
			shipment1Charge.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment1Charge.JR_OSCostAmt = 80m;
			shipment1Charge.JR_AgentDeclaredSellAmt = 150m;
			shipment1Charge.JR_AgentDeclaredCostAmt = 100m;

			shipment2Charge.JR_AC = Env.Registry.FreightChargeCode;
			shipment2Charge.JR_IsIncludedInProfitShare = true;
			shipment2Charge.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment2Charge.JR_OSSellAmt = 200m;
			shipment2Charge.JR_OSCostAmt = 100m;
			shipment2Charge.JR_AgentDeclaredSellAmt = 180m;
			shipment2Charge.JR_AgentDeclaredCostAmt = 120m;

			Factory.Save();
			apps.ReleaseMutexes();

			JobCollection jobs = new JobCollection(Factory);
			jobs.Load();

			#endregion

			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			ConsolInvoicingPostManager testPostManager = new ConsolInvoicingPostManager(Factory, jobs.Cast<Job>(), Consol, apps);
			testPostManager.ExportAgentPosting += new ExportAgentPostingEventHandler(TestPostManager_ExportAgentPosting_TestCreditProfitShareWhenPostingAgentInvoices);
			testPostManager.ProfitShareConfirmation += new ProfitShareConfirmationEventHandler(TestPostManager_ProfitShareConfirmation);
			//TransactionCreatorHashtable transactions = TestPostManager.CreateTransactions(JobInvoicingPostingOption.All);
			TransactionCreatorHashtable transactions = testPostManager.CreateTransactions(JobInvoicingPostingOption.Agent);

			AssertEquals("Expect 1 AR Invoice", 1, transactions.GetAllARInvoicesAndCreditNotes().Length);
			Assert(transactions.GetAllARInvoicesAndCreditNotes()[0] is ARInvoice);
			AssertEquals(6, transactions.GetAllARInvoicesAndCreditNotes()[0].Lines.Count);

			AssertEquals("Expect 1 AP Invoice", 1, transactions.GetAllAPInvoicesAndCreditNotes().Length);
			Assert(transactions.GetAllAPInvoicesAndCreditNotes()[0] is APInvoice);
			AssertEquals(4, transactions.GetAllAPInvoicesAndCreditNotes()[0].Lines.Count);

			Charge[] shipment1PSCharges = (Charge[])shipment1Job.Charges.Find(new ZQuery(JobChargeSchema.JR_AC, AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value));
			AssertEquals(1, shipment1PSCharges.Length);
			AssertEquals(true, shipment1PSCharges[0].JR_IsRevenuePosted);

			Charge[] shipment2PSCharges = (Charge[])shipment2Job.Charges.Find(new ZQuery(JobChargeSchema.JR_AC, AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value));
			AssertEquals(1, shipment2PSCharges.Length);
			AssertEquals(true, shipment2PSCharges[0].JR_IsRevenuePosted);
		}

		public void TestCreditProfitShareWhenPostingAgentInvoices_AR_WithAddressContact()
		{
			#region Setup

			TestObjectCreator.Agent.CompanyData.OB_IsCreditor = ZBool.True;
			TestObjectCreator.Agent.CompanyData.OB_IsDebtor = ZBool.True;
			TestObjectCreator.Agent.CompanyData.SetAPTaxApplicable(ZBool.True);
			TestObjectCreator.Agent.CompanyData.SetARTaxApplicable(ZBool.True);
			TestObjectCreator.Agent.MiscServ.OM_ARWHTApplicable = ZBool.False;
			CreateGroupWithPostingStyle(TestObjectCreator.AALSHI, InvoicePostingOptionsList.Codes.FinalInvoiceOnly);
			Factory.Save();

			ZString transportMode = "AIR";
			ZString origin = "AUSYD";
			ZString destination = "USLAX";

			SetupProfitShareRelationship(GlbCompany.CurrentCompany.OrgProxy, TestObjectCreator.Agent, 60m, 40m, origin, destination, transportMode);

			Factory.Save();

			SetupConsolAndShipments(transportMode, origin, destination, TestObjectCreator.Agent);
			Consol.JK_PrepaidCollect = Enterprise.Core.Constants.PaymentType.Collect;
			Consol.JK_RL_NKLoadPort = origin;
			Consol.JK_RL_NKDischargePort = destination;
			Consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);
			Consol.SetDefaultReceivingForwarderAddress(TestObjectCreator.Agent);

			ApportionmentListing apps = new ApportionmentListing(Factory, Consol);
			apps.IsActivated = true;
			apps.LoadChildShipmentsAndAcquireMutexesWhereRequired();
			JobConsolCost freightCost = apps.CostsCollection.TryAddNew();
			freightCost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			freightCost.E6_RX_NKCurrency = TestObjectCreator.USD.RX_Code;
			freightCost.E6_ExchangeRate = 0.65m;
			freightCost.E6_OSCostAmount = 200m;
			freightCost.E6_ApportionmentMethod = "SHP";
			freightCost.E6_OH_Creditor = TestObjectCreator.Agent.PK;
			freightCost.E6_InvoiceNum = "xawecadfa";
			freightCost.E6_InvoiceDate = ZDateTime.Now;

			Factory.Save();

			Job shipment1Job = new Job.Loader(Factory, Shipment1).Load();
			Job shipment2Job = new Job.Loader(Factory, Shipment2).Load();
			shipment1Job.PlugInData = Shipment1;
			shipment2Job.PlugInData = Shipment2;

			Charge shipment1Charge = shipment1Job.Charges.AddNew();
			Charge shipment2Charge = shipment2Job.Charges.AddNew();

			shipment1Charge.JR_AC = Env.Registry.FreightChargeCode;
			shipment1Charge.JR_IsIncludedInProfitShare = true;
			shipment1Charge.JR_OSSellAmt = 180m;
			shipment1Charge.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment1Charge.JR_OSCostAmt = 80m;
			shipment1Charge.JR_AgentDeclaredSellAmt = 150m;
			shipment1Charge.JR_AgentDeclaredCostAmt = 100m;

			shipment2Charge.JR_AC = Env.Registry.FreightChargeCode;
			shipment2Charge.JR_IsIncludedInProfitShare = true;
			shipment2Charge.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment2Charge.JR_OSSellAmt = 200m;
			shipment2Charge.JR_OSCostAmt = 100m;
			shipment2Charge.JR_AgentDeclaredSellAmt = 180m;
			shipment2Charge.JR_AgentDeclaredCostAmt = 120m;

			var address1 = TestObjectCreator.CreateAddress(shipment1Charge.SellAccount, "1 street");
			var address2 = TestObjectCreator.CreateAddress(shipment2Charge.SellAccount, "2 street");
			var contact1 = TestObjectCreator.CreateContact(shipment1Charge.SellAccount, "John");
			var contact2 = TestObjectCreator.CreateContact(shipment2Charge.SellAccount, "Bob");

			shipment1Charge.JR_OA_SellInvoiceAddress = address1.PK;
			shipment1Charge.JR_OC_SellInvoiceContact = contact1.PK;
			shipment2Charge.JR_OA_SellInvoiceAddress = address2.PK;
			shipment2Charge.JR_OC_SellInvoiceContact = contact2.PK;

			Factory.Save();
			apps.ReleaseMutexes();

			JobCollection jobs = new JobCollection(Factory);
			jobs.Load();

			#endregion

			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			ConsolInvoicingPostManager testPostManager = new ConsolInvoicingPostManager(Factory, jobs.Cast<Job>(), Consol, apps);
			testPostManager.ExportAgentPosting += new ExportAgentPostingEventHandler(TestPostManager_ExportAgentPosting_TestCreditProfitShareWhenPostingAgentInvoices);
			testPostManager.ProfitShareConfirmation += new ProfitShareConfirmationEventHandler(TestPostManager_ProfitShareConfirmation);
			testPostManager.CreateTransactions(JobInvoicingPostingOption.All);

			Charge[] shipment1PSCharges = (Charge[])shipment1Job.Charges.Find(new ZQuery(JobChargeSchema.JR_AC, AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value));
			AssertEquals(1, shipment1PSCharges.Length);
			var charge = shipment1PSCharges[0];
			AssertEquals("JR_IsRevenuePosted", true, charge.JR_IsRevenuePosted);
			AssertEquals("Address", ZGuid.Empty, charge.JR_OA_SellInvoiceAddress);
			AssertEquals("Contact", ZGuid.Empty, charge.JR_OC_SellInvoiceContact);

			Charge[] shipment2PSCharges = (Charge[])shipment2Job.Charges.Find(new ZQuery(JobChargeSchema.JR_AC, AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value));
			AssertEquals(1, shipment2PSCharges.Length);
			charge = shipment2PSCharges[0];
			AssertEquals("JR_IsRevenuePosted", true, charge.JR_IsRevenuePosted);
			AssertEquals("Address", ZGuid.Empty, charge.JR_OA_SellInvoiceAddress);
			AssertEquals("Contact", ZGuid.Empty, charge.JR_OC_SellInvoiceContact);
		}

		public void TestCreditProfitShareWhenRegistryIsIncorrect()
		{
			#region Setup

			TestObjectCreator.Agent.CompanyData.OB_IsCreditor = ZBool.True;
			TestObjectCreator.Agent.CompanyData.OB_IsDebtor = ZBool.True;
			TestObjectCreator.Agent.CompanyData.SetAPTaxApplicable(ZBool.True);
			TestObjectCreator.Agent.CompanyData.SetARTaxApplicable(ZBool.True);
			TestObjectCreator.Agent.MiscServ.OM_ARWHTApplicable = ZBool.False;
			CreateGroupWithPostingStyle(TestObjectCreator.AALSHI, InvoicePostingOptionsList.Codes.FinalInvoiceOnly);
			Factory.Save();

			ZString transportMode = "AIR";
			ZString origin = "AUSYD";
			ZString destination = "USLAX";

			SetupProfitShareRelationship(GlbCompany.CurrentCompany.OrgProxy, TestObjectCreator.Agent, 60m, 40m, origin, destination, transportMode);

			Factory.Save();

			SetupConsolAndShipments(transportMode, origin, destination, TestObjectCreator.Agent);
			Consol.JK_PrepaidCollect = Enterprise.Core.Constants.PaymentType.Collect;
			Consol.JK_RL_NKLoadPort = origin;
			Consol.JK_RL_NKDischargePort = destination;
			Consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);
			Consol.SetDefaultReceivingForwarderAddress(TestObjectCreator.Agent);

			ApportionmentListing apps = new ApportionmentListing(Factory, Consol);
			apps.IsActivated = true;
			apps.LoadChildShipmentsAndAcquireMutexesWhereRequired();
			JobConsolCost freightCost = apps.CostsCollection.TryAddNew();
			freightCost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			freightCost.E6_RX_NKCurrency = TestObjectCreator.USD.RX_Code;
			freightCost.E6_ExchangeRate = 0.65m;
			freightCost.E6_OSCostAmount = 200m;
			freightCost.E6_ApportionmentMethod = "SHP";
			freightCost.E6_OH_Creditor = TestObjectCreator.Agent.PK;
			freightCost.E6_InvoiceNum = "xawecadfa";
			freightCost.E6_InvoiceDate = ZDateTime.Now;

			Factory.Save();

			Job shipment1Job = new Job.Loader(Factory, Shipment1).Load();
			shipment1Job.JH_GE = ZGuid.Empty;
			shipment1Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			shipment1Job.PlugInData = Shipment1;

			Charge shipment1Charge = shipment1Job.Charges.AddNew();

			shipment1Charge.JR_AC = Env.Registry.FreightChargeCode;
			shipment1Charge.JR_IsIncludedInProfitShare = true;
			shipment1Charge.JR_OSSellAmt = 180m;
			shipment1Charge.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment1Charge.JR_OSCostAmt = 80m;
			shipment1Charge.JR_AgentDeclaredSellAmt = 150m;
			shipment1Charge.JR_AgentDeclaredCostAmt = 100m;

			Factory.Save();
			apps.ReleaseMutexes();

			JobCollection jobs = new JobCollection(Factory);
			jobs.Load();

			#endregion

			Guid oldRegistryValue = AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value;
			Assert("Precondition: Incorrect registry setup event have not been rised yet.", !IsRegistrySetupEventRised);
			try
			{
				AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);

				ConsolInvoicingPostManager testPostManager = new ConsolInvoicingPostManager(Factory, jobs.Cast<Job>(), Consol, apps);
				testPostManager.IncorrectRegistrySetup += new IncorrectRegistrySetupEventHandler(TestPostManager_IncorrectRegistrySetup);
				testPostManager.ProfitShareConfirmation += new ProfitShareConfirmationEventHandler(TestPostManager_ProfitShareConfirmation);
				TransactionCreatorHashtable transactions = testPostManager.CreateTransactions(JobInvoicingPostingOption.Agent);
				Assert("Incorrect registry setup event should be rised.", IsRegistrySetupEventRised);

				InvoicingBase[] aPInvoices = transactions.GetAllAPInvoicesAndCreditNotes();
				AssertEquals("Should be 2 AP invoices", 1, aPInvoices.Length);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldRegistryValue);
			}
		}

		public void TestNoTaxApplicableConfigurationErrorWhenCurrentLoginCompanyNotGSTRegistered()
		{
			#region Setup

			TestObjectCreator.Agent.CompanyData.OB_IsCreditor = ZBool.True;
			TestObjectCreator.Agent.CompanyData.OB_IsDebtor = ZBool.True;
			TestObjectCreator.Agent.CompanyData.SetAPTaxApplicable(ZBool.True);
			TestObjectCreator.Agent.CompanyData.SetARTaxApplicable(ZBool.False);
			TestObjectCreator.Agent.MiscServ.OM_ARWHTApplicable = ZBool.False;
			CreateGroupWithPostingStyle(TestObjectCreator.AALSHI, InvoicePostingOptionsList.Codes.FinalInvoiceOnly);
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			Factory.Save();

			ZString transportMode = "AIR";
			ZString origin = "AUSYD";
			ZString destination = "USLAX";

			SetupProfitShareRelationship(GlbCompany.CurrentCompany.OrgProxy, TestObjectCreator.Agent, 60m, 40m, origin, destination, transportMode);

			Factory.Save();

			SetupConsolAndShipments(transportMode, origin, destination, TestObjectCreator.Agent);
			Consol.JK_PrepaidCollect = Enterprise.Core.Constants.PaymentType.Collect;
			Consol.JK_RL_NKLoadPort = origin;
			Consol.JK_RL_NKDischargePort = destination;
			Consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);
			Consol.SetDefaultReceivingForwarderAddress(TestObjectCreator.Agent);

			ApportionmentListing apps = new ApportionmentListing(Factory, Consol);
			apps.IsActivated = true;
			apps.LoadChildShipmentsAndAcquireMutexesWhereRequired();
			JobConsolCost freightCost = apps.CostsCollection.TryAddNew();
			freightCost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			freightCost.E6_RX_NKCurrency = TestObjectCreator.USD.RX_Code;
			freightCost.E6_ExchangeRate = 0.65m;
			freightCost.E6_OSCostAmount = 200m;
			freightCost.E6_ApportionmentMethod = "SHP";
			freightCost.E6_OH_Creditor = TestObjectCreator.Agent.PK;
			freightCost.E6_InvoiceNum = "xawecadfa";
			freightCost.E6_InvoiceDate = ZDateTime.Now;

			Factory.Save();

			Job shipment1Job = new Job.Loader(Factory, Shipment1).Load();
			shipment1Job.PlugInData = Shipment1;

			Charge shipment1Charge = shipment1Job.Charges.AddNew();

			shipment1Charge.JR_AC = Env.Registry.FreightChargeCode;
			shipment1Charge.JR_IsIncludedInProfitShare = true;
			shipment1Charge.JR_OSSellAmt = 180m;
			shipment1Charge.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment1Charge.JR_OSCostAmt = 80m;
			shipment1Charge.JR_AgentDeclaredSellAmt = 150m;
			shipment1Charge.JR_AgentDeclaredCostAmt = 100m;

			Factory.Save();
			apps.ReleaseMutexes();

			JobCollection jobs = new JobCollection(Factory);
			jobs.Load();

			#endregion

			ConsolInvoicingPostManager testPostManager = new ConsolInvoicingPostManager(Factory, jobs.Cast<Job>(), Consol, apps);
			testPostManager.OnCriticalPostError += new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_OnCriticalPostError);
			TransactionCreatorHashtable transactions = testPostManager.CreateTransactions(JobInvoicingPostingOption.Agent);

			AssertEquals("OnCriticalPostError event should not be raised", false, IsOnCriticalPostErrorEventRaised);
			testPostManager.OnCriticalPostError -= new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_OnCriticalPostError);
		}

		public void TestProfitShareNotPostedWhenAgentConfigurationIsIncorrect()
		{
			#region Setup

			TestObjectCreator.Agent.CompanyData.OB_IsCreditor = ZBool.True;
			TestObjectCreator.Agent.CompanyData.OB_IsDebtor = ZBool.True;
			TestObjectCreator.Agent.CompanyData.SetAPTaxApplicable(ZBool.True);
			TestObjectCreator.Agent.CompanyData.SetARTaxApplicable(ZBool.False);
			TestObjectCreator.Agent.MiscServ.OM_ARWHTApplicable = ZBool.False;
			CreateGroupWithPostingStyle(TestObjectCreator.AALSHI, InvoicePostingOptionsList.Codes.FinalInvoiceOnly);
			Factory.Save();

			ZString transportMode = "AIR";
			ZString origin = "AUSYD";
			ZString destination = "USLAX";

			SetupProfitShareRelationship(GlbCompany.CurrentCompany.OrgProxy, TestObjectCreator.Agent, 60m, 40m, origin, destination, transportMode);

			Factory.Save();

			SetupConsolAndShipments(transportMode, origin, destination, TestObjectCreator.Agent);
			Consol.JK_PrepaidCollect = Enterprise.Core.Constants.PaymentType.Collect;
			Consol.JK_RL_NKLoadPort = origin;
			Consol.JK_RL_NKDischargePort = destination;
			Consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);
			Consol.SetDefaultReceivingForwarderAddress(TestObjectCreator.Agent);

			ApportionmentListing apps = new ApportionmentListing(Factory, Consol);
			apps.IsActivated = true;
			apps.LoadChildShipmentsAndAcquireMutexesWhereRequired();
			JobConsolCost freightCost = apps.CostsCollection.TryAddNew();
			freightCost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			freightCost.E6_RX_NKCurrency = TestObjectCreator.USD.RX_Code;
			freightCost.E6_OH_Creditor = TestObjectCreator.Agent.PK;
			freightCost.E6_ExchangeRate = 0.65m;
			freightCost.E6_OSCostAmount = 200m;
			freightCost.E6_ApportionmentMethod = "SHP";
			freightCost.E6_InvoiceNum = "xawecadfa";
			freightCost.E6_InvoiceDate = ZDateTime.Now;

			Factory.Save();

			Job shipment1Job = new Job.Loader(Factory, Shipment1).Load();
			shipment1Job.PlugInData = Shipment1;

			Charge shipment1Charge = shipment1Job.Charges.AddNew();

			shipment1Charge.JR_AC = Env.Registry.FreightChargeCode;
			shipment1Charge.JR_IsIncludedInProfitShare = true;
			shipment1Charge.JR_OSSellAmt = 180m;
			shipment1Charge.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment1Charge.JR_OSCostAmt = 80m;
			shipment1Charge.JR_AgentDeclaredSellAmt = 150m;
			shipment1Charge.JR_AgentDeclaredCostAmt = 100m;

			Factory.Save();
			apps.ReleaseMutexes();

			JobCollection jobs = new JobCollection(Factory);
			jobs.Load();

			#endregion

			ConsolInvoicingPostManager testPostManager = new ConsolInvoicingPostManager(Factory, jobs.Cast<Job>(), Consol, apps);
			testPostManager.ExportAgentPosting += new ExportAgentPostingEventHandler(TestPostManager_ExportAgentPosting_TestCreditProfitShareWhenPostingAgentInvoices);
			testPostManager.ProfitShareConfirmation += new ProfitShareConfirmationEventHandler(TestPostManager_ProfitShareConfirmation);
			testPostManager.OnCriticalPostError += new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_OnCriticalPostError);
			TransactionCreatorHashtable transactions = testPostManager.CreateTransactions(JobInvoicingPostingOption.Agent);

			Assert("OnCriticalPostError event should be raised", IsOnCriticalPostErrorEventRaised);
			AssertEquals("ProfitShareConfirmationEvent should not be raised", false, ProfitShareConfirmationEventRaised);
			AssertEquals("IsRegistrySetupEvent should not be raised", false, IsRegistrySetupEventRised);

			InvoicingBase[] aPInvoices = transactions.GetAllAPInvoicesAndCreditNotes();
			AssertEquals("Should be 1 AP invoices", 1, aPInvoices.Length);
			APInvoice invoice = (APInvoice)aPInvoices[0];
			AssertEquals("Invoice should be for zero", 0m, invoice.AH_OSExTaxAmount);
			AssertEquals("Invoice should have 4 lines", 4, invoice.Lines.Count);

			InvoicingBaseCollection postedInvoices = testPostManager.Poster.PostedInvoices;
			AssertEquals("Should have posted 1 AR invoice", 1, postedInvoices.Count);
			var agentCreditNote = (ARCreditNote)postedInvoices[0];
			AssertEquals("Should have been posted for the total amount", -127.69m, agentCreditNote.AH_InvoiceAmount);
			AssertEquals("Should have 6 lines on invoice", 3, agentCreditNote.Lines.Count);

			testPostManager.ExportAgentPosting -= new ExportAgentPostingEventHandler(TestPostManager_ExportAgentPosting_TestCreditProfitShareWhenPostingAgentInvoices);
			testPostManager.ProfitShareConfirmation -= new ProfitShareConfirmationEventHandler(TestPostManager_ProfitShareConfirmation);
			testPostManager.OnCriticalPostError -= new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_OnCriticalPostError);

			// Force it to go other way with no transactions created checking ZAgent configuration
			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			testPostManager = new ConsolInvoicingPostManager(Factory, jobs.Cast<Job>(), Consol, apps);
			testPostManager.ExportAgentPosting += new ExportAgentPostingEventHandler(TestPostManager_ExportAgentPosting_TestCreditProfitShareWhenPostingAgentInvoices);
			testPostManager.ProfitShareConfirmation += new ProfitShareConfirmationEventHandler(TestPostManager_ProfitShareConfirmation);
			testPostManager.OnCriticalPostError += new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_OnCriticalPostErrorForEmptyInvoice);
			transactions = testPostManager.CreateTransactions(JobInvoicingPostingOption.Agent);

			Assert("OnCriticalPostError event should be raised", IsOnCriticalPostErrorEventRaised);
			AssertEquals("ProfitShareConfirmationEvent should not be raised", false, ProfitShareConfirmationEventRaised);
			AssertEquals("IsRegistrySetupEvent should not be raised", false, IsRegistrySetupEventRised);

			AssertEquals("No invoices should be posted", 0, testPostManager.Poster.PostedInvoices.Count);

			testPostManager.ExportAgentPosting -= new ExportAgentPostingEventHandler(TestPostManager_ExportAgentPosting_TestCreditProfitShareWhenPostingAgentInvoices);
			testPostManager.ProfitShareConfirmation -= new ProfitShareConfirmationEventHandler(TestPostManager_ProfitShareConfirmation);
			testPostManager.OnCriticalPostError -= new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_OnCriticalPostErrorForEmptyInvoice);
		}

		bool IsOnCriticalPostErrorEventRaised;
		void TestPostManager_OnCriticalPostError(object sender, CriticalPostingErrorEventArgs e)
		{
			IsOnCriticalPostErrorEventRaised = true;
			if (e is CriticalTransactionPostingErrorEventArgs)
			{
				AssertHasRowError("RowError should be as expected", ((CriticalTransactionPostingErrorEventArgs)e).Header, "The Agent organization ZAgent used for posting Profit Share has an invalid configuration.\r\nIt should be both Receivables and Payables. Make sure the GST is Applicable flag is either checked or unchecked for both roles.\r\nPlease fix organization data and try again");
			}

			if (e is CriticalJobPostingErrorEventArgs)
			{
				AssertHasRowError("RowError should be as expected", ((CriticalJobPostingErrorEventArgs)e).Jobs[0], string.Format("Job {0} status will be changed to INV after posting the first AR Invoice. The Profit/Loss threshold settings require Profit/Loss reason to be set on this job before posting any AR invoices.", ((CriticalJobPostingErrorEventArgs)e).Jobs[0].JH_JobNum));
			}

			if (e is CriticalChargePostingErrorEventArgs)
			{
				AssertEquals(@"Please review the charges being posted.
You may need to post some charges through the Shipment.
Posting Overseas Agent charges is being prevented because you are attempting to post a transaction containing charges for a mix of incompatible branches.
Your transaction is not permitted because there are branches from different Branch Posting Groups.", e.ErrorMessage);
			}
		}

		void TestPostManager_OnCriticalPostErrorForEmptyInvoice(object sender, CriticalPostingErrorEventArgs e)
		{
			TestPostManager_OnCriticalPostError(sender, e);
			if (e is CriticalTransactionPostingErrorEventArgs)
			{
				TransactionHeader header = ((CriticalTransactionPostingErrorEventArgs)e).Header;
				AssertNotNull("Asosiated Header", header);
				AssertNull("Header is an empty invoice", header.Header);
			}
		}

		bool IsRegistrySetupEventRised;
		void TestPostManager_IncorrectRegistrySetup(object sender, IncorrectRegistrySetupEventArgs e)
		{
			IsRegistrySetupEventRised = true;
		}

		void TestPostManager_ExportAgentPosting_TestCreditProfitShareWhenPostingAgentInvoices(object sender, ExportAgentPostingEventArgs e)
		{
			e.OptionSelector.Currency = TestObjectCreator.USD.RX_Code;
			e.OptionSelector.ExchangeRate = 0.78m;
		}

		void TestPostManager_ExportAgentPosting_SetLocalCurrency(object sender, ExportAgentPostingEventArgs e)
		{
			e.OptionSelector.Currency = TestObjectCreator.AUD.RX_Code;
		}

		[DisableZeroExchangeRateOverriding]
		public void TestCreditMasterCollectFreightWhenPostingAgentInvoices()
		{
			#region Setup

			TestObjectCreator.Agent.CompanyData.OB_IsCreditor = ZBool.True;
			TestObjectCreator.Agent.CompanyData.OB_IsDebtor = ZBool.True;
			TestObjectCreator.Agent.CompanyData.SetAPTaxApplicable(ZBool.True);
			TestObjectCreator.Agent.CompanyData.SetARTaxApplicable(ZBool.True);
			TestObjectCreator.Agent.MiscServ.OM_ARWHTApplicable = ZBool.False;
			TestObjectCreator.Agent.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			TestObjectCreator.Agent.CompanyData.OB_RX_NKAPDefltCurrency = ZString.Empty;
			CreateGroupWithPostingStyle(TestObjectCreator.AALSHI, InvoicePostingOptionsList.Codes.FinalInvoiceOnly);

			ZString transportMode = "AIR";
			ZString origin = "AUSYD";
			ZString destination = "USLAX";

			SetupProfitShareRelationship(GlbCompany.CurrentCompany.OrgProxy, TestObjectCreator.Agent, 60m, 40m, origin, destination, transportMode);

			Factory.Save();

			SetupConsolAndShipments(transportMode, origin, destination, TestObjectCreator.Agent);
			Consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);
			Consol.JK_PrepaidCollect = Enterprise.Core.Constants.PaymentType.Collect;
			Consol.SetDefaultReceivingForwarderAddress(TestObjectCreator.Agent);
			Consol.JK_RL_NKLoadPort = origin;
			Consol.JK_RL_NKDischargePort = destination;
			Consol.SetDefaultReceivingForwarderAddress(TestObjectCreator.Agent);
			Consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);

			ApportionmentListing apps = new ApportionmentListing(Factory, Consol);
			JobConsolCost freightCost = apps.CostsCollection.TryAddNew();
			freightCost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			freightCost.E6_RX_NKCurrency = TestObjectCreator.USD.RX_Code;
			freightCost.E6_OH_Creditor = TestObjectCreator.Agent.PK;
			freightCost.E6_ExchangeRate = 0.775m;
			freightCost.E6_OSCostAmount = 250m;
			freightCost.E6_ApportionmentMethod = "SHP";
			freightCost.E6_InvoiceNum = "abcxyz";
			freightCost.E6_InvoiceDate = ZDateTime.Now;
			freightCost.E6_PaymentDate = ZDateTime.Now;

			Factory.Save();

			JobCollection jobs = new JobCollection(Factory);
			jobs.Load();

			Job shipment1Job = jobs.GetJobForOperationsPlugin(Shipment1);
			Job shipment2Job = jobs.GetJobForOperationsPlugin(Shipment2);

			Charge shipment1Charge = shipment1Job.Charges.AddNew();
			Charge shipment2Charge = shipment2Job.Charges.AddNew();

			ExchangeRate shipment1JobExRate = shipment1Job.ExchangeRates.FindByRefCurrency(TestObjectCreator.USD);
			shipment1JobExRate.JF_BaseRate = 0.77m;
			shipment1JobExRate.JF_IsTransformed = true;
			ExchangeRate shipment2JobExRate = shipment2Job.ExchangeRates.FindByRefCurrency(TestObjectCreator.USD);
			shipment2JobExRate.JF_BaseRate = 0.78m;
			shipment2JobExRate.JF_IsTransformed = true;

			//creditor rates for both of the jobs
			var rate1 = shipment1Job.AddCurrency(TestObjectCreator.USD, ExchangeRateValidLedgerEnum.AP);
			rate1.JF_BaseRate = 0.77m;
			rate1.JF_IsTransformed = true;
			var rate2 = shipment2Job.AddCurrency(TestObjectCreator.USD, ExchangeRateValidLedgerEnum.AP);
			rate2.JF_BaseRate = 0.78m;
			rate2.JF_IsTransformed = true;

			shipment1Charge.JR_AC = Env.Registry.FreightChargeCode;
			shipment1Charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			shipment1Charge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			shipment1Charge.JR_OSSellAmt = 180m;
			shipment1Charge.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment1Charge.JR_OSCostAmt = 80m;
			shipment1Charge.JR_IsIncludedInProfitShare = true;
			shipment1Charge.JR_AgentDeclaredSellAmt = 150m;
			shipment1Charge.JR_AgentDeclaredCostAmt = 100m;
			shipment1Charge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			shipment2Charge.JR_AC = Env.Registry.FreightChargeCode;
			shipment2Charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			shipment2Charge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			shipment2Charge.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment2Charge.JR_OSSellAmt = 200m;
			shipment2Charge.JR_OSCostAmt = 100m;
			shipment2Charge.JR_IsIncludedInProfitShare = true;
			shipment2Charge.JR_AgentDeclaredSellAmt = 180m;
			shipment2Charge.JR_AgentDeclaredCostAmt = 120m;
			shipment2Charge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			Factory.Save();

			#endregion

			ConsolInvoicingPostManager testPostManager = new ConsolInvoicingPostManager(Factory, jobs.Cast<Job>(), Consol, apps);
			testPostManager.ProfitShareConfirmation += new ProfitShareConfirmationEventHandler(TestPostManager_ProfitShareConfirmation);
			testPostManager.ExportAgentPosting += new ExportAgentPostingEventHandler(TestPostManager_ExportAgentPosting);
			TransactionCreatorHashtable transactions;
			Factory.SuspendValidation();
			try
			{
				IsTestPostManager_ExportAgentPostingCalled = false;
				transactions = testPostManager.CreateTransactions(JobInvoicingPostingOption.Agent);
				Assert("Postcondition: ExportAgentPosting even is raised.", IsTestPostManager_ExportAgentPostingCalled);
			}
			finally
			{
				Factory.ResumeValidation();
			}

			InvoicingBase[] aPInvoices = transactions.GetAllAPInvoicesAndCreditNotes();
			AssertEquals("Should be 2 invoices", 2, aPInvoices.Length);

			APInvoice invoice1 = (APInvoice)aPInvoices[0];
			AssertEquals("Invoice should be for zero", 0m, invoice1.AH_OSExTaxAmount);
			AssertEquals("Invoice should have 4 lines", 4, invoice1.Lines.Count);

			APInvoice invoice2 = (APInvoice)aPInvoices[1];
			AssertEquals("Invoice should be for zero", 0m, invoice2.AH_OSExTaxAmount);
			AssertEquals("Invoice should have 4 lines", 4, invoice2.Lines.Count);

			InvoicingBaseCollection postedInvoices = testPostManager.Poster.PostedInvoices;
			AssertEquals("Should have posted 1 AR invoice", 1, postedInvoices.Count);
			ARInvoice agentInvoice = (ARInvoice)postedInvoices[0];
			AssertEquals("Should have been posted for the total amount less the profit share credit and the master freight credit", 85.95m, agentInvoice.AH_OSExTaxAmount);
			AssertEquals("Should have 6 lines on invoice", 6, agentInvoice.Lines.Count);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestDoNotCollectFeesOnSingleInvoiceWhenPostingAgentInvoicesWithProfitShareAsAP()
		{
			AssertDoNotCollectFeesOnSingleInvoiceWhenPostingAgentInvoicesWithProfitShareAsAP();
		}

		[DisableZeroExchangeRateOverriding]
		public void TestDoNotCollectFeesOnSingleInvoiceWhenPostingAgentInvoicesWithProfitShareAsAP_PlaceOfSupply()
		{
			using (AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForReceivableTransactions.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				AssertDoNotCollectFeesOnSingleInvoiceWhenPostingAgentInvoicesWithProfitShareAsAP("NSW");
			}
		}

		void AssertDoNotCollectFeesOnSingleInvoiceWhenPostingAgentInvoicesWithProfitShareAsAP(ZString consolCostPlaceOfSupply = default)
		{
			#region Setup

			TestObjectCreator.Agent.CompanyData.OB_IsCreditor = ZBool.True;
			TestObjectCreator.Agent.CompanyData.OB_IsDebtor = ZBool.True;
			TestObjectCreator.Agent.CompanyData.SetAPTaxApplicable(ZBool.True);
			TestObjectCreator.Agent.CompanyData.SetARTaxApplicable(ZBool.True);
			TestObjectCreator.Agent.MiscServ.OM_ARWHTApplicable = ZBool.False;
			TestObjectCreator.Agent.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			TestObjectCreator.Agent.CompanyData.OB_RX_NKAPDefltCurrency = ZString.Empty;
			CreateGroupWithPostingStyle(TestObjectCreator.AALSHI, InvoicePostingOptionsList.Codes.FinalInvoiceOnly);

			TestObjectCreator.Agent.MiscServ.OM_FWBillCollectFeesOnSingleInvoice = ZBool.False;

			ZString transportMode = "AIR";
			ZString origin = "AUSYD";
			ZString destination = "USLAX";

			SetupProfitShareRelationship(GlbCompany.CurrentCompany.OrgProxy, TestObjectCreator.Agent, 60m, 40m, origin, destination, transportMode);

			Factory.Save();

			SetupConsolAndShipments(transportMode, origin, destination, TestObjectCreator.Agent);
			Consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);
			Consol.JK_PrepaidCollect = Enterprise.Core.Constants.PaymentType.Collect;
			Consol.JK_RL_NKLoadPort = origin;
			Consol.JK_RL_NKDischargePort = destination;
			Consol.SetDefaultReceivingForwarderAddress(TestObjectCreator.Agent);
			Consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);

			ApportionmentListing apps = new ApportionmentListing(Factory, Consol);
			JobConsolCost freightCost = apps.CostsCollection.TryAddNew();
			freightCost.E6_AC_ChargeCode = TestObjectCreator.FRT.PK;
			freightCost.E6_RX_NKCurrency = TestObjectCreator.USD.RX_Code;
			freightCost.E6_OH_Creditor = TestObjectCreator.Agent.PK;
			freightCost.E6_ExchangeRate = 0.775m;
			freightCost.E6_OSCostAmount = 250m;
			freightCost.E6_ApportionmentMethod = "SHP";
			freightCost.E6_InvoiceNum = "abcxyz";
			freightCost.E6_InvoiceDate = ZDateTime.Now;
			freightCost.E6_PaymentDate = ZDateTime.Now;
			freightCost.E6_PlaceOfSupply = consolCostPlaceOfSupply;

			Factory.Save();

			JobCollection jobs = new JobCollection(Factory);
			jobs.Load();

			Job shipment1Job = jobs.GetJobForOperationsPlugin(Shipment1);
			Job shipment2Job = jobs.GetJobForOperationsPlugin(Shipment2);

			Charge shipment1Charge = shipment1Job.Charges.AddNew();
			Charge shipment1AgentCharge = shipment1Job.Charges.AddNew();
			Charge shipment2Charge = shipment2Job.Charges.AddNew();
			Charge shipment2AgentCharge = shipment2Job.Charges.AddNew();

			ExchangeRate shipment1JobExRate = shipment1Job.ExchangeRates.FindByRefCurrency(TestObjectCreator.USD);
			shipment1JobExRate.JF_BaseRate = 0.777m;
			shipment1JobExRate.JF_IsTransformed = true;
			ExchangeRate shipment2JobExRate = shipment2Job.ExchangeRates.FindByRefCurrency(TestObjectCreator.USD);
			shipment2JobExRate.JF_BaseRate = 0.778m;
			shipment2JobExRate.JF_IsTransformed = true;

			var rate1 = shipment1Job.AddCurrency(TestObjectCreator.USD, ExchangeRateValidLedgerEnum.AP);
			rate1.JF_BaseRate = 0.777m;
			rate1.JF_IsTransformed = true;
			var rate2 = shipment2Job.AddCurrency(TestObjectCreator.USD, ExchangeRateValidLedgerEnum.AP);
			rate2.JF_BaseRate = 0.778m;
			rate2.JF_IsTransformed = true;

			shipment1Charge.JR_AC = Env.Registry.FreightChargeCode;
			shipment1Charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			shipment1Charge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			shipment1Charge.JR_OSSellAmt = 180m;
			shipment1Charge.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment1Charge.JR_OSCostAmt = 80m;
			shipment1Charge.JR_IsIncludedInProfitShare = true;
			shipment1Charge.JR_AgentDeclaredSellAmt = 150m;
			shipment1Charge.JR_AgentDeclaredCostAmt = 100m;
			shipment1Charge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			shipment1AgentCharge.JR_AC = TestObjectCreator.CC1.PK;
			shipment1AgentCharge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			shipment1AgentCharge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			shipment1AgentCharge.JR_OSSellAmt = 280m;
			shipment1AgentCharge.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment1AgentCharge.JR_OSCostAmt = 160m;
			shipment1AgentCharge.JR_IsIncludedInProfitShare = true;
			shipment1AgentCharge.JR_AgentDeclaredSellAmt = 200m;
			shipment1AgentCharge.JR_AgentDeclaredCostAmt = 160m;
			shipment1AgentCharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			shipment2Charge.JR_AC = Env.Registry.FreightChargeCode;
			shipment2Charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			shipment2Charge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			shipment2Charge.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment2Charge.JR_OSSellAmt = 200m;
			shipment2Charge.JR_OSCostAmt = 100m;
			shipment2Charge.JR_IsIncludedInProfitShare = true;
			shipment2Charge.JR_AgentDeclaredSellAmt = 180m;
			shipment2Charge.JR_AgentDeclaredCostAmt = 120m;
			shipment2Charge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			shipment2AgentCharge.JR_AC = TestObjectCreator.CC1.PK;
			shipment2AgentCharge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			shipment2AgentCharge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			shipment2AgentCharge.JR_OSSellAmt = 280m;
			shipment2AgentCharge.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment2AgentCharge.JR_OSCostAmt = 160m;
			shipment2AgentCharge.JR_IsIncludedInProfitShare = true;
			shipment2AgentCharge.JR_AgentDeclaredSellAmt = 200m;
			shipment2AgentCharge.JR_AgentDeclaredCostAmt = 160m;
			shipment2AgentCharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			Factory.Save();

			#endregion

			AssertEquals("E6_IsForCollectInvoice", true, freightCost.E6_IsForCollectInvoice);
			AssertEquals("CreateProfitShareAsAR", false, AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.Value);

			ConsolInvoicingPostManager testPostManager = new ConsolInvoicingPostManager(Factory, jobs.Cast<Job>(), Consol, apps);
			testPostManager.ProfitShareConfirmation += new ProfitShareConfirmationEventHandler(TestPostManager_ProfitShareConfirmation);
			testPostManager.ExportAgentPosting += new ExportAgentPostingEventHandler(TestPostManager_ExportAgentPosting);
			TransactionCreatorHashtable transactions = testPostManager.CreateTransactions(JobInvoicingPostingOption.Agent);

			InvoicingBase[] aPInvoices = transactions.GetAllAPInvoicesAndCreditNotes();
			AssertEquals("Should be 2 invoices", 2, aPInvoices.Length);

			APInvoice invoice1 = (APInvoice)aPInvoices[0];
			AssertEquals("Invoice should be for zero", 0m, invoice1.AH_OSExTaxAmount);
			AssertEquals("AH_PostedToEFT", true, invoice1.AH_PostedToEFT);
			AssertEquals("Exchange Rate is recaculated becasue of enabled AH_PostedToEFT(UseJobExchangeRate), 0 / 0.", 1m, invoice1.AH_ExchangeRate);
			AssertEquals("Invoice should have 4 lines", 4, invoice1.Lines.Count);
			var lines1 = invoice1.Lines.ToArray<APInvoiceLine>();
			AssertEquals("Contains 125 lines", 2, lines1.Count(x => x.AL_OSExTaxAmount == 125m));
			AssertEquals("Contains -125 lines", 2, lines1.Count(x => x.AL_OSExTaxAmount == -125m));

			APInvoice invoice2 = (APInvoice)aPInvoices[1];
			AssertEquals("Invoice should be for zero", 0m, invoice2.AH_OSExTaxAmount);
			AssertEquals("AH_PostedToEFT", true, invoice2.AH_PostedToEFT);
			AssertEquals("Exchange Rate is recaculated becasue of enabled AH_PostedToEFT(UseJobExchangeRate), 0 / 0.", 1m, invoice2.AH_ExchangeRate);
			AssertEquals("Invoice should have 4 lines", 4, invoice2.Lines.Count);
			var lines2 = invoice2.Lines.ToArray<APInvoiceLine>();
			AssertNotNull("Contains 40.24 line", lines2.FirstOrDefault(x => x.AL_OSExTaxAmount == 40.24m));
			AssertNotNull("Contains 36.14 line", lines2.FirstOrDefault(x => x.AL_OSExTaxAmount == 36.14m));
			AssertNotNull("Contains -40.24 line", lines2.FirstOrDefault(x => x.AL_OSExTaxAmount == -40.24m));
			AssertNotNull("Contains -36.14 line", lines2.FirstOrDefault(x => x.AL_OSExTaxAmount == -36.14m));

			InvoicingBaseCollection postedInvoices = testPostManager.Poster.PostedInvoices;
			AssertEquals("Should have posted 3 AR invoices", 3, postedInvoices.Count);
			var found = postedInvoices.Find(new ZQuery(AccTransactionHeaderSchema.AH_OSTotal, 488m));
			Assert("Found Invoice for 488", found != null && found.Any());
			var agentInvoice = ((ARInvoice)found[0]);
			AssertEquals("ExchangeRate", 0.775m, agentInvoice.AH_ExchangeRate);
			AssertEquals("AH_OSExTaxAmount", 460m, agentInvoice.AH_OSExTaxAmount);
			AssertEquals("Should have 2 lines on invoice", 2, agentInvoice.Lines.Count);

			found = postedInvoices.Find(new ZQuery(AccTransactionHeaderSchema.AH_OSTotal, 508m));
			Assert("Found Invoice for 508", found != null && found.Any());
			agentInvoice = ((ARInvoice)found[0]);
			AssertEquals("ExchangeRate", 0.775m, agentInvoice.AH_ExchangeRate);
			AssertEquals("AH_OSExTaxAmount", 480m, agentInvoice.AH_OSExTaxAmount);
			AssertEquals("Should have 2 lines on invoice", 2, agentInvoice.Lines.Count);

			found = postedInvoices.Find(new ZQuery(AccTransactionHeaderSchema.AH_OSTotal, -326.38m));
			Assert("Found Invoice for -326.38", found != null && found.Any());
			var agentCreditNote = ((ARCreditNote)found[0]);
			AssertEquals("ExchangeRate", 0.775m, agentCreditNote.AH_ExchangeRate);
			AssertEquals("AH_OSExTaxAmount", -326.38m, agentCreditNote.AH_OSTotal);
			AssertEquals("AH_LocalTotalAmount", -421.13m, agentCreditNote.AH_OutstandingAmount);
			AssertEquals("Should have 4 lines on invoice", 4, agentCreditNote.Lines.Count);
			AssertEquals("AH_PlaceOfSupply", consolCostPlaceOfSupply, agentCreditNote.AH_PlaceOfSupply);
		}

		public void TestDoNotCollectFeesOnSingleInvoiceWhenPostingAgentInvoicesWithProfitShareAsAR()
		{
			#region Setup

			TestObjectCreator.Agent.CompanyData.OB_IsCreditor = ZBool.True;
			TestObjectCreator.Agent.CompanyData.OB_IsDebtor = ZBool.True;
			TestObjectCreator.Agent.CompanyData.SetAPTaxApplicable(ZBool.True);
			TestObjectCreator.Agent.CompanyData.SetARTaxApplicable(ZBool.True);
			TestObjectCreator.Agent.MiscServ.OM_ARWHTApplicable = ZBool.False;
			TestObjectCreator.Agent.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			TestObjectCreator.Agent.CompanyData.OB_RX_NKAPDefltCurrency = ZString.Empty;
			CreateGroupWithPostingStyle(TestObjectCreator.AALSHI, InvoicePostingOptionsList.Codes.FinalInvoiceOnly);

			TestObjectCreator.Agent.MiscServ.OM_FWBillCollectFeesOnSingleInvoice = ZBool.False;

			ZString transportMode = "AIR";
			ZString origin = "AUSYD";
			ZString destination = "USLAX";

			SetupProfitShareRelationship(GlbCompany.CurrentCompany.OrgProxy, TestObjectCreator.Agent, 60m, 40m, origin, destination, transportMode);

			Factory.Save();

			SetupConsolAndShipments(transportMode, origin, destination, TestObjectCreator.Agent);
			Consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);
			Consol.JK_PrepaidCollect = Enterprise.Core.Constants.PaymentType.Collect;
			Consol.JK_RL_NKLoadPort = origin;
			Consol.JK_RL_NKDischargePort = destination;
			Consol.SetDefaultReceivingForwarderAddress(TestObjectCreator.Agent);
			Consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);

			ApportionmentListing apps = new ApportionmentListing(Factory, Consol);
			JobConsolCost freightCost = apps.CostsCollection.TryAddNew();
			freightCost.E6_AC_ChargeCode = TestObjectCreator.FRT.PK;
			freightCost.E6_RX_NKCurrency = TestObjectCreator.USD.RX_Code;
			freightCost.E6_OH_Creditor = TestObjectCreator.Agent.PK;
			freightCost.E6_ExchangeRate = 0.775m;
			freightCost.E6_OSCostAmount = 250m;
			freightCost.E6_ApportionmentMethod = "SHP";
			freightCost.E6_InvoiceNum = "abcxyz";
			freightCost.E6_InvoiceDate = ZDateTime.Now;
			freightCost.E6_PaymentDate = ZDateTime.Now;

			Factory.Save();

			JobCollection jobs = new JobCollection(Factory);
			jobs.Load();

			Job shipment1Job = jobs.GetJobForOperationsPlugin(Shipment1);
			Job shipment2Job = jobs.GetJobForOperationsPlugin(Shipment2);

			Charge shipment1Charge = shipment1Job.Charges.AddNew();
			Charge shipment1AgentCharge = shipment1Job.Charges.AddNew();
			Charge shipment2Charge = shipment2Job.Charges.AddNew();
			Charge shipment2AgentCharge = shipment2Job.Charges.AddNew();

			ExchangeRate shipment1JobExRate = shipment1Job.ExchangeRates.FindByRefCurrency(TestObjectCreator.USD);
			shipment1JobExRate.JF_BaseRate = 0.777m;
			shipment1JobExRate.JF_IsTransformed = true;
			ExchangeRate shipment2JobExRate = shipment2Job.ExchangeRates.FindByRefCurrency(TestObjectCreator.USD);
			shipment2JobExRate.JF_BaseRate = 0.778m;
			shipment2JobExRate.JF_IsTransformed = true;

			var rate1 = shipment1Job.AddCurrency(TestObjectCreator.USD, ExchangeRateValidLedgerEnum.AP);
			rate1.JF_BaseRate = 0.777m;
			rate1.JF_IsTransformed = true;
			var rate2 = shipment2Job.AddCurrency(TestObjectCreator.USD, ExchangeRateValidLedgerEnum.AP);
			rate2.JF_BaseRate = 0.778m;
			rate2.JF_IsTransformed = true;

			shipment1Charge.JR_AC = Env.Registry.FreightChargeCode;
			shipment1Charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			shipment1Charge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			shipment1Charge.JR_OSSellAmt = 180m;
			shipment1Charge.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment1Charge.JR_OSCostAmt = 80m;
			shipment1Charge.JR_IsIncludedInProfitShare = true;
			shipment1Charge.JR_AgentDeclaredSellAmt = 150m;
			shipment1Charge.JR_AgentDeclaredCostAmt = 100m;
			shipment1Charge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			shipment1AgentCharge.JR_AC = TestObjectCreator.CC1.PK;
			shipment1AgentCharge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			shipment1AgentCharge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			shipment1AgentCharge.JR_OSSellAmt = 280m;
			shipment1AgentCharge.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment1AgentCharge.JR_OSCostAmt = 160m;
			shipment1AgentCharge.JR_IsIncludedInProfitShare = true;
			shipment1AgentCharge.JR_AgentDeclaredSellAmt = 200m;
			shipment1AgentCharge.JR_AgentDeclaredCostAmt = 160m;
			shipment1AgentCharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			shipment2Charge.JR_AC = Env.Registry.FreightChargeCode;
			shipment2Charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			shipment2Charge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			shipment2Charge.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment2Charge.JR_OSSellAmt = 200m;
			shipment2Charge.JR_OSCostAmt = 100m;
			shipment2Charge.JR_IsIncludedInProfitShare = true;
			shipment2Charge.JR_AgentDeclaredSellAmt = 180m;
			shipment2Charge.JR_AgentDeclaredCostAmt = 120m;
			shipment2Charge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			shipment2AgentCharge.JR_AC = TestObjectCreator.CC1.PK;
			shipment2AgentCharge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			shipment2AgentCharge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			shipment2AgentCharge.JR_OSSellAmt = 280m;
			shipment2AgentCharge.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment2AgentCharge.JR_OSCostAmt = 160m;
			shipment2AgentCharge.JR_IsIncludedInProfitShare = true;
			shipment2AgentCharge.JR_AgentDeclaredSellAmt = 200m;
			shipment2AgentCharge.JR_AgentDeclaredCostAmt = 160m;
			shipment2AgentCharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Factory.Save();

			#endregion

			AssertEquals("E6_IsForCollectInvoice", true, freightCost.E6_IsForCollectInvoice);

			ConsolInvoicingPostManager testPostManager = new ConsolInvoicingPostManager(Factory, jobs.Cast<Job>(), Consol, apps);
			testPostManager.ProfitShareConfirmation += new ProfitShareConfirmationEventHandler(TestPostManager_ProfitShareConfirmation);
			testPostManager.ExportAgentPosting += new ExportAgentPostingEventHandler(TestPostManager_ExportAgentPosting);
			TransactionCreatorHashtable transactions = testPostManager.CreateTransactions(JobInvoicingPostingOption.Agent);

			InvoicingBase[] aPInvoices = transactions.GetAllAPInvoicesAndCreditNotes();
			AssertEquals("Should be 2 invoices", 1, aPInvoices.Length);

			APInvoice invoice1 = (APInvoice)aPInvoices[0];
			AssertEquals("Invoice should be for zero", 0m, invoice1.AH_OSExTaxAmount);
			AssertEquals("Invoice should have 4 lines", 4, invoice1.Lines.Count);

			InvoicingBaseCollection postedInvoices = testPostManager.Poster.PostedInvoices;
			AssertEquals("Should have posted 5 AR invoicea and Credit Notes", 5, postedInvoices.Count);
			var found = postedInvoices.Find(new ZQuery(AccTransactionHeaderSchema.AH_OSTotal, -250m));
			Assert("Found Invoice for -250", found != null && found.Any());
			var agentCreditNote = ((ARCreditNote)found[0]);
			AssertEquals("ExchangeRate", 0.775m, agentCreditNote.AH_ExchangeRate);
			AssertEquals("AH_OSExTaxAmount", -250m, agentCreditNote.AH_OSTotal);
			AssertEquals("Should have 2 lines", 2, agentCreditNote.Lines.Count);

			found = postedInvoices.Find(new ZQuery(AccTransactionHeaderSchema.AH_OSTotal, -46.17m));
			Assert("Found CreditNote for 46.17", found != null && found.Any());
			agentCreditNote = (ARCreditNote)found[0];
			AssertEquals("ExchangeRate", 1m, agentCreditNote.AH_ExchangeRate);
			AssertEquals("AH_OSExTaxAmount", 46.17m, agentCreditNote.AH_OSExTaxAmount);
			AssertEquals("Should have 1 line", 1, agentCreditNote.Lines.Count);

			found = postedInvoices.Find(new ZQuery(AccTransactionHeaderSchema.AH_OSTotal, 508m));
			Assert("Found Invoice for 508", found != null && found.Any());
			var agentInvoice = ((ARInvoice)found[0]);
			AssertEquals("ExchangeRate", 0.775m, agentInvoice.AH_ExchangeRate);
			AssertEquals("AH_OSExTaxAmount", 480m, agentInvoice.AH_OSExTaxAmount);
			AssertEquals("Should have 2 lines", 2, agentInvoice.Lines.Count);

			found = postedInvoices.Find(new ZQuery(AccTransactionHeaderSchema.AH_OSTotal, 488m));
			Assert("Found Invoice for 488", found != null && found.Any());
			agentInvoice = ((ARInvoice)found[0]);
			AssertEquals("ExchangeRate", 0.775m, agentInvoice.AH_ExchangeRate);
			AssertEquals("AH_OSExTaxAmount", 460m, agentInvoice.AH_OSExTaxAmount);
			AssertEquals("Should have 2 lines", 2, agentInvoice.Lines.Count);

			found = postedInvoices.Find(new ZQuery(AccTransactionHeaderSchema.AH_OSTotal, -51.16m));
			Assert("Found CreditNote for 51.16", found != null && found.Any());
			agentCreditNote = (ARCreditNote)found[0];
			AssertEquals("ExchangeRate", 1m, agentCreditNote.AH_ExchangeRate);
			AssertEquals("AH_OSExTaxAmount", 51.16m, agentCreditNote.AH_OSExTaxAmount);
			AssertEquals("Should have 1 line", 1, agentCreditNote.Lines.Count);
		}

		void TestPostManager_ExportAgentPosting(object sender, ExportAgentPostingEventArgs e)
		{
			Assert("OptionSelector must not have suspended validation", !e.OptionSelector.IsValidationSuspended);

			e.OptionSelector.Currency = TestObjectCreator.USD.RX_Code;
			e.OptionSelector.ExchangeRate = 0.775m;

			IsTestPostManager_ExportAgentPostingCalled = true;
		}

		bool IsTestPostManager_ExportAgentPostingCalled;
		ForwardingConsol Consol;
		ForwardingShipment Shipment1;
		ForwardingShipment Shipment2;

		void SetupConsolAndShipments(ZString transportMode, ZString origin, ZString destination, OrgHeader agent)
		{
			Consol = Factory.New<ForwardingConsol>();
			Factory.Save();

			Shipment1 = Consol.Shipments.AddNew();
			Shipment1.JS_TransportMode = transportMode;
			Shipment1.JS_RL_NKOrigin = origin;
			Shipment1.JS_RL_NKDestination = destination;
			Shipment1.JS_OH_DeliveryAgent = agent.PK;

			Shipment2 = Consol.Shipments.AddNew();
			Shipment2.JS_TransportMode = transportMode;
			Shipment2.JS_RL_NKOrigin = origin;
			Shipment2.JS_RL_NKDestination = destination;
			Shipment2.JS_OH_DeliveryAgent = agent.PK;
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

			OrgProfitShareParty sendParty = profitShare1.PartyDetails.AddNew();
			sendParty.PS_PartyType = "SEN";
			sendParty.PS_PartyProfitSharePercent = sendingProfitSharePercentage;

			OrgProfitShareParty rcvParty = profitShare1.PartyDetails.AddNew();
			rcvParty.PS_PartyType = "RCV";
			rcvParty.PS_PartyProfitSharePercent = receivingProfitSharePercentage;
		}

		#endregion

		#region TEST: Duplicate AP Invoice Posting Error Handling

		public void TestValidateDuplicateAPInvoiceNumberOnPosting()
		{
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_OH = Creditor1.PK;
			Factory.Save();
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			Job shipment1Job = Job.CreateWithMutex(Factory, shipment1);
			shipment1Job.PlugInData = shipment1;
			shipment1Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();

			JobCollection jobs = new JobCollection(Factory);
			jobs.Add(shipment1Job);

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = CC1.PK;
			cost.E6_OH_Creditor = Creditor1.PK;
			cost.E6_InvoiceNum = invoice.AH_TransactionNum;
			cost.E6_InvoiceDate = ZDateTime.Today;
			cost.E6_OSCostAmount = 500m;
			cost.E6_ApportionmentMethod = "SHP";
			Factory.Save();

			shipment1Job.Charges.Load();

			ConsolInvoicingPostManager testPostManager = new ConsolInvoicingPostManager(Factory, jobs.Cast<Job>(), consol, apps);
			testPostManager.OnCriticalPostError += new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_DuplicateAPTransactionNumber);
			testPostManager.CreateTransactions(JobInvoicingPostingOption.Costs);
			Assert(DuplicateAPInvoiceNumberError);

			AssertEquals("ExceptionReporter should have no exceptions caught", 0, ExceptionReporterTestListener.Instance.Count);

			DuplicateAPInvoiceNumberError = false;
			testPostManager.OnCriticalPostError -= new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_DuplicateAPTransactionNumber);
			shipment1Job.Charges[0].JR_APInvoiceNum = "XYZASDEVF";
			testPostManager = new ConsolInvoicingPostManager(Factory, jobs.Cast<Job>(), consol, apps);
			testPostManager.OnCriticalPostError += new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_DuplicateAPTransactionNumber);
			testPostManager.CreateTransactions(JobInvoicingPostingOption.Costs);
			testPostManager.OnCriticalPostError -= new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_DuplicateAPTransactionNumber);
			Assert(!DuplicateAPInvoiceNumberError);
		}

		void TestPostManager_DuplicateAPTransactionNumber(object sender, CriticalPostingErrorEventArgs e)
		{
			if (e is CriticalTransactionPostingErrorEventArgs)
			{
				InvoiceBaseValidation invoiceValidation = ((CriticalTransactionPostingErrorEventArgs)e).Header.Validation as InvoiceBaseValidation;
				foreach (INotification notification in ((CriticalTransactionPostingErrorEventArgs)e).Header.RowErrors)
				{
					if (invoiceValidation != null && notification.Message.Contains(" is already used "))
					{
						DuplicateAPInvoiceNumberError = true;
					}
				}
			}
		}

		bool DuplicateAPInvoiceNumberError;

		#endregion

		#region TEST: Invoices and CreditNotes without Lines are detected as Critical Errors

		public void TestInvoicesAndCreditNotesWithoutLinesAreDetectedAsCriticalErrors()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			Job shipment1Job = Job.CreateWithMutex(Factory, shipment1);
			shipment1Job.PlugInData = shipment1;
			shipment1Job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			JobCollection jobs = new JobCollection(Factory);
			jobs.Add(shipment1Job);
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);

			ConsolInvoicingPostManagerForTest testPostManager = new ConsolInvoicingPostManagerForTest(Factory, jobs.Cast<Job>(), consol, apps);
			testPostManager.OnCriticalPostError += new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_InvoicesAndCreditNotesWithoutLines);

			TransactionCreatorHashtable transactions = new TransactionCreatorHashtable();
			APInvoice aPInvoice = Factory.NewWithValidTestData<APInvoice>();
			aPInvoice.AH_OH = Creditor1.PK;

			transactions.AddAPInvoice(aPInvoice, Creditor1.OH_Code, aPInvoice.AH_TransactionNum);
			aPInvoice.Lines.RemoveAndDeleteAll();

			testPostManager.CheckForCriticalErrors_Exposed(transactions);
			Assert("Error should be found", InvoiceCreditNoteWithoutLinesError);
			Assert("Error should be reported", ExceptionReporterTestListener.Instance.Count > 0);
			InvoiceCreditNoteWithoutLinesError = false;
			ExceptionReporterTestListener.Instance.Clear();
			CargoWise.Common.ErrorReporter.Clear();

			transactions = new TransactionCreatorHashtable();
			APCreditNote aPCreditNote = Factory.NewWithValidTestData<APCreditNote>();
			aPCreditNote.AH_OH = Creditor1.PK;

			transactions.AddAPCreditNote(aPCreditNote, Creditor1.OH_Code, aPCreditNote.AH_TransactionNum);
			aPCreditNote.Lines.RemoveAndDeleteAll();

			testPostManager.CheckForCriticalErrors_Exposed(transactions);
			Assert("Error should be found", InvoiceCreditNoteWithoutLinesError);
			Assert("Error should be reported", ExceptionReporterTestListener.Instance.Count > 0);
			InvoiceCreditNoteWithoutLinesError = false;
			ExceptionReporterTestListener.Instance.Clear();
			CargoWise.Common.ErrorReporter.Clear();

			transactions = new TransactionCreatorHashtable();
			ARInvoice aRInvoice = Factory.NewWithValidTestData<ARInvoice>();

			transactions.AddARInvoice(aRInvoice);
			aRInvoice.Lines.RemoveAndDeleteAll();

			testPostManager.CheckForCriticalErrors_Exposed(transactions);
			Assert("Error should be found", InvoiceCreditNoteWithoutLinesError);
			Assert("Error should be reported", ExceptionReporterTestListener.Instance.Count > 0);
			InvoiceCreditNoteWithoutLinesError = false;
			ExceptionReporterTestListener.Instance.Clear();
			CargoWise.Common.ErrorReporter.Clear();

			transactions = new TransactionCreatorHashtable();
			ARCreditNote aRCreditNote = Factory.NewWithValidTestData<ARCreditNote>();

			transactions.AddARInvoice(aRCreditNote);
			aRCreditNote.Lines.RemoveAndDeleteAll();

			testPostManager.CheckForCriticalErrors_Exposed(transactions);
			Assert("Error should be found", InvoiceCreditNoteWithoutLinesError);
			Assert("Error should be reported", ExceptionReporterTestListener.Instance.Count > 0);
			ExceptionReporterTestListener.Instance.Clear();
			CargoWise.Common.ErrorReporter.Clear();

			testPostManager.OnCriticalPostError -= new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_InvoicesAndCreditNotesWithoutLines);
			shipment1Job.Dispose();
		}

		void TestPostManager_InvoicesAndCreditNotesWithoutLines(object sender, CriticalPostingErrorEventArgs e)
		{
			foreach (INotification notification in ((CriticalTransactionPostingErrorEventArgs)e).Header.RowErrors)
			{
				if (notification.Message.Contains(" has no lines and cannot be posted"))
				{
					InvoiceCreditNoteWithoutLinesError = true;
				}
			}
		}

		bool InvoiceCreditNoteWithoutLinesError;

		class ConsolInvoicingPostManagerForTest : ConsolInvoicingPostManager
		{
			public ConsolInvoicingPostManagerForTest(BusinessObjectFactory fallbackFactory, IEnumerable<Job> jobs, IJobCostingPlugIn consol, ApportionmentListing apportionments)
					: base(fallbackFactory, jobs, consol, apportionments)
			{
			}

			public void CheckForCriticalErrors_Exposed(TransactionCreatorHashtable transactions)
			{
				CheckForCriticalErrors(transactions);
			}
		}

		#endregion

		#region TEST: Gateway posting option only posts gateway agent charges

		public void TestGatewayOptionOnlyPostsGatewayCharges()
		{
			var receivingGatewayCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("GTWORG", true, true);
			var receivingGatewayCompany = TestObjectCreator.CreateNewCompany("CGW", orgProxy: receivingGatewayCompanyOrgProxy);
			var localClient = TestObjectCreator.CreateOrgHeader("LOCALO", true, true);
			var agent = TestObjectCreator.CreateOrgHeader("AGENTO", true, true);
			var consol = TestObjectCreator.CreateGatewayConsol(receivingGatewayCompany: receivingGatewayCompany);
			consol.JK_SendingForwarderHandlingType = ZString.Empty;
			consol.JK_ReceivingForwarderHandlingType = ZString.Empty;
			var shipment = TestObjectCreator.CreateShipment("S00123", consol);

			var job = TestObjectCreator.CreateJob(shipment, false);
			job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;
			job.JH_OA_AgentCollectAddr = agent.MainAddress.PK;

			var regularCharge = job.Charges.AddNew();
			regularCharge.JR_OH_SellAccount = localClient.PK;
			regularCharge.JR_AC = TestObjectCreator.CC1.PK;
			regularCharge.JR_OSSellAmt = 100;
			var agentCharge = job.Charges.AddNew();
			agentCharge.JR_OH_SellAccount = agent.PK;
			agentCharge.JR_AC = TestObjectCreator.CC1.PK;
			agentCharge.JR_OSSellAmt = 100;
			var gatewayCharge = job.Charges.AddNew();
			gatewayCharge.JR_OH_SellAccount = receivingGatewayCompanyOrgProxy.PK;
			gatewayCharge.JR_AC = TestObjectCreator.CC1.PK;
			gatewayCharge.JR_OSSellAmt = 100;
			Factory.Save();

			var transactionCreator = new ConsolInvoicingPostManager(Factory, new[] { job }, consol, new ApportionmentListing(Factory, consol));
			var transactions = transactionCreator.CreateTransactions(JobInvoicingPostingOption.Gateway);
			AssertEquals("Should not post any transactions when no valid debtor", 0, transactions.Count);

			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			transactionCreator = new ConsolInvoicingPostManager(Factory, new[] { job }, consol, new ApportionmentListing(Factory, consol));
			transactions = transactionCreator.CreateTransactions(JobInvoicingPostingOption.Gateway);
			Factory.Save();
			Assert("Should not post non-gateway charge", !regularCharge.IsRevenuePosted);
			Assert("Should not post non-gateway charge", !agentCharge.IsRevenuePosted);
			Assert("Should post gateway charge", gatewayCharge.IsRevenuePosted);
		}

		#endregion

		public void TestPostAgentConsolCostsCreditNote()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.SetDefaultReceivingForwarderAddress(TestObjectCreator.AALSHI);

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost negativeCost = apps.CostsCollection.TryAddNew();
			negativeCost.E6_AC_ChargeCode = TestObjectCreator.MRG100.PK;
			negativeCost.E6_OSCostAmount = -100m;
			negativeCost.E6_ApportionmentMethod = "SHP";
			negativeCost.E6_OH_Creditor = TestObjectCreator.ZECTRA.PK;
			negativeCost.E6_InvoiceNum = "ABC123";
			negativeCost.E6_InvoiceDate = ZDateTime.Now;
			negativeCost.E6_Sequence = 1;
			negativeCost.ChargeCode.AC_PrintSequence = 4;
			JobConsolCost positiveCost = apps.CostsCollection.TryAddNew();
			positiveCost.E6_AC_ChargeCode = TestObjectCreator.DSBChargeCode.PK;
			positiveCost.E6_OSCostAmount = 50m;
			positiveCost.E6_ApportionmentMethod = "SHP";
			positiveCost.E6_OH_Creditor = TestObjectCreator.ZECTRA.PK;
			positiveCost.E6_InvoiceNum = "ABC123";
			positiveCost.E6_InvoiceDate = ZDateTime.Now;
			positiveCost.E6_Sequence = 2;
			positiveCost.ChargeCode.AC_PrintSequence = 4;
			Factory.Save();

			JobCollection jobs = new JobCollection(Factory);
			jobs.Load();
			ConsolInvoicingPostManager transactionCreator = new ConsolInvoicingPostManager(Factory, jobs.Cast<Job>(), consol, apps);
			TransactionCreatorHashtable transactions = transactionCreator.CreateTransactions(JobInvoicingPostingOption.Costs);

			Factory.Save();

			InvoicingBase[] aPInvoices = transactions.GetAllAPInvoicesAndCreditNotes();
			AssertEquals(1, aPInvoices.Length);
		}

		public void TestCancelPosting()
		{
			#region Setup

			TestObjectCreator.Agent.CompanyData.OB_IsCreditor = ZBool.True;
			TestObjectCreator.Agent.CompanyData.OB_IsDebtor = ZBool.True;
			TestObjectCreator.Agent.MiscServ.OM_ARWHTApplicable = ZBool.False;
			CreateGroupWithPostingStyle(TestObjectCreator.AALSHI, InvoicePostingOptionsList.Codes.FinalInvoiceOnly);

			ZString transportMode = "AIR";
			ZString origin = "AUSYD";
			ZString destination = "USLAX";

			SetupProfitShareRelationship(GlbCompany.CurrentCompany.OrgProxy, TestObjectCreator.Agent, 60m, 40m, origin, destination, transportMode);

			SetupConsolAndShipments(transportMode, origin, destination, TestObjectCreator.Agent);
			Consol.SetDefaultReceivingForwarderAddress(TestObjectCreator.Agent);
			Consol.JK_PrepaidCollect = Enterprise.Core.Constants.PaymentType.Collect;

			ApportionmentListing apps = new ApportionmentListing(Factory, Consol);
			JobConsolCost freightCost = apps.CostsCollection.TryAddNew();
			freightCost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			freightCost.E6_RX_NKCurrency = TestObjectCreator.USD.RX_Code;
			freightCost.E6_ExchangeRate = 0.65m;
			freightCost.E6_OSCostAmount = 200m;
			freightCost.E6_ApportionmentMethod = "SHP";

			Factory.Save();

			JobCollection jobs = new JobCollection(Factory);
			jobs.Load();
			AssertEquals(2, jobs.Count);
			Job shipment1Job = jobs.GetJobForOperationsPlugin(Shipment1);
			Job shipment2Job = jobs.GetJobForOperationsPlugin(Shipment2);

			Charge shipment1Charge = shipment1Job.Charges.AddNew();
			Charge shipment2Charge = shipment2Job.Charges.AddNew();

			shipment1Charge.JR_AC = Env.Registry.FreightChargeCode;
			shipment1Charge.JR_OSSellAmt = 180m;
			shipment1Charge.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment1Charge.JR_OSCostAmt = 80m;
			shipment1Charge.JR_AgentDeclaredSellAmt = 150m;
			shipment1Charge.JR_AgentDeclaredCostAmt = 100m;

			shipment2Charge.JR_AC = Env.Registry.FreightChargeCode;
			shipment2Charge.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment2Charge.JR_OSSellAmt = 200m;
			shipment2Charge.JR_OSCostAmt = 100m;
			shipment2Charge.JR_AgentDeclaredSellAmt = 180m;
			shipment2Charge.JR_AgentDeclaredCostAmt = 120m;

			Factory.Save();

			#endregion

			ConsolInvoicingPostManager testPostManager = new ConsolInvoicingPostManager(Factory, jobs.Cast<Job>(), Consol, apps);
			testPostManager.SetCancelPostingForTestOnly(true);
			testPostManager.CreateTransactions(JobInvoicingPostingOption.All);

			AssertEquals("Shouldn't be any invoices posted", 0, testPostManager.Poster.PostedInvoices.Count);

			testPostManager.SetCancelPostingForTestOnly(false);
			testPostManager.CreateTransactions(JobInvoicingPostingOption.All);

			Assert("Should be invoices posted", testPostManager.Poster.PostedInvoices.Count > 0);
		}

		public void TestRollBackPosting()
		{
			#region Setup

			TestObjectCreator.Agent.CompanyData.OB_IsCreditor = ZBool.True;
			TestObjectCreator.Agent.CompanyData.OB_IsDebtor = ZBool.True;
			TestObjectCreator.Agent.MiscServ.OM_ARWHTApplicable = ZBool.False;
			CreateGroupWithPostingStyle(TestObjectCreator.AALSHI, InvoicePostingOptionsList.Codes.FinalInvoiceOnly);

			ZString transportMode = "AIR";
			ZString origin = "AUSYD";
			ZString destination = "USLAX";

			SetupProfitShareRelationship(GlbCompany.CurrentCompany.OrgProxy, TestObjectCreator.Agent, 60m, 40m, origin, destination, transportMode);

			SetupConsolAndShipments(transportMode, origin, destination, TestObjectCreator.Agent);
			Consol.SetDefaultReceivingForwarderAddress(TestObjectCreator.Agent);
			Consol.JK_PrepaidCollect = Enterprise.Core.Constants.PaymentType.Collect;

			ApportionmentListing apps = new ApportionmentListing(Factory, Consol);
			JobConsolCost freightCost = apps.CostsCollection.TryAddNew();
			freightCost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			freightCost.E6_RX_NKCurrency = TestObjectCreator.AUD.RX_Code;
			freightCost.E6_OSCostAmount = 200m;
			freightCost.E6_ApportionmentMethod = "SHP";

			Factory.Save();

			JobCollection jobs = new JobCollection(Factory);
			jobs.Load();
			AssertEquals(2, jobs.Count);
			Job shipment1Job = jobs.GetJobForOperationsPlugin(Shipment1);
			Job shipment2Job = jobs.GetJobForOperationsPlugin(Shipment2);
			shipment2Job.JH_GB = GlbBranch.CurrentBranch.PK;

			Charge shipment1Charge = shipment1Job.Charges.AddNew();
			Charge shipment2Charge = shipment2Job.Charges.AddNew();

			shipment1Charge.JR_AC = Env.Registry.FreightChargeCode;
			shipment1Charge.JR_OSSellAmt = 180m;
			shipment1Charge.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment1Charge.JR_OSCostAmt = 80m;
			shipment1Charge.JR_OH_CostAccount = TestObjectCreator.Agent.PK;
			shipment1Charge.JR_AgentDeclaredSellAmt = 150m;
			shipment1Charge.JR_AgentDeclaredCostAmt = 100m;

			shipment2Charge.JR_AC = Env.Registry.FreightChargeCode;
			shipment2Charge.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			shipment2Charge.JR_OSSellAmt = 200m;
			shipment2Charge.JR_OSCostAmt = 100m;
			shipment2Charge.JR_OH_CostAccount = TestObjectCreator.Agent.PK;
			shipment2Charge.JR_AgentDeclaredSellAmt = 180m;
			shipment2Charge.JR_AgentDeclaredCostAmt = 120m;

			SetAPInvoiceInfo(shipment2Charge, "1", ZDateTime.Now, ZDateTime.Now.AddDays(1));
			SetAPPaymentInfo(shipment2Charge, ReceiptTypes.Cash, AUDBankAccount, "CASH1");

			AssertNoErrors(shipment1Charge);
			AssertNoErrors(shipment2Charge);
			AssertNoErrors(shipment1Job);
			AssertNoErrors(shipment2Job);

			Factory.Save();

			#endregion

			int numberOfTransactions = Factory.GetDatabaseCount(typeof(AccTransactionHeader));

			ConsolInvoicingPostManager creator = new ConsolInvoicingPostManager(Factory, jobs.Cast<Job>(), Consol, apps);
			TransactionCreatorHashtable transactions = creator.CreateTransactions(JobInvoicingPostingOption.All);
			AssertNotEquals("Precondition: Should be some transactions posted", 0, creator.Poster.PostedInvoices.Count);
			AssertNotEquals("Precondition: Should be any AR transactions posted.", 0, creator.Poster.PostedInvoices.Count);
			AssertNotEquals("Precondition: Should be any AP transactions posted.", 0, transactions.Count);
			AssertNotEquals("Precondition: Should be any PaymentApprovals posted.", 0, transactions.GetAllAPPaymentApprovals().Length);

			creator.RollbackPosting();

			AssertEquals("Posting should be cancelled", true, creator.CancelPosting);
		}

		public void TestPostingDoesNotTriggersBillingEventForNonGatewayCost()
		{
			var newCompany = TestObjectCreator.CreateNewCompany("NGW");
			var newBranch = TestObjectCreator.CreateNewBranch(newCompany, "BGW");
			Factory.Save();

			var setup = TestObjectCreator.CreateGatewayConsolsAndShipments();
			Factory.Save();

			var job = TestObjectCreator.CreateJob(setup.s0001, false);
			job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
			job.JH_OA_AgentCollectAddr = TestObjectCreator.Agent.MainAddress.PK;

			//in the current company
			AssertBillingHeaderIsNotCreatedForNonGWCosts(new[] { setup.s0001.Job as Job, TestObjectCreator.CreateJob(setup.s0002), TestObjectCreator.CreateJob(setup.s0003) }, isGateWayEnabled: true);

			//In a different company
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), newBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var job1 = TestObjectCreator.CreateJob(setup.s0001);
				var job2 = TestObjectCreator.CreateJob(setup.s0002);
				var job3 = TestObjectCreator.CreateJob(setup.s0003);

				AssertBillingHeaderIsNotCreatedForNonGWCosts(new[] { job1, job2, job3 }, isGateWayEnabled: false);
			}

			void AssertBillingHeaderIsNotCreatedForNonGWCosts(Job[] jobs, bool isGateWayEnabled)
			{
				var consol = setup.gC0002;
				var apps = new ApportionmentListing(Factory, consol);
				var cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 250M, creditor: TestObjectCreator.Creditor1, apportionmentListing: apps);
				cost.E6_InvoiceDate = ZDateTime.Today;
				cost.E6_InvoiceNum = "TSTCC#100";
				Factory.Save();

				AssertEquals("Is Gateway enabled", isGateWayEnabled, consol.IsGatewayBillingEnabled());

				var creator = new ConsolInvoicingPostManager(Factory, jobs, consol, apps);
				TransactionCreatorHashtable transactions = creator.CreateTransactions(JobInvoicingPostingOption.All);

				var events = AccBillingEventCollector.GetInstance(Factory).GetEvents(AccBillingCodes.GatewayBilling);
				AssertEquals("No Billing header is created, as this consol is not GW", 0, events.Count());

				Factory.Save();
				AssertEquals(true, cost.IsPosted);

				var summary = AccBillingHandler.GetBillingSummary(Factory, AccBillingCodes.GatewayBilling, setup.gC0002.PK, "JK");
				AssertEquals("BillingCounter remained as before", 0, summary.BilledItemCount);
				AssertEquals("Billed Shipments remained as before", "", summary.BilliedShipmentNumbersAsCSV);
				AssertEquals("BillingHeader count remained as before", 0, summary.BillingHeaders.Count());
			}
		}

		void Creator_ExportAgentPosting(object sender, ExportAgentPostingEventArgs e)
		{
			e.OptionSelector.Currency = TestObjectCreator.USD.RX_Code;
			e.OptionSelector.ExchangeRate = 0.78m;
			e.OptionSelector.UpdateAllChargesPostingStyleAccordingToSelectedCurrency();
			e.OptionSelector.UpdateAllChargesAddressAndContactAccordingToSelectedAddressAndContact();
		}

		bool ProfitShareConfirmationEventRaised;
		bool TestPostManager_ProfitShareConfirmation(object sender, ProfitShareConfirmationEventArgs e)
		{
			ProfitShareConfirmationEventRaised = true;
			return true;
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

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			ProfitShareConfirmationEventRaised = false;
			IsOnCriticalPostErrorEventRaised = false;
			IsRegistrySetupEventRised = false;

			var rate = AccTaxRate.Helper.FindTaxRate(new BusinessObjectFactory(), AccTaxRate.Helper.MainFreeGSTTaxRegistryID, Env.CurrentCompanyPK);
			rate.SetRateNumerator_ForTestOnly(0);
			rate.Factory.Save();
		}

		void SetupCharges()
		{
			Job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			ExchangeRate rate1 = CreateExchangeRate(Job, USD, .7M);
			ExchangeRate rate2 = CreateExchangeRate(Job, GBP, .4M);

			Charge1 = CreateCharge(Job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge2 = CreateCharge(Job, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			Charge3 = CreateCharge(Job, CC3, "Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);
			Charge4 = CreateCharge(Job, CC4, "Charge Code 4", null, 0M, null, USD, 500M, Agent);
			Charge5 = CreateCharge(Job, CC5, "Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, LocalClient);
			Charge6 = CreateCharge(Job, CC6, "Charge Code 6", USD, 200M, Creditor2, USD, 275M, Agent);
			Charge7 = CreateCharge(Job, CC7, "Charge Code 7", AUD, 200M, Creditor1, AUD, 300M, LocalClient);
		}

		void SetupAPInvoiceInfo()
		{
			SetAPInvoiceInfo(Charge1, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(Charge2, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(Charge3, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(Charge5, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(Charge7, "1", Now.AddDays(10), Now.AddDays(20));
		}

		void InitializeWIPAccruals()
		{
			Charge1WIP = Charge1.WIP;
			Charge2WIP = Charge2.WIP;
			Charge3WIP = Charge3.WIP;
			Charge4WIP = Charge4.WIP;
			Charge5WIP = Charge5.WIP;
			Charge6WIP = Charge6.WIP;
			Charge7WIP = Charge7.WIP;

			Charge1Accrual = Charge1.Accrual;
			Charge2Accrual = Charge2.Accrual;
			Charge3Accrual = Charge3.Accrual;
			Charge5Accrual = Charge5.Accrual;
			Charge6Accrual = Charge6.Accrual;
			Charge7Accrual = Charge7.Accrual;
		}

		#region OnCriticalPostError Event

		void TestPostManager_OnTaxBranchCriticalPostError(object sender, CriticalPostingErrorEventArgs e)
		{
			IsOnCriticalPostErrorEventRaised = true;

			AssertEquals(@"Please review the charges being posted.
You may need to post some charges through the Shipment.
Posting charges is being prevented because you are attempting to post a transaction containing charges for different tax branches.
Your transaction is not permitted because there are different tax branches.", e.ErrorMessage);
		}

		#endregion

		#region Registry Setup

		PaymentTwelveLevelAuthorisationSettings GetNewAuthorisationSetting(PaymentTwelveLevelAuthorisationSettingsCollection collection,
				ZString range, ZInt amount, ZString requirement)
		{
			var newSetting = collection.AddNew();
			newSetting.Amount = (ZDecimal)amount;
			newSetting.AuthorisationRequirement = requirement;
			newSetting.Range = range;

			return newSetting;
		}

		protected void SetUpRegistryForTest()
		{
			OriginalRegistryValueBeforeTest = AccountingConfigurationRegistry.Instance.UnapprovedInvoicesAuthorizationSettings.Value;

			var valuesForTest = new PaymentTwelveLevelAuthorisationSettingsCollection();
			var upTo = GetNewAuthorisationSetting(valuesForTest, PaymentAuthorisationSettings.RangeCodes.UpTo, 250, PaymentAuthorisationSettings.AuthorisationRequirementCodes.NoApprovalRequired);
			var above = GetNewAuthorisationSetting(valuesForTest, PaymentAuthorisationSettings.RangeCodes.Above, 250, PaymentAuthorisationSettings.AuthorisationRequirementCodes.FirstApprovalRequiredOnly);

			AccountingConfigurationRegistry.Instance.UnapprovedInvoicesAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
		}

		void ResetRegistryForTest()
		{
			if (OriginalRegistryValueBeforeTest != null)
			{
				AccountingConfigurationRegistry.Instance.UnapprovedInvoicesAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, OriginalRegistryValueBeforeTest);
			}
		}

		PaymentTwelveLevelAuthorisationSettingsCollection OriginalRegistryValueBeforeTest;

		#endregion

		Job Job;
		Charge Charge1;
		Charge Charge2;
		Charge Charge3;
		Charge Charge4;
		Charge Charge5;
		Charge Charge6;
		Charge Charge7;

		AccTransactionLines Charge1WIP;
		AccTransactionLines Charge2WIP;
		AccTransactionLines Charge3WIP;
		AccTransactionLines Charge4WIP;
		AccTransactionLines Charge5WIP;
		AccTransactionLines Charge6WIP;
		AccTransactionLines Charge7WIP;

		AccTransactionLines Charge1Accrual;
		AccTransactionLines Charge2Accrual;
		AccTransactionLines Charge3Accrual;
		AccTransactionLines Charge5Accrual;
		AccTransactionLines Charge6Accrual;
		AccTransactionLines Charge7Accrual;

		#endregion
	}
}
