using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	public sealed class ZDBOnlySubQuery : ZDBOnlyQuery, IFilterPart
	{
		/// <param name="typeOfBusinessObjectToSubQuery">Equivalent to the FROM table of SubQuery</param>
		/// <param name="key">eg, JD_JS, JO_JD, etc. This is the field that has got two table prefixes in it and hence explains the join.</param>
		public ZDBOnlySubQuery(Type typeOfBusinessObjectToSubQuery, SchemaColumn key)
#if DEBUG
			: base(typeOfBusinessObjectToSubQuery, Assembly.GetCallingAssembly())
#else
			: base(typeOfBusinessObjectToSubQuery, null)
#endif
		{
			this.Key = key;
		}

		public ZDBOnlySubQuery(Type typeOfBusinessObjectToSubQuery, SchemaColumn key, SchemaColumn parentReferenceKey)
#if DEBUG
			: base(typeOfBusinessObjectToSubQuery, Assembly.GetCallingAssembly())
#else
			: base(typeOfBusinessObjectToSubQuery, null)
#endif
		{
			this.Key = key;
			this.ParentReferenceKey = parentReferenceKey;
		}

		public ZDBOnlySubQuery(Type typeOfBusinessObjectToSubQuery, SchemaColumn key, bool notIn)
#if DEBUG
			: base(typeOfBusinessObjectToSubQuery, Assembly.GetCallingAssembly())
#else
			: base(typeOfBusinessObjectToSubQuery, null)
#endif
		{
			this.NotIn = notIn;
			this.Key = key;
		}

		public ZDBOnlySubQuery(Type typeOfBusinessObjectToSubQuery, SchemaColumn key, bool notIn, bool ignoreIn, bool ignoreSelectFromOuter)
#if DEBUG
			: base(typeOfBusinessObjectToSubQuery, Assembly.GetCallingAssembly())
#else
			: base(typeOfBusinessObjectToSubQuery, null)
#endif
		{
			this.NotIn = notIn;
			this.IgnoreIn = ignoreIn;
			this.IgnoreSelectFromOuter = ignoreSelectFromOuter;
			this.Key = key;
		}

		internal readonly SchemaColumn Key;
		internal readonly SchemaColumn ParentReferenceKey;
		internal readonly bool NotIn;
		internal readonly bool IgnoreIn;
		internal readonly bool IgnoreSelectFromOuter;

		internal ZDBOnlySubQuery ShallowCopy(SchemaColumn field, SchemaColumn subQueryField)
		{
			ZDBOnlySubQuery result = (ZDBOnlySubQuery)ShallowClone();
			ModificationsEnabled = false;
			result.Field = field;
			result.SubQueryField = subQueryField;
			return result;
		}

		protected override void ParameterisedSql(SqlBuilder sqlBuilder)
		{
			if (Field == null)
			{
				throw new InvalidOperationException("Cannot get ParameterisedSql from a " + nameof(ZDBOnlySubQuery) + " that hasn't been addded to a " + nameof(ZDBOnlyQuery));
			}
			AddSql(sqlBuilder, () => base.ParameterisedSql(sqlBuilder));
		}

		IFilterPart[] IFilterPart.GetSimplifiedVersion(JoinCondition lastJoinCondition)
		{
			FilterParts.Simplify();
			return new IFilterPart[] { this };
		}

		IEnumerable<IFilterPart> IFilterPart.FilterParts => FilterParts;

		protected internal override void AddLiteralTextADO(SqlBuilder sqlBuilder)
		{
			AddSql(sqlBuilder, () => base.AddLiteralTextADO(sqlBuilder));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Partial SQL")]
		void AddSql(SqlBuilder sqlBuilder, Action addInnerSql)
		{
			if (!IgnoreIn)
			{
				if (Field != null)
				{
					sqlBuilder.Append(Field.Name);
				}

				sqlBuilder.Append(NotIn ? " NOT IN (" : " IN (");
			}

			if (IgnoreSelectFromOuter)
			{
				addInnerSql();
			}
			else
			{
				var isNullable = SubQueryField != null && SubQueryField.IsNullable;

				sqlBuilder.Append("SELECT ").Append(SubQueryFieldName).Append(" FROM ").Append(AddSchemaName(TableName));
				AddTableHints(sqlBuilder);

				var applyIsNotNull = isNullable && (NotIn || ObjectFactory.Get<IEntityFrameworkSettings>().ApplyIsNotNullToJoinOnFK);

				bool applySuffixFilter = false;
				using (sqlBuilder.WithPrefix(arg =>
				{
					if (!(arg is string str && str.StartsWith(" union", StringComparison.OrdinalIgnoreCase)))
					{
						sqlBuilder.Append(" WHERE " + (applyIsNotNull ? SubQueryFieldName + " IS NOT NULL AND " : ""));
					}
				}))
				{
					addInnerSql();
					applySuffixFilter = sqlBuilder.HasPendingPrefix && applyIsNotNull;
				}

				if (applySuffixFilter)
				{
					sqlBuilder.Append(" WHERE ").Append(SubQueryFieldName).Append(" IS NOT NULL");
				}
			}

			if (!IgnoreIn)
			{
				sqlBuilder.Append(")");
			}
		}

		protected internal override bool FilterIsEmpty
		{
			get { return false; }
		}

#if DEBUG
		protected override string GetCSharpConstructor(string variableName)
		{
			string typeRetrieverLocation = "typeof(" + TypeOfBusinessObjectToQuery.FullName + ")";
			if (string.IsNullOrEmpty(typeRetrieverLocation))
			{
				typeRetrieverLocation = "typeof(" + TypeOfBusinessObjectToQuery.Namespace + "." + TypeOfBusinessObjectToQuery.Name + ")";
			}
			return "ZDBOnlySubQuery " + variableName + " = new ZDBOnlySubQuery(" + typeRetrieverLocation + ", " + Key.TableName + "Schema." + Key.Name + ");";
		}
#endif

		#region Equals / GetHashCode

		public override bool Equals(object obj)
		{
			ZDBOnlySubQuery rhs = obj as ZDBOnlySubQuery;
			bool result = rhs != null;

			result = result && base.Equals(rhs);
			result = result && Key == rhs.Key;
			result = result && NotIn == rhs.NotIn;
			result = result && IgnoreIn == rhs.IgnoreIn;
			result = result && IgnoreSelectFromOuter == rhs.IgnoreSelectFromOuter;
			return result;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode() ^ Key.GetHashCode();
		}

		#endregion

		#region Implementation

		SchemaColumn Field;
		SchemaColumn SubQueryField;

		internal string SubQueryFieldName
		{
			get { return SubQueryField != null ? SubQueryField.Name : ""; }
		}

		internal string FieldName
		{
			get { return Field != null ? Field.Name : ""; }
		}

		#endregion
	}
}
