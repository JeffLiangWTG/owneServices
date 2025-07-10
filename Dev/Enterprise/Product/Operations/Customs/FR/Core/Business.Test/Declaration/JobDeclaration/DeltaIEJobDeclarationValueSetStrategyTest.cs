using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.Customs.FR.Registry;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(DeltaIEJobDeclarationValueSetStrategy))]
	class DeltaIEJobDeclarationValueSetStrategyTest : JobDeclarationValueSetStrategyAbstractTest
	{
		public override void TestDefaultJE_DeltaMode()
		{
			var declaration = Factory.New<JobDeclaration>().WithFlux(EU.Business.MessageTypeList.Codes.Import);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DEC, OrgCusAccountDeltaIETypeList.Codes.DCN, "DEC001", ZString.Empty, ZString.Empty, "B26F06FF");
			declaration.JE_OH_Importer = importer.PK;

			AssertEquals("Delta mode should never be defaulted for DeltaIE declarations.", ZString.Empty, declaration.JE_DeltaMode);
		}

		public void TestDefaultJE_CustomsGuaranteeNumber()
		{
			new UniversalReferenceTestDataHelper(Factory).CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE);

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "IMPORTER";
			importer.OH_FullName = "The importer";
			importer.OH_RL_NKClosestPort = "FRPAR";
			importer.MainAddress.Address1 = "ImporterAddress";
			importer.MainAddress.OA_Code = "ImporterAddress";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "IGUA", importer.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "ImporterAddress", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.France);

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "DECLARANT";
			declarant.OH_FullName = "The declarant";
			declarant.MainAddress.Address1 = "DeclarantAddress";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "DGUA", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "DeclarantAddress", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.France);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var invoice = declaration.Invoices.AddNew();

			CombineAssertions("Test when there is at least one guarantee consuming procedure in declaration invoice lines.", () =>
			{
				var procedure = Factory.New<RefCusProcedure>();
				procedure.ZZ6_ProcedureCode = "53";
				procedure.ZZ6_PreviousProcedureCode = "53";
				procedure.ZZ6_Concession = "D07";
				procedure.ZZ6_ShipmentType = JobMessageTypeList.Codes.Import;
				procedure.ZZ6_IsGuaranteeConsumed = Universal.CodeDescriptionPairLists.YesNoList.Codes.Yes;
				procedure.ZZ6_ZZZ_NKDataGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE;

				var invoiceLine1 = invoice.InvoiceLines.AddNew();
				invoiceLine1.JI_Procedure = "5353D07";
				var invoiceLine2 = invoice.InvoiceLines.AddNew();
				invoiceLine2.JI_Procedure = ZString.Empty;

				declaration.JE_OH_Importer = ZGuid.Empty;
				declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
				declaration.JE_DeclarantType = RepresentationTypeList.Codes.DIR;
				AssertEquals("Customs Guarantee Number should be empty when Declarant Type is Direct and Importer is not captured.", ZString.Empty, declaration.JE_CustomsGuaranteeNumber);

				declaration.JE_DeclarantType = RepresentationTypeList.Codes.IND;
				AssertEquals("Customs Guarantee Number should be empty when Declarant Type is Indirect and Declarant is not captured.", ZString.Empty, declaration.JE_CustomsGuaranteeNumber);

				declaration.JE_DeclarantType = RepresentationTypeList.Codes.SEL;
				AssertEquals("Customs Guarantee Number should be empty when Declarant Type is Self and Declarant is not captured.", ZString.Empty, declaration.JE_CustomsGuaranteeNumber);

				declaration.JE_OH_Importer = importer.PK;
				declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
				declaration.JE_DeclarantType = RepresentationTypeList.Codes.DIR;
				AssertEquals("Customs Guarantee Number should be defaulted to Importer when Declarant Type is Direct and Importer is captured.", "IGUA", declaration.JE_CustomsGuaranteeNumber);

				declaration.JE_DeclarantType = RepresentationTypeList.Codes.IND;
				AssertEquals("Customs Guarantee Number should be defaulted to Declarant when Declarant Type is Indirect and Declarant is captured.", "DGUA", declaration.JE_CustomsGuaranteeNumber);

				declaration.JE_DeclarantType = RepresentationTypeList.Codes.SEL;
				AssertEquals("Customs Guarantee Number should be defaulted to Declarant when Declarant Type is Self and Declarant is captured.", "DGUA", declaration.JE_CustomsGuaranteeNumber);
			});

			invoice.InvoiceLines.RemoveAndDeleteAll();

			CombineAssertions("Test when there is no guarantee consuming procedure in declaration invoice lines.", () =>
			{
				var procedure = Factory.New<RefCusProcedure>();
				procedure.ZZ6_ProcedureCode = "53";
				procedure.ZZ6_PreviousProcedureCode = "53";
				procedure.ZZ6_Concession = "D05";
				procedure.ZZ6_ShipmentType = JobMessageTypeList.Codes.Import;
				procedure.ZZ6_IsGuaranteeConsumed = Universal.CodeDescriptionPairLists.YesNoList.Codes.No;
				procedure.ZZ6_ZZZ_NKDataGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE;

				var invoiceLine1 = invoice.InvoiceLines.AddNew();
				invoiceLine1.JI_Procedure = "5353D05";
				var invoiceLine2 = invoice.InvoiceLines.AddNew();
				invoiceLine2.JI_Procedure = ZString.Empty;

				declaration.JE_OH_Importer = ZGuid.Empty;
				declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
				declaration.JE_DeclarantType = RepresentationTypeList.Codes.DIR;
				AssertEquals("Customs Guarantee Number should be empty irrespective of Declarant Type and Importer/Declarant values.", ZString.Empty, declaration.JE_CustomsGuaranteeNumber);

				declaration.JE_OH_Importer = importer.PK;
				declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
				declaration.JE_DeclarantType = RepresentationTypeList.Codes.DIR;
				AssertEquals("Customs Guarantee Number should be empty irrespective of Declarant Type and Importer/Declarant values.", ZString.Empty, declaration.JE_CustomsGuaranteeNumber);
			});
		}

		public override void TestDefaultJE_DeclarationLanguage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertEquals("JE_DeclarationLanguage should be defaulted to 'FR' for DeltaI/E declarations.", FRCustomsLanguageList.Codes.FR, declaration.JE_DeclarationLanguage);
		}

		public void TestGetProfileDefaultValue_WithDifferentJE_DeclarantType()
		{
			var declaration = Factory.New<JobDeclaration>().WithFlux(EU.Business.MessageTypeList.Codes.Import).WithDeltaIE();
			var valueSetStrategy = new DeltaIEJobDeclarationValueSetStrategy(declaration);
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DEC, OrgCusAccountDeltaIETypeList.Codes.DCC, "DEC001", "CUSOF001", ZString.Empty, "C447912A");
			declaration.SetupImporter(importer);
			declaration.SetupDeclarant(importer.MainAddress);
			declaration.JE_OA_Representative = ZGuid.Empty;
			AssertEquals("[Prerequisite]: When Importer equals Declarant and Representative is empty, JE_DeclarantType should be SEL", RepresentationTypeList.Codes.SEL, declaration.JE_DeclarantType);
			AssertEquals("JE_DeclarantType is 'SEL': Should only search Declarant's account.", "DEC001", declaration.JE_CustomsProfile);

			var representative = Factory.NewWithValidTestData<OrgHeader>();
			representative.SetupAccount(OrgCusAccountCodeList.Codes.DEC, OrgCusAccountDeltaIETypeList.Codes.DCC, "DEC002", "CUSOF002", ZString.Empty, "604D8AFA");
			declaration.SetupRepresentative(representative.MainAddress);
			AssertEquals("[Prerequisite]: When Importer equals Declarant and Representative is not empty, JE_DeclarantType should be DIR", RepresentationTypeList.Codes.DIR, declaration.JE_DeclarantType);
			AssertEquals("JE_DeclarantType is 'DIR': Should search Declarant's account first, and then search Representative's account.", "DEC001", declaration.JE_CustomsProfile);

			importer.SetupAccount(OrgCusAccountCodeList.Codes.DEC, OrgCusAccountDeltaIETypeList.Codes.DCC, "DEC003", "CUSOF003", ZString.Empty, "C447912A");
			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.SetupImporter(importer);
			AssertEquals("JE_DeclarantType is 'DIR': Should search Declarant's account first, and then search Representative's account. Even when the Declarant has multiple DCCs and the default value would be empty, we should still apply it.", ZString.Empty, declaration.JE_CustomsProfile);

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.SetupAccount(OrgCusAccountCodeList.Codes.DEC, OrgCusAccountDeltaIETypeList.Codes.DCC, "DEC004", "CUSOF004", ZString.Empty, "C447912A");
			declaration.SetupDeclarant(declarant.MainAddress);
			AssertEquals("[Prerequisite]: When Importer does not equal Declarant, JE_DeclarantType should be IND", RepresentationTypeList.Codes.IND, declaration.JE_DeclarantType);
			AssertEquals("JE_DeclarantType is 'IND': Should only search Declarant's account.", "DEC004", declaration.JE_CustomsProfile);
		}

		public void TestGetProfileDefaultValue_PriorityOfCZ_Type()
		{
			var declaration = Factory.New<JobDeclaration>().WithFlux(EU.Business.MessageTypeList.Codes.Import).WithDeltaIE();
			var valueSetStrategy = new DeltaIEJobDeclarationValueSetStrategy(declaration);
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.SetupAccount(OrgCusAccountCodeList.Codes.DEC, OrgCusAccountDeltaIETypeList.Codes.HDN, "DEC001", "CUSOF001", ZString.Empty, "604D8AFA");
			declarant.SetupAccount(OrgCusAccountCodeList.Codes.DEC, OrgCusAccountDeltaIETypeList.Codes.DCN, "DEC002", "CUSOF002", ZString.Empty, "604D8AFA");
			declaration.SetupDeclarant(declarant.MainAddress);
			AssertEquals("[Prerequisite]: When Importer does not equal Declarant, JE_DeclarantType should be IND.", RepresentationTypeList.Codes.IND, declaration.JE_DeclarantType);
			AssertEquals("The Priority Of CZ_Type should be DCC > DCN > HDN.", "DEC002", declaration.JE_CustomsProfile);

			declarant.SetupAccount(OrgCusAccountCodeList.Codes.DEC, OrgCusAccountDeltaIETypeList.Codes.DCC, "DEC003", "CUSOF003", ZString.Empty, "604D8AFA");
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			declaration.SetupDeclarant(declarant.MainAddress);
			AssertEquals("The Priority Of CZ_Type should be DCC > DCN > HDN.", "DEC003", declaration.JE_CustomsProfile);

			declarant.SetupAccount(OrgCusAccountCodeList.Codes.DEC, OrgCusAccountDeltaIETypeList.Codes.DCC, "DEC004", "CUSOF004", ZString.Empty, "604D8AFA");
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			declaration.SetupDeclarant(declarant.MainAddress);
			AssertEquals("When there are multiple accounts with same CZ_Type, then the default value should be empty string.", ZString.Empty, declaration.JE_CustomsProfile);
		}

		public void TestDefaultJE_CustomsProfile_TriggerWhenRepresentativeChanges()
		{
			var declaration = Factory.New<JobDeclaration>().WithFlux(EU.Business.MessageTypeList.Codes.Import).WithDeltaIE();
			var importer = declaration.SetupImporter();
			declaration.SetupDeclarant(importer.MainAddress);
			AssertEquals("Default to empty", "", declaration.JE_CustomsProfile);

			var representative = Factory.NewWithValidTestData<OrgAddress>();
			representative.Header.SetupAccount(OrgCusAccountCodeList.Codes.DEC, OrgCusAccountDeltaIETypeList.Codes.DCC, "DEC001", "CUSOF001", ZString.Empty, "604D8AFA");
			declaration.SetupRepresentative(representative);
			AssertEquals("[Prerequisite]: When Importer equals Declarant and Representative is not empty, JE_DeclarantType should be DIR", RepresentationTypeList.Codes.DIR, declaration.JE_DeclarantType);
			AssertEquals("When Representative changes, we should invoke DefaultJE_CustomsProfile()", "DEC001", declaration.JE_CustomsProfile);

			representative = Factory.NewWithValidTestData<OrgAddress>();
			representative.Header.SetupAccount(OrgCusAccountCodeList.Codes.DEC, OrgCusAccountDeltaIETypeList.Codes.DCC, "DEC002", "CUSOF002", ZString.Empty, "604D8AFA");
			declaration.SetupRepresentative(representative);
			AssertEquals("When Representative changes, we should invoke DefaultJE_CustomsProfile()", "DEC002", declaration.JE_CustomsProfile);
		}

		public void TestDefaultJE_CustomsProfile_ShouldNotDefaultIfMultipleRecordsAtSameLevel()
		{
			var declaration = Factory.New<JobDeclaration>().WithFlux(EU.Business.MessageTypeList.Codes.Import).WithDeltaIE();
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.SetupAccount(OrgCusAccountCodeList.Codes.DEC, OrgCusAccountDeltaIETypeList.Codes.DCN, "DIE001", ZString.Empty, ZString.Empty, "B26F06FF");
			declarant.SetupAccount(OrgCusAccountCodeList.Codes.DEC, OrgCusAccountDeltaIETypeList.Codes.DCN, "DIE002", ZString.Empty, ZString.Empty, "B26F06FF");
			declaration.SetupDeclarant(declarant.MainAddress);
			AssertEquals("should not set profile account - multiple declarant DEC account", ZString.Empty, declaration.JE_CustomsProfile);
		}

		public override void TestDefaultJE_CustomsProfile()
		{
			var declaration = Factory.New<JobDeclaration>().WithFlux(EU.Business.MessageTypeList.Codes.Import).WithDeltaIE();
			declaration.SetupImporter();
			declaration.SetupDeclarant();
			AssertEquals("no profile account - no declarant or importer DEC accounts", "", declaration.JE_CustomsProfile);

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.SetupAccount(OrgCusAccountCodeList.Codes.DEC, OrgCusAccountDeltaIETypeList.Codes.DCN, "DIE001", ZString.Empty, ZString.Empty, "B26F06FF");
			declaration.SetupDeclarant(declarant.MainAddress);
			AssertEquals("set profile account - one declarant DEC account", "DIE001", declaration.JE_CustomsProfile);
		}

		public void TestDefermentAccountNumberUpdatedWhenDeclarantTypeChanges()
		{
			var declaration = Factory.NewWithValidTestData<DummyJobDeclaration>();
			declaration.JE_PaymentMethod = MethodOfPaymentList.Codes.R;
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes.DIR;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "IMPORTER";
			importer.MainAddress.Address1 = "ImporterAddress";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.DEF, "IGUA", importer.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "ImporterAddress", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.France);

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "DECLARANT";
			declarant.MainAddress.Address1 = "DeclarantAddress";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.DEF, "DGUA", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "DeclarantAddress", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.France);

			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes.DIR;
			AssertEquals("Deferment account number should be Importer when Declarant type is DIR.", "IGUA", declaration.JE_DefermentAccountNumber);

			declaration.JE_DeclarantType = RepresentationTypeList.Codes.IND;
			AssertEquals("Deferment account number should be Declarant when Declarant type is changed to IND.", "DGUA", declaration.JE_DefermentAccountNumber);

			declaration.JE_DeclarantType = RepresentationTypeList.Codes.SEL;
			AssertEquals("Deferment account number should be Declarant when Declarant type is changed to SEL.", "DGUA", declaration.JE_DefermentAccountNumber);
		}

		public void TestCEI_SubStyleUpdatedWhenDateOfArrivalChanges()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var cei = declaration.CustomsEntryInstructions.FirstOrDefault<CusEntryInstruction>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = cei.PK;

			cei.EntryHeader.EntryNumber = "";

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration.JE_DateOfArrival = ZDateTime.Empty;
			AssertEquals(EntrySubstyleCodePairList.Codes.D, cei.CEI_SubStyle);

			declaration.JE_DateOfArrival = ZDateTime.Today.AddDays(-1);
			AssertEquals(EntrySubstyleCodePairList.Codes.A, cei.CEI_SubStyle);

			declaration.JE_DateOfArrival = ZDateTime.Today;
			AssertEquals(EntrySubstyleCodePairList.Codes.A, cei.CEI_SubStyle);

			declaration.JE_DateOfArrival = ZDateTime.Today.AddDays(1);
			AssertEquals(EntrySubstyleCodePairList.Codes.D, cei.CEI_SubStyle);
		}

		public void TestDefaultImporterChanged_DeclarantAndRepresentativeUpdatedOnDirectRepresentation()
		{
			var declaration = Factory.New<JobDeclaration>().WithFlux(EUJobMessageTypeList.Codes.Import);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.MainAddress.Address1 = "DeclarantAddress";
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DEC, OrgCusAccountDeltaIETypeList.Codes.DCN, "DIE003", ZString.Empty, ZString.Empty, "B26F06FF");
			EU.Business.EUOrgImpAddInfo.Get(importer, Core.Constants.CountryCodes.France).ZO_Box14UseIndirectRepresentation = false;
			importer.MainAddress.Address1 = "ImporterAddress";
			declaration.ImporterDocumentaryAddress.E2_AddressOverride = true;
			declaration.ImporterDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;

			AssertEquals("Representative should be the declarant address for direct representation when importer settings allows for it (ZO_Box14UseIndirectRepresentation is false).", "DeclarantAddress", declaration.Representative.Address1);
			AssertEquals("Declarant address should be the importer documentary address for direct representation when importer settings allows for it (ZO_Box14UseIndirectRepresentation is false).", "ImporterAddress", declaration.Declarant.Address1);
		}

		public void TestDefaultImporterChanged_NoUpdateForExport()
		{
			var declaration = Factory.New<JobDeclaration>().WithFlux(EUJobMessageTypeList.Codes.Export);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.MainAddress.Address1 = "DeclarantAddress";
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DEC, OrgCusAccountDeltaIETypeList.Codes.DCN, "DIE003", ZString.Empty, ZString.Empty, "B26F06FF");
			EU.Business.EUOrgImpAddInfo.Get(importer, Core.Constants.CountryCodes.France).ZO_Box14UseIndirectRepresentation = false;
			importer.MainAddress.Address1 = "ImporterAddress";
			declaration.ImporterDocumentaryAddress.E2_AddressOverride = true;
			declaration.ImporterDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;

			AssertEquals("Representative should be unchanged when declaration is not import.", null, declaration.Representative);
			AssertEquals("Declarant should be unchanged when Representative  is not import.", "DeclarantAddress", declaration.Declarant.Address1);
		}

		public void TestDefaultImporterChanged_NotUpdateForIndirectRepresentation()
		{
			var declaration = Factory.New<JobDeclaration>().WithFlux(EUJobMessageTypeList.Codes.Import);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.MainAddress.Address1 = "DeclarantAddress";
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DEC, OrgCusAccountDeltaIETypeList.Codes.DCN, "DIE003", ZString.Empty, ZString.Empty, "B26F06FF");
			EU.Business.EUOrgImpAddInfo.Get(importer, Core.Constants.CountryCodes.France).ZO_Box14UseIndirectRepresentation = true;
			importer.MainAddress.Address1 = "ImporterAddress";
			declaration.ImporterDocumentaryAddress.E2_AddressOverride = true;
			declaration.ImporterDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;

			AssertEquals("Representative should be defaulted when importer settings allows for it (ZO_Box14UseIndirectRepresentation is true).", null, declaration.Representative);
			AssertEquals("Declarant address should be unchanged when importer settings doesn't allow for it (ZO_Box14UseIndirectRepresentation is false).", "DeclarantAddress", declaration.Declarant.Address1);
		}

		public void TestDefaultImporterChanged_NoUpdateForDeltaG()
		{
			var declaration = Factory.New<JobDeclaration>().WithFlux(EUJobMessageTypeList.Codes.Import);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.MainAddress.Address1 = "DeclarantAddress";
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DEC, OrgCusAccountDeltaIETypeList.Codes.DCN, "DIE003", ZString.Empty, ZString.Empty, "B26F06FF");
			EU.Business.EUOrgImpAddInfo.Get(importer, Core.Constants.CountryCodes.France).ZO_Box14UseIndirectRepresentation = false;
			importer.MainAddress.Address1 = "ImporterAddress";
			declaration.ImporterDocumentaryAddress.E2_AddressOverride = true;
			declaration.ImporterDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;

			AssertEquals("Representative should not be defaulted when declaration is DeltaG.", null, declaration.Representative);
			AssertEquals("Declarant address should be unchanged when declaration is DeltaG.", "DeclarantAddress", declaration.Declarant.Address1);
		}

		public void TestPopulateAdditionalInfo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;

			var hasMandatoryInfo = DeltaIEDeclarationValidationHelper.HasMandatoryAdditionalInfos(declaration.AdditionalInfos);
			AssertEquals("Mandatory Additional Information should not be populated for DeltaG", hasMandatoryInfo, false);
			var hasFallbackProcedureReferenceInfo = DeltaIEDeclarationValidationHelper.HasFallbackProcedureReferenceAdditionalInfos(declaration.AdditionalInfos);
			AssertEquals("Fallback Procedure Reference Additional Information should not be populated for DeltaG", hasFallbackProcedureReferenceInfo, false);
			var hasFallbackProcedureInformationInfo = DeltaIEDeclarationValidationHelper.HasFallbackProcedureInformationAdditionalInfos(declaration.AdditionalInfos);
			AssertEquals("Fallback Procedure Information Additional Information should not be populated for DeltaG", hasFallbackProcedureInformationInfo, false);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			hasMandatoryInfo = DeltaIEDeclarationValidationHelper.HasMandatoryAdditionalInfos(declaration.AdditionalInfos);
			AssertEquals("Mandatory Additional Information should not be populated for DeltaIE Export type", hasMandatoryInfo, false);
			hasFallbackProcedureReferenceInfo = DeltaIEDeclarationValidationHelper.HasFallbackProcedureReferenceAdditionalInfos(declaration.AdditionalInfos);
			AssertEquals("Fallback Procedure Reference Additional Information should not be populated for DeltaIE Export type", hasFallbackProcedureReferenceInfo, false);
			hasFallbackProcedureInformationInfo = DeltaIEDeclarationValidationHelper.HasFallbackProcedureInformationAdditionalInfos(declaration.AdditionalInfos);
			AssertEquals("Fallback Procedure Information Additional Information should not be populated for DeltaIE Export type", hasFallbackProcedureInformationInfo, false);

			var fallbackSetting = new FallbackSettings();
			fallbackSetting.End = ZDateTime.Today.AddDays(1);
			fallbackSetting.Start = ZDateTime.Today.AddDays(1);

			FRCustomsDataRegistry.Instance.DeltaIMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);
			AssertEquals("Prerequisite: fallback should not be active", FRCustomsDataRegistry.DeltaIFallbackIsActive, false);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			hasMandatoryInfo = DeltaIEDeclarationValidationHelper.HasMandatoryAdditionalInfos(declaration.AdditionalInfos);
			AssertEquals("Mandatory Additional Information should be populated for DeltaIE Import type", hasMandatoryInfo, true);
			hasFallbackProcedureReferenceInfo = DeltaIEDeclarationValidationHelper.HasFallbackProcedureReferenceAdditionalInfos(declaration.AdditionalInfos);
			AssertEquals("Fallback Procedure Reference Additional Information should not be populated for DeltaIE Import type when fallback is not active", hasFallbackProcedureReferenceInfo, false);
			hasFallbackProcedureInformationInfo = DeltaIEDeclarationValidationHelper.HasFallbackProcedureInformationAdditionalInfos(declaration.AdditionalInfos);
			AssertEquals("Fallback Procedure Information Additional Information should not be populated for DeltaIE Import type when fallback is not active", hasFallbackProcedureInformationInfo, false);

			fallbackSetting.Start = ZDateTime.Today.AddDays(-1);

			FRCustomsDataRegistry.Instance.DeltaIMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);
			AssertEquals("Prerequisite: fallback should be active", FRCustomsDataRegistry.DeltaIFallbackIsActive, true);

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			hasFallbackProcedureReferenceInfo = DeltaIEDeclarationValidationHelper.HasFallbackProcedureReferenceAdditionalInfos(declaration.AdditionalInfos);
			AssertEquals("Fallback Procedure Reference Additional Information should be populated for DeltaIE Import type when fallback is active", hasFallbackProcedureReferenceInfo, true);
			hasFallbackProcedureInformationInfo = DeltaIEDeclarationValidationHelper.HasFallbackProcedureInformationAdditionalInfos(declaration.AdditionalInfos);
			AssertEquals("Fallback Procedure Information Additional Information should be populated for DeltaIE Import type when fallback is active", hasFallbackProcedureInformationInfo, true);
		}

		public void TestTogglePortCodeAdditionalReference()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.ChargePaymentOrDestinationID = "230";
			AssertNull("Port code Additional Reference should not be populated in Export declaration.", DeltaIEDeclarationValidationHelper.GetPortCodeAdditionalReference(declaration.AdditionalInfos));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			declaration.ChargePaymentOrDestinationID = "157";
			AssertNull("Port code Additional Reference should not be populated in DeltaG Import declaration.", DeltaIEDeclarationValidationHelper.GetPortCodeAdditionalReference(declaration.AdditionalInfos));

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration.ChargePaymentOrDestinationID = "230";
			CombineAssertions("Port code Additional Reference should be populated in DeltaIE Import declaration when ChargePaymentOrDestinationID is entered.", () =>
			{
				var portCodeAdditionalReference = DeltaIEDeclarationValidationHelper.GetPortCodeAdditionalReference(declaration.AdditionalInfos);
				AssertNotNull(portCodeAdditionalReference);
				AssertEquals(AdditionalInfoSubTypeList.Codes.AdditionalReference, portCodeAdditionalReference.CSI_SubType);
				AssertEquals(UniversalReferenceConstants.RefCusCodeList.AdditionalReferenceCodes.PortCode, portCodeAdditionalReference.CSI_Code);
				AssertEquals("230", portCodeAdditionalReference.CSI_ReferenceNumber);
			});

			declaration.ChargePaymentOrDestinationID = "157";
			CombineAssertions("Port code Additional Reference should be updated in DeltaIE Import declaration when ChargePaymentOrDestinationID is modified.", () =>
			{
				var portCodeAdditionalReference = DeltaIEDeclarationValidationHelper.GetPortCodeAdditionalReference(declaration.AdditionalInfos);
				AssertNotNull(portCodeAdditionalReference);
				AssertEquals(AdditionalInfoSubTypeList.Codes.AdditionalReference, portCodeAdditionalReference.CSI_SubType);
				AssertEquals(UniversalReferenceConstants.RefCusCodeList.AdditionalReferenceCodes.PortCode, portCodeAdditionalReference.CSI_Code);
				AssertEquals("157", portCodeAdditionalReference.CSI_ReferenceNumber);
			});

			declaration.ChargePaymentOrDestinationID = ZString.Empty;
			AssertNull("Port code Additional Reference should be deleted in DeltaIE Import declaration when ChargePaymentOrDestinationID is set to empty.", DeltaIEDeclarationValidationHelper.GetPortCodeAdditionalReference(declaration.AdditionalInfos));
		}

		public void TestDefaultIsHighValueOvrd()
		{
			var declaration = Factory.New<JobDeclaration>().WithFlux(EUJobMessageTypeList.Codes.Import);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			declaration.ZG_IsHighValueOvrd = true;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertEquals("ZG_IsHighValueOvrd should be defaulted to false when declaration is DeltaIE.", false, declaration.ZG_IsHighValueOvrd);
		}

		public void TestDefaultBypassCodeAndReason()
		{
			var declaration = Factory.New<JobDeclaration>().WithFlux(EUJobMessageTypeList.Codes.Import);
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			entryInstruction.ZG_BypassCode = "A";
			entryInstruction.ZG_BypassReason = "Reason";
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertEquals("ZG_BypassCode should be defaulted to empty when declaration is DeltaIE.", ZString.Empty, entryInstruction.ZG_BypassCode);
			AssertEquals("ZG_BypassReason should be defaulted to empty when declaration is DeltaIE.", ZString.Empty, entryInstruction.ZG_BypassReason);
		}

		class DummyJobDeclaration : JobDeclaration
		{
			public DummyJobDeclaration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override EU.Business.Declaration.VATDeferStrategy GetVATDeferStrategyCore() => new DeltaIEVATDeferStrategy(this);
		}
	}
}
