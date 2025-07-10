using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.DOC.REQ_271;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business
{
	public abstract class MessageSupportingDocumentWrapperBase : IMessageSupportingDocument
	{
		protected MessageSupportingDocumentWrapperBase(SupportingDocument supportingDocument)
		{
			this.supportingDocument = supportingDocument;
		}

		public int? DocumentId => ZInt.TryParse(supportingDocument.CSI_ReferenceNumber2, out var res)
			? res
			: null;

		public IAttachment Attachment => AttachmentWrapper.NewOrNull(supportingDocument);

		public IConnectedEntity RelatedEntity => RelatedEntityCore();

		public IRequestContentHeader RequestContentHeader => RequestContentHeaderWrapper.New();

		protected abstract IConnectedEntity RelatedEntityCore();

		protected readonly SupportingDocument supportingDocument;
	}
}
