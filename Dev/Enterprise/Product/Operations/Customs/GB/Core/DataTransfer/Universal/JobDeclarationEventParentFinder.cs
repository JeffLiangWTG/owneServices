using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Interfaces;
using Enterprise.Customs.GB.CDS;
using Enterprise.Customs.GB.CDS.Messaging;
using Enterprise.Customs.GB.Registry;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using WTG.StaticAnalysis.Annotation;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

[assembly: UsesConstants(typeof(CDSEDIMessageTypeList))]
namespace Enterprise.Customs.GB.DataTransfer.Universal
{
	public class JobDeclarationEventParentFinder : Customs.DataTransfer.Universal.JobDeclarationEventParentFinder
	{
		public JobDeclarationEventParentFinder(BusinessObjectFactory factory, JobDeclarationDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContextCore(UniversalEvent eventDataObject)
		{
			BusinessObject[] result = null;
			if (IsExpectedDataProvider(eventDataObject))
			{
				result = base.GetLogParentsForEventUsingContextCore(eventDataObject) ?? GetEventContextByHubTrackingIdAndLog(eventDataObject);
			}
			return result;
		}

		BusinessObject[] GetEventContextByHubTrackingIdAndLog(UniversalEvent eventDataObject)
		{
			UpdateEntry(eventDataObject);

			if (ZGuid.TryParse(((IXmlEventValueObject)eventDataObject).Context.EHubTrackingID, out var eHubTrackingID))
			{
				var originalInterchange = CDS.Helpers.EventParentFinderHelper.GetOutboundInterchange(eHubTrackingID, factory);
				if (originalInterchange != null)
				{
					var originalMessage = CDS.Helpers.EventParentFinderHelper.GetOriginalMessage(originalInterchange.PK, factory);

					if (originalMessage != null)
					{
						var entry = originalMessage.EM_LinkedObject as CusEntryHeader;

						if (entry != null)
						{
							return new BusinessObject[] { entry.Declaration };
						}
						else if (originalMessage is CDSDISQueryMessage)
						{
							return new BusinessObject[] { originalMessage };
						}
						else if (originalMessage.EM_LinkedObject is IMessageAttachee)
						{
							return new BusinessObject[] { originalMessage.EM_LinkedObject };
						}
					}
				}
			}

			return null;
		}

		protected override BusinessObject[] GetChildrenIfSpecifiedInContext(BusinessObject[] logParents, UniversalEvent eventData)
		{
			BusinessObject[] result = null;
			if (IsExpectedDataProvider(eventData))
			{
				UpdateEntry(eventData, logParents?.FirstOrDefault() as IMessageAttachee);
				result = base.GetChildrenIfSpecifiedInContext(logParents, eventData);
			}
			return result;
		}

		bool IsExpectedDataProvider(UniversalEvent eventDataObject)
		{
			var dataProvider = eventDataObject.DataContext?.DataProviderForCodeMapping ?? ZString.Empty;
			return !dataProvider.EqualsAny(Constants.GVMS, Constants.EMCS, Constants.ICSNI, Constants.ICSGB);
		}

		void UpdateEntry(UniversalEvent eventDataObject, IMessageAttachee messageAttachee = null)
		{
			if (ZGuid.TryParse(((IXmlEventValueObject)eventDataObject).Context.EHubTrackingID, out var eHubTrackingID))
			{
				var originalInterchange = CDS.Helpers.EventParentFinderHelper.GetOutboundInterchange(eHubTrackingID, factory);
				if (originalInterchange != null)
				{
					var processed = false;
					var originalMessage = CDS.Helpers.EventParentFinderHelper.GetOriginalMessage(originalInterchange.PK, factory);
					if (originalMessage != null)
					{
						processed = ProcessConversationID(eventDataObject, originalMessage, eHubTrackingID);

						if (!processed)
						{
							processed = ProcessCSPID(eventDataObject, originalMessage, eHubTrackingID);
						}

						if (!processed)
						{
							processed = ProcessQueryResponse(eventDataObject, originalMessage, eHubTrackingID);
						}
					}

					var eventType = ((IXmlEventValueObject)eventDataObject).EventType;
					if (eventType.EqualsAny(new[] { CDS.Constants.EHubEventTypes.MessageRejected, CDS.Constants.EHubEventTypes.MessageResponseRejected }))
					{
						processed = ProcessErrorEvent(eventDataObject, originalInterchange.PK);
					}
					if (eventType.Equals(CDS.Constants.EHubEventTypes.DocumentSent))
					{
						var linkedMessageAttachee = messageAttachee ?? originalMessage?.EM_LinkedObject as IMessageAttachee;
						processed = ProcessDocSentConfirmationEvent(eventDataObject, linkedMessageAttachee);
					}

					if (!processed)
					{
						logger.Log(Integration.LogType.Warning, $"Error Event Type: {eventType} was not processed");
					}
				}
			}
		}

		bool ProcessDocSentConfirmationEvent(UniversalEvent eventDataObject, IMessageAttachee messageAttachee)
		{
			var processed = false;
			var dsnResponse = ((IXmlEventValueObject)eventDataObject).Context.ResponseText;

			if (dsnResponse.HasValue && !dsnResponse.Value.IsEmpty)
			{
				ZString dsnMessage;
				try
				{
					dsnMessage = Encoding.UTF8.GetString(Convert.FromBase64String(dsnResponse.Value));
				}
				catch
				{
					dsnMessage = dsnResponse.Value;
				}

				if (!dsnMessage.IsEmpty)
				{
					var xmlDocument = new XmlDocument();
					try
					{
						xmlDocument.LoadXml(dsnMessage);
					}
					catch (XmlException)
					{
					}

					processed = true;

					var fileNodeReferences = xmlDocument.SelectNodes(FileUploadRefPath);
					foreach (XmlNode item in fileNodeReferences)
					{
						var fileReference = item.FirstChild?.Value;
						messageAttachee?.Logs.AddNew(Events.DocumentSent, GetFormattedEventReference(fileReference));
					}
				}
			}

			return processed;
		}

		ZString GetFormattedEventReference(string fileReference)
		{
			var parameters = new Dictionary<string, string>
			{
				{ CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ContentID, fileReference }
			};
			return StmALog.GenerateEventReference(ZString.Empty, parameters);
		}

		const string FileUploadRefPath = "//*[local-name()='FileUploadResponse']/*[local-name()='Files']/*[local-name()='File']/*[local-name()='Reference']";

		bool ProcessConversationID(UniversalEvent eventDataObject, EDIMessage originalMessage, ZGuid eHubTrackingID)
		{
			var processed = false;
			var conversationID = ((IXmlEventValueObject)eventDataObject).Context.ConversationID;
			if (conversationID.HasValue && !string.IsNullOrEmpty(conversationID.Value))
			{
				var conversationIDWithNoHyphens = conversationID.Value.Replace("-", string.Empty);
				processed = CreateResponseMessage(eventDataObject, originalMessage, conversationIDWithNoHyphens, CDSEDIMessageTypeList.Codes.ConversationID, eHubTrackingID, GetCIDMessage);
			}

			return processed;
		}

		bool ProcessCSPID(UniversalEvent eventDataObject, EDIMessage originalMessage, ZGuid eHubTrackingID)
		{
			var processed = false;
			var cspID = ((IXmlEventValueObject)eventDataObject).Context.CSPEntryTrackingID;
			if (cspID.HasValue && !string.IsNullOrEmpty(cspID.Value))
			{
				var cspIDWithNoHyphens = cspID.Value.Replace("-", string.Empty);
				processed = CreateResponseMessage(eventDataObject, originalMessage, cspIDWithNoHyphens, CDSEDIMessageTypeList.Codes.CSPID, eHubTrackingID, GetCSPMessage);
			}

			return processed;
		}

		bool ProcessQueryResponse(UniversalEvent eventDataObject, EDIMessage originalMessage, ZGuid eHubTrackingID)
		{
			var processed = false;
			var eventType = ((IXmlEventValueObject)eventDataObject).EventType;
			if (eventType == Events.ServiceRequestedCode) // Not 100% sure on content returned from eHub, could be Events.ServiceCompletedCode
			{
				var entry = originalMessage.EM_LinkedObject as CusEntryHeader;

				if (entry != null)
				{
					processed = true;

					_ = CreateMessageForQueryResponse(eventDataObject, originalMessage, entry);
				}
				else if (originalMessage is CDSDISQueryMessage)
				{
					var message = CreateMessageForQueryResponse(eventDataObject, originalMessage, entry);
					message.EM_LinkedObject = originalMessage;
					processed = true;
				}
			}

			return processed;
		}

		EDIMessage CreateMessageForQueryResponse(UniversalEvent eventDataObject, EDIMessage originalMessage, CusEntryHeader entry)
		{
			var newCDSResponseMessage = entry != null ? entry.Messages.AddNew(typeof(CDSDeclarationInfoResponseEDIMessage)) : factory.New<CDSDeclarationInfoResponseEDIMessage>();
			newCDSResponseMessage.EM_Status = EDIMessage.Status.Queued;
			newCDSResponseMessage.EM_MessageSubType = CDSEDIMessageTypeList.Codes.QueryResponse;
			newCDSResponseMessage.EM_ApplicationReference = originalMessage.EM_ApplicationReference;
			newCDSResponseMessage.EM_MessageNum = originalMessage.EM_MessageNum.Left(originalMessage.EM_MessageNumInfo.MaxLength - 1) + "R";
			string responseText = ((IXmlEventValueObject)eventDataObject).Context.ResponseText ?? ZString.Empty;
			newCDSResponseMessage.EM_MessageText = Encoding.UTF8.GetString(Convert.FromBase64String(responseText));

			if (!originalMessage.EM_Status.EqualsAny(new ZString[] { EDIMessage.Status.Acknowledged, EDIMessage.Status.Rejected }))
			{
				originalMessage.EM_Status = EDIMessage.Status.Acknowledged;
			}

			return newCDSResponseMessage;
		}

		delegate ZString GetMessageInterpretation(UniversalEvent eventDataObject, ZGuid ehubTrackingID, EDIMessage originalMessage, EDIMessage message, IMessageAttachee messageAttachee);
		bool CreateResponseMessage(UniversalEvent eventDataObject, EDIMessage originalMessage, ZString reference, string messageSubType, ZGuid eHubTrackingID, GetMessageInterpretation getInterpretation)
		{
			var created = false;

			if (originalMessage.EM_LinkedObject is IMessageAttachee messageAttachee)
			{
				var newCDSResponseMessage = messageAttachee.Messages.AddNew(typeof(CDSResponseEDIMessage));
				newCDSResponseMessage.EM_Status = EDIMessage.Status.Received;
				newCDSResponseMessage.EM_MessageSubType = messageSubType;
				newCDSResponseMessage.EM_ApplicationReference = reference;
				newCDSResponseMessage.EM_MessageInterpretation = getInterpretation.Invoke(eventDataObject, eHubTrackingID, originalMessage, newCDSResponseMessage, messageAttachee) + MoreInfo;

				if (!originalMessage.EM_Status.EqualsAny(new ZString[] { EDIMessage.Status.Acknowledged, EDIMessage.Status.Rejected }))
				{
					originalMessage.EM_Status = EDIMessage.Status.Acknowledged;
				}
				newCDSResponseMessage.EM_MessageNum = originalMessage.EM_MessageNum.Left(originalMessage.EM_MessageNumInfo.MaxLength - 1) + "R";

				OverwriteOriginalApplicationReference(messageAttachee, originalMessage, reference, messageSubType);
				created = true;

				var status = CDSMessageStatusCalculator.GetMessageSentStatus(originalMessage as CDSEDIMessage);
				messageAttachee.UpdateStatusIfNotEmpty(status);
			}
			return created;
		}

		void OverwriteOriginalApplicationReference(IMessageAttachee messageAttachee, EDIMessage originalMessage, ZString reference, string messageSubType)
		{
			var dontOverwriteIfAlreadyGUID = messageAttachee.Gateway.EqualsAny(gatewayCodesForDontOverwriteIfGUID) && messageSubType == CDSEDIMessageTypeList.Codes.CSPID;
			var dontOverwrite = dontOverwriteIfAlreadyGUID && ZGuid.TryParse(originalMessage.EM_ApplicationReference, out ZGuid _);
			if (!dontOverwrite)
			{
				originalMessage.EM_ApplicationReference = reference;
			}
		}

		readonly string[] gatewayCodesForDontOverwriteIfGUID = [GatewayList.Codes.Pentant, GatewayList.Codes.MCP_CUSDECOnly, GatewayList.Codes.CNS_CUSDECOnly];

		bool ProcessErrorEvent(UniversalEvent eventDataObject, ZGuid originalInterchangePK)
		{
			var processed = false;
			var errorMessage = ZString.Empty;
			var errorResponse = ((IXmlEventValueObject)eventDataObject).Context.ResponseText;
			var errorType = ((IXmlEventValueObject)eventDataObject).Context.ErrorSummary;
			var errorDetail = ((IXmlEventValueObject)eventDataObject).Context.ErrorDescription;
			var contexts = MessageSourceItemRetriever.GetSourceItems(factory, eventDataObject);

			if (errorResponse.HasValue && !errorResponse.Value.IsEmpty)
			{
				try
				{
					errorMessage = Encoding.UTF8.GetString(Convert.FromBase64String(errorResponse.Value));
				}
				catch
				{
					errorMessage = errorResponse.Value;
				}
			}
			else if (errorDetail.HasValue && !errorDetail.Value.IsEmpty)
			{
				errorMessage = CDS.Helpers.EventParentFinderHelper.GenericErrorWrapper(errorType, errorDetail.Value);
			}

			if (!errorMessage.IsEmpty)
			{
				processed = ProcessErrors(originalInterchangePK, contexts, errorMessage);
			}
			return processed;
		}

		EDIMessage ProcessErrorsForXmlPayload(ZGuid originalInterchangePK, ZString errorMessage)
		{
			EDIMessage newRejectionMessage = null;
			var originalMessage = CDS.Helpers.EventParentFinderHelper.GetOriginalMessage(originalInterchangePK, factory);

			if (originalMessage != null)
			{
				var entry = originalMessage.EM_LinkedObject as CusEntryHeader;

				if (entry != null)
				{
					newRejectionMessage = entry.Messages.AddNew(typeof(CDSErrorResponseEDIMessage));
					newRejectionMessage.EM_Status = EDIMessage.Status.Queued;
					newRejectionMessage.EM_MessageText = errorMessage;
					newRejectionMessage.EM_ApplicationReference = originalMessage.PK.ToString().Replace("-", string.Empty);
					originalMessage.EM_Status = EDIMessage.Status.Rejected;
					newRejectionMessage.EM_MessageNum = originalMessage.EM_MessageNum.Left(originalMessage.EM_MessageNumInfo.MaxLength - 1) + "E";
					var status = CDSMessageStatusCalculator.GetMessageRejectedStatus(originalMessage as CDSEDIMessage);
					entry.UpdateStatusIfNotEmpty(status);
				}
			}
			return newRejectionMessage;
		}

		#region Process errors

		bool ProcessErrors(ZGuid originalInterchangePK, KeyDataPairCollection contexts, string errorMessage)
		{
			var processed = false;
			var newRejectionMessage = ProcessErrorsForXmlPayload(originalInterchangePK, errorMessage);

			if (newRejectionMessage != null)
			{
				try
				{
					var xmlDocument = new XmlDocument();
					xmlDocument.LoadXml(errorMessage);
				}
				catch (XmlException ex)
				{
					var exceptionText = ex.Message; //Not XML, probably HTML error message
					newRejectionMessage.EM_MessageInterpretation = GetFriendlyErrorInterpretation(errorMessage, contexts, exceptionText);
					newRejectionMessage.EM_Status = EDIMessage.Status.ProcessedOK;
					processed = true;
				}
			}
			return processed;
		}

		ZString GetFriendlyErrorInterpretation(ZString responseTextFromContext, KeyDataPairCollection contexts, ZString exceptionText)
		{
			var interpretation = new ZStringBuilder(MessagePrettierCss.CSS);
			interpretation.Append(HeaderTextForMessageRejection);

			var errorFromContext = contexts.Cast<KeyDataPair>().FirstOrDefault(x => x.Key == ContextKey_Error);
			if (errorFromContext != null && !string.IsNullOrEmpty(errorFromContext.Data))
			{
				interpretation.Append($"<h2>{errorFromContext.Data}</h2>");
			}
			else
			{
				if (!exceptionText.IsEmpty)
				{
					interpretation.Append(ErrorMessageToDisplayWhenNonXmlPayload);
					interpretation.Append(exceptionText);
				}
			}

			foreach (KeyDataPair context in contexts)
			{
				if (context.Key != ContextKey_Error && context.Key != ContextKey_ResponseText)
				{
					var errorResponse = new CDSErrorResponse { FieldInError = context.Key, ErrorReason = context.Data };
					interpretation.AppendFormat("<li>{0}</li>", errorResponse.DisplayError);
				}
			}

			if (!string.IsNullOrEmpty(responseTextFromContext))
			{
				interpretation.Append(responseTextFromContext);
			}

			return interpretation.ToString();
		}

		#endregion

		ZString GetCIDMessage(UniversalEvent eventDataObject, ZGuid ehubTrackingID, EDIMessage originalMessage, EDIMessage message, IMessageAttachee messageAttachee)
		{
			var eventType = ((IXmlEventValueObject)eventDataObject).EventType;
			var interchange = CDS.Helpers.EventParentFinderHelper.GetOutboundInterchange(ehubTrackingID, factory);
			var cspID = ((IXmlEventValueObject)eventDataObject).Context.CSPEntryTrackingID;
			var cspMessage = messageAttachee.Messages.OfType<EDIMessage>().FirstOrDefault(x => x.EM_EI.Equals(interchange.PK) && x.EM_MessageSubType.EqualsIgnoringCase(CDSEDIMessageTypeList.Codes.CSPID));

			return CDSEDIMessagePrettier.GetCIDMessage(cspID, ehubTrackingID, originalMessage, message, cspMessage, CDS.Helpers.EventParentFinderHelper.ehubRecipient, eventDataObject.EventTime, eventType);
		}

		ZString GetCSPMessage(UniversalEvent eventDataObject, ZGuid ehubTrackingID, EDIMessage originalMessage, EDIMessage message, IMessageAttachee messageAttachee)
		{
			var interchange = CDS.Helpers.EventParentFinderHelper.GetOutboundInterchange(ehubTrackingID, factory);
			var cidMessage = messageAttachee.Messages.OfType<EDIMessage>().FirstOrDefault(x => x.EM_EI.Equals(interchange.PK) && x.EM_MessageSubType.EqualsIgnoringCase(CDSEDIMessageTypeList.Codes.ConversationID));
			return CDSEDIMessagePrettier.GetCSPMessage(cidMessage, ehubTrackingID, originalMessage, message, eventDataObject.EventTime);
		}

		public const string MoreInfo = @"
<p>
<h4>For further information please see our learning units</h4>
<ul>
<li>1BGB046 for direct communication with CDS</li>
<li>1BGB047 for CSP communication with CDS</li>
</ul>";

		const string HeaderTextForMessageRejection = "<h3>Message was rejected by an upstream system for the following reasons</h3>";
		const string ErrorMessageToDisplayWhenNonXmlPayload = "<h2>Error reading xml</h2>";
		const string ContextKey_ResponseText = "Response Text";
		const string ContextKey_Error = "Error";
	}
}
