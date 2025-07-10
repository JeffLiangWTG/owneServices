using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.Module.Testing
{
	[TestedType(typeof(UCC6TemporaryStorageFilterStripBusinessObject))]
	class UCC6TemporaryStorageFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new UCC6TemporaryStorageFilterStripBusinessObject();

		public void TestLookups()
		{
			var filterBizObj = (UCC6TemporaryStorageFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			AssertEquals("Lookups type", typeof(UCC6TemporaryStorageFilterLookups), filterBizObj.Lookups.GetType());
		}

		public void TestCompanyFilter() => AssertModuleTextFilter(AsycudaManifestHeaderSchema.Constants.AMA_RN_NKCountry, "Country");

		public void TestApplicationCodeFilter() => AssertModuleTextFilter(AsycudaManifestHeaderSchema.Constants.AMA_ApplicationCode, "Application Code");

		public void TestJobNumberFilter() => AssertModuleTextFilter(AsycudaManifestHeaderSchema.Constants.AMA_JobReference, "Job #");

		public void TestLRNFilter() => AssertModuleTextFilter<ReferenceNumberSubGroup>(nameof(TemporaryStorageHeader.LRN), "LRN", true);

		public void TestMRNFilter() => AssertModuleTextFilter<ReferenceNumberSubGroup>(nameof(TemporaryStorageHeader.MRN), "MRN", true);

		public void TestMessageTypeFilter() => AssertModuleTextFilter(AsycudaManifestHeaderSchema.Constants.AMA_MessageType, "Message Type", true);

		public void TestCustomsStatusFilter() => AssertModuleTextFilter(nameof(TemporaryStorageHeader.CustomsStatus), "Customs Status", true);

		public void TestTransporationModeFilter() => AssertModuleTextFilter(AsycudaManifestHeaderSchema.Constants.AMA_TransportMode, "Transportation Mode", true);

		public void TestTransportTypeFilter() => AssertModuleTextFilter(nameof(TemporaryStorageHeader.TransportType), "Transport Type", true, "10", "20");

		public void TestTransportIDFilter() => AssertModuleTextFilter(nameof(TemporaryStorageHeader.ArrivalTransportMeansCode), "Transport ID", true);

		public void TestCustomsOfficeFilter() => AssertModuleTextFilter(AsycudaManifestHeaderSchema.Constants.AMA_CustomsOffice, "Customs Office");

		public void TestBranchFilter() => AssertModuleGuidFilter(AsycudaManifestHeaderSchema.Constants.AMA_GB, "Branch");

		public void TestDeclarantFilter() => AssertModuleGuidFilter(AsycudaManifestHeaderSchema.Constants.AMA_OA_Declarant, "Declarant");

		public void TestConsigneeFilter()
		{
			var header1 = GetNewTemporaryStorageHeader();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var bills1 = header1.Bills.AddNew();
			bills1.ABL_OA_Consignee = org1.MainAddress.PK;

			var header2 = GetNewTemporaryStorageHeader();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var bills2 = header2.Bills.AddNew();
			bills2.ABL_OA_Consignee = org2.MainAddress.PK;

			Factory.Save();

			AssertMatchesGuidFilter(header1, header2, org1.PK, org2.PK, "Consignee");
		}

		public void TestConsignorFilter()
		{
			var header1 = GetNewTemporaryStorageHeader();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var bills1 = header1.Bills.AddNew();
			bills1.ABL_OA_Shipper = org1.MainAddress.PK;

			var header2 = GetNewTemporaryStorageHeader();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var bills2 = header2.Bills.AddNew();
			bills2.ABL_OA_Shipper = org2.MainAddress.PK;

			Factory.Save();

			AssertMatchesGuidFilter(header1, header2, org1.PK, org2.PK, "Consignor");
		}

		public void TestNotifyFilter()
		{
			var header1 = GetNewTemporaryStorageHeader();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var bills1 = header1.Bills.AddNew();
			bills1.ABL_OA_NotifyParty = org1.MainAddress.PK;

			var header2 = GetNewTemporaryStorageHeader();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var bills2 = header2.Bills.AddNew();
			bills2.ABL_OA_NotifyParty = org2.MainAddress.PK;

			Factory.Save();

			AssertMatchesGuidFilter(header1, header2, org1.PK, org2.PK, "Notify");
		}

		public void TestBillNumberFilter()
		{
			var header1 = GetNewTemporaryStorageHeader();
			var bills1 = header1.Bills.AddNew();
			bills1.ABL_BillNumber = "Bill1";

			var header2 = GetNewTemporaryStorageHeader();
			var bills2 = header2.Bills.AddNew();
			bills2.ABL_BillNumber = "Bill2";
			Factory.Save();

			AssertMatchesPropertyFilter(header1, header2, bills1.ABL_BillNumber, bills2.ABL_BillNumber, "Bill Number");
		}

		public void TestTariffFilter()
		{
			var header1 = GetNewTemporaryStorageHeader();
			var bills1 = header1.Bills.AddNew();
			var packedItem1 = bills1.PackedItems.AddNew();
			packedItem1.API_FormattedTariff = "1201";

			var header2 = GetNewTemporaryStorageHeader();
			var bills2 = header2.Bills.AddNew();
			var packedItem2 = bills2.PackedItems.AddNew();
			packedItem2.API_FormattedTariff = "1207";
			Factory.Save();

			AssertMatchesPropertyFilter(header1, header2, packedItem1.API_FormattedTariff, packedItem2.API_FormattedTariff, "Tariff");
		}

		public void TestPresenterFilter() => AssertModuleGuidFilter(AsycudaManifestHeaderSchema.Constants.AMA_OA_Presenter, "Presenter");

		public void TestRepresentativeFilter() => AssertModuleGuidFilter(AsycudaManifestHeaderSchema.Constants.AMA_OA_Representative, "Representative");

		public void TestPresentationDate() => AssertModuleDateFilter(AsycudaManifestHeaderSchema.Constants.AMA_DateAtCustomsOffice, "Presentation Date");

		public void TestPackingTypeFilter()
		{
			var header1 = GetNewTemporaryStorageHeader();
			var bills1 = header1.Bills.AddNew();
			var packs1 = bills1.Packs.AddNew();
			packs1.APA_PackUQ = "4B";

			var header2 = GetNewTemporaryStorageHeader();
			var bills2 = header2.Bills.AddNew();
			var packs2 = bills2.Packs.AddNew();
			packs2.APA_PackUQ = "4A";
			Factory.Save();

			AssertMatchesPropertyFilter(header1, header2, packs1.APA_PackUQ, packs2.APA_PackUQ, "Packing Type");
		}

		public void TestPackingMarksFilter()
		{
			var header1 = GetNewTemporaryStorageHeader();
			var bills1 = header1.Bills.AddNew();
			var packs1 = bills1.Packs.AddNew();
			packs1.APA_MarksAndNumbers = "Marks1";

			var header2 = GetNewTemporaryStorageHeader();
			var bills2 = header2.Bills.AddNew();
			var packs2 = bills2.Packs.AddNew();
			packs2.APA_MarksAndNumbers = "Marks2";
			Factory.Save();

			AssertMatchesPropertyFilter(header1, header2, packs1.APA_MarksAndNumbers, packs2.APA_MarksAndNumbers, "Packing Marks");
		}

		public void TestPresentationCustomsOfficeFilter()
		{
			var header1 = GetNewTemporaryStorageHeader();
			var header2 = GetNewTemporaryStorageHeader();

			var officeCode1 = GetNewEuOfficeCode();
			officeCode1.CY_ParentID = header1.PK;
			officeCode1.CY_Data = "AB";

			var officeCode2 = GetNewEuOfficeCode();
			officeCode2.CY_ParentID = header2.PK;
			officeCode2.CY_Data = "CD";

			Factory.Save();

			var filterStripBizObj = new UCC6TemporaryStorageFilterStripBusinessObject();
			var textFilter = (ModuleTextFilter)filterStripBizObj["Presentation Customs Office"];
			textFilter.IsActive = true;

			textFilter.Property = ZString.Empty;

			var collection = new TemporaryStorageHeaderCollection(Factory);
			collection.AdditionalFilter = filterStripBizObj.Filter;
			AssertEquals(2, collection.Count);
			AssertEquals(header1.PK, collection[0].PK);
			AssertEquals(header2.PK, collection[1].PK);

			textFilter.Property = "AB";

			collection.AdditionalFilter = filterStripBizObj.Filter;

			AssertEquals(1, collection.Count);
			AssertEquals(header1.PK, collection[0].PK);
		}

		public void TestSupportingDocumentsTypeFilter()
		{
			var header1 = GetNewTemporaryStorageHeader();
			var document1 = header1.Bills.AddNew().SupportingDocuments.AddNew();
			document1.CSI_Code = "A012";

			var header2 = GetNewTemporaryStorageHeader();
			var document2 = header2.Bills.AddNew().PreviousDocuments.AddNew();
			document2.CSI_Code = "A012";

			var header3 = GetNewTemporaryStorageHeader();
			var item1 = header3.Bills.AddNew().PackedItems.AddNew();
			var document3 = item1.SupportingDocuments.AddNew();
			document3.CSI_Code = "N705";

			var header4 = GetNewTemporaryStorageHeader();
			var item2 = header4.Bills.AddNew().PackedItems.AddNew();
			var document4 = item2.PreviousDocuments.AddNew();
			document4.CSI_Code = "N705";

			Factory.Save();

			var filterStripBizObj = new UCC6TemporaryStorageFilterStripBusinessObject();
			var textFilter = (ModuleNkFilter)filterStripBizObj["Supporting Documents Type"];

			AssertNotNull("[PRE-REQ] Supporting Documents Type filter", textFilter);

			textFilter.IsActive = true;
			var collection = new TemporaryStorageHeaderCollection(Factory);

			CombineAssertions("Bill SupportingDocument", () =>
			{
				textFilter.Property = "A012";
				collection.AdditionalFilter = filterStripBizObj.Filter;

				AssertEquals("Count", 1, collection.Count);
				AssertEquals("PK", header1.PK, collection[0].PK);
			});

			CombineAssertions("PackedItem SupportingDocument", () =>
			{
				textFilter.Property = "N705";
				collection.AdditionalFilter = filterStripBizObj.Filter;

				AssertEquals("Count", 1, collection.Count);
				AssertEquals("PK", header3.PK, collection[0].PK);
			});
		}

		public void TestSupportingDocumentsNumberFilter()
		{
			var header1 = GetNewTemporaryStorageHeader();
			var document1 = header1.Bills.AddNew().SupportingDocuments.AddNew();
			document1.CSI_ReferenceNumber = "123";

			var header2 = GetNewTemporaryStorageHeader();
			var document2 = header2.Bills.AddNew().PreviousDocuments.AddNew();
			document2.CSI_ReferenceNumber = "123";

			var header3 = GetNewTemporaryStorageHeader();
			var item1 = header3.Bills.AddNew().PackedItems.AddNew();
			var document3 = item1.SupportingDocuments.AddNew();
			document3.CSI_ReferenceNumber = "456";

			var header4 = GetNewTemporaryStorageHeader();
			var item2 = header4.Bills.AddNew().PackedItems.AddNew();
			var document4 = item2.PreviousDocuments.AddNew();
			document4.CSI_ReferenceNumber = "456";

			Factory.Save();

			var filterStripBizObj = new UCC6TemporaryStorageFilterStripBusinessObject();
			var textFilter = (ModuleTextFilter)filterStripBizObj["Supporting Documents Number"];

			AssertNotNull("[PRE-REQ] Supporting Documents Type filter", textFilter);

			textFilter.IsActive = true;
			var collection = new TemporaryStorageHeaderCollection(Factory);

			CombineAssertions("Bill SupportingDocument", () =>
			{
				textFilter.Property = "123";
				collection.AdditionalFilter = filterStripBizObj.Filter;

				AssertEquals("Count", 1, collection.Count);
				AssertEquals("PK", header1.PK, collection[0].PK);
			});

			CombineAssertions("PackedItem SupportingDocument", () =>
			{
				textFilter.Property = "456";
				collection.AdditionalFilter = filterStripBizObj.Filter;

				AssertEquals("Count", 1, collection.Count);
				AssertEquals("PK", header3.PK, collection[0].PK);
			});
		}

		ModuleTextFilter AssertModuleTextFilter(string propertyName, string filterName, bool saving = false, string propertyValue1 = "AB", string propertyValue2 = "CD")
		{
			var header1 = GetNewTemporaryStorageHeader();
			header1[propertyName] = propertyValue1;

			var header2 = GetNewTemporaryStorageHeader();
			header2[propertyName] = propertyValue2;

			if (saving)
			{
				Factory.Save();
			}

			var filterStripBizObj = new UCC6TemporaryStorageFilterStripBusinessObject();
			var textFilter = (ModuleTextFilter)filterStripBizObj[filterName];
			textFilter.IsActive = true;

			textFilter.Property = ZString.Empty;

			var collection = new TemporaryStorageHeaderCollection(Factory);
			collection.AdditionalFilter = filterStripBizObj.Filter;
			AssertEquals(2, collection.Count);
			AssertEquals(header1.PK, collection[0].PK);
			AssertEquals(header2.PK, collection[1].PK);

			textFilter.Property = propertyValue1;

			collection.AdditionalFilter = filterStripBizObj.Filter;
			AssertEquals(1, collection.Count);
			AssertEquals(header1.PK, collection[0].PK);

			return textFilter;
		}

		void AssertModuleTextFilter<T>(string propertyName, string filterName, bool saving = false)
		{
			var textFilter = AssertModuleTextFilter(propertyName, filterName, saving);
			AssertType<T>(textFilter.SubGroup);
		}

		void AssertModuleGuidFilter(string propertyName, string filterName)
		{
			var header1 = GetNewTemporaryStorageHeader();
			header1[propertyName] = ZGuid.NewZGuid();

			var header2 = GetNewTemporaryStorageHeader();
			header2[propertyName] = ZGuid.NewZGuid();

			var filterStripBizObj = new UCC6TemporaryStorageFilterStripBusinessObject();
			var guidFilter = (ModuleGuidFilter)filterStripBizObj[filterName];
			guidFilter.IsActive = true;

			guidFilter.Property = ZGuid.Empty;

			var collection = new TemporaryStorageHeaderCollection(Factory);
			collection.AdditionalFilter = filterStripBizObj.Filter;
			AssertEquals(2, collection.Count);
			AssertEquals(header1.PK, collection[0].PK);
			AssertEquals(header2.PK, collection[1].PK);

			guidFilter.Property = (ZGuid)header1[propertyName];

			collection.AdditionalFilter = filterStripBizObj.Filter;
			AssertEquals(1, collection.Count);
			AssertEquals(header1.PK, collection[0].PK);
		}

		void AssertModuleDateFilter(string propertyName, string filterName)
		{
			var today = ZDateTime.Today;

			var header1 = GetNewTemporaryStorageHeader();
			header1[propertyName] = today;

			var header2 = GetNewTemporaryStorageHeader();
			header2[propertyName] = today.AddDays(-1);

			var filterStripBizObj = new UCC6TemporaryStorageFilterStripBusinessObject();
			var dateFilter = (ModuleDateFilter)filterStripBizObj[filterName];
			dateFilter.IsActive = true;
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			dateFilter.Property1 = ZDateTime.Empty;
			dateFilter.Property2 = ZDateTime.Empty;

			var collection = new TemporaryStorageHeaderCollection(Factory);
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

		void AssertMatchesPropertyFilter(TemporaryStorageHeader header1, TemporaryStorageHeader header2, ZString property1, ZString property2, ZString filterName)
		{
			CombineAssertions(() =>
			{
				var filterStripBO = new UCC6TemporaryStorageFilterStripBusinessObject();
				var textFilter = (ModuleTextFilter)filterStripBO[filterName];
				AssertNotNull(textFilter);
				textFilter.IsActive = true;

				textFilter.Property = property1;
				AssertEquals("header1 does match filter", true, header1.MatchesFilter(filterStripBO.Filter));
				AssertEquals("header2 does not match filter", false, header2.MatchesFilter(filterStripBO.Filter));

				textFilter.Property = property2;
				AssertEquals("header1 does not match filter", false, header1.MatchesFilter(filterStripBO.Filter));
				AssertEquals("header2 does match filter", true, header2.MatchesFilter(filterStripBO.Filter));
			});
		}

		void AssertMatchesGuidFilter(TemporaryStorageHeader header1, TemporaryStorageHeader header2, ZGuid org1Pk, ZGuid org2Pk, ZString filterName)
		{
			CombineAssertions(() =>
			{
				var filterStripBO = new UCC6TemporaryStorageFilterStripBusinessObject();
				var guidFilter = (ModuleGuidFilter)filterStripBO[filterName];
				AssertNotNull(guidFilter);
				guidFilter.IsActive = true;

				guidFilter.Property = org1Pk;
				AssertEquals("header1 does match filter", true, header1.MatchesFilter(filterStripBO.Filter));
				AssertEquals("header2 does not match filter", false, header2.MatchesFilter(filterStripBO.Filter));

				guidFilter.Property = org2Pk;
				AssertEquals("header1 does not match filter", false, header1.MatchesFilter(filterStripBO.Filter));
				AssertEquals("header2 does match filter", true, header2.MatchesFilter(filterStripBO.Filter));
			});
		}

		public void TestPreviousDocumentsNumberFilter()
		{
			var header1 = GetNewTemporaryStorageHeader();
			var document1 = header1.Bills.AddNew().PreviousDocuments.AddNew();
			document1.CSI_ReferenceNumber = "123";

			var header2 = GetNewTemporaryStorageHeader();
			var document2 = header2.Bills.AddNew().SupportingDocuments.AddNew();
			document2.CSI_ReferenceNumber = "123";

			var header3 = GetNewTemporaryStorageHeader();
			var document3 = header3.Bills.AddNew().PackedItems.AddNew().PreviousDocuments.AddNew();
			document3.CSI_ReferenceNumber = "456";

			var header4 = GetNewTemporaryStorageHeader();
			var document4 = header3.Bills.AddNew().PackedItems.AddNew().SupportingDocuments.AddNew();
			document4.CSI_ReferenceNumber = "456";

			Factory.Save();

			var filterStripBizObj = new UCC6TemporaryStorageFilterStripBusinessObject();
			var textFilter = (ModuleTextFilter)filterStripBizObj["Previous Document Number"];
			textFilter.IsActive = true;
			var collection = new TemporaryStorageHeaderCollection(Factory);

			CombineAssertions("Bill PreviousDocument", () =>
			{
				textFilter.Property = "123";
				collection.AdditionalFilter = filterStripBizObj.Filter;

				AssertEquals("Count", 1, collection.Count);
				AssertEquals("PK", header1.PK, collection[0].PK);
			});

			CombineAssertions("PackedItem PreviousDocument", () =>
			{
				textFilter.Property = "456";
				collection.AdditionalFilter = filterStripBizObj.Filter;

				AssertEquals("Count", 1, collection.Count);
				AssertEquals("PK", header3.PK, collection[0].PK);
			});
		}

		public void TestPreviousDocumentsTypeFilter()
		{
			var header1 = GetNewTemporaryStorageHeader();
			var bill = header1.Bills.AddNew();
			var document1 = bill.PreviousDocuments.AddNew();
			document1.CSI_Code = "NNN";

			var header2 = GetNewTemporaryStorageHeader();
			var bill2 = header2.Bills.AddNew();
			var document2 = bill2.SupportingDocuments.AddNew();
			document2.CSI_Code = "NNN";

			var header3 = GetNewTemporaryStorageHeader();
			var item1 = header3.Bills.AddNew().PackedItems.AddNew();
			var document3 = item1.PreviousDocuments.AddNew();
			document3.CSI_Code = "337";

			var header4 = GetNewTemporaryStorageHeader();
			var item2 = header4.Bills.AddNew().PackedItems.AddNew();
			var document4 = item2.SupportingDocuments.AddNew();
			document4.CSI_Code = "337";

			Factory.Save();

			var filterStripBizObj = new UCC6TemporaryStorageFilterStripBusinessObject();
			var textFilter = (ModuleNkFilter)filterStripBizObj["Previous Document Type"];
			textFilter.IsActive = true;
			var collection = new TemporaryStorageHeaderCollection(Factory);

			CombineAssertions("Bill PreviousDocument", () =>
			{
				textFilter.Property = "NNN";
				collection.AdditionalFilter = filterStripBizObj.Filter;

				AssertEquals("Count", 1, collection.Count);
				AssertEquals("PK", header1.PK, collection[0].PK);
			});

			CombineAssertions("PackedItem PreviousDocument", () =>
			{
				textFilter.Property = "337";
				collection.AdditionalFilter = filterStripBizObj.Filter;

				AssertEquals("Count", 1, collection.Count);
				AssertEquals("PK", header3.PK, collection[0].PK);
			});
		}

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

		TemporaryStorageHeader GetNewTemporaryStorageHeader()
		{
			var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.France;
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.TemporaryStorage;
			return header;
		}

		EuOfficeCode GetNewEuOfficeCode()
		{
			var code = Factory.NewWithValidTestData<EuOfficeCode>();
			code.CY_ParentTableCode = AsycudaManifestHeaderSchema.Constants.Prefix;
			code.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;
			return code;
		}

		protected override void SetUp()
		{
			base.SetUp();

			if (GlbCompany.CurrentCompany.Country.Code != Core.Constants.CountryCodes.France)
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.France);
			}
		}

		List<Type> ExpectedFilterInflatorTypes =>
		[
			typeof(ApplicationCodeFilterInflator),
			typeof(BillNumberFilterInflator),
			typeof(BranchFilterInflator),
			typeof(ConsigneeFilterInflator),
			typeof(ConsignorFilterInflator),
			typeof(CountryFilterInflator),
			typeof(CustomsOfficeFilterInflator),
			typeof(CustomsStatusFilterInflator),
			typeof(DeclarantFilterInflator),
			typeof(JobNumberFilterInflator),
			typeof(LocalReferenceNumberFilterInflator),
			typeof(MessageTypeFilterInflator),
			typeof(MovementReferenceNumberFilterInflator),
			typeof(NotifyFilterInflator),
			typeof(PackingMarksFilterInflator),
			typeof(PackingTypeFilterInflator),
			typeof(PresentationCustomsOfficeFilterInflator),
			typeof(PresentationDateFilterInflator),
			typeof(PresenterFilterInflator),
			typeof(PreviousDocumentNumberFilterInflator),
			typeof(UCC6TemporaryStoragePreviousDocumentTypeFilterInflator),
			typeof(RepresentativeFilterInflator),
			typeof(SupportingDocumentNumberFilterInflator),
			typeof(UCC6TemporaryStorageSupportingDocumentTypeFilterInflator),
			typeof(TariffFilterInflator),
			typeof(TransportationModeFilterInflator),
			typeof(TransportIdFilterInflator),
			typeof(TransportTypeFilterInflator)
		];

		sealed class UCC6TemporaryStorageFilterStripBusinessObjectForTest : UCC6TemporaryStorageFilterStripBusinessObject
		{
			public UCC6TemporaryStorageFilterInflatorFactory GetFilterInflatorFactory_Exposed() => GetFilterInflatorFactory();
			public List<IFilterInflator> GetFilterInflators_Exposed() => GetFilterInflators();
		}
	}
}
