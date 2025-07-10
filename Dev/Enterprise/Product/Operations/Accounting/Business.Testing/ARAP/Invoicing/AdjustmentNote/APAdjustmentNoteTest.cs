using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(APAdjustmentNote))]
	public class APAdjustmentNoteTest : AdjustmentNoteTest
	{
		public void TestIEdocsParsingSupportProvider()
		{
			var bo = Factory.NewWithValidTestData<APAdjustmentNote>();
			var eDocsParsingSupport = bo as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		public void TestCodeProperty()
		{
			InvoicingBase adjustmentNote = (InvoicingBase)new BusinessObjectFactory().New(GetExpectedBusinessObjectType());
			adjustmentNote.AH_ConsolidatedInvoiceRef = "ABC123";
			adjustmentNote.AH_TransactionNum = "TransNum";
			AssertEquals("Code Property Should be AH_ConsolidatedInvoiceRef because it's unique for AP Trans", adjustmentNote.AH_ConsolidatedInvoiceRef, ((ICodeDescription)adjustmentNote).Code);
		}

		public void TestValidationForIncompleteTransaction()
		{
			var invoice = (InvoicingBase)new BusinessObjectFactory().New(GetExpectedBusinessObjectType());

			invoice.Factory.SetContext(Enterprise.Integration.Accounting.BusinessContext.SavingIncompleteTransaction);
			AssertEquals("Validation Type for Incomplete", typeof(IncompleteInvoicingBaseValidation), invoice.Validation.GetType());

			invoice.Factory.RemoveContext(Enterprise.Integration.Accounting.BusinessContext.SavingIncompleteTransaction);
			AssertNotEquals("Validation Type for Regular", typeof(IncompleteInvoicingBaseValidation), invoice.Validation.GetType());
		}

		public override void TestNumberFountainInternalRef()
		{
			APAdjustmentNote adjustmentnote = Factory.NewWithValidTestData<APAdjustmentNote>();
			ShareSequentialReferenceNumbers item = new ShareSequentialReferenceNumbers();
			item.Value = true;
			AccountingConfigurationRegistry.Instance.ShareSequentialInvoiceReferenceNumbers.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, item);
			AssertEquals("Should be APInvoiceInternalRef number fountain", Env.NumberFountains.APInvoiceInternalRef.GetTodaysPeriodFountain(), adjustmentnote.NumberFountainForInternalRef_ForTestOnly.numberFountainPooler.GetTodaysPeriodFountain());
			AssertNotEquals("Should be APInvoiceInternalRef number fountain", Env.NumberFountains.APAdjustmentNoteInternalRef.GetTodaysPeriodFountain(), adjustmentnote.NumberFountainForInternalRef_ForTestOnly.numberFountainPooler.GetTodaysPeriodFountain());
			item.Value = false;
			AccountingConfigurationRegistry.Instance.ShareSequentialInvoiceReferenceNumbers.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, item);
			AssertNotEquals("Should be APAdjustmentNoteInternalRef number fountain", Env.NumberFountains.APInvoiceInternalRef.GetTodaysPeriodFountain(), adjustmentnote.NumberFountainForInternalRef_ForTestOnly.numberFountainPooler.GetTodaysPeriodFountain());
			AssertEquals("Should be APAdjustmentNoteInternalRef number fountain", Env.NumberFountains.APAdjustmentNoteInternalRef.GetTodaysPeriodFountain(), adjustmentnote.NumberFountainForInternalRef_ForTestOnly.numberFountainPooler.GetTodaysPeriodFountain());
		}

		protected override bool ShouldSupportCalculatingTaxAtHeaderLevel
		{
			get { return false; }
		}
		public override void TestTransactionNumberOnSave()
		{
			AssertAPItemsRetainUserSetTransactionNum();
		}

		public override void TestCreditLimitExceededEmailWillBeSentOnlyOnce()
		{
			Assert("AR test", true);
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

		protected override Type TypeOfValidation
		{
			get { return typeof(APAdjustmentNoteValidation); }
		}

		protected override Type GetExpectedBusinessObjectLineType()
		{
			return typeof(APAdjustmentNoteLine);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var invoiceBase = Factory.New(GetExpectedBusinessObjectType());
			TestObjectCreator.FillInvoiceWithMinimumTestData((InvoicingBase)invoiceBase);
			return invoiceBase;
		}

		protected override void AssertEmailSendStatusForUnpostedInvoice()
		{
			AssertEquals("No email should be sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public override void TestBusinessContext()
		{
			AssertEquals("BusinessContext should be APInvoice", BusinessContext.APInvoice, InvoicingBase.DocumentSupporter.BusinessContext);
		}

		public void TestDocManagerCode()
		{
			APAdjustmentNote bizO = Factory.New<APAdjustmentNote>();
			AssertEquals("Wrong DocManagerCode. Any change to the IDocManagerSupport interface must also be changed in document scanning lookup", "PAN", ((IDocManagerSupport)bizO).DocManagerInfo.DocManagerCode);
		}

		protected override ZString ExpectedTransactionTypeForIncomplete
		{
			get { return TransactionTypes.IncompleteAdjustmentNote; }
		}

		protected override BooleanRegistryItem EnforcePostingAtFixedPlaceOfSupplyLevelForTransactionRuleRegistry => AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions;

		public void TestIsUseJobExchangeRateApplicable()
		{
			AccountingConfigurationRegistry.Instance.UseJobExchangeRateDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var bizO = Factory.New<APAdjustmentNote>();
			AssertEquals(bizO.AH_RX_NKTransactionCurrency, "AUD");
			AssertEquals(bizO.ExchangeRate.IsRateReadOnly, true);
			AssertEquals(bizO.UseJobExchangeRate, false);
			AssertEquals(bizO.AH_PostedToEFT, false);

			bizO.AH_RX_NKTransactionCurrency = "USD";
			AssertEquals(bizO.AH_RX_NKTransactionCurrency, "USD");
			AssertEquals(bizO.ExchangeRate.IsRateReadOnly, false);
			AssertEquals(bizO.UseJobExchangeRate, false);
			AssertEquals(bizO.AH_PostedToEFT, false);

			AccountingConfigurationRegistry.Instance.UseJobExchangeRateDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			bizO = Factory.New<APAdjustmentNote>();
			AssertEquals(bizO.AH_RX_NKTransactionCurrency, "AUD");
			AssertEquals(bizO.ExchangeRate.IsRateReadOnly, true);
			AssertEquals(bizO.UseJobExchangeRate, false);
			AssertEquals(bizO.AH_PostedToEFT, false);

			bizO.AH_RX_NKTransactionCurrency = "USD";
			AssertEquals(bizO.AH_RX_NKTransactionCurrency, "USD");
			AssertEquals(bizO.ExchangeRate.IsRateReadOnly, false);
			AssertEquals(bizO.UseJobExchangeRate, false);
			AssertEquals(bizO.AH_PostedToEFT, false);
		}

		protected override bool CouldHaveAssociatedDraftInvoice => true;
	}
}
