using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ElectronicMessaging.Malaysia
{
	public class MalaysiaEInvoiceAPICommandList : CodeDescriptionPairList
	{
		[EInvoiceMessageSubType]
		public static class Codes
		{
			public const string SubmitTransaction = EInvoiceAPICommandList.Codes.SubmitTransaction;
			public const string GetSubmission = EInvoiceAPICommandList.Codes.GetSubmission;
			public const string GetDocument = EInvoiceAPICommandList.Codes.GetDocument;
			public const string GetDocumentDetail = EInvoiceAPICommandList.Codes.GetDocumentDetail;
		}

		public static string GetMessageType(ZString actionType)
		{
			switch (actionType)
			{
				case Core.Constants.EInvoicingPivotActionType.Submit:
					return Codes.SubmitTransaction;
				case Core.Constants.EInvoicingPivotActionType.StatusCheck:
					return Codes.GetSubmission;
				case Core.Constants.EInvoicingPivotActionType.DocumentAction:
					return Codes.GetDocument;
				case Core.Constants.EInvoicingPivotActionType.DocumentDetail:
					return Codes.GetDocumentDetail;
				default:
					return string.Empty;
			}
		}
	}
}
