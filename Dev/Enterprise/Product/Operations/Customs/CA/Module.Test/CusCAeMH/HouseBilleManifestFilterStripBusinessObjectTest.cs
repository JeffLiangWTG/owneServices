using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.Common.CA;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(HouseBilleManifestFilterStripBusinessObject))]
	sealed class HouseBilleManifestFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestHouseBillLastestD4NoticeFilter()
		{
			var master1 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			var house1 = master1.HouseBills.AddNew();
			house1.FillWithValidTestData();
			house1.BW_D4MessageStatus = "8000";
			var master2 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			var house2 = master2.HouseBills.AddNew();
			house2.FillWithValidTestData();
			house2.BW_D4MessageStatus = "0010";
			var master3 = Factory.NewWithValidTestData<CusCAeMHMaster>();

			Factory.Save();

			var filterBO = new HouseBilleManifestFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBO[HouseBilleManifestFilterStripBusinessObject.Constants.HouseBillLatestD4Notice];
			filter.Property = "8000";
			filter.IsActive = true;

			AssertEquals(FilterCategories.StatusAndFlags, filter.Category);
			Assert(master1.MatchesFilter(filterBO.Filter));
			Assert(!master2.MatchesFilter(filterBO.Filter));
			Assert(!master3.MatchesFilter(filterBO.Filter));

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			Assert(!master1.MatchesFilter(filterBO.Filter));
			Assert(master2.MatchesFilter(filterBO.Filter));
			Assert(master3.MatchesFilter(filterBO.Filter));
		}

		public void TestMasterBillLastestD4NoticeFilter()
		{
			var master1 = Factory.New<CusCAeMHMaster>();
			master1.BP_D4MessageStatus = "8000";
			var master2 = Factory.New<CusCAeMHMaster>();
			master2.BP_D4MessageStatus = "0010";

			var filterBO = new HouseBilleManifestFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBO[HouseBilleManifestFilterStripBusinessObject.Constants.MasterBillLatestD4Notice];
			filter.Property = "8000";
			filter.IsActive = true;

			AssertEquals(FilterCategories.StatusAndFlags, filter.Category);
			Assert(master1.MatchesFilter(filterBO.Filter));
			Assert(!master2.MatchesFilter(filterBO.Filter));
		}

		public void TestHouseMessageReferenceFilter()
		{
			var master1 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			var house1 = master1.HouseBills.AddNew();
			house1.FillWithValidTestData();
			house1.BW_MessageReference = "REF1";
			var master2 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			var house2 = master2.HouseBills.AddNew();
			house2.FillWithValidTestData();
			house2.BW_MessageReference = "REF2";
			var master3 = Factory.NewWithValidTestData<CusCAeMHMaster>();

			Factory.Save();

			var filterBO = new HouseBilleManifestFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBO[HouseBilleManifestFilterStripBusinessObject.Constants.HouseMessageReference];
			filter.Property = "REF1";
			filter.IsActive = true;

			Assert(master1.MatchesFilter(filterBO.Filter));
			Assert(!master2.MatchesFilter(filterBO.Filter));
			Assert(!master3.MatchesFilter(filterBO.Filter));
			AssertEquals(CusCAeMHHouseSchema.BW_MessageReference.MaxLength, filter.MaxLength);

			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			Assert(!master1.MatchesFilter(filterBO.Filter));
			Assert(!master2.MatchesFilter(filterBO.Filter));
			Assert(master3.MatchesFilter(filterBO.Filter));
		}

		public void TestMasterMessageReferenceFilter()
		{
			var master1 = Factory.New<CusCAeMHMaster>();
			master1.BP_MessageReference = "REF1";
			var master2 = Factory.New<CusCAeMHMaster>();
			master2.BP_MessageReference = "REF2";

			var filterBO = new HouseBilleManifestFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBO[HouseBilleManifestFilterStripBusinessObject.Constants.MasterMessageReference];
			filter.Property = "REF1";
			filter.IsActive = true;

			Assert(master1.MatchesFilter(filterBO.Filter));
			Assert(!master2.MatchesFilter(filterBO.Filter));
		}

		public void TestMasterMessageStatusFilter()
		{
			var master1 = Factory.New<CusCAeMHMaster>();
			master1.BP_MessageStatus = MessageStatusList.Codes.AwaitingOriginal;
			var master2 = Factory.New<CusCAeMHMaster>();
			master2.BP_MessageStatus = MessageStatusList.Codes.ClearDelete;
			var master3 = Factory.New<CusCAeMHMaster>();
			master3.BP_MessageStatus = MessageStatusList.Codes.NotSent;

			var filterBO = new HouseBilleManifestFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBO[HouseBilleManifestFilterStripBusinessObject.Constants.MasterMessageStatus];
			filter.Property = MessageStatusList.Codes.AwaitingOriginal;
			filter.IsActive = true;

			AssertEquals(FilterCategories.StatusAndFlags, filter.Category);
			var list = filter.List as CodeDescriptionPairList;
			AssertCollectionNotContains(ZString.Empty, list.GetAllCodes());
			AssertEquals(CAExternalColumnsHelper.Constants.NotSentDescription, list.GetDescriptionFromCode(CAExternalColumnsHelper.Constants.NotSentCode));

			Assert(master1.MatchesFilter(filterBO.Filter));
			Assert(!master2.MatchesFilter(filterBO.Filter));
			Assert(!master3.MatchesFilter(filterBO.Filter));

			filter.Property = CAExternalColumnsHelper.Constants.NotSentCode;
			Assert(!master1.MatchesFilter(filterBO.Filter));
			Assert(!master2.MatchesFilter(filterBO.Filter));
			Assert(master3.MatchesFilter(filterBO.Filter));
		}

		public void TestMasterCustomsStatusFilter()
		{
			var master1 = Factory.New<CusCAeMHMaster>();
			master1.BP_CustomsStatus = EManifestForwarderJobStatusList.Codes.Clear;
			var master2 = Factory.New<CusCAeMHMaster>();
			master2.BP_CustomsStatus = EManifestForwarderJobStatusList.Codes.Error;

			var filterBO = new HouseBilleManifestFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBO[HouseBilleManifestFilterStripBusinessObject.Constants.MasterCustomsStatus];
			filter.Property = EManifestForwarderJobStatusList.Codes.Clear;
			filter.IsActive = true;

			AssertEquals(FilterCategories.StatusAndFlags, filter.Category);
			Assert(master1.MatchesFilter(filterBO.Filter));
			Assert(!master2.MatchesFilter(filterBO.Filter));
		}

		public void TestMasterBillFilter()
		{
			var master1 = Factory.New<CusCAeMHMaster>();
			master1.BP_MasterBill = "MB1";
			var master2 = Factory.New<CusCAeMHMaster>();
			master2.BP_MasterBill = "MB2";

			var filterBO = new HouseBilleManifestFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBO[HouseBilleManifestFilterStripBusinessObject.Constants.MasterBill];
			filter.Property = "MB1";
			filter.IsActive = true;

			Assert(master1.MatchesFilter(filterBO.Filter));
			Assert(!master2.MatchesFilter(filterBO.Filter));
		}

		public void TestMasterHouseBillFilter()
		{
			var master1 = Factory.New<CusCAeMHMaster>();
			master1.BP_MasterHouseBill = "HB1";
			var master2 = Factory.New<CusCAeMHMaster>();
			master2.BP_MasterHouseBill = "HB2";

			var filterBO = new HouseBilleManifestFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBO[HouseBilleManifestFilterStripBusinessObject.Constants.MasterHouseBill];
			filter.Property = "HB1";
			filter.IsActive = true;

			Assert(master1.MatchesFilter(filterBO.Filter));
			Assert(!master2.MatchesFilter(filterBO.Filter));
		}

		public void TestPrimaryCCNFilter()
		{
			var master1 = Factory.New<CusCAeMHMaster>();
			master1.BP_PrimaryCCN = "CCN1";
			var master2 = Factory.New<CusCAeMHMaster>();
			master2.BP_PrimaryCCN = "CCN2";

			var filterBO = new HouseBilleManifestFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBO[HouseBilleManifestFilterStripBusinessObject.Constants.PrimaryCCN];
			filter.Property = "CCN1";
			filter.IsActive = true;

			Assert(master1.MatchesFilter(filterBO.Filter));
			Assert(!master2.MatchesFilter(filterBO.Filter));
		}

		public void TestMasterHouseCCNFilter()
		{
			var master1 = Factory.New<CusCAeMHMaster>();
			master1.BP_MasterHouseCCN = "CCN1";
			var master2 = Factory.New<CusCAeMHMaster>();
			master2.BP_MasterHouseCCN = "CCN2";

			var filterBO = new HouseBilleManifestFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBO[HouseBilleManifestFilterStripBusinessObject.Constants.MasterHouseCCN];
			filter.Property = "CCN1";
			filter.IsActive = true;

			Assert(master1.MatchesFilter(filterBO.Filter));
			Assert(!master2.MatchesFilter(filterBO.Filter));
		}

		public void TestModeOfTransportFilter()
		{
			var master1 = Factory.New<CusCAeMHMaster>();
			master1.BP_ModeOfTransport = TransportTypeList.Codes.Sea;
			var master2 = Factory.New<CusCAeMHMaster>();
			master2.BP_ModeOfTransport = TransportTypeList.Codes.Road;

			var filterBO = new HouseBilleManifestFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBO[HouseBilleManifestFilterStripBusinessObject.Constants.ModeOfTransport];
			filter.Property = TransportTypeList.Codes.Sea;
			filter.IsActive = true;

			Assert(master1.MatchesFilter(filterBO.Filter));
			Assert(!master2.MatchesFilter(filterBO.Filter));
		}

		public void TestContainerNumberFilter()
		{
			var master1 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			var container1 = master1.Containers.AddNew();
			container1.FillWithValidTestData();
			container1.BQ_ContainerNumber = "NO1";
			var master2 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			var container2 = master2.Containers.AddNew();
			container2.FillWithValidTestData();
			container2.BQ_ContainerNumber = "NO2";

			Factory.Save();

			var filterBO = new HouseBilleManifestFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBO[HouseBilleManifestFilterStripBusinessObject.Constants.ContainerNumber];
			filter.Property = "NO1";
			filter.IsActive = true;

			Assert(master1.MatchesFilter(filterBO.Filter));
			Assert(!master2.MatchesFilter(filterBO.Filter));
			AssertEquals(CusCAeMHContainerSchema.BQ_ContainerNumber.MaxLength, filter.MaxLength);
		}

		public void TestDischargePortFilter()
		{
			var master1 = Factory.New<CusCAeMHMaster>();
			master1.BP_CBSADischargePort = "AAA";
			var master2 = Factory.New<CusCAeMHMaster>();
			master2.BP_CBSADischargePort = "BBB";

			var filterBO = new HouseBilleManifestFilterStripBusinessObject();
			var filter = (ModuleNkFilter)filterBO[HouseBilleManifestFilterStripBusinessObject.Constants.CBSADischargePort];
			filter.Property = "0AAA";
			filter.IsActive = true;

			AssertEquals(ModuleIDs.Customs.Universal.ZZRefCusCodeList, filter.ModuleId);
			Assert(master1.MatchesFilter(filterBO.Filter));
			Assert(!master2.MatchesFilter(filterBO.Filter));
		}

		public void TestDischargeSubLocationFilter()
		{
			CACSubLocationTest.CreateSubLocation(Factory, "AAA");
			CACSubLocationTest.CreateSubLocation(Factory, "BBB");
			Factory.Save();

			var master1 = Factory.New<CusCAeMHMaster>();
			master1.BP_CBSADischargeSubLocation = "AAA";
			var master2 = Factory.New<CusCAeMHMaster>();
			master2.BP_CBSADischargeSubLocation = "BBB";

			var filterBO = new HouseBilleManifestFilterStripBusinessObject();
			var filter = (ModuleNkFilter)filterBO[HouseBilleManifestFilterStripBusinessObject.Constants.CBSADischargeSubLocation];
			filter.Property = "AAA";
			filter.IsActive = true;

			Assert(master1.MatchesFilter(filterBO.Filter));
			Assert(!master2.MatchesFilter(filterBO.Filter));
		}

		public void TestCarrierCodeFilter()
		{
			var carrier1 = Factory.New<Universal.ZZRefCarrierCombined>();
			carrier1.ZZ4_Code = "AAA";
			carrier1.ZZ4_Description = "Carrier Name 1";
			carrier1.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.Canada;
			carrier1.Attributes.AddNew(Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER, Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER);

			var carrier2 = Factory.New<Universal.ZZRefCarrierCombined>();
			carrier2.ZZ4_Code = "BBB";
			carrier2.ZZ4_Description = "Carrier Name 2";
			carrier2.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.Canada;
			carrier2.Attributes.AddNew(Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER, Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER);
			Factory.Save();

			var master1 = Factory.New<CusCAeMHMaster>();
			master1.BP_CBSACarrierCode = "AAA";
			var master2 = Factory.New<CusCAeMHMaster>();
			master2.BP_CBSACarrierCode = "BBB";

			var filterBO = new HouseBilleManifestFilterStripBusinessObject();
			var filter = (ModuleNkFilter)filterBO[HouseBilleManifestFilterStripBusinessObject.Constants.CBSACarrierCode];
			filter.Property = "AAA";
			filter.IsActive = true;

			Assert(master1.MatchesFilter(filterBO.Filter));
			Assert(!master2.MatchesFilter(filterBO.Filter));
		}

		public void TestETAFilter()
		{
			var master1 = Factory.New<CusCAeMHMaster>();
			master1.BP_ETA = ZDateTime.Today;
			var master2 = Factory.New<CusCAeMHMaster>();
			master2.BP_ETA = ZDateTime.Today.AddDays(10);

			var filterBO = new HouseBilleManifestFilterStripBusinessObject();
			var filter = (ModuleDateFilter)filterBO[HouseBilleManifestFilterStripBusinessObject.Constants.ETA];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Today.AddDays(-1);
			filter.Property2 = ZDateTime.Today.AddDays(+1);
			filter.IsActive = true;

			Assert(master1.MatchesFilter(filterBO.Filter));
			Assert(!master2.MatchesFilter(filterBO.Filter));
		}

		public void TestATAFilter()
		{
			var master1 = Factory.New<CusCAeMHMaster>();
			master1.BP_ATA = ZDateTime.Today;
			var master2 = Factory.New<CusCAeMHMaster>();
			master2.BP_ATA = ZDateTime.Today.AddDays(10);

			var filterBO = new HouseBilleManifestFilterStripBusinessObject();
			var filter = (ModuleDateFilter)filterBO[HouseBilleManifestFilterStripBusinessObject.Constants.ATA];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Today.AddDays(-1);
			filter.Property2 = ZDateTime.Today.AddDays(+1);
			filter.IsActive = true;

			Assert(master1.MatchesFilter(filterBO.Filter));
			Assert(!master2.MatchesFilter(filterBO.Filter));
		}

		public void TestPlaceOfConsolidationFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var master1 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			master1.PlaceOfConsolidation.OrganisationPK = org1.PK;
			var master2 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			master2.PlaceOfConsolidation.OrganisationPK = org2.PK;

			Factory.Save();

			var filterBO = new HouseBilleManifestFilterStripBusinessObject();
			var filter = (ModuleGuidFilter)filterBO[HouseBilleManifestFilterStripBusinessObject.Constants.PlaceOfConsolidation];
			filter.Property = org1.PK;
			filter.IsActive = true;

			Assert(master1.MatchesFilter(filterBO.Filter));
			Assert(!master2.MatchesFilter(filterBO.Filter));
		}

		public void TestConsolidatorFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var master1 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			master1.Consolidator.OrganisationPK = org1.PK;
			var master2 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			master2.Consolidator.OrganisationPK = org2.PK;

			Factory.Save();

			var filterBO = new HouseBilleManifestFilterStripBusinessObject();
			var filter = (ModuleGuidFilter)filterBO[HouseBilleManifestFilterStripBusinessObject.Constants.Consolidator];
			filter.Property = org1.PK;
			filter.IsActive = true;

			Assert(master1.MatchesFilter(filterBO.Filter));
			Assert(!master2.MatchesFilter(filterBO.Filter));
		}

		public void TestHouseBillFilter()
		{
			var master1 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			var house1 = master1.HouseBills.AddNew();
			house1.FillWithValidTestData();
			house1.BW_HouseBill = "HB1";
			var master2 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			var house2 = master2.HouseBills.AddNew();
			house2.FillWithValidTestData();
			house2.BW_HouseBill = "HB2";
			var master3 = Factory.NewWithValidTestData<CusCAeMHMaster>();

			Factory.Save();

			var filterBO = new HouseBilleManifestFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBO[HouseBilleManifestFilterStripBusinessObject.Constants.HouseBill];
			filter.Property = "HB1";
			filter.IsActive = true;

			Assert(master1.MatchesFilter(filterBO.Filter));
			Assert(!master2.MatchesFilter(filterBO.Filter));
			Assert(!master3.MatchesFilter(filterBO.Filter));
			AssertEquals(CusCAeMHHouseSchema.BW_HouseBill.MaxLength, filter.MaxLength);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			Assert(!master1.MatchesFilter(filterBO.Filter));
			Assert(master2.MatchesFilter(filterBO.Filter));
			Assert(master3.MatchesFilter(filterBO.Filter));
		}

		public void TestHouseCCNFilter()
		{
			var master1 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			var house1 = master1.HouseBills.AddNew();
			house1.FillWithValidTestData();
			house1.BW_HouseCCN = "CCN1";
			var master2 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			var house2 = master2.HouseBills.AddNew();
			house2.FillWithValidTestData();
			house2.BW_HouseBill = "CCN2";
			var master3 = Factory.NewWithValidTestData<CusCAeMHMaster>();

			Factory.Save();

			var filterBO = new HouseBilleManifestFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBO[HouseBilleManifestFilterStripBusinessObject.Constants.HouseCCN];
			filter.Property = "CCN1";
			filter.IsActive = true;

			Assert(master1.MatchesFilter(filterBO.Filter));
			Assert(!master2.MatchesFilter(filterBO.Filter));
			Assert(!master3.MatchesFilter(filterBO.Filter));
			AssertEquals(CusCAeMHHouseSchema.BW_HouseCCN.MaxLength, filter.MaxLength);

			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			Assert(!master1.MatchesFilter(filterBO.Filter));
			Assert(master2.MatchesFilter(filterBO.Filter));
			Assert(master3.MatchesFilter(filterBO.Filter));
		}

		public void TestHouseUCRFilter()
		{
			var master1 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			var house1 = master1.HouseBills.AddNew();
			house1.FillWithValidTestData();
			house1.BW_UCR = "UCR1";
			var master2 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			var house2 = master2.HouseBills.AddNew();
			house2.FillWithValidTestData();
			house2.BW_UCR = "UCR2";
			var master3 = Factory.NewWithValidTestData<CusCAeMHMaster>();

			Factory.Save();

			var filterBO = new HouseBilleManifestFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBO[HouseBilleManifestFilterStripBusinessObject.Constants.UCR];
			filter.Property = "UCR1";
			filter.IsActive = true;

			Assert(master1.MatchesFilter(filterBO.Filter));
			Assert(!master2.MatchesFilter(filterBO.Filter));
			Assert(!master3.MatchesFilter(filterBO.Filter));
			AssertEquals(CusCAeMHHouseSchema.BW_UCR.MaxLength, filter.MaxLength);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			Assert(!master1.MatchesFilter(filterBO.Filter));
			Assert(master2.MatchesFilter(filterBO.Filter));
			Assert(master3.MatchesFilter(filterBO.Filter));
		}

		public void TestMovementTypeFilter()
		{
			var master1 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			var house1 = master1.HouseBills.AddNew();
			house1.FillWithValidTestData();
			house1.BW_MovementType = TransportTypeList.Codes.Sea;
			var master2 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			var house2 = master2.HouseBills.AddNew();
			house2.FillWithValidTestData();
			house2.BW_MovementType = TransportTypeList.Codes.Road;

			Factory.Save();

			var filterBO = new HouseBilleManifestFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBO[HouseBilleManifestFilterStripBusinessObject.Constants.MovementType];
			filter.Property = TransportTypeList.Codes.Sea;
			filter.IsActive = true;

			Assert(master1.MatchesFilter(filterBO.Filter));
			Assert(!master2.MatchesFilter(filterBO.Filter));
			AssertEquals(CusCAeMHHouseSchema.BW_MovementType.MaxLength, filter.MaxLength);
		}

		public void TestMessageStatusFilter()
		{
			var master1 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			var house1 = master1.HouseBills.AddNew();
			house1.FillWithValidTestData();
			house1.BW_MessageStatus = MessageStatusList.Codes.AwaitingOriginal;
			var master2 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			var house2 = master2.HouseBills.AddNew();
			house2.FillWithValidTestData();
			house2.BW_MessageStatus = MessageStatusList.Codes.AwaitingChange;
			var master3 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			var house3 = master3.HouseBills.AddNew();
			house3.FillWithValidTestData();
			house3.BW_MessageStatus = MessageStatusList.Codes.NotSent;

			Factory.Save();

			var filterBO = new HouseBilleManifestFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBO[HouseBilleManifestFilterStripBusinessObject.Constants.HouseMessageStatus];
			filter.Property = MessageStatusList.Codes.AwaitingOriginal;
			filter.IsActive = true;

			AssertEquals(FilterCategories.StatusAndFlags, filter.Category);
			AssertEquals(FilterCategories.StatusAndFlags, filter.Category);
			var list = filter.List as CodeDescriptionPairList;
			AssertCollectionNotContains(ZString.Empty, list.GetAllCodes());
			AssertEquals(CAExternalColumnsHelper.Constants.NotSentDescription, list.GetDescriptionFromCode(CAExternalColumnsHelper.Constants.NotSentCode));

			Assert(master1.MatchesFilter(filterBO.Filter));
			Assert(!master2.MatchesFilter(filterBO.Filter));
			Assert(!master3.MatchesFilter(filterBO.Filter));

			filter.Property = CAExternalColumnsHelper.Constants.NotSentCode;
			Assert(!master1.MatchesFilter(filterBO.Filter));
			Assert(!master2.MatchesFilter(filterBO.Filter));
			Assert(master3.MatchesFilter(filterBO.Filter));
		}

		public void TestCustomsStatusFilter()
		{
			var master1 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			var house1 = master1.HouseBills.AddNew();
			house1.FillWithValidTestData();
			house1.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.Clear;
			var master2 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			var house2 = master2.HouseBills.AddNew();
			house2.FillWithValidTestData();
			house2.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.Cancelled;

			Factory.Save();

			var filterBO = new HouseBilleManifestFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBO[HouseBilleManifestFilterStripBusinessObject.Constants.HouseCustomsStatus];
			filter.Property = B3EntryStatusList.Codes.Accepted;
			filter.IsActive = true;

			AssertEquals(FilterCategories.StatusAndFlags, filter.Category);
			Assert(master1.MatchesFilter(filterBO.Filter));
			Assert(!master2.MatchesFilter(filterBO.Filter));
		}

		public void TestAmendmentReasonFilter()
		{
			var master1 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			var house1 = master1.HouseBills.AddNew();
			house1.FillWithValidTestData();
			house1.BW_AmendReasonCode = EManifestAmendmentReasonCodes.Codes.ClientOutage;
			var master2 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			var house2 = master2.HouseBills.AddNew();
			house2.FillWithValidTestData();
			house2.BW_AmendReasonCode = EManifestAmendmentReasonCodes.Codes.CBSAOutage;

			Factory.Save();

			var filterBO = new HouseBilleManifestFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBO[HouseBilleManifestFilterStripBusinessObject.Constants.AmendmentReason];
			filter.Property = EManifestAmendmentReasonCodes.Codes.ClientOutage;
			filter.IsActive = true;

			Assert(master1.MatchesFilter(filterBO.Filter));
			Assert(!master2.MatchesFilter(filterBO.Filter));
			AssertEquals(CusCAeMHHouseSchema.BW_AmendReasonCode.MaxLength, filter.MaxLength);
		}

		public void TestReleasePortFilter()
		{
			var master1 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			var house1 = master1.HouseBills.AddNew();
			house1.FillWithValidTestData();
			house1.BW_CBSAReleasePort = "AAAA";
			var master2 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			var house2 = master2.HouseBills.AddNew();
			house2.FillWithValidTestData();
			house2.BW_CBSAReleasePort = "BBBB";

			Factory.Save();

			var filterBO = new HouseBilleManifestFilterStripBusinessObject();
			var filter = (ModuleNkFilter)filterBO[HouseBilleManifestFilterStripBusinessObject.Constants.CBSAReleasePort];
			filter.Property = "AAAA";
			filter.IsActive = true;

			AssertEquals(ModuleIDs.Customs.Universal.ZZRefCusCodeList, filter.ModuleId);
			Assert(master1.MatchesFilter(filterBO.Filter));
			Assert(!master2.MatchesFilter(filterBO.Filter));
		}

		public void TestReleaseSubLocationFilter()
		{
			CACSubLocationTest.CreateSubLocation(Factory, "AAAA");
			CACSubLocationTest.CreateSubLocation(Factory, "BBBB");

			var master1 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			var house1 = master1.HouseBills.AddNew();
			house1.FillWithValidTestData();
			house1.BW_CBSAReleaseSubLocation = "AAAA";
			var master2 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			var house2 = master2.HouseBills.AddNew();
			house2.FillWithValidTestData();
			house2.BW_CBSAReleaseSubLocation = "BBBB";

			Factory.Save();

			var filterBO = new HouseBilleManifestFilterStripBusinessObject();
			var filter = (ModuleNkFilter)filterBO[HouseBilleManifestFilterStripBusinessObject.Constants.CBSAReleaseSubLocation];
			filter.Property = "AAAA";
			filter.IsActive = true;

			Assert(master1.MatchesFilter(filterBO.Filter));
			Assert(!master2.MatchesFilter(filterBO.Filter));
		}

		public void TestConsigneeFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var master1 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			var house1 = master1.HouseBills.AddNew();
			house1.FillWithValidTestData();
			var docAddress1 = house1.DocAddresses.AddNew();
			docAddress1.E2_AddressType = DocAddressTypes.Codes.ConsigneeDocumentaryAddress;
			docAddress1.OrganisationPK = org1.PK;
			var master2 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			var house2 = master2.HouseBills.AddNew();
			house2.FillWithValidTestData();
			var docAddress2 = house2.DocAddresses.AddNew();
			docAddress2.E2_AddressType = DocAddressTypes.Codes.ConsigneeDocumentaryAddress;
			docAddress2.OrganisationPK = org2.PK;

			Factory.Save();

			var filterBO = new HouseBilleManifestFilterStripBusinessObject();
			var filter = (ModuleGuidFilter)filterBO[HouseBilleManifestFilterStripBusinessObject.Constants.Consignee];
			filter.Property = org1.PK;
			filter.IsActive = true;

			Assert(master1.MatchesFilter(filterBO.Filter));
			Assert(!master2.MatchesFilter(filterBO.Filter));
		}

		public void TestShipperFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var master1 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			var house1 = master1.HouseBills.AddNew();
			house1.FillWithValidTestData();
			var docAddress1 = house1.DocAddresses.AddNew();
			docAddress1.E2_AddressType = DocAddressTypes.Codes.ConsignorDocumentaryAddress;
			docAddress1.OrganisationPK = org1.PK;
			var master2 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			var house2 = master2.HouseBills.AddNew();
			house2.FillWithValidTestData();
			var docAddress2 = house2.DocAddresses.AddNew();
			docAddress2.E2_AddressType = DocAddressTypes.Codes.ConsignorDocumentaryAddress;
			docAddress2.OrganisationPK = org2.PK;

			Factory.Save();

			var filterBO = new HouseBilleManifestFilterStripBusinessObject();
			var filter = (ModuleGuidFilter)filterBO[HouseBilleManifestFilterStripBusinessObject.Constants.Shipper];
			filter.Property = org1.PK;
			filter.IsActive = true;

			Assert(master1.MatchesFilter(filterBO.Filter));
			Assert(!master2.MatchesFilter(filterBO.Filter));
		}

		public void TestDeliveryFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var master1 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			var house1 = master1.HouseBills.AddNew();
			house1.FillWithValidTestData();
			var docAddress1 = house1.DocAddresses.AddNew();
			docAddress1.E2_AddressType = DocAddressTypes.Codes.ConsigneePickupDeliveryAddress;
			docAddress1.OrganisationPK = org1.PK;
			var master2 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			var house2 = master2.HouseBills.AddNew();
			house2.FillWithValidTestData();
			var docAddress2 = house2.DocAddresses.AddNew();
			docAddress2.E2_AddressType = DocAddressTypes.Codes.ConsigneePickupDeliveryAddress;
			docAddress2.OrganisationPK = org2.PK;

			Factory.Save();

			var filterBO = new HouseBilleManifestFilterStripBusinessObject();
			var filter = (ModuleGuidFilter)filterBO[HouseBilleManifestFilterStripBusinessObject.Constants.Delivery];
			filter.Property = org1.PK;
			filter.IsActive = true;

			Assert(master1.MatchesFilter(filterBO.Filter));
			Assert(!master2.MatchesFilter(filterBO.Filter));
		}

		public void TestNotifyPartyFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var master1 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			var house1 = master1.HouseBills.AddNew();
			house1.FillWithValidTestData();
			var docAddress1 = house1.DocAddresses.AddNew();
			docAddress1.E2_AddressType = DocAddressTypes.Codes.NotifyParty;
			docAddress1.OrganisationPK = org1.PK;
			var master2 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			var house2 = master2.HouseBills.AddNew();
			house2.FillWithValidTestData();
			var docAddress2 = house2.DocAddresses.AddNew();
			docAddress2.E2_AddressType = DocAddressTypes.Codes.NotifyParty;
			docAddress2.OrganisationPK = org2.PK;

			Factory.Save();

			var filterBO = new HouseBilleManifestFilterStripBusinessObject();
			var filter = (ModuleGuidFilter)filterBO[HouseBilleManifestFilterStripBusinessObject.Constants.NotifyParty];
			filter.Property = org1.PK;
			filter.IsActive = true;

			Assert(master1.MatchesFilter(filterBO.Filter));
			Assert(!master2.MatchesFilter(filterBO.Filter));
		}

		public void TestSecondaryNotifyPartyFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var master1 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			var house1 = master1.HouseBills.AddNew();
			house1.FillWithValidTestData();
			var docAddress1 = house1.DocAddresses.AddNew();
			docAddress1.E2_AddressType = DocAddressTypes.Codes.ImportBroker;
			docAddress1.OrganisationPK = org1.PK;
			var master2 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			var house2 = master2.HouseBills.AddNew();
			house2.FillWithValidTestData();
			var docAddress2 = house2.DocAddresses.AddNew();
			docAddress2.E2_AddressType = DocAddressTypes.Codes.ImportBroker;
			docAddress2.OrganisationPK = org2.PK;

			Factory.Save();

			var filterBO = new HouseBilleManifestFilterStripBusinessObject();
			var filter = (ModuleGuidFilter)filterBO[HouseBilleManifestFilterStripBusinessObject.Constants.SecondaryNotifyParty];
			filter.Property = org1.PK;
			filter.IsActive = true;

			Assert(master1.MatchesFilter(filterBO.Filter));
			Assert(!master2.MatchesFilter(filterBO.Filter));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new HouseBilleManifestFilterStripBusinessObject();
	}
}
