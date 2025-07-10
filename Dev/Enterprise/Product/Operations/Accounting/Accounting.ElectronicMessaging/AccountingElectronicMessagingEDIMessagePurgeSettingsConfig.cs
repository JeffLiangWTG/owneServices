using System.Collections.Generic;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Accounting.ElectronicMessaging
{
	class AccountingElectronicMessagingEDIMessagePurgeSettingsConfig : PurgeSettingsConfig
	{
		public override IEnumerable<ApplicationCodeObj> GetPurgeSettings()
		{
			yield return AddApplicationCodeMessageSubTypePurgeType(ApplicationCodeList.Codes.GlobalElectronicInvoice, new InterchangeObjCollection() { NewInterchangeConfigObj(6, TimeUnit.Month) }, new MessageSubTypePurgeTypeObjCollection()
				.Add(EInvoiceAPICommandList.Codes.AdjustReceivablesInvoice, EInvoiceAPICommandList.Descriptions.AdjustReceivablesInvoice, 6, TimeUnit.Month)
				.Add(EInvoiceAPICommandList.Codes.ApproveReceivablesInvoice, EInvoiceAPICommandList.Descriptions.ApproveReceivablesInvoice, 6, TimeUnit.Month)
				.Add(EInvoiceAPICommandList.Codes.CallbackInvoceRequest, EInvoiceAPICommandList.Descriptions.CallbackInvoceRequest, 6, TimeUnit.Month)
				.Add(EInvoiceAPICommandList.Codes.CancelIRN, EInvoiceAPICommandList.Descriptions.CancelIRN, 6, TimeUnit.Month)
				.Add(EInvoiceAPICommandList.Codes.CancelReceivablesCircular78Invoice, EInvoiceAPICommandList.Descriptions.CancelReceivablesCircular78Invoice, 6, TimeUnit.Month)
				.Add(EInvoiceAPICommandList.Codes.CancelReceivablesInvoice, EInvoiceAPICommandList.Descriptions.CancelReceivablesInvoice, 6, TimeUnit.Month)
				.Add(EInvoiceAPICommandList.Codes.GenerateCancellationRequest, EInvoiceAPICommandList.Descriptions.GenerateCancellationRequest, 6, TimeUnit.Month)
				.Add(EInvoiceAPICommandList.Codes.GenerateDetailItemsInvoiceRequest, EInvoiceAPICommandList.Descriptions.GenerateDetailItemsInvoiceRequest, 6, TimeUnit.Month)
				.Add(EInvoiceAPICommandList.Codes.GenerateExportInvoiceRequest, EInvoiceAPICommandList.Descriptions.GenerateExportInvoiceRequest, 6, TimeUnit.Month)
				.Add(EInvoiceAPICommandList.Codes.GenerateInvoiceRequest, EInvoiceAPICommandList.Descriptions.GenerateInvoiceRequest, 6, TimeUnit.Month)
				.Add(EInvoiceAPICommandList.Codes.GenerateIRN, EInvoiceAPICommandList.Descriptions.GenerateIRN, 6, TimeUnit.Month)
				.Add(EInvoiceAPICommandList.Codes.GenerateLocalInvoiceRequest, EInvoiceAPICommandList.Descriptions.GenerateLocalInvoiceRequest, 6, TimeUnit.Month)
				.Add(EInvoiceAPICommandList.Codes.GetDocument, EInvoiceAPICommandList.Descriptions.GetDocument, 6, TimeUnit.Month)
				.Add(EInvoiceAPICommandList.Codes.GetDocumentDetail, EInvoiceAPICommandList.Descriptions.GetDocumentDetail, 6, TimeUnit.Month)
				.Add(EInvoiceAPICommandList.Codes.GetInboxInvoiceList, EInvoiceAPICommandList.Descriptions.GetInboxInvoiceList, 6, TimeUnit.Month)
				.Add(EInvoiceAPICommandList.Codes.GetSubmission, EInvoiceAPICommandList.Descriptions.GetSubmission, 6, TimeUnit.Month)
				.Add(EInvoiceAPICommandList.Codes.QueryInvoiceRequest, EInvoiceAPICommandList.Descriptions.QueryInvoiceRequest, 6, TimeUnit.Month)
				.Add(EInvoiceAPICommandList.Codes.ReceivablesInvoice, EInvoiceAPICommandList.Descriptions.ReceivablesInvoice, 6, TimeUnit.Month)
				.Add(EInvoiceAPICommandList.Codes.Request, EInvoiceAPICommandList.Descriptions.Request, 6, TimeUnit.Month)
				.Add(EInvoiceAPICommandList.Codes.RequestDocumentForInvoice, EInvoiceAPICommandList.Descriptions.RequestDocumentForInvoice, 6, TimeUnit.Month)
				.Add(EInvoiceAPICommandList.Codes.RequestPDFDocumentForInvoice, EInvoiceAPICommandList.Descriptions.RequestPDFDocumentForInvoice, 6, TimeUnit.Month)
				.Add(EInvoiceAPICommandList.Codes.RequestPDFofReceivablesInvoice, EInvoiceAPICommandList.Descriptions.RequestPDFofReceivablesInvoice, 6, TimeUnit.Month)
				.Add(EInvoiceAPICommandList.Codes.RomaniaInvoiceRefreshAccessToken, EInvoiceAPICommandList.Descriptions.RomaniaInvoiceRefreshAccessToken, 6, TimeUnit.Month)
				.Add(EInvoiceAPICommandList.Codes.SendApproveDocumentResponse, EInvoiceAPICommandList.Descriptions.SendApproveDocumentResponse, 6, TimeUnit.Month)
				.Add(EInvoiceAPICommandList.Codes.SendInvoiceBatch, EInvoiceAPICommandList.Descriptions.SendInvoiceBatch, 6, TimeUnit.Month)
				.Add(EInvoiceAPICommandList.Codes.SendReceivablesInvoice, EInvoiceAPICommandList.Descriptions.SendReceivablesInvoice, 6, TimeUnit.Month)
				.Add(EInvoiceAPICommandList.Codes.SendRejectDocumentResponse, EInvoiceAPICommandList.Descriptions.SendRejectDocumentResponse, 6, TimeUnit.Month)
				.Add(EInvoiceAPICommandList.Codes.SetInvoiceTaken, EInvoiceAPICommandList.Descriptions.SetInvoiceTaken, 6, TimeUnit.Month)
				.Add(EInvoiceAPICommandList.Codes.StatusRequestForReceivablesInvoice, EInvoiceAPICommandList.Descriptions.StatusRequestForReceivablesInvoice, 6, TimeUnit.Month)
				.Add(EInvoiceAPICommandList.Codes.SubmitTransaction, EInvoiceAPICommandList.Descriptions.SubmitTransaction, 6, TimeUnit.Month)
			);
		}
	}
}
