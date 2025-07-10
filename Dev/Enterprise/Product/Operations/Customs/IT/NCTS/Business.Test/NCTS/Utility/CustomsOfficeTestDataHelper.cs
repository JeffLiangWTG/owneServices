using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

static class CustomsOfficeTestDataHelper
{
	public static void SetupCustomsOfficeData(BusinessObjectFactory factory)
	{
		var helper = new UniversalReferenceTestDataHelper(factory);
		var eunZZZPK = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);

		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: eunZZZPK);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom);
		helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT303199", "CAMPOBASSO", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, new ZString("ROLE"), new ZString("GUA"));
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT212122", "MILANOOFICE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, new ZString("ROLE"), new ZString("ENQ"));
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "TR44556", "TROFFICE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, new ZString("ROLE"), new ZString("GUA"));
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "GB42556", "GBOFFICE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, new ZString("ROLE"), new ZString("GUA"));
	}

	public static void SetupTraderGroups(BusinessObjectFactory factory)
	{
		var helper = new UniversalReferenceTestDataHelper(factory);
		var traderGroupStartDate = ZDate.Today.AddYears(-20);
		var traderGroupEndDate = ZDate.Today.AddYears(+40);
		var tradeGroup = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, RefCusTradeGroup.Codes.EuropeanUnionForCustoms, traderGroupStartDate, traderGroupEndDate);
		helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Italy, traderGroupStartDate, traderGroupEndDate);
		helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.UnitedKingdom, traderGroupStartDate, traderGroupEndDate);
		var tradeGroupCUAM = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, RefCusTradeGroup.Codes.CustomsUnionAdditionalMembers, traderGroupStartDate, traderGroupEndDate);
		helper.AddCountry(tradeGroupCUAM, Core.Constants.CountryCodes.Turkey, traderGroupStartDate, traderGroupEndDate);
	}
}
