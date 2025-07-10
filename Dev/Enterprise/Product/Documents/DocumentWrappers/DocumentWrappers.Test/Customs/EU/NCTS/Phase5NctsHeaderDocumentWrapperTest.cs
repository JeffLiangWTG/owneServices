using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using Constants = Enterprise.Customs.Universal.Constants;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.DocumentWrappers.Customs.EU.NCTS.Testing
{
	[TestedType(typeof(Phase5NctsHeaderDocumentWrapper))]
	sealed class Phase5NctsHeaderDocumentWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMRN()
		{
			var mrnNumber = CusEntryNumber.New(NctsHeader.MovementHeader, "MRN", NctsHeader.CountryCode);
			mrnNumber.CE_EntryNum = "92CB9CE9C465";
			AssertEquals("MRN", "92CB9CE9C465", Wrapper.MRN);
		}

		public void TestGuarantees()
		{
			var guarantee1 = NctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee1.PW_BondType = "198E9D11";
			guarantee1.PW_BondNumber2 = "8560";
			guarantee1.PW_BondNumber = "7324D4936DE1";
			guarantee1.PW_Password = "8560";
			guarantee1.PW_BondAmount = 12.345;
			var guarantee2 = NctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee2.PW_BondType = "BDF76B0";
			guarantee2.PW_BondNumber2 = "122B";
			guarantee2.PW_BondNumber = "D3F4E6E35F85";
			guarantee2.PW_Password = "B70B";
			guarantee2.PW_BondAmount = 23.456;
			var guarantee3 = NctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee3.PW_BondType = "6A4A1E7D";
			guarantee3.PW_BondNumber2 = "B2F8";
			guarantee3.PW_BondNumber = "88DCBF872C6D";
			guarantee3.PW_Password = "889F";
			guarantee3.PW_BondAmount = 34.567;
			var guarantee4 = NctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee4.PW_BondType = "F96";
			guarantee4.PW_BondNumber2 = "437F";
			guarantee4.PW_BondNumber = "14AAFE739B05";
			guarantee4.PW_Password = "86AB";
			guarantee4.PW_BondAmount = 45.678;

			var expected =
@"198E9D11   - 7324D4936DE1
6A4A1E7D   - 88DCBF872C6D
BDF76B0    - D3F4E6E35F85
F96        - 14AAFE739B05";

			AssertEquals("Guarantees", expected, Wrapper.Guarantees);
		}

		public void TestCountryOfDispatch()
		{
			MovementHeader.BM_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.Germany;
			AssertEquals("CountryOfDispatch", Core.Constants.CountryCodes.Germany, Wrapper.CountryOfDispatch);
		}

		public void TestCountryOfDestination()
		{
			MovementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Latvia;
			AssertEquals("CountryOfDestination", Core.Constants.CountryCodes.Latvia, Wrapper.CountryOfDestination);
		}

		public void TestContainerIndicator()
		{
			AssertEquals("ContainerIndicator", CrossedCheckBox, Wrapper.ContainerIndicator);

			NctsHeader.DepartureHeaderContainers.AddNew().BC_ContainerNum = "730BEC62";

			var newWrapper = Phase5NctsHeaderDocumentWrapper.New(NctsHeader, NctsHeader.Factory);
			AssertEquals("ContainerIndicator", CheckedCheckBox, newWrapper.ContainerIndicator);
		}

		const string CheckedCheckBox = "\u2611";
		const string CrossedCheckBox = "\u2612";

		public void TestInlandModeOfTransport()
		{
			MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
			AssertEquals("InlandModeOfTransport", "3", Wrapper.InlandModeOfTransport);
		}

		public void TestModeOfTransportAtTheBorder()
		{
			MovementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
			AssertEquals("ModeOfTransportAtTheBorder", "8", Wrapper.ModeOfTransportAtTheBorder);
		}

		public void TestGrossMass()
		{
			MovementHeader.BM_GrossWeight = 123.456;
			AssertEquals("GrossMass", "123.456", Wrapper.GrossMass);
		}

		public void TestReferenceNumberUCR()
		{
			MovementHeader.BM_UniqueConsignmentReference = "0E15968DFC98";
			AssertEquals("ReferenceNumberUCR", "0E15968DFC98", Wrapper.ReferenceNumberUCR);
		}

		public void TestCarrierId()
		{
			SetupCarrier();
			AssertEquals("IT123456", Wrapper.CarrierId);
		}

		public void TestCarrierContact()
		{
			SetupCarrier();
			AssertEquals("Carrier CUS Contact, +12 09876543, cus-contact@carrier.com", Wrapper.CarrierContactPersonFormatted);
		}

		void SetupCarrier()
		{
			var carrier = Factory.New<OrgHeader>();

			var eoriCode = carrier.CustomsCodes.AddNew();
			eoriCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Italy;
			eoriCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			eoriCode.OK_CustomsRegNo = "123456";
			MovementHeader.Carrier.E2_OA_Address = carrier.MainAddress.PK;

			var nonCusContact = carrier.Contacts.AddNew();
			nonCusContact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.USPGA;
			nonCusContact.OC_ContactName = "Non-Cus Contact";

			var cusContact = carrier.Contacts.AddNew();
			cusContact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
			cusContact.OC_ContactName = "Carrier CUS Contact";
			cusContact.OC_Mobile = "+12 09876543";
			cusContact.OC_HomePhone = "+12 98765";
			cusContact.OC_Email = "cus-contact@carrier.com";
		}

		public void TestConsignorId()
		{
			SetupConsignor();
			AssertEquals("ES654321", Wrapper.ConsignorId);
		}

		public void TestConsignorAddress()
		{
			SetupConsignor();
			AssertEquals("Consignor Company LTD., Address line 1 Address line 2, 1234 CITY 1, ES", Wrapper.ConsignorAddressFormatted);
		}

		public void TestConsignorContact()
		{
			SetupConsignor();
			AssertEquals("Consignor CUS Contact, +12 34567890, cus-contact@consignor.com", Wrapper.ConsignorContactPersonFormatted);
		}

		void SetupConsignor()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "Consignor Company LTD.";

			var eoriCode = consignor.CustomsCodes.AddNew();
			eoriCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Spain;
			eoriCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			eoriCode.OK_CustomsRegNo = "654321";

			var address = consignor.MainAddress;
			address.OA_Address1 = "Address line 1";
			address.OA_Address2 = "Address line 2";
			address.OA_PostCode = "1234";
			address.OA_City = "CITY 1";
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Spain;

			var cusContact = consignor.Contacts.AddNew();
			cusContact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
			cusContact.OC_ContactName = "Consignor CUS Contact";
			cusContact.OC_Mobile = "+12 34567890";
			cusContact.OC_HomePhone = "+12 98765";
			cusContact.OC_Email = "cus-contact@consignor.com";

			NctsHeader.Consignor.E2_OA_Address = address.PK;
		}

		public void TestConsigneeId()
		{
			SetupConsignee();
			AssertEquals("FR283746", Wrapper.ConsigneeId);
		}

		public void TestConsigneeAddress()
		{
			SetupConsignee();
			AssertEquals("Consignee Company LTD., Address line 1 Address line 2, 1234 CITY 1, FR", Wrapper.ConsigneeAddressFormatted);
		}

		void SetupConsignee()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "Consignee Company LTD.";

			var eoriCode = consignee.CustomsCodes.AddNew();
			eoriCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			eoriCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			eoriCode.OK_CustomsRegNo = "283746";

			var address = consignee.MainAddress;
			address.OA_Address1 = "Address line 1";
			address.OA_Address2 = "Address line 2";
			address.OA_PostCode = "1234";
			address.OA_City = "CITY 1";
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			var cusContact = consignee.Contacts.AddNew();
			cusContact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
			cusContact.OC_ContactName = "Consignee CUS Contact";
			cusContact.OC_Mobile = "+12 34567890";
			cusContact.OC_HomePhone = "+12 98765";
			cusContact.OC_Email = "cus-contact@consignee.com";

			NctsHeader.Consignee.E2_OA_Address = address.PK;
		}

		public void TestAdditionalSupplyChain()
		{
			var act1 = MovementHeader.CusSupplyChainActors.AddNew();
			act1.CFR_Code = SupplyChainActorRoleList.Codes.WH;
			act1.CFR_Reference = "74ADD69ECD74";
			var act2 = MovementHeader.CusSupplyChainActors.AddNew();
			act2.CFR_Code = SupplyChainActorRoleList.Codes.FW;
			act2.CFR_Reference = "1FCFA7B902198D3FFB5E";

			var expected =
@"FW 1FCFA7B902198D3FFB5E
WH 74ADD69ECD74";

			AssertEquals("AdditionalSupplyChain", expected, Wrapper.AdditionalSupplyChain);
		}

		public void TestTransportEquipment()
		{
			AddContainersWithSealsAndItems(NctsHeader);

			var expected =
@"1/730BEC62/4/1
2/437514E909E7/2/2-3";

			AssertEquals("TransportEquipment", expected, Wrapper.TransportEquipment);
		}

		public void TestSeal()
		{
			AddContainersWithSeals(NctsHeader);

			var expected =
@"1/4240179DB205,DA346891,E8B4A2622590,BA42FEB3
2/857067C90FDF,DA6541519BC9";

			AssertEquals("Seal", expected, Wrapper.Seal);
		}

		public void TestLocationOfGoods()
		{
			var goodsLocation = MovementHeader.GoodsLocation;

			goodsLocation.CGL_Type = "A";

			CombineAssertions(() =>
			{
				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
				goodsLocation.Address.AuthorisationNumber = "5685C67D8B97";
				goodsLocation.Unlocode = "AT000000";
				var expected =
@"A
Y
5685C67D8B97
AT000000";
				AssertEquals("Y", expected, Wrapper.LocationOfGoods);

				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
				expected =
@"A
U";
				AssertEquals("U", expected, Wrapper.LocationOfGoods);

				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
				goodsLocation.CGL_CustomsOffice = "IEROS100";
				expected =
@"A
V
IEROS100";
				AssertEquals("V", expected, Wrapper.LocationOfGoods);

				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
				goodsLocation.Address.E2_GovRegNum = "EORI1234";
				expected =
@"A
X
EORI1234";
				AssertEquals("X", expected, Wrapper.LocationOfGoods);

				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.PostcodeAddress;
				goodsLocation.AdditionalIdentifier = "House 1";
				goodsLocation.Address.E2_Postcode = "POSTCODE";
				goodsLocation.Address.E2_RN_NKCountryCode = "IE";
				expected =
@"A
T
House 1
POSTCODE
IE";
				AssertEquals("T", expected, Wrapper.LocationOfGoods);

				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
				goodsLocation.Address.E2_Address1AndE2_Address2 = "Street + Number";
				goodsLocation.Address.E2_City = "City";
				goodsLocation.Address.E2_Postcode = "POSTCODE";
				goodsLocation.Address.E2_RN_NKCountryCode = "IE";
				expected =
@"A
Z
Street + Number
City
POSTCODE
IE";
				AssertEquals("Z", expected, Wrapper.LocationOfGoods);

				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.GnssCoordinates;
				goodsLocation.Address.E2_Latitude = 53.123;
				goodsLocation.Address.E2_Longitude = -7.5432;
				expected =
@"A
W
53.123
-7.5432";
				AssertEquals("W", expected, Wrapper.LocationOfGoods);
			});
		}

		public void TestLocationOfGoodsContactPerson()
		{
			var goodsLocation = MovementHeader.GoodsLocation;

			goodsLocation.CGL_Type = "A";
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;

			var goodsLocationAddress = goodsLocation.Address;
			goodsLocationAddress.E2_Contact = "LocationOfGoods CUS Contact";
			goodsLocationAddress.E2_Phone = "+12 34567890";
			goodsLocationAddress.E2_Email = "cus-contact@location.com";

			var expected =
@"LocationOfGoods CUS Contact
+12 34567890
cus-contact@location.com";
			AssertEquals("LocationOfGoodsContactPerson", expected, Wrapper.LocationOfGoodsContactPerson);
		}

		public void TestDeclarationType()
		{
			AssertEquals("When MovementHeader is null", ZString.Empty, Wrapper.DeclarationType);

			ResetWrapper();
			MovementHeader.BM_InBondEntryType = "T2SM";
			AssertEquals("T2SM", Wrapper.DeclarationType);
		}

		public void TestAdditionalDeclarationType()
		{
			AssertEquals("When MovementHeader is null", ZString.Empty, Wrapper.AdditionalDeclarationType);

			ResetWrapper();
			MovementHeader.BM_AdditionalDeclarationType = "A";
			AssertEquals("A", Wrapper.AdditionalDeclarationType);
		}

		public void TestTirCarnetNumber()
		{
			AssertEquals("When MovementHeader is null", ZString.Empty, Wrapper.TirCarnetNumber);

			ResetWrapper();
			MovementHeader.TirCarnetNumber = "TIR001";
			AssertEquals("TIR001", Wrapper.TirCarnetNumber);
		}

		public void TestSecurity()
		{
			AssertEquals("When MovementHeader is null", ZString.Empty, Wrapper.Security);

			ResetWrapper();
			MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			AssertEquals("0", Wrapper.Security);

			MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
			AssertEquals("1", Wrapper.Security);

			MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
			AssertEquals("2", Wrapper.Security);

			MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
			AssertEquals("3", Wrapper.Security);
		}

		public void TestReducedDatasetIndicator()
		{
			AssertEquals("When MovementHeader is null", CrossedCheckBox, Wrapper.ReducedDatasetIndicator);

			ResetWrapper();
			MovementHeader.BM_ReducedDatasetIndicator = true;
			AssertEquals("When BM_ReducedDatasetIndicator is true", CheckedCheckBox, Wrapper.ReducedDatasetIndicator);

			MovementHeader.BM_ReducedDatasetIndicator = false;
			AssertEquals("When BM_ReducedDatasetIndicator is false", CrossedCheckBox, Wrapper.ReducedDatasetIndicator);
		}

		public void TestSpecificCircumstanceIndicator()
		{
			AssertEquals("When MovementHeader is null", ZString.Empty, Wrapper.SpecificCircumstanceIndicator);

			ResetWrapper();
			MovementHeader.BM_SpecificCircumstance = "A20";
			AssertEquals("A20", Wrapper.SpecificCircumstanceIndicator);
		}

		public void TestBindingItinerary()
		{
			MovementHeader.BM_TypeOfSecurity = "ENT";
			AssertEquals("When Security is ENT", CheckedCheckBox, Wrapper.BindingItinerary);

			MovementHeader.BM_TypeOfSecurity = "EXI";
			AssertEquals("When Security is EXI", CheckedCheckBox, Wrapper.BindingItinerary);

			MovementHeader.BM_TypeOfSecurity = "BTH";
			AssertEquals("When Security is BTH", CheckedCheckBox, Wrapper.BindingItinerary);

			MovementHeader.BM_TypeOfSecurity = "NON";
			AssertEquals("When Security is NON", CrossedCheckBox, Wrapper.BindingItinerary);

			(NctsHeader.CountriesOfRouting as CountryOfRoutingCollection<CountryOfRouting>)?.AddNew();
			AssertEquals("When CountriesOfRouting has items", CheckedCheckBox, Wrapper.BindingItinerary);
		}

		public void TestLimitDate()
		{
			AssertEquals("When MovementHeader is null", ZString.Empty, Wrapper.LimitDate);

			ResetWrapper();
			MovementHeader.BM_ExportDate = new ZDateTime(2024, 07, 17, 16, 30, 59);
			AssertEquals("2024-07-17", Wrapper.LimitDate);
		}

		public void TestAuthorisationsFormatted()
		{
			AssertEquals("When MovementHeader is null", ZString.Empty, Wrapper.AuthorisationsFormatted);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType("EUNAU", "OUT", "Authorisation Codes to Document Type Code", true);
			helper.CreateCusMap("EUNAU", "CWP", "C517", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "EUN");
			helper.CreateCusMap("EUNAU", "ACT", "C520", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "EUN");
			Factory.Save();

			ResetWrapper();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var auth1 = MovementHeader.CusAuthorizationUsages.AddNew();
				auth1.AGC_Code = "CWP";
				auth1.AGC_Number = "001";
				var auth2 = MovementHeader.CusAuthorizationUsages.AddNew();
				auth2.AGC_Code = "ACT";
				auth2.AGC_Number = "002";
				AssertEquals(@"1: C517 - 001
2: C520 - 002", Wrapper.AuthorisationsFormatted);
			}
		}

		public void TestCustomsOfficeOfDeparture()
		{
			AssertEquals("When MovementHeader is null", ZString.Empty, Wrapper.CustomsOfficeOfDeparture);

			ResetWrapper();
			MovementHeader.CustomsOffices.RemoveAndDeleteAll();
			var customsOffice = MovementHeader.CustomsOffices.AddNew();
			customsOffice.CY_Type = "EUO";
			customsOffice.CY_Code = "DEP";
			customsOffice.CY_Data = "IEDUB100";
			AssertEquals("IEDUB100", Wrapper.CustomsOfficeOfDeparture);
		}

		public void TestCustomsOfficeOfDestination()
		{
			AssertEquals("When MovementHeader is null", ZString.Empty, Wrapper.CustomsOfficeOfDestination);

			ResetWrapper();
			MovementHeader.CustomsOffices.RemoveAndDeleteAll();
			var customsOffice = MovementHeader.CustomsOffices.AddNew();
			customsOffice.CY_Type = "EUO";
			customsOffice.CY_Code = "DES";
			customsOffice.CY_Data = "FR002270";
			AssertEquals("FR002270", Wrapper.CustomsOfficeOfDestination);
		}

		public void TestCustomsOfficesOfTransitFormatted()
		{
			AssertEquals("When MovementHeader is null", ZString.Empty, Wrapper.CustomsOfficesOfTransitFormatted);

			ResetWrapper();
			MovementHeader.CustomsOffices.RemoveAndDeleteAll();
			var customsOffice = MovementHeader.CustomsOffices.AddNew();
			customsOffice.CY_Type = "EUO";
			customsOffice.CY_Code = "TRA";
			customsOffice.CY_Data = "GB000080";
			customsOffice = MovementHeader.CustomsOffices.AddNew();
			customsOffice.CY_Type = "EUO";
			customsOffice.CY_Code = "TRA";
			customsOffice.CY_Data = "FR000380";

			AssertEquals(@"1/ GB000080
2/ FR000380", Wrapper.CustomsOfficesOfTransitFormatted);
		}

		public void TestCustomsOfficesOfExitForTransitFormatted()
		{
			AssertEquals("When MovementHeader is null", ZString.Empty, Wrapper.CustomsOfficesOfExitForTransitFormatted);

			ResetWrapper();
			var customsOffice = MovementHeader.CustomsOffices.AddNew();
			customsOffice.CY_Type = "EUO";
			customsOffice.CY_Code = "TXT";
			customsOffice.CY_Data = "IESNN400";
			customsOffice = MovementHeader.CustomsOffices.AddNew();
			customsOffice.CY_Type = "EUO";
			customsOffice.CY_Code = "TXT";
			customsOffice.CY_Data = "IETIV100";

			AssertEquals(@"1/ IESNN400
2/ IETIV100", Wrapper.CustomsOfficesOfExitForTransitFormatted);
		}

		public void TestHolderOfTheTransitProcedureId()
		{
			AssertEquals("When MovementHeader is null", ZString.Empty, Wrapper.HolderOfTheTransitProcedureId);

			ResetWrapper();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.MainAddress;
			NctsHeader.Principal.E2_OA_Address = address.PK;
			var tcu = orgHeader.CustomsCodes.AddNew();
			tcu.OK_CodeType = OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU;
			tcu.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Ireland;
			tcu.OK_CustomsRegNo = "TCU001";
			AssertEquals("TCU", "IETCU001", Wrapper.HolderOfTheTransitProcedureId);

			ResetAll();
			var eori = orgHeader.CustomsCodes.AddNew();
			eori.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			eori.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Ireland;
			eori.OK_CustomsRegNo = "IE9394798H";
			NctsHeader.Principal.E2_OA_Address = address.PK;
			AssertEquals("EORI", "IE9394798H", Wrapper.HolderOfTheTransitProcedureId);
		}

		public void TestHolderOfTheTransitProcedureTirNameAddressFormatted()
		{
			AssertEquals("When Principal is null", ZString.Empty, Wrapper.HolderOfTheTransitProcedureTirNameAddressFormatted);

			ResetWrapper();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "PRINCIPAL NAME";
			var address = orgHeader.MainAddress;
			address.OA_Address1 = "ADDRESS 1";
			address.OA_Address2 = "ADDRESS 2";
			address.OA_PostCode = "D00";
			address.OA_City = "PRINCIPAL CITY";
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			var tir = orgHeader.CustomsCodes.AddNew();
			tir.OK_CodeType = Enterprise.Customs.EU.NCTS.Business.NctsPhase5DeclarationTypeList.Codes.TIR;
			tir.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Ireland;
			tir.OK_CustomsRegNo = "IRL/024/1307078";
			NctsHeader.Principal.E2_OA_Address = address.PK;
			AssertEquals(@"IEIRL/024/1307078
PRINCIPAL NAME, ADDRESS 1 ADDRESS 2, D00 PRINCIPAL CITY, IE", Wrapper.HolderOfTheTransitProcedureTirNameAddressFormatted);
		}

		public void TestHolderOfTheTransitProcedureContactPersonFormatted()
		{
			AssertEquals("When Contact info is empty", ZString.Empty, Wrapper.HolderOfTheTransitProcedureContactPersonFormatted);

			ResetWrapper();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.MainAddress;
			address.OA_Phone = "+35318428289";
			address.OA_Email = "CONTACT@EMAIL.COM";
			var principal = NctsHeader.Principal;
			principal.E2_Contact = "CONTACT";
			principal.E2_OA_Address = address.PK;
			AssertEquals(@"CONTACT +35318428289 CONTACT@EMAIL.COM", Wrapper.HolderOfTheTransitProcedureContactPersonFormatted);
		}

		public void TestRepresentativeId()
		{
			AssertEquals("When MovementHeader is null", ZString.Empty, Wrapper.RepresentativeId);

			ResetWrapper();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.MainAddress;
			var eori = orgHeader.CustomsCodes.AddNew();
			eori.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			eori.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Ireland;
			eori.OK_CustomsRegNo = "IE9394798H";
			MovementHeader.Representative.E2_OA_Address = address.PK;
			AssertEquals("IE9394798H", Wrapper.RepresentativeId);
		}

		public void TestRepresentativeStatus()
		{
			AssertEquals("When MovementHeader is null", ZString.Empty, Wrapper.RepresentativeStatus);

			ResetWrapper();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.MainAddress;
			MovementHeader.Representative.E2_OA_Address = address.PK;
			AssertEquals("Representative valid, Principal invalid", "3", Wrapper.RepresentativeStatus);

			NctsHeader.Principal.E2_OA_Address = address.PK;
			AssertEquals("Both valid and equal", "2", Wrapper.RepresentativeStatus);

			MovementHeader.Representative.E2_OA_Address = ZGuid.Empty;
			AssertEquals("Representative invalid, Principal valid", "3", Wrapper.RepresentativeStatus);

			NctsHeader.Principal.E2_OA_Address = ZGuid.Empty;
			AssertEquals("Both invalid", ZString.Empty, Wrapper.RepresentativeStatus);
		}

		public void TestRepresentativeContactPersonFormatted()
		{
			AssertEquals("When Contact info is empty", ZString.Empty, Wrapper.RepresentativeContactPersonFormatted);

			ResetWrapper();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.MainAddress;
			address.OA_Phone = "+35318428289";
			address.OA_Email = "CONTACT@EMAIL.COM";
			var representative = MovementHeader.Representative;
			representative.E2_Contact = "CONTACT";
			representative.E2_OA_Address = address.PK;
			AssertEquals(@"CONTACT +35318428289 CONTACT@EMAIL.COM", Wrapper.RepresentativeContactPersonFormatted);
		}

		public void TestLines()
		{
			AssertType<Phase5NctsDepartureCargoDescWrapperCollection>(Wrapper.Lines);
		}

		public void TestDepartureTransportMeans()
		{
			AssertEquals("When Departure transport means are empty", ZString.Empty, Wrapper.DepartureTransportMeans);

			MovementHeader.TransportTypeAtDeparture = "AB";
			MovementHeader.TransportAtDeparture = "5";
			MovementHeader.TransportCountryAtDeparture = Core.Constants.CountryCodes.Spain;
			AssertEquals($"AB 5 {Core.Constants.CountryCodes.Spain}", Wrapper.DepartureTransportMeans);
		}

		public void TestCountryOfRoutingOfConsignment()
		{
			AssertEquals("When Country Of Routing Of Consignment are empty", ZString.Empty, Wrapper.CountryOfRoutingOfConsignment);
			var countryofRouting1 = NctsHeader.CountriesOfRouting.AddNew();
			countryofRouting1.CY_Order = 1;
			countryofRouting1.CY_Data = "ES";

			var countryofRouting2 = NctsHeader.CountriesOfRouting.AddNew();
			countryofRouting2.CY_Order = 3;
			countryofRouting2.CY_Data = "IT";

			AssertEquals(@"1 ES
3 IT", Wrapper.CountryOfRoutingOfConsignment);
		}

		public void TestActiveBorderTransportMeans()
		{
			AssertEquals("When Departure transport means are empty", ZString.Empty, Wrapper.ActiveBorderTransportMeans);

			MovementHeader.BM_CustomsOfficeAtBorder = "A100";
			MovementHeader.BM_ActiveBorderIdentificationType = "AC";
			MovementHeader.BM_TOLCarrierID = "ID500";
			MovementHeader.BM_RN_NKTOLCarrierNationality = Core.Constants.CountryCodes.Italy;
			AssertEquals($"A100 AC ID500 {Core.Constants.CountryCodes.Italy}", Wrapper.ActiveBorderTransportMeans);
		}

		public void TestConveyanceReferenceNumber()
		{
			AssertEquals("When Conveyance Ref No empty", ZString.Empty, Wrapper.ConveyanceRefNo);

			MovementHeader.BM_ConveyanceNumber = "81";
			AssertEquals("81", Wrapper.ConveyanceRefNo);
		}

		public void TestPlaceOfLoading()
		{
			AssertEquals("When Place of Loading are empty", ZString.Empty, Wrapper.PlaceOfLoading);

			MovementHeader.BM_PortOfPresentationCode = "COD:Y";
			MovementHeader.BM_PlaceOfLoading = "loading_location";
			AssertEquals(@"UNLOCODE: COD:Y", Wrapper.PlaceOfLoading);
			MovementHeader.BM_PortOfPresentationCode = Core.Constants.CountryCodes.Ireland;
			AssertEquals(@"COUNTRY: IE
LOCATION: loading_location", Wrapper.PlaceOfLoading);
		}

		public void TestPlaceOfUnloading()
		{
			AssertEquals("When Place of Unloading are empty", ZString.Empty, Wrapper.PlaceOfUnloading);

			MovementHeader.BM_ForeignDestPortKCode = "COD:X";
			MovementHeader.BM_PlaceOfUnloading = "unloadinglocation";
			AssertEquals(@"UNLOCODE: COD:X", Wrapper.PlaceOfUnloading);
			MovementHeader.BM_ForeignDestPortKCode = Core.Constants.CountryCodes.India;
			AssertEquals(@"COUNTRY: IN
LOCATION: unloadinglocation", Wrapper.PlaceOfUnloading);
		}

		public void TestPreviousDocument()
		{
			AssertEquals("When Previous Document are empty", ZString.Empty, Wrapper.PreviousDocument);

			var prevDoc = NctsHeader.PreviousDocuments.AddNew();
			prevDoc.CSI_Code = "ABC";
			prevDoc.CSI_ReferenceNumber = "1101";
			prevDoc.CSI_ReferenceNumber2 = "Additional-info";
			AssertEquals(@"1: ABC 1101 Additional-info", Wrapper.PreviousDocument);
		}

		public void TestSupportingDocument()
		{
			AssertEquals("When Supporting Document are empty", ZString.Empty, Wrapper.SupportingDocument);

			var supDoc = NctsHeader.MovementHeader.SupportingDocuments.AddNew();
			supDoc.CSI_Code = "ABD";
			supDoc.CSI_ReferenceNumber = "1102";
			supDoc.CSI_ReferenceNumber2 = "Additional-info-2";

			var supDoc2 = NctsHeader.MovementHeader.SupportingDocuments.AddNew();
			supDoc2.CSI_Code = "ACD";
			supDoc2.CSI_ReferenceNumber = "1202";
			supDoc2.CSI_ReferenceNumber2 = "Additional_info_2";
			AssertEquals(@"1: ABD 1102 Additional-info-2
2: ACD 1202 Additional_info_2", Wrapper.SupportingDocument);
		}

		public void TestTransportDocument()
		{
			AssertEquals("When Transport Document are empty", ZString.Empty, Wrapper.TransportDocument);

			AddAdditionalDocumentsToNctsHeader(NctsHeader);
			AssertEquals(@"1: 7002 1106
2: TR7005 1109", Wrapper.TransportDocument);
		}

		public void TestAdditionalReference()
		{
			AssertEquals("When Transport Document are empty", ZString.Empty, Wrapper.AdditionalReference);

			AddAdditionalDocumentsToNctsHeader(NctsHeader);
			AssertEquals(@"1: 7000 1104
2: RE7004 1108", Wrapper.AdditionalReference);
		}

		public void TestAdditionalInformation()
		{
			AssertEquals("When Transport Document are empty", ZString.Empty, Wrapper.AdditionalInfo);

			AddAdditionalDocumentsToNctsHeader(NctsHeader);
			AssertEquals(@"1: 7001 1105
2: IN7003 1107", Wrapper.AdditionalInfo);
		}

		public void TestTransportCharges()
		{
			AssertEquals("When Transport Charges are empty", ZString.Empty, Wrapper.TransportCharges);

			ResetWrapper();
			var bill = NctsHeader.Bills.FirstOrDefault() ?? NctsHeader.Bills.AddNew();
			bill.B0_TransportPaymentMethod = "100";
			AssertEquals("100", Wrapper.TransportCharges);
		}

		static void AddAdditionalDocumentsToNctsHeader(NctsHeader header)
		{
			header.AdditionalDocuments.RemoveAll();

			var addDoc = header.AdditionalDocuments.AddNew();
			addDoc.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			addDoc.CSI_Code = "7000";
			addDoc.CSI_ReferenceNumber = "1104";
			addDoc.CSI_ReferenceNumber2 = "2222";

			var addDoc2 = header.AdditionalDocuments.AddNew();
			addDoc2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			addDoc2.CSI_Code = "7001";
			addDoc2.CSI_ReferenceNumber = "1105";
			addDoc2.CSI_ReferenceNumber2 = "2222";

			var addDoc3 = header.AdditionalDocuments.AddNew();
			addDoc3.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			addDoc3.CSI_Code = "7002";
			addDoc3.CSI_ReferenceNumber = "1106";
			addDoc3.CSI_ReferenceNumber2 = "2222";

			var addDoc4 = header.AdditionalDocuments.AddNew();
			addDoc4.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			addDoc4.CSI_Code = "IN7003";
			addDoc4.CSI_ReferenceNumber = "1107";
			addDoc4.CSI_ReferenceNumber2 = "2222";

			var addDoc5 = header.AdditionalDocuments.AddNew();
			addDoc5.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			addDoc5.CSI_Code = "RE7004";
			addDoc5.CSI_ReferenceNumber = "1108";
			addDoc5.CSI_ReferenceNumber2 = "2222";

			var addDoc6 = header.AdditionalDocuments.AddNew();
			addDoc6.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			addDoc6.CSI_Code = "TR7005";
			addDoc6.CSI_ReferenceNumber = "1109";
			addDoc6.CSI_ReferenceNumber2 = "2222";
		}

		static void AddContainersWithSealsAndItems(NctsHeader nctsHeader)
		{
			var container1 = nctsHeader.DepartureHeaderContainers.AddNew();
			container1.BC_ContainerNum = "730BEC62";
			container1.Seal1 = "4240179DB205";
			container1.Seal2 = "DA346891";
			container1.AdditionalSeals.AddNew().BK_SealNumber = "E8B4A2622590";
			container1.AdditionalSeals.AddNew().BK_SealNumber = "BA42FEB3";

			var container2 = nctsHeader.DepartureHeaderContainers.AddNew();
			container2.BC_ContainerNum = "437514E909E7";
			container2.BC_Seal1 = "857067C90FDF";
			container2.BC_Seal2 = "DA6541519BC9";

			var bill1 = nctsHeader.Bills.FirstOrDefault() ?? nctsHeader.Bills.AddNew();
			var item1 = bill1.GoodsItems.AddNew();
			item1.BY_DeclarationGoodsItemNumber = 1;
			var package1 = item1.Packages.AddNew();
			var containerPivot1 = package1.ContainersPivot.AddNew();
			containerPivot1.XX_Relation1TableCode = "B5"; // CusInvPack
			containerPivot1.XX_Relation1ID = package1.PK;
			containerPivot1.XX_Relation2TableCode = "BC"; // CusInBondContainer
			containerPivot1.XX_Relation2ID = container1.PK;

			var bill2 = nctsHeader.Bills.AddNew();
			var item2 = bill2.GoodsItems.AddNew();
			item2.BY_DeclarationGoodsItemNumber = 2;
			var package2 = item2.Packages.AddNew();
			var containerPivot2 = package2.ContainersPivot.AddNew();
			containerPivot2.XX_Relation1TableCode = "B5";
			containerPivot2.XX_Relation1ID = package2.PK;
			containerPivot2.XX_Relation2TableCode = "BC";
			containerPivot2.XX_Relation2ID = container2.PK;
			var item3 = bill2.GoodsItems.AddNew();
			item3.BY_DeclarationGoodsItemNumber = 3;
			var package3 = item3.Packages.AddNew();
			var containerPivot3 = package3.ContainersPivot.AddNew();
			containerPivot3.XX_Relation1TableCode = "B5";
			containerPivot3.XX_Relation1ID = package3.PK;
			containerPivot3.XX_Relation2TableCode = "BC";
			containerPivot3.XX_Relation2ID = container2.PK;
		}

		static void AddContainersWithSeals(NctsHeader nctsHeader)
		{
			var container1 = nctsHeader.DepartureHeaderContainers.AddNew();
			container1.BC_ContainerNum = "730BEC62";
			container1.Seal1 = "4240179DB205";
			container1.Seal2 = "DA346891";
			container1.AdditionalSeals.AddNew().BK_SealNumber = "E8B4A2622590";
			container1.AdditionalSeals.AddNew().BK_SealNumber = "BA42FEB3";

			var container2 = nctsHeader.DepartureHeaderContainers.AddNew();
			container2.BC_ContainerNum = "437514E909E7";
			container2.BC_Seal1 = "857067C90FDF";
			container2.BC_Seal2 = "DA6541519BC9";
		}

		public void TestTotalItems()
		{
			var bill1 = NctsHeader.Bills.FirstOrDefault() ?? NctsHeader.Bills.AddNew();
			var item1 = bill1.GoodsItems.AddNew();
			item1.BY_DeclarationGoodsItemNumber = 1;

			var bill2 = NctsHeader.Bills.AddNew();
			var item2 = bill2.GoodsItems.AddNew();
			item2.BY_DeclarationGoodsItemNumber = 2;

			var totalItems = Wrapper.TotalItems;

			AssertEquals("TotalItems", "2", totalItems);
		}

		public void TestTotalPackages()
		{
			var bill1 = NctsHeader.Bills.FirstOrDefault() ?? NctsHeader.Bills.AddNew();
			var item1 = bill1.GoodsItems.AddNew();
			item1.BY_DeclarationGoodsItemNumber = 1;
			var package1 = item1.Packages.AddNew();

			var bill2 = NctsHeader.Bills.AddNew();
			var item2 = bill2.GoodsItems.AddNew();
			item2.BY_DeclarationGoodsItemNumber = 2;
			var package2 = item2.Packages.AddNew();

			var totalPackages = Wrapper.TotalPackages;

			AssertEquals("TotalPackages", "2", totalPackages);
		}

		NctsHeader NctsHeader
		{
			get
			{
				if (nctsHeaderCache == null)
				{
					nctsHeaderCache = Factory.New<NctsHeader>();
					nctsHeaderCache.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
					var movementHeader = Factory.New<NctsDepartureMovementHeader>();
					movementHeader.BM_BH = NctsHeader.PK;
					nctsHeaderCache.SetMovementType(NctsMovementType.Codes.Departure);
				}
				return nctsHeaderCache;
			}
		}
		NctsHeader nctsHeaderCache;

		NctsDepartureMovementHeader MovementHeader => NctsHeader.MovementHeader;

		Phase5NctsHeaderDocumentWrapper Wrapper => wrapper ??= Phase5NctsHeaderDocumentWrapper.New(NctsHeader, Factory);
		Phase5NctsHeaderDocumentWrapper wrapper;

		void ResetWrapper() => wrapper = null;

		void ResetAll()
		{
			ResetWrapper();
			nctsHeaderCache = null;
		}

		protected override BusinessObject GetNewBusinessObject() => Wrapper;

		IDisposable phase5Functionality;
		IDisposable temporarilyCurrentCountry;

		protected override void SetUp()
		{
			base.SetUp();

			temporarilyCurrentCountry = GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland);
			phase5Functionality = ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(
				Constants.FunctionalityTypes.NCTSTransitionPeriod,
				RefDataGroupingCodes.EuropeanUnionEUN,
				ZDate.Today,
				false
			);
		}

		protected override void TearDown()
		{
			phase5Functionality?.Dispose();
			temporarilyCurrentCountry?.Dispose();
			base.TearDown();
		}
	}
}
