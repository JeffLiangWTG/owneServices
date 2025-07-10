using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business.EDIMessages;
using Enterprise.Customs.ES.Business.MessageSending;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.ES.NCTS.Business.ESNctsMessageSender;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class ESNctsMessageSender : GenericMessageSender<MessageBuilderData, NctsHeaderMessageSendingObject>
	{
		public ESNctsMessageSender(NctsHeaderMessageSendingObjectParent sendingObjectParent)
		{
			this.sendingObjectParent = Argument.NotNull(sendingObjectParent, nameof(sendingObjectParent));
		}
		readonly NctsHeaderMessageSendingObjectParent sendingObjectParent;

		public override List<MessageBuilderData> GetMessageBuildersData()
		{
			var messageBuilders = new List<MessageBuilderData>();
			var certificateData = sendingObjectParent.CertificateData;
			foreach (var objectToSend in sendingObjectParent.SelectedSendingObjects.Cast<NctsHeaderMessageSendingObject>())
			{
				messageBuilders.AddRange(GetIndividualMessageBuilder(objectToSend, certificateData));
			}

			return messageBuilders;
		}

		protected override List<MessageBuilderData> GetIndividualMessageBuilder(NctsHeaderMessageSendingObject objectToSend, ICertificateProvider certificateData)
		{
			Argument.NotNull(objectToSend, "objectToSend cannot be null");

			var messageBuilders = new List<MessageBuilderData>();
			AddSpecificMessageBuilders(messageBuilders, objectToSend, certificateData);

			return messageBuilders;
		}

		void AddSpecificMessageBuilders(List<MessageBuilderData> messageBuilders, NctsHeaderMessageSendingObject objectToSend, ICertificateProvider certificateData)
		{
			try
			{
				var builderManager = new ESNctsMessageBuilderManager(objectToSend, certificateData);

				var messageType = objectToSend.MessageType;
				var nctsHeader = objectToSend.NctsHeader;

				if (messageType == DeclarationMessageTypeList.Codes.Ncts5DepartureAnnexes)
				{
					messageBuilders.AddRange(GetNCTSAnnexesMessageBuilders(nctsHeader, builderManager, objectToSend.RequestDispatch));
				}
				else
				{
					messageBuilders.Add(new MessageBuilderData
					{
						MessageBuilder = builderManager.NewMessageBuilder(),
						NctsHeader = nctsHeader
					});
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("ESNctsMessageSender.GetIndividualMessageBuilder", "Exception thrown when trying to create builder to send declaration", ex);
				messageBuilders.Add(new MessageBuilderData
				{
					FailureFlag = true
				});
			}
		}

		public static List<MessageBuilderData> GetNCTSAnnexesMessageBuilders(NctsHeader header, ESNctsMessageBuilderManager builderManager, ZString requestDispatch)
		{
			var messageBuilders = new List<MessageBuilderData>();

			var eDocPivotList = header.GetAllSendableEDocPivots();

			if (eDocPivotList.Skip(9).Any())
			{
				var msgToBeSentLater = eDocPivotList.FirstOrDefault(p => p.MessageStatus == ZString.Empty);
				eDocPivotList.Remove(msgToBeSentLater);

				var chunkedEDocPivotList = ChunkPivotListForNCTSAnnex(eDocPivotList);

				foreach (var edocPivotChunk in chunkedEDocPivotList)
				{
					var messageBuilder = GetNewMessageBuilderDataForNCTSAnnex(header, builderManager, edocPivotChunk, Customs.Business.YesNoList.Codes.No, requestDispatch);
					if (messageBuilder != null)
					{
						messageBuilders.Add(messageBuilder);
					}
				}
			}
			else if (requestDispatch == Customs.Business.YesNoList.Codes.No)
			{
				var msgToBeSentLater = eDocPivotList.FirstOrDefault(p => p.MessageStatus == ZString.Empty);
				eDocPivotList.Remove(msgToBeSentLater);

				var messageBuilder = GetNewMessageBuilderDataForNCTSAnnex(header, builderManager, eDocPivotList, requestDispatch, requestDispatch);
				if (messageBuilder != null)
				{
					messageBuilders.Add(messageBuilder);
				}
			}
			else if (requestDispatch == Customs.Business.YesNoList.Codes.Yes)
			{
				var messageBuilder = GetNewMessageBuilderDataForNCTSAnnex(header, builderManager, eDocPivotList, requestDispatch, requestDispatch);
				if (messageBuilder != null)
				{
					messageBuilders.Add(messageBuilder);
				}
			}

			return messageBuilders;
		}

		static IMessageBuilderBase GetIndividualNCTSAnnexMessageBuilder(ESNctsMessageBuilderManager builderManager, IEnumerable<NctsCusStorageDocPivot> edocPivotList, ZString requestDispatch)
		{
			return builderManager.NewNCTSAnnexMessageBuilder(edocPivotList, requestDispatch);
		}

		static IEnumerable<IEnumerable<NctsCusStorageDocPivot>> ChunkPivotListForNCTSAnnex(IEnumerable<NctsCusStorageDocPivot> pivotList)
		{
			var chunkSize = 9;
			while (pivotList.Any())
			{
				yield return pivotList.Take(chunkSize);
				pivotList = pivotList.Skip(chunkSize);
			}
		}

		static MessageBuilderData GetNewMessageBuilderDataForNCTSAnnex(NctsHeader header, ESNctsMessageBuilderManager builderManager, IEnumerable<NctsCusStorageDocPivot> eDocPivotList, ZString requestDispatchToSend, ZString requestDispatch)
		{
			return eDocPivotList.IsNullOrEmpty() ? null : new MessageBuilderData
			{
				MessageBuilder = GetIndividualNCTSAnnexMessageBuilder(builderManager, eDocPivotList, requestDispatchToSend),
				NctsHeader = header,
				EDocPivotList = eDocPivotList,
				RequestDispatch = requestDispatch
			};
		}

		public static List<MessageBuilderData> GetQueryMessageBuilders(NctsHeader nctsHeader, ESNctsMessageBuilderManager builderManager)
		{
			var messageBuilders = new List<MessageBuilderData>();

			messageBuilders.Add(new MessageBuilderData
			{
				MessageBuilder = builderManager.NewMessageBuilder(),
				NctsHeader = nctsHeader
			});

			return messageBuilders;
		}

		public static MessagesInfo Send(List<MessageBuilderData> messageBuildersToSend, List<ESEDIMessage> messages = null)
		{
			var messagesInfo = new MessagesInfo
			{
				MessagesSent = ZInt.Zero,
				MessagesWithCreateFailure = ZInt.Zero,
				MessagesWithSendFailure = ZInt.Zero
			};

			foreach (var objectToSend in messageBuildersToSend)
			{
				SendIndividualDeclaration(objectToSend, messagesInfo, messages);
			}

			return messagesInfo;
		}

		static void SendIndividualDeclaration(MessageBuilderData messageBuilderToSend, MessagesInfo messagesInfo, List<ESEDIMessage> messages = null)
		{
			ESEDIMessage message = null;

			if (!messageBuilderToSend.FailureFlag)
			{
				var initialMessagesSent = messagesInfo.MessagesSent;
				var nctsHeader = messageBuilderToSend.NctsHeader;
				var previousMessageStatus = nctsHeader?.EffectiveMessageStatus ?? ZString.Empty;

				try
				{
					var messageBuilder = Argument.NotNull(messageBuilderToSend.MessageBuilder, "messageBuilderToSend cannot be null");

					message = TrySendDeclaration(messageBuilderToSend);
					messagesInfo.MessagesSent++;

					if (messages != null)
					{
						messages.Add(message);
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					messagesInfo.MessagesWithSendFailure++;
					ErrorReporter.ReportOnce("ESNctsMessageSender.Send", "Exception thrown when trying to send declaration", ex);
				}
				finally
				{
					CleanUpDatabaseAfterFailure(message, nctsHeader, previousMessageStatus, initialMessagesSent, messagesInfo);
				}
			}
			else
			{
				messagesInfo.MessagesWithSendFailure++;
			}
		}

		static ESEDIMessage TrySendDeclaration(MessageBuilderData objectToSend)
		{
			var messageBuilderToSend = Argument.NotNull(objectToSend.MessageBuilder, "messageBuilderToSend cannot be null");
			var nctsHeader = Argument.NotNull(objectToSend.NctsHeader, "entryHeader cannot be null");
			var eDocPivotList = objectToSend.EDocPivotList;

			var messageType = messageBuilderToSend.MessageType;

			SetValuationDate(nctsHeader, messageType);

			var messageCreated = SendMessageBuilder(nctsHeader, messageBuilderToSend, eDocPivotList);

			SetMessageStatus(nctsHeader, messageType);

			nctsHeader.RequestDispatch = objectToSend.RequestDispatch;

			if (messageType == DeclarationMessageTypeList.Codes.TransitNcts5Query && ((nctsHeader.MovementHeader?.BM_CustomsStatus ?? ZString.Empty) == ESNCTS5DepartureCustomsStatusList.Codes.PreLodged))
			{
				nctsHeader.UpdatePreDeclaration = true;
			}

			SetPhaseStatus(nctsHeader, messageType);

			return messageCreated;
		}

		static ESEDIMessage SendMessageBuilder(NctsHeader header, IMessageBuilderBase messageBuilder, IEnumerable<NctsCusStorageDocPivot> eDocPivotList)
		{
			var factory = header.Factory;

			var messageCreator = new EDIMessageCreator(messageBuilder, factory);
			var message = messageCreator.CreateMessage();

			var messageTypeToAddPermitRecords = new List<ZString>()
												{
												DeclarationMessageTypeList.Codes.Ncts5Departure,
												DeclarationMessageTypeList.Codes.Ncts5DepartureNotification
												};

			if (header.BH_HeaderType == NctsMovementType.Codes.Departure
				&& (header.IsPhase4
					|| (header.IsPhase5 && messageTypeToAddPermitRecords.Contains(messageBuilder.MessageType))))
			{
				EU.NCTS.Business.MessageGeneration.NctsMessageGeneratorHelper.AddPermitRecords(header, message);
			}

			message.EM_LinkedObject = header;

			var messageType = messageBuilder.MessageType;

			if (messageType == DeclarationMessageTypeList.Codes.Ncts5DepartureAnnexes && eDocPivotList != null)
			{
				foreach (var eDoc in eDocPivotList)
				{
					CreateNewGenPivot(factory, message, eDoc);
				}
			}

			return message;
		}

		static void CreateNewGenPivot(BusinessObjectFactory factory, ESEDIMessage message, NctsCusStorageDocPivot eDoc)
		{
			var newDocPivot = factory.New<GenPivot>();
			newDocPivot.XX_RelationType = GenPivotTypes.CusStorageDocPivotEdiMessage;
			newDocPivot.XX_Relation1ID = eDoc.PK;
			newDocPivot.XX_Relation2ID = message.PK;
			newDocPivot.XX_Relation1TableCode = CusStorageDocPivotSchema.Constants.Prefix;
			newDocPivot.XX_Relation2TableCode = EDIMessageSchema.Constants.Prefix;
		}

		static void CleanUpDatabaseAfterFailure(ESEDIMessage message, NctsHeader header, ZString previousMessageStatus, ZInt initialMessagesSent, MessagesInfo messagesInfo)
		{
			if (messagesInfo.MessagesSent == initialMessagesSent && message == null && header != null)
			{
				header.EffectiveMessageStatus = previousMessageStatus;
			}
		}

		static void SetMessageStatus(NctsHeader nctsHeader, ZString messageType)
		{
			nctsHeader.EffectiveMessageStatus =
							nctsHeader.IsPhase5
									? LogicalStatusList.Codes.Sent
									: nctsHeader.BH_HeaderType == NctsMovementType.Codes.Departure
											? NctsMessageStatusList.Codes.DepartureDeclarationSent
											: messageType == DeclarationMessageTypeList.Codes.NctsUnloadingRemarks
												? NctsMessageStatusList.Codes.UnloadingRemarksSent
												: NctsMessageStatusList.Codes.ArrivalNotificationSent;

			if (messageType == DeclarationMessageTypeList.Codes.Ncts5IndirectDepartureRegistration && nctsHeader.IsArrivalMovement)
			{
				var departureHeaderTNN = nctsHeader.ArrivalMovementHeader.HeaderTNN;
				if (departureHeaderTNN != null)
				{
					departureHeaderTNN.MovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				}
			}
		}

		static void SetPhaseStatus(NctsHeader nctsHeader, ZString messageType)
		{
			var phaseStatusToAdd = ZString.Empty;
			switch (messageType)
			{
				case DeclarationMessageTypeList.Codes.Ncts5DepartureAnnexes:
					phaseStatusToAdd = ESNctsMovementHeaderTransactionStatusList.Codes.Annexes;
					break;
				case DeclarationMessageTypeList.Codes.Ncts5DepartureNotification:
					phaseStatusToAdd = ESNctsMovementHeaderTransactionStatusList.Codes.Presentation;
					break;
				case DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment:
					phaseStatusToAdd = ESNctsMovementHeaderTransactionStatusList.Codes.Amendment;
					break;
				case DeclarationMessageTypeList.Codes.Ncts5DepartureCancellation:
					phaseStatusToAdd = ESNctsMovementHeaderTransactionStatusList.Codes.Cancellation;
					break;
				case DeclarationMessageTypeList.Codes.Ncts5Departure:
				case DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration:
					phaseStatusToAdd = ESNctsMovementHeaderTransactionStatusList.Codes.Declaration;
					break;
				case DeclarationMessageTypeList.Codes.Ncts5ArrivalNotification:
					phaseStatusToAdd = ESNctsMovementHeaderTransactionStatusList.Codes.Arrival;
					break;
				case DeclarationMessageTypeList.Codes.Ncts5IndirectDepartureRegistration:
					phaseStatusToAdd = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;
					break;
				case DeclarationMessageTypeList.Codes.Ncts5ArrivalDownloadGoods:
					phaseStatusToAdd = ESNctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks;
					break;
			}

			if (!phaseStatusToAdd.IsEmpty)
			{
				if (nctsHeader.MovementHeader != null)
				{
					nctsHeader.MovementHeader.BM_Phase = phaseStatusToAdd;
				}
				else if (nctsHeader.ArrivalMovementHeader != null)
				{
					nctsHeader.ArrivalMovementHeader.BM_Phase = phaseStatusToAdd;
				}
			}
		}

		static void SetValuationDate(NctsHeader nctsHeader, ZString messageType)
		{
			if (messageType == DeclarationMessageTypeList.Codes.Ncts5Departure || messageType == DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration)
			{
				NctsMovementHeaderValuationDateHelper.SetValuationDate(nctsHeader.MovementHeader, false);
			}
			else if (messageType == DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment)
			{
				NctsMovementHeaderValuationDateHelper.SetValuationDate(nctsHeader.MovementHeader, true);
			}
		}

		public class MessageBuilderData
		{
			public IMessageBuilderBase MessageBuilder;
			public NctsHeader NctsHeader;
			public IEnumerable<NctsCusStorageDocPivot> EDocPivotList;
			public ZBool FailureFlag;
			public ZString RequestDispatch;
		}

		public class MessagesInfo
		{
			public ZInt MessagesSent;
			public ZInt MessagesWithCreateFailure;
			public ZInt MessagesWithSendFailure;
		}
	}
}
