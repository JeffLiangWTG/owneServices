using Enterprise.Accounting.ElectronicMessaging.Common;

namespace Enterprise.Accounting.ElectronicMessaging.Panama
{
	public static class EInvoiceAPICommandList
	{
		[EInvoiceMessageSubType]
		public static class Codes
		{
			public const string GenerateInvoiceRequest = Messaging.Integration.EInvoiceAPICommandList.Codes.GenerateInvoiceRequest;
		}
	}
}
