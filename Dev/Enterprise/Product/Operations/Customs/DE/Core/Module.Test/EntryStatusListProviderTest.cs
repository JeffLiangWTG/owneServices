using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.DE.Module.Testing
{
	sealed class EntryStatusListProviderTest : Customs.Module.Testing.EntryStatusListProviderTest
	{
		public override void TestEntryStatusLists()
		{
			var countryCode = Core.Constants.CountryCodes.Germany;
			var cusCodeHelper = new UniversalReferenceTestDataHelper(Factory);
			cusCodeHelper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ExportCustomsStatus, "Export Entry Status List Type");
			cusCodeHelper.CreateNewOrGetExistingCusCodeList(countryCode, RefCusCodeListTypes.Codes.ExportCustomsStatus, "A1", "a1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			cusCodeHelper.CreateNewOrGetExistingCusCodeList(countryCode, RefCusCodeListTypes.Codes.ExportCustomsStatus, "B2", "b2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			cusCodeHelper.CreateNewOrGetExistingCusCodeList(countryCode, RefCusCodeListTypes.Codes.ExportCustomsStatus, "C3", "c3", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			cusCodeHelper.CreateNewOrGetExistingCusCodeList(countryCode, RefCusCodeListTypes.Codes.ExportCustomsStatus, "D4", "d4", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			cusCodeHelper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.CustomsStatus, "Entry Status List Type");
			cusCodeHelper.CreateNewOrGetExistingCusCodeList(countryCode, RefCusCodeListTypes.Codes.CustomsStatus, "X", "x", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			cusCodeHelper.CreateNewOrGetExistingCusCodeList(countryCode, RefCusCodeListTypes.Codes.ExportCustomsStatus, "B2", "U", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			cusCodeHelper.CreateNewOrGetExistingCusCodeList(countryCode, RefCusCodeListTypes.Codes.CustomsStatus, "B3", "y", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			cusCodeHelper.CreateNewOrGetExistingCusCodeList(countryCode, RefCusCodeListTypes.Codes.CustomsStatus, "Z", "Z", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var provider = new EntryStatusListProvider();
			var entryStatusList = (CodeDescriptionPairList)provider.EntryStatusList(Factory, Core.Constants.CountryCodes.Germany, ZString.Empty);
			AssertEquals("A1, ACK, B2, B3, C3, D4, SUB, X, Z", entryStatusList.CodesAsString);
		}
	}
}
