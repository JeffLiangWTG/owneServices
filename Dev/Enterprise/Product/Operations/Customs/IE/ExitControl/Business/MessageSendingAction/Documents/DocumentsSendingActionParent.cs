using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class DocumentsSendingActionParent : BaseMessageSendingObjectParent<DocumentsSendingAction>
	{
		public DocumentsSendingActionParent(CusExitReport exitReport, string messageType) : base(exitReport.Factory)
		{
			CusExitReport = Argument.NotNull(exitReport, nameof(exitReport));
			MessageType = Argument.NotNullOrEmpty(messageType, nameof(messageType));
		}

		public string MessageType { get; }

		public CusExitReport CusExitReport { get; }

		public override BusinessObject TopLevelBusinessObject => CusExitReport.Header;

		public override Security.SecurityCheckpoint SecurityCheckpointToSendWithMessageError => Env.Security.CustomsDeclarationSendWithMessageErrors;

		protected Type SendingObjectCollectionType => typeof(DocumentsSendingActionCollection);

		protected override NonPersistentBusinessObjectCollection<DocumentsSendingAction> GetSendingObjectsCollectionCore()
		{
			var result = (DocumentsSendingActionCollection)Activator.CreateInstance(SendingObjectCollectionType, this);
			result.PopulateElements();
			return result;
		}
	}
}
