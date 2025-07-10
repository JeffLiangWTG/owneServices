using System.Diagnostics.CodeAnalysis;

namespace Enterprise.Customs.Common.GUI
{
	[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Job status")]
	public static class TariffClassificationJobStatus
	{
		public const string NotFound = "NotFound";

		public const string New = "New";

		public const string Working = "Working";

		public const string Edited = "Edited";

		public const string Completed = "Completed";

		public const string ProcessingInCW1 = "Processing In CW1";
	}
}
