namespace CargoWise.NetworkVisualisation.GUI
{
	public static class Constants
	{
		// TODO: replace these with configuration inside BMS
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		internal const int WorkingHoursPerDay = 8;
		internal const int WorkingDaysPerWeek = 5;
		internal const int DynamicGridRightMargin = 5;
		internal const int DynamicGridBottomMargin = 10;

		public const double EntityDetailsBottomMargin = 10d;
		public const double ChannelHeaderFontSize = 24d;

		public const double SectionSliderWidth = 8d;
		public const double NonScheduledSectionMinWidth = 500d;
		public const double SectionBaseMaxWidthBuffer = 350d;
		public const double ScheduledSectionExtraMaxWidthBuffer = 100d;
	}
}
