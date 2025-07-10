using System.Collections.Specialized;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._R20)]
	public class MessageR20Processor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBRR20DataProvider().GetMessageData(textReader);
				var typeCode = messageData.DeclarationType.Substring(6);

				var supporter = new MultiPurposeResponseSupporterProvider().GetSupporterForR20(typeCode);
				if (supporter != null)
				{
					var parentHeader = supporter.LoadParent(message.Factory, message.Company, messageData.ApplicationNumber, supporter.EntryType);
					if (parentHeader != null)
					{
						message.EM_LinkedObject = parentHeader;
						var key = supporter.GetKeyToFindOutgoingMessageOrEntryNum(parentHeader, messageData);
						supporter.UpdateParent(parentHeader, typeCode, key);

						var outgoingMessage = supporter.GetOutgoingMessage(parentHeader, typeCode, key);
						message.EM_ApplicationReference = outgoingMessage?.EM_MessageNum ?? ZString.Empty;
						message.EM_MessageSubType = typeCode;
					}
					message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(message, messageData, typeCode, MessageFunctions.MessageInterpretationMode.FullView);
					if (typeCode == ElectronicDocumentTypeList.Codes._5AC || typeCode == ElectronicDocumentTypeList.Codes._5GW || typeCode == ElectronicDocumentTypeList.Codes._5SG)
					{
						SendNotificationRequest(message, (CusMiscRequestHeader)parentHeader, messageData, typeCode);
					}
					else if (parentHeader != null && parentHeader.GetType() == typeof(CusReconDeclaration))
					{
						SendNotificationReconDeclaration(message, (CusReconDeclaration)parentHeader, messageData, typeCode);
					}
					else if (parentHeader == null || parentHeader.GetType() == typeof(CusEntryHeader))
					{
						SendNotification(message, (CusEntryHeader)parentHeader, messageData, typeCode);
					}
				}
			}
		}

		#region SuppressResourceStringsCheckRegion
		void SendNotification(EDIMessage message, CusEntryHeader entry, IGOVCBRR20MessageData messageData, ZString typeCode)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ImportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(message, messageData, typeCode, MessageFunctions.MessageInterpretationMode.Email),
				OriginalMessageTypes = new ZString[] { typeCode },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._R20}]",
				EntryNumber = messageData.ApplicationNumber
			};
			NotificationSender.SendNotification(notificationData);
		}

		void SendNotificationRequest(EDIMessage message, CusMiscRequestHeader requestHeader, IGOVCBRR20MessageData messageData, ZString typeCode)
		{
			var notificationData = new DeclarationEntryNotificationData()
			{
				ControllerIDProvider = requestHeader,
				MessagesParent = requestHeader,
				JobNumberDescription = $"Declaration Number: {requestHeader?.CMR_JobNumber ?? ZString.Empty} / 제출번호: {messageData.ApplicationNumber}",
				Branch = requestHeader?.Branch ?? message.Branch,
				EmailGroup = messageData.DeclarationType == ElectronicDocumentTypeList.Codes._5GW ? KRCustomsRegistry.Instance.ImportEmailGroup : KRCustomsRegistry.Instance.ExportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(message, messageData, typeCode, MessageFunctions.MessageInterpretationMode.Email),
				OriginalMessageTypes = new ZString[] { typeCode },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._R20}]",
				EntryNumber = messageData.ApplicationNumber,
				AlternativeRecipientStaff = requestHeader?.Broker
			};
			NotificationSender.SendNotification(notificationData);
		}

		void SendNotificationReconDeclaration(EDIMessage message, CusReconDeclaration reconDeclaration, IGOVCBRR20MessageData messageData, ZString typeCode)
		{
			var notificationData = NotificationSender.GetReconDeclarationNotificationData(reconDeclaration, messageData.ApplicationNumber);
			notificationData.EmailBody = CreateUserFriendlyMessageInterpretation(message, messageData, typeCode, MessageFunctions.MessageInterpretationMode.Email);
			notificationData.MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._R20}]";
			NotificationSender.SendNotification(notificationData);
		}

		string CreateUserFriendlyMessageInterpretation(EDIMessage message, IGOVCBRR20MessageData messageData, ZString typeCode, MessageFunctions.MessageInterpretationMode mode)
		{
			var result = new ZStringBuilder();

			var tableContents = new HtmlTableCreator(new string[] { "항 목", "내 용" });
			tableContents.WriteRow(new string[] { "제출문서", new ElectronicDocumentTypeList().GetDescriptionFromCode(typeCode) });
			tableContents.WriteRow(new string[] { "통보일시", messageData.NoticeDateTime.ToString(DateFormatType.DateTimeKorean) });
			tableContents.WriteRow(new string[] { "신청문서 제출번호", messageData.ApplicationNumber });
			tableContents.WriteRow(new string[] { "신청문서 정정차수", messageData.AmendSequence.ToString() });
			tableContents.WriteRow(new string[] { "신청문서 수신일시", messageData.AcceptDateTime.ToString(DateFormatType.DateTimeKorean) });
			tableContents.WriteRow(new string[] { "통보세관(과)", messageData.CustomsOfficeAndDivision.IsEmpty ? string.Empty : "[" + messageData.CustomsOfficeAndDivision + "] " + MessageFunctions.GetCustomsOfficeAndDivision(message.Factory, messageData.CustomsOfficeAndDivision) });
			result.Append(tableContents.ToHtml());

			var detailContents = new HtmlTableCreator(System.Array.Empty<string>());
			if (messageData.Error.Any())
			{
				detailContents.WriteRowWithFormatting(new CellWithFormatting("오류내역", new NameValueCollection { { "colspan", "4" }, { "align", "left" } }, true));
				detailContents.WriteRowWithFormatting(new CellWithFormatting("오류내역", new NameValueCollection { { "colspan", "1" }, { "align", "center" } }), new CellWithFormatting("오류문서 KEY", new NameValueCollection { { "colspan", "3" }, { "align", "center" } }));
				var i = 0;
				foreach (var error in messageData.Error)
				{
					if (mode == MessageFunctions.MessageInterpretationMode.Email && i > 9)
					{
						detailContents.WriteRowWithFormatting(new CellWithFormatting("나머지 내역은 프로그램에서 확인 하십시오. ", new NameValueCollection { { "colspan", "4" } }));
						break;
					}
					var keys = error.ApplicationKey;
					detailContents.WriteRowWithFormatting(new CellWithFormatting(error.ErrorDescription), new CellWithFormatting(keys.ElementAtOrDefault(0)), new CellWithFormatting(keys.ElementAtOrDefault(1)), new CellWithFormatting(keys.ElementAtOrDefault(2)));
					i += 1;
				}
				result.Append(detailContents.ToHtml());
			}

			return result.ToString();
		}
		#endregion
	}
}
