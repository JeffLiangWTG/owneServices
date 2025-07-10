using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class PaymentTransactionTypeList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string CustomsChargePayment = "CPY";
			public const string AQISPayment = "QPY";
			public const string CustomsAQISPayment = "CQP";
			public const string Refund = "REF";
		}

		public static class Descriptions
		{
			public const string CustomsChargePayment = "Customs Charge Payment";
			public const string AQISPayment = "Quarantine Payment";
			public const string CustomsAQISPayment = "Customs Charge and Quarantine Payment";
			public const string Refund = "Refund";
		}

		public PaymentTransactionTypeList()
		{
			AddPair(Codes.CustomsChargePayment, Descriptions.CustomsChargePayment);
			AddPair(Codes.AQISPayment, Descriptions.AQISPayment);
			AddPair(Codes.CustomsAQISPayment, Descriptions.CustomsAQISPayment);
			AddPair(Codes.Refund, Descriptions.Refund);
		}
	}
}
