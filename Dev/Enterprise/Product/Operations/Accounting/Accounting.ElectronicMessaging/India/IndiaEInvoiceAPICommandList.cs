using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ElectronicMessaging.India
{
	public class IndiaEInvoiceAPICommandList : CodeDescriptionPairList
	{
		[EInvoiceMessageSubType]
		public static class Codes
		{
			public const string GenerateIRN = EInvoiceAPICommandList.Codes.GenerateIRN;
		}

		public static class Descriptions
		{
			public static MultilingualString GenerateIRN { get { return ResString.GetMultilingualString("IndiaEInvoiceCommandList|GenerateIRN", "Generate IRN"); } }
		}

		public IndiaEInvoiceAPICommandList()
		{
			AddPair(Codes.GenerateIRN, Descriptions.GenerateIRN);
		}
	}
}
