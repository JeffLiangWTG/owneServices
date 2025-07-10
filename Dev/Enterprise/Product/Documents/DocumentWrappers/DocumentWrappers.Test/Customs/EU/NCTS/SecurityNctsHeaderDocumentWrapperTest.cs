using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.EU.NCTS.Testing
{
	[TestedType(typeof(SecurityNctsHeaderDocumentWrapper))]
	sealed class SecurityNctsHeaderDocumentWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestWrapperProperties()
		{
			var (header, wrapper) = SetUpData();

			CombineAssertions("Test properties when source is set up with data", () =>
			{
				AssertEquals(nameof(wrapper.BOXS32OTHERSCI), "A", wrapper.BOXS32OTHERSCI);
				AssertEquals(nameof(wrapper.BOXS12FIRSTARRIVALTIME), "---", wrapper.BOXS12FIRSTARRIVALTIME);
				AssertEquals(nameof(wrapper.BOXS29TRANSPORTCHARGESMOP), "B", wrapper.BOXS29TRANSPORTCHARGESMOP);
				AssertEquals(nameof(wrapper.BOX7REFERENCENUMBERS), "Local Ref Number", wrapper.BOX7REFERENCENUMBERS);
				AssertEquals(nameof(wrapper.BOX21BORDERTRANSPORTID), "EP-666-EW", wrapper.BOX21BORDERTRANSPORTID);
				AssertEquals(nameof(wrapper.BOX21BORDERTRANSPORTFLAG), "TR", wrapper.BOX21BORDERTRANSPORTFLAG);
				AssertEquals(nameof(wrapper.BOX25BORDERTRANSPORTMODE), "8", wrapper.BOX25BORDERTRANSPORTMODE);
				AssertEquals(nameof(wrapper.BOXS18PLACEOFUNLOADING), "IT UNLDG", wrapper.BOXS18PLACEOFUNLOADING);
				AssertEquals(nameof(wrapper.BOXS17PLACEOFLOADING), "PL LOADG", wrapper.BOXS17PLACEOFLOADING);
				AssertEquals(nameof(wrapper.BOXS10CONVEYANCE), "C000001", wrapper.BOXS10CONVEYANCE);
				AssertEquals(nameof(wrapper.BOXS6SECURITYCONSIGNEE), "SECURITYCONSIGNEE NAME\nSECURITYCONSIGNEE STREET\n555555555 SECURITYCONSIGNEE CITY\nTR", wrapper.BOXS6SECURITYCONSIGNEE);
				AssertEquals(nameof(wrapper.BOXS6SECURITYCONSIGNEEEORI), "TR555555555555555", wrapper.BOXS6SECURITYCONSIGNEEEORI);
				AssertEquals(nameof(wrapper.BOXS4SECURITYCONSIGNOR), "SECURITYCONSIGNOR NAME\nSECURITYCONSIGNOR STREET\n444444444 SECURITYCONSIGNOR CITY\nDE", wrapper.BOXS4SECURITYCONSIGNOR);
				AssertEquals(nameof(wrapper.BOXS4SECURITYCONSIGNOREORI), "DE444444444444444", wrapper.BOXS4SECURITYCONSIGNOREORI);
				AssertEquals(nameof(wrapper.BOXS00SECURITY), true, wrapper.BOXS00SECURITY);
				AssertEquals(nameof(wrapper.BOX15COUNTRYOFORIGIN), "IT", wrapper.BOX15COUNTRYOFORIGIN);
				AssertEquals(nameof(wrapper.BOXCOFFICEOFDEPARTURE), "IT123 (ITOffice)", wrapper.BOXCOFFICEOFDEPARTURE);
				AssertEquals(nameof(wrapper.BOX5ITEMS), "4", wrapper.BOX5ITEMS);
				AssertEquals(nameof(wrapper.BOX35GROSSMASS), "45.545567", wrapper.BOX35GROSSMASS);
				AssertEquals(nameof(wrapper.BOX6PACKAGES), "20", wrapper.BOX6PACKAGES);
				AssertEquals(nameof(wrapper.BOX52GUARANTEECODE), "Type1,Type2,Type3,Type4", wrapper.BOX52GUARANTEECODE);
				AssertEquals(nameof(wrapper.BOXS13ROUTING), "IE,GB,FR,BE", wrapper.BOXS13ROUTING);
				AssertEquals(nameof(wrapper.BOXDSEALSAFFIXEDNUMBER), "2", wrapper.BOXDSEALSAFFIXEDNUMBER);
				AssertEquals(nameof(wrapper.BOXS28SEALSNUMBER), "2", wrapper.BOXS28SEALSNUMBER);
				AssertEquals(nameof(wrapper.BOXDSEALSIDENTITY), "10, 20", wrapper.BOXDSEALSIDENTITY);
			});
		}

		public void TestWrapperPropertiesWhenSourceIsEmptyObject()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var wrapper = SecurityNctsHeaderDocumentWrapper.New(header, Factory);

			CombineAssertions("Test properties when source is empty", () =>
			{
				AssertEquals(nameof(wrapper.BOXS32OTHERSCI), "---", wrapper.BOXS32OTHERSCI);
				AssertEquals(nameof(wrapper.BOXS12FIRSTARRIVALTIME), "---", wrapper.BOXS12FIRSTARRIVALTIME);
				AssertEquals(nameof(wrapper.BOXS29TRANSPORTCHARGESMOP), "---", wrapper.BOXS29TRANSPORTCHARGESMOP);
				AssertEquals(nameof(wrapper.BOX7REFERENCENUMBERS), "", wrapper.BOX7REFERENCENUMBERS);
				AssertEquals(nameof(wrapper.BOX21BORDERTRANSPORTID), "---", wrapper.BOX21BORDERTRANSPORTID);
				AssertEquals(nameof(wrapper.BOX21BORDERTRANSPORTFLAG), "---", wrapper.BOX21BORDERTRANSPORTFLAG);
				AssertEquals(nameof(wrapper.BOX25BORDERTRANSPORTMODE), "---", wrapper.BOX25BORDERTRANSPORTMODE);
				AssertEquals(nameof(wrapper.BOX30LOCATIONOFGOODS), "", wrapper.BOX30LOCATIONOFGOODS);
				AssertEquals(nameof(wrapper.BOXS18PLACEOFUNLOADING), "---", wrapper.BOXS18PLACEOFUNLOADING);
				AssertEquals(nameof(wrapper.BOXS17PLACEOFLOADING), "---", wrapper.BOXS17PLACEOFLOADING);
				AssertEquals(nameof(wrapper.BOXS10CONVEYANCE), "---", wrapper.BOXS10CONVEYANCE);
				AssertEquals(nameof(wrapper.BOXS6SECURITYCONSIGNEE), "", wrapper.BOXS6SECURITYCONSIGNEE);
				AssertEquals(nameof(wrapper.BOXS6SECURITYCONSIGNEEEORI), "", wrapper.BOXS6SECURITYCONSIGNEEEORI);
				AssertEquals(nameof(wrapper.BOXS4SECURITYCONSIGNOR), "", wrapper.BOXS4SECURITYCONSIGNOR);
				AssertEquals(nameof(wrapper.BOXS4SECURITYCONSIGNOREORI), "", wrapper.BOXS4SECURITYCONSIGNOREORI);
				AssertEquals(nameof(wrapper.BOXS7CARRIER), "", wrapper.BOXS7CARRIER);
				AssertEquals(nameof(wrapper.BOXS7CARRIEREORI), "", wrapper.BOXS7CARRIEREORI);
				AssertEquals(nameof(wrapper.BOXS28SEALSNUMBER), "0", wrapper.BOXS28SEALSNUMBER);
				AssertEquals(nameof(wrapper.BOXS00SECURITY), false, wrapper.BOXS00SECURITY);
				AssertEquals(nameof(wrapper.BOXDSEALSIDENTITY), "", wrapper.BOXDSEALSIDENTITY);
				AssertEquals(nameof(wrapper.BOX5ITEMS), "0", wrapper.BOX5ITEMS);
				AssertEquals(nameof(wrapper.BOX6PACKAGES), "0", wrapper.BOX6PACKAGES);
				AssertEquals(nameof(wrapper.BOX35GROSSMASS), "0", wrapper.BOX35GROSSMASS);
				AssertEquals(nameof(wrapper.BOX15COUNTRYOFORIGIN), "---", wrapper.BOX15COUNTRYOFORIGIN);
				AssertEquals(nameof(wrapper.BOXS13ROUTING), "", wrapper.BOXS13ROUTING);
				AssertEquals(nameof(wrapper.BOXS28SEALSNUMBER), "0", wrapper.BOXS28SEALSNUMBER);
				AssertEquals(nameof(wrapper.BOXCOFFICEOFDEPARTURE), "", wrapper.BOXCOFFICEOFDEPARTURE);
				AssertEquals(nameof(wrapper.BOX52GUARANTEECODE), "", wrapper.BOX52GUARANTEECODE);
				AssertEquals(nameof(SecurityNctsHeaderDocumentWrapper.BOXS13ROUTING), "", wrapper.BOXS13ROUTING);
			});
		}

		public void TestLocationOfGoods_Phase4()
		{
			const string locationOfGoods = "Goods location", customsSubPlace = "Location subplace";

			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = header.MovementHeader;
			movementHeader.BM_LocationOfGoods = "Goods location";
			movementHeader.BM_CustomsSubPlace = "Location subplace";

			var wrapper = SecurityNctsHeaderDocumentWrapper.New(header, Factory);
			var expectedLocationOfGoods = $"{locationOfGoods} {customsSubPlace}";

			AssertEquals(nameof(wrapper.BOX30LOCATIONOFGOODS), expectedLocationOfGoods, wrapper.BOX30LOCATIONOFGOODS);
		}

		public void TestLocationOfGoods_Phase5()
		{
			var (header, wrapper) = SetUpData();
			var goodsLocation = header.MovementHeader.GoodsLocation;

			goodsLocation.CGL_Qualifier = "Y";
			goodsLocation.AdditionalIdentifier = "AdditionalItentifier";
			goodsLocation.Address.AuthorisationNumber = "1234";
			AssertEquals(nameof(wrapper.BOX30LOCATIONOFGOODS), "1234-AdditionalItentifier", wrapper.BOX30LOCATIONOFGOODS);

			goodsLocation.CGL_Qualifier = "V";
			goodsLocation.CGL_CustomsOffice = "CusOffice";
			AssertEquals(nameof(wrapper.BOX30LOCATIONOFGOODS), "CusOffice", wrapper.BOX30LOCATIONOFGOODS);

			goodsLocation.CGL_Qualifier = "Z";
			goodsLocation.Address.E2_Address1AndE2_Address2 = "Address1Address2";
			goodsLocation.Address.E2_City = "City";
			AssertEquals(nameof(wrapper.BOX30LOCATIONOFGOODS), "Address1Address2-City", wrapper.BOX30LOCATIONOFGOODS);
		}

		public void TestCarrierAndCarrierEORI_Phase4()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			SetupCarrierForPhase4(header);
			var wrapper = SecurityNctsHeaderDocumentWrapper.New(header, Factory);

			AssertEquals(nameof(wrapper.BOXS7CARRIER), "CIRCUIT EQUIPAMENTOS ESPORTIVOS LTD\nCARRIER STREET\n666666666 CARRIER CITY\nBR", wrapper.BOXS7CARRIER);
			AssertEquals(nameof(wrapper.BOXS7CARRIEREORI), "GB954131533000", wrapper.BOXS7CARRIEREORI);
		}

		public void TestMOVEMENTREFERENCENUMBER()
		{
			var nctsHeader = Factory.New<NctsHeaderForDocumentWrapperTest>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var wrapper = SecurityNctsHeaderDocumentWrapper.New(nctsHeader, Factory);

			nctsHeader.IsFallBackActiveForTest = true;
			AssertEquals("When IsFallBackActive is true", ZString.Empty, wrapper.MOVEMENTREFERENCENUMBER);

			nctsHeader.IsFallBackActiveForTest = false;
			AssertEquals("When Mrn is empty", ZString.Empty, wrapper.MOVEMENTREFERENCENUMBER);

			nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "21FR00007411BBC885";
			AssertEquals("When Mrn has value", "21FR00007411BBC885", wrapper.MOVEMENTREFERENCENUMBER);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return SecurityNctsHeaderDocumentWrapper.New(Factory.New<NctsHeader>(), Factory);
		}

		(NctsHeader header, SecurityNctsHeaderDocumentWrapper wrapper) SetUpData()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			SetUpSecurityConsignorForTest(header);
			SetUpSecurityConsigneeForTest(header);
			SetUpCarrierForTest(header);
			SetUpGoodsItemsForTest(header);
			SetUpOfficesForTest(header);
			SetUpGuaranteesForTest(header);
			SetUpCountriesForRouting(header);
			header.LocalReferenceNumber = "Local Ref Number";

			var movementHeader = header.MovementHeader;
			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
			movementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
			movementHeader.BM_BTAIndicator = "A";
			movementHeader.BM_MethodOfPayment = "B";
			movementHeader.BM_TOLCarrierID = "EP-666-EW";
			movementHeader.BM_TOLCarrierCode = "C";
			movementHeader.BM_LocationOfGoods = "Goods location";
			movementHeader.BM_CustomsSubPlace = "Location subplace";
			movementHeader.BM_ForeignDestPortKCode = Enterprise.Core.Constants.CountryCodes.Italy;
			movementHeader.BM_PlaceOfUnloading = "UNLDG";
			movementHeader.BM_PortOfPresentationCode = Enterprise.Core.Constants.CountryCodes.Poland;
			movementHeader.BM_PlaceOfLoading = "LOADG";
			movementHeader.BM_RL_NKForeignDestPort = "LOADG";
			movementHeader.BM_ConveyanceNumber = "C000001";
			movementHeader.BM_SealQty = 15;
			movementHeader.BM_RN_NKTOLCarrierNationality = Enterprise.Core.Constants.CountryCodes.Turkey;
			movementHeader.BM_RN_NKCountryOfDispatch = "IT";

			return (header, SecurityNctsHeaderDocumentWrapper.New(header, Factory));
		}

		void SetUpSecurityConsigneeForTest(NctsHeader header)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.MainAddress;
			orgHeader.OH_FullName = "SECURITYCONSIGNEE NAME";
			address.OA_Address1 = "SECURITYCONSIGNEE STREET";
			address.OA_PostCode = "555555555";
			address.OA_City = "SECURITYCONSIGNEE CITY";
			address.OA_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Turkey;
			header.SecurityConsignee.E2_OA_Address = address.PK;
			var eori = orgHeader.CustomsCodes.AddNew();
			eori.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			eori.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.Turkey;
			eori.OK_CustomsRegNo = "55555555555555555";
		}

		void SetUpSecurityConsignorForTest(NctsHeader header)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.MainAddress;
			orgHeader.OH_FullName = "SECURITYCONSIGNOR NAME";
			address.OA_Address1 = "SECURITYCONSIGNOR STREET";
			address.OA_PostCode = "444444444";
			address.OA_City = "SECURITYCONSIGNOR CITY";
			address.OA_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Germany;
			header.SecurityConsignor.E2_OA_Address = address.PK;
			var eori = orgHeader.CustomsCodes.AddNew();
			eori.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			eori.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.Germany;
			eori.OK_CustomsRegNo = "44444444444444444";
		}

		void SetUpCarrierForTest(NctsHeader header)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.MainAddress;
			orgHeader.OH_FullName = "CIRCUIT EQUIPAMENTOS ESPORTIVOS LTD";
			address.OA_Address1 = "CARRIER STREET";
			address.OA_PostCode = "666666666";
			address.OA_City = "CARRIER CITY";
			address.OA_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Brazil;
			header.MovementHeader.Carrier.E2_OA_Address = address.PK;
			var eori = orgHeader.CustomsCodes.AddNew();
			eori.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			eori.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.Switzerland;
			eori.OK_CustomsRegNo = "66666666666666666";
		}

		void SetUpGoodsItemsForTest(NctsHeader header)
		{
			var houseBill1 = header.Bills.AddNew();
			var item1 = houseBill1.GoodsItems.AddNew();
			item1.BY_GrossWeight = 12;
			item1.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			var pack1 = item1.Packages.AddNew();
			pack1.B5_UnitCount = 5;

			var headerContainer = header.DepartureHeaderContainers.AddNew();
			headerContainer.BC_ContainerNum = "CONTAINER1";
			headerContainer.BC_Seal1 = "10";
			headerContainer.BC_Seal2 = "20";

			SetUpHeaderContainers(pack1, headerContainer);

			var item2 = houseBill1.GoodsItems.AddNew();
			item2.BY_GrossWeight = 8;
			item2.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			var pack2 = item2.Packages.AddNew();
			pack2.B5_UnitCount = 6;

			var houseBill2 = header.Bills.AddNew();
			var item3 = houseBill2.GoodsItems.AddNew();
			item3.BY_GrossWeight = 2400;
			item3.BY_GrossWeightUnit = Core.Constants.Weight.Grams;
			var pack3 = item3.Packages.AddNew();
			pack3.B5_UnitCount = 9;

			var headerContainer2 = header.DepartureHeaderContainers.AddNew();
			headerContainer2.BC_ContainerNum = "CONTAINER2";
			headerContainer2.BC_Seal1 = "30";
			headerContainer2.BC_Seal2 = "40";
			var additionalSeals = headerContainer2.AdditionalSeals.AddNew();
			additionalSeals.BK_SealNumber = "50";
			var additionalSeals2 = headerContainer2.AdditionalSeals.AddNew();
			additionalSeals2.BK_SealNumber = "10";

			SetUpHeaderContainers(pack3, headerContainer2);

			var item4 = houseBill2.GoodsItems.AddNew();
			item4.BY_GrossWeight = 23.145567;
			item4.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
		}

		void SetUpHeaderContainers(NctsPackage package, NctsDepartureHeaderContainer container)
		{
			var pivot = Factory.New<NctsCusInBondContainerPackageGenPivot>();
			pivot.Relation1Object = package;
			pivot.Relation2Object = container;
		}

		void SetUpOffice(NctsHeader header, string countryCode, string officeCode, string role, string description, string street, string postCode, string city)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(countryCode, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");

			var cusCodeList = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList.ZZD_Code = officeCode;
			cusCodeList.ZZD_Description = description;
			cusCodeList.ZZD_CodeType = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
			cusCodeList.ZZD_CountryOrGrouping = countryCode;
			cusCodeList.ZZD_EndDate = ZDateTime.Today.AddDays(2);
			cusCodeList.ZZD_StartDate = ZDateTime.Today.AddDays(-2);

			if (street != ZString.Empty)
			{
				var officeAttribute = cusCodeList.Attributes.AddNew();
				officeAttribute.ZZE_ZXE_NKName = "Street";
				officeAttribute.ZZE_Value = street;
			}

			if (postCode != ZString.Empty)
			{
				var officeAttribute2 = cusCodeList.Attributes.AddNew();
				officeAttribute2.ZZE_ZXE_NKName = "PostCode";
				officeAttribute2.ZZE_Value = postCode;
			}

			if (city != ZString.Empty)
			{
				var officeAttribute3 = cusCodeList.Attributes.AddNew();
				officeAttribute3.ZZE_ZXE_NKName = "CITY";
				officeAttribute3.ZZE_Value = city;
			}

			NctsEuOfficeCode cusOffice = null;
			if (role == EuOfficeCodesTypes.Codes.OfficeOfDeparture)
			{
				cusOffice = header.MovementHeader.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDeparture);
			}
			else if (role == EuOfficeCodesTypes.Codes.OfficeOfDestination)
			{
				cusOffice = header.MovementHeader.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDestination);
			}

			if (cusOffice == null)
			{
				cusOffice = header.MovementHeader.CustomsOffices.AddNew();
				cusOffice.CY_Code = role;
			}

			cusOffice.CY_Data = officeCode;
		}

		void SetUpOfficesForTest(NctsHeader header)
		{
			SetUpOffice(header, Core.Constants.CountryCodes.Italy, "IT123", OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "ITOffice", "Departure Office Street", "DepartureOfficePostCode", "DepartureofficeCity");
			SetUpOffice(header, Core.Constants.CountryCodes.UnitedKingdom, "GB000001", OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination, "GBOffice1", "", "", "");
			SetUpOffice(header, Core.Constants.CountryCodes.Turkey, "TR000001", OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, "TROffice1", "", "", "");
		}

		void SetUpGuaranteesForTest(NctsHeader header)
		{
			AddGuarantee("GUAR1", "Type1");
			AddGuarantee("GUAR2", "Type2");
			AddGuarantee("GUAR3", "Type3");
			AddGuarantee("GUAR4", "Type4");

			void AddGuarantee(ZString bondNumber, ZString bondType)
			{
				var guarantee = header.MovementHeader.Guarantees.AddNew();
				guarantee.PW_BondNumber = bondNumber;
				guarantee.PW_BondType = bondType;
			}
		}

		void SetUpCountriesForRouting(NctsHeader header)
		{
			var country1 = header.CountriesOfRouting.AddNew();
			country1.CY_Data = Core.Constants.CountryCodes.Ireland;
			var country2 = header.CountriesOfRouting.AddNew();
			country2.CY_Data = Core.Constants.CountryCodes.UnitedKingdom;
			var country3 = header.CountriesOfRouting.AddNew();
			country3.CY_Data = Core.Constants.CountryCodes.France;
			var country4 = header.CountriesOfRouting.AddNew();
			country4.CY_Data = Core.Constants.CountryCodes.Belgium;
		}

		void SetupCarrierForPhase4(NctsHeader header)
		{
			var carrier = header.Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "CIREQU");
			var carrierAddress = carrier.MainAddress;
			carrierAddress.Address1 = "CARRIER STREET";
			carrierAddress.City = "CARRIER CITY";
			carrierAddress.Postcode = "666666666";
			carrier.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "GB954131533000", Core.Constants.CountryCodes.UnitedKingdom);
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers, "GBR/ABC1234", Core.Constants.CountryCodes.UnitedKingdom);
			header.BH_OH_Carrier = carrier.PK;
			header.Factory.Save();
		}
	}
}
