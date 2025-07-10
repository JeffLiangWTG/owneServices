using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingIServices.Testing
{
	class NumberFountainTransactionDataProviderTest : TestCaseWithFactory
	{
		[MasterFiles.Business.Testing.SuspendGLAccountAndChargeCodeCriticalValidation]
		public void TestInitializeOnlyOnceDuringOneSave()
		{
			var invoice = Factory.New<ARInvoice>();
			var line = (ARInvoiceLine)invoice.Lines.AddNew();
			var reportableTax = Factory.New<AccTaxRate>();
			reportableTax.AT_Type = AccTaxRate.Types.CapitalRated;

			Factory.Save();
			line.AL_AT = reportableTax.PK;
			Assert("Precondition: IsTaxReportable", invoice.IsTaxReportable);
			var checkResult = NumberFountainTransactionDataProvider.CheckDataIsCorrect(invoice);
			Assert("IsCorrect", checkResult.IsCorrect);
			AssertEquals("ErrorMessage", "", checkResult.ErrorMessage);
			Assert("GetIsTaxReported", NumberFountainTransactionDataProvider.GetIsTaxReported(invoice));

			NumberFountainTransactionDataProvider.Initialize(invoice);
			line.AL_AT = ZGuid.Empty;
			Assert("Precondition: IsTaxReportable", !invoice.IsTaxReportable);
			Assert("GetIsTaxReported", NumberFountainTransactionDataProvider.GetIsTaxReported(invoice));
			checkResult = NumberFountainTransactionDataProvider.CheckDataIsCorrect(invoice);
			Assert("IsCorrect", !checkResult.IsCorrect);
			AssertEquals("ErrorMessage", "TaxReported: original value: True, current value: False. It was an attempt to retrieve different value of the same data type for the same transaction in the same saving session.", checkResult.ErrorMessage);

			Factory.Save();
			NumberFountainTransactionDataProvider.Initialize(invoice);
			Assert("Precondition: IsTaxReportable", !invoice.IsTaxReportable);
			Assert("GetIsTaxReported", !NumberFountainTransactionDataProvider.GetIsTaxReported(invoice));
			checkResult = NumberFountainTransactionDataProvider.CheckDataIsCorrect(invoice);
			Assert("IsCorrect", checkResult.IsCorrect);
			AssertEquals("ErrorMessage", "", checkResult.ErrorMessage);
		}

		public void TestInvalidState()
		{
			var invoice = Factory.New<ARInvoice>();
			invoice.AH_TransactionCategory = TransactionCategory.Codes.SelfBilling;
			Assert("Precondition: IsSelfBillingInvoice", invoice.IsSelfBillingInvoice);
			Assert("GetIsSelfBilling", NumberFountainTransactionDataProvider.GetIsSelfBilling(invoice));

			invoice.AH_TransactionCategory = TransactionCategory.Codes.Standard;
			Assert("Precondition: IsSelfBillingInvoice", !invoice.IsSelfBillingInvoice);
			Assert("GetIsSelfBilling should return original state to provide consistent result.", NumberFountainTransactionDataProvider.GetIsSelfBilling(invoice));
			var checkResult = NumberFountainTransactionDataProvider.CheckDataIsCorrect(invoice);
			Assert("IsCorrect", !checkResult.IsCorrect);
			var expectedErrorMessage = "SelfBilling: original value: True, current value: False. It was an attempt to retrieve different value of the same data type for the same transaction in the same saving session.";
			AssertEquals("ErrorMessage", expectedErrorMessage, checkResult.ErrorMessage);

			invoice.AH_TransactionCategory = InvoiceTypesList.Codes.SelfBillingInvoice;
			Assert("Precondition: IsSelfBillingInvoice", invoice.IsSelfBillingInvoice);
			Assert("GetIsSelfBilling", NumberFountainTransactionDataProvider.GetIsSelfBilling(invoice));
			checkResult = NumberFountainTransactionDataProvider.CheckDataIsCorrect(invoice);
			Assert("IsCorrect", !checkResult.IsCorrect);
			AssertEquals("ErrorMessage: it should be the same even if you restore correct state as incorrect state was already used.", expectedErrorMessage, checkResult.ErrorMessage);

			NumberFountainTransactionDataProvider.Initialize(invoice);
			checkResult = NumberFountainTransactionDataProvider.CheckDataIsCorrect(invoice);
			Assert("IsCorrect", checkResult.IsCorrect);
			AssertEquals("ErrorMessage: call of Initialize should reset invalid stat as this is new saving and new number will be used based on valid data.", "", checkResult.ErrorMessage);
		}

		[MasterFiles.Business.Testing.SuspendGLAccountAndChargeCodeCriticalValidation]
		public void TestGetIsTaxReported()
		{
			var invoice = Factory.New<ARInvoice>();
			AssertNoExceptionThrown(() => Factory.Save());

			var line = (ARInvoiceLine)invoice.Lines.AddNew();
			var reportableTax = Factory.New<AccTaxRate>();
			reportableTax.AT_Type = AccTaxRate.Types.CapitalRated;
			line.AL_AT = reportableTax.PK;
			Assert("Precondition: IsTaxReportable", invoice.IsTaxReportable);
			var checkResult = NumberFountainTransactionDataProvider.CheckDataIsCorrect(invoice);
			Assert("IsCorrect", checkResult.IsCorrect);
			AssertEquals("ErrorMessage", "", checkResult.ErrorMessage);
			Assert("GetIsTaxReported", NumberFountainTransactionDataProvider.GetIsTaxReported(invoice));

			Factory.Save();
			NumberFountainTransactionDataProvider.Initialize(invoice);
			line.AL_AT = ZGuid.Empty;
			Assert("Precondition: IsTaxReportable", !invoice.IsTaxReportable);
			Assert("GetIsTaxReported", !NumberFountainTransactionDataProvider.GetIsTaxReported(invoice));
			checkResult = NumberFountainTransactionDataProvider.CheckDataIsCorrect(invoice);
			Assert("IsCorrect", checkResult.IsCorrect);
			AssertEquals("ErrorMessage", "", checkResult.ErrorMessage);

			line.AL_AT = reportableTax.PK;
			Assert("Precondition: IsTaxReportable", invoice.IsTaxReportable);
			checkResult = NumberFountainTransactionDataProvider.CheckDataIsCorrect(invoice);
			Assert("IsCorrect", !checkResult.IsCorrect);
			AssertEquals("ErrorMessage", "TaxReported: original value: False, current value: True.", checkResult.ErrorMessage);

			Factory.Save();
			NumberFountainTransactionDataProvider.Initialize(invoice);
			checkResult = NumberFountainTransactionDataProvider.CheckDataIsCorrect(invoice);
			Assert("IsCorrect", checkResult.IsCorrect);
			AssertEquals("ErrorMessage", "", checkResult.ErrorMessage);
		}

		public void TestGetIsSelfBilling_InvalidState()
		{
			var invoice = Factory.New<ARInvoice>();
			invoice.AH_TransactionCategory = TransactionCategory.Codes.SelfBilling;
			Assert("Precondition: IsSelfBillingInvoice", invoice.IsSelfBillingInvoice);
			Assert("GetIsSelfBilling", NumberFountainTransactionDataProvider.GetIsSelfBilling(invoice));
			var checkResult = NumberFountainTransactionDataProvider.CheckDataIsCorrect(invoice);
			Assert("IsCorrect", checkResult.IsCorrect);
			AssertEquals("ErrorMessage", "", checkResult.ErrorMessage);

			invoice.AH_TransactionCategory = TransactionCategory.Codes.Standard;
			Assert("Precondition: IsSelfBillingInvoice", !invoice.IsSelfBillingInvoice);
			checkResult = NumberFountainTransactionDataProvider.CheckDataIsCorrect(invoice);
			Assert("IsCorrect", !checkResult.IsCorrect);
			var expectedErrorMessage = "SelfBilling: original value: True, current value: False.";
			AssertEquals("ErrorMessage", expectedErrorMessage, checkResult.ErrorMessage);

			Assert("Getting other property doesn't reset an error.", !NumberFountainTransactionDataProvider.GetIsCorrected(invoice));
			checkResult = NumberFountainTransactionDataProvider.CheckDataIsCorrect(invoice);
			Assert("IsCorrect", !checkResult.IsCorrect);
			AssertEquals("ErrorMessage", expectedErrorMessage, checkResult.ErrorMessage);

			invoice.AH_TransactionCategory = TransactionCategory.Codes.SelfBilling;
			NumberFountainTransactionDataProvider.Initialize(invoice);
			checkResult = NumberFountainTransactionDataProvider.CheckDataIsCorrect(invoice);
			Assert("IsCorrect", checkResult.IsCorrect);
			AssertEquals("ErrorMessage", "", checkResult.ErrorMessage);
		}

		public void TestGetIsCorrected()
		{
			var invoice = Factory.New<ARInvoice>();
			invoice.AH_TransactionBelongsToGroup = ZGuid.NewZGuid();
			invoice.AH_IsCancelled = true;
			Assert("Precondition: IsAmendingOrReversal", invoice.IsAmendingOrReversal);
			Assert("GetIsCorrected", NumberFountainTransactionDataProvider.GetIsCorrected(invoice));

			invoice.AH_IsCancelled = false;
			Assert("Precondition: IsAmendingOrReversal", !invoice.IsAmendingOrReversal);
			var checkResult = NumberFountainTransactionDataProvider.CheckDataIsCorrect(invoice);
			Assert("IsCorrect", !checkResult.IsCorrect);
			AssertEquals("ErrorMessage", "Corrected: original value: True, current value: False.", checkResult.ErrorMessage);

			NumberFountainTransactionDataProvider.Initialize(invoice);
			checkResult = NumberFountainTransactionDataProvider.CheckDataIsCorrect(invoice);
			Assert("IsCorrect", checkResult.IsCorrect);
			AssertEquals("ErrorMessage", "", checkResult.ErrorMessage);
			Assert("GetIsCorrected", !NumberFountainTransactionDataProvider.GetIsCorrected(invoice));

			invoice.AH_IsCancelled = true;
			Assert("Precondition: IsAmendingOrReversal", invoice.IsAmendingOrReversal);
			checkResult = NumberFountainTransactionDataProvider.CheckDataIsCorrect(invoice);
			Assert("IsCorrect", !checkResult.IsCorrect);
			AssertEquals("ErrorMessage", "Corrected: original value: False, current value: True.", checkResult.ErrorMessage);
		}
	}
}