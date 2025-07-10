using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.IE.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public sealed class NctsHeaderMessageSendingObjectParent : EU.NCTS.Business.NctsHeaderMessageSendingObjectParent
	{
		public NctsHeaderMessageSendingObjectParent(NctsHeader nctsHeader) : base(nctsHeader)
		{
			this.nctsHeader = nctsHeader;
		}
		readonly NctsHeader nctsHeader;

		protected override NonPersistentBusinessObjectCollection<EU.NCTS.Business.NctsHeaderMessageSendingObject> GetSendingObjectsCollectionCore()
		{
			var sendingObjectCollection = new EU.NCTS.Business.NctsHeaderMessageSendingObjectCollection(Factory);
			sendingObjectCollection.Add(new NctsHeaderMessageSendingObject(nctsHeader));
			return sendingObjectCollection;
		}

		protected override bool SendAndSaveMessagesCore()
		{
			var result = false;
			var sentCount = 0;

			var list = new List<NctsHeaderMessageSendingObject>();
			try
			{
				foreach (var action in SelectedSendingObjects.Cast<NctsHeaderMessageSendingObject>())
				{
					list.Add(action);
					var sender = new NctsMessageSender(new NctsMessageSendingAction(action));
					if (sender.Send() is OutboundEDIMessage message)
					{
						action.GuaranteeProcessor?.AddPermitTransactions(message, (msg) => EU.NCTS.Business.NctsPermitHelper.GetPermitAppIdForMessage(msg));
						sentCount++;
					}
				}

				if (sentCount > 0)
				{
					try
					{
						Factory.Save();
						result = true;
					}
					catch (ZSaveException ex)
					{
						ZExceptionReporting.HandleSaveException(ex);
					}
				}
			}
			finally
			{
				list.ForEach(x => x.UnlockMutexGuaranteeProcessorIfNeeded());
			}
			return result;
		}

		public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties
		{
			get
			{
				foreach (var baseProperty in base.MessageSendingObjectProperties)
				{
					yield return baseProperty;
				}
				yield return new MessageSendingObjectProperty(NctsHeaderMessageSendingObject.Schema.MessageStatus, false, 60);
				yield return new MessageSendingObjectProperty(NctsHeaderMessageSendingObject.Schema.DestinationCustomsOfficeCode, false, 100);
				yield return new MessageSendingObjectProperty(NctsHeaderMessageSendingObject.Schema.Consignee, false, 100);
				yield return new MessageSendingObjectProperty(NctsHeaderMessageSendingObject.Schema.TC11DeliveryDate, false, 60);
				yield return new MessageSendingObjectProperty(NctsHeaderMessageSendingObject.Schema.EnquiryText, false, 160);
			}
		}
	}
}
