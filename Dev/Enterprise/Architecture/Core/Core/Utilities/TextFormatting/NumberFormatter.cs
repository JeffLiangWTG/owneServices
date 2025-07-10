namespace Enterprise.ZArchitecture.Core
{
	public static class NumberFormatter
	{
		public static string ToOrdinalString(int value)
		{
			return value.ToString() + GetOrdinalSuffix(value);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-semantic text")]
		public static string GetOrdinalSuffix(int value)
		{
			int hundredRemainder = value % 100;
			if (hundredRemainder >= 10 && hundredRemainder <= 20)
			{
				return "th";
			}
			int tenRemainder = value % 10;
			switch (tenRemainder)
			{
				case 1:
					return "st";
				case 2:
					return "nd";
				case 3:
					return "rd";
				default:
					return "th";
			}
		}
	}
}
