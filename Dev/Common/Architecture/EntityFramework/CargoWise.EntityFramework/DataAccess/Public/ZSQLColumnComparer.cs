using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	internal sealed class ZSQLColumnComparer : AbstractFilterPart, IFilterPart
	{
		internal ZSQLColumnComparer(SchemaColumn column, SQLComparisonOperator comparisonOperator, SchemaColumn withColumn)
		{
			#region Preconditions / Exception throwing
			if (column == null)
			{
				throw new ArgumentNullException(nameof(column));
			}
			if (comparisonOperator == null)
			{
				throw new ArgumentNullException(nameof(comparisonOperator));
			}
			if (withColumn == null)
			{
				throw new ArgumentNullException(nameof(withColumn));
			}
			if (comparisonOperator == SQLComparisonOperator.EqualToDatePartOnly ||
				comparisonOperator == SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly ||
				comparisonOperator == SQLComparisonOperator.LessThanOrEqualToDatePartOnly)
			{
				throw new ArgumentException("invalid comparison operator");
			}

			#endregion

			this.column = column;
			this.comparisonOperator = comparisonOperator;
			this.withColumn = withColumn;
		}

		readonly SchemaColumn column;
		readonly SchemaColumn withColumn;
		readonly SQLComparisonOperator comparisonOperator;

		public SchemaColumn Column1 => column;
		public SchemaColumn Column2 => withColumn;

		#region Equals / GetHashCode

		public override bool Equals(object obj)
		{
			ZSQLColumnComparer rhs = obj as ZSQLColumnComparer;
			return
				rhs != null &&
				column == rhs.column &&
				comparisonOperator == rhs.comparisonOperator &&
				withColumn == rhs.withColumn;
		}

		public override int GetHashCode()
		{
			return column.GetHashCode();
		}

		#endregion

		#region IFilterPart Members

		IFilterPart[] IFilterPart.GetSimplifiedVersion(JoinCondition lastJoinCondition)
		{
			return new IFilterPart[] { this };
		}

		IEnumerable<IFilterPart> IFilterPart.FilterParts => Enumerable.Empty<IFilterPart>();

		void IFilterPart.DisableModifications()
		{
		}

		IFilterPart IFilterPart.DeepClone()
		{
			return (ZSQLColumnComparer)MemberwiseClone();   // Immutable
		}

		ZNonPersistentDataQuery IFilterPart.ParameterisedSql(ParameterNameFactory factory)
		{
			return new ZNonPersistentDataQuery(((IFilterPart)this).LiteralTextADO);
		}

		void IFilterPart.ParameterisedSql(SqlBuilder sqlBuilder)
		{
			AddLiteralTextADO(sqlBuilder);
		}

		string IFilterPart.LiteralTextADO
		{
			get
			{
				var builder = new SqlBuilder();
				AddLiteralTextADO(builder);
				return builder.ToString();
			}
		}

		public void AddLiteralTextADO(SqlBuilder sqlBuilder)
		{
			sqlBuilder.Append(column.Name)
			.Append(" ")
			.Append(comparisonOperator.ComparisonText(withColumn))
			.Append(" ")
			.Append(withColumn.Name);
		}

		bool IFilterPart.NeedsBrackets
		{
			get { return false; }
		}

		bool IFilterPart.FilterIsEmpty
		{
			get { return false; }
		}

		bool IFilterPart.ContainsOrOperator
		{
			get { return false; }
		}

		IEnumerable<SchemaColumn> IFilterPart.BlobFilters
		{
			get { return Enumerable.Empty<SchemaColumn>(); }
		}

		#endregion
	}
}
