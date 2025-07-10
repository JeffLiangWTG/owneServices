using System;
using System.ComponentModel;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	internal class ZMaxLengthDataPropertyDescriptor : KPropertyDescriptor
	{
		public ZMaxLengthDataPropertyDescriptor(
			KPropertyDescriptorCollection collection, string propertyName, PropertyDescriptor accessorProperty, PropertyDescriptor innerMetaDataProperty)
			: base(collection, propertyName, null)
		{
			this.accessorProperty = accessorProperty;
			this.innerMetaDataProperty = innerMetaDataProperty;
			this.name = propertyName;
		}

		public override string Name
		{
			get { return name; }
		}
		readonly string name;

		public override Type PropertyType
		{
			get { return typeof(int); }
		}

		protected override sealed object GetValueCore(object component)
		{
			int result = innerMetaDataProperty == null ? -1 : (int)innerMetaDataProperty.GetValue(component);
			if (result == -1)
			{
				var businessObject = component as BusinessObject;
				if (!object.ReferenceEquals(businessObject, null))
				{
					result = GetMaxLengthFromInnerWrappedProperty(businessObject);
					if (result == -1)
					{
						result = GetMaxLengthFromSchemaColumn(businessObject);
					}
					if (result == -1)
					{
						result = GetMaxLengthFromAddInfoSchemaColumn(businessObject);
					}
				}
			}

			return result;
		}

		protected override bool HasSetterCore()
		{
			return false;
		}

		#region Implementation

		readonly PropertyDescriptor accessorProperty;
		readonly PropertyDescriptor innerMetaDataProperty;

		int GetMaxLengthFromSchemaColumn(BusinessObject businessObject)
		{
			if (maxLengthFromSchemaColumn == null)
			{
				var column = (object)businessObject == null ? null : ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumnSafe(accessorProperty.Name, businessObject.TableName);
				string tablePrefix = column == null ? string.Empty : CargoWise.Schema.Schema.GetPrefixFromColumnName(column.Name);
				string ptyDescriptorPrefix = CargoWise.Schema.Schema.GetPrefixFromColumnName(Name);
				maxLengthFromSchemaColumn = -1;
				if (column != null && businessObject.Row != null && tablePrefix == ptyDescriptorPrefix)
				{
					maxLengthFromSchemaColumn = column.MaxLength;
				}
			}
			return (int)maxLengthFromSchemaColumn;
		}
		int? maxLengthFromSchemaColumn;

		int GetMaxLengthFromAddInfoSchemaColumn(BusinessObject businessObject)
		{
			if (maxLengthFromAddInfoSchemaColumn == null)
			{
				if (businessObject is IAddInfoSchemaProvider addInfoSchemaProvider)
				{
					var column = addInfoSchemaProvider.AddInfoTableSchema.GetSchemaColumn(accessorProperty.Name);
					maxLengthFromAddInfoSchemaColumn = column?.MaxLength ?? -1;
				}
				else
				{
					maxLengthFromAddInfoSchemaColumn = -1;
				}
			}
			return maxLengthFromAddInfoSchemaColumn.Value;
		}
		int? maxLengthFromAddInfoSchemaColumn;

		int GetMaxLengthFromInnerWrappedProperty(BusinessObject businessObject)
		{
			int result = -1;
			if (!inGetMaxLengthFromInnerWrappedProperty && hasWrappedPropertyInfo)
			{
				inGetMaxLengthFromInnerWrappedProperty = true;
				try
				{
					ZWrappedPropertyInfo wrappedPropertyInfo = businessObject.ZPropertyInfoHash.GetPropertySafe(accessorProperty.Name) as ZWrappedPropertyInfo;
					if (wrappedPropertyInfo != null && wrappedPropertyInfo.InnerInfo != null)
					{
						result = wrappedPropertyInfo.InnerInfo.MaxLength;
					}
					else if (wrappedPropertyInfo != null)
					{
						result = -1;
					}
					else
					{
						hasWrappedPropertyInfo = false;
					}
				}
				finally
				{
					inGetMaxLengthFromInnerWrappedProperty = false;
				}
			}
			return result;
		}
		bool inGetMaxLengthFromInnerWrappedProperty;

		bool hasWrappedPropertyInfo = true;

		#endregion
	}
}
