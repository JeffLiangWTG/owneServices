using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class ExitControlMessageSendingObjectParent : EU.ExitControl.Business.ExitControlMessageSendingObjectParent
	{
		public ExitControlMessageSendingObjectParent(CusExitHeader cusExitHeader)
			: base(cusExitHeader)
		{
		}

		protected new CusExitHeader exitHeader => (CusExitHeader)base.exitHeader;

		public new ExitControlMessageSendingObjectCollection SendingObjectsCollection => (ExitControlMessageSendingObjectCollection)base.SendingObjectsCollection;

		protected override NonPersistentBusinessObjectCollection<EU.ExitControl.Business.ExitControlMessageSendingObject> GetSendingObjectsCollectionCore() => new ExitControlMessageSendingObjectCollection(reports, Factory);

		public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties => columnDefinitions;

		readonly IEnumerable<MessageSendingObjectProperty> columnDefinitions = new MessageSendingObjectProperty[]
		{
			new MessageSendingObjectProperty(nameof(ExitControlMessageSendingObject.Type), true, 80),
			new MessageSendingObjectProperty(nameof(ExitControlMessageSendingObject.DateTime), true, 200),
			new MessageSendingObjectProperty(nameof(ExitControlMessageSendingObject.ExitOffice), true, 200),
			new MessageSendingObjectProperty(nameof(ExitControlMessageSendingObject.MRN), true, 200),
			new MessageSendingObjectProperty(nameof(ExitControlMessageSendingObject.CustomsStatus), true, 80),
			new MessageSendingObjectProperty(nameof(ExitControlMessageSendingObject.MessageStatus), true, 80)
		};
	}
}
