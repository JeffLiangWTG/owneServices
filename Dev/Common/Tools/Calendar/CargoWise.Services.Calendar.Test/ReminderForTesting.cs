using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace CargoWise.Services.Calendar.Testing
{
	sealed class ReminderForTesting : ReminderBase
	{
		internal ReminderForTesting(string identifier, DateTimeKind dateTimeKind, ZDateTime from, ZDateTime to, string subject = "", string body = "", string htmlBody = "", ITimeZone timeZoneOverride = null)
			: base(identifier, dateTimeKind, from, to, subject, body, htmlBody, timeZoneOverride)
		{
		}

		internal ReminderForTesting(string identifier, DateTimeKind dateTimeKind, DateTime from, TimeSpan duration, string subject = "", string body = "", string htmlBody = "")
			: base(identifier, dateTimeKind, from, duration, subject, body, htmlBody)
		{
		}

		internal ReminderForTesting(string identifier)
			: base(identifier, DateTimeKind.Local, DateTime.Now, DateTime.Now, "", "", "")
		{
		}

		public override void CreateAppointment()
		{
			var creator = new VCalendarReminderCreator(this);
			creator.CreateAppointment();
		}

		protected internal override uint GenerateSequenceNumber()
		{
			return 123u;
		}
	}
}
