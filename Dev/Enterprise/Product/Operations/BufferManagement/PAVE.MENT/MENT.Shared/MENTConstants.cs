
namespace Enterprise.PAVE.MENT.Shared
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Are column names")]
	public static class MENTConstants
	{
		public const string WebBrowserSectionType = "WEB";
		public const string ChartSectionType = "MNT";

		public const string AgedScoreValueColumn = "score";
		public const string ReleaseGroupColumn = "releaseGroup";
		public const string ComponentColumn = "component";
		public const string AttributeValueColumn = "attributeValue";
		public const string StaffColumn = "staff";

		public const string StringColumn = "STR"; // Column Type because Columns aren't ZTypes
		public const string DecimalColumn = "DEC"; // Column Type because Columns aren't ZTypes
		public const string DateColumn = "DAT"; // Column Type because Columns aren't ZTypes
		public const string NullColumn = "NUL"; // Column Type because Columns aren't ZTypes

		public const int LongRunningQueryTimeoutSeconds = 720;
		public const int MaxNumberOfQueryAttempts = 3;
	}
}
