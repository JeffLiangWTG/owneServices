using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._RR3)]
	class GOVCBRRR3Processor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBRRR3DataProvider().GetMessageData(textReader);
				ZString declarationType = messageData.DeclarationType;
				var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, messageData.NoticeNumber, KRJobMessageTypeList.Codes.LocalExport);
				if (entry != null)
				{
					message.EM_LinkedObject = entry;
					ZString[] declarationTypeList = { ElectronicDocumentTypeList.Codes._5DS, ElectronicDocumentTypeList.Codes._5DQ, ElectronicDocumentTypeList.Codes._5DR, ElectronicDocumentTypeList.Codes._5DP };

					using (entry.SuspendCESLog())
					{
						if (declarationTypeList.Contains(declarationType))
						{
							message.EM_MessageSubType = declarationType;
							var originalMessage = entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, declarationType);
							if (messageData.ResultType == LocalExportProcessResultTypeCodeList.Codes.D1)
							{
								if (declarationType == ElectronicDocumentTypeList.Codes._5DS || declarationType == ElectronicDocumentTypeList.Codes._5DR)
								{
									if (originalMessage != null)
									{
										if (originalMessage.EM_MessageSubType == LocalExportAmendmentTypeList.Codes.Amendment)
										{
											entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.ANT;
										}
										else if (originalMessage.EM_MessageSubType == LocalExportAmendmentTypeList.Codes.Cancellation)
										{
											entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.CCL;
											entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationApprovedByCustoms;
										}
									}
								}
								else if (declarationType == ElectronicDocumentTypeList.Codes._5DP || declarationType == ElectronicDocumentTypeList.Codes._5DQ)
								{
									entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.ANT;
								}
							}
							else if (messageData.ResultType == LocalExportProcessResultTypeCodeList.Codes.D3)
							{
								entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.DMS;
								if (declarationType == ElectronicDocumentTypeList.Codes._5DS || declarationType == ElectronicDocumentTypeList.Codes._5DR)
								{
									if (originalMessage != null)
									{
										if (originalMessage.EM_MessageSubType == LocalExportAmendmentTypeList.Codes.Cancellation)
										{
											entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationDeclined;
										}
										else
										{
											entry.MarkLodgedSnapshotAsDeleted(entry.CH_MessageType);
										}
									}
								}
								else if (declarationType == ElectronicDocumentTypeList.Codes._5DP || declarationType == ElectronicDocumentTypeList.Codes._5DQ)
								{
									entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationByCustoms;
								}
							}
							else if (messageData.ResultType == LocalExportProcessResultTypeCodeList.Codes.B3)
							{
								entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.OGJ;
							}
							else if (messageData.ResultType == LocalExportProcessResultTypeCodeList.Codes.D5)
							{
								entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.CNR;
							}
							else if (messageData.ResultType == LocalExportProcessResultTypeCodeList.Codes.E1)
							{
								entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.CCL;
								if (declarationType == ElectronicDocumentTypeList.Codes._5DS || declarationType == ElectronicDocumentTypeList.Codes._5DR)
								{
									if (originalMessage != null)
									{
										if (originalMessage.EM_MessageSubType == LocalExportAmendmentTypeList.Codes.Cancellation)
										{
											entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationDeclined;
										}
										else
										{
											entry.MarkLodgedSnapshotAsDeleted(entry.CH_MessageType);
										}
									}
								}
								else if (declarationType == ElectronicDocumentTypeList.Codes._5DP || declarationType == ElectronicDocumentTypeList.Codes._5DQ)
								{
									entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationByCustoms;
								}
							}
							else if (messageData.ResultType == LocalExportProcessResultTypeCodeList.Codes.E7)
							{
								entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.MFI;
							}

							if (declarationType == ElectronicDocumentTypeList.Codes._5DP || declarationType == ElectronicDocumentTypeList.Codes._5DQ)
							{
								entry.CustomsOfficers.CreateOrUpdate(CustomsOfficerTypeList.Codes.ResponsibleCustomsOfficer, ZString.Empty, messageData.CustomsPersonName, messageData.NoticeDateTime);
							}

							var remarks = messageData.NoticeDateTime.ToString(DateFormatType.DateTime) + "\r\n" + messageData.ContentDescription;

							entry.CH_CustomsMessageRemarks = remarks + (!entry.CH_CustomsMessageRemarks.IsEmpty ? "\r\n\r\n" : "") + entry.CH_CustomsMessageRemarks;
						}
					}
					entry.Logs.AddNew(new EventValue(Events.CustomsEntryStatus, eventTime: messageData.CustomsDateTime.ToOffset(), reference: entry.CH_EntryStatus));

					message.EM_MessageOwner = entry.CH_EntryStatus;
					message.EM_ApplicationReference = entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, declarationType)?.EM_MessageNum ?? ZString.Empty;
				}
				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(message, messageData);
				SendNotification(message, entry, messageData, declarationType);
			}
		}

		#region SuppressResourceStringsCheckRegion
		void SendNotification(EDIMessage message, CusEntryHeader entry, IGOVCBRRR3MessageData messageData, ZString messageType)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.LocalExportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(message, messageData),
				OriginalMessageTypes = new ZString[] { messageType },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._RR3}]",
				EntryNumber = messageData.NoticeNumber,
			};
			NotificationSender.SendNotification(notificationData);
		}
		string CreateUserFriendlyMessageInterpretation(EDIMessage message, IGOVCBRRR3MessageData messageData)
		{
			var declartionType = new ElectronicDocumentTypeList();
			var resultType = new LocalExportProcessResultTypeCodeList();

			var tableContents = new HtmlTableCreator(new string[] { "항 목", "내 용" });
			tableContents.WriteRow(new string[] { "신청서 문서구분", GOVCBR + messageData.DeclarationType });
			tableContents.WriteRow(new string[] { "제출문서", declartionType.GetDescriptionFromCode(messageData.DeclarationType) });
			tableContents.WriteRow(new string[] { "심사결과구분", resultType.GetDescriptionFromCode(messageData.ResultType) });
			tableContents.WriteRow(new string[] { "심사결과내역", messageData.ContentDescription });
			tableContents.WriteRow(new string[] { "통보세관(과)", "[" + messageData.CustomsOfficeAndDivision + "] " + MessageFunctions.GetCustomsOfficeAndDivision(message.Factory, messageData.CustomsOfficeAndDivision) });
			tableContents.WriteRow(new string[] { "세관 담당자명", messageData.CustomsPersonName });
			tableContents.WriteRow(new string[] { "신청문서 심사일시", messageData.CustomsDateTime.ToString(DateFormatType.DateTimeKorean) });
			tableContents.WriteRow(new string[] { "심사결과 통보일시", messageData.NoticeDateTime.ToString(DateFormatType.DateTimeKorean) });
			tableContents.WriteRow(new string[] { "참조번호", messageData.ConfirmNumber });
			tableContents.WriteRow(new string[] { "정정차수", messageData.AmendSequence.ToString() });

			return tableContents.ToHtml();
		}
		#endregion
	}
}
