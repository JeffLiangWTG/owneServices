using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(ARCreditNoteApprovalSubscriber))]
	class ARCreditNoteApprovalSubscriberTest : LogSubscriberTest<ARCreditNoteApprovalSubscriber>
	{
		[SuspendCriticalValidation]
		public void TestAmendingWithCreditNote_ClosedJob()
		{
			var invoice = GetInvoiceWithLine();
			var creditNote = GetAmendingCreditNote(invoice);
			var invoiceDate = creditNote.AH_InvoiceDate;
			var postDate = creditNote.AH_PostDate;
			var amendingReasonCode = creditNote.AH_ReceiptType;
			Job.JH_Status = JobHeaderStatus.Closed.Code;
			CreateAndSaveRequest(creditNote, invoice);

			ProcessLogs();

			var newFactory = new BusinessObjectFactory();
			var transaction = GetTransactionNotMatchingInvoicePK(newFactory, invoice);

			AssertNull(transaction);
			Request = newFactory.Load<ARCreditNoteApprovalRequest>(Request.PK);
			AssertEquals("Approved", Constants.GenApprovalRequestApprovalStatus.Approved, Request.XP_ApprovalStatus);
		}

		[SuspendCriticalValidation]
		public void TestAmendingWithCreditNote_PreSaveValidationErrors()
		{
			var invoice = GetInvoiceWithLine();
			var creditNote = GetAmendingCreditNote(invoice);
			creditNote.AH_ExchangeRate = -1m;
			creditNote.AH_PostDate = ZDateTime.Today.AddDays(1);
			CreateAndSaveRequest(creditNote, invoice);

			ProcessLogs();

			var newFactory = new BusinessObjectFactory();
			var transaction = GetTransactionNotMatchingInvoicePK(newFactory, invoice);

			AssertNull(transaction);
			Request = newFactory.Load<ARCreditNoteApprovalRequest>(Request.PK);
			AssertEquals("Approved", Constants.GenApprovalRequestApprovalStatus.Approved, Request.XP_ApprovalStatus);
			Assert("Logs match", NotifiedEventList.Contains(string.Format(@"[ARCreditNoteApprovalSubscriber] Could not process Approved ARCreditNoteApprovalRequest {0} for credit note amendment because the amending AR Credit Note has validation errors:
Local Amount: The Local Amount should be equal to OS Amount when Local Currency is used
Local Tax Amount: The Local Tax Amount should be equal to OS Tax Amount when Local Currency is used
Exchange Rate: Exchange Rate cannot be negative.
Total Amount: The sum of the transaction lines should be greater than zero.
Exchange Rate: Exchange Rate for Currency AUD must be greater than 0.
Invoice Post Date: The post date cannot be in the future", Request.PK)));
		}

		[SuspendCriticalValidation]
		[TestDate(2020, 4, 7)]
		public void TestAutoPostingErrorWithRequestPostDateInThePast()
		{
			AccountingConfigurationRegistry.Instance.PostCreditNoteOnApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "DEF");
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting("TWO", true));

			Charge = TestObjectCreator.CreateCharge(Job, TestObjectCreator.CC1, "Desc 1", TestObjectCreator.AUD, 50M, null, TestObjectCreator.AUD, -100, TestObjectCreator.Debtor);
			TestObjectCreator.Debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
			Charge.JR_AT_SellGSTRate = TestObjectCreator.CC1.AC_AT_GSTRate;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newCreator = new TestObjectCreator(newFactory);
			var creditNote = newCreator.CreateARCreditNoteWithLine("001", TestObjectCreator.Debtor, TestObjectCreator.AUD, 1.0m, "Desc", Job, TestObjectCreator.CC1, 100.00m, ZDateTime.Today, false);
			creditNote.Lines[0].AL_GE = Job.Department.PK;
			creditNote.AH_TransactionCategory = "FIN";

			Request = Factory.New<ARCreditNoteApprovalRequest>();
			Request.Initialize(new[] { creditNote }, Job.PK, "JH", JobInvoicingPostingOption.Revenue);
			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			Request.XP_SystemCreateUser = Requestor.GS_Code;
			Factory.Save();

			AssertEquals("request post date is today's date.", new DateTime(2020, 4, 7), Request.PostingDetails.PostDate.Date);

			TestDateAttribute.Date = new DateTime(2020, 4, 9, 12, 0, 0);
			Request.UpdateApprovalUserAndStatus("ad1", Constants.GenApprovalRequestApprovalStatus.Approved);
			AssertEquals("double approval user needed", Constants.GenApprovalRequestApprovalStatus.Requested, Request.XP_ApprovalStatus);
			AssertEquals("Request post date should remain unchanged", new DateTime(2020, 4, 7), Request.PostingDetails.PostDate.Date);
			Request.UpdateApprovalUserAndStatus("ad2", Constants.GenApprovalRequestApprovalStatus.Approved);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Approved, Request.XP_ApprovalStatus);
			AssertEquals("request approval date is today's date", new DateTime(2020, 4, 9), Request.XP_ApprovalDate.Date);
			AssertEquals("when request is approved, request post date is updated with approval date", Request.XP_ApprovalDate.Date, Request.PostingDetails.PostDate.Date);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2020, 4, 11, 12, 0, 0);
			Assert("PostingDetails PostDate must be in the past", Request.PostingDetails.PostDate.Date < ZDateTime.Today);
			Assert("Charge is not posted yet", !Charge.IsRevenuePosted);

			ProcessLogs();

			Charge = newFactory.Load<Charge>(Charge.PK);
			Assert("Charge is posted", Charge.IsRevenuePosted);
			Request = newFactory.Load<ARCreditNoteApprovalRequest>(Request.PK);
			AssertEquals("Request is posted", Constants.GenApprovalRequestApprovalStatus.Posted, Request.XP_ApprovalStatus);

			var line = Factory.Load<AccTransactionLines>(Charge.JR_AL_ARLine);
			AssertNotNull(line);
			var header = Factory.Load<AccTransactionHeader>(line.AL_AH);
			AssertNotNull(header);
			AssertEquals("Post Date equals today's date", new DateTime(2020, 4, 11), header.AH_PostDate.Date);
		}

		[SuspendCriticalValidation]
		[TestDate(2015, 04, 01)]
		public void TestAutoPosting_WithInvoiceDateRegistry_Default()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(ZDateTime.Now.Year, 1, 1));
			TestObjectCreator.Debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;

			BackDateInvoicesConfiguration configuration = new BackDateInvoicesConfiguration();
			configuration.InvoiceDateConfigurationCollection.RemoveAll();
			TestObjectCreator.AddInvoiceDateConfiguration(configuration, "SHP", "ALL", "ALL", "ALL",
				InvoiceDateConfigurationLookups.SignificantDateCodes.InvoiceAddDate,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.SignificantDate,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.SignificantDate,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.EndOfPriorMonth,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.InvoiceAddDate,
				false, false, "OCD");
			configuration.DefaultPostDateFromInvoiceDate = true;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, configuration);

			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingConstants.InvAndPstDateDefaultingRuleTypes.Default.Code);
			AccountingConfigurationRegistry.Instance.UseCurrentDateAsTransactionDateWhenAutoPosting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			AssertAutoPostingInvoceDate_NegativeAR("NegativeAR CrediteNote InvoceDate should be same as GUI setting InvoceDate", "S00001001", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(-10));
			AssertAutoPostingInvoceDate_AmendAR("AmendAR CrediteNote InvoceDate should be same as original AR InvoceDate", "S00001002", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(-10));
			AssertAutoPostingInvoceDate_ReversalAR("ReversalAR CrediteNote InvoceDate should be same as original AR InvoceDate due to BackDateInvoicesConfiguration OCD", "S00001004", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(-10));
		}

		[SuspendCriticalValidation]
		[TestDate(2015, 04, 01)]
		public void TestAutoPosting_WithInvoiceDateRegistry_UseCurrentDate()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(ZDateTime.Now.Year, 1, 1));
			TestObjectCreator.Debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;

			BackDateInvoicesConfiguration configuration = new BackDateInvoicesConfiguration();
			configuration.InvoiceDateConfigurationCollection.RemoveAll();
			TestObjectCreator.AddInvoiceDateConfiguration(configuration, "SHP", "ALL", "ALL", "ALL",
				InvoiceDateConfigurationLookups.SignificantDateCodes.InvoiceAddDate,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.SignificantDate,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.SignificantDate,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.EndOfPriorMonth,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.InvoiceAddDate,
				false, false, "OCD");
			configuration.DefaultPostDateFromInvoiceDate = true;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, configuration);

			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingConstants.InvAndPstDateDefaultingRuleTypes.Default.Code);
			AccountingConfigurationRegistry.Instance.UseCurrentDateAsTransactionDateWhenAutoPosting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertAutoPostingInvoceDate_NegativeAR("InvoceDate should be today due to registry UseCurrentDateAsTransactionDateWhenAutoPosting is on", "S00001001", ZDateTime.Today.AddDays(-10), ZDateTime.Today);
			AssertAutoPostingInvoceDate_AmendAR("InvoceDate should be today due to registry UseCurrentDateAsTransactionDateWhenAutoPosting is on", "S00001002", ZDateTime.Today.AddDays(-10), ZDateTime.Today);
			AssertAutoPostingInvoceDate_ReversalAR("InvoceDate should be today due to registry UseCurrentDateAsTransactionDateWhenAutoPosting is on", "S00001004", ZDateTime.Today.AddDays(-10), ZDateTime.Today);
		}

		[SuspendCriticalValidation]
		[TestDate(2015, 04, 01)]
		public void TestAutoPosting_WithInvoiceDateRegistry_DefaultButMTH()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(ZDateTime.Now.Year, 1, 1));
			TestObjectCreator.Debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;

			BackDateInvoicesConfiguration configuration = new BackDateInvoicesConfiguration();
			configuration.InvoiceDateConfigurationCollection.RemoveAll();
			TestObjectCreator.AddInvoiceDateConfiguration(configuration, "SHP", "ALL", "ALL", "ALL",
				InvoiceDateConfigurationLookups.SignificantDateCodes.InvoiceAddDate,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.SignificantDate,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.SignificantDate,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.EndOfPriorMonth,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.InvoiceAddDate,
				false, false, "OCD");
			configuration.DefaultPostDateFromInvoiceDate = true;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, configuration);

			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingConstants.InvAndPstDateDefaultingRuleTypes.MonthEndSuspension.Code);
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());
			AccountingConfigurationRegistry.Instance.UseCurrentDateAsTransactionDateWhenAutoPosting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			AssertAutoPostingInvoceDate_NegativeAR("InvoceDate should be last month final date due to InvAndPstDateDefaultingBehaviour set as MTH", "S00001001", ZDateTime.Today.AddDays(-10), new ZDateTime(ZDateTime.Now.Year, ZDateTime.Now.Month, 1).AddDays(-1));
			AssertAutoPostingInvoceDate_AmendAR("InvoceDate should be last month final date due to InvAndPstDateDefaultingBehaviour set as MTH", "S00001002", ZDateTime.Today.AddDays(-10), new ZDateTime(ZDateTime.Now.Year, ZDateTime.Now.Month, 1).AddDays(-1));
			AssertAutoPostingInvoceDate_ReversalAR("InvoceDate should be last month final date due to InvAndPstDateDefaultingBehaviour set as MTH", "S00001004", ZDateTime.Today.AddDays(-10), new ZDateTime(ZDateTime.Now.Year, ZDateTime.Now.Month, 1).AddDays(-1));
		}

		[SuspendCriticalValidation]
		[TestDate(2015, 04, 01)]
		public void TestAutoPosting_WithInvoiceDateRegistry_UseCurrentDateButMTH()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(ZDateTime.Now.Year, 1, 1));
			TestObjectCreator.Debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;

			BackDateInvoicesConfiguration configuration = new BackDateInvoicesConfiguration();
			configuration.InvoiceDateConfigurationCollection.RemoveAll();
			TestObjectCreator.AddInvoiceDateConfiguration(configuration, "SHP", "ALL", "ALL", "ALL",
				InvoiceDateConfigurationLookups.SignificantDateCodes.InvoiceAddDate,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.SignificantDate,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.SignificantDate,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.EndOfPriorMonth,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.InvoiceAddDate,
				false, false, "OCD");
			configuration.DefaultPostDateFromInvoiceDate = true;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, configuration);

			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingConstants.InvAndPstDateDefaultingRuleTypes.MonthEndSuspension.Code);
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());
			AccountingConfigurationRegistry.Instance.UseCurrentDateAsTransactionDateWhenAutoPosting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertAutoPostingInvoceDate_NegativeAR("InvoceDate should be last month final date due to InvAndPstDateDefaultingBehaviour set as MTH", "S00001001", ZDateTime.Today.AddDays(-10), new ZDateTime(ZDateTime.Now.Year, ZDateTime.Now.Month, 1).AddDays(-1));
			AssertAutoPostingInvoceDate_AmendAR("InvoceDate should be last month final date due to InvAndPstDateDefaultingBehaviour set as MTH", "S00001002", ZDateTime.Today.AddDays(-10), new ZDateTime(ZDateTime.Now.Year, ZDateTime.Now.Month, 1).AddDays(-1));
			AssertAutoPostingInvoceDate_ReversalAR("InvoceDate should be last month final date due to InvAndPstDateDefaultingBehaviour set as MTH", "S00001004", ZDateTime.Today.AddDays(-10), new ZDateTime(ZDateTime.Now.Year, ZDateTime.Now.Month, 1).AddDays(-1));
		}

		void AssertAutoPostingInvoceDate_NegativeAR(string comment, string jobNum, ZDateTime approvedDate, ZDateTime expectedInvoiceDate)
		{
			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment(jobNum), TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc 1", TestObjectCreator.AUD, 50M, null, TestObjectCreator.AUD, -100, TestObjectCreator.Debtor);
			charge.JR_AT_SellGSTRate = TestObjectCreator.CC1.AC_AT_GSTRate;
			Factory.Save();

			var creditNote = TestObjectCreator.CreateARCreditNoteWithLine($"CR001", charge.SellAccount, charge.SellCurrency, 1.0m, "Desc", job, charge.ChargeCode, 100.00m, ZDateTime.Today, false);
			creditNote.Lines[0].AL_GE = job.Department.PK;
			creditNote.AH_InvoiceDate = approvedDate;
			creditNote.AH_TransactionCategory = "FIN";

			var creditNoteApprovalRequest = Factory.New<ARCreditNoteApprovalRequest>();
			creditNoteApprovalRequest.Initialize(new[] { creditNote }, job.PK, JobHeaderSchema.Constants.Prefix, JobInvoicingPostingOption.Revenue);
			creditNoteApprovalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			creditNoteApprovalRequest.XP_SystemCreateTimeUtc = approvedDate.AddDays(-1);
			creditNoteApprovalRequest.XP_GS_NKApprovingUser1 = Approver.GS_Code;

			creditNote.Delete();
			Factory.Save();

			creditNoteApprovalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			creditNoteApprovalRequest.XP_ApprovalDate = approvedDate;
			creditNoteApprovalRequest.XP_GS_NKApprovingUser1 = Approver.GS_Code;
			Factory.Save();

			ProcessLogs();

			var newFactory = new BusinessObjectFactory();
			var requests = newFactory.Load<ARCreditNoteApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, job.PK));
			AssertEquals("No duplicate request created", 1, requests.Length);
			var requestReload = newFactory.Load<ARCreditNoteApprovalRequest>(creditNoteApprovalRequest.PK);
			AssertEquals("PreCondition , request posted", Constants.GenApprovalRequestApprovalStatus.Posted, requestReload.XP_ApprovalStatus);

			var creditNotes = newFactory.Load<ARCreditNote>(new ZQuery(AccTransactionHeaderSchema.AH_JH, job.PK.ToGuid()));
			AssertEquals("PreCondition , No duplicate transaction header created", 1, creditNotes.Length);

			var credinoteReload = creditNotes.First();
			AssertEquals(comment, expectedInvoiceDate.Date, credinoteReload.InvoiceDate.Date);
		}

		void AssertAutoPostingInvoceDate_AmendAR(string comment, string jobNum, ZDateTime approvedDate, ZDateTime expectedInvoiceDate)
		{
			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment(jobNum), TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc 1", TestObjectCreator.AUD, 50M, null, TestObjectCreator.AUD, -100, TestObjectCreator.Debtor);
			charge.JR_AT_SellGSTRate = TestObjectCreator.CC1.AC_AT_GSTRate;
			Factory.Save();

			var invoiceWithLine = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", charge.SellCurrency, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			invoiceWithLine.AH_OH = charge.JR_OH_SellAccount;
			invoiceWithLine.AH_GB = job.Branch.PK;
			invoiceWithLine.AH_GE = job.Department.PK;
			invoiceWithLine.Lines[0].AL_GE = job.Department.PK;
			invoiceWithLine.AH_TransactionCategory = "FIN";
			invoiceWithLine.AH_JH = job.PK;
			invoiceWithLine.Lines[0].AL_AC = charge.ChargeCode.PK;
			invoiceWithLine.Lines[0].AL_JH = job.PK;
			Factory.Save();

			var creditNote = GetAmendingCreditNote(invoiceWithLine);
			creditNote.AH_JH = job.PK;
			creditNote.AH_InvoiceDate = approvedDate;
			creditNote.AH_OH = charge.JR_OH_SellAccount;
			creditNote.AH_TransactionCategory = "FIN";
			creditNote.Lines[0].AL_JH = job.PK;
			creditNote.Lines[0].AL_AC = charge.ChargeCode.PK;
			creditNote.Lines[0].AL_GE = job.Department.PK;
			creditNote.AH_ReceiptType = Constants.GenApprovalRequestReasonCode.Code.IncorrectOrganisationBilled;

			var creditNoteApprovalRequest = Factory.New<ARCreditNoteApprovalRequest>();
			creditNoteApprovalRequest.Initialize(new[] { creditNote }, invoiceWithLine.PK, AccTransactionHeaderSchema.Constants.Prefix, JobInvoicingPostingOption.All);
			creditNoteApprovalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			creditNoteApprovalRequest.XP_SystemCreateTimeUtc = approvedDate.AddDays(-1);
			creditNoteApprovalRequest.XP_GS_NKApprovingUser1 = Approver.GS_Code;

			creditNote.Delete();
			Factory.Save();

			creditNoteApprovalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			creditNoteApprovalRequest.XP_ApprovalDate = approvedDate;
			creditNoteApprovalRequest.XP_GS_NKApprovingUser1 = Approver.GS_Code;
			Factory.Save();

			ProcessLogs();

			var newFactory = new BusinessObjectFactory();
			var requests = newFactory.Load<ARCreditNoteApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, invoiceWithLine.PK));
			AssertEquals("No duplicate request created", 1, requests.Length);
			var requestReload = newFactory.Load<ARCreditNoteApprovalRequest>(creditNoteApprovalRequest.PK);
			AssertEquals("PreCondition , request posted", Constants.GenApprovalRequestApprovalStatus.Posted, requestReload.XP_ApprovalStatus);

			var creditNotes = newFactory.Load<ARCreditNote>(
				new ZQuery(AccTransactionHeaderSchema.AH_JH, job.PK.ToGuid())
				.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.CreditNote)
			);
			AssertEquals("PreCondition , No duplicate transaction header created", 1, creditNotes.Length);

			var credinoteReload = creditNotes.First();
			AssertEquals(comment, expectedInvoiceDate.Date, credinoteReload.InvoiceDate.Date);
		}

		void AssertAutoPostingInvoceDate_ReversalAR(string comment, string jobNum, ZDateTime approvedDate, ZDateTime expectedInvoiceDate)
		{
			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment(jobNum), TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc 1", TestObjectCreator.AUD, 50M, null, TestObjectCreator.AUD, -100, TestObjectCreator.Debtor);
			charge.JR_AT_SellGSTRate = TestObjectCreator.CC1.AC_AT_GSTRate;
			Factory.Save();

			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			arInvoice.Lines.Add(TestObjectCreator.CreateRevenueLine(charge, arInvoice.PK));
			arInvoice.AH_InvoiceDate = approvedDate;
			arInvoice.AH_PostDate = approvedDate;
			arInvoice.AH_JH = job.PK;
			Factory.Save();

			var creditNoteApprovalRequest = Factory.New<ARCreditNoteApprovalRequest>();
			creditNoteApprovalRequest.ChangeApprovalTypeForInvoiceReversal();
			creditNoteApprovalRequest.Initialize(new[] { arInvoice }, arInvoice.PK, AccTransactionHeaderSchema.Constants.Prefix, JobInvoicingPostingOption.Revenue);
			creditNoteApprovalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			creditNoteApprovalRequest.XP_ReasonCode = Enterprise.Core.Constants.GenApprovalRequestReasonCode.Code.IncorrectRating;
			creditNoteApprovalRequest.XP_SystemCreateTimeUtc = approvedDate.AddDays(-1);
			creditNoteApprovalRequest.XP_GS_NKApprovingUser1 = Approver.GS_Code;

			Factory.Save();

			creditNoteApprovalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			creditNoteApprovalRequest.XP_ApprovalDate = approvedDate;
			creditNoteApprovalRequest.XP_GS_NKApprovingUser1 = Approver.GS_Code;
			Factory.Save();

			ProcessLogs();

			var newFactory = new BusinessObjectFactory();
			var requests = newFactory.Load<ARCreditNoteApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, arInvoice.PK));
			AssertEquals("No duplicate request created", 1, requests.Length);
			var requestReload = newFactory.Load<ARCreditNoteApprovalRequest>(creditNoteApprovalRequest.PK);
			AssertEquals("PreCondition , request posted", Constants.GenApprovalRequestApprovalStatus.Posted, requestReload.XP_ApprovalStatus);

			var creditNotes = newFactory.Load<ARCreditNote>(
				new ZQuery(AccTransactionHeaderSchema.AH_JH, job.PK.ToGuid())
				.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.CreditNote)
			);
			AssertEquals("PreCondition , No duplicate transaction header created", 1, creditNotes.Length);

			var credinoteReload = creditNotes.First();
			AssertEquals(comment, expectedInvoiceDate.Date, credinoteReload.InvoiceDate.Date);
		}

		[SuspendCriticalValidation]
		public void TestAmendingWithCreditNote_EnsureUserContextOfTransactionBranchDepartmentWhileAutoPosting()
		{
			var invoice = GetInvoiceWithLine();
			var creditNote = GetAmendingCreditNote(invoice);

			var branch = creditNote.Branch;
			var department = creditNote.Department;
			branch.AllowedDepartments.DeleteAll();
			var newCombo = branch.AllowedDepartments.AddNew();
			newCombo.AAB_GB_Branch = branch.PK;
			newCombo.AAB_GE_Department = department.PK;

			CreateAndSaveRequest(creditNote, invoice);

			var newBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, "SIN"));
			var newDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FIA"));
			AssertNotNull(newBranch);
			AssertNotNull(newDepartment);
			AssertNotEquals(newBranch, branch);
			AssertNotEquals(newDepartment, department);

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), newBranch.PK.ToGuid(), newDepartment.PK.ToGuid()))
			{
				ProcessLogs();
			}
			var newFactory = new BusinessObjectFactory();
			var transaction = GetTransactionNotMatchingInvoicePK(newFactory, invoice);

			AssertNotNull(transaction);

			Request = newFactory.Load<ARCreditNoteApprovalRequest>(Request.PK);
			AssertEquals("Posted", Constants.GenApprovalRequestApprovalStatus.Posted, Request.XP_ApprovalStatus);
		}

		[SuspendCriticalValidation]
		public void TestAmendingWithCreditNote_WithNoLines()
		{
			var invoice = GetInvoiceWithLine();
			var creditNote = GetAmendingCreditNote(invoice);
			creditNote.Lines.RemoveAndDeleteAll();
			var invoiceDate = creditNote.AH_InvoiceDate;
			var postDate = creditNote.AH_PostDate;
			var amendingReasonCode = creditNote.AH_ReceiptType;
			CreateAndSaveRequest(creditNote, invoice);

			ProcessLogs();

			var newFactory = new BusinessObjectFactory();
			var transaction = GetTransactionNotMatchingInvoicePK(newFactory, invoice);

			AssertNull(transaction);
			Request = newFactory.Load<ARCreditNoteApprovalRequest>(Request.PK);
			AssertEquals("Approved", Constants.GenApprovalRequestApprovalStatus.Approved, Request.XP_ApprovalStatus);
			Assert("Logs match", NotifiedEventList.Contains(string.Format("[ARCreditNoteApprovalSubscriber] Could not process Approved ARCreditNoteApprovalRequest {0} for credit note amendment because amended AR Credit Note has no lines.", Request.PK)));
		}

		[SuspendCriticalValidation]
		public void TestAmendingWithCreditNote_ReasonCodeEmpty()
		{
			var invoice = GetInvoiceWithLine();
			var creditNote = GetAmendingCreditNote(invoice);
			var invoiceDate = creditNote.AH_InvoiceDate;
			var postDate = creditNote.AH_PostDate;
			var amendingReasonCode = ZString.Empty;
			creditNote.AH_ReceiptType = amendingReasonCode;
			CreateAndSaveRequest(creditNote, invoice);

			ProcessLogs();

			var newFactory = new BusinessObjectFactory();
			var transaction = GetTransactionNotMatchingInvoicePK(newFactory, invoice);

			AssertNull(transaction);
			Request = newFactory.Load<ARCreditNoteApprovalRequest>(Request.PK);
			AssertEquals("Approved", Constants.GenApprovalRequestApprovalStatus.Approved, Request.XP_ApprovalStatus);
			Assert("Logs match", NotifiedEventList.Contains(string.Format("[ARCreditNoteApprovalSubscriber] GenApprovalRequest {0} EDT log received, but cannot process due to constraints on ARCreditNoteApprovalRequest.", Request.PK)));
		}

		[SuspendCriticalValidation]
		public void TestAmendingWithCreditNote_SecurityCheckPointDisallowed()
		{
			var checkpoint = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AmendTransactionWCreditNote);
			checkpoint.IsAllowed = false;

			var invoice = GetInvoiceWithLine();
			var creditNote = GetAmendingCreditNote(invoice);
			var invoiceDate = creditNote.AH_InvoiceDate;
			var postDate = creditNote.AH_PostDate;
			var amendingReasonCode = creditNote.AH_ReceiptType;
			CreateAndSaveRequest(creditNote, invoice);

			ProcessLogs();

			var newFactory = new BusinessObjectFactory();
			var transaction = GetTransactionNotMatchingInvoicePK(newFactory, invoice);

			AssertNull(transaction);
			Request = newFactory.Load<ARCreditNoteApprovalRequest>(Request.PK);
			AssertEquals("Approved", Constants.GenApprovalRequestApprovalStatus.Approved, Request.XP_ApprovalStatus);
			Assert("Logs match", NotifiedEventList.Contains(string.Format("[ARCreditNoteApprovalSubscriber] Could not process Approved ARCreditNoteApprovalRequest {0} for credit note amendment because amendment credit note could not be generated from transaction.", Request.PK)));
		}

		[SuspendCriticalValidation]
		public void TestAmendingWithCreditNote_Posted()
		{
			var invoice = GetInvoiceWithLine();
			var creditNote = GetAmendingCreditNote(invoice);
			var invoiceDate = creditNote.AH_InvoiceDate;
			var postDate = creditNote.AH_PostDate;
			var amendingReasonCode = creditNote.AH_ReceiptType;
			CreateAndSaveRequest(creditNote, invoice);

			ProcessLogs();

			var newFactory = new BusinessObjectFactory();
			var transaction = GetTransactionNotMatchingInvoicePK(newFactory, invoice);

			AssertNotNull(transaction);
			AssertEquals("Invoice Date", invoiceDate, transaction.AH_InvoiceDate);
			AssertEquals("Post Date", postDate, transaction.AH_PostDate);
			AssertEquals("Amending Reason Code", amendingReasonCode, transaction.AH_ReceiptType);
			AssertEquals("Amending Reason Code", amendingReasonCode, ((IAmending)transaction).AmendingReasonCode);
			AssertEquals("Transaction lines count", 1, transaction.Lines.Count);

			var line = transaction.Lines[0];
			AssertEquals("AL_OSExTaxAmount", new ZDecimal(100.00), line.AL_OSExTaxAmount);
			AssertEquals("AL_LocalExTaxAmount", new ZDecimal(100.00), line.AL_LocalExTaxAmount);
			AssertEquals("AL_OSTaxAmount", new ZDecimal(10.00), line.AL_OSTaxAmount);
			AssertEquals("AL_LocalTaxAmount", new ZDecimal(10.00), line.AL_LocalTaxAmount);
			AssertEquals("ExRate", new ZDecimal(1.00), transaction.ExchangeRate.Rate);
			AssertEquals("TaxRate", TestObjectCreator.GST1.PK, line.AL_AT);

			Request = newFactory.Load<ARCreditNoteApprovalRequest>(Request.PK);
			AssertEquals("Posted", Constants.GenApprovalRequestApprovalStatus.Posted, Request.XP_ApprovalStatus);
		}

		[SuspendCriticalValidation]
		public void TestAmendingWithCreditNote_RefDatesEmpty()
		{
			using (InvoiceBaseValidationTest.InitaliseComplianceFactory(areOriginalTransactionReferenceFieldsMandatory: true))
			{
				var invoice = GetInvoiceWithLine();
				var creditNote = GetAmendingCreditNote(invoice);

				Assert("Invoice Date should not be empty", !creditNote.AH_InvoiceDate.IsEmpty);
				Assert("Amending Reason Code should not be empty", !creditNote.AH_ReceiptType.IsEmpty);

				Assert("Original Reference Start Date should be empty", creditNote.AH_OriginalReferenceStartDate.IsEmpty);
				Assert("Original Reference End Date should be empty", creditNote.AH_OriginalReferenceEndDate.IsEmpty);
			}
		}

		[SuspendCriticalValidation]
		public void TestAmendingWithCreditNote_SystemCreatedAmendingContext()
		{
			var invoice = GetInvoiceWithLine();
			var creditNote = GetAmendingCreditNote(invoice);
			CreateAndSaveRequest(creditNote, invoice);

			var newFactory = new BusinessObjectFactory();

			var mock = new Mock<IQueuedLog>();
			mock.SetupGet(x => x.Factory).Returns(newFactory);
			mock.SetupGet(x => x.SJ_ParentID).Returns(Request.PK);
			mock.SetupGet(x => x.SJ_ParentTableCode).Returns(Request.TablePrefix);

			SubscriberForTest.ProcessLogQueueItems_ForTestOnly(new[] { mock.Object });

			var transaction = GetTransactionNotMatchingInvoicePK(newFactory, invoice);

			AssertNotNull(transaction);
			Assert(transaction.HasContext(BusinessContext.SystemCreatedAmending));
		}

		[SuspendCriticalValidation]
		public void TestAmendingWithCreditNote_PeriodicInvoice()
		{
			AssertAmendingWithCreditNote_PeriodicOrConsolInvoice(true);
		}

		[SuspendCriticalValidation]
		public void TestAmendingWithCreditNote_ConsolInvoice()
		{
			AssertAmendingWithCreditNote_PeriodicOrConsolInvoice(false);
		}

		void AssertAmendingWithCreditNote_PeriodicOrConsolInvoice(bool isPeriodicInvoice)
		{
			var invoice = GetInvoiceWithLine();
			invoice.AH_JH = ZGuid.Empty;
			if (isPeriodicInvoice)
			{
				invoice.AH_TransactionCategory = "FID";
				Assert(invoice.IsPeriodicInvoice);
			}
			else
			{
				invoice.AH_ConsolidatedInvoiceRef = "C00001001";
				Assert(invoice.IsConsolInvoice);
			}
			var creditNote = GetAmendingCreditNote(invoice);
			CreateAndSaveRequest(creditNote, invoice);

			var newFactory = new BusinessObjectFactory();

			var mock = new Mock<IQueuedLog>();
			mock.SetupGet(x => x.Factory).Returns(newFactory);
			mock.SetupGet(x => x.SJ_ParentID).Returns(Request.PK);
			mock.SetupGet(x => x.SJ_ParentTableCode).Returns(Request.TablePrefix);

			SubscriberForTest.ProcessLogQueueItems_ForTestOnly(new[] { mock.Object });
			newFactory.Save();
			Assert(SubscriberForTest.ErrorBuffer_ForTestOnly.IsEmpty);

			var amendingCreditNote = newFactory.LoadTop1<ARCreditNote>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, invoice.PK));
			AssertNotNull(amendingCreditNote);
			AssertEquals("AMENDMENT RELATED TO INV 001", amendingCreditNote.AH_Desc);
		}

		internal InvoicingBase GetTransactionNotMatchingInvoicePK(BusinessObjectFactory newFactory, InvoicingBase invoice)
		{
			var transaction = newFactory.LoadTop1<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.AH_JH, Job.PK).AddToFilter(new ZQuery(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, invoice.PK)));
			return transaction;
		}

		internal void CreateAndSaveRequest(InvoicingBase creditNote, InvoicingBase invoice)
		{
			Request = Factory.New<ARCreditNoteApprovalRequest>();
			Request.Initialize(new[] { creditNote }, invoice.PK, "AH", JobInvoicingPostingOption.All);
			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			Request.XP_SystemCreateUser = Requestor.GS_Code;

			creditNote.Delete();
			Factory.Save();

			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			Request.XP_GS_NKApprovingUser1 = Approver.GS_Code;
			Factory.Save();
		}

		internal InvoicingBase GetInvoiceWithLine()
		{
			Charge = TestObjectCreator.CreateCharge(Job, TestObjectCreator.CC1, "Desc 1", TestObjectCreator.AUD, 50M, null, TestObjectCreator.AUD, -100, TestObjectCreator.Debtor);
			TestObjectCreator.Debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
			Charge.JR_AT_SellGSTRate = TestObjectCreator.CC1.AC_AT_GSTRate;
			Factory.Save();

			var invoiceWithLine = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			invoiceWithLine.AH_OH = TestObjectCreator.Debtor.PK;
			invoiceWithLine.AH_GB = Job.Branch.PK;
			invoiceWithLine.AH_GE = Job.Department.PK;
			invoiceWithLine.Lines[0].AL_GE = Job.Department.PK;
			invoiceWithLine.AH_TransactionCategory = "FIN";
			invoiceWithLine.AH_JH = Job.PK;
			invoiceWithLine.Lines[0].AL_AC = TestObjectCreator.CC1.PK;
			invoiceWithLine.Lines[0].AL_JH = Job.PK;

			return invoiceWithLine;
		}

		internal InvoicingBase GetAmendingCreditNote(InvoicingBase invoice)
		{
			var creditNote = ((IAmending)invoice).GenerateAmendingTransaction(TransactionTypes.CreditNote) as InvoicingBase;
			creditNote.AH_JH = Job.PK;
			creditNote.AH_GB = invoice.AH_GB;
			creditNote.AH_GE = invoice.AH_GE;
			creditNote.Lines[0].AL_JH = Job.PK;
			creditNote.AH_TransactionCategory = "FIN";
			creditNote.Lines[0].AL_AC = TestObjectCreator.CC1.PK;
			creditNote.AH_OH = TestObjectCreator.Debtor.PK;
			var amendingReasonCode = Constants.GenApprovalRequestReasonCode.Code.IncorrectOrganisationBilled;
			creditNote.AH_ReceiptType = amendingReasonCode;
			var invoiceDate = ZDateTime.Today.AddDays(-3);
			var postDate = ZDateTime.Today;
			creditNote.AH_InvoiceDate = invoiceDate;
			creditNote.AH_PostDate = postDate;

			return creditNote;
		}

		public void TestReverseJobRelatedARInvoice()
		{
			var charge1 = TestObjectCreator.CreateCharge(Job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			Factory.Save();
			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			arInvoice.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, arInvoice.PK));
			arInvoice.AH_JH = Job.PK;
			Factory.Save();

			Request = Factory.New<ARCreditNoteApprovalRequest>();
			Request.ChangeApprovalTypeForInvoiceReversal();
			Request.Initialize(new[] { arInvoice }, arInvoice.PK, AccTransactionHeaderSchema.Constants.Prefix, JobInvoicingPostingOption.Revenue);
			Request.XP_ApprovalStatus = Enterprise.Core.Constants.GenApprovalRequestApprovalStatus.Requested;
			Request.XP_ReasonCode = Enterprise.Core.Constants.GenApprovalRequestReasonCode.Code.IncorrectRating;
			Request.XP_SystemCreateUser = Requestor.GS_Code;
			Factory.Save();

			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			Request.XP_GS_NKApprovingUser1 = Approver.GS_Code;
			Factory.Save();

			Assert(!arInvoice.AH_IsCancelled);
			ProcessLogs();

			var expectedMessage = FormattableString.Invariant($"[ARCreditNoteApprovalSubscriber] Transaction {arInvoice.AH_TransactionNum} linked to ARCreditNoteApprovalRequest {Request.PK} can be reversed.");
			Assert(NotifiedEventList.Contains(expectedMessage));

			var newFactory = new BusinessObjectFactory();
			var reloadedInvoice = newFactory.Load<ARInvoice>(arInvoice.PK);
			Assert(reloadedInvoice.AH_IsCancelled);
			AssertEquals(0m, reloadedInvoice.AH_OutstandingAmount);

			var reloadedRequest = newFactory.Load<ARCreditNoteApprovalRequest>(Request.PK);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Posted, reloadedRequest.XP_ApprovalStatus);

			var arCreditNotes = newFactory.Load<ARCreditNote>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, "CRD"));
			AssertEquals(1, arCreditNotes.Length);
			AssertEquals(reloadedInvoice.AH_GB, arCreditNotes[0].AH_GB);
			AssertEquals(reloadedInvoice.AH_GE, arCreditNotes[0].AH_GE);
			AssertEquals(reloadedInvoice.AH_LocalTotalAmount, arCreditNotes[0].AH_LocalTotalAmount);
			AssertEquals(Requestor.GS_Code, arCreditNotes[0].AH_SystemCreateUser);
			AssertEquals(Core.Constants.GenApprovalRequestReasonCode.Code.IncorrectRating, arCreditNotes[0].AH_ReceiptType);
		}

		public void TestReverseMiscARInvoice()
		{
			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			var line = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1M, 700M, 70M, 0M);
			Factory.Save();

			Request = Factory.New<ARCreditNoteApprovalRequest>();
			Request.ChangeApprovalTypeForInvoiceReversal();
			Request.Initialize(new[] { arInvoice }, arInvoice.PK, AccTransactionHeaderSchema.Constants.Prefix, JobInvoicingPostingOption.Revenue);
			Request.XP_ApprovalStatus = Enterprise.Core.Constants.GenApprovalRequestApprovalStatus.Requested;
			Request.XP_ReasonCode = Enterprise.Core.Constants.GenApprovalRequestReasonCode.Code.IncorrectRating;
			Request.XP_SystemCreateUser = Requestor.GS_Code;
			Factory.Save();

			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			Request.XP_GS_NKApprovingUser1 = Approver.GS_Code;
			Factory.Save();

			Assert(!arInvoice.AH_IsCancelled);
			ProcessLogs();

			var expectedMessage = FormattableString.Invariant($"[ARCreditNoteApprovalSubscriber] Transaction {arInvoice.AH_TransactionNum} linked to ARCreditNoteApprovalRequest {Request.PK} can be reversed.");
			Assert(NotifiedEventList.Contains(expectedMessage));

			var nullReferenceExMessage = "Object reference not set to an instance of an object.";
			Assert(!NotifiedEventList.Any(x => x.Contains(nullReferenceExMessage)));

			var newFactory = new BusinessObjectFactory();
			var reloadedInvoice = newFactory.Load<ARInvoice>(arInvoice.PK);
			Assert(reloadedInvoice.AH_IsCancelled);
			AssertEquals(0m, reloadedInvoice.AH_OutstandingAmount);

			var reloadedRequest = newFactory.Load<ARCreditNoteApprovalRequest>(Request.PK);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Posted, reloadedRequest.XP_ApprovalStatus);

			var arCreditNotes = newFactory.Load<ARCreditNote>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, "CRD"));
			AssertEquals(1, arCreditNotes.Length);
			AssertEquals(reloadedInvoice.AH_GB, arCreditNotes[0].AH_GB);
			AssertEquals(reloadedInvoice.AH_GE, arCreditNotes[0].AH_GE);
			AssertEquals(reloadedInvoice.AH_LocalTotalAmount, arCreditNotes[0].AH_LocalTotalAmount);
			AssertEquals(Requestor.GS_Code, arCreditNotes[0].AH_SystemCreateUser);
			AssertEquals(Core.Constants.GenApprovalRequestReasonCode.Code.IncorrectRating, arCreditNotes[0].AH_ReceiptType);
		}

		public void TestReverseMiscARInvoice_NoErrorEmailSentWhenNoErrorDetected()
		{
			AssertReverseMiscARInvoice_ErrorEmailSent(false);
		}

		public void TestReverseMiscARInvoice_HasErrorEmailSentWhenErrorDetected()
		{
			AssertReverseMiscARInvoice_ErrorEmailSent(true);
		}

		public void AssertReverseMiscARInvoice_ErrorEmailSent(bool hasError)
		{
			Requestor.GS_EmailAddress = "requestor@aCompany.com.au";
			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			var line = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1M, 700M, 70M, 0M);
			Factory.Save();

			Request = Factory.New<ARCreditNoteApprovalRequest>();
			Request.ChangeApprovalTypeForInvoiceReversal();
			Request.Initialize(new[] { arInvoice }, arInvoice.PK, AccTransactionHeaderSchema.Constants.Prefix, JobInvoicingPostingOption.Revenue);
			Request.XP_ApprovalStatus = Enterprise.Core.Constants.GenApprovalRequestApprovalStatus.Requested;
			Request.XP_ReasonCode = Enterprise.Core.Constants.GenApprovalRequestReasonCode.Code.IncorrectRating;
			Request.XP_SystemCreateUser = Requestor.GS_Code;
			Factory.Save();

			var periodManager = TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddMonths(-1));
			periodManager.CloseSubLedgerPeriod();
			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			Request.XP_GS_NKApprovingUser1 = Approver.GS_Code;
			Factory.Save();
			Assert(!arInvoice.AH_IsCancelled);
			AssertEquals("Pre conditon - mailbox should have 1 email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			if (hasError)
			{
				AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfReversalTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			}
			ProcessLogs();

			var expectedMessage = hasError ? FormattableString.Invariant($"[ARCreditNoteApprovalSubscriber] Transaction {arInvoice.AH_TransactionNum} linked to ARCreditNoteApprovalRequest {Request.PK} could not be reversed. Posting is not permitted. Receivables Invoice Transactions cannot be reversed. This is controlled by the registry setting Accounting -> Receivable Defaults -> Default Settings -> Prevent Reversal of Invoice Transactions.")
								: FormattableString.Invariant($"[ARCreditNoteApprovalSubscriber] Transaction {arInvoice.AH_TransactionNum} linked to ARCreditNoteApprovalRequest {Request.PK} can be reversed.");
			Assert(NotifiedEventList.Contains(expectedMessage));
			if (hasError)
			{
				AssertEquals("Post condition - mailbox should have only 2 email as there is an new error to report", 2, Env.OutgoingMailManager.EmailsCreated.Count);
			}
			else
			{
				AssertEquals("Post condition - mailbox should still have only 1 email as there is no error to report", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		[SuspendCriticalValidation]
		[TestDate(2020, 4, 7)]
		public void TestReverseMiscARInvoice_PostDateIsNotUpdated_ErrorEmailSent()
		{
			AccountingConfigurationRegistry.Instance.PostCreditNoteOnApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "DEF");
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting("TWO", true));

			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			var line = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1M, 700M, 70M, 0M);
			arInvoice.Lines.Add(line);
			Factory.Save();

			Request = Factory.New<ARCreditNoteApprovalRequest>();
			Request.ChangeApprovalTypeForInvoiceReversal();
			Request.Initialize(new[] { arInvoice }, arInvoice.PK, AccTransactionHeaderSchema.Constants.Prefix, JobInvoicingPostingOption.Revenue);
			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			Request.XP_ReasonCode = Constants.GenApprovalRequestReasonCode.Code.IncorrectRating;
			Request.XP_SystemCreateUser = Requestor.GS_Code;
			Requestor.GS_EmailAddress = "requestor@aCompany.com.au";
			Factory.Save();

			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			Request.XP_GS_NKApprovingUser1 = Approver.GS_Code;

			Factory.Save();

			TestDateAttribute.Date = new DateTime(2020, 4, 9, 12, 0, 0);
			Assert("Predondition: Request posting details post date", Request.PostingDetails.PostDate.Date < ZDateTime.Today);
			Assert("Precondition: Invoice cancellation status", !arInvoice.AH_IsCancelled);

			var previousEmailCount = Env.OutgoingMailManager.EmailsCreated.Count;

			AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfReversalTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ProcessLogs();

			var expectedMessage = $"[ARCreditNoteApprovalSubscriber] Transaction {arInvoice.AH_TransactionNum} linked to ARCreditNoteApprovalRequest {Request.PK} could not be reversed. Posting is not permitted. Receivables Invoice Transactions cannot be reversed. This is controlled by the registry setting Accounting -> Receivable Defaults -> Default Settings -> Prevent Reversal of Invoice Transactions.";

			var newFactory = new BusinessObjectFactory();
			arInvoice = newFactory.Load<ARInvoice>(arInvoice.PK);
			Assert(!arInvoice.AH_IsCancelled);
			Assert(NotifiedEventList.Contains(expectedMessage));
			AssertEquals("Email count", previousEmailCount + 1, Env.OutgoingMailManager.EmailsCreated.Count);
			previousEmailCount++;

			Request = newFactory.Load<ARCreditNoteApprovalRequest>(Request.PK);
			AssertEquals("Request posting details post date", new DateTime(2020, 4, 7), Request.PostingDetails.PostDate.Date);
		}

		public void TestIfNewApprovalRequestIsGettingCreatedWhilePosting()
		{
			Charge = TestObjectCreator.CreateCharge(Job, TestObjectCreator.CC1, "Desc 1", TestObjectCreator.AUD, 50M, null, TestObjectCreator.AUD, -100, TestObjectCreator.Debtor);
			TestObjectCreator.Debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
			Charge.JR_AT_SellGSTRate = TestObjectCreator.CC1.AC_AT_GSTRate;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			TestObjectCreator newCreator = new TestObjectCreator(newFactory);
			ARCreditNote creditNote1 = newCreator.CreateARCreditNoteWithLine("001", TestObjectCreator.Debtor, TestObjectCreator.AUD, 1.0m, "Desc", Job, TestObjectCreator.CC1, 100.00m, ZDateTime.Today, false);
			creditNote1.Lines[0].AL_GE = Job.Department.PK;
			creditNote1.AH_TransactionCategory = "FIN";

			Request = Factory.New<ARCreditNoteApprovalRequest>();
			Request.Initialize(new[] { creditNote1 }, Job.PK, "JH", JobInvoicingPostingOption.Revenue);
			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			Request.XP_SystemCreateUser = Requestor.GS_Code;
			Factory.Save();

			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			Request.XP_GS_NKApprovingUser1 = Approver.GS_Code;
			Factory.Save();

			Assert(!Charge.IsRevenuePosted);
			ProcessLogs();

			Charge = newFactory.Load<Charge>(Charge.PK);
			Assert(Charge.IsRevenuePosted);
			AssertEquals("User match", Requestor.GS_Code, Charge.ARLine.TransactionHeader.AH_SystemCreateUser);
			var requests = newFactory.Load<ARCreditNoteApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, Job.PK));
			AssertEquals("No duplicate request created", 1, requests.Length);
			Request = newFactory.Load<ARCreditNoteApprovalRequest>(Request.PK);
			AssertEquals("Posted", Constants.GenApprovalRequestApprovalStatus.Posted, Request.XP_ApprovalStatus);
		}

		public void TestPostCreditNoteOnApprovalRegistryOptionNotEnabled()
		{
			AccountingConfigurationRegistry.Instance.PostCreditNoteOnApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			Charge = TestObjectCreator.CreateCharge(Job, TestObjectCreator.CC1, "Desc 1", TestObjectCreator.AUD, 50M, null, TestObjectCreator.AUD, -100, TestObjectCreator.Debtor);
			TestObjectCreator.Debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
			Charge.JR_AT_SellGSTRate = TestObjectCreator.CC1.AC_AT_GSTRate;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			TestObjectCreator newCreator = new TestObjectCreator(newFactory);
			ARCreditNote creditNote1 = newCreator.CreateARCreditNoteWithLine("001", TestObjectCreator.Debtor, TestObjectCreator.AUD, 1.0m, "Desc", Job, TestObjectCreator.CC1, 100.00m, ZDateTime.Today, false);
			creditNote1.Lines[0].AL_GE = Job.Department.PK;
			creditNote1.AH_TransactionCategory = "FIN";

			Request = Factory.New<ARCreditNoteApprovalRequest>();
			Request.Initialize(new[] { creditNote1 }, Job.PK, "JH", JobInvoicingPostingOption.Revenue);
			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			Request.XP_SystemCreateUser = Requestor.GS_Code;
			Factory.Save();

			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			Factory.Save();

			Assert(!Charge.IsRevenuePosted);
			ProcessLogs();

			Charge = Factory.Load<Charge>(Charge.PK);
			Assert(!Charge.IsRevenuePosted);

			Assert("Logs match", NotifiedEventList.Contains(string.Format(CultureInfo.InvariantCulture, "[ARCreditNoteApprovalSubscriber] PostCreditNoteOnApproval registry turned OFF, cannot process ARCreditNoteApprovalRequest {0} EDT logs.", Request.PK)));
		}

		public void TestLogGrouping_ShouldBeBasedOnParentID()
		{
			AccountingConfigurationRegistry.Instance.PostCreditNoteOnApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			Charge = TestObjectCreator.CreateCharge(Job, TestObjectCreator.CC1, "Desc 1", TestObjectCreator.AUD, 50M, null, TestObjectCreator.AUD, -100, TestObjectCreator.Debtor);
			TestObjectCreator.Debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
			Charge.JR_AT_SellGSTRate = TestObjectCreator.CC1.AC_AT_GSTRate;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			TestObjectCreator newCreator = new TestObjectCreator(newFactory);
			ARCreditNote creditNote1 = newCreator.CreateARCreditNoteWithLine("001", TestObjectCreator.Debtor, TestObjectCreator.AUD, 1.0m, "Desc", Job, TestObjectCreator.CC1, 100.00m, ZDateTime.Today, false);
			creditNote1.Lines[0].AL_GE = Job.Department.PK;
			creditNote1.AH_TransactionCategory = "FIN";

			var request1 = Factory.New<ARCreditNoteApprovalRequest>();
			request1.Initialize(new[] { creditNote1 }, Job.PK, "JH", JobInvoicingPostingOption.Revenue);
			request1.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			request1.XP_SystemCreateUser = Requestor.GS_Code;
			var request2 = Factory.New<ARCreditNoteApprovalRequest>();
			request2.Initialize(new[] { creditNote1 }, Job.PK, "JH", JobInvoicingPostingOption.Revenue);
			request2.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			request2.XP_SystemCreateUser = Requestor.GS_Code;
			Factory.Save();

			request1.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Cancelled;
			Factory.Save();
			request1.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			request2.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			Factory.Save();

			ProcessLogs();

			var logged = "[ARCreditNoteApprovalSubscriber] PostCreditNoteOnApproval registry turned OFF, cannot process ARCreditNoteApprovalRequest {0} EDT logs.";
			AssertEquals("Log count", 1, NotifiedEventList.Where(x => x == string.Format(CultureInfo.InvariantCulture, logged, request1.PK)).Count());
			AssertEquals("Log count", 1, NotifiedEventList.Where(x => x == string.Format(CultureInfo.InvariantCulture, logged, request1.PK)).Count());
		}

		[TestDate(2019, 10, 28)]
		public void TestChargeNotPosted_SubLedgerPeriodClosed()
		{
			var periodManager = TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddMonths(-1));
			var periodCalculator = new AccountingPeriodCalculator(Factory);

			Charge = TestObjectCreator.CreateCharge(Job, TestObjectCreator.CC1, "Desc 1", TestObjectCreator.AUD, 50M, null, TestObjectCreator.AUD, -100, TestObjectCreator.Debtor);
			TestObjectCreator.Debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
			Charge.JR_AT_SellGSTRate = TestObjectCreator.CC1.AC_AT_GSTRate;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			TestObjectCreator newCreator = new TestObjectCreator(newFactory);
			ARCreditNote creditNote1 = newCreator.CreateARCreditNoteWithLine("001", TestObjectCreator.Debtor, TestObjectCreator.AUD, 1.0m, "Desc", Job, TestObjectCreator.CC1, 100.00m, ZDateTime.Today, false);
			creditNote1.Lines[0].AL_GE = Job.Department.PK;
			creditNote1.AH_InvoiceDate = ZDateTime.Today.AddDays(-2);
			creditNote1.AH_PostDate = ZDateTime.Today.AddDays(-1);
			creditNote1.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;
			AssertEquals("Precondition: SubLedger Period should be OK to post", PostDateValidationResult.OK, periodCalculator.IsPostDateValid(creditNote1.AH_PostDate));

			Request = Factory.New<ARCreditNoteApprovalRequest>();
			Request.Initialize(new[] { creditNote1 }, Job.PK, JobHeaderSchema.Constants.Prefix, JobInvoicingPostingOption.Revenue);
			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			Request.XP_SystemCreateUser = Requestor.GS_Code;
			Factory.Save();

			periodManager.CloseSubLedgerPeriod();
			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			Request.XP_GS_NKApprovingUser1 = Approver.GS_Code;
			Factory.Save();

			AssertEquals("Precondition: SubLedger Period should be Closed to post", PostDateValidationResult.SubLedgerPeriodClosed, periodCalculator.IsPostDateValid(creditNote1.AH_PostDate));
			NotifiedEventList.Clear();
			ProcessLogs();

			Charge = Factory.Load<Charge>(Charge.PK);
			Assert(!Charge.IsRevenuePosted);
			NotifiedEventList.Contains("[ARCreditNoteApprovalSubscriber] Post Date: This date falls into a period where the sub-ledger is closed\r\n");
		}

		public void TestChargePosted_JobInvoicingPostingOption_Revenue()
		{
			Charge = TestObjectCreator.CreateCharge(Job, TestObjectCreator.CC1, "Desc 1", TestObjectCreator.AUD, 50M, null, TestObjectCreator.AUD, -100, TestObjectCreator.Debtor);
			TestObjectCreator.Debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
			Charge.JR_AT_SellGSTRate = TestObjectCreator.CC1.AC_AT_GSTRate;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			TestObjectCreator newCreator = new TestObjectCreator(newFactory);
			ARCreditNote creditNote1 = newCreator.CreateARCreditNoteWithLine("001", TestObjectCreator.Debtor, TestObjectCreator.AUD, 1.0m, "Desc", Job, TestObjectCreator.CC1, 100.00m, ZDateTime.Today, false);
			creditNote1.Lines[0].AL_GE = Job.Department.PK;
			creditNote1.AH_InvoiceDate = ZDateTime.Today.AddDays(-2);
			creditNote1.AH_PostDate = ZDateTime.Today.AddDays(-1);
			creditNote1.AH_TransactionCategory = "FIN";

			Request = Factory.New<ARCreditNoteApprovalRequest>();
			Request.Initialize(new[] { creditNote1 }, Job.PK, "JH", JobInvoicingPostingOption.Revenue);
			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			Request.XP_SystemCreateUser = Requestor.GS_Code;
			Factory.Save();

			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			Request.XP_GS_NKApprovingUser1 = Approver.GS_Code;
			Factory.Save();

			Assert(!Charge.IsRevenuePosted);
			ProcessLogs();

			Charge = newFactory.Load<Charge>(Charge.PK);
			Assert(Charge.IsRevenuePosted);
			AssertEquals("User match", Requestor.GS_Code, Charge.ARLine.TransactionHeader.AH_SystemCreateUser);
			Request = newFactory.Load<ARCreditNoteApprovalRequest>(Request.PK);
			AssertEquals("Posted", Constants.GenApprovalRequestApprovalStatus.Posted, Request.XP_ApprovalStatus);

			var line = Factory.Load<AccTransactionLines>(Charge.JR_AL_ARLine);
			AssertNotNull(line);
			var header = Factory.Load<AccTransactionHeader>(line.AL_AH);
			AssertNotNull(header);
			AssertEquals("Invoice Date", creditNote1.AH_InvoiceDate, header.AH_InvoiceDate);
			AssertEquals("Post Date", ZDate.Today, header.AH_PostDate.Date);
		}

		public void TestChargePosted_PositiveChargesAddedOnJobAfterRequestCreated()
		{
			Job.JH_OA_AgentCollectAddr = TestObjectCreator.Debtor.MainAddress.PK;
			Charge = TestObjectCreator.CreateCharge(Job, TestObjectCreator.CC1, "Desc 1", TestObjectCreator.AUD, 50M, null, TestObjectCreator.AUD, -100, TestObjectCreator.Debtor);
			TestObjectCreator.Debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
			Charge.JR_AT_SellGSTRate = TestObjectCreator.CC1.AC_AT_GSTRate;
			var charge2 = TestObjectCreator.CreateCharge(Job, TestObjectCreator.CC1, "Desc 2", TestObjectCreator.AUD, 50M, null, TestObjectCreator.AUD, 100, TestObjectCreator.Debtor1);
			TestObjectCreator.Debtor1.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
			charge2.JR_AT_SellGSTRate = TestObjectCreator.CC1.AC_AT_GSTRate;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			TestObjectCreator newCreator = new TestObjectCreator(newFactory);
			ARCreditNote creditNote1 = newCreator.CreateARCreditNoteWithLine("001", TestObjectCreator.Debtor, TestObjectCreator.AUD, 1.0m, "Desc", Job, TestObjectCreator.CC1, 100.00m, ZDateTime.Today, false);
			creditNote1.Lines[0].AL_GE = Job.Department.PK;
			creditNote1.AH_InvoiceDate = ZDateTime.Today.AddDays(-2);
			creditNote1.AH_PostDate = ZDateTime.Today.AddDays(-1);
			creditNote1.AH_TransactionCategory = "FIN";

			Request = Factory.New<ARCreditNoteApprovalRequest>();
			Request.Initialize(new[] { creditNote1 }, Job.PK, "JH", JobInvoicingPostingOption.Revenue);
			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			Request.XP_SystemCreateUser = Requestor.GS_Code;
			Factory.Save();

			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			Factory.Save();

			Assert(!Charge.IsRevenuePosted);
			Assert(!charge2.IsRevenuePosted);
			ProcessLogs();

			Charge = newFactory.Load<Charge>(Charge.PK);
			Assert(Charge.IsRevenuePosted);
			charge2 = newFactory.Load<Charge>(charge2.PK);
			Assert(charge2.IsRevenuePosted);
			AssertEquals("User match", Requestor.GS_Code, Charge.ARLine.TransactionHeader.AH_SystemCreateUser);
			Request = newFactory.Load<ARCreditNoteApprovalRequest>(Request.PK);
			AssertEquals("Posted", Constants.GenApprovalRequestApprovalStatus.Posted, Request.XP_ApprovalStatus);

			var line = Factory.Load<AccTransactionLines>(Charge.JR_AL_ARLine);
			AssertNotNull(line);
			var header = Factory.Load<AccTransactionHeader>(line.AL_AH);
			AssertNotNull(header);
			AssertEquals("Invoice Date", creditNote1.AH_InvoiceDate, header.AH_InvoiceDate);
			AssertEquals("Post Date", ZDate.Today, header.AH_PostDate.Date);

			line = Factory.Load<AccTransactionLines>(charge2.JR_AL_ARLine);
			AssertNotNull(line);
			header = Factory.Load<AccTransactionHeader>(line.AL_AH);
			AssertNotNull(header);
			AssertEquals("Invoice Date", creditNote1.AH_InvoiceDate, header.AH_InvoiceDate);
			AssertEquals("Post Date", ZDate.Today, header.AH_PostDate.Date);
		}

		public void TestChargePosted_JobInvoicingPostingOption_Agent()
		{
			Job.JH_OA_AgentCollectAddr = TestObjectCreator.Debtor.MainAddress.PK;
			Charge = TestObjectCreator.CreateCharge(Job, TestObjectCreator.CC1, "Desc 1", TestObjectCreator.AUD, 50M, null, TestObjectCreator.AUD, -100, TestObjectCreator.Debtor);
			TestObjectCreator.Debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
			Charge.JR_AT_SellGSTRate = TestObjectCreator.CC1.AC_AT_GSTRate;
			var charge2 = TestObjectCreator.CreateCharge(Job, TestObjectCreator.CC1, "Desc 1", TestObjectCreator.AUD, 50M, null, TestObjectCreator.AUD, -100, TestObjectCreator.Agent);
			TestObjectCreator.Agent.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
			charge2.JR_AT_SellGSTRate = TestObjectCreator.CC1.AC_AT_GSTRate;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			TestObjectCreator newCreator = new TestObjectCreator(newFactory);
			ARCreditNote creditNote1 = newCreator.CreateARCreditNoteWithLine("001", TestObjectCreator.Debtor, TestObjectCreator.AUD, 1.0m, "Desc", Job, TestObjectCreator.CC1, 100.00m, ZDateTime.Today, false);
			creditNote1.Lines[0].AL_GE = Job.Department.PK;
			creditNote1.AH_InvoiceDate = ZDateTime.Today.AddDays(-2);
			creditNote1.AH_PostDate = ZDateTime.Today.AddDays(-1);
			creditNote1.AH_TransactionCategory = "FIN";

			Request = Factory.New<ARCreditNoteApprovalRequest>();
			Request.Initialize(new[] { creditNote1 }, Job.PK, "JH", JobInvoicingPostingOption.Agent);
			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			Request.XP_SystemCreateUser = Requestor.GS_Code;
			Factory.Save();

			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			Factory.Save();

			Assert(!Charge.IsRevenuePosted);
			Assert(!charge2.IsRevenuePosted);
			ProcessLogs();

			Charge = newFactory.Load<Charge>(Charge.PK);
			Assert(Charge.IsRevenuePosted);
			charge2 = newFactory.Load<Charge>(charge2.PK);
			Assert(!charge2.IsRevenuePosted);
			AssertEquals("User match", Requestor.GS_Code, Charge.ARLine.TransactionHeader.AH_SystemCreateUser);
			Request = newFactory.Load<ARCreditNoteApprovalRequest>(Request.PK);
			AssertEquals("Posted", Constants.GenApprovalRequestApprovalStatus.Posted, Request.XP_ApprovalStatus);

			var line = Factory.Load<AccTransactionLines>(Charge.JR_AL_ARLine);
			AssertNotNull(line);
			var header = Factory.Load<AccTransactionHeader>(line.AL_AH);
			AssertNotNull(header);
			AssertEquals("Invoice Date", creditNote1.AH_InvoiceDate, header.AH_InvoiceDate);
			AssertEquals("Post Date", ZDate.Today, header.AH_PostDate.Date);
		}

		public void TestChargePosted_JobInvoicingPostingOption_SisterCompany()
		{
			Assert_JobInvoicingPostingOption_SisterCompanyAndLocalSisterCompany(JobInvoicingPostingOption.AllSisterCompanyCharges);
		}

		public void TestChargePosted_JobInvoicingPostingOption_LocalSisterCompany()
		{
			TestObjectCreator.Debtor.OH_RL_NKClosestPort = "AUMEL";
			Assert_JobInvoicingPostingOption_SisterCompanyAndLocalSisterCompany(JobInvoicingPostingOption.LocalSisterCompanyChargesOnly);
		}

		void Assert_JobInvoicingPostingOption_SisterCompanyAndLocalSisterCompany(JobInvoicingPostingOption option)
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_IsActive = true;
			company.GC_OH_OrgProxy = TestObjectCreator.Debtor.PK;
			Factory.Save();

			Charge = TestObjectCreator.CreateCharge(Job, TestObjectCreator.CC1, "Desc 1", TestObjectCreator.AUD, 50M, null, TestObjectCreator.AUD, -100, TestObjectCreator.Debtor);
			TestObjectCreator.Debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
			Charge.JR_AT_SellGSTRate = TestObjectCreator.CC1.AC_AT_GSTRate;
			var charge2 = TestObjectCreator.CreateCharge(Job, TestObjectCreator.CC1, "Desc 1", TestObjectCreator.AUD, 50M, null, TestObjectCreator.AUD, -100, TestObjectCreator.Agent);
			TestObjectCreator.Agent.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
			charge2.JR_AT_SellGSTRate = TestObjectCreator.CC1.AC_AT_GSTRate;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			TestObjectCreator newCreator = new TestObjectCreator(newFactory);
			ARCreditNote creditNote1 = newCreator.CreateARCreditNoteWithLine("001", TestObjectCreator.Debtor, TestObjectCreator.AUD, 1.0m, "Desc", Job, TestObjectCreator.CC1, 100.00m, ZDateTime.Today, false);
			creditNote1.Lines[0].AL_GE = Job.Department.PK;
			creditNote1.AH_InvoiceDate = ZDateTime.Today.AddDays(-2);
			creditNote1.AH_PostDate = ZDateTime.Today.AddDays(-1);
			creditNote1.AH_TransactionCategory = "FIN";

			Request = Factory.New<ARCreditNoteApprovalRequest>();
			Request.Initialize(new[] { creditNote1 }, Job.PK, "JH", option);
			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			Request.XP_SystemCreateUser = Requestor.GS_Code;
			Factory.Save();

			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			Factory.Save();

			Assert(!Charge.IsRevenuePosted);
			Assert(!charge2.IsRevenuePosted);
			ProcessLogs();

			Charge = newFactory.Load<Charge>(Charge.PK);
			Assert(Charge.IsRevenuePosted);
			charge2 = newFactory.Load<Charge>(charge2.PK);
			Assert(!charge2.IsRevenuePosted);
			AssertEquals("User match", Requestor.GS_Code, Charge.ARLine.TransactionHeader.AH_SystemCreateUser);
			Request = newFactory.Load<ARCreditNoteApprovalRequest>(Request.PK);
			AssertEquals("Posted", Constants.GenApprovalRequestApprovalStatus.Posted, Request.XP_ApprovalStatus);

			var line = Factory.Load<AccTransactionLines>(Charge.JR_AL_ARLine);
			AssertNotNull(line);
			var header = Factory.Load<AccTransactionHeader>(line.AL_AH);
			AssertNotNull(header);
			AssertEquals("Invoice Date", creditNote1.AH_InvoiceDate, header.AH_InvoiceDate);
			AssertEquals("Post Date", ZDate.Today, header.AH_PostDate.Date);
		}

		public void TestChargeNotPosted_JobInvoicingPostingOption_Unsupported()
		{
			Assert_JobInvoicingPostingOption_Unsupported(JobInvoicingPostingOption.LocalClient);
			Assert_JobInvoicingPostingOption_Unsupported(JobInvoicingPostingOption.Costs);
			Assert_JobInvoicingPostingOption_Unsupported(JobInvoicingPostingOption.Disbursement);
			Assert_JobInvoicingPostingOption_Unsupported(JobInvoicingPostingOption.CustomsDSBChargeAPOnly);
			Assert_JobInvoicingPostingOption_Unsupported(JobInvoicingPostingOption.CustomsDSBChargeAROnly);
			Assert_JobInvoicingPostingOption_Unsupported(JobInvoicingPostingOption.ConsolCosts);
		}

		void Assert_JobInvoicingPostingOption_Unsupported(JobInvoicingPostingOption option)
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_IsActive = true;
			company.GC_OH_OrgProxy = TestObjectCreator.Debtor.PK;
			Factory.Save();
			switch (option)
			{
				case JobInvoicingPostingOption.LocalClient:
					Job.JH_OA_LocalChargesAddr = TestObjectCreator.Debtor.MainAddress.PK;
					break;
				default:
					break;
			}

			Charge = TestObjectCreator.CreateCharge(Job, TestObjectCreator.CC1, "Desc 1", TestObjectCreator.AUD, 50M, null, TestObjectCreator.AUD, -100, TestObjectCreator.Debtor);
			TestObjectCreator.Debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
			Charge.JR_AT_SellGSTRate = TestObjectCreator.CC1.AC_AT_GSTRate;

			var newFactory = new BusinessObjectFactory();
			TestObjectCreator newCreator = new TestObjectCreator(newFactory);
			ARCreditNote creditNote1 = newCreator.CreateARCreditNoteWithLine("001", TestObjectCreator.Debtor, TestObjectCreator.AUD, 1.0m, "Desc", Job, TestObjectCreator.CC1, 100.00m, ZDateTime.Today, false);
			creditNote1.Lines[0].AL_GE = Job.Department.PK;
			creditNote1.AH_TransactionCategory = "FIN";

			Request = Factory.New<ARCreditNoteApprovalRequest>();
			Request.Initialize(new[] { creditNote1 }, Job.PK, "JH", option);
			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			Request.XP_SystemCreateUser = Requestor.GS_Code;
			Factory.Save();

			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			Factory.Save();

			Assert(!Charge.IsRevenuePosted);
			NotifiedEventList.Clear();
			ProcessLogs();

			Charge = Factory.Load<Charge>(Charge.PK);
			Assert(!Charge.IsRevenuePosted);
			AssertNull("No Transaction Header", Charge.ARLine.TransactionHeader);
			Request = newFactory.Load<ARCreditNoteApprovalRequest>(Request.PK);
			AssertEquals("Approved", Constants.GenApprovalRequestApprovalStatus.Approved, Request.XP_ApprovalStatus);
			Assert("Logs match", NotifiedEventList.Contains(string.Format(CultureInfo.InvariantCulture, "[ARCreditNoteApprovalSubscriber] Credit Note on '{0}' is not posted after approval because '{1}' is not supported. This credit note must be posted by selecting '{1}' from the job.", Job.JH_JobNum, Request.PostingOptionForDisplay)));
		}

		public void TestChargeNotPosted_CreditNoteReversalRequestWithJHParentTableCode()
		{
			Charge = TestObjectCreator.CreateCharge(Job, TestObjectCreator.CC1, "Desc 1", TestObjectCreator.AUD, 50M, null, TestObjectCreator.AUD, -100, TestObjectCreator.Debtor);
			TestObjectCreator.Debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
			Charge.JR_AT_SellGSTRate = TestObjectCreator.CC1.AC_AT_GSTRate;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			TestObjectCreator newCreator = new TestObjectCreator(newFactory);
			ARCreditNote creditNote1 = newCreator.CreateARCreditNoteWithLine("001", TestObjectCreator.Debtor, TestObjectCreator.AUD, 1.0m, "Desc", Job, TestObjectCreator.CC1, 100.00m, ZDateTime.Today, false);
			creditNote1.Lines[0].AL_GE = Job.Department.PK;
			creditNote1.AH_TransactionCategory = "FIN";

			Request = Factory.New<ARCreditNoteApprovalRequest>();
			Request.Initialize(new[] { creditNote1 }, Job.PK, "JH", JobInvoicingPostingOption.Revenue);
			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			Request.XP_ApprovalType = Constants.GenApprovalRequestApprovalType.ARCreditNoteForReversal;
			Request.XP_SystemCreateUser = Requestor.GS_Code;
			Factory.Save();

			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			Factory.Save();
			Assert(!Charge.IsRevenuePosted);
			ProcessLogs();

			Assert("Logs match", NotifiedEventList.Contains(string.Format(CultureInfo.InvariantCulture, "[ARCreditNoteApprovalSubscriber] GenApprovalRequest {0} EDT log received, but cannot process due to constraints on ARCreditNoteApprovalRequest.", Request.PK)));
			Charge = Factory.Load<Charge>(Charge.PK);
			Assert(!Charge.IsRevenuePosted);
		}

		public void TestChargeNotPosted_NotAnApprovedRequest()
		{
			Charge = TestObjectCreator.CreateCharge(Job, TestObjectCreator.CC1, "Desc 1", TestObjectCreator.AUD, 50M, null, TestObjectCreator.AUD, -100, TestObjectCreator.Debtor);
			TestObjectCreator.Debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
			Charge.JR_AT_SellGSTRate = TestObjectCreator.CC1.AC_AT_GSTRate;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			TestObjectCreator newCreator = new TestObjectCreator(newFactory);
			ARCreditNote creditNote1 = newCreator.CreateARCreditNoteWithLine("001", TestObjectCreator.Debtor, TestObjectCreator.AUD, 1.0m, "Desc", Job, TestObjectCreator.CC1, 100.00m, ZDateTime.Today, false);
			creditNote1.Lines[0].AL_GE = Job.Department.PK;
			creditNote1.AH_TransactionCategory = "FIN";

			Request = Factory.New<ARCreditNoteApprovalRequest>();
			Request.Initialize(new[] { creditNote1 }, Job.PK, "JH", JobInvoicingPostingOption.Revenue);
			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			Request.XP_SystemCreateUser = Requestor.GS_Code;
			Factory.Save();

			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Cancelled;
			Factory.Save();
			Assert(!Charge.IsRevenuePosted);
			ProcessLogs();

			Assert("Logs match", NotifiedEventList.Contains(string.Format(CultureInfo.InvariantCulture, "[ARCreditNoteApprovalSubscriber] GenApprovalRequest {0} EDT log received, but cannot process due to constraints on ARCreditNoteApprovalRequest.", Request.PK)));
			Charge = Factory.Load<Charge>(Charge.PK);
			Assert(!Charge.IsRevenuePosted);

			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Rejected;
			Factory.Save();
			Assert(!Charge.IsRevenuePosted);
			NotifiedEventList.Clear();
			ProcessLogs();

			Assert("Logs match", NotifiedEventList.Contains(string.Format("[ARCreditNoteApprovalSubscriber] GenApprovalRequest {0} EDT log received, but cannot process due to constraints on ARCreditNoteApprovalRequest.", Request.PK)));
			Charge = Factory.Load<Charge>(Charge.PK);
			Assert(!Charge.IsRevenuePosted);

			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Posted;
			Factory.Save();
			Assert(!Charge.IsRevenuePosted);
			NotifiedEventList.Clear();
			ProcessLogs();

			Assert("Logs match", NotifiedEventList.Contains(string.Format("[ARCreditNoteApprovalSubscriber] GenApprovalRequest {0} EDT log received, but cannot process due to constraints on ARCreditNoteApprovalRequest.", Request.PK)));
			Charge = Factory.Load<Charge>(Charge.PK);
			Assert(!Charge.IsRevenuePosted);

			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Error;
			Factory.Save();
			Assert(!Charge.IsRevenuePosted);
			NotifiedEventList.Clear();
			ProcessLogs();

			Assert("Logs match", NotifiedEventList.Contains(string.Format("[ARCreditNoteApprovalSubscriber] GenApprovalRequest {0} EDT log received, but cannot process due to constraints on ARCreditNoteApprovalRequest.", Request.PK)));
			Charge = Factory.Load<Charge>(Charge.PK);
			Assert(!Charge.IsRevenuePosted);
		}

		public void TestChargeNotPosed_UnsupportedParentTableCode()
		{
			Charge = TestObjectCreator.CreateCharge(Job, TestObjectCreator.CC1, "Desc 1", TestObjectCreator.AUD, 50M, null, TestObjectCreator.AUD, -100, TestObjectCreator.Debtor);
			TestObjectCreator.Debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
			Charge.JR_AT_SellGSTRate = TestObjectCreator.CC1.AC_AT_GSTRate;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			TestObjectCreator newCreator = new TestObjectCreator(newFactory);
			ARCreditNote creditNote1 = newCreator.CreateARCreditNoteWithLine("001", TestObjectCreator.Debtor, TestObjectCreator.AUD, 1.0m, "Desc", Job, TestObjectCreator.CC1, 100.00m, ZDateTime.Today, false);
			creditNote1.Lines[0].AL_GE = Job.Department.PK;
			creditNote1.AH_TransactionCategory = "FIN";

			Request = Factory.New<ARCreditNoteApprovalRequest>();
			Request.Initialize(new[] { creditNote1 }, Job.PK, "JR", JobInvoicingPostingOption.Revenue);
			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			Factory.Save();

			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			Factory.Save();

			Assert(!Charge.IsRevenuePosted);
			NotifiedEventList.Clear();
			ProcessLogs();

			Assert("Logs match", NotifiedEventList.Contains(string.Format(CultureInfo.InvariantCulture, "[ARCreditNoteApprovalSubscriber] GenApprovalRequest {0} EDT log received, but cannot process due to constraints on ARCreditNoteApprovalRequest.", Request.PK)));
			Charge = Factory.Load<Charge>(Charge.PK);
			Assert(!Charge.IsRevenuePosted);
		}

		public void TestChargeNotPosted_JobWithoutPlugin()
		{
			var branch = TestObjectCreator.CreateNewBranch(GlbCompany.CurrentCompany, "APR");
			Job = TestObjectCreator.CreateJob(TestObjectCreator.Creditor1, 0, TestObjectCreator.Agent, 0);

			var newFactory = new BusinessObjectFactory();
			TestObjectCreator newCreator = new TestObjectCreator(newFactory);
			ARCreditNote creditNote1 = newCreator.CreateARCreditNoteWithLine("001", TestObjectCreator.Debtor, TestObjectCreator.AUD, 1.0m, "Desc", Job, TestObjectCreator.CC1, 100.00m, ZDateTime.Today, false);
			creditNote1.Lines[0].AL_GE = Job.Department.PK;
			creditNote1.AH_TransactionCategory = "FIN";

			Request = Factory.New<ARCreditNoteApprovalRequest>();
			Request.Initialize(new[] { creditNote1 }, Job.PK, "JH", JobInvoicingPostingOption.Revenue);
			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			Factory.Save();

			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			Factory.Save();

			ProcessLogs();
			Assert("Logs match", NotifiedEventList.Contains(string.Format(CultureInfo.InvariantCulture, "[ARCreditNoteApprovalSubscriber] ARCreditNoteApprovalRequest {0} EDT log received, but cannot process due to missing parent form.", Request.PK)));
		}

		public void TestChargeNotPosted_WithoutJob()
		{
			var newFactory = new BusinessObjectFactory();
			var newCreator = new TestObjectCreator(newFactory);

			var invoice = newCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.AUD, 1.0m, TestObjectCreator.Debtor);
			newFactory.Save();

			var creditNote1 = newCreator.CreateARCreditNoteWithLine("001", TestObjectCreator.Debtor, TestObjectCreator.AUD, 1.0m, "Desc", Job, TestObjectCreator.CC1, 100.00m, ZDateTime.Today, false);
			creditNote1.Lines[0].AL_GE = GlbDepartment.CurrentDepartment.PK;
			creditNote1.AH_TransactionCategory = "FIN";

			Request = Factory.New<ARCreditNoteApprovalRequest>();
			Request.Initialize(new[] { creditNote1 }, invoice.PK, "AH", JobInvoicingPostingOption.Revenue);
			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			Request.XP_ReasonCode = "Test";
			Factory.Save();

			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			Factory.Save();

			ProcessLogs();
			Assert("Logs match", NotifiedEventList.Contains(string.Format(CultureInfo.InvariantCulture, "[ARCreditNoteApprovalSubscriber] ARCreditNoteApprovalRequest {0} EDT log received, but cannot process due to missing parent form.", Request.PK)));
		}

		[SuspendCriticalValidation]
		public void TestChargeNotPosted_DuplicateApprovedRequests()
		{
			var invoice = GetInvoiceWithLine();
			Factory.Save();
			var creditNote1 = GetAmendingCreditNote(invoice);

			var newFactory = new BusinessObjectFactory();
			var request1 = newFactory.New<ARCreditNoteApprovalRequest>();
			request1.Initialize(new[] { creditNote1 }, invoice.PK, "AH", JobInvoicingPostingOption.Revenue);
			request1.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			request1.XP_ReasonCode = "Test";
			newFactory.Save();

			request1.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			newFactory.Save();

			var request2 = newFactory.New<ARCreditNoteApprovalRequest>();
			request2.Initialize(new[] { creditNote1 }, invoice.PK, "AH", JobInvoicingPostingOption.Revenue);
			request2.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			request2.XP_ReasonCode = "Test";
			newFactory.Save();

			request2.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			newFactory.Save();

			var transactionsInDB = newFactory.Load<AccTransactionHeader>(new ZQuery());
			Assert("Pre-condition: Expect no AR CRD", !transactionsInDB.Any(x => x.AH_Ledger == LedgerTypes.AccountsReceivable && x.AH_TransactionType == TransactionTypes.CreditNote));
			ProcessLogs();
			transactionsInDB = newFactory.Load<AccTransactionHeader>(new ZQuery());
			Assert("Post-condition: Expect no AR CRD being created", !transactionsInDB.Any(x => x.AH_Ledger == LedgerTypes.AccountsReceivable && x.AH_TransactionType == TransactionTypes.CreditNote));
			Assert("Expect request1 cannot be processed error", NotifiedEventList.Contains(string.Format(CultureInfo.InvariantCulture, "[ARCreditNoteApprovalSubscriber] Could not process Approved ARCreditNoteApprovalRequest {0} for credit note amendment because there are other approved requests exist in database for the same invoice {1}.", request1.PK, invoice.AH_TransactionNum)));
			Assert("Expect request2 cannot be processed error", NotifiedEventList.Contains(string.Format(CultureInfo.InvariantCulture, "[ARCreditNoteApprovalSubscriber] Could not process Approved ARCreditNoteApprovalRequest {0} for credit note amendment because there are other approved requests exist in database for the same invoice {1}.", request2.PK, invoice.AH_TransactionNum)));
		}

		public void TestChargeNotPosted_NegativeChargesAddedOnJobAfterRequestCreated()
		{
			Charge = TestObjectCreator.CreateCharge(Job, TestObjectCreator.CC1, "Desc 1", TestObjectCreator.AUD, 50M, null, TestObjectCreator.AUD, -100, TestObjectCreator.Debtor);
			TestObjectCreator.Debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
			Charge.JR_AT_SellGSTRate = TestObjectCreator.CC1.AC_AT_GSTRate;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			TestObjectCreator newCreator = new TestObjectCreator(newFactory);
			ARCreditNote creditNote1 = newCreator.CreateARCreditNoteWithLine("001", TestObjectCreator.Debtor, TestObjectCreator.AUD, 1.0m, "Desc", Job, TestObjectCreator.CC1, 100.00m, ZDateTime.Today, false);
			creditNote1.Lines[0].AL_GE = Job.Department.PK;
			creditNote1.AH_TransactionCategory = "FIN";

			Request = Factory.New<ARCreditNoteApprovalRequest>();
			Request.Initialize(new[] { creditNote1 }, Job.PK, "JH", JobInvoicingPostingOption.Revenue);
			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			Request.XP_SystemCreateUser = Requestor.GS_Code;
			Factory.Save();

			var charge2 = TestObjectCreator.CreateCharge(Job, TestObjectCreator.CC1, 0, -10000);
			charge2.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
			charge2.JR_AT_SellGSTRate = TestObjectCreator.CC1.AC_AT_GSTRate;
			Factory.Save();

			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			Factory.Save();

			Assert(!Charge.IsRevenuePosted);
			Assert(!charge2.IsRevenuePosted);
			ProcessLogs();

			Charge = Factory.Load<Charge>(Charge.PK);
			Assert(!Charge.IsRevenuePosted);
			charge2 = Factory.Load<Charge>(charge2.PK);
			Assert(!charge2.IsRevenuePosted);

			Request = newFactory.Load<ARCreditNoteApprovalRequest>(Request.PK);
			AssertEquals("Approved", Constants.GenApprovalRequestApprovalStatus.Approved, Request.XP_ApprovalStatus);
			Assert("Logs match", NotifiedEventList.Contains(string.Format(CultureInfo.InvariantCulture, "[ARCreditNoteApprovalSubscriber] Can't post this request because source details have been modified since then.", Request.PK, Job.JH_JobNum, Request.PostingOptionForDisplay)));
		}

		public void TestJobChargeNotPosted_ErrorEmailSent()
		{
			Charge = TestObjectCreator.CreateCharge(Job, TestObjectCreator.CC1, "Desc 1", TestObjectCreator.AUD, 50M, null, TestObjectCreator.AUD, -100, TestObjectCreator.Debtor);
			TestObjectCreator.Debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
			Charge.JR_AT_SellGSTRate = TestObjectCreator.CC1.AC_AT_GSTRate;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			TestObjectCreator newCreator = new TestObjectCreator(newFactory);
			ARCreditNote creditNote1 = newCreator.CreateARCreditNoteWithLine("001", TestObjectCreator.Debtor, TestObjectCreator.AUD, 1.0m, "Desc", Job, TestObjectCreator.CC1, 100.00m, ZDateTime.Today, false);
			creditNote1.Lines[0].AL_GE = Job.Department.PK;
			creditNote1.AH_TransactionCategory = "FIN";

			Request = Factory.New<ARCreditNoteApprovalRequest>();
			Request.Initialize(new[] { creditNote1 }, Job.PK, "JH", JobInvoicingPostingOption.Revenue);
			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			Request.XP_SystemCreateUser = Requestor.GS_Code;
			Factory.Save();

			var charge2 = TestObjectCreator.CreateCharge(Job, TestObjectCreator.CC1, 0, -10000);
			charge2.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
			charge2.JR_AT_SellGSTRate = TestObjectCreator.CC1.AC_AT_GSTRate;
			Factory.Save();

			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;

			Requestor.GS_EmailAddress = "A@B.COM";
			Factory.Save();

			int previousEmailCount = Env.OutgoingMailManager.EmailsCreated.Count;

			Assert(!Charge.IsRevenuePosted);
			Assert(!charge2.IsRevenuePosted);
			ProcessLogs();

			Charge = Factory.Load<Charge>(Charge.PK);
			Assert(!Charge.IsRevenuePosted);
			charge2 = Factory.Load<Charge>(charge2.PK);
			Assert(!charge2.IsRevenuePosted);

			Request = newFactory.Load<ARCreditNoteApprovalRequest>(Request.PK);
			AssertEquals("Approved", Constants.GenApprovalRequestApprovalStatus.Approved, Request.XP_ApprovalStatus);
			Assert("Logs match", NotifiedEventList.Contains(string.Format(CultureInfo.InvariantCulture, "[ARCreditNoteApprovalSubscriber] Can't post this request because source details have been modified since then.", Request.PK, Job.JH_JobNum, Request.PostingOptionForDisplay)));

			AssertEquals("Email count", previousEmailCount + 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated.Last();
			AssertNotNull("email", email);
			AssertEquals("Email Subject", "Error posting approved AR Credit Note - " + Request.JobNumber, email.Subject);

			string expectedBody = GetExpectedErrorBody(Request, string.Format("Can't post this request because source details have been modified since then.", Job.JH_JobNum, Request.PostingOptionForDisplay));
			AssertContains("Email Body", expectedBody, email.Body);
		}

		[TestDate(2019, 03, 07)]
		public void TestWhetherOverrideDatesMethodCorrectsChargeExchangeRateCorrectly()
		{
			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PST");
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 5m, new ZDateTime(2019, 03, 05), new ZDateTime(2019, 03, 31));

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 6m, new ZDateTime(2019, 03, 01), new ZDateTime(2019, 03, 04));

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "SEL", 6m, new ZDateTime(2019, 03, 01), new ZDateTime(2019, 03, 31));

			Factory.Save();

			Job.JH_OA_AgentCollectAddr = TestObjectCreator.Debtor.MainAddress.PK;
			Charge = TestObjectCreator.CreateCharge(Job, TestObjectCreator.CC1, "Desc 1", TestObjectCreator.AUD, 50M, null, TestObjectCreator.USD, -100, TestObjectCreator.Debtor);
			TestObjectCreator.Debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
			Charge.JR_AT_SellGSTRate = TestObjectCreator.CC1.AC_AT_GSTRate;
			Charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.Code;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			TestObjectCreator newCreator = new TestObjectCreator(newFactory);
			ARCreditNote creditNote = newCreator.CreateARCreditNoteWithLine("001", TestObjectCreator.Debtor, TestObjectCreator.USD, 6.0m, "Desc", Job, TestObjectCreator.CC1, 100.00m, ZDateTime.Today, false);
			creditNote.Lines[0].AL_GE = Job.Department.PK;
			creditNote.AH_InvoiceDate = ZDateTime.Today;
			creditNote.AH_PostDate = new ZDateTime(2019, 03, 02);
			creditNote.AH_TransactionCategory = "FIN";

			Request = Factory.New<ARCreditNoteApprovalRequest>();
			Request.Initialize(new[] { creditNote }, Job.PK, "JH", JobInvoicingPostingOption.Revenue);
			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			Request.XP_SystemCreateUser = Requestor.GS_Code;
			Factory.Save();

			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			Factory.Save();

			Assert(!Charge.IsRevenuePosted);
			NotifiedEventList.Clear();
			ProcessLogs();

			Assert(!NotifiedEventList.Contains(string.Format("[ARCreditNoteApprovalSubscriber] Can't post this request because source details have been modified since then.")));
			Charge = newFactory.Load<Charge>(Charge.PK);
			Assert(Charge.IsRevenuePosted);
			Request = newFactory.Load<ARCreditNoteApprovalRequest>(Request.PK);
			AssertEquals("Posted", Constants.GenApprovalRequestApprovalStatus.Posted, Request.XP_ApprovalStatus);
		}

		[SuspendCriticalValidation]
		public void TestAutoPostErrorEmailSend_AmendingWithNoLines()
		{
			int previousEmailCount = Env.OutgoingMailManager.EmailsCreated.Count;
			var invoice = GetInvoiceWithLine();
			var creditNote = GetAmendingCreditNote(invoice);
			creditNote.Lines.RemoveAndDeleteAll();
			CreateAndSaveRequest(creditNote, invoice);

			Requestor.GS_EmailAddress = "A@B.COM";
			Factory.Save();

			ProcessLogs();

			var newFactory = new BusinessObjectFactory();
			var transaction = GetTransactionNotMatchingInvoicePK(newFactory, invoice);

			AssertNull(transaction);
			Request = newFactory.Load<ARCreditNoteApprovalRequest>(Request.PK);
			AssertEquals("Approved", Constants.GenApprovalRequestApprovalStatus.Approved, Request.XP_ApprovalStatus);
			Assert("Logs match", NotifiedEventList.Contains(string.Format("[ARCreditNoteApprovalSubscriber] Could not process Approved ARCreditNoteApprovalRequest {0} for credit note amendment because amended AR Credit Note has no lines.", Request.PK)));

			AssertEquals("Should be 1 emails sent", previousEmailCount + 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var email = Env.OutgoingMailManager.EmailsCreated[Env.OutgoingMailManager.EmailsCreated.Count - 1];
			AssertNotNull("email", email);
			AssertEquals("Email Subject", "Error posting approved AR Credit Note - " + Request.JobNumber, email.Subject);

			string expectedBody = GetExpectedErrorBody(Request, string.Format("Could not process Approved ARCreditNoteApprovalRequest {0} for credit note amendment because amended AR Credit Note has no lines.", Request.PK));
			AssertContains("Email Body", expectedBody, email.Body);
		}

		[SuspendCriticalValidation]
		public void TestCheckLogSubscriberToAbortLogGroupProcessingSilentlyExceptionThrown()
		{
			var invoice = GetInvoiceWithLine();
			var creditNote = GetAmendingCreditNote(invoice);
			CreateAndSaveRequest(creditNote, invoice);

			var mock = new Mock<IQueuedLog>();
			mock.SetupGet(x => x.Factory).Returns(Request.Factory);
			mock.SetupGet(x => x.SJ_ParentID).Returns(Request.PK);
			mock.SetupGet(x => x.SJ_ParentTableCode).Returns(Request.TablePrefix);

			AssertExceptionThrown<LogSubscriberToAbortLogGroupProcessingSilentlyException>("Should throw LogSubscriberToAbortLogGroupProcessingSilentlyException by default unless explicity decided to save Factory", () => SubscriberForTest.ProcessLogQueueItems_ForTestOnly(new[] { mock.Object }));
		}

		[SuspendCriticalValidation]
		public void TestAutoPosting_AuthorizationLogCreated()
		{
			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1M, 700M, 70M, 0M);
			Factory.Save();

			Request = Factory.New<ARCreditNoteApprovalRequest>();
			Request.ChangeApprovalTypeForInvoiceReversal();
			Request.Initialize(new[] { arInvoice }, arInvoice.PK, AccTransactionHeaderSchema.Constants.Prefix, JobInvoicingPostingOption.Revenue);
			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			Request.XP_ReasonCode = Constants.GenApprovalRequestReasonCode.Code.IncorrectRating;
			Request.XP_SystemCreateUser = Requestor.GS_Code;
			Factory.Save();

			Request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			Request.XP_GS_NKApprovingUser1 = Requestor.GS_Code;
			Request.XP_GS_NKApprovingUser2 = Approver.GS_Code;
			Factory.Save();

			ProcessLogs();

			var expectedMessage = FormattableString.Invariant($"[ARCreditNoteApprovalSubscriber] Transaction {arInvoice.AH_TransactionNum} linked to ARCreditNoteApprovalRequest {Request.PK} can be reversed.");
			Assert(NotifiedEventList.Contains(expectedMessage));

			var newFactory = new BusinessObjectFactory();

			var reloadedRequest = newFactory.Load<ARCreditNoteApprovalRequest>(Request.PK);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Posted, reloadedRequest.XP_ApprovalStatus);

			var arCreditNotes = newFactory.Load<ARCreditNote>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, "CRD"));
			AssertEquals(1, arCreditNotes.Length);

			var authLog = arCreditNotes[0].Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AuthorisedCode)).FirstOrDefault();
			AssertNotNull(authLog);
			AssertEquals(Requestor.GS_Code, authLog.SL_GS_NKUser);
			AssertEquals("Post authorized by REQ (Raquel Blue), APP (Apple White)", authLog.ReferenceFreeText);
		}

		[SuspendCriticalValidation]
		[ExpectNoExceptions("The method or operation is not implemented.")]
		public void TestAutoPosting_PostingDetailsChanged()
		{
			Requestor.GS_EmailAddress = "requestor@aCompany.com.au";

			var invoiceWithLine = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 123.16m, 0m, 123.16m, 0m);
			invoiceWithLine.AH_OH = TestObjectCreator.Debtor.PK;
			invoiceWithLine.AH_GB = Job.Branch.PK;
			invoiceWithLine.AH_GE = Job.Department.PK;
			invoiceWithLine.Lines[0].AL_GE = Job.Department.PK;
			invoiceWithLine.AH_TransactionCategory = "FIN";
			invoiceWithLine.AH_JH = Job.PK;
			invoiceWithLine.Lines[0].AL_AC = TestObjectCreator.CC1.PK;
			invoiceWithLine.Lines[0].AL_JH = Job.PK;

			var creditNote = GetAmendingCreditNote(invoiceWithLine);
			CreateAndSaveRequest(creditNote, invoiceWithLine);

			var roundingCollection = new InvoiceTotalRoundingCollection();
			var rounding1 = roundingCollection.AddNew();
			rounding1.Currency = Core.Constants.CurrencyCodes.Australia;
			rounding1.RoundingOption = AccountingConstants.RoundingOptionsCodes.AlwaysRoundUp;
			rounding1.RoundToCurrencyUnit = AccountingConstants.RoundToCurrencyUnits.OneMajorUnit;

			using (AccountingConfigurationRegistry.Instance.InvoiceTotalRounding.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, roundingCollection))
			using (AccountingConfigurationRegistry.Instance.InvoiceTotalRoundingChargeCode.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.NonAccrualChargeCode.PK.ToGuid()))
			{
				var newFactory = new BusinessObjectFactory();
				var mock = new Mock<IQueuedLog>();
				mock.SetupGet(x => x.Factory).Returns(newFactory);
				mock.SetupGet(x => x.SJ_ParentID).Returns(Request.PK);
				mock.SetupGet(x => x.SJ_ParentTableCode).Returns(Request.TablePrefix);

				var logger = new LoggerForTesting { AllowDebug = true };
				SubscriberForTest.SetDefaultLogger(logger);
				var ex = AssertExceptionThrown<LogSubscriberToAbortLogGroupProcessingSilentlyException>(() => SubscriberForTest.ProcessLogQueueItems_ForTestOnly(new[] { mock.Object }));

				var expectedLogMessageFragment = $@"Posting details were changed. Data for comparison follows: {System.Environment.NewLine}<!-- Database (Old) -->{System.Environment.NewLine}<?xml version=""1.0"" encoding=""utf-16""?><PostingRequest>";
				Assert("Service logs should contain the data for approval requests data serialized as XML.", logger.NotifiedEventList.Any(e => e.Contains(expectedLogMessageFragment)));

				string expectedBody = GetExpectedErrorBody(Request, string.Format(@"There is an approved request for this posting action, but data for approval is different. This approved request must be canceled to continue posting.
Approved ARCreditNoteApprovalRequest {0} could not be processed because Level authorization check failed with Approval Request.", Request.PK));
				AssertContains("Email Body", expectedBody, ((TransactionRequestApprovalHtmlEmailDef)ex.Emails[0]).Body);
			}
		}

		ARCreditNoteApprovalSubscriber SubscriberForTest => fSubscriberForTest ?? (fSubscriberForTest = new ARCreditNoteApprovalSubscriber());
		ARCreditNoteApprovalSubscriber fSubscriberForTest;

		string GetExpectedErrorBody(ARCreditNoteApprovalRequest approvalRequest, string errorMessage)
		{
			var email = new ARCreditNoteApprovalRequestEmailExposed(approvalRequest);
			return $@"<p>The following AR Credit Note approval request was approved, but could not be automatically posted:</p>
{email.GetLinkForMoreDetails_ForTest()}
<p>Reason the transaction could not be posted automatically:</p>
<p>{errorMessage}</p>
<p>Please post this credit note manually.</p>";
		}

		class ARCreditNoteApprovalRequestEmailExposed : ARCreditNoteApprovalRequestEmail
		{
			public ARCreditNoteApprovalRequestEmailExposed(ARCreditNoteApprovalRequest approvalRequest)
			: base(approvalRequest)
			{
			}

			public string GetLinkForMoreDetails_ForTest() => base.GetLinkForMoreDetails();
		}

		internal void ProcessLogs()
		{
			RunLogWalkerCycleForTest();
			Assert("No NotImplementedException should be exposed", !NotifiedEventList.Contains("[ARCreditNoteApprovalSubscriber] failed to process logs. Affected records will be processed again one-by-one.\r\nThe method or operation is not implemented."));
		}

		protected override void SetUp()
		{
			AccountingConfigurationRegistry.Instance.PostCreditNoteOnApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AuthorizationModeAndSettings { AuthorizationMode = Constants.AuthorizationMode.Codes.TwoApprovers });
			TestObjectCreator = new TestObjectCreator(Factory);

			Requestor = Factory.NewWithValidTestData<GlbStaff>();
			Requestor.GS_Code = Requestor.GS_LoginName = "REQ";
			Requestor.GS_FullName = "Raquel Blue";

			Approver = Factory.NewWithValidTestData<GlbStaff>();
			Approver.GS_Code = Approver.GS_LoginName = "APP";
			Approver.GS_FullName = "Apple White";
			Factory.Save();

			TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddDays(-10));
			Shipment = TestObjectCreator.CreateShipment("S00001000");
			Job = TestObjectCreator.CreateJob(Shipment, TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);

			var checkpoint = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AmendTransactionWCreditNote);
			checkpoint.IsAllowed = true;
		}

		AuthorizationModeAndSettings GetAuthorisationConfigSetting(string mode = "", bool includeLevelNone = false)
		{
			var result = new AuthorizationModeAndSettings();
			var collection = result.AuthorisationSettings;
			var upToPaymentAuthorisationSettings = collection.AddNew();
			upToPaymentAuthorisationSettings.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			upToPaymentAuthorisationSettings.Amount = 10;
			upToPaymentAuthorisationSettings.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			var abovePaymentAuthorisationSettings = collection.AddNew();
			abovePaymentAuthorisationSettings.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			abovePaymentAuthorisationSettings.Amount = 10;
			abovePaymentAuthorisationSettings.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			if (includeLevelNone)
			{
				upToPaymentAuthorisationSettings = collection.AddNew();
				upToPaymentAuthorisationSettings.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
				upToPaymentAuthorisationSettings.Amount = 5;
				upToPaymentAuthorisationSettings.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			}

			if (!string.IsNullOrWhiteSpace(mode))
			{
				result.AuthorizationMode = mode;
			}

			return result;
		}

		TestObjectCreator TestObjectCreator { get; set; }
		JobCharge Charge { get; set; }
		internal ARCreditNoteApprovalRequest Request { get; set; }
		Job Job { get; set; }
		GlbStaff Requestor { get; set; }
		GlbStaff Approver { get; set; }
		ForwardingShipment Shipment { get; set; }
		internal List<string> NotifiedEventListForTest => base.NotifiedEventList;
		internal BusinessObjectFactory FactoryForTest => Factory;

		internal void SetupForTest()
		{
			SetUp();
		}

		protected override bool IKnowEDTEventsAreUsuallyOnlyLoggedWhenAFormIsPresent
		{
			get { return true; }
		}

		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAP => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP;
		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAR => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR;
	}
}
