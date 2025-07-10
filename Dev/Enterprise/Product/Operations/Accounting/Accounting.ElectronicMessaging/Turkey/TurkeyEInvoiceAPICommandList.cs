using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey
{
	public class TurkeyEInvoiceAPICommandList : CodeDescriptionPairList
	{
		[EInvoiceMessageSubType]
		public static class Codes
		{
			public const string SendReceivablesInvoice = EInvoiceAPICommandList.Codes.ReceivablesInvoice;
			public const string StatusRequestForReceivablesInvoice = EInvoiceAPICommandList.Codes.StatusRequestForReceivablesInvoice;
			public const string RequestPDFofReceivablesInvoice = EInvoiceAPICommandList.Codes.RequestPDFofReceivablesInvoice;
			public const string CancelReceivablesInvoice = EInvoiceAPICommandList.Codes.CancelReceivablesInvoice;
			public const string GetInboxInvoiceList = EInvoiceAPICommandList.Codes.GetInboxInvoiceList;
			public const string SetInvoiceTaken = EInvoiceAPICommandList.Codes.SetInvoiceTaken;
			public const string SendApproveDocumentResponse = EInvoiceAPICommandList.Codes.SendApproveDocumentResponse;
			public const string SendRejectDocumentResponse = EInvoiceAPICommandList.Codes.SendRejectDocumentResponse;
		}

		public static class Descriptions
		{
			public static MultilingualString SendReceivablesInvoice { get { return ResString.GetMultilingualString("TurkeyEInvoiceCommandList|SendReceivablesInvoice", "Send Receivables Invoice"); } }
			public static MultilingualString StatusRequestForReceivablesInvoice { get { return ResString.GetMultilingualString("TurkeyEInvoiceCommandList|StatusRequestForReceivablesInvoice", "Status Request For Receivables Invoice"); } }
			public static MultilingualString RequestPDFofReceivablesInvoice { get { return ResString.GetMultilingualString("TurkeyEInvoiceCommandList|RequestPDFofReceivablesInvoice", "Request PDF of Receivables Invoice"); } }
			public static MultilingualString CancelReceivablesInvoice { get { return ResString.GetMultilingualString("TurkeyEInvoiceCommandList|CancelReceivablesInvoice", "Cancel Receivables Invoice"); } }
			public static MultilingualString GetInboxInvoiceList { get { return ResString.GetMultilingualString("TurkeyEInvoiceCommandList|GetInboxInvoiceList", "Get In-box Invoice List"); } }
			public static MultilingualString SetInvoiceTaken { get { return ResString.GetMultilingualString("TurkeyEInvoiceCommandList|SetInvoiceTaken", "Set AP Invoice as Taken"); } }
			public static MultilingualString SendApproveDocumentResponse { get { return ResString.GetMultilingualString("TurkeyEInvoiceCommandList|SendApproveDocumentResponse", "Send Approval Request for AP Invoice"); } }
			public static MultilingualString SendRejectDocumentResponse { get { return ResString.GetMultilingualString("TurkeyEInvoiceCommandList|SendRejectDocumentResponse", "Send Rejection Request for AP Invoice"); } }
		}

		public TurkeyEInvoiceAPICommandList()
		{
			AddPair(Codes.SendReceivablesInvoice, Descriptions.SendReceivablesInvoice);
			AddPair(Codes.StatusRequestForReceivablesInvoice, Descriptions.StatusRequestForReceivablesInvoice);
			AddPair(Codes.RequestPDFofReceivablesInvoice, Descriptions.RequestPDFofReceivablesInvoice);
			AddPair(Codes.CancelReceivablesInvoice, Descriptions.CancelReceivablesInvoice);
			AddPair(Codes.GetInboxInvoiceList, Descriptions.GetInboxInvoiceList);
			AddPair(Codes.SetInvoiceTaken, Descriptions.SetInvoiceTaken);
			AddPair(Codes.SendApproveDocumentResponse, Descriptions.SendApproveDocumentResponse);
			AddPair(Codes.SendRejectDocumentResponse, Descriptions.SendRejectDocumentResponse);
		}

		public static string GetMessageType(ZString actionType) => MessageTypeConfiguration.TryGetValue(actionType, out var result) ? result : string.Empty;

		static Dictionary<string, string> MessageTypeConfiguration => new Dictionary<string, string>()
		{
			{ Core.Constants.EInvoicingPivotActionType.Submit, Codes.SendReceivablesInvoice },
			{ Core.Constants.EInvoicingPivotActionType.StatusCheck, Codes.StatusRequestForReceivablesInvoice },
			{ Core.Constants.EInvoicingPivotActionType.DocumentAction, Codes.RequestPDFofReceivablesInvoice },
			{ Core.Constants.EInvoicingPivotActionType.Cancel, Codes.CancelReceivablesInvoice },
			{ Core.Constants.EInvoicingPivotActionType.ConfirmTransactionReceived, Codes.SetInvoiceTaken },
			{ Core.Constants.EInvoicingPivotActionType.Approve, Codes.SendApproveDocumentResponse },
			{ Core.Constants.EInvoicingPivotActionType.Reject, Codes.SendRejectDocumentResponse },
		};
	}
}
