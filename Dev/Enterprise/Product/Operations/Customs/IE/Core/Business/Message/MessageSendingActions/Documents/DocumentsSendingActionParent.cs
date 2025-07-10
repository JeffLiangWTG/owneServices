using System;
using CargoWise.Common;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business
{
	public class DocumentsSendingActionParent : CusEntryHeaderMessageSendingActionParent<DocumentsSendingAction>
	{
		public DocumentsSendingActionParent(JobDeclaration declaration, string messageType) : base(declaration)
		{
			MessageType = Argument.NotNullOrEmpty(messageType, nameof(messageType));
		}

		public string MessageType { get; }

		protected override Type SendingObjectCollectionType => typeof(DocumentsSendingActionCollection);
	}
}
