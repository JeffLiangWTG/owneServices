using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class DocumentSendingActionParent : BaseMessageSendingObjectParent<DocumentSendingAction>
	{
		public DocumentSendingActionParent(NctsHeader header) : base(header.Factory)
		{
			Header = Argument.NotNull(header, nameof(header));
			MessageType = NCTSOutgoingMessageTypeList.Codes.UploadSupportingDocuments;
		}
		public NctsHeader Header { get; }

		public string MessageType { get; }

		public override BusinessObject TopLevelBusinessObject => Header;

		public override SecurityCheckpoint SecurityCheckpointToSendWithMessageError => Env.Security.CustomsDeclarationSendWithMessageErrors;

		protected override NonPersistentBusinessObjectCollection<DocumentSendingAction> GetSendingObjectsCollectionCore()
		{
			var result = (DocumentSendingActionCollection)Activator.CreateInstance(typeof(DocumentSendingActionCollection), this);
			result.AddSendingAction();
			return result;
		}
	}
}
