using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._5AA)]
	public class GOVCBR5AAProcessor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBR5AADataProvider().GetMessageData(textReader);
				var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, messageData.ExportDeclarationNumber, SharedJobMessageTypeList.Codes.Export);
				if (entry != null)
				{
					message.EM_LinkedObject = entry;

					using (entry.SuspendCESLog())
					{
						entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.CLR;
					}
					entry.AddLog(Events.CustomsEntryStatus, entry.CH_EntryStatus);
					entry.CH_EntryReleaseDate = messageData.EntryReleaseDateTime;
					var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(SharedJobMessageTypeList.Codes.Export);
					entryNum.CE_ExpiryDate = messageData.LoadingDate;

					if (!messageData.CustomsOfficeContent.IsEmpty)
					{
						entry.CH_CustomsMessageRemarks = messageData.EntryReleaseDateTime.ToString(DateFormatType.DateTimeKoreanNoSecond) + "\r\n" + messageData.CustomsOfficeContent + "\r\n" + entry.CH_CustomsMessageRemarks;
					}
				}
				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(messageData);
				SendNotification(message, entry, messageData);
			}
		}

		#region SuppressResourceStringsCheckRegion

		void SendNotification(EDIMessage message, CusEntryHeader entry, IGOVCBR5AAMessageData messageData)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ExportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(messageData),
				OriginalMessageTypes = new ZString[] { ElectronicDocumentTypeList.Codes._830 },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._5AA}]",
				EntryNumber = messageData.ExportDeclarationNumber
			};
			NotificationSender.SendNotification(notificationData);
		}

		string CreateUserFriendlyMessageInterpretation(IGOVCBR5AAMessageData messageData)
		{
			var result = new ZStringBuilder();
			var tableContents = new HtmlTableCreator(new string[] { "항 목", "내 용" });
			tableContents.WriteRow(new string[] { "제출문서", "수출신고서" });
			tableContents.WriteRow(new string[] { "수리일시", messageData.EntryReleaseDateTime.ToString(DateFormatType.DateTimeKoreanNoSecond) });
			tableContents.WriteRow(new string[] { "수출신고번호", messageData.ExportDeclarationNumber });
			tableContents.WriteRow(new string[] { "적재의무기한", messageData.LoadingDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "신고가격(원화) FOB", messageData.CustomsValueKRW.ToString() });
			tableContents.WriteRow(new string[] { "신고가격(미화) FOB", messageData.CustomsValueUSD.ToString() });
			tableContents.WriteRow(new string[] { "세관기재란", messageData.CustomsOfficeContent });
			tableContents.WriteRow(new string[] { "특이사항", messageData.ContentDescription });
			tableContents.WriteRow(new string[] { "도로명표기안내", messageData.RoadNameRequest });
			result.Append(tableContents.ToHtml());

			return result.ToString();
		}

		#endregion
	}
}
