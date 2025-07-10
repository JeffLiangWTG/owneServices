using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	class CustomsDSBChargePostValidatorTest : TransactionCreatorBaseTest
	{
		public void TestValidateIfChargeCodeNotSetup()
		{
			ZGuid staff = SetupStaffMemberEmailAddress();

			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.NewGuid());
			var customsDisbursementChargeCode = RatingDataRegistry.Instance.CustomsDisbursementChargeCode;
			using (customsDisbursementChargeCode.DataType.SuspendValidation())
			{
				customsDisbursementChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);
			}

			var customDetail = Factory.New<MockCustomsJobProvider>();
			customDetail.fBranch = GlbBranch.CurrentBranch;
			customDetail.AutoPostingNotification = new AutoPostingNotification(new ZGuid[] { staff }, false);

			// Prepare data to avoid Validation errors on saving Job before posting
			var invoicingJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			invoicingJob.JH_JobNum = "TR436877";
			invoicingJob.JH_OA_AgentCollectAddr = TestObjectCreator.Agent.MainAddress.PK;
			invoicingJob.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
			invoicingJob.JH_GB = GlbBranch.CurrentBranch.PK;
			invoicingJob.JH_GE = TestObjectCreator.FESDepartment.PK;
			invoicingJob.PlugInData = customDetail;
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

			RaiseTestInvoice(customDetail);
			AssertEquals("Should have sent email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(true, Env.OutgoingMailManager.EmailsCreated[0].Body.Contains(string.Format(CustomsDSBChargePostValidator.GetCustomsDisbursementChargeCodeNotSet(customDetail.Branch.Company.GC_Code))));
			AssertEquals(false, Env.OutgoingMailManager.EmailsCreated[0].Body.Contains(string.Format(CustomsDSBChargePostValidator.GetCustomsDisbursementChargeCodeNotValid(customDetail.Branch.Company.GC_Code))));
		}

		public void TestValidateIfCreditorNotSetup()
		{
			ZGuid staff = SetupStaffMemberEmailAddress();

			AccChargeCode chargeCode = SetChargeCode();

			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);
			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid());

			var customDetail = Factory.New<MockCustomsJobProvider>();
			customDetail.fBranch = GlbBranch.CurrentBranch;
			customDetail.AutoPostingNotification = new AutoPostingNotification(new ZGuid[] { staff }, false);

			MockCustomCharge charge1 = GetCustomCharge(122, 0, false, TestObjectCreator.Creditor1.PK);
			MockCustomsChargesProvider chargeProvider = new MockCustomsChargesProvider();
			ICustomsCharges[] charges = new ICustomsCharges[] { charge1 };
			chargeProvider.InvoiceNumber = "ABC";
			chargeProvider.InvoiceDate = ZDateTime.Now;
			chargeProvider.CustomsCharges = charges;
			chargeProvider.CustomsJob = customDetail;
			chargeProvider.Factory = Factory;

			// Prepare data to avoid Validation errors on saving Job before posting
			var invoicingJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			invoicingJob.JH_JobNum = "TR436877";
			invoicingJob.JH_OA_AgentCollectAddr = TestObjectCreator.Agent.MainAddress.PK;
			invoicingJob.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
			invoicingJob.JH_GB = GlbBranch.CurrentBranch.PK;
			invoicingJob.JH_GE = TestObjectCreator.FESDepartment.PK;
			invoicingJob.PlugInData = customDetail;
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

			RaiseTestInvoice(customDetail);
			AssertEquals("Should have sent email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Should have sent email", true, Env.OutgoingMailManager.EmailsCreated[0].Body.Contains("Customs Disbursement Creditor is not set in the registry for the company "));
			AssertEquals("Should have sent email", false, Env.OutgoingMailManager.EmailsCreated[0].Body.Contains("The Customs Disbursement Creditor setup in the registry for the company "));

			Env.OutgoingMailManager.EmailsCreated.Clear();
			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.NewGuid());
			RaiseTestInvoice(customDetail);
			AssertEquals("Should have sent email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Should have sent email", false, Env.OutgoingMailManager.EmailsCreated[0].Body.Contains("Customs Disbursement Creditor is not set in the registry for the company "));
			AssertEquals("Should have sent email", true, Env.OutgoingMailManager.EmailsCreated[0].Body.Contains("The Customs Disbursement Creditor set up in the registry for the company "));
		}

		public void TestValidateIfCreditorIsSelfBilled()
		{
			var newFactory = new BusinessObjectFactory();
			var creditor = newFactory.Load<OrgHeader>(Creditor2.PK);

			var companyDataQuery = new ZQuery(OrgCompanyDataSchema.OB_OH, creditor.PK);
			companyDataQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);

			var companyData = newFactory.LoadTop1<OrgCompanyData>(companyDataQuery);
			if (companyData == null)
			{
				companyData = Factory.New<OrgCompanyData>();
				companyData.OB_OH = creditor.PK;
				companyData.OB_GC = GlbCompany.CurrentCompany.PK;
			}
			companyData.OB_APCostsSelfBilled = true;
			newFactory.Save();

			ZGuid staff = SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();

			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, creditor.PK.ToGuid());
			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid());

			var customDetail = Factory.New<MockCustomsJobProvider>();
			customDetail.fBranch = GlbBranch.CurrentBranch;
			customDetail.AutoPostingNotification = new AutoPostingNotification(new ZGuid[] { staff }, false);

			// Prepare data to avoid Validation errors on saving Job before posting
			var invoicingJob = new Job.Loader(customDetail).TryCreateWithoutMutexForTestOnly();
			invoicingJob.JH_OA_AgentCollectAddr = TestObjectCreator.Agent.MainAddress.PK;
			invoicingJob.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
			invoicingJob.JH_GE = TestObjectCreator.FESDepartment.PK;
			invoicingJob.PlugInData = customDetail;
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

			RaiseTestInvoice(customDetail);

			AssertEquals("Should have sent email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Should have sent email", true, Env.OutgoingMailManager.EmailsCreated[0].Body.Contains("should not be configured to issue self-billing invoices. The configuration to enable self-billing invoices is on the A/P tab for the Customs Disbursement Creditor."));

			Env.OutgoingMailManager.EmailsCreated.Clear();

			companyData.OB_APCostsSelfBilled = false;
			newFactory.Save();

			RaiseTestInvoice(customDetail);

			AssertEquals("Should have sent email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Should have sent email", false, Env.OutgoingMailManager.EmailsCreated[0].Body.Contains("should not be configured to issue self-billing invoices. The configuration to enable self-billing invoices is on the A/P tab for the Customs Disbursement Creditor."));
		}

		public void TestBodyIfNeitherRegistrySetup()
		{
			ZGuid staff = SetupStaffMemberEmailAddress();

			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);
			var customsDisbursementChargeCode = RatingDataRegistry.Instance.CustomsDisbursementChargeCode;
			using (customsDisbursementChargeCode.DataType.SuspendValidation())
			{
				customsDisbursementChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);
			}

			var customDetail = Factory.New<MockCustomsJobProvider>();
			customDetail.fBranch = GlbBranch.CurrentBranch;
			customDetail.AutoPostingNotification = new AutoPostingNotification(new ZGuid[] { staff }, false);

			RaiseTestInvoice(customDetail);

			AssertEquals("Should have sent email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(true, Env.OutgoingMailManager.EmailsCreated[0].Body.Contains("Customs Disbursement Charge Code is not set in the registry for the company "));
			AssertEquals(true, Env.OutgoingMailManager.EmailsCreated[0].Body.Contains("Customs Disbursement Creditor is not set in the registry for the company "));
		}

		public void TestValidationMessageWhenDuplicateUnpostedCUSDSBChargesExist()
		{
			ZGuid staff = SetupStaffMemberEmailAddress();

			var mockPlugIn = Factory.New<MockCustomsJobProvider>();
			mockPlugIn.JobNumber = "S001";
			var customDetail = Factory.New<MockCustomsJobProvider>();

			customDetail.AutoPostingNotification = new AutoPostingNotification(new ZGuid[] { staff }, false);
			MockCustomCharge charge1 = GetCustomCharge(122, 0, true, TestObjectCreator.Creditor1.PK);

			customDetail.fTopLevelObjectForJobToReference = mockPlugIn;

			Job expectedJob = new Job.Loader(mockPlugIn).TryCreateWithoutMutexForTestOnly();
			expectedJob.JH_OA_AgentCollectAddr = TestObjectCreator.Agent.MainAddress.PK;
			expectedJob.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
			// Prepare data to avoid Validation errors on saving Job before posting
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;
			expectedJob.PlugInData = mockPlugIn;

			Factory.Save();

			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, TestObjectCreator.Creditor1.PK.ToGuid());

			Charge unpostedCUSDSBCharge1 = expectedJob.Charges.AddNew();
			unpostedCUSDSBCharge1.JR_AC = RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value;
			unpostedCUSDSBCharge1.JR_OH_CostAccount = RatingDataRegistry.Instance.CustomsDisbursementCreditor.Value;
			unpostedCUSDSBCharge1.JR_LocalCostAmt = 100m;

			Charge unpostedCUSDSBCharge2 = expectedJob.Charges.AddNew();
			unpostedCUSDSBCharge2.JR_AC = RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value;
			unpostedCUSDSBCharge2.JR_OH_CostAccount = RatingDataRegistry.Instance.CustomsDisbursementCreditor.Value;
			unpostedCUSDSBCharge2.JR_LocalCostAmt = 100m;

			MockCustomsChargesProvider chargeProvider = new MockCustomsChargesProvider();
			ICustomsCharges[] charges = new ICustomsCharges[] { charge1 };
			chargeProvider.CustomsCharges = charges;
			chargeProvider.CustomsJob = customDetail;
			chargeProvider.Factory = Factory;

			RaiseTestInvoice(customDetail);

			string body = Env.OutgoingMailManager.EmailsCreated[0].Body;

			AssertEquals("Error when 2 unposted CUSDSB charges exist",
				true,
				body.Contains("This Job has more than one charge line for each of the following Charge Codes: CUSDSB. The sum of the amounts of these charge lines (grouped per charge code) differ from the actual Customs Disbursement amount represented by the charge code. Automatically processing failed."));
		}

		public void TestValidationWhenEntryIsWithdrawnForPostedCharges()
		{
			var staff = SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = Factory.LoadTop1<OrgHeader>(new ZQuery());

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var existingInvoice = Factory.NewWithValidTestData<APInvoice>();
				existingInvoice.AH_OH = testCreditor.PK;
				existingInvoice.AH_TransactionNum = "ABC123DEF";

				APInvoiceLine apLine = existingInvoice.Lines.AddNew() as APInvoiceLine;
				apLine.FillWithValidTestData();
				apLine.AL_AC = chargeCode.PK;

				Job invoicingJob = Factory.NewJobWithValidTestDataForTesting<Job>();
				Charge charge = invoicingJob.Charges.AddNew();
				charge.JR_AL_APLine = apLine.PK;
				charge.JR_AC = chargeCode.PK;

				apLine.AL_JH = invoicingJob.PK;
				var customDetail = Factory.New<MockCustomsJobProvider>();
				var cusCharge = GetCustomCharge(apLine.ChargeCode, 122, 0, true, testCreditor.PK);

				customDetail.JobNumber = "TR436877";
				customDetail.AutoPostingNotification = new AutoPostingNotification(new ZGuid[] { staff }, false);
				invoicingJob.PlugInData = customDetail;

				// Prepare data to avoid Validation errors on saving Job before posting
				invoicingJob.JH_ParentID = customDetail.PK;
				invoicingJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				invoicingJob.JH_OA_LocalChargesAddr = TestObjectCreator.Agent.MainAddress.PK;
				invoicingJob.JH_JobNum = "TR436877";
				GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

				Factory.Save();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARPostDSB | ChargePosterBehaviours.SendEmail, new ZGuid[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				AssertEquals("one email should have been sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);

				var body = Env.OutgoingMailManager.EmailsCreated[0].Body;
				AssertContains("The entry has been withdrawn, however there is at least one posted AR or AP for disbursement charges. Please reverse the invoice.", body);
			}
		}

		public void TestEmailSentIfDeferredChargeEnabledAndRegistryNotSet()
		{
			SetCustomsDeferredChargeInRegistry(Guid.Empty, true);

			AccChargeCode chargeCode = SetChargeCode();
			OrgHeader testCreditor = Factory.LoadTop1<OrgHeader>(new ZQuery());

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				ZGuid staff = SetupStaffMemberEmailAddress();

				MockCustomsJobProvider customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.AutoPostingNotification = new AutoPostingNotification(new ZGuid[] { staff }, false);

				// Prepare data to avoid Validation errors on saving Job before posting
				var invoicingJob = Factory.NewJobWithValidTestDataForTesting<Job>();
				invoicingJob.JH_OA_AgentCollectAddr = TestObjectCreator.Agent.MainAddress.PK;
				invoicingJob.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
				invoicingJob.JH_GB = GlbBranch.CurrentBranch.PK;
				invoicingJob.JH_GE = TestObjectCreator.FESDepartment.PK;
				invoicingJob.PlugInData = customDetail;
				GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

				RaiseTestInvoice(customDetail);

				AssertEquals("Should have sent email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEquals("Should have sent email", true, Env.OutgoingMailManager.EmailsCreated[0].Body.Contains(string.Format(CustomsDSBChargePostValidator.GetCustomDeferredChargeCodeNotSet(customDetail.Branch.Company.GC_Code))));
				AssertEquals("Should have sent email", false, Env.OutgoingMailManager.EmailsCreated[0].Body.Contains(string.Format(CustomsDSBChargePostValidator.GetCustomDeferredChargeCodeNotValid(customDetail.Branch.Company.GC_Code))));

				Env.OutgoingMailManager.EmailsCreated.Clear();
				SetCustomsDeferredChargeInRegistry(testCreditor.PK.ToGuid(), true);
				RaiseTestInvoice(customDetail);

				AssertEquals("Should have sent email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEquals("Should have sent email", false, Env.OutgoingMailManager.EmailsCreated[0].Body.Contains(string.Format(CustomsDSBChargePostValidator.GetCustomDeferredChargeCodeNotSet(customDetail.Branch.Company.GC_Code))));
				AssertEquals("Should have sent email", true, Env.OutgoingMailManager.EmailsCreated[0].Body.Contains(string.Format(CustomsDSBChargePostValidator.GetCustomDeferredChargeCodeNotValid(customDetail.Branch.Company.GC_Code))));
			}
		}

		public void TestMoreThanOneChargeLinesExistForAChargeCodeForAP()
		{
			AccChargeCode chargeCode = SetChargeCode();
			OrgHeader testCreditor = TestObjectCreator.Creditor1;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				ZGuid staff = SetupStaffMemberEmailAddress();

				MockCustomsJobProvider customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.AutoPostingNotification = new AutoPostingNotification(new ZGuid[] { staff }, false);
				Job existingJob = Factory.NewJobForTesting<Job>();
				existingJob.PlugInData = customDetail;
				// Prepare data to avoid Validation errors on saving Job before posting
				existingJob.JH_GB = GlbBranch.CurrentBranch.PK;
				existingJob.JH_GE = TestObjectCreator.FESDepartment.PK;
				existingJob.JH_OA_AgentCollectAddr = TestObjectCreator.Agent.MainAddress.PK;
				existingJob.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;

				Charge charge1 = existingJob.Charges.AddNew();
				charge1.JR_APInvoiceNum = "TR436877";
				charge1.JR_AC = chargeCode.PK;
				charge1.JR_LocalSellAmt = 100m;
				charge1.JR_LocalCostAmt = 100m;
				charge1.JR_OH_CostAccount = testCreditor.PK;
				charge1.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
				charge1.JR_GE = TestObjectCreator.FESDepartment.PK;

				Charge charge2 = existingJob.Charges.AddNew();
				charge2.JR_APInvoiceNum = "TR436877";
				charge2.JR_AC = chargeCode.PK;
				charge2.JR_LocalSellAmt = 100m;
				charge2.JR_LocalCostAmt = 100m;
				charge2.JR_OH_CostAccount = testCreditor.PK;
				charge2.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
				charge2.JR_GE = TestObjectCreator.FESDepartment.PK;

				GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

				RaiseTestInvoice(customDetail, ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB | ChargePosterBehaviours.SendEmail | ChargePosterBehaviours.ARPostDSB);
				AssertEquals("Should have sent email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEquals("Should have sent email", true, Env.OutgoingMailManager.EmailsCreated[0].Body.Contains(CustomsDSBChargePostValidator.GetMoreThanOneCustomsDSBCharges(chargeCode.AC_Code)));

				charge2.Delete();

				RaiseTestInvoice(customDetail, ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB | ChargePosterBehaviours.SendEmail | ChargePosterBehaviours.ARPostDSB);
				AssertEquals("charge1 is posted for AP", true, charge1.IsCostPosted);
				AssertEquals("charge1 is posted for AP", true, charge1.IsRevenuePosted);

				charge2 = existingJob.Charges.AddNew();
				charge2.JR_APInvoiceNum = "TR436877/1";
				charge2.JR_AC = chargeCode.PK;
				charge2.JR_LocalSellAmt = 100m;
				charge2.JR_LocalCostAmt = 100m;
				charge2.JR_OH_CostAccount = testCreditor.PK;
				charge2.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
				charge2.JR_GE = TestObjectCreator.NonCurrentDepartment.PK;

				RaiseTestInvoice(customDetail, ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB | ChargePosterBehaviours.SendEmail | ChargePosterBehaviours.ARPostDSB);
				AssertEquals("It is an error if there is an unposted Customs DSB charge if one is already posted", 1, Env.OutgoingMailManager.EmailsCreated.Count);

				Assert(!charge2.IsCostPosted);
				Assert(!charge2.IsRevenuePosted);
			}
		}

		public void TestMoreThanOneChargeLinesExistForAChargeCodeForAR()
		{
			AccChargeCode chargeCode = SetChargeCode();
			OrgHeader testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				ZGuid staff = SetupStaffMemberEmailAddress();

				MockCustomsJobProvider customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.AutoPostingNotification = new AutoPostingNotification(new ZGuid[] { staff }, false);

				Job existingJob = Factory.NewJobForTesting<Job>();
				existingJob.PlugInData = customDetail;

				Charge charge1 = existingJob.Charges.AddNew();
				charge1.JR_APInvoiceNum = "TR436877";
				charge1.JR_AC = chargeCode.PK;
				charge1.JR_LocalSellAmt = 100m;
				charge1.JR_LocalCostAmt = 100m;
				charge1.JR_OH_CostAccount = testCreditor.PK;
				charge1.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
				charge1.JR_GE = TestObjectCreator.FESDepartment.PK;

				Charge charge2 = existingJob.Charges.AddNew();
				charge2.JR_APInvoiceNum = "TR436877";
				charge2.JR_AC = chargeCode.PK;
				charge2.JR_LocalSellAmt = 100m;
				charge2.JR_OH_CostAccount = testCreditor.PK;
				charge2.JR_LocalCostAmt = 100m;
				charge2.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
				charge2.JR_GE = TestObjectCreator.FESDepartment.PK;

				// Prepare data to avoid Validation errors on saving Job before posting
				existingJob.JH_OA_AgentCollectAddr = TestObjectCreator.Agent.MainAddress.PK;
				existingJob.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
				existingJob.JH_GE = TestObjectCreator.FESDepartment.PK;
				GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

				RaiseTestInvoice(customDetail, ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARPostDSB | ChargePosterBehaviours.SendEmail);
				AssertEquals("Should have sent email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEquals("Should have sent email", true, Env.OutgoingMailManager.EmailsCreated[0].Body.Contains(CustomsDSBChargePostValidator.GetMoreThanOneCustomsDSBCharges(chargeCode.AC_Code)));

				charge2.Delete();

				RaiseTestInvoice(customDetail, ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARPostDSB | ChargePosterBehaviours.SendEmail);
				AssertEquals("charge1 is posted for AR", true, charge1.IsRevenuePosted);

				charge2 = existingJob.Charges.AddNew();
				charge2.JR_APInvoiceNum = "TR436877/1";
				charge2.JR_AC = chargeCode.PK;
				charge2.JR_LocalSellAmt = 100m;
				charge2.JR_LocalCostAmt = 100m;
				charge2.JR_OH_CostAccount = testCreditor.PK;
				charge2.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
				charge2.JR_GE = TestObjectCreator.FESDepartment.PK;

				var charge3 = existingJob.Charges.AddNew();
				charge3.JR_APInvoiceNum = "TR436877/1";
				charge3.JR_AC = chargeCode.PK;
				charge3.JR_LocalSellAmt = 100m;
				charge3.JR_LocalCostAmt = 100m;
				charge3.JR_OH_CostAccount = testCreditor.PK;
				charge3.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
				charge3.JR_GE = TestObjectCreator.FESDepartment.PK;

				RaiseTestInvoice(customDetail, ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARPostDSB | ChargePosterBehaviours.APPostDSB | ChargePosterBehaviours.SendEmail);
				AssertEquals("one email should have been sent as this time due to AP", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEquals("Should have sent email", true, Env.OutgoingMailManager.EmailsCreated[0].Body.Contains(CustomsDSBChargePostValidator.GetMoreThanOneCustomsDSBCharges(chargeCode.AC_Code)));
			}
		}

		[SuspendCriticalValidation]
		public void TestIfCustomsInvoiceExistWithSameNumber()
		{
			var staff = SetupStaffMemberEmailAddress();

			string aPInvoiceNum = "ABC123DEF";
			var chargeCode = SetChargeCode();
			var testCreditor = Factory.LoadTop1<OrgHeader>(new ZQuery());

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.AutoPostingNotification = new AutoPostingNotification(new ZGuid[] { staff }, false);
				var cusCharge = GetCustomCharge(122, 0, true, testCreditor.PK);
				customDetail.JobNumber = "TR436877";
				var invoicingJob = new Job.Loader(customDetail).TryCreateWithoutMutexForTestOnly();
				var existingInvoice = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
				existingInvoice.AH_OH = testCreditor.PK;
				existingInvoice.AH_TransactionNum = "ABC123DEF";

				var invoiceLine = (APInvoiceLine)existingInvoice.Lines.AddNew();
				invoiceLine.AL_JH = invoicingJob.PK;
				invoiceLine.AL_AC = chargeCode.PK;
				invoiceLine.AL_OSExTaxAmount = 122m;
				invoiceLine.AL_OSTaxAmount = 0m;

				var charge = TestObjectCreator.CreateCharge(invoiceLine, invoicingJob, chargeCode);

				var arInvoiceToPreventUnpostedARError = Factory.NewWithValidTestData<ARInvoice>();
				arInvoiceToPreventUnpostedARError.AH_OH = testCreditor.PK;
				var arInvoiceLine = (ARInvoiceLine)arInvoiceToPreventUnpostedARError.Lines.AddNew();
				arInvoiceLine.AL_JH = invoicingJob.PK;
				arInvoiceLine.AL_AC = chargeCode.PK;
				arInvoiceLine.AL_OSExTaxAmount = 122m;
				arInvoiceLine.AL_OSTaxAmount = 0m;
				charge.JR_AL_ARLine = arInvoiceLine.PK;
				charge.SetAmountsFromLinkedLinesForTests();
				Factory.Save();

				invoicingJob.PlugInData = customDetail;

				var chargeProvider = new MockCustomsChargesProvider();
				chargeProvider.CustomsCharges = new ICustomsCharges[] { cusCharge };
				chargeProvider.InvoiceNumber = aPInvoiceNum;
				chargeProvider.InvoiceDate = new ZDateTime(2005, 4, 12);
				chargeProvider.CustomsJob = customDetail;
				chargeProvider.Factory = Factory;
				chargeProvider.AutoPostingNotification = new AutoPostingNotification(new ZGuid[] { staff }, false);
				customDetail.DataProviders.Add(chargeProvider);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.SendEmail, new ZGuid[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				AssertEquals("Should not have sent any email", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		[SuspendCriticalValidation]
		public void TestIfCustomsInvoiceExistWithSameNumberHavingDiscrepancy()
		{
			var staff = SetupStaffMemberEmailAddress();

			string aPInvoiceNum = "ABC123DEF";
			var chargeCode = SetChargeCode();
			var testCreditor = Factory.LoadTop1<OrgHeader>(new ZQuery());

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.AutoPostingNotification = new AutoPostingNotification(new ZGuid[] { staff }, false);
				var cusCharge = GetCustomCharge(122, 0, true, testCreditor.PK);
				customDetail.JobNumber = "TR436877";
				var invoicingJob = new Job.Loader(customDetail).TryCreateWithoutMutexForTestOnly();
				var existingInvoice = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
				existingInvoice.AH_OH = testCreditor.PK;
				existingInvoice.AH_TransactionNum = "ABC123DEF";

				var invoiceLine = (APInvoiceLine)existingInvoice.Lines.AddNew();
				invoiceLine.AL_JH = invoicingJob.PK;
				invoiceLine.AL_AC = chargeCode.PK;
				invoiceLine.AL_OSExTaxAmount = 0m;
				invoiceLine.AL_OSTaxAmount = 0m;

				var charge = TestObjectCreator.CreateCharge(invoiceLine, invoicingJob, chargeCode);

				var arInvoiceToPreventUnpostedARError = Factory.NewWithValidTestData<ARInvoice>();
				arInvoiceToPreventUnpostedARError.AH_OH = testCreditor.PK;
				var arInvoiceLine = (ARInvoiceLine)arInvoiceToPreventUnpostedARError.Lines.AddNew();
				arInvoiceLine.AL_JH = invoicingJob.PK;
				arInvoiceLine.AL_AC = chargeCode.PK;
				arInvoiceLine.AL_OSExTaxAmount = 122m;
				arInvoiceLine.AL_OSTaxAmount = 0m;
				charge.JR_AL_ARLine = arInvoiceLine.PK;
				charge.SetAmountsFromLinkedLinesForTests();
				Factory.Save();

				invoicingJob.PlugInData = customDetail;

				var chargeProvider = new MockCustomsChargesProvider();
				chargeProvider.CustomsCharges = new ICustomsCharges[] { cusCharge };
				chargeProvider.InvoiceNumber = aPInvoiceNum;
				chargeProvider.InvoiceDate = new ZDateTime(2005, 4, 12);
				chargeProvider.CustomsJob = customDetail;
				chargeProvider.Factory = Factory;
				chargeProvider.AutoPostingNotification = new AutoPostingNotification(new ZGuid[] { staff }, false);
				customDetail.DataProviders.Add(chargeProvider);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.SendEmail, new ZGuid[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				AssertEquals("one email should have been sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);

				string emailBody = Env.OutgoingMailManager.EmailsCreated[0].Body;

				AssertEquals("Invoice Detail should contain Invoice Creation Date", true, emailBody.Contains(existingInvoice.CreatedDate.ToShortDateString()));
				AssertEquals("Invoice Detail should contain Invoice Creation Owner", true, emailBody.Contains(existingInvoice.CreatingUser));
				AssertEquals("Invoice Detail should contain Invoice Creation Owner's Tel no", true, emailBody.Contains(existingInvoice.Creator.GS_WorkPhone));
				AssertEquals("Invoice Detail should contain Invoice Creation Owner's Email", true, emailBody.Contains(existingInvoice.Creator.GS_EmailAddress));
				AssertEquals("Invoice Detail should contain Invoice Invoice Number", true, emailBody.Contains("ABC123DEF"));
			}
		}

		public void TestGenerateErrorMessageForExistingAPInvoice()
		{
			var staff = SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = Factory.LoadTop1<OrgHeader>(new ZQuery());

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var invoicingJob = Factory.NewJobWithValidTestDataForTesting<Job>();

				var existingInvoice = Factory.NewWithValidTestData<APInvoice>();
				existingInvoice.AH_OH = testCreditor.PK;
				existingInvoice.AH_TransactionNum = "ABC123DEF";

				var lineCharge1 = existingInvoice.Lines.AddNew() as APInvoiceLine;
				lineCharge1.FillWithValidTestData();
				lineCharge1.AL_AC = Factory.NewWithValidTestData<AccChargeCode>().PK;
				lineCharge1.AL_Desc = "DESC1\r\n ADDDESC11\r\n ADDDESC12";
				lineCharge1.AL_OSExTaxAmount = 120m;
				lineCharge1.AL_OSTaxAmount = 0m;
				lineCharge1.AL_JH = invoicingJob.PK;
				TestObjectCreator.CreateCharge(lineCharge1, invoicingJob, chargeCode);

				var lineCharge2 = existingInvoice.Lines.AddNew() as APInvoiceLine;
				lineCharge2.FillWithValidTestData();
				lineCharge2.AL_AC = Factory.NewWithValidTestData<AccChargeCode>().PK;
				lineCharge2.AL_Desc = "DESC2\r\n ADDDESC21\r\n";
				lineCharge2.AL_OSExTaxAmount = 200m;
				lineCharge2.AL_OSTaxAmount = 0m;
				lineCharge2.AL_JH = invoicingJob.PK;
				TestObjectCreator.CreateCharge(lineCharge2, invoicingJob, chargeCode);

				var customDetail = Factory.New<MockCustomsJobProvider>();
				var cusCharge = GetCustomCharge(lineCharge1.ChargeCode, 122, 0, true, testCreditor.PK);
				cusCharge.AddCustomsCharge(new CustomsCharge(lineCharge2.ChargeCode, "TEST", 200, 0, true, testCreditor.PK));
				customDetail.JobNumber = "TR436877";
				customDetail.AutoPostingNotification = new AutoPostingNotification(new ZGuid[] { staff }, false);
				invoicingJob.PlugInData = customDetail;

				// Prepare data to avoid Validation errors on saving Job before posting
				invoicingJob.JH_ParentID = customDetail.PK;
				invoicingJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				invoicingJob.JH_OA_LocalChargesAddr = TestObjectCreator.Agent.MainAddress.PK;
				invoicingJob.JH_JobNum = "TR436877";
				GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;
				invoicingJob.PlugInData = customDetail;
				// Add some Charge to let Preposting Validation pass
				var jobCharge = invoicingJob.Charges.AddNew();
				jobCharge.JR_AC = TestObjectCreator.FRT.PK;

				Factory.Save();

				var chargeProvider = new MockCustomsChargesProvider();
				chargeProvider.CustomsCharges = new ICustomsCharges[] { cusCharge };
				chargeProvider.InvoiceNumber = "ABC123DEF";
				chargeProvider.InvoiceDate = new ZDateTime(2005, 4, 12);
				chargeProvider.CustomsJob = customDetail;
				chargeProvider.Factory = Factory;
				chargeProvider.AutoPostingNotification = new AutoPostingNotification(new ZGuid[] { staff }, false);
				customDetail.DataProviders.Add(chargeProvider);

				Env.OutgoingMailManager.EmailsCreated.Clear();
				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB | ChargePosterBehaviours.SendEmail, new ZGuid[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				AssertEquals("one email should have been sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);

				var body = Env.OutgoingMailManager.EmailsCreated[0].Body;
				AssertEquals("Email body", true, body.Contains("DESC1"));
				AssertEquals("Email body", true, body.Contains("ADDDESC11"));
				AssertEquals("Email body", true, body.Contains("ADDDESC12"));
				AssertEquals("Email body", true, body.Contains("DESC2"));
				AssertEquals("Email body", true, body.Contains("ADDDESC21"));
			}
		}

		public void TestGenerateErrorMessageForExistingARInvoice()
		{
			var staff = SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = Factory.LoadTop1<OrgHeader>(new ZQuery());

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				//var invoicingJob = Factory.NewJobWithValidTestDataForTesting<Job>();
				var customDetail = Factory.New<MockCustomsJobProvider>();
				Job invoicingJob = new Job.Loader(customDetail).TryCreateWithoutMutexForTestOnly();
				// Prepare data to avoid Validation errors on saving Job before posting
				invoicingJob.JH_OA_AgentCollectAddr = TestObjectCreator.Agent.MainAddress.PK;
				invoicingJob.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;

				var existingInvoice = Factory.NewWithValidTestData<ARInvoice>();
				existingInvoice.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
				existingInvoice.AH_GB = GlbBranch.CurrentBranch.PK;

				var lineCharge1 = existingInvoice.Lines.AddNew() as ARInvoiceLine;
				lineCharge1.FillWithValidTestData();
				lineCharge1.AL_AC = TestObjectCreator.CC1.PK;
				lineCharge1.AL_Desc = "DESC1\r\n ADDDESC11\r\n ADDDESC12";
				lineCharge1.AL_OSExTaxAmount = 120m;
				lineCharge1.AL_OSTaxAmount = 0m;
				lineCharge1.AL_JH = invoicingJob.PK;
				TestObjectCreator.CreateCharge(lineCharge1, invoicingJob, chargeCode);

				var lineCharge2 = existingInvoice.Lines.AddNew() as ARInvoiceLine;
				lineCharge2.FillWithValidTestData();
				lineCharge2.AL_AC = TestObjectCreator.CC2.PK;
				lineCharge2.AL_Desc = "DESC2\r\n ADDDESC21\r\n";
				lineCharge2.AL_OSExTaxAmount = 200m;
				lineCharge2.AL_OSTaxAmount = 0m;
				lineCharge2.AL_JH = invoicingJob.PK;
				TestObjectCreator.CreateCharge(lineCharge2, invoicingJob, chargeCode);

				var cusCharge = GetCustomCharge(lineCharge1.ChargeCode, 122, 0, true, testCreditor.PK);
				cusCharge.AddCustomsCharge(new CustomsCharge(lineCharge2.ChargeCode, "TEST", 200, 0, true, testCreditor.PK));
				customDetail.JobNumber = "TR436877";
				customDetail.AutoPostingNotification = new AutoPostingNotification(new ZGuid[] { staff }, false);
				invoicingJob.PlugInData = customDetail;

				Factory.Save();
				var chargeProvider = new MockCustomsChargesProvider();
				chargeProvider.CustomsCharges = new ICustomsCharges[] { cusCharge };
				chargeProvider.CustomsJob = customDetail;
				chargeProvider.Factory = Factory;
				customDetail.DataProviders.Add(chargeProvider);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARPostDSB | ChargePosterBehaviours.SendEmail, new ZGuid[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				AssertEquals("one email should have been sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);

				var body = Env.OutgoingMailManager.EmailsCreated[0].Body;
				AssertEquals("Email body", true, body.Contains("DESC1"));
				AssertEquals("Email body", true, body.Contains("ADDDESC11"));
				AssertEquals("Email body", true, body.Contains("ADDDESC12"));
				AssertEquals("Email body", true, body.Contains("DESC2"));
				AssertEquals("Email body", true, body.Contains("ADDDESC21"));
			}
		}

		public void TestReportDiscrepancyOnlyForCustomsDisbursementCharges()
		{
			var staff = SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = Factory.LoadTop1<OrgHeader>(new ZQuery());

			using (AccountingConfigurationRegistry.Instance.NegativeCostValidationEnforced.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.AutoPostingNotification = new AutoPostingNotification(new ZGuid[] { staff }, false);
				var invoicingJob = new Job.Loader(customDetail).TryCreateWithoutMutexForTestOnly();
				invoicingJob.PlugInData = customDetail;
				// Prepare data to avoid Validation errors on saving Job before posting
				invoicingJob.JH_OA_AgentCollectAddr = TestObjectCreator.Agent.MainAddress.PK;
				invoicingJob.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
				GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;
				// Add some Charge to let Preposting Validation pass
				var jobCharge = invoicingJob.Charges.AddNew();
				jobCharge.JR_AC = TestObjectCreator.FRT.PK;
				jobCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;

				var existingInvoice = Factory.NewWithValidTestData<ARInvoice>();
				existingInvoice.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
				existingInvoice.AH_GB = GlbBranch.CurrentBranch.PK;

				var lineCharge1 = existingInvoice.Lines.AddNew() as ARInvoiceLine;
				lineCharge1.FillWithValidTestData();
				lineCharge1.AL_AC = chargeCode.PK;
				lineCharge1.AL_Desc = "DESC1\r\n ADDDESC11\r\n ADDDESC12";
				lineCharge1.AL_OSExTaxAmount = 122m;
				lineCharge1.AL_OSTaxAmount = 0m;
				lineCharge1.AL_JH = invoicingJob.PK;
				var charge1 = TestObjectCreator.CreateCharge(lineCharge1, invoicingJob, chargeCode);
				charge1.JR_OH_CostAccount = testCreditor.PK;
				charge1.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;

				var lineCharge2 = existingInvoice.Lines.AddNew() as ARInvoiceLine;
				lineCharge2.FillWithValidTestData();
				lineCharge2.AL_AC = Factory.NewWithValidTestData<AccChargeCode>().PK;//This should be disregarded
				lineCharge2.AL_Desc = "DESC2\r\n ADDDESC21\r\n";
				lineCharge2.AL_OSExTaxAmount = 200m;
				lineCharge2.AL_OSTaxAmount = 0m;
				lineCharge2.AL_JH = invoicingJob.PK;
				var charge2 = TestObjectCreator.CreateCharge(lineCharge2, invoicingJob, chargeCode);
				charge2.JR_OH_CostAccount = testCreditor.PK;
				charge2.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;

				var cusCharge = GetCustomCharge(chargeCode, 122, 0, true, testCreditor.PK);

				Factory.Save();

				var chargeProvider = new MockCustomsChargesProvider();
				chargeProvider.CustomsCharges = new ICustomsCharges[] { cusCharge };
				chargeProvider.InvoiceNumber = "INV123";
				chargeProvider.InvoiceDate = new ZDateTime(2005, 4, 12);
				chargeProvider.CustomsJob = customDetail;
				chargeProvider.Factory = Factory;
				customDetail.DataProviders.Add(chargeProvider);
				Env.OutgoingMailManager.EmailsCreated.Clear();
				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARPostDSB | ChargePosterBehaviours.SendEmail, new ZGuid[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				AssertEquals("one email should have been sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		public void TestReportDiscrepancyOnlyForCustomsDisbursementChargesForMultipleInvoices()
		{
			var staff = SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = Factory.LoadTop1<OrgHeader>(new ZQuery());

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				var invoicingJob = new Job.Loader(customDetail).TryCreateWithoutMutexForTestOnly();
				// Prepare data to avoid Validation errors on saving Job before posting
				invoicingJob.JH_OA_AgentCollectAddr = TestObjectCreator.Agent.MainAddress.PK;
				invoicingJob.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
				GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;
				// Add some Charge to let Preposting Validation pass
				var jobCharge = invoicingJob.Charges.AddNew();
				jobCharge.JR_AC = TestObjectCreator.FRT.PK;
				jobCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;

				var existingInvoice = Factory.NewWithValidTestData<ARInvoice>();
				existingInvoice.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
				existingInvoice.AH_GB = GlbBranch.CurrentBranch.PK;

				var lineCharge1 = existingInvoice.Lines.AddNew() as ARInvoiceLine;
				lineCharge1.FillWithValidTestData();
				lineCharge1.AL_AC = chargeCode.PK;
				lineCharge1.AL_Desc = "DESC1\r\n ADDDESC11\r\n ADDDESC12";
				lineCharge1.AL_OSExTaxAmount = 122m;
				lineCharge1.AL_OSTaxAmount = 0m;
				lineCharge1.AL_JH = invoicingJob.PK;
				TestObjectCreator.CreateJobCharge(lineCharge1, invoicingJob, chargeCode, TestObjectCreator.AUD);

				var existingInvoice2 = Factory.NewWithValidTestData<ARInvoice>();
				existingInvoice2.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
				existingInvoice2.AH_GB = GlbBranch.CurrentBranch.PK;

				var lineCharge2 = existingInvoice2.Lines.AddNew() as ARInvoiceLine;
				lineCharge2.FillWithValidTestData();
				lineCharge2.AL_AC = chargeCode.PK;
				lineCharge2.AL_Desc = "DESC1\r\n ADDDESC11\r\n ADDDESC12";
				lineCharge2.AL_OSExTaxAmount = 100m;
				lineCharge2.AL_OSTaxAmount = 0m;
				lineCharge2.AL_JH = invoicingJob.PK;
				TestObjectCreator.CreateJobCharge(lineCharge2, invoicingJob, chargeCode, TestObjectCreator.AUD);

				Factory.Save();

				customDetail.AutoPostingNotification = new AutoPostingNotification(new ZGuid[] { staff }, false);
				invoicingJob.PlugInData = customDetail;
				var cusCharge = GetCustomCharge(chargeCode, 222, 0, true, testCreditor.PK);

				var chargeProvider = new MockCustomsChargesProvider();
				chargeProvider.CustomsCharges = new ICustomsCharges[] { cusCharge };
				chargeProvider.InvoiceNumber = "INV123";
				chargeProvider.InvoiceDate = new ZDateTime(2005, 4, 12);
				chargeProvider.CustomsJob = customDetail;
				chargeProvider.Factory = Factory;
				customDetail.DataProviders.Add(chargeProvider);

				Env.OutgoingMailManager.EmailsCreated.Clear();
				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB | ChargePosterBehaviours.SendEmail, new ZGuid[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				AssertEquals("No email should have been sent. System should have collected all the AR/AP invoices for customs charge", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		public void TestNoExceptionWithNullCustomsJob()
		{
			ZGuid staff = SetupStaffMemberEmailAddress();

			AccChargeCode chargeCode = SetChargeCode();

			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);
			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid());

			var customDetail = Factory.New<MockCustomsJobProvider>();
			customDetail.fBranch = GlbBranch.CurrentBranch;
			customDetail.AutoPostingNotification = new AutoPostingNotification(new ZGuid[] { staff }, false);

			MockCustomCharge charge1 = GetCustomCharge(122, 0, false, TestObjectCreator.Creditor1.PK);
			MockCustomsChargesProvider chargeProvider = new MockCustomsChargesProvider();
			ICustomsCharges[] charges = new ICustomsCharges[] { charge1 };
			chargeProvider.InvoiceNumber = "ABC";
			chargeProvider.InvoiceDate = ZDateTime.Now;
			chargeProvider.CustomsCharges = charges;
			chargeProvider.CustomsJob = null;
			chargeProvider.Factory = Factory;
			chargeProvider.ReasonForUnbillability = "ReasonToFail";

			AssertNoExceptionThrown(() =>
			{
				var behaviour = ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB;
				customDetail.DataProviders.Add(chargeProvider);

				var poster = new CustomsDisbursementChargePoster(behaviour, new ZGuid[] { RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value });
				poster.RaiseInvoices(customDetail);
			});
		}

		void SetCustomsDeferredChargeInRegistry(Guid chargeCode, bool enabled)
		{
			RatingDataRegistry.Instance.CustomDeferredChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chargeCode);
			RatingDataRegistry.Instance.IncludeCustomDeferredChargeInInvoicing.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, enabled);
		}

		void RaiseTestInvoice(MockCustomsJobProvider customDetail)
		{
			RaiseTestInvoice(customDetail, ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB | ChargePosterBehaviours.SendEmail);
		}

		void RaiseTestInvoice(MockCustomsJobProvider customDetail, ChargePosterBehaviours behaviour)
		{
			var charge1 = GetCustomCharge(122, 0, true, RatingDataRegistry.Instance.CustomsDisbursementCreditor.Value);
			customDetail.JobNumber = "TR436877";

			var chargeProvider = new MockCustomsChargesProvider();
			chargeProvider.CustomsCharges = new ICustomsCharges[] { charge1 };
			chargeProvider.InvoiceNumber = "TR436877";
			chargeProvider.InvoiceDate = new ZDateTime(2005, 4, 12);
			chargeProvider.Factory = Factory;
			chargeProvider.CustomsJob = customDetail;
			chargeProvider.AutoPostingNotification = customDetail.AutoPostingNotification;
			customDetail.DataProviders.Add(chargeProvider);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			var poster = new CustomsDisbursementChargePoster(behaviour, new ZGuid[] { RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value });
			poster.RaiseInvoices(customDetail);
		}

		ZGuid SetupStaffMemberEmailAddress()
		{
			BusinessObjectFactory staffMemberFactory = new BusinessObjectFactory();
			GlbStaff currentStaffMember = staffMemberFactory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "PM"));
			currentStaffMember.GS_EmailAddress = EmailAddress;
			staffMemberFactory.Save();
			return currentStaffMember.PK;
		}

		protected ZString EmailAddress
		{
			get { return new ZString("blahblah@whatever.example"); }
		}

		protected IDisposable SetCustomsDisbursementDetailsInRegistry(OrgHeader testCreditor, AccChargeCode chargeCode, string countryCode)
		{
			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, testCreditor.PK.ToGuid());
			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid());

			return GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode);
		}

		protected AccChargeCode SetChargeCode()
		{
			AccTaxRate.LoadExistingOrCreateNewTaxRate(new BusinessObjectFactory(), "EXEMPT", AccTaxRate.Types.Exempt, 0).Factory.Save();

			ZQuery chargeFilter = new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			chargeFilter.AddToFilter(AccChargeCodeSchema.AC_AG_CostAccount, SQLComparisonOperator.NotEqual, null);
			chargeFilter.AddToFilter(AccChargeCodeSchema.AC_AG_AccrualAccount, SQLComparisonOperator.NotEqual, null);
			chargeFilter.AddToFilter(AccChargeCodeSchema.AC_AG_WIPAccount, SQLComparisonOperator.NotEqual, null);
			chargeFilter.AddToFilter(AccChargeCodeSchema.AC_ChargeType, SQLComparisonOperator.Equal, Constants.ChargeType.Disbursement);
			return Factory.LoadTop1(typeof(AccChargeCode), chargeFilter) as AccChargeCode;
		}

		protected MockCustomCharge GetCustomCharge(AccChargeCode chargeCode, ZDecimal amount, ZDecimal gST, ZBool paidByBroker, ZGuid creditorPK)
		{
			CustomsCharge customCharge1 = new CustomsCharge(chargeCode, "TEST", amount, gST, paidByBroker, creditorPK);
			MockCustomCharge charge1 = new MockCustomCharge();
			charge1.fIsActive = true;
			charge1.fCustomsCharges = new CustomsCharge[] { customCharge1 };
			return charge1;
		}

		protected MockCustomCharge GetCustomCharge(ZDecimal amount, ZDecimal gST, ZBool paidByBroker, ZGuid creditorPK)
		{
			return GetCustomCharge(null, amount, gST, paidByBroker, creditorPK);
		}

		protected override void SetUp()
		{
			base.SetUp();

			new AccountingPeriodTestHelper().SetupPeriods();

			var rate = AccTaxRate.Helper.FindTaxRate(new BusinessObjectFactory(), AccTaxRate.Helper.MainFreeGSTTaxRegistryID, Env.CurrentCompanyPK);
			rate.SetRateNumerator_ForTestOnly(0);
			rate.Factory.Save();
		}
	}
}
