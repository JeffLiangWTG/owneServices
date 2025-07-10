using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.Testing
{
	class SupportingDocumentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeList_Import()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.Country.RN_Desc, eun);

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var attributeNameValuePairs = new Dictionary<string, string[]>();
			attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
			attributeNameValuePairs.Add("ACTAV", new string[] { "AC", "AE", "AX", "HE" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				new string[] { importCodeType }, "9001", "9001 DES", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var supportingDocument = invoiceLine.SupportingDocuments.AddNew();

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var collection = (ZZRefCusCodeListCombinedCollection)supportingDocument.Lookups.CodeList;
			collection.Load();
			AssertEquals(1, collection.Count);
			AssertEquals("9001", collection[0].ZZD_Code);
		}

		public void TestCodeList_UnionOfParentAndChild()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.Country.RN_Desc, eun);

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var attributeNameValuePairs = new Dictionary<string, string[]>();
			attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
			attributeNameValuePairs.Add("ACTAV", new string[] { "AC", "AE", "AX", "HE" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				new string[] { importCodeType }, "9001", "9001 DES", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			attributeNameValuePairs = new Dictionary<string, string[]>();
			attributeNameValuePairs.Add("Level", new string[] { "ITEM", "HEADER" });
			attributeNameValuePairs.Add("ACTAV", new string[] { "AC" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(GlbCompany.CurrentCompany.Country.Code,
				new string[] { importCodeType }, "9002", "9002 DES CURRENT COMPANY COUNTRY", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var supportingDocument = invoiceLine.SupportingDocuments.AddNew();

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var collection = (ZZRefCusCodeListCombinedCollection)supportingDocument.Lookups.CodeList;
			collection.Load();
			CombineAssertions("Should load for parent and child grouping", () =>
			{
				AssertEquals(2, collection.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "9001", "9002" }, collection.Select(x => x.ZZD_Code));
			});
		}

		public void TestPermitCodes()
		{
			Factory.CreateSupportingDocumentCodeLists(
				new TestSupportingDocumentCodeList("AAA", isImport: true, hasPermitAttribute: true),
				new TestSupportingDocumentCodeList("BBB", isImport: false, hasPermitAttribute: true),
				new TestSupportingDocumentCodeList("CCC", isImport: true, hasPermitAttribute: false));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var supportingDocument = declaration.SupportingDocuments.AddNew();
			var permitCodes = supportingDocument.Lookups.PermitCodes;
			AssertContainsExactElementsInAnyOrder("It should load all code lists of export type with PERMIT attribute.", new string[] { "BBB" }, permitCodes);

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			permitCodes = supportingDocument.Lookups.PermitCodes;
			AssertContainsExactElementsInAnyOrder("It should load all code lists of import type with PERMIT attribute.", new string[] { "AAA" }, permitCodes);
		}

		public void TestPermitCodes_Cached()
		{
			Factory.CreateSupportingDocumentCodeLists(
				new TestSupportingDocumentCodeList("AAA", isImport: true, hasPermitAttribute: true),
				new TestSupportingDocumentCodeList("BBB", isImport: false, hasPermitAttribute: true),
				new TestSupportingDocumentCodeList("CCC", isImport: true, hasPermitAttribute: false));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var supportingDocument = declaration.SupportingDocuments.AddNew();
			var permitCodesExport = supportingDocument.Lookups.PermitCodes;

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var permitCodesImport = supportingDocument.Lookups.PermitCodes;
			AssertNotSame("The permit codes should be different if the direction changed.", permitCodesImport, permitCodesExport);

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var permitCodesExport2 = supportingDocument.Lookups.PermitCodes;
			AssertSame("The export permit codes should be cached.", permitCodesExport, permitCodesExport2);
		}

		public void TestPermits()
		{
			Factory.CreateSupportingDocumentCodeLists(
				new TestSupportingDocumentCodeList("AAA", hasPermitAttribute: true),
				new TestSupportingDocumentCodeList("BBB", hasPermitAttribute: true),
				new TestSupportingDocumentCodeList("CCC", hasPermitAttribute: true, country: Core.Constants.CountryCodes.SouthAfrica));

			var permit1 = Factory.NewWithValidTestData<CusPermitHeader>();
			permit1.CPH_Type = "AAA";
			permit1.CPH_Number = "P1";

			var permit2 = Factory.NewWithValidTestData<CusPermitHeader>();
			permit2.CPH_Type = "BBB";
			permit2.CPH_Number = "P2";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var permit3 = Factory.NewWithValidTestData<CusPermitHeader>();
				permit3.CPH_Type = "CCC";
				permit3.CPH_Number = "P3";
			}
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var supportingDocument = declaration.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = "AAA";
			var permits = supportingDocument.Lookups.Permits;
			AssertContainsExactElementsInAnyOrder("When we load permit numbers: a. we will never load the permits from other country, b. We don't care whether the CPH_Type equals to CSI_Code.",
				new[] { "P1", "P2" }, permits.Select(x => x.CPH_Number));
		}

		public void TestPermits_FilterBusinessObjectDefaults()
		{
			var declaration = Factory.New<JobDeclaration>();
			var supportingDocument = declaration.SupportingDocuments.AddNew();

			supportingDocument.CSI_Code = "AAA";
			var permits = supportingDocument.Lookups.Permits;
			AssertEquals("AAA", permits.FilterBusinessObjectDefaults.Cast<FilterBusinessObjectDefault>().Single(x => x.FilterName == CusPermitHeaderCollection.FilterConstants.PermitTypeSubType && x.PropertyName == "Property3").Value);

			supportingDocument.CSI_Code = "BBB";
			permits = supportingDocument.Lookups.Permits;
			AssertEquals("BBB", permits.FilterBusinessObjectDefaults.Cast<FilterBusinessObjectDefault>().Single(x => x.FilterName == CusPermitHeaderCollection.FilterConstants.PermitTypeSubType && x.PropertyName == "Property3").Value);
		}

		public void TestUnitOfQuantityList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var ie = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentsUnitOfMeasure, "SUPUQ");
			var cusCodeA = helper.CreateCusCodeList(Core.Constants.CountryCodes.Ireland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentsUnitOfMeasure, "TESTA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var cusCodeB = helper.CreateCusCodeList(Core.Constants.CountryCodes.Ireland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentsUnitOfMeasure, "TESTB", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			var invoiceHeader = dec.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var supportingDocumentForLine = invoiceLine.SupportingDocuments.AddNew();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var uqList = supportingDocumentForLine.Lookups.UnitOfQuantityList;
				CombineAssertions(() =>
				{
					AssertContainsExactElementsInAnyOrder("UQ list comes from ZZ db.", new[] { "TESTA", "TESTB" }, uqList.GetAllCodes());
					AssertSame("Cached", uqList, supportingDocumentForLine.Lookups.UnitOfQuantityList);
				});
			}
		}

		public void TestStatementTextList()
		{
			SupportingDocumentTestHelper.SetupRefCusCodeList(Factory);
			var countryCode = Core.Constants.CountryCodes.UnitedKingdom;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = "CHF";
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				var supportingDocument = declaration.SupportingDocuments.AddNew();

				supportingDocument.CSI_Code = "Y057";
				AssertEquals(1, supportingDocument.Lookups.StatementTextList.Count);
				AssertEquals("Import licence not required", supportingDocument.Lookups.StatementTextList[0].Code);

				supportingDocument.CSI_Code = "Y922";
				AssertEquals(2, supportingDocument.Lookups.StatementTextList.Count);
				var expectedCodes = new[] { "Education and taxidermy only", "No cat or dog fur" };
				AssertContainsExactElementsInAnyOrder(expectedCodes, supportingDocument.Lookups.StatementTextList.ToArray().Select(x => x.Code));

				supportingDocument.CSI_Code = "T123";
				AssertEquals(0, supportingDocument.Lookups.StatementTextList.Count);

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				supportingDocument.CSI_Code = "Y057";
				AssertEquals(0, supportingDocument.Lookups.StatementTextList.Count);
			}
		}
	}
}
