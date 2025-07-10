using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace CargoWise.EntityFramework
{
	public class DataBoundResourceStrings : IDataBoundResourceStrings
	{
		public static ResourceStringData GetDataForProperty(PropertyInfo property) => GetDataForProperty(property, businessObject: null);

		public static ResourceStringData GetDataForProperty(PropertyInfo property, IDataBoundBusinessObject businessObject)
		{
			if (property == null)
			{
				return null;
			}

			ResourceStringData result;
			var resourceStringDataAttribute = GetResourceStringDataAttribute(property, businessObject);
			if (resourceStringDataAttribute != null)
			{
				result = resourceStringDataAttribute.GetData(ResourceStringAssemblyIdAttribute.GetAsmid(property.DeclaringType));
			}
			else
			{
				result = SchemaBoundResourceStrings.Get(property.ReflectedType, property.Name);
			}

			if (result == null || result.IsEmpty())
			{
				result = GetDataForPropertyUsingColumnPrefix(property.Name);
			}

			return result;
		}

		public static ResourceStringData GetDataForPropertyWithMultipleResourceKey(PropertyDescriptor property, string multipleResourceKey, IDataBoundBusinessObject dataBoundBusinessObject = null)
			=> GetDataForPropertyWithMultipleResourceKey(property, dataBoundBusinessObject, string.IsNullOrEmpty(multipleResourceKey) ? null : new[] { multipleResourceKey });

		public static ResourceStringData GetDataForPropertyWithMultipleResourceKey(PropertyDescriptor property, IReadOnlyList<string> multipleResourceKeysInPreferenceOrder)
			=> GetDataForPropertyWithMultipleResourceKey(property, dataBoundBusinessObject: null, multipleResourceKeysInPreferenceOrder: multipleResourceKeysInPreferenceOrder);

		public static ResourceStringData GetDataForPropertyWithMultipleResourceKey(PropertyDescriptor property, IDataBoundBusinessObject dataBoundBusinessObject, IReadOnlyList<string> multipleResourceKeysInPreferenceOrder)
		{
			if (property == null)
			{
				return null;
			}

			var propertyInfo = property.ComponentType.GetProperty(property.Name);
			if (propertyInfo == null)
			{
				return null;
			}

			var resourceStringDataAttribute = GetResourceStringDataAttribute(propertyInfo, dataBoundBusinessObject, multipleResourceKeysInPreferenceOrder);
			if (resourceStringDataAttribute != null)
			{
				return resourceStringDataAttribute.GetData(ResourceStringAssemblyIdAttribute.GetAsmid(propertyInfo.DeclaringType));
			}

			return GetDataForProperty(property, dataBoundBusinessObject);
		}

		public static ResourceStringData GetDataForProperty(PropertyDescriptor property) => GetDataForProperty(property, businessObject: null);

		public static ResourceStringData GetDataForProperty(PropertyDescriptor property, IDataBoundBusinessObject businessObject) => property == null ? null : GetDataForProperty(property.ComponentType.GetProperty(property.Name), businessObject);

		public static ResourceStringData GetDataForProperty(Type type, string propertyName, IReadOnlyList<string> multipleResourceKeysInPreferenceOrder = null) => GetDataForProperty(type, propertyName, null, multipleResourceKeysInPreferenceOrder);

		public static ResourceStringData GetDataForProperty(Type type, string propertyName, IDataBoundBusinessObject businessObject, IReadOnlyList<string> multipleResourceKeysInPreferenceOrder = null)
		{
			ResourceStringData result = null;

			if (type != null)
			{
				result = GetAttributeSpecifiedDataString(type, propertyName, businessObject, multipleResourceKeysInPreferenceOrder);
				if (result == null)
				{
					result = SchemaBoundResourceStrings.Get(type, propertyName);
				}
			}
			if (result == null || result.IsEmpty())
			{
				result = GetDataForPropertyUsingColumnPrefix(propertyName);
			}
			return result;
		}

		static ResourceStringData GetDataForPropertyUsingColumnPrefix(string propertyName)
		{
			ResourceStringData result = null;

			var type = GetTypeForPropertyUsingColumnPrefix(propertyName);
			if (type != null)
			{
				result = GetAttributeSpecifiedDataString(type, propertyName, null);
				if (result == null)
				{
					result = SchemaBoundResourceStrings.Get(type, propertyName);
				}
			}
			return result;
		}

		internal static Type GetTypeForPropertyUsingColumnPrefix(string propertyName)
		{
			Type type = null;
			string columnPrefix = CargoWise.Schema.Schema.GetPrefixFromColumnName(propertyName);

			if (!String.IsNullOrWhiteSpace(columnPrefix))
			{
				var tableSchema = ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchemaFromColumnNamePrefix(columnPrefix);
				if (tableSchema != null)
				{
					type = GetTypeForTable(tableSchema.TableName);
				}
			}
			return type;
		}

		public static ResourceStringData GetDataForTable(Type type) => type == null ? null : SchemaBoundResourceStrings.Get(type);

		public static string GetStringForTable(Type type)
		{
			var data = GetDataForTable(type);

			if (data != null && !data.IsEmpty())
			{
				return data.Caption;
			}
			else
			{
				return SchemaBoundResourceStrings.GetTableName(type);
			}
		}

		static ResourceStringDataAttribute GetResourceStringDataAttribute(PropertyInfo propertyInfo, IDataBoundBusinessObject dataBoundBusinessObject, IReadOnlyList<string> multipleResourceKeysInPreferenceOrder = null)
		{
			if (propertyInfo == null)
			{
				return null;
			}

			var resourceStringDataAttributes = Attribute.GetCustomAttributes(propertyInfo, true)
				.OfType<ResourceStringDataAttribute>()
				.ToArray();

			var resourceStringDataAttributesWithMultipleKey = GetResourceStringDataAttributeForMultipleResourceKeys(resourceStringDataAttributes, multipleResourceKeysInPreferenceOrder);

			return GetResStringDataAttribute(resourceStringDataAttributesWithMultipleKey)
					?? GetResStringDataAttribute(resourceStringDataAttributes.Where(a => string.IsNullOrWhiteSpace(a.MultipleKey)).ToArray())
					?? GetAttributeWithoutIsApplicableMember(resourceStringDataAttributes);

			ResourceStringDataAttribute GetResStringDataAttribute(ResourceStringDataAttribute[] array) => GetApplicableAttribute(array) ?? GetAttributeWithoutIsApplicableMember(array);

			ResourceStringDataAttribute GetAttributeWithoutIsApplicableMember(ResourceStringDataAttribute[] array) => array.FirstOrDefault(a => string.IsNullOrEmpty(a.IsApplicableMember));

			ResourceStringDataAttribute GetApplicableAttribute(ResourceStringDataAttribute[] array) => array.FirstOrDefault(a => HasIsApplicableMemberAndIsValid(a, dataBoundBusinessObject));
		}

		static bool HasIsApplicableMemberAndIsValid(ResourceStringDataAttribute resourceStringDataAttribute, IDataBoundBusinessObject dataBoundBusinessObject)
		{
			var isApplicableMemberName = resourceStringDataAttribute.IsApplicableMember;

			if (string.IsNullOrWhiteSpace(isApplicableMemberName) || dataBoundBusinessObject == null)
			{
				return false;
			}

			var isApplicableBool = false;
			return dataBoundBusinessObject.HasProperty(isApplicableMemberName)
					&& (dataBoundBusinessObject.TryGetValue<ZBool>(isApplicableMemberName, out var isApplicableZBool) || dataBoundBusinessObject.TryGetValue(isApplicableMemberName, out isApplicableBool))
					&& (isApplicableZBool || isApplicableBool);
		}

		static ResourceStringDataAttribute[] GetResourceStringDataAttributeForMultipleResourceKeys(ResourceStringDataAttribute[] attributes, IReadOnlyList<string> multipleResourceKeysInPreferenceOrder)
		{
			var resourceStringDataAttributes = Array.Empty<ResourceStringDataAttribute>();

			if (multipleResourceKeysInPreferenceOrder == null || attributes == null)
			{
				return resourceStringDataAttributes;
			}

			if (multipleResourceKeysInPreferenceOrder.Where(x => !string.IsNullOrEmpty(x)).Select(x => x.ToUpperInvariant()).ToArray() is IReadOnlyList<string> multipleResourceKeys)
			{
				var dictionary = attributes.Where(y => !string.IsNullOrEmpty(y.MultipleKey))
					.GroupBy(x => x.MultipleKey.ToUpperInvariant())
					.ToDictionary(x => x.Key, y => y);

				foreach (var multipleResourceKey in multipleResourceKeys)
				{
					if (dictionary.TryGetValue(multipleResourceKey, out var list))
					{
						return list.ToArray();
					}
				}
			}

			return resourceStringDataAttributes;
		}

		static ResourceStringData GetAttributeSpecifiedDataString(Type type, string propertyName, IDataBoundBusinessObject dataBoundBusinessObject, IReadOnlyList<string> multipleResourceKeysInPreferenceOrder = null)
		{
			ResourceStringData result = null;
			var propertyInfo = GetPropertyInfo(type, propertyName, out var declaringType);

			if (propertyInfo != null)
			{
				var resourceDataAttribute = GetResourceStringDataAttribute(propertyInfo, dataBoundBusinessObject, multipleResourceKeysInPreferenceOrder);
				if (resourceDataAttribute != null)
				{
					result = resourceDataAttribute.GetData(ResourceStringAssemblyIdAttribute.GetAsmid(declaringType));
				}
			}

			return result;
		}

		static PropertyInfo GetPropertyInfo(Type type, string propertyName, out Type declaringType)
		{
			PropertyInfo propertyInfo;
			Type typeToCheck = type;
			do
			{
				declaringType = typeToCheck;
				propertyInfo = typeToCheck.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
				typeToCheck = typeToCheck.BaseType;
			}
			while (propertyInfo == null && typeToCheck != null);

			if (propertyInfo == null)
			{
				propertyInfo = type.GetProperty(propertyName);
			}

			return propertyInfo;
		}

		internal static AttributeType GetAttribute<AttributeType>(Type type, string propertyName, out Type declaringType) where AttributeType : Attribute
		{
			AttributeType attribute = null;

			var propertyInfo = GetPropertyInfo(type, propertyName, out declaringType);
			if (propertyInfo != null)
			{
				attribute = (AttributeType)Attribute.GetCustomAttribute(propertyInfo, typeof(AttributeType));
			}
			return attribute;
		}

		public static string GetColumnDescriptiveName(string tableName, string columnName)
		{
			IDataBoundResourceStrings resStrings = new DataBoundResourceStrings();
			return resStrings.GetStringForProperty(tableName, columnName);
		}

		string IDataBoundResourceStrings.GetStringForProperty(string tableName, string propertyName)
		{
			string result = null;
			Type type = GetTypeForTable(tableName);
			if (type != null)
			{
				var data = GetDataForProperty(type, propertyName);
				if (data != null)
				{
					result = data.Caption;
				}
			}
			if (string.IsNullOrEmpty(result))
			{
				result = (string.IsNullOrEmpty(tableName) ? "" : tableName + "|") + propertyName;
			}
			return result;
		}

		public static string GetTableDescriptiveName(string tableName)
		{
			IDataBoundResourceStrings resStrings = new DataBoundResourceStrings();
			return resStrings.GetStringForTable(tableName);
		}

		string IDataBoundResourceStrings.GetStringForTable(string tableName)
		{
			string result = null;
			Type type = GetTypeForTable(tableName);
			if (type != null)
			{
				result = GetStringForTable(type);
			}
			if (string.IsNullOrEmpty(result))
			{
				result = tableName;
			}
			return result;
		}

		internal static Type GetTypeForTable(string tableName)
		{
			return GetTypeForInversionTableEntry(tableName);
		}

		internal static Type GetTypeForInversionTableEntry(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				return null;
			}
			Type type;
			lock (inversionTable)
			{
				inversionTable.TryGetValue(key, out type);
				if (type == null)
				{
					string typeName;
					RawInversionTable.TryGetValue(key, out typeName);
					if (typeName != null)
					{
						type = Type.GetType(typeName);
						inversionTable.Add(key, type);
					}
				}
			}
			return type;
		}

		static readonly IDictionary<string, Type> inversionTable = new Dictionary<string, Type>();

		static IDictionary<string, string> RawInversionTable
		{
			get
			{
				if (rawInversionTable == null)
				{
					rawInversionTable = ResourceStringsInversionControlFile.Read();
				}
				return rawInversionTable;
			}
		}
		static IDictionary<string, string> rawInversionTable;
	}
}
