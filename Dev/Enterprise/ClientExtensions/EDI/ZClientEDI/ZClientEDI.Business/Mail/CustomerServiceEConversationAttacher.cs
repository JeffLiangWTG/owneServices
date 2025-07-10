using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.EConversation.Business;
using Enterprise.EConversation.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using MimeKit;

namespace ZClientEDI.Business.Mail
{
	public class CustomerServiceEConversationAttacher : BusinessObjectEConversationAttacher
	{
		protected override JobConversationParticipant AddParticipantFromEmail(string sender, JobConversationParticipantCollection participants, JobConversation eConvo)
		{
			if (!(eConvo.Parent is EdiIncidentRequest incident))
			{
				return base.AddParticipantFromEmail(sender, participants, eConvo);
			}

			ZQuery contactQuery = new ZQuery(OrgContactSchema.OC_Email, sender);
			contactQuery.AddToFilter(new ZQuery(OrgContactSchema.OC_IsActive, true));
			contactQuery.AddToFilter(new ZQuery(OrgContactSchema.OC_OH, incident.RelatedSupportIncident.IM_OH_Client));
			contactQuery.OrderBy = OrgContactSchema.Constants.OC_WebAccessEnabled + OrderByClause.Descending;

			var contacts = participants.Factory.Load<OrgContact>(contactQuery);

			if (contacts.Length > 0)
			{
				var existingParticipant = participants.FirstOrDefault(p => contacts.Any(c => c.PK == p.JCP_ParticipantID));
				if (existingParticipant != null)
				{
					return existingParticipant;
				}

				var contact = contacts[0];

				var participant = participants.Factory.New<JobConversationParticipant>();
				participant.JCP_ParticipantTableCode = contact.TablePrefix;
				participant.JCP_ParticipantID = contact.PK;
				participants.Add(participant);

				return participant;
			}
			else
			{
				return participants.AddNewParticipant(sender);
			}
		}

		public override void OnAttachedEmailToJobConversation(Email mailAsEDoc, JobConversation convo)
		{
			base.OnAttachedEmailToJobConversation(mailAsEDoc, convo);

			if (!(convo.Parent is EdiIncidentRequest incident))
			{
				return;
			}

			var participantsEmailAddressList = convo.Participants.Select(x => x.EmailAddress).Where(x => !string.IsNullOrEmpty(x)).Select(email => email.ToString()).ToList();
			var newParticipantEmailAddresses = new List<string>();
			AppendNewEmailAddresses(mailAsEDoc.Message.To, participantsEmailAddressList, newParticipantEmailAddresses);
			AppendNewEmailAddresses(mailAsEDoc.Message.Cc, participantsEmailAddressList, newParticipantEmailAddresses);

			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			foreach (var item in newParticipantEmailAddresses)
			{
				var contactQuery = new ZQuery(OrgContactSchema.OC_Email, item);
				contactQuery.AddToFilter(new ZQuery(OrgContactSchema.OC_IsActive, true));
				contactQuery.AddToFilter(new ZQuery(OrgContactSchema.OC_OH, incident.RelatedSupportIncident.IM_OH_Client));
				contactQuery.OrderBy = OrgContactSchema.Constants.OC_WebAccessEnabled + OrderByClause.Descending;

				var contacts = factory.Load<OrgContact>(contactQuery);

				if (contacts.Length > 0)
				{
					var contact = contacts[0];
					convo.RelatedParties.AddNewParticipant(contact);
				}
				else
				{
					convo.Participants.AddNewParticipant(item);
				}
			}
		}

		void AppendNewEmailAddresses(IList<InternetAddress> addressesToAdd, List<string> emailAddressesToSkip, List<string> emailAddressList)
		{
			foreach (var item in addressesToAdd)
			{
				if (item is MailboxAddress mailbox)
				{
					var mailAddress = mailbox.Address;
					if (EmailAddressValidation.IsEmailAddressValidAndNotEmpty(mailAddress) && !string.Equals(mailAddress, SupportIncidentLookups.SupportEmailAddress, StringComparison.OrdinalIgnoreCase) && !string.Equals(mailAddress, SupportIncidentLookups.EnterpriseProductionAddress, StringComparison.OrdinalIgnoreCase) && !emailAddressesToSkip.Contains(mailAddress, StringComparer.OrdinalIgnoreCase) && !emailAddressList.Contains(mailAddress, StringComparer.OrdinalIgnoreCase))
					{
						emailAddressList.Add(mailAddress);
					}
				}
			}
		}
	}
}
