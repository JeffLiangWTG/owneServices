using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.EU.Business.Declaration.CusEntryHeader;
using CusEntryLine = Enterprise.Customs.EU.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.EU.Module.Testing
{
	[TestedType(typeof(EntryHeaderFilterBusinessObject))]
	sealed class EntryHeaderTotalCustomsQuantityFilterTest : FilterStripBusinessObjectTestCase
	{
		public void TestCustomsQuantityFilter()
		{
			entryHeaderCollectionOne.Load(filterStripBusinessObject.Filter);
			entryHeaderCollectionTwo.Load(filterStripBusinessObject.Filter);
			CombineAssertions(() =>
			{
				AssertEquals("[PRE-CONDITION] When filter is not applied, EntryHeaderCollectionOne Count", 1, entryHeaderCollectionOne.Count);
				AssertEquals("[PRE-CONDITION] When filter is not applied, EntryHeaderCollectionTwo Count", 1, entryHeaderCollectionTwo.Count);
			});

			var cusQuantityFilter =
				(ModuleNumberRangeFilter)filterStripBusinessObject[
					EntryHeaderFilterBusinessObject.EUFilterConstants.CustomsQuantity];
			AssertNotNull("Custom Quantity Filter", cusQuantityFilter);

			cusQuantityFilter.IsActive = true;
			cusQuantityFilter.Property1 = 60;
			cusQuantityFilter.Property2 = 60;
			entryHeaderCollectionOne.Load(filterStripBusinessObject.Filter);
			entryHeaderCollectionTwo.Load(filterStripBusinessObject.Filter);
			CombineAssertions(() =>
			{
				AssertEquals("EntryHeaderCollectionOne Count", 1, entryHeaderCollectionOne.Count);
				AssertEquals("Single Entry Header Found", entryHeaderOne.PK, entryHeaderCollectionOne[0].PK);
				AssertEquals("EntryHeaderCollectionTwo Count", 0, entryHeaderCollectionTwo.Count);
			});

			cusQuantityFilter.IsActive = true;
			cusQuantityFilter.Property1 = 100;
			cusQuantityFilter.Property2 = 100;
			entryHeaderCollectionOne.Load(filterStripBusinessObject.Filter);
			entryHeaderCollectionTwo.Load(filterStripBusinessObject.Filter);
			CombineAssertions(() =>
			{
				AssertEquals("EntryHeaderCollectionOne Count", 0, entryHeaderCollectionOne.Count);
				AssertEquals("EntryHeaderCollectionTwo Count", 1, entryHeaderCollectionTwo.Count);
				AssertEquals("Single Entry Header Found", entryHeaderTwo.PK, entryHeaderCollectionTwo[0].PK);
			});

			cusQuantityFilter.IsActive = true;
			cusQuantityFilter.Property1 = 60;
			cusQuantityFilter.Property2 = 100;
			entryHeaderCollectionOne.Load(filterStripBusinessObject.Filter);
			entryHeaderCollectionTwo.Load(filterStripBusinessObject.Filter);
			CombineAssertions(() =>
			{
				AssertEquals("EntryHeaderCollectionOne Count", 1, entryHeaderCollectionOne.Count);
				AssertEquals("EntryHeaderCollectionTwo Count", 1, entryHeaderCollectionTwo.Count);
			});
		}

		public void TestEntryHeaderCustomsQtyFilterGenerate()
		{
			var filter = new EntryHeaderTotalCustomsQtyFilterGenerator().Generate("My Description",
				FilterCategories.Other,
				(NoResString)"TestString");

			AssertNotNull("CustomsQty Filter", filter);
			AssertEquals("CustomsQty Filter type", typeof(ModuleNumberRangeFilter), filter.GetType());
			AssertEquals("Decimal Positions", JobComInvoiceLineSchema.JI_CustomsQuantity.Scale, ((ModuleNumberRangeFilter)filter).Decimals);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declarationOne = Factory.New<JobDeclaration>();
			var jobComInvoice = declarationOne.Invoices.AddNew();
			jobComInvoiceLineOne = jobComInvoice.JobComInvoiceLines.AddNew();
			jobComInvoiceLineTwo = jobComInvoice.JobComInvoiceLines.AddNew();
			entryHeaderOne = declarationOne.CustomsEntryHeaders.AddNew();
			lineOne = entryHeaderOne.AllEntryLines.AddNew();
			lineTwo = entryHeaderOne.AllEntryLines.AddNew();
			jobComInvoiceLineOne.JI_CL = lineOne.PK;
			jobComInvoiceLineOne.JI_CustomsQuantity = 25.00m;
			jobComInvoiceLineTwo.JI_CL = lineTwo.PK;
			jobComInvoiceLineTwo.JI_CustomsQuantity = 35.00m;
			entryHeaderCollectionOne =
				new Customs.Business.CusEntryHeaderCollection<CusEntryHeader>(declarationOne, Factory);

			declarationTwo = Factory.New<JobDeclaration>();
			jobComInvoiceLineThree = declarationOne.Invoices.AddNew().JobComInvoiceLines.AddNew();
			jobComInvoiceLineFour = declarationOne.Invoices.AddNew().JobComInvoiceLines.AddNew();
			entryHeaderTwo = declarationTwo.CustomsEntryHeaders.AddNew();
			lineThree = entryHeaderTwo.AllEntryLines.AddNew();
			jobComInvoiceLineThree.JI_CL = lineThree.PK;
			jobComInvoiceLineThree.JI_CustomsQuantity = 75.00m;
			jobComInvoiceLineFour.JI_CL = lineThree.PK;
			jobComInvoiceLineFour.JI_CustomsQuantity = 25.00m;
			entryHeaderCollectionTwo =
				new Customs.Business.CusEntryHeaderCollection<CusEntryHeader>(declarationTwo, Factory);

			Factory.Save();

			filterStripBusinessObject = GetNewFilterStripBusinessObject();
		}

		JobDeclaration declarationOne, declarationTwo;
		CusEntryHeader entryHeaderOne, entryHeaderTwo;
		CusEntryLine lineOne, lineTwo, lineThree;
		BaseJobComInvoiceLine jobComInvoiceLineOne, jobComInvoiceLineTwo, jobComInvoiceLineThree, jobComInvoiceLineFour;
		FilterStripBusinessObject filterStripBusinessObject;
		Customs.Business.CusEntryHeaderCollection<CusEntryHeader> entryHeaderCollectionOne, entryHeaderCollectionTwo;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
			=> new EntryHeaderFilterBusinessObject();
	}
}
