using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.WorkflowManager;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentVisualizer.Business
{
	sealed class UXmlSender
	{
		public struct Parameters
		{
			public IMessageInstructions Instructions { get; set; }
			public IDocument Document { get; set; }
			public IVisualizerDocumentData DocumentData { get; set; }
			public IDocumentDeliveryService DeliveryService { get; set; }
			public MessageType MessageType { get; set; }
			public object ReasonForSending { get; set; }
		}

		public const string ReasonForMessageAmendmentNoteDescription = "ReasonForMessageAmendment";
		public const string ReasonForMessageCancellationNoteDescription = "ReasonForMessageCancellation";

		public UXmlSender(Parameters parameters)
		{
			this.parameters = parameters;
		}

		readonly Parameters parameters;

		bool AreParametersValid => parameters.Instructions != null
			&& parameters.Document != null
			&& parameters.DocumentData != null
			&& parameters.DeliveryService != null;

		public bool Send(INotifications notifications)
		{
			if (!AreParametersValid)
			{
				return false;
			}

			var storage = (BusinessObject)parameters.DocumentData;

			BusinessObjectFactory factory;
			BusinessObject docData;
			BusinessObject docDataParent;

			var useNewFactory = storage.IsInDatabase
				&& Globals.IsUserInteractive;

			if (useNewFactory)
			{
				factory = new BusinessObjectFactory();

				docData = factory.ImportFromAnotherFactory((BusinessObject)parameters.DocumentData);

				var parentBizObj = (BusinessObject)parameters.DocumentData.Parent;
				docDataParent = parentBizObj.GetSupporter()?.GetBusinessObjectInAnotherFactory(factory, parentBizObj) as BusinessObject
					?? factory.ImportFromAnotherFactory(parentBizObj);
			}
			else
			{
				factory = storage.Factory;

				docData = storage;
				docDataParent = (BusinessObject)parameters.DocumentData.Parent;
			}

			using (useNewFactory ? factory.AddDisposableService() : null)
			{
				var recipientDescription = GetUntranslatedMessageRecipient();
				var purposeCode = ProcessTaskTriggerPurposeList.Codes.AsPerPayload;

				var communicationProvider = new UniversalXmlCommunicationModeProvider(() =>
				{
					var id = parameters.Instructions.EHubClientID;

					var messageBroker = docDataParent.GetSupporter()?.GetMessageBroker();
					if (string.IsNullOrEmpty(messageBroker))
					{
						messageBroker = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
					}

					if (!string.IsNullOrEmpty(id) && messageBroker == EDICommunicationsModeCommunicationsTransportList.Codes.EHubService)
					{
						var mode = factory.New<VisualizerEDICommunicationsMode>();
						mode.EK_Module = (docDataParent as IWorkflowProvider)?.WorkflowType ?? ZString.Empty;
						mode.EK_Destination = parameters.Instructions.EHubClientID;

						return (new IEDICommunicationsMode[] { mode }, null);
					}
					else if (!string.IsNullOrEmpty(parameters.Instructions.DirectXTClientID) && messageBroker == EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface)
					{
						var mode = factory.New<VisualizerEDICommunicationsMode>();
						mode.EK_Module = (docDataParent as IWorkflowProvider)?.WorkflowType ?? ZString.Empty;
						mode.EK_Destination = parameters.Instructions.DirectXTClientID;
						mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface;

						return (new IEDICommunicationsMode[] { mode }, null);
					}
					else
					{
						var communicationSettings = parameters.DeliveryService.GetCommunicationSettings(docDataParent, notifications);

						recipientDescription = communicationSettings.Recipient.Description;
						purposeCode = communicationSettings.Purpose.Code;
						var communicationsModes = communicationSettings.CommunicationsModes.ToArray();
						if (!communicationsModes.Any())
						{
							return (Array.Empty<IEDICommunicationsMode>(), ResString.GetMultilingualString("4878e406-27cd-4d19-9099-1f650b7edb09", "No communications modes found for Client ID [{0}].", id));
						}
						else
						{
							return (communicationsModes, null);
						}
					}
				});

				if (parameters.MessageType == MessageType.Withdrawal)
				{
					purposeCode = MessagePurposes.Codes.Withdrawal;
				}

				var actionInfo = new ActionInfo(docData, purposeCode);

				var logParent = docData as IStmALogParent;

				if (SendUXml(logParent, notifications, actionInfo, communicationProvider, docDataParent))
				{
					if (!TryAddEventsFromSupporter(docDataParent, logParent, recipientDescription))
					{
						AddMessageEvents(docDataParent, logParent, recipientDescription);
					}

					// service tasks are taking care of the save operation
					if (Globals.IsUserInteractive
						|| Globals.IsWebService)
					{
						try
						{
							ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, null);
						}
						catch (ZSaveException ex)
						{
							ErrorReporter.ReportOnce("UXmlSender.ProcessWithSaveExceptionHandling", ex.Message, ex);
							notifications.AddError(Res.GetString("CE60C54F-E6FB-4EBB-8832-658595BCBBA0", "The system has encountered an error while sending message. Please try again later."));
							return false;
						}
					}

					return true;
				}

				return false;
			}
		}

		string GetUntranslatedMessageRecipient()
		{
			using (Res.TemporarilySwitchLanguage(Res.DefaultLanguage))
			{
				return parameters.Instructions.Recipient;
			}
		}

		bool SendUXml(IStmALogParent logParent, INotifications notifications, IUniversalActionInfo actionInfo, IMessageProcessorCommunicationModesResult communicationsModes, BusinessObject exportedBusinessObject)
		{
			var dataObjectWithWriter = GetTopLevelDataObject(parameters.Document, exportedBusinessObject);

			if (dataObjectWithWriter.IsLeft)
			{
				notifications.AddMessageError(dataObjectWithWriter.Left);
				return false;
			}

			var additionalDataObjectsWithWriter = GetAdditionalTopLevelDataObjects(parameters.Document, exportedBusinessObject);

			if (additionalDataObjectsWithWriter.IsLeft)
			{
				notifications.AddMessageError(additionalDataObjectsWithWriter.Left);
				return false;
			}

			var dataObject = dataObjectWithWriter.Right.TopLevelDataObject;

			PopulateDataContext(logParent, dataObject);
			PopulateSendReason(exportedBusinessObject, dataObject);

			var topLevelDataObjectWriter = new KnownTopLevelDataObjectWriter(dataObject);
			ITopLevelDataObjectWriter DataWriterGetter(IDataWritingManager x) => topLevelDataObjectWriter;

			var xmlWriter = dataObjectWithWriter.Right.XmlWriter;

			var processor = UniversalXmlWorkflowProcessorBuilder.New(
				actionInfo,
				communicationsModes,
				DataWriterGetter,
				exportedBusinessObject,
				null,
				xmlWriter,
				UniversalXmlSchema.Version_2012_11_DO_NOT_USE);

			foreach (var additionalDataObjectWithWriter in additionalDataObjectsWithWriter.Right)
			{
				PopulateDataContext(logParent, additionalDataObjectWithWriter.TopLevelDataObject, additionalDataObjectWithWriter.Document);

				((ISupportUniversalBatchExport)processor).AddAnotherTopLevelDataObjectWriterAndXmlWriter(new KnownTopLevelDataObjectWriter(additionalDataObjectWithWriter.TopLevelDataObject), additionalDataObjectWithWriter.XmlWriter);
			}

			using (processor is IUniversalXmlContentFilterApplicatorSuspendable suspendable
				? suspendable.SuspendUniversalXmlContentFilterApplicator()
				: null)
			{
				var replaceThisTokenEventuallyQuestionMarkExclamationMark = CancellationToken.None;
				return processor
					.Process(notifications, replaceThisTokenEventuallyQuestionMarkExclamationMark)
					.Any(e => e.EventType != Events.DataExportFailureCode);
			}
		}

		Either<string, DataObjectWithWriter> GetTopLevelDataObject(IDocument document, BusinessObject exportedBusinessObject)
		{
			var dynamicData = document.Data;
			if (dynamicData.Value is ITopLevelDataObject topLevelDataObject)
			{
				var dynamicDataUXmlWriter = new DynamicDataUXmlWriter(dynamicData, parameters.Instructions.XmlNamespace);
				var result = new DataObjectWithWriter(topLevelDataObject, dynamicDataUXmlWriter);

				return new Either<string, DataObjectWithWriter>(result);
			}

			var supporter = exportedBusinessObject.GetSupporter();

			if (supporter == null)
			{
				return Res.GetString("f94707b8-6bb2-4eee-a03e-5255425e7ea6", "Universal XML is not supported");
			}

			var uxmlDataObject = supporter.GetUniversalXmlDataObject(DefaultDataObjectWriterStrategy.Instance, document, parameters.MessageType);

			if (uxmlDataObject.IsLeft)
			{
				return uxmlDataObject.Left;
			}

			var xmlNamespace = GetXmlNamespace();
			var dataObjectUXmlWriter = new DocDataObjectUXmlWriter(uxmlDataObject.Right, xmlNamespace);
			return new DataObjectWithWriter(uxmlDataObject.Right, dataObjectUXmlWriter);
		}

		Either<string, IEnumerable<DataObjectWithWriter>> GetAdditionalTopLevelDataObjects(IDocument document, BusinessObject exportedBusinessObject)
		{
			var dynamicData = document.Data;
			if (dynamicData.Value is not ITopLevelDataObject topLevelDataObject)
			{
				var supporter = exportedBusinessObject.GetSupporter();

				if (supporter == null)
				{
					return Res.GetString("f94707b8-6bb2-4eee-a03e-5255425e7ea6", "Universal XML is not supported");
				}

				var result = new List<DataObjectWithWriter>();
				var additionalDocuments = supporter.GetAdditionalDocuments(document, parameters.Instructions);

				if (additionalDocuments != null)
				{
					foreach (var additionalDocument in additionalDocuments)
					{
						var additionalUxmlDataObject = supporter.GetUniversalXmlDataObject(DefaultDataObjectWriterStrategy.Instance, additionalDocument, parameters.MessageType);

						if (additionalUxmlDataObject.IsLeft)
						{
							return additionalUxmlDataObject.Left;
						}

						var xmlNamespace = GetXmlNamespace(additionalDocument);
						result.Add(new DataObjectWithWriter(additionalUxmlDataObject.Right, new DocDataObjectUXmlWriter(additionalUxmlDataObject.Right, xmlNamespace), additionalDocument));
					}
				}

				return result;
			}

			return new Either<string, IEnumerable<DataObjectWithWriter>>(Enumerable.Empty<DataObjectWithWriter>());
		}

		bool TryAddEventsFromSupporter(BusinessObject docDataParent, IStmALogProvider logParent, string recipientDescription)
		{
			var supporter = docDataParent.GetSupporter();

			if (supporter?.GetMessageLogCreator(parameters.Document) is IMessageLogCreator logCreator)
			{
				return parameters.MessageType == MessageType.Withdrawal
					? logCreator.CreateWithdrawalSentLog(logParent, parameters.Document.Data, parameters.Instructions.DocumentName, recipientDescription, parameters.ReasonForSending)
					: logCreator.CreateMessageSentLog(logParent, parameters.Document.Data, parameters.Instructions.DocumentName, recipientDescription);
			}

			return false;
		}

		void AddMessageEvents(BusinessObject docDataParent, IStmALogParent logParent, string recipient)
		{
			logParent?.Logs.CreateOrRecreateEventLog(
				GetEvent(),
				EstimateActual.Actual,
				ZDateTimeOffset.Now,
				ZString.Empty,
				GetParametersForEvent(docDataParent, parameters.ReasonForSending, recipient));
		}

		ZArchitecture.Business.Event GetEvent()
		{
			return parameters.MessageType == MessageType.Withdrawal
				? Events.MessageWithdrawCancelRequest
				: Events.MessageSent;
		}

		KeyValuePair<string, string>[] GetParametersForEvent(BusinessObject docDataParent, object reasonForSending, string recipient)
		{
			var result = new List<KeyValuePair<string, string>>();

			result.Add(new KeyValuePair<string, string>(
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType,
				parameters.Instructions.DocumentName));

			result.Add(new KeyValuePair<string, string>(
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department,
				recipient));

			var reason = Convert.ToString(reasonForSending);

			if (!string.IsNullOrWhiteSpace(reason))
			{
				result.Add(new KeyValuePair<string, string>(
					CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason,
					reason));
			}

			var supporter = docDataParent.GetSupporter();

			if (supporter?.GetMessagingExtensions(parameters.Document, parameters.Instructions) is IMessagingExtensions messagingExtensions)
			{
				var additionalParametersForEvent = messagingExtensions.GetAdditionalParametersForEvent();
				if (!additionalParametersForEvent.IsNullOrEmpty())
				{
					result.AddRange(additionalParametersForEvent);
				}
			}

			return result.ToArray();
		}

		void PopulateDataContext(IStmALogParent logParent, ITopLevelDataObject dataObject, IDocument document = null)
		{
			var submissionVersion = logParent?.CalculateSubmissionVersion(parameters.Instructions.DocumentName) ?? 1;

			var dataVersion = logParent?.CalculateDataVersion(parameters.Instructions.DocumentName, parameters.Instructions.OrderLogsByLocalTime) ?? 1;
			var purposeCode = GetDocumentPurposeCode(dataVersion);
			var documentaryOverrideDocumentName = GetDocumentaryOverrideDocumentName(document);

			dataObject?.DataContext?.SetDocumentaryOverride(
				documentaryOverrideDocumentName,
				purposeCode,
				null,
				true,
				dataVersion,
				submissionVersion);
		}

		void PopulateSendReason(BusinessObject docDataParent, ITopLevelDataObject dataObject)
		{
			if (parameters.ReasonForSending is null)
			{
				return;
			}

			var supporter = docDataParent.GetSupporter();

			if (supporter?.GetMessagingExtensions(parameters.Document, parameters.Instructions) is IMessagingExtensions messagingExtensions)
			{
				var hasBeenPopulated = false;

				if (parameters.MessageType == MessageType.Withdrawal)
				{
					hasBeenPopulated = messagingExtensions is ICustomMessageWithdrawalSupporter customMessageWithdrawalSupporter
						&& customMessageWithdrawalSupporter.PopulateMessageWithdrawalReason(dataObject, parameters.ReasonForSending);
				}
				else
				{
					hasBeenPopulated = messagingExtensions is ICustomMessageAmendmentSupporter customMessageAmendmentSupporter
						&& customMessageAmendmentSupporter.PopulateMessageAmendmentReason(dataObject, parameters.ReasonForSending);
				}

				if (hasBeenPopulated)
				{
					return;
				}
			}

			if (!(parameters.ReasonForSending is string reason)
				|| string.IsNullOrWhiteSpace(reason)
				|| !(dataObject is Shipment shipment))
			{
				return;
			}

			var noteDescription = parameters.MessageType == MessageType.Withdrawal
				? ReasonForMessageCancellationNoteDescription
				: ReasonForMessageAmendmentNoteDescription;

			var note = new Note
			{
				Description = noteDescription,
				IsCustomDescription = true,
				NoteText = reason
			};

			if (shipment.NoteCollection == null)
			{
				shipment.SetNoteCollection(() => new UniversalDataBuss.DataObjects.Core.DataObjectList<Note>
				{
					note
				});
			}
			else
			{
				var existingNote = shipment
					.NoteCollection
					.FirstOrDefault(n => n.Description.GetValueOrDefault() == noteDescription);

				if (existingNote != null)
				{
					shipment.NoteCollection.Remove(existingNote);
				}

				shipment.NoteCollection.Add(note);
			}
		}

		string GetDocumentPurposeCode(int dataVersion)
		{
			if (parameters.MessageType == MessageType.Withdrawal)
			{
				return MessagePurposes.Codes.Withdrawal;
			}

			if (IsSendingAmendment(dataVersion))
			{
				return MessagePurposes.Codes.Amendment;
			}

			return MessagePurposes.Codes.Original;
		}

		bool IsSendingAmendment(int dataVersion)
		{
			var bizObj = parameters.DocumentData?.Parent as BusinessObject;
			var isSendingAmendment = bizObj
				?.GetSupporter()
				?.GetMessagingExtensions(parameters.Document, parameters.Instructions)
				?.IsSendingAmendment();

			return isSendingAmendment ?? dataVersion > 1;
		}

		string GetXmlNamespace(IDocument document = null)
		{
			string xmlNamespace = null;

			if (parameters.DocumentData?.Parent is BusinessObject bizObj)
			{
				xmlNamespace = bizObj
					?.GetSupporter()
					?.GetMessagingExtensions(document ?? parameters.Document, parameters.Instructions)
					?.GetXmlNamespace();
			}

			return xmlNamespace ?? parameters.Instructions.XmlNamespace;
		}

		string GetDocumentaryOverrideDocumentName(IDocument document = null)
		{
			string documentName = null;

			if (parameters.DocumentData?.Parent is BusinessObject bizObj)
			{
				documentName = bizObj
					?.GetSupporter()
					?.GetMessagingExtensions(document ?? parameters.Document, parameters.Instructions)
					?.GetDocumentaryOverrideDocumentName();
			}

			return string.IsNullOrEmpty(documentName) ? parameters.Instructions.DocumentName : documentName;
		}

		#region Nested Types

		sealed class DataObjectWithWriter
		{
			public DataObjectWithWriter(ITopLevelDataObject topLevelDataObject, IXmlWriter xmlWriter, IDocument document = null)
			{
				TopLevelDataObject = topLevelDataObject;
				XmlWriter = xmlWriter;
				Document = document;
			}

			public ITopLevelDataObject TopLevelDataObject { get; }
			public IXmlWriter XmlWriter { get; }
			public IDocument Document { get; }
		}

		#endregion
	}
}
