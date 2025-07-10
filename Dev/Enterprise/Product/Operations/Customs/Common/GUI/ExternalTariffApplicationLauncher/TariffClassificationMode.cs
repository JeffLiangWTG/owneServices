using System.Diagnostics.CodeAnalysis;

namespace Enterprise.Customs.Common.GUI
{
	[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Tariff Classification Mode")]
	public static class TariffClassificationMode
	{
		public const string Single = "Single";

		public const string Batch = "Batch";
	}
}
