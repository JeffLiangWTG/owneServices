using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Enterprise.MailManager.MailFilters
{
	public static class MailFilterCodes
	{
		public const string EIDOMessageRetriever = "EID";
		public const string CMMMessage = "CMM";
		public const string CMDMessage = "CMD";
		public const string GbPentantEmails = "PET";
		public const string GbMcpMiscTextAndEmails = "MTI";
		public const string GbMcpRra12 = "RRA";
		public const string GbMcpRra11AndRra06AndRra01 = "MCP";
		public const string GbMcpPHS = "PHS";
		public const string GbCNS = "CNS";
		public const string GbChiefNesEmail = "NER";
		public const string AUCInterchange = "AUI";
		public const string DocumentImportManager = "DMI";
		public const string LocalCartageBooking = "LCB";
		public const string WooliesDataImporter = "WOW";
		public const string TELConsoleShipment = "ZT1";
		public const string MailProcessingTask = "MAP";
		public const string ESImportMailTask = "ESM";

		public static IEnumerable<string> All
			=> typeof(MailFilterCodes).GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => f.GetValue(null)).OfType<string>();
	}
}
