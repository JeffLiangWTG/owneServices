using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	[TestedType(typeof(ARPaymentApprovalMatching))]
	public class ARPaymentApprovalMatchingBaseTest : PaymentApprovalMatchingBaseTest
	{
		public override void AssertRecordCriticalValidationExceptionWithMiscTranasactionWhenBothMatchFails(string exceptionMessage)
		{
			var expectError = $@"Balance is not zero, Balance is 98.0

PaymentApprovalBaseMatchDetails:
PaymentApprovalMatchingBase.Match()
Current company's currency Code: AUD, Decimals: 2
Ledger: AR, Transaction type: UNA, Payment Amount: 105, Outstanding Amount: 105, OS Payment Amount: 105, OS Outstanding Amount: 105, Currency: AUD, Exchange Rate Amount: 1
Ledger: AP, Transaction type: INV, Payment Amount: -100, Outstanding Amount: -100, OS Payment Amount: -100, OS Outstanding Amount: -100, Currency: AUD, Exchange Rate Amount: 1

MatchingBase.Match()
Current company's currency Code: AUD, Decimals: 2
Ledger: AR, Transaction type: PAY, Payment Amount: 105, Outstanding Amount: 105, OS Payment Amount: 105, OS Outstanding Amount: 105, Currency: AUD, Exchange Rate Amount: 1
Ledger: AR, Transaction type: EXX, Payment Amount: -7, Outstanding Amount: -7, OS Payment Amount: -7, OS Outstanding Amount: -7, Currency: AUD, Exchange Rate Amount: 1

There are matching errors:
Balance of transaction match result is not zero.

MatchedTransactions contains transaction from primary organization: true

Match group info: There is no data collected.";

			AssertContains(expectError, exceptionMessage);
		}

		public override void AssertRecordCriticalValidationExceptionWithMiscTranasactionWhen2ndMatchFails(string exceptionMessage)
		{
			var expectError = @"Balance is not zero, Balance is -2.0

PaymentApprovalBaseMatchDetails:
PaymentApprovalMatchingBase.Match()
Current company's currency Code: AUD, Decimals: 2
Ledger: AR, Transaction type: UNA, Payment Amount: 105, Outstanding Amount: 105, OS Payment Amount: 105, OS Outstanding Amount: 105, Currency: AUD, Exchange Rate Amount: 1
Ledger: AP, Transaction type: INV, Payment Amount: -100, Outstanding Amount: -100, OS Payment Amount: -100, OS Outstanding Amount: -100, Currency: AUD, Exchange Rate Amount: 1

MatchingBase.Match()
Current company's currency Code: AUD, Decimals: 2
Ledger: AR, Transaction type: PAY, Payment Amount: 105, Outstanding Amount: 105, OS Payment Amount: 105, OS Outstanding Amount: 105, Currency: AUD, Exchange Rate Amount: 1
Ledger: AR, Transaction type: EXX, Payment Amount: -7, Outstanding Amount: -7, OS Payment Amount: -7, OS Outstanding Amount: -7, Currency: AUD, Exchange Rate Amount: 1
Ledger: AP, Transaction type: INV, Payment Amount: -100, Outstanding Amount: -100, OS Payment Amount: -100, OS Outstanding Amount: -100, Currency: AUD, Exchange Rate Amount: 1

There are matching errors:
Balance of transaction match result is not zero.

MatchedTransactions contains transaction from primary organization: true

Match group info: There is no data collected.";

			AssertContains(expectError, exceptionMessage);
		}

		#region Inherited Tests

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetTestMatchingBase();
		}

		protected override MatchingBase GetTestMatchingBase()
		{
			PaymentApprovalBase aRApproval = GetNewPaymentApproval();
			return new ARPaymentApprovalMatching(Factory, aRApproval);
		}

		protected override MatchingBase GetTestMatchingBaseInNewFactory(BusinessObjectFactory factoryForNewMatchingObject)
		{
			PaymentApprovalBase aRApproval = GetNewPaymentApproval();
			return new ARPaymentApprovalMatching(factoryForNewMatchingObject, aRApproval);
		}

		protected override PaymentApprovalBase GetNewPaymentApproval()
		{
			PaymentApprovalBase aRApproval = Factory.New<ARPaymentApprovalWithAuthorisation>();
			aRApproval.AV_OH = TestOrgHeader.PK;
			return aRApproval;
		}

		protected override Type InvoiceType
		{
			get { return typeof(ARInvoice); }
		}

		protected override ZString LedgerType
		{
			get { return LedgerTypes.AccountsReceivable; }
		}

		protected override MatchingBase GetTestMatchingBaseWithReceiptPaymentDetail(ReceiptPaymentBase receiptPaymentBase)
		{
			//ARPaymentApprovalMatching does not have ReceiptPaymentDetail
			throw new NotImplementedException();
		}

		#region TestMatchDateDefaultValue

		public override void TestMatchDateDefaultValue()
		{
			base.TestMatchDateDefaultValue();
			PaymentApprovalBase aRApproval = Factory.New<ARPaymentApprovalWithAuthorisation>();
			ZDateTime expectedDate = ZDateTime.Today;
			aRApproval.AV_PostDate = expectedDate;
			MatchingBase testMatchingBase = new ARPaymentApprovalMatching(Factory, aRApproval);
			AssertEquals(string.Format("ARPaymentApprovalMatching's default MatchDate should be {0}", expectedDate), expectedDate, testMatchingBase.MatchDate);
		}

		#endregion

		#region TestMatchLinksPostDate
		[TestDate(2020, 1, 8, 00, 00, 0)]
		public override void TestMatchLinksMatchDate()
		{
			SetUpTestDataSet();
			Factory.Save();

			Period testPeriod = Factory.NewWithValidTestData<Period>();
			testPeriod.AM_Year = (ZShort)ZDateTime.Now.Year;
			testPeriod.AM_StartDate = new ZDateTime(testPeriod.AM_Year, ZDateTime.Today.Month, 1);
			testPeriod.AM_EndDate = testPeriod.AM_StartDate.AddMonths(1).AddDays(-1);

			PaymentApprovalBase aRApproval = Factory.New<ARPaymentApprovalWithoutAuthorisation>();
			aRApproval.AV_OH = TestOrg1.PK;
			aRApproval.AV_Amount = 10M;
			aRApproval.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.EFT;
			aRApproval.AV_AB = TestObjectCreator.AUDBankAccount.PK;
			aRApproval.AV_ChequeOrReference = "121";

			fTestMatchingBase = aRApproval.PaymentMatchingBaseObject;

			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			ExchangeDifference testDSC = (ExchangeDifference)TestMatchingBase.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.ExchangeDifference);
			TestMatchingBase.AddMiscellaneousTransaction(testDSC);
			TestMatchingBase.MatchedTransactions.SetPartialPaidAmount();

			var expectedDate = ZDateTime.Today.AddDays(-7);
			TestMatchingBase.MatchDate = expectedDate;
			TestMatchingBase.MatchAndClearTransactions();
			testDSC.AH_PostDate = expectedDate;
			aRApproval.PostDate = expectedDate;
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Factory.Save();

			TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory);
			matchLinks.Load(new ZQuery());
			AssertEquals("4 Match link Rows should be created", 2, matchLinks.Count);
			foreach (TransactionMatchLink matchLink in matchLinks)
			{
				AssertEquals("MatchLink should have the same Match Date as in MatchingBase", expectedDate, matchLink.AP_MatchDate);
			}
		}

		#endregion

		#endregion
	}
}
