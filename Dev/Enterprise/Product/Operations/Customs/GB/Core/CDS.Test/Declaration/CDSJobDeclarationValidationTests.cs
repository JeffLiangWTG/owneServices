using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CDS.MessagingRules;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.CDS.Declaration.Testing
{
	public class CDSJobDeclarationValidationTests : Business.Testing.JobDeclarationValidationTest
	{
		public void TestCheckJE_SubLocationOfGoods()
		{
			var dec = GetJobDeclarationForTest();
			dec.JE_TransportMode = TransportTypeList.Codes.Air;
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			dec.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportClearanceRequest;
			dec.ZG_Gateway = GatewayList.Codes.CCSUKviaNTMsgGW;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "FAC");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "MNCMANDHXCUK", "MNCMANDHXCUK", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			dec.JE_SubLocationOfGoods = "TST";
			AssertHasMessageError(dec.JE_SubLocationOfGoodsInfo, "This location is not a known CCSUK airport/shed code. The inventory consignment reference (MUCR) generated may lack its ACP code prefix (the leading character).");

			dec.JE_SubLocationOfGoods = "MNCMANDHXCUK";
			AssertNoMessageError(dec.JE_SubLocationOfGoodsInfo, "This location is not a known CCSUK airport/shed code. The inventory consignment reference (MUCR) generated may lack its ACP code prefix (the leading character).");

			dec.JE_SubLocationOfGoods = "TST";
			dec.ZG_Gateway = GatewayList.Codes.CDS;
			dec.Validation.ValidateJE_SubLocationOfGoods();
			AssertNoMessageError(dec.JE_SubLocationOfGoodsInfo, "This location is not a known CCSUK airport/shed code. The inventory consignment reference (MUCR) generated may lack its ACP code prefix (the leading character).");

			dec.ZG_Gateway = GatewayList.Codes.CCSUKviaNTMsgGW;
			dec.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportFullWarehouse;
			dec.Validation.ValidateJE_SubLocationOfGoods();
			AssertNoMessageError(dec.JE_SubLocationOfGoodsInfo, "This location is not a known CCSUK airport/shed code. The inventory consignment reference (MUCR) generated may lack its ACP code prefix (the leading character).");

			dec.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportClearanceRequest;
			dec.JE_MessageType = MessageTypeList.Codes.Export;
			dec.Validation.ValidateJE_SubLocationOfGoods();
			AssertNoMessageError(dec.JE_SubLocationOfGoodsInfo, "This location is not a known CCSUK airport/shed code. The inventory consignment reference (MUCR) generated may lack its ACP code prefix (the leading character).");

			dec.JE_MessageType = MessageTypeList.Codes.Import;
			dec.JE_TransportMode = TransportTypeList.Codes.Sea;
			dec.Validation.ValidateJE_SubLocationOfGoods();
			AssertNoMessageError(dec.JE_SubLocationOfGoodsInfo, "This location is not a known CCSUK airport/shed code. The inventory consignment reference (MUCR) generated may lack its ACP code prefix (the leading character).");
		}

		public override void TestCheckJE_LocationQualifier()
		{
			base.TestCheckJE_LocationQualifier();
			var dec = GetJobDeclarationForTest();
			dec.JE_LocationQualifier = Business.CodeDescriptionPairLists.LocationQualifierList.Codes.CustomsWarehouse;  // CW
			AssertHasMessageErrorContaining(dec.JE_LocationQualifierInfo, CDSJobDeclarationValidation.DoNotUseCwHere);
			dec.JE_LocationQualifier = Business.CodeDescriptionPairLists.LocationQualifierList.Codes.InwardProcessing;
			AssertNoMessageErrorContaining(dec.JE_LocationQualifierInfo, CDSJobDeclarationValidation.DoNotUseCwHere);
			dec.JE_LocationQualifier = "";
			AssertNoMessageErrorContaining(dec.JE_LocationQualifierInfo, CDSJobDeclarationValidation.DoNotUseCwHere);
			dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			dec.JE_LocationQualifier = Business.CodeDescriptionPairLists.LocationQualifierList.Codes.CustomsWarehouse;  // CW
			AssertNoMessageErrorContaining(dec.JE_LocationQualifierInfo, CDSJobDeclarationValidation.DoNotUseCwHere);
		}

		public void TestCheckJE_RouteFRequested()
		{
			var dec = GetJobDeclarationForTest();
			dec.JE_RouteFRequested = ZBool.True;
			dec.AddInfoValidation.ValidateJE_RouteFRequested();
			AssertHasMessageError(dec.JE_RouteFRequestedInfo, "FEC challenges under CDS are not blocking and therefore Route F is obsolete. Untick this box.");

			dec.JE_RouteFRequested = ZBool.False;
			dec.AddInfoValidation.ValidateJE_RouteFRequested();
			AssertNoNotifications(dec.JE_RouteFRequestedInfo);
		}

		public void TestCheckJE_EntryStyle()
		{
			var dec = GetJobDeclarationForTest();
			dec.JE_MessageType = MessageTypeList.Codes.Import;

			AssertNoErrors(dec.JE_EntryStyleInfo);
			dec.JE_DeclarationType = ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse;
			dec.JE_EntryStyle = EntryStyleListImport.Codes.ImportFromSpecialTerritory;
			AssertHasMessageErrorContaining(dec.JE_EntryStyleInfo, "CO is not allowed for declaration type H1");

			dec.JE_DeclarationType = ImportDeclarationTypeList.Codes.DeclarationForGoodsFromTheSpecialFiscalTerritories;
			dec.Validation.ValidateJE_EntryStyle();
			AssertNoErrors(dec.JE_EntryStyleInfo);

			dec.JE_EntryStyle = EntryStyleListImport.Codes.ImportNormal;
			AssertHasMessageErrorContaining(dec.JE_EntryStyleInfo, "IM is not allowed for declaration type H5");

			dec.JE_EntryStyle = EntryStyleListImport.Codes.ImportFromEFTAMember;
			AssertNoErrors(dec.JE_EntryStyleInfo);
		}

		public void TestError1886TransportCountry()
		{
			var dec1 = GetJobDeclarationForTest();
			dec1.JE_TransportMode = TransportTypeList.Codes.Sea;
			dec1.CusEntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse;
			AssertHasMessageErrors(dec1.JE_RN_NKTransportNationalityInfo);
			dec1.JE_TransportMode = TransportTypeList.Codes.Rail;
			AssertNoMessageErrors(dec1.JE_RN_NKTransportNationalityInfo);

			var dec2 = GetJobDeclarationForTest();
			dec2.JE_TransportMode = TransportTypeList.Codes.Sea;
			dec2.CusEntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse;
			AssertHasMessageErrors(dec2.JE_RN_NKTransportNationalityInfo);
			dec2.CusEntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.DeclarationForCustomsWarehousing;
			AssertNoMessageErrors(dec2.JE_RN_NKTransportNationalityInfo);

			var dec3 = GetJobDeclarationForTest();
			dec3.JE_TransportMode = TransportTypeList.Codes.Sea;
			dec3.CusEntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse;
			AssertHasMessageErrors(dec3.JE_RN_NKTransportNationalityInfo);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EXNAT, "Export Nationality");
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EXNAT, "GB", "UnitedKingdom", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			dec3.JE_RN_NKTransportNationality = Core.Constants.CountryCodes.UnitedKingdom;
			AssertNoMessageErrors(dec3.JE_RN_NKTransportNationalityInfo);
		}

		public void TestCheckJE_CustomsProfile()
		{
			var dec = GetJobDeclarationForTest();
			dec.ZG_Gateway = "CDS";
			dec.JE_TransportMode = "XXX";
			dec.JE_CustomsProfile = "AMY";

			AssertHasMessageErrorContaining(dec.JE_CustomsProfileInfo, "No company-level CDS credentials exist");

			GBCustomsDataRegistry.Instance.BadgeCodes.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new BadgeCodeSettingCollection
			{
				new BadgeCodeSetting
				{
					BadgeCode = "AMY",
					RL_PortCode = "GBLBA",
					Direction = "IMP",
					CSPCode = GatewayList.Codes.CDS,
					ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services,
				}
			});
			GBCustomsDataRegistry.Instance.Credentials.SetTemporaryValue(dec.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, new CredentialsSettingCollection
			{
				new CredentialsSetting
				{
					BadgeCode = "AMY",
					Printer = "",
					Company = "ABC",
					Username = "Username",
					Password = "Password"
				}
			});
			var password = Factory.New<GlbExternalPassword_GB>();
			password.GP_GC = dec.CompanyPK;
			password.Badge = dec.JE_CustomsProfile;
			password.EORI = dec.DeclarantTraderId;
			password.StatusMessage = "Status Message";
			password.Status = PasswordStatusList.Codes.Valid;
			password.GP_ExpiryDate = new ZDate(2015, 12, 1);
			password.GP_IssueDate = new ZDate(2014, 3, 22);

			dec.Validation.ValidateJE_CustomsProfile();

			AssertNoMessageErrorContaining(dec.JE_CustomsProfileInfo, "No company-level CDS credentials exist");
			AssertNoMessageErrorContaining(dec.JE_CustomsProfileInfo, "CDS Export Inventory Dual Running is in Phase One. Do not send an inventory-linked declaration to CDS. Instead either make this declaration non-inventory or select a CHIEF profile/badge.");

			using (Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.CDS_ILE_PHASE1, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Now, value: true))
			{
				dec.JE_MessageType = MessageTypeList.Codes.Export;
				dec.JE_MasterUCR = "UCR Value";
				dec.ZG_Gateway = GatewayList.Codes.CDS;
				dec.Validation.ValidateJE_CustomsProfile();
				AssertHasMessageErrorContaining(dec.JE_CustomsProfileInfo, "CDS Export Inventory Dual Running is in Phase One. Do not send an inventory-linked declaration to CDS. Instead either make this declaration non-inventory or select a CHIEF profile/badge.");
			}
		}

		public void TestCheckJE_CustomsProfile_PentantRequiresDUNS()
		{
			var dec = GetJobDeclarationForTest();
			const string ExpectedBadgeHasNoDuns = "Your branch does not have a DUNS account number, which is needed for communication with Pentant; please refer to 1BGB059";

			using (GBCustomsDataRegistry.Instance.BadgeCodes.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new BadgeCodeSettingCollection
			{
				new BadgeCodeSetting { BadgeCode = "DJC", CSPCode = GatewayList.Codes.Pentant, ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services }
			}))
			using (GBCustomsDataRegistry.Instance.Credentials.SetTemporaryValue(dec.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, new CredentialsSettingCollection
			{
				new CredentialsSetting { BadgeCode = "DJC", Username = "Username", Password = "Password" }
			}))
			{
				dec.JE_CustomsProfile = "DJC";
				dec.Validation.ValidateJE_CustomsProfile(); // Needed because validation is called as soon as JE_CustomsProfile is set, which occurs before cascading sets to application and gateway occur, and the Check method of the profile looks at the gateway
				AssertHasMessageError("Warning for DUNS when absent", dec.JE_CustomsProfileInfo, ExpectedBadgeHasNoDuns);

				dec.ZG_Gateway = GatewayList.Codes.MCP_CUSDECOnly;
				dec.Validation.ValidateJE_CustomsProfile();
				AssertNoMessageError("No warning for DUNS when not Pentant", dec.JE_CustomsProfileInfo, ExpectedBadgeHasNoDuns);

				dec.ZG_Gateway = GatewayList.Codes.Pentant;
				dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
				dec.Validation.ValidateJE_CustomsProfile();
				AssertNoMessageError("No warning for DUNS when Pentant but not CDS", dec.JE_CustomsProfileInfo, ExpectedBadgeHasNoDuns);

				dec.ZG_Gateway = GatewayList.Codes.Pentant;
				dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				dec.Validation.ValidateJE_CustomsProfile();
				AssertHasMessageError("Pre-req: warning again when Pentant, CDS, and no DUNS present", dec.JE_CustomsProfileInfo, ExpectedBadgeHasNoDuns);

				dec.Branch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "anything", Core.Constants.CountryCodes.UnitedKingdom);
				dec.Validation.ValidateJE_CustomsProfile();
				AssertNoMessageError("No DUNS warning when Pentant, CDS, but has DUNS on org proxy", dec.JE_CustomsProfileInfo, ExpectedBadgeHasNoDuns);
			}
		}

		public void TestCheckJE_LocationOfGoods()
		{
			var dec = GetJobDeclarationForTest();
			dec.JE_TransportMode = TransportTypeList.Codes.Air;
			dec.JE_MessageType = MessageTypeList.Codes.Import;

			dec.JE_GoodsLocation = "XXX";
			dec.Validation.ValidateAll();
			AssertNoNotifications(dec.JE_LocationOfGoodsInfo);
		}

		public void TestCheckZG_SpecificCircumstanceIndicator()
		{
			var dec = GetJobDeclarationForTest();

			dec.Validation.ValidateAll();
			AssertNoMessageErrors(dec.ZG_SpecificCircumstanceIndicatorInfo);

			dec.ZG_SpecificCircumstanceIndicator = "B";
			AssertHasMessageErrors(dec.ZG_SpecificCircumstanceIndicatorInfo);

			dec.ZG_SpecificCircumstanceIndicator = "A";
			AssertNoMessageErrors(dec.ZG_SpecificCircumstanceIndicatorInfo);
		}

		public void TestIncoTerms()
		{
			var dec = GetJobDeclarationForTest();

			dec.JE_ShipmentIncoTermPlace = "GBLondon";
			AssertNoMessageErrors(dec.JE_ShipmentIncoTermPlaceInfo);
			dec.JE_ShipmentIncoTermPlace = "GBLHR";
			AssertNoMessageErrors(dec.JE_ShipmentIncoTermPlaceInfo);
			dec.JE_ShipmentIncoTermPlace = "IESantry";
			AssertHasWarningContaining(dec.JE_ShipmentIncoTermPlaceInfo, "When not using a UNLOCO, the location must be given with a country/region code");
			dec.JE_ShipmentIncoTermPlace = "DWAnkhmorpork";
			AssertHasMessageErrorContaining(dec.JE_ShipmentIncoTermPlaceInfo, "Use UNLOCO or country/region code prefix");
		}

		public new void TestCheckJE_GoodsLocation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Port");
			var cusCode = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "ABCDEF", "ABCDEF", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateCusCodeListAttribute(cusCode.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Facility, "AU");
			Factory.Save();

			var dec = GetJobDeclarationForTest();
			dec.JE_Calc_LocationOtherInformationType = "";
			dec.JE_GoodsLocation = "ABCDEF";
			AssertHasMessageErrorContaining(dec.JE_GoodsLocationInfo, string.Format(CDSJobDeclarationValidation.LocationTypeCodeMismatch, "AU", ""));

			dec.JE_Calc_LocationOtherInformationType = "XX";
			dec.JE_GoodsLocation = "ABCDEF";
			AssertHasMessageErrorContaining(dec.JE_GoodsLocationInfo, string.Format(CDSJobDeclarationValidation.LocationTypeCodeMismatch, "AU", "XX"));

			dec.JE_Calc_LocationOtherInformationType = "AU";
			dec.JE_GoodsLocation = "ABCDEF";
			AssertNoMessageErrors(dec.JE_GoodsLocationInfo);
		}

		public void TestCheckJE_OH_ShippingLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			declaration.JE_IsGvmsPort = true;
			declaration.JE_OH_ShippingLine = ZGuid.Empty;
			AssertHasMessageError(declaration.JE_OH_ShippingLineInfo, "This job moves through a GVMS-enabled port (as identified from the 5/23 location of goods code), and so requires an RRS01 statement. This can be set manually on the declaration's or instruction's Additional Information grid, or can be automated by completing this carrier field.");

			declaration.JE_IsGvmsPort = false;
			declaration.JE_OH_ShippingLine = new ZGuid();
			AssertNoMessageErrors(declaration.JE_OH_ShippingLineInfo);

			declaration.JE_IsGvmsPort = false;
			declaration.JE_OH_ShippingLine = ZGuid.Empty;
			AssertNoMessageErrors(declaration.JE_OH_ShippingLineInfo);

			declaration.JE_IsGvmsPort = true;
			var info = declaration.AdditionalInfos.AddNew();
			info.CSI_Code = GBCommonConstants.AdditonalInfoCodes.RRS01;

			declaration.JE_OH_ShippingLine = ZGuid.Empty;
			AssertNoMessageErrors(declaration.JE_OH_ShippingLineInfo);
		}

		public void TestCheckJE_TransportModeIsMandatory()
		{
			var declarationMock = Factory.NewMoq<JobDeclaration>();
			declarationMock.Setup(m => m.IsNonTransportDeclarationType).Returns(true);
			var dec = declarationMock.Object;

			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			dec.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			dec.Validation.ValidateJE_TransportMode();
			AssertNoMessageErrorContaining(dec.JE_TransportModeInfo, MandatoryValidation.YouHaveNotEntered);
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.Validation.ValidateJE_TransportMode();
			AssertNoMessageErrorContaining(dec.JE_TransportModeInfo, MandatoryValidation.YouHaveNotEntered);
			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			dec.Validation.ValidateJE_TransportMode();
			AssertNoMessageErrorContaining(dec.JE_TransportModeInfo, MandatoryValidation.YouHaveNotEntered);

			declarationMock.Setup(m => m.IsNonTransportDeclarationType).Returns(false);
			dec = declarationMock.Object;
			dec.Validation.ValidateJE_TransportMode();
			AssertHasMessageErrorContaining(dec.JE_TransportModeInfo, MandatoryValidation.YouHaveNotEntered);

			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.Validation.ValidateJE_TransportMode();
			AssertHasMessageErrorContaining(dec.JE_TransportModeInfo, MandatoryValidation.YouHaveNotEntered);

			dec.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			dec.Validation.ValidateJE_TransportMode();
			AssertNoMessageErrorContaining(dec.JE_TransportModeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_VesselNameIsMandatory()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_TransportMode = TransportTypeList.Codes.Air;
			dec.Validation.ValidateJE_VesselName();
			AssertNoMessageErrorContaining(dec.JE_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);

			dec.JE_TransportMode = TransportTypeList.Codes.Sea;
			dec.Validation.ValidateJE_VesselName();
			AssertHasMessageErrorContaining(dec.JE_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);

			dec.JE_TransportMode = TransportTypeList.Codes.Road;
			dec.Validation.ValidateJE_VesselName();
			AssertNoMessageErrorContaining(dec.JE_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_VoyageFlightNoIsMandatory()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_TransportMode = TransportTypeList.Codes.Air;
			dec.Validation.ValidateJE_VoyageFlightNo();
			AssertHasMessageErrorContaining(dec.JE_VoyageFlightNoInfo, MandatoryValidation.YouHaveNotEntered);

			dec.JE_TransportMode = TransportTypeList.Codes.Sea;
			dec.Validation.ValidateJE_VoyageFlightNo();
			AssertNoMessageErrorContaining(dec.JE_VoyageFlightNoInfo, MandatoryValidation.YouHaveNotEntered);

			dec.JE_TransportMode = TransportTypeList.Codes.Road;
			dec.Validation.ValidateJE_VoyageFlightNo();
			AssertNoMessageErrorContaining(dec.JE_VoyageFlightNoInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestPortsAndShedsList()
		{
			PortTest.CreatePort(Factory, "GB", "BLE", "Agility Logistics Ltd", portType: "SEA");
			PortTest.CreatePort(Factory, "GB", "LLA", "LLANDDULAS", portType: "ICD");
			PortTest.CreatePort(Factory, "GB", "FZO", "FILTON AERODROME", portType: "DES");
			PortTest.CreatePort(Factory, "GB", "LDC", "North West Collector’s Office", portType: "COL");
			PortTest.CreatePort(Factory, "GB", "MLS", "RAF MOLESHILL", portType: "MIL");
			PortTest.CreatePort(Factory, "GB", "FZY", "Port of Tilbury Free Zone", portType: "FZN");
			PortTest.CreatePort(Factory, "GB", "MPD", "MPD Mount Pleasant Depot", portType: "MAI");
			PortTest.CreatePort(Factory, "GB", "GLO", "GLOUCESTER (STAVERTON) AIRPORT	COA", portType: "COA");
			PortTest.CreatePort(Factory, "GB", "LHR", "AMERICAN AIRLINES at Heathrow", portType: "COA");
			PortTest.CreatePort(Factory, "GB", "MAN", "AMERICAN AIRLINES FALLBACK MANAAS at Manchester", portType: "COA");

			ShedTest.CreateShed(Factory, "GB", "LHRAAS", "AMERICAN AIRLINES at Heathrow");
			ShedTest.CreateShed(Factory, "GB", "LHRACS", "AIR CANADA  at Heathrow");
			ShedTest.CreateShed(Factory, "GB", "MANAAS", "AMERICAN AIRLINES FALLBACK MANAAS at Manchester");

			Factory.Save();

			var declaration = GetJobDeclarationForTest();
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var portsAndSheds = (CodeDescriptionPairList)declaration.Lookups.Locations;
			Assert(portsAndSheds.ContainsCode("FZO"));
			Assert(portsAndSheds.ContainsCode("MLS"));
			Assert(portsAndSheds.ContainsCode("GLO"));
			Assert(portsAndSheds.ContainsCode("LHR"));
			Assert(portsAndSheds.ContainsCode("MAN"));

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			portsAndSheds = (CodeDescriptionPairList)declaration.Lookups.Locations;
			Assert(portsAndSheds.ContainsCode("BLE"));
			Assert(portsAndSheds.ContainsCode("LDC"));
			Assert(portsAndSheds.ContainsCode("FZY"));
			Assert(portsAndSheds.ContainsCode("LLA"));

			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			portsAndSheds = (CodeDescriptionPairList)declaration.Lookups.Locations;
			Assert(portsAndSheds.ContainsCode("LDC"));
			Assert(portsAndSheds.ContainsCode("FZY"));
			Assert(portsAndSheds.ContainsCode("LLA"));
			Assert(portsAndSheds.ContainsCode("MPD"));

			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			portsAndSheds = (CodeDescriptionPairList)declaration.Lookups.Locations;
			Assert(portsAndSheds.ContainsCode("LDC"));
			Assert(portsAndSheds.ContainsCode("FZY"));
			Assert(portsAndSheds.ContainsCode("LLA"));
			Assert(portsAndSheds.ContainsCode("MPD"));

			declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
			portsAndSheds = (CodeDescriptionPairList)declaration.Lookups.Locations;
			Assert(portsAndSheds.ContainsCode("MPD"));
		}

		public void TestSupplierCityForImportsErrorAddressAndNameEmpty()
		{
			var dec = GetJobDeclarationForTest();
			OrgHeader supplier = Factory.New<OrgHeader>();
			supplier.OH_FullName = "Daniel";
			OrgAddress addressOfSupplier = supplier.Addresses.AddNewMainAddress();
			addressOfSupplier.OA_Address1 = "123 Fake Street";
			addressOfSupplier.OA_City = "";
			addressOfSupplier.OA_RL_NKRelatedPortCode = "USORD"; // gives country

			string expectedErrorPrefix = AddressValidationHelper.ErrorMessageAddressAndNameEmpty + "Supplier Documentary Address: ";
			dec.JE_MessageType = "IMP";
			TriggerValidation(dec, supplier, addressOfSupplier);
			AssertHasMessageErrorContaining(dec.SupplierDocumentaryAddress.E2_OA_AddressInfo, expectedErrorPrefix + "City");

			addressOfSupplier.OA_City = "Springfield";
			dec.SupplierDocumentaryAddress.E2_Postcode = "";
			TriggerValidation(dec, supplier, addressOfSupplier);
			AssertNoMessageErrorContaining(dec.SupplierDocumentaryAddress.E2_OA_AddressInfo, expectedErrorPrefix + "City");

			// AssertHasMessageErrorContaining(dec.SupplierDocumentaryAddress.E2_OA_AddressInfo, expectedErrorPrefix + "Postcode");
			// Missing postcode should provide a warning instead of a message error, removed above line which tests for unwanted behaviour
			AssertHasWarningContaining(dec.SupplierDocumentaryAddress.E2_OA_AddressInfo, expectedErrorPrefix + "Postcode");
		}

		static void TriggerValidation(JobDeclaration dec, OrgHeader supplier, OrgAddress addressOfSupplier)
		{
			dec.JE_OH_Supplier = ZGuid.Empty;
			dec.JE_OH_Supplier = supplier.PK;
			dec.SupplierDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
			dec.SupplierDocumentaryAddress.E2_OA_Address = addressOfSupplier.PK;
		}

		protected JobDeclaration GetJobDeclarationForTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			return declaration;
		}
	}
}
