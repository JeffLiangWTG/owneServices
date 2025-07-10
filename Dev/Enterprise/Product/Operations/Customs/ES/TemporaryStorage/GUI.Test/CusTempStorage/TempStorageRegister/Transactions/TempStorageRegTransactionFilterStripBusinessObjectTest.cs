using CargoWise.Types;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing;

[TestedType(typeof(TempStorageRegTransactionFilterStripBusinessObject))]
sealed class TempStorageRegTransactionFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
{
	public void TestBondAmountFilter() => CombineAssertions(() =>
	{
		transaction1.SRT_BondAmount = -1m;
		transaction2.SRT_BondAmount = 0m;
		transaction3.SRT_BondAmount = 12m;
		transaction4.SRT_BondAmount = 0m;

		Factory.Save();

		var transactionsCollection = new CusTempStorageRegLineTransactionCollection(regLine1);
		var stripBO = (TempStorageRegTransactionFilterStripBusinessObject)GetNewFilterStripBusinessObject();
		AssertEquals("[PRECONDITION] Total Declarations", 2, transactionsCollection.Count);

		transactionsCollection.AdditionalFilter = stripBO.Filter;

		LoadDeclarationCollectionNumberFilter("BondAmount", 0, transactionsCollection, stripBO);

		AssertEquals("Total Transactions match the filter", 1, transactionsCollection.Count);
		AssertEquals("T1 is not in the filter cause its not 0", expected: false, transactionsCollection.Contains(transaction1));
		AssertEquals("T2 is in the filter cause its 0", expected: true, transactionsCollection.Contains(transaction2));
		AssertEquals("T3 is not in the filter cause its not 0", expected: false, transactionsCollection.Contains(transaction3));
		AssertEquals("T4 is not in the filter cause its 0 but in different header", expected: false, transactionsCollection.Contains(transaction4));

		LoadDeclarationCollectionNumberFilter("BondAmount", 0, transactionsCollection, stripBO, ModuleNumberRangeFilter.SearchTexts.LessThanOrEqualTo);

		AssertEquals("Total Transactions match the filter", expected: 2, transactionsCollection.Count);
		AssertEquals("T1 is in the filter cause its less than 0", expected: true, transactionsCollection.Contains(transaction1));
		AssertEquals("T2 is in the filter cause its 0", expected: true, transactionsCollection.Contains(transaction2));
		AssertEquals("T3 is not in the filter cause its not less or equal to 0", expected: false, transactionsCollection.Contains(transaction3));
		AssertEquals("T4 is not in the filter cause its 0 but in different header", expected: false, transactionsCollection.Contains(transaction4));

		LoadDeclarationCollectionNumberFilter("BondAmount", -2, transactionsCollection, stripBO);

		AssertEquals("Total Transactions match the filter", expected: 0, transactionsCollection.Count);
		AssertEquals("T1 is not in the filter cause its not -2", expected: false, transactionsCollection.Contains(transaction1));
		AssertEquals("T2 is not in the filter cause its not -2", expected: false, transactionsCollection.Contains(transaction2));
		AssertEquals("T3 is not in the filter cause its not -2", expected: false, transactionsCollection.Contains(transaction3));
		AssertEquals("T4 is not in the filter cause its not -2", expected: false, transactionsCollection.Contains(transaction4));
	});

	public void TestGrossWeightFilter() => CombineAssertions(() =>
	{
		transaction1.SRT_GrossWeight = -1m;
		transaction2.SRT_GrossWeight = 0m;
		transaction3.SRT_GrossWeight = 12m;
		transaction4.SRT_GrossWeight = 0m;

		Factory.Save();

		var transactionsCollection = new CusTempStorageRegLineTransactionCollection(regLine1);
		var stripBO = (TempStorageRegTransactionFilterStripBusinessObject)GetNewFilterStripBusinessObject();
		AssertEquals("[PRECONDITION] Total Declarations", 2, transactionsCollection.Count);

		transactionsCollection.AdditionalFilter = stripBO.Filter;

		LoadDeclarationCollectionNumberFilter("GrossWeight", 0, transactionsCollection, stripBO);

		AssertEquals("Total Transactions match the filter", 1, transactionsCollection.Count);
		AssertEquals("T1 is not in the filter cause its not 0", expected: false, transactionsCollection.Contains(transaction1));
		AssertEquals("T2 is in the filter cause its 0", expected: true, transactionsCollection.Contains(transaction2));
		AssertEquals("T3 is not in the filter cause its not 0", expected: false, transactionsCollection.Contains(transaction3));
		AssertEquals("T4 is not in the filter cause its 0 but in different header", expected: false, transactionsCollection.Contains(transaction4));

		LoadDeclarationCollectionNumberFilter("GrossWeight", 0, transactionsCollection, stripBO, ModuleNumberRangeFilter.SearchTexts.LessThanOrEqualTo);

		AssertEquals("Total Transactions match the filter", expected: 2, transactionsCollection.Count);
		AssertEquals("T1 is in the filter cause its less than 0", expected: true, transactionsCollection.Contains(transaction1));
		AssertEquals("T2 is in the filter cause its 0", expected: true, transactionsCollection.Contains(transaction2));
		AssertEquals("T3 is not in the filter cause its not less or equal to 0", expected: false, transactionsCollection.Contains(transaction3));
		AssertEquals("T4 is not in the filter cause its 0 but in different header", expected: false, transactionsCollection.Contains(transaction4));

		LoadDeclarationCollectionNumberFilter("GrossWeight", -2, transactionsCollection, stripBO);

		AssertEquals("Total Transactions match the filter", expected: 0, transactionsCollection.Count);
		AssertEquals("T1 is not in the filter cause its not -2", expected: false, transactionsCollection.Contains(transaction1));
		AssertEquals("T2 is not in the filter cause its not -2", expected: false, transactionsCollection.Contains(transaction2));
		AssertEquals("T3 is not in the filter cause its not -2", expected: false, transactionsCollection.Contains(transaction3));
		AssertEquals("T4 is not in the filter cause its not -2", expected: false, transactionsCollection.Contains(transaction4));
	});

	public void TestPackageQtyFilter() => CombineAssertions(() =>
	{
		transaction1.SRT_PackageQty = 1;
		transaction2.SRT_PackageQty = 0;
		transaction3.SRT_PackageQty = 12;
		transaction4.SRT_PackageQty = 0;

		Factory.Save();

		var transactionsCollection = new CusTempStorageRegLineTransactionCollection(regLine1);
		var stripBO = (TempStorageRegTransactionFilterStripBusinessObject)GetNewFilterStripBusinessObject();
		AssertEquals("[PRECONDITION] Total Declarations", 2, transactionsCollection.Count);

		transactionsCollection.AdditionalFilter = stripBO.Filter;

		LoadDeclarationCollectionNumberFilter("PackageQty", 0, transactionsCollection, stripBO);

		AssertEquals("Total Transactions match the filter", 1, transactionsCollection.Count);
		AssertEquals("T1 is not in the filter cause its not 0", expected: false, transactionsCollection.Contains(transaction1));
		AssertEquals("T2 is in the filter cause its 0", expected: true, transactionsCollection.Contains(transaction2));
		AssertEquals("T3 is not in the filter cause its not 0", expected: false, transactionsCollection.Contains(transaction3));
		AssertEquals("T4 is not in the filter cause its 0 but in different header", expected: false, transactionsCollection.Contains(transaction4));

		LoadDeclarationCollectionNumberFilter("PackageQty", 1, transactionsCollection, stripBO, ModuleNumberRangeFilter.SearchTexts.LessThanOrEqualTo);

		AssertEquals("Total Transactions match the filter", expected: 2, transactionsCollection.Count);
		AssertEquals("T1 is in the filter cause its 1", expected: true, transactionsCollection.Contains(transaction1));
		AssertEquals("T2 is in the filter cause its less than 1", expected: true, transactionsCollection.Contains(transaction2));
		AssertEquals("T3 is not in the filter cause its not less or equal to 0", expected: false, transactionsCollection.Contains(transaction3));
		AssertEquals("T4 is not in the filter cause its 0 but in different header", expected: false, transactionsCollection.Contains(transaction4));

		LoadDeclarationCollectionNumberFilter("PackageQty", -2, transactionsCollection, stripBO);

		AssertEquals("Total Transactions match the filter", expected: 0, transactionsCollection.Count);
		AssertEquals("T1 is not in the filter cause its not -2", expected: false, transactionsCollection.Contains(transaction1));
		AssertEquals("T2 is not in the filter cause its not -2", expected: false, transactionsCollection.Contains(transaction2));
		AssertEquals("T3 is not in the filter cause its not -2", expected: false, transactionsCollection.Contains(transaction3));
		AssertEquals("T4 is not in the filter cause its not -2", expected: false, transactionsCollection.Contains(transaction4));
	});

	public void TestStatusFilter() => CombineAssertions(() =>
	{
		transaction1.SRT_TransactionStatus = "DEL";
		transaction2.SRT_TransactionStatus = "PND";
		transaction3.SRT_TransactionStatus = "PND";
		transaction4.SRT_TransactionStatus = "DEL";

		Factory.Save();

		var transactionsCollection = new CusTempStorageRegLineTransactionCollection(regLine1);
		var stripBO = (TempStorageRegTransactionFilterStripBusinessObject)GetNewFilterStripBusinessObject();

		var filter = (ModuleTextFilter)stripBO["Status"];
		AssertEquals("Default Comparison Operator", "not equal", filter.ComparisonOperator);
		AssertEquals("Default Comparison Operator", "DEL", filter.DefaultProperty);
		AssertEquals("[PRECONDITION] Total Declarations", 2, transactionsCollection.Count);

		transactionsCollection.AdditionalFilter = stripBO.Filter;

		LoadDeclarationCollectionTextFilter("Status", "DEL", transactionsCollection, stripBO);

		AssertEquals("Total Transactions match the filter", 1, transactionsCollection.Count);
		AssertEquals("T1 is in the filter cause its Status is DEL and it is in the collection", expected: true, transactionsCollection.Contains(transaction1));
		AssertEquals("T2 is not in the filter cause its Status is not DEL", expected: false, transactionsCollection.Contains(transaction2));
		AssertEquals("T3 is not in the filter cause its Status is not DEL", expected: false, transactionsCollection.Contains(transaction3));
		AssertEquals("T4 is not in the filter cause its Status is DEL but in different header", expected: false, transactionsCollection.Contains(transaction4));

		LoadDeclarationCollectionTextFilter("Status", "PND", transactionsCollection, stripBO);

		AssertEquals("Total Transactions match the filter", expected: 1, transactionsCollection.Count);
		AssertEquals("T1 is not in the filter cause its Status is not PND", expected: false, transactionsCollection.Contains(transaction1));
		AssertEquals("T2 is in the filter cause its Status is PND and it is in the collection", expected: true, transactionsCollection.Contains(transaction2));
		AssertEquals("T3 is not in the filter cause its Status is PND but in different line", expected: false, transactionsCollection.Contains(transaction3));
		AssertEquals("T4 is not in the filter cause its Status is not PND", expected: false, transactionsCollection.Contains(transaction4));

		LoadDeclarationCollectionTextFilter("Status", "OBL", transactionsCollection, stripBO);

		AssertEquals("Total Transactions match the filter", expected: 0, transactionsCollection.Count);
		AssertEquals("T1 is not in the filter cause its Status is not OBL", expected: false, transactionsCollection.Contains(transaction1));
		AssertEquals("T2 is not in the filter cause its Status is not OBL", expected: false, transactionsCollection.Contains(transaction2));
		AssertEquals("T3 is not in the filter cause its Status is not OBL", expected: false, transactionsCollection.Contains(transaction3));
		AssertEquals("T4 is not in the filter cause its Status is not OBL", expected: false, transactionsCollection.Contains(transaction4));
	});

	public void TestReferenceTypeFilter() => CombineAssertions(() =>
	{
		transaction1.SRT_ReferenceType = "TS1";
		transaction2.SRT_ReferenceType = "TS2";
		transaction3.SRT_ReferenceType = "TS2";
		transaction4.SRT_ReferenceType = "TS3";

		Factory.Save();

		var transactionsCollection = new CusTempStorageRegLineTransactionCollection(regLine1);
		var stripBO = (TempStorageRegTransactionFilterStripBusinessObject)GetNewFilterStripBusinessObject();
		AssertEquals("[PRECONDITION] Total Declarations", 2, transactionsCollection.Count);

		transactionsCollection.AdditionalFilter = stripBO.Filter;

		LoadDeclarationCollectionTextFilter("ReferenceType", "TS1", transactionsCollection, stripBO);

		AssertEquals("Total Transactions match the filter", 1, transactionsCollection.Count);
		AssertEquals("T1 is in the filter cause its ReferenceType is TS1 and it is in the collection", expected: true, transactionsCollection.Contains(transaction1));
		AssertEquals("T2 is not in the filter cause its ReferenceType is not TS1", expected: false, transactionsCollection.Contains(transaction2));
		AssertEquals("T3 is not in the filter cause its ReferenceType is not TS1", expected: false, transactionsCollection.Contains(transaction3));
		AssertEquals("T4 is not in the filter cause its ReferenceType is not TS1", expected: false, transactionsCollection.Contains(transaction4));

		LoadDeclarationCollectionTextFilter("ReferenceType", "TS2", transactionsCollection, stripBO);

		AssertEquals("Total Transactions match the filter", expected: 1, transactionsCollection.Count);
		AssertEquals("T1 is not in the filter cause its ReferenceType is not TS2", expected: false, transactionsCollection.Contains(transaction1));
		AssertEquals("T2 is in the filter cause its ReferenceType is TS2 and it is in the collection", expected: true, transactionsCollection.Contains(transaction2));
		AssertEquals("T3 is not in the filter cause its ReferenceType is TS2 but in different line", expected: false, transactionsCollection.Contains(transaction3));
		AssertEquals("T4 is not in the filter cause its ReferenceType is not TS2", expected: false, transactionsCollection.Contains(transaction4));

		LoadDeclarationCollectionTextFilter("ReferenceType", "TS3", transactionsCollection, stripBO);

		AssertEquals("Total Transactions match the filter", expected: 0, transactionsCollection.Count);
		AssertEquals("T1 is not in the filter cause its ReferenceType is not TS3", expected: false, transactionsCollection.Contains(transaction1));
		AssertEquals("T2 is not in the filter cause its ReferenceType is not TS3", expected: false, transactionsCollection.Contains(transaction2));
		AssertEquals("T3 is not in the filter cause its ReferenceType is not TS3", expected: false, transactionsCollection.Contains(transaction3));
		AssertEquals("T4 is not in the filter cause its ReferenceType is TS3 but in different header", expected: false, transactionsCollection.Contains(transaction4));
	});

	public void TestTransactionTypeFilter() => CombineAssertions(() =>
	{
		transaction1.SRT_TransactionType = "ADJ";
		transaction2.SRT_TransactionType = "OBL";
		transaction3.SRT_TransactionType = "OBL";
		transaction4.SRT_TransactionType = "TRN";

		Factory.Save();

		var transactionsCollection = new CusTempStorageRegLineTransactionCollection(regLine1);
		var stripBO = (TempStorageRegTransactionFilterStripBusinessObject)GetNewFilterStripBusinessObject();
		AssertEquals("[PRECONDITION] Total Declarations", 2, transactionsCollection.Count);

		transactionsCollection.AdditionalFilter = stripBO.Filter;

		LoadDeclarationCollectionTextFilter("TransactionType", "ADJ", transactionsCollection, stripBO);

		AssertEquals("Total Transactions match the filter", 1, transactionsCollection.Count);
		AssertEquals("T1 is in the filter cause its TransactionType is ADJ and it is in the collection", expected: true, transactionsCollection.Contains(transaction1));
		AssertEquals("T2 is not in the filter cause its TransactionType is not ADJ", expected: false, transactionsCollection.Contains(transaction2));
		AssertEquals("T3 is not in the filter cause its TransactionType is not ADJ", expected: false, transactionsCollection.Contains(transaction3));
		AssertEquals("T4 is not in the filter cause its TransactionType is not ADJ", expected: false, transactionsCollection.Contains(transaction4));

		LoadDeclarationCollectionTextFilter("TransactionType", "OBL", transactionsCollection, stripBO);

		AssertEquals("Total Transactions match the filter", expected: 1, transactionsCollection.Count);
		AssertEquals("T1 is not in the filter cause its TransactionType is not OBL", expected: false, transactionsCollection.Contains(transaction1));
		AssertEquals("T2 is in the filter cause its TransactionType is OBL and it is in the collection", expected: true, transactionsCollection.Contains(transaction2));
		AssertEquals("T3 is not in the filter cause its TransactionType is OBL but in different line", expected: false, transactionsCollection.Contains(transaction3));
		AssertEquals("T4 is not in the filter cause its TransactionType is not OBL", expected: false, transactionsCollection.Contains(transaction4));

		LoadDeclarationCollectionTextFilter("TransactionType", "TRN", transactionsCollection, stripBO);

		AssertEquals("Total Transactions match the filter", expected: 0, transactionsCollection.Count);
		AssertEquals("T1 is not in the filter cause its TransactionType is not TRN", expected: false, transactionsCollection.Contains(transaction1));
		AssertEquals("T2 is not in the filter cause its TransactionType is not TRN", expected: false, transactionsCollection.Contains(transaction2));
		AssertEquals("T3 is not in the filter cause its TransactionType is not TRN", expected: false, transactionsCollection.Contains(transaction3));
		AssertEquals("T4 is not in the filter cause its TransactionType is not TRN", expected: false, transactionsCollection.Contains(transaction4));
	});

	public void TestInternalReferenceTypeFilter() => CombineAssertions(() =>
	{
		transaction1.SRT_InternalReferenceType = "ABD";
		transaction2.SRT_InternalReferenceType = "H7";
		transaction3.SRT_InternalReferenceType = "H7";
		transaction4.SRT_InternalReferenceType = "DUA";

		Factory.Save();

		var transactionsCollection = new CusTempStorageRegLineTransactionCollection(regLine1);
		var stripBO = (TempStorageRegTransactionFilterStripBusinessObject)GetNewFilterStripBusinessObject();
		AssertEquals("[PRECONDITION] Total Declarations", 2, transactionsCollection.Count);

		transactionsCollection.AdditionalFilter = stripBO.Filter;

		LoadDeclarationCollectionTextFilter("InternalReferenceType", "ABD", transactionsCollection, stripBO);

		AssertEquals("Total Transactions match the filter", 1, transactionsCollection.Count);
		AssertEquals("T1 is in the filter cause its InternalReferenceType is ABD and it is in the collection", expected: true, transactionsCollection.Contains(transaction1));
		AssertEquals("T2 is not in the filter cause its InternalReferenceType is not ABD", expected: false, transactionsCollection.Contains(transaction2));
		AssertEquals("T3 is not in the filter cause its InternalReferenceType is not ABD", expected: false, transactionsCollection.Contains(transaction3));
		AssertEquals("T4 is not in the filter cause its InternalReferenceType is not ABD", expected: false, transactionsCollection.Contains(transaction4));

		LoadDeclarationCollectionTextFilter("InternalReferenceType", "H7", transactionsCollection, stripBO);

		AssertEquals("Total Transactions match the filter", expected: 1, transactionsCollection.Count);
		AssertEquals("T1 is not in the filter cause its InternalReferenceType is not H7", expected: false, transactionsCollection.Contains(transaction1));
		AssertEquals("T2 is in the filter cause its InternalReferenceType is H7 and it is in the collection", expected: true, transactionsCollection.Contains(transaction2));
		AssertEquals("T3 is not in the filter cause its InternalReferenceType is H7 but in different line", expected: false, transactionsCollection.Contains(transaction3));
		AssertEquals("T4 is not in the filter cause its InternalReferenceType is not H7", expected: false, transactionsCollection.Contains(transaction4));

		LoadDeclarationCollectionTextFilter("InternalReferenceType", "DUA", transactionsCollection, stripBO);

		AssertEquals("Total Transactions match the filter", expected: 0, transactionsCollection.Count);
		AssertEquals("T1 is not in the filter cause its InternalReferenceType is not DUA", expected: false, transactionsCollection.Contains(transaction1));
		AssertEquals("T2 is not in the filter cause its InternalReferenceType is not DUA", expected: false, transactionsCollection.Contains(transaction2));
		AssertEquals("T3 is not in the filter cause its InternalReferenceType is not DUA", expected: false, transactionsCollection.Contains(transaction3));
		AssertEquals("T4 is not in the filter cause its InternalReferenceType is DUA but in different header", expected: false, transactionsCollection.Contains(transaction4));
	});

	public void TestEntryDateFilter() => CombineAssertions(() =>
	{
		transaction1.SRT_PhysicalInOutDate = new ZDateTimeOffset(2022, 1, 1, 10, 0, 0);
		transaction2.SRT_PhysicalInOutDate = new ZDateTimeOffset(2022, 2, 26, 10, 0, 0);
		transaction2.SRT_TransactionType = "OBL";
		transaction3.SRT_PhysicalInOutDate = new ZDateTimeOffset(2022, 3, 25, 10, 0, 0);
		transaction3.SRT_TransactionType = "OBL";
		transaction4.SRT_PhysicalInOutDate = new ZDateTimeOffset(2022, 2, 26, 10, 0, 0);
		transaction4.SRT_TransactionType = "OBL";

		Factory.Save();

		var transactionsCollection = new CusTempStorageRegLineTransactionCollection(regLine1);
		var stripBO = (TempStorageRegTransactionFilterStripBusinessObject)GetNewFilterStripBusinessObject();
		AssertEquals("[PRECONDITION] Total Declarations 2", 2, transactionsCollection.Count);

		transactionsCollection.AdditionalFilter = stripBO.Filter;

		LoadDeclarationCollectionDateFilter("EntryDate", new ZDateTimeOffset(2022, 1, 1, 10, 0, 0), transactionsCollection, stripBO);

		AssertEquals("Total Transactions match the filter", 0, transactionsCollection.Count);
		AssertEquals("T1 is not in the filter cause date is correct but its not OBL", expected: false, transactionsCollection.Contains(transaction1));
		AssertEquals("T2 is not in the filter cause date is not correct", expected: false, transactionsCollection.Contains(transaction2));
		AssertEquals("T3 is not in the filter cause date is not correct", expected: false, transactionsCollection.Contains(transaction3));
		AssertEquals("T4 is not in the filter cause date is not correct", expected: false, transactionsCollection.Contains(transaction4));

		LoadDeclarationCollectionDateFilter("EntryDate", new ZDateTimeOffset(2022, 2, 26, 10, 0, 0), transactionsCollection, stripBO);

		AssertEquals("Total Transactions match the filter", expected: 1, transactionsCollection.Count);
		AssertEquals("T1 is not in the filter cause date is not correct", expected: false, transactionsCollection.Contains(transaction1));
		AssertEquals("T2 is in the filter cause date is correct and its OBL", expected: true, transactionsCollection.Contains(transaction2));
		AssertEquals("T3 is not in the filter cause date is not correct", expected: false, transactionsCollection.Contains(transaction3));
		AssertEquals("T4 is not in the filter cause date is correct and its OBL but in different header", expected: false, transactionsCollection.Contains(transaction4));

		LoadDeclarationCollectionDateFilter("EntryDate", new ZDateTimeOffset(2022, 3, 25, 10, 0, 0), transactionsCollection, stripBO);

		AssertEquals("Total Transactions match the filter", expected: 0, transactionsCollection.Count);
		AssertEquals("T1 is not in the filter cause date is not correct", expected: false, transactionsCollection.Contains(transaction1));
		AssertEquals("T2 is not in the filter cause date is not correct", expected: false, transactionsCollection.Contains(transaction2));
		AssertEquals("T3 is not in the filter cause date is correct and its OBL but in different line", expected: false, transactionsCollection.Contains(transaction3));
		AssertEquals("T4 is not in the filter cause date is not correct", expected: false, transactionsCollection.Contains(transaction4));
	});

	public void TestExitDateFilter() => CombineAssertions(() =>
	{
		transaction1.SRT_PhysicalInOutDate = new ZDateTimeOffset(2022, 1, 1, 10, 0, 0);
		transaction1.SRT_TransactionType = "ADJ";
		transaction2.SRT_PhysicalInOutDate = new ZDateTimeOffset(2022, 2, 26, 10, 0, 0);
		transaction2.SRT_TransactionType = "TRN";
		transaction3.SRT_PhysicalInOutDate = new ZDateTimeOffset(2022, 3, 25, 10, 0, 0);
		transaction3.SRT_TransactionType = "ADJ";
		transaction4.SRT_PhysicalInOutDate = new ZDateTimeOffset(2022, 2, 26, 10, 0, 0);
		transaction4.SRT_TransactionType = "TRN";

		Factory.Save();

		var transactionsCollection = new CusTempStorageRegLineTransactionCollection(regLine1);
		var stripBO = (TempStorageRegTransactionFilterStripBusinessObject)GetNewFilterStripBusinessObject();
		AssertEquals("[PRECONDITION] Total Declarations 2", 2, transactionsCollection.Count);

		transactionsCollection.AdditionalFilter = stripBO.Filter;

		LoadDeclarationCollectionDateFilter("ExitDate", new ZDateTimeOffset(2022, 1, 1, 10, 0, 0), transactionsCollection, stripBO);

		AssertEquals("Total Transactions match the filter", 1, transactionsCollection.Count);
		AssertEquals("T1 is in the filter cause date is correct and its ADJ", expected: true, transactionsCollection.Contains(transaction1));
		AssertEquals("T2 is not in the filter cause date is not correct", expected: false, transactionsCollection.Contains(transaction2));
		AssertEquals("T3 is not in the filter cause date is not correct", expected: false, transactionsCollection.Contains(transaction3));
		AssertEquals("T4 is not in the filter cause date is not correct", expected: false, transactionsCollection.Contains(transaction4));

		LoadDeclarationCollectionDateFilter("ExitDate", new ZDateTimeOffset(2022, 2, 26, 10, 0, 0), transactionsCollection, stripBO);

		AssertEquals("Total Transactions match the filter", expected: 1, transactionsCollection.Count);
		AssertEquals("T1 is not in the filter cause date is not correct", expected: false, transactionsCollection.Contains(transaction1));
		AssertEquals("T2 is in the filter cause date is correct and its TRN", expected: true, transactionsCollection.Contains(transaction2));
		AssertEquals("T3 is not in the filter cause date is not correct", expected: false, transactionsCollection.Contains(transaction3));
		AssertEquals("T4 is not in the filter cause date is correct and its TRN but in different header", expected: false, transactionsCollection.Contains(transaction4));

		LoadDeclarationCollectionDateFilter("ExitDate", new ZDateTimeOffset(2022, 3, 25, 10, 0, 0), transactionsCollection, stripBO);

		AssertEquals("Total Transactions match the filter", expected: 0, transactionsCollection.Count);
		AssertEquals("T1 is not in the filter cause date is not correct", expected: false, transactionsCollection.Contains(transaction1));
		AssertEquals("T2 is not in the filter cause date is not correct", expected: false, transactionsCollection.Contains(transaction2));
		AssertEquals("T3 is not in the filter cause date is correct and its ADJ but in different line", expected: false, transactionsCollection.Contains(transaction3));
		AssertEquals("T4 is not in the filter cause date is not correct", expected: false, transactionsCollection.Contains(transaction4));
	});

	public void TestDeclarationDateFilter() => CombineAssertions(() =>
	{
		transaction1.SRT_TransactionDate = new ZDateTimeOffset(2022, 1, 1, 10, 0, 0);
		transaction2.SRT_TransactionDate = new ZDateTimeOffset(2022, 2, 26, 10, 0, 0);
		transaction3.SRT_TransactionDate = new ZDateTimeOffset(2022, 3, 25, 10, 0, 0);
		transaction4.SRT_TransactionDate = new ZDateTimeOffset(2022, 2, 26, 10, 0, 0);

		Factory.Save();

		var transactionsCollection = new CusTempStorageRegLineTransactionCollection(regLine1);
		var stripBO = (TempStorageRegTransactionFilterStripBusinessObject)GetNewFilterStripBusinessObject();
		AssertEquals("[PRECONDITION] Total Declarations 2", 2, transactionsCollection.Count);

		transactionsCollection.AdditionalFilter = stripBO.Filter;

		LoadDeclarationCollectionDateFilter("DeclarationDate", new ZDateTimeOffset(2022, 1, 1, 10, 0, 0), transactionsCollection, stripBO);

		AssertEquals("Total Transactions match the filter", 1, transactionsCollection.Count);
		AssertEquals("T1 is in the filter cause date is correct", expected: true, transactionsCollection.Contains(transaction1));
		AssertEquals("T2 is not in the filter cause date is not correct", expected: false, transactionsCollection.Contains(transaction2));
		AssertEquals("T3 is not in the filter cause date is not correct", expected: false, transactionsCollection.Contains(transaction3));
		AssertEquals("T4 is not in the filter cause date is not correct", expected: false, transactionsCollection.Contains(transaction4));

		LoadDeclarationCollectionDateFilter("DeclarationDate", new ZDateTimeOffset(2022, 2, 26, 10, 0, 0), transactionsCollection, stripBO);

		AssertEquals("Total Transactions match the filter", expected: 1, transactionsCollection.Count);
		AssertEquals("T1 is not in the filter cause date is not correct", expected: false, transactionsCollection.Contains(transaction1));
		AssertEquals("T2 is in the filter cause date is correct", expected: true, transactionsCollection.Contains(transaction2));
		AssertEquals("T3 is not in the filter cause date is not correct", expected: false, transactionsCollection.Contains(transaction3));
		AssertEquals("T4 is not in the filter cause date is correct but in different header", expected: false, transactionsCollection.Contains(transaction4));

		LoadDeclarationCollectionDateFilter("DeclarationDate", new ZDateTimeOffset(2022, 3, 25, 10, 0, 0), transactionsCollection, stripBO);

		AssertEquals("Total Transactions match the filter", expected: 0, transactionsCollection.Count);
		AssertEquals("T1 is not in the filter cause date is not correct", expected: false, transactionsCollection.Contains(transaction1));
		AssertEquals("T2 is not in the filter cause date is not correct", expected: false, transactionsCollection.Contains(transaction2));
		AssertEquals("T3 is not in the filter cause date is correct but in different line", expected: false, transactionsCollection.Contains(transaction3));
		AssertEquals("T4 is not in the filter cause date is not correct", expected: false, transactionsCollection.Contains(transaction4));
	});

	public void TestCommentsFilter() => CombineAssertions(() =>
	{
		transaction1.SRT_Comments = "ZZZ";
		transaction2.SRT_Comments = "AH3";
		transaction3.SRT_Comments = "BBB";
		transaction4.SRT_Comments = "AH3";

		Factory.Save();

		var transactionsCollection = new CusTempStorageRegLineTransactionCollection(regLine1);
		var stripBO = (TempStorageRegTransactionFilterStripBusinessObject)GetNewFilterStripBusinessObject();
		AssertEquals("[PRECONDITION] Total Declarations", 2, transactionsCollection.Count);

		transactionsCollection.AdditionalFilter = stripBO.Filter;

		LoadDeclarationCollectionTextFilter("Comments", "AH3", transactionsCollection, stripBO);

		AssertEquals("Total Transactions match the filter", 1, transactionsCollection.Count);
		AssertEquals("T1 is not in the filter cause its not AH3", expected: false, transactionsCollection.Contains(transaction1));
		AssertEquals("T2 is in the filter cause its AH3", expected: true, transactionsCollection.Contains(transaction2));
		AssertEquals("T3 is not in the filter cause its not AH3", expected: false, transactionsCollection.Contains(transaction3));
		AssertEquals("T4 is not in the filter cause its AH3 but in different header", expected: false, transactionsCollection.Contains(transaction4));

		LoadDeclarationCollectionTextFilter("Comments", "ZZZ", transactionsCollection, stripBO);

		AssertEquals("Total Transactions match the filter", expected: 1, transactionsCollection.Count);
		AssertEquals("T1 is in the filter cause its ZZZ", expected: true, transactionsCollection.Contains(transaction1));
		AssertEquals("T2 is not in the filter cause its not ZZZ", expected: false, transactionsCollection.Contains(transaction2));
		AssertEquals("T3 is not in the filter cause its not ZZZ", expected: false, transactionsCollection.Contains(transaction3));
		AssertEquals("T4 is not in the filter cause its not ZZZ", expected: false, transactionsCollection.Contains(transaction4));

		LoadDeclarationCollectionTextFilter("Comments", "BBB", transactionsCollection, stripBO);

		AssertEquals("Total Transactions match the filter", expected: 0, transactionsCollection.Count);
		AssertEquals("T1 is not in the filter cause its not BBB", expected: false, transactionsCollection.Contains(transaction1));
		AssertEquals("T2 is not in the filter cause its not BBB", expected: false, transactionsCollection.Contains(transaction2));
		AssertEquals("T3 is not in the filter cause its BBB but in different line", expected: false, transactionsCollection.Contains(transaction3));
		AssertEquals("T4 is not in the filter cause its not BBB", expected: false, transactionsCollection.Contains(transaction4));
	});

	public void TestInternalReferenceNumberFilter() => CombineAssertions(() =>
	{
		transaction1.SRT_InternalReferenceNumber = "ZZZ";
		transaction2.SRT_InternalReferenceNumber = "AH3";
		transaction3.SRT_InternalReferenceNumber = "BBB";
		transaction4.SRT_InternalReferenceNumber = "AH3";

		Factory.Save();

		var transactionsCollection = new CusTempStorageRegLineTransactionCollection(regLine1);
		var stripBO = (TempStorageRegTransactionFilterStripBusinessObject)GetNewFilterStripBusinessObject();
		AssertEquals("[PRECONDITION] Total Declarations", 2, transactionsCollection.Count);

		transactionsCollection.AdditionalFilter = stripBO.Filter;

		LoadDeclarationCollectionTextFilter("InternalReferenceNumber", "AH3", transactionsCollection, stripBO);

		AssertEquals("Total Transactions match the filter", 1, transactionsCollection.Count);
		AssertEquals("T1 is not in the filter cause its not AH3", expected: false, transactionsCollection.Contains(transaction1));
		AssertEquals("T2 is in the filter cause its AH3", expected: true, transactionsCollection.Contains(transaction2));
		AssertEquals("T3 is not in the filter cause its not AH3", expected: false, transactionsCollection.Contains(transaction3));
		AssertEquals("T4 is not in the filter cause its AH3 but in different header", expected: false, transactionsCollection.Contains(transaction4));

		LoadDeclarationCollectionTextFilter("InternalReferenceNumber", "ZZZ", transactionsCollection, stripBO);

		AssertEquals("Total Transactions match the filter", expected: 1, transactionsCollection.Count);
		AssertEquals("T1 is in the filter cause its ZZZ", expected: true, transactionsCollection.Contains(transaction1));
		AssertEquals("T2 is not in the filter cause its not ZZZ", expected: false, transactionsCollection.Contains(transaction2));
		AssertEquals("T3 is not in the filter cause its not ZZZ", expected: false, transactionsCollection.Contains(transaction3));
		AssertEquals("T4 is not in the filter cause its not ZZZ", expected: false, transactionsCollection.Contains(transaction4));

		LoadDeclarationCollectionTextFilter("InternalReferenceNumber", "BBB", transactionsCollection, stripBO);

		AssertEquals("Total Transactions match the filter", expected: 0, transactionsCollection.Count);
		AssertEquals("T1 is not in the filter cause its not BBB", expected: false, transactionsCollection.Contains(transaction1));
		AssertEquals("T2 is not in the filter cause its not BBB", expected: false, transactionsCollection.Contains(transaction2));
		AssertEquals("T3 is not in the filter cause its BBB but in different line", expected: false, transactionsCollection.Contains(transaction3));
		AssertEquals("T4 is not in the filter cause its not BBB", expected: false, transactionsCollection.Contains(transaction4));
	});

	public void TestReferenceFilter() => CombineAssertions(() =>
	{
		transaction1.SRT_Reference = "ZZZ";
		transaction2.SRT_Reference = "AH3";
		transaction3.SRT_Reference = "BBB";
		transaction4.SRT_Reference = "AH3";

		Factory.Save();

		var transactionsCollection = new CusTempStorageRegLineTransactionCollection(regLine1);
		var stripBO = (TempStorageRegTransactionFilterStripBusinessObject)GetNewFilterStripBusinessObject();
		AssertEquals("[PRECONDITION] Total Declarations", 2, transactionsCollection.Count);

		transactionsCollection.AdditionalFilter = stripBO.Filter;

		LoadDeclarationCollectionTextFilter("Reference", "AH3", transactionsCollection, stripBO);

		AssertEquals("Total Transactions match the filter", 1, transactionsCollection.Count);
		AssertEquals("T1 is not in the filter cause its not AH3", expected: false, transactionsCollection.Contains(transaction1));
		AssertEquals("T2 is in the filter cause its AH3", expected: true, transactionsCollection.Contains(transaction2));
		AssertEquals("T3 is not in the filter cause its not AH3", expected: false, transactionsCollection.Contains(transaction3));
		AssertEquals("T4 is not in the filter cause its AH3 but in different header", expected: false, transactionsCollection.Contains(transaction4));

		LoadDeclarationCollectionTextFilter("Reference", "ZZZ", transactionsCollection, stripBO);

		AssertEquals("Total Transactions match the filter", expected: 1, transactionsCollection.Count);
		AssertEquals("T1 is in the filter cause its ZZZ", expected: true, transactionsCollection.Contains(transaction1));
		AssertEquals("T2 is not in the filter cause its not ZZZ", expected: false, transactionsCollection.Contains(transaction2));
		AssertEquals("T3 is not in the filter cause its not ZZZ", expected: false, transactionsCollection.Contains(transaction3));
		AssertEquals("T4 is not in the filter cause its not ZZZ", expected: false, transactionsCollection.Contains(transaction4));

		LoadDeclarationCollectionTextFilter("Reference", "BBB", transactionsCollection, stripBO);

		AssertEquals("Total Transactions match the filter", expected: 0, transactionsCollection.Count);
		AssertEquals("T1 is not in the filter cause its not BBB", expected: false, transactionsCollection.Contains(transaction1));
		AssertEquals("T2 is not in the filter cause its not BBB", expected: false, transactionsCollection.Contains(transaction2));
		AssertEquals("T3 is not in the filter cause its BBB but in different line", expected: false, transactionsCollection.Contains(transaction3));
		AssertEquals("T4 is not in the filter cause its not BBB", expected: false, transactionsCollection.Contains(transaction4));
	});

	void LoadDeclarationCollectionTextFilter(ZString filterField, ZString filterValue, CusTempStorageRegLineTransactionCollection transactionsCollection, TempStorageRegTransactionFilterStripBusinessObject stripBO, string comparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith)
	{
		var filter = (ModuleTextFilter)stripBO[filterField];
		filter.Property = filterValue;
		filter.ComparisonOperator = comparisonOperator;
		filter.IsActive = true;
		transactionsCollection.AdditionalFilter = stripBO.Filter;
	}

	void LoadDeclarationCollectionDateFilter(ZString filterField, ZDateTimeOffset filterValue, CusTempStorageRegLineTransactionCollection transactionsCollection, TempStorageRegTransactionFilterStripBusinessObject stripBO)
	{
		var filter = (ModuleDateTimeOffsetFilter)stripBO[filterField];
		filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
		filter.Property1 = filterValue.ToDateTime();
		filter.Property2 = filterValue.ToDateTime();
		filter.IsActive = true;
		transactionsCollection.AdditionalFilter = stripBO.Filter;
	}

	void LoadDeclarationCollectionNumberFilter(ZString filterField, ZDecimal filterValue, CusTempStorageRegLineTransactionCollection transactionsCollection, TempStorageRegTransactionFilterStripBusinessObject stripBO, string propertySearch = "Equal to")
	{
		var filter = (ModuleNumberRangeFilter)stripBO[filterField];
		filter.PropertySearch = propertySearch;
		if (propertySearch != ModuleNumberRangeFilter.SearchTexts.LessThanOrEqualTo)
		{
			filter.Property1 = filterValue;
		}
		filter.Property2 = filterValue;
		filter.IsActive = true;
		transactionsCollection.AdditionalFilter = stripBO.Filter;
	}

	protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => filterBusinessObject;

	protected override void SetUp()
	{
		base.SetUp();
		filterBusinessObject = new TempStorageRegTransactionFilterStripBusinessObject();
		regHeader1 = Factory.New<CusTempStorageRegHeader>();
		regHeader1.SRH_Reference = "REF1";
		regLine1 = regHeader1.CusTempStorageRegLines.AddNew();
		regLine1.SRL_LineNumber = 1;
		transaction1 = regLine1.CusTempStorageRegLineTransactions.AddNew();
		transaction2 = regLine1.CusTempStorageRegLineTransactions.AddNew();

		var line2 = regHeader1.CusTempStorageRegLines.AddNew();
		line2.SRL_LineNumber = 2;
		transaction3 = line2.CusTempStorageRegLineTransactions.AddNew();

		regHeader2 = Factory.New<CusTempStorageRegHeader>();
		regHeader2.SRH_Reference = "REF2";
		var line3 = regHeader2.CusTempStorageRegLines.AddNew();
		line3.SRL_LineNumber = 1;
		transaction4 = line3.CusTempStorageRegLineTransactions.AddNew();
	}
	TempStorageRegTransactionFilterStripBusinessObject filterBusinessObject;
	CusTempStorageRegHeader regHeader1;
	CusTempStorageRegHeader regHeader2;
	CusTempStorageRegLine regLine1;
	CusTempStorageRegLineTransaction transaction1;
	CusTempStorageRegLineTransaction transaction2;
	CusTempStorageRegLineTransaction transaction3;
	CusTempStorageRegLineTransaction transaction4;
}
