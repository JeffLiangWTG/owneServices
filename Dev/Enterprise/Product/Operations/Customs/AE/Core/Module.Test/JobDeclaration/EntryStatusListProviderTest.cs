using Enterprise.Customs.AE.Business;

namespace Enterprise.Customs.AE.Module.Testing;

public class EntryStatusListProviderTest : Customs.Module.Testing.EntryStatusListProviderTest
{
	public override void TestEntryStatusLists()
	{
		var codePair = new AEEntryStatusList();
		RunStatusCodeListForVariousCountriesTester(Core.Constants.CountryCodes.UnitedArabEmirates, codePair.GetAllCodes(), new string[] { "SUB", "ROK", "CEO" });
	}
}
