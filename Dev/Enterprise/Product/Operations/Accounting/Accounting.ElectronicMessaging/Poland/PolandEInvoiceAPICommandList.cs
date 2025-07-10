using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ElectronicMessaging.Poland
{
	public class PolandEInvoiceAPICommandList : CodeDescriptionPairList
	{
		[EInvoiceMessageSubType]
		public static class Codes
		{
			// Corresponds to the KSeF endpoint: /online/Invoice/Send
			// OR, if we are using the batch functionality: /batch/Upload
			public const string SendInvoiceBatch = EInvoiceAPICommandList.Codes.SendInvoiceBatch;
		}
	}
}
