namespace Enterprise.Customs.AU.Module
{
	public static class FilterConstants
	{
		public static class DateTypes
		{
			public const string ArrivalDate = "Arrival Date";
			public const string OutturnDate = "Outturn Date";
		}

		public static class NumberTypes
		{
			public const string Housebill = "House Bill No.";
			public const string Masterbill = "Master Bill No.";
			public const string Flight = "Flight No.";
			public const string SendersRef = "Sender Message Reference No.";
			public const string OutturnSendersRef = "Outturn Message Reference No.";
		}

		public static class StatusTypes
		{
			public const string Cargo = "Cargo Status";
			public const string Commercial = "Commercial Status";
			public const string Outturn = "Outturn Status";
			public const string Underbond = "Underbond Status";
		}
	}
}
