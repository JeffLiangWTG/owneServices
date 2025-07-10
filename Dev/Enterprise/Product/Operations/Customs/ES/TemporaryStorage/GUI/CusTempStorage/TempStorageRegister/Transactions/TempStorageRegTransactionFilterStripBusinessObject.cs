using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI;

public class TempStorageRegTransactionFilterStripBusinessObject : FilterStripBusinessObject
{
	public TempStorageRegTransactionFilterStripBusinessObject()
	{
		((IFilterStripBusinessObjectInternals)this).LayoutContext = "CusTempStorageRegLineTransaction";
	}

	public static class FilterConstants
	{
#pragma warning disable CW1161 // Res.GetString Analyzer
		public const string BondAmount = "BondAmount";
		public const string GrossWeight = "GrossWeight";
		public const string PackageQty = "PackageQty";
		public const string Status = "Status";
		public const string ReferenceType = "ReferenceType";
		public const string TransactionType = "TransactionType";
		public const string InternalReferenceType = "InternalReferenceType";
		public const string EntryDate = "EntryDate";
		public const string ExitDate = "ExitDate";
		public const string DeclarationDate = "DeclarationDate";
		public const string Comments = "Comments";
		public const string InternalReferenceNumber = "InternalReferenceNumber";
		public const string Reference = "Reference";
#pragma warning restore CW1161 // Res.GetString Analyzer
	}

	protected override ModuleFilterCollection GetModuleFiltersCore()
	{
		var filters = new ModuleFilterCollection();
		AddBondAmountFilter(filters);
		AddGrossWeightFilter(filters);
		AddPackageQtyFilter(filters);
		AddStatusFilter(filters);
		AddReferenceTypeFilter(filters);
		AddTransactionTypeFilter(filters);
		AddInternalReferenceTypeFilter(filters);
		AddEntryDateFilter(filters);
		AddExitDateFilter(filters);
		AddDeclarationDateFilter(filters);
		AddCommentsFilter(filters);
		AddInternalReferenceNumberFilter(filters);
		AddReferenceFilter(filters);
		return filters;
	}

	void AddBondAmountFilter(ModuleFilterCollection filters)
	{
		var filter = filters.AddNumberRangeFilter(FilterConstants.BondAmount, CusTempStorageRegLineTransactionSchema.SRT_BondAmount);
		filter.MultilingualDescription = ResString.GetMultilingualString("52558EA1-F53D-4D36-B013-D6BDEBFD3595", "Bond Amount");
		filter.Category = FilterCategories.NumbersAndReferences;
	}

	void AddGrossWeightFilter(ModuleFilterCollection filters)
	{
		var filter = filters.AddNumberRangeFilter(FilterConstants.GrossWeight, CusTempStorageRegLineTransactionSchema.SRT_GrossWeight);
		filter.MultilingualDescription = ResString.GetMultilingualString("2100B28F-DCCD-47C3-BC6F-D32BC255A1D3", "Gross Weight");
		filter.Category = FilterCategories.NumbersAndReferences;
	}

	void AddPackageQtyFilter(ModuleFilterCollection filters)
	{
		var filter = filters.AddNumberRangeFilter(FilterConstants.PackageQty, CusTempStorageRegLineTransactionSchema.SRT_PackageQty);
		filter.MultilingualDescription = ResString.GetMultilingualString("31BA19EE-3189-44A4-B559-66879DFAA28D", "Package Qty");
		filter.Category = FilterCategories.NumbersAndReferences;
	}

	void AddStatusFilter(ModuleFilterCollection filters)
	{
		var filter = filters.AddTextFilter(FilterConstants.Status, CusTempStorageRegLineTransactionSchema.SRT_TransactionStatus, list: Factory.GetCachedValue<CusTempStorageRegLineTransactionStatusList>());
		filter.MultilingualDescription = ResString.GetMultilingualString("04BC7122-07C5-4CFD-887B-0BBC6FBE6211", FilterConstants.Status);
		filter.DefaultProperty = CusTempStorageRegLineTransactionStatusList.Codes.Deleted;
		filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual;
		filter.Category = FilterCategories.StatusAndFlags;
	}

	void AddReferenceTypeFilter(ModuleFilterCollection filters)
	{
		var filter = filters.AddTextFilter(FilterConstants.ReferenceType, CusTempStorageRegLineTransactionSchema.SRT_ReferenceType, list: Factory.GetCachedValue<CusTempStorageRegLineTransactionReferenceTypeList>());
		filter.MultilingualDescription = ResString.GetMultilingualString("21FE8B0B-B067-4F37-B6B5-E183B69E6E79", "Reference Type");
		filter.Category = FilterCategories.StatusAndFlags;
	}

	void AddTransactionTypeFilter(ModuleFilterCollection filters)
	{
		var filter = filters.AddTextFilter(FilterConstants.TransactionType, CusTempStorageRegLineTransactionSchema.SRT_TransactionType, list: Factory.GetCachedValue<CusTempStorageRegLineTransactionTypeList>());
		filter.MultilingualDescription = ResString.GetMultilingualString("0B3C8D4B-330E-4392-B3D7-4C2DB4B60CAE", "Transaction Type");
		filter.Category = FilterCategories.StatusAndFlags;
	}

	void AddInternalReferenceTypeFilter(ModuleFilterCollection filters)
	{
		var filter = filters.AddTextFilter(FilterConstants.InternalReferenceType, CusTempStorageRegLineTransactionSchema.SRT_InternalReferenceType, list: Factory.GetCachedValue<CusTempStorageRegLineTransactionInternalReferenceTypeList>());
		filter.MultilingualDescription = ResString.GetMultilingualString("36D33E11-3495-4052-A6F8-7CC067D590A3", "Internal Reference Type");
		filter.Category = FilterCategories.StatusAndFlags;
	}

	void AddEntryDateFilter(ModuleFilterCollection filters)
	{
		var filter = filters.AddDateFilter(
				FilterConstants.EntryDate,
				(comparisonOperator, value1, value2) => GetRegLineDateOffsetQuery(FilterConstants.EntryDate, CusTempStorageRegLineTransactionSchema.SRT_PhysicalInOutDate, comparisonOperator, value1, value2)
			);
		filter.MultilingualDescription = ResString.GetMultilingualString("B34D2F60-C2FB-460A-AEF7-8341D4880FF3", "Entry Date");
		filter.Category = FilterCategories.Dates;
	}

	void AddExitDateFilter(ModuleFilterCollection filters)
	{
		var filter = filters.AddDateFilter(
				FilterConstants.ExitDate,
				(comparisonOperator, value1, value2) => GetRegLineDateOffsetQuery(FilterConstants.ExitDate, CusTempStorageRegLineTransactionSchema.SRT_PhysicalInOutDate, comparisonOperator, value1, value2)
			);
		filter.MultilingualDescription = ResString.GetMultilingualString("4B274D55-0524-45C3-BD8F-49A69DEC31F4", "Exit Date");
		filter.Category = FilterCategories.Dates;
	}

	ZQuery GetRegLineDateOffsetQuery(ZString filteredField, SchemaDateTimeOffsetColumn column, DateComparisonOperator comparisonOperator, ZDateTimeOffset value1, ZDateTimeOffset value2)
	{
		var query = new ZQuery();
		AddDateTimeOffsetRange(query, comparisonOperator, JoinCondition.And, column, value1, value2, lowerDatePartOnly: false, upperDatePartOnly: false);

		return filteredField.ToString() switch
		{
			FilterConstants.EntryDate => GetSecondQuery(query, typesForEntryDate, CusTempStorageRegLineTransactionSchema.SRT_TransactionType, SQLComparisonOperator.Contains, JoinCondition.Or),
			FilterConstants.ExitDate => GetSecondQuery(query, typesForExitDate, CusTempStorageRegLineTransactionSchema.SRT_TransactionType, SQLComparisonOperator.Contains, JoinCondition.Or),
			_ => query,
		};
	}

	readonly IEnumerable<string> typesForExitDate = new[] { CusTempStorageRegLineTransactionTypeList.Codes.Adjustment, CusTempStorageRegLineTransactionTypeList.Codes.Transaction };
	readonly IEnumerable<string> typesForEntryDate = new[] { CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance };

	ZQuery GetSecondQuery(ZQuery query, IEnumerable<string> typesControl, SchemaColumn colum, SQLComparisonOperator comparation, JoinCondition join)
	{
		var typeQuery = new ZQuery();
		typesControl.ForEach(x => typeQuery.AddToFilter(new ZQuery(colum, comparation, x), join));
		return query.AddToFilter(typeQuery, JoinCondition.And);
	}

	void AddDeclarationDateFilter(ModuleFilterCollection filters)
	{
		var filter = filters.AddDateFilter(FilterConstants.DeclarationDate, CusTempStorageRegLineTransactionSchema.SRT_TransactionDate);
		filter.MultilingualDescription = ResString.GetMultilingualString("36DF7A17-FEBC-4473-A729-0EA5E3A06ACE", "Declaration Date");
		filter.Category = FilterCategories.Dates;
	}

	void AddCommentsFilter(ModuleFilterCollection filters)
	{
		var filter = filters.AddTextFilter(FilterConstants.Comments, CusTempStorageRegLineTransactionSchema.SRT_Comments);
		filter.MultilingualDescription = ResString.GetMultilingualString("E50E603D-8E6D-41DD-AE3D-9B1028958A41", FilterConstants.Comments);
		filter.Category = FilterCategories.TextSearch;
	}

	void AddInternalReferenceNumberFilter(ModuleFilterCollection filters)
	{
		var filter = filters.AddTextFilter(FilterConstants.InternalReferenceNumber, CusTempStorageRegLineTransactionSchema.SRT_InternalReferenceNumber);
		filter.MultilingualDescription = ResString.GetMultilingualString("F20ACD27-2C45-4DBB-B23D-EC9B18874A41", "Internal Reference Number");
		filter.Category = FilterCategories.TextSearch;
	}

	void AddReferenceFilter(ModuleFilterCollection filters)
	{
		var filter = filters.AddTextFilter(FilterConstants.Reference, CusTempStorageRegLineTransactionSchema.SRT_Reference);
		filter.MultilingualDescription = ResString.GetMultilingualString("0CA20ECF-AF93-4992-8C0D-B5FE848F8012", FilterConstants.Reference);
		filter.Category = FilterCategories.TextSearch;
	}
}
