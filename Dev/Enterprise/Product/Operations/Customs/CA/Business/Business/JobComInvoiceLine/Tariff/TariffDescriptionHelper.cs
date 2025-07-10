using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.CA.Business
{
	public static class TariffDescriptionHelper
	{
		public static ZString GetTariffDescription(BusinessObjectFactory factory, ZString tariffNumber, ZBool isDataLoadingModule)
		{
			return factory.GetCachedValue(string.Join("_", tariffNumber, isDataLoadingModule, ZDateTime.Today), () =>
			{
				var result = ZString.Empty;
				if (isDataLoadingModule)
				{
					var tariffData = (ITariffData)new CACExportTariff.Loader(factory).LoadFromCode(tariffNumber);
					result = tariffData?.TariffDescription ?? ZString.Empty;
				}
				else
				{
					var tariffData = new TariffView.Loader(factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.Canada, Constants.TariffTypes.HarmonizedSystem, tariffNumber, ZDateTime.Today);
					result = tariffData?.ZZ1_Description ?? ZString.Empty;
				}
				return result;
			});
		}
	}
}
