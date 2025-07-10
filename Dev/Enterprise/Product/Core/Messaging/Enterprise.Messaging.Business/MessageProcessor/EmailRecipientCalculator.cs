using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Messaging.Business
{
	public class EmailRecipientCalculator
	{
		readonly ZString[] calculatedUserToNotifyEmailAddresses = System.Array.Empty<ZString>();
		readonly ZGuid calculatedGroupPK = ZGuid.Empty;
		readonly ZBool emailRedirected = false;

		protected virtual bool IsSystemCommunication => true;
		protected virtual IOutgoingMailManager OutgoingMailManager => Env.OutgoingCustomsMailManager;

		public EmailRecipientCalculator(ZString sendMode, ZGuid groupSendPK, GlbStaff userToNotify, ZGuid alternativeGroupPKIfNoRecipientFound)
			: this(sendMode, groupSendPK, userToNotify != null ? userToNotify.GS_EmailAddress : ZString.Empty, alternativeGroupPKIfNoRecipientFound)
		{
		}

		public EmailRecipientCalculator(ZString sendMode, ZGuid groupSendPK, ZString userToNotifyEmail, ZGuid alternativeGroupPKIfNoRecipientFound)
			: this(sendMode, groupSendPK, new ZString[] { userToNotifyEmail }, alternativeGroupPKIfNoRecipientFound)
		{
		}

		public EmailRecipientCalculator(ZString sendMode, ZGuid groupSendPK, ZString[] userToNotifyEmailAddresses, ZGuid alternativeGroupPKIfNoRecipientFound)
		{
			if (sendMode == Core.Constants.EmailTo.StaffMember
				|| sendMode == Core.Constants.EmailTo.StaffMemberAndNominatedGroup
				|| sendMode == GroupNotification.StaffMemberOrNominatedGroup)
			{
				this.calculatedUserToNotifyEmailAddresses = userToNotifyEmailAddresses;
			}

			if (sendMode == Core.Constants.EmailTo.NominatedGroup
				|| sendMode == Core.Constants.EmailTo.StaffMemberAndNominatedGroup
				|| (sendMode == GroupNotification.StaffMemberOrNominatedGroup && !HasValidEmailAddressInUserToNotifyEmailAddressesList))
			{
				this.calculatedGroupPK = groupSendPK;
			}

			if (!HasValidEmailAddressInUserToNotifyEmailAddressesList && !alternativeGroupPKIfNoRecipientFound.IsEmpty && sendMode == Core.Constants.EmailTo.StaffMember)
			{
				this.calculatedGroupPK = alternativeGroupPKIfNoRecipientFound;
				this.emailRedirected = true;
			}
		}

		ZBool HasValidEmailAddressInUserToNotifyEmailAddressesList => calculatedUserToNotifyEmailAddresses != null && calculatedUserToNotifyEmailAddresses.Any(address => !address.IsEmpty);

		public ZBool EmailRedirected
		{
			get { return emailRedirected; }
		}

		protected virtual List<string> GetEmailAddressesFromNominatedGroup(ZGuid groupPK)
		{
			var emailAddressList = new List<string>();
			var factory = new BusinessObjectFactory() { NameForDebugging = "Email Recipient Calculator Factory" };
			var staffEmailsInGroup = new DynamicBusinessObjectCollection(factory);
			staffEmailsInGroup.Load(@"
SELECT DISTINCT GS_EmailAddress
FROM dbo.GlbStaff
JOIN dbo.GlbGroupLink ON GK_GS = GS_PK AND GK_GG = @groupPK
WHERE GS_IsActive = 1
AND GS_EmailAddress <> ''", new[] { ZSqlParameter.New("@groupPK", groupPK, GlbGroupLinkSchema.GK_GG) });

			var emailGroupUtility = new EmailGroupUtility();
			foreach (var emailAddress in staffEmailsInGroup.Select(x => x["GS_EmailAddress"].ToString()))
			{
				if (!emailGroupUtility.IsHostNotificationEmail(emailAddress) && !emailAddressList.Contains(emailAddress))
				{
					emailAddressList.Add(emailAddress);
				}
			}

			return emailAddressList;
		}

		public void SendNotifications(BusinessObjectFactory factory, EmailDef email, IRegistryItem groupRegistryItem)
		{
			if (calculatedUserToNotifyEmailAddresses != null)
			{
				foreach (var userToNotifyEmailAddress in calculatedUserToNotifyEmailAddresses)
				{
					if (!userToNotifyEmailAddress.IsEmpty && !email.Recipients.Contains(userToNotifyEmailAddress))
					{
						AddRecipient(email, userToNotifyEmailAddress);
					}
				}
			}

			if (groupRegistryItem != null)
			{
				foreach (var emailFromGroup in GetEmailAddressesFromNominatedGroup(calculatedGroupPK))
				{
					if (!email.Recipients.Contains(emailFromGroup))
					{
						AddRecipient(email, emailFromGroup);
					}
				}

				if (emailRedirected && !calculatedGroupPK.IsEmpty)
				{
					email.Body += System.Environment.NewLine + Res.GetString("{A29C805A-D20B-4F32-BCE6-678E1314C977}", "(This email was redirected to the {0} group because the original email was sent to staff member who is not active now.)", groupRegistryItem.Caption);
				}
			}

			if (email.Recipients.Count > 0)
			{
				try
				{
					if (factory != null)
					{
						OutgoingMailManager.Create(factory, email);
					}
					else
					{
						OutgoingMailManager.CreateAndSave(email);
					}
				}
				catch (EmailHasNoFromAddressException)
				{
				}
			}
		}

		void AddRecipient(EmailDef email, ZString emailAddress)
		{
			if (IsSystemCommunication)
			{
				email.AddRecipientForSystemCommunication(emailAddress);
			}
			else
			{
				email.AddRecipientForUserCommunication(emailAddress);
			}
		}
	}
}
