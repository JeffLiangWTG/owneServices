using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.Accounting.Business.EmailNotification
{
	public abstract class TransactionApprovalRequestEmail
	{
		public TransactionApprovalRequestEmail(GenApprovalRequest approvalRequest)
		{
			ApprovalRequest = approvalRequest;
			ApprovalStatus = ApprovalRequest.Lookups.ApprovalStatusList.GetDescriptionFromCode(ApprovalRequest.XP_ApprovalStatus);
		}

		public TransactionRequestApprovalHtmlEmailDef CreateEmailDef()
		{
			var email = new TransactionRequestApprovalHtmlEmailDef();
			email.Subject = GetSubject();
			email.LoadHtmlUsingTemplate(GetBody());
			AddRecipients(email);

			return email;
		}

		public EmailSendResult Send()
		{
			var result = EmailSendResult.Unsuccessful;

			var email = CreateEmailDef();
			try
			{
				Env.OutgoingMailManager.CreateAndSave(email);
				result = EmailSendResult.Successful;
			}
			catch (EmailHasNoRecipientsException)
			{
			}
			catch (EmailNotCompleteException)
			{
			}
			catch (EmailSendFailedException)
			{
			}

			return result;
		}

		protected readonly GenApprovalRequest ApprovalRequest;
		protected readonly string ApprovalStatus;

		protected abstract string GetApprovalBizoName();
		protected abstract string GetRequestID();
		protected virtual string GetLinkForMoreDetails() => string.Empty;
		protected virtual GuidRegistryItem NotificationGroupForRequestedApproval() => null;

		protected BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		void AddRecipients(EmailDef email)
		{
			if (ApprovalRequest.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Requested)
			{
				var groupRegItem = NotificationGroupForRequestedApproval();
				if (groupRegItem != null)
				{
					email.AddGroupOfRecipientsOrAllUsersIfGroupIsEmpty(groupRegItem.Value, groupRegItem);
				}
			}
			else
			{
				var staff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, ApprovalRequest.XP_SystemCreateUser);
				if (staff != null && !staff.GS_EmailAddress.IsEmpty)
				{
					email.AddRecipientForUserCommunication(staff.GS_EmailAddress);
				}
			}
		}

		protected virtual string GetBody()
		{
			string link = GetLinkForMoreDetails();
			GlbStaff approvedStaff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, ApprovalRequest.XP_GS_NKApprovingUser1);

			var builder = new ZStringBuilder(Invariant($"<p>{GetApprovalBizoName()} approval request with description '{ApprovalRequest.XP_ReasonDescription}' was {ApprovalStatus} by user '{approvedStaff?.GS_FullName ?? string.Empty}'.</p>"));
			if (!string.IsNullOrWhiteSpace(link))
			{
				builder.Append(link);
			}

			return builder.ToStringWithNewLineBetweenAppends();
		}

		protected virtual string GetSubject()
		{
			return Invariant($"{GetApprovalBizoName()} approval request {GetRequestID()} was {ApprovalStatus}");
		}
	}
}
