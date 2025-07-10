using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ElectronicMessaging.Spain
{
	public class SpainEInvoiceAPICommandList : CodeDescriptionPairList
	{
		[EInvoiceMessageSubType]
		public static class Codes
		{
			public const string GenerateInvoiceRequest = EInvoiceAPICommandList.Codes.GenerateInvoiceRequest;
		}

		public SpainEInvoiceAPICommandList()
		{
			AddPair(Codes.GenerateInvoiceRequest, Res.GetString("adbe5e97-efee-474f-985a-6650f0afc0cd", "Generate Invoice Request"));
		}
	}
}
