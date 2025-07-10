using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business
{
	public class CalloutEFTPayment : CalloutPaymentDetails
	{
		public CalloutEFTPayment(Callout callout)
			: base(callout)
		{
		}

		[CargoWise.ComponentModel.MaxLength(6)]
		public ZString BSBNumber
		{
			get { return fBSBNumber; }
			set
			{
				if (fBSBNumber != value)
				{
					CheckMaximumLength(BSBNumberInfo, value);
					SetNonPersistentPropertyValue(BSBNumberInfo, ref fBSBNumber, value);
				}
			}
		}

		public ZPropertyInfo BSBNumberInfo
		{
			get { return GetZPropertyInfo(nameof(BSBNumber)); }
		}

		[CargoWise.ComponentModel.MaxLength(9)]
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

		[CargoWise.ComponentModel.MaxLength(35)]
		public ZString AccountName
		{
			get { return fAccountName; }
			set
			{
				if (fAccountName != value)
				{
					CheckMaximumLength(AccountNameInfo, value);
					SetNonPersistentPropertyValue(AccountNameInfo, ref fAccountName, value);
				}
			}
		}

		public ZPropertyInfo AccountNameInfo
		{
			get { return GetZPropertyInfo(nameof(AccountName)); }
		}

		protected override string PaymentMethodAsText
		{
			get { return "Electronic Fund Transfer (EFT)"; }
		}

		protected override string Reference
		{
			get { return string.Format("BSB Number: {0}\r\nAccount Number: {1}\r\nAccount Name: {2}", BSBNumber, AccountNumber, AccountName); }
		}

		ZString fBSBNumber;
		ZString fAccountNumber;
		ZString fAccountName;
	}
}
