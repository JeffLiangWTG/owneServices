using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ElectronicMessaging.Latvia
{
	public class LatviaEInvoiceAPICommandList : CodeDescriptionPairList
	{
		[EInvoiceMessageSubType]
		public static class Codes
		{
			public const string GenerateInvoiceRequest = EInvoiceAPICommandList.Codes.GenerateInvoiceRequest;
		}

		public LatviaEInvoiceAPICommandList()
		{
			AddPair(Codes.GenerateInvoiceRequest, Res.GetString("ea671a29-54fd-46f4-af42-9daa97cab0dc", "Generate Invoice Request"));
		}
	}
}
