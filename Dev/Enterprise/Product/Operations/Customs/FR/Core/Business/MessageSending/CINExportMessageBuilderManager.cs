using System;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MessagesWrappers.CIN;
using Enterprise.Customs.FR.Messaging.MessageBuilders.CIN;

namespace Enterprise.Customs.FR.Business.MessageSending
{
	public class CINExportMessageBuilderManager : MessageBuilderManager<DeltaGJobDeclarationMessageSendingObject>
	{
		public CINExportMessageBuilderManager(EU.Business.ErrorCollector errorCollector)
		{
			this.errorCollectorItem = Argument.NotNull(errorCollector, "errorCollector cannot be null");
		}
		public override IMessageBuilderBase NewMessageBuilder(DeltaGJobDeclarationMessageSendingObject objectToSend)
		{
			var messageType = objectToSend.MessageType;
			if (messageType.EqualsIgnoringCase(EntryActionCodeList.Codes.CIN745))
			{
				return new CIN745ExportMessageBuilder(GetCIN745ExportWrapper(objectToSend), errorCollectorItem, Messaging.MessageBuilders.TransactionTypes.Original);
			}

			if (messageType.EqualsIgnoringCase(EntryActionCodeList.Codes.CIN755))
			{
				return new CIN755ExportMessageBuilder(GetCIN755ExportWrapper(objectToSend), errorCollectorItem, Messaging.MessageBuilders.TransactionTypes.Original);
			}

			throw new NotImplementedException("CW1 doesn't yet support building messages of type " + messageType);
		}

		CIN755SendExpMessageWrapper GetCIN755ExportWrapper(DeltaGJobDeclarationMessageSendingObject objectToSend)
		{
			return new CIN755SendExpMessageWrapper(objectToSend);
		}

		CIN745SendExpMessageWrapper GetCIN745ExportWrapper(DeltaGJobDeclarationMessageSendingObject objectToSend)
		{
			return new CIN745SendExpMessageWrapper(objectToSend);
		}

		public override ZString BuilderType => MessageTypeList.Codes.CIN;

		public (SendMessageResult action, ZString result) SendMessage(JobDeclaration declaration, ZString messageType, bool shouldDelaySave = false)
		{
			var action = SendMessageResult.NoAction;
			var result = ZString.Empty;
			var decWrapper = new DeltaGJobDeclarationMessageSendingObjectParent(declaration);

			foreach (DeltaGJobDeclarationMessageSendingObject objectToSend in decWrapper.SendingObjectsCollection)
			{
				if (objectToSend.ShouldSend)
				{
					objectToSend.MessageType = messageType;
					var sender = new CINExportMessageSender(objectToSend, errorCollectorItem);
					result = sender.Send(shouldDelaySave);

					if (errorCollectorItem.ErrorCount == 0)
					{
						if (action == SendMessageResult.NoAction)
						{
							action = SendMessageResult.Successful;
						}
					}
					else
					{
						action = SendMessageResult.Error;
					}
				}
			}
			if (action == SendMessageResult.Error)
			{
				result = errorCollectorItem.GetErrorsAsString();
			}

			return (action, result);
		}

		public enum SendMessageResult
		{
			NoAction = 0,
			Successful = 1,
			Error = 2
		}

		readonly EU.Business.ErrorCollector errorCollectorItem;
	}
}
