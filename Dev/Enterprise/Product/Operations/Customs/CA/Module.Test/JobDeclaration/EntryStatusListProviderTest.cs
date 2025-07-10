using CargoWise.EntityFramework;
using Enterprise.Customs.Common.CA;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module.Testing
{
	sealed class EntryStatusListProviderTest : Customs.Module.Testing.EntryStatusListProviderTest
	{
		public override void TestEntryStatusLists()
		{
			var codePair = new CodeDescriptionPairList();
			codePair.AddRange((new ReadOnlyBusinessObjectFactory()).GetCachedValue<EDIReleaseImportEntryStatusList>());
			codePair.AddPairIfNotExist(ExtraConstantCodes.Codes.NotReleased, ExtraConstantCodes.Descriptions.NotReleased);

			RunStatusCodeListForVariousCountriesTester(Core.Constants.CountryCodes.Canada, codePair.GetAllCodes(), new string[] { "WTO", "ROK", "CEO" });
		}
	}
}
