using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.EU.Business.CusTempStorage.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing.CusTempStorage.ConfigurationProviders;

[TestedType(typeof(TemporaryStoragePreviousDocumentConfiguration))]
sealed class TemporaryStoragePreviousDocumentConfigurationTest : TemporaryStoragePreviousDocumentConfigurationAbstractTest<TemporaryStoragePreviousDocumentConfiguration>
{
	public void TestGetCodeListWithMessageTypeG5ROrG5E_CustomsOfficeES()
	{
		GenerateCodeList();

		var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		storageHeader.AMA_CustomsOffice = "ES009999";
		storageHeader.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
		RunAssertions(storageHeader, configuration, codeLists1, codeLists2, codeLists3);
		storageHeader.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
		RunAssertions(storageHeader, configuration, codeLists1, codeLists2, codeLists3);
	}

	public void TestGetCodeListWithMessageTypeG5ROrG5E_CustomsOfficeNotES()
	{
		GenerateCodeList();

		var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		storageHeader.AMA_CustomsOffice = "FROFF001";
		storageHeader.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
		RunAssertions(storageHeader, configuration, codeLists2, codeLists1, codeLists3);
		storageHeader.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
		RunAssertions(storageHeader, configuration, codeLists2, codeLists1, codeLists3);
	}

	public void TestGetCodeListWithMessageTypeG5ROrG5E_CustomsOfficeEmpty()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		SetUpCodeTypeAndDataGroups(helper);

		var codeListsES = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS, "ES Code", "Test 1",
			 ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();

		var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		storageHeader.AMA_CustomsOffice = ZString.Empty;
		storageHeader.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
		var previousDocument = storageHeader.PreviousDocuments.AddNew();
		var codeList = configuration.GetCodeList(previousDocument) as ZZRefCusCodeListCombinedCollection;
		var completeFilter = codeList.CompleteFilter;
		codeList.Load();
		AssertEquals("The Collection returns ES Code Lists", 1, codeList.Count);
		AssertEquals("Matches ES Code With G5v1Expedition", true, Factory.Load<ZZRefCusCodeListCombined>(codeListsES.PK).MatchesFilter(completeFilter));

		storageHeader.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
		previousDocument = storageHeader.PreviousDocuments.AddNew();
		codeList = configuration.GetCodeList(previousDocument) as ZZRefCusCodeListCombinedCollection;
		completeFilter = codeList.CompleteFilter;
		AssertEquals("Matches ES Code With G5v1Reception", true, Factory.Load<ZZRefCusCodeListCombined>(codeListsES.PK).MatchesFilter(completeFilter));
	}

	public void TestGetCodeListWithMessageTypeG5ROrG5E_CustomsOfficeEmpty2()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		SetUpCodeTypeAndDataGroups(helper);

		var codeListsEU = helper.CreateNewOrGetExistingCusCodeList("EUN",
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS, "EUN Code", "Test 1",
			ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();
		var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		storageHeader.AMA_CustomsOffice = ZString.Empty;
		storageHeader.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
		var previousDocument = storageHeader.PreviousDocuments.AddNew();
		var codeList = configuration.GetCodeList(previousDocument) as ZZRefCusCodeListCombinedCollection;
		var completeFilter = codeList.CompleteFilter;
		codeList.Load();
		AssertEquals("The Collection returns EUN Code List", 1, codeList.Count);
		AssertEquals("Matched EUN Code  With G5v1Expedition", true, Factory.Load<ZZRefCusCodeListCombined>(codeListsEU.PK).MatchesFilter(completeFilter));

		storageHeader.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
		previousDocument = storageHeader.PreviousDocuments.AddNew();
		codeList = configuration.GetCodeList(previousDocument) as ZZRefCusCodeListCombinedCollection;
		completeFilter = codeList.CompleteFilter;
		AssertEquals("Matches ES Code With G5v1Reception", true, Factory.Load<ZZRefCusCodeListCombined>(codeListsEU.PK).MatchesFilter(completeFilter));
	}

	public void TestGetCodeListWithoutMessageTypeG5RNorG5E()
	{
		GenerateCodeList();

		var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		storageHeader.AMA_CustomsOffice = "FROFF001";
		storageHeader.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
		var previousDocument = storageHeader.PreviousDocuments.AddNew();
		var codeList = configuration.GetCodeList(previousDocument) as ZZRefCusCodeListCombinedCollection;
		codeList.Load();
		AssertEquals("The Collection returns all Code Lists", 3, codeList.Count);
	}

	public override void TestGetCodeList() => Assert(true);

	protected override void SetUp()
	{
		base.SetUp();
		SetUpOffice();
	}

	RefCusCodeList codeLists1, codeLists2, codeLists3;

	void SetUpOffice()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var enuZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, parent: enuZZZ);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
		var cusCode1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "ES009999", "SOMEWHERE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		var cusCode2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FROFF001", "PARIS PORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();
	}

	void RunAssertions(TemporaryStorageHeader storageHeader, TemporaryStoragePreviousDocumentConfiguration configuration,
	RefCusCodeList expectedCodeList, RefCusCodeList unexpectedCodeList1, RefCusCodeList unexpectedCodeList2) => CombineAssertions(() =>
	{
		var previousDocument = storageHeader.PreviousDocuments.AddNew();
		var codeList = configuration.GetCodeList(previousDocument) as ZZRefCusCodeListCombinedCollection;
		var completeFilter = codeList.CompleteFilter;

		AssertEquals("Matched IsNational=Y", true, Factory.Load<ZZRefCusCodeListCombined>(expectedCodeList.PK).MatchesFilter(completeFilter));
		AssertEquals("Unmatched IsNational=N", false, Factory.Load<ZZRefCusCodeListCombined>(unexpectedCodeList1.PK).MatchesFilter(completeFilter));
		AssertEquals("Unmatched IsNational=N", false, Factory.Load<ZZRefCusCodeListCombined>(unexpectedCodeList2.PK).MatchesFilter(completeFilter));
		AssertEquals("Cached", (ZZRefCusCodeListCombinedCollection)previousDocument.Lookups.CodeList, codeList);
	});

	void SetUpCodeTypeAndDataGroups(UniversalReferenceTestDataHelper helper)
	{
		helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS,
			"Previous Documents Of PNTS");
		var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain, null, grouping);
	}

	void GenerateCodeList()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		SetUpCodeTypeAndDataGroups(helper);
		helper.CreateNewOrGetExistingRefCusCodeListAttributeName("IsNational", "IsNational", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS, Core.Constants.CountryCodes.Spain);

		var attributeNameValuePairs = new Dictionary<string, string[]>();
		codeLists1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS, "ES Code with IsNational Y", "Test 1",
			ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		codeLists1.Attributes.AddNew("IsNational", "Y");

		codeLists2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS, "ES Code with IsNational N", "Test 1",
			ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		codeLists2.Attributes.AddNew("IsNational", "N");

		codeLists3 = helper.CreateNewOrGetExistingCusCodeList("EUN",
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS, "EUN Code", "Test 1",
			ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();
	}
}
