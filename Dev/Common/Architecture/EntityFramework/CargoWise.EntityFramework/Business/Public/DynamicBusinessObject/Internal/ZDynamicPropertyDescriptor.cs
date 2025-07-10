using System;
using System.ComponentModel;
using CargoWise.ComponentModel;

namespace CargoWise.EntityFramework
{
	class ZDynamicPropertyDescriptor : KPropertyDescriptor
	{
		public ZDynamicPropertyDescriptor(DynamicBusinessObjectPropertyDescriptorCollection collection, string name, DynamicBusinessObjectProperty property)
			: base(collection, name, Array.Empty<Attribute>())
		{
			componentType = collection.ComponentType;
			propertyType = property.Type;
			readOnly = property.ReadOnly;
		}

		public ZDynamicPropertyDescriptor(DynamicBusinessObjectPropertyDescriptorCollection collection, string name, DynamicMetaData metaData)
			: base(collection, name, Array.Empty<Attribute>())
		{
			componentType = collection.ComponentType;
			propertyType = metaData.Value.GetType();
		}

		protected override object GetValueCore(object component)
		{
			return ((BusinessObject)component)[Name];
		}

		protected override void SetValueCore(object component, object value)
		{
			if (IsReadOnly)
			{
				throw new NotSupportedException();
			}
			else
			{
				((BusinessObject)component)[Name] = value;
			}
		}

		public override bool CanResetValue(object component)
		{
			return !IsReadOnly;
		}

		public override void ResetValue(object component)
		{
			((BusinessObject)component)[Name] = null;
		}

		public override Type ComponentType
		{
			get { return componentType; }
		}

		public override bool ShouldSerializeValue(object component)
		{
			return false;
		}

		public override bool IsReadOnly
		{
			get { return readOnly; }
		}

		public override Type PropertyType
		{
			get { return propertyType; }
		}

		public override TypeConverter Converter
		{
			get
			{
				TypeConverter result = TypeDescriptor.GetConverter(propertyType);

				return result;
			}
		}

		readonly Type componentType;
		readonly Type propertyType;
		readonly bool readOnly = true;
	}
}
