using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eHubMessaging.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.eHubMessaging.Business
{
	public class InterchangeAcknowledgement : IInterchangeAcknowledgement
	{
		const string AcknowledgementLogContextType = "AcknowledgementLog";

		public void Send(BusinessObjectFactory factoryParameter, INotifications notificationsParameter, IEDIInterchange interchange, string dataImportLog, IEnumerable<IValidationRule> validationRules, InterchangeAcknowledgementType acknowledgementType)
		{
			this.factory = Argument.NotNull(factoryParameter, "factoryParameter");
			this.notifications = Argument.NotNull(notificationsParameter, "notificationsParameter");
			this.interchange = interchange;

			Argument.NotNull(interchange, "interchange");
			Argument.NotNull(dataImportLog, "dataImportLog");

			if (!ParseAcknowledgementNodesAndAddNotesWhenFail(interchange, interchange as IStmNoteParent))
			{
				return;
			}

			if (!ShouldSendAcknowledgement(acknowledgementType))
			{
				return;
			}

			DeliverAcknowledgement(interchange.EI_Status, dataImportLog, validationRules, (BusinessObject)interchange, interchange.EI_ECC_CommunicationPartyConfig);
		}

		public void Send(BusinessObjectFactory factoryParameter, INotifications notificationsParameter, IEDIMessage message, string dataImportLog, IEnumerable<IValidationRule> validationRules, InterchangeAcknowledgementType acknowledgementType)
		{
			this.factory = Argument.NotNull(factoryParameter, "factoryParameter");
			this.notifications = Argument.NotNull(notificationsParameter, "notificationsParameter");
			Argument.NotNull(message, "message");
			Argument.NotNull(dataImportLog, "dataImportLog");

			if (message.Interchange == null)
			{
				return;
			}

			this.interchange = message.Interchange;

			if (!ParseAcknowledgementNodesAndAddNotesWhenFail(message.Interchange, message as IStmNoteParent))
			{
				return;
			}

			if (!ShouldSendAcknowledgement(acknowledgementType))
			{
				return;
			}

			DeliverAcknowledgement(message.EM_Status, dataImportLog, validationRules, (BusinessObject)message, message.EM_ECC_CommunicationPartyConfig, message.PK, message.EM_ExternalReferenceNumber);
		}

		#region Implementation

		#region Parse Acknowledgement Nodes

		bool ParseAcknowledgementNodesAndAddNotesWhenFail(IEDIInterchange interchange, IStmNoteParent noteParent)
		{
			if (!ParseAcknowledgementNodes(interchange))
			{
				if (noteParent != null && HasError())
				{
					noteParent.Notes.AddNew(true, NoteDescriptionFailureLog, errorLogBuilder.ToString());
				}
				return false;
			}

			return true;
		}

		string NoteDescriptionFailureLog
		{
			get { return Res.GetString("e1faf93f-b878-4873-822a-51f2bbf2eefb", "Send Acknowledge Failure Log"); }
		}

		bool ParseAcknowledgementNodes(IEDIInterchange interchange)
		{
			var acknowledgementXPath = AcknowledgementXPathProvider.GetInterchangeXPath(interchange.EI_InterchangeType);
			var headerText = interchange.EI_HeaderText;
			if (string.IsNullOrEmpty(acknowledgementXPath) || string.IsNullOrEmpty(headerText))
			{
				return false;
			}

			using (var textReader = new StringReader(headerText))
			{
				return ParseAcknowledgementNodes(acknowledgementXPath, textReader);
			}
		}

		bool ParseAcknowledgementNodes(string acknowledgementXPath, TextReader textReader)
		{
			using (var xPathReader = new XPathReader(textReader, acknowledgementXPath))
			{
				if (!FindAcknowledgeElement(xPathReader))
				{
					return false;
				}

				try
				{
					var namespaceURI = xPathReader.NamespaceURI;
					ParseRequired(xPathReader, namespaceURI);
					ParseChannel(xPathReader, namespaceURI);
					ParseRecipientID(xPathReader, namespaceURI);
					ParseContextCollection(xPathReader, namespaceURI);
					return errorLogBuilder.Length == 0;
				}
				catch (XmlException xmlEx)
				{
					errorLogBuilder.AppendLine(xmlEx.ToString());
					return false;
				}
			}
		}

		bool FindAcknowledgeElement(XPathReader xPathReader)
		{
			try
			{
				return xPathReader.ReadUntilMatch();
			}
			catch (XmlException)
			{
				return false;
			}
		}

		void ParseRequired(XPathReader xPathReader, string namespaceURI)
		{
			var requiredValue = xPathReader.GetElementAsString((NoResString)"Required", namespaceURI);
			if (!Enum.TryParse<InterchangeAcknowledgementRequiredList>(requiredValue, true, out required))
			{
				errorLogBuilder.AppendLine(Res.GetString("87ae4bc2-3529-4642-bdfe-168d6ee88f75", "Acknowledgement Required element has invalid value '{0}'. Valid values: '{1}', '{2}', '{3}'.", requiredValue, "OnAll", "OnError", "OnSuccess"));
			}
		}

		void ParseChannel(XPathReader xPathReader, string namespaceURI)
		{
			var channelValue = xPathReader.GetElementAsString((NoResString)"Channel", namespaceURI);
			if (!Enum.TryParse<InterchangeAcknowledgementChannelList>(channelValue, true, out channel))
			{
				errorLogBuilder.AppendLine(Res.GetString("28fba6e0-344a-44d4-a391-a4ea22bc058d", "Acknowledgement Channel element has invalid value '{0}'. Valid values: 'eHub', 'eAdaptor'.", channelValue));
			}
		}

		void ParseRecipientID(XPathReader xPathReader, string namespaceURI)
		{
			recipientID = xPathReader.GetElementAsString("RecipientID", namespaceURI);
			if (string.IsNullOrEmpty(recipientID))
			{
				errorLogBuilder.AppendLine(Res.GetString("253d4a49-d9e4-445b-9055-eba402d390ac", "Acknowledgement '{0}' element can not be empty.", "RecipientID"));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "This is a bloody XPath Element")]
		void ParseContextCollection(XPathReader xPathReader, string namespaceURI)
		{
			if (xPathReader.ReadUntilMatch("ContextCollection", namespaceURI))
			{
				contextCollection = new Dictionary<string, string>();
				while (ReadToNextElement(xPathReader))
				{
					if (xPathReader.LocalName == "Context")
					{
						ReadToNextElement(xPathReader);
						var key = (xPathReader.LocalName == "Type") ? xPathReader.ReadString() : string.Empty;
						ReadToNextElement(xPathReader);
						var value = (xPathReader.LocalName == "Value") ? xPathReader.ReadString() : string.Empty;
						contextCollection.Add(key, value);
					}
					else
					{
						break;
					}
				}
			}
		}

		bool ReadToNextElement(XmlReader reader)
		{
			while (reader.Read())
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					return true;
				}
			}

			return false;
		}

		bool HasError() { return errorLogBuilder.Length > 0; }

		#endregion

		bool ShouldSendAcknowledgement(InterchangeAcknowledgementType acknowledgementType)
		{
			Argument.NotNull(required, "required");

			return required == InterchangeAcknowledgementRequiredList.OnAll ||
				   acknowledgementType == InterchangeAcknowledgementType.Failed && required == InterchangeAcknowledgementRequiredList.OnError ||
				   acknowledgementType == InterchangeAcknowledgementType.Success && required == InterchangeAcknowledgementRequiredList.OnSuccess;
		}

		void DeliverAcknowledgement(string processingResultStatus, string dataImportLog, IEnumerable<IValidationRule> validationRules, BusinessObject parent, ZGuid configId, ZGuid requestMessagePK = default, ZString externalReferenceNumber = default)
		{
			Argument.NotNull(channel, "channel");
			Argument.NotNull(recipientID, "recipientID");

			EServicesDelivery delivery = null;
			var communicationMode = GetEDICommunicationsMode(recipientID, configId);

			if (channel == InterchangeAcknowledgementChannelList.eHub)
			{
				delivery = new EHubDelivery();
				communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			}
			else if (channel == InterchangeAcknowledgementChannelList.eAdapter || channel == InterchangeAcknowledgementChannelList.eAdaptor)
			{
				delivery = new EAdaptorDelivery()
				{
					RequestMessagePK = requestMessagePK,
					ExternalReferenceNumber = externalReferenceNumber
				};
				communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;

				if (communicationMode.EK_ECC_CommunicationPartyConfig.IsMissing)
				{
					(parent as IStmNoteParent)?.Notes?.AddNew(false, PredefinedNoteTypes.Instance.DataImportLogNote.Description, (NoResString)"Acknowledgement message requested, but acknowledgement can’t be generated as outbound configuration is missing.");
					return;
				}
			}

			var universalEventCreator = ObjectFactory.Get<IAcknowledgementUniversalEventCreator>();

			var message = universalEventCreator.CreateUniversalEvent(contextCollection, processingResultStatus, dataImportLog, validationRules);

			var deliveryContext = new DeliveryContext(factory);
			deliveryContext.ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			deliveryContext.MessageSubTypeCode = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			deliveryContext.MessageTypeCode = EDIMessageTypeList.Codes.XDC;
			deliveryContext.Notifications = notifications;
			deliveryContext.ParentInfo = EntityInfo.New(parent);

			delivery.Deliver(deliveryContext, communicationMode, new DeliveryStreamWrapperUXML(deliveryContext.ParentInfo, message, ObjectFactory.Get<IXmlWriter>(), null));
		}

		NonPersistentEDICommunicationMode GetEDICommunicationsMode(string recipientParameter, ZGuid configId)
		{
			var communicationsMode = new NonPersistentEDICommunicationMode { EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.UniversalDataBuss };
			communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
			communicationsMode.EK_Destination = recipientParameter;
			communicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationsMode.EK_ECC_CommunicationPartyConfig = GetEquivalentOutboundConfigFromInbound(configId);

			return communicationsMode;
		}

		ZGuid GetEquivalentOutboundConfigFromInbound(ZGuid configId)
		{
			if (configId.IsEmpty || !configId.IsValid)
			{
				return ZGuid.Empty;
			}

			var config = factory.Load<EDICommunicationPartyConfig>(configId);
			var outboundConfig = config.Party.Configs.Cast<EDICommunicationPartyConfig>().FirstOrDefault(c => c.ECC_Direction == EDICommunicationPartyConfigDirectionsList.Codes.Outbound && c.ECC_IsActive);

			if (!config.Party.ECP_IsActive || outboundConfig == null)
			{
				if (contextCollection == null)
				{
					contextCollection = new Dictionary<string, string>();
				}

				contextCollection.Add(AcknowledgementLogContextType, $"Cannot find the equivalent active outbound configuration for inbound configuration of EDI Client '{config.Party.Name}' for EDI Interchange '{interchange.EI_InterchangeNum}'");
				return ZGuid.Missing;
			}

			return outboundConfig.PK;
		}

		InterchangeAcknowledgementRequiredList required;
		InterchangeAcknowledgementChannelList channel;
		string recipientID;
		Dictionary<string, string> contextCollection;

		BusinessObjectFactory factory;
		INotifications notifications;
		IEDIInterchange interchange;
		readonly StringBuilder errorLogBuilder = new StringBuilder();

		#endregion

		public enum InterchangeAcknowledgementChannelList
		{
			eHub,
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1043:EAdaptorNamingRule")]
			eAdapter, // This should go away, but hangs around because LEGACY.
			eAdaptor,
		}

		public enum InterchangeAcknowledgementRequiredList
		{
			OnAll,
			OnError,
			OnSuccess
		}
	}
}
