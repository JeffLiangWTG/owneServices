using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IE.ExitControl.Business.AES
{
	public class EX583SupportingDocumentProvider : ISupportingDocumentWithImage
	{
		public EX583SupportingDocumentProvider(DocumentSendingObject sendingObject)
		{
			supportingDocument = Argument.NotNull(sendingObject, nameof(sendingObject));
			eDoc = Argument.NotNull(sendingObject.Document, nameof(sendingObject.Document));
		}

		readonly DocumentSendingObject supportingDocument;
		readonly IeDoc eDoc;

		public IDocumentImage DocumentImage => CachedValueHelper.GetValue(ref documentImageCache, () => new IE.Business.DocumentImageProvider(eDoc));
		CachedValue<IDocumentImage> documentImageCache;

		public string Description => supportingDocument.Document.Description;
	}
}
