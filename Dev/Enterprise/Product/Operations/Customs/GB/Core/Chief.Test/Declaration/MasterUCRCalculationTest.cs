using System;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Chief.Declaration.Testing
{
	class MasterUCRCalculationTest : TestCaseWithFactory
	{
		// Lots of similar code but that's cos the badge cacher screws us otherwise.  We can't change styles of generation on a badge without tearing down and setting up again

		public void TestMasterUcrCalulationScenarioGems()
		{
			ShedTest.CreateShed(Factory, "GB", "LBAELF", "UPS SCS (UK) LTD at Leeds/Bradford", acpCode: "Y");
			ShedTest.CreateShed(Factory, "GB", "MANABC", "Manchester", acpCode: "M");
			Factory.Save();

			BadgeCodeSettingCollection badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			BadgeCodeSetting badgeCodeSetting = badgeCodeSettings.AddNew();
			badgeCodeSetting.CSPCode = "CCSUK";
			badgeCodeSetting.Direction = "IMP";
			badgeCodeSetting.BadgeCode = "DSK";
			badgeCodeSetting.RL_PortCode = "GBLBA";
			badgeCodeSetting.ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			badgeCodeSetting.MasterUcrCalculationMode = Registry.MucrGenerationStyles.Codes.GemsCcsuk;
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings);

			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			dec.JE_MessageType = "IMP";
			dec.JE_LocationOfGoods = "LBA"; // LBA=Y
			dec.SubLocation = "ELF";
			dec.JE_CustomsProfile = "DSK";
			AssertEquals("No mawb yet, so don't generate", "", dec.JE_MasterUCR);
			dec.JE_MasterBill = "08112345678";
			AssertEquals("YELF08112345678", dec.JE_MasterUCR);
			dec.JE_HouseBill = "12345";
			AssertEquals("YELF0811234567800012345", dec.JE_MasterUCR);
			dec.JE_HouseBill = "";
			dec.JE_HouseBill = "12345";
			AssertEquals("YELF0811234567800012345", dec.JE_MasterUCR);

			dec.JE_TransportMode = TransportTypeList.Codes.Air;
			dec.JE_MessageType = "IMP";
			dec.JE_CustomsProfile = "DSK";
			dec.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportFullDeclaration;
			dec.ZG_HouseSplitReference = "68";
			AssertEquals("YELF081123456780001234568", dec.JE_MasterUCR);
			dec.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportFullWarehouse;  // no longer import-air-inventory....
			AssertEquals("IFW should wipe the SRF", string.Empty, dec.ZG_HouseSplitReference);
			dec.ZG_HouseSplitReference = "67";
			AssertEquals("YELF0811234567800012345", dec.JE_MasterUCR);
			dec.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportFullDeclaration;
			dec.JE_HouseBill = "";
			AssertEquals("Split basics have spaces for hawb number", "YELF08112345678        67", dec.JE_MasterUCR);
			dec.ZG_HouseSplitReference = "";
			AssertEquals("YELF08112345678", dec.JE_MasterUCR);
			dec.JE_HouseBill = "TEST";

			var invoiceLine = dec.InvoiceLines.AddNew();
			var ai = invoiceLine.AdditionalInfos.AddNew();
			dec.SubLocation = "XYZ";
			ai.CSI_Code = "GEN51";
			ai.CSI_Description = "MANABC GBAEO0123456";
			dec.JE_HouseBill = "";
			AssertEquals("Mucr is that of the ETSF from the GEN51 statement, not from box 30", "MABC08112345678", dec.JE_MasterUCR);
		}
		public void TestMasterUcrCalulationCnsCourier()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Test Facility Code");
			var shed = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "XXXABC", "Some Description1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			shed.Attributes.Add(helper.CreateNewOrGetExistingCusCodeListAttribute(shed.PK, EU.Business.UniversalReferenceConstants.ShedAttributes.SITECODE, "SITE1"));
			Factory.Save();

			BadgeCodeSettingCollection badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			BadgeCodeSetting badgeCodeSetting = badgeCodeSettings.AddNew();
			badgeCodeSetting.BadgeCode = "DSK";
			badgeCodeSetting.RL_PortCode = "GBLBA";
			badgeCodeSetting.CSPCode = "CNS";
			badgeCodeSetting.MasterUcrCalculationMode = Registry.MucrGenerationStyles.Codes.Courier;
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings);

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			dec.ZG_Gateway = GatewayList.Codes.CNS_CUSDECOnly;

			var c = dec.AdditionalReferenceNumbers.AddNew();
			c.CE_EntryType = Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CourierConsignmentReference;
			c.CE_EntryNum = "COU1";

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "C123", Core.Constants.CountryCodes.UnitedKingdom);
			dec.JE_OH_ShippingLine = carrier.PK;

			MasterUCRCalculator calc = new MasterUCRCalculator();
			dec.JE_CustomsProfile = "DSK";
			dec.JE_LocationOfGoods = "XXXABC";
			AssertEquals("SITE1C123COU1", calc.Calculate(dec));

			dec.JE_OH_ShippingLine = ZGuid.Empty;
			dec.JE_MasterUCR = ZString.Empty;
			dec.JE_OH_ShippingLine = carrier.PK;
			AssertEquals("Changing shipping line reclacluated MUICR", "SITE1C123COU1", dec.JE_MasterUCR);
		}

		public void TestMasterUcrCalulationScenarioAirStyle()
		{
			BadgeCodeSettingCollection badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			BadgeCodeSetting badgeCodeSetting = badgeCodeSettings.AddNew();
			badgeCodeSetting.BadgeCode = "DSK";
			badgeCodeSetting.RL_PortCode = "GBLBA";
			badgeCodeSetting.Direction = "EXP";
			badgeCodeSetting.CSPCode = "CCSUK";
			badgeCodeSetting.MasterUcrCalculationMode = Registry.MucrGenerationStyles.Codes.Air;
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings);

			MasterUCRCalculator calc = new MasterUCRCalculator();
			OrgHeader org = Factory.New<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Turn, "123456789000");
			var dec = Factory.New<JobDeclaration>();
			dec.JE_CustomsProfile = "DSK";
			dec.SubLocation = "BAC";
			dec.JE_MasterBill = "125";
			dec.JE_HouseBill = "abcdefgh";  // hawbs are not usually alpha but it makes the test easier to read
			dec.JE_OA_DeclarantAddress = org.MainAddress.PK;

			AssertEquals("", calc.Calculate(dec));

			dec.JE_MasterBill = "12512345678";
			AssertEquals("A:12512345678", calc.Calculate(dec));

			dec.JE_MasterBill = "FARTOOLONG1234567891345678913456789";
			AssertEquals("A:12512345678", dec.JE_MasterUCR);
			AssertEquals("No explosion on long MUCR", true, string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
		}

		public void TestMasterUcrCalulationScenarioCcsukStyle()
		{
			ShedTest.CreateShed(Factory, "GB", "LBABAC", "UPS SCS (UK) LTD at Leeds/Bradford", acpCode: "Y");
			Factory.Save();

			BadgeCodeSettingCollection badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			BadgeCodeSetting badgeCodeSetting = badgeCodeSettings.AddNew();
			badgeCodeSetting.BadgeCode = "DSK";
			badgeCodeSetting.RL_PortCode = "GBLBA";
			badgeCodeSetting.Direction = "EXP";
			badgeCodeSetting.CSPCode = "CCSUK";
			badgeCodeSetting.MasterUcrCalculationMode = Registry.MucrGenerationStyles.Codes.Ccsuk;
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings);

			MasterUCRCalculator calc = new MasterUCRCalculator();
			OrgHeader org = Factory.New<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Turn, "123456789000");
			var dec = Factory.New<JobDeclaration>();
			dec.JE_CustomsProfile = "DSK";
			dec.JE_LocationOfGoods = "LBA";  // Y
			dec.SubLocation = "BAC";
			dec.JE_MasterBill = "12512345678";
			dec.JE_HouseBill = "abcdefgh";  // hawbs are not usually alpha but it makes the test easier to read
			dec.JE_OA_DeclarantAddress = org.MainAddress.PK;

			dec.JE_HouseBill = "stuvwxyz";
			AssertEquals("GB/CUK1-YBAC12512345678stuvwxyz", calc.Calculate(dec));
			dec.JE_HouseBill = "xyz";
			AssertEquals("GB/CUK1-YBAC1251234567800000xyz", calc.Calculate(dec));
		}

		public void TestMasterUcrCalulationScenarioEoriStyle()
		{
			BadgeCodeSettingCollection badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			BadgeCodeSetting badgeCodeSetting = badgeCodeSettings.AddNew();
			badgeCodeSetting.BadgeCode = "DSK";
			badgeCodeSetting.RL_PortCode = "GBLBA";
			badgeCodeSetting.Direction = "EXP";
			badgeCodeSetting.CSPCode = "CCSUK";
			badgeCodeSetting.MasterUcrCalculationMode = Registry.MucrGenerationStyles.Codes.Eori;
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings);

			MasterUCRCalculator calc = new MasterUCRCalculator();
			OrgHeader org = Factory.New<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Turn, "123456789000");
			var dec = Factory.New<JobDeclaration>();
			dec.JE_CustomsProfile = "DSK";
			dec.SubLocation = "BAC";
			dec.JE_MasterBill = "12512345678";
			dec.JE_LocationOfGoods = "LBA";
			dec.JE_HouseBill = "abcdefgh";  // hawbs are not usually alpha but it makes the test easier to read
			dec.JE_OA_DeclarantAddress = org.MainAddress.PK;

			AssertEquals("GB/123456789000-12512345678abcdefgh", calc.Calculate(dec));
		}

		public void TestMasterUcrCalulationScenarioSeaConsolStyle()
		{
			BadgeCodeSettingCollection badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			BadgeCodeSetting badgeCodeSetting = badgeCodeSettings.AddNew();
			badgeCodeSetting.BadgeCode = "DSK";
			badgeCodeSetting.RL_PortCode = "GBLBA";
			badgeCodeSetting.Direction = "EXP";
			badgeCodeSetting.CSPCode = "CCSUK";
			badgeCodeSetting.MasterUcrCalculationMode = Registry.MucrGenerationStyles.Codes.SeaConsol;
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings);

			MasterUCRCalculator calc = new MasterUCRCalculator();
			OrgHeader org = Factory.New<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Turn, "123456789000");
			var dec = Factory.New<JobDeclaration>();
			dec.JE_CustomsProfile = "DSK";
			dec.SubLocation = "BAC";
			dec.JE_MasterBill = "12512345678";
			dec.JE_LocationOfGoods = "LBA";
			dec.JE_HouseBill = "abcdefgh";
			dec.JE_OA_DeclarantAddress = org.MainAddress.PK;
			AssertEquals("GB/123456789000-12512345678abcdefgh", calc.Calculate(dec));

			dec.JE_TransportMode = "SEA";
			AssertEquals("GB/123456789000-12512345678abcdefgh", calc.Calculate(dec));
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "GBFXT";
			consol.JK_UniqueConsignRef = "C0001000";
			var shipment = consol.Shipments.AddNew();
			dec.JE_JS = shipment.PK;
			AssertEquals("GB/123456789000-C0001000", calc.Calculate(dec));
			dec.JE_TransportMode = "AIR";
			AssertEquals("GB/123456789000-12512345678abcdefgh", calc.Calculate(dec));

			var mawbExportInfo = GetMawbExportAddInfo(consol);
			mawbExportInfo.ME_Profile = "DSK";
			AssertContains("-C0001000", calc.Calculate(mawbExportInfo));
		}

		public void TestMasterUcrCalulationScenarioNone()
		{
			BadgeCodeSettingCollection badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			BadgeCodeSetting badgeCodeSetting = badgeCodeSettings.AddNew();
			badgeCodeSetting.BadgeCode = "DSK";
			badgeCodeSetting.RL_PortCode = "GBLBA";
			badgeCodeSetting.Direction = "EXP";
			badgeCodeSetting.CSPCode = "CCSUK";
			badgeCodeSetting.MasterUcrCalculationMode = Registry.MucrGenerationStyles.Codes.None;
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings);

			MasterUCRCalculator calc = new MasterUCRCalculator();
			OrgHeader org = Factory.New<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Turn, "123456789000");
			var dec = Factory.New<JobDeclaration>();
			dec.JE_CustomsProfile = "DSK";
			dec.SubLocation = "BAC";
			dec.JE_MasterBill = "12512345678";
			dec.JE_HouseBill = "abcdefgh";  // hawbs are not usually alpha but it makes the test easier to read
			dec.JE_OA_DeclarantAddress = org.MainAddress.PK;

			AssertEquals("", calc.Calculate(dec));
		}

		public void TestMasterUcrCalulationScenarioImportAir()
		{
			BadgeCodeSettingCollection badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			BadgeCodeSetting badgeCodeSetting = badgeCodeSettings.AddNew();
			badgeCodeSetting.BadgeCode = "DSK";
			badgeCodeSetting.RL_PortCode = "GBLBA";
			badgeCodeSetting.Direction = "EXP";
			badgeCodeSetting.CSPCode = "CCSUK";
			badgeCodeSetting.MasterUcrCalculationMode = Registry.MucrGenerationStyles.Codes.Air;
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings);

			MasterUCRCalculator calc = new MasterUCRCalculator();
			OrgHeader org = Factory.New<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Turn, "123456789000");
			var dec = Factory.New<JobDeclaration>();
			dec.JE_CustomsProfile = "DSK";
			dec.SubLocation = "BAC";
			dec.JE_MasterBill = "12512345678";
			dec.JE_HouseBill = "abcdefgh";  // hawbs are not usually alpha but it makes the test easier to read
			dec.JE_OA_DeclarantAddress = org.MainAddress.PK;

			// Import decs 
			dec.JE_MessageType = "IMP";
			AssertEquals("No MUCR for this import job because the bage is only for exports", "", calc.Calculate(dec));
		}

		public void TestMasterUcrCalulationScenarioImportEori()
		{
			BadgeCodeSettingCollection badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			BadgeCodeSetting badgeCodeSetting = badgeCodeSettings.AddNew();
			badgeCodeSetting.BadgeCode = "DSK";
			badgeCodeSetting.RL_PortCode = "GBLBA";
			badgeCodeSetting.Direction = "EXP";
			badgeCodeSetting.CSPCode = "CCSUK";
			badgeCodeSetting.MasterUcrCalculationMode = Registry.MucrGenerationStyles.Codes.Eori;
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings);

			MasterUCRCalculator calc = new MasterUCRCalculator();
			OrgHeader org = Factory.New<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Turn, "123456789000");
			var dec = Factory.New<JobDeclaration>();
			dec.JE_CustomsProfile = "DSK";
			dec.SubLocation = "BAC";
			dec.JE_MasterBill = "12512345678";
			dec.JE_HouseBill = "abcdefgh";  // hawbs are not usually alpha but it makes the test easier to read
			dec.JE_OA_DeclarantAddress = org.MainAddress.PK;

			// Import decs 
			dec.JE_MessageType = "IMP";
			AssertEquals("", calc.Calculate(dec));
		}

		public void TestMasterUcrCalulationScenarioCSPExportStyle()
		{
			BadgeCodeSettingCollection badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			BadgeCodeSetting badgeCodeSetting = badgeCodeSettings.AddNew();
			badgeCodeSetting.BadgeCode = "DSK";
			badgeCodeSetting.RL_PortCode = "GBLBA";
			badgeCodeSetting.Direction = "EXP";
			badgeCodeSetting.CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW;
			badgeCodeSetting.MasterUcrCalculationMode = Registry.MucrGenerationStyles.Codes.CSPExport;
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings);

			MasterUCRCalculator calc = new MasterUCRCalculator();
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "CK1";
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Turn, "123456789000");
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "EXP";
			dec.JE_CustomsProfile = "DSK";
			dec.JE_RL_NKPortOfLoading = "GBLBA";
			dec.JE_LocationOfGoods = "LBA";
			dec.JE_OA_DeclarantAddress = org.MainAddress.PK;
			dec.JE_DeclarationReference = "B0000000001";
			AssertEquals("GB/CCSUK-B0000000001", calc.Calculate(dec));

			dec.JE_DeclarationReference = "";
			AssertEquals("", calc.Calculate(dec));
			dec.JE_DeclarationReference = "B0000000001";
			AssertEquals("GB/CCSUK-B0000000001", calc.Calculate(dec));

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "GBLBA";
			consol.JK_UniqueConsignRef = "C0001000";
			var shipment = consol.Shipments.AddNew();
			dec.JE_JS = shipment.PK;
			var mawbExportInfo = GetMawbExportAddInfo(consol);
			AssertEquals("GB/CCSUK-C0001000", calc.Calculate(mawbExportInfo));
		}

		public void TestMasterUcrCalulationWhenNoDeclarant()
		{
			var ukCompany = Factory.New<GlbCompany>();
			ukCompany.GC_Code = "DAN";
			ukCompany.GC_RN_NKCountryCode = "GB";
			var ukBranch = ukCompany.Branches.AddNew();
			ukBranch.GB_Code = "LHR";
			ukBranch.GB_RL_NKHomePort = "GBLON";
			ukBranch.GB_OH_OrgProxy = ZGuid.Empty;
			Factory.Save();

			using (DisposableEnvironment.ForBranch(ukBranch.PK.ToGuid()))
			{
				var badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, ukBranch.PK.ToGuid(), Guid.Empty);
				var badgeCodeSetting = badgeCodeSettings.AddNew();
				badgeCodeSetting.BadgeCode = "DSK";
				badgeCodeSetting.RL_PortCode = "GBLBA";
				badgeCodeSetting.Direction = "EXP";
				badgeCodeSetting.CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW;
				badgeCodeSetting.MasterUcrCalculationMode = Registry.MucrGenerationStyles.Codes.Eori;
				GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings);

				var calc = new MasterUCRCalculator();
				var dec = Factory.New<JobDeclaration>();
				dec.JE_MessageType = "EXP";
				dec.JE_CustomsProfile = "DSK";
				dec.JE_RL_NKPortOfLoading = "GBLBA";
				dec.JE_LocationOfGoods = "LBA";
				dec.JE_DeclarationReference = "B0000000001";
				dec.JE_MasterBill = "MB0002";
				AssertNoExceptionThrown(() => calc.Calculate(dec));
			}
		}

		MawbExportAddInfo GetMawbExportAddInfo(ForwardingConsol consol)
		{
			wrapper = new CustomsExportConsolIntegrationWrapper(consol, new SendsMessagesToCustomsShutterUpperer());
			return wrapper.MawbExportHelper;
		}
		CustomsExportConsolIntegrationWrapper wrapper;
	}
}
