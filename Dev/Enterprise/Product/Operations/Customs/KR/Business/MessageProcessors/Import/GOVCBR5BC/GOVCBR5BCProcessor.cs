using System.Collections.Specialized;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._5BC)]
	class GOVCBR5BCProcessor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBR5BCDataProvider().GetMessageData(textReader);
				var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, messageData.ImportDeclarationNumber, SharedJobMessageTypeList.Codes.Import);
				if (entry != null)
				{
					message.EM_LinkedObject = entry;
					var outgoingMessage = entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5BB);
					message.EM_ApplicationReference = outgoingMessage?.EM_MessageNum ?? ZString.Empty;
					message.EM_MessageSubType = ElectronicDocumentTypeList.Codes._5BB;

					if (messageData.ResultType == ResultTypeList.Codes.C)
					{
						message.EM_MessageOwner = CustomsEntryStatusTypeList.Codes.ANT;
					}
					else if (messageData.ResultType == ResultTypeList.Codes.E)
					{
						message.EM_MessageOwner = CustomsEntryStatusTypeList.Codes.DMS;
						entry.MarkLodgedSnapshotAsDeleted(ElectronicDocumentTypeList.Codes._5BA);
					}
				}
				var messageInterpretation = CreateUserFriendlyMessageInterpretation(messageData, message);
				message.EM_MessageInterpretation = messageInterpretation;
				SendNotification(message, entry, messageData, messageInterpretation);
			}
		}

		#region SuppressResourceStringsCheckRegion

		void SendNotification(EDIMessage message, CusEntryHeader entry, IGOVCBR5BCMessageData messageData, string messageInterpretation)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ImportEmailGroup,
				EmailBody = messageInterpretation,
				OriginalMessageTypes = new ZString[] { ElectronicDocumentTypeList.Codes._5BB },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._5BC}]",
				EntryNumber = messageData.ImportDeclarationNumber
			};
			NotificationSender.SendNotification(notificationData);
		}

		string CreateUserFriendlyMessageInterpretation(IGOVCBR5BCMessageData messageData, EDIMessage message)
		{
			var result = new ZStringBuilder();
			var tableContents = new HtmlTableCreator(System.Array.Empty<string>());
			tableContents.WriteRowWithFormatting(new CellWithFormatting("항 목", new NameValueCollection { { "width", "200" } }, true), new CellWithFormatting("내 용", new NameValueCollection { { "width", "300" } }, true));
			tableContents.WriteRow(new string[] { "제출문서", "합의세율 정정신청서" });
			tableContents.WriteRow(new string[] { "수입신고번호", MessageFunctions.DeclarationNumberFormat(messageData.ImportDeclarationNumber) });
			tableContents.WriteRow(new string[] { "승인(기각)일자", messageData.ApprovalDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "정정처리구분", new _5BAAmendmentType().GetDescriptionFromCode(messageData.AmendType) });
			tableContents.WriteRow(new string[] { "처리결과", new ResultTypeList().GetDescriptionFromCode(messageData.ResultType) });
			tableContents.WriteRow(new string[] { "처리세관", "[" + messageData.CustomsOffice + "] " + MessageFunctions.GetCustomsOfficeAndDivision(message.Factory, messageData.CustomsOffice) });
			tableContents.WriteRow(new string[] { "정정기각사유", messageData.ResultReason });
			tableContents.WriteRow(new string[] { "처리담당자명", messageData.CustomsManagerName });

			result.Append(tableContents.ToHtml());

			return result.ToString();
		}
		#endregion
	}
}
