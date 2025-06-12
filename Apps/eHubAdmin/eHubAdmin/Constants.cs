using System.Collections.Generic;
using System.Configuration;
using System.Web.WebPages.Html;

namespace eServices.eHubAdmin
{
	public static class Constants
	{
		public const string AdministratorGroups = @"CORPORATE\eHubAdministrators,WG\eHubAdministrators";
		public static readonly string[] AdministratorGroupsList = AdministratorGroups.Split(',');
		public static readonly int MaxMessagesInPage = int.TryParse(ConfigurationManager.AppSettings["MaxMessagesInPage"], out var result) ? result : 50;
		public static readonly string CW1_DOWNLOAD_MONITOR_ID = "CW1_DOWNLOAD";

		public static readonly IReadOnlyDictionary<string, string> ContollerDisplayName = new Dictionary<string, string>
		{
			{ "Messages", "Messages" },
			{ "RoutingRules", "Routing Rules" },
			{ "Air", "Air" },
			{ "Monitors", "Monitors" },
		};

		public static readonly IReadOnlyDictionary<string, string> ContollerDisplayNameSingular = new Dictionary<string, string>
		{
			{ "Messages", "Message" },
			{ "RoutingRules", "Routing Rule" },
			{ "Air", "Air" }
		};

		public static readonly IReadOnlyList<string> MessageStatusTypes = new List<string>
		{
			{ "Received" },
			{ "Processing" },
			{ "Not Processed" },
			{ "Processed" },
			{ "Sending" },
			{ "Not Delivered" },
			{ "Delivered" },
			{ "Error" }
		};

		public static readonly IReadOnlyList<string> DateRangeTypes = new List<string>
		{
			{ "Local" },
			{ "UTC" }
		};

		public static readonly IReadOnlyList<SelectListItem> PeriodOptions = new List<SelectListItem>
		{
			new SelectListItem {Text =  "Min(s)", Value = "Minute"},
			new SelectListItem {Text =  "Hour(s)", Value = "Hour"},
			new SelectListItem {Text =  "Day(s)", Value = "Day"},
			new SelectListItem {Text =  "Week(s)", Value = "Week"},
			new SelectListItem {Text =  "Month(s)", Value = "Month"}
		};

		public static readonly IReadOnlyList<int> NumDaysOptions = new List<int>
		{
			{ 1 },
			{ 2 },
			{ 3 },
			{ 4 },
			{ 5 },
			{ 6 },
			{ 7 },
			{ 8 },
			{ 9 },
			{ 10 }
		};

		public static readonly IReadOnlyList<int> NumWeeksOptions = new List<int>
		{
			{ 1 },
			{ 2 },
			{ 3 },
			{ 4 },
			{ 5 },
			{ 6 },
			{ 7 },
			{ 8 },
			{ 9 },
			{ 10 }
		};

		public static readonly IReadOnlyList<int> NumMonthsOptions = new List<int>
		{
			{ 1 },
			{ 2 },
			{ 3 },
			{ 4 },
			{ 5 },
			{ 6 },
			{ 7 },
			{ 8 },
			{ 9 },
			{ 10 },
			{ 11 },
			{ 12 }
		};

		public static readonly IReadOnlyDictionary<string, string> ApplicationCodeTypes = new Dictionary<string, string>
		{
			{ "AMS", "US Customs AMS Reporting" },
			{ "AMA", "US Customs Air AMS Reporting" },
			{ "BAT", "eHub Batching" },
			{ "BIZ", "BizTalk Interfaces" },
			{ "CAC", "Canadian Customs" },
			{ "CIM", "Air Messaging" },
			{ "CMR", "Australian Customs" },
			{ "CWS", "CustomsWare" },
			{ "MAN", "US Customs eManifest" },
			{ "NZC", "NZ Customs" },
			{ "SUB", "eHub Message Splitting" },

			{ "ACA", "AU Air Cargo" },
			{ "ACI", "CA ACI" },
			{ "ACO", "ACO Web Service" },
			{ "AIM", "US Air Import Message" },
			{ "ASY", "ASYCUDA Manifest" },
			{ "CAD", "CA Customs DIF" },
			{ "CAF", "CFIA Query" },
			{ "CAI", "CA Import" },
			{ "CAX", "CA Export" },
			{ "CDS", "GB Customs applications (including Customs Declaration Services)" },
			{ "CHM", "China's Interface Mapping" },
			{ "CI2", "CargoIMP Phase 2" },
			{ "CMD", "SG Customs CMD" },
			{ "CMG", "Container Management" },
			{ "CNC", "CNS code for eRouter only" },
			{ "CNS", "CNS text/XML status update processor" },
			{ "CTR", "ComTrac Container Request" },
			{ "CUK", "UK Cargo Community System (CCSUK)" },
			{ "CVC", "GB CDS via CCSUK" },
			{ "EDO", "E-IDO" },
			{ "ENE", "eNett" },
			{ "ERT", "E-router" },
			{ "ESC", "Spaninsh Customs" },
			{ "EXD", "AU ExDoc" },
			{ "FWA", "Forward Air" },
			{ "FWD", "Forwarder Edifact" },
			{ "GBE", "UK edifact messaging - processes responses (GBE)" },
			{ "GEI", "Global Electronic Invoice" },
			{ "GEP", "Global Electronic Payment" },
			{ "GMD", "Generic Message Delivery" },
			{ "HUB", "eHub" },
			{ "INM", "IN Consol Manifest" },
			{ "INT", "INTTRA" },
			{ "ITM", "IT Customs" },
			{ "MCP", "MCP code for eRouter only" },
			{ "MYC", "MY K4 K5" },
			{ "NCT", "EU New Computerised Transit System" },
			{ "NDM", "Native Data Messaging" },
			{ "NDQ", "Native Data Query" },
			{ "NES", "NES code for eRouter and documents only" },
			{ "NEX", "AU Customs NEXDOC" },
			{ "NOK", "Norken Web Service" },
			{ "NTP", "SG National Trade Platform" },
			{ "NZM", "NZ MPI eBACCa" },
			{ "PAT", "Port Authority" },
			{ "PHS", "UK MCP Port Health" },
			{ "PNT", "Pentant" },
			{ "PRA", "One Stop" },
			{ "R11", "UK MCP RRA11 - receives status updates (R11)" },
			{ "R12", "UK MCP RRA12 - receives status updates (R12)" },
			{ "S8C", "S8 Cargo" },
			{ "SCA", "AU Sea Cargo" },
			{ "SG4", "SG Customs Tradenet 4" },
			{ "SLE", "Shipping Line eHub Messaging" },
			{ "STW", "StowPlan" },
			{ "SYS", "Internal system message (ediProd)" },
			{ "TEL", "Telematics Interchange" },
			{ "TRX", "HK Traxon" },
			{ "TSE", "Temporary Storage" },
			{ "UAE", "UAE Customs" },
			{ "UDM", "Universal Data Messaging" },
			{ "UDQ", "Universal Data Query" },
			{ "UEM", "US Export Manifest Message" },
			{ "UNK", "Unknown application" },
			{ "URU", "UY Customs" },
			{ "USA", "US InBond" },
			{ "USD", "US Customs DIS" },
			{ "USE", "US Customs Export" },
			{ "USI", "US Customs Import" },
			{ "UXB", "US eBond" },
			{ "WDF", "Warehouse Docket" },
			{ "XDS", "Xml Data Service" },
			{ "XMS", "XML Message Service" },
			{ "ZAC", "ZA Customs" },
		};

		public static readonly IReadOnlyDictionary<string, string> MessageKeyTypes = new Dictionary<string, string>
		{
			{ "EI_PK", "EI_PK" },
			{ "OI_PK", "OI_PK" },
			{ "AM_PK", "AM_PK" },
			{ "MsgID", "MessageTrackingID" }
		};

		public static readonly IReadOnlyDictionary<string, string> LogLevels = new Dictionary<string, string>
		{
			{ "Info", "Info" },
			{ "Debug", "Debug" },
			{ "Trace", "Trace" },
		};

		public static readonly IReadOnlyDictionary<string, string> HeaderInfoDescriptions = new Dictionary<string, string>
		{
			{ "Received", "Message is received into the eHub’s Inbox from an external system (i.e. CW1, Third Party etc.)" },
			{ "Sent", "Message is processed and delivered to eHub’s Outbox, ready for an external system to receive it." },
		};

		public static readonly IReadOnlyDictionary<int, string> DayOfWeek = new Dictionary<int, string>
		{
			{0,""},
			{1, "Sun"},
			{2, "Mon"},
			{3, "Tue"},
			{4, "Wed"},
			{5, "Thu"},
			{6, "Fri"},
			{7, "Sat"},
		};

		public static readonly IReadOnlyDictionary<string, string> FunctionDescription = new Dictionary<string, string>
		{
			{"Any Role", "Enter the client ID for any role in the messages, including Inbox Recipient and Sender as well as Outbox Recipient and Sender."},
			{"Specific Role", "Enter the client ID for a specific role in the messages, as either the Inbox Recipient, Inbox Sender, Outbox Recipient or Outbox Sender, or a combination."},
			{"Status", "Specify the status for messages that must be returned in the query. All will indicate no restriction."},
			{"Application Code", "Specify the CargoWise One or eHub application code for messages that must be returned in the query. All will indicate no restriction."},
			{"Date Range", "Select this option if you want to specify the start and end dates for messages to be returned in the query."},
			{"Period", "Select this option if you want to specify a period, like 1 hour, for messages to be returned in the query. The initial default is 1 hour."},
			{"Key", "Select a key type and then enter the key value to search for specific message/s."},
			{"Search Location", "Select the database or databases to include in the query."},
			{"Waybill", "Enter the Air Waybill number."}
		};

		public static readonly IReadOnlyDictionary<string, string> EHubMessageStatusDescription = new Dictionary<string, string>
		{
			{"Inbox_255_Error", "An error occurred while processing the inbox message"},
			{"Inbox_0_Received", "No outbox messages"},
			{"Inbox_1_Processing", "Processing"},
			{"Inbox_2_Sending", "An outbox message is being sent"},
			{"Inbox_2_Processed", "Outbox messages are ready"},
			{"Inbox_3_Delivered", "All outbox messages have been delivered"},
			{"Inbox_3_Archived", "A delivered message that has been moved to the Archive database"},

			{"Outbox_255_Error", "An error occurred while processing the outbox message"},
			{"Outbox_0_SendQueue", "Send Queue"},
			{"Outbox_1_Sending", "Sending messages"},
			{"Outbox_3_Delivered", "All outbox messages have been delivered"}
		};

		public static readonly IReadOnlyDictionary<string, string> CargowiseOneMessageStatusDescription = new Dictionary<string, string>
		{
			{"Inbox_FAL", "An outgoing Interchange and all Messages are set to “FAL”, if: <br/><ul><li>The Recipient’s eHub Client ID is invalid</li><li>The Message Type is not supported by eHub</li><li>After 3 unsuccessful attempts to transmit an Interchange to eHub</li></li>"},
			{"Inbox_HPN", "After the Interchange including all Messages has been sent to eHub the status of the Interchange and all Messages will change to “HPN”."},
			{"Inbox_SNT", "When eHub confirms the successful transmission to the recipient, the status of the Interchange and all Messages will change to “SNT”."},

			{"Outbox_ERR", "If a critical error occurs when a Service Task tries to process a Message to create/update job data in CW1, the Message status is set to “ERR”."},
			{"Outbox_HPN", "This is the initial status of a Message unpacked from an Interchange while CW1 retrieves the Interchange from eHub. The status will remain “HPN” for all Messages until all Messages contained in an Interchange have been unpacked."},
			{"Outbox_QUE", "After all Messages have been unpacked from an Interchange, the status of all messages will change to “QUE”."},
			{"Outbox_RCV", "After a Service Task has successfully created/updated job data in CW1, the Message status is set to “RCV”."}
		};
	}
}
