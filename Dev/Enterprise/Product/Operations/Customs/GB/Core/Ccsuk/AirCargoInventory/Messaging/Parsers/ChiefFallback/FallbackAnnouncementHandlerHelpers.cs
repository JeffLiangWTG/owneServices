using System.Collections.Generic;
using System.Globalization;
using CargoWise.Types;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.ChiefFallback
{
	internal class FallbackMessage
	{
		public FallbackLineResponsiveness Responsiveness { get; set; }
		public FallbackLineAnnouncement Announcement { get; set; }
		public List<ZString> OtherDetails { get; set; }

		public string FormatNicely()
		{
			var responsivenessDatetime = ZDateTime.Empty;
			var announcementDatetime = ZDateTime.Empty;
			ZDateTime.TryParseExact(Announcement.DateOfAnnouncement + Announcement.TimeOfAnnouncement, out announcementDatetime, "ddMMyyyyHHmmss");
			ZDateTime.TryParseExact(Responsiveness.DateOfResponsiveness + Responsiveness.TimeOfResponsiveness, out responsivenessDatetime, "ddMMyyyyHHmmss");

			return string.Format(CultureInfo.InvariantCulture, @"
FALLBACK ANNOUNCEMENT
---------------------
Fallback for CHIEF {0} is being {1}
Time that CHIEF lost/gained responsiveness: {2}
Time of announcement by CCS-UK: {3}
Additional information: 
	{4}", Responsiveness.Direction, Responsiveness.Action,
		responsivenessDatetime.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture),
		announcementDatetime.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture),
		new ZStringBuilder(OtherDetails).ToStringWithDelimiterBetweenAppends("\r\n\t"));
		}
	}

	internal class FallbackLineResponsiveness
	{
		public string Direction { get; set; }
		public string Action { get; set; }
		public string DateOfResponsiveness { get; set; }
		public string TimeOfResponsiveness { get; set; }
	}

	internal class FallbackLineAnnouncement
	{
		public string DateOfAnnouncement { get; set; }
		public string TimeOfAnnouncement { get; set; }
	}

	internal static class Directions
	{
		public const string Import = "IMPORT";
		public const string Export = "EXPORT";
		public const string Both = "IMPORT AND EXPORT";
	}

	internal static class Actions
	{
		public const string Invoked = "INVOKED";
		public const string Revoked = "REVOKED";
	}

	internal static class SharedIdentifiers
	{
		public const string Fallback = "***FALLBACK***";
	}
}
