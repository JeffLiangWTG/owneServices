namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	public abstract class PaymentApprovalWithoutAuthorisationTest : PaymentApprovalBaseTest
	{
		protected override void AssertTransactionAlreadyPaidHasExpectedError(PaymentApprovalBase approval, string exceptionMessage)
		{
			var expectedError = $@"Transaction already paid

PaymentApprovalBaseMatchDetails:
PaymentApprovalMatchingBase.Match()
Current company's currency Code: AUD, Decimals: 2
Ledger: {approval.Ledger}, Transaction type: PAY, Payment Amount: 305, Outstanding Amount: 305, OS Payment Amount: 305, OS Outstanding Amount: 305, Currency: AUD, Exchange Rate Amount: 1";
			AssertContains(expectedError, exceptionMessage);

			expectedError = $@"MatchingBase.Match()
Current company's currency Code: AUD, Decimals: 2
Ledger: {approval.Ledger}, Transaction type: PAY, Payment Amount: 305, Outstanding Amount: 305, OS Payment Amount: 305, OS Outstanding Amount: 305, Currency: AUD, Exchange Rate Amount: 1
Ledger: {approval.Ledger}, Transaction type: EXX, Payment Amount: -5, Outstanding Amount: -5, OS Payment Amount: -5, OS Outstanding Amount: -5, Currency: AUD, Exchange Rate Amount: 1";
			AssertContains(expectedError, exceptionMessage);

			expectedError = @"Ledger: AP, Transaction type: CRD, Payment Amount: -200, Outstanding Amount: -200, OS Payment Amount: -200, OS Outstanding Amount: -200, Currency: AUD, Exchange Rate Amount: 1";
			AssertContains(expectedError, exceptionMessage);

			expectedError = @"Ledger: AP, Transaction type: CRD, Payment Amount: -100, Outstanding Amount: 0, OS Payment Amount: -100, OS Outstanding Amount: 0, Currency: AUD, Exchange Rate Amount: 1";
			AssertContains(expectedError, exceptionMessage);

			expectedError = "MatchedTransactions contains transaction from primary organization: true";
			AssertContains(expectedError, exceptionMessage);

			expectedError = "Match group info: There is no data collected.";
			AssertContains(expectedError, exceptionMessage);
		}

		protected override bool IsCurrencyReadOnly { get { return true; } }

		protected override bool GetIsCreatedForPostingValue(PaymentApprovalBase paymentApproval)
		{
			return true;
		}
	}
}
