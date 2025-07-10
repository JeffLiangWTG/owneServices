using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Module.Testing
{
	[TestedType(typeof(EntryHeaderFilterBusinessObject))]
	sealed class EntryHeaderFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestSupportRequestedProcedure()
		{
			Assert(!new EntryHeaderFilterBusinessObject().SupportRequestedProcedure);
		}

		public void TestLookups()
		{
			var entryHeaderFilterLookups = (EntryHeaderFilterBusinessObject)GetNewFilterStripBusinessObject();
			AssertType<EntryHeaderFilterLookups>("Lookups Type", entryHeaderFilterLookups.Lookups);
		}

		public void TestCustomsOfficeCategory()
		{
			var entryHeaderFilterLookups = (EntryHeaderFilterBusinessObject)GetNewFilterStripBusinessObject();
			AssertNotNull(nameof(entryHeaderFilterLookups.CustomsOfficeCategory), entryHeaderFilterLookups.CustomsOfficeCategory);
			AssertEquals("Customs Offices", entryHeaderFilterLookups.CustomsOfficeCategory.Description);
		}

		public void TestImporterSupplierCodesFilterMultilingualDescription()
		{
			var filterStripBusinessObject = GetNewFilterStripBusinessObject();

			var importerSupplierGuidFilter = filterStripBusinessObject["Importer/Supplier"];
			AssertNotNull("Importer / Supplier Filter", importerSupplierGuidFilter);
			AssertEquals("Importer / Supplier Multilingual Description", "Importer/Supplier (Codes)", importerSupplierGuidFilter.MultilingualDescription);
		}

		public void TestEntryNumberFilterMultilingualDescription()
		{
			var filterStripBusinessObject = GetNewFilterStripBusinessObject();

			var mrnFilter = filterStripBusinessObject["Entry Number"];
			AssertNotNull("MRN Filter", mrnFilter);
			AssertEquals("MRN Description", "MRN", mrnFilter.MultilingualDescription);
		}

		public void TestImporterFullNameFilter()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_FullName = "IMPORTER FULL NAME";

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_OH_Importer = importer.PK;
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();

			var declaration2 = Factory.New<JobDeclaration>();
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();

			Factory.Save();

			var filterStrip = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStrip["Importer (Full Name)"];
			AssertNotNull("Filter Importer by Full Name", filter);
			AssertEquals("Importer Multilingual Description", "Importer (Full Name)", filter.MultilingualDescription);
			filter.IsActive = true;

			filter.Property = "IMPORTER FULL NAME";
			CombineAssertions(() =>
			{
				AssertEquals("Entry of Declaration1, MatchesFilter", true, entry1.MatchesFilter(filterStrip.Filter));
				AssertEquals("Entry of Declaration2, MatchesFilter", false, entry2.MatchesFilter(filterStrip.Filter));
			});

			filter.Property = "";
			filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank;
			CombineAssertions(() =>
			{
				AssertEquals("Entry of Declaration1, MatchesFilter", false, entry1.MatchesFilter(filterStrip.Filter));
				AssertEquals("Entry of Declaration2, MatchesFilter", true, entry2.MatchesFilter(filterStrip.Filter));
			});
		}

		public void TestSupplierFullNameFilter()
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_FullName = "SUPPLIER FULL NAME";

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_OH_Supplier = supplier.PK;
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();

			var declaration2 = Factory.New<JobDeclaration>();
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();

			Factory.Save();

			var filterStrip = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStrip["Supplier (Full Name)"];
			AssertNotNull("Filter Supplier by Full Name", filter);
			AssertEquals("Supplier Multilingual Description", "Supplier (Full Name)", filter.MultilingualDescription);
			filter.IsActive = true;

			filter.Property = "SUPPLIER FULL NAME";
			CombineAssertions(() =>
			{
				AssertEquals("Entry of Declaration1, MatchesFilter", true, entry1.MatchesFilter(filterStrip.Filter));
				AssertEquals("Entry of Declaration2, MatchesFilter", false, entry2.MatchesFilter(filterStrip.Filter));
			});

			filter.Property = "";
			filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank;
			CombineAssertions(() =>
			{
				AssertEquals("Entry of Declaration1, MatchesFilter", false, entry1.MatchesFilter(filterStrip.Filter));
				AssertEquals("Entry of Declaration2, MatchesFilter", true, entry2.MatchesFilter(filterStrip.Filter));
			});
		}

		public void TestExitedStatusFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_ExitedStatus = ExportExitStatus.Codes.ExitedSatisfactorily;

			var declaration2 = Factory.New<JobDeclaration>();
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			entry2.CH_ExitedStatus = ExportExitStatus.Codes.ExitOfGoodsIsUnsatisfactory;

			var declaration3 = Factory.New<JobDeclaration>();
			var entry3 = declaration3.CustomsEntryHeaders.AddNew();
			entry3.CH_ExitedStatus = ZString.Empty;

			Factory.Save();

			var filterStrip = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStrip[EntryHeaderFilterBusinessObject.EUFilterConstants.ExitedStatus];
			AssertNotNull("Filter " + EntryHeaderFilterBusinessObject.EUFilterConstants.ExitedStatus, filter);
			AssertEquals("ExitedStatus Multilingual Description", EntryHeaderFilterBusinessObject.EUFilterConstants.ExitedStatus, filter.MultilingualDescription);

			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = ExportExitStatus.Codes.UnknownOrNotReported;
			CombineAssertions(() =>
			{
				AssertEquals("Entry of Declaration1, MatchesFilter", expected: true, entry1.MatchesFilter(filterStrip.Filter));
				AssertEquals("Entry of Declaration2, MatchesFilter", expected: true, entry2.MatchesFilter(filterStrip.Filter));
				AssertEquals("Entry of Declaration3, MatchesFilter", expected: true, entry3.MatchesFilter(filterStrip.Filter));
			});

			filter.Property = ExportExitStatus.Codes.ExitedSatisfactorily;
			CombineAssertions(() =>
			{
				AssertEquals("Entry of Declaration1, MatchesFilter", expected: true, entry1.MatchesFilter(filterStrip.Filter));
				AssertEquals("Entry of Declaration2, MatchesFilter", expected: false, entry2.MatchesFilter(filterStrip.Filter));
				AssertEquals("Entry of Declaration3, MatchesFilter", expected: false, entry3.MatchesFilter(filterStrip.Filter));
			});

			filter.Property = ExportExitStatus.Codes.ExitOfGoodsIsUnsatisfactory;
			CombineAssertions(() =>
			{
				AssertEquals("Entry of Declaration1, MatchesFilter", expected: false, entry1.MatchesFilter(filterStrip.Filter));
				AssertEquals("Entry of Declaration2, MatchesFilter", expected: true, entry2.MatchesFilter(filterStrip.Filter));
				AssertEquals("Entry of Declaration3, MatchesFilter", expected: false, entry3.MatchesFilter(filterStrip.Filter));
			});

			var operatorsToTest = new[] { SQLComparisonOperator.NotEqual, SQLComparisonOperator.NotContains, SQLComparisonOperator.DoesNotStartWith };
			foreach (var comparisonOperator in operatorsToTest)
			{
				filter.SqlComparisonOperator = comparisonOperator;
				filter.Property = ExportExitStatus.Codes.ExitedSatisfactorily;
				CombineAssertions(() =>
				{
					AssertEquals($"Entry of Declaration1, MatchesFilter, {comparisonOperator}", expected: false, entry1.MatchesFilter(filterStrip.Filter));
					AssertEquals($"Entry of Declaration2, MatchesFilter, {comparisonOperator}", expected: true, entry2.MatchesFilter(filterStrip.Filter));
					AssertEquals($"Entry of Declaration3, MatchesFilter, {comparisonOperator}", expected: true, entry3.MatchesFilter(filterStrip.Filter));
				});
			}

			operatorsToTest = new[] { SQLComparisonOperator.IsBlank, SQLComparisonOperator.IsNotBlank };
			foreach (var comparisonOperator in operatorsToTest)
			{
				var isBlank = comparisonOperator == SQLComparisonOperator.IsBlank;
				filter.SqlComparisonOperator = comparisonOperator;
				filter.Property = ZString.Empty;
				CombineAssertions(() =>
				{
					AssertEquals($"Entry of Declaration1, MatchesFilter, {comparisonOperator}", expected: !isBlank, entry1.MatchesFilter(filterStrip.Filter));
					AssertEquals($"Entry of Declaration2, MatchesFilter, {comparisonOperator}", expected: !isBlank, entry2.MatchesFilter(filterStrip.Filter));
					AssertEquals($"Entry of Declaration3, MatchesFilter, {comparisonOperator}", expected: isBlank, entry3.MatchesFilter(filterStrip.Filter));
				});
			}
		}

		public void TestRequestedProcedureFilter()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader1 = declaration1.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader1.AllEntryLines.AddNew();
			var invoiceLine1 = declaration1.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.SetFirst2CharactersOfJI_Procedure("01");

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
			var entryLine2 = entryHeader2.AllEntryLines.AddNew();
			var invoiceLine2 = declaration2.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.SetFirst2CharactersOfJI_Procedure("02");

			Factory.Save();

			var mockObj = new Mock<EntryHeaderFilterBusinessObject>();
			mockObj.Protected()
				.Setup<bool>("SupportRequestedProcedureCore")
				.Returns(true);
			mockObj.CallBase = true;

			var filterObj = mockObj.Object;
			var filter = (ModuleTextFilter)filterObj[DeclarationFilterConstants.RequestedProcedure];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "01";
			var filterQuery = filterObj.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("FilterCategory", FilterCategories.NumbersAndReferences, filter.Category);
				Assert("find first 2 characters of JI_Procedure", entryHeader1.MatchesFilter(filterQuery));
				Assert("find first 2 characters of JI_Procedure", !entryHeader2.MatchesFilter(filterQuery));
			});
		}

		public void TestPreviousProcedureCodeFilter()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader1 = declaration1.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader1.AllEntryLines.AddNew();
			var invoiceLine1 = declaration1.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.JI_Procedure = "0001001";

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
			var entryLine2 = entryHeader2.AllEntryLines.AddNew();
			var invoiceLine2 = declaration2.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_Procedure = "0002001";
			Factory.Save();

			var mockObj = new Mock<EntryHeaderFilterBusinessObject>();
			mockObj.Protected()
				.Setup<bool>("SupportRequestedProcedureCore")
				.Returns(true);
			mockObj.CallBase = true;

			var filterObj = mockObj.Object;
			var filter = (ModuleTextFilter)filterObj[DeclarationFilterConstants.PreviousProcedure];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "01";
			var filterQuery = filterObj.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("FilterCategory", FilterCategories.NumbersAndReferences, filter.Category);
				AssertEquals("01 matchs", true, entryHeader1.MatchesFilter(filterQuery));
				AssertEquals("02 should not match", false, entryHeader2.MatchesFilter(filterQuery));
			});
		}

		public void TestAdditionalProcedureFilter()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader1 = declaration1.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader1.AllEntryLines.AddNew();
			var invoiceLine1 = declaration1.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.JI_Procedure = "0001011";

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
			var entryLine2 = entryHeader2.AllEntryLines.AddNew();
			var invoiceLine2 = declaration2.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_Procedure = "0001022";
			Factory.Save();

			var mockObj = new Mock<EntryHeaderFilterBusinessObject>();
			mockObj.Protected()
				.Setup<bool>("SupportRequestedProcedureCore")
				.Returns(true);
			mockObj.CallBase = true;

			var filterObj = mockObj.Object;
			var filter = (ModuleTextFilter)filterObj[DeclarationFilterConstants.AdditionalProcedure];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "011";
			var filterQuery = filterObj.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("FilterCategory", FilterCategories.NumbersAndReferences, filter.Category);
				AssertEquals("011 matchs", true, entryHeader1.MatchesFilter(filterQuery));
				AssertEquals("022 should not match", false, entryHeader2.MatchesFilter(filterQuery));
			});
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new EntryHeaderFilterBusinessObject();
	}
}
