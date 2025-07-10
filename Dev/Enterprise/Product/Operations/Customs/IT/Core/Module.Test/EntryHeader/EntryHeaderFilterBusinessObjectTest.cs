using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Module.Testing;

[TestedType(typeof(EntryHeaderFilterBusinessObject))]
sealed class EntryHeaderFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
{
	public void TestControlChannelFilter()
	{
		entryHeader1.CustomsChannel = "CA";
		entryHeader2.CustomsChannel = "";
		Factory.Save();

		entryHeaderCollection.Load(filterStripBusinessObject.Filter);
		AssertEquals("[PRE-CONDITION] When filter is not applied, EntryHeaderCollection Count", 2, entryHeaderCollection.Count);

		var controlChannelFilter = (ModuleTextFilter)filterStripBusinessObject["Control Channel"];
		controlChannelFilter.Property = "CA";
		controlChannelFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith;
		controlChannelFilter.IsActive = true;

		entryHeaderCollection.Load(filterStripBusinessObject.Filter);
		CombineAssertions("[POST-CONDITION] When 'CA' filter is applied", () =>
		{
			AssertEquals("EntryHeaderCollection Count", 1, entryHeaderCollection.Count);
			AssertEquals("Single EntryHeader found PK", entryHeader1.PK, entryHeaderCollection[0].PK);
		});

		controlChannelFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank;
		entryHeaderCollection.Load(filterStripBusinessObject.Filter);
		CombineAssertions("[POST-CONDITION] When 'IsBlank' filter is applied", () =>
		{
			AssertEquals("EntryHeaderCollection Count", 1, entryHeaderCollection.Count);
			AssertEquals("Single EntryHeader found PK", entryHeader2.PK, entryHeaderCollection[0].PK);
		});
	}

	public void TestElectronicDocumentsUploadRequiredFilter()
	{
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		entryHeader1.CH_CEI_Instruction = entryInstruction1.PK;
		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		entryHeader2.CH_CEI_Instruction = entryInstruction2.PK;
		entryInstruction1.ElectronicDocuments = true;
		entryInstruction2.ElectronicDocuments = false;
		Factory.Save();

		entryHeaderCollection.Load(filterStripBusinessObject.Filter);
		AssertEquals("[PRE-CONDITION] When filter is not applied, EntryHeaderCollection Count", 2, entryHeaderCollection.Count);

		var electronicDocumentsUploadRequiredFilter = (ModuleFlagsFilter)filterStripBusinessObject["Electronic Documents Upload Required"];
		electronicDocumentsUploadRequiredFilter.Property0 = true;
		electronicDocumentsUploadRequiredFilter.IsActive = true;

		entryHeaderCollection.Load(filterStripBusinessObject.Filter);
		CombineAssertions("[POST-CONDITION] When 'true' filter is applied", () =>
		{
			AssertEquals("EntryHeaderCollection Count", 1, entryHeaderCollection.Count);
			AssertEquals("Single EntryHeader found PK", entryHeader1.PK, entryHeaderCollection[0].PK);
		});

		electronicDocumentsUploadRequiredFilter.Property0 = false;
		entryHeaderCollection.Load(filterStripBusinessObject.Filter);
		CombineAssertions("[POST-CONDITION] When 'false' filter is applied", () =>
		{
			AssertEquals("EntryHeaderCollection Count", 1, entryHeaderCollection.Count);
			AssertEquals("Single EntryHeader found PK", entryHeader2.PK, entryHeaderCollection[0].PK);
		});
	}

	public void TestEntryInvoiceCurrencyFilter()
	{
		entryHeader1.InvoiceAmountCurrency = "EUR";
		entryHeader2.InvoiceAmountCurrency = "";
		Factory.Save();

		var filterStripBusinessObject = GetNewFilterStripBusinessObject();
		var entryHeaderCollection = new CusEntryHeaderCollection(declaration, Factory);
		entryHeaderCollection.Load(filterStripBusinessObject.Filter);
		AssertEquals("[PRE-CONDITION] When filter is not applied, EntryHeaderCollection Count", 2, entryHeaderCollection.Count);

		var entryInvoiceCurrencyFilter = (ModuleTextFilter)filterStripBusinessObject["Entry Invoice Currency"];
		entryInvoiceCurrencyFilter.Property = "EUR";
		entryInvoiceCurrencyFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith;
		entryInvoiceCurrencyFilter.IsActive = true;
		entryHeaderCollection.Load(filterStripBusinessObject.Filter);
		CombineAssertions("[POST-CONDITION] When 'EUR' filter is applied", () =>
		{
			AssertEquals("EntryHeaderCollection Count", 1, entryHeaderCollection.Count);
			AssertEquals("Single EntryHeader found PK", entryHeader1.PK, entryHeaderCollection[0].PK);
		});

		entryInvoiceCurrencyFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank;
		entryHeaderCollection.Load(filterStripBusinessObject.Filter);
		CombineAssertions("[POST-CONDITION] When 'IsBlank' filter is applied", () =>
		{
			AssertEquals("EntryHeaderCollection Count", 1, entryHeaderCollection.Count);
			AssertEquals("Single EntryHeader found PK", entryHeader2.PK, entryHeaderCollection[0].PK);
		});
	}

	public void TestEntryInvoiceAmountFilter()
	{
		entryHeader1.InvoiceAmount = 111m;
		entryHeader2.InvoiceAmount = 999m;
		Factory.Save();

		entryHeaderCollection.Load(filterStripBusinessObject.Filter);
		AssertEquals("[PRE-CONDITION] When filter is not applied, EntryHeaderCollection Count", 2, entryHeaderCollection.Count);

		var entryInvoiceAmountFilter = (ModuleNumberRangeFilter)filterStripBusinessObject["Entry Invoice Amount"];
		entryInvoiceAmountFilter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.EqualTo;
		entryInvoiceAmountFilter.Property1 = 111m;
		entryInvoiceAmountFilter.IsActive = true;
		entryHeaderCollection.Load(filterStripBusinessObject.Filter);
		CombineAssertions("[POST-CONDITION] When EqualTo '111' filter is applied", () =>
		{
			AssertEquals("EntryHeaderCollection Count", 1, entryHeaderCollection.Count);
			AssertEquals("Single EntryHeader found PK", entryHeader1.PK, entryHeaderCollection[0].PK);
		});

		entryInvoiceAmountFilter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.GreaterThanOrEqualTo;
		entryInvoiceAmountFilter.Property1 = 800;
		entryHeaderCollection.Load(filterStripBusinessObject.Filter);
		CombineAssertions("[POST-CONDITION] When GreaterThan '800' filter is applied", () =>
		{
			AssertEquals("EntryHeaderCollection Count", 1, entryHeaderCollection.Count);
			AssertEquals("Single EntryHeader found PK", entryHeader2.PK, entryHeaderCollection[0].PK);
		});
	}

	public void TestLookups()
	{
		var entryHeaderFilterLookups = (EntryHeaderFilterBusinessObject)GetNewFilterStripBusinessObject();
		AssertType<EntryHeaderFilterLookups>("Lookups Type", entryHeaderFilterLookups.Lookups);
	}

	protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new EntryHeaderFilterBusinessObject();

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
		entryHeader2 = declaration.CustomsEntryHeaders.AddNew();

		entryHeaderCollection = new CusEntryHeaderCollection(declaration, Factory);
		filterStripBusinessObject = GetNewFilterStripBusinessObject();
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader1;
	CusEntryHeader entryHeader2;
	CusEntryHeaderCollection entryHeaderCollection;
	FilterStripBusinessObject filterStripBusinessObject;
}
