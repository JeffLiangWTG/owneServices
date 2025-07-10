using CargoWise.ComponentModel;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.Business
{
	public static class NoUIMessageSender
	{
		public static bool SendUXml(IMessageInstructions messageInstructions, IVisualizerDocumentData documentDataStorage, DocDataObject docDataObject, INotifications notifications)
		{
			if (messageInstructions == null
				|| documentDataStorage == null
				|| docDataObject == null
				|| notifications == null)
			{
				return false;
			}

			var data = docDataObject.MakeDynamic();
			var document = new EmptyDocument(messageInstructions.DocumentName, messageInstructions.DataContext, data);

			var dataVersion = documentDataStorage.CalculateDataVersion(messageInstructions.DocumentName, messageInstructions.OrderLogsByLocalTime);
			var messageType = dataVersion > 1
				? MessageType.Amendment
				: MessageType.Original;

			var parameters = new UXmlSender.Parameters
			{
				Instructions = messageInstructions,
				Document = document,
				DeliveryService = new EmptyDocumentDeliveryService(),
				DocumentData = documentDataStorage,
				MessageType = messageType,
				ReasonForSending = string.Empty
			};

			var uxmlSender = new UXmlSender(parameters);
			return uxmlSender.Send(notifications);
		}

		public static bool SendUXml(IMessageInstructions messageInstructions, IDocumentInfo documentInfo, INotifications notifications)
		{
			var documentDataStorage = documentInfo?.DocumentData;
			if (messageInstructions == null
				|| documentInfo == null
				|| documentDataStorage == null
				|| notifications == null)
			{
				return false;
			}

			var dataVersion = documentDataStorage.CalculateDataVersion(messageInstructions.DocumentName, messageInstructions.OrderLogsByLocalTime);
			var messageType = dataVersion > 1
				? MessageType.Amendment
				: MessageType.Original;

			var parameters = new UXmlSender.Parameters
			{
				Instructions = messageInstructions,
				Document = documentInfo.Document,
				DeliveryService = new EmptyDocumentDeliveryService(),
				DocumentData = documentDataStorage,
				MessageType = messageType,
				ReasonForSending = string.Empty
			};

			var uxmlSender = new UXmlSender(parameters);
			return uxmlSender.Send(notifications);
		}
	}
}
