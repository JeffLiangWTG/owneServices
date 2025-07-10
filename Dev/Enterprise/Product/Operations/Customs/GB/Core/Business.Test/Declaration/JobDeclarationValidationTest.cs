using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.Testing
{
	public class JobDeclarationValidationTest : TestCaseWithFactory
	{
		public void TestCheckJE_EidrType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_EidrType = EidrTypeList.Codes.CFS;
			AssertNoMessageError(declaration.JE_EidrTypeInfo, "The code you have selected is not in the list.");
			declaration.JE_EidrType = "123";
			AssertHasMessageError(declaration.JE_EidrTypeInfo, "The code you have selected is not in the list.");
		}

		public void TestJE_MasterUCR_GatewayIsCDS()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			using (GBCustomsDataRegistry.Instance.BadgeCodes.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new BadgeCodeSettingCollection
			{
				new BadgeCodeSetting
				{
					BadgeCode = "ABC",
					RL_PortCode = "GBLBA",
					Direction = JobMessageTypeList.Codes.Import,
					CSPCode = GatewayList.Codes.CDS,
					MasterUcrCalculationMode = MucrGenerationStyles.Codes.Ccsuk,
					ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services,
				}
			}))
			{
				declaration.JE_CustomsProfile = "ABC";
				declaration.JE_MasterUCR = "mucr";
				AssertHasMessageError(declaration.JE_MasterUCRInfo, "Inventory-linked imports cannot be sent directly to CDS, and must instead go via a CSP.  This is a policy restriction from HMRC. Either select another profile via a CSP or remove the MUCR.");

				declaration.JE_MasterUCR = ZString.Empty;
				AssertNoMessageError(declaration.JE_MasterUCRInfo, "Inventory-linked imports cannot be sent directly to CDS, and must instead go via a CSP.  This is a policy restriction from HMRC. Either select another profile via a CSP or remove the MUCR.");
			}
		}

		public void TestCheckJE_MasterUCR()
		{
			var warningMessage = "Inventory linking will not occur if you do not enter a Master UCR";
			AssertEquals("Prerequisite: DisableBlueValidationOnMUCRForInventoryLinkedPortsImport is set to No", false, GBCustomsDataRegistry.Instance.DisableBlueValidationOnMUCRForInventoryLinkedPortsImport.Value);

			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			var code1 = refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "AR1", GBCommonConstants.RefCusCodeListAttributeCodes.Inventory, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			refHelper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK.ToGuid(), GBCommonConstants.RefCusCodeListAttributeCodes.Inventory, "true");
			Factory.Save();

			var importDeclaration = Factory.New<JobDeclaration>();
			importDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			importDeclaration.JE_CHIEF_GoodsLocation = "AR1";
			AssertEquals("Port of Arrival has Inventory attribute", true, importDeclaration.HasPortInventoryAttribute);
			importDeclaration.JE_MasterUCR = ZString.Empty;
			AssertHasMessageError("Message Error:", importDeclaration.JE_MasterUCRInfo, "Inventory linking will not occur if you do not enter a Master UCR (inventory consignment reference, ICR)");
			using (GBCustomsDataRegistry.Instance.DisableBlueValidationOnMUCRForInventoryLinkedPortsImport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Prerequisite: DisableBlueValidationOnMUCRForInventoryLinkedPortsImport is set to Yes", true, GBCustomsDataRegistry.Instance.DisableBlueValidationOnMUCRForInventoryLinkedPortsImport.Value);
				importDeclaration.JE_MasterUCR = ZString.Empty;
				AssertHasWarning("Warning:", importDeclaration.JE_MasterUCRInfo, warningMessage);
			}

			AssertEquals("Prerequisite: DisableBlueValidationOnMUCRForExport is set to No", false, GBCustomsDataRegistry.Instance.DisableBlueValidationOnMUCRForExport.Value);
			var exportDeclaration = Factory.New<JobDeclaration>();
			exportDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			exportDeclaration.JE_MasterUCR = ZString.Empty;
			AssertHasMessageError("Message Error:", exportDeclaration.JE_MasterUCRInfo, "Inventory linking will not occur if you do not enter a Master UCR. You may associate this entry to a MUCR later by sending an ASS (associate) message for one entry, or using Operational Actions to associate many entries to a MUCR in bulk.");
			using (GBCustomsDataRegistry.Instance.DisableBlueValidationOnMUCRForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Prerequisite: DisableBlueValidationOnMUCRForExport is set to Yes", true, GBCustomsDataRegistry.Instance.DisableBlueValidationOnMUCRForExport.Value);
				exportDeclaration.JE_MasterUCR = ZString.Empty;
				AssertHasWarning("Warning:", exportDeclaration.JE_MasterUCRInfo, warningMessage);
			}

			var miscDeclaration = Factory.New<JobDeclaration>();
			miscDeclaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			miscDeclaration.JE_MasterUCR = ZString.Empty;
			AssertHasWarning("Warning:", miscDeclaration.JE_MasterUCRInfo, warningMessage);
		}

		public void TestCheckG9_Nch1RequestType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_Nch1RequestType = "X";
			AssertHasMessageErrorContaining(declaration.JE_Nch1RequestTypeInfo, "list");
			declaration.JE_Nch1RequestType = Nch1RequestTypes.Codes.AdditionalInformation;
			AssertNoMessageErrorContaining(declaration.JE_Nch1RequestTypeInfo, "list");
			declaration.JE_Nch1RequestType = ZString.Empty;
			AssertEquals(false, declaration.JE_Nch1RequestTypeInfo.HasNotifications());
			declaration.JE_Nch1RequestType = Nch1RequestTypes.Codes.C1602ExportExitNotification;
			AssertHasMessageErrorContaining(declaration.JE_Nch1RequestTypeInfo, "export");
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_Nch1RequestType = Nch1RequestTypes.Codes.C1603ExportRetrospectiveArrival;
			AssertNoMessageErrorContaining(declaration.JE_Nch1RequestTypeInfo, "export");
		}

		public void TestCheckCheckG9_Nch1Priority()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_Nch1Priority = "X";
			AssertHasMessageErrorContaining(declaration.JE_Nch1PriorityInfo, "list");
			declaration.JE_Nch1Priority = Nch1PriorityTypes.Codes.LiveAnimalsHumanRemains;
			AssertNoMessageErrorContaining(declaration.JE_Nch1PriorityInfo, "list");
			declaration.JE_Nch1RequestType = ZString.Empty;
			AssertEquals(false, declaration.JE_Nch1PriorityInfo.HasNotifications());
		}

		public void TestJE_DeclarationTypeComesFromCei()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationType = "DEF";

			AssertEquals("DEF", dec.CustomsEntryInstructionProvider.CustomsEntryInstructions[0].CEI_Style);
			AssertEquals("DEF", dec.CusEntryInstruction.CEI_Style);
			dec.CusEntryInstruction.CEI_Style = "ABC";
			AssertEquals("ABC", dec.JE_DeclarationType);
			AssertEquals(dec.CustomsEntryInstructionProvider.CustomsEntryInstructions[0], dec.CusEntryInstruction);
		}

		public void TestCheckJE_UCR()
		{
			var declaration = Factory.New<JobDeclaration>();
			Factory.Save();
			declaration.JE_UCR = "9GB123456789000-B01010101";
			AssertNoMessageErrorContaining(declaration.JE_UCRInfo, "must match format");
			declaration.JE_UCR = "9GB123456789000-B01010101/1";
			AssertHasMessageErrorContaining(declaration.JE_UCRInfo, "must match format");
			declaration.JE_UCR = "someCrap";
			AssertHasMessageErrorContaining(declaration.JE_UCRInfo, "must match format");

			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_UCR = "BLAH";
			Factory.Save();
			AssertNoMessageErrorContaining(dec1.JE_UCRInfo, "This DUCR is already in use on another declaration");

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_UCR = "BLAH";
			AssertHasMessageErrorContaining(dec2.JE_UCRInfo, "This DUCR is already in use on another declaration");
		}

		public void TestCheckJE_IATALoadPort()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var iataType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUIATA, "EU IATA");
			var iataPort = helper.CreateNewOrGetExistingCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUIATA, "KD1", "KD1 DESC", ZDate.Today.AddYears(-1), ZDate.Today.AddYears(1));
			Factory.Save();
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			dec.JE_TransportMode = dec.TransportModeAirCodeForTesting;
			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertNoMessageErrors(dec.JE_IATALoadPortInfo);
			dec.JE_IATALoadPort = "XXX";
			AssertHasMessageErrorContaining(dec.JE_IATALoadPortInfo, "foreign airport code. It is not needed for exports");
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_IATALoadPort = "XXX";
			AssertNoMessageErrorContaining(dec.JE_IATALoadPortInfo, "foreign airport code. It is not needed for exports");
			AssertHasMessageErrorContaining(dec.JE_IATALoadPortInfo, ListValidation.InvalidCodeMessageError);
			dec.JE_IATALoadPort = "KD1";
			AssertNoMessageErrors(dec.JE_IATALoadPortInfo);
		}

		public void TestCheckJE_VesselName()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			var vessel = Factory.New<RefVessel>();
			vessel.RV_LloydsNumber = "7724090";  // Bad checksum
			vessel.RV_Code = "HMS Daniel";
			declaration.JE_VesselName = vessel.RV_Code;
			AssertEquals("No message error for Lloyds number since Chief cares not", false, declaration.JE_VesselNameInfo.HasMessageError("Lloyds"));
		}

		public void TestCheckJE_OH_Supplier()
		{
			var dec = Factory.New<JobDeclaration>();
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "AGI";
			var inv = dec.Invoices.AddNew();
			inv.Validation.ValidateJZ_OH_Supplier();
			AssertHasMessageErrorContaining(inv.JZ_OH_SupplierInfo, invoiceWarning);
			inv.JZ_OH_Supplier = org.PK;
			AssertNoMessageErrorContaining(dec.SupplierDocumentaryAddress.E2_OA_AddressInfo, invoiceWarning);

			inv.JZ_OH_Supplier = ZGuid.Empty;
			dec.JE_OH_Supplier = org.PK;
			AssertNoWarningContaining(dec.SupplierDocumentaryAddress.E2_OA_AddressInfo, decPartyWarningPartial_Supplier);

			dec.JE_OH_Supplier = ZGuid.Empty;
			AssertHasWarningContaining(dec.SupplierDocumentaryAddress.E2_OA_AddressInfo, decPartyWarningPartial_Supplier);
		}

		public void TestCheckJE_OH_Importer()
		{
			var dec = Factory.New<JobDeclaration>();
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "AGI";
			var inv = dec.Invoices.AddNew();
			inv.Validation.ValidateJZ_OH_Buyer();
			AssertHasMessageErrorContaining(inv.JZ_OH_BuyerInfo, invoiceWarning);
			inv.JZ_OH_Buyer = org.PK;
			AssertNoMessageErrorContaining(dec.ImporterDocumentaryAddress.E2_OA_AddressInfo, invoiceWarning);

			inv.JZ_OH_Buyer = ZGuid.Empty;
			dec.JE_OH_Importer = org.PK;
			AssertNoWarningContaining(dec.ImporterDocumentaryAddress.E2_OA_AddressInfo, decPartyWarningPartial_Importer);

			dec.JE_OH_Importer = ZGuid.Empty;
			AssertHasWarningContaining(dec.ImporterDocumentaryAddress.E2_OA_AddressInfo, decPartyWarningPartial_Importer);
		}

		public void TestCheckJE_TransportMode()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportClearanceRequest;
			dec.Validation.ValidateJE_TransportMode();
			AssertNoMessageErrorContaining(dec.JE_TransportModeInfo, MandatoryValidation.YouHaveNotEntered);
			dec.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportFullDeclaration;
			dec.Validation.ValidateJE_TransportMode();
			AssertHasMessageErrorContaining(dec.JE_TransportModeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_HouseBill()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportFullDeclaration;

			var badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			var badgeCodeSetting = badgeCodeSettings.AddNew();
			badgeCodeSetting.BadgeCode = "CUK";
			badgeCodeSetting.RL_PortCode = "GBLHR";
			badgeCodeSetting.CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW;
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings);
			badgeCodeSetting = badgeCodeSettings.AddNew();
			badgeCodeSetting.BadgeCode = "CNS";
			badgeCodeSetting.RL_PortCode = "GBLHR";
			badgeCodeSetting.CSPCode = GatewayList.Codes.CNS_CUSDECOnly;
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings);

			declaration.JE_CustomsProfile = "CUK";
			declaration.JE_HouseBill = "12345678";
			AssertNoMessageError(declaration.JE_HouseBillInfo, "House Number must be 8 characters.");
			declaration.JE_HouseBill = "123";
			AssertHasMessageError(declaration.JE_HouseBillInfo, "House Number must be 8 characters.");
			declaration.JE_HouseBill = "ABC45678";
			AssertNoMessageError(declaration.JE_HouseBillInfo, "House Number must be 8 characters.");
			declaration.JE_HouseBill = "1234567890";
			AssertHasMessageError(declaration.JE_HouseBillInfo, "House Number must be 8 characters.");
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.Validation.ValidateJE_HouseBill();
			AssertNoMessageError(declaration.JE_HouseBillInfo, "House Number must be 8 characters.");
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_HouseBill = "12345678";
			AssertNoMessageError(declaration.JE_HouseBillInfo, "House Number must be 8 characters.");

			declaration.JE_HouseBill = "CRAPPY";
			declaration.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportFullWarehouse;
			declaration.Validation.ValidateJE_HouseBill();
			AssertNoMessageError(declaration.JE_HouseBillInfo, "House Number must be 8 characters.");
			declaration.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportSupplementaryDeclaration;
			declaration.Validation.ValidateJE_HouseBill();
			AssertNoMessageError(declaration.JE_HouseBillInfo, "House Number must be 8 characters.");
			declaration.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportSupplementaryWarehouse;
			declaration.Validation.ValidateJE_HouseBill();
			AssertNoMessageError(declaration.JE_HouseBillInfo, "House Number must be 8 characters.");
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportFullDeclaration;
			declaration.Validation.ValidateJE_HouseBill();
			AssertNoMessageError(declaration.JE_HouseBillInfo, "House Number must be 8 characters.");

			declaration.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportFullDeclaration;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_CustomsProfile = "CNS";
			declaration.JE_HouseBill = "CRAP";
			AssertHasMessageError(declaration.JE_HouseBillInfo, "House Number must be 8 characters.");
			declaration.JE_CustomsProfile = "CUK";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.Validation.ValidateJE_HouseBill();
			AssertNoMessageError(declaration.JE_HouseBillInfo, "House Number must be 8 characters.");
			declaration.JE_CustomsProfile = "CNS";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_HouseBill = "CRAP";
			AssertHasMessageError(declaration.JE_HouseBillInfo, "House Number must be 8 characters.");
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_HouseBill = "CRAP2";
			AssertNoMessageError(declaration.JE_HouseBillInfo, "House Number must be 8 characters.");
		}

		public void TestBadgeValidationForHouseBill_CS00144905()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportFullDeclaration;

			declaration.JE_CustomsProfile = "CUK";
			AssertNoExceptionThrown(delegate
			{ declaration.Validation.ValidateJE_HouseBill(); });
		}

		public void TestCheckJE_Calc_LocationOtherInformationCountry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_Calc_LocationOtherInformationCountry = "XX";
			AssertHasMessageErrorContaining(declaration.JE_Calc_LocationOtherInformationCountryInfo, "Two-character codes must match those in the Tariff, Vol III, appendix C1");
			declaration.JE_Calc_LocationOtherInformationCountry = Core.Constants.CountryCodes.UnitedKingdom;
			AssertNoMessageErrorContaining(declaration.JE_Calc_LocationOtherInformationCountryInfo, "Two-character codes must match those in the Tariff, Vol III, appendix C1");
		}

		public void TestCheckJE_Calc_LocationOtherInformationType()
		{
			var universalReferenceTestDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var eun = universalReferenceTestDataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			var uk = universalReferenceTestDataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, "United Kingdom", eun);
			universalReferenceTestDataHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GoodsOfLocationType, "Additional Country");
			var cusCode = universalReferenceTestDataHelper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GoodsOfLocationType, "BY", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_Calc_LocationOtherInformationType = "XX";
			AssertHasMessageErrorContaining(declaration.JE_Calc_LocationOtherInformationTypeInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_Calc_LocationOtherInformationType = "BY";
			AssertNoMessageErrorContaining(declaration.JE_Calc_LocationOtherInformationTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public virtual void TestCheckJE_LocationQualifier()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_LocationQualifier = "XX";
			AssertHasMessageErrorContaining(declaration.JE_LocationQualifierInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_LocationQualifier = "CW";
			AssertNoMessageErrorContaining(declaration.JE_LocationQualifierInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJE_GoodsLocation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Port");

			var code1 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "AU_AIRPORT", "AU AIRPORT1", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var attribute1 = helper.CreateCusCodeListAttribute(code1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Facility, "AU");
			var attTransportMode1 = helper.CreateTransportModeForCusCodeList(code1.PK, TransportTypeList.Codes.Air);

			var code2 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "AU_PORT", "AU PORT1", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var attribute2 = helper.CreateCusCodeListAttribute(code2.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Facility, "AU");
			var attTransportMode2 = helper.CreateTransportModeForCusCodeList(code2.PK, TransportTypeList.Codes.Sea);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_Calc_LocationOtherInformationType = "AU";

			declaration.JE_GoodsLocation = "XXX";
			AssertHasMessageErrorContaining(declaration.JE_GoodsLocationInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_GoodsLocation = "AU_PORT";
			AssertNoMessageErrorContaining(declaration.JE_GoodsLocationInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.Validation.ValidateJE_GoodsLocation();
			AssertHasMessageErrorContaining(declaration.JE_GoodsLocationInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_GoodsLocation = "AU_AIRPORT";
			AssertNoMessageErrorContaining(declaration.JE_GoodsLocationInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestGBValidationUsesCustomsValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.Declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, ZString.Empty);

			declaration.JE_PaymentMethod = "A";
			declaration.JE_DefermentAccountNumber = ZString.Empty;

			AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "The DAN prefix & DAN must both be present");
			AssertHasMessageErrorContaining(declaration.JE_DefermentAccountNumberInfo, "The DAN prefix & DAN must both be present");
		}

		public void TestGBJE_VesselName()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			var eunGroup = refHelper.CreateNewOrGetExistingDataGrouping("EUN");
			var gbGroup = refHelper.CreateNewOrGetExistingDataGrouping("GB", parent: eunGroup);

			var portCodeType = refHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Port");
			var code1 = refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PtRoRo", "RoRo Location for Goods", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			refHelper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK.ToGuid(), RefCusCodeListAttributeTypes.Codes.RoRoLocation, "");
			Factory.Save();

			JobDeclaration dec = Factory.New<JobDeclaration>();
			dec.JE_LocationOfGoods = "PtRoRo";

			AssertEquals(true, dec.IsRoRoLocation);
			AssertEquals(string.Empty, dec.JE_VesselName);
			dec.Validation.ValidateJE_VesselName();
			AssertHasMessageError(dec.JE_VesselNameInfo, "The transport ID must be completed for a RORO location.");

			dec.JE_VesselName = "DONE";
			dec.Validation.ValidateJE_VesselName();
			AssertNoMessageError(dec.JE_VesselNameInfo, "The transport ID must be completed for a RORO location.");
		}

		public void TestGBJE_TransportMode()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			var eunGroup = refHelper.CreateNewOrGetExistingDataGrouping("EUN");
			var gbGroup = refHelper.CreateNewOrGetExistingDataGrouping("GB", parent: eunGroup);

			var portCodeType = refHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Port");
			var code1 = refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PtRoRo", "RoRo Location for Goods", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			var roroAttr = refHelper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK.ToGuid(), RefCusCodeListAttributeTypes.Codes.RoRoLocation, "");

			Factory.Save();
			JobDeclaration dec = Factory.New<JobDeclaration>();
			dec.JE_LocationOfGoods = "PtRoRo";

			AssertEquals(true, dec.IsRoRoOnlyLocation);
			AssertEquals(string.Empty, dec.JE_TransportMode);
			dec.Validation.ValidateJE_TransportMode();
			AssertHasMessageError(dec.JE_TransportModeInfo, "Transport type must be \"ROR\" when Location of goods is a RoRo-Only location.");

			dec.JE_TransportMode = GBTransportTypeList.Codes.ROR;
			dec.Validation.ValidateJE_TransportMode();
			AssertNoMessageError(dec.JE_TransportModeInfo, "Transport type must be \"ROR\" when Location of goods is a RoRo-Only location.");

			JobDeclaration declaration1 = Factory.New<JobDeclaration>();
			var code2 = refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PrRoRo", "RoRo Location for Goods", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			var roroAttr1 = refHelper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK.ToGuid(), RefCusCodeListAttributeTypes.Codes.RoRoLocation, "MIXED");
			Factory.Save();
			declaration1.JE_LocationOfGoods = "PrRoRo";

			AssertEquals(false, declaration1.IsRoRoOnlyLocation);
			AssertEquals(string.Empty, declaration1.JE_TransportMode);

			declaration1.Validation.ValidateJE_TransportMode();
			AssertNoMessageError(declaration1.JE_TransportModeInfo, "Transport type must be \"ROR\" when Location of goods is a RoRo-Only location.");
		}

		public void TestMessageErrorForEmptyMUCR()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportFullDeclaration;
			AssertNoWarningContaining(dec.JE_MasterUCRInfo, "Inventory linking");
			dec.JE_MasterUCR = ZString.Empty;
			AssertHasWarningContaining(dec.JE_MasterUCRInfo, "Inventory linking");
		}

		public void TestSupervisingOfficeValidation()
		{
			var universalReferenceTestDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var eun = universalReferenceTestDataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			var uk = universalReferenceTestDataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, "United Kingdom", eun);
			universalReferenceTestDataHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupervisingOffice, "SupervisingOffice");
			var cusCode = universalReferenceTestDataHelper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupervisingOffice, "CKSPCODE1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "CK1";
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, "CKSPCODE1");

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "CK2";

			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			dec.SupervisingOfficeDocAddress.E2_OA_Address = org.MainAddress.PK;

			AssertNoMessageErrors(dec.SupervisingOfficeDocAddress.E2_OA_AddressInfo);

			dec.SupervisingOfficeDocAddress.E2_OA_Address = org2.MainAddress.PK;
			AssertHasMessageErrorContaining(dec.SupervisingOfficeDocAddress.E2_OA_AddressInfo, Declaration.JobDeclarationValidation.InvalidSupervisingOffice_ErrorMessage);

			org.CustomsCodes.RemoveAll();
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, "CKSPCODE2");
			dec.SupervisingOfficeDocAddress.E2_OA_Address = org.MainAddress.PK;
			AssertHasMessageErrorContaining(dec.SupervisingOfficeDocAddress.E2_OA_AddressInfo, Declaration.JobDeclarationValidation.SupervisingOfficeDoesNotExist_ErrorMessage);

			org.CustomsCodes.RemoveAll();
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, "CKSPCODE1");
			dec.SupervisingOfficeDocAddress.E2_OA_Address = ZGuid.Empty;
			dec.SupervisingOfficeDocAddress.E2_OA_Address = org.MainAddress.PK;
			AssertNoMessageErrors(dec.SupervisingOfficeDocAddress.E2_OA_AddressInfo);

			dec.SupervisingOfficeDocAddress.E2_OA_Address = org2.MainAddress.PK;
			AssertHasMessageErrorContaining(dec.SupervisingOfficeDocAddress.E2_OA_AddressInfo, Declaration.JobDeclarationValidation.InvalidSupervisingOffice_ErrorMessage);

			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			dec.RunPreSaveValidation();
			AssertNoMessageErrors(dec.SupervisingOfficeDocAddress.E2_OA_AddressInfo);
		}

		public void TestGBOfficeCodeValidation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunzzz = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eunzzz);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: eunzzz);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, parent: eunzzz);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			var codeGB000001 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "GB000001", "Central Community Transit Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var validGBOffice = codeGB000001.ZZD_Code;
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeGB000001.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExit);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeGB000001.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeForCentralizedClearance);

			var codeDE000001 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE000001", "Zi Germans Transit Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var validDEOffice = codeDE000001.ZZD_Code;
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeDE000001.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExit);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeDE000001.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeForCentralizedClearance);
			Factory.Save();

			SetupAndAssertGBOfficeCodeValidation(JobMessageTypeList.Codes.Export, validGBOffice); // GB Only
			SetupAndAssertGBOfficeCodeValidation(JobMessageTypeList.Codes.Import, validDEOffice); // NOT GB
		}

		void SetupAndAssertGBOfficeCodeValidation(string messageType, string existingOfficeCode)
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			dec.JE_MessageType = messageType;

			dec.JE_CustomsOffice = "IE111111";
			AssertHasMessageError(dec.JE_CustomsOfficeInfo, "The code you have selected is not in the list.");
			dec.JE_CustomsOffice = existingOfficeCode;
			dec.Validation.ValidateAll();
			AssertNoMessageErrorContaining(dec.JE_CustomsOfficeInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckJE_EntryAuthorisationDate()
		{
			string message = "Date should not be empty for CHIEF supplementary declarations";

			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationType = "ESD";
			Assert(dec.IsSupplementaryDeclarationType);

			dec.JE_EntryAuthorisationDate = ZDateTime.Now;
			AssertNoMessageErrorContaining(dec.JE_EntryAuthorisationDateInfo, message);

			dec.JE_EntryAuthorisationDate = ZDateTime.Empty;
			AssertHasMessageError(dec.JE_EntryAuthorisationDateInfo, message);

			dec.JE_DeclarationType = ZString.Empty;
			dec.Validation.ValidateJE_EntryAuthorisationDate();
			AssertNoMessageErrorContaining(dec.JE_EntryAuthorisationDateInfo, message);
		}

		public void TestValidateJE_CustomsProfileMatchesJE_ApplicationCodeWhenSelectedCDS()
		{
			const string CDS = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			const string CHIEF = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			string message = "The selected profile is set to be a CHIEF profile. Select a different profile or application";

			var badge = new BadgeCodeSetting() { CSPCode = GatewayList.Codes.CNS_CUSDECOnly, BadgeCode = "JCH" };
			var badgeCollection = new BadgeCodeSettingCollection() { badge };
			var currentBranchId = GlbBranch.CurrentBranch.PK.ToGuid();
			var declaration = Factory.New<JobDeclaration>();

			badge.ApplicationCode = CDS;
			using (GBCustomsDataRegistry.Instance.BadgeCodes.SetTemporaryValue(Guid.Empty, currentBranchId, Guid.Empty, badgeCollection))
			{
				declaration.JE_ApplicationCode = CDS;
				declaration.JE_CustomsProfile = badge.BadgeCode;
				AssertNoMessageErrorContaining(declaration.JE_CustomsProfileInfo, message);

				declaration.JE_ApplicationCode = CHIEF;
				declaration.JE_CustomsProfile = badge.BadgeCode;
				AssertNoMessageErrorContaining(declaration.JE_CustomsProfileInfo, message);
			}

			badge.ApplicationCode = CHIEF;
			using (GBCustomsDataRegistry.Instance.BadgeCodes.SetTemporaryValue(Guid.Empty, currentBranchId, Guid.Empty, badgeCollection))
			{
				declaration.JE_ApplicationCode = CDS;
				declaration.JE_CustomsProfile = badge.BadgeCode;
				AssertHasMessageErrorContaining(declaration.JE_CustomsProfileInfo, message);
			}

			badge.ApplicationCode = ""; // Equivalent to CHIEF
			using (GBCustomsDataRegistry.Instance.BadgeCodes.SetTemporaryValue(Guid.Empty, currentBranchId, Guid.Empty, badgeCollection))
			{
				declaration.JE_ApplicationCode = CDS;
				declaration.JE_CustomsProfile = badge.BadgeCode;
				AssertHasMessageErrorContaining(declaration.JE_CustomsProfileInfo, message);

				declaration.JE_ApplicationCode = CHIEF;
				declaration.JE_CustomsProfile = badge.BadgeCode;
				AssertNoMessageErrorContaining(declaration.JE_CustomsProfileInfo, message);
			}
		}

		public void TestJE_Application_CodeNotAllowNewCHFCreate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;

			var errorMessage = "You cannot convert a non-CHIEF JobDeclaration to the CHIEF type or create a new CHIEF JobDeclaration.";
			bool hasErrorMessageContains = declaration.JE_ApplicationCodeInfo.Notifications.Any(notification => notification.Message.Equals(errorMessage));
			AssertEquals(true, hasErrorMessageContains);
		}

		public void TestJE_ApplicationCode_CHFSaveIfOnlyPreviousIsCHF()
		{
			var declarationCHF = Factory.New<JobDeclaration>();
			declarationCHF.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;

			var declarationOther = Factory.New<JobDeclaration>();

			Factory.Save();

			var errorMessage = "You cannot convert a non-CHIEF JobDeclaration to the CHIEF type or create a new CHIEF JobDeclaration.";

			declarationCHF.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			ZBool hasErrorMessageContains = declarationCHF.JE_ApplicationCodeInfo.Notifications.Any(notification => notification.Message.Equals(errorMessage));
			AssertEquals(ZBool.False, hasErrorMessageContains);

			declarationOther.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			hasErrorMessageContains = declarationOther.JE_ApplicationCodeInfo.Notifications.Any(notification => notification.Message.Equals(errorMessage));
			AssertEquals(ZBool.True, hasErrorMessageContains);
		}

		readonly string decPartyWarningPartial_Supplier = "Please ensure that you select item-level suppliers against the invoice headers.";
		readonly string decPartyWarningPartial_Importer = "You have not entered an importer. Please ensure that you select item-level importers against the invoice headers";
		readonly string invoiceWarning = "You need to select a party since you have not selected one at declaration header level";
	}
}
