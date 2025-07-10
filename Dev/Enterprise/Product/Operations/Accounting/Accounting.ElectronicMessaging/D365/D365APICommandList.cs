using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ElectronicMessaging.D365
{
	public class D365APICommandList : CodeDescriptionPairList
	{
		[EInvoiceMessageSubType]
		public static class Codes
		{
			public const string GenerateInvoiceRequest = EInvoiceAPICommandList.Codes.GenerateInvoiceRequest;
		}

		public D365APICommandList()
		{
			AddPair(Codes.GenerateInvoiceRequest, Res.GetString("5745cc60-71c8-4765-a9cb-a9ded8b63511", "Generate Invoice Request"));
		}
	}
}
