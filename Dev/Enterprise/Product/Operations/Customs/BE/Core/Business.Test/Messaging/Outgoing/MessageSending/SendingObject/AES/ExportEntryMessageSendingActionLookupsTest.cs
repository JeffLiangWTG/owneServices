using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(ExportEntryMessageSendingActionLookups))]
sealed class ExportEntryMessageSendingActionLookupsTest : BEJobDeclarationMessageSendingObjectLookupsTest<ExportEntryMessageSendingActionLookups, ExportEntryMessageSendingAction>
{
	public void TestListTypes()
	{
		AssertType<CustomsOfficeCodeCollection>("ExitCustomsOfficeList", lookups.ExitCustomsOfficeList);
	}

	public void TestExitCustomsOfficeList()
	{
		var tomorrow = ZDateTime.Today.AddDays(1);
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Belgium, "Belgium", eun);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, "Italy", eun);
		helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
		helper.CreateCusCodeType(RefCusCodeListAttributeTypes.Codes.ROLE, "ROLE");
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BE004323", "OFFICE1", ZDateTime.BrettsBirthday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.CompetentAuthorityOfEnquiry);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BE004324", "OFFICE2", ZDateTime.BrettsBirthday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExitInland);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BE004325", "OFFICE3", ZDateTime.BrettsBirthday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExit);

		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT004323", "Italy OFFICE1", ZDateTime.BrettsBirthday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.CompetentAuthorityOfEnquiry);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT004324", "Italy OFFICE2", ZDateTime.BrettsBirthday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExitInland);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT004325", "Italy OFFICE3", ZDateTime.BrettsBirthday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExit);
		Factory.Save();

		var coll = lookups.ExitCustomsOfficeList;
		coll.Load();
		AssertContainsExactElementsInAnyOrder(new[] { "BE004324", "BE004325" }, coll.Select(x => x.ZZD_Code));
	}

	public void TestEntryTypeListValues()
	{
		AssertEquals("AMD, CAN, DEC, PRN", BEExportEntryTypeList.GetExportEntryTypeList(EntrySubStyleList.Codes.PreliminarySimplifiedDeclarationUnderCodeB).CodesAsString);
	}

	public override void TestEntryTypeList()
	{
		AssertEquals("EntryType code list lookup content should match the code pair description list", BEExportEntryTypeList.GetExportEntryTypeList(EntrySubStyleList.Codes.StandardDeclaration).CodesAsString, lookups.EntryTypeList.CodesAsString);
	}
}
