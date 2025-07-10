using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Module.Testing
{
	[TestedType(typeof(EUH7BillFilterBusinessObject))]
	public class EUH7BillFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new EUH7BillFilterBusinessObject();

		protected virtual CodeDescriptionPairList GetExpectedMessageStatusList() => Factory.GetCachedValue<LogicalStatusList>();

		protected virtual CodeDescriptionPairList GetExpectedCustomStatusList(FilterStripBusinessObject filterObj) => ((EUH7BillFilterBusinessObject)filterObj).Lookups.CustomsStatusList;

		public void TestBillNumberFilter()
		{
			var asyheader1 = Factory.New<AsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			var bill1 = asyheader1.Bills.AddNew();
			bill1.ABL_BillNumber = "123";
			var bill2 = asyheader1.Bills.AddNew();
			bill2.ABL_BillNumber = "456";
			Factory.Save();

			var filterObj = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterObj[EUH7BillFilterBusinessObject.Descriptions.BillNumber];
			AssertNotNull(filter);
			filter.IsActive = true;
			filter.Property = "123";
			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		public void TestJobReferenceFilter()
		{
			var asyheader1 = Factory.New<AsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			var bill1 = asyheader1.Bills.AddNew();
			bill1.ABL_BillNumber = "123";

			var asyheader2 = Factory.New<AsycudaManifestHeader>();
			asyheader2.AMA_JobReference = "MAN000002";
			var bill2 = asyheader2.Bills.AddNew();
			bill2.ABL_BillNumber = "456";
			Factory.Save();

			var filterObj = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterObj[EUH7BillFilterBusinessObject.Descriptions.JobReference];
			AssertNotNull(filter);
			filter.IsActive = true;
			filter.Property = "MAN000001";

			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		public void TestManifestNumberFilter()
		{
			var asyheader1 = Factory.New<AsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			asyheader1.AMA_MasterBill = "123";
			var bill1 = asyheader1.Bills.AddNew();

			var asyheader2 = Factory.New<AsycudaManifestHeader>();
			asyheader2.AMA_JobReference = "MAN000002";
			asyheader2.AMA_MasterBill = "456";
			var bill2 = asyheader2.Bills.AddNew();
			Factory.Save();

			var filterObj = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterObj[EUH7BillFilterBusinessObject.Descriptions.ManifestNumber];
			AssertNotNull(filter);
			filter.IsActive = true;
			filter.Property = "123";

			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		public virtual void TestMRNFilter()
		{
			var asyheader1 = Factory.New<AsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			var bill1 = CreateBillAndEntry(asyheader1, null, "MRN", "MRN1");
			var bill2 = CreateBillAndEntry(asyheader1, null, "MRN", "MRN2");
			var bill3 = CreateBillAndEntry(asyheader1, null, "LRN", "MRN1");

			Factory.Save();

			var filterObj = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterObj[EUH7BillFilterBusinessObject.Descriptions.MRN];
			AssertNotNull(filter);
			filter.IsActive = true;
			filter.Property = "MRN1";

			CombineAssertions(() =>
			{
				Assert("MRN&MRN1",bill1.MatchesFilter(filterObj.Filter));
				Assert("MRN&MRN2", !bill2.MatchesFilter(filterObj.Filter));
				Assert("LRN&MRN1", !bill3.MatchesFilter(filterObj.Filter));
			});
		}

		public virtual void TestLRNFilter()
		{
			var asyheader1 = Factory.New<AsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			var bill1 = CreateBillAndEntry(asyheader1, null, "LRN", "LRN1");
			var bill2 = CreateBillAndEntry(asyheader1, null, "LRN", "LRN2");
			var bill3 = CreateBillAndEntry(asyheader1, null, "MRN", "LRN1");

			Factory.Save();

			var filterObj = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterObj[EUH7BillFilterBusinessObject.Descriptions.LRN];
			AssertNotNull(filter);
			filter.IsActive = true;
			filter.Property = "LRN1";

			CombineAssertions(() =>
			{
				Assert("LRN&LRN1", bill1.MatchesFilter(filterObj.Filter));
				Assert("LRN&LRN2", !bill2.MatchesFilter(filterObj.Filter));
				Assert("MRN&LRN1", !bill3.MatchesFilter(filterObj.Filter));
			});
		}

		public void TestBranchFilter()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch1 = company.Branches.AddNew();
			branch1.GB_Code = "1";
			var branch2 = company.Branches.AddNew();
			branch2.GB_Code = "2";

			var asyheader1 = Factory.New<AsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			var bill1 = asyheader1.Bills.AddNew();
			asyheader1.AMA_GB = branch1.PK;

			var asyheader2 = Factory.New<AsycudaManifestHeader>();
			asyheader2.AMA_JobReference = "MAN000002";
			var bill2 = asyheader2.Bills.AddNew();
			asyheader2.AMA_GB = branch2.PK;
			Factory.Save();

			var filterObj = GetNewFilterStripBusinessObject();
			var filter = (ModuleGuidFilter)filterObj[EUH7BillFilterBusinessObject.Descriptions.Branch];
			AssertNotNull(filter);
			filter.IsActive = true;
			filter.Property = branch1.PK;

			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		public void TestCustomsOfficeFilter()
		{
			var asyheader1 = Factory.New<AsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			asyheader1.AMA_CustomsOffice = "Office1";
			var bill1 = asyheader1.Bills.AddNew();

			var asyheader2 = Factory.New<AsycudaManifestHeader>();
			asyheader2.AMA_JobReference = "MAN000002";
			asyheader2.AMA_CustomsOffice = "Office2";
			var bill2 = asyheader2.Bills.AddNew();
			Factory.Save();

			var filterObj = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterObj[EUH7BillFilterBusinessObject.Descriptions.CustomsOffice];
			AssertNotNull(filter);
			filter.IsActive = true;
			filter.Property = "Office1";

			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		public void TestDeclarantFilter()
		{
			var asyheader1 = Factory.New<AsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			var bill1 = asyheader1.Bills.AddNew();
			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			asyheader1.AMA_OA_Declarant = address1.PK;

			var asyheader2 = Factory.New<AsycudaManifestHeader>();
			asyheader2.AMA_JobReference = "MAN000002";
			var bill2 = asyheader2.Bills.AddNew();
			var address2 = Factory.NewWithValidTestData<OrgAddress>();
			asyheader2.AMA_OA_Declarant = address2.PK;
			Factory.Save();

			var filterObj = GetNewFilterStripBusinessObject();
			var filter = (ModuleGuidFilter)filterObj[EUH7BillFilterBusinessObject.Descriptions.Declarant];
			AssertNotNull(filter);
			filter.IsActive = true;
			filter.Property = address1.PK;

			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		public void TestRepresentiveFilter()
		{
			var asyheader1 = Factory.New<AsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			var bill1 = asyheader1.Bills.AddNew();
			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			asyheader1.AMA_OA_Representative = address1.PK;

			var asyheader2 = Factory.New<AsycudaManifestHeader>();
			asyheader2.AMA_JobReference = "MAN000002";
			var bill2 = asyheader2.Bills.AddNew();
			var address2 = Factory.NewWithValidTestData<OrgAddress>();
			asyheader2.AMA_OA_Representative = address2.PK;
			Factory.Save();

			var filterObj = GetNewFilterStripBusinessObject();
			var filter = (ModuleGuidFilter)filterObj[EUH7BillFilterBusinessObject.Descriptions.Representative];
			AssertNotNull(filter);
			filter.IsActive = true;
			filter.Property = address1.PK;

			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		public void TestMessageStatusFilter()
		{
			var asyheader1 = Factory.New<AsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			var bill1 = asyheader1.Bills.AddNew();
			bill1.ABL_MessageStatus = "123";
			var bill2 = asyheader1.Bills.AddNew();
			bill2.ABL_MessageStatus = "456";
			Factory.Save();

			var filterObj = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterObj[EUH7BillFilterBusinessObject.Descriptions.MessageStatus];
			AssertNotNull(filter);
			filter.IsActive = true;
			filter.Property = "123";

			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));

			var actualFilterList = filter.List;
			var expectFilterList = GetExpectedMessageStatusList();
			AssertEquals("MessageStatusFilter's list should be the same", expectFilterList, actualFilterList);

			var comparisonOperator_List = filter.ComparisonOperator_List;
			var expectOperetor_List = new[] { ModuleNumberFilter.ComparisonConstants.Exact, ModuleNumberFilter.ComparisonConstants.NotEqual, ModuleNumberFilter.ComparisonConstants.IsBlank, ModuleNumberFilter.ComparisonConstants.IsNotBlank };
			AssertContainsExactElementsInAnyOrder("MessageStatusFilter's ComparisonOperator", expectOperetor_List, comparisonOperator_List.GetAllCodes());
			AssertEquals(comparisonOperator_List.DefaultCode, ModuleNumberFilter.ComparisonConstants.Exact);
		}

		public void TestCustomsStatusFilter()
		{
			var asyheader1 = Factory.New<AsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			var bill1 = asyheader1.Bills.AddNew();
			bill1.ABL_BillStatus = "123";
			var bill2 = asyheader1.Bills.AddNew();
			bill2.ABL_BillStatus = "456";

			Factory.Save();

			var filterObj = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterObj[EUH7BillFilterBusinessObject.Descriptions.CustomsStatus];
			AssertNotNull(filter);
			filter.IsActive = true;
			filter.Property = "123";

			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));

			var actualFilterList = filter.List;
			var expectFilterList = GetExpectedCustomStatusList(filterObj);
			AssertEquals("CustomsStatusFilter's list should be the same", expectFilterList, actualFilterList);

			var comparisonOperator_List = filter.ComparisonOperator_List;
			var expectOperetor_List = new[] { ModuleNumberFilter.ComparisonConstants.Exact, ModuleNumberFilter.ComparisonConstants.NotEqual, ModuleNumberFilter.ComparisonConstants.IsBlank, ModuleNumberFilter.ComparisonConstants.IsNotBlank };
			AssertContainsExactElementsInAnyOrder("CustomsStatusFilter's ComparisonOperator", expectOperetor_List, comparisonOperator_List.GetAllCodes());
			AssertEquals(comparisonOperator_List.DefaultCode, ModuleNumberFilter.ComparisonConstants.Exact);
		}

		public void TestMemberStateFilter()
		{
			var asyheader1 = Factory.New<ASYCUDA.Business.AsycudaManifestHeader>();
			asyheader1.SuspendCheckBusinessObjectType();
			asyheader1.AMA_JobReference = "MAN000001";
			asyheader1.AMA_RN_NKCountry = Core.Constants.CountryCodes.Latvia;
			var bill1 = asyheader1.Bills.AddNew();

			var asyheader2 = Factory.New<ASYCUDA.Business.AsycudaManifestHeader>();
			asyheader2.SuspendCheckBusinessObjectType();
			asyheader2.AMA_JobReference = "MAN000002";
			asyheader2.AMA_RN_NKCountry = "SG";
			var bill2 = asyheader2.Bills.AddNew();
			Factory.Save();

			var filterObj = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterObj[EUH7BillFilterBusinessObject.Descriptions.MemberState];
			AssertNotNull(filter);
			filter.IsActive = true;
			filter.Property = Core.Constants.CountryCodes.Latvia;

			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		public void TestOriginFilter()
		{
			var asyheader1 = Factory.New<AsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			var bill1 = asyheader1.Bills.AddNew();
			bill1.ABL_RL_NKOrigin = "IE001";
			var bill2 = asyheader1.Bills.AddNew();
			bill2.ABL_RL_NKOrigin = "RA001";
			Factory.Save();

			var filterObj = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterObj[EUH7BillFilterBusinessObject.Descriptions.Origin];
			AssertNotNull(filter);
			filter.IsActive = true;
			filter.Property = "IE001";

			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		public void TestFinalDestinationFilter()
		{
			var asyheader1 = Factory.New<AsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			var bill1 = asyheader1.Bills.AddNew();
			bill1.ABL_RL_NKFinalDestination = "IE001";
			var bill2 = asyheader1.Bills.AddNew();
			bill2.ABL_RL_NKFinalDestination = "RA001";
			Factory.Save();

			var filterObj = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterObj[EUH7BillFilterBusinessObject.Descriptions.FinalDestination];
			AssertNotNull(filter);
			filter.IsActive = true;
			filter.Property = "IE001";

			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		public void TestEstimateDateOfArrivalFilter()
		{
			var header1 = Factory.New<AsycudaManifestHeader>();
			header1.AMA_E_ARV = ZDateTime.Today.AddDays(6);
			var bill1 = header1.Bills.AddNew();

			var header2 = Factory.New<AsycudaManifestHeader>();
			header2.AMA_E_ARV = ZDateTime.Today.AddDays(20);
			var bill2 = header2.Bills.AddNew();
			Factory.Save();

			var filterObj = GetNewFilterStripBusinessObject();
			var filter = (ModuleDateFilter)filterObj[EUH7BillFilterBusinessObject.Descriptions.EstimateDateOfArrival];
			AssertNotNull(filter);
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Today.AddDays(5);
			filter.Property2 = ZDateTime.Today.AddDays(7);

			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		public void TestActualArrivalDateFilter()
		{
			var header1 = Factory.New<AsycudaManifestHeader>();
			header1.AMA_A_ARV = ZDateTime.Today.AddDays(6);
			var bill1 = header1.Bills.AddNew();

			var header2 = Factory.New<AsycudaManifestHeader>();
			header2.AMA_A_ARV = ZDateTime.Today.AddDays(20);
			var bill2 = header2.Bills.AddNew();
			Factory.Save();

			var filterObj = GetNewFilterStripBusinessObject();
			var filter = (ModuleDateFilter)filterObj[EUH7BillFilterBusinessObject.Descriptions.ActualArrivalDate];
			AssertNotNull(filter);
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Today.AddDays(5);
			filter.Property2 = ZDateTime.Today.AddDays(7);
			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		public void TestEstimatedDateOfDepartureFilter()
		{
			var header1 = Factory.New<AsycudaManifestHeader>();
			header1.AMA_E_DEP = ZDateTime.Today.AddDays(6);
			var bill1 = header1.Bills.AddNew();

			var header2 = Factory.New<AsycudaManifestHeader>();
			header2.AMA_E_DEP = ZDateTime.Today.AddDays(20);
			var bill2 = header2.Bills.AddNew();
			Factory.Save();

			var filterObj = GetNewFilterStripBusinessObject();
			var filter = (ModuleDateFilter)filterObj[EUH7BillFilterBusinessObject.Descriptions.EstimatedDateOfDeparture];
			AssertNotNull(filter);
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Today.AddDays(5);
			filter.Property2 = ZDateTime.Today.AddDays(7);
			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		public void TestMRNIssuedDateFilter()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			var bill1 = header.Bills.AddNew();
			var mrn1 = bill1.CustomsEntryNumbers.AddNew();
			mrn1.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			mrn1.CE_EntryNum = "1";
			mrn1.CE_IssueDate = ZDateTime.Today.AddDays(6);

			var bill2 = header.Bills.AddNew();
			var mrn2 = bill2.CustomsEntryNumbers.AddNew();
			mrn2.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			mrn2.CE_EntryNum = "2";
			mrn2.CE_IssueDate = ZDateTime.Today.AddDays(20);

			var bill3 = header.Bills.AddNew();

			var bill4 = header.Bills.AddNew();
			var mrn4 = bill4.CustomsEntryNumbers.AddNew();
			mrn4.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			mrn4.CE_EntryNum = "4";
			Factory.Save();

			var filterObj = GetNewFilterStripBusinessObject();
			var filter = (ModuleDateFilter)filterObj[EUH7BillFilterBusinessObject.Descriptions.MRNIssuedDate];
			AssertNotNull(filter);
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Today.AddDays(5);
			filter.Property2 = ZDateTime.Today.AddDays(7);

			CombineAssertions("When there is a range of Date filter applied", () =>
			{
				Assert(bill1.MatchesFilter(filterObj.Filter));
				Assert(!bill2.MatchesFilter(filterObj.Filter));
			});

			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			CombineAssertions("When 'HasNoDateEntered' filter is applied", () =>
			{
				Assert(bill3.MatchesFilter(filterObj.Filter));
				Assert(bill4.MatchesFilter(filterObj.Filter));
			});
		}

		public void TestReleaseDateFilter()
		{
			var header1 = Factory.New<AsycudaManifestHeader>();
			var bill1 = header1.Bills.AddNew();
			bill1.ABL_ReleaseDate = ZDate.Today.AddDays(6);
			var bill2 = header1.Bills.AddNew();
			bill2.ABL_ReleaseDate = ZDate.Today.AddDays(20);

			Factory.Save();

			var filterObj = GetNewFilterStripBusinessObject();
			var filter = (ModuleDateFilter)filterObj[EUH7BillFilterBusinessObject.Descriptions.ReleaseDate];
			AssertNotNull(filter);
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Today.AddDays(5);
			filter.Property2 = ZDateTime.Today.AddDays(7);

			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		public void TestNumbersAndReferencesFilters()
		{
			var filterObj = GetNewFilterStripBusinessObject();
			AssertEquals(FilterCategories.NumbersAndReferences, filterObj[EUH7BillFilterBusinessObject.Descriptions.BillNumber].Category);
			AssertEquals(FilterCategories.NumbersAndReferences, filterObj[EUH7BillFilterBusinessObject.Descriptions.JobReference].Category);
			AssertEquals(FilterCategories.NumbersAndReferences, filterObj[EUH7BillFilterBusinessObject.Descriptions.ManifestNumber].Category);
			AssertLRNAndMRNFilterCategory(filterObj);
		}

		public virtual void AssertLRNAndMRNFilterCategory(FilterStripBusinessObject filterObj)
		{
			AssertEquals(FilterCategories.NumbersAndReferences, filterObj[EUH7BillFilterBusinessObject.Descriptions.MRN].Category);
			AssertEquals(FilterCategories.NumbersAndReferences, filterObj[EUH7BillFilterBusinessObject.Descriptions.LRN].Category);
		}

		public void TestOrganizationsOrStaffFilters()
		{
			var filterObj = GetNewFilterStripBusinessObject();
			AssertEquals(FilterCategories.Organisations, filterObj[EUH7BillFilterBusinessObject.Descriptions.Branch].Category);
			AssertEquals(FilterCategories.Organisations, filterObj[EUH7BillFilterBusinessObject.Descriptions.CustomsOffice].Category);
			AssertEquals(FilterCategories.Organisations, filterObj[EUH7BillFilterBusinessObject.Descriptions.Declarant].Category);
			AssertEquals(FilterCategories.Organisations, filterObj[EUH7BillFilterBusinessObject.Descriptions.Representative].Category);
		}

		public void TestStatusAndFlagFilters()
		{
			var filterObj = GetNewFilterStripBusinessObject();
			AssertEquals(FilterCategories.StatusAndFlags, filterObj[EUH7BillFilterBusinessObject.Descriptions.MessageStatus].Category);
			AssertEquals(FilterCategories.StatusAndFlags, filterObj[EUH7BillFilterBusinessObject.Descriptions.CustomsStatus].Category);
		}

		public void TestLocationsFilters()
		{
			var filterObj = GetNewFilterStripBusinessObject();
			AssertEquals(FilterCategories.Locations, filterObj[EUH7BillFilterBusinessObject.Descriptions.MemberState].Category);
			AssertEquals(FilterCategories.Locations, filterObj[EUH7BillFilterBusinessObject.Descriptions.Origin].Category);
			AssertEquals(FilterCategories.Locations, filterObj[EUH7BillFilterBusinessObject.Descriptions.FinalDestination].Category);
		}

		public void TestDateFilters()
		{
			var filterObj = GetNewFilterStripBusinessObject();
			AssertEquals(FilterCategories.Dates, filterObj[EUH7BillFilterBusinessObject.Descriptions.EstimateDateOfArrival].Category);
			AssertEquals(FilterCategories.Dates, filterObj[EUH7BillFilterBusinessObject.Descriptions.ActualArrivalDate].Category);
			AssertEquals(FilterCategories.Dates, filterObj[EUH7BillFilterBusinessObject.Descriptions.EstimatedDateOfDeparture].Category);
			AssertEquals(FilterCategories.Dates, filterObj[EUH7BillFilterBusinessObject.Descriptions.MRNIssuedDate].Category);
			AssertEquals(FilterCategories.Dates, filterObj[EUH7BillFilterBusinessObject.Descriptions.ReleaseDate].Category);
		}

		public void TestCustomsDocsRequiredFilter()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			var bill3 = header.Bills.AddNew();
			var bill4 = header.Bills.AddNew();
			var bill5 = header.Bills.AddNew();

			var cusSupportingInfo1 = bill1.RequestedDocuments.AddNew();
			cusSupportingInfo1.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestOpened;

			var cusSupportingInfo2 = bill2.RequestedDocuments.AddNew();
			cusSupportingInfo2.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestCancelled;

			var cusSupportingInfo3 = bill3.RequestedDocuments.AddNew();
			cusSupportingInfo3.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.PhysicallyPresentDocument;

			var cusSupportingInfo4 = bill4.RequestedDocuments.AddNew();
			cusSupportingInfo4.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived;

			Factory.Save();

			var filterObj = GetNewFilterStripBusinessObject();
			var filter = (ModuleFlagsFilter)filterObj[EUH7BillFilterBusinessObject.Descriptions.CustomsDocsRequired];
			AssertNotNull(filter);
			filter.IsActive = true;

			filter.Property0 = true;
			CombineAssertions("When CustomsDocsRequired's checkbox is ticked", () =>
			{
				Assert(bill1.MatchesFilter(filterObj.Filter));
				Assert(!bill2.MatchesFilter(filterObj.Filter));
				Assert(bill3.MatchesFilter(filterObj.Filter));
				Assert(!bill4.MatchesFilter(filterObj.Filter));
				Assert(!bill5.MatchesFilter(filterObj.Filter));
			});

			filter.Property0 = false;
			CombineAssertions("When CustomsDocsRequired's checkbox is not ticked", () =>
			{
				Assert(bill1.MatchesFilter(filterObj.Filter));
				Assert(bill2.MatchesFilter(filterObj.Filter));
				Assert(bill3.MatchesFilter(filterObj.Filter));
				Assert(bill4.MatchesFilter(filterObj.Filter));
				Assert(bill5.MatchesFilter(filterObj.Filter));
			});
		}

		protected AsycudaBill CreateBillAndEntry(AsycudaManifestHeader header, string entryLineReference, string entryType, string entryNum)
		{
			var bill = header.Bills.AddNew();
			var entry = Factory.New<CusEntryNumber>();
			entry.CE_ParentID = bill.PK;
			entry.CE_ParentTable = bill.TableName;
			entry.CE_EntryLineReference = entryLineReference;
			entry.CE_EntryType = entryType;
			entry.CE_EntryNum = entryNum;

			return bill;
		}
	}
}
