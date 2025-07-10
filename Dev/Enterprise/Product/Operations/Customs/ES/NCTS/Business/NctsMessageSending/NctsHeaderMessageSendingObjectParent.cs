using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public sealed class NctsHeaderMessageSendingObjectParent : EU.NCTS.Business.NctsHeaderMessageSendingObjectParent
	{
		public NctsHeaderMessageSendingObjectParent(NctsMessageSendingObject sendingObject, SendingType sendingType = SendingType.None, NctsMessageFunctionSet messageFunction = null) : base(sendingObject.Header)
		{
			this.sendingObject = sendingObject;
			this.sendingType = sendingType;
			this.messageFunction = messageFunction;
			topBusinessObject = sendingObject.Header;
		}

		readonly NctsMessageSendingObject sendingObject;
		readonly SendingType sendingType;
		readonly NctsMessageFunctionSet messageFunction;
		readonly NctsHeader topBusinessObject;

		public ICertificateProvider CertificateData => sendingObject;

		public ZBool ShouldEditMessage => sendingObject.ShouldEditMessage;

		public new NctsHeaderMessageSendingObjectCollection SendingObjectsCollection => (NctsHeaderMessageSendingObjectCollection)base.SendingObjectsCollection;

		protected override NonPersistentBusinessObjectCollection<EU.NCTS.Business.NctsHeaderMessageSendingObject> GetSendingObjectsCollectionCore()
		{
			var sendingObjectCollection = new NctsHeaderMessageSendingObjectCollection(Factory)
			{
				new NctsHeaderMessageSendingObject((NctsHeader)NctsHeader, sendingType, messageFunction)
			};
			return sendingObjectCollection;
		}

		public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties
		{
			get
			{
				foreach (var baseProperty in base.MessageSendingObjectProperties)
				{
					yield return baseProperty;
				}
				yield return new MessageSendingObjectProperty(NctsHeaderMessageSendingObject.Schema.CustomsStatus, false, 100);
				yield return new MessageSendingObjectProperty(NctsHeaderMessageSendingObject.Schema.MessageStatus, false, 100);
				yield return new MessageSendingObjectProperty(NctsHeaderMessageSendingObject.Schema.MessageSubType, false, 100);
			}
		}

		protected override IEnumerable<INotification> GetNewMessageErrorCollector()
			=> new SelectiveMessageErrorCollector(GetHeaderForValidations(topBusinessObject), SelectedSendingObjects.Cast<NctsHeaderMessageSendingObject>().Select(x => GetHeaderForValidations(x.NctsHeader))).GetMessageErrors();

		NctsHeader GetHeaderForValidations(NctsHeader header) =>
			(header.IsArrivalMovement && header.ESNctsHeader.CEN_TNNArrival && header.ArrivalMovementHeader.HeaderTNN is not null && RelevantTNNDepartureMRNIsNotAllocated(header))
			? header.ArrivalMovementHeader.HeaderTNN
			: header;

		bool RelevantTNNDepartureMRNIsNotAllocated(NctsHeader header) => (header.ArrivalMovementHeader.HeaderTNN?.MovementHeader?.BM_CustomsStatus ?? ZString.Empty) != ESNCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
	}
}
