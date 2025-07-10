using System.Linq;
using CargoWise.Common;

namespace CargoWise.EntityFramework
{
	public class SqlFilterPartAppender
	{
		public SqlFilterPartAppender(SqlBuilder.QueryType queryType)
		{
			this.queryType = queryType;
		}
		readonly SqlBuilder.QueryType queryType;

		public ZNonPersistentDataQuery GetParameterisedSql(FilterStringBuilder filterStringBuilder)
		{
			var sqlBuilder = new SqlBuilder(queryType);
			AddParameterisedSql(sqlBuilder, filterStringBuilder);
			return new ZNonPersistentDataQuery(sqlBuilder.GetSql(), sqlBuilder.GetParameters());
		}

		public void AddParameterisedSql(SqlBuilder sqlBuilder, FilterStringBuilder filterStringBuilder)
		{
			var initialLen = sqlBuilder.Length;
			JoinCondition lastJoinCondition = null;
			IFilterPart lastFilterPart = null;
			bool hasAndCondition = false;
			bool hasOrCondition = false;
			bool bracketsNeeded = GetBracketsNeeded(filterStringBuilder);
			foreach (IFilterPart filterPart in filterStringBuilder.filterParts)
			{
				if (filterPart is JoinCondition joinCondition)
				{
					if (lastJoinCondition != null)
					{
						ErrorReporter.ReportOnce("Stutter", "Multiple join conditions in a row");
					}
					lastJoinCondition = joinCondition;
					hasAndCondition |= (lastJoinCondition == JoinCondition.And);
					hasOrCondition |= (lastJoinCondition == JoinCondition.Or);
				}
				else
				{
					void BuildJoinCondition()
					{
						if (lastJoinCondition != null)
						{
							if (lastFilterPart != null)
							{
								if (lastJoinCondition.Equals(JoinCondition.Union) || lastJoinCondition.Equals(JoinCondition.UnionAll))
								{
									sqlBuilder.Append(System.Environment.NewLine);
									sqlBuilder.Append(lastJoinCondition.Text);
									sqlBuilder.Append(System.Environment.NewLine);
									sqlBuilder.AppendSelectStatement();
									sqlBuilder.Append(" WHERE ");
								}
								else
								{
									sqlBuilder.Append(" ");
									sqlBuilder.Append(lastJoinCondition.Text);
									sqlBuilder.Append(" ");
								}
							}
						}
					}

					bool addBracket = bracketsNeeded && filterPart.NeedsBrackets;

					using (sqlBuilder.WithPrefix(_ => BuildJoinCondition()))
					using (addBracket ? sqlBuilder.WithPrefix("(") : null)
					{
						switch (queryType)
						{
							case SqlBuilder.QueryType.LiteralADO:
								filterPart.AddLiteralTextADO(sqlBuilder);
								break;
							case SqlBuilder.QueryType.Parameterised:
							case SqlBuilder.QueryType.Literal:
								filterPart.ParameterisedSql(sqlBuilder);
								break;
						}

						if (!sqlBuilder.HasPendingPrefix) // HasPendingPrefix is a cheeky way to check if this sql part actually changed anything.
						{
							if (addBracket)
							{
								sqlBuilder.Append(")");
							}
							lastFilterPart = filterPart;
						}
						lastJoinCondition = null;
					}
				}
			}
		}

		bool GetBracketsNeeded(FilterStringBuilder builder)
		{
			return builder.filterParts.Where(f => !f.FilterIsEmpty).Take(2).Count() == 2;
		}
	}
}
