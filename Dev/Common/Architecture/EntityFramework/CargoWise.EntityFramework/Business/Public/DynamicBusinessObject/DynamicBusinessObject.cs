using System;
using System.Data;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// Dynamically generated based on query - cannot be stored in the database.
	/// </summary>
	[TestExcludeBusinessObjectsAllHaveTestCases]
	public class DynamicBusinessObject : BusinessObject, IDynamicBusinessObject
	{
		public DynamicBusinessObject(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override object this[string propertyName]
		{
			get
			{
				if (Table.Columns.Contains(propertyName))
				{
					return ZDataType.ObjectToZType(Row.Table.Columns[propertyName], Row[propertyName]);
				}
				else
				{
					PropertyInfo property = GetType().GetProperty(propertyName);
					if (property != null)
					{
						return property.GetValue(this, null);
					}
					else
					{
						throw new DynamicPropertyNotFoundException(propertyName);
					}
				}
			}
		}

		[Serializable]
		public class DynamicPropertyNotFoundException : Exception
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "developer error message")]
			public DynamicPropertyNotFoundException(string propertyName)
				: base("Cannot find property <" + propertyName + "> on DynamicBusinessObject. Check it is in your Query, or if you have inhertied from DynamicBusinessObject, a property of that class.")
			{
			}

#if NETFRAMEWORK
			protected DynamicPropertyNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif
		}

		internal override ZGuid GetPKInternal()
		{
			return ZGuid.Empty;
		}

		public override SchemaGuidColumn PKSchemaColumn
		{
			get { return CargoWise.Schema.Schema.GenericPkColumn; }
		}

		public string[] PropertyNames
		{
			get
			{
				string[] properties = new string[Row.Table.Columns.Count];
				foreach (DataColumn column in Row.Table.Columns)
				{
					properties[column.Ordinal] = column.ColumnName;
				}

				return properties;
			}
		}

		protected internal override void SetPKAndDefaults()
		{
			SetDefaultValues();
		}

		public override ZPropertyInfoHashtable ZPropertyInfoHash
		{
			get
			{
				if (fZPropertyInfoHash == null)
				{
					fZPropertyInfoHash = new ZDynamicPropertyInfoHashtable(this);
				}

				return fZPropertyInfoHash;
			}
		}

		protected override void AddToFactoryCache()
		{
			// don't call base - we don't know a PK for factory cache
		}

		protected override sealed bool EnableLightValidationIfAvailable
		{
			get { return false; }
		}

		ZPropertyInfoHashtable fZPropertyInfoHash;

		#region IDynamicBusinessObject

		DynamicBusinessObjectProperty IDynamicBusinessObject.GetProperty(string propertyName)
		{
			return new DynamicBusinessObjectProperty(this[propertyName].GetType(), true);
		}

		#endregion
	}
}
