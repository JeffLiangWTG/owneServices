using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class PreviousDocumentLookupsTest : EU.Business.Declaration.MultiLineAddInfos.Testing.PreviousDocumentLookupsTest
	{
		public void TestReferenceList_OnlyForIST()
		{
			var declaration = Factory.New<JobDeclaration>();
			var decPreDoc = declaration.PreviousDocuments.AddNew();
			CombineAssertions("Only CSI_Code = IST does CSI_Reference get a lookup", () =>
			{
				decPreDoc.CSI_Code = "XXX";
				AssertNull(decPreDoc.Lookups.ReferenceList);
				decPreDoc.CSI_Code = "IST";
				AssertNotNull(decPreDoc.Lookups.ReferenceList);
			});
		}

		public override void TestReferenceList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var decPreDoc = declaration.PreviousDocuments.AddNew();
			CombineAssertions(() =>
			{
				decPreDoc.CSI_Code = string.Empty;
				AssertNull("ReferenceList is null if the previous document code is not IST", decPreDoc.Lookups.ReferenceList);

				decPreDoc.CSI_Code = PreviousDocumentCodeList.Codes.IST;
				var referenceList = decPreDoc.Lookups.ReferenceList;
				AssertNotNull("ReferenceList is not null if the previous document code is IST", referenceList);
				AssertType<CusTempStorageRegHeaderCollection>("ReferenceList is of type CusTempStorageRegHeaderCollection if previous document code is IST", referenceList);

				decPreDoc.CSI_Code = UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage;
				referenceList = decPreDoc.Lookups.ReferenceList;
				AssertNotNull("ReferenceList is not null if the previous document code is N337", referenceList);
				AssertType<CusTempStorageRegHeaderCollection>("ReferenceList is of type CusTempStorageRegHeaderCollection if previous document code is N337", referenceList);
			});
		}

		public void TestReferenceListFilters()
		{
			var declaration = Factory.New<JobDeclaration>();
			var decPreDoc = declaration.PreviousDocuments.AddNew();
			decPreDoc.CSI_Code = PreviousDocumentCodeList.Codes.IST;
			var cusTempStorageHeaders = decPreDoc.Lookups.ReferenceList as CusTempStorageRegHeaderCollection;
			var filters = cusTempStorageHeaders.FilterBusinessObjectDefaults;
			CombineAssertions("IST Filters", () =>
			{
				AssertEquals("Count", 3, filters.Count);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.Status, value: "OPN", isRemovable: false);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.CustomsOffice, value: string.Empty, isRemovable: true);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.PackingType, value: string.Empty, isRemovable: true);
			});

			decPreDoc.CSI_Code = UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage;
			cusTempStorageHeaders = decPreDoc.Lookups.ReferenceList as CusTempStorageRegHeaderCollection;
			filters = cusTempStorageHeaders.FilterBusinessObjectDefaults;
			CombineAssertions("N337 Filters", () =>
			{
				AssertEquals("Count", 5, filters.Count);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.TsdNumber, value: string.Empty, isRemovable: true);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.Status, value: "OPN", isRemovable: false);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.CustomsOffice, value: string.Empty, isRemovable: true);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.PackingType, value: string.Empty, isRemovable: true);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.PreviousReferenceNumber, value: string.Empty, isRemovable: true);
			});
		}

		public void TestReferenceListFilters_IfAnyJE_CustomsOffice()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "FROFC";
			var decPreDoc = declaration.PreviousDocuments.AddNew();
			decPreDoc.CSI_Code = PreviousDocumentCodeList.Codes.IST;
			var cusTempStorageHeaders = decPreDoc.Lookups.ReferenceList as CusTempStorageRegHeaderCollection;
			var filters = cusTempStorageHeaders.FilterBusinessObjectDefaults;
			CombineAssertions("IST Filters", () =>
			{
				AssertEquals("Count", 3, filters.Count);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.Status, value: "OPN", isRemovable: false);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.CustomsOffice, value: "FROFC", isRemovable: true);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.PackingType, value: string.Empty, isRemovable: true);
			});

			decPreDoc.CSI_Code = UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage;
			cusTempStorageHeaders = decPreDoc.Lookups.ReferenceList as CusTempStorageRegHeaderCollection;
			filters = cusTempStorageHeaders.FilterBusinessObjectDefaults;
			CombineAssertions("N337 Filters", () =>
			{
				AssertEquals("Count", 5, filters.Count);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.TsdNumber, value: string.Empty, isRemovable: true);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.Status, value: "OPN", isRemovable: false);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.CustomsOffice, value: "FROFC", isRemovable: true);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.PackingType, value: string.Empty, isRemovable: true);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.PreviousReferenceNumber, value: string.Empty, isRemovable: true);
			});
		}

		public void TestReferenceListFilters_IfForInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var invLinePreDoc = invoiceLine.PreviousDocuments.AddNew();
			invLinePreDoc.CSI_Code = PreviousDocumentCodeList.Codes.IST;
			var cusTempStorageHeaders = invLinePreDoc.Lookups.ReferenceList as CusTempStorageRegHeaderCollection;
			var filters = cusTempStorageHeaders.FilterBusinessObjectDefaults;
			CombineAssertions("IST Filters: If no packages for this line", () =>
			{
				AssertEquals("Count ", 3, filters.Count);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.Status, value: "OPN", isRemovable: false);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.CustomsOffice, value: string.Empty, isRemovable: true);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.PackingType, value: string.Empty, isRemovable: true);
			});

			invLinePreDoc.CSI_Code = UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage;
			cusTempStorageHeaders = invLinePreDoc.Lookups.ReferenceList as CusTempStorageRegHeaderCollection;
			filters = cusTempStorageHeaders.FilterBusinessObjectDefaults;
			CombineAssertions("N337 Filters: If no packages for this line", () =>
			{
				AssertEquals("Count ", 5, filters.Count);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.TsdNumber, value: string.Empty, isRemovable: true);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.Status, value: "OPN", isRemovable: false);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.CustomsOffice, value: string.Empty, isRemovable: true);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.PackingType, value: string.Empty, isRemovable: true);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.PreviousReferenceNumber, value: string.Empty, isRemovable: true);
			});

			var package1 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			var pack = invoiceLine.PackagesForInvoiceLinesForBindingOnly[0];
			pack.IsLinked = true;
			pack.Package.CW_PackType = "1D";
			pack.Package.CW_PackQty = 10;
			invLinePreDoc.CSI_Code = PreviousDocumentCodeList.Codes.IST;
			cusTempStorageHeaders = invLinePreDoc.Lookups.ReferenceList as CusTempStorageRegHeaderCollection;
			filters = cusTempStorageHeaders.FilterBusinessObjectDefaults;
			CombineAssertions("IST Filters: If any packages for this line", () =>
			{
				AssertEquals("Count ", 3, filters.Count);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.Status, value: "OPN", isRemovable: false);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.CustomsOffice, value: string.Empty, isRemovable: true);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.PackingType, value: "1D", isRemovable: true);
			});

			invLinePreDoc.CSI_Code = UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage;
			cusTempStorageHeaders = invLinePreDoc.Lookups.ReferenceList as CusTempStorageRegHeaderCollection;
			filters = cusTempStorageHeaders.FilterBusinessObjectDefaults;
			CombineAssertions("N337 Filters: If any packages for this line", () =>
			{
				AssertEquals("Count ", 5, filters.Count);
				AssertFilterBODefault(filters, Declaration.PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.TsdNumber, value: string.Empty, isRemovable: true);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.Status, value: "OPN", isRemovable: false);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.CustomsOffice, value: string.Empty, isRemovable: true);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.PackingType, value: "1D", isRemovable: true);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.PreviousReferenceNumber, value: string.Empty, isRemovable: true);
			});
		}

		public void TestReferenceListFilters_IfAnyCSI_ReferenceNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var decPreDoc = declaration.PreviousDocuments.AddNew();
			decPreDoc.CSI_Code = UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage;
			decPreDoc.CSI_ReferenceNumber = "12345";
			var cusTempStorageHeaders = decPreDoc.Lookups.ReferenceList as CusTempStorageRegHeaderCollection;
			var filters = cusTempStorageHeaders.FilterBusinessObjectDefaults;
			CombineAssertions("N337 Filters", () =>
			{
				AssertEquals("Count", 5, filters.Count);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.TsdNumber, value: "12345", isRemovable: true);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.Status, value: "OPN", isRemovable: false);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.CustomsOffice, value: string.Empty, isRemovable: true);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.PackingType, value: string.Empty, isRemovable: true);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.PreviousReferenceNumber, value: string.Empty, isRemovable: true);
			});
		}

		void AssertFilterBODefault(FilterBusinessObjectDefaults filters, string propertyName, string value, bool isRemovable)
		{
			var filterKey = new StringBuilder().Append(propertyName).Append(FilterBusinessObjectDefault.FilterPropertyDelimiter).Append("Property").ToString();
			var filter = filters[filterKey];
			AssertEquals($"{propertyName} filter value", filter.Value, value);
			AssertEquals($"{propertyName} Filter removable", isRemovable, filter.IsRemovable);
		}

		public new void TestCodeList()
		{
			SetUp214IMCodeList();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			var previousDocument = declaration.Invoices.AddNew().PreviousDocuments.AddNew();
			AssertContainsExactElementsInAnyOrder("For DeltaG, CodeList for previous document should be hard-coded PreviousDocumentCodeList.", new PreviousDocumentCodeList(), previousDocument.Lookups.CodeList);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertContainsExactElementsInAnyOrder("For DeltaIE, CodeList for previous document should include values of type 214IM from both FR and EU.", new[] { "1DAU", "1MDF", "ZZZ" }, previousDocument.Lookups.CodeList.Cast<ICodeDescription>().Select(x => x.Code));

			void SetUp214IMCodeList()
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
				helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, parent: eun);

				helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfUCCImport, "214IM Desc.");
				helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfUCCImport, "1DAU", "1DAU Desc.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfUCCImport, "1MDF", "1MDF Desc.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

				helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfUCCImport, "ZZZ", "ZZZ Desc.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				Factory.Save();
			}
		}

		public void TestPackageTypeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"VG", "VG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var previousDocument = declaration.Invoices.AddNew().PreviousDocuments.AddNew();
			var lookups = new PreviousDocumentLookups(previousDocument);
			var list = lookups.PackageTypeList;
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("List", new[] { "VG", "NE" }, list.GetAllCodes());
				AssertSame("Cached", list, lookups.PackageTypeList);
			});
		}

		public void TestUnitOfQuantityList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, "France");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "CustomsUQ");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "UQ1", "test UQ1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "UQ2", "test UQ2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var previousDocument = declaration.Invoices.AddNew().PreviousDocuments.AddNew();
			var lookups = new PreviousDocumentLookups(previousDocument);
			var list = lookups.UnitOfQuantityList;
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("List", new[] { "UQ1", "UQ2" }, list.GetAllCodes());
				AssertSame("Cached", list, lookups.UnitOfQuantityList);
			});
		}
	}
}
