using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business.Interfaces;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using ZPropertyInfoExtensions = Enterprise.Customs.Business.ZPropertyInfoExtensions;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsCommonMovementHeaderTest : TestCaseWithFactory
	{
		public void TestIsPhase5()
		{
			AssertEquals("IsPhase5 should be false if parent header application code is NC4.", false, commonMovement.IsPhase5);
			commonMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertEquals("IsPhase5 should be true if parent header application code is NC5.", true, commonMovement.IsPhase5);
		}

		public void TestIsPhase4()
		{
			AssertEquals("IsPhase4 should be true if parent header application code is NC4.", true, commonMovement.IsPhase4);
			commonMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertEquals("IsPhase4 should be false if parent header application code is NC5.", false, commonMovement.IsPhase4);
		}

		public void TestValidationDecider()
		{
			AssertType<NctsDepartureMovementHeaderPhase4ValidationDecider>(commonMovement.ValidationDecider);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals(Core.Constants.Weight.Kilograms, commonMovement.BM_GrossWeightUQ);
		}

		public void TestOnLoad_BM_GrossWeightUQ_Defaulted()
		{
			commonMovement.BM_GrossWeight = new(0m);
			commonMovement.BM_GrossWeightUQ = ZString.Empty;
			Factory.Save();

			var factory2 = Factory.CreateNewFactory();
			var departureMovement2 = factory2.Load<NctsCommonMovementHeaderForTesting>(commonMovement.PK);

			CombineAssertions(() =>
			{
				AssertEquals(0M, departureMovement2.BM_GrossWeight);
				AssertEquals(Core.Constants.Weight.Kilograms, departureMovement2.BM_GrossWeightUQ);
			});
		}

		public void TestBM_ReducedDatasetIndicator_Caption()
		{
			NCTSTestHelper.AssertCaptions(commonMovement.BM_ReducedDatasetIndicatorInfo, "Reduced Dataset Indicator", "Reduced Dataset Ind.", string.Empty);
		}

		public void TestICusInBondCargoDescTypeProvider()
		{
			AssertEquals(typeof(NctsDepartureCargoDesc), ((ICusInBondCargoDescTypeProvider)commonMovement).CusInBondCargoDescType);
		}

		public void TestBM_TransportAtDeparture_Phase4Caption()
		{
			NCTSTestHelper.AssertCaptionsAndFullDescription(commonMovement.BM_TransportAtDepartureInfo, NctsHeader.Phase4CaptionKey, "[18] Transport ID (Departure)", string.Empty, "Dept. Transp. ID", "Departure Transport Identification");
		}

		public void TestBM_TransportAtDeparture_Phase5Caption()
		{
			NCTSTestHelper.AssertCaptions(commonMovement.BM_TransportAtDepartureInfo, NctsHeader.Phase5CaptionKey, "Transport Identification", "Transport ID", "Trans. ID");
		}

		public void TestBM_RN_NKTransportAtDepartureCountry_Phase4Caption()
		{
			NCTSTestHelper.AssertCaptionsAndFullDescription(commonMovement.BM_RN_NKTransportAtDepartureCountryInfo, NctsHeader.Phase4CaptionKey, "[18] Transport Nationality (Departure)", string.Empty, "Dept. Nat.", "Departure Transport Nationality");
		}

		public void TestBM_RN_NKTransportAtDepartureCountry_Phase5Caption()
		{
			NCTSTestHelper.AssertCaptions(commonMovement.BM_RN_NKTransportAtDepartureCountryInfo, NctsHeader.Phase5CaptionKey, "Transport Nationality", "Trans. Nationality", "Nationality");
		}

		public void TestBM_PaperlessInbondNum_Caption()
		{
			NCTSTestHelper.AssertCaptions(commonMovement.BM_PaperlessInbondNumInfo, "Customer Reference", "Customer Ref.", "LRN");
		}

		public void TestBM_TransportAtDeparture_MaxLength()
		{
			AssertEquals("Max Length", 27, commonMovement.BM_TransportAtDepartureInfo.MaxLength);
		}

		public void TestBM_InBondEntryType_MaxLength()
		{
			AssertEquals("Max Length", 4, commonMovement.BM_InBondEntryTypeInfo.MaxLength);
		}

		public void TestBM_InBondEntryType_Phase4Caption()
		{
			NCTSTestHelper.AssertCaptions(commonMovement.BM_InBondEntryTypeInfo, NctsHeader.Phase4CaptionKey, "[1] Declaration Type", string.Empty, "Dec. Ty.");
		}

		public void TestBM_InBondEntryType_Phase5Caption()
		{
			NCTSTestHelper.AssertCaptions(commonMovement.BM_InBondEntryTypeInfo, NctsHeader.Phase5CaptionKey, "Declaration Type", "Dec. Type", "Type");
		}

		public void TestRepresentative()
		{
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "REP", commonMovement.Representative, "1", traderTir: "GBR/022/1234567");
			NCTSTestHelper.AssertJobDocAddress(commonMovement.Representative, DocAddressType.Representative, "1", traderTir: "GBR/022/1234567");

			var oldRepresntativeTrader = commonMovement.Representative;
			oldRepresntativeTrader.Delete();

			CombineAssertions(() =>
			{
				var representative1 = commonMovement.Representative;
				AssertNotEquals("New Representative created", oldRepresntativeTrader.PK, representative1.PK);
				AssertSame("Cached", representative1, commonMovement.Representative);

				representative1.Delete();
				var representative2 = commonMovement.DocAddresses.CreateWithRequirement(commonMovement.RepresentativeJobDocAddressRequirement);
				AssertEquals("Representative from DocAddresses", representative2.PK, commonMovement.Representative.PK);
			});
		}

		public void TestRepresentativeDefaultContact()
		{
			var orgHeader = NCTSTestHelper.CreateJobDocAddressForTest(Factory, "REP", commonMovement.Representative, "1", traderTir: "GBR/022/1234567", contactName: "Non-CUS Contact", contactAllocation: "XXX");

			AssertEquals("Representative contact should not be set", ZString.Empty, commonMovement.Representative.E2_Contact);

			var cusContact = orgHeader.Contacts.AddNew();
			cusContact.OC_ContactName = "CUS Contact";
			cusContact.Allocations.AddNew().PC_Type = "CUS";

			commonMovement.Representative.E2_OA_Address = ZGuid.Empty;
			commonMovement.Representative.E2_OA_Address = orgHeader.MainAddress.PK;

			AssertEquals("Representative contact should be set", "CUS Contact", commonMovement.Representative.E2_Contact);
		}

		public void TestRepresentativeAdditionalValidation()
		{
			AssertNull("Representative AdditionalValidation", commonMovement.Representative.AdditionalValidation);
		}

		public void TestRepresentativeJobDocAddressRequirement()
		{
			AssertNotNull("RepresentativeJobDocAddressRequirement", commonMovement.RepresentativeJobDocAddressRequirement);
			var representativeJobDocAddressRequirement = commonMovement.RepresentativeJobDocAddressRequirement;
			AssertSame("RepresentativeJobDocAddressRequirement Cached", representativeJobDocAddressRequirement, commonMovement.RepresentativeJobDocAddressRequirement);
		}

		public void TestJobDocAddressManager()
		{
			AssertNotNull(nameof(NctsDepartureMovementHeader.JobDocAddressManager), commonMovement.JobDocAddressManager);
			var jobDocAddressManager = commonMovement.JobDocAddressManager;
			AssertSame($"{nameof(NctsDepartureMovementHeader.JobDocAddressManager)} cached", jobDocAddressManager, commonMovement.JobDocAddressManager);
		}

		public void TestDocAddresses()
		{
			AssertNotNull(nameof(NctsDepartureMovementHeader.DocAddresses), commonMovement.DocAddresses);
			var docAddresses = commonMovement.DocAddresses;
			AssertSame($"{nameof(NctsDepartureMovementHeader.DocAddresses)} cached", docAddresses, commonMovement.DocAddresses);
		}

		public void TestBM_PlaceOfUnloading_MaxLength_Phase4()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var movementHeader = header.ArrivalMovementHeader;
			AssertEquals("Max Length", 5, movementHeader.BM_PlaceOfUnloadingInfo.MaxLength);
		}

		public void TestBM_PlaceOfUnloading_MaxLength_Phase5()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertEquals("Max Length", 35, commonMovement.BM_PlaceOfUnloadingInfo.MaxLength);
		}

		public void TestBM_PlaceOfUnloading_Phase4Caption()
		{
			NCTSTestHelper.AssertCaptionsAndFullDescription(commonMovement.BM_PlaceOfUnloadingInfo, NctsHeader.Phase4CaptionKey, "Place of Unloading Code", string.Empty, "Unloading", "Place of Unloading (code)");
		}

		public void TestBM_PlaceOfUnloading_Phase5Caption()
		{
			NCTSTestHelper.AssertCaptions(commonMovement.BM_PlaceOfUnloadingInfo, NctsHeader.Phase5CaptionKey, "Place of Unloading", string.Empty, "Unloading");
		}

		public void TestBM_PlaceOfLoading_Caption()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(commonMovement.BM_PlaceOfLoadingInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Place of Loading", captionResourceString.Caption);
				AssertEquals("ShortCaption", "Loading", captionResourceString.ShortCaption);
			});
		}

		public void TestBM_RL_NKDestinationPort_MaxLength()
		{
			AssertEquals("Max Length", 2, commonMovement.BM_RL_NKDestinationPortInfo.MaxLength);
		}

		public void TestBM_RL_NKDestinationPort_Phase4Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(commonMovement.BM_RL_NKDestinationPortInfo, NctsHeader.Phase4CaptionKey, "[17A] Country/Region of Destination", "Dest.", fullDescription: "Destination Country/Region");
		}

		public void TestBM_RL_NKDestinationPort_Phase5Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(commonMovement.BM_RL_NKDestinationPortInfo, NctsHeader.Phase5CaptionKey, "Country/Region of Destination", "Destin. Ctry./Rgn.", "Destination Ctry./Rgn.");
		}

		public void TestBM_AdditionalText_MaxLength()
		{
			AssertEquals("Max Length", 70, commonMovement.BM_AdditionalTextInfo.MaxLength);
		}

		public void TestBM_GONumber_MaxLength()
		{
			AssertEquals("Max Length", 2, commonMovement.BM_GONumberInfo.MaxLength);
		}

		public void TestBM_Phase()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(commonMovement.BM_PhaseInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Phase Status", captionResourceString.Caption);
				AssertEquals("ShortCaption", "Phase", captionResourceString.ShortCaption);
				AssertEquals("List", nameof(commonMovement.Lookups) + "." + nameof(NctsCommonMovementHeaderLookups.NctsMovementHeaderTransactionStatusList), ZPropertyInfoExtensions.GetAttribute<ListAttribute>(commonMovement.BM_PhaseInfo).ListDataSourceMember);
				AssertEquals("ReadOnly", true, commonMovement.BM_PhaseInfo.ReadOnly);
				AssertEquals("Max Length", 3, commonMovement.BM_PhaseInfo.MaxLength);
			});
		}

		public void TestGrossWeight() => CombineAssertions(() =>
		{
			AssertEquals("Default", ZWeight.Empty, commonMovement.GrossWeight);

			commonMovement.BM_GrossWeightUQ = Core.Constants.Weight.Grams;
			AssertEquals("BM_GrossWeightUQ: g", ZWeight.Empty, commonMovement.GrossWeight);

			commonMovement.BM_GrossWeight = 100m;
			AssertEquals("BM_GrossWeightUQ: g, BM_GrossWeight: 100", new ZWeight(0.1m, Core.Constants.Weight.Kilograms), commonMovement.GrossWeight);

			commonMovement.BM_GrossWeightUQ = "ZZ";
			Assert("BM_GrossWeightUQ: <invalid>, BM_GrossWeight: 100", !commonMovement.GrossWeight.IsValid);
		});

		public void TestGrossWeightInKilograms() => CombineAssertions(() =>
		{
			AssertEquals("Default", 0m, commonMovement.GrossWeightInKilograms);

			commonMovement.BM_GrossWeightUQ = Core.Constants.Weight.Grams;
			AssertEquals("BM_GrossWeightUQ: g", 0m, commonMovement.GrossWeightInKilograms);

			commonMovement.BM_GrossWeight = 100m;
			AssertEquals("BM_GrossWeightUQ: g, BM_GrossWeight: 100", 0.1m, commonMovement.GrossWeightInKilograms);

			commonMovement.BM_GrossWeightUQ = "ZZ";
			AssertEquals("BM_GrossWeightUQ: <invalid>, BM_GrossWeight: 100", 0m, commonMovement.GrossWeightInKilograms);
		});

		public void TestIsArrivalMovementHeader()
		{
			CombineAssertions(() =>
			{
				commonMovement.BM_SubApplicationCode = Common.EU.NctsMoveHeaderType.Codes.Arrival;
				AssertEquals("Arrival", true, commonMovement.IsArrivalMovementHeader);
				commonMovement.BM_SubApplicationCode = Common.EU.NctsMoveHeaderType.Codes.Departure;
				AssertEquals("Departure", false, commonMovement.IsArrivalMovementHeader);
			});
		}

		public void TestIsDepartureMovementHeader()
		{
			CombineAssertions(() =>
			{
				commonMovement.BM_SubApplicationCode = Common.EU.NctsMoveHeaderType.Codes.Departure;
				AssertEquals("Departure", true, commonMovement.IsDepartureMovementHeader);
				commonMovement.BM_SubApplicationCode = Common.EU.NctsMoveHeaderType.Codes.Arrival;
				AssertEquals("Arrival", false, commonMovement.IsDepartureMovementHeader);
			});
		}

		public void TestIsUnloadingMovementHeader()
		{
			CombineAssertions(() =>
			{
				commonMovement.BM_SubApplicationCode = Common.EU.NctsMoveHeaderType.Codes.Unloading;
				AssertEquals("Unloading", true, commonMovement.IsUnloadingMovementHeader);
				commonMovement.BM_SubApplicationCode = Common.EU.NctsMoveHeaderType.Codes.Departure;
				AssertEquals("Departure", false, commonMovement.IsUnloadingMovementHeader);
			});
		}

		public void TestDefaultDataGroupingCode()
		{
			AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, commonMovement.DefaultDataGroupingCode);
		}

		public void TestSupportsClone()
		{
			AssertEquals(true, commonMovement.SupportsClone());
		}

		public void TestCloneInternal_NotCopyBM_BM_DepartureMovement()
		{
			commonMovement.BM_BM_DepartureMovement = new ZGuid("90267DA0-A314-48A0-B6BA-21D3BDF7D0E9");
			var clonedDepartureMovement = (NctsDepartureMovementHeader)commonMovement.Clone();
			AssertEquals("BM_BM_DepartureMovement should be empty", ZGuid.Empty, clonedDepartureMovement.BM_BM_DepartureMovement);
		}

		public void TestLoadOrCreate_Load()
		{
			var departureMovement2 = Factory.New<NctsDepartureMovementHeader>();
			departureMovement2.BM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);
			departureMovement2.BM_BH = nctsHeader.PK;
			Factory.Save();

			var loadedDepartureMovement = NctsCommonMovementHeader.LoadOrCreate<NctsDepartureMovementHeader>(nctsHeader, Common.EU.NctsMoveHeaderType.Codes.Departure);
			CombineAssertions(() =>
			{
				AssertEquals("Correct object", departureMovement2.PK, loadedDepartureMovement.PK);
				AssertEquals("Loaded", true, loadedDepartureMovement.IsInDatabase);
			});
		}

		public void TestLoadOrCreate_Create()
		{
			var departureMovement = NctsCommonMovementHeader.LoadOrCreate<NctsDepartureMovementHeader>(nctsHeader, Common.EU.NctsMoveHeaderType.Codes.Departure);
			CombineAssertions(() =>
			{
				AssertEquals("FK Set", nctsHeader.PK, departureMovement.BM_BH);
				AssertEquals("New Object", false, departureMovement.IsInDatabase);
			});
		}

		public void TestCreateNctsDepartureMovementHeader_Phase4()
		{
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			var departureMovement = NctsCommonMovementHeader.LoadOrCreate<NctsDepartureMovementHeader>(nctsHeader, Common.EU.NctsMoveHeaderType.Codes.Departure);
			AssertEquals("BM_TypeOfSecurity not set", ZString.Empty, departureMovement.BM_TypeOfSecurity);
		}

		public void TestCreateNctsArrivalMovementHeader()
		{
			var nctsHeader2 = Factory.New<NctsHeader>();
			nctsHeader2.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			var arrivalMovement = NctsCommonMovementHeader.LoadOrCreate<NctsArrivalMovementHeader>(nctsHeader2, Common.EU.NctsMoveHeaderType.Codes.Arrival);
			AssertEquals("BM_TypeOfSecurity not set", ZString.Empty, arrivalMovement.BM_TypeOfSecurity);
		}

		public void TestIsSimplifiedNctsProcedure()
		{
			CombineAssertions(() =>
			{
				commonMovement.IsSimplifiedNctsProcedure = true;
				AssertEquals("Underlying set A3", NctsControlResult.Codes.AuthorizedTrader, commonMovement.BM_GONumber);
				commonMovement.IsSimplifiedNctsProcedure = false;
				AssertEquals("Underlying set empty", ZString.Empty, commonMovement.BM_GONumber);
			});
		}

		public void TestBM_UniqueConsignmentReference_Phase4()
		{
			var goodsItem = commonMovement.GoodsItems.AddNew();
			goodsItem.MarkLightValidationAsValidForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Initially goodsItem.LightValidationIsValid = True", true, goodsItem.LightValidationIsValid);
				AssertEquals("Initially goodsItem.ShouldValidateOnSave = False", false, goodsItem.ShouldValidateOnSave);

				commonMovement.BM_UniqueConsignmentReference = "ref";

				AssertEquals("After set BM_UniqueConsignmentReference, goodsItem.LightValidationIsValid = True", true, goodsItem.LightValidationIsValid);
				AssertEquals("After set BM_UniqueConsignmentReference, goodsItem.ShouldValidateOnSave = False", false, goodsItem.ShouldValidateOnSave);
			});
		}

		public void TestTotalNumberOfPackages()
		{
			var bulkType = Factory.SetupBulkCusCode();

			var goodsItem1 = commonMovement.GoodsItems.AddNew();
			var package1 = goodsItem1.Packages.AddNew();
			package1.B5_UnitCount = 20;
			var package2 = goodsItem1.Packages.AddNew();
			package2.B5_UnitCount = 30;
			var goodsItem2 = commonMovement.GoodsItems.AddNew();
			var package3 = goodsItem2.Packages.AddNew();
			package3.B5_UnitCount = 50;

			var goodsItem3 = commonMovement.GoodsItems.AddNew();
			var package4 = goodsItem3.Packages.AddNew();
			package4.B5_UnitCount = 0;
			package4.B5_UnitType = bulkType;

			var goodsItem4 = commonMovement.GoodsItems.AddNew();
			var package5 = goodsItem4.Packages.AddNew();
			package5.B5_UnitCount = 10;
			package5.B5_UnitType = bulkType;
			AssertEquals(111, commonMovement.TotalNumberOfPackages);
		}

		public void TestTotalNumberOfItems()
		{
			commonMovement.GoodsItems.AddNew();
			commonMovement.GoodsItems.AddNew();
			AssertEquals(2, commonMovement.TotalNumberOfItems);
		}

		public void TestTotalGrossMassInKilograms()
		{
			var goodsItem1 = commonMovement.GoodsItems.AddNew();
			goodsItem1.BY_GrossWeight = 30;
			goodsItem1.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			var goodsItem2 = commonMovement.GoodsItems.AddNew();
			goodsItem2.BY_GrossWeight = 25;
			goodsItem2.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			AssertEquals(55m, commonMovement.TotalGrossMassInKilograms);
		}

		public void TestGoodItemsSequenceNumber()
		{
			CombineAssertions(() =>
			{
				var goodsItem1 = commonMovement.GoodsItems.AddNew();
				AssertEquals("Line 1", (ZShort)1, goodsItem1.BY_LineNo);
				var goodsItem2 = commonMovement.GoodsItems.AddNew();
				AssertEquals("Line 2", (ZShort)2, goodsItem2.BY_LineNo);
				var goodsItem3 = commonMovement.GoodsItems.AddNew();
				AssertEquals("Line 3", (ZShort)3, goodsItem3.BY_LineNo);
				commonMovement.GoodsItems.Delete(goodsItem2);
				AssertEquals("Renumbered 3 to 2", (ZShort)2, goodsItem3.BY_LineNo);
			});
		}

		public void TestINctsCusInBondCargoDescMaster_Header()
		{
			AssertEquals(nctsHeader.PK, ((INctsCusInBondCargoDescMaster)commonMovement).Header.PK);
		}

		public void TestBM_ExportTimeLimitCaption()
		{
			NCTSTestHelper.AssertCaptions(commonMovement.BM_ExportTimeLimitInfo, "Time Limit For Transit", string.Empty, "Time Limit");
		}

		public void TestLogCustomsStatus()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			commonMovement.LogCustomsStatus(NctsTransitStatusList.Codes.DeclarationInitial);
			AssertEquals("BH_ApplicationCode is not NC5, a new event is added to the logs of NctsHeader.", NctsTransitStatusList.Codes.DeclarationInitial, commonMovement.Header.Logs.Find(x => x.Event.SE_Code == AutoEvents.CustomsEntryStatusCode).FirstOrDefault()?.SL_Reference);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			commonMovement.LogCustomsStatus(NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested);
			AssertEquals("BH_ApplicationCode is NC5, a new event is added to the logs of NctsCommonMovementHeader.", NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested, commonMovement.Logs.GetAllLogs().Find(x => x.Event.SE_Code == AutoEvents.CustomsEntryStatusCode).FirstOrDefault()?.SL_Reference);
		}

		public void TestLogCustomsStatus_Phase4()
		{
			Factory.Save();
			var cesLogs = nctsHeader.Logs.GetAllLogs().Find(x => x.Event.SE_Code == AutoEvents.CustomsEntryStatusCode);
			AssertEquals("Status not set. No event logs", 0, cesLogs.Count());

			commonMovement.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested;
			AssertEquals("Status not saved. No event logs should be logged yet", 0, cesLogs.Count());
			Factory.Save();
			AssertEquals("Status saved. Event log should be created", 1, cesLogs.Count());
			var log1 = cesLogs.First();
			AssertEquals(NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested, log1.SL_Reference);

			Factory.Save();
			AssertEquals("Status not changed, no further log created", 1, cesLogs.Count());

			commonMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationAccepted;
			Factory.Save();
			AssertEquals("Status changed, another log created", 2, cesLogs.Count());
			var log2 = cesLogs.Last();
			AssertEquals(NctsTransitStatusList.Codes.DeclarationAccepted, log2.SL_Reference);

			var newNctsHeader = Factory.New<NctsHeader>();
			newNctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			newNctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			newNctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationInitial;
			Factory.Save();
			AssertEquals("Initially set status is logged", NctsTransitStatusList.Codes.DeclarationInitial, newNctsHeader.Logs.GetAllLogs().Find(x => x.Event.SE_Code == AutoEvents.CustomsEntryStatusCode).FirstOrDefault()?.SL_Reference);
		}

		public void TestLogCustomsStatus_Phase5()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			Factory.Save();
			var cesLogs = commonMovement.Logs.GetAllLogs().Find(x => x.Event.SE_Code == AutoEvents.CustomsEntryStatusCode);
			AssertEquals("Status not set. No event logs", 0, cesLogs.Count());

			commonMovement.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested;
			AssertEquals("Status not saved. No event logs should be logged yet", 0, cesLogs.Count());
			Factory.Save();
			AssertEquals("Status saved. Event log should be created", 1, cesLogs.Count());
			var log1 = cesLogs.First();
			AssertEquals(NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested, log1.SL_Reference);

			Factory.Save();
			AssertEquals("Status not changed, no further log created", 1, cesLogs.Count());

			commonMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationAccepted;
			Factory.Save();
			AssertEquals("Status changed, another log created", 2, cesLogs.Count());
			var log2 = cesLogs.Last();
			AssertEquals(NctsTransitStatusList.Codes.DeclarationAccepted, log2.SL_Reference);

			var newNctsHeader = Factory.New<NctsHeader>();
			newNctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			newNctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationInitial;
			Factory.Save();
			AssertEquals("Initially set status is logged", NctsTransitStatusList.Codes.DeclarationInitial, newNctsHeader.MovementHeader.Logs.GetAllLogs().Find(x => x.Event.SE_Code == AutoEvents.CustomsEntryStatusCode).FirstOrDefault()?.SL_Reference);
		}

		public void TestLogMessageStatus()
		{
			Factory.Save();
			var mscLogs = nctsHeader.CommonMovementHeader.Logs.GetAllLogs().Find(x => x.Event.SE_Code == AutoEvents.MessageStatusChangeCode);
			AssertEquals("Status not set. No event logs", 0, mscLogs.Count());

			commonMovement.BM_MessageStatus = NctsMessageStatusList.Codes.MessageQueued;
			AssertEquals("Status not saved. No event logs should be logged yet", 0, mscLogs.Count());
			Factory.Save();
			AssertEquals("Status saved. Event log should be created", 1, mscLogs.Count());
			var log1 = mscLogs.First();
			AssertEquals(NctsMessageStatusList.Codes.MessageQueued, log1.SL_Reference);

			Factory.Save();
			AssertEquals("Status not changed, no further log created", 1, mscLogs.Count());

			commonMovement.BM_MessageStatus = NctsMessageStatusList.Codes.SentToCustoms;
			Factory.Save();
			AssertEquals("Status changed, another log created", 2, mscLogs.Count());
			var log2 = mscLogs.Last();
			AssertEquals(NctsMessageStatusList.Codes.SentToCustoms, log2.SL_Reference);

			var newNctsHeader = Factory.New<NctsHeader>();
			newNctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			newNctsHeader.CommonMovementHeader.BM_MessageStatus = NctsMessageStatusList.Codes.MessageQueued;
			Factory.Save();
			AssertEquals("Initially set status is logged", NctsMessageStatusList.Codes.MessageQueued, newNctsHeader.CommonMovementHeader.Logs.GetAllLogs().Find(x => x.Event.SE_Code == AutoEvents.MessageStatusChangeCode).FirstOrDefault()?.SL_Reference);
		}

		public void TestRepresentativeReadOnly()
		{
			AssertEquals("Representative should be not readonly", false, nctsHeader.MovementHeader.Representative.ReadOnly);
		}

		public void TestPhaseStatusDescription()
		{
			NCTSTestHelper.AssertCaptions(typeof(NctsCommonMovementHeader), nameof(NctsCommonMovementHeader.PhaseStatusDescription), "Phase Status Description", string.Empty, "Ph. Desc.");

			commonMovement.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			AssertEquals("Valid code", NctsMovementHeaderTransactionStatusList.Descriptions.Declaration, commonMovement.PhaseStatusDescription);

			commonMovement.BM_Phase = "@@@";
			AssertEquals("Invalid code", ZString.Empty, commonMovement.PhaseStatusDescription);

			commonMovement.BM_Phase = ZString.Empty;
			AssertEquals("Empty code", ZString.Empty, commonMovement.PhaseStatusDescription);
		}

		public void TestCustomsOffices()
		{
			var customsOffices = commonMovement.CustomsOffices;
			CombineAssertions(() =>
			{
				AssertType<NctsEuOfficeCodeCollection>(customsOffices);
				AssertEquals("IsRegisteredEditableChildObject", true, commonMovement.IsRegisteredEditableChildObject(customsOffices));
				AssertSame("Cached", customsOffices, commonMovement.CustomsOffices);
				AssertEquals("IsLoaded", true, customsOffices.IsLoaded);
			});
		}

		public void TestShouldGenerateLocalReferenceNumberOnFactorySavingInsteadOnSaving()
		{
			var commonMovementForTesting = Factory.New<NctsCommonMovementHeaderForTesting>();
			commonMovementForTesting.BM_PaperlessInbondNum = ZString.Empty;
			commonMovementForTesting.ShouldGenerateLocalReferenceNumberOnSavingCoreForTesting = false;

			CombineAssertions(() =>
			{
				AssertEquals("Condition for OnSave Local Reference Number Generation returns false", (commonMovementForTesting.ShouldGenerateLocalReferenceNumberOnSaving && !commonMovementForTesting.ShouldGenerateLocalReferenceNumberOnFactorySaving), false);
				AssertEquals("Condition for OnFactorySave Local Reference Number Generation returns true", commonMovementForTesting.ShouldGenerateLocalReferenceNumberOnFactorySaving, true);
			});
		}

		public void TestForceRegenerateLocalReferenceNumber()
		{
			var commonMovementForTesting = Factory.New<NctsCommonMovementHeaderForTesting>();
			commonMovementForTesting.BM_PaperlessInbondNum = ZString.Empty;

			CombineAssertions(() =>
			{
				AssertEquals("ShouldGenerateLocalReferenceNumberOnSaving returns true due to ForceRegenerateLocalReferenceNumber being true", commonMovementForTesting.ShouldGenerateLocalReferenceNumberOnSaving, true);
				AssertEquals("ShouldGenerateLocalReferenceNumberOnFactorySaving returns true due to ForceRegenerateLocalReferenceNumber being true", commonMovementForTesting.ShouldGenerateLocalReferenceNumberOnFactorySaving, true);

				commonMovementForTesting.ForceRegenerateLocalReferenceNumberCore = false;
				commonMovementForTesting.BM_PaperlessInbondNum = "ABC";

				AssertEquals("ShouldGenerateLocalReferenceNumberOnSaving returns true due to ForceRegenerateLocalReferenceNumber being false", commonMovementForTesting.ShouldGenerateLocalReferenceNumberOnSaving, false);
				AssertEquals("ShouldGenerateLocalReferenceNumberOnFactorySaving returns true due to ForceRegenerateLocalReferenceNumber being false", commonMovementForTesting.ShouldGenerateLocalReferenceNumberOnFactorySaving, false);
			});
		}

		[TestDate(2023, 12, 22)]
		public void TestOnSavedRecoverFromUnsuccessfulSave()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			commonMovement.HeaderBranch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "BE1230789654", "BE");

			CombineAssertions(() =>
			{
				nctsHeader.BH_OH_Carrier = ZGuid.Invalid;
				AssertExceptionThrown<ZSaveException>(Factory.Save);
				AssertEquals("BM_PaperlessInbondNum recovered to empty when Movement is not in database", ZString.Empty, commonMovement.BM_PaperlessInbondNum);

				nctsHeader.BH_OH_Carrier = ZGuid.Empty;
				Factory.Save();

				var savedLRNNumber = commonMovement.BM_PaperlessInbondNum;
				AssertNotEquals("Autogenerated LNR Nbr from Save()", ZString.Empty, savedLRNNumber);

				commonMovement.BM_PaperlessInbondNum = "";
				nctsHeader.BH_OH_Carrier = ZGuid.Invalid;
				AssertExceptionThrown<ZSaveException>(Factory.Save);
				AssertEquals("BM_PaperlessInbondNum recovered to saved LRN when Movement is in database", savedLRNNumber, nctsHeader.MovementHeader.BM_PaperlessInbondNum);
			});
		}

		public void TestIWorkflowTriggerEventSource()
		{
			var source = commonMovement as IWorkflowTriggerEventSource;
			AssertNotNull(source);

			CombineAssertions(() =>
			{
				var parentWorkflowProviders = source.ParentWorkflowProviders;
				AssertEquals(1, parentWorkflowProviders.Count);
				AssertEquals(nctsHeader.PK, source.ParentWorkflowProviders[0].PK);
				AssertEquals(nctsHeader.Company.PK, source.JobHeaderCompany.PK);
				AssertSame("Cached", parentWorkflowProviders, source.ParentWorkflowProviders);
			});
		}

		public void TestNewDepartureMovementHeader_CorrectTypeDecided()
		{
			var deBranch = Factory.NewCompanyAndBranchWith("DEC", "DEB", "DE");
			var auBranch = Factory.NewCompanyAndBranchWith("AUC", "AUB", "AU");
			Factory.Save();

			NctsHeader nctsHeader;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), deBranch.PK.ToGuid(), deBranch.Company.PK.ToGuid()))
			{
				nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

				nctsHeader.MovementHeader.Delete();
			}

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), auBranch.PK.ToGuid(), auBranch.Company.PK.ToGuid()))
			{
				var departureMovement = nctsHeader.MovementHeader;

				AssertEquals("Type should be decided based on Company Country of parent NCTS Header", true, departureMovement is Integration.Customs.DE.IDepartureMovementHeader);
			}
		}

		public void TestMessageStatusLogAddedIsInvoked()
		{
			commonMovement.BM_MessageStatus = "XXX";
			var messageStatusLogAddedEventCount = 0;
			commonMovement.MessageStatusLogAdded += CommonMovement_MessageStatusLogAdded;

			Factory.Save();
			commonMovement.MessageStatusLogAdded -= CommonMovement_MessageStatusLogAdded;

			AssertEquals("MessageStatusLogAdded event was invoked", 1, messageStatusLogAddedEventCount);

			void CommonMovement_MessageStatusLogAdded(object sender, NctsCommonMovementHeader.CustomsStatusLogAddedEventArgs e)
			{
				messageStatusLogAddedEventCount++;

				CombineAssertions("Argument Args", () =>
				{
					AssertSame("MovementHeader", commonMovement, e.MovementHeader);
					AssertNotNull("Log", e.Log);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			commonMovement = nctsHeader.CommonMovementHeader;
		}

		class NctsCommonMovementHeaderForTesting : NctsCommonMovementHeader
		{
			public NctsCommonMovementHeaderForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}
			protected override bool BM_PaperlessInbondNumReadOnly => true;

			protected override bool ForceRegenerateLocalReferenceNumber => ForceRegenerateLocalReferenceNumberCore;

			public bool ForceRegenerateLocalReferenceNumberCore
			{
				get { return forceRegenerateLocalReferenceNumberCore; }

				set { forceRegenerateLocalReferenceNumberCore = value; }
			}

			bool forceRegenerateLocalReferenceNumberCore = true;

			protected override bool ShouldGenerateLocalReferenceNumberOnSavingCore => ShouldGenerateLocalReferenceNumberOnSavingCoreForTesting;

			public bool ShouldGenerateLocalReferenceNumberOnSavingCoreForTesting = true;

			protected override bool ShouldGenerateLocalReferenceNumberOnFactorySavingCore => ShouldGenerateLocalReferenceNumberOnFactorySavingCoreForTesting;

			public bool ShouldGenerateLocalReferenceNumberOnFactorySavingCoreForTesting = true;

			protected override Type CusInBondCargoDescTypeCore => throw new NotImplementedException();
		}

		NctsHeader nctsHeader;
		NctsCommonMovementHeader commonMovement;
	}
}
