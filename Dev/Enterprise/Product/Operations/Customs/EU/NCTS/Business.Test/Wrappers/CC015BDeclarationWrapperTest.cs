using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Messaging;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(CC015BDeclarationWrapper))]
	class CC015BDeclarationWrapperTest : DeclarationWrapperAbstractTest<CC015BDeclarationWrapper>
	{
		public void TestTypeOfDeclaration()
		{
			header.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
			AssertEquals(NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration, wrapper.TypeOfDeclaration);
		}

		public void TestIsTIRDeclaration()
		{
			header.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
			AssertEquals(false, wrapper.IsTIRDeclaration);

			header.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
			AssertEquals(true, wrapper.IsTIRDeclaration);
		}

		public void TestCountryOfDestinationCode()
		{
			header.MovementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Turkmenistan;
			AssertEquals(Core.Constants.CountryCodes.Turkmenistan, wrapper.CountryOfDestinationCode);
		}

		public void TestAgreedLocationOfGoodsCode()
		{
			header.MovementHeader.BM_LocationOfGoodsCode = Core.Constants.CountryCodes.France;
			AssertEquals(Core.Constants.CountryCodes.France, wrapper.AgreedLocationOfGoodsCode);
		}

		public void TestAgreedLocationOfGoods()
		{
			header.MovementHeader.BM_LocationOfGoods = "Paris";
			AssertEquals("Paris", wrapper.AgreedLocationOfGoods);
		}

		public void TestAgreedLocationOfGoodsLanguage()
		{
			AssertEquals(ZString.Empty, wrapper.AgreedLocationOfGoodsLanguage);
		}

		public void TestAuthorisedLocationOfGoodsCode()
		{
			header.MovementHeader.BM_LocationOfGoodsCode = Core.Constants.CountryCodes.Estonia;
			AssertEquals(Core.Constants.CountryCodes.Estonia, wrapper.AuthorisedLocationOfGoodsCode);
		}

		public void TestPlaceOfLoadingCode()
		{
			header.MovementHeader.BM_RL_NKForeignDestPort = "GBDVR";
			AssertEquals("GBDVR", wrapper.PlaceOfLoadingCode);
		}

		public void TestCountryOfDispatchExportCode()
		{
			header.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.UnitedKingdom;
			AssertEquals(Core.Constants.CountryCodes.UnitedKingdom, wrapper.CountryOfDispatchExportCode);

			header.BH_RL_NKImportLoadPort = "";
			AssertEquals("", wrapper.CountryOfDispatchExportCode);

			var header2 = Factory.New<NctsHeader>();
			header2.SetMovementType(NctsMovementType.Codes.Departure);
			var nctsConfigurationMock = new Mock<NctsConfiguration>() { CallBase = true };
			nctsConfigurationMock.Protected().Setup<ZBool>("FullLoadPortSupportCore", ItExpr.IsAny<bool>()).Returns(new ZBool(true)).Verifiable();
			header2.Factory.ClearCachedValue<NctsConfiguration>($"NctsConfiguration_{GlbCompany.CurrentCompany.GC_RN_NKCountryCode}");

			var objectHandleMock = new Mock<ObjectHandle>();
			objectHandleMock.Setup(x => x.GetObject()).Returns(nctsConfigurationMock.Object);
			var nctsConfiguration = new KeyObjectHandleDictionaryObject { { GlbCompany.CurrentCompany.GC_RN_NKCountryCode, objectHandleMock.Object } };

			using (ObjectFactory.Substitute("NCTS.NctsConfiguration", nctsConfiguration))
			{
				header2.BH_RL_NKImportLoadPort = "GBPOR";
				var wrapper2 = new CC015BDeclarationWrapper(header2);
				AssertEquals(Core.Constants.CountryCodes.UnitedKingdom, wrapper2.CountryOfDispatchExportCode);
			}
		}

		public void TestCustomsSubPlace()
		{
			header.MovementHeader.BM_CustomsSubPlace = "DOVER ERTS";
			AssertEquals("DOVER ERTS", wrapper.CustomsSubPlace);
		}

		public void TestInlandTransportMode()
		{
			header.MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._9_OwnPropulsion;
			AssertEquals(ModeOfTransportList.Codes._9_OwnPropulsion, wrapper.InlandTransportMode);
		}

		public void TestTransportModeAtBorder()
		{
			header.MovementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._4_AirTransport;
			AssertEquals(ModeOfTransportList.Codes._4_AirTransport, wrapper.TransportModeAtBorder);
		}

		public void TestIdentityOfMeansOfTransportAtDeparture()
		{
			header.MovementHeader.BM_TransportAtDeparture = "QF34";
			AssertEquals("QF34", wrapper.IdentityOfMeansOfTransportAtDeparture);
		}

		public void TestIdentityOfMeansOfTransportAtDepartureLanguage()
		{
			AssertEquals(ZString.Empty, wrapper.IdentityOfMeansOfTransportAtDepartureLanguage);
		}

		public void TestNationalityOfMeansOfTransportAtDeparture()
		{
			header.MovementHeader.BM_RN_NKTransportAtDepartureCountry = Core.Constants.CountryCodes.CaymanIslands;
			AssertEquals(Core.Constants.CountryCodes.CaymanIslands, wrapper.NationalityOfMeansOfTransportAtDeparture);
		}

		public void TestIdentityOfMeansOfTransportCrossingBorder()
		{
			header.MovementHeader.BM_TOLCarrierID = "WHY I3I";
			AssertEquals("WHY I3I", wrapper.IdentityOfMeansOfTransportCrossingBorder);
		}

		public void TestIdentityOfMeansOfTransportCrossingBorderLanguage()
		{
			AssertEquals(ZString.Empty, wrapper.IdentityOfMeansOfTransportCrossingBorderLanguage);
		}

		public void TestNationalityOfMeansOfTransportCrossingBorder()
		{
			header.MovementHeader.BM_TOLCarrierCode = Core.Constants.CountryCodes.Germany;
			AssertEquals(Core.Constants.CountryCodes.Germany, wrapper.NationalityOfMeansOfTransportCrossingBorder);
		}

		public void TestTypeOfMeansOfTransportCrossingBorder()
		{
			header.MovementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
			AssertEquals(ModeOfTransportList.Codes._3_RoadTransport, wrapper.TypeOfMeansOfTransportCrossingBorder);
		}

		public void TestIsContainerised()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Containers", false, wrapper.IsContainerised);
				var container = Factory.New<NctsDepartureHeaderContainer>();
				container.BC_Mode = "CNT";
				var item = header.Bills.AddNew().GoodsItems.AddNew();
				AssertEquals("Not linked to a goods item", false, wrapper.IsContainerised);
				var containerPivot = item.ContainersPivots.AddNew();
				containerPivot.Container = container;
				containerPivot.ContainerSelected = true;
				containerPivot.ContainerNumber = "CONT123456";
				header.DepartureHeaderContainers.Add(container);
				AssertEquals("Linked and has Container Number", true, wrapper.IsContainerised);
			});
		}

		public void TestDialogLanguageIndicatorAtDeparture()
		{
			AssertEquals(ZString.Empty, wrapper.DialogLanguageIndicatorAtDeparture);
		}

		public void TestAccompanyingDocumentLanguage_ConsigneeLng()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Language = "TR-TR";
			header.Consignee.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("TR", wrapper.AccompanyingDocumentLanguage);
		}

		public void TestAccompanyingDocumentLanguage_CurrentCompany()
		{
			AssertEquals("EN", wrapper.AccompanyingDocumentLanguage);
		}

		public void TestTotalNumberOfItems()
		{
			var bill = header.Bills.AddNew();
			bill.GoodsItems.AddNew();
			bill.GoodsItems.AddNew();
			AssertEquals(2, wrapper.TotalNumberOfItems);
		}

		public void TestTotalNumberOfPackages()
		{
			var item1 = header.Bills.AddNew().GoodsItems.AddNew();
			item1.Packages.AddNew();
			item1.Packages.AddNew();
			AssertEquals(2, wrapper.TotalNumberOfPackages);
		}

		public void TestTotalGrossMass_Phase4()
		{
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var goodsItem1 = header.MovementHeader.GoodsItems.AddNew();
			goodsItem1.BY_GrossWeight = 30;
			goodsItem1.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			var goodsItem2 = header.MovementHeader.GoodsItems.AddNew();
			goodsItem2.BY_GrossWeight = 25;
			goodsItem2.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			AssertEquals(55m, wrapper.TotalGrossMass);
		}

		public void TestTotalGrossMass_Phase5()
		{
			header.MovementHeader.BM_GrossWeight = 55;
			AssertEquals(55m, wrapper.TotalGrossMass);
		}

		public void TestTotalInvoiceValue()
		{
			var goodsItem1 = header.Bills.AddNew().GoodsItems.AddNew();
			goodsItem1.BY_GrossWeight = 30;
			goodsItem1.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			goodsItem1.BY_MonetaryValue = 10;
			var goodsItem2 = header.Bills.AddNew().GoodsItems.AddNew();
			goodsItem2.BY_GrossWeight = 25;
			goodsItem2.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			goodsItem2.BY_MonetaryValue = 20;
			AssertEquals(30m, wrapper.TotalInvoiceValue);
		}

		public void TestDeclarationDate()
		{
			header.MovementHeader.BM_EntryDate = ZDateTime.Empty;
			AssertEquals(ZDateTime.Today.GetLongDate(), wrapper.DeclarationDate);

			header.MovementHeader.BM_EntryDate = new ZDateTime(2021, 1, 14);
			AssertEquals("20210114", wrapper.DeclarationDate);
		}

		public void TestDeclarationPlace()
		{
			AssertEquals("Brisbane", wrapper.DeclarationPlace);
		}

		public void TestDeclarationPlaceLanguage()
		{
			AssertEquals(ZString.Empty, wrapper.DeclarationPlaceLanguage);
		}

		public void TestSpecificCircumstanceIndicator()
		{
			header.MovementHeader.BM_BTAIndicator = SpecificCircumstanceIndicator.Codes.RoadModeOfTransport;
			AssertEquals(SpecificCircumstanceIndicator.Codes.RoadModeOfTransport, wrapper.SpecificCircumstanceIndicator);
		}

		public void TestTransportChargesMethodOfPayment()
		{
			header.MovementHeader.BM_MethodOfPayment = Customs.Business.TransportChargesModeOfPayment.Codes.ElectronicCreditTransfer;
			AssertEquals(Customs.Business.TransportChargesModeOfPayment.Codes.ElectronicCreditTransfer, wrapper.TransportChargesMethodOfPayment);
		}

		public void TestCommercialReferenceNumber()
		{
			header.MovementHeader.BM_AdditionalText = "ADDITIONAL TEXT";
			AssertEquals("ADDITIONAL TEXT", wrapper.CommercialReferenceNumber);
		}

		public void TestSecurityIndicator()
		{
			CombineAssertions(() =>
			{
				header.MovementHeader.BM_TypeOfSecurity = "BTH";
				var organisation = Factory.New<OrgHeader>();
				header.SecurityConsignor.OrganisationPK = organisation.PK;
				AssertEquals("Security Org Entered", true, wrapper.SecurityIndicator);
				header.BH_FTZMove = ZBool.False;
				AssertEquals("FTZ Move false", false, wrapper.SecurityIndicator);
			});
		}

		public void TestSafetyAndSecurityData()
		{
			header.MovementHeader.BM_TypeOfSecurity = "BTH";
			AssertEquals(true, wrapper.SafetyAndSecurityData);
			header.MovementHeader.BM_TypeOfSecurity = "ZZZ";
			AssertEquals(false, wrapper.SafetyAndSecurityData);
		}

		[TestDate(2020, 12, 18)]
		public void TestProvisionalTransitDepartureDate()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.CustomsOffices.RemoveAndDeleteAll();
			var office = movementHeader.CustomsOffices.AddNew();
			office.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;
			var wrapper = new CC015BDeclarationWrapper(nctsHeader);
			AssertEquals("ProvisionalTransitDepartureDate should be empty as there is no NCTSOfficeOfDeparture", ZString.Empty, wrapper.ProvisionalTransitDepartureDate);

			office = movementHeader.CustomsOffices.AddNew();
			office.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
			office.CY_Date = ZDateTime.Today.AddDays(1);
			wrapper = new CC015BDeclarationWrapper(nctsHeader);
			AssertEquals("ProvisionalTransitDepartureDate should be equal to date of NCTSOfficeOfDeparture", "20201219", wrapper.ProvisionalTransitDepartureDate);
		}

		public void TestModeOfRepresentation()
		{
			var nctsHeader = Factory.NewMoq<NctsHeader>();
			nctsHeader.Object.SetMovementType(NctsMovementType.Codes.Departure);
			var modeOfRepresentationCalculator = new Mock<ModeOfRepresentationCalculator>(nctsHeader.Object);
			var getModeOfRepresentationCoreSetup = modeOfRepresentationCalculator.Protected().Setup<ZString>("GetModeOfRepresentationCore");
			nctsHeader.Protected().Setup<ModeOfRepresentationCalculator>("GetModeOfRepresentationCalculator").Returns(modeOfRepresentationCalculator.Object);

			wrapper = new CC015BDeclarationWrapper(nctsHeader.Object);

			getModeOfRepresentationCoreSetup.Returns(NctsConstants.ModeOfRepresentation.Codes.Blank);
			AssertEquals(NctsConstants.ModeOfRepresentation.Codes.Blank, wrapper.ModeOfRepresentation);

			getModeOfRepresentationCoreSetup.Returns(NctsConstants.ModeOfRepresentation.Codes.One);
			AssertEquals(NctsConstants.ModeOfRepresentation.Codes.One, wrapper.ModeOfRepresentation);

			getModeOfRepresentationCoreSetup.Returns(NctsConstants.ModeOfRepresentation.Codes.Two);
			AssertEquals(NctsConstants.ModeOfRepresentation.Codes.Two, wrapper.ModeOfRepresentation);
		}

		public void TestConveyanceReferenceNumber()
		{
			header.MovementHeader.BM_ConveyanceNumber = "AAA1234Z";
			AssertEquals("AAA1234Z", wrapper.ConveyanceReferenceNumber);
		}

		public void TestTransportReferenceNumber()
		{
			header.BH_VoyageNumber = "1A";
			AssertEquals("1A", wrapper.TransportReferenceNumber);
		}

		public void TestPlaceOfUnloadingCode()
		{
			header.MovementHeader.BM_PlaceOfUnloading = "Paris";
			AssertEquals("Paris", wrapper.PlaceOfUnloadingCode);
		}

		public void TestPlaceOfUnloadingCodeLanguage()
		{
			AssertEquals(ZString.Empty, wrapper.PlaceOfUnloadingCodeLanguage);
		}

		public void TestConsignor()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "CONSIGNOR NAME";
			header.Consignor.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("CONSIGNOR NAME", wrapper.Consignor.Name);
		}

		public void TestConsignee()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "CONSIGNEE NAME";
			var vatFR = org.CustomsCodes.AddNew();
			vatFR.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			vatFR.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			vatFR.OK_CustomsRegNo = "32582075100080";
			header.Consignee.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("CONSIGNEE NAME", wrapper.Consignee.Name);
			AssertEquals("FR32582075100080", wrapper.Consignee.TIN);
		}

		public void TestDeclarant()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "DECLARANT NAME";
			header.Declarant.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("DECLARANT NAME", wrapper.Declarant.Name);
		}

		public void TestDeclarantTIN_IsDepartureFallback()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_CustomsRegNo = "2222222222";
			header.Consignor.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("FR2222222222", wrapper.DeclarantTIN);
		}

		public void TestPrincipalTIN()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_CustomsRegNo = "3333333333";
			header.Principal.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("FR3333333333", wrapper.PrincipalTIN);
		}

		public void TestAuthorisedConsigneeTIN()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var vatFR = org.CustomsCodes.AddNew();
			vatFR.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			vatFR.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			vatFR.OK_CustomsRegNo = "32582075100080";
			header.Consignee.E2_OA_Address = org.MainAddress.PK;
			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorizationHeader.CPH_OH_PermitHolder = org.PK;
			authorizationHeader.CPH_OA_AppliesTo = org.MainAddress.PK;
			authorizationHeader.CPH_Number = "FR32582075100080";
			authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
			AssertEquals("FR32582075100080", wrapper.AuthorisedConsigneeTIN);
		}

		public void TestTransitCustomsOffices()
		{
			var movementHeader = header.MovementHeader;
			var office1 = movementHeader.CustomsOffices.AddNew();
			var office2 = movementHeader.CustomsOffices.AddNew();
			office1.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;
			office2.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;
			AssertEquals(2, wrapper.TransitCustomsOffices.Count);
		}

		public void TestControlResultCode()
		{
			header.MovementHeader.BM_GONumber = "TT";
			AssertEquals("TT", wrapper.ControlResultCode);
		}

		public void TestControlResultDateLimit()
		{
			header.MovementHeader.BM_ExportDate = new ZDateTime(2020, 12, 17);
			AssertEquals("20201217", wrapper.ControlResultDateLimit);
		}

		public void TestRepresentative()
		{
			header.MovementHeader.BM_GS_NKCusAgent = "EBN";
			AssertEquals("EBN", wrapper.Representative);
		}

		public void TestNumberOfSeals()
		{
			var container = Factory.New<NctsDepartureHeaderContainer>();
			container.BC_Seal1 = "123456";
			container.BC_Seal2 = "987654";
			header.MovementHeader.BM_SealType = SealTypeList.Codes.ContainerSeal;
			var item = header.Bills.AddNew().GoodsItems.AddNew();
			var containerPivot = item.ContainersPivots.AddNew();
			containerPivot.Container = container;
			containerPivot.ContainerSelected = true;
			AssertEquals(2, wrapper.NumberOfSeals);

			header.MovementHeader.BM_SealType = SealTypeList.Codes.PackageSeal;
			var seal1 = header.Seals.AddNew();
			seal1.CY_Data = "111111";
			var seal2 = header.Seals.AddNew();
			seal2.CY_Data = "222222";
			var wrapper2 = new CC015BDeclarationWrapper(header);
			AssertEquals(2, wrapper2.NumberOfSeals);
		}

		public void TestSeals()
		{
			TestPackageSeals();
			TestContainerSeals();
		}

		void TestPackageSeals()
		{
			header.MovementHeader.BM_SealType = SealTypeList.Codes.PackageSeal;
			var seal1 = header.Seals.AddNew();
			seal1.CY_Data = "111111";
			var seal2 = header.Seals.AddNew();
			seal2.CY_Data = "222222";
			AssertContainsExactElementsInAnyOrder("Package seals values", new ZString[] { "111111", "222222" }, wrapper.Seals.Select(x => x.SealIdentity));
		}

		void TestContainerSeals()
		{
			var container = Factory.New<NctsDepartureHeaderContainer>();
			container.BC_Seal1 = "123456";
			container.BC_Seal2 = "987654";
			header.MovementHeader.BM_SealType = SealTypeList.Codes.ContainerSeal;
			var item = header.Bills.AddNew().GoodsItems.AddNew();
			var containerPivot = item.ContainersPivots.AddNew();
			containerPivot.Container = container;
			containerPivot.ContainerSelected = true;
			var wrapper2 = new CC015BDeclarationWrapper(header);
			AssertContainsExactElementsInAnyOrder("Container seals values", new ZString[] { "123456", "987654" }, wrapper2.Seals.Select(x => x.SealIdentity));
		}

		public void TestGuarantees()
		{
			header.MovementHeader.Guarantees.AddNew();
			header.MovementHeader.Guarantees.AddNew();
			AssertEquals(2, wrapper.Guarantees.Count);
		}

		public void TestGoodsItems()
		{
			var goodsItems = header.Bills.AddNew().GoodsItems;
			goodsItems.AddNew();
			goodsItems.AddNew();
			AssertEquals(2, wrapper.GoodsItems.Count);
		}

		public void TestItinerary()
		{
			header.Itinerary.AddNew();
			header.Itinerary.AddNew();
			AssertEquals(2, wrapper.Itinerary.Count);
		}

		public void TestSecurityConsignor()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "SECURITY CONSIGNOR NAME";
			header.SecurityConsignor.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("SECURITY CONSIGNOR NAME", wrapper.SecurityConsignor.Name);
		}

		public void TestSecurityConsignee()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "SECURITY CONSIGNEE NAME";
			header.SecurityConsignee.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("SECURITY CONSIGNEE NAME", wrapper.SecurityConsignee.Name);
		}

		public void TestSecurityConsignorCountryGroup()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			header.SecurityConsignor.E2_OA_Address = org.MainAddress.PK;
			org.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			AssertEquals(SecurityTraderCountryGroup.EuForSafetyAndSecurity, wrapper.SecurityConsignorCountryGroup);
		}

		public void TestSecurityConsigneeCountryGroup()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			header.SecurityConsignee.E2_OA_Address = org.MainAddress.PK;
			org.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			AssertEquals(SecurityTraderCountryGroup.EuForSafetyAndSecurity, wrapper.SecurityConsigneeCountryGroup);
		}

		public void TestIsProduction()
		{
			AssertEquals(false, wrapper.IsProduction);
		}

		public void TestUnloadedGoodsItemsOrder()
		{
			var bill = header.Bills.AddNew();
			var item1 = bill.GoodsItems.AddNew();
			item1.BY_LineNo = 10;
			var item2 = bill.GoodsItems.AddNew();
			item2.BY_LineNo = 2;
			var item3 = bill.GoodsItems.AddNew();
			item3.BY_LineNo = 1;

			AssertEquals(3, wrapper.GoodsItems.Count);

			var listOfGoodItem = wrapper.GoodsItems;
			AssertEquals(1, listOfGoodItem.ElementAt(0).ItemNumber);
			AssertEquals(2, listOfGoodItem.ElementAt(1).ItemNumber);
			AssertEquals(10, listOfGoodItem.ElementAt(2).ItemNumber);
		}

		public void TestPlaceOfLoading()
		{
			header.MovementHeader.BM_RL_NKForeignDestPort = "FRCDG";
			AssertEquals("FRCDG", wrapper.PlaceOfLoadingCode);
			AssertEquals(17, wrapper.PlaceOfLoading.Length);
			AssertEquals("Charles-de-Gaulle", wrapper.PlaceOfLoading);
		}

		public void TestPlaceOfUnloading()
		{
			header.MovementHeader.BM_PlaceOfUnloading = "FRCDG";
			AssertEquals("FRCDG", wrapper.PlaceOfUnloadingCode);
			AssertEquals(17, wrapper.PlaceOfUnloading.Length);
			AssertEquals("Charles-de-Gaulle", wrapper.PlaceOfUnloading);
		}

		public void TestSecurityCarrierEORI()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_CustomsRegNo = "12345645";
			header.BH_OH_Carrier = org.PK;
			AssertEquals("FR12345645", wrapper.SecurityCarrierEORI);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			wrapper = new CC015BDeclarationWrapper(header);
		}
		NctsHeader header;
		CC015BDeclarationWrapper wrapper;

		protected override CC015BDeclarationWrapper GetProvider()
		{
			header.Consignor.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			header.Declarant.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			header.SecurityConsignor.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			header.SecurityConsignee.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;

			return wrapper;
		}
	}
}
