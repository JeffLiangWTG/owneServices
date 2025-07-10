using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.EDIMessages;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.ES.Business.MessageSending.ESMessageSender;

namespace Enterprise.Customs.ES.Business.MessageSending
{
	public class ESMessageSender : GenericMessageSender<MessageBuilderData, JobDeclarationMessageSendingObject>
	{
		public ESMessageSender(JobDeclarationMessageSendingObjectParent sendingObjectParent)
		{
			this.sendingObjectParent = Argument.NotNull(sendingObjectParent, nameof(sendingObjectParent));
		}
		readonly JobDeclarationMessageSendingObjectParent sendingObjectParent;

		public override List<MessageBuilderData> GetMessageBuildersData()
		{
			var messageBuilders = new List<MessageBuilderData>();
			var certificateData = sendingObjectParent.CertificateData;
			foreach (var objectToSend in sendingObjectParent.SelectedSendingObjects
				.Cast<JobDeclarationMessageSendingObject>())
			{
				messageBuilders.AddRange(GetIndividualMessageBuilder(objectToSend, certificateData));
			}

			return messageBuilders;
		}

		protected override List<MessageBuilderData> GetIndividualMessageBuilder(JobDeclarationMessageSendingObject objectToSend, ICertificateProvider certificateData)
		{
			Argument.NotNull(objectToSend, "objectToSend cannot be null");

			var messageBuilders = new List<MessageBuilderData>();

			AddSpecificMessageBuilders(messageBuilders, objectToSend, certificateData);

			return messageBuilders;
		}

		void AddSpecificMessageBuilders(List<MessageBuilderData> messageBuilders, JobDeclarationMessageSendingObject objectToSend, ICertificateProvider certificateData)
		{
			try
			{
				var builderManager = new ESMessageBuilderManager(objectToSend, certificateData);

				var messageType = objectToSend.MessageType;
				var entryHeader = objectToSend.Header;

				if (messageType == DeclarationMessageTypeList.Codes.T2lAnnex)
				{
					messageBuilders.AddRange(GetT2LAnnexesMessageBuilders(entryHeader, builderManager));
				}
				else if (messageType == DeclarationMessageTypeList.Codes.ExportAnnexes)
				{
					messageBuilders.AddRange(GetAESAnnexesMessageBuilders(entryHeader, builderManager, objectToSend.RequestDispatch));
				}
				else if (messageType == DeclarationMessageTypeList.Codes.T2lDocumentationPous)
				{
					messageBuilders.AddRange(GetCommonAnnexMessageBuilders(entryHeader, builderManager, objectToSend.RequestDispatch));
				}
				else if (messageType == DeclarationMessageTypeList.Codes.ImportQuery)
				{
					messageBuilders.AddRange(GetImportQueryMessageBuilders(entryHeader, builderManager));
				}
				else if (messageType == DeclarationMessageTypeList.Codes.DvdH2Query)
				{
					messageBuilders.AddRange(GetDVDQueryMessageBuilders(entryHeader, builderManager));
				}
				else if (messageType == DeclarationMessageTypeList.Codes.ImportH1Query)
				{
					messageBuilders.AddRange(GetImportH1QueryMessageBuilders(entryHeader, builderManager));
				}
				else
				{
					messageBuilders.Add(new MessageBuilderData
					{
						MessageBuilder = GetDeclarationMessageBuilder(builderManager),
						EntryHeader = entryHeader
					});
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("ESMessageSender.GetIndividualMessageBuilder", "Exception thrown when trying to create builder to send declaration", ex);
				messageBuilders.Add(new MessageBuilderData
				{
					FailureFlag = true
				});
			}
		}

		public static List<MessageBuilderData> GetT2LAnnexesMessageBuilders(CusEntryHeader header, ESMessageBuilderManager builderManager)
		{
			var messageBuilders = new List<MessageBuilderData>();

			var eDocPivotList = header.GetAllSendableEDocPivots();

			if (eDocPivotList.Skip(1).Any())
			{
				var msgToBeSentLater = eDocPivotList.FirstOrDefault(p => p.MessageStatus == ZString.Empty);
				eDocPivotList.Remove(msgToBeSentLater);

				foreach (var edoc in eDocPivotList)
				{
					messageBuilders.Add(new MessageBuilderData
					{
						MessageBuilder = GetIndividualT2LAnnexMessageBuilder(builderManager, edoc, false),
						EntryHeader = header,
						EDocPivotList = new List<CusStorageDocPivot>() { edoc }
					});
				}
			}
			else if (eDocPivotList.Any())
			{
				var edoc = eDocPivotList.Single();
				messageBuilders.Add(new MessageBuilderData
				{
					MessageBuilder = GetIndividualT2LAnnexMessageBuilder(builderManager, edoc, true),
					EntryHeader = header,
					EDocPivotList = new List<CusStorageDocPivot>() { edoc }
				});
			}

			return messageBuilders;
		}

		static IMessageBuilderBase GetIndividualT2LAnnexMessageBuilder(ESMessageBuilderManager builderManager, CusStorageDocPivot edoc, ZBool isLast)
		{
			return builderManager.NewT2LAnnexMessageBuilder(edoc, isLast);
		}

		public static List<MessageBuilderData> GetAESAnnexesMessageBuilders(CusEntryHeader header, ESMessageBuilderManager builderManager, ZString requestDispatch)
		{
			var messageBuilders = new List<MessageBuilderData>();

			var eDocPivotList = header.GetAllSendableEDocPivots();

			if (eDocPivotList.Skip(9).Any())
			{
				var msgToBeSentLater = eDocPivotList.FirstOrDefault(p => p.MessageStatus == ZString.Empty);
				eDocPivotList.Remove(msgToBeSentLater);

				var chunkedEDocPivotList = ChunkPivotListForAESAnnex(eDocPivotList);

				foreach (var edocPivotChunk in chunkedEDocPivotList)
				{
					var messageBuilder = GetNewMessageBuilderDataForAESAnnex(header, builderManager, edocPivotChunk, Customs.Business.YesNoList.Codes.No, requestDispatch);
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

				var messageBuilder = GetNewMessageBuilderDataForAESAnnex(header, builderManager, eDocPivotList, requestDispatch, requestDispatch);
				if (messageBuilder != null)
				{
					messageBuilders.Add(messageBuilder);
				}
			}
			else if (requestDispatch == Customs.Business.YesNoList.Codes.Yes)
			{
				var messageBuilder = GetNewMessageBuilderDataForAESAnnex(header, builderManager, eDocPivotList, requestDispatch, requestDispatch);
				if (messageBuilder != null)
				{
					messageBuilders.Add(messageBuilder);
				}
			}

			return messageBuilders;
		}

		static IMessageBuilderBase GetIndividualAESAnnexMessageBuilder(ESMessageBuilderManager builderManager, IEnumerable<CusStorageDocPivot> edocPivotList, ZString requestDispatch)
		{
			return builderManager.NewAESAnnexMessageBuilder(edocPivotList, requestDispatch);
		}

		static IEnumerable<IEnumerable<CusStorageDocPivot>> ChunkPivotListForAESAnnex(IEnumerable<CusStorageDocPivot> pivotList)
		{
			var chunkSize = 9;
			while (pivotList.Any())
			{
				yield return pivotList.Take(chunkSize);
				pivotList = pivotList.Skip(chunkSize);
			}
		}

		static MessageBuilderData GetNewMessageBuilderDataForAESAnnex(CusEntryHeader header, ESMessageBuilderManager builderManager, IEnumerable<CusStorageDocPivot> eDocPivotList, ZString requestDispatchToSend, ZString requestDispatch)
		{
			return eDocPivotList.IsNullOrEmpty() ? null : new MessageBuilderData
			{
				MessageBuilder = GetIndividualAESAnnexMessageBuilder(builderManager, eDocPivotList, requestDispatchToSend),
				EntryHeader = header,
				EDocPivotList = eDocPivotList,
				RequestDispatch = requestDispatch
			};
		}

		public static List<MessageBuilderData> GetCommonAnnexMessageBuilders(CusEntryHeader header, ESMessageBuilderManager builderManager, ZString requestDispatch)
		{
			var messageBuilders = new List<MessageBuilderData>();

			var eDocPivotList = header.GetAllSendableEDocPivots();

			if (eDocPivotList.Skip(1).Any())
			{
				var msgToBeSentLater = eDocPivotList.FirstOrDefault(p => p.MessageStatus == ZString.Empty);
				eDocPivotList.Remove(msgToBeSentLater);

				foreach (var edocPivot in eDocPivotList)
				{
					messageBuilders.Add(new MessageBuilderData
					{
						MessageBuilder = GetIndividualCommonAnnexMessageBuilder(builderManager, edocPivot, Customs.Business.YesNoList.Codes.No),
						EntryHeader = header,
						EDocPivotList = new List<CusStorageDocPivot>() { edocPivot },
						RequestDispatch = requestDispatch
					});
				}
			}
			else if (eDocPivotList.Any())
			{
				if (requestDispatch == Customs.Business.YesNoList.Codes.No)
				{
					var msgToBeSentLater = eDocPivotList.FirstOrDefault(p => p.MessageStatus == ZString.Empty);
					eDocPivotList.Remove(msgToBeSentLater);
				}

				if (!eDocPivotList.IsNullOrEmpty())
				{
					var edocPivot = eDocPivotList.Single();
					messageBuilders.Add(new MessageBuilderData
					{
						MessageBuilder = GetIndividualCommonAnnexMessageBuilder(builderManager, edocPivot, requestDispatch),
						EntryHeader = header,
						EDocPivotList = new List<CusStorageDocPivot>() { edocPivot },
						RequestDispatch = requestDispatch
					});
				}
			}

			return messageBuilders;
		}

		static IMessageBuilderBase GetIndividualCommonAnnexMessageBuilder(ESMessageBuilderManager builderManager, CusStorageDocPivot edocPivot, ZString requestDispatch)
		{
			return builderManager.NewCommonAnnexMessageBuilder(edocPivot, requestDispatch);
		}

		public static List<MessageBuilderData> GetDVDQueryMessageBuilders(CusEntryHeader header, ESMessageBuilderManager builderManager)
		{
			var messageBuilders = new List<MessageBuilderData>();

			messageBuilders.Add(new MessageBuilderData
			{
				MessageBuilder = builderManager.NewDVDQueryBuilder(),
				EntryHeader = header
			});

			if (header.Declaration.IsCustomOfficeCanaryIsland)
			{
				messageBuilders.Add(new MessageBuilderData
				{
					MessageBuilder = builderManager.NewDVDQueryBuilderCanaryIslands(),
					EntryHeader = header
				});
			}

			return messageBuilders;
		}

		public static List<MessageBuilderData> GetImportQueryMessageBuilders(CusEntryHeader header, ESMessageBuilderManager builderManager)
		{
			var messageBuilders = new List<MessageBuilderData>();

			messageBuilders.Add(GetImportQueryMessageBuilderForAEAT(header, builderManager));

			if (header.Declaration.DestinationStateIsCanaryIsland)
			{
				messageBuilders.Add(GetImportQueryMessageBuilderForATC(header, builderManager));
			}

			return messageBuilders;
		}

		public static MessageBuilderData GetImportQueryMessageBuilderForAEAT(CusEntryHeader header, ESMessageBuilderManager builderManager)
		{
			return new MessageBuilderData
			{
				MessageBuilder = builderManager.NewImportQueryBuilder(),
				EntryHeader = header
			};
		}

		public static MessageBuilderData GetImportQueryMessageBuilderForATC(CusEntryHeader header, ESMessageBuilderManager builderManager)
		{
			return new MessageBuilderData
			{
				MessageBuilder = builderManager.NewImportQueryBuilderCanaryIslands(),
				EntryHeader = header
			};
		}

		public static List<MessageBuilderData> GetImportH1QueryMessageBuilders(CusEntryHeader header, ESMessageBuilderManager builderManager)
		{
			var messageBuilders = new List<MessageBuilderData>();

			messageBuilders.Add(new MessageBuilderData
			{
				MessageBuilder = builderManager.NewQueryImportH1Builder(),
				EntryHeader = header
			});

			if (header.Declaration.IsCustomOfficeCanaryIsland)
			{
				messageBuilders.Add(new MessageBuilderData
				{
					MessageBuilder = builderManager.NewQueryImportH1Builder(true),
					EntryHeader = header
				});
			}

			return messageBuilders;
		}

		IMessageBuilderBase GetDeclarationMessageBuilder(ESMessageBuilderManager builderManager)
		{
			return builderManager.NewMessageBuilder();
		}

		public static MessagesInfo Send(List<MessageBuilderData> messageBuildersToSend, List<ESEDIMessage> messages = null, Action<ESEDIMessage> messageDecorator = null)
		{
			var messagesInfo = new MessagesInfo
			{
				MessagesSent = ZInt.Zero,
				MessagesWithCreateFailure = ZInt.Zero,
				MessagesWithSendFailure = ZInt.Zero
			};

			foreach (var objectToSend in messageBuildersToSend)
			{
				SendIndividualDeclaration(objectToSend, messagesInfo, messages, messageDecorator);
			}

			return messagesInfo;
		}

		static void SendIndividualDeclaration(MessageBuilderData objectToSend, MessagesInfo messagesInfo, List<ESEDIMessage> messages = null, Action<ESEDIMessage> messageDecorator = null)
		{
			if (!objectToSend.FailureFlag)
			{
				var entryHeader = objectToSend.EntryHeader;

				var previousCHStatus = entryHeader?.CH_Status ?? ZString.Empty;
				var initialMessagesSent = messagesInfo.MessagesSent;
				try
				{
					var message = TrySendDeclaration(objectToSend);
					messageDecorator?.Invoke(message);
					messagesInfo.MessagesSent++;

					if (messages != null)
					{
						messages.Add(message);
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					messagesInfo.MessagesWithSendFailure++;
					ErrorReporter.ReportOnce("ESMessageSender.SendIndividualDeclaration", "Exception thrown when trying to send declaration", ex);
				}
				finally
				{
					CleanUpDatabaseAfterFailure(entryHeader, previousCHStatus, initialMessagesSent, messagesInfo);
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
			var entryHeader = Argument.NotNull(objectToSend.EntryHeader, "entryHeader cannot be null");
			var eDocPivotList = objectToSend.EDocPivotList;

			var messageType = messageBuilderToSend.MessageType;

			var messageCreated = SendMessageBuilder(entryHeader, messageBuilderToSend, eDocPivotList);

			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingResponse;
			entryHeader.ZG_RequestDispatch = objectToSend.RequestDispatch;

			SetTotalAmountAfterSending(entryHeader, messageType);

			return messageCreated;
		}

		public static ESEDIMessage SendMessageBuilder(CusEntryHeader header, IMessageBuilderBase messageBuilder, IEnumerable<CusStorageDocPivot> eDocPivotList)
		{
			var factory = header.Factory;

			var messageCreator = new EDIMessageCreator(messageBuilder, factory);
			var message = messageCreator.CreateMessage();
			message.EM_LinkedObject = header;

			var messageType = messageBuilder.MessageType;

			if (messageType == DeclarationMessageTypeList.Codes.T2lAnnex && eDocPivotList != null && eDocPivotList.FirstOrDefault() != null)
			{
				CreateNewGenPivot(factory, message, eDocPivotList.FirstOrDefault());
			}

			if (messageType == DeclarationMessageTypeList.Codes.ExportAnnexes && eDocPivotList != null)
			{
				foreach (var eDoc in eDocPivotList)
				{
					CreateNewGenPivot(factory, message, eDoc);
				}
			}

			if (messageType == DeclarationMessageTypeList.Codes.T2lDocumentationPous && eDocPivotList != null && eDocPivotList.FirstOrDefault() != null)
			{
				CreateNewGenPivot(factory, message, eDocPivotList.FirstOrDefault());
			}

			if (messageType == DeclarationMessageTypeList.Codes.T2lReceptionPous)
			{
				var eDocPivotListForReceptionPOUS = header.GetAllSendableEDocPivots();
				foreach (var eDoc in eDocPivotListForReceptionPOUS)
				{
					CreateNewGenPivot(factory, message, eDoc);
				}
			}

			return message;
		}

		static void CreateNewGenPivot(BusinessObjectFactory factory, ESEDIMessage message, CusStorageDocPivot eDoc)
		{
			var newDocPivot = factory.New<GenPivot>();
			newDocPivot.XX_RelationType = GenPivotTypes.CusStorageDocPivotEdiMessage;
			newDocPivot.XX_Relation1ID = eDoc.PK;
			newDocPivot.XX_Relation2ID = message.PK;
			newDocPivot.XX_Relation1TableCode = CusStorageDocPivotSchema.Constants.Prefix;
			newDocPivot.XX_Relation2TableCode = EDIMessageSchema.Constants.Prefix;
		}

		static void SetTotalAmountAfterSending(CusEntryHeader header, ZString messageType)
		{
			if (messageType == DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration || messageType == DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration)
			{
				header.TotalAmount = header.GetTotalAmountToDeclare();
			}
		}

		static void CleanUpDatabaseAfterFailure(CusEntryHeader entryHeader, ZString previousCHStatus, ZInt initialMessagesSent, MessagesInfo messagesInfo)
		{
			if (messagesInfo.MessagesSent == initialMessagesSent && entryHeader != null)
			{
				entryHeader.CH_Status = previousCHStatus;
			}
		}

		public class MessageBuilderData
		{
			public IMessageBuilderBase MessageBuilder;
			public CusEntryHeader EntryHeader;
			public IEnumerable<CusStorageDocPivot> EDocPivotList;
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
