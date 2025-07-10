using System;
using System.Xml;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.EMCS.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.EMCS
{
	public class EMCSInboundInterchangeProcessor : InboundInterchangeProcessor
	{
		public EMCSInboundInterchangeProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string[] ApplicationCodes => new[] { EDIMessage.ApplicationCodes.GbCustomsEMCS };

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange) => new EMCSInboundMessageCreator();

		protected override Type TypeOfInterchangeToCreate() => typeof(EMCSInterchange);

		class EMCSInboundMessageCreator : IInboundMessageCreator
		{
			void IInboundMessageCreator.CreateMessagesForInterchange(EDIInterchange interchange)
			{
				if (interchange is EMCSInterchange emcsInterchange)
				{
					var customsBusinessResponse = emcsInterchange.EMCSCustomsBusinessResponse;
					var bodyXml = customsBusinessResponse.BodyXml;
					if (!bodyXml.IsEmpty)
					{
						ZString messageType = ZString.Empty;
						try
						{
							var bodyXmlDoc = new XmlDocument();
							bodyXmlDoc.LoadXml(bodyXml);

							messageType = new ZString(bodyXmlDoc?.FirstChild?.LocalName ?? ZString.Empty).SubstringSafe(2, 3);
						}
						catch (XmlException) { }

						var message = Spawn(interchange, bodyXml, customsBusinessResponse.ServiceReference);
						message.EM_MessageNum = interchange.EI_InterchangeNum.SubstringSafe(0, EDIMessage.Schema.EM_MessageNumMaxLength);
						var declaration = EMCSHelper.GetDeclarationFromCustomsBusinessResponse(interchange.Factory, customsBusinessResponse);
						message.EM_LinkedObject = declaration;

						if (EMCSResponseMessageDetails.Instance.ResponseMessages.ContainsKey(messageType))
						{
							message.EM_MessageType = messageType;
							message.EM_Status = EDIMessage.Status.Queued;
						}
						else
						{
							message.EM_MessageType = ZString.Empty;
							message.EM_Status = EDIMessage.Status.Failed;
							if (declaration != null)
							{
								declaration.JE_MessageStatus = EDIMessageStatusList.Codes.Error;
							}
						}
					}
				}
			}

			static EMCSInboundEDIMessage Spawn(EDIInterchange interchange, ZString messageText, ZString conversationId)
			{
				var message = interchange.Factory.New<EMCSInboundEDIMessage>();
				message.EM_ApplicationCode = interchange.EI_ApplicationCode;
				message.EM_MessageText = messageText;
				message.EM_ApplicationReference = conversationId;

				interchange.ContainedMessages.Add(message);
				return message;
			}
		}
	}
}
