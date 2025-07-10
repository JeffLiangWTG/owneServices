using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ElectronicMessaging.Vietnam
{
	public class VietnamEInvoiceAPICommandList : CodeDescriptionPairList
	{
		[EInvoiceMessageSubType]
		public static class Codes
		{
			public const string SendReceivablesInvoice = EInvoiceAPICommandList.Codes.SendReceivablesInvoice;
			public const string CancelReceivablesInvoice = EInvoiceAPICommandList.Codes.CancelIRN;
			public const string CancelReceivablesCircular78Invoice = EInvoiceAPICommandList.Codes.CancelReceivablesCircular78Invoice;
			public const string RequestDocumentForInvoice = EInvoiceAPICommandList.Codes.RequestDocumentForInvoice;
			public const string AdjustReceivablesInvoice = EInvoiceAPICommandList.Codes.AdjustReceivablesInvoice;
			public const string ApproveReceivablesInvoice = EInvoiceAPICommandList.Codes.ApproveReceivablesInvoice;
		}

		public static class Descriptions
		{
			public static MultilingualString SendReceivablesInvoice { get { return ResString.GetMultilingualString("VietnamEInvoiceCommandList|SendReceivablesInvoice", "Send Receivables Invoice"); } }
			public static MultilingualString CancelReceivablesInvoice { get { return ResString.GetMultilingualString("VietnamEInvoiceCommandList|CancelReceivablesInvoice", "Cancel Receivables Invoice"); } }
			public static MultilingualString CancelReceivablesCircular78Invoice { get { return ResString.GetMultilingualString("VietnamEInvoiceCommandList|CancelReceivablesCircular78Invoice", "Cancel Receivables Circular 78 Invoice"); } }
			public static MultilingualString RequestDocumentForInvoice { get { return ResString.GetMultilingualString("VietnamEInvoiceCommandList|RequestDocumentForInvoice", "Request Document For Invoice"); } }
			public static MultilingualString AdjustReceivablesInvoice { get { return ResString.GetMultilingualString("VietnamEInvoiceCommandList|AdjustReceivablesInvoiceInvoice", "Adjust Receivables Invoice"); } }
			public static MultilingualString ApproveReceivablesInvoice { get { return ResString.GetMultilingualString("VietnamEInvoiceCommandList|ApproveReceivablesInvoice", "Approve Receivables Invoice"); } }
		}

		public VietnamEInvoiceAPICommandList()
		{
			AddPair(Codes.SendReceivablesInvoice, Descriptions.SendReceivablesInvoice);
			AddPair(Codes.CancelReceivablesInvoice, Descriptions.CancelReceivablesInvoice);
			AddPair(Codes.CancelReceivablesCircular78Invoice, Descriptions.CancelReceivablesCircular78Invoice);
			AddPair(Codes.RequestDocumentForInvoice, Descriptions.RequestDocumentForInvoice);
			AddPair(Codes.AdjustReceivablesInvoice, Descriptions.AdjustReceivablesInvoice);
			AddPair(Codes.ApproveReceivablesInvoice, Descriptions.ApproveReceivablesInvoice);
		}

		public static string GetMessageType(ZString actionType)
		{
			switch (actionType)
			{
				case Core.Constants.EInvoicingPivotActionType.Submit:
					return Codes.SendReceivablesInvoice;
				case Core.Constants.EInvoicingPivotActionType.Cancel:
					return Codes.CancelReceivablesInvoice;
				case Core.Constants.EInvoicingPivotActionType.DocumentAction:
					return Codes.RequestDocumentForInvoice;
				case Core.Constants.EInvoicingPivotActionType.Adjustment:
					return Codes.AdjustReceivablesInvoice;
				case Core.Constants.EInvoicingPivotActionType.Approve:
					return Codes.ApproveReceivablesInvoice;
				default:
					return string.Empty;
			}
		}
	}
}
