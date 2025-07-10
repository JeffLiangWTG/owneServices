using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.TemporaryStorage.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Module.Testing;

[TestedType(typeof(UCC6TemporaryStorageFilterStripBusinessObject))]
sealed class UCC6TemporaryStorageFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
{
	public void TestLookups()
	{
		var filterBizObj = (UCC6TemporaryStorageFilterStripBusinessObject)GetNewFilterStripBusinessObject();
		AssertType<UCC6TemporaryStorageFilterLookups>(filterBizObj.Lookups);
	}

	public void TestLRNFilter() => AssertModuleTextFilter<BillCusEntryNumModuleFilterSubGroup>(CusEntryNumberTypes.Standard.LocalReferenceNumber, "LRN");

	public void TestMRNFilter() => AssertModuleTextFilter<BillCusEntryNumModuleFilterSubGroup>(CusEntryNumberTypes.Standard.MovementReferenceNumber, "MRN");

	public void TestReleaseDateFilter() => AssertModuleDateFilter(CusEntryNumberTypes.EU.CustomsRegistry, "Release Date");

	public void TestRegistrationNumberFilter() => AssertModuleTextFilter(CusEntryNumberTypes.EU.CustomsRegistry, "Registration Number");

	public void TestSupervisingCustomsOfficeFilter()
	{
		var header1 = SetUpBillCusEntryNums(CusEntryNumberTypes.Standard.LocalReferenceNumber, "AB");
		var header2 = SetUpBillCusEntryNums(CusEntryNumberTypes.Standard.LocalReferenceNumber, "CD");

		header1.AMA_CustomsOffice = "AB";
		header2.AMA_CustomsOffice = "CD";

		var filterStripBizObj = new UCC6TemporaryStorageFilterStripBusinessObject();
		var textFilter = (ModuleNkFilter)filterStripBizObj["Supervising Customs Office"];
		textFilter.IsActive = true;

		textFilter.Property = ZString.Empty;

		var collection = new EU.Business.CusTempStorage.TemporaryStorageHeaderCollection(Factory);
		collection.AdditionalFilter = filterStripBizObj.Filter;
		AssertEquals(2, collection.Count);
		AssertEquals(header1.PK, collection[0].PK);
		AssertEquals(header2.PK, collection[1].PK);

		textFilter.Property = "AB";

		collection.AdditionalFilter = filterStripBizObj.Filter;
		AssertEquals(1, collection.Count);
		AssertEquals(header1.PK, collection[0].PK);
	}

	public void TestPresentationCustomsOfficeFilter()
	{
		var header1 = SetUpBillCusEntryNums(CusEntryNumberTypes.Standard.LocalReferenceNumber, "AB");
		var header2 = SetUpBillCusEntryNums(CusEntryNumberTypes.Standard.LocalReferenceNumber, "CD");

		var officeCode1 = GetNewEuOfficeCode();
		officeCode1.CY_ParentID = header1.PK;
		officeCode1.CY_Data = "AB";

		var officeCode2 = GetNewEuOfficeCode();
		officeCode2.CY_ParentID = header2.PK;
		officeCode2.CY_Data = "CD";

		Factory.Save();

		var filterStripBizObj = new UCC6TemporaryStorageFilterStripBusinessObject();
		var textFilter = (ModuleNkFilter)filterStripBizObj["Presentation Customs Office"];
		textFilter.IsActive = true;

		textFilter.Property = ZString.Empty;

		var collection = new EU.Business.CusTempStorage.TemporaryStorageHeaderCollection(Factory);
		collection.AdditionalFilter = filterStripBizObj.Filter;
		AssertEquals(2, collection.Count);
		AssertEquals(header1.PK, collection[0].PK);
		AssertEquals(header2.PK, collection[1].PK);

		textFilter.Property = "AB";

		collection.AdditionalFilter = filterStripBizObj.Filter;

		AssertEquals(1, collection.Count);
		AssertEquals(header1.PK, collection[0].PK);
	}

	public void TestCustomsStatusDateFilter()
	{
		var today = ZDateTime.Today;

		var header1 = SetUpHeaderWithRegistrationCusEntryNumber(today);
		var header2 = SetUpHeaderWithRegistrationCusEntryNumber(today.AddDays(-1));

		Factory.Save();

		var filterStripBizObj = new UCC6TemporaryStorageFilterStripBusinessObject();

		var dateFilter = (ModuleDateFilter)filterStripBizObj["Customs Status Date"];

		dateFilter.IsActive = true;
		dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

		dateFilter.Property1 = ZDateTime.Empty;
		dateFilter.Property2 = ZDateTime.Empty;

		var collection = new EU.Business.CusTempStorage.TemporaryStorageHeaderCollection(Factory);
		collection.AdditionalFilter = filterStripBizObj.Filter;
		AssertEquals(2, collection.Count);
		AssertEquals(header1.PK, collection[0].PK);
		AssertEquals(header2.PK, collection[1].PK);

		dateFilter.Property1 = today;
		dateFilter.Property2 = today;

		collection.AdditionalFilter = filterStripBizObj.Filter;
		AssertEquals(1, collection.Count);
		AssertEquals(header1.PK, collection[0].PK);

		dateFilter.Property1 = today.AddDays(-1);
		collection.AdditionalFilter = filterStripBizObj.Filter;
		AssertEquals(2, collection.Count);
		AssertEquals(header1.PK, collection[0].PK);
		AssertEquals(header2.PK, collection[1].PK);
	}

	public void TestMessageStatusFilter()
	{
		var header1 = Factory.New<TemporaryStorageHeader>();
		var header2 = Factory.New<TemporaryStorageHeader>();

		header1.AMA_MessageStatus = "ACK";
		header2.AMA_MessageStatus = "SNT";

		var filterStripBizObj = new UCC6TemporaryStorageFilterStripBusinessObject();

		var textFilter = (ModuleTextFilter)filterStripBizObj["Message Status"];

		textFilter.IsActive = true;
		textFilter.Property = ZString.Empty;

		var collection = new EU.Business.CusTempStorage.TemporaryStorageHeaderCollection(Factory);
		collection.AdditionalFilter = filterStripBizObj.Filter;

		AssertEquals(2, collection.Count);
		AssertEquals(header1.PK, collection[0].PK);
		AssertEquals(header2.PK, collection[1].PK);

		textFilter.Property = "ACK";
		collection.AdditionalFilter = filterStripBizObj.Filter;

		AssertEquals(1, collection.Count);
		AssertEquals(header1.PK, collection[0].PK);

		textFilter.Property = "SNT";
		collection.AdditionalFilter = filterStripBizObj.Filter;

		AssertEquals(1, collection.Count);
		AssertEquals(header2.PK, collection[0].PK);

		textFilter.Property = "FAL";
		collection.AdditionalFilter = filterStripBizObj.Filter;

		AssertEquals(0, collection.Count);
	}

	public void TestOrganizationAddressInformationFilter()
	{
		var header1 = Factory.New<TemporaryStorageHeader>();
		var header2 = Factory.New<TemporaryStorageHeader>();

		var org1 = Factory.NewWithValidTestData<OrgHeader>();
		var org2 = Factory.NewWithValidTestData<OrgHeader>();

		var goods1 = header1.GoodsLocation;
		goods1.Address.E2_AdditionalAddressInformation = org1.PK.ToString();
		var goods2 = header2.GoodsLocation;
		goods2.Address.E2_AdditionalAddressInformation = org2.PK.ToString();

		Factory.Save();

		var filterStripBizObj = new UCC6TemporaryStorageFilterStripBusinessObject();
		var guidFilter = (ModuleGuidFilter)filterStripBizObj["Location of Goods: Organization"];
		guidFilter.IsActive = true;
		guidFilter.Property = ZGuid.Empty;

		var collection = new EU.Business.CusTempStorage.TemporaryStorageHeaderCollection(Factory);
		collection.AdditionalFilter = filterStripBizObj.Filter;

		AssertEquals(2, collection.Count);
		AssertEquals(header1.PK, collection[0].PK);
		AssertEquals(header2.PK, collection[1].PK);

		guidFilter.Property = org1.PK;

		collection.AdditionalFilter = filterStripBizObj.Filter;

		AssertEquals(1, collection.Count);
		AssertEquals(header1.PK, collection[0].PK);
	}

	public void TestAuthorizationNumberFilter()
	{
		var header1 = Factory.New<TemporaryStorageHeader>();
		var header2 = Factory.New<TemporaryStorageHeader>();

		var goods1 = header1.GoodsLocation;
		goods1.Address.AuthorisationNumber = "TestRegistrationNumber1";

		var goods2 = header2.GoodsLocation;
		goods2.Address.AuthorisationNumber = "TestRegistrationNumber2";

		Factory.Save();

		var filterStripBizObj = new UCC6TemporaryStorageFilterStripBusinessObject();
		var textFilter = (ModuleTextFilter)filterStripBizObj["Location of Goods: Authorization No."];
		textFilter.IsActive = true;
		textFilter.Property = ZString.Empty;

		var collection = new EU.Business.CusTempStorage.TemporaryStorageHeaderCollection(Factory);
		collection.AdditionalFilter = filterStripBizObj.Filter;

		AssertEquals(2, collection.Count);
		AssertEquals(header1.PK, collection[0].PK);
		AssertEquals(header2.PK, collection[1].PK);
		textFilter.Property = "TestRegistrationNumber1";

		collection.AdditionalFilter = filterStripBizObj.Filter;

		AssertEquals(1, collection.Count);
		AssertEquals(header1.PK, collection[0].PK);

		textFilter.Property = "TestRegistrationNumber2";
		collection.AdditionalFilter = filterStripBizObj.Filter;

		AssertEquals(1, collection.Count);
		AssertEquals(header2.PK, collection[0].PK);
	}

	public void TestPlaceIdFilter()
	{
		var header1 = Factory.New<TemporaryStorageHeader>();
		var header2 = Factory.New<TemporaryStorageHeader>();

		var goods1 = header1.GoodsLocation;
		goods1.AdditionalIdentifier = "TestPlaceId1";
		var goods2 = header2.GoodsLocation;
		goods2.AdditionalIdentifier = "TestPlaceId2";

		Factory.Save();

		var filterStripBizObj = new UCC6TemporaryStorageFilterStripBusinessObject();
		var textFilter = (ModuleTextFilter)filterStripBizObj["Location of Goods: Place ID"];
		textFilter.IsActive = true;
		textFilter.Property = ZString.Empty;

		var collection = new EU.Business.CusTempStorage.TemporaryStorageHeaderCollection(Factory);
		collection.AdditionalFilter = filterStripBizObj.Filter;

		AssertEquals(2, collection.Count);
		AssertEquals(header1.PK, collection[0].PK);
		AssertEquals(header2.PK, collection[1].PK);

		textFilter.Property = "TestPlaceId1";

		collection.AdditionalFilter = filterStripBizObj.Filter;

		AssertEquals(1, collection.Count);
		AssertEquals(header1.PK, collection[0].PK);

		textFilter.Property = "TestPlaceId2";
		collection.AdditionalFilter = filterStripBizObj.Filter;

		AssertEquals(1, collection.Count);
		AssertEquals(header2.PK, collection[0].PK);
	}

	public void TestDeclarantFilter() => AssertModuleGuidFilter(AsycudaManifestHeaderSchema.Constants.AMA_OA_Declarant, "Declarant");

	public void TestRepresentativeFilter() => AssertModuleGuidFilter(AsycudaManifestHeaderSchema.Constants.AMA_OA_Representative, "Representative");

	protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new UCC6TemporaryStorageFilterStripBusinessObject();

	public void TestGetFilterInflatorFactory() => AssertType<UCC6TemporaryStorageFilterInflatorFactory>(new UCC6TemporaryStorageFilterStripBusinessObjectForTest().GetFilterInflatorFactory_Exposed());

	public void TestGetFilterInflators()
	{
		var filterStrip = new UCC6TemporaryStorageFilterStripBusinessObjectForTest();
		var filterInflators = filterStrip.GetFilterInflators_Exposed().WhereNotNull().ToList();
		AssertEquals(ExpectedFilterInflatorTypes.Count, filterInflators.Count);

		var inflatorTypes = filterInflators.ToDictionary(inf => inf.GetType());
		CombineAssertions(() => ExpectedFilterInflatorTypes.ForEach(t =>
			Assert($"Filter inflator '{t.FullName}' should be present", inflatorTypes.ContainsKey(t))));
	}

	ModuleTextFilter AssertModuleTextFilter(string entryType, string filterName)
	{
		var header1 = SetUpBillCusEntryNums(entryType, "AB");
		var header2 = SetUpBillCusEntryNums(entryType, "CD");

		Factory.Save();

		var filterStripBizObj = new UCC6TemporaryStorageFilterStripBusinessObject();
		var textFilter = (ModuleTextFilter)filterStripBizObj[filterName];
		textFilter.IsActive = true;

		textFilter.Property = ZString.Empty;

		var collection = new EU.Business.CusTempStorage.TemporaryStorageHeaderCollection(Factory);
		collection.AdditionalFilter = filterStripBizObj.Filter;
		CombineAssertions("When filter text is empty", () =>
		{
			AssertEquals("Count", 2, collection.Count);
			AssertEquals("header1", header1.PK, collection[0].PK);
			AssertEquals("header2", header2.PK, collection[1].PK);
		});

		textFilter.Property = "AB";

		collection.AdditionalFilter = filterStripBizObj.Filter;
		CombineAssertions("When filter text is AB", () =>
		{
			AssertEquals("Count", 1, collection.Count);
			AssertEquals("header1", header1.PK, collection[0].PK);
		});

		return textFilter;
	}

	void AssertModuleTextFilter<T>(string propertyName, string filterName)
	{
		var textFilter = AssertModuleTextFilter(propertyName, filterName);
		AssertType<T>(textFilter.SubGroup);
	}

	void AssertModuleDateFilter(string entryType, string filterName)
	{
		var today = ZDateTime.Today;

		var header1 = SetUpBillCusEntryNums(entryType, "AB", today);
		var header2 = SetUpBillCusEntryNums(entryType, "CD", today.AddDays(-1));

		Factory.Save();

		var filterStripBizObj = new UCC6TemporaryStorageFilterStripBusinessObject();
		var dateFilter = (ModuleDateFilter)filterStripBizObj[filterName];
		dateFilter.IsActive = true;
		dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

		dateFilter.Property1 = ZDateTime.Empty;
		dateFilter.Property2 = ZDateTime.Empty;

		var collection = new EU.Business.CusTempStorage.TemporaryStorageHeaderCollection(Factory);
		collection.AdditionalFilter = filterStripBizObj.Filter;
		AssertEquals(2, collection.Count);
		AssertEquals(header1.PK, collection[0].PK);
		AssertEquals(header2.PK, collection[1].PK);

		dateFilter.Property1 = ZDateTime.Today;
		dateFilter.Property2 = ZDateTime.Today;

		collection.AdditionalFilter = filterStripBizObj.Filter;
		AssertEquals(1, collection.Count);
		AssertEquals(header1.PK, collection[0].PK);

		dateFilter.Property1 = today.AddDays(-1);
		collection.AdditionalFilter = filterStripBizObj.Filter;
		AssertEquals(2, collection.Count);
		AssertEquals(header1.PK, collection[0].PK);
		AssertEquals(header2.PK, collection[1].PK);
	}

	void AssertModuleGuidFilter(string entryType, string filterName)
	{
		var orgHeader1 = Factory.New<OrgHeader>();
		orgHeader1.OH_Code = "ORG1";
		var orgHeader2 = Factory.New<OrgHeader>();
		orgHeader2.OH_Code = "ORG2";

		var header1 = GetNewTemporaryStorageHeader();
		header1[entryType] = orgHeader1.MainAddress.PK;

		var header2 = GetNewTemporaryStorageHeader();
		header2[entryType] = orgHeader2.MainAddress.PK;
		Factory.Save();

		var filterStripBizObj = new UCC6TemporaryStorageFilterStripBusinessObject();
		var filter = (ModuleGuidFilter)filterStripBizObj[filterName];
		filter.IsActive = true;

		filter.Property = ZGuid.Empty;

		var collection = new EU.Business.CusTempStorage.TemporaryStorageHeaderCollection(Factory);
		collection.AdditionalFilter = filterStripBizObj.Filter;

		CombineAssertions("When filter is empty", () =>
		{
			AssertEquals("Count", 2, collection.Count);
			AssertEquals("header1", header1.PK, collection[0].PK);
			AssertEquals("header2", header2.PK, collection[1].PK);
		});

		filter.Property = orgHeader1.PK;

		collection.AdditionalFilter = filterStripBizObj.Filter;

		CombineAssertions("When filter is orgHeader1", () =>
		{
			AssertEquals("Count", 1, collection.Count);
			AssertEquals("header1", header1.PK, collection[0].PK);
		});
	}

	EuOfficeCode GetNewEuOfficeCode()
	{
		var code = Factory.NewWithValidTestData<EuOfficeCode>();
		code.CY_ParentTableCode = AsycudaManifestHeaderSchema.Constants.Prefix;
		code.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;
		return code;
	}

	TemporaryStorageHeader SetUpBillCusEntryNums(ZString entryType, ZString entryNum, ZDateTime? issueDate = default)
	{
		var header = GetNewTemporaryStorageHeader();
		var bill = header.Bills.AddNew();
		var entryNumber = CusEntryNumber.LoadOrCreate(bill, entryType, Core.Constants.CountryCodes.Italy);
		entryNumber.CE_EntryNum = entryNum;
		entryNumber.CE_IssueDate = issueDate ?? ZDateTime.Today;
		return header;
	}

	TemporaryStorageHeader SetUpHeaderWithRegistrationCusEntryNumber(ZDateTime issueDate)
	{
		var header = GetNewTemporaryStorageHeader();
		var entry = CusEntryNumber.New<CusEntryNumber>(header, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, Core.Constants.CountryCodes.Italy);
		entry.CE_IssueDate = issueDate;
		entry.CE_ParentTable = "AsycudaManifestHeader";
		entry.CE_ParentID = header.PK;
		return header;
	}

	TemporaryStorageHeader GetNewTemporaryStorageHeader()
	{
		var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Italy;
		header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.TemporaryStorage;
		return header;
	}

	List<Type> ExpectedFilterInflatorTypes =>
	[
		typeof(EU.TemporaryStorage.Module.ApplicationCodeFilterInflator),
		typeof(EU.TemporaryStorage.Module.BillNumberFilterInflator),
		typeof(EU.TemporaryStorage.Module.BranchFilterInflator),
		typeof(EU.TemporaryStorage.Module.ConsigneeFilterInflator),
		typeof(EU.TemporaryStorage.Module.ConsignorFilterInflator),
		typeof(EU.TemporaryStorage.Module.CountryFilterInflator),
		typeof(SupervisingCustomsOfficeFilterInflator),
		typeof(EU.TemporaryStorage.Module.CustomsStatusFilterInflator),
		typeof(DeclarantFilterInflator),
		typeof(EU.TemporaryStorage.Module.JobNumberFilterInflator),
		typeof(LocalReferenceNumberFilterInflator),
		typeof(EU.TemporaryStorage.Module.MessageTypeFilterInflator),
		typeof(MovementReferenceNumberFilterInflator),
		typeof(EU.TemporaryStorage.Module.NotifyFilterInflator),
		typeof(EU.TemporaryStorage.Module.PackingMarksFilterInflator),
		typeof(EU.TemporaryStorage.Module.PackingTypeFilterInflator),
		typeof(PresentationCustomsOfficeFilterInflator),
		typeof(EU.TemporaryStorage.Module.PresentationDateFilterInflator),
		typeof(EU.TemporaryStorage.Module.PresenterFilterInflator),
		typeof(EU.TemporaryStorage.Module.PreviousDocumentNumberFilterInflator),
		typeof(EU.TemporaryStorage.Module.UCC6TemporaryStoragePreviousDocumentTypeFilterInflator),
		typeof(RepresentativeFilterInflator),
		typeof(EU.TemporaryStorage.Module.SupportingDocumentNumberFilterInflator),
		typeof(EU.TemporaryStorage.Module.UCC6TemporaryStorageSupportingDocumentTypeFilterInflator),
		typeof(EU.TemporaryStorage.Module.TariffFilterInflator),
		typeof(EU.TemporaryStorage.Module.TransportationModeFilterInflator),
		typeof(EU.TemporaryStorage.Module.TransportIdFilterInflator),
		typeof(EU.TemporaryStorage.Module.TransportTypeFilterInflator),
		typeof(CustomsStatusDateFilterInflator),
		typeof(LocationOfGoodsAuthorizationNumberFilterInflator),
		typeof(LocationOfGoodsOrganizationFilterInflator),
		typeof(LocationOfGoodsPlaceIdFilterInflator),
		typeof(MessageStatusFilterInflator),
		typeof(RegistrationNumberFilterInflator),
		typeof(ReleaseDateFilterInflator)
	];

	sealed class UCC6TemporaryStorageFilterStripBusinessObjectForTest : UCC6TemporaryStorageFilterStripBusinessObject
	{
		public EU.TemporaryStorage.Module.UCC6TemporaryStorageFilterInflatorFactory GetFilterInflatorFactory_Exposed() => GetFilterInflatorFactory();
		public List<IFilterInflator> GetFilterInflators_Exposed() => GetFilterInflators();
	}
}
