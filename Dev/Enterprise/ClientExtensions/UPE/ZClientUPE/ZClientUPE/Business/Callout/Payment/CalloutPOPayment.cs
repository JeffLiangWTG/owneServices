using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business
{
	public class CalloutPOPayment : CalloutPaymentDetails
	{
		public CalloutPOPayment(Callout callout)
			: base(callout)
		{
		}

		[CargoWise.ComponentModel.MaxLength(35)]
		public ZString PurchaseOrderNumber
		{
			get { return fPurchaseOrderNumber; }
			set
			{
				if (fPurchaseOrderNumber != value)
				{
					CheckMaximumLength(PurchaseOrderNumberInfo, value);
					SetNonPersistentPropertyValue(PurchaseOrderNumberInfo, ref	fPurchaseOrderNumber, value);
				}
			}
		}

		public ZPropertyInfo PurchaseOrderNumberInfo
		{
			get { return GetZPropertyInfo(nameof(PurchaseOrderNumber)); }
		}

		[CargoWise.ComponentModel.MaxLength(35)]
		public ZString PurchaseOrderName
		{
			get { return fPurchaseOrderName; }
			set
			{
				if (fPurchaseOrderName != value)
				{
					CheckMaximumLength(PurchaseOrderNameInfo, value);
					SetNonPersistentPropertyValue(PurchaseOrderNameInfo, ref	fPurchaseOrderName, value);
				}
			}
		}

		public ZPropertyInfo PurchaseOrderNameInfo
		{
			get { return GetZPropertyInfo(nameof(PurchaseOrderName)); }
		}

		protected override string PaymentMethodAsText
		{
			get { return "Purchase Order"; }
		}

		protected override string Reference
		{
			get { return string.Format("Purchase Order Number: {0}\r\nPurchase Order Name: {1}", PurchaseOrderNumber, PurchaseOrderName); }
		}

		ZString fPurchaseOrderNumber;
		ZString fPurchaseOrderName;
	}
}
