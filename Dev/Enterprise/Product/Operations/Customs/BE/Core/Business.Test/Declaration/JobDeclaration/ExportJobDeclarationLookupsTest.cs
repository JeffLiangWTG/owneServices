using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

sealed class ExportJobDeclarationLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestJE_CustomsOfficeList_NoBLT()
	{
		var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
		var eunZZZ = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Belgium, parent: eunZZZ);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BE000001", "BE000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, UniversalReferenceConstants.Role, UniversalReferenceConstants.Export);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BE000002", "BE000002", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, UniversalReferenceConstants.Role, "SUP");
		helper.CreateCusCodeList(Core.Constants.CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BE000003", "BE000003", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "BE000004", "BE000004", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, UniversalReferenceConstants.Role, UniversalReferenceConstants.Export);

		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "FAC");
		var noBLTCodeList = helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Belgium, "FAC", "noBLT", "desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, UniversalReferenceConstants.Type, UniversalReferenceConstants.DALocatie);
		helper.CreateCusCodeListAttribute(noBLTCodeList.PK, UniversalReferenceConstants.SubType, UniversalReferenceConstants.Kantoor);
		Factory.Save();

		var jobDeclaration = GetExportDeclaration(DeclarationApplicationCodeList.Codes.Interfaced);
		var lookups = jobDeclaration.Lookups.JE_CustomsOfficeList;
		lookups.Reload(true);
		var codes = lookups.Cast<ZZRefCusCodeListCombined>().Select(c => c.ZZD_Code);
		CombineAssertions(() =>
		{
			AssertSame("Cached", jobDeclaration.Lookups.JE_CustomsOfficeList, lookups);
			AssertCollectionContains("BE000001", "BE000001", codes, false);
			AssertCollectionContains("BE000002", "BE000002", codes, false);
			AssertCollectionContains("BE000003", "BE000003", codes, false);
			AssertCollectionContains("BE000004", "BE000004", codes, false);
			AssertCollectionContains("noBLT", "noBLT", codes, true);
		});
	}

	public void TestJE_CustomsOfficeList_BLT()
	{
		var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
		var eunZZZ = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Belgium, parent: eunZZZ);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BE000001", "BE000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, UniversalReferenceConstants.Role, UniversalReferenceConstants.Export);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BE000002", "BE000002", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, UniversalReferenceConstants.Role, "Sup");
		helper.CreateCusCodeList(Core.Constants.CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BE000003", "BE000003", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "BE000004", "BE000004", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, UniversalReferenceConstants.Role, UniversalReferenceConstants.Export);
		Factory.Save();

		var jobDeclaration = GetExportDeclaration(DeclarationApplicationCodeList.Codes.Builtin);
		var lookups = jobDeclaration.Lookups.JE_CustomsOfficeList;
		lookups.Reload(true);
		var codes = lookups.Cast<ZZRefCusCodeListCombined>().Select(c => c.ZZD_Code);
		CombineAssertions(() =>
		{
			AssertSame("Cached", jobDeclaration.Lookups.JE_CustomsOfficeList, lookups);
			AssertCollectionContains("BE000001", "BE000001", codes, true);
			AssertCollectionContains("BE000002, incorrect Role", "BE000002", codes, false);
			AssertCollectionContains("BE000003, no Role", "BE000003", codes, false);
			AssertCollectionContains("BE000004, incorrect list type", "BE000004", codes, false);
		});
	}

	public void TestTransportMeansList_IWT()
	{
		var jobDeclaration = GetExportDeclaration(DeclarationApplicationCodeList.Codes.Builtin);
		jobDeclaration.JE_TransportModeInland = Customs.Business.TransportTypeList.Codes.InlandWaterwayTransport;
		var lookups = jobDeclaration.Lookups.TransportMeansList;
		CombineAssertions(() =>
		{
			AssertSame("Cached", jobDeclaration.Lookups.TransportMeansList, lookups);
			AssertContainsExactElementsInExactOrder(new[] { "80", "81" }, lookups.GetAllCodes());
		});
	}

	public void TestTransportMeansList_RAI()
	{
		var jobDeclaration = GetExportDeclaration(DeclarationApplicationCodeList.Codes.Builtin);
		jobDeclaration.JE_TransportModeInland = Customs.Business.TransportTypeList.Codes.Rail;
		var lookups = jobDeclaration.Lookups.TransportMeansList;
		CombineAssertions(() =>
		{
			AssertSame("Cached", jobDeclaration.Lookups.TransportMeansList, lookups);
			AssertContainsExactElementsInExactOrder(new[] { "20", "21" }, lookups.GetAllCodes());
		});
	}

	public void TestTransportMeansList_ROA()
	{
		var jobDeclaration = GetExportDeclaration(DeclarationApplicationCodeList.Codes.Builtin);
		jobDeclaration.JE_TransportModeInland = Customs.Business.TransportTypeList.Codes.Road;
		var lookups = jobDeclaration.Lookups.TransportMeansList;
		CombineAssertions(() =>
		{
			AssertSame("Cached", jobDeclaration.Lookups.TransportMeansList, lookups);
			AssertContainsExactElementsInExactOrder(new[] { "30" }, lookups.GetAllCodes());
		});
	}

	JobDeclaration GetExportDeclaration(string applicationCode)
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
		jobDeclaration.JE_ApplicationCode = applicationCode;
		return jobDeclaration;
	}
}
