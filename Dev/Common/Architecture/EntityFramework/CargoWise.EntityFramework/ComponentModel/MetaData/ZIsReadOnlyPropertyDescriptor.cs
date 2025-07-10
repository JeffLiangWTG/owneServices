using System.ComponentModel;
using CargoWise.ComponentModel;

namespace CargoWise.EntityFramework
{
	internal class ZIsReadOnlyPropertyDescriptor : IsReadOnlyPropertyDescriptor
	{
		public ZIsReadOnlyPropertyDescriptor(KPropertyDescriptorCollection collection, string propertyName, PropertyDescriptor propertyToBeReadOnly, PropertyDescriptor innerMetaDataProperty)
			: base(collection, propertyName, propertyToBeReadOnly, innerMetaDataProperty)
		{
		}

		protected override object GetValueCore(object component)
		{
			bool result = !PropertyToBeReadOnly.HasSetter();
			BusinessObject businessObject = component as BusinessObject;
			if (!result)
			{
				if (InnerMetaDataProperty != null && (object)businessObject != null)
				{
					result =
						IsBusinessObjectReadOnlyIncludingWrapped(InnerMetaDataProperty, businessObject) ||
						(bool)businessObject.GetObsoletePropertyInfoMetaData(MetaDataTypes.ReadOnly, PropertyToBeReadOnly.Name) ||
						Convert(InnerMetaDataProperty.GetValue(component));
				}
				else
				{
					result = (bool)base.GetValueCore(component);
					if (!result && (object)businessObject != null)
					{
						result =
							IsBusinessObjectReadOnlyIncludingWrapped(PropertyToBeReadOnly, businessObject) ||
							(bool)businessObject.GetObsoletePropertyInfoMetaData(MetaDataTypes.ReadOnly, PropertyToBeReadOnly.Name) ||
							GetReadOnlyFromInnerWrappedProperty(businessObject);
					}
				}
			}
			return result;
		}

		static bool IsBusinessObjectReadOnlyIncludingWrapped(PropertyDescriptor property, BusinessObject businessObject)
		{
			bool result = false;
			if ((object)businessObject != null)
			{
				result = businessObject.ReadOnly;
				var wrappingProperty = property as IWrappingPropertyDescriptor;
				if (!result && wrappingProperty != null)
				{
					BusinessObject outerBusinessObject = wrappingProperty.Outer.GetValue(businessObject) as BusinessObject;
					result = IsBusinessObjectReadOnlyIncludingWrapped(wrappingProperty.Outer, outerBusinessObject);
				}
			}
			return result;
		}

		bool GetReadOnlyFromInnerWrappedProperty(BusinessObject businessObject)
		{
			bool result = false;
			if (!inGetReadOnlyFromInnerWrappedProperty && hasWrappedPropertyInfo)
			{
				inGetReadOnlyFromInnerWrappedProperty = true;
				try
				{
					ZWrappedPropertyInfo wrappedPropertyInfo = businessObject.ZPropertyInfoHash.GetPropertySafe(PropertyToBeReadOnly.Name) as ZWrappedPropertyInfo;
					if (wrappedPropertyInfo != null && wrappedPropertyInfo.InnerInfo != null)
					{
						result = wrappedPropertyInfo.InnerInfo.ReadOnly;
					}
					else if (wrappedPropertyInfo != null)
					{
						result = true;
					}
					else
					{
						hasWrappedPropertyInfo = false;
					}
				}
				finally
				{
					inGetReadOnlyFromInnerWrappedProperty = false;
				}
			}
			return result;
		}
		bool inGetReadOnlyFromInnerWrappedProperty;

		bool hasWrappedPropertyInfo = true;
	}
}
