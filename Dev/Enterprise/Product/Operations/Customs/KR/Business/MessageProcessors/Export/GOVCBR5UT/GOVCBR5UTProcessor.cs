using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using WTG.StaticAnalysis.Annotation;
using static CargoWise.EventReference.Constants;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._5UT)]
	class GOVCBR5UTProcessor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBR5UTDataProvider().GetMessageData(textReader);
				var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, messageData.ExportDeclarationNumber, SharedJobMessageTypeList.Codes.Export);

				var errorMessage = string.Empty;
				if (entry != null)
				{
					message.EM_LinkedObject = entry;
					if (messageData.ShippingYN == YesNoList.Codes.Yes && messageData.ShippingDate.IsValid)
					{
						var dictionary = new System.Collections.Generic.Dictionary<string, string>();
						dictionary.Add(EventReferenceParameters.Codes.Location, entry.Declaration.JE_RL_NKPortOfLoading);
						dictionary.Add(EventReferenceParameters.Codes.Facility, Facilities.Code.Terminal);
						entry.AddLog(Events.FreightLoaded, StmALog.GenerateEventReference(string.Empty, dictionary), messageData.ShippingDate.ToZDateTime().ToOffset());

						if (entry.Declaration.JE_EntryDate.IsEmpty)
						{
							entry.Declaration.JE_EntryDate = messageData.ShippingDate;
						}

						if (!entry.Declaration.JE_EntryDate.IsEmpty && entry.Declaration.JE_EntryDate != messageData.ShippingDate)
						{
							errorMessage = Res.GetString("44F5A0F0-4DFC-4294-B1C1-9954847D2D8B", "System was going to set the actual date of loading to Declaration, but it could not because it already has a different date. Please check");
						}
					}
				}
				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(messageData, errorMessage);
				SendNotification(message, entry, messageData, errorMessage);
			}
		}

		#region SuppressResourceStringsCheckRegion
		void SendNotification(EDIMessage message, CusEntryHeader entry, IGOVCBR5UTMessageData messageData, string errorMessage)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ExportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(messageData, errorMessage),
				OriginalMessageTypes = new ZString[] { ElectronicDocumentTypeList.Codes._830 },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._5UT}]",
				EntryNumber = messageData.ExportDeclarationNumber
			};
			NotificationSender.SendNotification(notificationData);
		}

		string CreateUserFriendlyMessageInterpretation(IGOVCBR5UTMessageData messageData, string errorMessage)
		{
			var result = new ZStringBuilder();
			var tableContents = new HtmlTableCreator(new string[] { "항 목", "내 용" });
			tableContents.WriteRow(new string[] { "제출문서", "수출신고서" });
			tableContents.WriteRow(new string[] { "수출신고번호", MessageFunctions.DeclarationNumberFormat(messageData.ExportDeclarationNumber) });
			tableContents.WriteRow(new string[] { "선적여부", messageData.ShippingYN });
			tableContents.WriteRow(new string[] { "선적일자", messageData.ShippingDate.ToString(DateFormatType.DateKorean) });
			result.Append(tableContents.ToHtml());
			if (!string.IsNullOrEmpty(errorMessage))
			{
				result.Append("<br>");
				result.Append("<b style=\"color:red\">" + errorMessage + "</b>");
			}
			return result.ToString();
		}
		#endregion
	}
}
