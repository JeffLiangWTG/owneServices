using System;
using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	internal sealed class BusinessObjectPropertyDescriptor : KPropertyDescriptor
	{
		public BusinessObjectPropertyDescriptor(BusinessObjectPropertyDescriptorCollection collection, PropertyDescriptor inner)
			: base(collection, inner)
		{
			InnerReflectPropertyDescriptor = inner as KReflectPropertyDescriptor;
			IsPropertyTypePersistentBusinessObjectExceptBusinessObject = typeof(BusinessObject) != PropertyType && typeof(BusinessObject).IsAssignableFrom(PropertyType) && !typeof(NonPersistentBusinessObject).IsAssignableFrom(PropertyType);
		}

		protected override object GetValueForMetaData(object component)
		{
			var result = base.GetValueForMetaData(component);

			if (result is IZTypeInternals zTypeInternal)
			{
				result = zTypeInternal.GetValueForLogicalDataLayer(false);
			}

			return result;
		}

		protected override object GetValueCore(object component)
		{
			object value = InnerReflectPropertyDescriptor != null ? InnerReflectPropertyDescriptor.GetValue(component) : base.GetValueCore(component);
			if (value == null && IsPropertyTypePersistentBusinessObjectExceptBusinessObject)
			{
				IBusiness business = component as IBusiness;
				if (business != null && business.Factory != null)
				{
					value = business.Factory.GetNull(PropertyType);
				}
			}
			return value;
		}

		protected override void SetValueCore(object component, object value)
		{
			if (!HasSetter())
			{
				throw new InvalidOperationException(GetType().Name + " is read-only <" + Name + ">.");
			}
			else
			{
				base.SetValueCore(component, value is DBNull ? null : value);
			}
		}

		public override PropertyDescriptorCollection GetChildProperties(object instance, Attribute[] filter)
		{
			return ZCustomTypeDescriptor.GetProperties(PropertyType);
		}

		#region Implementation

		readonly KReflectPropertyDescriptor InnerReflectPropertyDescriptor;
		readonly bool IsPropertyTypePersistentBusinessObjectExceptBusinessObject;

		#endregion
	}
}
