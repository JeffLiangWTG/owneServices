using System.Collections.Generic;
using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	public interface IFilterPart
	{
		/// <summary>
		/// Simplify is designed to remove layers of complexity from IFilterPart statements
		/// Remove any part of a statement that is deemed unnecessary
		/// If the entire statement can be removed return null
		/// </summary>
		IFilterPart[] GetSimplifiedVersion(JoinCondition lastJoinCondition);

		/// <summary>
		/// Sometimes you might just need to find the filter parts inside of the filter parts.
		/// This is a non-destructive way of doing this.
		/// </summary>
		IEnumerable<IFilterPart> FilterParts { get; }

		void DisableModifications();

		IFilterPart DeepClone();
		ZNonPersistentDataQuery ParameterisedSql(ParameterNameFactory factory);
		void ParameterisedSql(SqlBuilder sqlBuilder);
		string LiteralTextADO { get; }
		void AddLiteralTextADO(SqlBuilder sqlBuilder);

		/// <summary>
		/// Indicates that when merged with OTHER statements, that this object will need bracketing
		/// </summary>
		bool NeedsBrackets { get; }
		bool FilterIsEmpty { get; }
		bool ContainsOrOperator { get; }
		IEnumerable<SchemaColumn> BlobFilters { get; }
		bool HasParameters { get; }
		bool HasComparisonOperatorLike { get; }
	}
}
