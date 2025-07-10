using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.ValueProviders.Macros.UtilityClasses
{
	static class CountryExtraTaxDescriptionHelper
	{
		public static string GetDescription(bool addInputPerfix = false)
		{
			if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.China)
			{
				return Res.GetString("ad460814-7a49-4e90-b294-9c7980b22c8e", "Input VAT Claimed");
			}
			else
			{
				return addInputPerfix ? Res.GetString("5f04a355-8f41-4522-9526-5c133e4dbad7", "Input Extra Tax") : Res.GetString("5ca5883e-489e-4baa-88c7-d64c5712a9dd", "Extra Tax");
			}
		}
	}
}
