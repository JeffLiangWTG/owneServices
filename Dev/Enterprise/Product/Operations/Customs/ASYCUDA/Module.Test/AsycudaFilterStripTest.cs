using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ApplicationCodeTypeList = Enterprise.Customs.ManifestBase.ApplicationCodeTypeList;

namespace Enterprise.Customs.ASYCUDA.Module.Testing
{
	[TestedType(typeof(AsycudaFilterStrip))]
	sealed class AsycudaFilterStripTest : FilterStripBusinessObjectTestCase
	{
		public void TestArrivalStatusForHouseLevel()
		{
			var asyheader1 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedStates, "IAM");
			asyheader1.FillWithValidTestData();
			asyheader1.AMA_JobReference = "MAN000001";
			asyheader1.AMA_AgentType = "CLD";
			asyheader1.AMA_RN_NKCountry = "US";
			var arrival1 = asyheader1.ArrivalHeaders.AddNew();
			var arrivalLine1 = arrival1.ArrivalDetails.AddNew();
			arrivalLine1.ATL_MessageStatus = string.Empty;

			var asyheader2 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "ALH");
			asyheader2.FillWithValidTestData();
			asyheader2.AMA_JobReference = "MAN000002";
			asyheader2.AMA_AgentType = "FWB";
			asyheader2.AMA_RN_NKCountry = "ZA";
			var arrival2 = asyheader2.ArrivalHeaders.AddNew();
			var arrivalLine2 = arrival2.ArrivalDetails.AddNew();
			arrivalLine2.ATL_MessageStatus = "NOT";

			var asyheader3 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.China, "ASY");
			asyheader3.FillWithValidTestData();
			asyheader3.AMA_JobReference = "MAN000003";
			asyheader3.AMA_AgentType = "FWB";
			asyheader3.AMA_MessageStatus = "AWA";
			asyheader3.AMA_ManifestType = "GHI";
			asyheader3.AMA_CustomsOffice = "GHI1";
			var arrival3 = asyheader3.ArrivalHeaders.AddNew();
			var arrivalLine3 = arrival3.ArrivalDetails.AddNew();
			arrivalLine3.ATL_MessageStatus = "AWA";
			Factory.Save();

			var filterObj = new AsycudaFilterStrip();
			var filter = (CountryRelatedFilter)filterObj[AsycudaFilterStrip.FilterConstants.ArrivalStatus];
			filter.IsActive = true;
			filter.Property2 = "NOT";

			var headers = Factory.Load<AsycudaManifestHeader>(filterObj.Filter);
			AssertEquals(true, headers.Any());
			Assert(headers.Any(x => x.PK == asyheader1.PK));
			Assert(headers.Any(x => x.PK == asyheader2.PK));
			Assert(headers.All(x => x.PK != asyheader3.PK));
		}

		public void TestCountryFilter()
		{
			var asyheader1 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedStates, "IAM");
			asyheader1.FillWithValidTestData();
			asyheader1.AMA_JobReference = "MAN000001";
			asyheader1.AMA_AgentType = "CLD";
			asyheader1.AMA_RN_NKCountry = "US";
			asyheader1.AMA_MessageStatus = "QUE";
			var bill1 = asyheader1.Bills.AddNew();

			var asyheader2 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "ALH");
			asyheader2.FillWithValidTestData();
			asyheader2.AMA_JobReference = "MAN000002";
			asyheader2.AMA_AgentType = "FWB";
			asyheader2.AMA_RN_NKCountry = "ZA";
			var bil2 = asyheader2.Bills.AddNew();
			Factory.Save();

			var filterObj = new AsycudaFilterStrip();
			var filter = (ModuleNkFilter)filterObj[AsycudaFilterStrip.FilterConstants.Country];
			filter.IsActive = true;
			filter.Property = "US";

			var headers = Factory.Load<AsycudaManifestHeader>(filterObj.Filter);
			AssertEquals(true, headers.Any());
			AssertEquals(true, headers.Any(x => x.PK == asyheader1.PK));
			AssertEquals(false, headers.Any(x => x.PK == asyheader2.PK));

			filterObj = new AsycudaFilterStrip(false);
			filter = filterObj[AsycudaFilterStrip.FilterConstants.Country] as ModuleNkFilter;
			AssertNull(filter);
		}

		public void TestAgentTypeFilter()
		{
			var asyheader1 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedStates, "IAM");
			asyheader1.FillWithValidTestData();
			asyheader1.AMA_JobReference = "MAN000001";
			asyheader1.AMA_AgentType = "CLD";
			asyheader1.AMA_RN_NKCountry = "US";
			asyheader1.AMA_MessageStatus = "QUE";
			var bill1 = asyheader1.Bills.AddNew();

			var asyheader2 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "ALH");
			asyheader2.FillWithValidTestData();
			asyheader2.AMA_JobReference = "MAN000002";
			asyheader2.AMA_AgentType = "FWB";
			asyheader2.AMA_RN_NKCountry = "ZA";
			var bil2 = asyheader2.Bills.AddNew();
			Factory.Save();

			var filterObj = new AsycudaFilterStrip();
			var filter = (ModuleTextFilter)filterObj[AsycudaFilterStrip.FilterConstants.AgentType];
			filter.IsActive = true;
			filter.Property = "CLD";

			var headers = Factory.Load<AsycudaManifestHeader>(filterObj.Filter);
			AssertEquals(true, headers.Any());
			AssertEquals(true, headers.Any(x => x.PK == asyheader1.PK));
			AssertEquals(false, headers.Any(x => x.PK == asyheader2.PK));
		}

		public void TestWorkFlowMilestoneDateFilterAdded()
		{
			var asyheader1 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedStates, "IAM");
			asyheader1.FillWithValidTestData();
			asyheader1.AMA_JobReference = "MAN000001";
			asyheader1.AMA_MasterBill = "123";
			asyheader1.AMA_RN_NKCountry = "US";
			asyheader1.AMA_MessageStatus = "QUE";
			var bill1 = asyheader1.Bills.AddNew();
			bill1.ABL_BolType = AsycudaBill.ChildBolCode;
			bill1.ABL_BillNumber = "123";
			var milestone1 = asyheader1.WorkflowItems.Milestones.AddNew();
			milestone1.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 2));
			milestone1.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2000, 1, 2)));

			var asyheader2 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "ALH");
			asyheader2.FillWithValidTestData();
			asyheader2.AMA_JobReference = "MAN000002";
			asyheader2.AMA_MasterBill = "456";
			asyheader2.AMA_RN_NKCountry = "ZA";
			var bil2 = asyheader2.Bills.AddNew();
			var milestone2 = asyheader2.WorkflowItems.Milestones.AddNew();
			milestone2.SetMilestoneScheduledDateForTest((new ZDateTimeOffset(new ZDateTime(2000, 1, 4))));

			Factory.Save();

			var allFilters = new DummyFilterStripBizOWithWorkflowFilters();
			var milestoneFilter = (WorkflowModuleFilter)allFilters["Milestone Date"];
			milestoneFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			milestoneFilter.Property1 = new ZDateTime(2000, 1, 1);
			milestoneFilter.Property2 = new ZDateTime(2000, 1, 3);
			milestoneFilter.IsActive = true;

			var dummyCollection = new DummyBusinessObjectCollection(Factory, allFilters.Filter);
			dummyCollection.Load();

			var filterObj = new AsycudaFilterStrip();
			var filter = (WorkflowModuleFilter)filterObj["Milestone Date"];
			filter.IsActive = true;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 1, 5);
			var headers = Factory.Load<AsycudaManifestHeader>(filterObj.Filter);
			AssertEquals(true, headers.Any());
			AssertEquals(true, headers.Any(x => x.PK == asyheader1.PK));
			AssertEquals(true, headers.Any(x => x.PK == asyheader2.PK));
		}

		public void TestMasterBillNumberFilter()
		{
			var asyheader1 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedStates, "IAM");
			asyheader1.FillWithValidTestData();
			asyheader1.AMA_JobReference = "MAN000001";
			asyheader1.AMA_MasterBill = "123";
			asyheader1.AMA_RN_NKCountry = "US";
			asyheader1.AMA_MessageStatus = "QUE";
			var bill1 = asyheader1.Bills.AddNew();
			bill1.ABL_BolType = AsycudaBill.ChildBolCode;
			bill1.ABL_BillNumber = "123";

			var asyheader2 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "ALH");
			asyheader2.FillWithValidTestData();
			asyheader2.AMA_JobReference = "MAN000002";
			asyheader2.AMA_MasterBill = "456";
			asyheader2.AMA_RN_NKCountry = "ZA";
			var bill2 = asyheader2.Bills.AddNew();
			Factory.Save();

			var filterObj = new AsycudaFilterStrip();
			var filter = (ModuleTextFilter)filterObj[AsycudaFilterStrip.FilterConstants.MasterBillNumber];
			filter.IsActive = true;
			filter.Property = "123";
			var headers = Factory.Load<AsycudaManifestHeader>(filterObj.Filter);
			AssertEquals(true, headers.Any());
			AssertEquals(true, headers.Any(x => x.PK == asyheader1.PK));
			AssertEquals(false, headers.Any(x => x.PK == asyheader2.PK));
		}

		public void TestCustomsOfficeFilter()
		{
			var asyheader1 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedStates, "IAM");
			asyheader1.FillWithValidTestData();
			asyheader1.AMA_JobReference = "MAN000001";
			asyheader1.AMA_AgentType = "CLD";
			asyheader1.AMA_MessageStatus = string.Empty;
			asyheader1.AMA_ManifestType = "DEF";
			asyheader1.AMA_CustomsOffice = "DEF1";

			var asyheader2 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "ALH");
			asyheader2.FillWithValidTestData();
			asyheader2.AMA_JobReference = "MAN000002";
			asyheader2.AMA_AgentType = "FWB";
			asyheader2.AMA_MessageStatus = "NOT";
			asyheader2.AMA_ManifestType = "DEF";
			asyheader2.AMA_CustomsOffice = "DEF1";

			var asyheader3 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.China, "ASY");
			asyheader3.FillWithValidTestData();
			asyheader3.AMA_JobReference = "MAN000003";
			asyheader3.AMA_AgentType = "FWB";
			asyheader3.AMA_MessageStatus = "AWA";
			asyheader3.AMA_ManifestType = "GHI";
			asyheader3.AMA_CustomsOffice = "GHI1";

			Factory.Save();

			var filterObj = new AsycudaFilterStrip();
			var filter = (CountryRelatedFilter)filterObj[AsycudaFilterStrip.FilterConstants.CustomsOffice];
			filter.IsActive = true;
			filter.Property1 = "ZA";
			filter.Property2 = "DEF1";

			var headers = Factory.Load<AsycudaManifestHeader>(filterObj.Filter);
			AssertEquals(true, headers.Any());
			Assert(headers.Any(x => x.PK == asyheader1.PK));
			Assert(headers.Any(x => x.PK == asyheader2.PK));
			Assert(headers.All(x => x.PK != asyheader3.PK));
		}

		public void TestManifestTypeFilter()
		{
			var asyheader1 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedStates, "IAM");
			var asyheader2 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "ALH");
			var asyheader3 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.China, "ASY");
			var asyheader4 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedStates, "EFM");
			var asyheader5 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Canada, "ASY");
			Factory.Save();

			var filterObj = new AsycudaFilterStrip();
			var filter = filterObj[AsycudaFilterStrip.FilterConstants.ManifestType] as DataGroupingRelatedFilter;
			AssertNotNull(filter);
			filter.IsActive = true;
			AssertEquals("filter.Category", FilterCategories.ModesAndTypes, filter.Category);
			AssertEquals("filter.Description", "Manifest Type", filter.Description);
			AssertEquals("filter.MultilingualDescription", "Manifest Type", filter.MultilingualDescription);
			AssertEquals("filter.Property2FieldType", FieldType.TextDropEdit, filter.Property2FieldType);
			AssertEquals("filter.Property2MaxLength", AsycudaManifestHeaderSchema.AMA_ManifestType.MaxLength, filter.Property2MaxLength);
			AssertEquals("filter.Property2ResourceString.Caption", "Manifest Type", filter.Property2ResourceString.Caption);
			AssertEquals("filter.UseProperty2ListGetterWhenProperty1IsEmpty", true, filter.UseProperty2ListGetterWhenProperty1IsEmpty);

			filter.Property1 = Core.Constants.CountryCodes.UnitedStates;
			filter.Property2 = ZString.Empty;
			var zq = filterObj.Filter;
			var headers = Factory.Load<AsycudaManifestHeader>(zq);
			AssertEquals(5, headers.Length);
			Assert(asyheader1.MatchesFilter(zq));
			Assert(asyheader2.MatchesFilter(zq));
			Assert(asyheader3.MatchesFilter(zq));
			Assert(asyheader4.MatchesFilter(zq));
			Assert(asyheader5.MatchesFilter(zq));

			filter.Property1 = ZString.Empty;
			filter.Property2 = "ASY";
			zq = filterObj.Filter;
			headers = Factory.Load<AsycudaManifestHeader>(zq);
			AssertEquals(2, headers.Length);
			Assert(!asyheader1.MatchesFilter(zq));
			Assert(!asyheader2.MatchesFilter(zq));
			Assert(asyheader3.MatchesFilter(zq));
			Assert(!asyheader4.MatchesFilter(zq));
			Assert(asyheader5.MatchesFilter(zq));

			filter.Property1 = Core.Constants.CountryCodes.Canada;
			filter.Property2 = "ASY";
			zq = filterObj.Filter;
			headers = Factory.Load<AsycudaManifestHeader>(zq);
			AssertEquals(1, headers.Length);
			Assert(!asyheader1.MatchesFilter(zq));
			Assert(!asyheader2.MatchesFilter(zq));
			Assert(!asyheader3.MatchesFilter(zq));
			Assert(!asyheader4.MatchesFilter(zq));
			Assert(asyheader5.MatchesFilter(zq));

			filter.Property1 = Core.Constants.CountryCodes.SouthAfrica;
			filter.Property2 = "ALH";
			zq = filterObj.Filter;
			headers = Factory.Load<AsycudaManifestHeader>(zq);
			AssertEquals(1, headers.Length);
			Assert(!asyheader1.MatchesFilter(zq));
			Assert(asyheader2.MatchesFilter(zq));
			Assert(!asyheader3.MatchesFilter(zq));
			Assert(!asyheader4.MatchesFilter(zq));
			Assert(!asyheader5.MatchesFilter(zq));

			var filter2 = filterObj[AsycudaFilterStrip.FilterConstants.ManifestType] as DataGroupingRelatedFilter;
			filter2.IsActive = true;
			filter2.Property1 = Core.Constants.CountryCodes.SouthAfrica;
			filter2.Property2 = "ALH";
			AssertSame("Property2List Cached", filter.Property2List, filter2.Property2List);
		}

		public void TestSpecificCircumstanceTypeFilter()
		{
			var asyHeader1 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.EuropeanUnion, "IC2");
			asyHeader1.AMA_JobReference = "MAN000001";
			asyHeader1.AMA_TransportMode = "ROA";

			var addOnColumn1 = Factory.New<GenAddOnColumn>();
			addOnColumn1.XA_ParentID = asyHeader1.PK;
			addOnColumn1.XA_ParentTableCode = "AMA";
			addOnColumn1.XA_Name = AsycudaManifestHeader.Schema.SpecificCircumstanceIndicator;
			addOnColumn1.XA_Type = AddOnColumnDataType.Codes.String;
			addOnColumn1.XA_Data = "F40";

			var asyHeader2 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.EuropeanUnion, "IC2");
			asyHeader2.AMA_JobReference = "MAN000002";
			asyHeader2.AMA_TransportMode = "ROA";

			var addOnColumn2 = Factory.New<GenAddOnColumn>();
			addOnColumn2.XA_ParentID = asyHeader2.PK;
			addOnColumn2.XA_ParentTableCode = "AMA";
			addOnColumn2.XA_Name = AsycudaManifestHeader.Schema.SpecificCircumstanceIndicator;
			addOnColumn2.XA_Type = AddOnColumnDataType.Codes.String;
			addOnColumn2.XA_Data = "F40";

			var asyHeader3 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.EuropeanUnion, "IC2");
			asyHeader3.AMA_JobReference = "MAN000003";
			asyHeader3.AMA_TransportMode = "AIR";

			Factory.Save();

			var filterObj = new AsycudaFilterStrip();
			var filter = (ModuleTextFilter)filterObj[AsycudaFilterStrip.FilterConstants.SpecificCircumstanceType];
			filter.IsActive = true;
			filter.Property = "F40";
			Assert(asyHeader1.MatchesFilter(filterObj.Filter));
			Assert(asyHeader2.MatchesFilter(filterObj.Filter));
			Assert(!asyHeader3.MatchesFilter(filterObj.Filter));
		}

		public void TestMessageStatusFilter()
		{
			var asyheader1 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedStates, "IAM");
			asyheader1.FillWithValidTestData();
			asyheader1.AMA_JobReference = "MAN000001";
			asyheader1.AMA_AgentType = "CLD";
			asyheader1.AMA_RN_NKCountry = "US";
			var bill1 = asyheader1.Bills.AddNew();
			bill1.ABL_MessageStatus = string.Empty;

			var asyheader2 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "ALH");
			asyheader2.FillWithValidTestData();
			asyheader2.AMA_JobReference = "MAN000002";
			asyheader2.AMA_AgentType = "FWB";
			asyheader2.AMA_RN_NKCountry = "ZA";
			var bill2 = asyheader2.Bills.AddNew();
			bill2.ABL_MessageStatus = "NOT";
			var bill3 = asyheader2.Bills.AddNew();
			bill3.ABL_MessageStatus = "AWA";
			Factory.Save();

			var filterObj = new AsycudaFilterStrip();
			var filter = (CountryRelatedFilter)filterObj[AsycudaFilterStrip.FilterConstants.MessageStatus];
			filter.IsActive = true;
			filter.Property2 = "NOT";

			var headers = Factory.Load<AsycudaManifestHeader>(filterObj.Filter);
			AssertEquals(true, headers.Any());
			Assert(headers.Any(x => x.PK == asyheader1.PK));
			Assert(headers.Any(x => x.PK == asyheader2.PK));

			filter.Property2 = "AWA";
			headers = Factory.Load<AsycudaManifestHeader>(filterObj.Filter);
			Assert(headers.All(x => x.PK != asyheader1.PK));
			Assert(headers.Any(x => x.PK == asyheader2.PK));
		}

		public void TestDateFilters()
		{
			AssertDateFilter(AsycudaFilterStrip.FilterConstants.EstimatedDateofDeparture, AsycudaBillSchema.ABL_E_DEP);
			AssertDateFilter(AsycudaFilterStrip.FilterConstants.EstimatedTimeArrival, AsycudaBillSchema.ABL_E_ARV);
		}

		public void TestCustomsStatusFilter()
		{
			var header1 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "ALH");
			var bill1 = header1.Bills.AddNew();
			bill1.ABL_BillStatus = "1";
			var header2 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedStates, "IAM");
			var bill2 = header2.Bills.AddNew();
			bill2.ABL_BillStatus = "2";
			var header3 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedKingdom, "ICS");
			var bill3 = header3.Bills.AddNew();
			bill3.ABL_BillStatus = "3";
			Factory.Save();

			var filterObj = new AsycudaFilterStrip();
			var filter = (CountryRelatedFilter)filterObj[AsycudaFilterStrip.FilterConstants.CustomsStatus];
			filter.IsActive = true;
			filter.Property1 = "US";
			filter.Property2 = "2";
			Assert(!header1.MatchesFilter(filterObj.Filter));
			Assert(header2.MatchesFilter(filterObj.Filter));
			Assert(!header3.MatchesFilter(filterObj.Filter));

			filterObj = new AsycudaFilterStrip();
			filter = (CountryRelatedFilter)filterObj[AsycudaFilterStrip.FilterConstants.CustomsStatus];
			filter.IsActive = true;
			filter.Property1 = "US";
			filter.Property2 = "3";
			Assert(!header1.MatchesFilter(filterObj.Filter));
			Assert(!header2.MatchesFilter(filterObj.Filter));
			Assert(header3.MatchesFilter(filterObj.Filter));
		}

		public void TestLocalReferenceNumberFilter()
		{
			var header1 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.EuropeanUnion, "IC2");
			var entry1 = Factory.New<CusEntryNumber>();
			entry1.CE_EntryType = "LRN";
			entry1.CE_ParentID = header1.PK;
			entry1.CE_ParentTable = "AsycudaManifestHeader";
			entry1.CE_EntryNum = "LRN1";

			var header2 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.EuropeanUnion, "IC2");
			var entry2 = Factory.New<CusEntryNumber>();
			entry2.CE_EntryType = "LRN";
			entry2.CE_ParentID = header2.PK;
			entry2.CE_ParentTable = "AsycudaManifestHeader";
			entry2.CE_EntryNum = "LRN2";

			var header3 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.EuropeanUnion, "IC2");
			var entry3 = Factory.New<CusEntryNumber>();
			entry3.CE_EntryType = "PRE";
			entry3.CE_ParentID = header3.PK;
			entry3.CE_ParentTable = "AsycudaManifestHeader";
			entry3.CE_EntryNum = "LRN1";

			Factory.Save();

			var filterObj = new AsycudaFilterStrip();
			var filter = (ModuleTextFilter)filterObj[AsycudaFilterStrip.FilterConstants.LocalReferenceNumber];
			filter.IsActive = true;
			filter.Property = "LRN1";
			Assert(header1.MatchesFilter(filterObj.Filter));
			Assert(!header2.MatchesFilter(filterObj.Filter));
			Assert(!header3.MatchesFilter(filterObj.Filter));
		}

		public void TestTextFilters()
		{
			AssertTextFilter(AsycudaFilterStrip.FilterConstants.JobReference, AsycudaManifestHeaderSchema.AMA_JobReference);
			AssertTextFilter(AsycudaFilterStrip.FilterConstants.VesselName, AsycudaManifestHeaderSchema.AMA_VesselName);
			AssertTextFilter(AsycudaFilterStrip.FilterConstants.VoyageNumber, AsycudaManifestHeaderSchema.AMA_Voyage);
			AssertTextFilter(AsycudaFilterStrip.FilterConstants.TransportMode, AsycudaManifestHeaderSchema.AMA_TransportMode);
			AssertTextFilter(AsycudaFilterStrip.FilterConstants.VesselImoNumber, AsycudaManifestHeaderSchema.AMA_LloydsNumber);
		}

		public void TestHeaderTextFilters()
		{
			AssertTextFilterHeader(AsycudaFilterStrip.FilterConstants.ContainerMode, AsycudaManifestHeaderSchema.AMA_ContainerMode);
			AssertTextFilterHeader(AsycudaFilterStrip.FilterConstants.VehicleReg, AsycudaManifestHeaderSchema.AMA_VehicleRegistration);
		}

		public void TestCountryTextFilters()
		{
			AssertCountryTextFilter(AsycudaFilterStrip.FilterConstants.Nature, AsycudaManifestHeaderSchema.AMA_Nature);
			AssertCountryTextFilter(AsycudaFilterStrip.FilterConstants.CarrierCode, AsycudaManifestHeaderSchema.AMA_CarrierCode);
		}

		public void TestCountryNKFilters()
		{
			AssertCountryNKFilter(AsycudaFilterStrip.FilterConstants.FirstArrivalPort, AsycudaManifestHeaderSchema.AMA_RL_NKPortOfFirstArrival);
		}

		public void TestManifestApplicationTypeFilter()
		{
			var manifest1 = Factory.New<AsycudaManifestHeader>();
			manifest1.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			manifest1.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var manifest2 = Factory.New<AsycudaManifestHeader>();
			manifest2.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			manifest2.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			Factory.Save();

			var filterObj = new AsycudaFilterStrip();
			var filter = (ModuleTextFilter)filterObj[AsycudaFilterStrip.FilterConstants.ManifestApplicationType];
			filter.IsActive = true;
			filter.Property = ApplicationCodeTypeList.Codes.ShippingLine;
			var filteredDecs = Factory.Load(typeof(AsycudaManifestHeader), filterObj.Filter);
			AssertEquals("Should have found 1 Records", 1, filteredDecs.Length);
		}

		public void TestShippingAgentAddressAndNameFilter()
		{
			var asyheader1 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedStates, "IAM");
			asyheader1.FillWithValidTestData();
			asyheader1.AMA_JobReference = "MAN000001";
			var shippingAgent1 = Factory.New<OrgHeader>();
			shippingAgent1.OH_Code = "CCC";
			shippingAgent1.OH_FullName = "Consignee";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = shippingAgent1.PK;
			orgAddress.OA_Address1 = "Consignee Address1";
			orgAddress.OA_City = "NY";
			orgAddress.OA_State = "NY";
			orgAddress.OA_PostCode = "1000";
			orgAddress.OA_Phone = "12345678";
			orgAddress.OA_RN_NKCountryCode = "US";
			asyheader1.AMA_OA_ShippingAgent = orgAddress.PK;

			var asyheader2 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "ALH");
			asyheader2.FillWithValidTestData();
			asyheader2.AMA_JobReference = "MAN000002";
			var shippingAgent2 = Factory.New<OrgHeader>();
			shippingAgent2.OH_Code = "DDD";
			shippingAgent2.OH_FullName = "SomethingElse";
			var orgAddress2 = Factory.New<OrgAddress>();
			orgAddress2.OA_OH = shippingAgent2.PK;
			orgAddress2.OA_Address1 = "Consignee Address2";
			orgAddress2.OA_City = "CT";
			orgAddress2.OA_State = "CT";
			orgAddress2.OA_PostCode = "1000";
			orgAddress2.OA_Phone = "12345678";
			orgAddress2.OA_RN_NKCountryCode = "ZA";
			asyheader2.AMA_OA_ShippingAgent = orgAddress2.PK;

			Factory.Save();

			var filterObj = new AsycudaFilterStrip();
			var filter = (ModuleGuidFilter)filterObj[AsycudaFilterStrip.FilterConstants.ShippingAgentAddress];
			filter.IsActive = true;
			filter.Property = orgAddress.PK;

			var headers = Factory.Load<AsycudaManifestHeader>(filterObj.Filter);
			AssertEquals(true, headers.Any());
			AssertEquals(true, headers.Any(x => x.PK == asyheader1.PK));
			AssertEquals(false, headers.Any(x => x.PK == asyheader2.PK));

			var filterObj2 = new AsycudaFilterStrip();
			var filter2 = (ModuleTextFilter)filterObj2[AsycudaFilterStrip.FilterConstants.ShippingAgentName];
			filter2.IsActive = true;
			filter2.Property = shippingAgent1.OH_FullName;

			headers = Factory.Load<AsycudaManifestHeader>(filterObj2.Filter);
			AssertEquals(true, headers.Any());
			AssertEquals(true, headers.Any(x => x.PK == asyheader1.PK));
			AssertEquals(false, headers.Any(x => x.PK == asyheader2.PK));
		}

		public void TestRegistrationNumberFilter()
		{
			var asyheader1 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedStates, "IAM");
			asyheader1.FillWithValidTestData();
			asyheader1.AMA_JobReference = "MAN000001";
			var shippingAgent1 = Factory.New<OrgHeader>();
			shippingAgent1.OH_Code = "CCC";
			shippingAgent1.OH_FullName = "Consignee";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = shippingAgent1.PK;
			orgAddress.OA_Address1 = "Consignee Address1";
			orgAddress.OA_City = "NY";
			orgAddress.OA_State = "NY";
			orgAddress.OA_PostCode = "1000";
			orgAddress.OA_Phone = "12345678";
			orgAddress.OA_RN_NKCountryCode = "US";
			asyheader1.AMA_OA_ShippingAgent = orgAddress.PK;
			asyheader1.RegistrationNumber = "1";

			var asyheader2 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "ALH");
			asyheader2.FillWithValidTestData();
			asyheader2.AMA_JobReference = "MAN000002";
			var shippingAgent2 = Factory.New<OrgHeader>();
			shippingAgent2.OH_Code = "DDD";
			shippingAgent2.OH_FullName = "SomethingElse";
			var orgAddress2 = Factory.New<OrgAddress>();
			orgAddress2.OA_OH = shippingAgent2.PK;
			orgAddress2.OA_Address1 = "Consignee Address2";
			orgAddress2.OA_City = "CT";
			orgAddress2.OA_State = "CT";
			orgAddress2.OA_PostCode = "1000";
			orgAddress2.OA_Phone = "12345678";
			orgAddress2.OA_RN_NKCountryCode = "ZA";
			asyheader2.AMA_OA_ShippingAgent = orgAddress2.PK;
			asyheader2.RegistrationNumber = "2";

			Factory.Save();

			var filterObj = new AsycudaFilterStrip();
			var filter = (ModuleTextFilter)filterObj[AsycudaFilterStrip.FilterConstants.RegistrationNumber];
			filter.IsActive = true;
			filter.Property = asyheader1.RegistrationNumber;

			var headers = Factory.Load<AsycudaManifestHeader>(filterObj.Filter);
			AssertEquals(true, headers.Any());
			AssertEquals(true, headers.Any(x => x.PK == asyheader1.PK));
			AssertEquals(false, headers.Any(x => x.PK == asyheader2.PK));
		}

		public void TestRegistrationDateFilter()
		{
			var asyheader1 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedStates, "IAM");
			asyheader1.FillWithValidTestData();
			asyheader1.AMA_JobReference = "MAN000001";
			var shippingAgent1 = Factory.New<OrgHeader>();
			shippingAgent1.OH_Code = "CCC";
			shippingAgent1.OH_FullName = "Consignee";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = shippingAgent1.PK;
			orgAddress.OA_Address1 = "Consignee Address1";
			orgAddress.OA_City = "NY";
			orgAddress.OA_State = "NY";
			orgAddress.OA_PostCode = "1000";
			orgAddress.OA_Phone = "12345678";
			orgAddress.OA_RN_NKCountryCode = "US";
			asyheader1.AMA_OA_ShippingAgent = orgAddress.PK;
			asyheader1.RegistrationNumber = "1";
			asyheader1.RegistrationDate = ZDateTime.UtcNow;

			var asyheader2 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "ALH");
			asyheader2.FillWithValidTestData();
			asyheader2.AMA_JobReference = "MAN000002";
			var shippingAgent2 = Factory.New<OrgHeader>();
			shippingAgent2.OH_Code = "DDD";
			shippingAgent2.OH_FullName = "SomethingElse";
			var orgAddress2 = Factory.New<OrgAddress>();
			orgAddress2.OA_OH = shippingAgent2.PK;
			orgAddress2.OA_Address1 = "Consignee Address2";
			orgAddress2.OA_City = "CT";
			orgAddress2.OA_State = "CT";
			orgAddress2.OA_PostCode = "1000";
			orgAddress2.OA_Phone = "12345678";
			orgAddress2.OA_RN_NKCountryCode = "ZA";
			asyheader2.AMA_OA_ShippingAgent = orgAddress2.PK;
			asyheader2.RegistrationNumber = "2";
			asyheader2.RegistrationDate = ZDateTime.UtcNow.AddDays(-10);

			Factory.Save();

			var filterObj = new AsycudaFilterStrip();
			var filter = (ModuleDateFilter)filterObj[AsycudaFilterStrip.FilterConstants.RegistrationDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.UtcNow.AddDays(-5);
			filter.Property2 = ZDateTime.UtcNow.AddDays(5);

			var headers = Factory.Load<AsycudaManifestHeader>(filterObj.Filter);
			AssertEquals(true, headers.Any());
			AssertEquals(true, headers.Any(x => x.PK == asyheader1.PK));
			AssertEquals(false, headers.Any(x => x.PK == asyheader2.PK));
		}

		public void TestManifestRegistrationDateFilter()
		{
			var asyheader1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			var bill1 = Factory.New<AsycudaBill>();
			bill1.ABL_BolType = AsycudaBill.ChildBolCode;
			bill1.ABL_BillNumber = "MB1";
			bill1.ABL_BillIssueDate = ZDate.Today;
			bill1.ABL_AMA = asyheader1.PK;

			var asyheader2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			asyheader2.AMA_JobReference = "MAN000002";
			var bill2 = Factory.New<AsycudaBill>();
			bill2.ABL_BolType = AsycudaBill.ChildBolCode;
			bill2.ABL_BillNumber = "MB2";
			bill2.ABL_BillIssueDate = ZDate.Today.AddDays(-10);
			bill2.ABL_AMA = asyheader1.PK;

			Factory.Save();

			var filterObj = new AsycudaFilterStrip();
			var filter = (ModuleDateFilter)filterObj[AsycudaFilterStrip.FilterConstants.IssueDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.UtcNow.AddDays(-5);
			filter.Property2 = ZDateTime.UtcNow.AddDays(5);

			var headers = Factory.Load<AsycudaManifestHeader>(filterObj.Filter);
			AssertEquals(true, headers.Any());
			AssertEquals(true, headers.Any(x => x.PK == asyheader1.PK));
			AssertEquals(false, headers.Any(x => x.PK == asyheader2.PK));
		}

		public void TestLoadDischargeFilter()
		{
			var header1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header1.AMA_JobReference = "MAN00001";
			header1.AMA_RL_NKPortOfLoading = "AUSYD";
			header1.AMA_RL_NKPortOfDischarge = "JPTKY";
			var header2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header2.AMA_RL_NKPortOfLoading = "AUMEL";
			header2.AMA_RL_NKPortOfDischarge = "SGSIN";
			header2.AMA_JobReference = "MAN00002";

			Factory.Save();

			var filterObj = new AsycudaFilterStrip();
			var filter = (ModuleLocationFilter)filterObj[AsycudaFilterStrip.FilterConstants.PortOfLoadingDischarge];
			AssertEquals("Loading", filter.ItemDescription1.Caption);
			AssertEquals("Discharge", filter.ItemDescription2.Caption);
			filter.IsActive = true;

			filter.Property1 = "AU";
			filter.Property2 = ZString.Empty;
			AssertEquals("header1 match filter", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(filterObj.Filter));

			filter.Property2 = "SGSIN";
			AssertEquals("header1 match filter", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(filterObj.Filter));

			filter.Property2 = "USLAX";
			AssertEquals("header1 match filter", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("header2 match filter", false, header2.MatchesFilter(filterObj.Filter));

			filter.Property2 = "JP";
			AssertEquals("header1 match filter", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("header2 match filter", false, header2.MatchesFilter(filterObj.Filter));

			filter.Property1 = "AUMEL";
			AssertEquals("header1 match filter", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("header2 match filter", false, header2.MatchesFilter(filterObj.Filter));

			filter.Property1 = ZString.Empty;
			AssertEquals("header1 match filter", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("header2 match filter", false, header2.MatchesFilter(filterObj.Filter));
		}

		public void TestModuleTextFiltersMaximunLength()
		{
			var filterObj = new AsycudaFilterStrip();

			var filter = (ModuleTextFilter)filterObj[AsycudaFilterStrip.FilterConstants.AgentType];
			AssertEquals(AsycudaManifestHeaderSchema.AMA_AgentType.MaxLength, filter.MaxLength);

			filter = (ModuleTextFilter)filterObj[AsycudaFilterStrip.FilterConstants.MasterBillNumber];
			AssertEquals(ModuleNumberFilter.MultiplyMaxLength(AsycudaBillSchema.ABL_BillNumber.MaxLength), filter.MaxLength);
		}

		public void TestRegisteredUserFilter()
		{
			var header1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header1.AMA_JobReference = "MAN00001";
			header1.AMA_GS_NKCustomsAgent = "KNZ";
			header1.AMA_IsActive = true;

			var header2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header2.AMA_JobReference = "MAN00002";
			header2.AMA_GS_NKCustomsAgent = "MD2";
			header2.AMA_IsActive = true;

			Factory.Save();

			var filterObj = new AsycudaFilterStrip();
			var filter = (ModuleNkFilter)filterObj[AsycudaFilterStrip.FilterConstants.RegisteredUser];
			filter.IsActive = true;
			filter.Property = "KNZ";

			var headers = Factory.Load<AsycudaManifestHeader>(filterObj.Filter);

			CombineAssertions(() =>
			{
				AssertEquals("AMA_GS_NKCustomsAgent = 'KNZ'", filter.Query.LiteralTextADO);
				AssertEquals(true, headers.Any());
				AssertEquals(true, headers.Any(x => x.PK == header1.PK));
				AssertEquals(false, headers.Any(x => x.PK == header2.PK));
			});
		}

		public void TestBillConsignorCodeFilter()
		{
			AssertBillAddressFilter("Consignor Code", AsycudaBillSchema.ABL_OA_Shipper);
		}

		public void TestBillConsignorNameFilter()
		{
			AssertBillTextFilter("Consignor Name", AsycudaBillSchema.ABL_ShipperName);
		}

		public void TestBillConsigneeCodeFilter()
		{
			AssertBillAddressFilter("Consignee Code", AsycudaBillSchema.ABL_OA_Consignee);
		}

		public void TestBillConsigneeNameFilter()
		{
			AssertBillTextFilter("Consignee Name", AsycudaBillSchema.ABL_ConsigneeName);
		}

		public void TestBillNumberFilter()
		{
			AssertBillTextFilter("Bill Number", AsycudaBillSchema.ABL_BillNumber);
		}

		public void TestBillUniqueConsignmentNumberFilter()
		{
			AssertBillTextFilter("Unique Consignment Number", AsycudaBillSchema.ABL_UCRNumber);
		}

		public void TestBillOriginDestination()
		{
			var header1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header1.AMA_JobReference = "MAN00001";
			header1.MasterBill.ABL_RL_NKOrigin = "XXXXX";
			header1.MasterBill.ABL_RL_NKFinalDestination = "YYYYY";
			var billHeader1 = header1.Bills.AddNew();
			billHeader1.ABL_RL_NKOrigin = "AUSYD";
			billHeader1.ABL_RL_NKFinalDestination = "JPTKY";
			var header2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header2.AMA_JobReference = "MAN00002";
			header2.MasterBill.ABL_RL_NKOrigin = "XXXXX";
			header2.MasterBill.ABL_RL_NKFinalDestination = "YYYYY";
			var billHeader2 = header2.Bills.AddNew();
			billHeader2.ABL_RL_NKOrigin = "AUMEL";
			billHeader2.ABL_RL_NKFinalDestination = "SGSIN";
			Factory.Save();

			var filterObj = new AsycudaFilterStrip();
			var filter = (ModuleLocationFilter)filterObj["Origin / Destination (Bills)"];
			AssertEquals("Origin", filter.ItemDescription1.Caption);
			AssertEquals("Destination", filter.ItemDescription2.Caption);
			filter.IsActive = true;

			filter.Property1 = "AU";
			filter.Property2 = ZString.Empty;
			AssertEquals("AsycudaManifestHeader1 matches filter 'AU' - ''", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("AsycudaManifestHeader2 matches filter 'AU' - ''", true, header2.MatchesFilter(filterObj.Filter));

			filter.Property2 = "SGSIN";
			AssertEquals("AsycudaManifestHeader1 matches filter 'AU' - 'SGIN'", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("AsycudaManifestHeader2 matches filter 'AU' - 'SGIN'", true, header2.MatchesFilter(filterObj.Filter));

			filter.Property2 = "USLAX";
			AssertEquals("AsycudaManifestHeader1 matches filter 'AU' - 'USLAX'", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("AsycudaManifestHeader2 matches filter 'AU' - 'USLAX'", false, header2.MatchesFilter(filterObj.Filter));

			filter.Property2 = "JP";
			AssertEquals("AsycudaManifestHeader1 matches filter 'AU' - 'JP'", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("AsycudaManifestHeader2 matches filter 'AU' - 'JP'", false, header2.MatchesFilter(filterObj.Filter));

			filter.Property1 = "AUMEL";
			AssertEquals("AsycudaManifestHeader1 matches filter 'AUMEL' - 'JP'", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("AsycudaManifestHeader2 matches filter 'AUMEL' - 'JP'", false, header2.MatchesFilter(filterObj.Filter));

			filter.Property1 = ZString.Empty;
			AssertEquals("AsycudaManifestHeader1 matches filter '' - 'AUMEL'", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("AsycudaManifestHeader2 matches filter '' - 'AUMEL'", false, header2.MatchesFilter(filterObj.Filter));

			filter.Property1 = "XXXXX";
			filter.Property2 = "YYYYY";
			AssertEquals("AsycudaManifestHeader1 matches filter 'XXXXX' - 'YYYYY' - MasterBill is deliberately excluded", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("AsycudaManifestHeader2 matches filter 'XXXXX' - 'YYYYY' - MasterBill is deliberately excluded", false, header2.MatchesFilter(filterObj.Filter));
		}

		public void TestContainerNumberFilter()
		{
			var header1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header1.AMA_JobReference = "MAN00001";
			var container1 = header1.Containers.AddNew();
			container1.ACN_ContainerNumber = "AAA";

			var header2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header2.AMA_JobReference = "MAN00002";
			var container2 = header2.Containers.AddNew();
			container2.ACN_ContainerNumber = "BBB";
			Factory.Save();

			var filterObj = new AsycudaFilterStrip();
			var filter = (ModuleTextFilter)filterObj["Container Number"];
			filter.IsActive = true;
			filter.Property = "AAA";

			AssertEquals("AsycudaManifestHeader #1 matches filter", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("AsycudaManifestHeader #2 matches filter", false, header2.MatchesFilter(filterObj.Filter));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new AsycudaFilterStrip();

		void AssertDateFilter(ZString filterName, SchemaDateTimeColumn schemaCol)
		{
			var header1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header1.AMA_JobReference = "";
			header1.MasterBill[schemaCol] = ZDateTime.Today.AddDays(1);
			var header2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header2.AMA_JobReference = "";
			header2.MasterBill[schemaCol] = ZDateTime.Today.AddDays(100);
			Factory.Save();

			var filterObj = new AsycudaFilterStrip();
			var filter = (ModuleDateFilter)filterObj[filterName];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property2 = ZDateTime.Today.AddDays(7);
			Assert(header1.MatchesFilter(filterObj.Filter));
			Assert(!header2.MatchesFilter(filterObj.Filter));
		}

		void AssertTextFilterHeader(ZString filterName, SchemaStringColumn schemaCol)
		{
			var asyheader1 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedStates, "IAM");
			asyheader1.FillWithValidTestData();
			asyheader1.AMA_JobReference = filterName + "2";
			asyheader1[schemaCol] = "1";
			var bill = asyheader1.Bills.AddNew();
			var asyheader2 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "ALH");
			asyheader2.FillWithValidTestData();
			asyheader2.AMA_JobReference = filterName + 1 + "2";
			asyheader2[schemaCol] = "2";
			var bill2 = asyheader2.Bills.AddNew();
			Factory.Save();
			var filterObj = new AsycudaFilterStrip();
			var filter = (ModuleTextFilter)filterObj[filterName];
			filter.IsActive = true;
			filter.Property = "1";

			var headers = Factory.Load<AsycudaManifestHeader>(filterObj.Filter);
			AssertEquals(true, headers.Any());
			AssertEquals(true, headers.Any(x => x.PK == asyheader1.PK));
			AssertEquals(false, headers.Any(x => x.PK == asyheader2.PK));
		}

		void AssertCountryTextFilter(ZString filterName, SchemaStringColumn schemaCol)
		{
			var asyheader1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			asyheader1.AMA_JobReference = filterName + "1";
			asyheader1.AMA_MasterBill = "123";
			asyheader1[schemaCol] = "1";
			var bill1 = asyheader1.Bills.AddNew();

			var asyheader2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			asyheader2.AMA_JobReference = filterName + "2";
			asyheader2.AMA_MasterBill = "456";
			var bill2 = asyheader2.Bills.AddNew();
			Factory.Save();

			var filterObj = new AsycudaFilterStrip();
			var filter = (ModuleTextFilter)filterObj[filterName];
			filter.IsActive = true;
			filter.Property = "1";
			var headers = Factory.Load<AsycudaManifestHeader>(filterObj.Filter);
			AssertEquals(true, headers.Any());
			AssertEquals(true, headers.Any(x => x.PK == asyheader1.PK));
			AssertEquals(false, headers.Any(x => x.PK == asyheader2.PK));
		}

		void AssertCountryNKFilter(ZString filterName, SchemaStringColumn schemaCol)
		{
			var asyheader1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			asyheader1.AMA_JobReference = filterName + "1";
			asyheader1.AMA_MasterBill = "123";
			asyheader1[schemaCol] = "1";
			asyheader1.AMA_ManifestType = "MGI";
			asyheader1.AMA_MessageStatus = "QUE";
			var bill1 = asyheader1.Bills.AddNew();

			var asyheader2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			asyheader2.AMA_JobReference = filterName + "2";
			asyheader2.AMA_MasterBill = "456";
			var bil2 = asyheader2.Bills.AddNew();
			Factory.Save();

			var filterObj = new AsycudaFilterStrip();
			var filter = (ModuleNkFilter)filterObj[filterName];
			filter.IsActive = true;
			filter.Property = "1";
			var headers = Factory.Load<AsycudaManifestHeader>(filterObj.Filter);
			AssertEquals(true, headers.Any());
			AssertEquals(true, headers.Any(x => x.PK == asyheader1.PK));
			AssertEquals(false, headers.Any(x => x.PK == asyheader2.PK));
		}

		void AssertTextFilter(ZString filterName, SchemaStringColumn schemaCol)
		{
			var header1 = Factory.New<AsycudaManifestHeader>();
			header1[schemaCol] = "1";
			var header2 = Factory.New<AsycudaManifestHeader>();
			header2[schemaCol] = "2";

			var filterObj = new AsycudaFilterStrip();
			var filter = (ModuleTextFilter)filterObj[filterName];
			filter.IsActive = true;
			filter.Property = "1";
			Assert(header1.MatchesFilter(filterObj.Filter));
			Assert(!header2.MatchesFilter(filterObj.Filter));
		}

		void AssertBillAddressFilter(string filterName, SchemaGuidColumn schemaCol)
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			var header1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header1.AMA_JobReference = "MAN00001";
			var billHeader1 = header1.Bills.AddNew();
			billHeader1[schemaCol] = orgHeader1.MainAddress.PK;

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var header2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header2.AMA_JobReference = "MAN00002";
			var billHeader2 = header2.Bills.AddNew();
			billHeader2[schemaCol] = orgHeader2.MainAddress.PK;
			Factory.Save();

			var filterObj = new AsycudaFilterStrip();
			var filter = (ModuleGuidFilter)filterObj[filterName];
			filter.IsActive = true;
			filter.Property = orgHeader1.PK;
			AssertEquals("AsycudaManifestHeader #1 matches filter", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("AsycudaManifestHeader #2 matches filter", false, header2.MatchesFilter(filterObj.Filter));

			billHeader1[schemaCol] = ZGuid.Empty;
			billHeader2[schemaCol] = ZGuid.Empty;
			header1.MasterBill[schemaCol] = orgHeader1.MainAddress.PK;
			header2.MasterBill[schemaCol] = orgHeader1.MainAddress.PK;
			Factory.Save();
			AssertEquals("AsycudaManifestHeader #1 matches filter - MasterBill is deliberately excluded", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("AsycudaManifestHeader #2 matches filter - MasterBill is deliberately excluded", false, header2.MatchesFilter(filterObj.Filter));
		}

		void AssertBillTextFilter(string filterName, SchemaStringColumn schemaCol)
		{
			var header1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header1.AMA_JobReference = "MAN00001";
			var billHeader1 = header1.Bills.AddNew();
			billHeader1[schemaCol] = "AAA";

			var header2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header2.AMA_JobReference = "MAN00002";
			var billHeader2 = header2.Bills.AddNew();
			billHeader2[schemaCol] = "BBB";
			Factory.Save();

			var filterObj = new AsycudaFilterStrip();
			var filter = (ModuleTextFilter)filterObj[filterName];
			filter.IsActive = true;
			filter.Property = "AAA";
			AssertEquals("AsycudaManifestHeader #1 matches filter", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("AsycudaManifestHeader #2 matches filter", false, header2.MatchesFilter(filterObj.Filter));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			AssertEquals("AsycudaManifestHeader #1 matches filter - MasterBill is deliberately excluded", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("AsycudaManifestHeader #2 matches filter - MasterBill is deliberately excluded", false, header2.MatchesFilter(filterObj.Filter));
		}
	}
}
