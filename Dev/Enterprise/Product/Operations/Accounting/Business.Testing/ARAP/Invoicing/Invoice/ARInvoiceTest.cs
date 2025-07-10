using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(ARInvoice))]
	public class ARInvoiceTest : InvoiceTestForReceiptPayment
	{
		public void TestIEdocsParsingSupportProvider()
		{
			var bo = Factory.NewWithValidTestData<ARInvoice>();
			var eDocsParsingSupport = bo as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		public void TestReceiptPaymentAH_AB()
		{
			var cashAccount = TestObjectCreator.CreateBankAccount("TST", "Test Cash Account", TestObjectCreator.AUD, TestObjectCreator.GLHeader1);
			cashAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CSH;

			var invoice = Factory.New<ARInvoice>();
			AssertNotEquals(ReceiptTypes.Cash, invoice.ReceiptPaymentAH_ReceiptType);
			invoice.ReceiptPaymentAH_AB = cashAccount.PK;
			AssertEquals("Setting a cash account should set receipt type to CSH", ReceiptTypes.Cash, invoice.ReceiptPaymentAH_ReceiptType);
		}

		public override void TestShouldShowOriginalInvoiceReferenceFields()
		{
			base.TestShouldShowOriginalInvoiceReferenceFields();
			AssertEquals("ShouldShowOriginalInvoiceReferenceFields should be true.", true, CreateInvoiceWithOriginalTransactionReference("001").ShouldShowOriginalInvoiceReferenceFields);
		}

		public override void TestShouldShowOriginalInvoiceReferenceReasonFields()
		{
			base.TestShouldShowOriginalInvoiceReferenceReasonFields();
			AssertEquals("ShouldShowOriginalInvoiceReferenceReasonFields should be true.", true, CreateInvoiceWithOriginalTransactionReference("001").ShouldShowOriginalInvoiceReferenceReasonFields);
		}

		public void TestCheckLevelSecurityRightsWhenEnforceTwoApproverRequired()
		{
			var setting = new AuthorizationModeAndSettings();
			setting.AuthorizationMode = Constants.AuthorizationMode.Codes.TwoApprovers;
			var valuesForTest = setting.AuthorisationSettings;
			var upTo1000 = valuesForTest.AddNew();
			upTo1000.Amount = 1000;
			upTo1000.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			upTo1000.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			var upTo2000 = valuesForTest.AddNew();
			upTo2000.Amount = 2000;
			upTo2000.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			upTo2000.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			var above2000 = valuesForTest.AddNew();
			above2000.Amount = 2000;
			above2000.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			above2000.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, setting);

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_LocalExTaxAmount = 2500m;
			invoice.IsCreatingCreditNoteForReversal = true;
			invoice.IsCreatedFromApprovalRequest = false;
			invoice.SecurityOverrideProvider = new NonInteractiveSecurityOverrideProvider();
			(invoice.SecurityOverrideProvider as NonInteractiveSecurityOverrideProvider).OverrideLogin = "testuser";
			(invoice.SecurityOverrideProvider as NonInteractiveSecurityOverrideProvider).OverridePassword = "password";
			invoice.CheckLevelSecurityRights();
			Assert("Expect SecurityOverrideProviderIsInvoked is true due to enforce two approver triggers security check", invoice.SecurityOverrideProviderIsInvoked);
		}

		public void TestAuthorisationRequiredBranchDepartment_ReceivableReversalAuthorizationSettingsRegsitry()
		{
			var originalFirstAmountLevelAllows = Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed;
			var originalSecondAmountLevelAllows = Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed;
			try
			{
				var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
				arInvoice.IsCreatingCreditNoteForReversal = true;
				TestObjectCreator.SetBranchDepartmentAuthorizationLevelSettings(arInvoice.Branch.PK, arInvoice.Department.PK, 1000m, 2000m, true);
				arInvoice.AH_LocalExTaxAmount = 955M;
				AssertCorrectAuthorisationRequired(arInvoice, false, false);
				arInvoice.AH_LocalExTaxAmount = 1955M;
				AssertCorrectAuthorisationRequired(arInvoice, true, false);
				arInvoice.AH_LocalExTaxAmount = 2955M;
				AssertCorrectAuthorisationRequired(arInvoice, false, true);
			}
			finally
			{
				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = originalFirstAmountLevelAllows;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = originalSecondAmountLevelAllows;
			}
		}

		public override void TestImportSingleCostPreserveIndexOfImportedUniversalTransactionLineValue()
		{
			Assert("Not applicable", true);
		}

		public override void TestImportAllApportionmentsFromCostingPreserveIndexOfImportedUniversalTransactionLineValue()
		{
			Assert("Not applicable", true);
		}

		public void TestAH_InvoiceTerm_ReadOnly()
		{
			IAmending original = InvoicingBase as IAmending;
			AssertNotNull("Should be IAmending", original);
			Assert("Should not be AmendingTransaction", !original.IsAmendingTransaction);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.NewReceivablesInvoiceTerm.IsAllowed = true;
			AssertEquals(false, InvoicingBase.AH_InvoiceTermInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Env.Security.NewReceivablesInvoiceTerm.IsAllowed = true;
			AssertEquals(true, InvoicingBase.AH_InvoiceTermInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.NewReceivablesInvoiceTerm.IsAllowed = false;
			AssertEquals(true, InvoicingBase.AH_InvoiceTermInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Env.Security.NewReceivablesInvoiceTerm.IsAllowed = false;
			AssertEquals(true, InvoicingBase.AH_InvoiceTermInfo.ReadOnly);

			var amending = original.GenerateAmendingTransaction(InvoicingBase.AH_TransactionType) as InvoicingBase;
			AssertNotNull("Should generate Amending transaction", amending);
			Assert("Should be AmendingTransaction", amending.IsAmendingTransaction);

			var shipment = TestObjectCreator.CreateShipment("1001");
			amending.AH_JH = Job.PK;
			amending.InvoicingJob.PlugInData = shipment;

			var invoicingBase = amending.GetType().BaseType.BaseType;
			var info = invoicingBase.GetProperty("SecurityHelper", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			var securityTestHelper = info.GetValue(amending, null) as JobInvoicingSecurityHelper;
			var securityCheckPoint = securityTestHelper.GetInvSecurity(SecurityCore.AllowOverrideARInvoiceTermWAmendTransactionInvoice);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			securityCheckPoint.IsAllowed = true;
			AssertEquals(false, amending.AH_InvoiceTermInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			securityCheckPoint.IsAllowed = true;
			AssertEquals(true, amending.AH_InvoiceTermInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			securityCheckPoint.IsAllowed = false;
			AssertEquals(true, amending.AH_InvoiceTermInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			securityCheckPoint.IsAllowed = false;
			AssertEquals(true, amending.AH_InvoiceTermInfo.ReadOnly);
		}

		public void TestAH_InvoiceTermDays_ReadOnly()
		{
			IAmending original = InvoicingBase as IAmending;
			AssertNotNull("Should be IAmending", original);
			Assert("Should not be AmendingTransaction", !original.IsAmendingTransaction);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			InvoicingBase.AH_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			Env.Security.NewReceivablesInvoiceTerm.IsAllowed = true;
			AssertEquals(true, InvoicingBase.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			InvoicingBase.AH_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			Env.Security.NewReceivablesInvoiceTerm.IsAllowed = true;
			AssertEquals(true, InvoicingBase.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			InvoicingBase.AH_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			Env.Security.NewReceivablesInvoiceTerm.IsAllowed = false;
			AssertEquals(true, InvoicingBase.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			InvoicingBase.AH_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			Env.Security.NewReceivablesInvoiceTerm.IsAllowed = false;
			AssertEquals(true, InvoicingBase.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			InvoicingBase.AH_InvoiceTerm = Constants.InvoiceTerms.PaymentInAdvance;
			Env.Security.NewReceivablesInvoiceTerm.IsAllowed = true;
			AssertEquals(true, InvoicingBase.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			InvoicingBase.AH_InvoiceTerm = Constants.InvoiceTerms.PaymentInAdvance;
			Env.Security.NewReceivablesInvoiceTerm.IsAllowed = true;
			AssertEquals(true, InvoicingBase.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			InvoicingBase.AH_InvoiceTerm = Constants.InvoiceTerms.PaymentInAdvance;
			Env.Security.NewReceivablesInvoiceTerm.IsAllowed = false;
			AssertEquals(true, InvoicingBase.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			InvoicingBase.AH_InvoiceTerm = Constants.InvoiceTerms.PaymentInAdvance;
			Env.Security.NewReceivablesInvoiceTerm.IsAllowed = false;
			AssertEquals(true, InvoicingBase.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			InvoicingBase.AH_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			Env.Security.NewReceivablesInvoiceTerm.IsAllowed = true;
			AssertEquals(false, InvoicingBase.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			InvoicingBase.AH_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			Env.Security.NewReceivablesInvoiceTerm.IsAllowed = true;
			AssertEquals(true, InvoicingBase.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			InvoicingBase.AH_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			Env.Security.NewReceivablesInvoiceTerm.IsAllowed = false;
			AssertEquals(true, InvoicingBase.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			InvoicingBase.AH_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			Env.Security.NewReceivablesInvoiceTerm.IsAllowed = false;
			AssertEquals(true, InvoicingBase.AH_InvoiceTermDaysInfo.ReadOnly);

			var amending = original.GenerateAmendingTransaction(InvoicingBase.AH_TransactionType) as InvoicingBase;
			AssertNotNull("Should generate Amending transaction", amending);
			Assert("Should be AmendingTransaction", amending.IsAmendingTransaction);

			var shipment = TestObjectCreator.CreateShipment("1001");
			amending.AH_JH = Job.PK;
			amending.InvoicingJob.PlugInData = shipment;

			var invoicingBase = amending.GetType().BaseType.BaseType;
			var info = invoicingBase.GetProperty("SecurityHelper", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			var securityTestHelper = info.GetValue(amending, null) as JobInvoicingSecurityHelper;
			var securityCheckPoint = securityTestHelper.GetInvSecurity(SecurityCore.AllowOverrideARInvoiceTermWAmendTransactionInvoice);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			securityCheckPoint.IsAllowed = true;
			amending.AH_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			AssertEquals(true, amending.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			securityCheckPoint.IsAllowed = true;
			amending.AH_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			AssertEquals(true, amending.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			securityCheckPoint.IsAllowed = false;
			amending.AH_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			AssertEquals(true, amending.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			securityCheckPoint.IsAllowed = false;
			amending.AH_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			AssertEquals(true, amending.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			securityCheckPoint.IsAllowed = true;
			amending.AH_InvoiceTerm = Constants.InvoiceTerms.PaymentInAdvance;
			AssertEquals(true, amending.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			securityCheckPoint.IsAllowed = true;
			amending.AH_InvoiceTerm = Constants.InvoiceTerms.PaymentInAdvance;
			AssertEquals(true, amending.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			securityCheckPoint.IsAllowed = false;
			amending.AH_InvoiceTerm = Constants.InvoiceTerms.PaymentInAdvance;
			AssertEquals(true, amending.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			securityCheckPoint.IsAllowed = false;
			amending.AH_InvoiceTerm = Constants.InvoiceTerms.PaymentInAdvance;
			AssertEquals(true, amending.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			securityCheckPoint.IsAllowed = true;
			amending.AH_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			AssertEquals(false, amending.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			securityCheckPoint.IsAllowed = true;
			amending.AH_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			AssertEquals(true, amending.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			securityCheckPoint.IsAllowed = false;
			amending.AH_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			AssertEquals(true, amending.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			securityCheckPoint.IsAllowed = false;
			amending.AH_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			AssertEquals(true, amending.AH_InvoiceTermDaysInfo.ReadOnly);
		}

		public void TestAH_InvoiceDate_ReadOnly()
		{
			var original = InvoicingBase as IAmending;
			AssertNotNull("Should be IAmending.", original);
			Assert("Should not be AmendingTransaction.", !original.IsAmendingTransaction);

			Env.Security.NewReceivablesInvoiceInvoiceDate.IsAllowed = true;
			Assert("Should not be readonly as the security right is true.", !InvoicingBase.AH_InvoiceDateInfo.ReadOnly);

			Env.Security.NewReceivablesInvoiceInvoiceDate.IsAllowed = false;
			Assert("Should not be readonly as the security right is false.", InvoicingBase.AH_InvoiceDateInfo.ReadOnly);

			var amending = original.GenerateAmendingTransaction(InvoicingBase.AH_TransactionType) as InvoicingBase;
			AssertNotNull("Should generate Amending transaction.", amending);
			Assert("Should be AmendingTransaction.", amending.IsAmendingTransaction);

			var shipment = TestObjectCreator.CreateShipment("1001");
			amending.AH_JH = Job.PK;
			amending.InvoicingJob.PlugInData = shipment;

			Env.Security.NewReceivablesInvoiceInvoiceDate.IsAllowed = true;
			Assert("Should not be readonly as the security right is true.", !amending.AH_InvoiceDateInfo.ReadOnly);

			Env.Security.NewReceivablesInvoiceInvoiceDate.IsAllowed = false;
			Assert("Should be readonly as the security right is false.", amending.AH_InvoiceDateInfo.ReadOnly);

			using (AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingConstants.InvAndPstDateDefaultingRuleTypes.MonthEndSuspension.Code))
			{
				Env.Security.NewReceivablesInvoiceInvoiceDate.IsAllowed = true;
				Assert("Should be readonly as the registry is set to MTH and then it doesn't matter to the security right.", InvoicingBase.AH_InvoiceDateInfo.ReadOnly);

				Env.Security.NewReceivablesInvoiceInvoiceDate.IsAllowed = false;
				Assert("Should be readonly as the registry is set to MTH and then it doesn't matter to the security right.", InvoicingBase.AH_InvoiceDateInfo.ReadOnly);
			}
		}

		public void TestDeclarationAdditionalReferenceNumber()
		{
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_Status = JobHeaderStatus.JobReadyForCostPosting.Code;
			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();

			TestObjectCreator.SetupRegistrySetJobStatusToInvoicedWhenFirstARInvoicePosted(Env.CurrentCompany.PK, new JobHeaderStatusList());
			invoice.AH_JH = job.PK;

			AssertNotNull(invoice.Job);

			var mockDeclaration = Factory.New<BaseJobDeclarationForTesting>();
			mockDeclaration.EntryDetailsInARInvoiceReturns = new ZString("ENT123456789");

			job.JH_ParentID = mockDeclaration.PK;
			job.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			AssertEquals("Declaration Addtiional Reference Number", "ENT123456789", invoice.DeclarationEntryDetails);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			AssertEquals("DeclarationEntryDetails", "", invoice.DeclarationEntryDetails);

			mockDeclaration.JE_JS = shipment.PK;
			AssertEquals("DeclarationEntryDetails", "ENT123456789", invoice.DeclarationEntryDetails);
		}

		public void TestSetJobStatusToInvoicedWhenANewARInvoicePosted()
		{
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_Status = JobHeaderStatus.JobReadyForCostPosting.Code;
			Factory.Save();
			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();

			TestObjectCreator.SetupRegistrySetJobStatusToInvoicedWhenFirstARInvoicePosted(Env.CurrentCompany.PK, new JobHeaderStatusList());
			invoice.AH_JH = job.PK;

			Factory.Save();
			AssertEquals("Status must stay unchanged because registry is false", JobHeaderStatus.JobReadyForCostPosting.Code, job.JH_Status);

			job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_Status = JobHeaderStatus.JobReadyForCostPosting.Code;
			Factory.Save();
			APInvoice apInvoice = Factory.NewWithValidTestData<APInvoice>();
			invoice = Factory.NewWithValidTestData<ARInvoice>();

			TestObjectCreator.SetupRegistrySetJobStatusToInvoicedWhenFirstARInvoicePosted(Env.CurrentCompany.PK, null);
			invoice.AH_JH = job.PK;
			apInvoice.AH_JH = job.PK;

			Factory.Save();
			AssertEquals("Status must be changed because registry is true and this is first ARInvoice for this Job", JobHeaderStatus.JobInvoiced.Code, job.JH_Status);

			job.JH_Status = JobHeaderStatus.JobReadyForCostPosting.Code;
			Factory.Save();

			invoice.AH_Desc = "Speeding fine";
			Factory.Save();

			AssertEquals("Status must stay unchanged because this ARInvoice already in DataBase", JobHeaderStatus.JobReadyForCostPosting.Code, job.JH_Status);

			ARInvoice invoice2 = Factory.NewWithValidTestData<ARInvoice>();
			invoice2.AH_JH = job.PK;

			Factory.Save();
			AssertEquals("Status must change because a new ARInvoice has been posted for this job", JobHeaderStatus.JobInvoiced.Code, job.JH_Status);
		}

		public void TestAllowedJobStatusesGetUpdatedToInvoicedStatusWhenANewARInvoicePosted()
		{
			var list = new CodeDescriptionPairList();
			list.Add(JobHeaderStatus.Complete);
			TestObjectCreator.SetupRegistrySetJobStatusToInvoicedWhenFirstARInvoicePosted(Env.CurrentCompany.PK, list);

			var job1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job1.JH_Status = JobHeaderStatus.JobReadyForCostPosting.Code;

			var job2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job2.JH_Status = JobHeaderStatus.Complete.Code;

			var job3 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job3.JH_Status = JobHeaderStatus.Working.Code;

			Factory.Save();
			var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
			invoice1.AH_JH = job1.PK;

			var invoice2 = Factory.NewWithValidTestData<ARInvoice>();
			invoice2.AH_JH = job2.PK;

			var invoice3 = Factory.NewWithValidTestData<ARInvoice>();
			invoice3.AH_JH = job3.PK;

			Factory.Save();

			AssertEquals("Status must be changed because registry is false for 'JRC' and this is first ARInvoice for this Job", JobHeaderStatus.JobInvoiced.Code, job1.JH_Status);
			AssertEquals("Status must remain unchanged because registry is true for 'CMP' and this is first ARInvoice for this Job", JobHeaderStatus.Complete.Code, job2.JH_Status);
			AssertEquals("Status must be changed because registry is false for 'WRK' and this is first ARInvoice for this Job", JobHeaderStatus.JobInvoiced.Code, job3.JH_Status);
		}

		public void TestSetJobStatusToInvoicedWhenANewARInvoicePosted_MultipleJobInvoices()
		{
			JobHeader job1 = TestObjectCreator.CreateJobHeader();
			JobHeader job2 = TestObjectCreator.CreateJobHeader();
			job1.JH_Status = JobHeaderStatus.JobReadyForCostPosting.Code;
			job2.JH_Status = JobHeaderStatus.JobReadyForCostPosting.Code;
			Factory.Save();
			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();

			TestObjectCreator.SetupRegistrySetJobStatusToInvoicedWhenFirstARInvoicePosted(Env.CurrentCompany.PK, new JobHeaderStatusList());

			ARInvoiceLine line = (ARInvoiceLine)invoice.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.FillWithValidTestData();
			line.AL_JH = job1.PK;
			JobCharge charge = TestObjectCreator.CreateJobCharge(line, job1, TestObjectCreator.CC1, TestObjectCreator.AUD);
			line = (ARInvoiceLine)invoice.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.FillWithValidTestData();
			line.AL_JH = job2.PK;
			charge = TestObjectCreator.CreateJobCharge(line, job2, TestObjectCreator.CC1, TestObjectCreator.AUD);
			Factory.Save();
			AssertEquals("Status must stay unchanged because registry is false.", JobHeaderStatus.JobReadyForCostPosting.Code, job1.JH_Status);
			AssertEquals("Status must stay unchanged because registry is false.", JobHeaderStatus.JobReadyForCostPosting.Code, job2.JH_Status);

			job1 = TestObjectCreator.CreateJobHeader();
			job2 = TestObjectCreator.CreateJobHeader();
			job1.JH_Status = JobHeaderStatus.JobReadyForCostPosting.Code;
			job2.JH_Status = JobHeaderStatus.JobReadyForCostPosting.Code;
			Factory.Save();
			invoice = Factory.NewWithValidTestData<ARInvoice>();

			TestObjectCreator.SetupRegistrySetJobStatusToInvoicedWhenFirstARInvoicePosted(Env.CurrentCompany.PK, null);

			invoice.AH_ConsolidatedInvoiceRef = "1234";
			line = (ARInvoiceLine)invoice.Lines.AddNew();
			line.FillWithValidTestData();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.AL_JH = job1.PK;
			charge = TestObjectCreator.CreateJobCharge(line, job1, TestObjectCreator.CC1, TestObjectCreator.AUD);
			line = (ARInvoiceLine)invoice.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.FillWithValidTestData();
			line.AL_JH = job2.PK;
			charge = TestObjectCreator.CreateJobCharge(line, job2, TestObjectCreator.CC1, TestObjectCreator.AUD);
			Factory.Save();
			AssertEquals("Status must stay unchanged because it is consol AR invoice.", JobHeaderStatus.JobReadyForCostPosting.Code, job1.JH_Status);
			AssertEquals("Status must stay unchanged because it is consol AR invoice.", JobHeaderStatus.JobReadyForCostPosting.Code, job2.JH_Status);

			job1 = TestObjectCreator.CreateJobHeader();
			job2 = TestObjectCreator.CreateJobHeader();
			job1.JH_Status = JobHeaderStatus.JobReadyForCostPosting.Code;
			job2.JH_Status = JobHeaderStatus.JobReadyForCostPosting.Code;
			Factory.Save();
			invoice = Factory.NewWithValidTestData<ARInvoice>();
			line = (ARInvoiceLine)invoice.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.FillWithValidTestData();
			line.AL_JH = job1.PK;
			charge = TestObjectCreator.CreateJobCharge(line, job1, TestObjectCreator.CC1, TestObjectCreator.AUD);
			line = (ARInvoiceLine)invoice.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.FillWithValidTestData();
			line.AL_JH = job2.PK;
			charge = TestObjectCreator.CreateJobCharge(line, job2, TestObjectCreator.CC1, TestObjectCreator.AUD);
			Factory.Save();
			AssertEquals("Status must be changed because it is the first non consol AR invoice for this job.", JobHeaderStatus.JobInvoiced.Code, job1.JH_Status);
			AssertEquals("Status must be changed because it is the first non consol AR invoice for this job.", JobHeaderStatus.JobInvoiced.Code, job2.JH_Status);

			job1.JH_Status = JobHeaderStatus.JobReadyForCostPosting.Code;
			Factory.Save();
			invoice.AH_Desc = "Speeding fine";
			Factory.Save();
			AssertEquals("Status must stay unchanged because this ARInvoice already in DataBase", JobHeaderStatus.JobReadyForCostPosting.Code, job1.JH_Status);

			invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_JH = job1.PK;
			Factory.Save();
			AssertEquals("Status must change to invoice because a new ARInvoice has been posted for this job", JobHeaderStatus.JobInvoiced.Code, job1.JH_Status);

			job2.JH_Status = JobHeaderStatus.JobReadyForCostPosting.Code;
			Factory.Save();
			invoice = Factory.NewWithValidTestData<ARInvoice>();
			line = (ARInvoiceLine)invoice.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.FillWithValidTestData();
			line.AL_JH = job2.PK;
			charge = TestObjectCreator.CreateJobCharge(line, job2, TestObjectCreator.CC1, TestObjectCreator.AUD);
			Factory.Save();
			AssertEquals("Status must change to invoice because a new ARInvoice has been posted for this job", JobHeaderStatus.JobInvoiced.Code, job2.JH_Status);
		}

		public void TestOnlyAllowedJobStatusesGetUpdatedToInvoicedStatusWhenANewARInvoicePosted_MultipleJobInvoices()
		{
			var list = new CodeDescriptionPairList();
			list.Add(JobHeaderStatus.WorkOnHold);
			TestObjectCreator.SetupRegistrySetJobStatusToInvoicedWhenFirstARInvoicePosted(Env.CurrentCompany.PK, list);

			var job1 = TestObjectCreator.CreateJobHeader();
			var job2 = TestObjectCreator.CreateJobHeader();
			job1.JH_Status = JobHeaderStatus.JobReadyForCostPosting.Code;
			job2.JH_Status = JobHeaderStatus.WorkOnHold.Code;
			Factory.Save();

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var line = (ARInvoiceLine)invoice.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.FillWithValidTestData();
			line.AL_JH = job1.PK;
			var charge = TestObjectCreator.CreateJobCharge(line, job1, TestObjectCreator.CC1, TestObjectCreator.AUD);

			line = (ARInvoiceLine)invoice.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.FillWithValidTestData();
			line.AL_JH = job2.PK;
			charge = TestObjectCreator.CreateJobCharge(line, job2, TestObjectCreator.CC1, TestObjectCreator.AUD);

			Factory.Save();

			AssertEquals("Status must be changed because registry is false for 'JRC' and this is first ARInvoice for this Job", JobHeaderStatus.JobInvoiced.Code, job1.JH_Status);
			AssertEquals("Status must remain unchanged because registry is true for 'CMP' and this is first ARInvoice for this Job", JobHeaderStatus.WorkOnHold.Code, job2.JH_Status);
		}

		public void TestOnlyAllowedJobStatusesGetUpdatedToInvoicedStatusWhenANewARInvoicePosted_DifferentCompany()
		{
			var list = new CodeDescriptionPairList();
			list.Add(JobHeaderStatus.Complete);
			TestObjectCreator.SetupRegistrySetJobStatusToInvoicedWhenFirstARInvoicePosted(TestObjectCreator.NonCurrentCompanyBranch.Company.PK.ToGuid(), list);
			TestObjectCreator.SetupRegistrySetJobStatusToInvoicedWhenFirstARInvoicePosted(Env.CurrentCompany.PK, null);

			var job1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job1.JH_Status = JobHeaderStatus.JobReadyForCostPosting.Code;

			var job2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job2.JH_Status = JobHeaderStatus.Complete.Code;

			var job3 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job3.JH_Status = JobHeaderStatus.Working.Code;

			Factory.Save();

			var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
			invoice1.AH_JH = job1.PK;
			invoice1.AH_GB = TestObjectCreator.NonCurrentCompanyBranch.PK;
			invoice1.AH_GC = TestObjectCreator.NonCurrentCompanyBranch.Company.PK;

			var invoice2 = Factory.NewWithValidTestData<ARInvoice>();
			invoice2.AH_JH = job2.PK;
			invoice2.AH_GB = TestObjectCreator.NonCurrentCompanyBranch.PK;
			invoice2.AH_GC = TestObjectCreator.NonCurrentCompanyBranch.Company.PK;

			var invoice3 = Factory.NewWithValidTestData<ARInvoice>();
			invoice3.AH_JH = job3.PK;
			invoice3.AH_GB = TestObjectCreator.NonCurrentCompanyBranch.PK;
			invoice3.AH_GC = TestObjectCreator.NonCurrentCompanyBranch.Company.PK;

			Factory.Save();

			AssertEquals("Status must be changed because registry is false for 'JRC' and this is first ARInvoice for this Job", JobHeaderStatus.JobInvoiced.Code, job1.JH_Status);
			AssertEquals("Status must remain unchanged because registry is true for 'CMP' and this is first ARInvoice for this Job", JobHeaderStatus.Complete.Code, job2.JH_Status);
			AssertEquals("Status must be changed because registry is false for 'WRK' and this is first ARInvoice for this Job", JobHeaderStatus.JobInvoiced.Code, job3.JH_Status);
		}

		#region Copying

		public void TestCopyLinesCopyDescription()
		{
			InvoicingBase.Lines.AddNew();
			InvoicingBase.Lines.AddNew();

			InvoicingBase.Lines[0].AL_AC = ChargeCode.PK;
			InvoicingBase.Lines[1].AL_AC = ChargeCode.PK;

			InvoicingBase.Lines[0].AL_Desc = "Line Description 1";
			InvoicingBase.Lines[1].AL_Desc = "Line Description 2";

			TransactionHeader copiedARInvoice = ((ARInvoice)InvoicingBase).CopyTransaction_ForTestOnly();

			AssertEquals(ChargeCode.PK, ((ARInvoice)copiedARInvoice).Lines[0].AL_AC);
			AssertEquals(ChargeCode.PK, ((ARInvoice)copiedARInvoice).Lines[1].AL_AC);
			AssertEquals("Line Description 1", ((ARInvoice)copiedARInvoice).Lines[0].AL_Desc);
			AssertEquals("Line Description 2", ((ARInvoice)copiedARInvoice).Lines[1].AL_Desc);
		}

		#endregion

		[UseSnapshotProtection]
		public void TestNumberFountainWhenTransactionTypeChanges()
		{
			InvoicingBase.AH_TransactionType = TransactionTypes.Invoice;
			AssertEquals("Should be AR Invoice number fountain", Env.NumberFountains.ARInvoiceNo.GetTodaysPeriodFountain(), ((ARInvoice)InvoicingBase).NumberFountainForTransactionNumber_ForTestOnly.numberFountainPooler.GetTodaysPeriodFountain());
			AssertNotEquals("Should be AR Invoice number fountain", Env.NumberFountains.ARCreditNoteNo.GetTodaysPeriodFountain(), ((ARInvoice)InvoicingBase).NumberFountainForTransactionNumber_ForTestOnly.numberFountainPooler.GetTodaysPeriodFountain());

			ShareSequentialTransactionNumbers item = new ShareSequentialTransactionNumbers();
			item.Value = true;
			AccountingConfigurationRegistry.Instance.ShareSequentialInvoiceTransactionNumbers.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, item);
			AssertEquals("Should be AR Invoice number fountain", Env.NumberFountains.ARInvoiceNo.GetTodaysPeriodFountain(), ((ARInvoice)InvoicingBase).NumberFountainForTransactionNumber_ForTestOnly.numberFountainPooler.GetTodaysPeriodFountain());
			AssertNotEquals("Should be AR Invoice number fountain", Env.NumberFountains.ARCreditNoteNo.GetTodaysPeriodFountain(), ((ARInvoice)InvoicingBase).NumberFountainForTransactionNumber_ForTestOnly.numberFountainPooler.GetTodaysPeriodFountain());
			item.Value = false;
			AccountingConfigurationRegistry.Instance.ShareSequentialInvoiceTransactionNumbers.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, item);

			AssertEquals("Should be AR Invoice number fountain", Env.NumberFountains.ARInvoiceNo.GetTodaysPeriodFountain(), ((ARInvoice)InvoicingBase).NumberFountainForTransactionNumber_ForTestOnly.numberFountainPooler.GetTodaysPeriodFountain());
			AssertNotEquals("Should be AR Invoice number fountain", Env.NumberFountains.ARCreditNoteNo.GetTodaysPeriodFountain(), ((ARInvoice)InvoicingBase).NumberFountainForTransactionNumber_ForTestOnly.numberFountainPooler.GetTodaysPeriodFountain());
		}

		public void TestReceiptPaymentOnlyIfUserSelected()
		{
			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.Equal, TransactionTypes.Receipt);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			TransactionHeader[] transactions = (TransactionHeader[])Factory.Load(typeof(TransactionHeader), filter);
			int receiptCount = transactions.Length;

			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_IsDebtor = true;
			header.MiscServ.OM_ARReceiptInvoiceAfterPostingDefault = true;
			header.CompanyData.SetARTaxApplicable(true);

			ARInvoice invoice = InvoicingBase as ARInvoice;
			invoice.Lines.RemoveAndDeleteAll();
			invoice.AH_OH = header.PK;
			invoice.AH_Desc = "Test Invoice";
			invoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			invoice.AH_OSExTaxAmount = 100M;
			invoice.AH_OSTaxAmount = 10M;
			invoice.AH_ExchangeRate = 1M;
			Factory.Save();

			transactions = (TransactionHeader[])Factory.Load(typeof(TransactionHeader), filter);

			AssertEquals("Receipt should not be created from default", receiptCount, transactions.Length);
		}

		public void TestSourceReferenceMutex()
		{
			var factory1 = new BusinessObjectFactory();
			var factory2 = new BusinessObjectFactory();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
			using (factory1.AddDisposableService())
			using (factory2.AddDisposableService())
			{
				var invoice1 = factory1.NewWithValidTestData<ARInvoice>();
				invoice1.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.TXM;
				invoice1.SourceReference = "FTM a10";

				var invoice1Mutex = TransactionSourceReferenceMutexService.GetTransactionSourceReferenceMutexService(invoice1.Factory).GetSourceReferenceMutex(invoice1.Company.GC_Code, invoice1.SourceReference);
				Assert(!invoice1Mutex.IsLocked);

				invoice1.RunPreSaveValidation();
				Assert(invoice1Mutex.IsLocked);
				Assert(invoice1Mutex.HasLock);

				var invoice2 = factory2.NewWithValidTestData<ARInvoice>();
				invoice2.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.TXM;
				invoice2.SourceReference = "FTM a10";

				var invoice2Mutex = TransactionSourceReferenceMutexService.GetTransactionSourceReferenceMutexService(invoice2.Factory).GetSourceReferenceMutex(invoice2.Company.GC_Code, invoice2.SourceReference);
				Assert(invoice2Mutex.IsLocked);
				Assert(!invoice2Mutex.HasLock);

				invoice1.SourceReference = "FTM a11";
				Assert(!invoice1Mutex.IsLocked);
				Assert(!invoice2Mutex.IsLocked);

				invoice2.RunPreSaveValidation();
				Assert(invoice2Mutex.IsLocked);
				invoice2.UnlockAllSourceReferenceMutexes();
				Assert(!invoice2Mutex.IsLocked);
			}
		}

		public void TestSourceReferenceMutexUnlockWhenInvoiceIsSaved()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
			using (Factory.AddDisposableService())
			{
				var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
				invoice1.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.TXM;
				invoice1.SourceReference = "FTM a10";

				invoice1.RunPreSaveValidation();
				var invoice1Mutex = TransactionSourceReferenceMutexService.GetTransactionSourceReferenceMutexService(invoice1.Factory).GetSourceReferenceMutex(invoice1.Company.GC_Code, invoice1.SourceReference);
				Assert(invoice1Mutex.IsLocked);
				Assert(invoice1Mutex.HasLock);

				Factory.Save();
				Assert(!invoice1Mutex.IsLocked);
			}
		}

		public void TestSourceReferenceMutexIsDisposedWithFactory()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
			{
				ZGlobalMutex invoiceMutex;
				var factory = new BusinessObjectFactory();
				using (factory.AddDisposableService())
				{
					var invoice = factory.NewWithValidTestData<ARInvoice>();
					invoice.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.TXM;
					invoice.SourceReference = "FTM a10";
					invoice.RunPreSaveValidation();
					invoiceMutex = TransactionSourceReferenceMutexService.GetTransactionSourceReferenceMutexService(invoice.Factory).GetSourceReferenceMutex(invoice.Company.GC_Code, invoice.SourceReference);
					Assert(invoiceMutex.IsLocked);
					Assert(invoiceMutex.HasLock);
				}
				Assert(!invoiceMutex.IsLocked);
			}
		}

		public void TestSourceReferenceMutexDisposableManagerIsCreatedWhenMissing()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
			{
				var invoice = Factory.NewWithValidTestData<ARInvoice>();
				invoice.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.TXM;
				invoice.SourceReference = "FTM a10";
				Assert(!Factory.TryGetDisposableManager(out var disposableManager));
				invoice.RunPreSaveValidation();
				var invoiceMutex = TransactionSourceReferenceMutexService.GetTransactionSourceReferenceMutexService(invoice.Factory).GetSourceReferenceMutex(invoice.Company.GC_Code, invoice.SourceReference);
				Assert(invoiceMutex.IsLocked);
				Assert(Factory.TryGetDisposableManager(out disposableManager));
				disposableManager.Dispose();
			}
		}

		public void TestARInvoiceReversed()
		{
			ARInvoice invoice = InvoicingBase as ARInvoice;
			invoice.Lines.RemoveAndDeleteAll();
			invoice.AH_OH = Header.PK;
			invoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			invoice.AH_ExchangeRate = 1M;
			invoice.Lines.Add(Factory.NewWithValidTestData<ARInvoiceLine>());
			invoice.Lines.Add(Factory.NewWithValidTestData<ARInvoiceLine>());
			invoice.Lines.Add(Factory.NewWithValidTestData<ARInvoiceLine>());

			invoice.GenerateReverseTransaction(true);
			TransactionHeaderWithLines reversingHeader = ((IReversing)invoice).ReverseTransaction as TransactionHeaderWithLines;

			AssertEquals("Reversed AR Transaction type", TransactionTypes.CreditNote, reversingHeader.AH_TransactionType);
			AssertEquals("Reversing has credit note lines", 3, reversingHeader.Lines.Count);
		}

		public void TestDocManagerCode()
		{
			ARInvoice invoice = Factory.New<ARInvoice>();
			AssertEquals("Code should be RIN. Any change to the IDocManagerSupport interface must also be changed in document scanning lookup", "RIN", ((IDocManagerSupport)invoice).DocManagerInfo.DocManagerCode);
		}

		protected override Type TypeOfValidation
		{
			get { return typeof(InvoiceValidation); }
		}

		protected override Type GetExpectedBusinessObjectLineType()
		{
			return typeof(ARInvoiceLine);
		}

		public override void TestReceiptPaymentBankAccount()
		{
			GlbCompany diffCompany = Factory.NewWithValidTestData<GlbCompany>();

			RefCurrency currency = Factory.NewWithValidTestData<RefCurrency>();

			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			bank.AB_IsDefaultReceiptBankAccount = true;
			bank.AB_RX_NKAccountCurrency = currency.RX_Code;
			bank.AB_GB = ZGuid.Empty;
			bank.AB_GC = diffCompany.PK;

			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			aRInv.AH_RX_NKTransactionCurrency = currency.RX_Code;
			aRInv.SubmittedFromInvoicingForm = true;
			aRInv.IsInvoiceReceiptPayment = true;

			Assert("Default bank account should be empty", aRInv.DefaultBankAccount_ForTestOnly.IsEmpty);
		}

		public override void TestSettingDrawerDetailsForInvoice()
		{
			base.TestSettingDrawerDetailsForInvoice();
			AssertEquals("Cheque Drawer is set", TestInvoice.ReceiptPayment.AH_ChequeDrawer, "Cheque Drawer");
			AssertEquals("Drawer Bank is not set", TestInvoice.ReceiptPayment.AH_DrawerBank, "Drawer Bank");
			AssertEquals("Drawer Branch is not set", TestInvoice.ReceiptPayment.AH_DrawerBranch, "Drawer Branch");
		}

		public override void TestCheckBankAccountAndCheckBookValidationOnChangingPaymentType()
		{
			base.TestCheckBankAccountAndCheckBookValidationOnChangingPaymentType();
			TestInvoice.ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cheque;

			AssertNoNotifications("Please enter a Check Book.", TestInvoice.ReceiptPaymentAK_ABInfo);
			AssertHasNotifications("Please enter a value.", TestInvoice.ReceiptPaymentAH_ChequeOrReferenceInfo);
		}

		public void TestChangeReceiptTypeResetsDrawerDetails()
		{
			TestObjectCreator.AALSHI.MiscServ.OM_ARPreviousChequeDrawer = "Cheque Drawer";
			TestObjectCreator.AALSHI.MiscServ.OM_ARPreviousChequeDrawerBank = "Drawer Bank";
			TestObjectCreator.AALSHI.MiscServ.OM_ARPreviousChequeDrawerBankBranch = "Drawer Branch";

			Factory.Save();

			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;

			invoice.ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cheque;

			Assert("Cheque Drawer is not read only", !invoice.ReceiptPaymentAH_ChequeDrawerInfo.ReadOnly);
			AssertEquals("Check Drawer is not empty", "Cheque Drawer", invoice.ReceiptPaymentAH_ChequeDrawer);

			Assert("Drawer Bank is not read only", !invoice.ReceiptPaymentAH_DrawerBankInfo.ReadOnly);
			AssertEquals("Drawer Bank is not empty", "Drawer Bank", invoice.ReceiptPaymentAH_DrawerBank);

			Assert("Drawer Branch is not read only", !invoice.ReceiptPaymentAH_DrawerBranchInfo.ReadOnly);
			AssertEquals("Drawer Branch is not empty", "Drawer Branch", invoice.ReceiptPaymentAH_DrawerBranch);

			invoice.ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cash;

			Assert("Cheque Drawer is read only", invoice.ReceiptPaymentAH_ChequeDrawerInfo.ReadOnly);
			AssertEquals("Check Drawer is empty", ZString.Empty, invoice.ReceiptPaymentAH_ChequeDrawer);

			Assert("Drawer Bank is read only", invoice.ReceiptPaymentAH_DrawerBankInfo.ReadOnly);
			AssertEquals("Drawer Bank is empty", ZString.Empty, invoice.ReceiptPaymentAH_DrawerBank);

			Assert("Drawer Branch is read only", invoice.ReceiptPaymentAH_DrawerBranchInfo.ReadOnly);
			AssertEquals("Drawer Branch is empty", ZString.Empty, invoice.ReceiptPaymentAH_DrawerBranch);
		}

		#region Reversing

		public void TestReversingShipmentInvoiceSetsJobInvoiceNumber()
		{
			Job testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.JH_JobNum = "S00001000";

			TestInvoice.AH_JH = testJob.PK;
			TestInvoice.AH_ConsolidatedInvoiceRef = InvoiceLiteralNumberGenerator.GetNextAndUpdateUniqueJobARInvoiceNumber(TestInvoice, testJob);
			Factory.Save();
			AssertEquals("Job Invoice number should be first in sequence", "S00001000", TestInvoice.AH_ConsolidatedInvoiceRef);

			IReversing beingReversed = TestInvoice;
			beingReversed.GenerateReverseTransaction(true);
			CreditNote reversingCrd = beingReversed.ReverseTransaction as CreditNote;
			AssertNotNull("Reversing transaction should be a CreditNote", reversingCrd);
			AssertEquals("Job Invoice number on reversing creditnote should be second in sequence", "S00001000/A", reversingCrd.AH_ConsolidatedInvoiceRef);
		}

		public void TestReversingShipmentInvoiceWithCustomisedShipmentNo()
		{
			Job testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.JH_JobNum = "CTEST001";

			TestInvoice.AH_JH = testJob.PK;
			TestInvoice.AH_ConsolidatedInvoiceRef = InvoiceLiteralNumberGenerator.GetNextAndUpdateUniqueJobARInvoiceNumber(TestInvoice, testJob);
			Factory.Save();

			AssertEquals("Job Invoice number should be first in sequence", "CTEST001", TestInvoice.AH_ConsolidatedInvoiceRef);

			IReversing beingReversed = TestInvoice;
			beingReversed.GenerateReverseTransaction(true);
			CreditNote reversingCrd = beingReversed.ReverseTransaction as CreditNote;
			AssertNotNull("Reversing transaction should be a CreditNote", reversingCrd);
			AssertEquals("Job Invoice number on reversing creditnote should be second in sequence", "CTEST001/A", reversingCrd.AH_ConsolidatedInvoiceRef);
		}

		public void TestReversingConsolInvoiceSetsJobInvoiceNumber()
		{
			ZString consolNumber = "C00001003";
			TestInvoice.AH_ConsolidatedInvoiceRef = InvoiceLiteralNumberGenerator.GetNextConsolARInvoiceNumber(Factory, consolNumber, TestInvoice.PK);
			Factory.Save();

			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			aRInv.AH_ConsolidatedInvoiceRef = InvoiceLiteralNumberGenerator.GetNextConsolARInvoiceNumber(Factory, consolNumber, aRInv.PK);
			Factory.Save();
			AssertEquals("Job Invoice number should be C00001003/A", "C00001003/A", aRInv.AH_ConsolidatedInvoiceRef);

			IReversing beingReversed = aRInv;
			beingReversed.GenerateReverseTransaction(true);
			ARCreditNote reversingCrd = beingReversed.ReverseTransaction as ARCreditNote;
			AssertEquals("Job Invoice number on reversing creditnote should be C00001003/B", "C00001003/B", reversingCrd.AH_ConsolidatedInvoiceRef);
		}

		public void TestReversingConsolInvoiceSetsJobInvoiceNumber_WithLongConsolNumber()
		{
			ZString consolNumber = string.Format("C{0}1", "".PadRight(JobConsolSchema.JK_UniqueConsignRef.MaxLength - 2, '0'));

			var unrelatedInvoiceWithSimilarJobInvNum = Factory.NewWithValidTestData<ARInvoice>();
			unrelatedInvoiceWithSimilarJobInvNum.AH_ConsolidatedInvoiceRef = consolNumber.Substring(0, 9);
			Factory.Save();

			TestInvoice.AH_ConsolidatedInvoiceRef = InvoiceLiteralNumberGenerator.GetNextConsolARInvoiceNumber(Factory, consolNumber, TestInvoice.PK);
			Factory.Save();

			var aRInv = Factory.NewWithValidTestData<ARInvoice>();
			aRInv.AH_ConsolidatedInvoiceRef = InvoiceLiteralNumberGenerator.GetNextConsolARInvoiceNumber(Factory, consolNumber, aRInv.PK);
			Factory.Save();
			AssertEquals("AR Invoice Job Invoice Number", consolNumber + "/A", aRInv.AH_ConsolidatedInvoiceRef);

			IReversing beingReversed = aRInv;
			beingReversed.GenerateReverseTransaction(true);
			ARCreditNote reversingCrd = beingReversed.ReverseTransaction as ARCreditNote;
			AssertEquals("Job Invoice Number on reversing Credit Note", consolNumber + "/B", reversingCrd.AH_ConsolidatedInvoiceRef);
		}

		#endregion

		#region Bad Debt Writting Off

		[SuspendGLAccountAndChargeCodeCriticalValidation]
		public void TestGenerateReverseTransaction_BadDebt()
		{
			Job testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.JH_JobNum = "S00001000";

			TestInvoice.AH_JH = testJob.PK;
			TestInvoice.AH_ConsolidatedInvoiceRef = InvoiceLiteralNumberGenerator.GetNextAndUpdateUniqueJobARInvoiceNumber(TestInvoice, testJob);

			ZGuid expectedGenericCharge1 = ZGuid.NewZGuid();
			ZString expectedDesc1 = "Description 1";
			ZString expectedSupply1 = "LOC";
			ZGuid expectedGenericCharge2 = ZGuid.NewZGuid();
			ZString expectedDesc2 = "Description 2";
			ZString expectedSupply2 = "LOX";
			ZGuid expectedTax = TestObjectCreator.GST1.PK;
			ZGuid badDebtGenericCharge = (Guid)AccountingConfigurationRegistry.Instance.BadDebtWriteOffAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);

			TestInvoice.Lines.RemoveAndDeleteAll();
			InvoicingLineBase line1 = (InvoicingLineBase)TestInvoice.Lines.AddNew();
			line1.GenericCharge = expectedGenericCharge1;
			line1.AL_Desc = expectedDesc1;
			line1.AL_AT = expectedTax;
			line1.AL_SupplyType = expectedSupply1;

			InvoicingLineBase line2 = (InvoicingLineBase)TestInvoice.Lines.AddNew();
			line2.GenericCharge = expectedGenericCharge2;
			line2.AL_Desc = expectedDesc2;
			line2.AL_SupplyType = expectedSupply2;

			Factory.Save();
			AssertEquals("Job Invoice number should be first in sequence.", "S00001000", TestInvoice.AH_ConsolidatedInvoiceRef);

			IReversing beingReversed = TestInvoice;
			beingReversed.GenerateReverseTransaction(true);
			ARCreditNote reversingCrd = beingReversed.ReverseTransaction as ARCreditNote;
			AssertNotNull("Reversing transaction should be a CreditNote", reversingCrd);
			AssertEquals("Job Invoice number on reversing creditnote should be second in sequence.", "S00001000/A", reversingCrd.AH_ConsolidatedInvoiceRef);

			AssertEquals("Reversing Invoice line 1 should have GenericCharge the same as in source.", expectedGenericCharge1, reversingCrd.Lines[0].GenericCharge);
			AssertEquals("Reversing Invoice line 2 should have GenericCharge the same as in source.", expectedGenericCharge2, reversingCrd.Lines[1].GenericCharge);
			AssertEquals("Reversing Invoice line 1 should have Tax ID the same as in source.", expectedTax, reversingCrd.Lines[0].AL_AT);
			AssertEquals("Reversing Invoice line 2 should have TAX ID the same as in source.", ZGuid.Empty, reversingCrd.Lines[1].AL_AT);

			IBadDebtWritingOff badDebt = TestInvoice as IBadDebtWritingOff;
			AssertNotNull(badDebt);
			badDebt.IsWritingOff = true;

			TestInvoice.GenerateReverseTransaction(true);
			reversingCrd = beingReversed.ReverseTransaction as ARCreditNote;
			AssertNotNull("Reversing transaction should be a CreditNote.", reversingCrd);

			Factory.Save();

			AssertEquals("Reversing Invoice should have empty AH_JH.", ZGuid.Empty, reversingCrd.AH_JH);
			AssertEquals("Job Invoice number on reversing creditnote should be empty.", "", reversingCrd.AH_ConsolidatedInvoiceRef);

			AssertEquals("Reversing Invoice line 1 should have Bad Debt GenericCharge.", badDebtGenericCharge, reversingCrd.Lines[0].GenericCharge);
			AssertNotEquals("Reversing Invoice line 1 should not have old description.", expectedDesc1, reversingCrd.Lines[0].AL_Desc);
			AssertEquals("Reversing Invoice line 1 should have empty AL_JH.", ZGuid.Empty, reversingCrd.Lines[0].AL_JH);
			AssertEquals("Reversing Invoice line 1 should have same supply as origin invoice line 1.", expectedSupply1, reversingCrd.Lines[0].AL_SupplyType);

			AssertEquals("Reversing Invoice line 2 should have Bad Debt GenericCharge.", badDebtGenericCharge, reversingCrd.Lines[1].GenericCharge);
			AssertNotEquals("Reversing Invoice line 2 should not have old description.", expectedDesc2, reversingCrd.Lines[1].AL_Desc);
			AssertEquals("Reversing Invoice line 2 should have empty AL_JH.", ZGuid.Empty, reversingCrd.Lines[1].AL_JH);
			AssertEquals("Reversing Invoice line 2 should have same supply as origin invoice line 2.", expectedSupply2, reversingCrd.Lines[1].AL_SupplyType);

			AssertEquals("Reversing Invoice line 1 should have Tax ID the same as in source.", expectedTax, reversingCrd.Lines[0].AL_AT);
			AssertEquals("Reversing Invoice line 2 should have TAX ID the same as in source.", ZGuid.Empty, reversingCrd.Lines[1].AL_AT);

			AssertEquals("Reversing Invoice should have IsWrittingOff the same as in source.", true, reversingCrd.IsWritingOff);
		}

		public void TestWritingOffCommentLines()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			ARInvoice invoice1 = creator.CreateARInvoice<ARInvoice>("110212", creator.AUD, 1, creator.ABIGAS);
			InvoicingLineBase line1 = creator.CreateInvoiceLine(TransactionLineTypes.Revenue, invoice1, creator.Job1, creator.CommentChargeCode, creator.AUD, 1.0m, "Comment", 0);
			InvoicingLineBase line2 = creator.CreateInvoiceLine(TransactionLineTypes.Revenue, invoice1, creator.Job2, creator.CC4, creator.AUD, 1.0m, "HIHIHI", 10000);
			creator.CreateJobCharge(line1, creator.Job1, creator.CommentChargeCode, creator.AUD);
			creator.CreateJobCharge(line2, creator.Job2, creator.CC4, creator.AUD);

			Factory.Save();

			IBadDebtWritingOff badDebt = invoice1;
			badDebt.IsWritingOff = true;
			badDebt.GenerateReverseTransaction(true);
			var reversingCrd = badDebt.ReverseTransaction as ARCreditNote;

			var invoiceLines = reversingCrd.Lines.ToArray<InvoicingLineBase>();
			AssertEquals("Should be 2 lines in the Write Off transaction", 2, invoiceLines.Length);

			var commentLine = invoiceLines.FirstOrDefault(x => x.ChargeCode.AC_ChargeType == Constants.ChargeType.Comment);
			AssertNotNull(commentLine);
			AssertEquals("Comment Line Should Have Same Generic Charge", line1.GenericCharge, commentLine.GenericCharge);
			AssertEquals("Comment Line Should Have 'Comment' Description", line1.AL_Desc, commentLine.AL_Desc);

			var writeOffAccount = (Guid)AccountingConfigurationRegistry.Instance.BadDebtWriteOffAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			var revLine = invoiceLines.FirstOrDefault(x => x.GenericCharge != writeOffAccount);
			AssertNotNull(revLine);
		}

		public void TestSetDescriptionWrittingOff()
		{
			ZString expectedDesc1 = "Description 1";
			ZString expectedDesc2 = "Description 2";
			ZString descriptionToSet = "Another Description";

			TestInvoice.Lines.RemoveAndDeleteAll();
			InvoicingLineBase line1 = (InvoicingLineBase)TestInvoice.Lines.AddNew();
			line1.AL_Desc = expectedDesc1;
			line1.AL_AG = TestObjectCreator.GLHeader1.PK;

			InvoicingLineBase line2 = (InvoicingLineBase)TestInvoice.Lines.AddNew();
			line2.AL_Desc = expectedDesc2;
			line2.AL_AG = TestObjectCreator.GLHeader1.PK;

			Factory.Save();

			IReversing beingReversed = TestInvoice;
			beingReversed.GenerateReverseTransaction(true);
			ARCreditNote reversingCrd = beingReversed.ReverseTransaction as ARCreditNote;
			AssertNotNull("Reversing transaction should be a CreditNote.", reversingCrd);

			((IReversing)reversingCrd).SetDescription(descriptionToSet);
			AssertEquals("Reversing Invoice should have new Description", descriptionToSet, reversingCrd.AH_Desc);
			AssertEquals("Reversing Invoice line 1 should have Description the same as in source.", expectedDesc1, reversingCrd.Lines[0].AL_Desc);
			AssertEquals("Reversing Invoice line 2 should have Description the same as in source.", expectedDesc2, reversingCrd.Lines[1].AL_Desc);

			IBadDebtWritingOff badDebt = TestInvoice as IBadDebtWritingOff;
			AssertNotNull(badDebt);
			badDebt.IsWritingOff = true;

			beingReversed.GenerateReverseTransaction(true);
			reversingCrd = beingReversed.ReverseTransaction as ARCreditNote;
			AssertNotNull("Reversing transaction should be a CreditNote.", reversingCrd);

			((IReversing)reversingCrd).SetDescription(descriptionToSet);

			AssertEquals("Reversing Invoice should have new Description.", descriptionToSet, reversingCrd.AH_Desc);
			AssertEquals("Reversing Invoice line 1 should have new Description.", descriptionToSet, reversingCrd.Lines[0].AL_Desc);
			AssertEquals("Reversing Invoice line 2 should have new Description.", descriptionToSet, reversingCrd.Lines[1].AL_Desc);
		}

		#endregion

		public void TestJobRelatedLogicOnCopiedLine()
		{
			ARInvoiceLine aRLineOld = (ARInvoiceLine)GetTestInvoiceLine();
			ARInvoiceLine aRLineNew = Factory.New<ARInvoiceLine>();
			Assert("Precondition: AL_JH on new AR Line should not be read only", !aRLineNew.AL_JHInfo.ReadOnly);
			((ARInvoice)TestInvoice).JobRelatedLogicOnCopiedLine_ForTestOnly(aRLineOld, aRLineNew);
			Assert("Precondition: AL_JH on new AR Line should be read only", aRLineNew.AL_JHInfo.ReadOnly);
		}

		public override void TestInvoiceBatchNumber()
		{
			Header.AH_ReceiptBatchNo = "300";
			AssertEquals("Invoice Batch Number should be 300", "300", Header.InvoiceBatchNumber);
		}

		public override void TestDocManagerInfo()
		{
			ARInvoice invoice = (ARInvoice)GetNewBusinessObject();
			AssertEquals("DocManagerInfo.GetType()", GetExpectedDocManagerInfoType(), invoice.DocManagerInfo.GetType());
		}

		protected virtual Type GetExpectedDocManagerInfoType()
		{
			return typeof(ARInvoiceDocManagerInfo);
		}

		protected override ZString ExpectedTransactionTypeForIncomplete
		{
			get { throw new NotSupportedException(); }
		}

		[ExpectNoExceptions]
		public void TestAuthorisationLevelForTransactionWithErrors()
		{
			Header.FillWithValidTestData();
			Header.RunPreSaveValidation();
			Assert("Precondition: ", Header.HasErrors);
			Factory.Save();
			Assert("Precondition: ", Header.IsInDatabase);

			UnapprovedTransactionCandidateCollection unapprovedTransactionCollection = new UnapprovedTransactionCandidateCollection(Factory);
			unapprovedTransactionCollection.Add(Header);

			UnapprovedTransactionConverter converter = new UnapprovedTransactionConverter(new BusinessObjectFactory());
			InvoicingBase invoice = converter.ConvertToAP((InvoicingBase)Header, true);
			AssertNotNull("Precondition:", invoice);

			string level = Header.AuthorisationLevel;
		}

		public void TestAuthorisationLevelNotGenerateInvoicePdf()
		{
			AccountingConfigurationRegistry.Instance.PayableAllowUserToStoreARDoc.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var invoice = Header as ARInvoice;
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			AssertNotNull(invoice);

			invoice.AH_GB = TestObjectCreator.NonCurrentCompanyBranch.PK;
			invoice.AH_GC = TestObjectCreator.NonCurrentCompanyBranch.Company.PK;

			foreach (InvoiceLine line in invoice.Lines)
			{
				line.AL_GB = TestObjectCreator.NonCurrentCompanyBranch.PK;
				line.AL_GC = TestObjectCreator.NonCurrentCompanyBranch.Company.PK;
			}

			Factory.Save();

			var unapprovedTransactionCollection = new UnapprovedTransactionCandidateCollection(Factory);
			unapprovedTransactionCollection.Add(invoice);

			string level = invoice.AuthorisationLevel;

			AssertNotNull("Should convert to an AP Invoice", invoice.ConvertedAPInvoiceForTest);
			AssertEquals("Shouldn't generate any document.", 0, invoice.ConvertedAPInvoiceForTest.DocManagerInfo.AllEDocs.Count);
		}

		public void TestStampDutyCalculation()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);

			GlbCompany.CurrentCompany.SetCountry("IT");

			AccTaxRate taxRate;
			AccTaxRate taxRate2;
			AccInvMsg taxInvMsg1;
			AccInvMsg taxInvMsg2;
			AccChargeCode chargeCode;
			SetupStampDutyCalculationTaxIDsAndRegistry(creator, out taxRate, out taxRate2, out chargeCode, out taxInvMsg1, out taxInvMsg2);

			AccGLHeader glHeader = creator.GLHeader1;

			AssertStampDutyCalculation(30m, 50m, true, true, taxRate, taxRate2, chargeCode, glHeader, taxInvMsg2);
			AssertStampDutyCalculation(30m, -50m, true, true, taxRate, taxRate2, chargeCode, glHeader, taxInvMsg2);
			AssertStampDutyCalculation(30m, 40m, false, false, taxRate, taxRate2, chargeCode, glHeader, null);
		}

		public void TestStampDutyCalculationEventOnly()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);

			GlbCompany.CurrentCompany.SetCountry("IT");

			AccTaxRate taxRate;
			AccTaxRate taxRate2;
			AccInvMsg taxInvMsg1;
			AccInvMsg taxInvMsg2;
			AccChargeCode chargeCode;
			SetupStampDutyCalculationTaxIDsAndRegistry(creator, out taxRate, out taxRate2, out chargeCode, out taxInvMsg1, out taxInvMsg2);

			AccGLHeader glHeader = creator.GLHeader1;

			AssertStampDutyCalculation(30m, 50m, true, false, taxRate, taxRate2, chargeCode, glHeader, taxInvMsg2);
			AssertStampDutyCalculation(30m, -50m, true, false, taxRate, taxRate2, chargeCode, glHeader, taxInvMsg2);
			AssertStampDutyCalculation(30m, 40m, false, false, taxRate, taxRate2, chargeCode, glHeader, null);
		}

		public void TestStampDutyCalculationValidation()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);

			GlbCompany.CurrentCompany.SetCountry("IT");

			AccTaxRate taxRate;
			AccTaxRate taxRate2;
			AccInvMsg taxInvMsg1;
			AccInvMsg taxInvMsg2;
			AccChargeCode chargeCode;
			SetupStampDutyCalculationTaxIDsAndRegistry(creator, out taxRate, out taxRate2, out chargeCode, out taxInvMsg1, out taxInvMsg2);
			AccountingConfigurationRegistry.Instance.StampDutyChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);

			AccGLHeader glHeader = creator.GLHeader1;

			AssertStampDutyCalculationValidation(30m, 50m, true, taxRate, taxRate2, chargeCode, glHeader);
			AssertStampDutyCalculationValidation(30m, 40m, false, taxRate, taxRate2, chargeCode, glHeader);
		}

		void AssertStampDutyCalculationValidation(decimal line1LocalExTaxAmount, decimal line2LocalExTaxAmount, bool shouldHaveValidationError, AccTaxRate taxRate, AccTaxRate taxRate2, AccChargeCode chargeCode, AccGLHeader glHeader)
		{
			ARInvoice invoice = Factory.New<ARInvoice>();
			OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			orgHeader.OH_RL_NKClosestPort = "ITROM";
			invoice.AH_OH = orgHeader.PK;
			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AG = glHeader.PK;
			line.AL_LocalExTaxAmount = line1LocalExTaxAmount;
			line.AL_AT = taxRate.PK;
			InvoicingLineBase line2 = (InvoicingLineBase)invoice.Lines.AddNew();
			line2.AL_AG = glHeader.PK;
			line2.AL_LocalExTaxAmount = line2LocalExTaxAmount;
			line2.AL_AT = taxRate2.PK;

			if (shouldHaveValidationError)
			{
				invoice.RunPreSaveValidation();
				AssertEquals("Invoice should have errors", true, invoice.HasErrors);
				AssertEquals("Invoice should have error", true, invoice.Notifications.ContainsNotificationContaining("The invoice being posted attracts stamp duty, but there is no 'Stamp Duty Charge Code' defined in the registry. Please define an appropriate charge code in the registry under Accounting > Receivable Defaults > Default Settings > Stamp Duty Charge Code."));
			}
			else
			{
				invoice.RunPreSaveValidation();
				AssertEquals("Invoice should not have error", false, invoice.Notifications.ContainsNotificationContaining("The invoice being posted attracts stamp duty, but there is no 'Stamp Duty Charge Code' defined in the registry. Please define an appropriate charge code in the registry under Accounting > Receivable Defaults > Default Settings > Stamp Duty Charge Code."));
			}
		}

		public void TestUserErrorPomptDoesNotOccurDuringSaving()
		{
			using (Env.CurrentUser.SetIsControllerOverrideForTesting(false))
			{
				Env.Security.ChangeStatusOfCompleteJobs.IsAllowed = false;

				Factory.Save();

				TestObjectCreator.SetupRegistrySetJobStatusToInvoicedWhenFirstARInvoicePosted(Env.CurrentCompany.PK, null);

				var shipment = TestObjectCreator.CreateShipment("100101");
				var job = TestObjectCreator.CreateJob(shipment, false);

				var errMsg = string.Empty;
				job.OnCannotChangeStatusUserMessage += (object sender, UserMessageEventArgs e) => { errMsg = e.Message; };

				job.JH_Status = JobHeaderStatus.Complete.Code;
				Factory.Save();

				var fromJobStatus = JobHeaderStatus.Complete.Code;
				var jobStatusUpdateRestrictionRuleCollection = AccountingConfigurationRegistry.Instance.JobStatusUpdateRestrictionRule.DefaultValue;
				var jobStatusUpdateRestrictionRule = jobStatusUpdateRestrictionRuleCollection.Cast<JobStatusUpdateRestrictionRule>().FirstOrDefault(x => x.JobStatus == fromJobStatus);
				jobStatusUpdateRestrictionRule.Working = "YES";

				using (AccountingConfigurationRegistry.Instance.JobStatusUpdateRestrictionRule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, jobStatusUpdateRestrictionRuleCollection))
				{
					job.JH_Status = JobHeaderStatus.Working.Code;
					job.Validation.ValidateJH_Status();
					AssertHasError("Changing Status of Complete Job if no security access should cause error.", job.JH_StatusInfo, Env.Security.ChangeStatusOfCompleteJobs.ErrorMessageForNotAllowed + JobValidation.CompleteJobSecurityErrorMessage);
				}

				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("1010101", TestObjectCreator.AUD, 1.0M, TestObjectCreator.ABIGAS);
				var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1.0m, 100m);
				job.JH_Status = JobHeaderStatus.Working.Code;
				line.AL_LineType = TransactionLineTypes.Revenue;
				line.AL_AC = TestObjectCreator.CC1.PK;
				line.AL_JH = job.PK;
				var testCharge = TestObjectCreator.CreateCharge(line);

				invoice.OnFactorySavingBeforeTransactionCore_ForTestOnly();
				AssertEquals("No Error Message", string.Empty, errMsg);

				errMsg = string.Empty;
				using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
				{
					invoice.OnSavingCore_ForTestOnly();
					AssertEquals("No Error Message", errMsg, string.Empty);
				}
			}
		}

		public override void TestIsAllowModifyAmendStatusCode()
		{
			var amendingAR = CreateAmendingAR();
			AssertEquals("PreCondition", true, amendingAR.IsAmendingTransaction);

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals(false, InvoicingBase.IsAllowModifyAmendStatusCode);
			AssertEquals(false, amendingAR.IsAllowModifyAmendStatusCode);

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals(false, InvoicingBase.IsAllowModifyAmendStatusCode);
			AssertEquals(false, amendingAR.IsAllowModifyAmendStatusCode);

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals(false, InvoicingBase.IsAllowModifyAmendStatusCode);
			AssertEquals(false, amendingAR.IsAllowModifyAmendStatusCode);

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals(false, InvoicingBase.IsAllowModifyAmendStatusCode);
			AssertEquals(true, amendingAR.IsAllowModifyAmendStatusCode);
		}

		public override void TestAH_Calc_AmendStatusCode()
		{
			var auBranch = TestObjectCreator.CreateBranchWithCompany("AU");
			Factory.Save();
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, auBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertNormalAR(string.Empty, string.Empty);
				AssertAmendingAR(string.Empty, string.Empty);
			}

			var krBranch = TestObjectCreator.CreateBranchWithCompany("KR");
			Factory.Save();
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, krBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertNormalAR("01", "02");
				AssertAmendingAR("01", "02");
			}

			void AssertNormalAR(string expectedAmendStatusCodeFromDB, string expectedAmendStatusCodeFromBizO)
			{
				var sampleInvoice = PrepareTransactionHeaderForTest() as InvoicingBase;
				AssertEquals("PreCondition", false, sampleInvoice.IsAmendingTransaction);

				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				AssertAH_Calc_AmendStatusCode(() => PrepareTransactionHeaderForTest() as InvoicingBase, expectedAmendStatusCodeFromDB, expectedAmendStatusCodeFromBizO);

				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				AssertAH_Calc_AmendStatusCode(() => PrepareTransactionHeaderForTest() as InvoicingBase, expectedAmendStatusCodeFromDB, expectedAmendStatusCodeFromBizO);
			}

			void AssertAmendingAR(string expectedAmendStatusCodeFromDB, string expectedAmendStatusCodeFromBizO)
			{
				var sampleInvoice = CreateAmendingAR() as InvoicingBase;
				AssertEquals("PreCondition", true, sampleInvoice.IsAmendingTransaction);

				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				AssertAH_Calc_AmendStatusCode(CreateAmendingAR, expectedAmendStatusCodeFromDB, expectedAmendStatusCodeFromBizO);

				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				AssertAH_Calc_AmendStatusCode(CreateAmendingAR, expectedAmendStatusCodeFromDB, expectedAmendStatusCodeFromBizO);
			}
		}

		public override void TestAH_Calc_AmendStatusCode_ReadOnly()
		{
			AssertNormalAR();
			AssertAmendingAR();

			void AssertNormalAR()
			{
				AssertEquals("PreCondition", false, InvoicingBase.IsAmendingTransaction);

				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				AssertEquals(true, InvoicingBase.AH_Calc_AmendStatusCodeInfo.ReadOnly);

				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				AssertEquals(true, InvoicingBase.AH_Calc_AmendStatusCodeInfo.ReadOnly);

				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				AssertEquals(true, InvoicingBase.AH_Calc_AmendStatusCodeInfo.ReadOnly);

				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				AssertEquals(true, InvoicingBase.AH_Calc_AmendStatusCodeInfo.ReadOnly);
			}

			void AssertAmendingAR()
			{
				var amendingAR = CreateAmendingAR();

				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				AssertEquals(true, amendingAR.AH_Calc_AmendStatusCodeInfo.ReadOnly);

				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				AssertEquals(true, amendingAR.AH_Calc_AmendStatusCodeInfo.ReadOnly);

				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				AssertEquals(true, amendingAR.AH_Calc_AmendStatusCodeInfo.ReadOnly);

				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				AssertEquals(false, amendingAR.AH_Calc_AmendStatusCodeInfo.ReadOnly);

				Factory.Save();
				AssertEquals("it will be read only after being saved", true, amendingAR.AH_Calc_AmendStatusCodeInfo.ReadOnly);
			}
		}

		public void TestExchangeRateIsEditable()
		{
			AccountingConfigurationRegistry.Instance.UseJobExchangeRateDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			Assert("Exchange Rate should be read only when currency is default current company currency", invoice.ExchangeRate.IsRateReadOnly);
			invoice.AH_RX_NKTransactionCurrency = "TND";
			Assert("Exchange Rate should not be read only when currency is different to the default login company currency", !invoice.ExchangeRate.IsRateReadOnly);
		}

		ARInvoice CreateAmendingAR()
		{
			var arInvoice = PrepareTransactionHeaderForTest() as ARInvoice;
			IAmending original = arInvoice;
			var amendingAR = original.GenerateAmendingTransaction(arInvoice.AH_TransactionType) as ARInvoice;
			AssertEquals("PreCondition", true, amendingAR.IsAmendingTransaction);

			return amendingAR;
		}

		public void TestReasonFields_ReadOnly()
		{
			using (InvoiceBaseValidationTest.InitaliseComplianceFactory())
			{
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV1", TestObjectCreator.EUR, 1m, TestObjectCreator.AALSHI);
				var creditNote = TestObjectCreator.CreateARCreditNote("ARCRD1", TestObjectCreator.AALSHI, TestObjectCreator.EUR, 1m);
				Assert("reason fields read only when Original transaction is empty", invoice.ReasonCodeInfo.ReadOnly);
				Assert("reason fields read only when Original transaction is empty", invoice.ReasonDescriptionInfo.ReadOnly);

				invoice.OriginalTransactionReference = creditNote.PK;
				Assert("reason code is available when Original transaction is filled", !invoice.ReasonCodeInfo.ReadOnly);
				Assert("reason code is empty", invoice.ReasonCode.IsEmpty);
				Assert("reason description is not available when reason code is empty", invoice.ReasonDescriptionInfo.ReadOnly);

				invoice.ReasonCode = "IDE";
				Assert(!invoice.ReasonCodeInfo.ReadOnly);
				Assert("reason description is not available when reason code is not TXT", invoice.ReasonDescriptionInfo.ReadOnly);

				invoice.ReasonCode = "TXT";
				Assert(!invoice.ReasonCodeInfo.ReadOnly);
				Assert("reason description is available only when reason code is TXT", !invoice.ReasonDescriptionInfo.ReadOnly);
			}
		}

		public void TestReasonFields_WhenReasonCodeIsDeletedFromRegistry()
		{
			using (InvoiceBaseValidationTest.InitaliseComplianceFactory())
			{
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.EUR, 1m, TestObjectCreator.AALSHI);
				var creditNote = TestObjectCreator.CreateARCreditNote("ARCRD001", TestObjectCreator.AALSHI, TestObjectCreator.EUR, 1m);
				invoice.OriginalTransactionReference = creditNote.PK;
				invoice.ReasonCode = "TXT";
				Factory.Save();

				Assert(invoice.IsInDatabase);
				AssertEquals("TXT", invoice.ReasonCode);
				AssertEquals("Free Text", invoice.ReasonDescription);
				AssertContainsExactElementsInAnyOrder("default reason codes", new[] { "IDE", "IAM", "TXT" }, invoice.ReasonCodes.GetAllCodes());

				var reasonCodes = new CodeDescriptionPairList();
				reasonCodes.Add(new CodeDescriptionPair("AAA", "test aaa"));
				reasonCodes.Add(new CodeDescriptionPair("BBB", "test bbb"));
				AccountingMasterFilesRegistry.Instance.AmendmentReasonCodesList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, reasonCodes);

				var invoiceReloaded = new BusinessObjectFactory().Load<ARInvoice>(invoice.PK);
				AssertContainsExactElementsInAnyOrder("PreCondition: default reasonCodes doesn't have TXT anymore", new[] { "AAA", "BBB" }, invoiceReloaded.ReasonCodes.GetAllCodes());

				AssertEquals("TXT", invoiceReloaded.ReasonCode);
				AssertEquals("Free Text", invoiceReloaded.ReasonDescription);
			}
		}

		public void TestReasonFields_DescriptionIsFilledAutomatically()
		{
			var invoice1 = CreateInvoiceWithOriginalTransactionReference("001");
			invoice1.ReasonCode = "TXT";
			AssertEquals("reason description is automatically filled", "Free Text", invoice1.ReasonDescription);

			invoice1.ReasonCode = "IDE";
			AssertEquals("reason description is automatically filled", "Incorrect Data Entry", invoice1.ReasonDescription);

			invoice1.ReasonCode = string.Empty;
			AssertEquals("reason description is reset when code is reset", string.Empty, invoice1.ReasonDescription);

			invoice1.ReasonCode = "TXT";
			AssertEquals("Free Text", invoice1.ReasonDescription);
			invoice1.ReasonDescription = "FREE TEXT 2";
			AssertEquals("updating reason description does not change the reason code", "TXT", invoice1.ReasonCode);
			AssertEquals("FREE TEXT 2", invoice1.ReasonDescription);
		}

		public void TestReasonFields_CodeIsRetrievedFromSavedDescription()
		{
			AssertTest("001", "IDE", "Incorrect Data Entry", true);
			AssertTest("002", "IAM", "Incorrect Amounts", true);
			AssertTest("003", "TXT", "Random description", true);
			AssertTest("004", "IDE", "Incorrect Data Entry", false);

			void AssertTest(string transactionNumber, string code, string description, bool areReasonFieldEnabled)
			{
				using (InvoiceBaseValidationTest.InitaliseComplianceFactory(shouldShowOriginalInvoiceReferenceReasonFields: areReasonFieldEnabled))
				{
					var invoice = CreateInvoiceWithOriginalTransactionReference(transactionNumber);
					invoice.ReasonCode = code;
					invoice.ReasonDescription = description;
					Factory.Save();

					var reloadedinvoice = new BusinessObjectFactory().Load<ARInvoice>(invoice.PK);
					if (areReasonFieldEnabled)
					{
						AssertEquals(code, reloadedinvoice.ReasonCode);
						AssertEquals(description, reloadedinvoice.ReasonDescription);
					}
					else
					{
						AssertEquals(string.Empty, reloadedinvoice.ReasonCode);
						AssertEquals(string.Empty, reloadedinvoice.ReasonDescription);
					}
				}
			}
		}

		public void TestReasonFields_ResetWhenOriginalTransactionReferenceIsChanged()
		{
			using (InvoiceBaseValidationTest.InitaliseComplianceFactory())
			{
				var invoice1 = CreateInvoiceWithOriginalTransactionReference("001");
				invoice1.ReasonCode = "IAM";

				AssertEquals("IAM", invoice1.ReasonCode);
				AssertEquals("Incorrect Amounts", invoice1.ReasonDescription);

				var creditNote2 = TestObjectCreator.CreateARCreditNote("ARCRD001", TestObjectCreator.AALSHI, TestObjectCreator.EUR, 1m);
				invoice1.OriginalTransactionReference = creditNote2.PK;
				AssertEquals("reason fields reset when OriginalTransactionReference changed to another invoice", string.Empty, invoice1.ReasonCode);
				AssertEquals("reason fields reset when OriginalTransactionReference changed to another invoice", string.Empty, invoice1.ReasonDescription);

				invoice1.ReasonCode = "IAM";
				AssertEquals("IAM", invoice1.ReasonCode);
				AssertEquals("Incorrect Amounts", invoice1.ReasonDescription);

				invoice1.OriginalTransactionReference = ZGuid.Empty;
				AssertEquals("reason fields reset when OriginalTransactionReference changed to empty", string.Empty, invoice1.ReasonCode);
				AssertEquals("reason fields reset when OriginalTransactionReference changed to empty", string.Empty, invoice1.ReasonDescription);
			}
		}

		public void TestReasonFields_Saving()
		{
			using (InvoiceBaseValidationTest.InitaliseComplianceFactory())
			{
				var invoice1 = CreateInvoiceWithOriginalTransactionReference("001");
				invoice1.ReasonCode = "TXT";
				invoice1.ReasonDescription = string.Empty;
				Factory.Save();

				AssertReasonCodeAndDescription("reason fields are not saved if description is missing", invoice1.PK, string.Empty, string.Empty);

				var invoice2 = CreateInvoiceWithOriginalTransactionReference("002");
				invoice2.ReasonCode = string.Empty;
				invoice2.ReasonDescription = "reason desc";
				Factory.Save();

				AssertReasonCodeAndDescription("reason fields are not saved if code is missing", invoice2.PK, string.Empty, string.Empty);
				AssertEquals("no GenAddOnColumn data created", 0, Factory.Load<GenAddOnColumn>(new ZQuery(GenAddOnColumnSchema.XA_Name, "CreditNoteReason")).Length);

				Assert(invoice1.IsInDatabase);
				invoice1.ReasonCode = "TXT";
				invoice1.ReasonDescription = "new DESC";
				Factory.Save();

				AssertReasonCodeAndDescription("reason fields are not saved if credit note is already saved", invoice1.PK, string.Empty, string.Empty);
				AssertEquals("no GenAddOnColumn data created", 0, Factory.Load<GenAddOnColumn>(new ZQuery(GenAddOnColumnSchema.XA_Name, "CreditNoteReason")).Length);

				var invoice3 = CreateInvoiceWithOriginalTransactionReference("003");
				invoice3.ReasonCode = "TXT";
				invoice3.ReasonDescription = "new DESC";
				Factory.Save();
				AssertReasonCodeAndDescription("reason fields are saved", invoice3.PK, "TXT", "new DESC");

				var addOnColumns = Factory.Load<GenAddOnColumn>(new ZQuery(GenAddOnColumnSchema.XA_Name, InvoicingBase.GenAddOnColumnReasonName).AddToFilter(GenAddOnColumnSchema.XA_ParentID, invoice3.PK));
				AssertEquals(1, addOnColumns.Length);
				AssertEquals("XA_ParentTableCode is AH", AccTransactionHeaderSchema.Constants.Prefix, addOnColumns[0].XA_ParentTableCode);
				AssertEquals(AddOnColumnDataType.Codes.String, addOnColumns[0].XA_Type);
				AssertEquals("TXT|new DESC", addOnColumns[0].XA_Data);

				addOnColumns[0].XA_Data = "dataWithoutSeparator";
				Factory.Save();
				AssertReasonCodeAndDescription("wrong XA_Data return empty values", invoice3.PK, string.Empty, string.Empty);
			}

			void AssertReasonCodeAndDescription(string message, ZGuid invoicePK, string code, string description)
			{
				var reloadedInvoice = new BusinessObjectFactory().Load<ARInvoice>(invoicePK);
				AssertEquals(message, code, reloadedInvoice.ReasonCode);
				AssertEquals(message, description, reloadedInvoice.ReasonDescription);
			}
		}

		public void TestReasonCodeSavingLocation()
		{
			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.EUR, 1m, TestObjectCreator.AALSHI);
			Assert("The reason code is saved in XA_Type, if the max length of XA_Type is lower than the max length of the reason code, then it won't work",
				GenAddOnColumnSchema.XA_Type.MaxLength >= invoice.ReasonCodeInfo.MaxLength);
		}

		public void TestIsReasonFieldsMandatory()
		{
			AssertIsReasonFieldsMandatory(false, false);
			AssertIsReasonFieldsMandatory(true, true);

			void AssertIsReasonFieldsMandatory(bool getIsARInvoiceReasonFieldsMandatoryValue, bool expectedInvoiceAreReasonFieldsMandatory)
			{
				using (InvoiceBaseValidationTest.InitaliseComplianceFactory(areOriginalTransactionReferenceFieldsMandatory: getIsARInvoiceReasonFieldsMandatoryValue))
				{
					var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV1", TestObjectCreator.EUR, 1m, TestObjectCreator.AALSHI);
					AssertNoErrors(invoice.ReasonCodeInfo);

					var creditNote = TestObjectCreator.CreateARCreditNote("ARCRD1", TestObjectCreator.AALSHI, TestObjectCreator.EUR, 1m);
					invoice.OriginalTransactionReference = creditNote.PK;

					if (expectedInvoiceAreReasonFieldsMandatory)
					{
						AssertHasError("reason fields are mandatory only for Portugal and when the Original Transaction Reference is set",
						invoice.ReasonCodeInfo, "Please enter a value.");
					}
					else
					{
						AssertNoErrors(invoice.ReasonCodeInfo);
					}
				}
			}
		}

		public void TestReasonFields_ReasonCodes()
		{
			AssertContainsExactElementsInAnyOrder("PreCondition: registry default values are IDE, IAM and TXT",
				new string[] { "IDE", "IAM", "TXT" }, AccountingMasterFilesRegistry.Instance.AmendmentReasonCodesList.Value.Cast<CodeDescriptionPair>().Select(x => x.Code));

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV1", TestObjectCreator.EUR, 1m, TestObjectCreator.AALSHI);
			AssertContainsExactElementsInAnyOrder(new string[] { "IDE", "IAM", "TXT" }, invoice.ReasonCodes.Cast<CodeDescriptionPair>().Select(x => x.Code));
		}

		public void TestPopulateFromOriginalTransaction_HeaderDetails_SetComplianceSubType()
		{
			var originalTxn = GetNewBusinessObject() as InvoicingBase;
			originalTxn.AH_ComplianceSubType = "TES";
			originalTxn.AH_TransactionReference = "TEST01";

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry("VN"))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.VietnamIssuePositiveAdjustmentViaAmendWithInvoice.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var amendment = Factory.NewWithValidTestData<ARInvoice>();

				amendment.PopulateFromOriginalTransaction(originalTxn, false);

				AssertEquals("ComplianceSubType should be TES", "TES", amendment.AH_ComplianceSubType);
			}

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry("CN"))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.VietnamIssuePositiveAdjustmentViaAmendWithInvoice.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var amendment = Factory.NewWithValidTestData<ARInvoice>();

				amendment.PopulateFromOriginalTransaction(originalTxn, false);

				AssertEquals("ComplianceSubType should be empty", string.Empty, amendment.AH_ComplianceSubType);
			}

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry("VN"))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (AccountingConfigurationRegistry.Instance.VietnamIssuePositiveAdjustmentViaAmendWithInvoice.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var amendment = Factory.NewWithValidTestData<ARInvoice>();

				amendment.PopulateFromOriginalTransaction(originalTxn, false);

				AssertEquals("ComplianceSubType should be empty", string.Empty, amendment.AH_ComplianceSubType);
			}

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry("VN"))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.VietnamIssuePositiveAdjustmentViaAmendWithInvoice.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var amendment = Factory.NewWithValidTestData<ARInvoice>();

				amendment.PopulateFromOriginalTransaction(originalTxn, false);

				AssertEquals("ComplianceSubType should be empty", string.Empty, amendment.AH_ComplianceSubType);
			}

			originalTxn.AH_TransactionReference = string.Empty;

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry("VN"))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.VietnamIssuePositiveAdjustmentViaAmendWithInvoice.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var amendment = Factory.NewWithValidTestData<ARInvoice>();

				amendment.PopulateFromOriginalTransaction(originalTxn, false);

				AssertEquals("ComplianceSubType should be empty", string.Empty, amendment.AH_ComplianceSubType);
			}
		}

		public void TestAH_ComplianceSubType_ReadOnly_Malaysia()
		{
			var amending = Factory.New<ARInvoice>();
			var iAmending = amending as IAmending;
			iAmending.FlagAsCreatedAmending();
			AssertEquals(false, amending.AH_ComplianceSubType_ReadOnly);

			amending.Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Malaysia;
			AssertEquals(true, amending.AH_ComplianceSubType_ReadOnly);
		}

		public void TestAH_ComplianceSubType_ReadOnly_Vietnam()
		{
			var amending = Factory.New<ARInvoice>();
			var iAmending = amending as IAmending;
			iAmending.FlagAsCreatedAmending();
			amending.Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.VietNam;
			var invoice = Factory.New<ARInvoice>();
			invoice.Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.VietNam;

			using (AccountingConfigurationRegistry.Instance.VietnamIssuePositiveAdjustmentViaAmendWithInvoice.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(true, amending.AH_ComplianceSubType_ReadOnly);
				AssertEquals(false, invoice.AH_ComplianceSubType_ReadOnly);
			}

			using (AccountingConfigurationRegistry.Instance.VietnamIssuePositiveAdjustmentViaAmendWithInvoice.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(false, amending.AH_ComplianceSubType_ReadOnly);
				AssertEquals(false, invoice.AH_ComplianceSubType_ReadOnly);
			}
		}

		#region CustomLogReferenceSuffix

		public void TestSaveLogCustomLogReferenceSuffix()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV001", TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 200M);
			Factory.Save();

			new ARInvoiceReversing(invoice as ARInvoice).Reverse();
			var reverseInvoice = invoice.ReverseInvoice;
			reverseInvoice.AH_TransactionNum = "CRD001";
			Factory.Save();

			var storageFile = invoice.DocManagerInfo.AddFileOrDocument(new ZBlob(new byte[] { 0, 1, 2, 3 }), "testFileName1", "ACV", description: "StorageFile type file");
			invoice.AH_Desc = "Des";
			Factory.Save();

			var logs = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, invoice.PK));
			AssertEquals(1, logs.Count(x => x.SL_Reference == "AR|INV|Reversed"));
			AssertEquals(2, logs.Count(x => x.SL_SE_NKEvent == Events.EditedARecordCode && x.SL_Reference.IsEmpty));

			storageFile = reverseInvoice.DocManagerInfo.AddFileOrDocument(new ZBlob(new byte[] { 0, 1, 2, 3 }), "testFileName1", "ACV", description: "StorageFile type file");
			reverseInvoice.AH_Desc = "Des";
			Factory.Save();

			logs = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, reverseInvoice.PK));
			AssertEquals(1, logs.Count(x => x.SL_Reference == "AR|CRD|Reversed"));
			AssertEquals(2, logs.Count(x => x.SL_SE_NKEvent == Events.EditedARecordCode && x.SL_Reference.IsEmpty));
		}

		#endregion

		public void TestKoreaSouthSubtypeAutoAssign()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.KoreaSouth))
			{
				var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV001", TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
				var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 200M);
				line.AL_AT = TestObjectCreator.FREEVAT.PK;

				Factory.Save();

				AssertEquals("Original AR Invoice with FREEVAT only should be assigned with 102.", KoreaSouthComplianceInfo.ComplianceSubTypeCodes.TZI, invoice.AH_ComplianceSubType);

				new ARInvoiceReversing(invoice as ARInvoice).Reverse();
				var reverseInvoice = invoice.ReverseInvoice;
				reverseInvoice.AH_TransactionNum = "CRD001";

				Factory.Save();

				AssertEquals("Reverse AR Invoice with FREEVAT only should be assigned with 202.", KoreaSouthComplianceInfo.ComplianceSubTypeCodes.IZI, reverseInvoice.AH_ComplianceSubType);

				invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV002", TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
				line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 200M);
				line.AL_AT = TestObjectCreator.EXEMPT.PK;

				Factory.Save();

				AssertEquals("Original AR Invoice with EXEMPT only should be assigned with 301.", KoreaSouthComplianceInfo.ComplianceSubTypeCodes.NTI, invoice.AH_ComplianceSubType);

				new ARInvoiceReversing(invoice as ARInvoice).Reverse();
				reverseInvoice = invoice.ReverseInvoice;
				reverseInvoice.AH_TransactionNum = "CRD002";

				Factory.Save();

				AssertEquals("Reverse AR Invoice with EXEMPT only should be assigned with 401.", KoreaSouthComplianceInfo.ComplianceSubTypeCodes.INI, reverseInvoice.AH_ComplianceSubType);

				invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV003", TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
				line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 200M);
				line.AL_AT = TestObjectCreator.EXEMPT.PK;
				line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 200M);
				line.AL_AT = TestObjectCreator.FREEVAT.PK;

				Factory.Save();

				AssertEquals("Original AR Invoice with EXEMPT and FREEVAT should be assigned with 101.", KoreaSouthComplianceInfo.ComplianceSubTypeCodes.TXI, invoice.AH_ComplianceSubType);

				new ARInvoiceReversing(invoice as ARInvoice).Reverse();
				reverseInvoice = invoice.ReverseInvoice;
				reverseInvoice.AH_TransactionNum = "CRD003";

				Factory.Save();

				AssertEquals("Reverse AR Invoice with EXEMPT and FREEVAT should be assigned with 201.", KoreaSouthComplianceInfo.ComplianceSubTypeCodes.ITI, reverseInvoice.AH_ComplianceSubType);
			}
		}

		ARInvoice CreateInvoiceWithOriginalTransactionReference(string transactionNum)
		{
			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV" + transactionNum, TestObjectCreator.EUR, 1m, TestObjectCreator.AALSHI);
			var creditNote = TestObjectCreator.CreateARCreditNote("ARCRD" + transactionNum, TestObjectCreator.AALSHI, TestObjectCreator.EUR, 1m);
			invoice.OriginalTransactionReference = creditNote.PK;
			return invoice;
		}

		AccTaxRate excludedTax;
		protected AccTaxRate ExcludedTax
		{
			get
			{
				if (excludedTax == null)
				{
					excludedTax = Factory.NewWithValidTestData<AccTaxRate>();
					excludedTax.AT_Code = "EXL";
					excludedTax.AT_Type = AccTaxRate.Types.ExcludedFromTheTaxBase;
					excludedTax.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				}
				return excludedTax;
			}
		}

		protected override bool ShouldSupportCalculatingTaxAtHeaderLevel
		{
			get { return true; }
		}

		protected override BooleanRegistryItem EnforcePostingAtFixedPlaceOfSupplyLevelForTransactionRuleRegistry => AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForReceivableTransactions;

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(AccPeriodManagement.Schema.TableName);
			PeriodManagementTestHelper = new AccountingPeriodTestHelper();
			PeriodManagementTestHelper.SetupPeriods();
		}

		sealed class BaseJobDeclarationForTesting : BaseJobDeclaration
		{
			public BaseJobDeclarationForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public ZString EntryDetailsInARInvoiceReturns { get; set; }

			public override ZString EntryDetailsInARInvoice => EntryDetailsInARInvoiceReturns;
		}
	}
}
