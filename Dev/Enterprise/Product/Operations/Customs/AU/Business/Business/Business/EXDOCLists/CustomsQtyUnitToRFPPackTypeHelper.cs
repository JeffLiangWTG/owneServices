using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public partial class CustomsQtyUnitToRFPPackType
	{
		public static string GetRFPPackTypeForCustomsQtyUnit(string countryCode, string customsQtyUnit)
		{
			var result = string.Empty;
			if (!string.IsNullOrEmpty(countryCode))
			{
				var packageType = new CustomsQtyUnitToRFPPackType().GetDescriptionFromCode(customsQtyUnit);
				if (packageType != null && packageType.IndexOf('|') >= 0)
				{
					var packageTypes = packageType.Split('|');
					result = Enterprise.Customs.Business.Extensions.BusinessObjectExtensions.IsMemberOfEU(new ReadOnlyBusinessObjectFactory(), countryCode) || countryCode == Core.Constants.CountryCodes.Turkey
						? packageTypes[0]
						: packageTypes[1];
				}
			}

			return result;
		}
	}
}
