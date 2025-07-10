using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IE.Business
{
	internal class SupportingDocumentWithImageProvider : ISupportingDocumentWithImage
	{
		public SupportingDocumentWithImageProvider(DocumentSendingObject sendingObject)
		{
			supportingDocument = Argument.NotNull(sendingObject, nameof(sendingObject));
			eDoc = Argument.NotNull(sendingObject.Document, nameof(sendingObject.Document));
		}

		readonly DocumentSendingObject supportingDocument;
		readonly IeDoc eDoc;

		public IDocumentImage DocumentImage => CachedValueHelper.GetValue(ref documentImageCache, () => new DocumentImageProvider(eDoc));
		CachedValue<IDocumentImage> documentImageCache;

		public string Description => supportingDocument.FileDescription;
	}
}
