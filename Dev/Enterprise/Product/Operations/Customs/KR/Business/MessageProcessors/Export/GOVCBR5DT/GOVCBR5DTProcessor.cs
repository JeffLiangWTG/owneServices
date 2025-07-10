using System.Linq;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5AS;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._5DT)]
	public class GOVCBR5DTProcessor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var outgoingMessageTypes = ElectronicDocumentTypeList.GetOutgoingMessagesToReceiveCustomsReviewMessage(ElectronicDocumentTypeList.Codes._5DT);
				var messageData = new GOVCBR5DTDataProvider().GetMessageData(textReader);
				var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, messageData.ExportDeclarationNumber, SharedJobMessageTypeList.Codes.Export);
				EDIMessage originalMessage = null;
				if (entry != null)
				{
					message.EM_LinkedObject = entry;

					var amendSequence = messageData.AmendSequence + 1;
					originalMessage = entry.Messages.Cast<EDIMessage>().Where(x => outgoingMessageTypes.Contains(x.EM_MessageType) && x.EM_ApplicationReference == amendSequence.ToString()).OrderBy(x => x.EM_SystemCreateTimeUtc).LastOrDefault();
					message.EM_ApplicationReference = originalMessage?.EM_MessageNum ?? ZString.Empty;

					if (messageData.NoticeType == ExportNotificationTypeList.Codes._05)
					{
						if (originalMessage != null)
						{
							if (originalMessage.EM_MessageType == ElectronicDocumentTypeList.Codes._DKJ)
							{
								entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.CCL;
								entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationApprovedByCustoms;
							}
							else
							{
								if (originalMessage.EM_MessageSubType == _5ASAmendmentType.Codes.Extension)
								{
									using (var outgoingTextReader = originalMessage.GetEM_MessageTextReader())
									{
										var declaration = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<Declaration>(outgoingTextReader);
										ZString amendmentItem = declaration.Consignment.FirstOrDefault(x => x.Amendment.Pointer.TagId.Value == ExportAmendmentDataItemIDList.Codes.A608)?.Amendment?.AdjustmentDescription?.Value;
										var newExpiryDate = amendmentItem.SubstringSafe(0, 4) + "-" +
															amendmentItem.SubstringSafe(4, 2) + "-" +
															amendmentItem.SubstringSafe(6, 2);

										if (newExpiryDate.Length == 10)
										{
											var exportEntryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(SharedJobMessageTypeList.Codes.Export);
											exportEntryNum.CE_ExpiryDate = new ZDateTime(newExpiryDate);
										}
									}
								}
								entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.ANT;
							}
						}
					}
					else if (messageData.NoticeType == ExportNotificationTypeList.Codes._07)
					{
						entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.CGD;
					}
					else if (messageData.NoticeType == ExportNotificationTypeList.Codes._09)
					{
						entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.DMS;
						if (originalMessage != null)
						{
							if (originalMessage.EM_MessageType == ElectronicDocumentTypeList.Codes._DKJ)
							{
								entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationDeclined;
							}
							else
							{
								entry.MarkLodgedSnapshotAsDeleted(ElectronicDocumentTypeList.Codes._830);
							}
						}
					}

					message.EM_MessageOwner = entry.CH_EntryStatus;
				}
				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(messageData, message);
				SendNotification(message, entry, messageData, originalMessage == null ? outgoingMessageTypes : new ZString[] { originalMessage.EM_MessageType });
			}
		}

		#region SuppressResourceStringsCheckRegion

		void SendNotification(EDIMessage message, CusEntryHeader entry, IGOVCBR5DTMessageData messageData, ZString[] originalMessageTypes)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ExportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(messageData, message),
				OriginalMessageTypes = originalMessageTypes,
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._5DT}]",
				EntryNumber = messageData.ExportDeclarationNumber
			};
			NotificationSender.SendNotification(notificationData);
		}

		string CreateUserFriendlyMessageInterpretation(IGOVCBR5DTMessageData messageData, EDIMessage message)
		{
			var result = new ZStringBuilder();
			var customsofficeAnddivision = MessageFunctions.GetCustomsOfficeAndDivision(message.Factory, messageData.CustomsOfficeAndDivision);
			var tableContents = new HtmlTableCreator(new string[] { "항 목", "내 용" });
			tableContents.WriteRow(new string[] { "제출문서", "수출신고수리 정정/취하 신청서" });
			tableContents.WriteRow(new string[] { "통보일시", messageData.NoticeDateTime.ToString(DateFormatType.DateTimeKorean) });
			tableContents.WriteRow(new string[] { "수출신고번호", MessageFunctions.DeclarationNumberFormat(messageData.ExportDeclarationNumber) });
			tableContents.WriteRow(new string[] { "결정일자", messageData.DecisionDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "통보구분", new ExportNotificationTypeList().GetDescriptionFromCode(messageData.NoticeType) });
			tableContents.WriteRow(new string[] { "귀책사유", new ExportImputationReasonCodeList().GetDescriptionFromCode(messageData.FaultParty) });
			tableContents.WriteRow(new string[] { "귀책사유부호 변경사유", messageData.FaultPartyChangeReason });
			tableContents.WriteRow(new string[] { "통보내용", messageData.NoticeDescription });
			tableContents.WriteRow(new string[] { "결정 승인번호", messageData.ApprovalNo });
			tableContents.WriteRow(new string[] { "세관 담당자", messageData.CustomsPersonID + " " + messageData.CustomsPersonName });
			tableContents.WriteRow(new string[] { "세관(과)", "[" + messageData.CustomsOfficeAndDivision + "]" + customsofficeAnddivision });
			result.Append(tableContents.ToHtml());

			return result.ToString();
		}
		#endregion
	}
}
