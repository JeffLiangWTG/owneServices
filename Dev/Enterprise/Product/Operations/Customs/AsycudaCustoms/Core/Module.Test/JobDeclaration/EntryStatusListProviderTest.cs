using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.AsycudaCustoms.Module.Testing
{
	public class EntryStatusListProviderTest : Customs.Module.Testing.EntryStatusListProviderTest
	{
		public override void TestEntryStatusLists()
		{
			var countryCode = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(RefCusCodeListTypes.Codes.UserDefinedEntryStatus, "User Defined Entry Status");
			helper.CreateCusCodeList(countryCode, RefCusCodeListTypes.Codes.UserDefinedEntryStatus, "STA1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(countryCode, RefCusCodeListTypes.Codes.UserDefinedEntryStatus, "STA2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList("XX", RefCusCodeListTypes.Codes.UserDefinedEntryStatus, "STA3", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			RunStatusCodeListForVariousCountriesTester(countryCode, new string[] { "STA1", "STA2" }, new string[] { "STA3" });
		}
	}
}
