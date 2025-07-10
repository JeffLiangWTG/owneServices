using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Schema;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.EDIMessages;
using Enterprise.Customs.ES.Business.MessageSending;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business
{
	public abstract class ESCommonResponseMessageProcessor<T, TResponseProvider> : ESBranchCustomsApplicationTypeMessageProcessor<TResponseProvider>
		where T : BusinessObject
	{
		protected ESCommonResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger)
		{
			EDocsSaver = eDocsSaver;
		}

		protected readonly IEDocsDelayedSaver EDocsSaver;

		protected virtual ZBool ShouldHaveSentInterchange => true;
		protected virtual ZBool IsInboxDeclaration => false;
		protected virtual ZBool IsQueryMessage => false;
		protected virtual ZBool IsAnnexMessage(EDIMessage message) => false;

		protected sealed override void PreProcessMessageCore(EDIMessage message)
		{
			base.PreProcessMessageCore(message);

			var isDirectxTMessage = (message.Interchange?.EI_TransportType ?? ZString.Empty) == EDIInterchange.TransportType.xT;

			if (!message.EM_ApplicationReference.IsEmpty || isDirectxTMessage)
			{
				var relevantBO = FindRelevantBusinessObject(message, isDirectxTMessage);
				if (relevantBO == null)
				{
					SetMessageStatusAsFailed(message);
					Logger.LogWarning(Res.GetString("2734361F-98D4-41A9-A0CF-CDB7CCC220C5", "Unable to find business object for message {0}", GetEDIMessageStatusLogDescription(message)));
				}
				else
				{
					message.EM_LinkedObject = relevantBO;
					var branchPK = GetRelevantBOBranch(relevantBO);
					if (!branchPK.IsEmpty)
					{
						message.EM_GB = branchPK;
					}
					message.EM_Status = EDIMessageStatusList.Codes.PreProcessedOK;
				}
			}
			else
			{
				SetMessageStatusAsFailed(message);
				Logger.LogWarning(Res.GetString("534BE49F-93D5-4EE3-8F22-13DFED1F0E2E", "No Application Reference found for message {0}", GetEDIMessageStatusLogDescription(message)));
			}
		}

		protected sealed override void ProcessMessageCore(EDIMessage message)
		{
			var linkedObject = message.EM_LinkedObject as T;
			try
			{
				CheckDataConsistency(message, linkedObject);
				var provider = GetMessageProvider(message);
				ProcessMessageCore(message, linkedObject, provider);
			}
			catch (Exception ex) when (ex is XmlSchemaValidationException || ex is InvalidOperationException)
			{
				SetMessageStatusAsFailed(message);
				if (linkedObject != null)
				{
					SetCHStatusAsFailed(linkedObject);
				}
				Logger.LogWarning(GetEDIMessageFailedLogDescription(message));
				Logger.LogWarning(GetExceptionLogDescription(ex));

				message.EM_MessageInterpretation = GetFailureMessageDetails(ex.Message, message);
			}
		}

		void CheckDataConsistency(EDIMessage message, T linkedObject)
		{
			if (linkedObject == null)
			{
				throw new InvalidOperationException(Res.GetString("0D9187D6-EBBF-41A0-AEE6-E3F9749243D7", "Message's Linked Object is null so can't continue with processing"));
			}
			CheckMessageTextNotEmpty(message);
		}

		protected abstract void ProcessMessageCore(EDIMessage message, T linkedBusinessObject, TResponseProvider provider);

		T FindRelevantBusinessObject(EDIMessage message, bool isDirectxTMessage)
		{
			try
			{
				var factory = message.Factory;
				var sentBusinessObjects = Array.Empty<BusinessObject>();
				if (ShouldHaveSentInterchange)
				{
					sentBusinessObjects = MessageProcessorHelper.GetRelatedSentInterchange(message).ContainedMessages.Cast<EDIMessage>().Where(x => x.EM_LinkedObject != null).Select(x => x.EM_LinkedObject).ToArray();
				}
				return FindRelevantBusinessObjectCore(message, sentBusinessObjects, isDirectxTMessage);
			}
			catch (Exception ex) when (ex is XmlSchemaValidationException || ex is InvalidOperationException)
			{
				Logger.LogWarning(GetExceptionLogDescription(ex));
				return null;
			}
		}

		protected virtual T FindRelevantBusinessObjectCore(EDIMessage message, BusinessObject[] sentBusinessObjects, bool isDirectxTMessage) => isDirectxTMessage
																																				? FindRelevantBusinessObjectCoreForDirectxT(message, sentBusinessObjects)
																																				: FindRelevantBusinessObjectCoreForEHub(message, sentBusinessObjects);

		protected virtual T FindRelevantBusinessObjectCoreForEHub(EDIMessage message, BusinessObject[] sentBusinessObjects) => (T)sentBusinessObjects.FirstOrDefault(x => ((IESResponseBusinessObject)x).EntryReference == message.EM_ApplicationReference);

		protected virtual T FindRelevantBusinessObjectCoreForDirectxT(EDIMessage message, BusinessObject[] sentBusinessObjects) => (T)sentBusinessObjects.FirstOrDefault(x => !((IESResponseBusinessObject)x).EntryReference.IsEmpty);

		protected void SetMessageStatusAsRejected(EDIMessage message) => message.EM_Status = EDIMessageStatusList.Codes.Rejected;

		protected void SetMessageSubTypeAsRejected(EDIMessage message) => message.EM_MessageSubType = Messaging.DeclarationMessageSubTypeList.Codes.RejectedResponse;

		protected void SetCHStatusAsFailed(T businessObject)
		{
			if (businessObject is IESResponseBOMessageStatus boWithMessageStatus)
			{
				boWithMessageStatus.MessageStatus = EDIMessageStatusList.Codes.Failed;
				SetExtraCHStatus(businessObject, EDIMessageStatusList.Codes.Failed);
			}
		}

		protected void SetCHStatusAsReceived(T businessObject, string messageStatus = EDIMessageStatusList.Codes.Received)
		{
			if (businessObject is IESResponseBOMessageStatus boWithMessageStatus)
			{
				boWithMessageStatus.MessageStatus = messageStatus;
				SetExtraCHStatus(businessObject, messageStatus);
			}
		}

		protected void SetCHStatusAsRejected(T businessObject, string messageStatus = EDIMessageStatusList.Codes.Rejected)
		{
			if (businessObject is IESResponseBOMessageStatus boWithMessageStatus)
			{
				boWithMessageStatus.MessageStatus = messageStatus;
				SetExtraCHStatus(businessObject, messageStatus);
			}
		}

		protected virtual void SetExtraCHStatus(T businessObject, ZString messageStatus) { }

		ZGuid GetRelevantBOBranch(T businessObject) => ((IESResponseBusinessObject)businessObject).BranchPK;

		EDIMessageCollection GetRelevantBOMessages(T businessObject) => ((IESResponseBusinessObject)businessObject).MessageCollection;

		protected void SetCSVClearanceAndTriggerDocumentRequest(T businessObject, EDIMessage message, ZString newCSVClearance, ZString mrn)
		{
			var oldCSVClearance = GetOldCSVClearance(businessObject);

			if ((!oldCSVClearance.IsEmpty && !newCSVClearance.IsEmpty && oldCSVClearance != newCSVClearance)
				|| ForceChangeExistingEdocsFileNames(message))
			{
				var fileNamesDict = FileNamesDict(mrn, oldCSVClearance, message);
				EDocHelper.ChangeExistingEDocsFileNames(((IDocManagerSupport)businessObject), fileNamesDict, EDocsSaver);
			}

			if (!newCSVClearance.IsEmpty)
			{
				SetNewCSVClearance(businessObject, newCSVClearance);

				TriggerMisingDocumentRequest(businessObject, message);
			}
		}

		protected void TriggerMisingDocumentRequest(T businessObject, EDIMessage message)
		{
			var certificateName = MessageProcessorHelper.GetInterchangeCertificateName(message, Logger);
			var docRequest = GetNewDocumentRequest(businessObject, certificateName, message);
			docRequest.RequestMissingDocuments();
		}

		protected void TriggerInboxRequest(T businessObject, EDIMessage message, ZString[] messageTypesList)
		{
			var mrn = ZString.Empty;
			var boReference = ZString.Empty;
			if (businessObject is IESMessageInfoProvider messageInfoProvider)
			{
				mrn = messageInfoProvider.MRN;
				boReference = messageInfoProvider.EntryReference;
			}

			var certificateName = MessageProcessorHelper.GetInterchangeCertificateName(message, Logger);
			foreach (var messageType in messageTypesList)
			{
				MessageRequest.CreateEDIMessageForInboxRequest(message.Factory, businessObject as IPollingTransactionParent, businessObject.PK, businessObject.TablePrefix, mrn, boReference, messageType, certificateName);
			}
		}

		protected void RemoveCusPollingTransactionsIfNeeded(BusinessObjectFactory factory, IReadOnlyList<ZString> messageTypesList, ZString mrnCode)
		{
			if (IsInboxDeclaration && IsDirectXTInterface())
			{
				var query = new ZQuery(CusPollingTransactionSchema.CPT_ApplicationCode, Enterprise.Messaging.Integration.ApplicationCodeList.Codes.ESCustomsMessage);
				query.AddToFilter(CusPollingTransactionSchema.CPT_Type, (from ZString messageType in messageTypesList select messageType));
				query.AddToFilter(CusPollingTransactionSchema.CPT_TransactionID, mrnCode);
				var transactionsToRemove = factory.Load<CusPollingTransaction>(query);

				foreach (var transaction in transactionsToRemove)
				{
					transaction.Delete();
				}
			}

			bool IsDirectXTInterface() => Registry.ESCustomsDataRegistry.Instance.EnableESInboxMessagesThroughDirectxTInterface.Value;
		}

		protected ZString GetCircuitCodeFromText(string circuitCode)
		{
			return circuitCode switch
			{
				MessageFunctionCodeList.Codes.GreenCircuitText => CircuitCodeList.Codes.GREEN,
				MessageFunctionCodeList.Codes.RedCircuitText => CircuitCodeList.Codes.RED,
				MessageFunctionCodeList.Codes.OrangeCircuitText => CircuitCodeList.Codes.ORANGE,
				_ => ZString.Empty,
			};
		}

		protected virtual ZString GetOldCSVClearance(T businessObject) => ZString.Empty;
		protected virtual void SetNewCSVClearance(T businessObject, ZString newCSVClearance) { }

		protected virtual ZBool ForceChangeExistingEdocsFileNames(EDIMessage message) => false;

		protected virtual Dictionary<string, string> FileNamesDict(string mrn, string oldCSVClearance, EDIMessage message) => new Dictionary<string, string>() { };

		protected void TriggerMisingDocumentRequestForEmails(T businessObject, EDIMessage message)
		{
			var certificateName = GetCertificateNameFromPreviousAcceptedResponse(businessObject, Logger);
			if (!certificateName.IsEmpty)
			{
				var docRequest = GetNewDocumentRequest(businessObject, certificateName, message);
				docRequest.RequestMissingDocuments();
			}
		}

		protected virtual CommonDocumentRequest<T> GetNewDocumentRequest(T businessObject, ZString certName, EDIMessage message) => null;

		ZString GetCertificateNameFromPreviousAcceptedResponse(T businessObject, LoggingInformation logger)
		{
			var returnText = ZString.Empty;

			var message = GetRelevantBOMessages(businessObject).Cast<EDIMessage>().Where(x => x.EM_ReceiveTransmit == EDIInterchange.Direction.Receive
																		&& !x.EM_EI.IsEmpty
																		&& x.EM_Status == EDIMessageStatusList.Codes.Received
																		&& x.EM_MessageSubType == Messaging.DeclarationMessageSubTypeList.Codes.AcceptedResponse).OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();

			if (message != null)
			{
				returnText = MessageProcessorHelper.GetInterchangeCertificateName(message, logger);
			}

			return returnText;
		}

		EDIMessage GetSentMessageForAnnexAndAddAnnexGenPivotIfNeeded(EDIMessage message, BusinessObjectFactory factory)
		{
			EDIMessage sentMessage = MessageProcessorHelper.GetOutgoingMessage(message);
			if (sentMessage != null)
			{
				var docPivotList = GetSentAnnexGenPivot(sentMessage, factory);
				if (docPivotList != null)
				{
					foreach (var docPivot in docPivotList)
					{
						var newDocPivot = factory.New<GenPivot>();
						newDocPivot.XX_RelationType = GenPivotTypes.CusStorageDocPivotEdiMessage;
						newDocPivot.XX_Relation1ID = docPivot.XX_Relation1ID;
						newDocPivot.XX_Relation2ID = message.PK;
						newDocPivot.XX_Relation1TableCode = CusStorageDocPivotSchema.Constants.Prefix;
						newDocPivot.XX_Relation2TableCode = EDIMessageSchema.Constants.Prefix;
					}
				}
			}
			return sentMessage;
		}

		GenPivot[] GetSentAnnexGenPivot(EDIMessage sentMessage, BusinessObjectFactory factory)
		{
			var pivotQuery = new ZQuery(GenPivotSchema.XX_RelationType, GenPivotTypes.CusStorageDocPivotEdiMessage);
			pivotQuery.AddToFilter(GenPivotSchema.XX_Relation2ID, sentMessage.PK);
			pivotQuery.AddToFilter(GenPivotSchema.XX_Relation1TableCode, CusStorageDocPivotSchema.Constants.Prefix);
			pivotQuery.AddToFilter(GenPivotSchema.XX_Relation2TableCode, EDIMessageSchema.Constants.Prefix);
			return factory.Load<GenPivot>(pivotQuery);
		}

		protected void SetSentMessageStatusAsReceived(EDIMessage message, BusinessObjectFactory factory)
		{
			var sentMessage = GetSentMessageForAnnexAndAddAnnexGenPivotIfNeeded(message, factory);
			if (sentMessage != null)
			{
				SetMessageStatusAsReceived(sentMessage);
			}
		}

		protected void SetSentMessageStatusAsRejected(EDIMessage message, BusinessObjectFactory factory)
		{
			var sentMessage = GetSentMessageForAnnexAndAddAnnexGenPivotIfNeeded(message, factory);
			if (sentMessage != null)
			{
				SetMessageStatusAsRejected(sentMessage);
			}
		}

		protected void ProcessRejectedDeclarationForAnnexes(EDIMessage message, T bo)
		{
			SetSentMessageStatusAsRejected(message, bo.Factory);
			SetMessageStatusAsRejected(message);
			SetCHStatusAsRejected(bo);
		}

		protected ZBool TriggerMessageSendingCommon(EDIMessage message, ZString sentBrokerCode, GlbStaff broker, ZBool shouldTriggerAnnexes, T bo, Func<T, CertificateObject, List<ESEDIMessage>> sendDeclaration, string messageStatus = MessageStatusList.Codes.AwaitingResponse)
		{
			var sentMessagesCorrectly = false;

			if (shouldTriggerAnnexes)
			{
				var certificateObject = GetCertificateObject(message, sentBrokerCode, broker);

				if (certificateObject != null)
				{
					sentMessagesCorrectly = SendAndProcessMessages(bo, sendDeclaration, certificateObject, messageStatus);
				}
			}

			return sentMessagesCorrectly;
		}

		protected CertificateObject GetCertificateObject(EDIMessage message, ZString sentBrokerCode, GlbStaff broker)
		{
			var receivedInterchange = message.Interchange;
			var certificateName = ZString.Empty;
			var certificateThumbPrint = ZString.Empty;

			if (receivedInterchange.EI_TransportType == EDIInterchange.TransportType.xT)
			{
				certificateName = MessageProcessorHelper.GetOutgoingMessage(message).EM_ApplicationReference;
			}
			else
			{
				try
				{
					using var textReader = receivedInterchange.GetEI_HeaderTextReader();
					var interchangeHeader = CargoWise.Customs.Shared.MessageContracts.XmlObjectSerializer.DeserializeWithXSDValidation<Headers>(HeadersXsdSchemaEmbeddedResourceName, textReader);
					certificateName = interchangeHeader.CertificateName;
					sentBrokerCode = interchangeHeader.BrokerCode;
					certificateThumbPrint = interchangeHeader.CertificateThumbPrint;
				}
				catch (Exception ex) when (ex is XmlSchemaValidationException || ex is InvalidOperationException || ex is NullReferenceException)
				{
					Logger.Log(string.Format(CultureInfo.CurrentCulture, "Error reading xml {0}", ex.ToString()), Integration.LogType.Error);
				}
			}

			if (broker != null && broker.GS_Code == sentBrokerCode)
			{
				return new CertificateObject(broker, certificateName, certificateThumbPrint);
			}

			return null;
		}

		protected bool SendAndProcessMessages(T bo, Func<T, CertificateObject, List<ESEDIMessage>> sendDeclaration, CertificateObject certificateObject, string messageStatus)
		{
			try
			{
				var messages = sendDeclaration(bo, certificateObject);

				if (messages.Count > 0)
				{
					((IESResponseBOMessageStatus)bo).MessageStatus = messageStatus;
					return true;
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Logger.LogWarning(Res.GetString("7983BD16-51C2-4F5C-B319-990D949D1B6A", "Error when creating the Annexes messages"));
			}

			return false;
		}

		protected ZDateTime SetEntryIssueDate(ZDateTime currentIssueDate, ZDateTime issueDate) => (IsQueryMessage && !currentIssueDate.Date.Equals(issueDate.Date) || !IsQueryMessage) ? issueDate : currentIssueDate;

		protected virtual bool ShouldSetMovementReferenceNumber(EDIMessage message) => true;

		protected const string HeadersXsdSchemaEmbeddedResourceName = "CargoWise.Customs.ES.MessageDefinitions.Interchange.Headers.xsd";

		protected void UpdateEntryInstructionSubStyleFromBToX(CusEntryHeader entryHeader)
		{
			if (entryHeader.EntryInstruction is not null && entryHeader.EntryInstruction.CEI_SubStyle == EntrySubStyleList.Codes.B)
			{
				entryHeader.EntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.X;
			}
		}

		protected void UpdateEntryInstructionSubStyleFromCToY(CusEntryHeader entryHeader, EDIMessage message)
		{
			var outgoingMessageSubType = MessageProcessorHelper.GetOutgoingMessage(message)?.EM_MessageSubType ?? ZString.Empty;
			if (outgoingMessageSubType == DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration
				&& entryHeader.EntryInstruction is not null
				&& entryHeader.EntryInstruction.CEI_SubStyle == EntrySubStyleList.Codes.C)
			{
				entryHeader.EntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Y;
			}
		}

		protected void UpdateEntryInstructionSubStyleToXOrY(CusEntryHeader entryHeader, string responseSubStyle)
		{
			if (entryHeader.EntryInstruction is not null && (responseSubStyle == EntrySubStyleList.Codes.X || responseSubStyle == EntrySubStyleList.Codes.Y))
			{
				entryHeader.EntryInstruction.CEI_SubStyle = responseSubStyle;
			}
		}
	}
}
