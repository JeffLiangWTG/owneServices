using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class ESUniversalReferenceTestDataHelper : UniversalReferenceTestDataHelper
	{
		public ESUniversalReferenceTestDataHelper(BusinessObjectFactory factory) : base(factory)
		{
		}

		public void CreateCusCodeListCanaryIsland(ZString countryCode, ZString islandCode, ZString islandDescription)
		{
			CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsFiscalTerritories, "State and Territories");

			var grouping = CreateNewOrGetExistingDataGrouping("EUN");
			CreateNewOrGetExistingDataGrouping(countryCode + "C", parent: grouping);
			CreateNewOrGetExistingCusCodeList(countryCode + "C", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsFiscalTerritories, islandCode, islandDescription, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			factory.Save();
		}

		public void CreateCusCodeListNorthAfricanTerritory(ZString countryCode, ZString territoryCode, ZString territoryDescription)
		{
			CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsFiscalTerritories, "State and Territories");

			var grouping = CreateNewOrGetExistingDataGrouping("EUN");
			CreateNewOrGetExistingDataGrouping(countryCode + "A", parent: grouping);
			CreateNewOrGetExistingCusCodeList(countryCode + "A", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsFiscalTerritories, territoryCode, territoryDescription, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			factory.Save();
		}

		public void CreateCusCodeListTerritory(ZString countryCode, ZString territoryCode, ZString territoryDescription)
		{
			CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsFiscalTerritories, "State and Territories");

			var grouping = CreateNewOrGetExistingDataGrouping("EUN");
			CreateNewOrGetExistingDataGrouping(countryCode, parent: grouping);
			CreateNewOrGetExistingCusCodeList(countryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsFiscalTerritories, territoryCode, territoryDescription, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			factory.Save();
		}
	}
}
