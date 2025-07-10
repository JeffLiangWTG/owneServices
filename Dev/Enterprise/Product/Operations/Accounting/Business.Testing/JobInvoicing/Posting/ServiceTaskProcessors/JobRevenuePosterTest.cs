using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	class JobRevenuePosterTest : JobPostingWorkflowProcessorSupportingARCreditNoteLevelAuthorizationTest
	{
		public void TestErrorsAreReported()
		{
			Charge.JR_RX_NKSellCurrency = "USD";
			Charge.JR_LocalSellAmt = 100m;
			Charge.JR_OSSellAmt = 100m;
			Charge.JR_OSSellExRate = 2m;
			Job.JH_OA_LocalChargesAddr = ZGuid.Empty;
			Factory.Save();
			Notifications.Clear();
			var exceptionThrown = AssertExceptionThrown<LogSubscriberToAbortLogGroupProcessingSilentlyException>(() => CreateJobPostingProcessor(Shipment).Process(Notifications));
			AssertNotNull("Email in exception", exceptionThrown.Emails);
			AssertEquals("Pre Post Validation Errors", @"You cannot post because job S00010001 has errors. Please fix errors before posting.
 - Overseas Agent: Please enter Local Client or Overseas Agent.
 - Local Client: Please enter Local Client or Overseas Agent.
", Notifications.AsString);

			Charge.JR_OSSellExRate = 1m;
			Job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
			Factory.Save();
			AssertNoErrors(Job);

			Job.JH_ProfitLossReasonCode = ZString.Empty;
			JobProfitLossReasonCodeCollection plReasonCodes = new JobProfitLossReasonCodeCollection();
			JobProfitLossReasonCode plReasonCode = plReasonCodes.AddNew();
			plReasonCode.Code = "TST";
			plReasonCode.Description = (NoResString)"Test";
			AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, plReasonCodes);
			TestObjectCreator.SetupRegistrySetJobStatusToInvoicedWhenFirstARInvoicePosted(GlbCompany.CurrentCompany.PK.ToGuid(), null);

			JobProfitLossRequiringReasonParameters plRequiringReasonParameters = new JobProfitLossRequiringReasonParameters();
			plRequiringReasonParameters.ProfitThreshold = 10M;
			plRequiringReasonParameters.JobStatusCollection.AddNew().Code = JobHeaderStatus.JobInvoiced.Code;
			AccountingConfigurationRegistry.Instance.JobProfitLossRequiringReasonParameters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, plRequiringReasonParameters);
			Factory.Save();

			Notifications.Clear();
			exceptionThrown = AssertExceptionThrown<LogSubscriberToAbortLogGroupProcessingSilentlyException>(() => CreateJobPostingProcessor(Shipment).Process(Notifications));
			AssertNotNull("Email in exception", exceptionThrown.Emails);
			AssertEquals("Critical Post Errors", @"Job S00010001 status will be changed to INV after posting the first AR Invoice. The Profit/Loss threshold settings require Profit/Loss reason to be set on this job before posting any AR invoices.
", Notifications.AsString);

			Job.ClearRowNotifications();
			Job.JH_OA_LocalChargesAddr = ZGuid.Empty;
			Job.JH_ProfitLossReasonCode = ZString.Empty;
			Factory.Save();
			Notifications.Clear();
			exceptionThrown = AssertExceptionThrown<LogSubscriberToAbortLogGroupProcessingSilentlyException>(() => CreateJobPostingProcessor(Shipment).Process(Notifications));
			AssertNotNull("Email in exception", exceptionThrown.Emails);
			AssertEquals("Should only notify pre post validation errors because posting should not proceed.", @"You cannot post because job S00010001 has errors. Please fix errors before posting.
 - Overseas Agent: Please enter Local Client or Overseas Agent.
 - Local Client: Please enter Local Client or Overseas Agent.
", Notifications.AsString);
		}

		public void TestNoErrorWhenJobIsReadyForFinancialClosure()
		{
			Charge.JR_RX_NKSellCurrency = "USD";
			Charge.JR_LocalSellAmt = 100m;
			Charge.JR_OSSellAmt = 100m;
			Charge.JR_OSSellExRate = 1m;
			Job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
			Factory.Save();
			AssertNoErrors(Job);

			Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = false;
			Job.JH_Status = "JFC";
			Factory.Save();

			Notifications.Clear();
			var exceptionThrown = AssertExceptionThrown<LogSubscriberToAbortLogGroupProcessingSilentlyException>(() => CreateJobPostingProcessor(Shipment).Process(Notifications));
			AssertNotNull("Email in exception", exceptionThrown.Emails);
			AssertEquals("No Post Error", string.Empty, Notifications.AsString);
		}

		public void TestComplianceSequenceFailedToAssign()
		{
			ComplianceSubTypeAttributionRuleConfigurationCollection collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
			ComplianceSubTypeAttributionRuleConfiguration item = collection.AddNew();
			item.Country = Enterprise.Core.Constants.CountryCodes.Mexico;
			item.SubType = "TXI";
			item.LedgerType = "AR";
			item.InvoiceType = "INV";
			item.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID; // "TID";
			item.DisbursementRule = DisbursementRuleCodes.AllTransactions; // "ALL";
			item.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly; //"OTO";
			item.OrganisationLocation = "";

			AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Mexico);

			var group = Factory.LoadTop1<GlbGroup>(new ZQuery());

			Factory.Save();

			AccountingConfigurationRegistry.Instance.ComplianceInvoiceBookAllocaltionFailureNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			Env.OutgoingMailManager.EmailsCreated.Clear();

			AssertNoExceptionThrown(() => jobPostingProcessor.Process(Notifications));
			Factory.Save();

			var invoice = GetInvoice();

			AssertEquals("Should have sent email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];

			var expectedSubject = string.Format("Compliance Invoice Book Allocation Failure - Branch {0} / Sub Type TXI", GlbBranch.CurrentBranch.GB_Code);
			AssertEquals("Mail subject", expectedSubject, email.Subject);

			var expectedBody = string.Format(@"Please review your Compliance Invoice Book setups for Branch {0} and Sub Type {1} in the Maintain > Account > Compliance Sequences module.
Whilst logged in to Branch {0}  User {2} attempted to assign a Compliance Number against transaction {3}.
This assignment could not be made because an Active Compliance Book for Branch {0} and Sub Type {1} did not exist.
If required, please configure / activate a new Compliance Invoice Book.", GlbBranch.CurrentBranch.GB_Code, "TXI", Env.CurrentUser.FullName, invoice.AH_TransactionNum);
			AssertEquals("Email body", expectedBody, email.Body);
		}

		[TestDate(2018, 10, 10)]
		public void TestDigitalSignatureFailedToSign()
		{
			var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
			var item = collection.AddNew();
			item.Country = Enterprise.Core.Constants.CountryCodes.Portugal;
			item.SubType = "TXI";
			item.LedgerType = "AR";
			item.InvoiceType = "INV";
			item.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID; // "TID";
			item.DisbursementRule = DisbursementRuleCodes.AllTransactions; // "ALL";
			item.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly; //"OTO";
			item.OrganisationLocation = "";

			AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Portugal);
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "PTLIS";

			var sequence = TestObjectCreator.CreateNewComplianceSequence(ZGuid.Empty, "TXI", 1, 100, 10);
			sequence.XD_Prefix = "0102";
			sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			var group = Factory.LoadTop1<GlbGroup>(new ZQuery());

			Factory.Save();

			AccountingConfigurationRegistry.Instance.ComplianceInvoiceBookAllocaltionFailureNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			Env.OutgoingMailManager.EmailsCreated.Clear();

			AssertNoExceptionThrown(() => jobPostingProcessor.Process(Notifications));
			var invoice = GetInvoice();
			invoice.AH_InvoiceDate = invoice.AH_PostDate;
			Factory.Save();

			AssertEquals("Should have sent email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];

			AssertEquals("Mail subject", "Failed to sign transaction AR INV 00001000 with a valid digital Signature", email.Subject);

			var expectedBody = string.Format(@"When post transaction, CW1 failed to sign transaction AR INV 00001000 with a valid digital Signature due to the following reason:
Current invoice was not signed with a digital signature as it failed to find the previous invoice in the sequence.");
			AssertEquals("Email body", expectedBody, email.Body);
		}

		public void TestPerformTransactionDescriptionDefaulting()
		{
			var jobInvoiceDescriptionConfig = AccountingConfigurationRegistry.Instance.JobInvoiceDescriptionConfiguration.Value;
			var item = jobInvoiceDescriptionConfig.AddNew();
			item.JobType = "SHP";
			item.DirectionCode = "EXP";
			item.Mode = "ALL";
			item.InvoiceDescription = "Default Description";
			AccountingConfigurationRegistry.Instance.JobInvoiceDescriptionConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, jobInvoiceDescriptionConfig);

			AssertNoExceptionThrown(() => jobPostingProcessor.Process(Notifications));
			var invoice = GetInvoice();
			AssertEquals("AH_Desc should be defaulted", "Default Description", invoice.AH_Desc);
		}

		[TestDate(2013, 05, 16)]
		public void TestBackDateARInvoices_DefaultResponseNo()
		{
			AssertBackDateARInvoices(false);
		}

		[TestDate(2013, 05, 16)]
		public void TestBackDateARInvoices_DefaultResponseYes()
		{
			AssertBackDateARInvoices(true);
		}

		void AssertBackDateARInvoices(bool defaultResponse)
		{
			AccountingConfigurationRegistry.Instance.DefaultAllowUsersToBackDateInvoicesSetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultResponse);
			BackDateInvoicesConfiguration config = new BackDateInvoicesConfiguration();
			config.InvoiceDateConfigurationCollection[0].CurrentPeriod = InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.EndOfPriorMonth;
			config.InvoiceDateConfigurationCollection[0].Today = true;
			config.DefaultPostDateFromInvoiceDate = true;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			Factory.Save();

			jobPostingProcessor = new JobRevenuePoster(Shipment, null, ZDateTime.Today, PostDateOverride);
			AssertNoExceptionThrown(() => jobPostingProcessor.Process(Notifications));

			var invoice = GetInvoice();

			var expectedDate = defaultResponse ? new ZDateTime(2013, 4, 30) : new ZDateTime(2013, 5, 16);
			AssertEquals("Invoice Date", expectedDate, invoice.AH_InvoiceDate);
			AssertEquals("Post Date", expectedDate, invoice.AH_PostDate);
		}

		[TestDate(2013, 5, 16)]
		public void TestRecognizeRevenueBehaviourForDateBeforeFirstPeriod()
		{
			RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
			RevenueRecognition setting = valuesForTest.AddNew();
			setting.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting.Mode = Enterprise.Core.Constants.TransportModes.All;
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			Shipment.DocsAndCartage.JP_EstimatedPickup = ZDateTime.BrettsBirthday;
			Factory.Save();

			var jobCollection = new JobCollection(new BusinessObjectFactory(), new ZQuery(new ZQuery(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK), JoinCondition.And, new ZQuery(JobHeaderSchema.JH_ParentID, Shipment.PK)));
			jobCollection.Load();
			var jobsToValidate = jobCollection.Cast<Job>();
			var validationResult = new PostManagerValidation(jobsToValidate, JobInvoicingPostingOption.Revenue, jobsToValidate).Validate();

			AssertContains("Precondition: revenue recognition setup would normally cause an error", "cannot be set because an appropriate General Ledger Accounting Period has not been created to include this date.", validationResult.Message);

			AssertNoExceptionThrown(() => jobPostingProcessor.Process(Notifications));
			AssertEquals("Should be no notifications", ZString.Empty, Notifications.AsString);
		}

		#region Implementation

		protected override IProcessor CreateJobPostingProcessor(IJobInvoicingPlugIn plugin)
		{
			return new JobRevenuePoster(plugin, GUIProvider, InvoiceDateOverride, PostDateOverride);
		}

		protected override void SetUp()
		{
			SetUpCore();

			base.SetUp();
		}

		protected virtual void SetUpCore()
		{
			Shipment = TestObjectCreator.CreateShipment("S00010001", "AUSYD", "NZAKL", null);
			Job = TestObjectCreator.CreateJob(Shipment, false, false);
			Job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
			Charge = TestObjectCreator.CreateCharge(Job, TestObjectCreator.CC1, "Desc 1", TestObjectCreator.AUD, 50M, null, TestObjectCreator.AUD, 100M, TestObjectCreator.LocalClient);

			Job.RunPreSaveValidation();
			AssertNoErrors(Job);

			Factory.Save();
		}

		#endregion
	}
}
