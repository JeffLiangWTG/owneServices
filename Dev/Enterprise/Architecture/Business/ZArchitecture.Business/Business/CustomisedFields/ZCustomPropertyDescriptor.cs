using System;
using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public interface IPropertyReadonlyOnComponentRetriver
	{
		bool IsReadOnlyOnComponent(object component);
	}

	public class ZCustomPropertyDescriptor : KPropertyDescriptor, IPropertyReadonlyOnComponentRetriver
	{
		public ZCustomPropertyDescriptor(string identifier, Type propertyType)
			: base(null, identifier, new Attribute[] { new BrowsableAttribute(false) })
		{
			this.propertyType = propertyType;
		}

		protected override object GetValueCore(object component)
		{
			ICustomPropertyContainer propertyContainer = GetCustomPropertyContainer(component);
			BusinessObject businessObject = propertyContainer as BusinessObject ?? component as BusinessObject;
			return propertyContainer != null ? propertyContainer.GetValue(businessObject, this.Name) : null;
		}

		protected override void SetValueCore(object component, object value)
		{
			ICustomPropertyContainer propertyContainer = GetCustomPropertyContainer(component);
			if (propertyContainer != null)
			{
				BusinessObject businessObject = propertyContainer as BusinessObject ?? component as BusinessObject;
				propertyContainer.TrySetValue(businessObject, this.Name, value); // Is there a problem here?
			}
		}

		public bool IsReadOnlyOnComponent(object component)
		{
			return IsReadOnlyOnComponentCore(component);
		}

		protected virtual bool IsReadOnlyOnComponentCore(object component)
		{
			ICustomPropertyContainer propertyContainer = GetCustomPropertyContainer(component);
			return propertyContainer == null || propertyContainer.IsReadonly(this.Name);
		}

		protected virtual ICustomPropertyContainer GetCustomPropertyContainer(object component)
		{
			return component as ICustomPropertyContainer;
		}

		public override Type PropertyType
		{
			get { return propertyType; }
		}
		readonly Type propertyType;

		public override TypeConverter Converter
		{
			get
			{
				object[] customAttributes = PropertyType.GetCustomAttributes(typeof(TypeConverterAttribute), true);
				if (customAttributes != null && customAttributes.Length > 0)
				{
					return (TypeConverter)Activator.CreateInstance(Type.GetType(((TypeConverterAttribute)customAttributes[0]).ConverterTypeName));
				}

				return base.Converter;
			}
		}
	}
}
