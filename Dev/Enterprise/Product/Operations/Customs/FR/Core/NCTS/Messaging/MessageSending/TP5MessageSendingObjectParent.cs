using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging
{
	public sealed class TP5MessageSendingObjectParent : NctsHeaderMessageSendingObjectParent
	{
		public TP5MessageSendingObjectParent(Business.NCTS.NctsHeader nctsHeader) : base(nctsHeader)
		{
			this.nctsHeader = nctsHeader;
		}

		protected override NonPersistentBusinessObjectCollection<NctsHeaderMessageSendingObject> GetSendingObjectsCollectionCore() => new TP5MessageSendingObjectCollection(nctsHeader);

		public new TP5MessageSendingObjectCollection SendingObjectsCollection => (TP5MessageSendingObjectCollection)base.SendingObjectsCollection;

		public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties
		{
			get
			{
				foreach (var property in base.MessageSendingObjectProperties)
				{
					yield return property;
				}
				yield return new MessageSendingObjectProperty(TP5MessageSendingObject.Schema.JustificationCode, false, 150);
			}
		}

		protected override bool SendAndSaveMessagesCore()
		{
			var result = false;
			var errorCollector = new EU.Business.ErrorCollector();
			foreach (var objectToSend in SelectedSendingObjects.Cast<TP5MessageSendingObject>())
			{
				var messageSender = new TP5MessageSender(objectToSend, errorCollector);
				var sentMessage = messageSender.Send();
				result = TP5MessageSender.MessageSendSuccessful == sentMessage;
			}
			return result;
		}

		readonly Business.NCTS.NctsHeader nctsHeader;
	}
}
