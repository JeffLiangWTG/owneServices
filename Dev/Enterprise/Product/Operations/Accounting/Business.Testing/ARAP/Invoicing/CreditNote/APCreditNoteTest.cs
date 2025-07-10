using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(APCreditNote))]
	public class APCreditNoteTest : CreditNoteTest
	{
		public void TestBusinessObjectsWithRelatedEvents()
		{
			var draftInvoice = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
			var creditNote = Factory.NewWithValidTestData<APCreditNote>();

			AssertEquals(0, creditNote.BusinessObjectsWithRelatedEvents.Length);

			draftInvoice.AIH_AH_PostedTransactionHeader = creditNote.PK;
			AssertEquals(1, creditNote.BusinessObjectsWithRelatedEvents.Length);
			AssertCollectionContains(draftInvoice, creditNote.BusinessObjectsWithRelatedEvents);
		}

		public void TestIEdocsParsingSupportProvider()
		{
			var bo = Factory.NewWithValidTestData<ARCreditNote>();
			var eDocsParsingSupport = bo as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		public override void TestShouldShowOriginalInvoiceReferenceFields()
		{
			AssertEquals("ShouldShowOriginalInvoiceReferenceFields should be true.", true, InvoicingBase.ShouldShowOriginalInvoiceReferenceFields);
		}

		public void TestCodeProperty()
		{
			InvoicingBase creditNote = (InvoicingBase)new BusinessObjectFactory().New(GetExpectedBusinessObjectType());
			creditNote.AH_ConsolidatedInvoiceRef = "ABC123";
			creditNote.AH_TransactionNum = "TransNum";
			AssertEquals("Code Property Should be AH_ConsolidatedInvoiceRef because it's unique for AP Trans", creditNote.AH_ConsolidatedInvoiceRef, ((ICodeDescription)creditNote).Code);
		}

		public override void TestDocManagerInfo()
		{
			InvoicingBase invoice = (InvoicingBase)GetNewBusinessObject();
			AssertEquals("DocManagerInfo.GetType()", typeof(APInvoiceDocManagerInfo), invoice.DocManagerInfo.GetType());
		}

		public void TestValidationForIncompleteTransaction()
		{
			var invoice = (InvoicingBase)new BusinessObjectFactory().New(GetExpectedBusinessObjectType());

			invoice.Factory.SetContext(BusinessContext.SavingIncompleteTransaction);
			AssertEquals("Validation Type for Incomplete", typeof(IncompleteInvoicingBaseValidation), invoice.Validation.GetType());

			invoice.Factory.RemoveContext(BusinessContext.SavingIncompleteTransaction);
			AssertNotEquals("Validation Type for Regular", typeof(IncompleteInvoicingBaseValidation), invoice.Validation.GetType());
		}

		public override void TestNumberFountainInternalRef()
		{
			APCreditNote creditnote = Factory.NewWithValidTestData<APCreditNote>();
			ShareSequentialReferenceNumbers item = new ShareSequentialReferenceNumbers();
			item.Value = true;
			AccountingConfigurationRegistry.Instance.ShareSequentialInvoiceReferenceNumbers.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, item);
			AssertEquals("Should be APInvoiceInternalRef number fountain", Env.NumberFountains.APInvoiceInternalRef.GetTodaysPeriodFountain(), creditnote.NumberFountainForInternalRef_ForTestOnly.numberFountainPooler.GetTodaysPeriodFountain());
			AssertNotEquals("Should be APInvoiceInternalRef number fountain", Env.NumberFountains.APCreditNoteInternalRef.GetTodaysPeriodFountain(), creditnote.NumberFountainForInternalRef_ForTestOnly.numberFountainPooler.GetTodaysPeriodFountain());
			item.Value = false;
			AccountingConfigurationRegistry.Instance.ShareSequentialInvoiceReferenceNumbers.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, item);
			AssertNotEquals("Should be APCreditNoteInternalRef number fountain", Env.NumberFountains.APInvoiceInternalRef.GetTodaysPeriodFountain(), creditnote.NumberFountainForInternalRef_ForTestOnly.numberFountainPooler.GetTodaysPeriodFountain());
			AssertEquals("Should be APCreditNoteInternalRef number fountain", Env.NumberFountains.APCreditNoteInternalRef.GetTodaysPeriodFountain(), creditnote.NumberFountainForInternalRef_ForTestOnly.numberFountainPooler.GetTodaysPeriodFountain());
		}

		public void TestOnSavingApprovalLogIsNotAddedWhenThereIsNoRequestAndNoApprovingUser()
		{
			var creditNote = Factory.NewWithValidTestData<APCreditNote>();
			AssertNull("Precondition: no approval request", creditNote.APInvoiceTransactionRelatedApprovalRequest);
			AssertEquals("Precondition: no approving user", ZGuid.Empty, creditNote.ApprovingUserPK);

			Factory.Save();
			var creditNoteApprovedLog = creditNote.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == Events.TransactionApprovalActioned.Code);
			AssertNull("No log should be added as there is no request and no approving user", creditNoteApprovedLog);
		}

		public void TestOnSavingApprovalLogIsAddedForOnlyApprovedRequest()
		{
			//Case - Only Approved -- Assert no log is added

			var creditNote = TestObjectCreator.CreateAPInvoiceForApprovalRequest<APCreditNote>(TestObjectCreator.AALSHI, 100);
			var approvalRequestInOtherFactory = TestObjectCreator.CreateAndSaveApprovalRequestForInvoiceInNewFactoryAsInProduction(creditNote);

			AssertEquals("Precondition: approval request status should be requested", Constants.GenApprovalRequestApprovalStatus.Requested, approvalRequestInOtherFactory.XP_ApprovalStatus);
			var creditNoteApprovedLog = creditNote.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == Events.TransactionApprovalActioned.Code);
			Assert("Precondition: creditnote saved as incomplete", creditNote.IsIncompleteInvoice);
			AssertNull("No log should be added as the request status is not posted", creditNoteApprovedLog);

			approvalRequestInOtherFactory.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			approvalRequestInOtherFactory.Factory.Save();

			creditNoteApprovedLog = creditNote.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == Events.TransactionApprovalActioned.Code);
			AssertNotNull("Log should be added as the request status is approved", creditNoteApprovedLog);
			AssertEquals("IN|INC|Approved For Posting", creditNoteApprovedLog.SL_Reference);
		}

		public void TestOnSavingApprovalLogIsNotAddedForApprovedThenCanceledRequest()
		{
			//Case - CreditNote Approval request approved and then the request is canceled and after that invoice is posted --  Assert no log is added

			var creditNote = TestObjectCreator.CreateAPInvoiceForApprovalRequest<APCreditNote>(TestObjectCreator.AALSHI, 100);
			var approvalRequestInOtherFactory = TestObjectCreator.CreateAndSaveApprovalRequestForInvoiceInNewFactoryAsInProduction(creditNote);

			AssertEquals("Precondition: approval request status should be requested", Constants.GenApprovalRequestApprovalStatus.Requested, approvalRequestInOtherFactory.XP_ApprovalStatus);
			var creditNoteApprovedLog = creditNote.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == Events.TransactionApprovalActioned.Code);
			Assert("Precondition: creditnote saved as incomplete", creditNote.IsIncompleteInvoice);
			AssertNull("No log should be added as the request status is not posted", creditNoteApprovedLog);

			approvalRequestInOtherFactory.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			approvalRequestInOtherFactory.Factory.Save();

			AssertEquals("Precondition: Credit Note Factory request status is approved", Constants.GenApprovalRequestApprovalStatus.Approved, creditNote.APInvoiceTransactionRelatedApprovalRequest.XP_ApprovalStatus);
			creditNoteApprovedLog = creditNote.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == Events.TransactionApprovalActioned.Code);
			AssertNotNull("Log should be added as the request status is approved", creditNoteApprovedLog);
			AssertEquals("IN|INC|Approved For Posting", creditNoteApprovedLog.SL_Reference);

			approvalRequestInOtherFactory.Factory.RefreshEnabled = false;
			approvalRequestInOtherFactory.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Cancelled;
			approvalRequestInOtherFactory.SetContext(BusinessContext.CancelApprovalRequestByUser);
			approvalRequestInOtherFactory.Factory.Save();

			AssertEquals("Precondition: Credit Note Factory request status is still approved", Constants.GenApprovalRequestApprovalStatus.Approved, creditNote.APInvoiceTransactionRelatedApprovalRequest.XP_ApprovalStatus);
			creditNote.MoveFromIncompleteToPayableLedger();
			Factory.Save();

			AssertEquals("creditnote should be posted", true, creditNote.IsPosted);
			creditNoteApprovedLog = creditNote.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == Events.TransactionApprovalActioned.Code).Skip(1).FirstOrDefault();
			AssertNull("No log should be added as the request status is not posted even if creditnote is posted", creditNoteApprovedLog);
		}

		[TestDate(2016, 11, 09)]
		public void TestOnSavingApprovalLogIsAddedForInvoiceWithApprovingUserPK()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ABC";
			Factory.Save();

			var creditNoteWithoutApprovalRequest = Factory.NewWithValidTestData<APCreditNote>();
			creditNoteWithoutApprovalRequest.ApprovingUserPK = staff.PK;
			Factory.Save();

			var creditNoteApprovedLog = creditNoteWithoutApprovalRequest.Logs.GetAllLogs().FirstOrDefault(x => ((StmALog)x).SL_SE_NKEvent == Events.TransactionApprovalActioned.Code) as StmALog;
			AssertNotNull("Credit Note approved log should be added", creditNoteApprovedLog);
			AssertEquals("Approving Reference", "AP|CRD|Approved and Posted", creditNoteApprovedLog.SL_Reference);
			AssertEquals("Approving DateTime", ZDateTime.Today.Date, creditNoteApprovedLog.SL_EventTime.Date);
			AssertEquals("Approving user", "ABC", creditNoteApprovedLog.SL_GS_NKUser);
		}

		[TestDate(2017, 2, 2)]
		public void TestLinesAndHeaderCopiedToTheAmendingTransaction()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001017");
			Job job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "AP001", TestObjectCreator.USD, 1.35M, TestObjectCreator.AALSHI);
			apInvoice.FillWithValidTestData();
			apInvoice.IsSelfBillingInvoice = true;
			apInvoice.UseJobExchangeRate = true;
			apInvoice.AH_ComplianceSubType = "TXI";

			var line1 = TestObjectCreator.CreateInvoiceLine(apInvoice, TestObjectCreator.GLHeader1.PK, 100M, apInvoice.TransactionCurrency, 0.9m);
			line1.AL_Sequence = 1;
			line1.AL_InputGSTVATRecoverable = 0.25m;
			line1.AL_AT = TaxRate.PK;
			line1.AL_TaxDate = new ZDate(2016, 12, 12);

			var line2 = TestObjectCreator.CreateInvoiceLine(apInvoice, TestObjectCreator.GLHeader2.PK, 300M, apInvoice.TransactionCurrency, 0.95m);
			line2.AL_Sequence = 3;
			line2.AL_InputGSTVATRecoverable = 0.1m;
			line2.AL_AT = TaxRate.PK;
			line2.AL_TaxDate = new ZDate(2017, 05, 05);

			var line3 = TestObjectCreator.CreateInvoiceLine(apInvoice, job, TestObjectCreator.FRT, 2500M, apInvoice.TransactionCurrency, 0.95m);
			line3.AL_Sequence = 2;
			line3.AL_InputGSTVATRecoverable = 0.1m;
			line3.AL_AT = TaxRate.PK;
			line3.AL_TaxDate = new ZDate(2017, 05, 05);

			apInvoice.SubmittedFromInvoicingForm = true;

			Factory.Save();

			var creditNote = Factory.New<APCreditNote>();
			creditNote.SubmittedFromInvoicingForm = true;

			AssertNotEquals(creditNote.AH_OH, TestObjectCreator.AALSHI.PK);
			AssertNotEquals(creditNote.AH_RX_NKTransactionCurrency, apInvoice.AH_RX_NKTransactionCurrency);
			AssertNotEquals(creditNote.IsSelfBillingInvoice, apInvoice.IsSelfBillingInvoice);
			AssertNotEquals(creditNote.UseJobExchangeRate, apInvoice.UseJobExchangeRate);
			AssertNotEquals(creditNote.AH_ComplianceSubType, apInvoice.AH_ComplianceSubType);

			creditNote.OriginalTransactionReference = apInvoice.PK;

			AssertEquals(creditNote.AH_OH, TestObjectCreator.AALSHI.PK);
			AssertEquals(creditNote.AH_RX_NKTransactionCurrency, apInvoice.AH_RX_NKTransactionCurrency);
			AssertEquals(creditNote.IsSelfBillingInvoice, apInvoice.IsSelfBillingInvoice);
			AssertEquals(creditNote.UseJobExchangeRate, apInvoice.UseJobExchangeRate);
			AssertNotEquals(creditNote.AH_ComplianceSubType, apInvoice.AH_ComplianceSubType);

			var expected = new[]
			{
					((ZShort)1, "BNE", "BRN", "USD", 100m, 0.90m, 25m, "12-Dec-16", ""),
					((ZShort)3, "BNE", "BRN", "USD", 300m, 0.95m, 10m, "05-May-17", ""),
					((ZShort)2, "BNE", "BRN", "USD", 2500m, 0.95m, 10m, "05-May-17", "S00001017"),
				};

			AssertContainsExactElementsInAnyOrder(expected, creditNote.Lines.Cast<InvoicingLineBase>().Select(x => (
				x.AL_Sequence,
				x.Branch.GB_Code.ToString(),
				x.Department.GE_Code.ToString(),
				x.AL_RX_NKTransactionCurrency.ToString(),
				Math.Round(x.AL_OSExTaxAmount, 2),
				Math.Round(x.AL_ExchangeRate, 2),
				Math.Round(x.AL_Calc_InputGSTVATRecoverablePercentage, 2),
				x.AL_TaxDate.ToShortDateString(),
				(string)(x.Job?.JH_JobNum ?? string.Empty))).ToArray());

			creditNote.Delete();
			AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TOD");
			TestObjectCreator.CreateExchangeRate(apInvoice.TransactionCurrency, "BUY", 0.71m, ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10));
			Factory.Save();

			creditNote = Factory.New<APCreditNote>();
			creditNote.SubmittedFromInvoicingForm = true;

			AssertNotEquals(creditNote.AH_OH, TestObjectCreator.AALSHI.PK);
			AssertNotEquals(creditNote.AH_RX_NKTransactionCurrency, apInvoice.AH_RX_NKTransactionCurrency);
			AssertNotEquals(creditNote.IsSelfBillingInvoice, apInvoice.IsSelfBillingInvoice);
			AssertNotEquals(creditNote.UseJobExchangeRate, apInvoice.UseJobExchangeRate);
			AssertNotEquals(creditNote.AH_ComplianceSubType, apInvoice.AH_ComplianceSubType);

			creditNote.OriginalTransactionReference = apInvoice.PK;

			AssertEquals(creditNote.AH_OH, TestObjectCreator.AALSHI.PK);
			AssertEquals(creditNote.AH_RX_NKTransactionCurrency, apInvoice.AH_RX_NKTransactionCurrency);
			AssertEquals(creditNote.IsSelfBillingInvoice, apInvoice.IsSelfBillingInvoice);
			AssertEquals(creditNote.UseJobExchangeRate, apInvoice.UseJobExchangeRate);
			AssertNotEquals(creditNote.AH_ComplianceSubType, apInvoice.AH_ComplianceSubType);

			expected = new[]
			{
					((ZShort)1, "BNE", "BRN", "USD", 100m, 0.71m, 25m, "12-Dec-16", ""),
					((ZShort)3, "BNE", "BRN", "USD", 300m, 0.71m, 10m, "05-May-17", ""),
					((ZShort)2, "BNE", "BRN", "USD", 2500m, 0.71m, 10m, "05-May-17", "S00001017"),
				};

			AssertContainsExactElementsInAnyOrder(expected, creditNote.Lines.Cast<InvoicingLineBase>().Select(x => (
				x.AL_Sequence,
				x.Branch.GB_Code.ToString(),
				x.Department.GE_Code.ToString(),
				x.AL_RX_NKTransactionCurrency.ToString(),
				Math.Round(x.AL_OSExTaxAmount, 2),
				Math.Round(x.AL_ExchangeRate, 2),
				Math.Round(x.AL_Calc_InputGSTVATRecoverablePercentage, 2),
				x.AL_TaxDate.ToShortDateString(),
				(string)(x.Job?.JH_JobNum ?? string.Empty))).ToArray());

			Factory.Save();
			var factory = new BusinessObjectFactory();
			job = new Job.Loader(factory, shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			AssertEquals(2, job.Charges.Count);
		}

		public void TestInvoiceHeaderBranchAndDepartmentCopiedToTheAmendingTransaction()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001017");
			Job job = TestObjectCreator.CreateJob(shipment, false);
			var branch = TestObjectCreator.CreateBranch("BCH", GlbCompany.CurrentCompany);
			var department = TestObjectCreator.CreateDepartment("DEP");
			Factory.Save();

			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "AP001", TestObjectCreator.USD, 1.35M, TestObjectCreator.AALSHI);
			apInvoice.FillWithValidTestData();
			apInvoice.AH_GB = branch.PK;
			apInvoice.AH_GE = department.PK;
			apInvoice.SubmittedFromInvoicingForm = true;

			Factory.Save();

			var creditNote = Factory.New<APCreditNote>();
			creditNote.SubmittedFromInvoicingForm = true;
			AssertNotEquals(creditNote.AH_GB, apInvoice.AH_GB);
			AssertNotEquals(creditNote.AH_GE, apInvoice.AH_GE);

			creditNote.OriginalTransactionReference = apInvoice.PK;

			AssertEquals(creditNote.AH_GB, apInvoice.AH_GB);
			AssertEquals(creditNote.AH_GE, apInvoice.AH_GE);
		}

		public void TestOnSavingApprovalLogIsAddedForPostedRequestInAnotherFactory()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ABC";
			Factory.Save();

			var creditNoteWithApprovalRequest = TestObjectCreator.CreateAPInvoiceForApprovalRequest<APCreditNote>(TestObjectCreator.AALSHI, 100);
			var approvalRequestInOtherFactory = TestObjectCreator.CreateAndSaveApprovalRequestForInvoiceInNewFactoryAsInProduction(creditNoteWithApprovalRequest);

			AssertEquals("Precondition: approval request status should be requested", Constants.GenApprovalRequestApprovalStatus.Requested, approvalRequestInOtherFactory.XP_ApprovalStatus);
			var creditNoteApprovedLog = creditNoteWithApprovalRequest.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == Events.TransactionApprovalActioned.Code);
			Assert("Precondition: Invoice saved as incomplete", creditNoteWithApprovalRequest.IsIncompleteInvoice);
			AssertNull("No log should be added as the request status is not posted", creditNoteApprovedLog);

			approvalRequestInOtherFactory.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			approvalRequestInOtherFactory.XP_GS_NKApprovingUser1 = staff.GS_Code;
			approvalRequestInOtherFactory.XP_ApprovalDate = new ZDateTime(2016, 3, 15);
			approvalRequestInOtherFactory.Factory.Save();

			AssertEquals("Precondition: Credit Note Factory request status is approved", Constants.GenApprovalRequestApprovalStatus.Approved, creditNoteWithApprovalRequest.APInvoiceTransactionRelatedApprovalRequest.XP_ApprovalStatus);
			creditNoteApprovedLog = creditNoteWithApprovalRequest.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == Events.TransactionApprovalActioned.Code);
			AssertNotNull("Log should be added as the request status is approved", creditNoteApprovedLog);
			AssertEquals("IN|INC|Approved For Posting", creditNoteApprovedLog.SL_Reference);

			approvalRequestInOtherFactory.Factory.RefreshEnabled = false;
			approvalRequestInOtherFactory.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Posted;
			approvalRequestInOtherFactory.Factory.Save();

			AssertEquals("Precondition: Credit Note Factory request status is still approved", Constants.GenApprovalRequestApprovalStatus.Approved, creditNoteWithApprovalRequest.APInvoiceTransactionRelatedApprovalRequest.XP_ApprovalStatus);
			creditNoteWithApprovalRequest.MoveFromIncompleteToPayableLedger();
			Factory.Save();

			creditNoteApprovedLog = creditNoteWithApprovalRequest.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == Events.TransactionApprovalActioned.Code).Skip(1).FirstOrDefault();
			AssertNotNull("Credit Note approved log should be added", creditNoteApprovedLog);
			AssertEquals("Approving Reference", "AP|CRD|Approved and Posted", creditNoteApprovedLog.SL_Reference);
			AssertEquals("Approving DateTime", new ZDateTime(2016, 3, 15), creditNoteApprovedLog.SL_EventTime);
			AssertEquals("Approving user", "ABC", creditNoteApprovedLog.SL_GS_NKUser);
		}

		public void TestOnSavingApprovalLogIsAddedForPostedRequestInSameFactory()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ABC";
			Factory.Save();

			var creditNoteWithApprovalRequest = TestObjectCreator.CreateAPInvoiceForApprovalRequest<APCreditNote>(TestObjectCreator.AALSHI, 100);
			var approvalRequestInOtherFactory = TestObjectCreator.CreateAndSaveApprovalRequestForInvoiceInNewFactoryAsInProduction(creditNoteWithApprovalRequest);
			var requestInCurrentFactory = Factory.Load<APInvoiceChargesApprovalRequest>(approvalRequestInOtherFactory.PK);

			AssertEquals("Precondition: approval request status should be requested", Constants.GenApprovalRequestApprovalStatus.Requested, approvalRequestInOtherFactory.XP_ApprovalStatus);
			var creditNoteApprovedLog = creditNoteWithApprovalRequest.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == Events.TransactionApprovalActioned.Code);
			Assert("Precondition: Credit Note saved as incomplete", creditNoteWithApprovalRequest.IsIncompleteInvoice);
			AssertNull("No log should be added as the request status is not posted", creditNoteApprovedLog);

			approvalRequestInOtherFactory.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			approvalRequestInOtherFactory.XP_GS_NKApprovingUser1 = staff.GS_Code;
			approvalRequestInOtherFactory.XP_ApprovalDate = new ZDateTime(2016, 3, 15);
			approvalRequestInOtherFactory.Factory.Save();

			AssertEquals("Precondition: Credit Note Factory request status is approved", Constants.GenApprovalRequestApprovalStatus.Approved, creditNoteWithApprovalRequest.APInvoiceTransactionRelatedApprovalRequest.XP_ApprovalStatus);
			creditNoteApprovedLog = creditNoteWithApprovalRequest.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == Events.TransactionApprovalActioned.Code);
			AssertNotNull("Log should be added as the request status is approved", creditNoteApprovedLog);
			AssertEquals("IN|INC|Approved For Posting", creditNoteApprovedLog.SL_Reference);

			approvalRequestInOtherFactory.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Posted;
			approvalRequestInOtherFactory.Factory.Save();

			AssertEquals("Precondition: Credit Note Factory request status is posted", Constants.GenApprovalRequestApprovalStatus.Posted, creditNoteWithApprovalRequest.APInvoiceTransactionRelatedApprovalRequest.XP_ApprovalStatus);
			creditNoteWithApprovalRequest.MoveFromIncompleteToPayableLedger();
			Factory.Save();

			creditNoteApprovedLog = creditNoteWithApprovalRequest.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == Events.TransactionApprovalActioned.Code).Skip(1).FirstOrDefault();
			AssertNotNull("Credit Note approved log should be added", creditNoteApprovedLog);
			AssertEquals("Approving Reference", "AP|CRD|Approved and Posted", creditNoteApprovedLog.SL_Reference);
			AssertEquals("Approving DateTime", new ZDateTime(2016, 3, 15), creditNoteApprovedLog.SL_EventTime);
			AssertEquals("Approving user", "ABC", creditNoteApprovedLog.SL_GS_NKUser);
		}

		protected override bool ShouldSupportCalculatingTaxAtHeaderLevel
		{
			get { return false; }
		}
		public override void TestTransactionNumberOnSave()
		{
			AssertAPItemsRetainUserSetTransactionNum();
		}

		protected APCreditNote APCreditNote
		{
			get
			{
				return (APCreditNote)Header;
			}
		}

		public override void TestCreditLimitExceededEmailWillBeSentOnlyOnce()
		{
			Assert("AR test", true);
		}

		protected override Type TypeOfValidation
		{
			get { return typeof(APCreditNoteValidation); }
		}

		protected override Type GetExpectedBusinessObjectLineType()
		{
			return typeof(APCreditNoteLine);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var invoiceBase = Factory.New(GetExpectedBusinessObjectType());
			TestObjectCreator.FillInvoiceWithMinimumTestData((InvoicingBase)invoiceBase);
			return invoiceBase;
		}

		public override void TestSetDefaultValues()
		{
			base.TestSetDefaultValues();

			AccountingConfigurationRegistry.Instance.UseJobExchangeRateDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			APCreditNote creditNote1 = Factory.New<APCreditNote>();

			Assert("AH_PostedToEFT must default to false due to local currency.", !creditNote1.AH_PostedToEFT);

			AccountingConfigurationRegistry.Instance.UseJobExchangeRateDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			APCreditNote creditNote2 = Factory.New<APCreditNote>();

			Assert("AH_PostedToEFT must default to false due to local currency.", !creditNote2.AH_PostedToEFT);
		}

		public void TestTransactionReferenceDetails()
		{
			// Let's have original Reference
			SetupForSave(); //Lines are already added in setup
			InvoicingBase.AH_RX_NKTransactionCurrency = new TestObjectCreator(Factory).GBP.RX_Code;
			InvoicingBase.AH_ExchangeRate = 0.78m;

			InvoicingBase.Lines[0].AL_OSTaxAmount = 10m;
			InvoicingBase.Lines[0].AL_OSExTaxAmount = 140m;

			InvoicingBase.Lines[1].AL_OSTaxAmount = 5m;
			InvoicingBase.Lines[1].AL_OSExTaxAmount = 100m;

			Factory.Save();

			var newInvoicingBase = Factory.New(GetExpectedBusinessObjectType()) as APCreditNote;
			newInvoicingBase.OriginalTransactionReference = InvoicingBase.PK;

			AssertEquals("Lines should be copied for AP Credit Notes", 2, newInvoicingBase.Lines.Count);

			AssertEquals("GBP", newInvoicingBase.AH_RX_NKTransactionCurrency);
			AssertEquals(0.78m, newInvoicingBase.AH_ExchangeRate);
			AssertEquals(-240m, newInvoicingBase.AH_OSTotal);
			AssertEquals(0m, newInvoicingBase.AH_OSTaxAmount);
			AssertEquals(-307.70m, newInvoicingBase.AH_InvoiceAmount);
			AssertEquals(0m, newInvoicingBase.AH_GSTAmount);
		}

		protected override void AssertEmailSendStatusForUnpostedInvoice()
		{
			AssertEquals("No email should be sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public override void TestBusinessContext()
		{
			AssertEquals("BusinessContext should be APInvoice", CargoWise.Definitions.BusinessContext.APInvoice, InvoicingBase.DocumentSupporter.BusinessContext);
		}

		public void TestDocManagerCode()
		{
			AssertEquals("Wrong DocManagerCode. Any change to the IDocManagerSupport interface must also be changed in document scanning lookup", "PCR", ((IDocManagerSupport)GetNewBusinessObject()).DocManagerInfo.DocManagerCode);
		}

		public void TestTaxRegistrationSubTypeForMexicoXCLCreditNote()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Mexico))
			{
				APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();

				InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
				line.AL_AT = TestObjectCreator.ExcludedTax.PK;
				line.AL_AG = TestObjectCreator.GLHeader1.PK;

				APCreditNote creditNote = Factory.NewWithValidTestData<APCreditNote>();
				APCreditNoteLine creditline = (APCreditNoteLine)creditNote.Lines.AddNew();
				creditline.AL_AT = TestObjectCreator.ExcludedTax.PK;
				creditline.AL_AG = TestObjectCreator.GLHeader1.PK;
				creditNote.AH_TransactionBelongsToGroup = invoice.PK;
				creditNote.OriginalTransaction = invoice;
				AssertEquals("Precondition: AH_ComplianceSubType should be empty for creditNote", string.Empty, creditNote.AH_ComplianceSubType);

				Factory.Save();
				AssertEquals("AH_ComplianceSubType should be XCL, for credit note", MexicoComplianceInfo.ComplianceSubTypeCodes.XCL, creditNote.AH_ComplianceSubType);
			}
		}

		public void TestTaxRegistrationSubTypeForMexicoTCRCreditNote()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Mexico))
			{
				APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
				InvoicingLineBase invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
				invoiceLine.AL_AT = TaxRate2.PK;
				invoiceLine.AL_AG = TestObjectCreator.GLHeader1.PK;

				APCreditNote creditNote1 = Factory.NewWithValidTestData<APCreditNote>();
				APCreditNoteLine creditNoteLine1 = (APCreditNoteLine)creditNote1.Lines.AddNew();
				creditNoteLine1.AL_AT = TaxRate2.PK;
				creditNoteLine1.AL_AG = TestObjectCreator.GLHeader1.PK;
				creditNote1.AH_TransactionBelongsToGroup = invoice.PK;
				creditNote1.OriginalTransaction = invoice;
				AssertEquals("Precondition: AH_ComplianceSubType should be empty for creditNote1", string.Empty, creditNote1.AH_ComplianceSubType);

				Factory.Save();
				AssertEquals("AH_ComplianceSubType should be TCR, for credit note", MexicoComplianceInfo.ComplianceSubTypeCodes.TCR, creditNote1.AH_ComplianceSubType);

				APCreditNote creditNote2 = Factory.NewWithValidTestData<APCreditNote>();
				APCreditNoteLine creditNoteLine2 = (APCreditNoteLine)creditNote2.Lines.AddNew();
				creditNoteLine2.AL_AT = TaxRate2.PK;
				creditNoteLine2.AL_AG = TestObjectCreator.GLHeader1.PK;
				AssertEquals("Precondition: AH_ComplianceSubType should be empty for creditNote2", string.Empty, creditNote2.AH_ComplianceSubType);

				Factory.Save();
				AssertEquals("AH_ComplianceSubType should be TCR, for credit note", MexicoComplianceInfo.ComplianceSubTypeCodes.TCR, creditNote2.AH_ComplianceSubType);
			}
		}

		protected override ZString ExpectedTransactionTypeForIncomplete
		{
			get { return TransactionTypes.IncompleteCreditNote; }
		}

		protected override BooleanRegistryItem EnforcePostingAtFixedPlaceOfSupplyLevelForTransactionRuleRegistry => AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions;

		public class APCreditNoteApportionmentTest : APApportionmentTest
		{
			protected override InvoicingBase GetInvoiceBase(BusinessObjectFactory factory)
			{
				return factory.New<APCreditNote>();
			}

			protected override InvoicingLineBase GetInvoiceLineBase(BusinessObjectFactory factory)
			{
				return factory.New<APCreditNoteLine>();
			}
		}

		protected override bool ShouldExpectTaxTotal => true;

		protected override bool CouldHaveAssociatedDraftInvoice => true;
	}
}
