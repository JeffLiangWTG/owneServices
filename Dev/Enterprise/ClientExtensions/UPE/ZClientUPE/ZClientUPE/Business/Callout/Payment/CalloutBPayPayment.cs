using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business
{
	public class CalloutBPayPayment : CalloutPaymentDetails
	{
		public CalloutBPayPayment(Callout callout)
			: base(callout)
		{
		}

		static class Schema
		{
			public const string ReceiptNumber = "ReceiptNumber";
			public const string AccountName = "AccountName";
			public const string DatePaid = "DatePaid";
		}

		[CargoWise.ComponentModel.MaxLength(128)]
		public ZString ReceiptNumber
		{
			get { return fReceiptNumber; }
			set
			{
				if (fReceiptNumber != value)
				{
					SetNonPersistentPropertyValue(ReceiptNumberInfo, ref fReceiptNumber, value);
					ValidateReceiptNumber();
				}
			}
		}

		public void ValidateReceiptNumber()
		{
			if (!IsValidationSuspended)
			{
				ReceiptNumberInfo.ClearAllNotifications();
				if (ReceiptNumber.IsEmpty)
				{
					ReceiptNumberInfo.AddError("Please enter a Receipt Number");
				}
			}
		}

		public ZPropertyInfo ReceiptNumberInfo
		{
			get { return GetZPropertyInfo(Schema.ReceiptNumber); }
		}

		[CargoWise.ComponentModel.MaxLength(128)]
		public ZString AccountName
		{
			get { return fAccountName; }
			set
			{
				if (fAccountName != value)
				{
					SetNonPersistentPropertyValue(AccountNameInfo, ref fAccountName, value);
					ValidateAccountName();
				}
			}
		}

		public void ValidateAccountName()
		{
			if (!IsValidationSuspended)
			{
				AccountNameInfo.ClearAllNotifications();
				if (AccountName.IsEmpty)
				{
					AccountNameInfo.AddError("Please enter an Account Name");
				}
			}
		}

		public ZPropertyInfo AccountNameInfo
		{
			get { return GetZPropertyInfo(Schema.AccountName); }
		}

		public ZDateTime DatePaid
		{
			get { return fDatePaid; }
			set
			{
				if (fDatePaid != value)
				{
					SetNonPersistentPropertyValue(DatePaidInfo, ref fDatePaid, value);
					ValidateDatePaid();
				}
			}
		}

		public void ValidateDatePaid()
		{
			if (!IsValidationSuspended)
			{
				DatePaidInfo.ClearAllNotifications();
				if (DatePaid.IsEmpty || !DatePaid.IsValid)
				{
					DatePaidInfo.AddError("Please enter a valid date");
				}
			}
		}

		public ZPropertyInfo DatePaidInfo
		{
			get { return GetZPropertyInfo(Schema.DatePaid); }
		}

		protected override string PaymentMethodAsText
		{
			get { return "BPay"; }
		}

		protected override string Reference
		{
			get
			{
				return string.Format("Receipt Number: {0}" + System.Environment.NewLine + "Account Name: {1}" + System.Environment.NewLine + "Date Paid: {2}", ReceiptNumber, AccountName, DatePaid.ToShortDateString());
			}
		}

		ZString fReceiptNumber;
		ZString fAccountName;
		ZDateTime fDatePaid;
	}
}
