using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.MasterFiles;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class SupportingDocumentValueSetStrategyTest : TestCaseWithFactory
	{
		public void TestCSI_UnitOfQuantityDefaultIfBoundToListWithOnlyOneRecord()
		{
			SupportingDocumentTest.SetUpRefDataForSupportDocumentUnitOfQuantityTest(Factory);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var supportingDocument = declaration.SupportingDocuments.AddNew();
			CombineAssertions(() =>
			{
				supportingDocument.CSI_Code = string.Empty;
				AssertEquals("CSI_Code is Empty.", string.Empty, supportingDocument.CSI_UnitOfQuantity);

				supportingDocument.CSI_Code = "CODE1";
				AssertEquals("CSI_Quantity has a lookup list with more than 1 record", string.Empty, supportingDocument.CSI_UnitOfQuantity);

				supportingDocument.CSI_Code = "CODE2";
				AssertEquals("CSI_Quantity has a lookup list with only 1 record", "LTR", supportingDocument.CSI_UnitOfQuantity);

				supportingDocument.CSI_Code = "CODE3";
				AssertEquals("CSI_Quantity has a lookup list with no records", "LTR", supportingDocument.CSI_UnitOfQuantity);
			});
		}

		[TestDate(2022, 12, 21)]
		public void TestPropertiesOfAI2TypeOfDocument()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();

			var ai2Document1 = declaration.SupportingDocuments.AddNew();
			ai2Document1.CSI_Code = VATDeferStrategyCodeList.Codes.VisaFreeAi2VatOnlyAdditionalCode;
			AssertEquals("AI2 SupportingDocument CSI_ReferenceNumber couldn't be defaulted because its declaration lacks AI2 type of guarantee.", ZString.Empty, ai2Document1.CSI_ReferenceNumber);
			AssertEquals("AI2 SupportingDocument CSI_DateOfIssue couldn't be defaulted because its declaration lacks AI2 type of guarantee.", new ZDate(2022, 12, 21), ai2Document1.CSI_DateOfIssue);

			var importer = Factory.NewWithValidTestData<OrgHeader>();

			var ai2Guarantee = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			ai2Guarantee.CPH_Type = GuaranteeTypeList.Codes.AI2;
			ai2Guarantee.CPH_Number = "VATFR345";
			ai2Guarantee.CPH_OH_PermitHolder = importer.PK;
			ai2Guarantee.CPH_StartDate = ZDate.BrettsBirthday;
			Factory.Save();

			declaration.JE_OH_Importer = importer.PK;
			declaration.ZG_VATDeferType = VATProcedureList.Codes._2;
			declaration.ZG_VATDeferNumber = "VATFR345";
			AssertEquals("Prerequisite: declaration AI2 type of guarantee has been set up properly.", ai2Guarantee, declaration.Ai2Permit);

			var ai2Document2 = declaration.SupportingDocuments.AddNew();
			ai2Document2.CSI_Code = VATDeferStrategyCodeList.Codes.VisaFreeAi2VatOnlyAdditionalCode;
			AssertEquals("AI2 SupportingDocument CSI_ReferenceNumber should be defaulted with AI2 type of guarantee CPH_Number.", "VATFR345", ai2Document2.CSI_ReferenceNumber);
			AssertEquals("AI2 SupportingDocument CSI_DateOfIssue should be defaulted with AI2 type of guarantee CPH_StartDate.", ZDate.BrettsBirthday, ai2Document2.CSI_DateOfIssue);
		}

		public void TestCSI_ReferenceForCSI_Code1008_DeltaIE()
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
			declaration.SupportingDocuments.RemoveAndDeleteAll();

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

			var supportingDocument = declaration.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = VATDeferStrategyCodeList.Codes.IdentifiedVATNumber;
			AssertEquals("TVA_RelatedParty", supportingDocument.CSI_ReferenceNumber);

			EUOrgImpAddInfo.Get(importer, Core.Constants.CountryCodes.France).ZO_UseFr3FiscalRepresentation = false;
			Factory.Save();
			supportingDocument = declaration.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = VATDeferStrategyCodeList.Codes.IdentifiedVATNumber;
			AssertEquals("FR3_FiscalReference", supportingDocument.CSI_ReferenceNumber);

			fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR2_Customer;
			supportingDocument = declaration.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = VATDeferStrategyCodeList.Codes.IdentifiedVATNumber;
			AssertEquals("TVA_Importer", supportingDocument.CSI_ReferenceNumber);
		}

		public void TestCSI_ReferenceForCSI_Code1008_DeltaG()
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
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			var supportingDocument = declaration.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = VATDeferStrategyCodeList.Codes.IdentifiedVATNumber;

			AssertEquals("TVA_Importer", supportingDocument.CSI_ReferenceNumber);
		}

		[TestDate(2024, 04,02,12,25,22)]
		public void TestCSI_DateOfIssueForCSI_Code1008AndG008()
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
			declaration.SupportingDocuments.RemoveAndDeleteAll();

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

			CombineAssertions(() =>
			{
				var supportingDocument = declaration.SupportingDocuments.AddNew();
				supportingDocument.CSI_Code = VATDeferStrategyCodeList.Codes.IdentifiedVATNumber;
				AssertEquals(ZDateTime.Now, supportingDocument.CSI_DateOfIssue);

				supportingDocument.CSI_DateOfIssue = ZDateTime.Now.AddDays(-2);

				supportingDocument.CSI_Code = VATDeferStrategyCodeList.Codes.Ai2_SpecialMention;
				supportingDocument.CSI_Code = VATDeferStrategyCodeList.Codes.IdentifiedVATNumber;
				AssertEquals(ZDateTime.Now.AddDays(-2), supportingDocument.CSI_DateOfIssue);

				supportingDocument = declaration.SupportingDocuments.AddNew();
				supportingDocument.CSI_Code = VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber;
				AssertEquals(ZDateTime.Now, supportingDocument.CSI_DateOfIssue);

				supportingDocument.CSI_DateOfIssue = ZDateTime.Now.AddDays(-2);

				supportingDocument.CSI_Code = VATDeferStrategyCodeList.Codes.Ai2_SpecialMention;
				supportingDocument.CSI_Code = VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber;
				AssertEquals(ZDateTime.Now.AddDays(-2), supportingDocument.CSI_DateOfIssue);

				supportingDocument = declaration.SupportingDocuments.AddNew();
				supportingDocument.CSI_Code = VATDeferStrategyCodeList.Codes.Ai2_SpecialMention;
				AssertEquals(ZDateTime.Empty, supportingDocument.CSI_DateOfIssue);
			});
		}

		public void TestCSI_ReferenceForCSI_CodeG008()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.SupportingDocuments.RemoveAndDeleteAll();

			var supportingDocument = declaration.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber;
			AssertEquals("Constant reference if CSI_Code set to G008.", "Redevable non identifié à la TVA en France", supportingDocument.CSI_ReferenceNumber);
		}

		public void TestCSI_IsDTPForDTP()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "Supporting Document of Import Direction");
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "0001", "statut juridique", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsDTP, YesNoList.Codes.Yes);
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "2044", "Demande d'autorisation d'importation de radionucléides (DAI) visée par l'IRSN", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsDTP, YesNoList.Codes.No);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "Supporting Document of Export Direction");
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "0002", "attestation  produite par l'ONU ou une de ses institutions spécialisées", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsDTP, YesNoList.Codes.Yes);
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "2045", "Demande d'autorisation d'exportation de radionucléides (DAE) visée par l'IRSN", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.SupportingDocuments.AddNew();
			var jobComInvoiceHeader = declaration.Invoices.AddNew();
			jobComInvoiceHeader.SupportingDocuments.AddNew();
			var jobComInvoiceLine = declaration.InvoiceLines.AddNew();
			var lineSupportingDocument1 = jobComInvoiceLine.SupportingDocuments.AddNew();
			lineSupportingDocument1.CSI_Code = "0001";
			AssertEquals(true, lineSupportingDocument1.CSI_IsDTP);
			var lineSupportingDocument2 = jobComInvoiceLine.SupportingDocuments.AddNew();
			lineSupportingDocument2.CSI_Code = "2044";
			AssertEquals("value of IsDTP is No, so CSI_IsDTP is false", false, lineSupportingDocument2.CSI_IsDTP);

			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			var lineSupportingDocument3 = jobComInvoiceLine.SupportingDocuments.AddNew();
			lineSupportingDocument3.CSI_Code = "0002";
			AssertEquals(true, lineSupportingDocument3.CSI_IsDTP);
			var lineSupportingDocument4 = jobComInvoiceLine.SupportingDocuments.AddNew();
			lineSupportingDocument4.CSI_Code = "2045";
			AssertEquals("2045 doesn't have the attribute IsDTP, so CSI_IsDTP is false", false, lineSupportingDocument4.CSI_IsDTP);
		}

		public void TestCSI_DateOfExpiryForD48()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "Supporting Document of Export Direction");
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "0001", "attestation  produite par l'ONU ou une de ses institutions spécialisées", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "0003", "attestation  produite par l'ONU ou une de ses institutions spécialisées", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "2045", "Demande d'autorisation d'exportation de radionucléides (DAE) visée par l'IRSN", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			var supportingDocument = declaration.SupportingDocuments.AddNew();
			AssertCSI_DateOfExpiry(supportingDocument);

			var jobComInvoiceHeader = declaration.Invoices.AddNew();
			var headerSupportingDocument = jobComInvoiceHeader.SupportingDocuments.AddNew();
			AssertCSI_DateOfExpiry(headerSupportingDocument);

			var jobComInvoiceLine = declaration.InvoiceLines.AddNew();
			var lineSupportingDocument = jobComInvoiceLine.SupportingDocuments.AddNew();
			AssertCSI_DateOfExpiry(lineSupportingDocument);
		}

		void AssertCSI_DateOfExpiry(SupportingDocument sd)
		{
			sd.CSI_Code = "0001";
			sd.CSI_DateOfIssue = ZDateTime.Today;
			sd.CSI_Quantity3 = 0m;
			AssertEquals("1.Init: CSI_DateOfExpiry is empty when CSI_Quantity3 is 0", ZDateTime.Empty, sd.CSI_DateOfExpiry);

			sd.CSI_Quantity3 = 1m;
			AssertEquals("2.IsD48: CSI_DateOfExpiry is updated (CSI_DateOfExpiry = CSI_DateOfIssue.AddMonths(CSI_Quantity3)) when CSI_Quantity3 is changed", sd.CSI_DateOfIssue.AddMonths(sd.CSI_Quantity3.ToZInt()), sd.CSI_DateOfExpiry);
			sd.CSI_DateOfIssue = sd.CSI_DateOfIssue.AddDays(11);
			AssertEquals("3.IsD48: CSI_DateOfExpiry is updated (CSI_DateOfExpiry = CSI_DateOfIssue.AddMonths(CSI_Quantity3)) when CSI_DateOfIssue is changed", sd.CSI_DateOfIssue.AddMonths(sd.CSI_Quantity3.ToZInt()), sd.CSI_DateOfExpiry);
			sd.CSI_Code = "0003";
			AssertEquals("4.IsD48: CSI_DateOfExpiry is updated (CSI_DateOfExpiry = CSI_DateOfIssue.AddMonths(CSI_Quantity3)) when CSI_Code is changed (ISD48)", sd.CSI_DateOfIssue.AddMonths(sd.CSI_Quantity3.ToZInt()), sd.CSI_DateOfExpiry);
			sd.CSI_Quantity3 = 0m;
			AssertEquals("5.IsD48: CSI_DateOfExpiry is cleared when CSI_Quantity3 is changed to '0'", ZDateTime.Empty, sd.CSI_DateOfExpiry);

			var csiDateOfExpiry = ZDateTime.Today.AddYears(1);
			sd.CSI_DateOfExpiry = csiDateOfExpiry;
			sd.CSI_Code = "2044";
			AssertEquals("6.IsNotD48: Nothing to do about CSI_DateOfExpiry when CSI_Code is not D48", csiDateOfExpiry, sd.CSI_DateOfExpiry);
			sd.CSI_DateOfIssue = sd.CSI_DateOfIssue.AddDays(11);
			AssertEquals("7.IsNotD48: Nothing to do about CSI_DateOfExpiry when CSI_DateOfIssue is changed", csiDateOfExpiry, sd.CSI_DateOfExpiry);

			sd.CSI_Code = "0003";
			AssertEquals("8.IsD48: CSI_DateOfExpiry is empty when CSI_Quantity3 = 0", ZDateTime.Empty, sd.CSI_DateOfExpiry);
			sd.CSI_DateOfIssue = ZDateTime.Invalid;
			sd.CSI_Quantity3 = 1m;
			AssertEquals("8.IsD48: CSI_DateOfExpiry is still default value when CSI_DateOfIssue is invalid", ZDateTime.Empty, sd.CSI_DateOfExpiry);
		}
	}
}
