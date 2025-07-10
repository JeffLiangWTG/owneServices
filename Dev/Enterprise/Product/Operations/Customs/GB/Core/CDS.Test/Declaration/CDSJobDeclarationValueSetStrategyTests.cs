using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.CDS.Messaging.Testing;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	public class CDSJobDeclarationValueSetStrategyTests : TestCaseWithFactory
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
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_OH_Importer = orgHeader.PK;
			AssertEquals("Z", declaration.ZG_VATDeferType);
			AssertEquals(true, declaration.ZG_UsePostponedVatAccounting);
		}

		public void TestDefaultImporterChanged_PrivateIndividual()
		{
			var standardOrg = Factory.NewWithValidTestData<OrgHeader>();
			var privateOrg = OrganisationWrapperTest.CreatePrivateIndividualOrg(Factory);
			var secondPrivateOrg = OrganisationWrapperTest.CreatePrivateIndividualOrg(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			declaration.JE_OH_Importer = standardOrg.PK;
			var ai = declaration.AdditionalInfos.Find(x => x.CSI_Code == "00500").FirstOrDefault();
			AssertNull("AI record 00500 should not exist for organisation", ai);

			declaration.JE_OH_Importer = privateOrg.PK;
			AssertNoExceptionThrown("One AI record 00500 should exist for private individual", () => ai = declaration.AdditionalInfos.Find(x => x.CSI_Code == "00500").Single());
			AssertEquals("IMPORTER", ai.CSI_Description);

			declaration.JE_OH_Importer = secondPrivateOrg.PK;
			AssertNoExceptionThrown("One AI record 00500 should exist for private individual", () => ai = declaration.AdditionalInfos.Find(x => x.CSI_Code == "00500").Single());
			AssertEquals("IMPORTER", ai.CSI_Description);

			declaration.JE_OH_Importer = standardOrg.PK;
			ai = declaration.AdditionalInfos.Find(x => x.CSI_Code == "00500").FirstOrDefault();
			AssertNull("AI record 00500 should not exist for organisation", ai);
		}

		public void TestCalculateMasterUCR()
		{
			MawbTestHelper.MakeBadge("ABC", GatewayList.Codes.CDS, "CUKFFW98000", false, "", false, false, BadgeDirectionList.Codes.EXP, "GBLHR", Registry.MucrGenerationStyles.Codes.Air);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_CustomsProfile = "ABC";

			declaration.JE_MasterBill = "MB-12345678";
			AssertEquals("A:MB12345678", declaration.JE_MasterUCR);

			declaration.JE_MasterUCR = "";
			declaration.JE_HouseBill = "MB001";
			AssertEquals("A:MB12345678", declaration.JE_MasterUCR);

			declaration.JE_MasterUCR = "";
			declaration.JE_LocationOfGoods = "MB001";
			AssertEquals("A:MB12345678", declaration.JE_MasterUCR);

			declaration.JE_MasterUCR = "";
			declaration.JE_SubLocationOfGoods = "MB001";
			AssertEquals("A:MB12345678", declaration.JE_MasterUCR);

			declaration.JE_MasterUCR = "";
			declaration.JE_OH_ShippingLine = ZGuid.NewZGuid();
			AssertEquals("A:MB12345678", declaration.JE_MasterUCR);
		}

		public void TestCalculateMasterUCR_GemsCcsuk()
		{
			ShedTest.CreateShed(Factory, "GB", "LHRELX", "TORQUE LOGISTICS LIMITED at Heathrow", acpCode: "H");
			ShedTest.CreateShed(Factory, "GB", "LHRXYZ", "XYZ at Heathrow", acpCode: "H");
			MawbTestHelper.MakeBadge("ABC", GatewayList.Codes.CDS, "CUKFFW98000", makeCredentialToo: false, badgeDirection: BadgeDirectionList.Codes.IMP, portCode: "GBLHR", mucrCalculationMode: MucrGenerationStyles.Codes.GemsCcsuk, applicationCode: Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_CustomsProfile = "ABC";
			AssertEquals("Default MUCR", string.Empty, declaration.JE_MasterUCR);

			declaration.JE_SubLocationOfGoods = "LHRELX";
			declaration.JE_MasterBill = "11122222222";
			AssertEquals("MUCR should be recalculated when MasterBill changed", "HELX11122222222", declaration.JE_MasterUCR);

			declaration.JE_SubLocationOfGoods = "LHRXYZ";
			AssertEquals("MUCR should be recalculated when SubLocationOfGoods changed", "HXYZ11122222222", declaration.JE_MasterUCR);

			declaration.JE_HouseBill = "12345678";
			AssertEquals("MUCR should be recalculated when HouseBill changed", "HXYZ1112222222212345678", declaration.JE_MasterUCR);

			declaration.ZG_HouseSplitReference = "01";
			AssertEquals("MUCR should be recalculated when SplitReference changed", "HXYZ111222222221234567801", declaration.JE_MasterUCR);
		}

		public void TestCtStatusIdDefault()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

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

				declaration.JE_LocationOtherInformation = "GBAUQFMNCMANDHXCUK";
				AssertEquals("Import Location Other Information", "MANDHX", declaration.JE_SubLocationOfGoods);
			});
		}

		public void TestDefaultValueCEIStyle()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals(ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse, declaration.CusEntryInstruction.CEI_Style);

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals(ExportDeclarationTypeList.Codes.DeclarationForExport, declaration.CusEntryInstruction.CEI_Style);
		}

		public void TestDefaultEntrySubStyle()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DateOfArrival = ZDateTime.UtcNow.AddDays(-1);
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals(EntrySubStyleCodeList.Codes.A, declaration.CusEntryInstruction.CEI_SubStyle);

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = "ABC";
			declaration.JE_DateOfArrival = ZDateTime.UtcNow.AddDays(1);
			AssertEquals(EntrySubStyleCodeList.Codes.A, declaration.CusEntryInstruction.CEI_SubStyle);

			entryHeader.CH_EntryStatus = "";
			declaration.JE_DateOfArrival = ZDateTime.UtcNow.AddDays(2);
			AssertEquals(EntrySubStyleCodeList.Codes.D, declaration.CusEntryInstruction.CEI_SubStyle);
		}

		public void TestDefaultEntrySubStyleForDeclaration_FS()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertNotEquals(ImportDeclarationTypeList.Codes.FinalSupplementaryDeclaration, declaration.CusEntryInstruction.CEI_Style);
			declaration.CusEntryInstruction.CEI_SubStyle = EntrySubStyleCodeList.Codes.D;
			AssertNotEquals(ImportDeclarationTypeList.Codes.FinalSupplementaryDeclaration, declaration.CusEntryInstruction.CEI_Style);
			declaration.CusEntryInstruction.CEI_SubStyle = EntrySubStyleCodeList.Codes.Q;
			AssertEquals(ImportDeclarationTypeList.Codes.FinalSupplementaryDeclaration, declaration.CusEntryInstruction.CEI_Style);
		}

		public void TestDefaultEntrySubStyleForDeclaration_H5()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_DateOfArrival = ZDateTime.UtcNow.AddDays(2);

			declaration.CusEntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.DeclarationForGoodsFromTheSpecialFiscalTerritories;
			AssertEquals("Not arrived", EntrySubStyleCodeList.Codes.D, declaration.CusEntryInstruction.CEI_SubStyle);

			declaration.JE_DateOfArrival = ZDateTime.UtcNow.AddDays(-2);
			AssertEquals("Arrived", EntrySubStyleCodeList.Codes.A, declaration.CusEntryInstruction.CEI_SubStyle);
		}

		public void TestDefaultEntrySubStyleForDeclaration_I1()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_DateOfArrival = ZDateTime.UtcNow.AddDays(2);

			declaration.CusEntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.ImportSimplifiedDeclaration;
			AssertEquals("Not arrived", EntrySubStyleCodeList.Codes.F, declaration.CusEntryInstruction.CEI_SubStyle);

			declaration.JE_DateOfArrival = ZDateTime.UtcNow.AddDays(-2);
			AssertEquals("Arrived", EntrySubStyleCodeList.Codes.C, declaration.CusEntryInstruction.CEI_SubStyle);
		}

		public void TestDefaultDeclarationForEntrySubStyle_Q()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertNotEquals(EntrySubStyleCodeList.Codes.Q, declaration.CusEntryInstruction.CEI_SubStyle);
			declaration.CusEntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.ImportClearanceRequestC21I;
			AssertNotEquals(EntrySubStyleCodeList.Codes.Q, declaration.CusEntryInstruction.CEI_SubStyle);
			declaration.CusEntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.FinalSupplementaryDeclaration;
			AssertEquals(EntrySubStyleCodeList.Codes.Q, declaration.CusEntryInstruction.CEI_SubStyle);
		}

		public void TestProfileIsSelectedWhenChangingLoadPortForExportJobs()
		{
			MawbTestHelper.MakeBadge("ABC", GatewayList.Codes.CDS, "CUKFFW98000", false, "", false, false, BadgeDirectionList.Codes.EXP, "GBLHR");
			MawbTestHelper.MakeBadge("DEF", GatewayList.Codes.CDS, "CUKFFW98001", false, "", false, false, BadgeDirectionList.Codes.IMP, "GBLHR");
			MawbTestHelper.MakeBadge("GHI", GatewayList.Codes.CDS, "CUKFFW98002", false, "", false, false, BadgeDirectionList.Codes.EXP, "GBMAN");
			MawbTestHelper.MakeBadge("JKL", GatewayList.Codes.CDS, "CUKFFW980003", false, "", true, false, BadgeDirectionList.Codes.EXP, "GBLHR");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
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
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_RL_NKPortOfArrival = "GBLHR";
			AssertEquals("JKL", declaration.JE_CustomsProfile);

			declaration.JE_RL_NKPortOfArrival = "GBMAN";
			AssertEquals("GHI", declaration.JE_CustomsProfile);
		}

		public void TestDefaultImporterChanged_WhenCountryIsNotInEU()
		{
			var foreignCompany = Factory.NewWithValidTestData<GlbCompany>();
			foreignCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.ZG_UsePostponedVatAccounting = true;
			var additionalInfo = declaration.AdditionalInfos.AddNew();
			additionalInfo.CSI_Code = "1001";
			declaration.JE_GC = foreignCompany.PK;

			var standardOrg = Factory.NewWithValidTestData<OrgHeader>();
			((EUOrgImpAddInfo)standardOrg.CountryData.RegionSpecificImpAddInfo).ZO_UseFr3FiscalRepresentation = false;

			var privateOrg = OrganisationWrapperTest.CreatePrivateIndividualOrg(Factory);
			((EUOrgImpAddInfo)privateOrg.CountryData.RegionSpecificImpAddInfo).ZO_UseFr3FiscalRepresentation = false;

			declaration.JE_OH_Importer = standardOrg.PK;
			var ai = declaration.AdditionalInfos.Find(x => x.CSI_Code == "IMPORTER").FirstOrDefault();
			AssertNull("AI record IMPORTER should not exist for organisation", ai);
			Assert("Fiscal representation should remain true", declaration.ZG_UsePostponedVatAccounting);
			ai = declaration.AdditionalInfos.Find(x => x.CSI_Code == "1001").FirstOrDefault();
			AssertNotNull("AI record 1001 should remain for organisation", ai);

			declaration.JE_OH_Importer = privateOrg.PK;
			ai = declaration.AdditionalInfos.Find(x => x.CSI_Code == "00500").FirstOrDefault();
			AssertNull("AI record 00500 should not exist for organisation", ai);
			Assert("Fiscal representation should remain true", declaration.ZG_UsePostponedVatAccounting);
			ai = declaration.AdditionalInfos.Find(x => x.CSI_Code == "1001").FirstOrDefault();
			AssertNotNull("AI record 1001 should remain for organisation", ai);
		}

		public void TestSupplierChanged_WhenCountryIsNotInEU()
		{
			var foreignCompany = Factory.NewWithValidTestData<GlbCompany>();
			foreignCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarantType = "IND";
			declaration.JE_GC = foreignCompany.PK;

			var standardOrg = Factory.NewWithValidTestData<OrgHeader>();
			((EUOrgImpAddInfo)standardOrg.CountryData.RegionSpecificImpAddInfo).ZO_Box14UseIndirectRepresentation = false;

			declaration.JE_OH_Supplier = standardOrg.PK;
			AssertEquals("Declaration type should remain the same", "IND", declaration.JE_DeclarantType);
		}

		public void TestDefaultingOfOfficeOfExitBasedOnSuppliersRelatedPartyOfTypeCustomsOffice()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var org1WithRelatedPartyWithOfficeCusCode = Factory.NewWithValidTestData<OrgHeader>();
				var org2WithRelatedPartyOfWrongType = Factory.NewWithValidTestData<OrgHeader>();
				var org3WithNoRelatedParties = Factory.NewWithValidTestData<OrgHeader>();
				var org4WithRelatedPartyWithNoOfficeCusCode = Factory.NewWithValidTestData<OrgHeader>();
				var org5 = Factory.NewWithValidTestData<OrgHeader>();

				var org1RelatedParty1 = org1WithRelatedPartyWithOfficeCusCode.AllRelatedParties.AddNew();
				org1RelatedParty1.PR_PartyType = RelatedPartyTypeList.Codes.CustomsPostponedVatAccounting;
				org1RelatedParty1.PR_OH_RelatedParty = org3WithNoRelatedParties.PK;

				var org1RelatedParty2 = org1WithRelatedPartyWithOfficeCusCode.AllRelatedParties.AddNew();
				org1RelatedParty2.PR_PartyType = RelatedPartyTypeList.Codes.CustomsOffice;
				org1RelatedParty2.PR_OH_RelatedParty = org4WithRelatedPartyWithNoOfficeCusCode.PK;
				org4WithRelatedPartyWithNoOfficeCusCode.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "1234");
				org4WithRelatedPartyWithNoOfficeCusCode.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.CustomsOfficeForExit, "NL999999", "NL");
				org4WithRelatedPartyWithNoOfficeCusCode.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.CustomsOfficeForExit, "GB999999", "GB");

				var org2RelatedParty1 = org2WithRelatedPartyOfWrongType.AllRelatedParties.AddNew();
				org2RelatedParty1.PR_PartyType = RelatedPartyTypeList.Codes.CustomsPostponedVatAccounting;
				org2RelatedParty1.PR_OH_RelatedParty = org4WithRelatedPartyWithNoOfficeCusCode.PK;

				var org4RelatedParty1 = org4WithRelatedPartyWithNoOfficeCusCode.AllRelatedParties.AddNew();
				org4RelatedParty1.PR_PartyType = RelatedPartyTypeList.Codes.CustomsOffice;
				org4RelatedParty1.PR_OH_RelatedParty = org5.PK;
				org5.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "1234");

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				ChangeSupplierAndAssertResult(declaration, org1WithRelatedPartyWithOfficeCusCode, "Set default office of exit to supplier's related party and get it's office cuscode", "", "GB999999");
				declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
				ChangeSupplierAndAssertResult(declaration, org1WithRelatedPartyWithOfficeCusCode, "No defaulting - CHIEF", "", "");
				declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				ChangeSupplierAndAssertResult(declaration, org1WithRelatedPartyWithOfficeCusCode, "No defaulting - office value not empty", "GB123456", "GB123456");
				ChangeSupplierAndAssertResult(declaration, org3WithNoRelatedParties, "No defaulting - org3 has no related parties", "", "");
				ChangeSupplierAndAssertResult(declaration, org2WithRelatedPartyOfWrongType, "No defaulting - org2 has related party of wrong type (not COF)", "", "");
				ChangeSupplierAndAssertResult(declaration, org4WithRelatedPartyWithNoOfficeCusCode, "No defaulting - org4 has valid related party but it has no office cuscode", "", "");
			}
		}

		void ChangeSupplierAndAssertResult(JobDeclaration declaration, OrgHeader org, ZString message, ZString officeCode, ZString result)
		{
			declaration.JE_CustomsOffice = officeCode;
			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.JE_OH_Supplier = org.PK;
			AssertEquals(message, result, declaration.JE_CustomsOffice);
		}
	}
}
