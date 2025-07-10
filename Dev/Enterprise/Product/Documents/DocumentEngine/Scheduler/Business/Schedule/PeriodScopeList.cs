using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	public partial class PeriodScopeList : CodeDescriptionPairList
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Code description pair code value")]
		public static class Codes
		{
			public const string Previous = "Previous";
			public const string This = "This";
			public const string Next = "Next";
		}

		public PeriodScopeList(bool isHourMinute = false)
		{
			AddPair(ResString.GetMultilingualString("PeriodScope|Previous", Codes.Previous), string.Empty);

			if (!isHourMinute)
			{
				AddPair(ResString.GetMultilingualString("PeriodScope|This", Codes.This), string.Empty);
			}

			AddPair(ResString.GetMultilingualString("PeriodScope|Next", Codes.Next), string.Empty);
		}
	}
}
