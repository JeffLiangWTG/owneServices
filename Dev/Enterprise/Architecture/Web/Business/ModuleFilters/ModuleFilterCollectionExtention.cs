using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.Web.Business
{
	public static class ModuleFilterCollectionExtention
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter name")]
		public static string GetUniqueDescription(this ModuleFilterCollection filters, string description)
		{
			if (filters[description] != null)
			{
				description += " (Web)";
			}

			return description;
		}
	}
}
