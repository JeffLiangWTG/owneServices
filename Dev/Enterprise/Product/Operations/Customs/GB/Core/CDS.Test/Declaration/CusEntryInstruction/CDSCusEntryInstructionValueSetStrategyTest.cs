using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CDS.CodeDescriptionPairLists;
using Enterprise.Customs.GB.CDS.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using CusEntryInstruction = Enterprise.Customs.GB.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.GB.CDS.Declaration.Testing
{
	sealed class CDSCusEntryInstructionValueSetStrategyTest : TestCaseWithFactory
	{
		public void TestFillAuthorisationFromStyleAndSubStyle()
		{
			var importer1 = OrganisationWrapperTest.CreateOrgHeaderWithEori(Factory, "AR1", Core.Constants.CountryCodes.UnitedKingdom);
			importer1.OH_Code = "IMP001";

			var importer2 = OrganisationWrapperTest.CreateOrgHeaderWithEori(Factory, "AR2", Core.Constants.CountryCodes.UnitedKingdom);
			importer2.OH_Code = "IMP001";

			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			dec.JE_OH_Importer = importer1.PK;
			dec.JE_MessageType = "IMP";
			var cei = dec.CusEntryInstruction;

			cei.CEI_Style = ZString.Empty;
			cei.CEI_SubStyle = ZString.Empty;

			AssertEquals("Pre-req - No Authorisations", 0, cei.CusAuthorizationUsages.Count);

			cei.CEI_Style = "I1";
			cei.CEI_SubStyle = "C";

			CombineAssertions("Fill first Auth", () =>
			{
				AssertEquals("1 Authorisation", 1, cei.CusAuthorizationUsages.Count);
				var auth = cei.CusAuthorizationUsages[0];
				AssertEquals("Auth Type", CDSAuthorisationHeaderTypeList.Codes.SimplifiedDeclaration, auth.AGC_Code);
				AssertEquals("Auth Owner", importer1.PK, auth.AGC_OH_Owner);

				auth.AGC_OH_Owner = importer2.PK;
			});

			CombineAssertions("Do not replace existing auths", () =>
			{
				cei.CEI_SubStyle = "F";
				AssertEquals("1 Authorisation", 1, cei.CusAuthorizationUsages.Count);
				var auth = cei.CusAuthorizationUsages[0];
				AssertEquals("Auth Type", CDSAuthorisationHeaderTypeList.Codes.SimplifiedDeclaration, auth.AGC_Code);
				AssertEquals("Auth Owner - no change expected", importer2.PK, auth.AGC_OH_Owner);
			});
		}

		public void TestEntryStyleAuthorisationMapping()
		{
			var importerPK = OrganisationWrapperTest.CreateOrgHeaderWithEori(Factory, "AR1", Core.Constants.CountryCodes.UnitedKingdom).PK;
			var importerPKWithoutEORI = Factory.NewWithValidTestData<OrgHeader>().PK;

			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			var cei = dec.CusEntryInstruction;
			CombineAssertions("Mappings", () =>
			{
				AssertMapping(cei, "I1", EntrySubStyleListImport.Codes.SdpSfdGoodsArrived, ZString.Empty, ZGuid.Empty, "No Mapping");
				AssertMapping(cei, "I1", EntrySubStyleListImport.Codes.SdpSfdGoodsNotArrived, ZString.Empty, ZGuid.Empty, "No Mapping");
				AssertMapping(cei, "I1", EntrySubStyleListImport.Codes.FinalSupplementaryDeclaration, ZString.Empty, ZGuid.Empty, "No Mapping");
			});

			dec.JE_OH_Importer = importerPKWithoutEORI;
			CombineAssertions("Mappings", () =>
			{
				AssertMapping(cei, "I1", EntrySubStyleListImport.Codes.SdpSfdGoodsArrived, ZString.Empty, ZGuid.Empty, "No Mapping");
				AssertMapping(cei, "I1", EntrySubStyleListImport.Codes.SdpSfdGoodsNotArrived, ZString.Empty, ZGuid.Empty, "No Mapping");
				AssertMapping(cei, "I1", EntrySubStyleListImport.Codes.FinalSupplementaryDeclaration, ZString.Empty, ZGuid.Empty, "No Mapping");
			});

			dec.JE_OH_Importer = importerPK;
			CombineAssertions("Mappings", () =>
			{
				AssertMapping(cei, "I1", EntrySubStyleListImport.Codes.SdpSfdGoodsArrived, GBCommonConstants.AuthorisationTypeCodes.SDE, importerPK, "I1,C => SDE Importer");
				AssertMapping(cei, "I1", EntrySubStyleListImport.Codes.SdpSfdGoodsNotArrived, GBCommonConstants.AuthorisationTypeCodes.SDE, importerPK, "I1,F => SDE Importer");
				AssertMapping(cei, "I1", EntrySubStyleListImport.Codes.FinalSupplementaryDeclaration, ZString.Empty, ZGuid.Empty, "No Mapping");
			});
		}

		void AssertMapping(CusEntryInstruction cei, ZString style, ZString subStyle, ZString expectedCode, ZGuid expectedOwner, string mappingName)
		{
			cei.CEI_Style = style;
			cei.CEI_SubStyle = subStyle;

			var actual = CDSCusEntryInstructionValueSetStrategy.GetAuthorisationMapping(cei);

			AssertEquals($"{mappingName}: Code", expectedCode, actual.code);
			AssertEquals($"{mappingName}: Owner", expectedOwner, actual.owner);
		}

		public void TestArrivedFroniterSubstyleCodes()
		{
			AssertEquals(4, CDSJobDeclarationValueSetStrategy.ArrivedFroniterSubstyleCodes.Count());
			Assert(CDSJobDeclarationValueSetStrategy.ArrivedFroniterSubstyleCodes.Contains(EntrySubStyleCodeList.Codes.A));
			Assert(CDSJobDeclarationValueSetStrategy.ArrivedFroniterSubstyleCodes.Contains(EntrySubStyleCodeList.Codes.B));
			Assert(CDSJobDeclarationValueSetStrategy.ArrivedFroniterSubstyleCodes.Contains(EntrySubStyleCodeList.Codes.J));
			Assert(CDSJobDeclarationValueSetStrategy.ArrivedFroniterSubstyleCodes.Contains(EntrySubStyleCodeList.Codes.C));
		}

		public void TestCreateEXRRAuthorisationForArrivedROROExports()
		{
			AssertEquals("Pre-requisite: Default rego CDSEnableEXRRAutomationForArrivedROROExports should be true",
				expected: true,
				Registry.GBCustomsDataRegistry.Instance.CDSEnableEXRRAutomationForArrivedROROExports.Value);

			var supplierPK = OrganisationWrapperTest.CreateOrgHeaderWithEori(Factory, "123", Core.Constants.CountryCodes.UnitedKingdom).PK;
			var supplierPKWithoutEORI = Factory.NewWithValidTestData<OrgHeader>().PK;

			SetupDeclarationAndAssert(supplierPK,
						Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services,
						MessageTypeList.Codes.Export,
						GBTransportTypeList.Codes.ROR,
						EntrySubStyleCodeList.Codes.A,
						"New EXRR usage created",
						assertion: true);
			SetupDeclarationAndAssert(supplierPKWithoutEORI,
						Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services,
						MessageTypeList.Codes.Export,
						GBTransportTypeList.Codes.ROR,
						EntrySubStyleCodeList.Codes.A,
						"No EXRR usage created: No EORI for the supplier",
						assertion: false);
			SetupDeclarationAndAssert(ZGuid.Empty,
									Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services,
									MessageTypeList.Codes.Export,
									GBTransportTypeList.Codes.ROR,
									EntrySubStyleCodeList.Codes.A,
									"No EXRR usage: No supplier set",
									assertion: false);
			SetupDeclarationAndAssert(supplierPK,
									Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF,
									MessageTypeList.Codes.Export,
									GBTransportTypeList.Codes.ROR,
									EntrySubStyleCodeList.Codes.A,
									"No EXRR usage: CHIEF dec",
									assertion: false);
			SetupDeclarationAndAssert(supplierPK,
									Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services,
									MessageTypeList.Codes.Import,
									GBTransportTypeList.Codes.ROR,
									EntrySubStyleCodeList.Codes.A,
									"No EXRR usage: Not an export",
									assertion: false);
			SetupDeclarationAndAssert(supplierPK,
									Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services,
									MessageTypeList.Codes.Export,
									TransportTypeList.Codes.Air,
									EntrySubStyleCodeList.Codes.A,
									"No EXRR usage: Not RORO",
									assertion: false);
			SetupDeclarationAndAssert(supplierPK,
									Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services,
									MessageTypeList.Codes.Export,
									GBTransportTypeList.Codes.ROR,
									"Q",
									"No EXRR usage: Not an arrived SubStyle",
									assertion: false);
		}

		CusEntryInstruction SetupDeclarationAndAssert(ZGuid supplierPK, ZString appCode, ZString messageType, ZString transportType, ZString subStyle, ZString message, ZBool assertion)
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = appCode;
			dec.JE_OH_Supplier = supplierPK;
			dec.JE_MessageType = messageType;
			dec.JE_TransportMode = transportType;
			var cei = dec.CusEntryInstruction;
			cei.CEI_SubStyle = subStyle;
			AssertEquals(message, assertion ? 1 : 0, cei.CusAuthorizationUsages.Count);
			dec.CusEntryInstruction.CEI_SubStyle = subStyle;
			AssertEquals("CusAuthorization Usage " + (assertion ? "expected" : "not expected"), assertion ? 1 : 0, dec.CusEntryInstruction.CusAuthorizationUsages.Count);
			if (dec.CusEntryInstruction.CusAuthorizationUsages.Count > 0)
			{
				var usage = cei.CusAuthorizationUsages[0];
				AssertEquals("1 usage: type EXRR", CDSAuthorisationHeaderTypeList.Codes.CustomsSupervisedExportsAtARoroLocation, usage.AGC_Code);
				AssertEquals("1 usage: owner set to supplier", supplierPK, usage.AGC_OH_Owner);
			}
			return cei;
		}

		public void TestValueSetDefaultEXRRAuthorisationForArrivedROROExports()
		{
			var supplierPK = OrganisationWrapperTest.CreateOrgHeaderWithEori(Factory, "123", Core.Constants.CountryCodes.UnitedKingdom).PK;
			var cei = SetupDeclarationAndAssert(supplierPK,
						Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services,
						MessageTypeList.Codes.Export,
						GBTransportTypeList.Codes.ROR,
						EntrySubStyleCodeList.Codes.A,
						"New EXRR usage created",
						assertion: true);

			cei.CusAuthorizationUsages.RemoveAndDeleteAll();

			cei.CEI_Style = "I2";
			AssertEquals("Changed CEI_Style", 0, cei.CusAuthorizationUsages.Count);

			cei.CEI_SubStyle = "B";
			AssertEquals("Changed CEI_SubStyle", 1, cei.CusAuthorizationUsages.Count);
		}
	}
}
