using System;
using System.Collections.Specialized;
using System.Text;
using CargoWise.Common;
using Enterprise.eHubMessaging.Business.Extensions;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Email.Html;

namespace Enterprise.eHubMessaging.Business
{
	public class eServicesHealthCheckHtmlEmailBuilder
	{
		public eServicesHealthCheckHtmlEmailBuilder(GuidRegistryItem notificationGroup, string serviceTaskName, string healthCheckName)
			: this(notificationGroup, Guid.Empty, serviceTaskName, healthCheckName)
		{ }

		public eServicesHealthCheckHtmlEmailBuilder(GuidRegistryItem notificationGroup, Guid companyGuid, string serviceTaskName, string healthCheckName)
		{
			htmlEmail = new HtmlEmailDef();
			Recipients = PrepareRecipients(Argument.NotNull(notificationGroup, "notificationGroup"), companyGuid);
			if (Recipients.Count == 0)
			{
				throw new EmailHasNoRecipientsException(Res.GetString("c3b596e7-5818-48a8-a1a6-5a654ed7eca0", "The notification group specified contains no valid email recipients. To specify a different group go to {0}.", Utils.GetFullPath(notificationGroup)));
			}

			htmlEmail.AddRecipientForUserCommunication(Recipients);
			SetHeader(Argument.NotNullOrEmpty(serviceTaskName, "serviceTaskName"), Argument.NotNullOrEmpty(healthCheckName, "healthCheckName"));
		}

		public HtmlEmailDef GetResult()
		{
			return htmlEmail;
		}

		public void SetSubject(string subject)
		{
			htmlEmail.Subject = subject;
		}

		public void SetBody(string customMessages, GlbCompany company = null)
		{
			var body = new StringBuilder();
			body.Append(Header);
			body.Append(customMessages);
			body.Append(noteBuilder);
			body.Append(Footer);
			IDisposable tempEnvironment;
			if (company != null && (tempEnvironment = DisposableEnvironment.ForCompany(company.GC_Code, reportInactive: false)) != null)
			{
				using (tempEnvironment)
				{
					htmlEmail.LoadHtmlUsingTemplate(body.ToString());
				}
			}
			else
			{
				htmlEmail.LoadHtmlUsingTemplate(body.ToString(), null, Guid.Empty, Guid.Empty, Guid.Empty);
			}
		}

		#region private methods

		void SetHeader(string serviceTaskName, string healthCheckName)
		{
			Header = Res.GetString("a7c94079-6413-47e5-9026-b1e32609ca6e", "{2}{0} Health Check report for {1}:{3}", healthCheckName, serviceTaskName, EmailHtmlTags.BIG_Start + EmailHtmlTags.B_Start, EmailHtmlTags.B_End + EmailHtmlTags.BIG_End + EmailHtmlTags.BR);
		}

		static StringCollection TryGetAllStaffEmail()
		{
			return new EmailGroupUtility().GetStaffEmailCollection(false);
		}

		static StringCollection TryGetSystemNotificationEmails()
		{
			return new EmailGroupUtility().GetCompanyNotificationGroupEmails();
		}

		static StringCollection TryGetEHubNotificationEmails(GuidRegistryItem notificationGroup, Guid targetGuid)
		{
			return new EmailGroupUtility().GetGroupEmailCollection(notificationGroup.GetFallBackValueAtAllLevels(targetGuid, Guid.Empty, Guid.Empty), false);
		}

		StringCollection PrepareRecipients(GuidRegistryItem notificationGroup, Guid companyGuid)
		{
			noteBuilder.Append(EmailHtmlTags.BR + EmailHtmlTags.BR + EmailHtmlTags.B_Start);
			noteBuilder.Append(Res.GetString("5dc78b8c-419d-4571-89e7-a06bce65ef50", "Notes"));
			noteBuilder.Append(EmailHtmlTags.B_End + EmailHtmlTags.BR);
			noteBuilder.Append(Res.GetString("7add56be-2320-494a-8a9d-a192a3800967", "You are receiving this Email because:"));
			noteBuilder.Append(EmailHtmlTags.BR + EmailHtmlTags.UL_Start);
			var recipients = PrepareGroupRecipient(noteBuilder, notificationGroup, companyGuid);
			noteBuilder.Append(EmailHtmlTags.UL_End);
			noteBuilder.Append(Res.GetString("0f7e4e5d-a48c-4177-b467-c8968769aa8d", "If you are not supposed to receive this email, please contact your system administrator to change the corresponding notification group setting"));
			noteBuilder.Append(EmailHtmlTags.BR);
			noteBuilder.Append(Utils.GetFullPath(notificationGroup));
			return recipients;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		static StringCollection PrepareGroupRecipient(StringBuilder bodyBuilder, GuidRegistryItem notificationGroup, Guid companyGuid)
		{
			var groupEmails = new StringCollection();
			if (companyGuid != Guid.Empty)
			{
				groupEmails.MergeStringCollection(TryGetEHubNotificationEmails(notificationGroup, companyGuid));
				if (groupEmails.Count > 0)
				{
					bodyBuilder.Append(EmailHtmlTags.LI_Start).Append(Res.GetString("ff7c5ccd-04cd-43ac-85be-8e98165b8812", "You are listed as a member of the eHub Error Notification Group for this company")).Append(EmailHtmlTags.LI_End);
					return groupEmails;
				}
			}

			groupEmails.MergeStringCollection(TryGetEHubNotificationEmails(notificationGroup, Guid.Empty));
			if (groupEmails.Count > 0)
			{
				bodyBuilder.Append(EmailHtmlTags.LI_Start).Append(Res.GetString("f86851ec-f91a-4009-a2d8-34187cfd6d71", "You are listed as a member of the eHub Error Notification Group for this {0} System", "CargoWise")).Append(EmailHtmlTags.LI_End);
				return groupEmails;
			}

			groupEmails.MergeStringCollection(TryGetSystemNotificationEmails());
			if (groupEmails.Count > 0)
			{
				bodyBuilder.Append(EmailHtmlTags.LI_Start).Append(Res.GetString("34607803-ecb4-4899-bc51-d5470a5ee291", "You are listed as a member of the System Notification Group ({0})", Utils.GetFullPath(RawDataRegistry.Instance.NotificationGroup))).Append(EmailHtmlTags.LI_End);
				return groupEmails;
			}

			groupEmails.MergeStringCollection(TryGetAllStaffEmail());
			if (groupEmails.Count > 0)
			{
				bodyBuilder.Append(EmailHtmlTags.LI_Start).Append(Res.GetString("1e3bee91-8c69-4eb7-aec3-279198621239", "This notification are sending to all the staff listed in the {0} System", "CargoWise")).Append(EmailHtmlTags.LI_End);
				return groupEmails;
			}

			return groupEmails;
		}

		#endregion

		string Header { get; set; }
		string Footer { get; set; }
		StringCollection Recipients { get; set; }

		readonly HtmlEmailDef htmlEmail;
		readonly StringBuilder noteBuilder = new StringBuilder();
	}
}
