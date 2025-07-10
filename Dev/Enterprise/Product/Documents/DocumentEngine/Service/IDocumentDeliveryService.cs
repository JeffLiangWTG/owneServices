using System;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineIntegration;

namespace Enterprise.DocumentEngine.Service
{
	public interface IDocumentDeliveryService
	{
		ActionResult DeliverDocument(Guid documentCommandPk, string tablePrefix, Guid businessObjectPk, DeliveryInstructionsBase deliveryInstructions, DocumentDetail[] documents);
		RecipientDetail[] GetDeliveryRecipients(Guid documentCommandPk, string tablePrefix, Guid businessObjectPk);
		Guid? GetDefaultPrinterKey(Guid documentCommandPK);
		PrinterDetail[] GetPrinters();
		DocumentDetail[] GetDocuments(Guid documentCommandPk);
		DocumentSupporterDataState CheckDataState(Guid documentCommandPk, string tablePrefix, Guid businessObjectPk);
		bool CanPreview(Guid documentCommandPk);
		bool ShowOnlyPrintersUserCanPrintTo { get; set; }
	}
}
