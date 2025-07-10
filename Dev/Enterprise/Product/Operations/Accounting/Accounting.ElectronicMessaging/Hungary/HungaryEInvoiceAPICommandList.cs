using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ElectronicMessaging.Hungary
{
	public class HungaryEInvoiceAPICommandList : CodeDescriptionPairList
	{
		[EInvoiceMessageSubType]
		public static class Codes
		{
			public const string GenerateInvoiceRequest = EInvoiceAPICommandList.Codes.GenerateInvoiceRequest;
		}

		public static class Descriptions
		{
			public static MultilingualString GenerateInvoiceRequest { get { return ResString.GetMultilingualString("HungaryEInvoiceCommandList|GenerateInvoiceRequest", "Generate Invoice Request"); } }
		}

		public HungaryEInvoiceAPICommandList()
		{
			AddPair(Codes.GenerateInvoiceRequest, Descriptions.GenerateInvoiceRequest);
		}
	}
}
