using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.ComponentModel;

namespace CargoWise.EntityFramework
{
	internal sealed class DynamicBusinessObjectPropertyDescriptorCollection : BusinessObjectPropertyDescriptorCollection
	{
		public DynamicBusinessObjectPropertyDescriptorCollection(IDynamicBusinessObject templateObject)
			: base(templateObject.GetType(), false)
		{
			PopulatePropertyDescriptors();

			RefreshDynamicPropertyDescriptorCollectionFromBusinessObject(templateObject);
		}

		public void RefreshDynamicPropertyDescriptorCollectionFromBusinessObject(IDynamicBusinessObject templateObject)
		{
			foreach (string propertyName in templateObject.PropertyNames)
			{
				var existingPropertyOnObject = this[propertyName];
				if (existingPropertyOnObject == null)
				{
					AddDynamicPropertyDescriptor(templateObject, propertyName);
				}
			}
		}

		void AddDynamicPropertyDescriptor(IDynamicBusinessObject templateObject, string name)
		{
			DynamicBusinessObjectProperty property = templateObject.GetProperty(name);
			dynamicProperties.Add(name, property);
			if (property != null)
			{
				Add(new ZDynamicPropertyDescriptor(this, name, property));
			}
			else
			{
				DynamicMetaData metaData = templateObject.GetMetaData(name);
				if (metaData != null)
				{
					Add(new ZDynamicPropertyDescriptor(this, name, metaData));
				}
			}
		}

		protected override PropertyDescriptor FindOrCreatePropertyNotInCollection(string name, bool ignoreCase)
		{
			DynamicMetaData metaData = GetMetaData(name);
			if (metaData != null && metaData.Value != null)
			{
				return new ZDynamicPropertyDescriptor(this, name, metaData);
			}

			return base.FindOrCreatePropertyNotInCollection(name, ignoreCase);
		}

		public override KPropertyDescriptorCollection AllProperties
		{
			get { return this; }
		}

		protected override KPropertyDescriptorCollection NewEmptyPropertyDescriptorCollectionCore(Type componentType, bool includePrivate)
		{
			throw new NotSupportedException("There is no need to create new DynamicBusinessObjectPropertyDescriptorCollection for same IDynamicBusinessObject.");
		}

		DynamicMetaData GetMetaData(string name)
		{
			int sepIndex = name.IndexOf(PropertyDescriptorCollectionWithMetaData.MetaDataPropertyNameSeparator);
			if (sepIndex >= 0)
			{
				string propertyName = name.Substring(0, sepIndex);
				DynamicBusinessObjectProperty property;
				if (dynamicProperties.TryGetValue(propertyName, out property))
				{
					string metaDataTypeId = name.Substring(sepIndex + PropertyDescriptorCollectionWithMetaData.MetaDataPropertyNameSeparator.Length);
					return property.GetMetaData(metaDataTypeId);
				}
			}

			return null;
		}
		readonly Dictionary<string, DynamicBusinessObjectProperty> dynamicProperties = new Dictionary<string, DynamicBusinessObjectProperty>();
	}
}
