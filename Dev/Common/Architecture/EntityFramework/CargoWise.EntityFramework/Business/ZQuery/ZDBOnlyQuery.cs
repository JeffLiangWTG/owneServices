using System;
using System.Reflection;
using CargoWise.Schema;
using JC = CargoWise.EntityFramework.JoinCondition;

namespace CargoWise.EntityFramework
{
	public class ZDBOnlyQuery : ZQuery, IDbOnlyFilter
	{
		public ZDBOnlyQuery(Type typeOfBusinessObjectToQuery)
#if DEBUG
			: this(typeOfBusinessObjectToQuery, Assembly.GetCallingAssembly())
#else
			: this(typeOfBusinessObjectToQuery, null)
#endif
		{
		}

		protected ZDBOnlyQuery()
		{
		}

		protected ZDBOnlyQuery(Type typeOfBusinessObjectToQuery, Assembly callingAssembly)
		{
			this.TypeOfBusinessObjectToQuery = BusinessObjectFactory.GetConcreteBusinessObjectType(callingAssembly, typeOfBusinessObjectToQuery);
			Initialise(BusinessObjectFactory.GetTableNameFromType(this.TypeOfBusinessObjectToQuery), BusinessObjectFactory.GetPKColumnFromType(this.TypeOfBusinessObjectToQuery));
		}

		bool IDbOnlyFilter.IsDbOnlyFilter
		{
			get { return true; }
		}

		protected void Initialise(string tableName, SchemaPKColumn pkColumn)
		{
			TableName = tableName;
			PKColumn = pkColumn;
			AddUsedTable(TableName);
			IsDBOnlyQuery = true;
		}

		internal readonly Type TypeOfBusinessObjectToQuery;
		internal string TableName { get; private set; }
		public SchemaPKColumn PKColumn { get; private set; }

		public void AddAsUnionQuery(ZDBOnlySubQuery query, bool addAsUnionAll = false)
		{
			this.ContainsZDBOnlyUnionQuery = true;
			FilterParts.Append(new ZDBOnlyUnionQuery(query, addAsUnionAll));
		}

		public void AddSubQuery(ZDBOnlySubQuery subQuery, JC condition)
		{
			// eg, if ("JS_PK".StartsWith("JS_JC".SubString(0, 3)))
			// ie, PK and PK are both in the subquery table
			if (subQuery.PKColumn.Name.StartsWith(subQuery.Key.Name.Substring(0, 3)))
			{
				if (subQuery.ParentReferenceKey != null)
				{
					AddSubQuery(subQuery.ParentReferenceKey, subQuery.Key, subQuery, condition);
				}
				else
				{
					AddSubQuery(PKColumn, subQuery.Key, subQuery, condition);
				}
			}
			else if (subQuery.Key.Name.StartsWith(PKColumn.Name.Substring(0, 3))) // FK/Natural Key is in parent query's table
			{
				if (subQuery.ParentReferenceKey != null)
				{
					AddSubQuery(subQuery.Key, subQuery.ParentReferenceKey, subQuery, condition);
				}
				else
				{
					AddSubQuery(subQuery.Key, subQuery.PKColumn, subQuery, condition);
				}
			}
			else
			{
				throw new ApplicationException("Fields or Tables are not following CargoWise naming standards.");
			}
		}

		/// <summary>
		/// Add SubQuery in the following format: [FieldNameOverride] IN (SELECT [SubQuery])
		/// </summary>
		public void AddSubQuery(SchemaColumn fieldNameOverride, ZDBOnlySubQuery subQuery, JC condition)
		{
			AddSubQuery(fieldNameOverride, subQuery.Key, subQuery, condition);
		}

		/// <summary>
		/// Ultimate AddSubQuery - all AddSubQuery methods end up here
		/// </summary>
		/// <param name="fieldName"></param>
		/// <param name="subQueryFieldName"></param>
		/// <param name="subQuery"></param>
		/// <param name="condition"></param>
		public void AddSubQuery(SchemaColumn fieldName, SchemaColumn subQueryFieldName, ZDBOnlySubQuery subQuery, JC condition)
		{
			ZDBOnlySubQuery clonedSubQuery = subQuery.ShallowCopy(fieldName, subQueryFieldName);
			if (!IgnoreActiveFilter && !subQuery.IgnoreActiveFilter)
			{
				clonedSubQuery.AddToFilter(BusinessObject.GetActiveFilter(subQuery.TypeOfBusinessObjectToQuery), JC.And);
			}

			if (IsNoLock)
			{
				clonedSubQuery.IsNoLock = IsNoLock;
			}

			AddToFilter(clonedSubQuery, condition);
		}

#if DEBUG
		protected override string GetCSharpConstructor(string variableName)
		{
			string typeRetrieverLocation = "typeof(" + TypeOfBusinessObjectToQuery.FullName + ")";
			if (string.IsNullOrEmpty(typeRetrieverLocation))
			{
				typeRetrieverLocation = "typeof(" + TypeOfBusinessObjectToQuery.Namespace + "." + TypeOfBusinessObjectToQuery.Name + ")";
			}
			return "ZDBOnlyQuery " + variableName + " = new ZDBOnlyQuery(" + typeRetrieverLocation + ");";
		}
#endif

		#region Equals / GetHashCode

		public override bool Equals(object obj)
		{
			ZDBOnlyQuery rhs = obj as ZDBOnlyQuery;
			bool result = rhs != null;

			result = result && base.Equals(rhs);
			result = result && TypeOfBusinessObjectToQuery == rhs.TypeOfBusinessObjectToQuery;
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Required for Equals override")]
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#endregion
	}
}
