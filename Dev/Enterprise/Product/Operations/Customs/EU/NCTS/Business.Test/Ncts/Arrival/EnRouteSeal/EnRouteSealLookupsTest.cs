using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class EnRouteSealLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEventCountries()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009, "C0009");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009, "GB", "United Kingdom", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009, "AD", "Andorra", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var eventCountries = enRouteSeal.Lookups.EventCountries;
			eventCountries.Load();
			AssertContainsExactElementsInAnyOrder(new[] { "AD", "GB" }, eventCountries.Select(c => c.ZZD_Code).ToArray());
		}

		protected override void SetUp()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			enRouteSeal = header.EnRouteSeals.AddNew();
		}
		EnRouteSeal enRouteSeal;
	}
}
