using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.MasterFiles;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class JobComInvoiceLineValueSetStrategyTest : TestCaseWithFactory
	{
		[TestDate(2024, 08, 05, 5, 26, 20)]
		public void TestJI_FormattedProcedureChangeMiscSupportingDocumentEvolve()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France);
			var procedure1 = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "", "50", "0", "   ", "", "IMP", "");
			helper.CreateRefCusProcedureAttribute(procedure1.PK, "VATNumberExempt", "N");
			var procedure2 = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "", "40", "0", "   ", "", "IMP", "");
			helper.CreateRefCusProcedureAttribute(procedure2.PK, "VATNumberExempt", "N");
			var procedure3 = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "", "42", "0", "   ", "", "IMP", "");
			helper.CreateRefCusProcedureAttribute(procedure3.PK, "VATNumberExempt", "Y");

			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "additional info");
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "I9101", "doc I9101", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Category, Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.DCC);
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "10100", "10100 doc", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Export, "Export");
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "50000", "50000 doc", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Export, "Export");
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.TVA, "VATFR345", declaration.CountryCode);

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
			declaration.AdditionalInfos.RemoveAndDeleteAll();
			declaration.SupportingDocuments.RemoveAndDeleteAll();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			invoiceLine.JI_FormattedProcedure = "500";
			Assert(!declaration.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.IdentifiedVATNumber));

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.ZG_VATDeferType = "L";
			declaration.SupportingDocuments.RemoveAndDeleteAll();

			invoiceLine.JI_FormattedProcedure = "";
			Assert(!declaration.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.IdentifiedVATNumber || x.CSI_Code == VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber));

			invoiceLine.JI_FormattedProcedure = "400";
			Assert(declaration.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.IdentifiedVATNumber));

			declaration.SupportingDocuments.RemoveAndDeleteAll();

			invoiceLine.JI_FormattedProcedure = "420";
			Assert(!declaration.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.IdentifiedVATNumber));

			declaration.SupportingDocuments.RemoveAndDeleteAll();

			var addinfo = declaration.AdditionalInfos.AddNew();
			addinfo.CSI_Code = "I9101";
			invoiceLine.JI_FormattedProcedure = "400";
			Assert(!declaration.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.IdentifiedVATNumber));

			declaration.SupportingDocuments.RemoveAndDeleteAll();

			addinfo.CSI_Code = "10100";
			invoiceLine.JI_FormattedProcedure = "500";
			Assert(declaration.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.IdentifiedVATNumber));

			addinfo.CSI_Code = "I9101";
			invoiceLine.JI_FormattedProcedure = "400";
			Assert(declaration.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.IdentifiedVATNumber));

			invoiceLine.JI_FormattedProcedure = "420";
			Assert(declaration.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.IdentifiedVATNumber));

			addinfo.CSI_Code = "10100";
			invoiceLine2.JI_FormattedProcedure = "400";
			Assert(declaration.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.IdentifiedVATNumber));
			var supDoc = declaration.SupportingDocuments.Cast<SupportingDocument>().FirstOrDefault();
			AssertEquals("VATFR345", supDoc.CSI_ReferenceNumber);
			AssertEquals(ZDateTime.Now, supDoc.CSI_DateOfIssue);

			declaration.SupportingDocuments.RemoveAndDeleteAll();

			declaration.JE_OH_Importer = importerAI2WithoutTVA.PK;
			invoiceLine.JI_FormattedProcedure = "500";
			Assert(!declaration.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.IdentifiedVATNumber));

			declaration.JE_OH_Importer = importerALT.PK;
			invoiceLine.JI_FormattedProcedure = "400";
			Assert(declaration.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.IdentifiedVATNumber));

			declaration.SupportingDocuments.RemoveAndDeleteAll();

			declaration.JE_OH_Importer = importerALTWithoutTVA.PK;
			invoiceLine.JI_FormattedProcedure = "500";
			Assert(declaration.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber));
			supDoc = declaration.SupportingDocuments.Cast<SupportingDocument>().FirstOrDefault();
			AssertEquals("Redevable non identifié à la TVA en France", supDoc.CSI_ReferenceNumber);
			AssertEquals(ZDateTime.Now, supDoc.CSI_DateOfIssue);
		}
	}
}
