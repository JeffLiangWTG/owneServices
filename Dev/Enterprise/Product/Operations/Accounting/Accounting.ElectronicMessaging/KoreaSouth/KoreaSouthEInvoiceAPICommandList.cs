using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Messaging.Integration;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth
{
	public static class KoreaSouthEInvoiceAPICommandList
	{
		[EInvoiceMessageSubType]
		public static class Codes
		{
			public const string GenerateInvoiceRequest = EInvoiceAPICommandList.Codes.GenerateInvoiceRequest;
			public const string QueryInvoiceRequest = EInvoiceAPICommandList.Codes.QueryInvoiceRequest;
			public const string CallbackInvoceRequest = EInvoiceAPICommandList.Codes.CallbackInvoceRequest;
		}
	}
}
