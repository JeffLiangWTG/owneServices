using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	class EntryFeePaymentPartyUnderstanderTest : TestCaseWithFactory
	{
		public void TestShouldBrokerPayThisFee()
		{
			var declaration = Factory.New<JobDeclaration>();
			var logger = new DetailedLoggerForTest();

			CombineAssertions(() =>
			{
				AssertShouldBrokerPayThisFeeResponseAndLoggerForPaymentMethod(declaration, logger, "B", false, "B (not empty or A)");

				AssertShouldBrokerPayThisFeeResponseAndLoggerForPaymentMethod(declaration, logger, ZString.Empty, true, "empty");

				AssertShouldBrokerPayThisFeeResponseAndLoggerForPaymentMethod(declaration, logger, "C", false, "C (not empty or A)");

				AssertShouldBrokerPayThisFeeResponseAndLoggerForPaymentMethod(declaration, logger, "A", true, "A");

				AssertShouldBrokerPayThisFeeResponseAndLoggerForPaymentMethod(declaration, logger, "D", false, "D (not empty or A)");
			});
		}

		void AssertShouldBrokerPayThisFeeResponseAndLoggerForPaymentMethod(JobDeclaration declaration, DetailedLoggerForTest logger, ZString paymentMethod, bool expectedIncluded, string assertMessage)
		{
			var expectedLogText = "Fee A00 with MoP=Y with deferral payment party " + (paymentMethod.IsEmpty ? (ZString)"empty" : paymentMethod) + (expectedIncluded ? " is always paid by broker - included in rating" : " is never paid by broker - excluded from rating");
			declaration.JE_PaymentMethod = paymentMethod;
			var entryFeePaymentPartyUnderstander = new EntryFeePaymentPartyUnderstander(declaration);
			AssertEquals("When JE_PaymentMethod is " + assertMessage + " ShouldBrokerPayThisFee should return", expectedIncluded, entryFeePaymentPartyUnderstander.ShouldBrokerPayThisFee("A00", "Y", logger));
			AssertEquals("When JE_PaymentMethod is " + assertMessage + " logger should contain a log indicating the fee is " + (expectedIncluded ? "" : "not") + " included in rating", true, logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == expectedLogText));
			logger.Logs.Clear();
		}
	}
}
