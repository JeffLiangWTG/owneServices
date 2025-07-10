using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IE.H7.Business
{
	public class SupportingDocumentWithImageProvider : ISupportingDocumentWithImage
	{
		public SupportingDocumentWithImageProvider(EU.H7.Business.DocumentSendingObject sendingObject)
		{
			supportingDocument = Argument.NotNull(sendingObject, nameof(sendingObject));
			eDoc = Argument.NotNull(sendingObject.Document, nameof(sendingObject.Document));
		}

		readonly EU.H7.Business.DocumentSendingObject supportingDocument;
		readonly IeDoc eDoc;

		public IDocumentImage DocumentImage => CachedValueHelper.GetValue(ref documentImageCache, () => new DocumentImageProvider(eDoc));
		CachedValue<IDocumentImage> documentImageCache;

		public string Description => supportingDocument.FileDescription;
	}
}
