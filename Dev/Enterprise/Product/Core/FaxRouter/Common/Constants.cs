using System;
using System.Configuration;

namespace Enterprise.FaxRouter
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "The class below is inherited in multiple places, so this class cannot be static. Therefore adding the below suppress message related to static type.")]
	public class Constants
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		public const String LONGDATEFORMAT = "dd-MM-yyyy HH:mm:ss";
		public const String SQLDATEFORMAT = "yyyyMMdd HH:mm:ss";

		public static int DAILY_REPORT_DISCREPANCY_THRESHOLD = int.Parse(ConfigurationSettings.AppSettings["DAILY_REPORT_DISCREPANCY_THRESHOLD"]);
		public static string BLACKLIST = ConfigurationSettings.AppSettings["BLACKLIST"];
		public static String ENTERPRISE_DATABASE_NAME = ConfigurationSettings.AppSettings["ENTERPRISE_DATABASE_NAME"];
		public static String ENTERPRISE_SERVER_NAME = ConfigurationSettings.AppSettings["ENTERPRISE_SERVER_NAME"];
		public static String FAX_GATEWAY_SYSID = ConfigurationSettings.AppSettings["FAX_GATEWAY_SYSID"];
		public static String FAX_GATEWAY_EMAIL = ConfigurationSettings.AppSettings["FAX_GATEWAY_EMAIL"];
		public static String NOREPLY_EMAIL = ConfigurationSettings.AppSettings["NOREPLY_EMAIL"];
		public static String FAX_ADMINISTRATOR_EMAIL = ConfigurationSettings.AppSettings["FAX_GATEWAY_ADMINISTRATOR_EMAIL"];
		public static String EDI_FAXJOB_SMTP_SERVER = ConfigurationSettings.AppSettings["EDI_FAXJOB_SMTP_SERVER"];
		public static String EDI_ACK_SMTP_SERVER = ConfigurationSettings.AppSettings["EDI_ACK_SMTP_SERVER"];
		public static String FAX_HANDLER_EMAIL = ConfigurationSettings.AppSettings["FAX_HANDLER_EMAIL"];
		public static String FAX_ACK_RETURN_EMAIL = ConfigurationSettings.AppSettings["FAX_ACK_RETURN_EMAIL"];
		public static String FAX_ACK_SENDER = ConfigurationSettings.AppSettings["FAX_ACK_SENDER"];
		public static String FAX_GATEWAY_TEMP_FILE_DIRECTORY = ConfigurationSettings.AppSettings["FAX_GATEWAY_TEMP_FILE_DIRECTORY"];
		public static bool AUTO_START = ConfigurationSettings.AppSettings["AUTO_START"].Equals("1");
		public static bool EDI_SEND_FAX_ACK_EMAIL_ON_SUCCESS = ConfigurationSettings.AppSettings["EDI_SEND_FAX_ACK_EMAIL_ON_SUCCESS"] == "1";
		public static bool EDI_SEND_FAX_ACK_EMAIL_ON_FAILURE = ConfigurationSettings.AppSettings["EDI_SEND_FAX_ACK_EMAIL_ON_FAILURE"] == "1";
		public static bool EDI_SEND_FAX_ACK_EMAIL_ON_OVERDUE = ConfigurationSettings.AppSettings["EDI_SEND_FAX_ACK_EMAIL_ON_OVERDUE"] == "1";
		public static bool FAX_ACK_IS_HUMAN = ConfigurationSettings.AppSettings["FAX_ACK_IS_HUMAN"] == "1";
		public static bool IS_FAX_ACK_ON = ConfigurationSettings.AppSettings["IS_FAX_ACK_ON"] == "1";
		public static int FAX_GATEWAY_POLLING_INTERVAL = Int32.Parse(ConfigurationSettings.AppSettings["FAX_GATEWAY_POLLING_INTERVAL"]);
		public static int FAX_VIEWER_PAGE_HIEGHT = Int32.Parse(ConfigurationSettings.AppSettings["FAX_VIEWER_PAGE_HIEGHT"]);
		public static int FAX_VIEWER_PAGE_WIDTH = Int32.Parse(ConfigurationSettings.AppSettings["FAX_VIEWER_PAGE_WIDTH"]);
		public static String MAILDB_CONNECTION = ConfigurationSettings.AppSettings["MailDBConnectionString"];

		public static String EDIFAXDB_CONNECTION = ConfigurationSettings.AppSettings["EDIFaxDBConnectionString"];
		public static String FAX_GATEWAY_LOG = ConfigurationSettings.AppSettings["FAX_GATEWAY_LOG"];
		public static string CHARGE_CODE_CONTROL_FILE_FIELD_NAME = ConfigurationSettings.AppSettings["CHARGE_CODE_CONTROL_FILE_FIELD_NAME"];
		public static string ACK_FORMAT = ConfigurationSettings.AppSettings["ACK_FORMAT"];

		public const string WWFax = "WWFax";
		public const string TNZ = "TNZ";
	}
}
