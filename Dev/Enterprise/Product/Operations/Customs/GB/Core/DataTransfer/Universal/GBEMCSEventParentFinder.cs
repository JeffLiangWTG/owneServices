using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.CDS;
using Enterprise.Customs.GB.EMCS.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.GB.DataTransfer.Universal
{
	public class GBEMCSEventParentFinder : EventParentFinder
	{
		public GBEMCSEventParentFinder(BusinessObjectFactory factory, IEventDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent eventDataObject)
		{
			return FindHeaderAndProcess(eventDataObject);
		}

		protected override BusinessObject[] GetChildrenIfSpecifiedInContext(BusinessObject[] logParents, UniversalEvent eventData)
		{
			BusinessObject[] result = null;
			if (FindHeaderAndProcess(eventData) != null)
			{
				result = base.GetChildrenIfSpecifiedInContext(logParents, eventData);
			}
			return result;
		}

		protected BusinessObject[] FindHeaderAndProcess(UniversalEvent xmlEvent)
		{
			BusinessObject[] result = null;

			if (IsExpectedDataProvider(xmlEvent) && GetOriginalMessage(xmlEvent) is EDIMessage originalMessage)
			{
				UpdateEntry(xmlEvent, originalMessage);

				var declaration = originalMessage.EM_LinkedObject as EMCSJobDeclaration;
				if (declaration != null)
				{
					if (originalMessage.EM_Status == EDIMessage.Status.Failed)
					{
						var contextCollection = xmlEvent.ContextCollection;
						if (contextCollection != null)
						{
							CreateNewRejectionMessageWithErrors(declaration, xmlEvent);
						}
					}
					result = new BusinessObject[] { declaration };
				}
			}
			return result;
		}

		static string GetDecodedResponseText(IXmlEventValueObject eventDataObject)
		{
			try
			{
				var responseText = eventDataObject.Context.ResponseText;
				return Encoding.UTF8.GetString(Convert.FromBase64String(responseText));
			}
			catch
			{
				return null;
			}
		}

		public static EMCSJsonResponse GetJsonResponse(string responseTextDecoded)
		{
			try
			{
				var jsonResponse = JsonSerializer.Deserialize<EMCSJsonResponse>(responseTextDecoded);
				jsonResponse.MessageText = responseTextDecoded;
				return jsonResponse;
			}
			catch
			{
				return null;
			}
		}

		public static EMCSXmlResponse GetResponseTextXml(string responseTextDecoded, IXmlEventValueObject xmlEvent)
		{
			try
			{
				var xmlResponse = new EMCSXmlResponse();
				xmlResponse.Document.LoadXml(InterpretationHelper.RemoveAllNamespaces(responseTextDecoded));
				xmlResponse.ErrorSummary = xmlEvent.Context.ErrorSummary;
				xmlResponse.MessageText = responseTextDecoded;
				return xmlResponse;
			}
			catch
			{
				return null;
			}
		}

		void CreateNewRejectionMessageWithErrors(EMCSJobDeclaration jobDeclaration, UniversalEvent universalEvent)
		{
			var responseTextDecoded = GetDecodedResponseText(universalEvent);
			if (responseTextDecoded != null)
			{
				var jsonResponse = GetJsonResponse(responseTextDecoded);
				if (jsonResponse != null)
				{
					CreateNewRejectionMessageWithErrorsFromJson(jobDeclaration, jsonResponse);
				}
				else
				{
					var xmlResponse = GetResponseTextXml(responseTextDecoded, universalEvent);
					if (xmlResponse != null)
					{
						CreateNewRejectionMessageWithErrorsFromXml(jobDeclaration, xmlResponse);
					}
				}
			}
		}

		void CreateNewRejectionMessageWithErrorsFromJson(EMCSJobDeclaration jobDeclaration, EMCSJsonResponse jsonResponse)
		{
			var interpretation = new GBEMCSInterpretationPrettier().CreatePrettyRejectionInterpretationFromJson(jsonResponse);
			CreateNewRejectionMessageWithErrors(jobDeclaration, jsonResponse.MessageText, interpretation);
		}

		void CreateNewRejectionMessageWithErrorsFromXml(EMCSJobDeclaration jobDeclaration, EMCSXmlResponse xmlResponse)
		{
			var interpretation = new GBEMCSInterpretationPrettier().CreatePrettyRejectionInterpretationFromXml(xmlResponse);
			CreateNewRejectionMessageWithErrors(jobDeclaration, xmlResponse.MessageText, interpretation);
		}

		void CreateNewRejectionMessageWithErrors(EMCSJobDeclaration declaration, ZString messageText, ZString interpretation)
		{
			var newRejectionMessage = declaration.Messages.AddNew(typeof(EMCSInboundEDIMessage));
			newRejectionMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			newRejectionMessage.EM_MessageType = CDSEDIMessageTypeList.Codes.EHubErrorResponse;
			newRejectionMessage.EM_Status = EDIMessage.Status.ProcessedOK;
			newRejectionMessage.EM_MessageInterpretation = interpretation;
			newRejectionMessage.EM_MessageText = messageText;
			declaration.JE_MessageStatus = EDIMessageStatusList.Codes.Rejected;
		}

		void AddMessageForPVTQueryResponse(EDIMessage originalMessage, IXmlEventValueObject eventDataObject)
		{
			var response = GetDecodedResponseText(eventDataObject);
			if (response != null)
			{
				var header = originalMessage.EM_LinkedObject as EMCSJobDeclaration;
				if (header != null)
				{
					var eventDeserializer = new XmlEventDeserializer();
					var outgoingXmlEvent = eventDeserializer.Parse(originalMessage.EM_MessageText);
					var outgoingEventDataObject = outgoingXmlEvent as UniversalEvent;
					var requestEncoded = outgoingEventDataObject.ContextCollection
						.Find(x => (string)x.Type?.Type == PreValidateTraderHelper.Constants.ContextTypes.PreValidateTraderBody)?.Value ?? ZString.Empty;
					var request = !requestEncoded.IsEmpty ? Encoding.UTF8.GetString(Convert.FromBase64String(requestEncoded)) : string.Empty;

					var prettier = new GBEMCSInterpretationPrettier();
					var message = header.Messages.AddNew(typeof(EMCSInboundEDIMessage));
					message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
					message.EM_MessageType = CDSEDIMessageTypeList.Codes.QueryResponse;
					message.EM_Status = EDIMessage.Status.ProcessedOK;
					message.EM_MessageText = response;
					message.EM_MessageInterpretation = prettier.CreatePrettyPVTResponseInterpretationFromJson(request, response);
				}
			}
		}

		bool IsExpectedDataProvider(UniversalEvent eventDataObject)
		{
			return (eventDataObject.DataContext?.DataProviderForCodeMapping ?? ZString.Empty) == Constants.EMCS;
		}

		EDIMessage GetOriginalMessage(UniversalEvent eventDataObject)
		{
			EDIMessage result = null;
			if (ZGuid.TryParse(((IXmlEventValueObject)eventDataObject).Context.EHubTrackingID, out var eHubTrackingID))
			{
				result = EMCSHelper.GetOriginalMessage(factory, eHubTrackingID);
			}
			return result;
		}

		void UpdateEntry(UniversalEvent eventDataObject, EDIMessage originalMessage)
		{
			if (originalMessage != null)
			{
				var processed = ProcessEvent(eventDataObject, originalMessage);
				if (!processed)
				{
					logger.Log(Integration.LogType.Warning, "Event was not processed");
				}
			}
		}

		bool ProcessEvent(UniversalEvent eventDataObject, EDIMessage originalMessage)
		{
			var responseType = eventDataObject.ContextCollection.Find(x => (string)x.Type?.Type == "ResponseType")?.Value ?? ZString.Empty;
			var processed = false;
			var eventType = ((IXmlEventValueObject)eventDataObject).EventType;
			switch (eventType)
			{
				case Events.MessageRejectedCode when responseType == "Pre-Validate Trader Response":
					processed = true;
					break;
				case Events.ExternalValidationPassedCode when responseType == "Pre-Validate Trader Response":
					originalMessage.EM_Status = EDIMessage.Status.Acknowledged;
					AddMessageForPVTQueryResponse(originalMessage, eventDataObject);
					processed = true;
					break;

				case Events.MessageRequestedToBeSentCode:
					originalMessage.EM_Status = EDIMessage.Status.Acknowledged;
					var movementId = ((IXmlEventValueObject)eventDataObject).Context.CorrelationID;
					if (movementId.HasValue)
					{
						originalMessage.EM_ApplicationReference = ((ZString)movementId).SubstringSafe(0, EDIMessage.Schema.EM_ApplicationReferenceMaxLength);
					}
					SetDeclarationMessageStatusToAcknowledged(originalMessage.EM_LinkedObject as EMCSJobDeclaration);
					processed = true;
					break;
				case Events.MessageRejectedCode:
					originalMessage.EM_Status = EDIMessage.Status.Failed;
					processed = true;
					break;
			}
			return processed;
		}

		void SetDeclarationMessageStatusToAcknowledged(EMCSJobDeclaration declaration)
		{
			if (declaration != null)
			{
				try
				{
					var newFactory = new BusinessObjectFactory();
					var reloadedDeclaration = newFactory.Load<EMCSJobDeclaration>(declaration.PK);
					if (reloadedDeclaration.JE_MessageStatus == EDIMessage.Status.Sent)
					{
						reloadedDeclaration.JE_MessageStatus = EDIMessage.Status.Acknowledged;
						newFactory.Save();
					}
				}
				catch (ZSaveConcurrencyException)
				{
				}
			}
		}

		public sealed class EMCSXmlResponse
		{
			public EMCSXmlResponse()
			{
				Document = new XmlDocument();
			}

			public XmlDocument Document { get; set; }
			public string ErrorSummary { get; set; }
			public string MessageText { get; set; }
		}

		public sealed class EMCSJsonResponse
		{
			[JsonPropertyName("dateTime")]
			public string DateTime { get; set; }

			[JsonPropertyName("emcsCorrelationId")]
			public string EmcsCorrelationId { get; set; }

			[JsonPropertyName("correlationId")]
			public string CorrelationId { get; set; }

			[JsonPropertyName("message")]
			public string Message { get; set; }

			[JsonPropertyName("debugMessage")]
			public string DebugMessage { get; set; }

			[JsonPropertyName("errors")]
			public IEnumerable<EMCSJsonResponseErrors> Errors { get; set; }

			[JsonPropertyName("validatorResults")]
			public IEnumerable<EMCSJsonResponseValidatorResults> ValidatorResults { get; set; }

			[JsonIgnore]
			public string MessageText { get; set; }
		}

		public sealed class EMCSJsonResponseErrors
		{
			[JsonPropertyName("errorCode")]
			public int ErrorCode { get; set; }

			[JsonPropertyName("errorMessage")]
			public string ErrorMessage { get; set; }

			[JsonPropertyName("location")]
			public string Location { get; set; }

			[JsonPropertyName("value")]
			public string Value { get; set; }
		}

		public sealed class EMCSJsonResponseValidatorResults
		{
			[JsonPropertyName("errorCategory")]
			public string ErrorCategory { get; set; }

			[JsonPropertyName("errorType")]
			public int ErrorType { get; set; }

			[JsonPropertyName("errorReason")]
			public string ErrorReason { get; set; }

			[JsonPropertyName("errorLocation")]
			public string ErrorLocation { get; set; }

			[JsonPropertyName("originalAttributeValue")]
			public string OriginalAttributeValue { get; set; }
		}
	}
}
