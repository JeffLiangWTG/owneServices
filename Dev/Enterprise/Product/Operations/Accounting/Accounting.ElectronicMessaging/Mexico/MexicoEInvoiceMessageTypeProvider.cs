using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Messaging.Integration;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico
{
	public static class MexicoEInvoiceMessageTypeProvider
	{
		[EInvoiceMessageSubType]
		public static class Codes
		{
			public const string GenerateInvoiceRequest = EInvoiceAPICommandList.Codes.Request;
			public const string GenerateCancellationRequest = EInvoiceAPICommandList.Codes.GenerateCancellationRequest;
			public const string RequestPDFDocumentForInvoice = EInvoiceAPICommandList.Codes.RequestPDFDocumentForInvoice;
		}

		public static ZString GetMessageType(AccEInvoicingTransactionPivot pivot)
		{
			var invoicing = pivot.ParentTransactionHeader as InvoicingBase;
			var pivotActionType = pivot.AIP_ActionType;

			switch (pivotActionType)
			{
				case Core.Constants.EInvoicingPivotActionType.DocumentAction:
					return Codes.RequestPDFDocumentForInvoice;
				default:
					return string.IsNullOrEmpty(invoicing.ReversalStatusCode)
						? Codes.GenerateInvoiceRequest
						: Codes.GenerateCancellationRequest;
			}
		}
	}
}
