using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	[Serializable]
	public class PermitTypePartItem : AutoPermitTypePartItem, IJsonSerializable
	{
		public PermitTypePartItem(string column, ZString code, ZString description)
			: base(code, description)
		{
			Argument.NotNullOrEmpty(column, "column");
			Argument.NotNullOrEmpty(code, "code");

			Column = column;
		}

		public readonly string Column;

		#region Included

		public void RecursivelySetIncluded(bool value)
		{
			Include = value;
			SubItemsCollection.RecursivelySetIncluded(value);
		}

		#endregion

		#region Where Clause

		SqlParameter CodeParam
		{
			get
			{
				if (codeParam == null)
				{
					codeParam = new SqlParameter(SqlParameterNameGenerator.Next(), SqlDbType.VarChar);
					codeParam.Value = Code.ToString();
				}

				return codeParam;
			}
		}

		SqlParameter codeParam;

		string WhereClause
		{
			get
			{
				if (whereClause == null)
				{
					whereClause = string.Format(CultureInfo.InvariantCulture, "{0} = {1}", Column, CodeParam);
				}

				return whereClause;
			}
		}
		string whereClause;

		public string BuildFullWhereClause(SqlParameterList parameterListToAppendTo)
		{
			var result = new StringBuilder();
			result.Append(WhereClause);
			parameterListToAppendTo.Add(CodeParam);

			if (SubItemsCollection.HasIncludedItems)
			{
				result.Append((NoResString)" AND (" + SubItemsCollection.BuildFullWhereClause(parameterListToAppendTo) + (NoResString)")");
			}

			return result.ToString();
		}

		#endregion

		#region SubItems

		public PermitTypePartItemCollection SubItemsCollection
		{
			get
			{
				if (subItemsCollection == null)
				{
					SetSubItemsCollection(new PermitTypePartItemCollection());
				}

				return subItemsCollection;
			}
		}

		void SetSubItemsCollection(PermitTypePartItemCollection value)
		{
			if (subItemsCollection != null)
			{
				((IBindingList)subItemsCollection).ListChanged -= PermitTypePartItem_ListChanged;
			}

			subItemsCollection = value;

			if (subItemsCollection != null)
			{
				((IBindingList)subItemsCollection).ListChanged += PermitTypePartItem_ListChanged;
			}
		}

		PermitTypePartItemCollection subItemsCollection;

		void PermitTypePartItem_ListChanged(object sender, ListChangedEventArgs e)
		{
			if (!Include && SubItemsCollection.Cast<PermitTypePartItem>().Any(x => x.Include))
			{
				Include = true;
			}
		}

		#endregion

		#region Constructor For IJsonSerializable

		internal PermitTypePartItem(PermitTypePartItemJsonData data)
			: base(data.Code, data.Description)
		{
			Column = data.Column;
			Include = data.Include;
			SetSubItemsCollection(new PermitTypePartItemCollection(data.SubItemsCollection));
		}

		#endregion

		#region IJsonSerializable Members

		public object GetJsonData() =>
			new PermitTypePartItemJsonData
			{
				Column = Column,
				Include = Include,
				Code = Code,
				Description = Description,
				SubItemsCollection = (PermitTypePartItemCollectionJsonData)SubItemsCollection?.GetJsonData()
			};

		#endregion
	}
}

