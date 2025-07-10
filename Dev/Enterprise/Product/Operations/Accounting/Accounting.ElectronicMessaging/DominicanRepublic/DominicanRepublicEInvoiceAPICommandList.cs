using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Messaging.Integration;

namespace Enterprise.Accounting.ElectronicMessaging.DominicanRepublic
{
	class DominicanRepublicEInvoiceAPICommandList
	{
		[EInvoiceMessageSubType]
		public static class Codes
		{
			public const string GenerateInvoiceRequest = EInvoiceAPICommandList.Codes.GenerateInvoiceRequest;
		}
	}
}
