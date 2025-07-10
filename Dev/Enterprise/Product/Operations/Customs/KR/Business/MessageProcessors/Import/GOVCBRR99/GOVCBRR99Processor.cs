using System.Collections.Specialized;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._R99)]
	class GOVCBRR99Processor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBRR99DataProvider().GetMessageData(textReader);
				var typeCode = messageData.DeclarationType;
				var supporter = new MultiPurposeResponseSupporterProvider().GetSupporterForR99(typeCode);
				if (supporter != null)
				{
					ZStringBuilder emailBuilder = new ZStringBuilder();
					var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, messageData.ApplicationNumber, supporter.EntryType);
					if (entry != null)
					{
						message.EM_LinkedObject = entry;
						message.EM_MessageSubType = typeCode;
						message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(message, messageData, MessageFunctions.MessageInterpretationMode.FullView, emailBuilder);

						supporter.UpdateParent(message, entry, typeCode, messageData, emailBuilder);
					}
					SendNotification(message, entry, messageData, emailBuilder);
				}
			}
		}

		#region SuppressResourceStringsCheckRegion

		void SendNotification(EDIMessage message, CusEntryHeader entry, IGOVCBRR99MessageData messageData, ZStringBuilder stringBuilder)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ImportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(message, messageData, MessageFunctions.MessageInterpretationMode.Email, stringBuilder),
				OriginalMessageTypes = new ZString[] { messageData.DeclarationType },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._R99}]",
				EntryNumber = messageData.ApplicationNumber
			};
			NotificationSender.SendNotification(notificationData);
		}

		string CreateUserFriendlyMessageInterpretation(EDIMessage message, IGOVCBRR99MessageData messageData, MessageFunctions.MessageInterpretationMode mode, ZStringBuilder stringBuilder)
		{
			var tableContents = new HtmlTableCreator(System.Array.Empty<string>());
			tableContents.WriteRowWithFormatting(new CellWithFormatting("항 목", new NameValueCollection { { "width", "200" } }, true), new CellWithFormatting("내 용", new NameValueCollection { { "width", "300" } }, true));
			var electronicDocumentTypeList = new ElectronicDocumentTypeList();
			tableContents.WriteRow(new string[] { "제출문서", electronicDocumentTypeList.GetDescriptionFromCode(messageData.DeclarationType) });
			tableContents.WriteRow(new string[] { "신청문서 제출번호", messageData.ApplicationNumber });
			tableContents.WriteRow(new string[] { "신청문서 제출차수", messageData.AmendSequence.ToString() });
			tableContents.WriteRow(new string[] { "수신문서 제출구분", messageData.DeclarationSubType });
			tableContents.WriteRow(new string[] { "접수세관", messageData.CustomsOffice.IsEmpty ? string.Empty : "[" + messageData.CustomsOffice + "] " + MessageFunctions.GetCustomsOfficeAndDivision(message.Factory, messageData.CustomsOffice) });
			tableContents.WriteRow(new string[] { "담당자", messageData.CustomsPersonID.IsEmpty ? messageData.CustomsPersonName.ToString() : "[" + messageData.CustomsPersonID + "] " + messageData.CustomsPersonName });
			tableContents.WriteRow(new string[] { "신청일자", messageData.DeclarationDate.ToString(DateFormatType.Date) });
			tableContents.WriteRow(new string[] { "수신일시", messageData.AcceptDateTime.ToString(DateFormatType.DateTimeKorean) });
			tableContents.WriteRow(new string[] { "통보일시", messageData.NoticeDateTime.ToString(DateFormatType.DateTimeKorean) });
			tableContents.WriteRow(new string[] { "통보번호", messageData.NoticeNumber });
			stringBuilder.Append(tableContents.ToHtml());

			if (messageData.Content != null)
			{
				var detailContents = new HtmlTableCreator(System.Array.Empty<string>()) { EnableHTMLEncoding = false };
				detailContents.WriteRowWithFormatting(new CellWithFormatting("선별결과 내역", new NameValueCollection { { "align", "left" }, { "colspan", "2" } }, true));
				detailContents.WriteRowWithFormatting(new CellWithFormatting("구분", new NameValueCollection { { "align", "center" }, { "width", "200" } }), new CellWithFormatting("내용", new NameValueCollection { { "align", "center" }, { "width", "300" } }));
				var i = 0;
				foreach (var content in messageData.Content)
				{
					if (mode == MessageFunctions.MessageInterpretationMode.Email && i > 9)
					{
						detailContents.WriteRowWithFormatting(new CellWithFormatting("나머지 내역은 프로그램에서 확인 하십시오.", new NameValueCollection { { "colspan", "2" } }));
						break;
					}
					var additionalInformationCodeList = new AdditionalInformationCodeList();
					detailContents.WriteRow(new string[] { "[" + content.ContentType + "] " + additionalInformationCodeList.GetDescriptionFromCode(content.ContentType), content.ContentDescription });
					i += 1;
				}
				stringBuilder.Append(detailContents.ToHtml());
			}

			return stringBuilder.ToString();
		}

		#endregion
	}
}
