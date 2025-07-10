using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	public sealed class PaymentApprovalConcurrencyHelperTest : TestCaseWithFactory
	{
		public void TestCreateCheckConcurrencyQuery()
		{
			var paymentApprovalPK = ZGuid.NewZGuid();
			var existedQuotePKs = new List<ZGuid>() { ZGuid.NewZGuid(), ZGuid.NewZGuid() };
			var query = PaymentApprovalConcurrencyHelper.CreateCheckConcurrencyQuery_ForTestOnly(paymentApprovalPK, existedQuotePKs);
			AssertContains($"QU_PK not in", query.LiteralTextSqlFormatted);
			AssertContains($"QU_AV = '{paymentApprovalPK}'", query.LiteralTextSqlFormatted);
		}

		public void TestCheckHasNewQuoteInDB()
		{
			AssertNoExceptionThrown(
				() => PaymentApprovalConcurrencyHelper.CheckAndReportNewQuoteInDB(ZGuid.NewZGuid(), new List<ZGuid>()));

			var approval = CreatePaymentApproval(ZGuid.Empty);
			var quote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(EPaymentStatusCodes.Quote.Accepted, approval);
			Factory.Save();

			var ex = AssertExceptionThrown<ZCannotSaveException>(
				() => PaymentApprovalConcurrencyHelper.CheckAndReportNewQuoteInDB(approval.PK, new List<ZGuid>()));
			AssertContains(expectedConcurrencyErrorMsg, ex.Message);

			AssertNoExceptionThrown(
				() => PaymentApprovalConcurrencyHelper.CheckAndReportNewQuoteInDB(approval.PK, new List<ZGuid>() { quote.PK }));
		}

		public void TestBatchCheckAndReportNewQuoteInDB()
		{
			var approval = CreatePaymentApproval(ZGuid.Empty);
			var quote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(EPaymentStatusCodes.Quote.Accepted, approval);
			Factory.Save();

			AssertNoExceptionThrown(
				() => PaymentApprovalConcurrencyHelper.BatchCheckAndReportNewQuoteInDB(new List<PaymentApprovalBase>() { approval }));

			CreateQuoteInAnotherFactory(approval);

			var ex = AssertExceptionThrown<ZCannotSaveException>(
				() => PaymentApprovalConcurrencyHelper.BatchCheckAndReportNewQuoteInDB(new List<PaymentApprovalBase>() { approval }));
			AssertContains(expectedConcurrencyErrorMsg, ex.Message);
		}

		public void TestBatchCheckHasNewQuoteInDB()
		{
			var quotes = Factory.NewWithValidTestData<AccEPaymentQuote>();
			Factory.Save();

			var queryWithRandomPK = new ZQuery(AccEPaymentQuoteSchema.PK, ZGuid.NewZGuid());
			var queries = new List<ZQuery>();
			var isExisted = PaymentApprovalConcurrencyHelper.CheckHasNewQuoteInDBWithQuery_ForTestOnly(queries);
			AssertEquals(false, isExisted);

			queries.Add(queryWithRandomPK);
			isExisted = PaymentApprovalConcurrencyHelper.CheckHasNewQuoteInDBWithQuery_ForTestOnly(queries);
			AssertEquals(false, isExisted);

			for (var i = 0; i < 12; i++)
			{
				queries.Add(queryWithRandomPK);
			}
			isExisted = PaymentApprovalConcurrencyHelper.CheckHasNewQuoteInDBWithQuery_ForTestOnly(queries);
			AssertEquals(false, isExisted);

			queries = new List<ZQuery>();
			var queryWithSamePK = new ZQuery(AccEPaymentQuoteSchema.PK, quotes.PK);
			queries.Add(queryWithRandomPK);
			queries.Add(queryWithSamePK);
			isExisted = PaymentApprovalConcurrencyHelper.CheckHasNewQuoteInDBWithQuery_ForTestOnly(queries);
			AssertEquals(true, isExisted);

			queries = new List<ZQuery>();
			for (var i = 0; i < 12; i++)
			{
				queries.Add(queryWithRandomPK);
			}
			queries.Add(queryWithSamePK);
			isExisted = PaymentApprovalConcurrencyHelper.CheckHasNewQuoteInDBWithQuery_ForTestOnly(queries);
			AssertEquals(true, isExisted);

			queries = new List<ZQuery>();
			for (var i = 0; i < 12; i++)
			{
				queries.Add(queryWithSamePK);
			}
			isExisted = PaymentApprovalConcurrencyHelper.CheckHasNewQuoteInDBWithQuery_ForTestOnly(queries);
			AssertEquals(true, isExisted);
		}

		AccEPaymentQuote CreateQuoteInAnotherFactory(PaymentApprovalBase approval)
		{
			var tempFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var testObjectCreatorInNewFactory = new TestObjectCreator(tempFactory);
			var quote = testObjectCreatorInNewFactory.CreateValidEPaymentQuoteForStatus(EPaymentStatusCodes.Quote.Accepted, approval);
			tempFactory.Save();

			return quote;
		}

		APPaymentApprovalWithoutAuthorisation CreatePaymentApproval(ZGuid paymentBatchPK)
		{
			var approval = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			approval.AV_RX_NKPaymentCurrency = "CAD";
			approval.AV_Amount = 200m;
			approval.AV_PayExRate = 0.5m;
			approval.InitializeForPaymentBatch(() => false);
			approval.AV_APB_PaymentBatch = paymentBatchPK;
			return approval;
		}

		const string expectedConcurrencyErrorMsg = @"Another user has changed payment information after this form was opened.
Please reload current form to continue.";

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (testObjectCreator == null)
				{
					testObjectCreator = new TestObjectCreator(Factory);
				}
				return testObjectCreator;
			}
		}
		TestObjectCreator testObjectCreator;
	}
}
