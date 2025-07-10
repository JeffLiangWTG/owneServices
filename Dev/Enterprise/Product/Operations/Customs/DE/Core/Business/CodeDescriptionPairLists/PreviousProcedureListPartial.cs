namespace Enterprise.Customs.DE.Business
{
	public partial class PreviousProcedureList
	{
		public static class Ncts
		{
			public static class Codes
			{
				public const string _FV = "FV";
			}
			public static class Descriptions
			{
				[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "German Description")]
				public const string _FV = "Besondere Verwendung";
			}
		}

		public static bool IsValidForPrematureInputFlag(string previousProcedureCode)
		{
			var result = false;
			if (previousProcedureCode != Codes._ATNEU
				&& previousProcedureCode != Codes._ATAV
				&& previousProcedureCode != Codes._ATZL
				&& previousProcedureCode != Codes._ESUMA
				&& previousProcedureCode != Codes._T1
				&& previousProcedureCode != Codes._T2
				&& previousProcedureCode != Codes._ATA
				&& previousProcedureCode != Codes._VO
				&& previousProcedureCode != Codes._TIR
				&& previousProcedureCode != Codes._OHNE)
			{
				result = true;
			}
			return result;
		}
	}
}
