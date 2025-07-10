using System;
using System.Globalization;
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
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(UACreditNote))]
	public class UACreditNoteTest : APCreditNoteTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<UACreditNote>();
		}

		public override void TestShouldShowOriginalInvoiceReferenceFields()
		{
			AssertEquals("ShouldShowOriginalInvoiceReferenceFields should be false.", false, InvoicingBase.ShouldShowOriginalInvoiceReferenceFields);
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

		protected override Type GetExpectedBusinessObjectLineType()
		{
			return typeof(UACreditNoteLine);
		}

		public new void TestValidationForIncompleteTransaction()
		{
			Assert(true);
		}

		public override void TestBusinessContext()
		{
			AssertEquals("BusinessContext should be APInvoice", BusinessContext.APInvoice, InvoicingBase.DocumentSupporter.BusinessContext);
		}

		public void TestDescription()
		{
			SetupForSave();
			AssertEquals("Description should be 'Unapproved Credit Note'", "Unapproved Credit Note".ToUpper(), Header.AH_Desc);
			AssertEquals("Number Of Supporting Documents should be 1", 1, Header.AH_NumberOfSupportingDocuments.ToZInt());
		}

		public override void TestTransactionNumberOnSave()
		{
			SetupForSave();
			var defaultTransactionNum = "TransNum";
			Header.AH_TransactionNum = defaultTransactionNum;
			AssertEquals("should have default transaction number on instantiation", defaultTransactionNum, Header.AH_TransactionNum);
			Factory.Save();
			AssertEquals("Should not get transactionnumber from number fountain", defaultTransactionNum, Header.AH_TransactionNum);
		}

		public override void TestGenerateReverseTransaction()
		{
			int transactionNum = 1;

			foreach (var regValue in new[] { true, false })
			{
				using (AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, regValue))
				{
					var invoicingBase = (InvoicingBase)PrepareTransactionHeaderForTest();
					invoicingBase.FillWithValidTestData();
					invoicingBase.AH_TransactionNum = transactionNum.ToString("D8");
					transactionNum++;
					var line = TestObjectCreator.CreateInvoiceLine(invoicingBase, TestObjectCreator.Job1, TestObjectCreator.CC1, 100);
					Charge charge = TestObjectCreator.CreateCharge(line);
					Factory.Save();

					invoicingBase.GenerateReverseTransaction(true);
					Assert("Should be canceled.", invoicingBase.AH_IsCancelled);
					Assert("IsReversing", invoicingBase.IsReversing);
					Assert("IsReverseTransaction", invoicingBase.IsReverseTransaction);
					Assert("Transformer should not be run on this stage. So line is still linked to a charge.", charge.APLine != null && charge.APLine.AL_LineType == ZArchitecture.Core.TransactionLineTypes.UnapprovedCost);

					invoicingBase.Delete();
					Factory.Save();
					Assert("Should be canceled.", invoicingBase.AH_IsCancelled);
					Assert(string.Format(CultureInfo.InvariantCulture, "Transformer has run. So line is unlinked. Accrual is {0} created as it is Credit Note reversed and not Invoice and EnableNegativeAccrualBehaviors is {1}", regValue ? "" : "not", regValue ? "ON" : "OFF"), (regValue ? charge.APLine != null : charge.APLine == null));
				}
			}
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

		public void TestAuthorisationLevels()
		{
			var creditNote = Factory.New<UACreditNote>();
			AssertEquals("Does Not Apply", creditNote.AuthorisationLevel);
			AssertEquals("Does Not Apply", creditNote.MaxAuthorisationLevel);
		}

		public override void TestGenerateReverseTransaction_CopyCharges()
		{
			Assert("This test is not applied to this class.", true);
		}

		public new void TestGenerateReverseTransactionWithDifferentCurrencyLines()
		{
			Assert("This test is not applied to this class.", true);
		}

		public new void TestSendEmailWhenCreditLimitExceeded()
		{
			GlbStaff currentUserInCurrentFactory = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentUserInCurrentFactory.GS_EmailAddress = "A@B.COM";
			Factory.Save();

			GlbGroup group = Factory.New<GlbGroup>();
			group.Staff.Add(currentUserInCurrentFactory);

			AccountingConfigurationRegistry.Instance.DebtorCreditLimitNotifyGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			UACreditNote creditNote = Factory.New<UACreditNote>();
			creditNote.AH_TransactionNum = "10001002";
			var line = (UACreditNoteLine)creditNote.Lines.AddNew();
			line.AL_AC = TestObjectCreator.NonAccrualChargeCode.PK;
			line.AL_LocalExTaxAmount = 2000m;
			TestObjectCreator.AALSHI.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			TestObjectCreator.AALSHI.CompanyData.OB_APPaymentTerms = "CRD";
			TestObjectCreator.AALSHI.CompanyData.OB_APCreditLimit = 200m;
			creditNote.AH_OH = TestObjectCreator.AALSHI.PK;
			creditNote.SubmittedFromInvoicingForm = true;

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

			var creditNote = (UACreditNote)factory.NewWithValidTestData(GetExpectedBusinessObjectType());
			creditNote.SubmittedFromInvoicingForm = true;
			JobConsolCost cost = creditNote.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			cost.E6_AC_ChargeCode = creator.CC1.PK;
			cost.E6_OSCostAmount = 300m;
			creditNote.ImportAllApportionmentsFromCosting();
			factory.Save();

			InvoicingBase converted = new UnapprovedTransactionConverter(factory).ConvertToAP(creditNote, false);
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
			UACreditNote creditNote = Factory.NewWithValidTestData<UACreditNote>();
			Factory.Save();
			AssertEquals(ZString.Empty, creditNote.AH_TransactionReference);
		}

		public void TestDelete()
		{
			var invoice = Factory.NewWithValidTestData<UACreditNote>();
			invoice.Lines.AddNew();
			invoice.Lines.AddNew();
			invoice.Delete();

			Assert("Invoice that not saved in db should have been deleted.", invoice.IsDeleted);
		}

		public void TestReject()
		{
			UACreditNote creditNote = Factory.NewWithValidTestData<UACreditNote>();
			creditNote.AH_TransactionNum = TestObjectCreator.GetRandomString(AccTransactionHeaderSchema.AH_TransactionNum.MaxLength);
			creditNote.Lines.AddNew();
			creditNote.Lines.AddNew();

			Factory.Save();

			var oldTransactionNumber = creditNote.AH_TransactionNum;
			AssertEquals("Count of lines after save", 2, creditNote.Lines.Count);

			creditNote.Delete();

			Assert("Should not have been deleted", !creditNote.IsDeleted);
			AssertEquals("AH_IsCancelled", true, creditNote.AH_IsCancelled);
			AssertNotNull("Cancelled Event should have been recorded in the logs", creditNote.Logs.MostRecentLogByEventTime(Events.Cancelled));
			AssertEquals("Transaction Number", oldTransactionNumber.Substring(0, creditNote.AH_TransactionNumInfo.MaxLength - 2) + "RJ", creditNote.AH_TransactionNum);
			AssertEquals("All lines should have been deleted", 0, creditNote.Lines.Count);
		}

		public void TestAddOneEditLogOfAccTransactionHeaderWhenEditUACreditNote()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsDebtor = true;
			org.OH_IsCreditor = true;

			var uaCreditNote = Factory.NewWithValidTestData<UACreditNote>();
			uaCreditNote.AH_OH = org.PK;
			var uaCreditNoteLine = TestObjectCreator.CreateInvoiceLine(uaCreditNote, uaCreditNote.TransactionCurrency, uaCreditNote.AH_ExchangeRate, 200m, 0m, 0m, 200m, 0m, 0m);

			DateTime highWaterMark = ZDateTime.UtcNow.ToDateTime();
			StmALog[] logsBeforeAdd = GetEDTStmALogAfterSL_PostedTime(highWaterMark, uaCreditNote);
			Factory.Save();
			StmALog[] logsAfterAdd = GetEDTStmALogAfterSL_PostedTime(highWaterMark, uaCreditNote);
			AssertEquals("There should be 1 logs added.", 1, logsAfterAdd.Length - logsBeforeAdd.Length);

			uaCreditNote.AH_Desc = "Changed";
			uaCreditNoteLine.AL_Desc = "Overridden";

			highWaterMark = ZDateTime.UtcNow.ToDateTime();
			StmALog[] logsBeforeEdit = GetEDTStmALogAfterSL_PostedTime(highWaterMark, uaCreditNote);
			Factory.Save();
			StmALog[] logsAfterEdit = GetEDTStmALogAfterSL_PostedTime(highWaterMark, uaCreditNote);
			AssertEquals("There should be 1 logs added.", 1, logsAfterEdit.Length - logsBeforeEdit.Length);
			var stmALogAccTransactionHeader = GetLogByTable(logsAfterEdit, "AccTransactionHeader");
			AssertEquals("There should be 1 acctransactionheader edit log.", 1, stmALogAccTransactionHeader.Length);
			AssertEquals("AccTransactionHeader Should be edit.", "EDT", stmALogAccTransactionHeader[0].SL_SE_NKEvent);
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

		#endregion

		protected override Type TypeOfValidation
		{
			get { return typeof(UACreditNoteValidation); }
		}

		protected override bool ShouldCallUpdatePostDateOnParentReversing => false;

		protected override bool CouldHaveAssociatedDraftInvoice => false;
	}
}
