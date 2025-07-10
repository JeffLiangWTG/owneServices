using System;
using System.Collections.Specialized;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal
{
	public abstract class AFREventProcessor
	{
		protected AFREventProcessor(IXmlEventValueObject eventDataObject, IXmlImportLogger logger, BusinessObjectFactory factory)
		{
			this.eventDataObject = Argument.NotNull(eventDataObject, "eventDataObject");
			this.logger = Argument.NotNull(logger, "logger");
			this.factory = Argument.NotNull(factory, "factory");
		}

		public void Process(JPAFRHeader header)
		{
			lastTransmitMessageCached = null;
			originalMessageCached = null;
			ProcessCore(header);
			SendNotification(header);
		}
		protected abstract void ProcessCore(JPAFRHeader header);

		protected abstract string GetMessageTypeDesc(JPAFRHeader header);

		void SendNotification(JPAFRHeader header)
		{
			string url = null;
			string jobNumber = null;
			if (header != null)
			{
				url = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(header);
				jobNumber = header.JPH_JobReference;
			}
			var body = GetEmailBody();
			var isFailure = !IsSuccessResponse;
			GenerateHtmlEmailAndSendToOriginalOrGroup(url, jobNumber, GetMessageTypeDesc(header), body, isFailure, header);
		}

		string GetEmailBody()
		{
			ZString result;
			if (eventDataObject.Context.NotificationDetails.HasValue)
			{
				result = eventDataObject.Context.NotificationDetails.Value;
			}
			else
			{
				var creator = new HtmlTableCreator(new[] { new CellWithFormatting("Manifest Details", new NameValueCollection { { "colspan", "2" } }, true) });
				if (eventDataObject.Context.CarrierCode.HasValue)
				{
					creator.WriteRow("Carrier Code", eventDataObject.Context.CarrierCode.Value);
				}
				if (eventDataObject.Context.VesselName.HasValue)
				{
					creator.WriteRow("Vessel Name", eventDataObject.Context.VesselName.Value);
				}
				if (!eventDataObject.Context.VesselCallSign.IsEmpty)
				{
					creator.WriteRow("Vessel Call Sign", eventDataObject.Context.VesselCallSign);
				}
				if (eventDataObject.Context.VoyageNumber.HasValue)
				{
					creator.WriteRow("Voyage Number", eventDataObject.Context.VoyageNumber.Value);
				}
				if (eventDataObject.Context.PortOfLoadingUNLOCO.HasValue)
				{
					creator.WriteRow("Port Of Loading", eventDataObject.Context.PortOfLoadingUNLOCO.Value);
				}
				if (eventDataObject.Context.PortOfLoadingSuffix.HasValue)
				{
					creator.WriteRow("Port Of Loading Suffix", eventDataObject.Context.PortOfLoadingSuffix.Value);
				}
				if (eventDataObject.Context.TimeOfDeparture.HasValue)
				{
					creator.WriteRow("Time Of Departure", eventDataObject.Context.TimeOfDeparture.Value);
				}
				if (eventDataObject.Context.TimeOfArrival.HasValue)
				{
					creator.WriteRow("Time Of Arrival", eventDataObject.Context.TimeOfArrival.Value);
				}
				if (eventDataObject.Context.MBOLNumber.HasValue)
				{
					creator.WriteRow("Master Bill", eventDataObject.Context.MBOLNumber.Value);
				}
				if (eventDataObject.Context.HBOLNumber.HasValue)
				{
					creator.WriteRow("House Bill", eventDataObject.Context.HBOLNumber.Value);
				}
				result = creator.ToHtml();
			}
			return result;
		}

		protected virtual bool IsSuccessResponse => eventDataObject.EventReference.TrimEnd().EndsWith("-ACCEPTED", StringComparison.OrdinalIgnoreCase);

		#region Send emails to the original sender

		protected void SendEmailToOriginalSenderOrGroupIfSenderInvalid(EmailDef email, JPAFRHeader header)
		{
			SendEmailToOriginalSenderOrGroupIfSenderInvalid(email, GetBranch(header), GetEmailAddressToSendTo(header));
		}

		void SendEmailToOriginalSenderOrGroupIfSenderInvalid(EmailDef email, IGlbBranch branch, ZString emailAddressToSendTo)
		{
			if (email != null)
			{
				using (branch.SetAsTemporaryContext())
				{
					var sendMode = EmailModeRegistryItem.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
					if (sendMode != Enterprise.Core.Constants.EmailTo.NoEmails)
					{
						var calculatedUserToNotifyEmail = ZString.Empty;
						if (sendMode == Enterprise.Core.Constants.EmailTo.StaffMember || sendMode == Core.Constants.EmailTo.StaffMemberAndNominatedGroup)
						{
							calculatedUserToNotifyEmail = emailAddressToSendTo;
						}
						if (!calculatedUserToNotifyEmail.IsEmpty && !email.Recipients.Contains(calculatedUserToNotifyEmail))
						{
							email.AddRecipientForSystemCommunication(calculatedUserToNotifyEmail);
						}

						var calculatedGroupPK = ZGuid.Empty;
						if (sendMode == Enterprise.Core.Constants.EmailTo.NominatedGroup || sendMode == Core.Constants.EmailTo.StaffMemberAndNominatedGroup)
						{
							calculatedGroupPK = EmailGroupRegistryItem.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
						}

						if (!calculatedGroupPK.IsEmpty)
						{
							Env.OutgoingCustomsMailManager.Create(factory, email, calculatedGroupPK.ToGuid(), GroupSourceLocator.GetFromRegistryItem(EmailGroupRegistryItem));
						}
						else if (!calculatedUserToNotifyEmail.IsEmpty)
						{
							Env.OutgoingCustomsMailManager.Create(factory, email);
						}
						else if (!IsSuccessResponse)
						{
							SendEmailToPostMastersGroup(email);
						}
					}
				}
			}
		}

		void SendEmailToPostMastersGroup(EmailDef email)
		{
			try
			{
				Env.OutgoingCustomsMailManager.Create(factory, email, Core.Constants.Groups.PostMastersGroupPK, GroupSourceLocator.GetFromRegistryItem(Env.Registry.RawRegistry.NotificationGroup));
			}
			catch (EmailHasNoRecipientsException noRecipientsException)
			{
				logger.Log(Integration.LogType.Warning, noRecipientsException.Message);
			}
		}

		protected ZString GetEmailAddressToSendTo(JPAFRHeader header)
		{
			var result = ZString.Empty;
			if (header != null)
			{
				var originalMessage = GetOriginalMessage(header);
				var originalSender = (IUser)originalMessage?.UserWhoQueuedThisRecord;

				if (originalSender == null || originalSender.IsBatchProcessor)
				{
					originalMessage = GetLastTransmitMessage(header);
					originalSender = originalMessage?.UserWhoQueuedThisRecord;
				}

				if (!HasValidEmailRecipient(originalSender))
				{
					originalMessage = GetLasTransmitMessageWithValidRecipient(header);
					originalSender = originalMessage?.UserWhoQueuedThisRecord;
				}
				result = originalSender?.EmailAddress ?? string.Empty;
			}

			return result;
		}

		bool HasValidEmailRecipient(IUser staff)
		{
			return staff != null && !(staff.IsSystemAccount || string.IsNullOrWhiteSpace(staff.EmailAddress));
		}

		protected EDIMessage GetOriginalMessage(JPAFRHeader header)
		{
			EDIMessage result = null;
			if (header != null)
			{
				if (originalMessageCached == null || originalMessageCached.EM_LinkUniqueID != header.PK)
				{
					originalMessageCached = null;
					var messageNumber = eventDataObject.Context.InternalTransactionNumber;
					if (!messageNumber.IsEmpty)
					{
						originalMessageCached = header.Messages.OfType<XmlEDIMessage>().FirstOrDefault(x => x.EM_MessageNum == messageNumber && x.IsAFRTransmitMessage());
					}
				}
				result = originalMessageCached;
			}
			return result;
		}
		EDIMessage originalMessageCached;

		public EDIMessage GetLastTransmitMessage(JPAFRHeader header)
		{
			EDIMessage result = null;
			if (header != null)
			{
				if (lastTransmitMessageCached == null || lastTransmitMessageCached.EM_LinkUniqueID != header.PK)
				{
					lastTransmitMessageCached = header.Messages.OfType<XmlEDIMessage>()
						.Where(x => x.IsAFRTransmitMessage() && x.EM_SystemCreateUser != User.ServiceUserCode)
							.OrderByDescending(x => x.EM_SystemCreateTimeUtc)
								.FirstOrDefault();
				}
				result = lastTransmitMessageCached;
			}
			return result;
		}
		EDIMessage lastTransmitMessageCached;

		EDIMessage GetLasTransmitMessageWithValidRecipient(JPAFRHeader header)
		{
			var headerMessages = header.Messages.OfType<XmlEDIMessage>();

			var lastTransmitMessageWithValidRecipient = headerMessages
				.Where(message => message.IsAFRTransmitMessage())
				.OrderByDescending(message => message.EM_MessageNum == eventDataObject.Context.InternalTransactionNumber ? ZDateTime.Empty : message.EM_SystemCreateTimeUtc)
				.FirstOrDefault(message => HasValidEmailRecipient(message.UserWhoQueuedThisRecord));

			return lastTransmitMessageWithValidRecipient;
		}

		protected void GenerateHtmlEmailAndSendToOriginalOrGroup(string uri, string jobNumber, string messageTypeInSubject, string body, bool isFailure, JPAFRHeader header)
		{
			var email = new HtmlResponseEmailGenerator().GenerateEmail(uri, jobNumber, messageTypeInSubject, body, isFailure, GetBranch(header));
			SendEmailToOriginalSenderOrGroupIfSenderInvalid(email, header);
		}

		CodePairRegistryItem EmailModeRegistryItem
		{
			get { return IsSuccessResponse ? JPAFRRegistry.Instance.SendMessageAcknowledgements : JPAFRRegistry.Instance.SendMessageErrors; }
		}

		GuidRegistryItem EmailGroupRegistryItem
		{
			get { return IsSuccessResponse ? JPAFRRegistry.Instance.SendMessageAcknowledgementsToGroup : JPAFRRegistry.Instance.SendMessageErrorsToGroup; }
		}

		static IGlbBranch GetBranch(JPAFRHeader header) => header?.Branch ?? GlbBranch.CurrentBranch;

		#endregion

		protected readonly IXmlEventValueObject eventDataObject;
		protected readonly IXmlImportLogger logger;
		protected readonly BusinessObjectFactory factory;
	}
}
