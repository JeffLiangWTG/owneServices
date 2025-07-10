using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Messaging.Integration;

namespace Enterprise.Accounting.ElectronicMessaging.China
{
	public static class ChinaEInvoiceAPICommandList
	{
		[EInvoiceMessageSubType]
		public static class Codes
		{
			public const string GenerateInvoiceRequest = EInvoiceAPICommandList.Codes.GenerateInvoiceRequest;
			public const string RequestDocumentForInvoice = EInvoiceAPICommandList.Codes.RequestDocumentForInvoice;
		}

		public static string GetMessageType(ZString actionType)
		{
			switch (actionType)
			{
				case Core.Constants.EInvoicingPivotActionType.Submit:
					return Codes.GenerateInvoiceRequest;
				case Core.Constants.EInvoicingPivotActionType.DocumentAction:
					return Codes.RequestDocumentForInvoice;
				default:
					return string.Empty;
			}
		}
	}
}
