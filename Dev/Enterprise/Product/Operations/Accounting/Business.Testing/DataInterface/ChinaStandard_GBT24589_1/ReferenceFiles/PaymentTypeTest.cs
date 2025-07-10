using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1.Testing
{
	using CargoWise.EntityFramework.Testing;
	using NUnit.Framework;

	[TestedType(typeof(PaymentType))]
	public class PaymentTypeTest : NonPersistentBusinessObjectTestCase
	{
		public void TestClassProperties()
		{
			AssertEquals("T106", PaymentType.LocID);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PaymentType();
		}
	}

	[TestedType(typeof(PaymentTypeCollection))]
	public class PaymentTypeCollectionTest : NonPersistentBusinessObjectCollectionTestCase<PaymentTypeCollection>
	{
		public void TestDefaultValues()
		{
			PaymentTypeCollection collection = new PaymentTypeCollection(Factory);
			AssertEquals(20, collection.Count);
			AssertEquals(ReceiptTypes.Cheque, collection[0].PaymentTypeCode);
			AssertEquals("支票", collection[0].PaymentTypeName);
			AssertEquals(ReceiptTypes.CreditCard, collection[1].PaymentTypeCode);
			AssertEquals("信用卡", collection[1].PaymentTypeName);
			AssertEquals(ReceiptTypes.Cash, collection[2].PaymentTypeCode);
			AssertEquals("现金", collection[2].PaymentTypeName);
			AssertEquals(ReceiptTypes.DirectDebit, collection[3].PaymentTypeCode);
			AssertEquals("直接付款", collection[3].PaymentTypeName);
			AssertEquals(ReceiptTypes.DirectCredit, collection[4].PaymentTypeCode);
			AssertEquals("直接入账", collection[4].PaymentTypeName);
			AssertEquals(ReceiptTypes.EFT, collection[5].PaymentTypeCode);
			AssertEquals("电子转账", collection[5].PaymentTypeName);
			AssertEquals(ReceiptTypes.ScheduledEFT, collection[6].PaymentTypeCode);
			AssertEquals("预定电子转账", collection[6].PaymentTypeName);
			AssertEquals(ReceiptTypes.CollectionRequest, collection[7].PaymentTypeCode);
			AssertEquals("托收申请", collection[7].PaymentTypeName);
			AssertEquals(ReceiptTypes.eNettDirectDebit, collection[8].PaymentTypeCode);
			AssertEquals("通过ComPay直接扣款", collection[8].PaymentTypeName);
			AssertEquals(ReceiptTypes.eNettCreditCard, collection[9].PaymentTypeCode);
			AssertEquals("通过ComPay 信用卡付款", collection[9].PaymentTypeName);
			AssertEquals(ReceiptTypes.eNettDirectCredit, collection[10].PaymentTypeCode);
			AssertEquals("通过ComPay 直接入账", collection[10].PaymentTypeName);
			AssertEquals(ReceiptTypes.AccountMaintenanceFee, collection[11].PaymentTypeCode);
			AssertEquals("帐户维护费", collection[11].PaymentTypeName);
			AssertEquals(ReceiptTypes.BankDebitTax, collection[12].PaymentTypeCode);
			AssertEquals("银行直接扣税", collection[12].PaymentTypeName);
			AssertEquals(ReceiptTypes.BankDepositFee, collection[13].PaymentTypeCode);
			AssertEquals("银行存款费", collection[13].PaymentTypeName);
			AssertEquals(ReceiptTypes.InterestPaid, collection[14].PaymentTypeCode);
			AssertEquals("支付利息", collection[14].PaymentTypeName);
			AssertEquals(ReceiptTypes.InterestReceived, collection[15].PaymentTypeCode);
			AssertEquals("收到利息", collection[15].PaymentTypeName);
			AssertEquals(ReceiptTypes.PeriodicPayment, collection[16].PaymentTypeCode);
			AssertEquals("定期付款", collection[16].PaymentTypeName);
			AssertEquals(ReceiptTypes.StampDuty, collection[17].PaymentTypeCode);
			AssertEquals("印花税", collection[17].PaymentTypeName);
			AssertEquals(ReceiptTypes.MiscellaneousReceipt, collection[18].PaymentTypeCode);
			AssertEquals("杂项入账", collection[18].PaymentTypeName);
			AssertEquals(ReceiptTypes.MiscellaneousFees, collection[19].PaymentTypeCode);
			AssertEquals("杂费", collection[19].PaymentTypeName);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PaymentType();
		}

		protected override PaymentTypeCollection GetCollectionToTest()
		{
			return new PaymentTypeCollection(Factory);
		}
	}
}
