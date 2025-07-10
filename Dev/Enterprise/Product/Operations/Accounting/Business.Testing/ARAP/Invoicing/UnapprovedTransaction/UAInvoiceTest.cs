using System;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(UAInvoice))]
	public class UAInvoiceTest : APInvoiceTestForReceiptPayment
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<UAInvoice>();
		}

		protected override Type GetExpectedBusinessObjectLineType()
		{
			return typeof(UAInvoiceLine);
		}

		public override void TestCopiedFromPK()
		{
			Assert("Tax Transactions are not applicable to unapproved invoices", true);
		}

		public override void TestCalculatePaidOutstandingAmountInSpecificCurrencyOnly()
		{
			Assert("Matching is not applicable to unapproved invoices", true);
		}

		public override void TestIsAllPaidInTheSameCurrency()
		{
			Assert("Matching is not applicable to unapproved invoices", true);
		}

		public override void TestBusinessContext()
		{
			AssertEquals("BusinessContext should be APInvoice", BusinessContext.APInvoice, InvoicingBase.DocumentSupporter.BusinessContext);
		}

		public void TestDescription()
		{
			SetupForSave();
			AssertEquals("Description should be 'Unapproved Credit Note'", "Unapproved INVOICE".ToUpper(), Header.AH_Desc);
			AssertEquals("Number Of Supporting Documents should be 1", 1, Header.AH_NumberOfSupportingDocuments.ToZInt());
		}
		public override void TestTransactionNumberOnSave()
		{
			SetupForSave();
			var defaultTransactionNum = "TransNum";
			Header.AH_TransactionNum = defaultTransactionNum;
			AssertEquals("should have default transaction number on instantiation", defaultTransactionNum, Header.AH_TransactionNum);
			Factory.Save();
			AssertEquals("Should not get transaction number from number fountain", defaultTransactionNum, Header.AH_TransactionNum);
		}

		public override void TestCallingOnSavingTwiceDoesntCreateTwoPayments()
		{
			Assert("Cash Payments not applicable to unapproved invoices", true);
		}

		public void TestAuthorisationLevels()
		{
			var creditNote = Factory.New<UAInvoice>();
			AssertEquals("Does Not Apply", creditNote.MaxAuthorisationLevel);
			AssertEquals("Not Defined", creditNote.AuthorisationLevel);
		}

		public override void TestCashBasisVATForCashInvoice()
		{
			Assert("Cash Payments not applicable to unapproved invoices", true);
		}

		public override void TestCashPaymentReceiptDetailsSetOnSaving()
		{
			Assert("Cash Payments not applicable to unapproved invoices", true);
		}

		public void TestDontSetCashInvoiceForUnApprovedInvoices()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_APPayInvoiceAfterPostingDefault = true;

			UAInvoice invoice = Factory.New<UAInvoice>();
			invoice.AH_OH = org.PK;
			Assert("Cash Payments can never be made from unapproved invoices", !invoice.IsInvoiceReceiptPayment);

			org.CompanyData.OB_APPayInvoiceAfterPostingDefault = false;
			invoice.AH_OH = org.PK;
			Assert("Cash Payments can never be made from unapproved invoices", !invoice.IsInvoiceReceiptPayment);
		}

		public override void TestGenerateReverseTransaction()
		{
			InvoicingBase.FillWithValidTestData();
			var line = TestObjectCreator.CreateInvoiceLine(InvoicingBase, TestObjectCreator.Job1, TestObjectCreator.CC1, 100);
			Charge charge = TestObjectCreator.CreateCharge(line);
			Factory.Save();

			InvoicingBase.GenerateReverseTransaction(true);
			Assert("Should be canceled.", InvoicingBase.AH_IsCancelled);
			Assert("IsReversing", InvoicingBase.IsReversing);
			Assert("IsReverseTransaction", InvoicingBase.IsReverseTransaction);
			Assert("Transformer should not be run on this stage. So line is still linked to a charge.", charge.APLine != null && charge.APLine.AL_LineType == ZArchitecture.Core.TransactionLineTypes.UnapprovedCost);

			InvoicingBase.Delete();
			Factory.Save();
			Assert("Should be canceled.", InvoicingBase.AH_IsCancelled);
			Assert("Transformer has run. So line is unlinked. Accrual was created.", charge.APLine != null && charge.APLine.AL_LineType == ZArchitecture.Core.TransactionLineTypes.Accrual);
		}

		public void TestGenerateReverseTransactionUsesEmptyLineValidation()
		{
			InvoicingBase.FillWithValidTestData();
			InvoicingLineBase invLine = InvoicingBase.Lines.AddNew() as InvoicingLineBase;
			invLine.FillWithValidTestData();
			Charge charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_OSCostAmt = 100;
			charge.JR_AL_APLine = invLine.PK;

			InvoicingBase.GenerateReverseTransaction(true);
			InvoicingBase.RunPreSaveValidation();
			InvoicingBase.Validation.ValidateAll();
			var validationClass = InvoicingBase.Lines[0].Validation;
			AssertEquals("Check validation class is", true, validationClass is TransactionLineEmptyValidation);
		}

		protected override void CancelTransactionHeaderToBeAbleToSave(TransactionHeader header)
		{
			base.CancelTransactionHeaderToBeAbleToSave(header);
			header.Delete();
		}

		public override void TestGenerateReverseTransaction_CopyCharges()
		{
			Assert("This test is not applied to this class.", true);
		}

		public new void TestGenerateReverseTransactionWithDifferentCurrencyLines()
		{
			Assert("This test is not applied to this class.", true);
		}

		public new void TestAPItemsRetainUserSetTransactionNumWhenReversing()
		{
			Header.IsReverseTransaction = true;
			AssertAPItemsRetainUserSetTransactionNum();
		}

		public new void TestSendEmailWhenCreditLimitExceeded()
		{
			GlbStaff currentUserInCurrentFactory = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentUserInCurrentFactory.GS_EmailAddress = "A@B.COM";
			Factory.Save();

			GlbGroup group = Factory.New<GlbGroup>();
			group.Staff.Add(currentUserInCurrentFactory);

			AccountingConfigurationRegistry.Instance.DebtorCreditLimitNotifyGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			UAInvoice invoice = Factory.New<UAInvoice>();
			var line = (UAInvoiceLine)invoice.Lines.AddNew();
			line.AL_AC = TestObjectCreator.NonAccrualChargeCode.PK;
			line.AL_LocalExTaxAmount = 2000m;
			TestObjectCreator.AALSHI.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			TestObjectCreator.AALSHI.CompanyData.OB_APPaymentTerms = "INV";
			TestObjectCreator.AALSHI.CompanyData.OB_APCreditLimit = 200m;
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			invoice.SubmittedFromInvoicingForm = true;
			invoice.AH_TransactionNum = "10001005";

			Factory.Save();

			AssertEquals("Should be 0 email sent for credit limit exceeded", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestImportingChargesDoesntAttemptToModifyApportionedLine()
		{
			var factory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(factory);
			var consol = creator.CreateConsol("AUSYD", "NZAKL", "C001");
			var shipment = creator.CreateShipment("S0001", consol);
			factory.Save();

			var invoice = (UAInvoice)factory.NewWithValidTestData(GetExpectedBusinessObjectType());
			invoice.SubmittedFromInvoicingForm = true;
			JobConsolCost cost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			cost.E6_AC_ChargeCode = creator.CC1.PK;
			cost.E6_OSCostAmount = 300m;
			invoice.ImportAllApportionmentsFromCosting();
			factory.Save();

			InvoicingBase converted = new UnapprovedTransactionConverter(factory).ConvertToAP(invoice, false);
			try
			{
				cost = converted.Factory.Load<JobConsolCost>(cost.PK);
				cost.ApportionmentCharges[0].JR_OSCostAmt = 100M;

				var line = converted.Lines[0];
				AssertEquals("Precondition: AL_OSExTaxAmount must be equal JR_OSCostAmt", 300m, line.AL_OSExTaxAmount);
				AssertEquals("Precondition: ApportionmentChargeImportedFrom", cost.ApportionmentCharges[0], line.ApportionmentChargeImportedFrom);
				converted.ImportSingleCost(cost, null);
				AssertEquals("AL_OSExTaxAmount must be equal JR_OSCostAmt", 100m, line.AL_OSExTaxAmount);
			}
			finally
			{
				converted.ClearApportionmentJobMutexes();
			}
		}

		public void TestTransactionReferenceNotSetOnSaving()
		{
			UAInvoice invoice = Factory.NewWithValidTestData<UAInvoice>();
			Factory.Save();
			AssertEquals(ZString.Empty, invoice.AH_TransactionReference);
		}

		public void TestDelete()
		{
			var invoice = Factory.NewWithValidTestData<UAInvoice>();
			invoice.Lines.AddNew();
			invoice.Lines.AddNew();
			invoice.Delete();

			Assert("Should have been deleted", invoice.IsDeleted);
		}

		public void TestRejectAndSave()
		{
			try
			{
				var newFactory = Factory.CreateNewFactory();
				var transactionCreator = new TransactionCreator();

				var invoice = transactionCreator.CreateTransaction(newFactory, LedgerTypes.UnapprovedPayableTransactions, TransactionTypes.UAInvoice);
				newFactory.Save();
				invoice.AH_TransactionNum += "RJ";
				AssertEquals("Precondition", true, invoice.AH_TransactionNumInfo.HasChanges);
				AssertEquals(false, invoice.AH_IsCancelled);
				AssertEquals(true, invoice.IsInDatabase);

				var ex = AssertExceptionThrown<OnSavingCriticalCheckException<AccTransactionHeader>>("Should have error", () => newFactory.Save());
				AssertContains("The transaction number was modified after being saved.", ex.DeveloperErrorMessage);

				newFactory = Factory.CreateNewFactory();
				invoice = transactionCreator.CreateTransaction(newFactory, LedgerTypes.UnapprovedPayableTransactions, TransactionTypes.UAInvoice);
				newFactory.Save();
				invoice.Delete();
				AssertEquals("Precondition", true, invoice.AH_TransactionNumInfo.HasChanges);
				AssertEquals(true, invoice.AH_IsCancelled);
				AssertEquals(true, invoice.IsInDatabase);

				AssertNoExceptionThrown("Reject UA and save should have no error.", () => newFactory.Save());
			}
			finally
			{
				ErrorReporter.Instance.Clear();
			}
		}

		public void TestReject()
		{
			UAInvoice invoice = Factory.NewWithValidTestData<UAInvoice>();
			invoice.AH_TransactionNum = TestObjectCreator.GetRandomString(AccTransactionHeaderSchema.AH_TransactionNum.MaxLength);
			invoice.Lines.AddNew();
			invoice.Lines.AddNew();

			Factory.Save();

			var oldTransactionNumber = invoice.AH_TransactionNum;
			AssertEquals("Count of lines after save", 2, invoice.Lines.Count);

			invoice.Delete();

			Assert("Should not have been deleted", !invoice.IsDeleted);
			AssertEquals("AH_IsCancelled", true, invoice.AH_IsCancelled);
			AssertNotNull("Cancelled Event should have been recorded in the logs", invoice.Logs.MostRecentLogByEventTime(Events.Cancelled));
			AssertEquals("Transaction Number", oldTransactionNumber.Substring(0, invoice.AH_TransactionNumInfo.MaxLength - 2) + "RJ", invoice.AH_TransactionNum);
			AssertEquals("All lines should have been deleted", 0, invoice.Lines.Count);
		}

		public void TestRejecter()
		{
			string transactionNum = TestObjectCreator.GetRandomString(AccTransactionHeaderSchema.AH_TransactionNum.MaxLength);

			UAInvoice invoice = Factory.NewWithValidTestData<UAInvoice>();
			invoice.Lines.AddNew();
			invoice.AH_TransactionNum = transactionNum;
			Factory.Save();
			invoice.Delete();
			Factory.Save();

			UAInvoice invoice2 = Factory.NewWithValidTestData<UAInvoice>();
			invoice2.Lines.AddNew();
			invoice2.AH_TransactionNum = transactionNum;
			Factory.Save();
			invoice2.Delete();
			Factory.Save();

			UAInvoice invoice3 = Factory.NewWithValidTestData<UAInvoice>();
			invoice3.Lines.AddNew();
			invoice3.AH_TransactionNum = transactionNum;
			Factory.Save();
			invoice3.Delete();
			Factory.Save();

			AssertEquals("transaction numbers", invoice.AH_TransactionNum, invoice2.AH_TransactionNum);
			AssertEquals("transaction numbers", invoice.AH_TransactionNum, invoice3.AH_TransactionNum);

			AssertEquals("First Invoice should have transaction count 1", (ZByte)1, invoice.AH_TransactionCount);
			AssertEquals("Second Invoice should have transaction count 2", (ZByte)2, invoice2.AH_TransactionCount);
			AssertEquals("Third Invoice should have transaction count 3", (ZByte)3, invoice3.AH_TransactionCount);
		}

		public void TestAddOneEditLogOfAccTransactionHeaderWhenEditUAInvoice()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsDebtor = true;
			org.OH_IsCreditor = true;

			var uaInvoice = Factory.NewWithValidTestData<UAInvoice>();
			uaInvoice.AH_OH = org.PK;
			var uaInvoiceLine = TestObjectCreator.CreateInvoiceLine(uaInvoice, uaInvoice.TransactionCurrency, uaInvoice.AH_ExchangeRate, 100m, 0m, 0m, 100m, 0m, 0m);

			DateTime highWaterMark = ZDateTime.UtcNow.ToDateTime();
			StmALog[] logsBeforeAdd = GetEDTStmALogAfterSL_PostedTime(highWaterMark, uaInvoice);
			Factory.Save();
			StmALog[] logsAfterSave = GetEDTStmALogAfterSL_PostedTime(highWaterMark, uaInvoice);
			AssertEquals("There should be 1 logs added.", 1, logsAfterSave.Length - logsBeforeAdd.Length);

			uaInvoice.AH_Desc = "Changed";
			uaInvoiceLine.AL_Desc = "Overridden";

			highWaterMark = ZDateTime.UtcNow.ToDateTime();
			StmALog[] logsBeforeEdit = GetEDTStmALogAfterSL_PostedTime(highWaterMark, uaInvoice);
			Factory.Save();
			StmALog[] logsAfterEdit = GetEDTStmALogAfterSL_PostedTime(highWaterMark, uaInvoice);
			AssertEquals("There should be 1 logs added.", 1, logsAfterEdit.Length - logsBeforeEdit.Length);
			var stmALogAccTransactionHeader = GetLogByTable(logsAfterEdit, "AccTransactionHeader");
			AssertEquals("There should be 1 acctransactionheader edit log.", 1, stmALogAccTransactionHeader.Length);
			AssertEquals("AccTransactionHeader Should be edit.", "EDT", stmALogAccTransactionHeader[0].SL_SE_NKEvent);
		}

		public void TestAutoCreateComplianceDocumentWhenPostUnapprovedInvoice()
		{
			AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			TestObjectCreator.AALSHI.CompanyData.OB_APCreateVATComplianceDocumentOnPosting = Constants.OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge;
			TestObjectCreator.AALSHI.CompanyData.OB_ARCreateVATComplianceDocumentOnPosting = Constants.OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge;
			Factory.Save();

			var uaInvoice = new TransactionCreator().CreateTransaction(Factory, LedgerTypes.UnapprovedPayableTransactions, TransactionTypes.UAInvoice) as UAInvoice;
			TestObjectCreator.InitializeInvoicingBase(uaInvoice, "TEST001", TestObjectCreator.AALSHI, TestObjectCreator.OverheadChargeCode, TestObjectCreator.GST1);
			Factory.Save();

			AssertEquals("Pre-condition", true, uaInvoice.IsInDatabase);
			AssertEquals("Auto-create compliance document feature does NOT apply to unapproved transactions", false, uaInvoice.CanCreateComplianceDocument);

			var approvedInvoice = new UnapprovedTransactionConverter(Factory).ConvertToAP(uaInvoice, false);

			AssertEquals("Pre-condition", true, approvedInvoice.IsInDatabase);
			var query = approvedInvoice.GetComplianceDocumentByTransactionQuery_ForTestOnly(approvedInvoice.PK);
			var complianceDocuments = Factory.Load<AccComplianceDocumentHeader>(query);
			AssertEquals(0, complianceDocuments.Length);
			AssertEquals("Auto-create compliance document feature applied when post an approved transaction", true, approvedInvoice.CanCreateComplianceDocument);

			approvedInvoice.Factory.Save();

			complianceDocuments = Factory.Load<AccComplianceDocumentHeader>(query);
			AssertEquals("Compliance document created once posted", 1, complianceDocuments.Length);
		}

		public override void TestAddLogsWhenJobChargeCriteriaMatchFailed()
		{
			Assert("Not Applicable", true);
		}

		#region Not Actual Tests

		public override void TestImportSingleCostPreserveIndexOfImportedUniversalTransactionLineValue()
		{
			Assert("Not applicable", true);
		}

		public override void TestImportAllApportionmentsFromCostingPreserveIndexOfImportedUniversalTransactionLineValue()
		{
			Assert("Not applicable", true);
		}

		public override void TestSetTransactionBelongsToGroupField()
		{
			Assert(true);
		}

		public new void TestSetDescription()
		{
			Assert(true);
		}

		public new void TestReversingReason()
		{
			Assert(true);
		}

		public new void TestReversingErroneousTransaction()
		{
			Assert(true);
		}

		public override void TestReversingErroneousTransactionLine()
		{
			Assert(true);
		}

		public new void TestReverseTransactionHasLinesCorrectlyOrdered()
		{
			Assert(true);
		}

		public new void TestReverseTransactionHasCorrectPlaceOfSupply()
		{
			Assert("Not Applicable", true);
		}

		public new void TestReverseTransaction()
		{
			Assert(true);
		}

		public new void TestAPInvoicingForChinaNolongerGeneratesTransactionReferenceInReversing()
		{
			Assert(true);
		}

		#region TestExchangeRateRecalculatedFromOtherTaxesAmounts

		public override void TestExchangeRateRecalculatedFromOSTaxAmountOtherTaxes_WhenAPARInvoicePostingExchangeRateOptionIsDEF()
		{
			Assert("Not Applicable", true);
		}

		public override void TestExchangeRateRecalculatedFromLocalTaxAmountOtherTaxes_WhenAPARInvoicePostingExchangeRateOptionIsDEF()
		{
			Assert("Not Applicable", true);
		}

		public override void TestExchangeRateRecalculatedFromOSTaxAmountOtherTaxes_WhenAPInvoiceUseJobExchangeRateFlagIsTicked()
		{
			Assert("Not Applicable", true);
		}

		public override void TestExchangeRateRecalculatedFromLocalTaxAmountOtherTaxes_WhenAPInvoiceUseJobExchangeRateFlagIsTicked()
		{
			Assert("Not Applicable", true);
		}

		public override void TestExchangeRateNOTRecalculatedFromOSTaxAmountOtherTaxes_WhenAPARInvoicePostingExchangeRateOptionIsTOD()
		{
			Assert("Not Applicable", true);
		}

		public override void TestExchangeRateNOTRecalculatedFromLocalTaxAmountOtherTaxes_WhenAPARInvoicePostingExchangeRateOptionIsTOD()
		{
			Assert("Not Applicable", true);
		}

		public override void TestExchangeRateNOTRecalculatedFromOSTaxAmountOtherTaxes_WhenAPARInvoicePostingExchangeRateOptionIsINV()
		{
			Assert("Not Applicable", true);
		}

		public override void TestExchangeRateNOTRecalculatedFromLocalTaxAmountOtherTaxes_WhenAPARInvoicePostingExchangeRateOptionIsINV()
		{
			Assert("Not Applicable", true);
		}

		public override void TestExchangeRateNOTRecalculatedFromOSTaxAmountOtherTaxes_WhenAPARInvoicePostingExchangeRateOptionIsPST()
		{
			Assert("Not Applicable", true);
		}

		public override void TestExchangeRateNOTRecalculatedFromLocalTaxAmountOtherTaxes_WhenAPARInvoicePostingExchangeRateOptionIsPST()
		{
			Assert("Not Applicable", true);
		}

		#endregion

		public override void TestCashInvoice_TaxRealisationEnablerIsCalledBeforeBeforeGLMovementProcessor()
		{
			Assert("UA Invoice can't be Cash Invoice", true);
		}

		public override void TestCashInvoiceWithSPRTaxTransactions_BalanceAdjustment_LocalInvoiceLocalPayment()
		{
			Assert("Not Applicable", true);
		}

		public override void TestCashInvoiceWithSPRTaxTransactions_BalanceAdjustment_ForeignInvoiceLocalPayment()
		{
			Assert("Not Applicable", true);
		}

		public override void TestCashInvoiceWithSPRTaxTransactions_BalanceAdjustment_ForeignInvoiceForeignPayment()
		{
			Assert("Not Applicable", true);
		}

		public override void TestCashInvoiceWithSPRTaxTransactions_BalanceAdjustment_ForeignInvoiceForeignPayment_RoundingErrors()
		{
			Assert("Not Applicable", true);
		}

		#endregion

		protected override Type TypeOfValidation
		{
			get { return typeof(APInvoiceValidation); }
		}

		protected override bool ShouldCallUpdatePostDateOnParentReversing => false;

		protected override bool CouldHaveAssociatedDraftInvoice => false;
	}
}
