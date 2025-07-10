using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using GBCodeList = Enterprise.Customs.GB.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.Chief.Declaration.Testing
{
	public class ChiefJobDeclarationValueSetStrategyTests : TestCaseWithFactory
	{
		public void TestDefaultImporterChanged()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var host = Factory.New<OrgCountryData>();
			host.OV_OH_OrgHeader = orgHeader.PK;
			host.OV_RN_NKClientCountryRelation = "GB";

			var obj = new GBOrgImpAddInfo((ZPropertyInfoString)host.OV_ImportCustomsDefaultAddInfoInfo);
			obj.ZO_VATDeferType = "Z";

			var obj2 = new EUOrgImpAddInfo((ZPropertyInfoString)host.OV_CustomsEconomicGroupAddInfoInfo);
			obj2.ZO_UseFr3FiscalRepresentation = true;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			declaration.JE_OH_Importer = orgHeader.PK;
			AssertEquals("Z", declaration.ZG_VATDeferType);
			AssertEquals(true, declaration.ZG_UsePostponedVatAccounting);
		}

		public void TestCtStatusIdDefault()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;

			// Brexit means brexit
			RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.UnitedKingdom).RN_EconomicGrouping = "";

			var belfast = new RefUNLOCO.Loader(Factory).Load("GBBEL");
			if (belfast.CountryStates == null || string.Compare(belfast.CountryStates.RW_RegionName, "NORTHERN IRELAND", true) != 0)
			{
				var ni = Factory.New<RefCountryStates>();
				belfast.RL_RW = ni.PK;
				ni.RW_RegionName = "NORTHERN IRELAND";
			}

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				declaration.JE_RL_NKOrigin = "GBBEL";
				declaration.JE_RL_NKFinalDestination = "USATL";
				AssertEquals("NI Export Is Not EU destination", ExportCommunityTransitStatusList.Codes.X, declaration.ZG_CTStatusID);
				declaration.JE_RL_NKFinalDestination = "GBLHR";
				AssertEquals("NI Export Is GB mainland destination", ExportCommunityTransitStatusList.Codes.X, declaration.ZG_CTStatusID);
				declaration.JE_RL_NKFinalDestination = "ESBCN";
				AssertEquals("NI Export Is EU Destination", ExportCommunityTransitStatusList.Codes.C, declaration.ZG_CTStatusID);

				declaration.JE_RL_NKFinalDestination = "IMDGS";
				AssertEquals("NI Export Is EU Special Territory", ExportCommunityTransitStatusList.Codes.TF, declaration.ZG_CTStatusID);
				declaration.JE_RL_NKFinalDestination = "ZZZZZ";
				AssertEquals("NI Export Invalid Final Destination", ZString.Empty, declaration.ZG_CTStatusID);

				declaration.JE_RL_NKOrigin = "GBLHR";
				declaration.JE_RL_NKFinalDestination = "USATL";
				AssertEquals("GB Export Is Not EU destination", ExportCommunityTransitStatusList.Codes.X, declaration.ZG_CTStatusID);
				declaration.JE_RL_NKFinalDestination = "ESBCN";
				AssertEquals("GB Export Is EU Destination", ExportCommunityTransitStatusList.Codes.X, declaration.ZG_CTStatusID);

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				declaration.JE_RL_NKFinalDestination = "LVRIX";
				declaration.JE_RL_NKOrigin = "USATL";
				AssertEquals("Import Is Not EU origin", ZString.Empty, declaration.ZG_CTStatusID);
				declaration.JE_RL_NKOrigin = "GBLHR";
				AssertEquals("Import Is EU origin", ZString.Empty, declaration.ZG_CTStatusID);
				declaration.JE_RL_NKOrigin = "IMDGS";
				AssertEquals("Import Is EU Special Territory", "T2LF", declaration.ZG_CTStatusID);
				declaration.JE_RL_NKOrigin = "SMSAI";
				AssertEquals("Import Country is San Marino", "T2LSM", declaration.ZG_CTStatusID);
				declaration.JE_RL_NKOrigin = "ZZZZZ";
				AssertEquals("Import Invalid Origin", ZString.Empty, declaration.ZG_CTStatusID);
			});
		}

		public void TestDefaultValueMessageType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			AssertEquals(declaration.JE_DeclarationType, ImportSADDeclarationTypeList.Codes.ImportFullDeclaration);
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals(declaration.JE_DeclarationType, ExportSADDeclarationTypeList.Codes.ExportFullDeclaration);
		}

		public void TestProfileIsSelectedWhenChangingLoadPortForExportJobs()
		{
			MawbTestHelper.MakeBadge("ABC", GatewayList.Codes.CDS, "CUKFFW98000", false, "", false, false, BadgeDirectionList.Codes.EXP, "GBLHR");
			MawbTestHelper.MakeBadge("DEF", GatewayList.Codes.CDS, "CUKFFW98001", false, "", false, false, BadgeDirectionList.Codes.IMP, "GBLHR");
			MawbTestHelper.MakeBadge("GHI", GatewayList.Codes.CDS, "CUKFFW98002", false, "", false, false, BadgeDirectionList.Codes.EXP, "GBMAN");
			MawbTestHelper.MakeBadge("JKL", GatewayList.Codes.CDS, "CUKFFW980003", false, "", true, false, BadgeDirectionList.Codes.EXP, "GBLHR");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_RL_NKPortOfLoading = "GBLHR";
			AssertEquals("JKL", declaration.JE_CustomsProfile);

			declaration.JE_RL_NKPortOfLoading = "GBMAN";
			AssertEquals("GHI", declaration.JE_CustomsProfile);
		}

		public void TestProfileIsSelectedWhenChangingArrivalPortForImportJobs()
		{
			MawbTestHelper.MakeBadge("ABC", GatewayList.Codes.CDS, "CUKFFW98000", false, "", false, false, BadgeDirectionList.Codes.IMP, "GBLHR");
			MawbTestHelper.MakeBadge("DEF", GatewayList.Codes.CDS, "CUKFFW98001", false, "", false, false, BadgeDirectionList.Codes.EXP, "GBLHR");
			MawbTestHelper.MakeBadge("GHI", GatewayList.Codes.CDS, "CUKFFW98002", false, "", false, false, BadgeDirectionList.Codes.IMP, "GBMAN");
			MawbTestHelper.MakeBadge("JKL", GatewayList.Codes.CDS, "CUKFFW980003", false, "", true, false, BadgeDirectionList.Codes.IMP, "GBLHR");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_RL_NKPortOfArrival = "GBLHR";
			AssertEquals("JKL", declaration.JE_CustomsProfile);

			declaration.JE_RL_NKPortOfArrival = "GBMAN";
			AssertEquals("GHI", declaration.JE_CustomsProfile);
		}

		public void TestIsGvmsIsSetWhenLocationOfGoodsOrSubLocationChanged()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Port Code");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT001", "Test Port 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "GvmsPortId", "Gvms Port Id");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			Assert("Port is not GVMS", !declaration.JE_IsGvmsPort);
			declaration.JE_LocationOfGoods = "PORT001";
			Assert("Port is GVMS", declaration.JE_IsGvmsPort);
		}

		public void TestAdditionalInfoIsRemovedWhenExplicitlySet()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Port Code");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT001", "Test Port 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "GvmsPortId", "Gvms Port Id");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			Assert("Additional Info RRS01 not present", !declaration.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == "RRS01"));
			declaration.JE_IsGvmsPort = false;
			declaration.JE_MasterUCR = "";
			var addInfo = declaration.AdditionalInfos.AddNew();
			addInfo.CSI_Code = GBCommonConstants.AdditonalInfoCodes.RRS01;
			Assert("Additional Info RRS01 present", declaration.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == "RRS01"));

			declaration.JE_MasterUCR = "UCR1234";
			Assert("Additional Info RRS01 not present because it is automatically removed", !declaration.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == "RRS01"));
		}

		public void TestShouldDefaultRRS01()
		{
			//SetUp combinations of the following fields:
			SetupAndAssertShouldDefaultRRS01(string.Empty, true, false, false, false, "Should not default - IsIFD, IsEFD, IsESP are all false", false);
			SetupAndAssertShouldDefaultRRS01(string.Empty, true, true, false, false, "ShouldDefaultRRS01", true);
			SetupAndAssertShouldDefaultRRS01(string.Empty, true, false, true, false, "ShouldDefaultRRS01", true);
			SetupAndAssertShouldDefaultRRS01(string.Empty, true, false, false, true, "ShouldDefaultRRS01", true);
			SetupAndAssertShouldDefaultRRS01(string.Empty, false, false, false, false, "Should not default - IsGoodsNotArrivedSubStyle, IsIFD, IsEFD, IsESP are all false", false);
			SetupAndAssertShouldDefaultRRS01(string.Empty, false, true, false, false, "Should not default - IsGoodsNotArrivedSubStyle is false", false);
			SetupAndAssertShouldDefaultRRS01(string.Empty, false, false, true, false, "Should not default - IsGoodsNotArrivedSubStyle is false", false);
			SetupAndAssertShouldDefaultRRS01(string.Empty, false, false, false, true, "Should not default - IsGoodsNotArrivedSubStyle is false", false);

			SetupAndAssertShouldDefaultRRS01("MUCR", true, false, false, false, "Should not default - MUCR not empty, IsIFD, IsEFD, IsESP are all false", false);
			SetupAndAssertShouldDefaultRRS01("MUCR", true, true, false, false, "Should not default - MUCR not empty", false);
			SetupAndAssertShouldDefaultRRS01("MUCR", true, false, true, false, "Should not default - MUCR not empty", false);
			SetupAndAssertShouldDefaultRRS01("MUCR", true, false, false, true, "Should not default - MUCR not empty", false);
			SetupAndAssertShouldDefaultRRS01("MUCR", false, false, false, false, "Should not default - MUCR not empty, IsGoodsNotArrivedSubStyle, IsIFD, IsEFD, IsESP are all false", false);
			SetupAndAssertShouldDefaultRRS01("MUCR", false, true, false, false, "Should not default - MUCR not empty, IsGoodsNotArrivedSubStyle is false", false);
			SetupAndAssertShouldDefaultRRS01("MUCR", false, false, true, false, "Should not default - MUCR not empty, IsGoodsNotArrivedSubStyle is false", false);
			SetupAndAssertShouldDefaultRRS01("MUCR", false, false, false, true, "Should not default - MUCR not empty, IsGoodsNotArrivedSubStyle is false", false);
		}

		void SetupAndAssertShouldDefaultRRS01(string masterUCR, bool isGoodsNotArrivedSubStyle, bool isIFD, bool isEFD, bool isESP, string assertionDescritpion, bool assertResult)
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MasterUCR = masterUCR;
			dec.JE_DeclarationType = "XXX";
			if (isIFD)
			{
				dec.JE_MessageType = JobMessageTypeList.Codes.Import;
				dec.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportFullDeclaration;
			}
			if (isEFD)
			{
				dec.JE_MessageType = JobMessageTypeList.Codes.Export;
				dec.JE_DeclarationType = ExportSADDeclarationTypeList.Codes.ExportFullDeclaration;
			}
			if (isESP)
			{
				dec.JE_MessageType = JobMessageTypeList.Codes.Export;
				dec.JE_DeclarationType = ExportSADDeclarationTypeList.Codes.ExportSDPPreShipment;
			}
			dec.JE_EntrySubStyle = "X";
			if (isGoodsNotArrivedSubStyle)
			{
				dec.JE_EntrySubStyle = GBCodeList.EntrySubStyleListExport.Codes.C21_GoodsNotArrived_IECR;
			}
			AssertEquals(assertionDescritpion, assertResult, dec.ShouldDefaultRRS01);
		}

		public void TestDefaultImporterChanged_WhenCountryIsNotInEU()
		{
			var foreignCompany = Factory.NewWithValidTestData<GlbCompany>();
			foreignCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var host = Factory.New<OrgCountryData>();
			host.OV_OH_OrgHeader = orgHeader.PK;
			host.OV_RN_NKClientCountryRelation = "GB";

			var obj = new GBOrgImpAddInfo((ZPropertyInfoString)host.OV_ImportCustomsDefaultAddInfoInfo);
			obj.ZO_VATDeferType = "Z";

			var obj2 = new EUOrgImpAddInfo((ZPropertyInfoString)host.OV_CustomsEconomicGroupAddInfoInfo);
			obj2.ZO_UseFr3FiscalRepresentation = true;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			declaration.JE_GC = foreignCompany.PK;
			declaration.ZG_UsePostponedVatAccounting = false;

			declaration.JE_OH_Importer = orgHeader.PK;
			AssertEquals("Postponed VAT accounting should remain the same", false, declaration.ZG_UsePostponedVatAccounting);
		}

		public void TestSupplierChanged_WhenCountryIsNotInEU()
		{
			var foreignCompany = Factory.NewWithValidTestData<GlbCompany>();
			foreignCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			declaration.JE_DeclarantType = "IND";
			declaration.JE_GC = foreignCompany.PK;

			var standardOrg = Factory.NewWithValidTestData<OrgHeader>();
			((EUOrgImpAddInfo)standardOrg.CountryData.RegionSpecificImpAddInfo).ZO_Box14UseIndirectRepresentation = false;

			declaration.JE_OH_Supplier = standardOrg.PK;
			AssertEquals("Declaration type should remain the same", "IND", declaration.JE_DeclarantType);
		}
	}
}
