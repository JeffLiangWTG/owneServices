using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Registry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	internal class SupportingDocumentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeList()
		{
			PrepareCusCodeListData();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			supportingDocument = invoiceLine.SupportingDocuments.AddNew();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var collection = (ZZRefCusCodeListCombinedCollection)supportingDocument.Lookups.CodeList;
			collection.Load();
			AssertEquals(2, collection.Count);
			AssertContainsExactElementsInAnyOrder("The list of codes on the Inv.Lines tab filters to find CDS data grouping with Level=header and having level=item, CodeList Attributes", ["9001", "9002"]
			, collection.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code).ToArray());

			var supportingDocumentForHeader = invoiceHeader.SupportingDocuments.AddNew();
			collection = (ZZRefCusCodeListCombinedCollection)supportingDocumentForHeader.Lookups.CodeList;
			collection.Load();
			AssertEquals(2, collection.Count);
			AssertContainsExactElementsInAnyOrder("The list of codes on the Inv.Headers tab filters to find CDS data grouping with Level=header and having level=item, CodeList Attributes", ["9001", "9002"]
			, collection.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code).ToArray());

			var supportingDocumentForDeclaration_MiscTab = declaration.SupportingDocuments.AddNew();
			collection = (ZZRefCusCodeListCombinedCollection)supportingDocumentForDeclaration_MiscTab.Lookups.CodeList;
			collection.Load();
			AssertEquals(2, collection.Count);
			AssertContainsExactElementsInAnyOrder("The list of codes on the Declaration_Misc tab filters to find CDS data grouping with Level=header and having level=item, CodeList Attributes", ["9001", "9002"]
			, collection.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code).ToArray());

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;
			collection = (ZZRefCusCodeListCombinedCollection)supportingDocument.Lookups.CodeList;
			collection.Load();
			AssertEquals(1, collection.Count);
			AssertEquals("9001", collection[0].ZZD_Code);

			supportingDocumentForHeader = invoiceHeader.SupportingDocuments.AddNew();
			collection = (ZZRefCusCodeListCombinedCollection)supportingDocumentForHeader.Lookups.CodeList;
			collection.Load();
			AssertEquals("9001", collection[0].ZZD_Code);
		}

		public void TestCodeList_ChildFirst()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			var gb = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, "United Kingdom", eun);

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			var attributeNameValuePairs = new Dictionary<string, string[]>
			{
				{ "Level", new string[] { "ITEM", "HEADER" } },
				{ "ACTAV", new string[] { "AC" } }
			};
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				[importCodeType, exportCodeType], "9999", "9999 DES", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			attributeNameValuePairs.Clear();
			attributeNameValuePairs.Add("Level", ["ITEM"]);
			attributeNameValuePairs.Add("ACTAV", ["AC"]);
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.UnitedKingdom,
				[importCodeType, exportCodeType], "9998", "9998 DES", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			invoiceHeader = declaration.Invoices.AddNew();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var supportingDocumentForHeader = invoiceHeader.SupportingDocuments.AddNew();
			var collection = (ZZRefCusCodeListCombinedCollection)supportingDocumentForHeader.Lookups.CodeList;
			collection.Load();

			CombineAssertions("Only child grouping supporting doc code is retrieved", () =>
			{
				AssertEquals(1, collection.Count);
				AssertEquals("9998", collection[0].ZZD_Code);
				AssertEquals(gb.ZZZ_DataGrouping, collection[0].ZZD_CountryOrGrouping);
			});
		}

		public void TestCodeList_ThenParent()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, "United Kingdom", eun);

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			var attributeNameValuePairs = new Dictionary<string, string[]>
			{
				{ "Level", new string[] { "ITEM", "HEADER" } },
				{ "ACTAV", new string[] { "AC" } }
			};
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				new string[] { importCodeType, exportCodeType }, "9999", "9999 DES", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;
			invoiceHeader = declaration.Invoices.AddNew();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var supportingDocumentForHeader = invoiceHeader.SupportingDocuments.AddNew();
			var collection = (ZZRefCusCodeListCombinedCollection)supportingDocumentForHeader.Lookups.CodeList;
			collection.Load();

			CombineAssertions("No child grouping codes found, parent grouping codes retrieved", () =>
			{
				AssertEquals(1, collection.Count);
				AssertEquals(eun.ZZZ_DataGrouping, collection[0].ZZD_CountryOrGrouping);
			});
		}

		#region Implementation
		void PrepareCusCodeListData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, "United Kingdom", parent: eun);
			helper.CreateNewOrGetExistingDataGrouping(DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, parent: eun);

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			var attributeNameValuePairs = new Dictionary<string, string[]>
			{
				{ "Level", new string[] { "ITEM" } },
				{ "ACTAV", new string[] { "AC", "AE", "AX", "HE" } }
			};
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				[importCodeType, exportCodeType], "9001", "9001 DES", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var attributeNameValuePairs2 = new Dictionary<string, string[]>
			{
				{ "Level", new string[] { "HEADER" } },
				{ "ACTAV", new string[] { "AC", "AE", "AX", "HE" } }
			};
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				[importCodeType, exportCodeType], "9002", "9002 DES", attributeNameValuePairs2, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
		}

		SupportingDocument supportingDocument;
		JobComInvoiceLine invoiceLine;
		JobComInvoiceHeader invoiceHeader;
		JobDeclaration declaration;

		#endregion
	}
}
