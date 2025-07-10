using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusCAeMHHouseLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLookupLists()
		{
			var houseBill = Factory.New<CusCAeMHMaster>().HouseBills.AddNew();
			AssertType(typeof(eMHMovementTypeList), houseBill.Lookups.MovementTypes);
			AssertType(typeof(EManifestUnitOfWeightList), houseBill.Lookups.WeightUnits);
			AssertEquals(14, houseBill.Lookups.VolumnUnits.Count);
			AssertType(typeof(CACSubLocationCollection), houseBill.Lookups.ReleaseSubLocations);
			AssertType(typeof(ZZRefCusCodeListCombinedCollection), houseBill.Lookups.ReleasePorts);
			AssertType(typeof(EManifestAmendmentReasonCodes), houseBill.Lookups.AmendmentCodes);
			AssertType(typeof(EManifestForwarderJobStatusList), houseBill.Lookups.CustomsStatuses);
			AssertType(typeof(MessageStatusList), houseBill.Lookups.MessageStatuses);
		}

		public void TestReleasePorts()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Port");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1111", "CA Customs Office Code", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1111", "US Customs Office Code", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "1111", "CA Customs Port Code", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1234", "CA Customs Office Code (Expired)", ZDateTime.BrettsBirthday, ZDateTime.Today.AddDays(-1));
			Factory.Save();

			var releasePorts = Factory.New<CusCAeMHMaster>().HouseBills.AddNew().Lookups.ReleasePorts;
			releasePorts.Load();

			AssertEquals(1, releasePorts.Count);
			AssertType<ZZRefCusCodeListCombinedCollection>(releasePorts);
			Assert(releasePorts.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "1111"));
			AssertEquals("CA Customs Office Code", releasePorts.Cast<ZZRefCusCodeListCombined>().FirstOrDefault(x => x.ZZD_Code == "1111").ZZD_Description);
			Assert(!releasePorts.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "1234"));
		}
	}
}
