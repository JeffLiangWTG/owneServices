using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentVisualizer.Business
{
	sealed class DataObjectBuilder : IDataOverrideProvider
	{
		public DataObjectBuilder(string uxmlNamespace, IDynamicData dynamicData)
		{
			Argument.NotNullOrEmpty(uxmlNamespace, nameof(uxmlNamespace));
			Argument.NotNull(dynamicData, nameof(dynamicData));

			this.uxmlNamespace = uxmlNamespace;
			this.dynamicData = dynamicData;
		}

		readonly string uxmlNamespace;
		readonly IDynamicData dynamicData;

		public IDataObject Build()
		{
			return CreateDataObject(dynamicData)
				?? (IDataObject)Activator.CreateInstance(dynamicData.Type);
		}

		readonly ITypeDescriptorCache typeDescriptorCache = new TypeDescriptorCache();

		#region Implementation

		IDataObject CreateDataObject(IDynamicData data, Type type = null)
		{
			var dataType = type ?? data.Type;

			var dataObject = (IDataObject)Activator.CreateInstance(dataType);
			var isEmpty = true;

			var typeDescriptor = typeDescriptorCache.Get(dataType);
			var propertyNames = new HashSet<string>(typeDescriptor.PropertyNames.Concat(data.Properties.Select(p => p.Key)));

			foreach (var propertyName in propertyNames)
			{
				var propertyDescriptor = typeDescriptor.GetPropertyDescriptor(propertyName);
				var propertyValue = data.GetDynamicProperty(propertyName);

				if (propertyValue == null)
				{
					continue;
				}

				// custom field
				if (propertyValue.IsAdded())
				{
					if (!propertyValue.GetMetaData<bool>(MetaDataType.IsLocal))
					{
						MapPropertyOverride(dataObject, propertyName, propertyValue);
						isEmpty = false;
					}

					continue;
				}

				if (propertyDescriptor == null)
				{
					continue;
				}

				// simple property
				if (propertyValue.IsValueType())
				{
					if (propertyValue.OriginalValue != null)
					{
						propertyDescriptor.SetValue(dataObject, propertyValue.OriginalValue);
						isEmpty = false;
					}

					if (propertyValue.IsOverridden)
					{
						MapPropertyOverride(dataObject, propertyName, propertyValue);
						isEmpty = false;
					}

					continue;
				}

				var propertyType = propertyDescriptor.Type;

				if (propertyType.IsInterface)
				{
					if (!NamespaceDependentAttribute.TryGetType(propertyDescriptor.PropertyInfo, uxmlNamespace, out propertyType))
					{
						throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Unmapped type: {0}", propertyType.Name));
					}
				}

				var value = CreateComplexPropertyValue(propertyValue, propertyType);

				if (value != null)
				{
					propertyDescriptor.SetValue(dataObject, value);
					isEmpty = false;
				}
			}

			if (!isEmpty)
			{
				if (dataObject is ITopLevelDataObject topLevelDataObject)
				{
					CopyDataContext(topLevelDataObject, data, dataType);
				}

				return dataObject;
			}

			return null;
		}

		void CopyDataContext(ITopLevelDataObject dataObject, IDynamicData data, Type type)
		{
			var typeDescriptor = typeDescriptorCache.Get(type);

			var dataContextDescr = typeDescriptor.GetPropertyDescriptor(nameof(dataObject.DataContext));
			var dataContext = dataContextDescr.GetValue(data.OriginalValue);

			dataContextDescr.SetValue(dataObject, dataContext);
		}

		void MapPropertyOverride(IDataObject parent, string name, IDynamicData data)
		{
			var tryGetCustomFieldValue = GetCustomFieldValue(data);

			if (tryGetCustomFieldValue.IsFaulted)
			{
				return;
			}

			if (!propertyOverrideMap.TryGetValue(parent, out var customFields))
			{
				customFields = new Dictionary<string, IZType>();
			}

			customFields[name] = tryGetCustomFieldValue.Value;
			propertyOverrideMap[parent] = customFields;
		}

		Try<IZType> GetCustomFieldValue(IDynamicData customField)
		{
			var customValue = customField.Value is ZBool
				? Convert.ToString(customField.Value, CultureInfo.InvariantCulture)
				: customField.Value is UXmlDateTime uXmlDateTime
				? uXmlDateTime.ZDate
				: customField.Value;

			if (ZDataType.IsConvertibleToZType(customValue))
			{
				return Try<IZType>.Success(ZDataType.ObjectToZType(customValue));
			}

			return Try<IZType>.Failure(string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} is not convertible to ZType", customValue));
		}

		object CreateComplexPropertyValue(IDynamicData value, Type type)
		{
			if (value is IDynamicDataCollection dynamicDataCollection)
			{
				var collection = Activator.CreateInstance(type);

				var list = (IList)collection;

				var elementType = GetElementType(type);

				foreach (var element in CreateElements(dynamicDataCollection, elementType))
				{
					if (element != null)
					{
						list.Add(element);
					}
				}

				return list.Count > 0
					? collection
					: null;
			}

			return CreateDataObject(value, type);
		}

		Type GetElementType(Type type)
		{
			if (type.IsGenericType && type.GetGenericArguments().Any())
			{
				return type.GetGenericArguments().First();
			}

			return null;
		}

		IEnumerable<IDataObject> CreateElements(IDynamicDataCollection collection, Type elementType)
		{
			foreach (var element in collection.GetAllElements())
			{
				var instance = CreateDataObject(element, elementType);
				if (instance != null)
				{
					if (element.IsAdded())
					{
						dataObjectStateMap[instance] = DataObjectState.Added;
					}
					else if (element.IsRemoved())
					{
						dataObjectStateMap[instance] = DataObjectState.Removed;
					}

					yield return instance;
				}
			}
		}

		#endregion

		#region IDataOverrideProvider members

		readonly IDictionary<IDataObject, IDictionary<string, IZType>> propertyOverrideMap = new Dictionary<IDataObject, IDictionary<string, IZType>>();
		readonly IDictionary<IDataObject, DataObjectState> dataObjectStateMap = new Dictionary<IDataObject, DataObjectState>();

		DataObjectState IDataOverrideProvider.GetDataObjectState(IDataObject dataObject)
		{
			var result = DataObjectState.Default;

			return dataObjectStateMap.TryGetValue(dataObject, out result)
				? result
				: DataObjectState.Default;
		}

		IEnumerable<IPropertyOverride> IDataOverrideProvider.GetPropertyOverrides(IDataObject dataObject)
		{
			IDictionary<string, IZType> map;

			if (!propertyOverrideMap.TryGetValue(dataObject, out map))
			{
				yield break;
			}

			foreach (var entry in map)
			{
				yield return new PropertyOverride(
					entry.Key,
					entry.Value);
			}
		}

		sealed class PropertyOverride : IPropertyOverride
		{
			public PropertyOverride(string name, IZType value)
			{
				Argument.NotNullOrEmpty(name, nameof(name));
				Argument.NotNull(value, nameof(value));

				Name = name;
				Value = value;
			}

			public string Name { get; }
			public IZType Value { get; }
		}

		#endregion
	}
}
