using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.ComponentModel;

namespace CargoWise.EntityFramework
{
	public interface IDynamicBusinessObjectExtensionsInstance
	{
		(DynamicBusinessObjectProperty, string) GetPropertyByDescription(IDynamicBusinessObject dynamicBusinessObject, string fieldName, string type);
		(string, string) DescriptionsFor(DynamicBusinessObjectProperty property);
	}

	public interface IDynamicBusinessObject
	{
		string[] PropertyNames { get; }
		DynamicBusinessObjectProperty GetProperty(string propertyName);
	}

	public interface IDynamicBusinessObjectCollection
	{
		IDynamicBusinessObject Template { get; }
	}

	public class DynamicBusinessObjectProperty
	{
		public const int DefaultDecimalScale = 2;

		public DynamicBusinessObjectProperty(Type type, bool readOnly, bool visible = true, params DynamicMetaData[] metaData)
		{
			Type = type;
			ReadOnly = readOnly;
			MetaData = new ReadOnlyCollection<DynamicMetaData>(metaData);
			Visible = visible;
		}

		public DynamicMetaData GetMetaData(string metaDataTypeId)
		{
			return MetaData.FirstOrDefault(md => md.Id.Equals(metaDataTypeId, StringComparison.Ordinal));
		}

		public Type Type { get; }
		public bool ReadOnly { get; }
		public bool Visible { get; }
		public ReadOnlyCollection<DynamicMetaData> MetaData { get; }
	}

	public static class DynamicBusinessObjectExtensions
	{
		public static DynamicMetaData GetMetaData(this IDynamicBusinessObject bo, string name)
		{
			int sepIndex = name.IndexOf(PropertyDescriptorCollectionWithMetaData.MetaDataPropertyNameSeparator);
			if (sepIndex >= 0)
			{
				string propertyName = name.Substring(0, sepIndex);
				DynamicBusinessObjectProperty property = bo.GetProperty(propertyName);
				if (property != null)
				{
					string metaDataTypeId = name.Substring(sepIndex + PropertyDescriptorCollectionWithMetaData.MetaDataPropertyNameSeparator.Length);

					return property.GetMetaData(metaDataTypeId);
				}
			}

			return null;
		}
	}
}
