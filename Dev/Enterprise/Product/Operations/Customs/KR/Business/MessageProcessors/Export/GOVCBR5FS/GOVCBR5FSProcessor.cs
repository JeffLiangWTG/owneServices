using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._5FS)]
	class GOVCBR5FSProcessor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBR5FSDataProvider().GetMessageData(textReader);
				var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, messageData.ExportDeclarationNumber, SharedJobMessageTypeList.Codes.Export);
				if (entry != null)
				{
					message.EM_LinkedObject = entry;
				}
				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(messageData);
				SendNotification(message, entry, messageData);
			}
		}

		#region SuppressResourceStringsCheckRegion

		void SendNotification(EDIMessage message, CusEntryHeader entry, IGOVCBR5FSMessageData messageData)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ExportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(messageData),
				OriginalMessageTypes = new ZString[] { ElectronicDocumentTypeList.Codes._830 },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._5FS}]",
				EntryNumber = messageData.ExportDeclarationNumber
			};
			NotificationSender.SendNotification(notificationData);
		}
		string CreateUserFriendlyMessageInterpretation(IGOVCBR5FSMessageData messageData)
		{
			var result = new ZStringBuilder();
			var tableContents = new HtmlTableCreator(new string[] { "항 목", "내 용" });
			tableContents.WriteRow(new string[] { "제출문서", "수출신고서" });
			tableContents.WriteRow(new string[] { "수출신고번호", MessageFunctions.DeclarationNumberFormat(messageData.ExportDeclarationNumber) });
			tableContents.WriteRow(new string[] { "수리일자", messageData.EntryReleaseDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "수출화주명", messageData.SupplierCompanyName });
			tableContents.WriteRow(new string[] { "신고자부호", messageData.DeclarantID });
			tableContents.WriteRow(new string[] { "제조자 통관고유부호", messageData.ManufacturerID });
			tableContents.WriteRow(new string[] { "품명", messageData.TradeName });
			tableContents.WriteRow(new string[] { "총포장개수", messageData.TotalPackQty.ToString() });
			tableContents.WriteRow(new string[] { "포장종류", messageData.PackType });
			tableContents.WriteRow(new string[] { "총중량", messageData.TotalGrossWeightInKG.ToString() });
			result.Append(tableContents.ToHtml());

			return result.ToString();
		}
		#endregion
	}
}
