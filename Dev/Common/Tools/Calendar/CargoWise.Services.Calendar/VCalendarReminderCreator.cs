using System;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.BrandManager;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace CargoWise.Services.Calendar
{
	/// <summary>
	/// Creates vCalendar compliant email messages for calendar reminder integration.
	/// Created according to RFC 2445.
	/// 
	/// See following links for more information:
	///		http://www.ietf.org/rfc/rfc2445.txt
	///		http://www.kanzaki.com/docs/ical/
	/// </summary>
	public class VCalendarReminderCreator
	{
		public VCalendarReminderCreator(ReminderBase reminder)
		{
			this.reminder = reminder;
			reminder.Sequence = reminder.GenerateSequenceNumber();
		}

		readonly ReminderBase reminder;

		public virtual void CreateAppointment()
		{
			if (Env.Registry.CalendarIntegration)
			{
				if (reminder.Recipients.Count == 0)
				{
					if (!string.IsNullOrEmpty(Env.CurrentUser.EmailAddress))
					{
						reminder.Recipients.Add(Env.CurrentUser.FullName, Env.CurrentUser.EmailAddress);
					}
					else
					{
						return; // No Recipients
					}
				}

				if (Env.Registry.CalendarInvitationSolutionForOrganisers)
				{
					bool hasOrganiserAsRecipient = HasOrganiserAsRecipient();
					if (hasOrganiserAsRecipient)
					{
						EmailDef emailToOrganiser = GetEmailWithAttachedCalendar(true);
						emailToOrganiser.AddRecipientForUserCommunication(CurrentUserEmailAddress);
						Env.OutgoingMailManager.CreateAndSave(emailToOrganiser);
					}

					if ((hasOrganiserAsRecipient && reminder.Recipients.Count > 1) || (!hasOrganiserAsRecipient && reminder.Recipients.Count > 0))
					{
						EmailDef emailToOtherUsers = GetEmailWithAttachedCalendar(false);
						foreach (ReminderRecipient recipient in reminder.Recipients)
						{
							if (hasOrganiserAsRecipient && IsOrganiser(recipient))
							{
								continue;
							}
							emailToOtherUsers.AddRecipientForUserCommunication(recipient.Email);
						}
						if (emailToOtherUsers.Recipients.Count > 0)
						{
							Env.OutgoingMailManager.CreateAndSave(emailToOtherUsers);
						}
					}
				}
				else
				{
					EmailDef email = GetEmailWithAttachedCalendar(false);
					foreach (ReminderRecipient recipient in reminder.Recipients)
					{
						email.AddRecipientForUserCommunication(recipient.Email);
					}
					Env.OutgoingMailManager.CreateAndSave(email);
				}

				reminder.OnAppointmentCreated();
			}
		}

		EmailDef GetEmailWithAttachedCalendar(bool isForOrganiser)
		{
			EmailDef email = new EmailDef
			{
				ContentType = EmailContentTypes.Calendar,
				FromAddress = isForOrganiser ? SystemEmailAddress : CurrentUserEmailAddress,
				FromDisplayName = isForOrganiser ? CurrentCompanyName : CurrentUserName,
				Subject = reminder.Subject,
				Body = GetVCalendarMessage(isForOrganiser)
			};

			foreach (var attachment in reminder.Attachments.Cast<AttachmentDef>())
			{
				email.Attachments.Add(attachment);
			}

			return email;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "vCalendar Message Text")]
		string GetVCalendarMessage(bool isForOrganiser)
		{
			string attendeesString = GetAttendeeString();
			string vCal = "BEGIN:VCALENDAR\r" +
							"METHOD:{METHOD}\r" +
							"PRODID:{PRODUCTNAME}\r" +
							"VERSION:2.0\r" +
							"BEGIN:VEVENT\r" +
							"DTSTAMP:{DTSTAMP}\r" +
							"DTSTART:{DTSTART}\r" +
							"SUMMARY:{SUMMARY}\r" +
							"UID:{UID}\r" +
							"SEQUENCE:{SEQUENCE}\r" +
							"{ATTENDEES}" +
							"ORGANIZER;CN=\"{ORGANIZER}\":MAILTO:{ORGANIZEREMAIL}\r" +
							"LOCATION:{LOCATION}\r" +
							"DTEND:{DTEND}\r" +
							"DESCRIPTION:{DESCRIPTION}\r" +
							"X-ALT-DESC;FMTTYPE=text/html:{HTML_DESCRIPTION}\r" +
							"BEGIN:VALARM\r" +
							"ACTION:DISPLAY\r" +
							"DESCRIPTION:REMINDER\r" +
							"TRIGGER;RELATED=START:-PT{ALARMHOURS}H{ALARMMINUTES}M00S\r" +
							"END:VALARM\r" +
							"TRANSP:{TRANSP}\r" +
							"{CancelReminder}" +
							"END:VEVENT\r" +
							"END:VCALENDAR";

			var now = ZDateTime.Now;

			vCal = vCal.Replace("{PRODUCTNAME}", BrandingFactory.Instance.ProductName);
			vCal = vCal.Replace("{METHOD}", reminder.ReminderType == ReminderType.Cancellation ? "CANCEL" : "REQUEST");
			vCal = vCal.Replace("{DTSTAMP}", ConvertTimeToCalendarText(now, now));
			vCal = vCal.Replace("{DTSTART}", ConvertTimeToCalendarText(reminder.UTCDateFrom, now));
			vCal = vCal.Replace("{SUMMARY}", reminder.Subject);
			vCal = vCal.Replace("{UID}", reminder.ID);
			vCal = vCal.Replace("{SEQUENCE}", reminder.Sequence.ToString());
			vCal = vCal.Replace("{ATTENDEES}", attendeesString);
			vCal = vCal.Replace("{ORGANIZER}", isForOrganiser ? CurrentCompanyName : CurrentUserName);
			vCal = vCal.Replace("{ORGANIZEREMAIL}", isForOrganiser ? SystemEmailAddress : CurrentUserEmailAddress);
			vCal = vCal.Replace("{LOCATION}", reminder.Location);
			vCal = vCal.Replace("{DTEND}", ConvertTimeToCalendarText(reminder.UTCDateTo, now));
			vCal = vCal.Replace("{DESCRIPTION}", reminder.Body.Replace(Environment.NewLine, "\\N"));
			vCal = vCal.Replace("{ALARMHOURS}", reminder.AlarmPeriod.Hours.ToString().PadLeft(2, '0'));
			vCal = vCal.Replace("{ALARMMINUTES}", reminder.AlarmPeriod.Minutes.ToString().PadLeft(2, '0'));
			vCal = vCal.Replace("{TRANSP}", "OPAQUE");
			vCal = vCal.Replace("{CancelReminder}", reminder.ReminderType == ReminderType.Cancellation ? "STATUS:CANCELLED\r" : "");

			if (reminder.HasHtmlBody)
			{
				vCal = vCal.Replace("{HTML_DESCRIPTION}", reminder.HtmlBody.Replace(Environment.NewLine, "<BR />"));
			}
			else
			{
				vCal = vCal.Replace("X-ALT-DESC;FMTTYPE=text/html:{HTML_DESCRIPTION}\r", "");
			}

			return FormatAndWrapLines(vCal);
		}

		static string ConvertTimeToCalendarText(ZDateTime time, ZDateTime now)
		{
			const string format = "yyyyMMdd\\THHmmss\\Z";
			if (time.IsValid)
			{
				return time.ToString(format, CultureInfo.InvariantCulture);
			}
			else
			{
				return now.ToString(format, CultureInfo.InvariantCulture);
			}
		}

		#region Implementation

		string GetAttendeeString()
		{
			string result = string.Empty;
			foreach (ReminderRecipient recipient in reminder.Recipients)
			{
				result += "ATTENDEE;";
				result += "ROLE=REQ-PARTICIPANT;";
				result += "PARTSTAT=NEEDS-ACTION;";
				result += "RSVP=" + (IsOrganiser(recipient) ? "FALSE" : "TRUE") + ";";
				result += "CN=\"" + recipient.Email + "\":MAILTO:" + recipient.Email + "\r";
			}

			return result;
		}

		bool IsOrganiser(ReminderRecipient recipient)
		{
			return recipient.Email == CurrentUserEmailAddress && recipient.Name == CurrentUserName;
		}

		bool HasOrganiserAsRecipient()
		{
			foreach (ReminderRecipient recipient in reminder.Recipients)
			{
				if (IsOrganiser(recipient))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// RFC specifies lines must be wrapped at 75 chars, with the wrapped lines starting with a single blank space 
		/// </summary>
		string FormatAndWrapLines(string vCalMessage)
		{
			StringBuilder result = new StringBuilder();

			string[] lines = vCalMessage.Split('\r');
			for (int i = 0; i < lines.Length; i++)
			{
				string line = lines[i];
				while (true)
				{
					if (line.Length <= 75)
					{
						result.Append(line);
						if (i < lines.Length - 1)
						{
							result.Append(Environment.NewLine);
						}
						break;
					}
					else
					{
						result.Append(line.Substring(0, 75) + Environment.NewLine);
						line = " " + line.Substring(75);
					}
				}
			}

			return result.ToString();
		}

		string CurrentUserEmailAddress => currentUserEmailAddress ?? (currentUserEmailAddress = new EmailDef(Env.CurrentUser.PK).FromAddress);
		string currentUserEmailAddress;

		string CurrentUserName => Env.CurrentUser.FullName;

		string SystemEmailAddress => Env.Instance.Registry.EnterpriseMailboxEmailAddress;

		string CurrentCompanyName => Env.CurrentCompany.Name;

		#endregion
	}
}
