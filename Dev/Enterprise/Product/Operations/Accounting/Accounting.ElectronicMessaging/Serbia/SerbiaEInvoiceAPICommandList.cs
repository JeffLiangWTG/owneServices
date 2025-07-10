using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ElectronicMessaging.Serbia
{
	public class SerbiaEInvoiceAPICommandList : CodeDescriptionPairList
	{
		[EInvoiceMessageSubType]
		public static class Codes
		{
			public const string GenerateInvoiceSubmission = EInvoiceAPICommandList.Codes.GenerateInvoiceRequest;
		}

		public SerbiaEInvoiceAPICommandList()
		{
			AddPair(Codes.GenerateInvoiceSubmission, Res.GetString("24f5ac65-ab2d-487d-8cd1-22e755d14071", "Generate Invoice Submission"));
		}
	}
}
