using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace CargoWise.Services.Calendar
{
	#region Reminder Types Enum

	public enum ReminderType
	{
		//Tentative,		// not yet working
		Confirmed,
		Cancellation
	}

	#endregion

	public abstract class ReminderBase
	{
		protected ReminderBase(string identifier, DateTimeKind dateTimeKind, ZDateTime fromDate, ZDateTime toDate, string subject, string body, string htmlBody = "", ITimeZone timeZoneOverride = null, BusinessObjectFactory factory = null)
		{
			this.ID = identifier;
			this.timeZoneOverride = timeZoneOverride;

			this.LocalDateFrom = (dateTimeKind == DateTimeKind.Utc) ? FromUtcToLocal(fromDate) : fromDate;
			this.LocalDateTo = (dateTimeKind == DateTimeKind.Utc) ? FromUtcToLocal(toDate) : toDate;

			//if start time and end time are equal, not empty, and the time component is zero then add 9 hours to both
			if (this.LocalDateFrom == this.LocalDateTo && !this.LocalDateFrom.IsEmpty && TimeIsNotSetOnDate(this.LocalDateFrom))
			{
				this.LocalDateFrom = this.LocalDateFrom.Date.AddHours(9);
				this.LocalDateTo = this.LocalDateTo.Date.AddHours(9);
			}

			if (!this.LocalDateTo.IsEmpty)
			{
				this.LocalDateTo = this.LocalDateFrom == this.LocalDateTo ? this.LocalDateTo.AddMinutes(1) : this.LocalDateTo;
			}

			this.UTCDateFrom = ConvertToUTC(LocalDateFrom);
			this.UTCDateTo = ConvertToUTC(LocalDateTo);

			this.Subject = subject;
			this.Body = body;
			this.HtmlBody = htmlBody;
			this.HasHtmlBody = !string.IsNullOrEmpty(htmlBody);
			this.Factory = factory;
		}

		protected ReminderBase(string identifier, DateTimeKind dateTimeKind, DateTime fromDate, TimeSpan duration, string subject, string body, string htmlBody = "")
			: this(identifier, dateTimeKind, fromDate, fromDate.Add(duration), subject, body, htmlBody)
		{
		}

		#region Properties

		#region Recipients

		/// <summary>
		/// The recipients (Name / Email) who should receive this reminder.
		/// If none are specified, the reminder is sent to the current user.
		/// </summary>
		public ReminderRecipientCollection Recipients
		{
			get
			{
				if (fRecipients == null)
				{
					fRecipients = new ReminderRecipientCollection();
				}

				return fRecipients;
			}
		}

		ReminderRecipientCollection fRecipients;

		#endregion

		#region ID

		/// <summary>
		/// Unique Identifier for this appointment. This will be used when deciding whether
		/// to create a new appointment, or update an existing appointment.
		/// </summary>
		public readonly string ID;

		#endregion

		#region Sequence

		/// <summary>
		/// Sequence number for this appointment. An appointment with the same ID and a later
		/// sequence number will trigger an existing appointment to be refreshed with new details.
		/// </summary>
		public uint Sequence { get; internal set; }

		protected internal abstract uint GenerateSequenceNumber();

		#endregion

		#region Date

		#region Alarm Period

		public TimeSpan AlarmPeriod
		{
			get { return fAlarmPeriod; }
			set { fAlarmPeriod = value; }
		}

		TimeSpan fAlarmPeriod = new TimeSpan(0, 15, 0);

		#endregion

		#region Date To / From

		public readonly ZDateTime LocalDateFrom;
		public readonly ZDateTime LocalDateTo;

		ZDateTime FromUtcToLocal(ZDateTime utcDateTime)
		{
			if (!utcDateTime.IsValid)
			{
				return utcDateTime;
			}

			return (timeZoneOverride != null) ?
				timeZoneOverride.ToLocalTime(utcDateTime.ToDateTime()) :
				Env.Time.GetLocalTimeFromUtc(utcDateTime.ToDateTime());
		}

		bool TimeIsNotSetOnDate(ZDateTime date)
		{
			return date.Hour == 0 && date.Minute == 0 && date.Second == 0 && date.Millisecond == 0;
		}

		#endregion

		#region UTC Dates

		readonly ITimeZone timeZoneOverride;

		ZDateTime ConvertToUTC(ZDateTime localDateTime)
		{
			DateTime result;

			if (!localDateTime.IsEmpty)
			{
				if (timeZoneOverride != null)
				{
					result = timeZoneOverride.ToUniversalTime(localDateTime.ToDateTime());
				}
				else
				{
					result = Env.Time.GetUtcFromLocalTime(localDateTime.ToDateTime());
				}
			}
			else
			{
				result = DateTime.MinValue;
			}

			return result;
		}

		/// <summary>
		/// The actual date, in UTC time, that this reminder should be set from.
		/// </summary>
		public readonly ZDateTime UTCDateFrom;

		/// <summary>
		/// The actual date, in UTC time, that this reminder should be set until.
		/// </summary>
		public readonly ZDateTime UTCDateTo;

		#endregion

		#endregion

		#region Subject & Body & Location

		public readonly string Subject;
		public readonly string Body;
		public readonly string HtmlBody;
		public string Location;
		public readonly bool HasHtmlBody;

		#endregion

		#region Reminder Type

		public ReminderType ReminderType
		{
			get { return fReminderType; }
			set { fReminderType = value; }
		}

		ReminderType fReminderType = ReminderType.Confirmed;

		#endregion

		#region Attachments

		public readonly AttachmentDefCollection Attachments = new AttachmentDefCollection();

		#endregion

		#endregion

		#region Create

		/// <summary>
		/// Push this appointment to the recipient.
		/// </summary>
		public virtual void CreateAppointment()
		{
			if (LocalDateFrom != DateTime.MinValue && LocalDateTo != DateTime.MinValue)
			{
				new VCalendarReminderCreator(this).CreateAppointment();
			}
		}

		protected internal virtual void OnAppointmentCreated()
		{
		}

		protected BusinessObjectFactory Factory { get; set; }

		#endregion
	}
}
