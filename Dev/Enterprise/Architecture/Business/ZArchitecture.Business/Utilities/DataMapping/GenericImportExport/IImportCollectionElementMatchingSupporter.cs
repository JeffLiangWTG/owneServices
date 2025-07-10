using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.DataMapping
{
	public interface IImportCollectionElementMatchingSupporter
	{
		/// <summary>
		/// Returns the name of the column to be used for matching an element
		/// in the collection against a single column/value.
		/// </summary>
		string MatchingColumnName { get; }

		/// <summary>
		/// Returns a Bizo that contains a `MatchingColumnName` column with the value
		/// `value`. If `MatchingColumnName` is string.empty then this method
		/// must not be called.
		///
		/// There should be only one Bizo that matches a given key in the collection
		/// </summary>
		/// <returns>The match or null if not found.</returns>
		BusinessObject GetMatchingBizObject(string value);

		/// <summary>
		/// Called to prepare the bizo for reuse. For example, clearing out
		/// child objects or resetting some state.
		/// </summary>
		void PrepareForReuse(BusinessObject matchedBizo);

		/// <summary>
		/// When true, the generic matching code for matching a bizo based on
		/// all the ADAW-provided columns and values matching is executed.
		/// </summary>
		bool IsGenericColumnMatchingAllowed { get; }

		/// <summary>
		/// When true, allows the logic in GetBizoBasedOnColumnMatches to run.
		/// </summary>
		bool FindGenericColumnMatches { get; }
	}
}
