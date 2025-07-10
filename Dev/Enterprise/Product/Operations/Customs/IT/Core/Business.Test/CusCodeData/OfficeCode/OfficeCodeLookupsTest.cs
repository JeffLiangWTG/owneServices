using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class OfficeCodeLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestOfficeCodeListImportDeclaration()
	{
		AssertOfficeCodeListBasedOnMessageTypeAndOfficeRole(Common.EU.EUJobMessageTypeList.Codes.Import, EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent, "FR0000", "XI0000");
	}

	public void TestOfficeCodeListExportDeclaration()
	{
		AssertOfficeCodeListBasedOnMessageTypeAndOfficeRole(Common.EU.EUJobMessageTypeList.Codes.Export, EuOfficeCodesTypes.Codes.OfficeOfExit, "FR0000", "XI0000");
	}

	void AssertOfficeCodeListBasedOnMessageTypeAndOfficeRole(string messageType, string officeRole, params string[] expectedCustomsOfficesInAnyOrder)
	{
		SetUpReferenceCustomsOfficesWithRole(officeRole);

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = messageType;
		var customsOffice = declaration.CustomsOffices.AddNew();
		customsOffice.CY_Code = officeRole;
		var officeCodeList = customsOffice.Lookups.OfficeCodeList;
		officeCodeList.Load();
		AssertContainsExactElementsInAnyOrder(expectedCustomsOfficesInAnyOrder, officeCodeList.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
	}

	void SetUpReferenceCustomsOfficesWithRole(string role)
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, parent: eun);
		var tradeGroupEUC = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, Core.Constants.Customs.Universal.RefCusTradeGroup.Codes.EuropeanUnionForCustoms, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.AddCountry(tradeGroupEUC, Core.Constants.CountryCodes.France, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		var tradeGroupCUAM = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, Core.Constants.Customs.Universal.RefCusTradeGroup.Codes.CustomsUnionAdditionalMembers, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.AddCountry(tradeGroupCUAM, Core.Constants.CountryCodes.Iceland, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		var tradeGroupCTP = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, Core.Constants.Customs.Universal.RefCusTradeGroup.Codes.EUCommonTransitProcedure, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.AddCountry(tradeGroupCTP, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FR0000", "FR0000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, role);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Iceland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IS0000", "IS0000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, role);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "GB0000", "GB0000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, role);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "XI0000", "XI0000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, role);
		Factory.Save();
	}
}
