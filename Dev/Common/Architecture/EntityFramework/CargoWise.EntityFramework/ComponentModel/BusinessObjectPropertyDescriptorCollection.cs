using System;
using System.ComponentModel;
using CargoWise.ComponentModel;

namespace CargoWise.EntityFramework
{
	public class BusinessObjectPropertyDescriptorCollection : PropertyDescriptorCollectionWithWrappingProperties
	{
		#region Factory Method

		protected internal BusinessObjectPropertyDescriptorCollection(Type componentType, bool includePrivate)
			: base(componentType, includePrivate)
		{
		}

		public new static BusinessObjectPropertyDescriptorCollection FromType(Type componentType)
		{
			return FromType(componentType, false);
		}

		public new static BusinessObjectPropertyDescriptorCollection FromType(Type componentType, bool includePrivate)
		{
			return factory.FromType(componentType, includePrivate);
		}

		[Common.Testing.SuppressThreadStaticFieldMessage]
		static readonly PropertyDescriptorCollectionFactory<BusinessObjectPropertyDescriptorCollection> factory = new PropertyDescriptorCollectionFactory<BusinessObjectPropertyDescriptorCollection>(delegate(Type componentType)
				{
					return new BusinessObjectPropertyDescriptorCollection(componentType, false);
				});

		#endregion

		#region GetPropertyDescriptorCollectionForComponentType / NewEmptyPropertyDescriptorCollection

		protected override KPropertyDescriptorCollection GetPropertyDescriptorCollectionForComponentTypeCore(Type componentType, bool includePrivate)
		{
			return FromType(componentType, includePrivate);
		}

		protected override KPropertyDescriptorCollection NewEmptyPropertyDescriptorCollectionCore(Type componentType, bool includePrivate)
		{
			return new BusinessObjectPropertyDescriptorCollection(componentType, includePrivate);
		}

		#endregion

		protected override PropertyDescriptor GetInnerPropertyDescriptorForWrapped(PropertyDescriptor outer, string innerName, bool ignoreCase)
		{
			PropertyDescriptorCollection children = BusinessObjectPropertyDescriptorCollection.FromType(outer.PropertyType, IncludePrivate);
			return children.Find(innerName, ignoreCase);
		}

		protected override WrappingPropertyDescriptor NewWrappingPropertyDescriptor(PropertyDescriptorCollectionWithWrappingProperties collection, PropertyDescriptor outer, PropertyDescriptor inner, string name)
		{
			WrappingPropertyDescriptor result;

			string wrappingPropertyName = outer.Name + "+" + inner.Name;
			if (typeof(ZPropertyInfo).IsAssignableFrom(inner.PropertyType) && typeof(BusinessObject).IsAssignableFrom(outer.ComponentType))
			{
				result = new ZPropertyInfoWrappingPropertyDescriptor(this, outer, inner, wrappingPropertyName);
			}
			else
			{
				result = new WrappingPropertyDescriptor(this, outer, inner, wrappingPropertyName);
			}

			return result;
		}

		protected override PropertyDescriptor NewMetaDataPropertyCore(PropertyDescriptor property, string metaDataTypeId, string metaDataPropertyName, PropertyDescriptor metaDataAccessorFromAttributes)
		{
			PropertyDescriptor result = base.NewMetaDataPropertyCore(property, metaDataTypeId, metaDataPropertyName, metaDataAccessorFromAttributes);
			if (metaDataTypeId == MetaDataTypes.ReadOnly)
			{
				result = new ZIsReadOnlyPropertyDescriptor(this, result.Name, property, metaDataAccessorFromAttributes);
			}
			else if (metaDataTypeId == MetaDataTypes.MaxLength)
			{
				result = new ZMaxLengthDataPropertyDescriptor(this, metaDataPropertyName, property, result);
			}
			else if (metaDataTypeId == MetaDataTypes.Notifications)
			{
				result = new ZNotificationsPropertyDescriptor(this, metaDataPropertyName, property.Name);
			}
			return result;
		}

		protected override bool SupportsStraightThroughMetaDataAccessor(PropertyDescriptor property, string member, string metaDataTypeId)
		{
			return !(metaDataTypeId == MetaDataTypes.ReadOnly || metaDataTypeId == MetaDataTypes.MaxLength) && base.SupportsStraightThroughMetaDataAccessor(property, member, metaDataTypeId);
		}

		protected override PropertyDescriptor NewPropertyDescriptor(PropertyDescriptor reflectedProperty)
		{
			return new BusinessObjectPropertyDescriptor(this, reflectedProperty);
		}
	}
}
