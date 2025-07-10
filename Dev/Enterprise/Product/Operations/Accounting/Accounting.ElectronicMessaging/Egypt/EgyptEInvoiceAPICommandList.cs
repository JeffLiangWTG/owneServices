using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ElectronicMessaging.Egypt
{
	public class EgyptEInvoiceAPICommandList : CodeDescriptionPairList
	{
		[EInvoiceMessageSubType]
		public static class Codes
		{
			public const string GenerateInvoiceSubmission = EInvoiceAPICommandList.Codes.GenerateInvoiceRequest;
		}

		public EgyptEInvoiceAPICommandList()
		{
			AddPair(Codes.GenerateInvoiceSubmission, Res.GetString("9ab4b3b7-783a-497e-98ef-f5f453e5a158", "Generate Invoice Submission"));
		}
	}
}
