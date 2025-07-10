using System;
namespace CargoWise.Bi.Registration.PowerBi
{
	public enum BiReportCategory
	{
		All = 0,
		Analytics,
		Warehouse,
		Finance
	}

	public static class BiReportCategories
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "not to be translated")]
		const string Warehouse = "warehouse";

		public static BiReportCategory GetBiReportCategory(string reportCategory)
		{
			var biReportCategory = BiReportCategory.All;

			if (reportCategory.Equals(Warehouse, StringComparison.OrdinalIgnoreCase))
			{
				biReportCategory = BiReportCategory.Warehouse;
			}

			return biReportCategory;
		}
	}
}
