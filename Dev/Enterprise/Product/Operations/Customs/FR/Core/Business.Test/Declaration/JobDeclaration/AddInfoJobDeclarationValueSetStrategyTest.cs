using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.FR.Business.MasterFiles;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	sealed class AddInfoJobDeclarationValueSetStrategyTest : TestCaseWithFactory
	{
		public void TestCusDV1DetailCollectionWhenZG_IsHighValueOvrdChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.ZG_IsHighValueOvrd = true;
			declaration.ZG_IsHighValueOvrd = false;
			AssertEquals("If ZG_IsHighValueOvrd is changing from true to false, delete all DV1 lines.", 0, declaration.DV1Details.Count);
			declaration.ZG_IsHighValueOvrd = true;
			AssertEquals("If ZG_IsHighValueOvrd is changing from false to true, and there is no DV1 line, then add a DV1 line.", 1, declaration.DV1Details.Count);
			declaration.ZG_IsHighValueOvrd = false;
			AssertEquals("If ZG_IsHighValueOvrd is changing from true to false, delete all DV1 lines.", 0, declaration.DV1Details.Count);
		}

		public void TestOnVATDeferTypeChangedToAI2()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "", "40", "00", "000", "", "IMP", "");
			var procedureDIE = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, "", "40", "00", "000", "", "IMP", "");
			helper.CreateRefCusProcedureAttribute(procedure.PK, "VATNumberExempt", "N");
			helper.CreateRefCusProcedureAttribute(procedureDIE.PK, "VATNumberExempt", "N");
			Factory.Save();

			var ai2Importer = Factory.NewWithValidTestData<OrgHeader>();
			ai2Importer.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.TVA, "VATFR345", Core.Constants.CountryCodes.France);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = ai2Importer.PK;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_FormattedProcedure = "4000000";
			Assert("Prerequisite", invoiceLine.RequiresVATNumberDocument);

			declaration.SupportingDocuments.RemoveAndDeleteAll();
			declaration.ZG_VATDeferType = VATProcedureList.Codes.L;
			Assert("No 1001 document should have been automatically added to declaration.", !declaration.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.VisaFreeAi2VatOnlyAdditionalCode));

			declaration.ZG_VATDeferType = VATProcedureList.Codes._2;
			Assert("A 1001 document should have been be added automatically to declaration.", declaration.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.VisaFreeAi2VatOnlyAdditionalCode));

			declaration.ZG_VATDeferType = VATProcedureList.Codes.L;
			Assert("Existing 1001 document should have been automatically removed from declaration when procedure type changes for non AI2 procedure.", !declaration.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.VisaFreeAi2VatOnlyAdditionalCode));

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration.SupportingDocuments.RemoveAndDeleteAll();
			declaration.ZG_VATDeferType = VATProcedureList.Codes.L;
			Assert("No 1001 document should have been automatically added to UCC6 declaration.", !declaration.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.VisaFreeAi2VatOnlyAdditionalCode));

			declaration.ZG_VATDeferType = VATProcedureList.Codes._2;
			Assert("A 1001 document should have been be added automatically to UCC6 declaration.", declaration.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.VisaFreeAi2VatOnlyAdditionalCode));

			declaration.ZG_VATDeferType = VATProcedureList.Codes.L;
			Assert("Existing 1001 document should have been automatically removed from UCC6 declaration when procedure type changes for non AI2 procedure.", !declaration.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.VisaFreeAi2VatOnlyAdditionalCode));
		}

		void PrepareVATRefDatas()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE);
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

			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "additional info");
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "I9101", "doc I9101", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Category, Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.DCC);
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "10100", "10100 doc", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Export, "Export");
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "50000", "50000 doc", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Export, "Export");
			Factory.Save();
		}

		void PrepareImporterData(JobDeclaration declaration, ZBool needVATCustomsCode)
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			if (needVATCustomsCode)
			{
				importer.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.TVA, "VATFR345", declaration.CountryCode);
			}

			var importerALT = Factory.NewWithValidTestData<OrgHeader>();
			importerALT.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.TVA, "VATFR345", declaration.CountryCode);

			var importerAI2WithoutTVA = Factory.NewWithValidTestData<OrgHeader>();
			var importerALTWithoutTVA = Factory.NewWithValidTestData<OrgHeader>();

			var frOrgImpAddInfo = FROrgImpAddInfo.Get(importer);
			frOrgImpAddInfo.ZO_VATDeferType = VATProcedureList.Codes._2;
			frOrgImpAddInfo.ZO_VATProcedureDateLimit = new ZDateTime(2019, 01, 01);

			var frOrgImpAddInfo2 = FROrgImpAddInfo.Get(importerAI2WithoutTVA);
			frOrgImpAddInfo2.ZO_VATDeferType = VATProcedureList.Codes._2;
			frOrgImpAddInfo2.ZO_VATProcedureDateLimit = new ZDateTime(2019, 01, 01);

			var frOrgImpAddInfo3 = FROrgImpAddInfo.Get(importerALTWithoutTVA);
			frOrgImpAddInfo3.ZO_VATDeferType = VATProcedureList.Codes.L;
			frOrgImpAddInfo3.ZO_VATProcedureDateLimit = new ZDateTime(2019, 01, 01);

			var frOrgImpAddInfo4 = FROrgImpAddInfo.Get(importerALT);
			frOrgImpAddInfo4.ZO_VATDeferType = VATProcedureList.Codes.L;
			frOrgImpAddInfo4.ZO_VATProcedureDateLimit = new ZDateTime(2019, 01, 01);
			Factory.Save();

			declaration.JE_OH_Importer = importer.PK;
		}

		public void TestOnVATDeferTypeChangedIfVATDeferNumberIsNotEmptyWhenNotUCC6()
		{
			PrepareVATRefDatas();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			PrepareImporterData(declaration, true);

			declaration.AdditionalInfos.RemoveAndDeleteAll();
			declaration.SupportingDocuments.RemoveAndDeleteAll();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();

			invoiceLine.JI_FormattedProcedure = "400";
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			Assert("No SupportingDocument before ZG_VATDeferType change", !declaration.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.IdentifiedVATNumber));

			declaration.ZG_VATDeferType = "L";
			Assert("Should trigger creating SupportingDocument when ZG_VATDeferType changes.", declaration.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.IdentifiedVATNumber));

			declaration.SupportingDocuments.RemoveAndDeleteAll();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			invoiceLine.JI_FormattedProcedure = "400";
			declaration.ZG_VATDeferType = "S";
			declaration.ZG_VATDeferType = "L";
			Assert("Should not trigger creating SupportingDocument when export.", !declaration.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.IdentifiedVATNumber));

			declaration.SupportingDocuments.RemoveAndDeleteAll();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			invoiceLine.JI_FormattedProcedure = "420";
			invoiceLine2.JI_FormattedProcedure = "420";
			declaration.ZG_VATDeferType = "S";
			declaration.ZG_VATDeferType = "L";
			Assert("Should not trigger creating SupportingDocument when all invoice lines do not RequiresVATNumberDocument.", !declaration.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.IdentifiedVATNumber));
		}

		public void TestOnVATDeferTypeChangedIfVATDeferNumberIsNotEmptyWhenUCC6()
		{
			PrepareVATRefDatas();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			PrepareImporterData(declaration, true);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			invoiceLine1.JI_FormattedProcedure = "400";
			declaration.ZG_VATDeferType = "S";
			declaration.ZG_VATDeferType = "L";
			Assert("Should trigger creating FiscalReference when ZG_VATDeferType changes.", entryInstruction.FiscalReferences.Cast<CusFiscalReference>().Any(x => x.CFR_Code == DeltaIEFiscalReferenceCodeList.Codes.FR7));

			entryInstruction.FiscalReferences.RemoveAndDeleteAll();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			invoiceLine1.JI_FormattedProcedure = "400";
			declaration.ZG_VATDeferType = "S";
			declaration.ZG_VATDeferType = "L";
			Assert("Should not trigger creating FiscalReference when export.", !entryInstruction.FiscalReferences.Cast<CusFiscalReference>().Any(x => x.CFR_Code == DeltaIEFiscalReferenceCodeList.Codes.FR7));

			entryInstruction.FiscalReferences.RemoveAndDeleteAll();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			invoiceLine1.JI_FormattedProcedure = "420";
			invoiceLine2.JI_FormattedProcedure = "420";
			declaration.ZG_VATDeferType = "S";
			declaration.ZG_VATDeferType = "L";
			Assert("Should not trigger creating FiscalReference when all invoice lines do not RequiresVATNumberDocument.", !entryInstruction.FiscalReferences.Cast<CusFiscalReference>().Any(x => x.CFR_Code == DeltaIEFiscalReferenceCodeList.Codes.FR7));
		}

		public void TestOnVATDeferTypeChangedIfVATDeferNumberIsEmptyWhenNotUCC6()
		{
			PrepareVATRefDatas();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			PrepareImporterData(declaration, false);

			declaration.AdditionalInfos.RemoveAndDeleteAll();
			declaration.SupportingDocuments.RemoveAndDeleteAll();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();

			invoiceLine.JI_FormattedProcedure = "400";
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			Assert("No SupportingDocument before ZG_VATDeferType change", !declaration.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber));

			declaration.ZG_VATDeferType = "L";
			Assert("Should trigger creating SupportingDocument when ZG_VATDeferType changes.", declaration.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber));

			declaration.SupportingDocuments.RemoveAndDeleteAll();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			invoiceLine.JI_FormattedProcedure = "400";
			declaration.ZG_VATDeferType = "S";
			declaration.ZG_VATDeferType = "L";
			Assert("Should not trigger creating SupportingDocument when export.", !declaration.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber));

			declaration.SupportingDocuments.RemoveAndDeleteAll();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			invoiceLine.JI_FormattedProcedure = "420";
			invoiceLine2.JI_FormattedProcedure = "420";
			declaration.ZG_VATDeferType = "S";
			declaration.ZG_VATDeferType = "L";
			Assert("Should not trigger creating SupportingDocument when all invoice lines do not RequiresVATNumberDocument.", !declaration.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber));
		}

		public void TestOnVATDeferTypeChangedIfVATDeferNumberIsEmptyWhenUCC6()
		{
			PrepareVATRefDatas();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			PrepareImporterData(declaration, false);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			invoiceLine1.JI_FormattedProcedure = "400";
			declaration.ZG_VATDeferType = "S";
			declaration.ZG_VATDeferType = "L";
			Assert("Should trigger creating SupportingDocument when ZG_VATDeferType changes.", declaration.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber));

			declaration.SupportingDocuments.RemoveAndDeleteAll();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			invoiceLine1.JI_FormattedProcedure = "400";
			declaration.ZG_VATDeferType = "S";
			declaration.ZG_VATDeferType = "L";
			Assert("Should not trigger creating SupportingDocument when export.", !declaration.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber));

			declaration.SupportingDocuments.RemoveAndDeleteAll();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			invoiceLine1.JI_FormattedProcedure = "420";
			invoiceLine2.JI_FormattedProcedure = "420";
			declaration.ZG_VATDeferType = "S";
			declaration.ZG_VATDeferType = "L";
			Assert("Should not trigger creating SupportingDocument when all invoice lines do not RequiresVATNumberDocument.", !declaration.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber));
		}

		public void TestSpecialMentionsValueWhenZG_VATCANAChange()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, "VAT National Additional Codes");
			helper.CreateCusCodeListWithAttribute("FR", UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, "1035", "Article 1695 du CGI - Autoliquidation de la TVA à l''importation", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.SpecialMention, "61000");
			helper.CreateCusCodeListWithAttribute("FR", UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, "1001", "Je m''engage à respecter les conditions de l''article 275 du CGI - TVA seule", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.SpecialMention, "60900");
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;

			Factory.Save();
			var addinfoFirst = declaration.AdditionalInfos.AddNew();
			addinfoFirst.CSI_Code = VATDeferStrategyCodeList.Codes.Ai2_SpecialMention;

			declaration.ZG_VATCANACode = "1035";
			AssertDoesNotContainCode(declaration.AdditionalInfos, "60900", "Remove all Ai2_SpecialMention");
			AssertContainsOneCode(declaration.AdditionalInfos, "61000", "Add one ALT_SpecialMention");

			declaration.ZG_VATCANACode = "1001";
			AssertContainsOneCode(declaration.AdditionalInfos, "60900", "Add one Ai2_SpecialMention");
			AssertDoesNotContainCode(declaration.AdditionalInfos, "61000", "Remove all ALT_SpecialMention");

			declaration.ZG_VATCANACode = "4042";
			AssertDoesNotContainCode(declaration.AdditionalInfos, "60900", "Remove all Ai2_SpecialMention");
			AssertDoesNotContainCode(declaration.AdditionalInfos, "61000", "Remove all ALT_SpecialMention");
		}

		void AssertContainsOneCode<T>(CusSupportingInfoCollection<T> cusSupportingInfoCollection, ZString code, string because = "") where T : CusSupportingInfo
		{
			var found = cusSupportingInfoCollection.Cast<T>().Where(x => x.CSI_Code == code);
			AssertEquals(because, 1, found.Count());
		}

		void AssertDoesNotContainCode<T>(CusSupportingInfoCollection<T> cusSupportingInfoCollection, ZString code, string because = "") where T : CusSupportingInfo
		{
			var found = cusSupportingInfoCollection.Cast<T>().Where(x => x.CSI_Code == code);
			AssertEquals(because, 0, found.Count());
		}
	}
}
