using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting.Testing
{
	public class PostingChargeEligibilityDeciderTest : TestCaseWithFactory
	{
		public void TestEligibilityForChargesDisallowedToView()
		{
			#region Setup security

			GlbStaff testUser = Factory.NewWithValidTestData<GlbStaff>();
			testUser.GS_IsController = false;
			Factory.Save();

			var securityFactory = new BusinessObjectFactory();
			SecurityCore security = new UserLoginController().GetSecurityForUser(testUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);

			var loginSecurity = securityFactory.New<GlbSecurity>();
			loginSecurity.GU_GB = TestHelper.NonCurrentBranch.PK;
			loginSecurity.GU_GE = TestHelper.NonCurrentDepartment.PK;
			loginSecurity.GU_GS = testUser.PK;
			loginSecurity.GU_SecurityRight = security.Login.Code;
			loginSecurity.GU_SecurityItemIsAllowed = false;

			var invoicingSecurity = securityFactory.New<GlbSecurity>();
			invoicingSecurity.GU_GB = Env.CurrentBranch.PK;
			invoicingSecurity.GU_GE = Env.CurrentDepartment.PK;
			invoicingSecurity.GU_GS = testUser.PK;
			invoicingSecurity.GU_SecurityRight = security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowEnterModifyCharges).Code;
			invoicingSecurity.GU_SecurityItemIsAllowed = false;
			security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowEnterModifyCharges).IsAllowed = false;
			securityFactory.Save();

			#endregion

			using (Env.SetTemporaryUserContext(testUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (AccountingConfigurationRegistry.Instance.RestrictPostingOfSellChargesVisibleToLoginUserOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				Globals.IsUserInteractive = true;
				AssertEligibilityForChargesDisallowedToView();
			}
		}

		protected virtual void AssertEligibilityForChargesDisallowedToView()
		{
			ForwardingShipment shipment = TestHelper.CreateShipment("S00001234");
			TestJob = TestHelper.CreateJob(shipment);
			Charge1 = TestHelper.CreateCharge(TestJob, TestHelper.CC1, "charge 1", TestHelper.AUD, 100m, TestHelper.Creditor1, TestHelper.AUD, 100m, TestHelper.Debtor);
			Charge1.JR_GB = TestHelper.NonCurrentBranch.PK;
			Charge1.JR_GE = TestHelper.NonCurrentDepartment.PK;

			Charge2 = TestHelper.CreateCharge(TestJob, TestHelper.CC1, "charge 2", TestHelper.AUD, 200m, TestHelper.Creditor1, TestHelper.AUD, 200m, TestHelper.Debtor);
			Charge2.JR_GB = Env.CurrentBranch.PK;
			Charge2.JR_GE = Env.CurrentDepartment.PK;

			Factory.Save();

			AssertEquals("User is disallowed to view charge 1", false, Charge1.IsAllowedToViewThisCharge);
			AssertEquals("User is allowed to view charge 2", true, Charge2.IsAllowedToViewThisCharge);

			var decider = GetEligibilityDecider((Charge[])TestJob.Charges.ToArray(typeof(Charge)));
			var results = decider.GetEligibleCharges(JobInvoicingPostingOption.All);

			AssertEquals("Charge 1 is ineligible to post", false, results.Contains(Charge1));
			AssertEquals("Charge 2 is eligible to post", true, results.Contains(Charge2));
		}

		public void TestEligibilityForPostingAllSisterCompanyCharges()
		{
			var orgHeader = TestHelper.CreateOrgHeader("ORGUS", false, true, "USCHI");
			var orgHeader1 = TestHelper.CreateOrgHeader("ORGNZ1", false, true, "NZAKL");
			var orgHeader2 = TestHelper.CreateOrgHeader("ORGNZ2", false, true, "NZAKL");

			var companyNZ = TestHelper.CreateNewCompany("NZ1", "NZ");
			companyNZ.GC_OH_OrgProxy = orgHeader1.PK;

			var branchNZ = TestHelper.CreateNewBranch(companyNZ, "AKL");
			branchNZ.GB_OH_OrgProxy = orgHeader2.PK;

			var shipment = TestHelper.CreateShipment("S001001");
			var job = TestHelper.CreateJob(shipment, false, false);
			var charge = TestHelper.CreateCharge(job, TestHelper.CC1, "test", TestHelper.AUD, 100M, TestHelper.AALSHI, TestHelper.AUD, 120M, orgHeader);
			var charge1 = TestHelper.CreateCharge(job, TestHelper.CC1, "test", TestHelper.AUD, 100M, TestHelper.AALSHI, TestHelper.AUD, 120M, orgHeader1);
			var charge2 = TestHelper.CreateCharge(job, TestHelper.CC1, "test", TestHelper.AUD, 100M, TestHelper.AALSHI, TestHelper.AUD, 120M, orgHeader2);

			Factory.Save();

			PostingChargeEligibilityDecider decider = new PostingChargeEligibilityDecider((Charge[])job.Charges.ToArray(typeof(Charge)));
			var results = decider.GetEligibleCharges(JobInvoicingPostingOption.AllSisterCompanyCharges);
			AssertEquals("Only 2 charges are eligible", 2, results.Count);
			AssertEquals(true, results.Contains(charge1));
			AssertEquals(true, results.Contains(charge2));
		}

		public void TestEligibilityForPostingLocalSisterCompanyCharges()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			var orgHeader = TestHelper.CreateOrgHeader("ORGUS", false, true, "USCHI");
			var orgHeader1 = TestHelper.CreateOrgHeader("ORGNZ1", false, true, "NZAKL");
			var orgHeader2 = TestHelper.CreateOrgHeader("ORGNZ2", false, true, "NZAKL");
			var orgHeader3 = TestHelper.CreateOrgHeader("ORGAU1", false, true, "AUMEL");
			var orgHeader4 = TestHelper.CreateOrgHeader("ORGAU2", false, true, "AUMEL");

			var companyNZ = TestHelper.CreateNewCompany("NZ1", "NZ");
			companyNZ.GC_OH_OrgProxy = orgHeader1.PK;

			var branchNZ = TestHelper.CreateNewBranch(companyNZ, "AKL");
			branchNZ.GB_OH_OrgProxy = orgHeader2.PK;

			var companyAU = TestHelper.CreateNewCompany("AU1", "AU");
			companyAU.GC_OH_OrgProxy = orgHeader3.PK;

			var branchAU = TestHelper.CreateNewBranch(companyAU, "SY1");
			branchAU.GB_OH_OrgProxy = orgHeader4.PK;

			var shipment = TestHelper.CreateShipment("S001001");
			var job = TestHelper.CreateJob(shipment, false, false);

			var charge = TestHelper.CreateCharge(job, TestHelper.CC1, "test", TestHelper.AUD, 100M, TestHelper.AALSHI, TestHelper.AUD, 120M, orgHeader);
			var charge1 = TestHelper.CreateCharge(job, TestHelper.CC1, "test", TestHelper.AUD, 100M, TestHelper.AALSHI, TestHelper.AUD, 120M, orgHeader1);
			var charge2 = TestHelper.CreateCharge(job, TestHelper.CC1, "test", TestHelper.AUD, 100M, TestHelper.AALSHI, TestHelper.AUD, 120M, orgHeader2);
			var charge3 = TestHelper.CreateCharge(job, TestHelper.CC1, "test", TestHelper.AUD, 100M, TestHelper.AALSHI, TestHelper.AUD, 120M, orgHeader3);
			var charge4 = TestHelper.CreateCharge(job, TestHelper.CC1, "test", TestHelper.AUD, 100M, TestHelper.AALSHI, TestHelper.AUD, 120M, orgHeader4);

			Factory.Save();

			PostingChargeEligibilityDecider decider = new PostingChargeEligibilityDecider((Charge[])job.Charges.ToArray(typeof(Charge)));
			var results = decider.GetEligibleCharges(JobInvoicingPostingOption.LocalSisterCompanyChargesOnly);
			AssertEquals("Only 2 charges are eligible", 2, results.Count);
			AssertEquals(true, results.Contains(charge3));
			AssertEquals(true, results.Contains(charge4));
		}

		public void TestEligibilityForCustomsDSBChargeAR()
		{
			AccChargeCode customsDisbursementChargeCode = TestHelper.CreateChargeCode("CC2_X", "Disbursement Charge Code", Constants.ChargeType.Disbursement, 50m, null, null);
			AccChargeCode revenueChargeCode = TestHelper.CreateChargeCode("CC3_X", "Revenue Charge Code", Constants.ChargeType.Revenue, 50m, null, null);

			MockCustomsJobProvider customDetail = Factory.New<MockCustomsJobProvider>();
			customDetail.JobNumber = "TR436877";

			OrgHeader localClient = TestHelper.CreateOrgHeader("LOCAL_X", false, true);
			OrgHeader agent = TestHelper.CreateOrgHeader("AGENT_X", false, true);
			Job job = TestHelper.CreateJob(localClient, 0m, agent, 0m);

			Charge charge1 = job.Charges.AddNew();
			charge1.JR_AC = customsDisbursementChargeCode.PK;
			charge1.JR_OH_SellAccount = localClient.PK;
			charge1.JR_LocalSellAmt = 100m;

			Charge charge2 = job.Charges.AddNew();
			charge2.JR_AC = revenueChargeCode.PK;
			charge2.JR_OH_SellAccount = localClient.PK;
			charge2.JR_LocalSellAmt = 100m;

			Charge charge3 = job.Charges.AddNew();
			charge3.JR_AC = customsDisbursementChargeCode.PK;
			charge3.JR_InvoiceType = ZArchitecture.Core.InvoiceTypesList.Codes.DisbursementInForeignCurrency;
			charge3.JR_OH_SellAccount = localClient.PK;
			charge3.JR_LocalSellAmt = 100m;

			job.PlugInData = customDetail;

			PostingChargeEligibilityDecider decider = new PostingChargeEligibilityDecider((Charge[])job.Charges.ToArray(typeof(Charge)), x => x.JR_AC == customsDisbursementChargeCode.PK);
			IReceivablesPostingChargeCollection results = decider.GetEligibleCharges(JobInvoicingPostingOption.CustomsDSBChargeAROnly);
			AssertEquals("Only 2 charges are eligible", 2, results.Count);
			AssertEquals(true, results.Contains(charge3));
			AssertEquals(true, results.Contains(charge1));
		}

		public virtual void TestEligibilityForPostingGatewayCharges()
		{
			var orgHeader = TestHelper.CreateOrgHeader("ORGUS", false, true, "USCHI");
			var orgHeader1 = TestHelper.CreateOrgHeader("ORGNZ1", false, true, "NZAKL");
			var orgHeader2 = TestHelper.CreateOrgHeader("ORGAU1", false, true, "AUSYD");

			var companyNZ = TestHelper.CreateNewCompany("NZ1", "NZ");
			companyNZ.GC_OH_OrgProxy = orgHeader1.PK;
			var companyAU = TestHelper.CreateNewCompany("AU1", "AU");
			companyAU.GC_OH_OrgProxy = orgHeader2.PK;

			var consol = TestHelper.CreateGatewayConsol(sendingGatewayCompany: companyNZ, receivingGatewayCompany: companyAU);
			consol.JK_SendingForwarderHandlingType = ZString.Empty;
			var shipment = TestHelper.CreateShipment("S00123", consol);
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var job = TestHelper.CreateJob(shipment, false);

			var charge1 = TestHelper.CreateCharge(job, TestHelper.CC1, "test", TestHelper.AUD, 100M, TestHelper.AALSHI, TestHelper.AUD, 120M, orgHeader);
			var charge2 = TestHelper.CreateCharge(job, TestHelper.CC1, "test", TestHelper.AUD, 100M, TestHelper.AALSHI, TestHelper.AUD, 120M, orgHeader1);
			var charge3 = TestHelper.CreateCharge(job, TestHelper.CC1, "test", TestHelper.AUD, 100M, TestHelper.AALSHI, TestHelper.AUD, 120M, orgHeader2);

			Factory.Save();

			Assert("Pre-condition, charge is not gateway", !consol.IsGatewayCharge(charge1));
			Assert("Pre-condition, charge1 is not gateway", !consol.IsGatewayCharge(charge2));
			Assert("Pre-condition, charge2 is gateway", consol.IsGatewayCharge(charge3));

			var decider = new PostingChargeEligibilityDecider((Charge[])job.Charges.ToArray(typeof(Charge)));
			AssertEquals("no charges are eligible as gateway posting cannot be done in this class", 0, decider.GetEligibleCharges(JobInvoicingPostingOption.Gateway).Count);
		}

		public void TestDoNotPostCharge()
		{
			OrgHeader localClient = TestHelper.CreateOrgHeader("LOCAL_X", false, true);
			OrgHeader agent = TestHelper.CreateOrgHeader("AGENT_X", false, true);
			Job testJob = TestHelper.CreateJob(localClient, 0m, agent, 0m);

			AccChargeCode revenueChargeCode = TestHelper.CreateChargeCode("CC3", "Revenue Charge Code", Constants.ChargeType.Revenue, 50m, null, null);

			TestHelper.CreateCharge(testJob, revenueChargeCode, "Charge 1", TestHelper.AUD, 0m, null, TestHelper.AUD, 100m, localClient);
			Charge charge2 = TestHelper.CreateCharge(testJob, revenueChargeCode, "Charge 2", TestHelper.AUD, 0m, null, TestHelper.AUD, 200m, localClient);
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.DoNotPost;

			AssertEquals("Precondition: 2 charges on job", 2, testJob.Charges.Count);

			PostingChargeEligibilityDecider decider = new PostingChargeEligibilityDecider((Charge[])testJob.Charges.ToArray(typeof(Charge)));
			IReceivablesPostingChargeCollection results = decider.GetEligibleCharges(JobInvoicingPostingOption.All);
			AssertEquals("Only 1 charge is eligible as the 2nd one is marked as do not post", 1, results.Count);

			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			results = decider.GetEligibleCharges(JobInvoicingPostingOption.All);
			AssertEquals("Now both charges are eligible", 2, results.Count);
		}

		public void TestCommentChargeTypeWithZeroAmount()
		{
			AccChargeCode commentChargeCode = DisbursementChargeCode;
			commentChargeCode.AC_Desc = "Comment Charge Code";
			commentChargeCode.AC_ChargeType = Constants.ChargeType.Comment;
			commentChargeCode.AC_MarginPercentage = 0M;

			Charge dBTCharge = Factory.NewWithValidTestData<Charge>();
			dBTCharge.JR_AC = RevenueChargeCode.PK;
			dBTCharge.JR_OH_SellAccount = LocalClient.PK;
			dBTCharge.JR_InvoiceType = ZArchitecture.Core.InvoiceTypesList.Codes.DisbursementInvoice;
			dBTCharge.JR_LocalSellAmt = 30m;
			dBTCharge.JR_JH = TestJob.PK;

			Charge cMTCharge = Factory.NewWithValidTestData<Charge>();
			cMTCharge.JR_AC = commentChargeCode.PK;
			cMTCharge.JR_OH_SellAccount = LocalClient.PK;
			cMTCharge.JR_JH = TestJob.PK;

			Factory.Save();

			PostingChargeEligibilityDecider decider = new PostingChargeEligibilityDecider((new Charge[] { cMTCharge, dBTCharge }));
			IReceivablesPostingChargeCollection results = decider.GetEligibleCharges(JobInvoicingPostingOption.All);
			AssertEquals("Comment Charge Code should be eligible although it has 0 amount.", 2, results.Count);
		}

		public void TestPostDisbursementOnly()
		{
			Charge mRGCharge = TestJob.Charges.AddNew();
			mRGCharge.JR_AC = MarginChargeCode.PK;
			mRGCharge.JR_OH_SellAccount = LocalClient.PK;
			mRGCharge.JR_LocalSellAmt = 10m;

			Charge dSBCharge = TestJob.Charges.AddNew();
			dSBCharge.JR_AC = DisbursementChargeCode.PK;
			dSBCharge.JR_OH_SellAccount = LocalClient.PK;
			dSBCharge.JR_LocalSellAmt = 20m;

			foreach (string invoiceType in InvoiceTypeCalculationProvider.DisbursementInvoiceTypes)
			{
				Charge charge = TestJob.Charges.AddNew();
				charge.JR_AC = RevenueChargeCode.PK;
				charge.JR_OH_SellAccount = LocalClient.PK;
				charge.JR_InvoiceType = invoiceType;
				charge.JR_LocalSellAmt = 30m;
			}

			Factory.Save();

			PostingChargeEligibilityDecider decider = GetEligibilityDecider((Charge[])TestJob.Charges.ToArray(typeof(Charge)));
			IReceivablesPostingChargeCollection eligibleCharges = decider.GetEligibleCharges(JobInvoicingPostingOption.Disbursement);

			foreach (Charge charge in TestJob.Charges)
			{
				AssertEquals(ShouldChargeBeEligible(charge), eligibleCharges.Contains(charge));
			}
		}

		protected virtual bool ShouldChargeBeEligible(Charge charge)
		{
			return InvoiceTypeCalculationProvider.IsDisbursementInvoiceType(charge.JR_InvoiceType) && !charge.IsDeferredCharge;
		}

		#region Implementation

		protected TestObjectCreator TestHelper;

		protected virtual PostingChargeEligibilityDecider GetEligibilityDecider(Charge[] charges)
		{
			return new PostingChargeEligibilityDecider(charges);
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestHelper = new TestObjectCreator(Factory);

			LocalClient = TestHelper.LocalClient;
			Agent = TestHelper.Agent;
			MarginChargeCode = TestHelper.CreateChargeCode("CCC1", "Margin Charge Code", Constants.ChargeType.Margin, 50m, null, null);
			DisbursementChargeCode = TestHelper.CreateChargeCode("CCC2", "Disbursement Charge Code", Constants.ChargeType.Disbursement, 50m, null, null);
			RevenueChargeCode = TestHelper.CreateChargeCode("CCC3", "Revenue Charge Code", Constants.ChargeType.Revenue, 50m, null, null);
			Factory.Save();

			TestJob = TestHelper.CreateJob(LocalClient, 0m, Agent, 0m);
			TestJob.PlugInData = TestHelper.CreateShipment("S001");
		}

		protected virtual void SetupEligibleCharges()
		{
			RefCurrency aUD = TestHelper.AUD;

			OrgInvoiceRollupOrGroup group1 = LocalClient.CompanyData.InvoiceRollupOrGroups.AddNew();
			group1.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			group1.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			group1.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			group1.PG_InvoicePostingStyle = InvoicePostingOptionsList.Codes.DisbursementFreightAndFinal;
			OrgInvoiceRollupOrGroup group2 = Agent.CompanyData.InvoiceRollupOrGroups.AddNew();
			group2.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			group2.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			group2.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			group2.PG_InvoicePostingStyle = InvoicePostingOptionsList.Codes.DisbursementFreightAndFinal;
			Factory.Save();

			Charge1 = TestHelper.CreateCharge(TestJob, RevenueChargeCode, "Charge", aUD, 0m, null, aUD, 100m, LocalClient);
			Charge2 = TestHelper.CreateCharge(TestJob, RevenueChargeCode, "Charge", aUD, 0m, null, aUD, 0m, LocalClient);
			Charge3 = TestHelper.CreateCharge(TestJob, MarginChargeCode, "Charge", aUD, 0m, null, aUD, 100m, LocalClient);
			Charge = TestHelper.CreateCharge(TestJob, MarginChargeCode, "Charge", aUD, 0m, null, aUD, 100m, LocalClient);
			Charge4 = TestHelper.CreateCharge(TestJob, DisbursementChargeCode, "Charge", aUD, 0m, null, aUD, 100m, LocalClient);
			Charge5 = TestHelper.CreateCharge(TestJob, RevenueChargeCode, "Charge", aUD, 0m, null, aUD, 100m, Agent);
			Charge6 = TestHelper.CreateCharge(TestJob, MarginChargeCode, "Charge", aUD, 0m, null, aUD, 100m, Agent);
			Charge7 = TestHelper.CreateCharge(TestJob, DisbursementChargeCode, "Charge", aUD, 0m, null, aUD, 100m, Agent);
			Charge8 = TestHelper.CreateCharge(TestJob, DisbursementChargeCode, "Charge", aUD, 0m, null, aUD, 100m, Agent);
		}

		protected Job TestJob;
		protected Charge Charge;
		protected AccChargeCode MarginChargeCode;
		protected AccChargeCode DisbursementChargeCode;
		protected AccChargeCode RevenueChargeCode;
		protected OrgHeader LocalClient;
		protected OrgHeader Agent;

		protected Charge Charge1;
		protected Charge Charge2;
		protected Charge Charge3;
		protected Charge Charge4;
		protected Charge Charge5;
		protected Charge Charge6;
		protected Charge Charge7;
		protected Charge Charge8;

		#endregion
	}
}
