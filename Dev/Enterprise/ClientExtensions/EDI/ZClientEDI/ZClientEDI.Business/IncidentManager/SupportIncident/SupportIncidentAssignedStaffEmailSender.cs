using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class SupportIncidentAssignedStaffEmailSender
	{
		public SupportIncidentAssignedStaffEmailSender(SupportIncident incident)
		{
			Incident = incident;
		}

		protected readonly SupportIncident Incident;

		public virtual bool SendEmailToAssignedStaff(EmailDef email, bool save = true)
		{
			var unsendableLog = new List<string>();

			var daysToReceiveNotification = EDIDataRegistry.Instance.ClosedIncidentLastAssigneeNotificationPeriod.Value;

			if (Incident.IM_Category == SupportIncidentCategoriesList.Codes.Support)
			{
				GlbStaff supportStaff = null;
				if (Incident.WorkflowItems.Tasks.Count > 0 || Incident.IM_Status != SupportIncidentLookups.Status.Closed || Incident.IM_CloseTimeUtc >= ZDateTime.UtcNow.AddDays(-daysToReceiveNotification))
				{
					supportStaff = Incident.CustServiceContact;
				}
				var supportStaffDescription = Incident.WorkflowItems.Tasks.Count > 0 ? "Staff Assigned to Current Task" : "Assigned Customer Service Staff";
				var priorityCode = !Incident.IM_Priority.IsEmpty ? " (" + Incident.IM_Priority + ")" : "";

				if (TrySendEmailToStaff(email, supportStaff, supportStaffDescription, unsendableLog, save))
				{
					if (!CheckStaffIsLoggedIn(supportStaff))
					{
						SendStaffUnavailableEmailNotification(email, "User " + supportStaff.GS_FullName + " is unavailable for Customer Service" + priorityCode, "Staff Member " + supportStaff.GS_FullName + " is not logged in.", save);
					}
					else
					{
						var inactiveTimeout = GetInactivityTimeForCriticality();
						var lastActivityUtc = Incident.CustServiceContact.LastActivityUtc(inactiveTimeout);
						if (lastActivityUtc.IsEmpty)
						{
							SendStaffUnavailableEmailNotification(email, "User " + supportStaff.GS_FullName + " is unavailable for Customer Service" + priorityCode, "Staff Member " + supportStaff.GS_FullName + " has no activity in the last " + inactiveTimeout.ToString(CultureInfo.InvariantCulture) + " minutes.", save);
						}
					}

					return true;
				}
			}
			else
			{
				if (TrySendEmailToStaff(email, Incident.AssignedToCurrent, "Staff Assigned to Current Task", unsendableLog, save))
				{
					return true;
				}
			}

			if (Incident.IM_Status != SupportIncidentLookups.Status.Closed || Incident.IM_CloseTimeUtc >= ZDateTime.UtcNow.AddDays(-daysToReceiveNotification))
			{
				var lastClosedTask = GetLastClosedTaskWithEmailableStaff();
				var lastClosedTaskAssignedStaff = (lastClosedTask != null) ? lastClosedTask.AssignedStaffMember : null;
				if (TrySendEmailToStaff(email, lastClosedTaskAssignedStaff, "Staff Assigned to Last Closed Task", unsendableLog, save))
				{
					return true;
				}
			}

			if (TrySendEmailToStaff(email, Incident.ProductAreaAssignedStaff, "Incident Product Area Assignee", unsendableLog, save))
			{
				return true;
			}

			string emailSubject = (Incident.IM_Category == SupportIncidentCategoriesList.Codes.Support) ?
									"Incident Customer Service Staff is Unavailable" :
									"No Available Contact Assigned to Incident";
			return SendStaffUnavailableEmailNotification(email, emailSubject, unsendableLog, save);
		}

		static bool CheckStaffIsLoggedIn(GlbStaff staff)
		{
			var result = false;

			var query =
@"SELECT COUNT(*)
FROM
	dbo.StmServiceSemaphore WITH(READPAST, READCOMMITTEDLOCK)
	INNER JOIN dbo.StmServiceHeartBeat WITH(READPAST, READCOMMITTEDLOCK) ON SS_SV = SV_PK
WHERE
	(SS_LockInfo = 'EnterpriseActiveLogin' OR SS_LockInfo = 'EnterpriseWinzorActiveLogin')
	AND SV_ExpiresAtUtc > sysutcdatetime()
	AND SV_ParentId = @StaffPK";

			using (var command = Db.Connection.Command(query))
			{
				command.AddParameter("@StaffPK", SqlDbType.UniqueIdentifier, staff.PK.ToGuid());
				var rowCount = (int)command.ExecuteScalar();
				result = rowCount > 0;
			}

			return result;
		}

		protected int GetInactivityTimeForCriticality()
		{
			int result = 0;

			switch (Incident.IM_Priority)
			{
				case Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown:
					result = 15;
					break;

				case Core.Constants.CustomerService.CriticalityCodes.CR2_ModuleDown:
					result = 30;
					break;

				default:
					result = 60;
					break;
			}

			return result;
		}

		ProcessTask GetLastClosedTaskWithEmailableStaff()
		{
			return
				(from task in Incident.WorkflowItems.Tasks.Cast<ProcessTask>()
				 where task.P9_Status == ProcessTaskStatusCodeList.Codes.Closed
					 && IsEmailableStaff(task.AssignedStaffMember)
				 orderby task.P9_Sequence descending
				 select task).FirstOrDefault();
		}

		#region Send Email to Staff

		protected virtual bool TrySendEmailToStaff(EmailDef email, GlbStaff staff, string staffDescription, IList<string> unsendableLog, bool save)
		{
			if (AddStaffAsEmailRecipientIfSendable(email, staff, staffDescription, unsendableLog))
			{
				return SendEmailWithReasons(email, staffDescription, unsendableLog, save);
			}

			return false;
		}

		bool SendEmailWithReasons(EmailDef email, string recipientDescription, IList<string> unsendableLog, bool save)
		{
			if (unsendableLog.Count > 0)
			{
				var body = new ZStringBuilder(email.Body);
				body.AppendLine();
				body.AppendLine();
				body.AppendLine("----");
				body.AppendLine("You are receiving this Email because:");
				foreach (var reason in unsendableLog)
				{
					body.AppendLine(" - " + reason);
				}
				body.AppendLine(" - You are the " + recipientDescription + ".");

				email.Body = (email.ContentType == EmailContentTypes.HTML) ? body.ToString().Replace("\r\n", "<br />") : body.ToString();
			}

			try
			{
				if (save)
				{
					Env.OutgoingMailManager.CreateAndSave(email);
				}
				else
				{
					Env.OutgoingMailManager.Create(Incident.Factory, email);
				}
			}
			catch (EmailSendFailedException)
			{
				email.Recipients.Clear();
				unsendableLog.Add("Email could not be sent to " + recipientDescription + ".");
				return false;
			}

			return true;
		}

		protected bool AddStaffAsEmailRecipientIfSendable(EmailDef email, GlbStaff staff, string staffDescription, IList<string> unsendableLog)
		{
			var added = false;

			if (staff == null)
			{
				unsendableLog.Add("There is no " + staffDescription + ".");
			}
			else if (!staff.GS_IsActive)
			{
				unsendableLog.Add("The " + staffDescription + " is inactive.");
			}
			else if (staff.GS_EmailAddress.IsEmpty)
			{
				unsendableLog.Add("The " + staffDescription + " has no email address specified.");
			}
			else if (IsSystemUser(staff))
			{
				unsendableLog.Add("The " + staffDescription + " is a system user.");
			}
			else
			{
				email.AddRecipientForUserCommunication(staff.GS_EmailAddress);
				added = true;
			}

			return added;
		}

		bool IsEmailableStaff(GlbStaff staff)
		{
			return staff != null
				&& staff.GS_IsActive
				&& !staff.GS_EmailAddress.IsEmpty
				&& !IsSystemUser(staff);
		}

		readonly string[] systemUsersList = new string[] { User.ServiceUserCode, User.WebUserCode };
		bool IsSystemUser(GlbStaff staff)
		{
			return staff.GS_EmailAddress.EqualsIgnoringCase(SupportIncidentLookups.SupportEmailAddress) ||
				systemUsersList.Contains(staff.GS_Code.ToString(), StringComparer.OrdinalIgnoreCase);
		}

		#endregion

		#region Send Email to Support Mail Box

		bool SendStaffUnavailableEmailNotification(EmailDef email, string subject, string reason, bool save)
		{
			var reasons = new List<string>();
			reasons.Add(reason);
			return SendStaffUnavailableEmailNotification(email, subject, reasons, save);
		}

		protected bool SendStaffUnavailableEmailNotification(EmailDef email, string subject, IList<string> reasons, bool save)
		{
			if (!EDIDataRegistry.Instance.IncidentStaffUnavailableSupportNotificationEnable.Value)
			{
				return false;
			}

			var supportEmail = GetEmailToSupportMailBox(email, subject);

			var groupPk = EDIDataRegistry.Instance.IncidentStaffUnavailableSupportNotificationGroup.Value;
			if (groupPk != Guid.Empty)
			{
				var group = new BusinessObjectFactory().Load<GlbGroup>(groupPk);
				if (group != null)
				{
					foreach (GlbStaff staff in group.Staff)
					{
						if (!staff.GS_EmailAddress.IsEmpty)
						{
							supportEmail.AddRecipientForUserCommunication(staff.GS_EmailAddress);
						}
					}
				}
			}

			if (supportEmail.Recipients.Count == 0)
			{
				supportEmail.AddRecipientForUserCommunication(EDIDataRegistry.Instance.IncidentFromEmailAddress.Value);
			}

			return SendEmailWithReasons(supportEmail, "Fallback Recipient for incidents where the staff is unavailable", reasons, save);
		}

		protected virtual EmailDef GetEmailToSupportMailBox(EmailDef originalEmail, string subject)
		{
			var email = new EmailDef();
			email.ContentType = originalEmail.ContentType;
			email.FromDisplayName = originalEmail.FromDisplayName;
			email.FromAddress = originalEmail.FromAddress;
			email.Subject = subject;
			email.Body = originalEmail.Body;
			email.Headers = originalEmail.Headers.ToDictionary(x => x.Key, x => x.Value);
			return email;
		}

		#endregion
	}
}
