using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.ZArchitecture.Business
{
	public static class CustomPropertyExtensions
	{
		public static bool HasMatchingLegacyIdentifier(this ICustomProperty customProperty, string legacyIdentifier)
		{
			if (legacyIdentifier.Equals(customProperty.Identifier, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}

			var description = (IDescription)customProperty.Info.MetaData.FirstOrDefault(x => x.Id == MetaDataTypes.Description)?.Value;

			try
			{
				if (description != null)
				{
					if (!CustomPropertyHelper.ExtractPropertyNameAndTypeFromIdentifier(customProperty.Identifier, out var propertyAndName))
					{
						return false;
					}

					var propertyName = propertyAndName.PropertyName;
					var propertyType = customProperty.Info.Type;

					if (legacyIdentifier.Equals(CustomBusinessObject.GetLegacyIdentifier1(propertyName, propertyType), StringComparison.OrdinalIgnoreCase) ||
						legacyIdentifier.Equals(CustomBusinessObject.GetLegacyIdentifier2(propertyName, propertyType), StringComparison.OrdinalIgnoreCase) ||
						legacyIdentifier.Equals(CustomBusinessObject.GetLegacyIdentifier3(propertyName, propertyType), StringComparison.OrdinalIgnoreCase))
					{
						return true;
					}
				}
			}
			catch (ArgumentException)
			{
			}

			return false;
		}

		public static string GetPropertyName(this ICustomProperty customProperty)
		{
			var description = (IDescription)customProperty.Info.MetaData.FirstOrDefault(x => x.Id == MetaDataTypes.Description)?.Value;

			if (description != null)
			{
				return description.GetDescription(0, CultureInfo.InvariantCulture);
			}
			else
			{
				return customProperty.Identifier;
			}
		}

		public static ICustomProperty Create(this CustomPropertyCollection customPropertyCollection, Type type, string identifier, params DynamicMetaData[] metaData)
		{
			return new CustomPropertyImpl(customPropertyCollection, type, identifier, false, null, null, metaData);
		}

		public static ICustomProperty Create(this CustomPropertyCollection customPropertyCollection, Type type, string identifier, bool readOnly, params DynamicMetaData[] metaData)
		{
			return new CustomPropertyImpl(customPropertyCollection, type, identifier, readOnly, null, null, metaData);
		}

		public static ICustomProperty Create(this CustomPropertyCollection customPropertyCollection, Type type, string identifier, Action<ZPropertyInfo> validator, params DynamicMetaData[] metaData)
		{
			return new CustomPropertyImpl(customPropertyCollection, type, identifier, false, validator, null, metaData);
		}

		public static ICustomProperty Create(this CustomPropertyCollection customPropertyCollection, Type type, string identifier, Action<ZPropertyInfo> validator, Action<BusinessObject, string, object, ZGuid> onSet, params DynamicMetaData[] metaData)
		{
			return new CustomPropertyImpl(customPropertyCollection, type, identifier, false, validator, onSet, metaData);
		}

		public static ICustomProperty Create(this CustomPropertyCollection customPropertyCollection, Type type, string identifier, Action<ZPropertyInfo> validator, Action<BusinessObject, string, object, ZGuid> onSet, Func<IEnumerable<ICustomProperty>> relatedProperties, params DynamicMetaData[] metaData)
		{
			return new CustomPropertyImpl(customPropertyCollection, type, identifier, false, validator, onSet, metaData, relatedProperties);
		}

		sealed class CustomPropertyImpl : ICustomProperty
		{
			readonly Func<IEnumerable<ICustomProperty>> relatedProperties;

			public CustomPropertyImpl(
				CustomPropertyCollection parent,
				Type type,
				string identifier,
				bool readOnly,
				Action<ZPropertyInfo> validator,
				Action<BusinessObject, string, object, ZGuid> onSet,
				DynamicMetaData[] metaData,
				Func<IEnumerable<ICustomProperty>> relatedProperties = null)
			{
				this.CustomColumnDefinition = null;
				this.parent = parent;
				Identifier = identifier;
				Info = new DynamicBusinessObjectProperty(type, readOnly, metaData: metaData);
				this.validator = validator;
				this.onSet = onSet;
				Func<IEnumerable<ICustomProperty>> defaultRelatedProperties = () => Enumerable.Empty<ICustomProperty>();
				this.relatedProperties = relatedProperties ?? defaultRelatedProperties;
			}

			public string Identifier { get; private set; }
			public DynamicBusinessObjectProperty Info { get; private set; }
			public IEnumerable<ICustomProperty> RelatedProperties => relatedProperties.Invoke();
			public ICustomColumnDefinition CustomColumnDefinition { get; }
			public bool IsDeleted => false;

			public object GetValue(BusinessObject cusObj)
			{
				return parent.GetValue(cusObj, Identifier);
			}

			public bool TrySetValue(BusinessObject cusObj, object value)
			{
				if (parent.TrySetValue(cusObj, Identifier, value))
				{
					onSet?.Invoke(cusObj, Identifier, value, ZGuid.Empty);
					return true;
				}
				return false;
			}

			public void Validate(BusinessObject cusObj)
			{
				validator?.Invoke(((CustomBusinessObject)cusObj).GetZPropertyInfo(Identifier));
			}

			readonly Action<ZPropertyInfo> validator;
			readonly Action<BusinessObject, string, object, ZGuid> onSet;
			readonly CustomPropertyCollection parent;
		}
	}
}
