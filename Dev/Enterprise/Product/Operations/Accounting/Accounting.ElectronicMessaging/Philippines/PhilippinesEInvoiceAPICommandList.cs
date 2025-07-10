using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ElectronicMessaging.Philippines
{
	public class PhilippinesEInvoiceAPICommandList : CodeDescriptionPairList
	{
		[EInvoiceMessageSubType]
		public static class Codes
		{
			public const string GenerateSubmitTaxInvoice = EInvoiceAPICommandList.Codes.GenerateInvoiceRequest;
		}
	}
}
