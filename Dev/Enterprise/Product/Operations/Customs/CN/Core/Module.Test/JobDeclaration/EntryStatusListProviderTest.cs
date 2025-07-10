using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Customs.Common.CN;

namespace Enterprise.Customs.CN.Module.Testing
{
	class EntryStatusListProviderTest : Customs.Module.Testing.EntryStatusListProviderTest
	{
		public override void TestEntryStatusLists()
		{
			var codePair = new ReadOnlyBusinessObjectFactory().GetCachedValue<EntryStatusList>();
			RunStatusCodeListForVariousCountriesTester(Constants.CountryCodes.China, codePair.GetAllCodes(), new[] { "WTO", "ROK", "CEO" });
		}
	}
}
