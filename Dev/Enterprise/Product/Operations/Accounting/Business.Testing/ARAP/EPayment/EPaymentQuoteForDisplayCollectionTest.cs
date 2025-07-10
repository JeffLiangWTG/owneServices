using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.EPayment;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using QuoteStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Quote;

namespace Enterprise.Accounting.Business.Testing.ARAP.EPayment
{
	[TestedType(typeof(EPaymentQuoteForDisplayCollection))]
	class EPaymentQuoteForDisplayCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EPaymentQuoteForDisplayCollection>
	{
		protected override EPaymentQuoteForDisplayCollection GetCollectionToTest()
		{
			return new EPaymentQuoteForDisplayCollection(new EPaymentQuoteCollection(TestObjectCreator.CreatePaymentApproval(typeof(APPaymentApprovalWithoutAuthorisation), ReceiptTypes.EFT, TestObjectCreator.AUDBankAccount, TestObjectCreator.AUDChequeBook)));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new EPaymentQuoteForDisplay(Factory.New<EPaymentQuote>());
		}

		public void TestDisplayBizObjectsAreTiedToRealObjects()
		{
			var approval = TestObjectCreator.CreatePaymentApproval(typeof(APPaymentApprovalWithoutAuthorisation), ReceiptTypes.EFT, TestObjectCreator.AUDBankAccount, TestObjectCreator.AUDChequeBook);
			approval.AV_RX_NKPaymentCurrency = CurrencyCodes.UnitedStates;
			approval.AV_Amount = 1000;
			approval.AV_PayExRate = 1;
			approval.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX, true);
			approval.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX, true);
			approval.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX, true);
			Factory.Save();
			var realCollection = new EPaymentQuoteCollection(approval);
			realCollection.Load();
			AssertEquals(3, realCollection.Count);

			var displayCollection = new EPaymentQuoteForDisplayCollection(realCollection);
			displayCollection.Load();
			AssertEquals(3, displayCollection.Count);
			AssertEquals(3, realCollection.Count(x => ((EPaymentQuote)x).QU_Status == QuoteStatusCodes.Queued));
			AssertEquals(3, displayCollection.Count(x => ((EPaymentQuoteForDisplay)x).StatusDescription == "QUE - Queued"));

			realCollection[0].QU_Status = QuoteStatusCodes.Requested;

			AssertEquals(2, realCollection.Count(x => ((EPaymentQuote)x).QU_Status == QuoteStatusCodes.Queued));
			AssertEquals(1, realCollection.Count(x => ((EPaymentQuote)x).QU_Status == QuoteStatusCodes.Requested));
			AssertEquals(2, displayCollection.Count(x => ((EPaymentQuoteForDisplay)x).StatusDescription == "QUE - Queued"));
			AssertEquals(1, displayCollection.Count(x => ((EPaymentQuoteForDisplay)x).StatusDescription == "REQ - Requested"));
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
