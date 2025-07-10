using System.Collections.Generic;
using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	public interface IFetchHint
	{
		/// <summary>
		/// Return the hint expressed as a query
		/// </summary>
		/// <returns></returns>
		ZQuery GetQuery();

		/// <summary>
		/// When fetch hint executes, should blobs be loaded too?
		/// </summary>
		IEnumerable<SchemaColumn> LoadWithBlobs { get; }

		string TableName { get; }

		/// <summary>
		/// Indicates whether the rowfactory has executed this fetch hint
		/// </summary>
		bool IsDataHintLoaded { get; set; }

		/// <summary>
		/// The key that will uniquely identify the fetch hint so duplicate hints can be discarded
		/// </summary>
		/// <returns></returns>
		IQueryHashKey GetHashKeyObject();

		/// <summary>
		/// Generate whatever Sql you want for the hint - remember to keep previous hint data in the builder
		/// </summary>
		/// <param name="builder"></param>
		void GenerateQuery(QueryBuilder builder);

		/// <summary>
		/// Each Fetch Hint implementation should return a key that will separate it from other builders
		/// Typically the column that the fetch hint is using
		/// </summary>
		string BuilderKey { get; }

		/// <summary>
		/// Indicate whether the fetch hint cannot be satisfied by the current contents
		/// of the RowFactory - if data in memory is good, return false
		/// </summary>
		bool IsNeeded(QueryHistoryProvider historyProvider);
	}
}
