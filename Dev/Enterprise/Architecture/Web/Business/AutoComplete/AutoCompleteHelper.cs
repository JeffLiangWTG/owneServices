using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.Business
{
	public abstract class AutoCompleteHelper
	{
		public AutoCompleteHelper(BusinessObjectFactory factory)
		{
			this.factory = factory ?? new BusinessObjectFactory();
		}

		#region Public Methods

		public IZType GetKey(ZString text)
		{
			object key = null;

			if (!text.IsEmpty)
			{
				ZQuery filter = GetKeyFilter(text);

				BusinessObject bizO = Factory.LoadTop1(BusinessObjectType, filter);

				if (bizO != null)
				{
					key = (IZType)ZPropertyAccessor.Get(bizO, KeyColumn.ObjectName);
				}
			}

			switch (KeyColumn.ColumnType)
			{
				case SchemaColumnType.Guid:
					return (ZGuid)(key ?? ZGuid.Empty);

				case SchemaColumnType.String:
					return (ZString)(key ?? ZString.Empty);

				default:
					throw new NotImplementedException("Unexpected column type: " + KeyColumn.ColumnType.ToString());
			}
		}

		public ZString GetText(IZType key)
		{
			if (!key.IsEmpty)
			{
				ZQuery filter = GetTextFilter(key);

				BusinessObject bizO = Factory.LoadTop1(BusinessObjectType, filter);
				if (bizO != null)
				{
					return (ZString)ZPropertyAccessor.Get(bizO, TextColumn.ObjectName);
				}
			}
			return ZString.Empty;
		}

		public List<string> GetList(string textToSearch)
		{
			var res = new List<string>();
			ZQuery filter = GetListFilter(textToSearch);
			if (!filter.IsNoResultQuery)
			{
				filter.OrderBy = TextColumn.Name;

				string sql = GetSQLSelectClause() + filter.GetAsWhereAndOrderByClause(false);

				DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(Factory);
				collection.Load(sql, filter.Params);

				for (int i = 0; i < collection.Count; i++)
				{
					res.Add(GetTextFromBusinessObject(collection[i]));
				}
			}

			return res;
		}

		public virtual string SerializeAdditionalParamsToString()
		{
			return string.Empty;
		}

		public virtual void RestoreAdditionalParamsFromSerializedString(string serializedParamsString)
		{
		}

		public virtual ZGuid GetPKFromCode(ZString code)
		{
			if (CodeColumn != null)
			{
				ZQuery filter = new ZQuery();
				filter.AddToFilter(CodeColumn, code);

				BusinessObject bizO = Factory.LoadTop1(BusinessObjectType, filter);

				if (bizO != null)
				{
					return (ZGuid)ZPropertyAccessor.Get(bizO, bizO.PKSchemaColumn.ObjectName);
				}
				return ZGuid.Empty;
			}
			else
			{
				throw new NotSupportedException("Search by code doesn't supported or implemented for " + GetType().Name);
			}
		}

		#endregion

		#region Public Properties

		public bool UsesMultiRowOptions
		{
			get { return ColumnsToSelectForListFilter().Length > 0; }
		}

		public bool UsesGuidKey
		{
			get { return KeyColumn is SchemaPKColumn; }
		}

		public int MaxOptionsCount
		{
			get { return maxOptionsCount; }
			set { maxOptionsCount = value; }
		}
		int maxOptionsCount = 10;

		#endregion

		#region Abstract Members

		protected abstract SchemaColumn TextColumn { get; }
		protected abstract SchemaColumn KeyColumn { get; }
		protected abstract Type BusinessObjectType { get; }

		#endregion

		#region Implementation

		protected virtual SchemaColumn CodeColumn
		{
			get { return null; }
		}

		protected virtual ZQuery GetKeyFilter(ZString text)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(TextColumn, GetTextColumnFilterValue(text));
			return filter;
		}

		protected virtual ZQuery GetTextFilter(IZType key)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(KeyColumn, key);
			return filter;
		}

		protected virtual ZQuery GetListFilter(ZString textToSearch)
		{
			ZDBOnlyQuery filter = new ZDBOnlyQuery(BusinessObjectType);
			filter.AddToFilter(TextColumn, SQLComparisonOperator.StartsWith, GetTextColumnFilterValue(textToSearch));
			return filter;
		}

		protected ZString GetTextColumnFilterValue(ZString text) => TextColumn.HasMaxLength ? text.SubstringSafe(0, TextColumn.MaxLength) : text;

		string GetTextFromBusinessObject(DynamicBusinessObject bizO)
		{
			SchemaColumn[] columns = PropertiesForList;
			if (columns.Length == 1)
			{
				return bizO[columns[0].Name].ToString();
			}
			else
			{
				StringBuilder sb = new StringBuilder();
				foreach (SchemaColumn column in columns)
				{
					sb.AppendLine(bizO[column.Name].ToString());
				}
				return sb.ToString();
			}
		}

		protected SchemaColumn[] KeyColumnsToSelectForListFilter()
		{
			return new SchemaColumn[] { KeyColumn, TextColumn };
		}

		protected virtual SchemaColumn[] ColumnsToSelectForListFilter()
		{
			return Array.Empty<SchemaColumn>();
		}

		protected virtual SchemaColumn[] PropertiesForList
		{
			get
			{
				if (fPropertiesForList == null)
				{
					fPropertiesForList = new List<SchemaColumn>();
					foreach (SchemaColumn sc in KeyColumnsToSelectForListFilter())
					{
						fPropertiesForList.Add(sc);
					}
					foreach (SchemaColumn sc in ColumnsToSelectForListFilter())
					{
						fPropertiesForList.Add(sc);
					}
				}
				return fPropertiesForList.ToArray();
			}
		}
		List<SchemaColumn> fPropertiesForList;

		protected BusinessObjectFactory Factory
		{
			get
			{
				return factory;
			}
		}
		readonly BusinessObjectFactory factory;

		string GetSQLSelectClause()
		{
			List<string> columnsNames = new List<string>();
			foreach (SchemaColumn column in PropertiesForList)
			{
				columnsNames.Add(column.Name);
			}

			return string.Format("select top {0} {1} from {2} ",
				MaxOptionsCount,
				string.Join(", ", columnsNames.ToArray()),
				BusinessObjectFactory.GetTableNameFromType(BusinessObjectType));
		}

		#endregion

	}
}
