using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BE.Business.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

class PreviousDocumentLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCodeList()
	{
		const string previousDocumentTypeExport = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection;
		const string previousDocumentTypeImport = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfImportDirection;
		var helper = new UniversalReferenceTestDataHelper(Factory);

		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Belgium, "Belgium", helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN"));
		helper.CreateNewOrGetExistingCusCodeType(previousDocumentTypeExport, "Previous Documents Export (BOX40)");
		helper.Create_CusCodeList_RefData(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, previousDocumentTypeExport, "T1");
		helper.Create_CusCodeList_RefData(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, previousDocumentTypeImport, "T1");
		helper.Create_CusCodeList_RefData(Core.Constants.CountryCodes.Belgium, previousDocumentTypeExport, "AA");
		helper.Create_CusCodeList_RefData(Core.Constants.CountryCodes.Belgium, previousDocumentTypeExport, "BB");
		helper.Create_CusCodeList_RefData(Core.Constants.CountryCodes.Belgium, previousDocumentTypeImport, "CC");
		helper.Create_CusCodeList_RefData(Core.Constants.CountryCodes.Belgium, previousDocumentTypeImport, "DD");

		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var previousDocument = invoiceLine.PreviousDocuments.AddNew();
		var lookups = new PreviousDocumentLookups(previousDocument);
		var collection = (ZZRefCusCodeListCombinedCollection)lookups.CodeList;

		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			collection.Load();
			AssertContainsExactElementsInAnyOrder(new[] { "AA", "BB" }, collection.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			collection = (ZZRefCusCodeListCombinedCollection)lookups.CodeList;
			collection.Load();
			AssertContainsExactElementsInAnyOrder(new[] { "CC", "DD" }, collection.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
		});
	}

	public void TestPackageCodeList()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);

		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, "UnitedNationsRecommendations", helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN"));
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Belgium, "BE");
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UNPKG");
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "PKG");
		helper.Create_CusCodeList_RefData(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "1A");
		helper.Create_CusCodeList_RefData(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "1B");
		helper.Create_CusCodeList_RefData(Core.Constants.CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "1C");
		helper.Create_CusCodeList_RefData(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "1D");
		helper.Create_CusCodeList_RefData(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "1E");

		Factory.Save();

		var lookups = new PreviousDocumentLookups(Factory.New<PreviousDocument>());
		var packageCodeList = lookups.PackageCodeList;

		CombineAssertions(() =>
		{
			AssertEquals("Codes", "1A, 1D, 1E", packageCodeList.CodesAsString);
			AssertSame("Cached", lookups.PackageCodeList, packageCodeList);
		});
	}
}
