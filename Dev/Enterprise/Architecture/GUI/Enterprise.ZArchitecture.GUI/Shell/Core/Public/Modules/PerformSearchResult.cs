using System;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI
{
	public class PerformSearchResult
	{
		PerformSearchResult(PerformSearchResultType type, BusinessObjectFactory factory = null, ZQuery query = null, BusinessObject[] loadedRows = null, string errorMessage = null, bool permitActiveCollectionUpdates = false)
		{
			Type = type;
			Factory = factory;
			Query = query;
			LoadedRows = loadedRows ?? Array.Empty<BusinessObject>();
			ErrorMessage = errorMessage ?? string.Empty;
			PermitActiveCollectionUpdates = permitActiveCollectionUpdates;
		}
		public static PerformSearchResult Success(BusinessObjectFactory factory, ZQuery query, BusinessObject[] bizos, bool permitActiveCollectionUpdates) =>
			new PerformSearchResult(PerformSearchResultType.Success, factory: factory, query: query, loadedRows: bizos, permitActiveCollectionUpdates: permitActiveCollectionUpdates);
		public static PerformSearchResult MaxRowsExceeded(ZQuery query) => new PerformSearchResult(PerformSearchResultType.MaxRowsExceeded, query: query);

		public static PerformSearchResult TooManyParameters() => new PerformSearchResult(PerformSearchResultType.TooManyParameters);
		public static PerformSearchResult QueryTooComplicated() => new PerformSearchResult(PerformSearchResultType.QueryTooComplicated);
		public static PerformSearchResult SqlException285() => new PerformSearchResult(PerformSearchResultType.SqlException258);
		public static PerformSearchResult MinimumRowSizeExceeds() => new PerformSearchResult(PerformSearchResultType.MinimumRowSizeExceeds);
		public static PerformSearchResult LimitedRunError(string errorMessage) => new PerformSearchResult(PerformSearchResultType.LimitedRunError, errorMessage: errorMessage);
		public static PerformSearchResult CustomGridLoad(BusinessObjectFactory factory, ZQuery query) => new PerformSearchResult(PerformSearchResultType.CustomGridLoad, factory: factory, query: query);
		public static PerformSearchResult CustomGridLoad(BusinessObjectFactory factory, ZQuery query, BusinessObject[] loadedRows, bool permitActiveCollectionUpdates) => new PerformSearchResult(PerformSearchResultType.CustomGridLoad, factory: factory, query: query, loadedRows: loadedRows, permitActiveCollectionUpdates: permitActiveCollectionUpdates);

		public PerformSearchResultType Type { get; }
		public BusinessObjectFactory Factory { get; }
		public BusinessObject[] LoadedRows { get; set; }
		public ZQuery Query { get; }
		public string ErrorMessage { get; }
		public bool PermitActiveCollectionUpdates { get; }
	}

	public enum PerformSearchResultType
	{
		Success,
		CustomGridLoad, // For that one trouble maker who decided to hijack the default behaviour and do their own thing.
		QueryTooComplicated,
		TooManyParameters,
		SqlException258,
		LimitedRunError,
		MaxRowsExceeded,
		MinimumRowSizeExceeds
	}
}
