using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	sealed class SupportingDocumentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUnitOfQuantityList_Cached()
		{
			SupportingDocumentTest.SetUpRefDataForSupportDocumentUnitOfQuantityTest(Factory);
			var declaration = Factory.New<JobDeclaration>();
			var supportingDocument = declaration.SupportingDocuments.AddNew();
			var list = supportingDocument.Lookups.UnitOfQuantityList;
			AssertSame("The UnitOfQuantityList should be cached", list, supportingDocument.Lookups.UnitOfQuantityList);
		}

		public void TestUnitOfQuantityList()
		{
			SupportingDocumentTest.SetUpRefDataForSupportDocumentUnitOfQuantityTest(Factory);
			var declaration = Factory.New<JobDeclaration>();
			var supportingDocument = declaration.SupportingDocuments.AddNew();
			CombineAssertions(() =>
			{
				supportingDocument.CSI_Code = string.Empty;
				var unitList = supportingDocument.Lookups.UnitOfQuantityList;
				AssertEquals("UnitOfQuantityList contains none if CSI_Code is empty.", 0, unitList.Count);

				supportingDocument.CSI_Code = "CODE1";
				unitList = supportingDocument.Lookups.UnitOfQuantityList;
				AssertEquals("UnitOfQuantityList contains codes if CSI_Code have value with UQMapType attribute and the CusMap is configured for the UQMapType value", 3, unitList.Count);
				AssertContainsExactElementsInAnyOrder("UnitOfQuantityList has codes mapped by mapType SGLUQ", new[] { "KGM", "TNE", "MTK" }, unitList.GetAllCodes());
				AssertEquals("UnitOfQuantityList descriptions should return what's in RefCusCodeMap ", "kilo", unitList.GetDescriptionFromCode("KGM"));
				AssertEquals("UnitOfQuantityList descriptions should return what's in RefCusCodeMap ", "tonne", unitList.GetDescriptionFromCode("TNE"));
				AssertEquals("UnitOfQuantityList descriptions should return what's in RefCusCodeMap with their accents and unchanged case", "mètre carré", unitList.GetDescriptionFromCode("MTK"));

				supportingDocument.CSI_Code = "CODE2";
				unitList = supportingDocument.Lookups.UnitOfQuantityList;
				AssertEquals("UnitOfQuantityList contains codes if CSI_Code have value with UQMapType attribute and the CusMap is configured for the UQMapType value", 1, unitList.Count);
				AssertContainsExactElementsInAnyOrder("UnitOfQuantityList has code mapped by mapType CALUQ", new[] { "LTR" }, unitList.GetAllCodes());
				AssertEquals("UnitOfQuantityList descriptions should return what's in RefCusCodeMap ", "litre", unitList.GetDescriptionFromCode("LTR"));

				supportingDocument.CSI_Code = "CODE3";
				unitList = supportingDocument.Lookups.UnitOfQuantityList;
				AssertEquals("UnitOfQuantityList contains none if CSI_Code have value without UQMapType attribute", 0, unitList.Count);

				supportingDocument.CSI_Code = "CODE4";
				unitList = supportingDocument.Lookups.UnitOfQuantityList;
				AssertEquals("UnitOfQuantityList contains none if CSI_Code have value with UQMapType attribute but the CusMap is not configured for the UQMapType value", 0, unitList.Count);
			});
		}

		public void TestCodeListWithoutAttribute()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var fr = helper.CreateNewOrGetExistingDataGrouping("FR", "France");
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, parent: eun);
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.Country.RN_Desc, fr);
		
			helper.CreateNewOrGetExistingCusCodeType("DC44I", GlbCompany.CurrentCompany.Country.RN_Desc);
			helper.CreateNewOrGetExistingCusCodeType("DC44I", "DC44I Desc.");

			ZDateTime dtStart = ZDateTime.MinSmallDateTimeValue;
			ZDateTime dtEnd = ZDateTime.MaxSmallDateTimeValue;

			helper.CreateNewOrGetExistingCusCodeList("FR", "DC44I", "Y001", "A", dtStart, dtEnd);
			helper.CreateNewOrGetExistingCusCodeList("FR", "DC44I", "Y003", "B", dtStart, dtEnd);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, "DC44I", "Y004", "C", dtStart, dtEnd);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "DC44I", "Y005", "C", dtStart, dtEnd);

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;

			var attributeNameValuePairs = new Dictionary<string, string[]>();
			attributeNameValuePairs.Add("Level", new string[] { "HEADER" });
			attributeNameValuePairs.Add("ACTAV", new string[] { "AC", "AE", "AX", "HE" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes("FR",
				new string[] { importCodeType }, "9001", "9001 DES", attributeNameValuePairs, dtStart, dtEnd);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var supportingDocument = invoiceLine.SupportingDocuments.AddNew();

			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			var collection = (ZZRefCusCodeListCombinedCollection)supportingDocument.Lookups.CodeList;
			collection.Load();
			AssertEquals(3, collection.Count);
			collection.Sort("ZZD_Code", System.ComponentModel.ListSortDirection.Ascending);

			AssertEquals("9001", collection[0].ZZD_Code);
			AssertEquals("Y001", collection[1].ZZD_Code);
			AssertEquals("Y003", collection[2].ZZD_Code);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			collection = (ZZRefCusCodeListCombinedCollection)supportingDocument.Lookups.CodeList;
			collection.Load();
			AssertContainsExactElementsInAnyOrder("For DeltaIE, CodeList for supporting document should be codeList of type DC44I under DIE only(not from parent EUN).", new[] { "Y004" }, collection.Cast<ICodeDescription>().Select(x => x.Code));
		}

		public void TestGetIsDTPCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "Supporting Document of Import Direction");
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "0001", "statut juridique", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsDTP, YesNoList.Codes.Yes);
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "2044", "Demande d'autorisation d'importation de radionucléides (DAI) visée par l'IRSN", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "Supporting Document of Export Direction");
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "0001", "attestation  produite par l'ONU ou une de ses institutions spécialisées", new ZDateTime(2022, 11, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsDTP, YesNoList.Codes.Yes);
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "0003", "attestation  produite par l'ONU ou une de ses institutions spécialisées", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsDTP, YesNoList.Codes.Yes);
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "2045", "Demande d'autorisation d'exportation de radionucléides (DAE) visée par l'IRSN", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var jobComInvoiceLine = declaration.InvoiceLines.AddNew();
			var lineSupportingDocument = jobComInvoiceLine.SupportingDocuments.AddNew();
			AssertContainsExactElementsInAnyOrder("1.IsDTP and 3.IsNotDTP", new ZString[] { "0001" }, lineSupportingDocument.Lookups.GetIsDTPCodeList(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, ZDateTime.Now));
			AssertContainsExactElementsInAnyOrder("2.IsDTP and 4.IsNotDTP", new ZString[] { "0003" }, lineSupportingDocument.Lookups.GetIsDTPCodeList(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, new ZDateTime(2022, 10, 1)));
		}

		public void TestD48ZZDCodeSet()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "Supporting Document of Import Direction");
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "0001", "statut juridique", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "2044", "Demande d'autorisation d'importation de radionucléides (DAI) visée par l'IRSN", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "Supporting Document of Export Direction");
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "0001", "attestation  produite par l'ONU ou une de ses institutions spécialisées", new ZDateTime(2022, 11, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "0003", "attestation  produite par l'ONU ou une de ses institutions spécialisées", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "2045", "Demande d'autorisation d'exportation de radionucléides (DAE) visée par l'IRSN", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var supportingDocument = declaration.SupportingDocuments.AddNew();
			AssertIsD48(supportingDocument);

			var jobComInvoiceHeader = declaration.Invoices.AddNew();
			var headerSupportingDocument = jobComInvoiceHeader.SupportingDocuments.AddNew();
			AssertIsD48(headerSupportingDocument);

			var jobComInvoiceLine = declaration.InvoiceLines.AddNew();
			var lineSupportingDocument = jobComInvoiceLine.SupportingDocuments.AddNew();
			AssertIsD48(lineSupportingDocument);
		}

		public void TestGetIsODSCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "Supporting Document of Import Direction");
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "L100", "The document has ODS requirements", new ZDateTime(1900, 1, 1), new ZDateTime(2059, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsODS, YesNoList.Codes.Yes);
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "E013", "The document has ODS requirements", new ZDateTime(1900, 1, 1), new ZDateTime(2059, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsODS, YesNoList.Codes.Yes);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "Supporting Document of Export Direction");
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "L100", "The document has ODS requirements", new ZDateTime(1900, 1, 1), new ZDateTime(2059, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsODS, YesNoList.Codes.Yes);
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "E013", "The document has ODS requirements", new ZDateTime(1900, 1, 1), new ZDateTime(2059, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsODS, YesNoList.Codes.Yes);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var jobComInvoiceLine = declaration.InvoiceLines.AddNew();
			var lineSupportingDocument = jobComInvoiceLine.SupportingDocuments.AddNew();
			AssertContainsExactElementsInAnyOrder("1.IsODS and 3.IsNotODS", new ZString[] { "L100", "E013" }, lineSupportingDocument.Lookups.GetIsODSCodeList(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, ZDateTime.Now));
			AssertContainsExactElementsInAnyOrder("2.IsODS and 4.IsNotODS", new ZString[] { "L100", "E013" }, lineSupportingDocument.Lookups.GetIsODSCodeList(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, new ZDateTime(2022, 10, 1)));
		}

		void AssertIsD48(SupportingDocument sd)
		{
			AssertContainsExactElementsInAnyOrder("1.IsD48 and 3.IsNotD48", new ZString[] { "0001" }, sd.Lookups.GetIsD48CodeList(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, ZDateTime.Now));
			AssertContainsExactElementsInAnyOrder("2.IsD48 and 4.IsNotD48", new ZString[] { "0003" }, sd.Lookups.GetIsD48CodeList(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, new ZDateTime(2022, 10, 1)));
		}
	}
}
