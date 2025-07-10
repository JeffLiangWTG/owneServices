using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CommonLookups))]
sealed class CommonLookupsTest : TestCaseWithFactory
{
	public void TestCustomsOfficeList() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateCustomsOfficesList(Factory);

		var lookup = CommonLookups.CustomsOfficeList(LookupParent);
		lookup.Load();

		AssertContainsExactElementsInAnyOrder(["CH001251", "CH001252"], lookup.Select(x => x.ZZD_Code));
		AssertSame("Cached", lookup, CommonLookups.CustomsOfficeList(LookupParent));
	});

	public void TestEntrySubStyleList_Import() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateEnsubCodeList(Factory);

		var lookup = CommonLookups.DeclarationTimeCodeList(LookupParent);
		AssertEquals("Codes", "01, 02, 03, 05, 99", lookup.CodesAsString);
		AssertSame("Cached", CommonLookups.DeclarationTimeCodeList(LookupParent), CommonLookups.DeclarationTimeCodeList(LookupParent));
	});

	public void TestNextProcedureList() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateNextProcedureList(Factory);

		var lookup = CommonLookups.NextProcedureList(LookupParent);
		AssertEquals("Codes", "1, 2, 3, 4", lookup.CodesAsString);
		AssertSame("Cached", lookup, CommonLookups.NextProcedureList(LookupParent));
	});

	public void TestEntryStatusList()
	{
		RefCusCodeTestHelper.CreateEntryStatusList(Factory);

		var lookup = CommonLookups.CustomsStatusList(Factory);
		AssertContainsExactElementsInAnyOrder(["0", "ACT", "CAN", "CLR", "CO1", "EVV", "REL"], lookup.GetAllCodes());
		AssertSame("Cached", lookup, CommonLookups.CustomsStatusList(Factory));
	}

	public void TestTransportModeList() => CombineAssertions(() =>
	{
		var lookup = CommonLookups.TransportModeList(Factory);
		AssertType<TransportMeansList>(lookup);
		AssertSame("Cached", lookup, CommonLookups.TransportModeList(Factory));
	});

	public void TestTransportTypeList() => CombineAssertions(() =>
	{
		var lookup = CommonLookups.TransportTypeList(Factory);
		AssertType<TransportTypeList>(lookup);
		AssertSame("Cached", lookup, CommonLookups.TransportTypeList(Factory));
	});

	public void TestTransportationTypeList() => CombineAssertions(() =>
	{
		new RefDataTestHelper(Factory).CreateCodeList(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TransportationType).CreateCode("22").Save();

		var lookup = CommonLookups.TransportationTypeList(LookupParent);
		AssertEquals("Codes", "22", lookup.CodesAsString);
		AssertSame("Cached", lookup, CommonLookups.TransportationTypeList(LookupParent));
	});

	public void TestDeclarationLanguageList() => CombineAssertions(() =>
	{
		var lookup = CommonLookups.CommunicationLanguageList(Factory);
		AssertEquals("Codes", "DE, FR, IT", lookup.CodesAsString);
		AssertSame("Cached", lookup, CommonLookups.CommunicationLanguageList(Factory));
	});

	public void TestMessageSubTypeList() => CombineAssertions(() =>
	{
		var lookup = CommonLookups.ActivationTypeList(Factory);
		AssertType<ActivationTypeList>(lookup);
		AssertEquals("Codes", "EDC, PAS", lookup.CodesAsString);
		AssertSame("Cached", lookup, CommonLookups.ActivationTypeList(Factory));
	});

	public void TestMessageStatusList() => CombineAssertions(() =>
	{
		var lookup = CommonLookups.MessageStatusList(Factory);
		AssertType<CHLogicalStatusList>(lookup);
		AssertSame("Cached", lookup, CommonLookups.MessageStatusList(Factory));
	});

	public void TestExportAuthorizationsList() => CombineAssertions(() =>
	{
		var declarant1 = Factory.NewWithValidTestData<OrgHeader>();
		declarant1.OH_Code = "CH123";
		var declarant2 = Factory.NewWithValidTestData<OrgHeader>();
		declarant2.OH_Code = "CH456";
		var appliesTo1 = Factory.NewWithValidTestData<OrgHeader>();
		appliesTo1.OH_Code = "APT01";
		var appliesTo2 = Factory.NewWithValidTestData<OrgHeader>();
		appliesTo2.OH_Code = "APT02";
		var appliesTo3 = Factory.NewWithValidTestData<OrgHeader>();
		appliesTo3.OH_Code = "APT03";

		var authorization1 = Factory.New<CusAuthorisationHeader>();
		authorization1.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedLocationCodeForPassar;
		authorization1.CPH_OH_PermitHolder = declarant1.PK;
		authorization1.CPH_Number = "123";
		authorization1.CPH_PermitDescription = "DESC123";
		authorization1.CPH_OA_AppliesTo = appliesTo1.MainAddress.PK;
		authorization1.CPH_StartDate = ZDate.Today.AddDays(-10);

		var authorization2 = Factory.New<CusAuthorisationHeader>();
		authorization2.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedLocationCodeForPassar;
		authorization2.CPH_OH_PermitHolder = declarant2.PK;
		authorization2.CPH_Number = "456";
		authorization2.CPH_PermitDescription = "DESC456";
		authorization2.CPH_OA_AppliesTo = appliesTo2.MainAddress.PK;
		authorization2.CPH_StartDate = ZDate.Today.AddDays(-10);

		var authorization3 = Factory.New<CusAuthorisationHeader>();
		authorization3.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedLocationCodeForPassar;
		authorization3.CPH_OH_PermitHolder = declarant1.PK;
		authorization3.CPH_Number = "789";
		authorization3.CPH_PermitDescription = "DESC789";
		authorization3.CPH_OA_AppliesTo = appliesTo3.MainAddress.PK;
		authorization3.CPH_StartDate = ZDate.Today.AddDays(-5);

		var authorization4 = Factory.New<CusAuthorisationHeader>();
		authorization4.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeLocationsForEDec;
		authorization4.CPH_OH_PermitHolder = declarant1.PK;
		authorization4.CPH_Number = "000";
		authorization4.CPH_PermitDescription = "DESC000";
		authorization4.CPH_OA_AppliesTo = appliesTo3.MainAddress.PK;
		authorization4.CPH_StartDate = ZDate.Today.AddDays(-10);

		var lookup = CommonLookups.ExportAuthorizationsList(LookupParent, false, declarant1.PK);
		AssertEquals($"Pasar {declarant1.OH_Code} CodesAsString", "123, 789", lookup.CodesAsString);
		AssertEquals($"Pasar {declarant1.OH_Code} Description for 123", "DESC123", lookup.GetDescriptionFromCode("123"));
		AssertEquals($"Pasar {declarant1.OH_Code} Description for 789", "DESC789", lookup.GetDescriptionFromCode("789"));

		lookup = CommonLookups.ExportAuthorizationsList(LookupParent, true, declarant1.PK);
		AssertEquals($"e-dec {declarant1.OH_Code} CodesAsString", "123, 789", lookup.CodesAsString);
		AssertEquals($"e-dec {declarant1.OH_Code} Description for 123", "APT01", lookup.GetDescriptionFromCode("123"));
		AssertEquals($"e-dec {declarant1.OH_Code} Description for 789", "APT03", lookup.GetDescriptionFromCode("789"));

		lookup = CommonLookups.ExportAuthorizationsList(LookupParent, false, declarant2.PK);
		AssertEquals($"Pasar {declarant2.OH_Code} CodesAsString", "456", lookup.CodesAsString);

		lookup = CommonLookups.ExportAuthorizationsList(LookupParent, true, declarant2.PK);
		AssertEquals($"e-dec {declarant2.OH_Code} CodesAsString", "456", lookup.CodesAsString);

		LookupParent.DateOfValuation = ZDate.Today.AddDays(-8);

		lookup = CommonLookups.ExportAuthorizationsList(LookupParent, false, declarant1.PK);
		AssertEquals($"Pasar {declarant1.OH_Code} CodesAsString", "123", lookup.CodesAsString);

		lookup = CommonLookups.ExportAuthorizationsList(LookupParent, true, declarant1.PK);
		AssertEquals($"e-dec {declarant1.OH_Code} CodesAsString", "123", lookup.CodesAsString);

		AssertSame("Cached", lookup, CommonLookups.ExportAuthorizationsList(LookupParent, true, declarant1.PK));
	});

	LookupParentForTesting LookupParent => lookupParent ??= Factory.New<LookupParentForTesting>();
	LookupParentForTesting lookupParent;

	class LookupParentForTesting : DummyBusinessObject, IDateOfValuationProvider
	{
		public LookupParentForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZDateTime DateOfValuation { get; set; } = ZDateTime.Now;
	}
}
