using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;

namespace Enterprise.ArchiveManager.Business.ArchiveEligibility
{
	public abstract class ArchiveableFilter<TBizo>(string filterName, string filterClause) where TBizo : BusinessObject
	{
		public readonly string FilterName = filterName;
		public readonly string FilterClause = FormatFilterClauseString(filterClause);

		protected ArchiveableFilter(string filterName, ZQuery filterClause)
			: this(filterName, filterClause.LiteralTextSqlFormatted) { }

		public void ApplyTo(ZQuery query)
			=> query.AddFilterAndZSQLParameterCollection(FilterClause, null);

		public bool ExecuteFor(TBizo bizo)
		{
			if (!bizo.IsInDatabase)
			{
				throw new InvalidOperationException("Bizo must exist in database");
			}

			var pk = bizo.PK.ToSqlGuid();
			var pkColumnName = bizo.PKSchemaColumn.Name.QuoteName();
			var tableName = bizo.TableName.QuoteName();

			var query = $"FROM dbo.{tableName} AS mainArchiveableItem WHERE {pkColumnName} = {pk} AND ({FilterClause})";
			var result = Db.Connection.Exists(query);

			return result;
		}

		public override string ToString()
			=> $"{GetType().Name}({FilterName})";

		public override bool Equals(object obj)
			=> obj is ArchiveableFilter<TBizo> other && GetHashCode() == other.GetHashCode();

		public override int GetHashCode()
			=> (typeof(TBizo), FilterName, FilterClause).GetHashCode();

		static string FormatFilterClauseString(string filterClause)
		{
			var lines = filterClause
				.Split('\n')
				.Select(x => x.Trim());

			return string.Join("\n", lines).Trim();
		}
	}
}
