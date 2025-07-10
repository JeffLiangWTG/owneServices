using System;
using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	sealed class ZDBOnlyUnionQuery : ZDBOnlyQuery
	{
		internal ZDBOnlyUnionQuery(Type typeOfBusinessObjectToQuery, SchemaColumn selectColumn, bool unionAllQuery)
			: base(typeOfBusinessObjectToQuery)
		{
			this.selectColumn = selectColumn;
			this.unionAllQuery = unionAllQuery;
		}

		public ZDBOnlyUnionQuery(ZDBOnlySubQuery query, bool addAsUnionAllQuery)
			: base(query.TypeOfBusinessObjectToQuery)
		{
			FilterParts.Append(query.FilterParts);
			selectColumn = query.Key;
			this.unionAllQuery = addAsUnionAllQuery;
		}

		protected override bool NeedsBrackets
		{
			get { return false; }
		}

		protected override void ParameterisedSql(SqlBuilder sqlBuilder)
		{
			AddUnionSQL(sqlBuilder);
			using (sqlBuilder.WithPrefix(" WHERE "))
			{
				base.ParameterisedSql(sqlBuilder);
			}
		}

		public override void AddAsCompleteSQLStatement(SqlBuilder sqlBuilder, string tableName, bool combineFilterAndParams, SchemaColumn[] selectList = null)
		{
			if (combineFilterAndParams)
			{
				AddLiteralTextADO(sqlBuilder);
			}
			else
			{
				var dataQuery = ParameterisedText;
				sqlBuilder.Append(dataQuery.ParameterisedQueryText);
			}
		}

		protected internal override void AddLiteralTextADO(SqlBuilder sqlBuilder)
		{
			AddUnionSQL(sqlBuilder);
			using (sqlBuilder.WithPrefix(" WHERE "))
			{
				base.AddLiteralTextADO(sqlBuilder);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Partial SQL")]
		void AddUnionSQL(SqlBuilder sqlBuilder)
		{
			if (unionAllQuery)
			{
				sqlBuilder.Append(" UNION ALL SELECT ");
			}
			else
			{
				sqlBuilder.Append(" UNION SELECT ");
			}
			if (MaximumRows >= 0)
			{
				sqlBuilder.Append("TOP ");
				sqlBuilder.Append(MaximumRows.ToString());
				sqlBuilder.Append(" ");
			}
			sqlBuilder.Append(selectColumn.Name);
			sqlBuilder.Append(" FROM ");
			sqlBuilder.Append(AddSchemaName(TableName));
			AddTableHints(sqlBuilder);
		}

		#region Equals / GetHashCode

		public override bool Equals(object obj)
		{
			ZDBOnlyUnionQuery rhs = obj as ZDBOnlyUnionQuery;
			return
				rhs != null &&
				base.Equals(rhs) &&

				selectColumn == rhs.selectColumn;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Required for Equals override")]
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#endregion

		#region Implementation

		readonly SchemaColumn selectColumn;
		readonly Boolean unionAllQuery;

		#endregion
	}
}
