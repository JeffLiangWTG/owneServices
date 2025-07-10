using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business
{
	public class CalloutAccountPayment : CalloutPaymentDetails
	{
		public CalloutAccountPayment(Callout callout)
			: base(callout)
		{
		}

		[CargoWise.ComponentModel.MaxLength(35)]
		public ZString AccountNumber
		{
			get { return fAccountNumber; }
			set
			{
				if (fAccountNumber != value)
				{
					CheckMaximumLength(AccountNumberInfo, value);
					SetNonPersistentPropertyValue(AccountNumberInfo, ref fAccountNumber, value);
				}
			}
		}

		public ZPropertyInfo AccountNumberInfo
		{
			get { return GetZPropertyInfo(nameof(AccountNumber)); }
		}

		protected override string Reference
		{
			get { return "Account Number: " + AccountNumber; }
		}

		protected override string PaymentMethodAsText
		{
			get { return "Account"; }
		}

		ZString fAccountNumber;
	}
}
