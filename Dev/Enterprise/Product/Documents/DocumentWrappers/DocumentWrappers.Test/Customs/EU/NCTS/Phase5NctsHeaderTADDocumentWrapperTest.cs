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
	[TestedType(typeof(Phase5NctsHeaderTADDocumentWrapper))]
	sealed class Phase5NctsHeaderTADDocumentWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMovementReferenceNumber()
		{
			var entryNum = CusEntryNumber.LoadOrCreate(NctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, NctsHeader.CountryCode);
			entryNum.CE_EntryNum = "21FR00007411BBC885";
			AssertEquals("MRN", "21FR00007411BBC885", Wrapper.MovementReferenceNumber);
		}

		public void TestLocalReferenceNumber() => CombineAssertions(() =>
		{
			NctsHeader.MovementHeader.BM_PaperlessInbondNum = ZString.Empty;
			AssertEquals("LRN", ZString.Empty, Wrapper.LocalReferenceNumber);

			NctsHeader.MovementHeader.BM_PaperlessInbondNum = "BH_JOBREFERENCE";
			AssertEquals("LRN", "BH_JOBREFERENCE", Wrapper.LocalReferenceNumber);
		});

		public void TestDeclarationType()
		{
			NctsHeader.MovementHeader.BM_InBondEntryType = "T1";
			AssertEquals("Declaration type", "T1", Wrapper.DeclarationType);
		}

		public void TestDeclAdditionalType()
		{
			NctsHeader.MovementHeader.BM_AdditionalDeclarationType = "A";
			AssertEquals("Additional type", "A", Wrapper.DeclAdditionalType);
		}

		public void TestSecurity() => CombineAssertions(() =>
		{
			NctsHeader.MovementHeader.BM_TypeOfSecurity = ZString.Empty;
			AssertEquals("Type of security", ZString.Empty, Wrapper.Security);
			NctsHeader.MovementHeader.BM_TypeOfSecurity = "NON";
			AssertEquals("Type of security", "0", Wrapper.Security);
			NctsHeader.MovementHeader.BM_TypeOfSecurity = "ENT";
			AssertEquals("Type of security", "1", Wrapper.Security);
			NctsHeader.MovementHeader.BM_TypeOfSecurity = "EXI";
			AssertEquals("Type of security", "2", Wrapper.Security);
			NctsHeader.MovementHeader.BM_TypeOfSecurity = "BTH";
			AssertEquals("Type of security", "3", Wrapper.Security);
			NctsHeader.MovementHeader.BM_TypeOfSecurity = "UNK";
			AssertEquals("Type of security", ZString.Empty, Wrapper.Security);
		});

		public void TestConsignorId() => CombineAssertions(() =>
		{
			SetupNctsHeaderAddress(NctsHeader.Consignor);
			AssertEquals("Consignor Id", "ES654321", Wrapper.ConsignorId);
			NctsHeader.Consignor.Organisation.CustomsCodes[0].OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedKingdom;
			NctsHeader.Consignor.Organisation.CustomsCodes[0].OK_CodeType = OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU;
			AssertEquals("Consignor Id", "GB654321", Wrapper.ConsignorId);
		});

		public void TestConsignorAddress() => AssertAddressMultiLine(NctsHeader.Consignor, () => Wrapper.ConsignorAddress);

		public void TestConsignorContact() => CombineAssertions(() => AssertAddressContact(NctsHeader.Consignor, () => Wrapper.ConsignorContact));

		public void TestConsigneeId() => CombineAssertions(() =>
		{
			SetupNctsHeaderAddress(NctsHeader.Consignee);
			AssertEquals("Consignee Id", "ES654321", Wrapper.ConsigneeId);
			NctsHeader.Consignee.Organisation.CustomsCodes[0].OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedKingdom;
			NctsHeader.Consignee.Organisation.CustomsCodes[0].OK_CodeType = OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU;
			AssertEquals("Consignee Id", "GB654321", Wrapper.ConsigneeId);
		});

		public void TestConsigneeAddress() => CombineAssertions(() =>
		{
			SetupNctsHeaderAddress(NctsHeader.Consignee);

			var expected = "Organisation Company LTD.";
			AssertEquals("Not overridden", expected, Wrapper.ConsigneeAddress);

			NctsHeader.Consignee.E2_AddressOverride = ZBool.True;
			NctsHeader.Consignee.E2_CompanyName = "E2 Company";

			expected = "E2 Company";
			AssertEquals("Overridden", expected, Wrapper.ConsigneeAddress);
		});

		public void TestHolderOfTheTransitProcedureId() => CombineAssertions(() =>
		{
			SetupNctsHeaderAddress(NctsHeader.Principal);
			AssertEquals("Holder of transit procedure Id", "ES654321", Wrapper.HolderOfTheTransitProcedureId);
			NctsHeader.Principal.Organisation.CustomsCodes[0].OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedKingdom;
			NctsHeader.Principal.Organisation.CustomsCodes[0].OK_CodeType = OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU;
			AssertEquals("Holder of transit procedure Id", "GB654321", Wrapper.HolderOfTheTransitProcedureId);
		});

		public void TestRepresentativeContact() => CombineAssertions(() => AssertAddressContact(NctsHeader.MovementHeader.Representative, () => Wrapper.RepresentativeContact));

		public void TestCarrierId() => CombineAssertions(() =>
		{
			SetupNctsHeaderAddress(NctsHeader.MovementHeader.Carrier);
			AssertEquals("Carrier Id", "ES654321", Wrapper.CarrierId);
			NctsHeader.MovementHeader.Carrier.Organisation.CustomsCodes[0].OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedKingdom;
			NctsHeader.MovementHeader.Carrier.Organisation.CustomsCodes[0].OK_CodeType = OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU;
			AssertEquals("Carrier Id", "GB654321", Wrapper.CarrierId);
		});

		public void TestCarrierAddress()
		{
			SetupNctsHeaderAddress(NctsHeader.MovementHeader.Carrier);
			var expected = "Organisation Company LTD.";
			AssertEquals("Carrier Address", expected, Wrapper.CarrierAddress);
		}

		public void TestCarrierContact()
		{
			SetupNctsHeaderAddress(NctsHeader.MovementHeader.Carrier);
			var expected = "Organisation CUS Contact +12 34547850 cus-contact@test.com";
			AssertEquals("Carrier contact", expected, Wrapper.CarrierContact);
		}

		public void TestHolderOfTheTransitProcedureAddress() => AssertAddressMultiLine(NctsHeader.Principal, () => Wrapper.HolderOfTheTransitProcedureAddress);

		public void TestHolderOfTheTransitProcedureContact() => CombineAssertions(() =>
		{
			AssertAddressContact(NctsHeader.Principal, () => Wrapper.HolderOfTheTransitProcedureContact);

			NctsHeader.Principal.E2_AddressOverride = ZBool.False;
			NctsHeader.Principal.E2_Contact = "Mario";
			var expected = "Mario  ";
			AssertEquals("With contact, not overridden", expected, Wrapper.HolderOfTheTransitProcedureContact);

			SetupNctsHeaderAddress(NctsHeader.MovementHeader.Representative);
			AssertEquals("Representative not null", ZString.Empty, Wrapper.HolderOfTheTransitProcedureContact);
		});

		public void TestRepresentativeId() => CombineAssertions(() =>
		{
			SetupNctsHeaderAddress(NctsHeader.MovementHeader.Representative);
			AssertEquals("Representative Id", "ES654321", Wrapper.RepresentativeId);
			NctsHeader.MovementHeader.Representative.Organisation.CustomsCodes[0].OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedKingdom;
			NctsHeader.MovementHeader.Representative.Organisation.CustomsCodes[0].OK_CodeType = OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU;
			AssertEquals("Representative Id", "GB654321", Wrapper.RepresentativeId);
		});

		public void TestRepresentativeAddress() => CombineAssertions(() =>
		{
			SetupNctsHeaderAddress(NctsHeader.MovementHeader.Representative);

			var expected = "Organisation Company LTD.";
			AssertEquals("Not overridden", expected, Wrapper.RepresentativeAddress);

			NctsHeader.MovementHeader.Representative.E2_AddressOverride = ZBool.True;
			NctsHeader.MovementHeader.Representative.E2_CompanyName = "E2 Company";

			expected = "E2 Company";
			AssertEquals("Overridden", expected, Wrapper.RepresentativeAddress);
		});

		public void TestTIRCarnetNumber() => CombineAssertions(() =>
		{
			AssertEquals("When MovementHeader is null", ZString.Empty, Wrapper.TIRCarnetNumber);
			MovementHeader.TirCarnetNumber = "TIR001";
			AssertEquals("TIR001", Wrapper.TIRCarnetNumber);
		});

		public void TestUniqueConsignmentReference()
		{
			NctsHeader.MovementHeader.BM_UniqueConsignmentReference = "123UCR";
			AssertEquals("UCR", "123UCR", Wrapper.UniqueConsignmentReference);
		}

		public void TestPlaceOfLoading() => CombineAssertions(() =>
		{
			AssertEquals("PortOfPresentationCode and PlaceOfLoading are empty", ZString.Empty, Wrapper.PlaceOfLoading);

			MovementHeader.BM_PortOfPresentationCode = "CH123";
			MovementHeader.BM_PlaceOfLoading = "loading_location";
			AssertEquals("PortOfPresentationCode length > 2", @"CH123,CH,loading_location", Wrapper.PlaceOfLoading);
			MovementHeader.BM_PortOfPresentationCode = Core.Constants.CountryCodes.Ireland;
			AssertEquals("PortOfPresentationCode length == 2", @"IE,loading_location", Wrapper.PlaceOfLoading);
			MovementHeader.BM_PortOfPresentationCode = ZString.Empty;
			AssertEquals("PortOfPresentationCode empty", @"loading_location", Wrapper.PlaceOfLoading);
		});

		public void TestPlaceOfUnloading() => CombineAssertions(() =>
		{
			AssertEquals("When Place of Unloading are empty", ZString.Empty, Wrapper.PlaceOfUnloading);

			MovementHeader.BM_ForeignDestPortKCode = "CH123";
			MovementHeader.BM_PlaceOfUnloading = "unloadinglocation";
			AssertEquals("ForeignDestPortKCode length > 2", @"CH123,CH,unloadinglocation", Wrapper.PlaceOfUnloading);
			MovementHeader.BM_ForeignDestPortKCode = Core.Constants.CountryCodes.Ireland;
			AssertEquals("ForeignDestPortKCode length == 2", @"IE,unloadinglocation", Wrapper.PlaceOfUnloading);
			MovementHeader.BM_ForeignDestPortKCode = ZString.Empty;
			AssertEquals("ForeignDestPortKCode empty", @"unloadinglocation", Wrapper.PlaceOfUnloading);
		});

		public delegate ZString AddressSelectionDelegate();
		void AssertAddressMultiLine(JobDocAddress addressObject, AddressSelectionDelegate addressSelection) => CombineAssertions(() =>
		{
			SetupNctsHeaderAddress(addressObject);

			var expected = @"Organisation Company LTD.
Address line 1 Address line 2
1234 CITY 1 ES";
			AssertEquals("Not overridden", expected, addressSelection());

			addressObject.E2_AddressOverride = ZBool.True;
			addressObject.E2_CompanyName = "E2 Company";
			addressObject.E2_Address1 = "Address line 3";
			addressObject.E2_Address2 = "Address line 4";
			addressObject.E2_Postcode = "5678";
			addressObject.E2_City = "CITY 2";
			addressObject.E2_RN_NKCountryCode = "FR";

			expected = @"E2 Company
Address line 3 Address line 4
5678 CITY 2 FR";
			AssertEquals("Overridden", expected, addressSelection());
		});

		public delegate ZString ContactSelectionDelegate();
		void AssertAddressContact(JobDocAddress address, ContactSelectionDelegate contactSelection)
		{
			SetupNctsHeaderAddress(address);
			var expected = "Organisation CUS Contact +12 34547850 cus-contact@test.com";
			AssertEquals("Not overridden", expected, contactSelection());

			address.E2_AddressOverride = ZBool.True;
			address.E2_Contact = "Consignor Override contact";
			address.E2_Phone = "+15 34547851";
			address.E2_Email = "cus-contact-override@consignor.com";

			expected = "Consignor Override contact +15 34547851 cus-contact-override@consignor.com";
			AssertEquals("Overridden", expected, contactSelection());
		}

		public void TestModeOfTransportAtTheBorder()
		{
			MovementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
			AssertEquals("ModeOfTransportAtTheBorder", "8", Wrapper.ModeOfTransportAtTheBorder);
		}

		public void TestInlandModeOfTransport()
		{
			MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
			AssertEquals("InlandModeOfTransport", "3", Wrapper.InlandModeOfTransport);
		}

		public void TestLocationOfGoods() => CombineAssertions(() =>
		{
			var goodsLocation = MovementHeader.GoodsLocation;

			goodsLocation.CGL_Type = "A";

			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			goodsLocation.Address.AuthorisationNumber = "5685C67D8B97";
			goodsLocation.Unlocode = "AT000000";

			var expected = @"A - Y
5685C67D8B97
AT000000";
			AssertEquals("Qualifier = Y", expected, Wrapper.LocationOfGoods);

			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
			expected = @"A - U";
			AssertEquals("Qualifier = U", expected, Wrapper.LocationOfGoods);

			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
			goodsLocation.CGL_CustomsOffice = "IEROS100";
			expected = @"A - V
IEROS100";
			AssertEquals("Qualifier = V", expected, Wrapper.LocationOfGoods);

			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
			goodsLocation.Address.E2_GovRegNum = "EORI1234";
			expected = @"A - X
EORI1234";
			AssertEquals("Qualifier = X", expected, Wrapper.LocationOfGoods);

			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.PostcodeAddress;
			goodsLocation.AdditionalIdentifier = "House 1";
			goodsLocation.Address.E2_Postcode = "POSTCODE";
			goodsLocation.Address.E2_RN_NKCountryCode = "IE";
			expected = @"A - T
House 1
POSTCODE
IE";
			AssertEquals("Qualifier = T", expected, Wrapper.LocationOfGoods);

			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
			goodsLocation.Address.E2_Address1AndE2_Address2 = "Street + Number";
			goodsLocation.Address.E2_City = "City";
			goodsLocation.Address.E2_Postcode = "POSTCODE";
			goodsLocation.Address.E2_RN_NKCountryCode = "IE";
			expected = @"A - Z
Street + Number
City
POSTCODE
IE";
			AssertEquals("Qualifier = Z", expected, Wrapper.LocationOfGoods);

			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.GnssCoordinates;
			goodsLocation.Address.E2_Latitude = 53.123;
			goodsLocation.Address.E2_Longitude = -7.5432;
			expected = @"A - W
53.123
-7.5432";
			AssertEquals("Qualifier = W", expected, Wrapper.LocationOfGoods);
		});

		public void TestLocationOfGoodsContactPerson()
		{
			var goodsLocation = MovementHeader.GoodsLocation;
			goodsLocation.CGL_Type = "A";
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			var goodsLocationAddress = goodsLocation.Address;
			goodsLocationAddress.E2_Contact = "LocationOfGoods CUS Contact";
			goodsLocationAddress.E2_Phone = "+12 34567890";
			goodsLocationAddress.E2_Email = "cus-contact@location.com";

			var expected = @"LocationOfGoods CUS Contact
+12 34567890
cus-contact@location.com";
			AssertEquals("LocationOfGoodsContactPerson", expected, Wrapper.LocationOfGoodsContactPerson);
		}

		public void TestDepartureTransportMeansWhenModeRail() => CombineAssertions(() =>
		{
			MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;
			MovementHeader.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._21;
			MovementHeader.TransportAtDeparture = "5";
			MovementHeader.TransportCountryAtDeparture = Core.Constants.CountryCodes.Spain;
			var wagon1 = MovementHeader.AdditionalWagons.AddNew();
			wagon1.WagonNationality = Core.Constants.CountryCodes.Italy;
			wagon1.WagonNumber = "609";
			var wagon2 = MovementHeader.AdditionalWagons.AddNew();
			wagon2.WagonNationality = Core.Constants.CountryCodes.France;
			wagon2.WagonNumber = "127";
			var wagon3 = MovementHeader.AdditionalWagons.AddNew();
			wagon3.WagonNationality = Core.Constants.CountryCodes.Spain;
			wagon3.WagonNumber = "3882";

			AssertEquals($"1,{NctsTransportTypeOfIdList.Codes._21},5,{Core.Constants.CountryCodes.Spain}; 2,{NctsTransportTypeOfIdList.Codes._20},609,{Core.Constants.CountryCodes.Italy}; 3,{NctsTransportTypeOfIdList.Codes._20},127,{Core.Constants.CountryCodes.France}; 4,{NctsTransportTypeOfIdList.Codes._20},3882,{Core.Constants.CountryCodes.Spain}", Wrapper.DepartureTransportMeans);
		});

		public void TestDepartureTransportMeansWhenModeRoad() => CombineAssertions(() =>
		{
			MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
			MovementHeader.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._30;
			MovementHeader.TransportAtDeparture = "5";
			MovementHeader.TransportCountryAtDeparture = Core.Constants.CountryCodes.Spain;
			MovementHeader.Trailer1IDAtDeparture = "609";
			MovementHeader.Trailer1NationalityAtDeparture = Core.Constants.CountryCodes.Italy;

			AssertEquals($"1,{NctsTransportTypeOfIdList.Codes._30},5,{Core.Constants.CountryCodes.Spain}; 2,{NctsTransportTypeOfIdList.Codes._31},609,{Core.Constants.CountryCodes.Italy}", Wrapper.DepartureTransportMeans);

			MovementHeader.Trailer2IDAtDeparture = "127";
			MovementHeader.Trailer2NationalityAtDeparture = Core.Constants.CountryCodes.France;

			AssertEquals($"1,{NctsTransportTypeOfIdList.Codes._30},5,{Core.Constants.CountryCodes.Spain}; 2,{NctsTransportTypeOfIdList.Codes._31},609,{Core.Constants.CountryCodes.Italy}; 3,{NctsTransportTypeOfIdList.Codes._31},127,{Core.Constants.CountryCodes.France}", Wrapper.DepartureTransportMeans);
		});

		public void TestDepartureTransportMeans() => CombineAssertions(() =>
		{
			AssertEquals("When Departure transport means are empty", ZString.Empty, Wrapper.DepartureTransportMeans);

			MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
			MovementHeader.TransportTypeAtDeparture = "AB";
			MovementHeader.TransportAtDeparture = "5";
			MovementHeader.TransportCountryAtDeparture = Core.Constants.CountryCodes.Spain;
			AssertEquals($"1,AB,5,{Core.Constants.CountryCodes.Spain}", Wrapper.DepartureTransportMeans);
		});

		public void TestSeal()
		{
			AddContainersWithSeals(NctsHeader);
			var expected = @"1/4240179DB205,DA346891,E8B4A2622590,BA42FEB3; 2/857067C90FDF,DA6541519BC9; 3/4575547C90FDF,PA6737519BC9";
			AssertEquals("Seal", expected, Wrapper.Seal);
		}

		public void TestAdditionalSupplyChain()
		{
			var act1 = MovementHeader.CusSupplyChainActors.AddNew();
			act1.CFR_Code = SupplyChainActorRoleList.Codes.WH;
			act1.CFR_Reference = "74ADD69ECD74";
			var act2 = MovementHeader.CusSupplyChainActors.AddNew();
			act2.CFR_Code = SupplyChainActorRoleList.Codes.FW;
			act2.CFR_Reference = "1FCFA7B902198D3FFB5E";
			var act3 = MovementHeader.CusSupplyChainActors.AddNew();
			act3.CFR_Code = SupplyChainActorRoleList.Codes.CS;
			act3.CFR_Reference = "1FCFA7B9B2198D3FFF5E";
			var act4 = MovementHeader.CusSupplyChainActors.AddNew();
			act4.CFR_Code = SupplyChainActorRoleList.Codes.MF;
			act4.CFR_Reference = "197CB9B2198D3FFF5E";
			var act5 = MovementHeader.CusSupplyChainActors.AddNew();
			act5.CFR_Code = SupplyChainActorRoleList.Codes.WH;
			act5.CFR_Reference = "A1B2C3D4E5F6G7H8";
			var act6 = MovementHeader.CusSupplyChainActors.AddNew();
			act6.CFR_Code = SupplyChainActorRoleList.Codes.FW;
			act6.CFR_Reference = "B2C3D4E5F6G7H8I9J";
			var act7 = MovementHeader.CusSupplyChainActors.AddNew();
			act7.CFR_Code = SupplyChainActorRoleList.Codes.CS;
			act7.CFR_Reference = "C3D4E5F6G7H8I9J0K1L";
			var act8 = MovementHeader.CusSupplyChainActors.AddNew();
			act8.CFR_Code = SupplyChainActorRoleList.Codes.MF;
			act8.CFR_Reference = "D4E5F6G7H8I9J0K1L2M";
			var act9 = MovementHeader.CusSupplyChainActors.AddNew();
			act9.CFR_Code = SupplyChainActorRoleList.Codes.WH;
			act9.CFR_Reference = "E5F6G7H8I9J0K1L2M3N4";

			var expected = "CS 1FCFA7B9B2198D3FFF5E; (...); WH E5F6G7H8I9J0K1L2M3N4";
			AssertEquals("AdditionalSupplyChain", expected, Wrapper.AdditionalSupplyChain);
		}

		public void TestActiveBorderTransportMeansAndConveyanceRefNo() => CombineAssertions(() =>
		{
			AssertEquals("When Departure transport means are empty - ActiveBorderTransportMeans", ZString.Empty, Wrapper.ActiveBorderTransportMeans);
			AssertEquals("When Departure transport means are empty - ConveyanceRefNo", ZString.Empty, Wrapper.ConveyanceRefNo);

			MovementHeader.BM_CustomsOfficeAtBorder = "A100";
			MovementHeader.BM_ActiveBorderIdentificationType = "AC";
			MovementHeader.BM_TOLCarrierID = "ID500";
			MovementHeader.BM_RN_NKTOLCarrierNationality = Core.Constants.CountryCodes.Italy;
			MovementHeader.BM_ConveyanceNumber = "CY8348957";
			AssertEquals("No additional transports - ActiveBorderTransportMeans", $"1,A100,AC,ID500,{Core.Constants.CountryCodes.Italy}", Wrapper.ActiveBorderTransportMeans);
			AssertEquals("No additional transports - ConveyanceRefNo", "CY8348957", Wrapper.ConveyanceRefNo);

			var addTransport = MovementHeader.AdditionalTransportAtBorderList.AddNew();
			addTransport.TPM_CustomsOffice = "A200";
			addTransport.TPM_TypeOfIdentification = "BC";
			addTransport.TPM_IdentificationNumber = "12345";
			addTransport.TPM_RN_NKTransportNationality = Core.Constants.CountryCodes.France;
			addTransport.TPM_ReferenceNumber = "9876";

			AssertEquals("1 additional transport - ActiveBorderTransportMeans", $"1,A100,AC,ID500,{Core.Constants.CountryCodes.Italy}; 2,A200,BC,12345,{Core.Constants.CountryCodes.France}", Wrapper.ActiveBorderTransportMeans);
			AssertEquals("1 additional transport - ConveyanceRefNo", "CY8348957; 9876", Wrapper.ConveyanceRefNo);

			addTransport = MovementHeader.AdditionalTransportAtBorderList.AddNew();
			addTransport.TPM_CustomsOffice = "A201";
			addTransport.TPM_TypeOfIdentification = "BD";
			addTransport.TPM_IdentificationNumber = "12346";
			addTransport.TPM_RN_NKTransportNationality = Core.Constants.CountryCodes.Portugal;
			addTransport.TPM_ReferenceNumber = "9875";

			AssertEquals("2 additional transports - ActiveBorderTransportMeans", $"1,A100,AC,ID500,{Core.Constants.CountryCodes.Italy}; (...)", Wrapper.ActiveBorderTransportMeans);
			AssertEquals("2 additional transports - ConveyanceRefNo", "CY8348957; 9876; 9875", Wrapper.ConveyanceRefNo);
		});

		public void TestTransportEquipment()
		{
			AddContainersWithSealsAndItems(NctsHeader);

			var expected = @"1/730BEC62/4/1; 2/437514E909E7/2/2-3";

			AssertEquals("TransportEquipment", expected, Wrapper.TransportEquipment);
		}

		public void TestPreviousDocument() => CombineAssertions(() =>
		{
			AssertEquals("When Previous Document are empty", ZString.Empty, Wrapper.PreviousDocument);

			AddPreviousDocument("ABC", "1101", "Additional-info001");
			AddPreviousDocument("DEF", "1102", "Additional-info002");
			AddPreviousDocument("GHJ", "1103", "Additional-info003");
			AddPreviousDocument("KLM", "1104", "Additional-info004");
			AddPreviousDocument("NOP", "1105", "Additional-info005");
			AddPreviousDocument("QRS", "1106", "Additional-info006");
			AddPreviousDocument("TUV", "1107", "Additional-info007");
			AddPreviousDocument("XYZ", "1108", "Additional-info008");

			AssertEquals(@"1: ABC,1101,Additional-info001; 2: DEF,1102,Additional-info002; 3: GHJ,1103,Additional-info003; 4: KLM,1104,Additional-info004; 5: NOP,1105,Additional-info005; (...); 8: XYZ,1108,Additional-info008", Wrapper.PreviousDocument);

			void AddPreviousDocument(ZString csiCode, ZString refNumber, ZString refNumber2)
			{
				var prevDoc = NctsHeader.PreviousDocuments.AddNew();
				prevDoc.CSI_Code = csiCode;
				prevDoc.CSI_ReferenceNumber = refNumber;
				prevDoc.CSI_ReferenceNumber2 = refNumber2;
			}
		});

		public void TestSupportingDocument() => CombineAssertions(() =>
		{
			AssertEquals("When Supporting Document are empty", ZString.Empty, Wrapper.SupportingDocument);

			AddSupportingDocument("ABC", "1101", "Additional-info001");
			AddSupportingDocument("DEF", "1102", "Additional-info002");
			AddSupportingDocument("GHJ", "1103", "Additional-info003");
			AddSupportingDocument("KLM", "1104", "Additional-info004");
			AddSupportingDocument("NOP", "1105", "Additional-info005");
			AddSupportingDocument("QRS", "1106", "Additional-info006");
			AddSupportingDocument("TUV", "1107", "Additional-info007");
			AddSupportingDocument("XYZ", "1108", "Additional-info008");

			AssertEquals(@"1: ABC,1101,Additional-info001; 2: DEF,1102,Additional-info002; 3: GHJ,1103,Additional-info003; 4: KLM,1104,Additional-info004; 5: NOP,1105,Additional-info005; (...); 8: XYZ,1108,Additional-info008", Wrapper.SupportingDocument);

			void AddSupportingDocument(ZString csiCode, ZString refNumber, ZString refNumber2)
			{
				var prevDoc = MovementHeader.SupportingDocuments.AddNew();
				prevDoc.CSI_Code = csiCode;
				prevDoc.CSI_ReferenceNumber = refNumber;
				prevDoc.CSI_ReferenceNumber2 = refNumber2;
			}
		});

		public void TestTransportDocument() => CombineAssertions(() =>
		{
			AssertEquals("When Transport Document are empty", ZString.Empty, Wrapper.TransportDocument);

			AddTransportDocument("ABC", "1101", "Additional-info001");
			AddTransportDocument("DEF", "1102", "Additional-info002");
			AddTransportDocument("GHJ", "1103", "Additional-info003");
			AddTransportDocument("KLM", "1104", "Additional-info004");
			AddTransportDocument("NOP", "1105", "Additional-info005");
			AddTransportDocument("QRS", "1106", "Additional-info006");
			AddTransportDocument("TUV", "1107", "Additional-info007");
			AddTransportDocument("XYZ", "1108", "Additional-info008");

			AssertEquals(@"1: ABC,1101; 2: DEF,1102; 3: GHJ,1103; 4: KLM,1104; 5: NOP,1105; 6: QRS,1106; 7: TUV,1107; 8: XYZ,1108", Wrapper.TransportDocument);

			void AddTransportDocument(ZString csiCode, ZString refNumber, ZString refNumber2)
			{
				var addDoc3 = NctsHeader.AdditionalDocuments.AddNew();
				addDoc3.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				addDoc3.CSI_Code = csiCode;
				addDoc3.CSI_ReferenceNumber = refNumber;
				addDoc3.CSI_ReferenceNumber2 = refNumber2;
			}
		});

		public void TestAdditionalReference() => CombineAssertions(() =>
		{
			AssertEquals("When Transport Document are empty", ZString.Empty, Wrapper.AdditionalReference);

			AddAdditionalDocumentsToNctsHeader(NctsHeader);
			AssertEquals(@"1: 7000,1104; 2: RE7004,1108", Wrapper.AdditionalReference);
		});

		public void TestAdditionalInformation() => CombineAssertions(() =>
		{
			AssertEquals("When Transport Document are empty", ZString.Empty, Wrapper.AdditionalInfo);

			AddAdditionalDocumentsToNctsHeader(NctsHeader);
			AssertEquals(@"1: 7001,Description 7001; 2: IN7003,Description IN7003", Wrapper.AdditionalInfo);
		});

		public void TestTransportCharges() => CombineAssertions(() =>
		{
			AssertEquals("When Transport Charges are empty", ZString.Empty, Wrapper.TransportCharges);
			ResetWrapper();
			MovementHeader.BM_MethodOfPayment = "H";
			AssertEquals("H", Wrapper.TransportCharges);
		});

		public void TestReducedDatasetIndicator() => CombineAssertions(() =>
		{
			AssertEquals("When MovementHeader is null", UncheckedCheckBox, Wrapper.ReducedDatasetIndicator);

			ResetWrapper();
			MovementHeader.BM_ReducedDatasetIndicator = true;
			AssertEquals("When BM_ReducedDatasetIndicator is true", CheckedCheckBox, Wrapper.ReducedDatasetIndicator);

			MovementHeader.BM_ReducedDatasetIndicator = false;
			AssertEquals("When BM_ReducedDatasetIndicator is false", UncheckedCheckBox, Wrapper.ReducedDatasetIndicator);
		});

		public void TestGuarantees()
		{
			AddGuarantee("198E9D11", "7324D4936DE178", "8560", "8560", 12.345, Core.Constants.CurrencyCodes.EuropeanUnion);
			AddGuarantee("BDF76B0", "D3F4E6E35F85", "122B", "B70B", 23.456, Core.Constants.CurrencyCodes.EuropeanUnion);
			AddGuarantee("6A4A1E7D", "88DCBF872C6D", "B2F8", "889F", 0, Core.Constants.CurrencyCodes.EuropeanUnion);
			AddGuarantee("F96", "14AAFE739B05", "437F", "86AB", 45.678, Core.Constants.CurrencyCodes.EuropeanUnion);
			AddGuarantee("198WFD11", "7326278936DE1", "8160", "8533", 67.901, Core.Constants.CurrencyCodes.EuropeanUnion);
			AddGuarantee("BHF73B0", "D3F4E6E67F85", "121H", "L70D", 89.456, Core.Constants.CurrencyCodes.EuropeanUnion);
			AddGuarantee("6T4A1E9D", "81JJCBF872C6D", "B2F64", "849F", 97.531, Core.Constants.CurrencyCodes.EuropeanUnion);
			AddGuarantee("F338", "14AALL39B05", "43REF", "87DB", 25.678, Core.Constants.CurrencyCodes.EuropeanUnion);
			AddGuarantee("6T531E9D", "K66541JJC72C6D", "B8T64", "8BDF", 98.225, Core.Constants.CurrencyCodes.EuropeanUnion);
			AddGuarantee("NCS38", "14AAL6GGDB05", "455DS", "47AB", 18.680, Core.Constants.CurrencyCodes.EuropeanUnion);

			var expected = "1: 198E9D11-7324D4936DE178,12.35 EUR; 2: 198WFD11-7326278936DE1,67.9 EUR; 3: 6A4A1E7D-88DCBF872C6D; 4: 6T4A1E9D-81JJCBF872C6D,97.53 EUR; (...); 10: NCS38-14AAL6GGDB05,18.68 EUR";

			AssertEquals("Guarantees", expected, Wrapper.Guarantees);

			void AddGuarantee(ZString bondType, ZString bondNumber, ZString bondNumber2, ZString password, ZDecimal amount, ZString currency)
			{
				var guarantee = NctsHeader.MovementHeader.Guarantees.AddNew();
				guarantee.PW_BondType = bondType;
				guarantee.PW_BondNumber2 = bondNumber2;
				guarantee.PW_BondNumber = bondNumber;
				guarantee.PW_Password = password;
				guarantee.PW_BondAmount = amount;
				guarantee.PW_RX_NKCurrency = currency;
			}
		}

		public void TestAuthorisations() => CombineAssertions(() =>
		{
			AssertEquals("When MovementHeader is null", ZString.Empty, Wrapper.Authorisations);

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
				var auth3 = MovementHeader.CusAuthorizationUsages.AddNew();
				auth3.AGC_Code = "CWP";
				auth3.AGC_Number = "003";
				var auth4 = MovementHeader.CusAuthorizationUsages.AddNew();
				auth4.AGC_Code = "ACT";
				auth4.AGC_Number = "004";
				AssertEquals(@"1: C517 - 001; 2: C520 - 002; 3: C517 - 003; 4: C520 - 004", Wrapper.Authorisations);
			}
		});

		public void TestCountryOfRoutingOfConsignment() => CombineAssertions(() =>
		{
			AssertEquals("When Country Of Routing Of Consignment are empty", ZString.Empty, Wrapper.CountryOfRoutingOfConsignment);
			var countryofRouting1 = NctsHeader.CountriesOfRouting.AddNew();
			countryofRouting1.CY_Order = 1;
			countryofRouting1.CY_Data = "ES";

			var countryofRouting2 = NctsHeader.CountriesOfRouting.AddNew();
			countryofRouting2.CY_Order = 3;
			countryofRouting2.CY_Data = "IT";

			AssertEquals("1 ES; 3 IT", Wrapper.CountryOfRoutingOfConsignment);
		});

		public void TestCustomsOfficesOfTransit() => CombineAssertions(() =>
		{
			AssertEquals("When MovementHeader is null", ZString.Empty, Wrapper.CustomsOfficesOfTransit);

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

			AssertEquals(@"1/ GB000080; 2/ FR000380", Wrapper.CustomsOfficesOfTransit);
		});

		public void TestCustomsOfficesOfExitForTransit() => CombineAssertions(() =>
		{
			AssertEquals("When MovementHeader is null", ZString.Empty, Wrapper.CustomsOfficesOfExitForTransit);

			ResetWrapper();
			var customsOffice = MovementHeader.CustomsOffices.AddNew();
			customsOffice.CY_Type = "EUO";
			customsOffice.CY_Code = "TXT";
			customsOffice.CY_Data = "IESNN400";
			customsOffice = MovementHeader.CustomsOffices.AddNew();
			customsOffice.CY_Type = "EUO";
			customsOffice.CY_Code = "TXT";
			customsOffice.CY_Data = "IETIV100";

			AssertEquals(@"1/ IESNN400; 2/ IETIV100", Wrapper.CustomsOfficesOfExitForTransit);
		});

		public void TestCustomsOfficeOfDeparture() => CombineAssertions(() =>
		{
			AssertEquals("When MovementHeader is null", ZString.Empty, Wrapper.CustomsOfficeOfDeparture);

			ResetWrapper();
			MovementHeader.CustomsOffices.RemoveAndDeleteAll();
			var customsOffice = MovementHeader.CustomsOffices.AddNew();
			customsOffice.CY_Type = "EUO";
			customsOffice.CY_Code = "DEP";
			customsOffice.CY_Data = "IEDUB100";
			AssertEquals("IEDUB100", Wrapper.CustomsOfficeOfDeparture);
		});

		public void TestCustomsOfficeOfDestination() => CombineAssertions(() =>
		{
			AssertEquals("When MovementHeader is null", ZString.Empty, Wrapper.CustomsOfficeOfDestination);

			ResetWrapper();
			MovementHeader.CustomsOffices.RemoveAndDeleteAll();
			var customsOffice = MovementHeader.CustomsOffices.AddNew();
			customsOffice.CY_Type = "EUO";
			customsOffice.CY_Code = "DES";
			customsOffice.CY_Data = "FR002270";
			AssertEquals("FR002270", Wrapper.CustomsOfficeOfDestination);
		});

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

		public void TestBindingItinerary() => CombineAssertions(() =>
		{
			MovementHeader.BM_TypeOfSecurity = "ENT";
			AssertEquals("When Security is ENT", CheckedCheckBox, Wrapper.BindingItinerary);

			MovementHeader.BM_TypeOfSecurity = "EXI";
			AssertEquals("When Security is EXI", CheckedCheckBox, Wrapper.BindingItinerary);

			MovementHeader.BM_TypeOfSecurity = "BTH";
			AssertEquals("When Security is BTH", CheckedCheckBox, Wrapper.BindingItinerary);

			MovementHeader.BM_TypeOfSecurity = "NON";
			AssertEquals("When Security is NON", UncheckedCheckBox, Wrapper.BindingItinerary);

			(NctsHeader.CountriesOfRouting as CountryOfRoutingCollection<CountryOfRouting>)?.AddNew();
			AssertEquals("When CountriesOfRouting has items", CheckedCheckBox, Wrapper.BindingItinerary);
		});

		public void TestTotalItems()
		{
			var bill1 = NctsHeader.Bills.FirstOrDefault() ?? NctsHeader.Bills.AddNew();
			var item1 = bill1.GoodsItems.AddNew();
			item1.BY_DeclarationGoodsItemNumber = 1;

			var bill2 = NctsHeader.Bills.AddNew();
			var item2 = bill2.GoodsItems.AddNew();
			item2.BY_DeclarationGoodsItemNumber = 2;

			AssertEquals("TotalItems", "2", Wrapper.TotalItems);
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

			AssertEquals("TotalPackages", "2", Wrapper.TotalPackages);
		}

		public void TestTotalGrossMass()
		{
			var bill = NctsHeader.Bills.FirstOrDefault() ?? NctsHeader.Bills.AddNew();
			var item1 = bill.GoodsItems.AddNew();
			item1.BY_DeclarationGoodsItemNumber = 1;
			item1.BY_GrossWeight = 153.7;
			item1.BY_GrossWeightUnit = "KG";

			var item2 = bill.GoodsItems.AddNew();
			item2.BY_DeclarationGoodsItemNumber = 2;
			item2.BY_GrossWeight = 25690;
			item2.BY_GrossWeightUnit = "G";

			MovementHeader.BM_GrossWeight = 230.6048545;

			AssertEquals("TotalPackages", "230.604855", Wrapper.TotalGrossMass);
		}

		public void TestContainerIndicator() => CombineAssertions(() =>
		{
			AddContainersWithSeals(NctsHeader);
			AssertEquals("No CNT mode", UncheckedCheckBox, Wrapper.ContainerIndicator);
			NctsHeader.DepartureHeaderContainers.First().BC_Mode = "CNT";
			AssertEquals("With CNT mode", CheckedCheckBox, Wrapper.ContainerIndicator);
		});

		public void TestLines()
		{
			AssertType<Phase5NctsTADItemWrapperCollection>(Wrapper.Lines);
		}

		public void TestDateLimit()
		{
			MovementHeader.BM_ExportDate = new ZDateTime(2024, 07, 17, 16, 30, 59);
			AssertEquals("2024-07-17", Wrapper.DateLimit);
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

		Phase5NctsHeaderTADDocumentWrapper Wrapper => wrapper ??= Phase5NctsHeaderTADDocumentWrapper.New(NctsHeader, Factory);
		Phase5NctsHeaderTADDocumentWrapper wrapper;

		void ResetWrapper() => wrapper = null;

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

		void SetupNctsHeaderAddress(JobDocAddress addressObject)
		{
			var organisationHeader = Factory.New<OrgHeader>();
			organisationHeader.OH_FullName = "Organisation Company LTD.";

			var eoriCode = organisationHeader.CustomsCodes.AddNew();
			eoriCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Spain;
			eoriCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			eoriCode.OK_CustomsRegNo = "654321";

			var address = organisationHeader.MainAddress;
			address.OA_Address1 = "Address line 1";
			address.OA_Address2 = "Address line 2";
			address.OA_PostCode = "1234";
			address.OA_City = "CITY 1";
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Spain;

			var cusContact = organisationHeader.Contacts.AddNew();
			cusContact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
			cusContact.OC_ContactName = "Organisation CUS Contact";
			cusContact.OC_Mobile = "+12 34567890";
			cusContact.OC_Phone = "+12 34547850";
			cusContact.OC_HomePhone = "+12 98765";
			cusContact.OC_Email = "cus-contact@test.com";

			addressObject.E2_OA_Address = address.PK;
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

			var container3 = nctsHeader.DepartureHeaderContainers.AddNew();
			container3.BC_ContainerNum = "346514E909E7";
			container3.BC_Seal1 = "4575547C90FDF";
			container3.BC_Seal2 = "PA6737519BC9";
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

		static void AddAdditionalDocumentsToNctsHeader(NctsHeader header)
		{
			header.AdditionalDocuments.RemoveAll();

			var addDoc = header.AdditionalDocuments.AddNew();
			addDoc.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			addDoc.CSI_Code = "7000";
			addDoc.CSI_ReferenceNumber = "1104";
			addDoc.CSI_ReferenceNumber2 = "2222";
			addDoc.CSI_Description = "Description 7000";

			var addDoc2 = header.AdditionalDocuments.AddNew();
			addDoc2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			addDoc2.CSI_Code = "7001";
			addDoc2.CSI_ReferenceNumber = "1105";
			addDoc2.CSI_ReferenceNumber2 = "2222";
			addDoc2.CSI_Description = "Description 7001";

			var addDoc3 = header.AdditionalDocuments.AddNew();
			addDoc3.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			addDoc3.CSI_Code = "7002";
			addDoc3.CSI_ReferenceNumber = "1106";
			addDoc3.CSI_ReferenceNumber2 = "2222";
			addDoc3.CSI_Description = "Description 7002";

			var addDoc4 = header.AdditionalDocuments.AddNew();
			addDoc4.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			addDoc4.CSI_Code = "IN7003";
			addDoc4.CSI_ReferenceNumber = "1107";
			addDoc4.CSI_ReferenceNumber2 = "2222";
			addDoc4.CSI_Description = "Description IN7003";

			var addDoc5 = header.AdditionalDocuments.AddNew();
			addDoc5.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			addDoc5.CSI_Code = "RE7004";
			addDoc5.CSI_ReferenceNumber = "1108";
			addDoc5.CSI_ReferenceNumber2 = "2222";
			addDoc5.CSI_Description = "Description RE7004";

			var addDoc6 = header.AdditionalDocuments.AddNew();
			addDoc6.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			addDoc6.CSI_Code = "TR7005";
			addDoc6.CSI_ReferenceNumber = "1109";
			addDoc6.CSI_ReferenceNumber2 = "2222";
			addDoc6.CSI_Description = "Description TR7005";
		}

		protected override void TearDown()
		{
			phase5Functionality?.Dispose();
			temporarilyCurrentCountry?.Dispose();
			base.TearDown();
		}

		const string UncheckedCheckBox = "\u2610";
		const string CheckedCheckBox = "\u2611";
	}
}
