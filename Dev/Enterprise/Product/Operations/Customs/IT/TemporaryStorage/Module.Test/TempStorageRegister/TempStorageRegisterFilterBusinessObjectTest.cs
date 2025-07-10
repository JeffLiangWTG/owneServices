using System;
using CargoWise.Types;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using CusTempStorageRegHeader = Enterprise.Customs.IT.TemporaryStorage.Business.CusTempStorageRegHeader;
using CusTempStorageRegHeaderCollection = Enterprise.Customs.IT.TemporaryStorage.Business.CusTempStorageRegHeaderCollection;

namespace Enterprise.Customs.IT.TemporaryStorage.Module.Testing;

[TestedType(typeof(TempStorageRegisterFilterBusinessObject))]
sealed class TempStorageRegisterFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
{
	public void TestRegisterReferenceFilter()
	{
		var header1 = Factory.New<CusTempStorageRegHeader>();
		header1.SRH_Reference = "Job1";
		var header2 = Factory.New<CusTempStorageRegHeader>();
		header2.SRH_Reference = "Job2";
		var header3 = Factory.New<CusTempStorageRegHeader>();
		header3.SRH_Reference = "Job23";

		var filter = (ModuleTextFilter)filterObject[TempStorageRegisterFilterBusinessObject.FilterConstants.RegisterReference];
		filter.IsActive = true;
		filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
		filter.Property = "Job2";
		var filterQuery = filterObject.Filter;

		CombineAssertions(() =>
		{
			AssertEquals("Category", FilterCategories.NumbersAndReferences, filter.Category);
			AssertEquals("header1, SRH_Reference doesn't match", false, header1.MatchesFilter(filterQuery));
			AssertEquals("header2, SRH_Reference matches", true, header2.MatchesFilter(filterQuery));
			AssertEquals("header3, SRH_Reference matches", true, header3.MatchesFilter(filterQuery));
		});
	}

	public void TestBillMrnFilter()
	{
		var header1 = Factory.New<CusTempStorageRegHeader>();
		header1.SRH_PreviousReference = "REF001";
		var header2 = Factory.New<CusTempStorageRegHeader>();
		header2.SRH_PreviousReference = "REF002";
		var header3 = Factory.New<CusTempStorageRegHeader>();
		header3.SRH_PreviousReference = "REF003";

		var filter = (ModuleTextFilter)filterObject[TempStorageRegisterFilterBusinessObject.FilterConstants.BillMrn];
		filter.IsActive = true;
		filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
		filter.Property = "REF002";
		var filterQuery = filterObject.Filter;

		CombineAssertions(() =>
		{
			AssertEquals("Category", FilterCategories.NumbersAndReferences, filter.Category);
			AssertEquals("header1, SRH_PreviousReference doesn't match", false, header1.MatchesFilter(filterQuery));
			AssertEquals("header2, SRH_PreviousReference matches", true, header2.MatchesFilter(filterQuery));
			AssertEquals("header3, SRH_PreviousReference doesn't match", false, header3.MatchesFilter(filterQuery));
		});
	}

	public void TestPreviousRefTypeFilter()
	{
		AssertNull("PreviousRefType filter should not be present", filterObject[TempStorageRegisterFilterBusinessObject.Schema.PreviousRefType]);
	}

	public void TestRemainingPackageQuantityFilter()
	{
		AssertNull("RemainingPackagesQuantity filter should not be present", filterObject[TempStorageRegisterFilterBusinessObject.Schema.RemainingPackagesQuantity]);
	}

	public void TestPackageTypeTextFilter()
	{
		AssertNull("PackageType filter should not be present", filterObject[TempStorageRegisterFilterBusinessObject.Schema.PackageType]);
	}

	public void TestContainerNumberFilter()
	{
		var header1 = CreateContainerNumber("AB");
		var header2 = CreateContainerNumber("CD");
		var header3 = CreateContainerNumber("EF");
		Factory.Save();

		var filter = (ModuleTextFilter)filterObject["Container Number"];
		AssertEquals("Category", FilterCategories.NumbersAndReferences, filter.Category);

		filter.IsActive = true;
		filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
		filter.Property = string.Empty;

		var collection = new CusTempStorageRegHeaderCollection(Factory);
		collection.AdditionalFilter = filterObject.Filter;

		AssertEquals(3, collection.Count);
		AssertEquals(header1.PK, collection[0].PK);
		AssertEquals(header2.PK, collection[1].PK);
		AssertEquals(header3.PK, collection[2].PK);

		filter.Property = "EF";
		collection.AdditionalFilter = filterObject.Filter;

		AssertEquals(1, collection.Count);
		AssertEquals(header3.PK, collection[0].PK);

		filter.Property = "AB";
		collection.AdditionalFilter = filterObject.Filter;

		AssertEquals(1, collection.Count);
		AssertEquals(header1.PK, collection[0].PK);

		filter.Property = "XY";
		collection.AdditionalFilter = filterObject.Filter;

		AssertEquals(0, collection.Count);

		return;

		CusTempStorageRegHeader CreateContainerNumber(string val)
		{
			var header = Factory.NewWithValidTestData<CusTempStorageRegHeader>();

			var line = header.CusTempStorageRegLines.AddNew();
			line.SRL_LineNumber = 1;

			var container = line.Containers.AddNew();
			container.CY_Code = val;

			return header;
		}
	}

	public void TestGrossWeightTransactionFilter()
	{
		var header1 = CreateTransaction(1.234);
		var header2 = CreateTransaction(9.876);
		var header3 = CreateTransaction(1.248);
		Factory.Save();

		var filter = (ModuleNumberRangeFilter)filterObject["Gross Weight (transaction)"];
		AssertEquals("Category", FilterCategories.NumbersAndReferences, filter.Category);

		filter.IsActive = true;
		filter.Property1 = 0;
		filter.Property2 = 10;

		var collection = new CusTempStorageRegHeaderCollection(Factory);
		collection.AdditionalFilter = filterObject.Filter;

		AssertEquals(3, collection.Count);
		AssertEquals(header1.PK, collection[0].PK);
		AssertEquals(header2.PK, collection[1].PK);
		AssertEquals(header3.PK, collection[2].PK);

		filter.Property1 = 1.238;
		filter.Property2 = 1.258;
		collection.AdditionalFilter = filterObject.Filter;

		AssertEquals(1, collection.Count);
		AssertEquals(header3.PK, collection[0].PK);

		filter.Property1 = 1.224;
		filter.Property2 = 1.244;
		collection.AdditionalFilter = filterObject.Filter;

		AssertEquals(1, collection.Count);
		AssertEquals(header1.PK, collection[0].PK);

		filter.Property1 = 10;
		filter.Property2 = 20;
		collection.AdditionalFilter = filterObject.Filter;

		AssertEquals(0, collection.Count);

		return;

		CusTempStorageRegHeader CreateTransaction(ZDecimal val)
		{
			var header = Factory.NewWithValidTestData<CusTempStorageRegHeader>();

			var line = header.CusTempStorageRegLines.AddNew();
			line.SRL_LineNumber = 1;

			var transaction = line.CusTempStorageRegLineTransactions.AddNew();
			transaction.SRT_TransactionType = "OBL";
			transaction.SRT_GrossWeight = val;
			return header;
		}
	}

	public void TestJobReferenceTransactionFilter() =>
		AssertTransactionTextFilter("Job Reference (transaction)", FilterCategories.NumbersAndReferences, (transaction, val) => transaction.SRT_Reference = val);

	public void TestPreviousDocumentTransactionFilter() =>
		AssertTransactionTextFilter("Previous Document (transaction)", FilterCategories.NumbersAndReferences, (transaction, val) => transaction.SRT_InternalReferenceNumber = val);

	public void TestPreviousDocumentTypeTransactionFilter() =>
		AssertTransactionTextFilter("Previous Document Type (transaction)", FilterCategories.NumbersAndReferences, (transaction, val) => transaction.SRT_InternalReferenceType = val,
			CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Destruction,
			CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others,
			CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);

	public void TestReferenceTypeFilter() =>
		AssertTransactionTextFilter("Reference Type", FilterCategories.NumbersAndReferences, (transaction, val) => transaction.SRT_ReferenceType = val,
			CusTempStorageRegLineTransactionReferenceTypeList.Codes.MovementReferenceNumber,
			string.Empty,
			CusTempStorageRegLineTransactionReferenceTypeList.Codes.Number);

	public void TestTransactionTypeFilter() =>
		AssertTransactionTextFilter("Transaction Type", FilterCategories.NumbersAndReferences, (transaction, val) => transaction.SRT_TransactionType = val,
			CusTempStorageRegLineTransactionTypeList.Codes.Adjustment,
			CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance,
			CusTempStorageRegLineTransactionTypeList.Codes.Transaction);

	void AssertTransactionTextFilter(string filterName, FilterCategory category, Action<CusTempStorageRegLineTransaction, string> setValue,
		string val1 = null, string val2 = null, string val3 = null, string invalidVal = null)
	{
		val1 ??= "Val1";
		val2 ??= "Val2";
		val3 ??= "Val3";
		invalidVal ??= "XYZ";

		var header1 = CreateTransaction(val1);
		var header2 = CreateTransaction(val2);
		var header3 = CreateTransaction(val3);
		Factory.Save();

		var filter = (ModuleTextFilter)filterObject[filterName];
		AssertEquals("Category", category, filter.Category);

		filter.IsActive = true;
		filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
		filter.Property = string.Empty;

		var collection = new CusTempStorageRegHeaderCollection(Factory);
		collection.AdditionalFilter = filterObject.Filter;

		AssertEquals(3, collection.Count);
		AssertEquals(header1.PK, collection[0].PK);
		AssertEquals(header2.PK, collection[1].PK);
		AssertEquals(header3.PK, collection[2].PK);

		filter.Property = val3;
		collection.AdditionalFilter = filterObject.Filter;

		AssertEquals(1, collection.Count);
		AssertEquals(header3.PK, collection[0].PK);

		filter.Property = val1;
		collection.AdditionalFilter = filterObject.Filter;

		AssertEquals(1, collection.Count);
		AssertEquals(header1.PK, collection[0].PK);

		filter.Property = invalidVal;
		collection.AdditionalFilter = filterObject.Filter;

		AssertEquals(0, collection.Count);

		return;

		CusTempStorageRegHeader CreateTransaction(string val)
		{
			var header = Factory.NewWithValidTestData<CusTempStorageRegHeader>();

			var line = header.CusTempStorageRegLines.AddNew();
			line.SRL_LineNumber = 1;

			var transaction = line.CusTempStorageRegLineTransactions.AddNew();
			transaction.SRT_TransactionType = "OBL";
			setValue(transaction, val);
			return header;
		}
	}

	protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new TempStorageRegisterFilterBusinessObject();

	protected override void SetUp()
	{
		base.SetUp();
		filterObject = new TempStorageRegisterFilterBusinessObject();
	}
	TempStorageRegisterFilterBusinessObject filterObject;
}
