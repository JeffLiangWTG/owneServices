using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(FSimplifiedDeclarationFilterBusinessObject))]
	sealed class FSimplifiedDeclarationFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFiltersExist()
		{
			var filter = new FSimplifiedDeclarationFilterBusinessObject();
			AssertNotNull(filter[FSimplifiedDeclarationFilterBusinessObject.Schema.EntryDate]);
			AssertNotNull(filter[FSimplifiedDeclarationFilterBusinessObject.Schema.OriginalEntryNumber]);
			AssertNotNull(filter[FSimplifiedDeclarationFilterBusinessObject.Schema.DeclarationReference]);
			AssertNotNull(filter[FSimplifiedDeclarationFilterBusinessObject.Schema.OwnerRef]);
			AssertNotNull(filter[FSimplifiedDeclarationFilterBusinessObject.Schema.LineNumber]);
			AssertNotNull(filter[FSimplifiedDeclarationFilterBusinessObject.Schema.CustomsStatus]);
		}

		public void TestDeclarationReferenceFilter()
		{
			jobDeclaration.JE_DeclarationReference = "B00222768";
			var entryHeader = FSimplifiedDeclarationHelper.CreateEntryHeader(jobDeclaration);
			FSimplifiedDeclarationHelper.CreateCusReconEntry(declaration, entryHeader);

			Factory.Save();

			var filterObject = new FSimplifiedDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObject[FSimplifiedDeclarationFilterBusinessObject.Schema.DeclarationReference];
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "B00222";

			var collection = new CusReconEntryCollection(declaration);
			collection.AdditionalFilter = filterObject.Filter;

			filter.Property = "B00222768";

			var collection2 = new CusReconEntryCollection(declaration);
			collection2.AdditionalFilter = filterObject.Filter;

			CombineAssertions(() =>
			{
				AssertEquals(0, collection.Count);
				AssertEquals(1, collection2.Count);
			});
		}

		public void TestOwnerRefFilter()
		{
			jobDeclaration.JE_OwnerRef = "AZ_FV_06";
			var entryHeader = FSimplifiedDeclarationHelper.CreateEntryHeader(jobDeclaration);
			FSimplifiedDeclarationHelper.CreateCusReconEntry(declaration, entryHeader);

			Factory.Save();

			var filterObject = new FSimplifiedDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObject[FSimplifiedDeclarationFilterBusinessObject.Schema.OwnerRef];
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			filter.Property = "FV_06";

			var collection = new CusReconEntryCollection(declaration);
			collection.AdditionalFilter = filterObject.Filter;

			AssertEquals(1, collection.Count);
		}

		public void TestEntryDateFilter()
		{
			var entryHeader = FSimplifiedDeclarationHelper.CreateEntryHeader(jobDeclaration);
			var entry1 = FSimplifiedDeclarationHelper.CreateCusReconEntry(declaration, entryHeader);
			entry1.CRE_EntryDate = ZDate.Today.AddDays(-1);
			var entry2 = FSimplifiedDeclarationHelper.CreateCusReconEntry(declaration, entryHeader);
			entry2.CRE_EntryDate = ZDate.Today;
			var entry3 = FSimplifiedDeclarationHelper.CreateCusReconEntry(declaration, entryHeader);
			entry3.CRE_EntryDate = ZDate.Today.AddDays(5);
			Factory.Save();

			var filterObject = new FSimplifiedDeclarationFilterBusinessObject();
			var filter = (ModuleDateFilter)filterObject[FSimplifiedDeclarationFilterBusinessObject.Schema.EntryDate];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.IsActive = true;
			filter.Property1 = ZDateTime.Today.AddDays(-2);
			filter.Property2 = ZDateTime.Today.AddDays(2);

			var collection = new CusReconEntryCollection(declaration);
			collection.AdditionalFilter = filterObject.Filter;

			AssertEquals(2, collection.Count);
		}

		public void TestOriginalEntryNumberFilter()
		{
			var entryHeader = FSimplifiedDeclarationHelper.CreateEntryHeader(jobDeclaration);
			var entry1 = FSimplifiedDeclarationHelper.CreateCusReconEntry(declaration, entryHeader);
			entry1.CRE_OriginalEntryNumber = "ATE400000060120225864";
			var entry2 = FSimplifiedDeclarationHelper.CreateCusReconEntry(declaration, entryHeader);
			entry2.CRE_OriginalEntryNumber = "ATE400000060120225865";
			var entry3 = FSimplifiedDeclarationHelper.CreateCusReconEntry(declaration, entryHeader);
			entry3.CRE_OriginalEntryNumber = "ATE400000060120225866";
			Factory.Save();

			var filterObject = new FSimplifiedDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObject[FSimplifiedDeclarationFilterBusinessObject.Schema.OriginalEntryNumber];
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "ATE";

			var collection = new CusReconEntryCollection(declaration);
			collection.AdditionalFilter = filterObject.Filter;

			AssertEquals(3, collection.Count);
		}

		public void TestLineNumberFilter()
		{
			jobDeclaration.JE_DeclarationReference = "B00222768";
			var entryHeader1 = FSimplifiedDeclarationHelper.CreateEntryHeader(jobDeclaration);
			var entry1 = FSimplifiedDeclarationHelper.CreateCusReconEntry(declaration, entryHeader1);
			FSimplifiedDeclarationHelper.CreateCusReconEntryLine(entry1,1, entryHeader1.AllEntryLines.First().CL_LineNumber);
			FSimplifiedDeclarationHelper.CreateCusReconEntryLine(entry1, 2, entryHeader1.AllEntryLines.First().CL_LineNumber);

			jobDeclaration.JE_DeclarationReference = "B00222769";
			var entryHeader2 = FSimplifiedDeclarationHelper.CreateEntryHeader(jobDeclaration);
			var entry2 = FSimplifiedDeclarationHelper.CreateCusReconEntry(declaration, entryHeader2);
			FSimplifiedDeclarationHelper.CreateCusReconEntryLine(entry2, 3, entryHeader2.AllEntryLines.First().CL_LineNumber);
			FSimplifiedDeclarationHelper.CreateCusReconEntryLine(entry2, 4, entryHeader2.AllEntryLines.First().CL_LineNumber);
			Factory.Save();

			var filterObject = new FSimplifiedDeclarationFilterBusinessObject();
			var filter = (ModuleNumberFilter)filterObject[FSimplifiedDeclarationFilterBusinessObject.Schema.LineNumber];
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "1";

			var entryCollection = new CusReconEntryCollection(declaration);
			entryCollection.AdditionalFilter = filterObject.Filter;

			var entryLineCollection1 = new CusReconEntryLineCollection(entry1);
			entryLineCollection1.AdditionalFilter = filterObject.LineOnlyQuery;

			var entryLineCollection2 = new CusReconEntryLineCollection(entry2);
			entryLineCollection2.AdditionalFilter = filterObject.LineOnlyQuery;

			CombineAssertions(() =>
			{
				AssertEquals(1, entryCollection.Count);
				AssertEquals(1, entryLineCollection1.Count);
				AssertEquals(0, entryLineCollection2.Count);
			});
		}

		public void TestCustomsStatusFilter()
		{
			var entryHeader = FSimplifiedDeclarationHelper.CreateEntryHeader(jobDeclaration);
			var entry = FSimplifiedDeclarationHelper.CreateCusReconEntry(declaration, entryHeader);
			var entryLine1 = FSimplifiedDeclarationHelper.CreateCusReconEntryLine(entry, 1, entryHeader.AllEntryLines.First().CL_LineNumber);
			entryLine1.CRL_CustomsStatus = UniversalReferenceConstants.EntryStatus.TX7;
			var entryLine2 = FSimplifiedDeclarationHelper.CreateCusReconEntryLine(entry, 2, entryHeader.AllEntryLines.First().CL_LineNumber);
			entryLine2.CRL_CustomsStatus = UniversalReferenceConstants.EntryStatus.TX7;
			var entryLine3 = FSimplifiedDeclarationHelper.CreateCusReconEntryLine(entry, 3, entryHeader.AllEntryLines.First().CL_LineNumber);
			entryLine3.CRL_CustomsStatus = UniversalReferenceConstants.EntryStatus.TX7;
			var entryLine4 = FSimplifiedDeclarationHelper.CreateCusReconEntryLine(entry, 4, entryHeader.AllEntryLines.First().CL_LineNumber);
			entryLine4.CRL_CustomsStatus = UniversalReferenceConstants.EntryStatus.TX8;
			Factory.Save();

			var filterObject = new FSimplifiedDeclarationFilterBusinessObject();
			var filterStatus = (ModuleTextFilter)filterObject[FSimplifiedDeclarationFilterBusinessObject.Schema.CustomsStatus];
			filterStatus.IsActive = true;
			filterStatus.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filterStatus.Property = UniversalReferenceConstants.EntryStatus.TX7;

			var entryCollection = new CusReconEntryCollection(declaration);
			entryCollection.AdditionalFilter = filterObject.Filter;

			var lineCollection = new CusReconEntryLineCollection(entry);
			lineCollection.AdditionalFilter = filterObject.LineOnlyQuery;

			AssertEquals(3, lineCollection.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();

			jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration = Factory.NewWithValidTestData<CusReconDeclaration>();
			declaration.CRD_OA_DeclarantAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
		}

		public JobDeclaration jobDeclaration;
		public CusReconDeclaration declaration;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new FSimplifiedDeclarationFilterBusinessObject();
	}
}
