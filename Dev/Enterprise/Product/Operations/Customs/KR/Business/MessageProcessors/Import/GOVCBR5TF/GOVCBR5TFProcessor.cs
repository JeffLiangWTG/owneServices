using System.Collections.Specialized;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("Soon to be used")]
	[MessageType(ElectronicDocumentTypeList.Codes._5TF)]
	class GOVCBR5TFProcessor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBR5TFDataProvider().GetMessageData(textReader);
				var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, messageData.ImportDeclarationNumber, SharedJobMessageTypeList.Codes.Import);
				if (entry != null)
				{
					message.EM_LinkedObject = entry;

					SendEmailToOriginalSender(message, entry, messageData);
				}
			}
		}

		#region SuppressResourceStringsCheckRegion
		void SendEmailToOriginalSender(EDIMessage message, CusEntryHeader entry, IGOVCBR5TFMessageData messageData)
		{
			var originalMessage = entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._929);
			var originalSender = originalMessage?.UserWhoQueuedThisRecord?.GS_EmailAddress ?? ZString.Empty;

			if (!originalSender.IsEmpty)
			{
				var tableContents = new HtmlTableCreator(System.Array.Empty<string>());
				tableContents.WriteRowWithFormatting(new CellWithFormatting("항 목", new NameValueCollection { { "width", "200" } }, true), new CellWithFormatting("내 용", new NameValueCollection { { "width", "300" } }, true));
				tableContents.WriteRow(new string[] { "제출문서", "수입신고서" });
				tableContents.WriteRow(new string[] { "신고번호", MessageFunctions.DeclarationNumberFormat(messageData.ImportDeclarationNumber) });
				tableContents.WriteRow(new string[] { "신고일자", messageData.DeclarationDate.ToString(DateFormatType.DateKorean) });
				tableContents.WriteRow(new string[] { "수리일자", messageData.EntryReleaseDate.ToString(DateFormatType.DateKorean) });
				tableContents.WriteRow(new string[] { "신고인 상호 및 성명", messageData.DeclarantCompanyName });
				tableContents.WriteRow(new string[] { "수입자 상호 또는 성명", messageData.ImportCompanyName });

				var title = $"[" + ElectronicDocumentTypeList.Descriptions._5TF + "]";
				var jobNumber = $"Declaration Number: {entry.Declaration.JE_DeclarationReference} / 제출번호: {messageData.ImportDeclarationNumber}";
				var queryData = (IControllerIDProvider)entry.Declaration;
				var url = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(queryData);

				EmailDef email;
				new HtmlResponseEmailGenerator().TryGenerateEmail(url, jobNumber, title, tableContents.ToHtml(), false, out email, message.Branch);
				email.AddRecipientForSystemCommunication(originalSender);
				Env.OutgoingCustomsMailManager.CreateAndSave(email);
			}
		}
		#endregion
	}
}
