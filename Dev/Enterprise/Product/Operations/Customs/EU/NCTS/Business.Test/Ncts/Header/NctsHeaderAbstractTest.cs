using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestsSubclassesOf(typeof(NctsHeader))]
	public abstract class NctsHeaderAbstractTest : EnterpriseBusinessObjectTestCase
	{
		public virtual void TestOverrideAdditionalInfoTypeForOptimisation()
		{
			var header = (NctsHeader)GetNewBusinessObject();
			var headerType = header.GetType();
			if (headerType != typeof(NctsHeader))
			{
				var cusSupportingInfoTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)header).GetCusSupportingInfoTypes();
				if (cusSupportingInfoTypes.TryGetValue(Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo, out var additionalInfoType)
					&& additionalInfoType != typeof(NctsAdditionalInfo)
					&& headerType.GetProperty("AdditionalInfoType", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly) == null)
				{
					Fail($"Override {headerType.FullName}.AdditionalInfoType to return {additionalInfoType.FullName} for better performance instead of relying on NctsTypeDecider");
				}
			}
			Assert(true);
		}

		public void TestClone_Phase5Departure()
		{
			var departure = CreateTestDeparture(CusInBondApplicationCodeList.Codes.NCTS5, inlandTransportMode: ModeOfTransportList.Codes._2_RailTransport);
			departure.PreviousDocuments.AddNew().CSI_ReferenceNumber = "PD3";
			departure.PreviousDocuments.AddNew().CSI_ReferenceNumber = "PD4";
			departure.AdditionalDocuments.AddNew().CSI_Description = "AI3";
			departure.AdditionalDocuments.AddNew().CSI_Description = "AI4";
			departure.CountriesOfRouting.AddNew().CY_Data = Core.Constants.CountryCodes.Australia;
			departure.CountriesOfRouting.AddNew().CY_Data = Core.Constants.CountryCodes.NewZealand;
			departure.CusSupplyChainActors.AddNew().CFR_Reference = "SAR3";
			departure.CusSupplyChainActors.AddNew().CFR_Reference = "SAR4";
			AddBill(departure, "MB1", false);
			AddBill(departure, "MB2", true);
			var departureMovementHeader = departure.MovementHeader;
			departureMovementHeader.AdditionalTransportAtBorderList.AddNew().TPM_IdentificationNumber = "1234";
			departureMovementHeader.AdditionalTransportAtBorderList.AddNew().TPM_IdentificationNumber = "5678";
			departureMovementHeader.SupportingDocuments.AddNew().CSI_ReferenceNumber = "SD3";
			departureMovementHeader.SupportingDocuments.AddNew().CSI_ReferenceNumber = "SD4";

			CombineAssertions(() =>
			{
				var clone = (NctsHeader)departure.TemplateCopy();
				var cloneMovementHeader = clone.MovementHeader;
				AssertCloneResult_Common(clone, NctsMovementType.Codes.Departure);
				AssertCloneResult_Departure_Phase5(clone, ModeOfTransportList.Codes._2_RailTransport);
				AssertEquals(ZString.Empty, cloneMovementHeader.BM_MessageStatus);
				AssertEquals("clone.SupportingDocuments.Count", 2, cloneMovementHeader.SupportingDocuments.Count);
				AssertEquals("clone.SupportingDocuments[0].CSI_ReferenceNumber", "SD3", cloneMovementHeader.SupportingDocuments[0].CSI_ReferenceNumber);
				AssertEquals("clone.SupportingDocuments[1].CSI_ReferenceNumber", "SD4", cloneMovementHeader.SupportingDocuments[1].CSI_ReferenceNumber);
				AssertEquals("clone.PreviousDocuments.Count", 2, clone.PreviousDocuments.Count);
				AssertEquals("clone.PreviousDocuments[0].CSI_ReferenceNumber", "PD3", clone.PreviousDocuments[0].CSI_ReferenceNumber);
				AssertEquals("clone.PreviousDocuments[1].CSI_ReferenceNumber", "PD4", clone.PreviousDocuments[1].CSI_ReferenceNumber);
				AssertEquals("clone.AdditionalDocuments.Count", 2, clone.AdditionalDocuments.Count);
				AssertEquals("clone.AdditionalDocuments[0].CSI_Description", "AI3", clone.AdditionalDocuments[0].CSI_Description);
				AssertEquals("clone.AdditionalDocuments[1].CSI_Description", "AI4", clone.AdditionalDocuments[1].CSI_Description);
				AssertEquals("clone.CusSupplyChainActors.Count", 2, clone.CusSupplyChainActors.Count);
				AssertEquals("clone.CusSupplyChainActors[0].CFR_Reference", "SAR3", clone.CusSupplyChainActors[0].CFR_Reference);
				AssertEquals("clone.CusSupplyChainActors[1].CFR_Reference", "SAR4", clone.CusSupplyChainActors[1].CFR_Reference);
				AssertEquals("clone.Bills.Count", 2, clone.Bills.Count);
				AssertCloneResult_Bill(clone.Bills[0], "MB1", true);
				AssertCloneResult_Bill(clone.Bills[1], "MB2", false);
			});
		}

		public virtual void TestClone_Phase4()
		{
			var departure = CreateTestDeparture(CusInBondApplicationCodeList.Codes.NCTS4);
			var clone = (NctsHeader)departure.TemplateCopy();
			var cloneGuarantees = clone.Guarantees;
			CombineAssertions(() =>
			{
				for (var i = 0; i <= 1; i++)
				{
					AssertEquals($"clone.Guarantee{i}: PW_BondNumber", $"G{i + 1} ref", cloneGuarantees[i].PW_BondNumber);
					AssertEquals($"clone.Guarantee{i}: PW_ParentID", clone.PK, cloneGuarantees[i].PW_ParentID);
				}
			});
		}

		public void TestClone_Phase5Arrival()
		{
			var arrival = CreateTestArrival(CusInBondApplicationCodeList.Codes.NCTS5);
			var clone = (NctsHeader)arrival.TemplateCopy();
			AssertCloneResult_Common(clone, NctsMovementType.Codes.Arrival);
			AssertCloneResult_Arrival_Phase5(clone);
		}

		public void TestCusSealType()
		{
			var header = (NctsHeader)GetNewBusinessObject();
			AssertType(((ICusSealTypeSupporter)header).CusSealType, header.CusSeals.AddNew());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = (NctsHeader)base.GetNewBusinessObject();

			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			return header;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = (NctsHeader)base.GetNewBusinessObjectForDeleteTest(factory);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.BH_HeaderType = NctsMovementType.Codes.Departure;
			header.MovementHeader.CustomsOffices.AddNew();
			return header;
		}

		protected override void SetUp()
		{
			base.SetUp();
			helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNctsDeclarationTypeList(GlbCompany.CurrentCompany.Country.Code);
			Factory.Save();
		}

		protected UniversalReferenceTestDataHelper helper;

		protected IReadOnlyList<string> ExpectedUnloadingAllowedOrCompleteStatusList => new string[] { "AUP", "ART" };

		public void TestIsUnloadingAllowedOrComplete_Arrival_Phase4()
		{
			TestIsUnloadingAllowedOrComplete_Arrival(CusInBondApplicationCodeList.Codes.NCTS4, ExpectedUnloadingAllowedOrCompleteStatusList);
		}

		protected IReadOnlyList<string> ExpectedUnloadingAllowedOrCompleteStatusListPhase5 => new string[] { "UAP", "ULR", "CL1", "CL3", "CD2", "CD4" };

		public void TestIsUnloadingAllowedOrComplete_Arrival_Phase5()
		{
			TestIsUnloadingAllowedOrComplete_Arrival(CusInBondApplicationCodeList.Codes.NCTS5, ExpectedUnloadingAllowedOrCompleteStatusListPhase5);
		}

		protected virtual void TestIsUnloadingAllowedOrComplete_Arrival(ZString applicationCode, IReadOnlyList<string> statusList)
		{
			var nctsHeader = (NctsHeader)Factory.New(TestedTypeHelper.GetTestedType(GetType()));

			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = applicationCode;
			PrepareArrivalMovementHeaderForIsUnloadingAllowedOrCompleteTests(nctsHeader);
			var arrivalMovementHeader = nctsHeader.ArrivalMovementHeader;
			var statusCodeList = new NctsTransitStatusList().GetAllCodes();
			CombineAssertions(() =>
			{
				foreach (var status in statusCodeList)
				{
					arrivalMovementHeader.BM_CustomsStatus = status;
					AssertEquals($"Code {status} - {nctsHeader.BH_ApplicationCode}", statusList.Contains(status), nctsHeader.IsUnloadingAllowedOrComplete);
				}
			});
		}

		protected virtual void PrepareArrivalMovementHeaderForIsUnloadingAllowedOrCompleteTests(NctsHeader header) { }

		public void TestIsUnloadingAllowedOrComplete_Departure()
		{
			var nctsHeader = (NctsHeader)Factory.New(TestedTypeHelper.GetTestedType(GetType()));

			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			AssertEquals(false, nctsHeader.IsUnloadingAllowedOrComplete);
		}

		public virtual void TestICusInBondContainerTypeSupporter()
		{
			var header = (NctsHeader)Factory.New(TestedTypeHelper.GetTestedType(GetType()));
			header.BH_HeaderType = ZString.Empty;
			var supporter = header as ICusInBondContainerTypeSupporter;
			AssertEquals("DepartureContainerTypeCore should return correct Departure ContainerType", supporter.ContainerType, header.DepartureHeaderContainers.AddNew().GetType());

			header = (NctsHeader)Factory.New(TestedTypeHelper.GetTestedType(GetType()));
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			supporter = header;
			header.BH_HeaderType = NctsMovementType.Codes.Arrival;
			AssertEquals("ArrivalContainerTypeCore should return Departure for Phase 4 Arrival", supporter.ContainerType, header.DepartureHeaderContainers.AddNew().GetType());

			header = (NctsHeader)Factory.New(TestedTypeHelper.GetTestedType(GetType()));
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			supporter = header;
			header.BH_HeaderType = NctsMovementType.Codes.Arrival;
			AssertEquals("ArrivalContainerTypeCore should return Arrival for Phase 5 Arrival", supporter.ContainerType, header.ArrivalHeaderContainers.AddNew().GetType());

			header = (NctsHeader)Factory.New(TestedTypeHelper.GetTestedType(GetType()));
			supporter = header;
			header.BH_HeaderType = NctsMovementType.Codes.Departure;
			AssertEquals("DepartureContainerTypeCore should return correct Departure ContainerType", supporter.ContainerType, header.DepartureHeaderContainers.AddNew().GetType());
		}

		protected ZGuid cnrPk;
		protected ZGuid cnePk;
		protected ZGuid cnrOrgPk;
		protected ZGuid principalPk;
		protected ZGuid secCnrPk;
		protected ZGuid secCnePk;
		protected ZGuid carrierPk;
		protected ZGuid representativePk;
		protected ZGuid lineCnrPk;
		protected ZGuid lineCnePk;
		protected ZGuid lineSecCnrPk;
		protected ZGuid lineSecCnePk;

		protected void CreateTestOrgAddresses()
		{
			var orgCnr = Factory.NewWithValidTestData<OrgHeader>();
			var orgCne = Factory.NewWithValidTestData<OrgHeader>();
			cnrOrgPk = orgCnr.PK;
			cnrPk = orgCnr.MainAddress.PK;
			cnePk = orgCne.MainAddress.PK;
			principalPk = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			secCnrPk = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			secCnePk = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			carrierPk = Factory.NewWithValidTestData<OrgHeader>().PK;
			representativePk = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			lineCnrPk = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			lineCnePk = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			lineSecCnrPk = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			lineSecCnePk = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
		}

		protected NctsHeader CreatePhase4DepartureHeader() => CreateHeader(CusInBondApplicationCodeList.Codes.NCTS4, NctsMovementType.Codes.Departure);
		protected NctsHeader CreatePhase5DepartureHeader() => CreateHeader(CusInBondApplicationCodeList.Codes.NCTS5, NctsMovementType.Codes.Departure);
		protected NctsHeader CreatePhase4ArrivalHeader()
		{
			var arrivalHeader = CreateHeader(CusInBondApplicationCodeList.Codes.NCTS4, NctsMovementType.Codes.Arrival);
			arrivalHeader.BH_ExportFlag = EventFlagList.Codes.Yes;
			return arrivalHeader;
		}
		protected NctsHeader CreatePhase5ArrivalHeader()
		{
			var arrivalHeader = CreateHeader(CusInBondApplicationCodeList.Codes.NCTS5, NctsMovementType.Codes.Arrival);
			arrivalHeader.BH_ExportFlag = EventFlagList.Codes.Yes;
			return arrivalHeader;
		}

		protected NctsHeader CreateHeader(ZString applicationCode, ZString headerType)
		{
			var departureHeader = Factory.New<NctsHeader>();
			departureHeader.BH_ApplicationCode = applicationCode;
			departureHeader.SetMovementType(headerType);
			return departureHeader;
		}

		protected NctsHeader CreateTestArrival(ZString applicationCode)
		{
			var arrivalHeader = CreateHeader(applicationCode, NctsMovementType.Codes.Arrival);
			arrivalHeader.BH_ExportFlag = EventFlagList.Codes.Yes;
			CreateTestOrgAddresses();
			NCTSTestHelper.SetMrnForTest(arrivalHeader, "MRN");
			NCTSTestHelper.SetupContainersAndSealsForTest(arrivalHeader);
			//authorisation
			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorizationHeader.CPH_OH_PermitHolder = cnrOrgPk;
			authorizationHeader.CPH_OA_AppliesTo = cnrPk;
			authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;

			//Front screen
			var arrivalMovementHeader = arrivalHeader.ArrivalMovementHeader;
			arrivalMovementHeader.BM_InBondEntryType = "T-";
			arrivalHeader.BH_FTZMove = true;
			arrivalMovementHeader.IsSimplifiedNctsProcedure = true;
			arrivalMovementHeader.BM_GS_NKCusAgent = "~BP";
			arrivalHeader.Consignor.E2_OA_Address = cnrPk;
			arrivalHeader.Consignee.E2_OA_Address = cnePk;
			arrivalHeader.Principal.E2_OA_Address = principalPk;
			arrivalMovementHeader.AuthorizationCode = "AZC";
			arrivalMovementHeader.AuthorizationNumber = "AZNumber";
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			arrivalMovementHeader.AuthorizationOwner = orgHeader.PK;
			var goodsLocation = arrivalMovementHeader.GoodsLocation;
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
			arrivalHeader.BH_ExportFlag = "Y";

			// Transport and locations tab
			arrivalMovementHeader.BM_InlandTransportMode = "26";
			arrivalMovementHeader.BM_TransportAtDeparture = "18.ID";
			arrivalMovementHeader.BM_RN_NKTransportAtDepartureCountry = "18";
			arrivalMovementHeader.BM_ExportTransportMode = "25";
			arrivalMovementHeader.BM_TOLCarrierID = "21.ID";
			arrivalMovementHeader.BM_TOLCarrierCode = "21";
			arrivalHeader.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.UnitedKingdom;
			arrivalMovementHeader.BM_RL_NKDestinationPort = "IT";
			arrivalMovementHeader.BM_RL_NKForeignDestPort = "27.N";
			arrivalMovementHeader.BM_LocationOfGoodsCode = "30.1";
			arrivalMovementHeader.BM_LocationOfGoods = "30.2";
			arrivalMovementHeader.BM_CustomsSubPlace = "30.3";
			arrivalMovementHeader.BM_ExportDate = new ZDateTime(1986, 3, 12);

			// Security tab
			arrivalMovementHeader.BM_BTAIndicator = "S";
			arrivalMovementHeader.BM_MethodOfPayment = "P";
			arrivalMovementHeader.BM_AdditionalText = "7";
			arrivalMovementHeader.BM_ConveyanceNumber = "S7";
			arrivalHeader.PlaceOfUnloadingCode = "UNLOD";
			arrivalHeader.SecurityConsignor.E2_OA_Address = secCnrPk;
			arrivalHeader.SecurityConsignee.E2_OA_Address = secCnePk;
			arrivalHeader.BH_OH_Carrier = carrierPk;
			arrivalHeader.BH_UniqueVoyageIdentifier = "112233"; // Itinerary

			// Offices
			var dep = arrivalMovementHeader.CustomsOffices.AddNew();
			var des = arrivalMovementHeader.CustomsOffices.AddNew();
			var enq = arrivalMovementHeader.CustomsOffices.AddNew();
			dep.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
			des.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival;
			enq.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfEnquiry;
			dep.CY_Data = "Off1";
			des.CY_Data = "Off2";
			enq.CY_Data = "Off3";

			if (applicationCode == CusInBondApplicationCodeList.Codes.NCTS4)
			{
				AddLine(arrivalHeader, Core.Constants.Weight.Kilograms, "T1");
				AddLine(arrivalHeader, Core.Constants.Weight.Pounds, "T2");

				var g1 = arrivalHeader.Guarantees.AddNew();
				g1.PW_BondNumber = "G1 ref";
				var g2 = arrivalHeader.Guarantees.AddNew();
				g2.PW_BondNumber = "G2 ref";
			}

			arrivalHeader.Messages.AddNew().EM_ReceiveTransmit = "RCV";

			return arrivalHeader;
		}

		protected NctsHeader CreateTestDeparture(ZString applicationCode, string headerType = NctsMovementType.Codes.Departure, string inlandTransportMode = "26")
		{
			CreateTestOrgAddresses();
			var departureHeader = CreateHeader(applicationCode, headerType);
			NCTSTestHelper.SetMrnForTest(departureHeader, "MRN");
			NCTSTestHelper.SetupContainersAndSealsForTest(departureHeader);
			departureHeader.Seals.AddNew().CY_Data = "SLN1";
			departureHeader.Seals.AddNew().CY_Data = "SLN2";

			//authorisation
			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorizationHeader.CPH_OH_PermitHolder = cnrOrgPk;
			authorizationHeader.CPH_OA_AppliesTo = cnrPk;
			authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;

			//Front screen
			var departureMovementHeader = departureHeader.MovementHeader;
			departureMovementHeader.SetMixedConsignment();
			departureHeader.BH_FTZMove = true;
			departureMovementHeader.IsSimplifiedNctsProcedure = true;
			departureMovementHeader.BM_GS_NKCusAgent = "~BP";
			departureHeader.Consignor.E2_OA_Address = cnrPk;
			departureHeader.Consignee.E2_OA_Address = cnePk;
			departureHeader.Principal.E2_OA_Address = principalPk;
			departureMovementHeader.Representative.E2_OA_Address = representativePk;
			var goodsLocation = departureMovementHeader.GoodsLocation;
			goodsLocation.CGL_Qualifier = "T";
			goodsLocation.CGL_Type = "B";
			goodsLocation.CGL_AdditionalIdentifier = "AH3Location";

			// Transport and locations tab
			departureMovementHeader.BM_InlandTransportMode = inlandTransportMode;
			departureMovementHeader.BM_TransportAtDeparture = "18.ID";
			departureMovementHeader.BM_RN_NKTransportAtDepartureCountry = "18";
			departureMovementHeader.BM_ExportTransportMode = "25";
			departureMovementHeader.BM_TOLCarrierID = "21.ID";
			departureMovementHeader.BM_TOLCarrierCode = "21";
			departureHeader.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.UnitedKingdom;
			departureMovementHeader.BM_RL_NKDestinationPort = "IT";
			departureMovementHeader.BM_RL_NKForeignDestPort = "27.N";
			departureMovementHeader.BM_LocationOfGoodsCode = "30.1";
			departureMovementHeader.BM_LocationOfGoods = "30.2";
			departureMovementHeader.BM_CustomsSubPlace = "30.3";
			departureMovementHeader.BM_ExportDate = new ZDateTime(1986, 3, 12);

			// Security tab
			departureMovementHeader.BM_BTAIndicator = "S";
			departureMovementHeader.BM_MethodOfPayment = "P";
			departureMovementHeader.BM_AdditionalText = "7";
			departureMovementHeader.BM_ConveyanceNumber = "S7";
			departureHeader.PlaceOfUnloadingCode = "UNLOD";
			departureHeader.SecurityConsignor.E2_OA_Address = secCnrPk;
			departureHeader.SecurityConsignee.E2_OA_Address = secCnePk;
			departureHeader.BH_OH_Carrier = carrierPk;
			departureHeader.BH_UniqueVoyageIdentifier = "112233"; // Itinerary

			// Offices

			departureMovementHeader.CustomsOffices.RemoveAndDeleteAll();
			var dep = departureMovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "Off1");
			var des = departureMovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination, "Off2");
			var enq = departureMovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfEnquiry, "Off3");

			if (applicationCode == CusInBondApplicationCodeList.Codes.NCTS4)
			{
				AddLine(departureHeader, Core.Constants.Weight.Kilograms, "T1");
				AddLine(departureHeader, Core.Constants.Weight.Pounds, "T2");

				var g1 = departureHeader.Guarantees.AddNew();
				g1.PW_BondNumber = "G1 ref";
				var g2 = departureHeader.Guarantees.AddNew();
				g2.PW_BondNumber = "G2 ref";
			}

			departureHeader.Messages.AddNew().EM_ReceiveTransmit = "RCV";

			return departureHeader;
		}

		protected NctsBill AddBill(NctsHeader header, ZString masterBillNumber, bool setTransportDeparture)
		{
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = masterBillNumber;
			bill.SupportingDocuments.AddNew().CSI_ReferenceNumber = "SD5";
			bill.SupportingDocuments.AddNew().CSI_ReferenceNumber = "SD6";
			bill.PreviousDocuments.AddNew().CSI_ReferenceNumber = "PD5";
			bill.PreviousDocuments.AddNew().CSI_ReferenceNumber = "PD6";
			var additionalDocuments1 = bill.AdditionalDocuments.AddNew();
			additionalDocuments1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			additionalDocuments1.CSI_Description = "AI5";
			var additionalDocuments2 = bill.AdditionalDocuments.AddNew();
			additionalDocuments2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			additionalDocuments2.CSI_Description = "AI6";
			bill.CusSupplyChainActorReferences.AddNew().CFR_Reference = "SAR5";
			bill.CusSupplyChainActorReferences.AddNew().CFR_Reference = "SAR6";
			AddDepartureCargoDesc(bill.GoodsItems, "DESC5");
			AddDepartureCargoDesc(bill.GoodsItems, "DESC6");

			if (setTransportDeparture)
			{
				bill.TransportTypeAtDeparture = "30";
				bill.FirstDepartureTransportMeansID = "DEP1";
				bill.FirstDepartureTransportMeansNationality = "IT";
				bill.SecondDepartureTransportMeansID = "DEP2";
				bill.SecondDepartureTransportMeansNationality = "FR";
				bill.ThirdDepartureTransportMeansID = "DEP3";
				bill.ThirdDepartureTransportMeansNationality = "ES";
			}

			return bill;
		}

		protected NctsDepartureCargoDesc AddDepartureCargoDesc(INctsDepartureCargoDescCollection<NctsDepartureCargoDesc> goodsItems, ZString description)
		{
			var cargoDesc = goodsItems.AddNew();
			cargoDesc.BY_Description = description;
			cargoDesc.Packages.AddNew().B5_MarksAndNumbers = "MARKS1";
			cargoDesc.Packages.AddNew().B5_MarksAndNumbers = "MARKS2";
			cargoDesc.AdditionalInfos.AddNew().CSI_Description = "AI7";
			cargoDesc.AdditionalInfos.AddNew().CSI_Description = "AI8";
			cargoDesc.SupportingDocuments.AddNew().CSI_ReferenceNumber = "SD7";
			cargoDesc.SupportingDocuments.AddNew().CSI_ReferenceNumber = "SD8";
			cargoDesc.PreviousDocuments.AddNew().CSI_ReferenceNumber = "PD7";
			cargoDesc.PreviousDocuments.AddNew().CSI_ReferenceNumber = "PD8";
			cargoDesc.Consignee.E2_OA_Address = lineCnePk;
			cargoDesc.Consignor.E2_OA_Address = lineCnrPk;
			cargoDesc.UNDGs.AddNew().DI_DG = DangerousSubstance1.PK;
			cargoDesc.UNDGs.AddNew().DI_DG = DangerousSubstance2.PK;
			cargoDesc.BY_CustomsThirdQuantity = 3.3m;
			cargoDesc.BY_CustomsThirdUnitQty = "K3";
			cargoDesc.AdditionalSupplementaryCodes.AddNew().CY_Data = "ASC7";
			cargoDesc.AdditionalSupplementaryCodes.AddNew().CY_Data = "ASC8";
			cargoDesc.Fees.AddNew().BFE_ChargeType = "A01";
			cargoDesc.Fees.AddNew().BFE_ChargeType = "A02";
			cargoDesc.CusSupplyChainActorReferences.AddNew().CFR_Reference = "SAR7";
			cargoDesc.CusSupplyChainActorReferences.AddNew().CFR_Reference = "SAR8";
			return cargoDesc;
		}

		protected UNDGSubstance DangerousSubstance1 => dangerousSubstance1 ??= UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First();
		UNDGSubstance dangerousSubstance1;

		protected UNDGSubstance DangerousSubstance2 => dangerousSubstance2 ??= UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "b", "IMO").First();
		UNDGSubstance dangerousSubstance2;

		protected override void TestBizObjectField(ZPropertyInfo info)
		{
			var nctsHeader = (NctsHeader)info.BizObj;
			var excluded = nctsHeader.IsPhase5 && info.Name.In(new string[] { nameof(NctsHeader.DestinationCustomsOfficeCodeForDeparture), nameof(NctsHeader.DestinationCustomsOfficeCodeForArrival) });
			if (!excluded)
			{
				base.TestBizObjectField(info);
			}
		}

		protected void AssertCloneResult_Bill(NctsBill bill, ZString masterBillNumber, bool assertEmptyTransportDeparture)
		{
			AssertEquals("bill.B0_MasterBillNumber", masterBillNumber, bill.B0_MasterBillNumber);
			AssertEquals("bill.SupportingDocuments.Count", 2, bill.SupportingDocuments.Count);
			AssertEquals("bill.SupportingDocuments[0].CSI_ReferenceNumber", "SD5", bill.SupportingDocuments[0].CSI_ReferenceNumber);
			AssertEquals("bill.SupportingDocuments[1].CSI_ReferenceNumber", "SD6", bill.SupportingDocuments[1].CSI_ReferenceNumber);
			AssertEquals("bill.PreviousDocuments.Count", 2, bill.PreviousDocuments.Count);
			AssertEquals("bill.PreviousDocuments[0].CSI_ReferenceNumber", "PD5", bill.PreviousDocuments[0].CSI_ReferenceNumber);
			AssertEquals("bill.PreviousDocuments[1].CSI_ReferenceNumber", "PD6", bill.PreviousDocuments[1].CSI_ReferenceNumber);
			AssertEquals("bill.AdditionalDocuments.Count", 2, bill.AdditionalDocuments.Count);
			AssertEquals("bill.AdditionalDocuments[0].CSI_Description", "AI5", bill.AdditionalDocuments[0].CSI_Description);
			AssertEquals("bill.AdditionalDocuments[1].CSI_Description", "AI6", bill.AdditionalDocuments[1].CSI_Description);
			AssertEquals("bill.AdditionalDocuments[0].CSI_LineNo", (ZShort)1, bill.AdditionalDocuments[0].CSI_LineNo);
			AssertEquals("bill.AdditionalDocuments[1].CSI_LineNo", (ZShort)2, bill.AdditionalDocuments[1].CSI_LineNo);
			AssertEquals("bill.CusSupplyChainActorReferences.Count", 2, bill.CusSupplyChainActorReferences.Count);
			AssertEquals("bill.CusSupplyChainActorReferences[0].CFR_Reference", "SAR5", bill.CusSupplyChainActorReferences[0].CFR_Reference);
			AssertEquals("bill.CusSupplyChainActorReferences[1].CFR_Reference", "SAR6", bill.CusSupplyChainActorReferences[1].CFR_Reference);
			AssertEquals("bill.GoodsItems.Count", 2, bill.GoodsItems.Count);

			var expectedTransportTypeAtDeparture = assertEmptyTransportDeparture ? string.Empty : "30";
			var expectedFirstDepartureTransportMeansID = assertEmptyTransportDeparture ? string.Empty : "DEP1";
			var expectedFirstDepartureTransportMeansNationality = assertEmptyTransportDeparture ? string.Empty : "IT";
			var expectedSecondDepartureTransportMeansID = assertEmptyTransportDeparture ? string.Empty : "DEP2";
			var expectedSecondDepartureTransportMeansNationality = assertEmptyTransportDeparture ? string.Empty : "FR";
			var expectedThirdDepartureTransportMeansID = assertEmptyTransportDeparture ? string.Empty : "DEP3";
			var expectedThirdDepartureTransportMeansNationality = assertEmptyTransportDeparture ? string.Empty : "ES";

			AssertEquals("bill.TransportTypeAtDeparture", expectedTransportTypeAtDeparture, bill.TransportTypeAtDeparture);
			AssertEquals("bill.FirstDepartureTransportMeansID", expectedFirstDepartureTransportMeansID, bill.FirstDepartureTransportMeansID);
			AssertEquals("bill.FirstDepartureTransportMeansNationality", expectedFirstDepartureTransportMeansNationality, bill.FirstDepartureTransportMeansNationality);
			AssertEquals("bill.SecondDepartureTransportMeansID", expectedSecondDepartureTransportMeansID, bill.SecondDepartureTransportMeansID);
			AssertEquals("bill.SecondDepartureTransportMeansNationality", expectedSecondDepartureTransportMeansNationality, bill.SecondDepartureTransportMeansNationality);
			AssertEquals("bill.ThirdDepartureTransportMeansID", expectedThirdDepartureTransportMeansID, bill.ThirdDepartureTransportMeansID);
			AssertEquals("bill.ThirdDepartureTransportMeansNationality", expectedThirdDepartureTransportMeansNationality, bill.ThirdDepartureTransportMeansNationality);

			AssertCloneResult_DepartureCargoDesc_Phase5(bill.GoodsItems[0], "DESC5");
			AssertCloneResult_DepartureCargoDesc_Phase5(bill.GoodsItems[1], "DESC6");
		}

		protected void AssertCloneResult_DepartureCargoDesc_Phase5(NctsDepartureCargoDesc cargoDesc, ZString description)
		{
			AssertEquals("cargoDesc.BY_Description", description, cargoDesc.BY_Description);
			AssertEquals("cargoDesc.Packages.Count", 2, cargoDesc.Packages.Count);
			AssertEquals("cargoDesc.Packages[0].B5_MarksAndNumbers", "MARKS1", cargoDesc.Packages[0].B5_MarksAndNumbers);
			AssertEquals("cargoDesc.Packages[1].B5_MarksAndNumbers", "MARKS2", cargoDesc.Packages[1].B5_MarksAndNumbers);
			AssertEquals("cargoDesc.AdditionalInfos.Count", 2, cargoDesc.AdditionalInfos.Count);
			AssertEquals("cargoDesc.AdditionalInfos[0].CSI_Description", "AI7", cargoDesc.AdditionalInfos[0].CSI_Description);
			AssertEquals("cargoDesc.AdditionalInfos[1].CSI_Description", "AI8", cargoDesc.AdditionalInfos[1].CSI_Description);
			AssertEquals("cargoDesc.SupportingDocuments.Count", 2, cargoDesc.SupportingDocuments.Count);
			AssertEquals("cargoDesc.SupportingDocuments[0].CSI_ReferenceNumber", "SD7", cargoDesc.SupportingDocuments[0].CSI_ReferenceNumber);
			AssertEquals("cargoDesc.SupportingDocuments[1].CSI_ReferenceNumber", "SD8", cargoDesc.SupportingDocuments[1].CSI_ReferenceNumber);
			AssertEquals("cargoDesc.PreviousDocuments.Count", 2, cargoDesc.PreviousDocuments.Count);
			AssertEquals("cargoDesc.PreviousDocuments[0].CSI_ReferenceNumber", "PD7", cargoDesc.PreviousDocuments[0].CSI_ReferenceNumber);
			AssertEquals("cargoDesc.PreviousDocuments[1].CSI_ReferenceNumber", "PD8", cargoDesc.PreviousDocuments[1].CSI_ReferenceNumber);
			AssertEquals("cargoDesc.Consignee.E2_OA_Address", ZGuid.Empty, cargoDesc.Consignee.E2_OA_Address);
			AssertEquals("cargoDesc.Consignor.E2_OA_Address", lineCnrPk, cargoDesc.Consignor.E2_OA_Address);
			AssertEquals("cargoDesc.UNDGs.Count", 2, cargoDesc.UNDGs.Count);
			AssertEquals("cargoDesc.UNDGs[0].DI_DG", DangerousSubstance1.PK, cargoDesc.UNDGs[0].DI_DG);
			AssertEquals("cargoDesc.UNDGs[1].DI_DG", DangerousSubstance2.PK, cargoDesc.UNDGs[1].DI_DG);
			AssertEquals("cargoDesc.BY_CustomsThirdQuantity", 3.3m, cargoDesc.BY_CustomsThirdQuantity);
			AssertEquals("cargoDesc.BY_CustomsThirdUnitQty", "K3", cargoDesc.BY_CustomsThirdUnitQty);
			AssertEquals("cargoDesc.AdditionalSupplementaryCodes.Count", 2, cargoDesc.AdditionalSupplementaryCodes.Count);
			AssertEquals("cargoDesc.AdditionalSupplementaryCodes[0].CY_Data", "ASC7", cargoDesc.AdditionalSupplementaryCodes[0].CY_Data);
			AssertEquals("cargoDesc.AdditionalSupplementaryCodes[1].CY_Data", "ASC8", cargoDesc.AdditionalSupplementaryCodes[1].CY_Data);
			AssertEquals("cargoDesc.Fees.Count", 2, cargoDesc.Fees.Count);
			AssertEquals("cargoDesc.Fees[0].BFE_ChargeType", "A01", cargoDesc.Fees[0].BFE_ChargeType);
			AssertEquals("cargoDesc.Fees[1].BFE_ChargeType", "A02", cargoDesc.Fees[1].BFE_ChargeType);
			AssertEquals("cargoDesc.CusSupplyChainActorReferences.Count", 2, cargoDesc.CusSupplyChainActorReferences.Count);
			AssertEquals("cargoDesc.CusSupplyChainActorReferences[0].CFR_Reference", "SAR7", cargoDesc.CusSupplyChainActorReferences[0].CFR_Reference);
			AssertEquals("cargoDesc.CusSupplyChainActorReferences[1].CFR_Reference", "SAR8", cargoDesc.CusSupplyChainActorReferences[1].CFR_Reference);
		}

		protected void AssertCloneResult_Common(NctsHeader clone, ZString expectedType)
		{
			AssertEquals("Testing headerType", expectedType, clone.BH_HeaderType);

			AssertEquals("LocalReferenceNumber", ZString.Empty, clone.LocalReferenceNumber); // Excluded expressly
			AssertEquals("MovementReferenceNumber", ZString.Empty, clone.MovementReferenceNumber);  // Omitted
			AssertEquals("BH_RL_NKImportLoadPort", Core.Constants.CountryCodes.UnitedKingdom, clone.BH_RL_NKImportLoadPort);
			AssertEquals("Consignor.E2_OA_Address", cnrPk, clone.Consignor.E2_OA_Address);
			AssertEquals("Consignee.E2_OA_Address", cnePk, clone.Consignee.E2_OA_Address);
			AssertEquals("Principal.E2_OA_Address", principalPk, clone.Principal.E2_OA_Address);
		}

		protected void AssertCloneResult_Arrival_NonPhase5(NctsHeader clone)
		{
			AssertEquals("BH_MessageStatus should be MAN for NonPhase5 items.", NctsMessageStatusList.Codes.ArrivalNotificationNotSent, clone.BH_MessageStatus);
			AssertNullOrEmpty("BM_MessageStatus should be empty for NonPhase5 items.", clone.ArrivalMovementHeader.BM_MessageStatus);
		}

		protected void AssertCloneResult_Arrival_Phase5(NctsHeader clone)
		{
			AssertNullOrEmpty("BH_MessageStatus should be empty for Phase5 items.", clone.BH_MessageStatus);
			AssertEquals("BM_MessageStatus should be empty for Phase5 items.", clone.Configuration.GetDefaultMessageStatusForArrival(clone), clone.ArrivalMovementHeader.BM_MessageStatus);
		}

		protected void AssertCloneResult_Departure_Common_Phase4(NctsHeader clone, ZString inBondEntryType, string inlandTransportMode = "26")
		{
			AssertEquals("IsDepartureMovement should be true", true, clone.IsDepartureMovement);
			var movementHeader = clone.MovementHeader;
			AssertEquals("Representative.E2_OA_Address", representativePk, movementHeader.Representative.E2_OA_Address);
			AssertEquals("movement.IsSimplifiedNctsProcedure", true, movementHeader.IsSimplifiedNctsProcedure);
			AssertCloneResult_Movement_Common(movementHeader, inBondEntryType, inlandTransportMode);
			AssertCloneResult_Movement_Departure_Common(movementHeader);

			AssertCloneResult_DepartureGoodsItem_Common_Phase4(movementHeader, 1, Core.Constants.Weight.Kilograms, "T1");
			AssertCloneResult_DepartureGoodsItem_Common_Phase4(movementHeader, 2, Core.Constants.Weight.Pounds, "T2");
			AssertCloneResult_GoodsItem_Departure_Common_Phase4(movementHeader);
		}

		protected void AssertCloneResult_Departure_Phase5(NctsHeader clone, string inlandTransportMode = "26")
		{
			var movementHeader = clone.MovementHeader;
			AssertEquals("IsPhase5 should be true", true, movementHeader.IsPhase5);
			AssertCloneResult_Movement_Common(movementHeader, NctsConstants.NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase5, inlandTransportMode: inlandTransportMode);
			AssertCloneResult_Movement_Departure_Common(movementHeader);
			AssertCloneResult_Movement_Departure_Phase5(movementHeader);
			AssertCloneResult_GoodsItem_Departure_Phase5(clone);
		}

		void AssertCloneResult_Movement_Common(NctsCommonMovementHeader movement, ZString inBondEntryType, string inlandTransportMode = "26")
		{
			AssertEquals("movement.BM_CustomsStatus", ZString.Empty, movement.BM_CustomsStatus);
			AssertEquals("movement.BM_InBondEntryType", inBondEntryType, movement.BM_InBondEntryType);
			AssertEquals("Broker movement.BM_GS_NKCusAgent", ExpectedDefaultBM_GS_NKCusAgent, movement.BM_GS_NKCusAgent);
			AssertEquals("movement.BM_RL_NKDestinationPort", "IT", movement.BM_RL_NKDestinationPort);
			AssertEquals("movement.BM_InlandTransportMode", inlandTransportMode, movement.BM_InlandTransportMode);
			AssertEquals("movement.BM_TransportAtDeparture", "18.ID", movement.BM_TransportAtDeparture);
			AssertEquals("movement.BM_RN_NKTransportAtDepartureCountry", "18", movement.BM_RN_NKTransportAtDepartureCountry);
			AssertEquals("movement.BM_ExportTransportMode", "25", movement.BM_ExportTransportMode);
			AssertEquals("movement.BM_TOLCarrierID", "21.ID", movement.BM_TOLCarrierID);
			AssertEquals("movement.BM_TOLCarrierCode", "21", movement.BM_TOLCarrierCode);
			AssertEquals("movement.BM_RL_NKForeignDestPort", "27.N", movement.BM_RL_NKForeignDestPort);
			AssertEquals("movement.BM_LocationOfGoodsCode", "30.1", movement.BM_LocationOfGoodsCode);
			AssertEquals("movement.BM_LocationOfGoods", "30.2", movement.BM_LocationOfGoods);
			AssertEquals("movement.BM_CustomsSubPlace", "30.3", movement.BM_CustomsSubPlace);
			AssertEquals("movement.BM_ExportDate", ZDateTime.Empty, movement.BM_ExportDate);
		}
		protected virtual ZString ExpectedDefaultBM_GS_NKCusAgent => ZString.Empty;

		void AssertCloneResult_Movement_Departure_Common(NctsCommonMovementHeader movement)
		{
			AssertEquals("Representative.E2_OA_Address", representativePk, movement.Representative.E2_OA_Address);
			AssertEquals("movement.IsSimplifiedNctsProcedure", true, movement.IsSimplifiedNctsProcedure);
		}

		void AssertCloneResult_Movement_Departure_Phase5(NctsDepartureMovementHeader movement)
		{
			var goodsLocation = movement.GoodsLocation;
			AssertEquals("GoodsLocation CGL_Qualifier cloned", "T", goodsLocation.CGL_Qualifier);
			AssertEquals("GoodsLocation CGL_Type cloned", "B", goodsLocation.CGL_Type);
			AssertEquals("GoodsLocation CGL_AdditionalIdentifier cloned", new ZString("AH3Location").Left(goodsLocation.CGL_AdditionalIdentifierInfo.MaxLength), goodsLocation.CGL_AdditionalIdentifier);
			AssertEquals("movement.BM_EntryDate", ZDateTime.Empty, movement.BM_EntryDate);
		}

		void AssertCloneResult_DepartureGoodsItem_Common_Phase4(NctsCommonMovementHeader movementHeader, int lineNum, ZString weightUnit, ZString declarationType)
		{
			var line = movementHeader.GoodsItems.FirstOrDefault(l => l.BY_LineNo == lineNum);
			AssertEquals("BY_LineNo", lineNum, line.BY_LineNo);
			AssertEquals("BY_HarmonisedTariff", "1234.56 78 90", line.BY_HarmonisedTariff);
			AssertEquals("BY_Type", declarationType, line.BY_Type);
			AssertEquals("BY_Description", "Stuff", line.BY_Description);
			AssertEquals("BY_GrossWeight", 35m, line.BY_GrossWeight);
			AssertEquals("BY_GrossWeightUnit", weightUnit, line.BY_GrossWeightUnit);
			AssertEquals("BY_NetWeight", 38m, line.BY_NetWeight);
			AssertEquals("BY_NetWeightUnit", weightUnit, line.BY_NetWeightUnit);
			AssertEquals("BY_RN_NKCountryOfDispatch", "11", line.BY_RN_NKCountryOfDispatch);
			AssertEquals("BY_RN_NKCountryOfDestination", "22", line.BY_RN_NKCountryOfDestination);

			AssertEquals("CommercialReferenceNumber", "CommRef", line.BY_CommercialReferenceNumber);
			AssertEquals("TransportChargesMoP", "P", line.BY_TransportChargesMethodOfPayment);

			AssertEquals("Packages[0]", "1P1Marks1", line.Packages[0].B5_UnitCount.ToString() + line.Packages[0].B5_UnitType + line.Packages[0].B5_MarksAndNumbers);
			AssertEquals("Packages[1]", "2P2Marks2", line.Packages[1].B5_UnitCount.ToString() + line.Packages[1].B5_UnitType + line.Packages[1].B5_MarksAndNumbers);

			AssertEquals("SupportingDocuments[0]", "SD1SD2", line.SupportingDocuments[0].CSI_ReferenceNumber + line.SupportingDocuments[1].CSI_ReferenceNumber);
			AssertEquals("AdditionalInfos[0]", "AI1AI2", line.AdditionalInfos[0].CSI_Description + line.AdditionalInfos[1].CSI_Description);
		}

		void AssertCloneResult_GoodsItem_Departure_Common_Phase4(NctsCommonMovementHeader movementHeader)
		{
			AssertEquals("IsDepartureMovementHeader should return true", true, movementHeader.IsDepartureMovementHeader);
			foreach (var line in movementHeader.GoodsItems.Cast<NctsDepartureCargoDesc>().ToArray())
			{
				AssertEquals("Consignor.E2_OA_Address", lineCnrPk, line.Consignor.E2_OA_Address);
				AssertEquals("Consignee.E2_OA_Address", lineCnePk, line.Consignee.E2_OA_Address);
				AssertEquals("UNDGs[0].UNDGSubstance.DG_Code", "0004a", line.UNDGs[0].UNDGSubstance.DG_Code);
				AssertEquals("SecurityConsignor.E2_OA_Address", lineSecCnrPk, line.SecurityConsignor.E2_OA_Address);
				AssertEquals("SecurityConsignee.E2_OA_Address", lineSecCnePk, line.SecurityConsignee.E2_OA_Address);
				AssertEquals("PreviousDocuments[0]", "PD1PD2", line.PreviousDocuments[0].CSI_ReferenceNumber + line.PreviousDocuments[1].CSI_ReferenceNumber);
				AssertEquals("BY_CustomsThirdQuantity", 123.45m, line.BY_CustomsThirdQuantity);
				AssertEquals("BY_CustomsThirdUnitQty", "KG", line.BY_CustomsThirdUnitQty);
				AssertEquals("BY_Supplements", "SUP1,SUP2", line.BY_Supplements);

				AssertEquals("Fees.Count", 1, line.Fees.Count);
				AssertEquals("Fees[0].BFE_ChargeType", "A00", line.Fees[0].BFE_ChargeType);
				AssertEquals("Fees[0].BFE_RateOverrideReasonCode", "ADD", line.Fees[0].BFE_RateOverrideReasonCode);
				AssertEquals("Fees[0].BFE_BaseValue", 2m, line.Fees[0].BFE_BaseValue);
				AssertEquals("Fees[0].BFE_MethodOfCalculation", "X", line.Fees[0].BFE_MethodOfCalculation);
				AssertEquals("Fees[0].BFE_Rate", 10m, line.Fees[0].BFE_Rate);
				AssertEquals("Fees[0].BFE_ChargeAmount", 2000m, line.Fees[0].BFE_ChargeAmount);
				AssertEquals("Fees[0].BFE_MethodOfPayment", "F", line.Fees[0].BFE_MethodOfPayment);
			}
		}

		void AssertCloneResult_GoodsItem_Departure_Phase5(NctsHeader header)
		{
			AssertEquals("Header.IsPhase5 should return true", true, header.IsPhase5);
			foreach (var line in header.Bills.SelectMany(x => x.GoodsItems))
			{
				AssertEquals("BY_DeclarationGoodsItemNumber", ZInt.Zero, line.BY_DeclarationGoodsItemNumber);
			}
		}

		protected void AddLine(NctsHeader header, ZString weightUnit, ZString declarationType)
		{
			NctsCommonCargoDesc line;
			var useNctsDepartureCargoDesc = header.IsDepartureMovement && !header.IsArrivalMovement;
			if (useNctsDepartureCargoDesc)
			{
				line = header.MovementHeader.GoodsItems.AddNew();
			}
			else
			{
				line = header.ArrivalMovementHeader.GoodsItems.AddNew();
			}

			// Main
			line.BY_HarmonisedTariff = "1234.56 78 90";
			line.BY_Type = declarationType;
			line.BY_Description = "Stuff";
			line.BY_GrossWeight = 35m;
			line.BY_GrossWeightUnit = weightUnit;
			line.BY_NetWeight = 38m;
			line.BY_NetWeightUnit = weightUnit;
			line.BY_RN_NKCountryOfDispatch = "11";
			line.BY_RN_NKCountryOfDestination = "22";
			line.BY_DeclarationGoodsItemNumber = 2;

			//Security
			line.BY_CommercialReferenceNumber = "CommRef";
			line.BY_TransportChargesMethodOfPayment = "P";

			//Packages
			var p1 = line.Packages.AddNew();
			p1.B5_UnitType = "P1";
			p1.B5_UnitCount = 1;
			p1.B5_MarksAndNumbers = "Marks1";
			var p2 = line.Packages.AddNew();
			p2.B5_UnitType = "P2";
			p2.B5_UnitCount = 2;
			p2.B5_MarksAndNumbers = "Marks2";

			// Supp docs
			line.SupportingDocuments.AddNew().CSI_ReferenceNumber = "SD1";
			line.SupportingDocuments.AddNew().CSI_ReferenceNumber = "SD2";

			// Add Info
			line.AdditionalInfos.AddNew().CSI_Description = "AI1";
			line.AdditionalInfos.AddNew().CSI_Description = "AI2";

			if (useNctsDepartureCargoDesc)
			{
				var departureLine = (NctsDepartureCargoDesc)line;
				departureLine.Consignor.E2_OA_Address = lineCnrPk;
				departureLine.Consignee.E2_OA_Address = lineCnePk;
				departureLine.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
				departureLine.SecurityConsignor.E2_OA_Address = lineSecCnrPk;
				departureLine.SecurityConsignee.E2_OA_Address = lineSecCnePk;

				//Select Containers
				departureLine.ContainersPivots[1].ContainerSelected = true;
				//Prev docs
				departureLine.PreviousDocuments.AddNew().CSI_ReferenceNumber = "PD1";
				departureLine.PreviousDocuments.AddNew().CSI_ReferenceNumber = "PD2";

				departureLine.BY_CustomsThirdQuantity = 123.45m;
				departureLine.BY_CustomsThirdUnitQty = "KG";
				departureLine.AdditionalSupplementaryCodes.AddNew().CY_Code = "SUP1";
				departureLine.AdditionalSupplementaryCodes.AddNew().CY_Code = "SUP2";

				var fee = departureLine.Fees.AddNew();
				fee.BFE_ChargeType = "A00";
				fee.BFE_RateOverrideReasonCode = "ADD";
				fee.BFE_BaseValue = 2m;
				fee.BFE_MethodOfCalculation = "X";
				fee.BFE_Rate = 10m;
				fee.BFE_ChargeAmount = 2000m;
				fee.BFE_MethodOfPayment = "F";
			}
		}
	}
}
