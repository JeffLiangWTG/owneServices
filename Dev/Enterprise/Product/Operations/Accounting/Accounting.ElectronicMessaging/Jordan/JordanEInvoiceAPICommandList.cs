using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Messaging.Integration;

namespace Enterprise.Accounting.ElectronicMessaging.Jordan
{
	public static class JordanEInvoiceAPICommandList
	{
		[EInvoiceMessageSubType]
		public static class Codes
		{
			public const string SubmitTransaction = EInvoiceAPICommandList.Codes.SubmitTransaction;
		}
	}
}
