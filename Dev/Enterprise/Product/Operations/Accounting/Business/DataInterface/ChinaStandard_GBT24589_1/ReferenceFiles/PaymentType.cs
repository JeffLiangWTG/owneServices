using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1
{
	public sealed class PaymentType : NonPersistentBusinessObject, IObsoleteValidation
	{
		public const string LocID = "T106";
		public ZString PaymentTypeCode { get; set; }
		public ZString PaymentTypeName { get; set; }
	}

	public sealed class PaymentTypeCollection : NonPersistentBusinessObjectCollection<PaymentType>	{
		public PaymentTypeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			AddDefaultElements();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PaymentType();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "China's Accounting fixed value")]
		void AddDefaultElements()
		{
			PaymentType paymentType = AddNew();
			paymentType.PaymentTypeCode = ReceiptTypes.Cheque;
			paymentType.PaymentTypeName = "支票";

			paymentType = AddNew();
			paymentType.PaymentTypeCode = ReceiptTypes.CreditCard;
			paymentType.PaymentTypeName = "信用卡";

			paymentType = AddNew();
			paymentType.PaymentTypeCode = ReceiptTypes.Cash;
			paymentType.PaymentTypeName = "现金";

			paymentType = AddNew();
			paymentType.PaymentTypeCode = ReceiptTypes.DirectDebit;
			paymentType.PaymentTypeName = "直接付款";

			paymentType = AddNew();
			paymentType.PaymentTypeCode = ReceiptTypes.DirectCredit;
			paymentType.PaymentTypeName = "直接入账";

			paymentType = AddNew();
			paymentType.PaymentTypeCode = ReceiptTypes.EFT;
			paymentType.PaymentTypeName = "电子转账";

			paymentType = AddNew();
			paymentType.PaymentTypeCode = ReceiptTypes.ScheduledEFT;
			paymentType.PaymentTypeName = "预定电子转账";

			paymentType = AddNew();
			paymentType.PaymentTypeCode = ReceiptTypes.CollectionRequest;
			paymentType.PaymentTypeName = "托收申请";

			paymentType = AddNew();
			paymentType.PaymentTypeCode = ReceiptTypes.eNettDirectDebit;
			paymentType.PaymentTypeName = "通过ComPay直接扣款";

			paymentType = AddNew();
			paymentType.PaymentTypeCode = ReceiptTypes.eNettCreditCard;
			paymentType.PaymentTypeName = "通过ComPay 信用卡付款";

			paymentType = AddNew();
			paymentType.PaymentTypeCode = ReceiptTypes.eNettDirectCredit;
			paymentType.PaymentTypeName = "通过ComPay 直接入账";

			paymentType = AddNew();
			paymentType.PaymentTypeCode = ReceiptTypes.AccountMaintenanceFee;
			paymentType.PaymentTypeName = "帐户维护费";

			paymentType = AddNew();
			paymentType.PaymentTypeCode = ReceiptTypes.BankDebitTax;
			paymentType.PaymentTypeName = "银行直接扣税";

			paymentType = AddNew();
			paymentType.PaymentTypeCode = ReceiptTypes.BankDepositFee;
			paymentType.PaymentTypeName = "银行存款费";

			paymentType = AddNew();
			paymentType.PaymentTypeCode = ReceiptTypes.InterestPaid;
			paymentType.PaymentTypeName = "支付利息";

			paymentType = AddNew();
			paymentType.PaymentTypeCode = ReceiptTypes.InterestReceived;
			paymentType.PaymentTypeName = "收到利息";

			paymentType = AddNew();
			paymentType.PaymentTypeCode = ReceiptTypes.PeriodicPayment;
			paymentType.PaymentTypeName = "定期付款";

			paymentType = AddNew();
			paymentType.PaymentTypeCode = ReceiptTypes.StampDuty;
			paymentType.PaymentTypeName = "印花税";

			paymentType = AddNew();
			paymentType.PaymentTypeCode = ReceiptTypes.MiscellaneousReceipt;
			paymentType.PaymentTypeName = "杂项入账";

			paymentType = AddNew();
			paymentType.PaymentTypeCode = ReceiptTypes.MiscellaneousFees;
			paymentType.PaymentTypeName = "杂费";
		}
	}
}

