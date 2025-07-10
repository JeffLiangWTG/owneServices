using System;
using System.Collections.Generic;
using System.Xml;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.MessagesWrappers.Common;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class UCC5InboundMessageCreator : InboundMessageCreator
	{
		protected override List<FREDIMessage> CreateMessagesFromInterchange(EDIInterchange interchange)
		{
			var messages = new List<FREDIMessage>();
			var msgType = GetMessageType(interchange);
			Func<FREDIMessage> getMessageFunc = (msgType.ToString() switch
			{
				MessageEnvelopeWrapper.deltaCSchemaResponseImport => () => interchange.Factory.New<DeltaCImportFREDIMessage>(),
				MessageEnvelopeWrapper.deltaCSchemaResponseExport => () => interchange.Factory.New<DeltaCExportFREDIMessage>(),
				MessageEnvelopeWrapper.deltaDSchemaResponseImport => () => interchange.Factory.New<DeltaDImportFREDIMessage>(),
				MessageEnvelopeWrapper.deltaDSchemaResponseExport => () => interchange.Factory.New<DeltaDExportFREDIMessage>(),
				MessageEnvelopeWrapper.deltaDcgSchemaResponse => () => interchange.Factory.New<DCGResponseFREDIMessage>(),
				MessageEnvelopeWrapper.cinSchema => () => interchange.Factory.New<CINImportResponseFREDIMessage>(),
				MessageEnvelopeWrapper.ecs507Schema or MessageEnvelopeWrapper.ecs618Schema or MessageEnvelopeWrapper.ecsEtatSchema => () =>
				{
					var msg = interchange.Factory.New<FREDIMessage>();
					msg.EM_MessageType = MessageTypeList.Codes.ECS;
					return msg;
				}
				,
				MessageSubTypeList.Codes.DT => () =>
				{
					var dtMessage = interchange.Factory.New<FREDIMessage>();
					dtMessage.EM_MessageType = Core.Constants.CountryCodes.France;
					dtMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.EuNcts;
					dtMessage.EM_ReceiveTransmit = Enterprise.Messaging.Integration.ReceiveTransmitList.Codes.Receive;
					return dtMessage;
				}
				,
				MessageSubTypeList.Codes.DOA => () => interchange.Factory.New<DOAResponseFREDIMessage>(),
				MessageSubTypeList.Codes.CAED => () => interchange.Factory.New<CAEDResponseFREDIMessage>(),
				_ => null
			});
			var message = getMessageFunc?.Invoke();
			if (message is not null)
			{
				message.EM_MessageText = interchange.EI_BodyText;
				messages.Add(message);
			}
			return messages;
		}

		ZString GetMessageType(EDIInterchange interchange)
		{
			ZString msgType;
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(interchange.EI_BodyText);
			var schemaId = xmlDoc.GetElementsByTagName("schemaID")?.Item(0)?.InnerText ?? ZString.Empty;

			if (schemaId == MessageEnvelopeWrapper.deltaCSchemaResponseImport
				|| schemaId == MessageEnvelopeWrapper.deltaCSchemaResponseExport
				|| schemaId == MessageEnvelopeWrapper.deltaDSchemaResponseImport
				|| schemaId == MessageEnvelopeWrapper.deltaDSchemaResponseExport
				|| schemaId == MessageEnvelopeWrapper.deltaDcgSchemaResponse
				|| schemaId == MessageEnvelopeWrapper.ecs507Schema
				|| schemaId == MessageEnvelopeWrapper.ecs618Schema
				|| schemaId == MessageEnvelopeWrapper.ecsEtatSchema)
			{
				msgType = schemaId;
			}
			else if (schemaId.StartsWith("DELTAT"))
			{
				msgType = MessageSubTypeList.Codes.DT;
			}
			else if (GetElementTextByTagNameWithDefaultValue(xmlDoc, "TYPE") == "DOA")
			{
				msgType = MessageSubTypeList.Codes.DOA;
			}
			else if (GetElementTextByTagNameWithDefaultValue(xmlDoc, "TYPE") == "CAED")
			{
				msgType = MessageSubTypeList.Codes.CAED;
			}
			else
			{
				msgType = xmlDoc.GetElementsByTagName(MessageEnvelopeWrapper.cinSchema)?.Count > 0 ? new ZString(MessageEnvelopeWrapper.cinSchema) : ZString.Empty;
			}

			return msgType;
		}

		string GetElementTextByTagNameWithDefaultValue(XmlDocument xmlDoc, string myTagName, string defaultValue = "")
		{
			var outputValue = defaultValue;
			var nodeList = xmlDoc.GetElementsByTagName(myTagName);

			if (nodeList != null && nodeList.Count > 0)
			{
				outputValue = nodeList[0].InnerText;
			}

			return outputValue;
		}
	}
}
