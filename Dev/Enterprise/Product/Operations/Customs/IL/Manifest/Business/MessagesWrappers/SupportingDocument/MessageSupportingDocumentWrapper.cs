using CargoWise.Customs.IL.MessageDefinitions.DOC.REQ_271;
using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class MessageSupportingDocumentWrapper : MessageSupportingDocumentWrapperBase
	{
		MessageSupportingDocumentWrapper(SupportingDocument supportingDocument) : base(supportingDocument)
		{
		}

		internal static MessageSupportingDocumentWrapper NewOrNull(SupportingDocument supportingDocument)
			=> supportingDocument != null
			? new MessageSupportingDocumentWrapper(supportingDocument) : null;

		protected override IConnectedEntity RelatedEntityCore()
			=> ConnectedEntityWrapper.NewOrNull((SupportingDocument)supportingDocument);
	}
}
