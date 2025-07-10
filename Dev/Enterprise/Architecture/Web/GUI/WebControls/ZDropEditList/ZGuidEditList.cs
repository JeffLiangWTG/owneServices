using System;
using System.ComponentModel;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// A Drop Edit List that displays description for Guid objects.
	/// </summary>
	public class ZGuidEditList : ZDropEditList
	{
		#region Control Overrides

		#region Selected value

		protected override IZType GetSelectedValue()
		{
			IBusinessObjectCollection collection = ListBoxControl.DataSource as IBusinessObjectCollection;
			if (collection != null && !string.IsNullOrWhiteSpace(TextBoxControl.Text))
			{
				// string TableName = BusinessObjectFactory.GetTableNameFromType(Collection.TypeOfElements);

				BusinessObject[] matches = collection.Find(GetFilter(collection.TypeOfElements));
				if (matches.Length > 0)
				{
					return matches[0].PK;
				}
				else
				{
					return ZGuid.Invalid;
				}
			}
			return null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "property name should not be translated")]
		protected override string GetTextFromValue(IZType newValue)
		{
			if (newValue is ZGuid)
			{
				ZGuid pK = new ZGuid(newValue);
				IBusinessObjectCollection collection = ListBoxControl.DataSource as IBusinessObjectCollection;
				if (collection != null)
				{
					BusinessObject match = collection.FindByPK(pK);
					if (match != null)
					{
						ZString newText = match is ICodeDescription ? new ZString(((ICodeDescription)match).Code) : (ZString)ZPropertyAccessor.Get(match, "Code");
						return newText;
					}
				}
			}
			return null;
		}

		#endregion

		#endregion Control Overrides

		#region BindTo

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindTo))]
		public override string BindTo
		{
			get { return fBindTo; }
			set { fBindTo = value; }
		}
		string fBindTo = "";

		#endregion BindTo 

		#region Implementation

		protected internal ZQuery GetFilter(Type type)
		{
			ZQuery result = new ZQuery();
			try
			{
				string tableName = BusinessObjectFactory.GetTableNameFromType(type);
				string propertyName = CodePropertyAttribute.CodePropertyNameFromType(type);
				SchemaColumn schemaColumn = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumnSafe(propertyName, tableName) as SchemaStringColumn;
				if (schemaColumn != null)
				{
					result.AddToFilter(schemaColumn, TextBoxControl.Text);
				}
			}
			catch (NoCodePropertyException)
			{
				result.IsNoResultQuery = ZBool.True;
			}
			return result;
			// return new ZQuery(EnterpriseSchema.GetSchemaColumn("Code", TableName), SQLComparisonOperator.Equal, TextBoxControl.Text); 
		}

		#endregion Implementation
	}
}
