using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class DeltaIEVATNumberSupporterTest : TestCaseWithFactory
	{
		public void TestGetVATDeferNumberForAutoliquidation()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "IMPORTER";
			importer.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.TVA, "TVA_Importer", Core.Constants.CountryCodes.France);

			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var fiscalReference = entryInstruction.FiscalReferences.AddNew();
			fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR3_TaxRepresentative;
			fiscalReference.CFR_Reference = "FR3_FiscalReference";

			EUOrgImpAddInfo.Get(importer, Core.Constants.CountryCodes.France).ZO_UseFr3FiscalRepresentation = true;
			Factory.Save();
			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
			relatedParty.OH_Code = "RelatedParty";
			importer.AddRelatedParty(relatedParty.PK, RelatedPartyTypeList.Codes.CustomsPostponedVatAccounting, EU.Business.MessageTypeList.Codes.Import, ZString.Empty, ZString.Empty, GlbCompany.CurrentCompany);
			relatedParty.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.TVA, "TVA_RelatedParty", Core.Constants.CountryCodes.France);

			var vatNumberSupporter = new DeltaIEVATNumberSupporter(declaration);

			AssertEquals("VAT number for Autoliquidation should equal organisation related party VAT number when able.", "TVA_RelatedParty", vatNumberSupporter.GetVATDeferNumberForAutoliquidation(importer));
			EUOrgImpAddInfo.Get(importer, Core.Constants.CountryCodes.France).ZO_UseFr3FiscalRepresentation = false;
			Factory.Save();
			AssertEquals("VAT number for Autoliquidation should fallback to entry instruction fiscal reference when related party has no VAT number.", "FR3_FiscalReference", vatNumberSupporter.GetVATDeferNumberForAutoliquidation(importer));

			entryInstruction.FiscalReferences.RemoveAndDeleteAll();
			AssertEquals("VAT number for Autoliquidation should fallback to organisation VAT number when  related party has no VAT number and entry instruction has no fiscal reference.", "TVA_Importer", vatNumberSupporter.GetVATDeferNumberForAutoliquidation(importer));
		}

		public void TestValidateVATNumber_MessageErrorCorrectlyAdded_WhenImporterTVARegistrationNumberIsOCCASIONNEL()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE);
			helper.CreateOrFindExistingRefCusProcedure(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, "", "50", "0", "   ", "", "IMP", "");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			SetUpImporter();

			using (declaration.SuspendValidationTesting())
			{
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_FormattedProcedure = "500";
				var propertyInfo = declaration.ZG_VATDeferTypeInfo;

				var addInfo = declaration.AdditionalInfos.AddNew();
				addInfo.CSI_Code = UniversalReferenceConstants.RefCusCodeList.AdditionalInformationCodes.UnidentifiedVATLiableInFrance;
				addInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				declaration.VATNumberSupporter.ValidateVATNumber(propertyInfo);
				AssertNoMessageErrors("No message errors should be added as G0008 additional information of INF type is provided as it's required.", propertyInfo);

				declaration.AdditionalInfos.RemoveAndDeleteAll();
				declaration.VATNumberSupporter.ValidateVATNumber(propertyInfo);
				AssertHasMessageError("Message error should be added as no G0008 additional information of INF type is provided while it's not required.", propertyInfo, "Procedure 500 on invoice line 1 requires a G0008 additional information of type INF for VAT number.");
			}

			void SetUpImporter()
			{
				var importer = Factory.NewWithValidTestData<OrgHeader>();
				importer.CustomsCodes.AddNew("TVA", "OCCASIONNEL", Core.Constants.CountryCodes.France);
				declaration.JE_OH_Importer = importer.PK;
			}
		}

		public void TestValidateVATNumber_MessageErrorCorrectlyAdded_WhenVATNumberDocumentIsRequired()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE);
			helper.CreateOrFindExistingRefCusProcedure(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, "", "50", "0", "   ", "", "IMP", "");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			using (declaration.SuspendValidationTesting())
			{
				invoiceLine.JI_FormattedProcedure = "500";
				var propertyInfo = declaration.ZG_VATDeferTypeInfo;
				declaration.VATNumberSupporter.ValidateVATNumber(propertyInfo);
				AssertHasMessageError("Message error should be added as no G008 supporting document or FR7 fiscal reference is provided while VATNumberDocument is required.", propertyInfo, "Procedure 500 on invoice line 1 requires a FR7 fiscal reference or G008 supporting document for VAT number.");

				propertyInfo.ClearAllNotifications();
				var supportingDocument = declaration.SupportingDocuments.AddNew();
				supportingDocument.CSI_Code = VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber;
				declaration.VATNumberSupporter.ValidateVATNumber(propertyInfo);
				AssertNoMessageErrors("No message error should be added as G008 supporting document is provided as VATNumberDocument is required.", propertyInfo);

				propertyInfo.ClearAllNotifications();
				declaration.SupportingDocuments.RemoveAndDeleteAll();
				var fiscalReference = entryInstruction.FiscalReferences.AddNew();
				fiscalReference.CFR_Code = DeltaIEFiscalReferenceCodeList.Codes.FR7;
				declaration.VATNumberSupporter.ValidateVATNumber(propertyInfo);
				AssertNoMessageErrors("No message error should be added as FR7 fiscal reference is provided as VATNumberDocument is required.", propertyInfo);
			}
		}

		public void TestValidateVATNumber_MessageErrorCorrectlyAdded_WhenVATNumberDocumentIsNotRequired()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			using (declaration.SuspendValidationTesting())
			{
				var propertyInfo = declaration.ZG_VATDeferTypeInfo;

				var supportingDocument = declaration.SupportingDocuments.AddNew();
				supportingDocument.CSI_Code = VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber;
				declaration.VATNumberSupporter.ValidateVATNumber(propertyInfo);
				AssertHasMessageError("Message error should be added as G008 supporting document is provided while no VATNumberDocument is required.", propertyInfo, "No procedure requires a FR7 fiscal reference nor G008 supporting document for VAT number.");

				propertyInfo.ClearAllNotifications();
				declaration.SupportingDocuments.RemoveAndDeleteAll();
				var fiscalReference = entryInstruction.FiscalReferences.AddNew();
				fiscalReference.CFR_Code = DeltaIEFiscalReferenceCodeList.Codes.FR7;
				declaration.VATNumberSupporter.ValidateVATNumber(propertyInfo);
				AssertHasMessageError("Message error should be added as FR7 fiscal reference is provided while no VATNumberDocument is required.", propertyInfo, "No procedure requires a FR7 fiscal reference nor G008 supporting document for VAT number.");

				propertyInfo.ClearAllNotifications();
				entryInstruction.FiscalReferences.RemoveAndDeleteAll();
				declaration.VATNumberSupporter.ValidateVATNumber(propertyInfo);
				AssertNoMessageErrors("No message errors should be added as no G008 supporting document or FR fiscal reference is provided as no VATNumberDocument is required.", propertyInfo);
			}
		}

		public void TestSetIdentifiedVATNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			declaration.VATNumberSupporter.SetIdentifiedVATNumber();
			AssertEquals(0, declaration.SupportingDocuments.Count);
			AssertEquals(1, entryInstruction.FiscalReferences.Count);
			AssertEquals(DeltaIEFiscalReferenceCodeList.Codes.FR7, entryInstruction.FiscalReferences[0].CFR_Code);
		}

		public void TestSetUnidentifiedVATNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration.VATNumberSupporter.SetUnidentifiedVATNumber();
			AssertEquals(1, declaration.SupportingDocuments.Count);
			AssertEquals(VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber, declaration.SupportingDocuments[0].CSI_Code);
		}

		public void TestHasIdentifiedVATNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			Assert(!declaration.VATNumberSupporter.HasIdentifiedVATNumber(invoiceLine));

			var supportingDocument = declaration.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = VATDeferStrategyCodeList.Codes.IdentifiedVATNumber;
			Assert(!declaration.VATNumberSupporter.HasIdentifiedVATNumber(invoiceLine));

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var fiscalReference = instruction.FiscalReferences.AddNew();
			fiscalReference.CFR_Code = DeltaIEFiscalReferenceCodeList.Codes.FR7;
			Assert(declaration.VATNumberSupporter.HasIdentifiedVATNumber(invoiceLine));
		}

		public void TestHasUnidentifiedVATNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			Assert(!declaration.VATNumberSupporter.HasUnidentifiedVATNumber(invoiceLine));

			var supportingDocument = declaration.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber;
			Assert(declaration.VATNumberSupporter.HasUnidentifiedVATNumber(invoiceLine));
		}

		public void TestCodeIsVATNumberRelated()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var supportingDocument = declaration.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = VATDeferStrategyCodeList.Codes.IdentifiedVATNumber;
			Assert(!declaration.VATNumberSupporter.CodeIsVATNumberRelated(supportingDocument));

			supportingDocument.CSI_Code = VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber;
			Assert(declaration.VATNumberSupporter.CodeIsVATNumberRelated(supportingDocument));

			supportingDocument.CSI_Code = VATDeferStrategyCodeList.Codes.Ai2_SpecialMention;
			Assert(!declaration.VATNumberSupporter.CodeIsVATNumberRelated(supportingDocument));
		}
	}
}

