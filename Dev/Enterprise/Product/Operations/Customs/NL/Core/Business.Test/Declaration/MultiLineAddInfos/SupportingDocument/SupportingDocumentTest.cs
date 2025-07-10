using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(SupportingDocument))]
sealed class SupportingDocumentTest : Customs.Business.Testing.CusSupportingInfoTest<SupportingDocument>
{
	public void TestReferenceNumber2()
	{
		var doc = (SupportingDocument)GetNewBusinessObject();
		AssertEquals(70, doc.CSI_ReferenceNumber2Info.MaxLength);
	}

	public void TestLookupsType()
	{
		AssertType<SupportingDocumentLookups>(supDoc.Lookups);
	}

	public void TestValidationType()
	{
		AssertType<SupportingDocumentValidation>(supDoc.Validation);
	}

	public void TestIsEffectiveSupportingDocumentsForLine()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
		helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.Country.RN_Desc, eun);

		var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
		var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
		var attributeNameValuePairs = new Dictionary<string, string[]>();

		attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
		helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
			new string[] { importCodeType, exportCodeType }, "9001", "9001 DES", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		attributeNameValuePairs.Clear();
		attributeNameValuePairs.Add("Level", new string[] { "HEADER" });
		helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
			new string[] { importCodeType, exportCodeType }, "9002", "9002 DES", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		attributeNameValuePairs.Clear();
		attributeNameValuePairs.Add("Level", new string[] { "ITEM", "HEADER" });
		helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
			new string[] { importCodeType, exportCodeType }, "9003", "9003 DES", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		Factory.Save();

		CombineAssertions(() =>
		{
			supDoc.CSI_Code = "9001";
			AssertEquals(supDoc.CSI_Code, true, supDoc.IsEffectiveSupportingDocumentsForLine);

			supDoc.CSI_Code = "9002";
			AssertEquals(supDoc.CSI_Code, false, supDoc.IsEffectiveSupportingDocumentsForLine);

			supDoc.CSI_Code = "9003";
			AssertEquals(supDoc.CSI_Code, true, supDoc.IsEffectiveSupportingDocumentsForLine);
		});
	}

	protected override IEnumerable<SupportingDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		yield return declaration.SupportingDocuments.AddNew();
		var invoice = declaration.Invoices.AddNew();
		yield return invoice.SupportingDocuments.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		yield return invoiceLine.SupportingDocuments.AddNew();
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		return invoiceLine.SupportingDocuments.AddNew();
	}

	protected override void SetUp()
	{
		base.SetUp();
		var decl = Factory.New<JobDeclaration>();
		supDoc = decl.SupportingDocuments.AddNew();
	}
	SupportingDocument supDoc;
}
