using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ElectronicMessaging.Romania
{
	public class RomaniaEInvoiceAPICommandList : CodeDescriptionPairList
	{
		[EInvoiceMessageSubType]
		public static class Codes
		{
			public const string GenerateInvoiceRequest = EInvoiceAPICommandList.Codes.GenerateInvoiceRequest;
			public const string QueryInvoiceRequest = EInvoiceAPICommandList.Codes.QueryInvoiceRequest;
			public const string RomaniaInvoiceRefreshAccessToken = EInvoiceAPICommandList.Codes.RomaniaInvoiceRefreshAccessToken;
		}
	}
}
