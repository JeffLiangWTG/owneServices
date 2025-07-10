using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.MasterFiles;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	sealed class VATDeferStrategyBaseOnlyTest : TestCaseWithFactory
	{
		public void TestVATDeferNumberShouldNotBeDefaultedWhenOrganisationChangedForExport()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "DECLARANT";
			declarant.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.TVA, "TVA0", declaration.CountryCode);
			var frOrgImpAddInfo0 = FROrgImpAddInfo.Get(declarant);
			frOrgImpAddInfo0.ZO_VATDeferType = VATProcedureList.Codes.L;

			Factory.Save();

			AssertEquals("Prerequisite to next assertion", ZString.Empty, declaration.ZG_VATDeferNumber);

			declaration.JE_OA_DeclarantAddress = declarant.Addresses[0].PK;
			AssertEquals("Declarant has changed, but it should have no effect on deferment number calculation for an export declaration.", ZString.Empty, declaration.ZG_VATDeferNumber);
		}

		public void TestVATDeferNumberCalculation_WhenVATDeferTypeIsSetTo_S()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.ZG_VATDeferNumber = "PLACEHOLDER";

			declaration.ZG_VATDeferType = VATProcedureList.Codes.S;
			Assert("VATDeferNumber should be set to empty when VATDeferType is set to S.", declaration.ZG_VATDeferNumber.IsEmpty);
		}

		public void TestVATDeferNumberCalculation_WhenVATDeferTypeIsSetTo_2()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "IMPORTER";
			SetUpGuaranteeHeader(importer.PK, "IMPORTER_AI2");

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			SetUpGuaranteeHeader(declarant.PK, "DECLARANT_AI2");

			var frOrgImpAddInfo = FROrgImpAddInfo.Get(importer);
			frOrgImpAddInfo.ZO_VATDeferType = VATProcedureList.Codes._2;
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;

			AssertEquals("Prerequisite: VATDeferType is set to 2.", VATProcedureList.Codes._2, declaration.ZG_VATDeferType);
			AssertEquals("When ZO_VATDeferType is not empty, Deferment number should be the first importer valid AI2 guarantee number.", "IMPORTER_AI2", declaration.ZG_VATDeferNumber);

			void SetUpGuaranteeHeader(ZGuid permitHolder, ZString number)
			{
				var guarantee = Factory.New<CusGuaranteeHeader>();
				guarantee.CPH_OH_PermitHolder = permitHolder;
				guarantee.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
				guarantee.CPH_StartDate = ZDate.Today;
				guarantee.CPH_Type = GuaranteeTypeList.Codes.AI2;
				guarantee.CPH_Number = number;
			}
		}

		public void TestVATDeferNumberCalculation_WhenVATDeferTypeIsSetTo_L_DELTAIE()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "IMPORTER";
			importer.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.TVA, "TVA_Importer", Core.Constants.CountryCodes.France);

			var frOrgImpAddInfo = FROrgImpAddInfo.Get(importer);
			frOrgImpAddInfo.ZO_VATDeferType = VATProcedureList.Codes.L;
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var fiscalReference = entryInstruction.FiscalReferences.AddNew();
			fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR3_TaxRepresentative;
			fiscalReference.CFR_Reference = "FR3_FiscalReference";
			Factory.Save();

			EUOrgImpAddInfo.Get(importer, Core.Constants.CountryCodes.France).ZO_UseFr3FiscalRepresentation = true;
			Factory.Save();

			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
			relatedParty.OH_Code = "RelatedParty";
			importer.AddRelatedParty(relatedParty.PK, RelatedPartyTypeList.Codes.CustomsPostponedVatAccounting, EU.Business.MessageTypeList.Codes.Import, ZString.Empty, ZString.Empty, GlbCompany.CurrentCompany);
			relatedParty.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.TVA, "TVA_RelatedParty", Core.Constants.CountryCodes.France);

			Assert("Prerequisite", declaration.IsDeltaIE);
			AssertEquals("Prerequisite", VATProcedureList.Codes.L, declaration.ZG_VATDeferType);

			declaration.VATDeferStrategy.OnVATDeferTypeChanged();
			AssertEquals("(DeltaIE only) When ZO_UseFr3FiscalRepresentation is true for importer, a related party of CPV exists, and TVA is configured for the related party.", "TVA_RelatedParty", declaration.ZG_VATDeferNumber);

			EUOrgImpAddInfo.Get(importer, Core.Constants.CountryCodes.France).ZO_UseFr3FiscalRepresentation = false;
			Factory.Save();
			declaration.VATDeferStrategy.OnVATDeferTypeChanged();
			AssertEquals("(DeltaIE only) When ZO_UseFr3FiscalRepresentation is false, fallback to retrieve FR3 FiscalReference of entryInstructions.", "FR3_FiscalReference", declaration.ZG_VATDeferNumber);

			fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR2_Customer;
			declaration.VATDeferStrategy.OnVATDeferTypeChanged();
			AssertEquals("(DeltaIE only) When no FR3 FiscalReference exists, fallback to TVA from importer.", "TVA_Importer", declaration.ZG_VATDeferNumber);
		}

		public void TestVATDeferNumberCalculation_WhenVATDeferTypeIsSetTo_L_DELTAG()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "IMPORTER";

			var frOrgImpAddInfo = FROrgImpAddInfo.Get(importer);
			frOrgImpAddInfo.ZO_VATDeferType = VATProcedureList.Codes.L;
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			Assert("Prerequisite", declaration.IsDeltaG);
			AssertEquals("Prerequisite", VATProcedureList.Codes.L, declaration.ZG_VATDeferType);

			declaration.VATDeferStrategy.OnVATDeferTypeChanged();
			AssertEquals("(DeltaG only) VAT defer number for autoliquidation should be empty when the importer has no VAT configured", ZString.Empty, declaration.ZG_VATDeferNumber);

			importer.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.TVA, "TVA_Importer", Core.Constants.CountryCodes.France);
			declaration.VATDeferStrategy.OnVATDeferTypeChanged();
			AssertEquals("(DeltaG only) VAT defer number should be defaulted with importer VAT number for Autoliquidation.", "TVA_Importer", declaration.ZG_VATDeferNumber);
		}

		public void TestVATDeferTypeIsNotDefaultedForExportWhenOrganisationChanged()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var frOrgImpAddInfo = FROrgImpAddInfo.Get(importer);
			frOrgImpAddInfo.ZO_VATDeferType = VATProcedureList.Codes.S;

			var importer2 = Factory.NewWithValidTestData<OrgHeader>();
			var frOrgImpAddInfo2 = FROrgImpAddInfo.Get(importer2);
			frOrgImpAddInfo2.ZO_VATDeferType = VATProcedureList.Codes.L;
			Factory.Save();

			declaration.JE_OH_Importer = importer2.PK;
			AssertEquals("It should not default ZG_VATDeferType from dbo.OrgHeader's ImpAddInfo.ZO_VATDeferType for export when JE_PaymentMethod changed.", ZString.Empty, declaration.ZG_VATDeferType);

			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("It should default ZG_VATDeferType from dbo.OrgHeader's ImpAddInfo.ZO_VATDeferType when JE_PaymentMethod changed.", VATProcedureList.Codes.S, declaration.ZG_VATDeferType);
		}

		public void TestVatCanaValueChangeWhenVatDeferTypeIsUpdated()
		{
			var importerOrSupplier = Factory.New<OrgHeader>();
			importerOrSupplier.OH_Code = "TEST001";
			importerOrSupplier.MainAddress.Address1 = "TestMatchAddress";
			importerOrSupplier.MainAddress.OA_Code = "TestMatchAddress";
			importerOrSupplier.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "DGI001", ZString.Empty, ZString.Empty, "0771DEC5");

			var declarant = Factory.New<OrgHeader>();
			declarant.OH_Code = "TEST002";
			declarant.MainAddress.Address1 = "TestMatchAddress21";
			declarant.MainAddress.OA_Code = "TestMatchAddress21";
			var declarantAddressMatch2 = declarant.Addresses.AddNew();
			declarantAddressMatch2.Address1 = "TestMatchAddress22";
			declarantAddressMatch2.OA_Code = "TestMatchAddress22";
			var declarantAddressNoMatch = declarant.Addresses.AddNew();
			declarantAddressNoMatch.Address1 = "TestNoMatchAddress";
			declarantAddressNoMatch.OA_Code = "TestNoMatchAddress";
			declarant.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G2, "DGI002", ZString.Empty, ZString.Empty, "2D46B297");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsProfile = "DGI001";
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			declaration.SetupImporter(importerOrSupplier);
			declaration.SetupDeclarant(declarant.MainAddress);
			declaration.ZG_VATDeferNumber = "AI2000";

			var guarantee = CreateGuaranteeHeader("AI2000", importerOrSupplier.PK, "TestMatchAddress", countryCode: Core.Constants.CountryCodes.Germany);

			Factory.Save();

			declaration.WithFlux(EU.Business.MessageTypeList.Codes.Import);
			declaration.ZG_VATDeferType = VATProcedureList.Codes._2;
			declaration.ZG_VATDeferNumber = "AI2000";

			AssertNull("AI2 type of guarantee no set yet.", declaration.Ai2Permit);
			AssertEquals("No AI2 permit has been set so ZG_VATCANACode should be empty.", ZString.Empty, declaration.ZG_VATCANACode);

			guarantee.CPH_Type = GuaranteeTypeList.Codes.AI2;
			AssertNotNull("An AI2 type of guarantee should have been set", declaration.Ai2Permit);

			declaration.ZG_VATDeferType = VATProcedureList.Codes.S;
			declaration.ZG_VATDeferNumber = "AI2000";
			declaration.ZG_VATDeferType = VATProcedureList.Codes._2;
			AssertEquals("No CAN type of rule has been set against the AI2 type of guarantee. So ZG_VATCANACode should still be empty.", ZString.Empty, declaration.ZG_VATCANACode);

			var rule = guarantee.CusGuaranteeRules.AddNew();
			rule.CPR_RuleCode = PermitRuleCodeList.Codes.ADD;
			var ruleFromValue = "HasAValueFrom";
			rule.CPR_ValueFrom = ruleFromValue;
			declaration.ZG_VATDeferType = VATProcedureList.Codes.S;
			declaration.ZG_VATDeferNumber = "AI2000";
			declaration.ZG_VATDeferType = VATProcedureList.Codes._2;
			AssertEquals("No CAN type of rule has been set against the AI2 type of guarantee. So ZG_VATCANACode should still be empty.", ZString.Empty, declaration.ZG_VATCANACode);

			rule.CPR_RuleCode = PermitRuleCodeList.Codes.CAN;
			rule.CPR_ValueFrom = ruleFromValue;
			declaration.ZG_VATDeferType = VATProcedureList.Codes.S;
			declaration.ZG_VATDeferNumber = "AI2000";
			declaration.ZG_VATDeferType = VATProcedureList.Codes._2;
			AssertEquals("A CAN type of rule has been set against the AI2 type of guarantee. So ZG_VATCANACode should not be empty anymore.", "HasA", declaration.ZG_VATCANACode);

			declaration.ZG_VATDeferType = VATProcedureList.Codes.S;
			declaration.ZG_VATDeferNumber = "AI2000";
			Assert(declaration.ZG_VATCANACode.IsEmpty);

			declaration.ZG_VATDeferType = VATProcedureList.Codes._2;
			AssertEquals("A CAN type of rule has been set against the AI2 type of guarantee. ZG_VATCANACode should refresh accordingly.", "HasA", declaration.ZG_VATCANACode);

			guarantee.CPH_Type = GuaranteeTypeList.Codes.ALT;
			declaration.ZG_VATDeferType = VATProcedureList.Codes.L;
			Assert("No CANA should be set for Autoliquidation", declaration.ZG_VATCANACode.IsEmpty);
		}

		CusGuaranteeHeader CreateGuaranteeHeader(ZString number, ZGuid orgHeaderPK, ZString addressCode, string ruleCode = EU.Business.PermitRuleCodeList.Codes.ADD, string countryCode = Core.Constants.CountryCodes.France)
		{
			var result = Factory.New<CusGuaranteeHeader>();
			result.CPH_OH_PermitHolder = orgHeaderPK;
			result.CPH_RN_NKCountryCode = countryCode;
			result.CPH_StartDate = ZDate.Today;
			result.CPH_Type = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
			result.CPH_Number = number;
			if (!string.IsNullOrEmpty(ruleCode))
			{
				var rule1 = result.CusGuaranteeRules.AddNew();
				rule1.CPR_RuleCode = ruleCode;
				rule1.CPR_ValueFrom = addressCode;
			}
			return result;
		}
	}
}
