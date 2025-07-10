using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.ExitControl.Business
{
	public class ExitControlMessageSendingObjectParent : EU.ExitControl.Business.ExitControlMessageSendingObjectParent
	{
		public ExitControlMessageSendingObjectParent(MessageSendingObject sendingObject) : base(sendingObject.ExitHeader)
		{
			this.sendingObject = sendingObject;
			ParentExitHeader = sendingObject.ExitHeader;
		}
		readonly MessageSendingObject sendingObject;

		public CusExitHeader ParentExitHeader { get; }

		public ICertificateProvider CertificateData => sendingObject;

		public ZBool ShouldEditMessage => sendingObject.ShouldEditMessage;

		protected new CusExitHeader exitHeader => (CusExitHeader)base.exitHeader;

		public new ExitControlMessageSendingObjectCollection SendingObjectsCollection => (ExitControlMessageSendingObjectCollection)base.SendingObjectsCollection;

		protected override NonPersistentBusinessObjectCollection<EU.ExitControl.Business.ExitControlMessageSendingObject> GetSendingObjectsCollectionCore() => new ExitControlMessageSendingObjectCollection(reports, Factory);
	}
}
