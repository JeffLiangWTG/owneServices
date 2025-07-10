using CargoWise.EntityFramework;
using Enterprise.Integration.DocumentVisualizer;

namespace Enterprise.DocumentVisualizer.Delivery
{
	public sealed class DocumentEDocsDelivery : IDocumentEDocsDelivery
	{
		public void SaveCopyToEDocs(Enterprise.Integration.DocumentEngine.IDocument document, string name, string title)
		{
			if (document is DocumentDeliverable documentDeliverable)
			{
				var eDocDeliveryParameters = new EDocsDeliveryParameters
				{
					DocumentName = string.IsNullOrEmpty(name)
						? documentDeliverable.DocumentName
						: name,
					DocumentTitle = string.IsNullOrEmpty(title)
						? documentDeliverable.DocumentTitle
						: title,
					DocumentType = documentDeliverable.DocumentType,
					AttachedFileName = string.IsNullOrEmpty(name)
						? documentDeliverable.DocumentName
						: name,
					BusinessObject = documentDeliverable.EDocsParent as IBusiness
				};

				documentDeliverable.Document.AddCopyToEDocs(eDocDeliveryParameters);
			}
		}
	}
}
