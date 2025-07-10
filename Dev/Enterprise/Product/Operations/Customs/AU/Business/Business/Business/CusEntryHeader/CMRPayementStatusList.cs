using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMREntryPaymentStatusList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string NoAmountDue = "NAD";
			public const string NotPaid = "";
			public const string Paid = "PAD";
			public const string PayPending = "PPE";
			public const string PayRejected = "PRJ";
			public const string Refunded = "RFD";
			public const string RefundPending = "RPE";
			public const string RefundRejected = "RRJ";
			public const string PayAckPending = "PAP";
		}

		public static class Descriptions
		{
			public const string NoAmountDue = "No Payment Required";
			public const string NotPaid = "Not Paid";
			public const string Paid = "Paid";
			public const string PayPending = "Pay Pending";
			public const string PayRejected = "Pay Rejected";
			public const string Refunded = "Refunded";
			public const string RefundPending = "Refund Pending";
			public const string RefundRejected = "Refund Rejected";
			public const string PayAckPending = "Pay Sent, Acknowledge Pending";
		}

		public CMREntryPaymentStatusList()
		{
			AddPair(Codes.NoAmountDue, Descriptions.NoAmountDue);
			AddPair(Codes.NotPaid, Descriptions.NotPaid);
			AddPair(Codes.Paid, Descriptions.Paid);
			AddPair(Codes.PayPending, Descriptions.PayPending);
			AddPair(Codes.PayRejected, Descriptions.PayRejected);
			AddPair(Codes.Refunded, Descriptions.Refunded);
			AddPair(Codes.RefundPending, Descriptions.RefundPending);
			AddPair(Codes.RefundRejected, Descriptions.RefundRejected);
			AddPair(Codes.PayAckPending, Descriptions.PayAckPending);
		}

		public static bool IsClearedStatus(string statusCode)
		{
			return statusCode == Codes.Paid || statusCode == Codes.Refunded;
		}

		public static bool IsPaymentMessageSentOrCleared(string statusCode)
		{
			return statusCode == Codes.PayAckPending || IsClearedStatus(statusCode);
		}

		public static bool ArePaymentDetailsRequiredForAmendmentOrWithdrawal(string statusCode)
		{
			return IsPaymentMessageSentOrCleared(statusCode)
				|| statusCode == Codes.RefundRejected
				|| statusCode == Codes.RefundPending;
		}
	}
}
