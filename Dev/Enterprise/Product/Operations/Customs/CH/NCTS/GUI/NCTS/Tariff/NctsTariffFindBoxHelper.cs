using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.CH.NCTS.GUI;

public static class NctsTariffFindBoxHelper
{
	public static void InitializeFindBox(this INctsTariffFindBox findBox)
	{
		findBox.GetDataGrouping = () => findBox.IsFindHarmonizedWCOCode() ? Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO : Core.Constants.CountryCodes.Switzerland;
		findBox.GetTariffType = () => findBox.IsFindHarmonizedWCOCode() ? Universal.Constants.TariffTypes.HarmonizedSystem : Universal.Constants.TariffTypes.Export;
	}

	public static IFindBoxListProvider GetFindBoxListProvider(this INctsTariffFindBox findBox) => new TariffFindBoxListProvider(findBox.GetDataGrouping(), findBox.GetTariffType(), findBox.GetEffectiveDate, null, findBox.GetTariffFormatter(), findBox.TariffCode().Length == 8 && !findBox.ShouldShowExactDescription());

	static bool IsFindHarmonizedWCOCode(this INctsTariffFindBox findBox) => !findBox.ShouldShowExactDescription() && findBox.TariffCode().Length <= 6;

	static bool ShouldShowExactDescription(this INctsTariffFindBox findBox) => findBox.GetShouldShowExactDescription?.Invoke() ?? false;

	static ZString TariffCode(this INctsTariffFindBox findBox) => findBox.GetTariffFormatter()?.Format(findBox.Code) ?? findBox.Code;
}
