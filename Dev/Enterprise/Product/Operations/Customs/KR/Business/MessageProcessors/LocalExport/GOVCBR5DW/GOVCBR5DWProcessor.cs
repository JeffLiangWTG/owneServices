using System.Collections.Specialized;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._5DW)]
	public class GOVCBR5DWProcessor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBR5DWDataProvider().GetMessageData(textReader);
				var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, messageData.DeclarationNumber, KRJobMessageTypeList.Codes.LocalExport);
				if (entry != null)
				{
					message.EM_LinkedObject = entry;
				}
				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(messageData);
				SendNotification(message, entry, messageData);
			}
		}

		#region SuppressResourceStringsCheckRegion
		void SendNotification(EDIMessage message, CusEntryHeader entry, IGOVCBR5DWMessageData messageData)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.LocalExportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(messageData),
				OriginalMessageTypes = new ZString[] { ElectronicDocumentTypeList.Codes._5DP },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._5DW}]",
				EntryNumber = messageData.DeclarationNumber,
			};
			NotificationSender.SendNotification(notificationData);
		}

		string CreateUserFriendlyMessageInterpretation(IGOVCBR5DWMessageData messageData)
		{
			var electronicDocumentTypeList = new ElectronicDocumentTypeList();
			var tableContents = new HtmlTableCreator(new string[] { "항 목", "내 용" });
			tableContents.WriteRow(new string[] { "제출문서", electronicDocumentTypeList.GetDescriptionFromCode(ElectronicDocumentTypeList.Codes._5DP) });
			tableContents.WriteRow(new string[] { "접수번호", messageData.AcceptNumber });
			tableContents.WriteRow(new string[] { "접수일자", messageData.AcceptDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "제출번호", MessageFunctions.DeclarationNumberFormat(messageData.DeclarationNumber) });
			tableContents.WriteRow(new string[] { "공급(신청자) 상호", messageData.DeclarantCompanyName });
			tableContents.WriteRow(new string[] { "제조자 상호", messageData.ManufacturerCompanyName });
			tableContents.WriteRow(new string[] { "양수자 상호", messageData.ImporterCompanyName });
			tableContents.WriteRow(new string[] { "물품확인일자", messageData.ProductConfirmationDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "물품확인직원부호", messageData.ProductConfirmationManagerID });
			tableContents.WriteRow(new string[] { "물품확인직원명", messageData.ProductConfirmationManagerName });
			tableContents.WriteRow(new string[] { "총란수", messageData.TotalEntryLineCount.ToString() });
			tableContents.WriteRow(new string[] { "총포장갯수", messageData.TotalPackages.ToString() });
			tableContents.WriteRow(new string[] { "공급금액합계", messageData.TotalDeclarationAmount.ToString() });
			tableContents.WriteRow(new string[] { "총중량", messageData.TotalGrossWeight.ToString() });
			tableContents.WriteRow(new string[] { "세관기재란", messageData.CustomsOfficeContent });
			tableContents.WriteRowWithFormatting(new CellWithFormatting("나머지 내역은 프로그램에서 확인 하십시오.", new NameValueCollection { { "align", "left" }, { "colspan", "2" } }, false));

			return tableContents.ToHtml();
		}
		#endregion
	}
}
