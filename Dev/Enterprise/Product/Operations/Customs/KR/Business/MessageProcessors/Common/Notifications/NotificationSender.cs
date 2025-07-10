using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.KR.Business
{
	#region SuppressResourceStringsCheckRegion

	public static class NotificationSender
	{
		public static void SendNotification(NotificationData notificationData)
		{
			if (!TrySendEmailToOriginatorOrEmailGroup(notificationData))
			{
				SendErrorNotificationToEmailGroup(notificationData);
			}
		}

		static ZString GetNotificationRecipient(this NotificationData notificationData)
		{
			var result = ZString.Empty;
			if (notificationData.MessagesParent != null && notificationData.OriginalMessageTypes != null)
			{
				var outgoingMessages = notificationData.MessagesParent.Messages.GetMatchingMessages(EDIMessage.ApplicationCodes.KRCustoms, notificationData.OriginalMessageTypes, EDIMessage.Direction.Transmit);
				var lastOutgoingMessage = outgoingMessages.OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();
				result = lastOutgoingMessage?.UserWhoQueuedThisRecord?.GS_EmailAddress ?? ZString.Empty;
			}
			if (result.IsEmpty)
			{
				result = notificationData.AlternativeRecipientStaff?.GS_EmailAddress ?? ZString.Empty;
			}
			return result;
		}

		static bool TrySendEmailToOriginatorOrEmailGroup(NotificationData notificationData, string responseDescription = null)
		{
			var result = false;
			var parent = notificationData.MessagesParent;
			if (parent != null)
			{
				var emailGenerator = new HtmlResponseEmailGenerator();
				if (responseDescription != null)
				{
					emailGenerator.ResponseDescription = responseDescription;
				}

				var jobNumber = notificationData.JobNumberDescription;
				var url = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(notificationData.ControllerIDProvider);
				var recipient = notificationData.GetNotificationRecipient();
				if (!recipient.IsEmpty)
				{
					emailGenerator.TryGenerateEmail(url, jobNumber, notificationData.MessageTypeDescription, notificationData.EmailBody, false, out EmailDef email, notificationData.Branch);
					email.AddRecipientForSystemCommunication(recipient);
					Env.OutgoingCustomsMailManager.CreateAndSave(email);
					result = true;
				}
				else
				{
					string reasonsToSendToEmailGroup = "";
					if (notificationData.OriginalMessageTypes != null)
					{
						var originalTypeDescriptions = new ZStringBuilder();
						var list = parent.Factory.GetCachedValue<ElectronicDocumentTypeList>();

						foreach (var originalMessageType in notificationData.OriginalMessageTypes)
						{
							originalTypeDescriptions.Append(list.GetDescriptionFromCode(originalMessageType) ?? originalMessageType);
						}

						reasonsToSendToEmailGroup = $"요청 메시지 [{originalTypeDescriptions.ToStringWithDelimiterBetweenAppends(", ")}]를 찾을 수 없어 해당 메시지 송신자가 아닌 ";
					}
					else
					{
						reasonsToSendToEmailGroup = "관련 메세지가 없으므로 ";
					}
					var additionalDetail = reasonsToSendToEmailGroup + $"레지스트리에 설정된 이메일 그룹으로 보내집니다. <br />";
					emailGenerator.TryGenerateEmail(url, jobNumber, notificationData.MessageTypeDescription, additionalDetail + notificationData.EmailBody, false, out EmailDef email, notificationData.Branch);
					Env.OutgoingCustomsMailManager.CreateAndSave(
						email,
						notificationData.EmailGroup.GetFallBackValueAtAllLevels(notificationData.Branch.GB_GC.ToGuid(), notificationData.Branch.PK.ToGuid(), Guid.Empty),
						GroupSourceLocator.GetFromRegistryItem(notificationData.EmailGroup));
					result = true;
				}
			}
			return result;
		}

		static void SendErrorNotificationToEmailGroup(NotificationData notificationData)
		{
			var emailSubject = $"이메일 전송실패: {notificationData.MessageTypeDescription + notificationData.EntryNumber} 사유: 신고내역을 찾을 수 없습니다.";
			var emailGenerator = new HtmlResponseEmailGenerator();
			emailGenerator.TryGenerateEmail(emailSubject, emailSubject, emailGenerator.ResponseDescription, notificationData.EmailBody, "", out EmailDef email, notificationData.Branch);
			Env.OutgoingCustomsMailManager.CreateAndSave(
					email,
					notificationData.EmailGroup.GetFallBackValueAtAllLevels(notificationData.Branch.GB_GC.ToGuid(), notificationData.Branch.PK.ToGuid(), Guid.Empty),
					GroupSourceLocator.GetFromRegistryItem(notificationData.EmailGroup));
		}

		public static string XERNotificationEmailBody => "Your message could not go through the xT server.<br />Please try to send a message again later and contact the WTG Support team for more information.";
		public static string ESRNotificationEmailBody => "Your message has been rejected by Customs. Please check the details below and rectify the problem.<br /><br />";

		public static NotificationData GetNotificationData(EDIMessage message, CusMiscRequestHeader requestHeader, string emailBody)
		{
			return new NotificationData()
			{
				ControllerIDProvider = requestHeader,
				MessagesParent = requestHeader,
				JobNumberDescription = $"Request Number: {requestHeader.CMR_JobNumber} / 제출번호: {requestHeader.CusEntryNumber.CE_EntryNum}",
				Branch = requestHeader.Branch ?? message.Branch,
				EmailBody = emailBody,
				EntryNumber = requestHeader.CusEntryNumber.CE_EntryNum,
				AlternativeRecipientStaff = requestHeader.Broker
			};
		}

		public static NotificationData GetNotificationData(EDIMessage message, CusEntryHeader entry, string emailBody)
		{
			return new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = entry.Declaration.Branch ?? message.Branch,
				EmailBody = emailBody,
				EntryNumber = entry.EntryNumber
			};
		}

		public static void ErrorSendNotification(NotificationData notificationData, string responseDescription = null)
		{
			TrySendEmailToOriginatorOrEmailGroup(notificationData, responseDescription);
		}

		public static void ErrorSendNotification(EDIMessage message, string emailBody, string emailSubject, GuidRegistryItem emailGroup, string responseDescription = null)
		{
			var responseGenerator = new HtmlResponseEmailGenerator();
			if (responseDescription != null)
			{
				responseGenerator.ResponseDescription = responseDescription;
			}
			responseGenerator.TryGenerateEmail(emailSubject, emailSubject, "", emailBody, "", out EmailDef email, message.Branch);
			Env.OutgoingCustomsMailManager.CreateAndSave(email, emailGroup.GetFallBackValueAtAllLevels(message.Branch.GB_GC.ToGuid(), message.Branch.PK.ToGuid(), Guid.Empty), GroupSourceLocator.GetFromRegistryItem(emailGroup));
		}

		public static NotificationData GetReconDeclarationNotificationData(CusReconDeclaration reconDeclaration, string entryNumber)
		{
			return new NotificationData()
			{
				ControllerIDProvider = reconDeclaration,
				MessagesParent = reconDeclaration,
				JobNumberDescription = $"Refund Declaration Number: {reconDeclaration.CRD_JobReferenceNumber} / 제출번호: {entryNumber}",
				Branch = reconDeclaration.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ImportEmailGroup,
				OriginalMessageTypes = new ZString[] { ElectronicDocumentTypeList.Codes._5UL },
				EntryNumber = entryNumber,
				AlternativeRecipientStaff = reconDeclaration.CustomsAgent
			};
		}
		#endregion
	}
}
