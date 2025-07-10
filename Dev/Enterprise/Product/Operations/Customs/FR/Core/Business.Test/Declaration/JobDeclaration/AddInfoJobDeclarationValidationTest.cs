using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.FR.Business.MasterFiles;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	sealed class AddInfoJobDeclarationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestRuleC0810_N01ZG_Box18TransportNationalityNotAllowed()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Procedure = "1122F15";
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Procedure = "3344F15";

			var message = "[C0810_N01] Transport Nationality must be empty when all invoice lines Customs procedure end with F15 (Trading With Special Fiscal Territories).";

			declaration.ZG_Box18TransportNationality = Core.Constants.CountryCodes.Spain;
			AssertHasMessageError("Rule C0810 should apply when all invoice lines have procedure ending with F15 and Transport Nationality is not empty.", declaration.ZG_Box18TransportNationalityInfo, message);

			invoiceLine2.JI_Procedure = "3344000";
			declaration.Validation.ValidateAll();
			AssertNoMessageError("Rule C0810_N01 should not apply when not all invoice lines have procedure ending with F15 even if Transport Nationality is not empty.", declaration.ZG_Box18TransportNationalityInfo, message);

			invoiceLine2.JI_Procedure = "3344F15";
			declaration.ZG_Box18TransportNationality = ZString.Empty;
			AssertNoMessageError("Rule C0810_N01 should not apply when all invoice lines have procedure ending with F15 but Transport Nationality of goods is empty. ", declaration.ZG_Box18TransportNationalityInfo, message);
		}

		public void TestCheckRuleC0810_N01_ZG_Box18TransportNationalityMandatory()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Procedure = "1122F15";
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Procedure = "3344D01";

			var message = "[C0810_N01] Nationality for Active border transport means is mandatory.";

			declaration.ZG_Box18TransportNationality = ZString.Empty;
			AssertHasMessageError("ZG_Box18TransportNationality should be mandatory when not all invoice lines have procedure ending with F15.", declaration.ZG_Box18TransportNationalityInfo, message);
			declaration.ZG_Box18TransportNationality = Core.Constants.CountryCodes.Spain;
			AssertNoMessageError("There shouldn't be any error message when ZG_Box18TransportNationality is not empty.", declaration.ZG_Box18TransportNationalityInfo, message);

			invoiceLine2.JI_Procedure = "3344F15";
			declaration.ZG_Box18TransportNationality = ZString.Empty;
			AssertNoMessageError("ZG_Box18TransportNationality should not be mandatory when all invoice lines have procedure ending with F15.", declaration.ZG_Box18TransportNationalityInfo, message);
		}

		public void TestCheckZG_VATDeferNumber()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;

			SetUpAI2Permit();

			declaration.ZG_VATDeferType = VATProcedureList.Codes._2;
			declaration.ZG_VATDeferNumber = "XXX";
			declaration.AddInfoValidation.ValidateZG_VATDeferNumber();
			AssertHasMessageError(declaration.ZG_VATDeferNumberInfo, "Could not find matching AI2 permit, please check the number and expiration date.");

			declaration.ZG_VATDeferType = VATProcedureList.Codes._2;
			declaration.ZG_VATDeferNumber = "YYY";
			declaration.AddInfoValidation.ValidateZG_VATDeferNumber();
			AssertNoMessageErrors(declaration.ZG_VATDeferNumberInfo);

			declaration.ZG_VATDeferType = VATProcedureList.Codes.L;
			declaration.ZG_VATDeferNumber = "XXX";
			declaration.AddInfoValidation.ValidateZG_VATDeferNumber();
			AssertNoMessageErrors(declaration.ZG_VATDeferNumberInfo);

			declaration.ZG_VATDeferType = VATProcedureList.Codes.S;
			declaration.ZG_VATDeferNumber = "XXX";
			declaration.AddInfoValidation.ValidateZG_VATDeferNumber();
			AssertNoMessageErrors(declaration.ZG_VATDeferNumberInfo);

			void SetUpAI2Permit()
			{
				var ai2Permit = Factory.New<CusGuaranteeHeader>();
				ai2Permit.CPH_OH_PermitHolder = importer.PK;
				ai2Permit.CPH_Type = GuaranteeTypeList.Codes.AI2;
				ai2Permit.CPH_StartDate = ZDate.Today.AddDays(-1);
				ai2Permit.CPH_EndDate = ZDate.Today.AddDays(1);
				ai2Permit.CPH_Number = "YYY";
				Factory.Save();
			}

			declaration.ZG_VATDeferType = VATProcedureList.Codes.L;
			declaration.ZG_VATDeferNumber = ZString.Empty;
			declaration.AddInfoValidation.ValidateZG_VATDeferNumber();
			AssertHasMessageError(declaration.ZG_VATDeferNumberInfo, "VAT number is mandatory when VAT Defer Type is L.");
		}

		public void TestCheckZG_AgreedPlaceCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				declaration.ZG_AgreedPlaceCode = "Z";
				AssertNoMessageErrorContaining("No message error is expected when ZG_AgreedPlaceCode is set.", declaration.ZG_AgreedPlaceCodeInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.ZG_AgreedPlaceCode = ZString.Empty;
				AssertHasMessageErrorContaining("A message error is expected for import and non UCC6 declaration when ZG_AgreedPlaceCode is empty.", declaration.ZG_AgreedPlaceCodeInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				declaration.ZG_AgreedPlaceCode = ZString.Empty;
				AssertHasMessageErrorContaining("A message error is expected for export and non UCC6 declaration when ZG_AgreedPlaceCode is empty.", declaration.ZG_AgreedPlaceCodeInfo, MandatoryValidation.YouHaveNotEntered);
			}

			declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				declaration.ZG_AgreedPlaceCode = "Z";
				AssertNoMessageErrorContaining("No message error is expected when ZG_AgreedPlaceCode is set.", declaration.ZG_AgreedPlaceCodeInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.ZG_AgreedPlaceCode = ZString.Empty;
				AssertNoMessageErrorContaining("No Validation is expected for UCC6 and import declarations.", declaration.ZG_AgreedPlaceCodeInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				declaration.ZG_AgreedPlaceCode = ZString.Empty;
				AssertHasMessageErrorContaining("A message error is expected for export and UCC6 declaration when ZG_AgreedPlaceCode is empty.", declaration.ZG_AgreedPlaceCodeInfo, MandatoryValidation.YouHaveNotEntered);
			}
		}

		public void TestCheckZG_AgreedPlaceCodeToBeSameAsIncoTermPlaceCodeOnInvoice()
		{
			var messageErrorOrWarning = "Incoterm place code values do not match between Declaration and Invoice Header.";

			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfigurationAndReturnMock(declaration, "IsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoiceCore", true), true))
			{
				var invoiceHeader1 = declaration.Invoices.AddNew();
				invoiceHeader1.ZG_AgreedPlaceCode = "2";
				var invoiceHeader2 = declaration.Invoices.AddNew();
				invoiceHeader2.ZG_AgreedPlaceCode = "2";

				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				declaration.ZG_AgreedPlaceCode = "1";
				AssertHasWarning("A warning is expected for UCC6 and import declaration when Incoterm place code values do not match between Declaration and Invoice Header.", declaration.ZG_AgreedPlaceCodeInfo, messageErrorOrWarning);

				declaration.ZG_AgreedPlaceCode = "2";
				AssertNoWarningContaining("No warning is expected when Incoterm place code values match between Declaration and Invoice Header.", declaration.ZG_AgreedPlaceCodeInfo, messageErrorOrWarning);

				invoiceHeader2.ZG_AgreedPlaceCode = "3";
				declaration.AddInfoValidation.ValidateZG_AgreedPlaceCode();
				AssertHasWarning("A warning is expected for UCC6 and import declaration when Incoterm place code values do not match between Declaration and Invoice Header.", declaration.ZG_AgreedPlaceCodeInfo, messageErrorOrWarning);

				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				invoiceHeader1.ZG_AgreedPlaceCode = "2";
				invoiceHeader2.ZG_AgreedPlaceCode = "2";

				declaration.ZG_AgreedPlaceCode = "1";
				AssertHasMessageError("A message error is expected for UCC6 and export declaration when Incoterm place code values do not match between Declaration and Invoice Header.", declaration.ZG_AgreedPlaceCodeInfo, messageErrorOrWarning);

				declaration.ZG_AgreedPlaceCode = "2";
				AssertNoMessageErrorContaining("No message error is expected when Incoterm place code values match between Declaration and Invoice Header.", declaration.ZG_AgreedPlaceCodeInfo, messageErrorOrWarning);

				invoiceHeader2.ZG_AgreedPlaceCode = "3";
				declaration.AddInfoValidation.ValidateZG_AgreedPlaceCode();
				AssertHasMessageError("A message error is expected for UCC6 and export declaration when Incoterm place code values do not match between Declaration and Invoice Header.", declaration.ZG_AgreedPlaceCodeInfo, messageErrorOrWarning);
			}

			declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfigurationAndReturnMock(declaration, "IsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoiceCore", true).disposable)
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				var invoiceHeader1 = declaration.Invoices.AddNew();
				invoiceHeader1.ZG_AgreedPlaceCode = "2";
				var invoiceHeader2 = declaration.Invoices.AddNew();
				invoiceHeader2.ZG_AgreedPlaceCode = "2";

				declaration.ZG_AgreedPlaceCode = "1";
				AssertHasMessageError("A message error is expected for non UCC6 and export declaration when Incoterm place code values do not match between Declaration and Invoice Header.", declaration.ZG_AgreedPlaceCodeInfo, messageErrorOrWarning);

				declaration.ZG_AgreedPlaceCode = "2";
				AssertNoMessageErrorContaining("No message error is expected when Incoterm place code values match between Declaration and Invoice Header.", declaration.ZG_AgreedPlaceCodeInfo, messageErrorOrWarning);

				invoiceHeader2.ZG_AgreedPlaceCode = "3";
				declaration.AddInfoValidation.ValidateZG_AgreedPlaceCode();
				AssertHasMessageError("A message error is expected for non UCC6 and export declaration when Incoterm place code values do not match between Declaration and Invoice Header.", declaration.ZG_AgreedPlaceCodeInfo, messageErrorOrWarning);

				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				invoiceHeader1.ZG_AgreedPlaceCode = "2";
				invoiceHeader2.ZG_AgreedPlaceCode = "2";

				declaration.ZG_AgreedPlaceCode = "1";
				AssertHasMessageError("A message error is expected for non UCC6 and import declaration when Incoterm place code values do not match between Declaration and Invoice Header.", declaration.ZG_AgreedPlaceCodeInfo, messageErrorOrWarning);

				declaration.ZG_AgreedPlaceCode = "2";
				AssertNoMessageErrorContaining("No message error is expected when Incoterm place code values match between Declaration and Invoice Header.", declaration.ZG_AgreedPlaceCodeInfo, messageErrorOrWarning);

				invoiceHeader2.ZG_AgreedPlaceCode = "3";
				declaration.AddInfoValidation.ValidateZG_AgreedPlaceCode();
				AssertHasMessageError("A message error is expected for non UCC6 and import declaration when Incoterm place code values do not match between Declaration and Invoice Header.", declaration.ZG_AgreedPlaceCodeInfo, messageErrorOrWarning);
			}
		}

		public void TestCheckZG_AgreedPlaceCode_NotMandatoryForUCC6AndImportDeclaration()
		{
			var messageError = "Incoterm Place Code or Country Code is required";

			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				declaration.ZG_AgreedPlaceCode = "Z";
				AssertNoMessageErrorContaining("No message error is expected when ZG_AgreedPlaceCode is set.", declaration.ZG_AgreedPlaceCodeInfo, messageError);
				declaration.ZG_AgreedPlaceCode = ZString.Empty;
				AssertHasMessageErrorContaining("A message error is expected for UCC6 and export declaration when ZG_AgreedPlaceCode is empty.", declaration.ZG_AgreedPlaceCodeInfo, messageError);
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				declaration.ZG_AgreedPlaceCode = ZString.Empty;
				AssertNoMessageErrors("No validation is expected for UCC6 and import declarations.", declaration.ZG_AgreedPlaceCodeInfo);
			}
		}

		public void TestCheckZG_VATDeferType()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;

			declaration.ZG_VATDeferType = "X";
			AssertHasMessageErrorContaining(declaration.ZG_VATDeferTypeInfo, ListValidation.InvalidCodeMessageError);

			declaration.ZG_VATDeferType = VATProcedureList.Codes._2;
			AssertNoMessageErrors(declaration.ZG_VATDeferTypeInfo);

			declaration.ZG_VATDeferType = VATProcedureList.Codes.L;
			AssertNoMessageErrors(declaration.ZG_VATDeferTypeInfo);

			declaration.ZG_VATDeferType = VATProcedureList.Codes.S;
			AssertNoMessageErrors(declaration.ZG_VATDeferTypeInfo);

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;

			declaration.ZG_VATDeferType = "X";
			AssertHasMessageErrorContaining(declaration.ZG_VATDeferTypeInfo, ListValidation.InvalidCodeMessageError);

			declaration.ZG_VATDeferType = VATProcedureList.Codes._2;
			AssertHasMessageError(declaration.ZG_VATDeferTypeInfo, "This VAT procedure is not allowed for exports.");

			declaration.ZG_VATDeferType = VATProcedureList.Codes.L;
			AssertHasMessageError(declaration.ZG_VATDeferTypeInfo, "This VAT procedure is not allowed for exports.");

			declaration.ZG_VATDeferType = VATProcedureList.Codes.S;
			AssertNoMessageErrors(declaration.ZG_VATDeferTypeInfo);
		}

		void PrepareProcedureData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France);
			var procedure1 = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "", "50", "0", "   ", "", "IMP", "");
			helper.CreateRefCusProcedureAttribute(procedure1.PK, "VATNumberExempt", "N");
			var procedure2 = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "", "40", "0", "   ", "", "IMP", "");
			helper.CreateRefCusProcedureAttribute(procedure2.PK, "VATNumberExempt", "N");
			var procedure3 = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "", "42", "0", "   ", "", "IMP", "");
			helper.CreateRefCusProcedureAttribute(procedure3.PK, "VATNumberExempt", "Y");

			var procedure1DIE = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, "", "50", "0", "   ", "", "IMP", "");
			helper.CreateRefCusProcedureAttribute(procedure1DIE.PK, "VATNumberExempt", "N");
			var procedure2DIE = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, "", "40", "0", "   ", "", "IMP", "");
			helper.CreateRefCusProcedureAttribute(procedure2DIE.PK, "VATNumberExempt", "N");
			var procedure3DIE = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, "", "42", "0", "   ", "", "IMP", "");
			helper.CreateRefCusProcedureAttribute(procedure3DIE.PK, "VATNumberExempt", "Y");

			Factory.Save();
		}

		public void TestCheckZG_VATDeferType_WithRequiresVATNumberDocument()
		{
			PrepareProcedureData();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.TVA, "VATFR345", declaration.CountryCode);

			var frOrgImpAddInfo = FROrgImpAddInfo.Get(importer);
			frOrgImpAddInfo.ZO_VATDeferType = VATProcedureList.Codes._2;
			frOrgImpAddInfo.ZO_VATProcedureDateLimit = new ZDateTime(2019, 01, 01);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			declaration.ZG_VATDeferType = "L";

			invoiceLine.JI_FormattedProcedure = "";
			Assert(!declaration.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.IdentifiedVATNumber || x.CSI_Code == VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber));
			declaration.AddInfoValidation.ValidateAll();
			AssertNoMessageErrors(declaration.ZG_VATDeferTypeInfo);

			invoiceLine.JI_FormattedProcedure = "400";
			Assert(declaration.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.IdentifiedVATNumber || x.CSI_Code == VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber));
			declaration.AddInfoValidation.ValidateAll();
			AssertNoMessageError(declaration.ZG_VATDeferTypeInfo, "Procedure 400 on invoice line 1 requires a 1008 or G008 supporting document for VAT number.");

			declaration.SupportingDocuments.RemoveAndDeleteAll();
			declaration.AddInfoValidation.ValidateAll();
			AssertHasMessageError(declaration.ZG_VATDeferTypeInfo, "Procedure 400 on invoice line 1 requires a 1008 or G008 supporting document for VAT number.");

			invoiceLine.JI_FormattedProcedure = "420";
			Assert(!declaration.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.IdentifiedVATNumber || x.CSI_Code == VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber));
			declaration.AddInfoValidation.ValidateAll();
			AssertNoMessageError(declaration.ZG_VATDeferTypeInfo, "No procedure requires a 1008 nor G008 supporting document for VAT number.");

			var supportingDocument = declaration.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber;
			declaration.AddInfoValidation.ValidateAll();
			AssertHasMessageError(declaration.ZG_VATDeferTypeInfo, "No procedure requires a 1008 nor G008 supporting document for VAT number.");

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_FormattedProcedure = "400";
			Assert(declaration.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.IdentifiedVATNumber || x.CSI_Code == VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber));
			declaration.AddInfoValidation.ValidateAll();
			AssertNoMessageError(declaration.ZG_VATDeferTypeInfo, "Procedure 400 on invoice line 2 requires a 1008 or G008 supporting document for VAT number.");

			declaration.SupportingDocuments.RemoveAndDeleteAll();
			declaration.AddInfoValidation.ValidateAll();
			AssertHasMessageError(declaration.ZG_VATDeferTypeInfo, "Procedure 400 on invoice line 2 requires a 1008 or G008 supporting document for VAT number.");
		}

		public void TestCheckZG_VATDeferType_WithRequiresVATNumberDocumentForDeltaIE()
		{
			PrepareProcedureData();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.TVA, "VATFR345", declaration.CountryCode);

			var frOrgImpAddInfo = FROrgImpAddInfo.Get(importer);
			frOrgImpAddInfo.ZO_VATDeferType = VATProcedureList.Codes._2;
			frOrgImpAddInfo.ZO_VATProcedureDateLimit = new ZDateTime(2019, 01, 01);
			Factory.Save();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			declaration.ZG_VATDeferType = "L";
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			invoiceLine.JI_FormattedProcedure = "";
			Assert(!declaration.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.IdentifiedVATNumber || x.CSI_Code == VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber));
			declaration.AddInfoValidation.ValidateAll();
			AssertNoMessageErrors(declaration.ZG_VATDeferTypeInfo);

			invoiceLine.JI_FormattedProcedure = "400";
			Assert(!declaration.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.IdentifiedVATNumber || x.CSI_Code == VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber));
			Assert(instruction.FiscalReferences.Cast<CusFiscalReference>().Any(x => x.CFR_Code == DeltaIEFiscalReferenceCodeList.Codes.FR7));
			declaration.AddInfoValidation.ValidateAll();
			AssertNoMessageError(declaration.ZG_VATDeferTypeInfo, "Procedure 400 on invoice line 1 requires a FR7 fiscal reference or G008 supporting document for VAT number.");

			declaration.SupportingDocuments.RemoveAndDeleteAll();
			instruction.FiscalReferences.RemoveAndDeleteAll();
			declaration.AddInfoValidation.ValidateAll();
			AssertHasMessageError(declaration.ZG_VATDeferTypeInfo, "Procedure 400 on invoice line 1 requires a FR7 fiscal reference or G008 supporting document for VAT number.");

			invoiceLine.JI_FormattedProcedure = "420";
			Assert(!declaration.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.IdentifiedVATNumber || x.CSI_Code == VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber));
			declaration.AddInfoValidation.ValidateAll();
			AssertNoMessageError(declaration.ZG_VATDeferTypeInfo, "No procedure requires a FR7 fiscal reference nor G008 supporting document for VAT number.");

			var supportingDocument = declaration.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber;
			declaration.AddInfoValidation.ValidateAll();
			AssertHasMessageError(declaration.ZG_VATDeferTypeInfo, "No procedure requires a FR7 fiscal reference nor G008 supporting document for VAT number.");

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_FormattedProcedure = "400";
			Assert(declaration.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.IdentifiedVATNumber || x.CSI_Code == VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber));
			declaration.AddInfoValidation.ValidateAll();
			AssertNoMessageError(declaration.ZG_VATDeferTypeInfo, "Procedure 400 on invoice line 2 requires a FR7 fiscal reference or G008 supporting document for VAT number.");

			declaration.SupportingDocuments.RemoveAndDeleteAll();
			instruction.FiscalReferences.RemoveAndDeleteAll();
			declaration.AddInfoValidation.ValidateAll();
			AssertHasMessageError(declaration.ZG_VATDeferTypeInfo, "Procedure 400 on invoice line 2 requires a FR7 fiscal reference or G008 supporting document for VAT number.");
		}

		public void TestCheckNAT_041Quinquies()
		{
			const string expectedMessage = "[Nat_041_05] CANA AI2 cannot be selected when tax A435 or A825 must be paid.";

			var declaration = Factory.New<JobDeclaration>();
			declaration.ZG_VATCANACode = "1001";

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();

			using (var ctx = new Business.Testing.ConfigurationProviders.DeclarationValidationDeciderTestContext(declaration, true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				ctx.EnableRule(x => x.IsRuleNAT_041QuinquiesActive);

				declaration.ZG_VATDeferType = VATProcedureList.Codes.S;
				declaration.Validation.ValidateAll();
				AssertNoMessageError("Should not error when defer type is not 2", declaration.ZG_VATCANACodeInfo, expectedMessage);
				declaration.ZG_VATDeferType = VATProcedureList.Codes._2;

				declaration.Validation.ValidateAll();
				AssertNoMessageError("Should not error when no A435/A825 fee is present", declaration.ZG_VATCANACodeInfo, expectedMessage);

				var fee = entryLine.Fees.AddNew();
				declaration.ZG_VATCANACode = "1001";
				fee.NationalFeeTypeCode = UniversalReferenceConstants.RefCusRateCodes.A435;
				fee.CF_ChargeAmount = 10m;
				declaration.Validation.ValidateAll();
				AssertHasMessageError("Should trigger error when an A435 fee is present", declaration.ZG_VATCANACodeInfo, expectedMessage);

				fee.NationalFeeTypeCode = UniversalReferenceConstants.RefCusRateCodes.A825;
				declaration.Validation.ValidateAll();
				AssertHasMessageError("Should trigger error when an A825 fee is present", declaration.ZG_VATCANACodeInfo, expectedMessage);

				fee.NationalFeeTypeCode = UniversalReferenceConstants.RefCusRateCodes.A35;
				declaration.Validation.ValidateAll();
				AssertNoMessageError("Should not error when A435/A825 fee is not present", declaration.ZG_VATCANACodeInfo, expectedMessage);

				ctx.DisableRule(x => x.IsRuleNAT_041QuinquiesActive);
				fee.NationalFeeTypeCode = UniversalReferenceConstants.RefCusRateCodes.A435;
				declaration.Validation.ValidateAll();
				AssertNoMessageError("Should not error when rule [NAT_041Quinquies] is disabled", declaration.ZG_VATCANACodeInfo, expectedMessage);
			}
		}

		public void TestCheckVATCana()
		{
			GenerateCusCodeListAttribute();

			var declaration = Factory.New<JobDeclaration>();
			declaration.ZG_VATDeferType = VATProcedureList.Codes._2;

			declaration.ZG_VATCANACode = new VatCanaForAI2List(Factory).GetAllCodes().ElementAt(0);
			AssertNoMessageError(declaration.ZG_VATCANACodeInfo, ListValidation.InvalidCodeMessageError);

			declaration.ZG_VATCANACode = "4444";
			AssertHasMessageError(declaration.ZG_VATCANACodeInfo, ListValidation.InvalidCodeMessageError);

			declaration.ZG_VATCANACode = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.ZG_VATCANACodeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration = Factory.New<JobDeclaration>();

			declaration.ZG_VATDeferType = VATProcedureList.Codes.L;

			declaration.ZG_VATCANACode = new VatCanaForALTList(Factory).GetAllCodes().ElementAt(0);
			AssertNoMessageError(declaration.ZG_VATCANACodeInfo, ListValidation.InvalidCodeMessageError);

			declaration.ZG_VATCANACode = "4444";
			AssertNoMessageError(declaration.ZG_VATCANACodeInfo, ListValidation.InvalidCodeMessageError);

			declaration.ZG_VATCANACode = ZString.Empty;
			AssertNoMessageError(declaration.ZG_VATCANACodeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration = Factory.New<JobDeclaration>();
			declaration.ZG_VATDeferType = VATProcedureList.Codes.S;
			declaration.ZG_VATCANACode = "4444";
			AssertNoMessageError(declaration.ZG_VATCANACodeInfo, ListValidation.InvalidCodeMessageError);

			declaration.ZG_VATCANACode = "a24z";
			AssertNoMessageError(declaration.ZG_VATCANACodeInfo, ListValidation.InvalidCodeMessageError);

			declaration.ZG_VATCANACode = ZString.Empty;
			AssertNoMessageErrorContaining(declaration.ZG_VATCANACodeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckVATCana_WithAdditionalCodeValue()
		{
			GenerateCusCodeListAttribute();

			var declaration = Factory.New<JobDeclaration>();
			declaration.ZG_VATDeferType = VATProcedureList.Codes._2;

			declaration.ZG_VATCANACode = "1001";
			declaration.Validation.ValidateAll();
			AssertNoMessageError(declaration.ZG_VATCANACodeInfo, "VAT CANA 1001 requires special mention 60900");

			declaration.AdditionalInfos.Cast<AdditionalInfo>().ElementAt(0).CSI_Code = "61000";
			declaration.Validation.ValidateAll();
			AssertHasMessageError(declaration.ZG_VATCANACodeInfo, "VAT CANA 1001 requires special mention 60900");

			declaration.AdditionalInfos.RemoveAll();
			declaration.Validation.ValidateAll();
			AssertHasMessageError(declaration.ZG_VATCANACodeInfo, "VAT CANA 1001 requires special mention 60900");

			declaration = Factory.New<JobDeclaration>();
			declaration.ZG_VATDeferType = VATProcedureList.Codes.L;
			declaration.ZG_VATCANACode = "1035";
			declaration.Validation.ValidateAll();
			AssertNoMessageError(declaration.ZG_VATCANACodeInfo, "VAT CANA 1035 requires special mention 61000");

			declaration.AdditionalInfos.Cast<AdditionalInfo>().ElementAt(0).CSI_Code = "60900";
			declaration.Validation.ValidateAll();
			AssertHasMessageError(declaration.ZG_VATCANACodeInfo, "VAT CANA 1035 requires special mention 61000");

			declaration.AdditionalInfos.RemoveAll();
			declaration.Validation.ValidateAll();
			AssertHasMessageError(declaration.ZG_VATCANACodeInfo, "VAT CANA 1035 requires special mention 61000");

			declaration = Factory.New<JobDeclaration>();
			declaration.ZG_VATDeferType = VATProcedureList.Codes.S;
			declaration.ZG_VATCANACode = "1035";
			declaration.Validation.ValidateAll();
			AssertNoMessageError(declaration.ZG_VATCANACodeInfo, "VAT CANA 1035 requires special mention 61000");

			declaration.AdditionalInfos.Cast<AdditionalInfo>().ElementAt(0).CSI_Code = "60900";
			declaration.Validation.ValidateAll();
			AssertHasMessageError(declaration.ZG_VATCANACodeInfo, "VAT CANA 1035 requires special mention 61000");

			declaration.AdditionalInfos.RemoveAll();
			declaration.Validation.ValidateAll();
			AssertHasMessageError(declaration.ZG_VATCANACodeInfo, "VAT CANA 1035 requires special mention 61000");

			declaration.AdditionalInfos.AddNew().CSI_Code = "4000";
			declaration.ZG_VATCANACode = "";
			declaration.Validation.ValidateAll();
			AssertNoMessageErrors(declaration.ZG_VATCANACodeInfo);

			declaration.AdditionalInfos.RemoveAll();
			declaration.Validation.ValidateAll();
			AssertNoMessageErrors(declaration.ZG_VATCANACodeInfo);
		}

		public void TestCheckZG_AgreedPlaceCode_Country_Validation_JE_ShipmentIncoTermPlace()
		{
			var declaration = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				declaration.ZG_AgreedPlaceCode = "FRLHV";
				AssertNoMessageErrors(declaration.JE_ShipmentIncoTermPlaceInfo);
				declaration.ZG_AgreedPlaceCode = Core.Constants.CountryCodes.France;
				AssertNoMessageErrors(declaration.JE_ShipmentIncoTermPlaceInfo);
			});
		}

		void GenerateCusCodeListAttribute()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, "France");
			helper.CreateCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, "VAT National Additional Codes", "FR");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.VatProcedure, "vat Procedure", UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, "FR", UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.SpecialMention, "special mention", UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, "FR", UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana);
			Factory.Save();

			var cuslist = helper.CreateCusCodeListWithAttribute("FR", UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, "1035", "Article 1695 du CGI - Autoliquidation de la TVA à l''importation", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.VatProcedure, GuaranteeTypeList.Codes.ALT);
			var attribute = cuslist.Attributes.AddNew();
			attribute.ZZE_ZXE_NKName = UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.SpecialMention;
			attribute.ZZE_Value = "61000";

			var cuslist2 = helper.CreateCusCodeListWithAttribute("FR", UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, "1001", "Je m''engage à respecter les conditions de l''article 275 du CGI - TVA seule", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.VatProcedure, GuaranteeTypeList.Codes.AI2);
			helper.CreateCusCodeListWithAttribute("FR", UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, "1002", "Je m''engage à respecter les conditions de l''article 275 du CGI - taxes fiscales seules", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.VatProcedure, GuaranteeTypeList.Codes.AI2);
			helper.CreateCusCodeListWithAttribute("FR", UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, "1003", "Je m''engage à respecter les conditions de l''article 275 du CGI - TVA et taxes fiscales", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.VatProcedure, GuaranteeTypeList.Codes.AI2);
			helper.CreateCusCodeListWithAttribute("FR", UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, "1011", "Article 275 du CGI sans dispense de visa - TVA seule", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.VatProcedure, GuaranteeTypeList.Codes.AI2);
			helper.CreateCusCodeListWithAttribute("FR", UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, "1012", "Article 275 du CGI sans dispense de visa - taxes fiscales seules", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.VatProcedure, GuaranteeTypeList.Codes.AI2);
			helper.CreateCusCodeListWithAttribute("FR", UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, "1013", "Article 275 du CGI sans dispense de visa - TVA et taxes fiscales", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.VatProcedure, GuaranteeTypeList.Codes.AI2);

			var attribute2 = cuslist2.Attributes.AddNew();
			attribute2.ZZE_ZXE_NKName = UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.SpecialMention;
			attribute2.ZZE_Value = "60900";
			Factory.Save();
		}
	}
}
