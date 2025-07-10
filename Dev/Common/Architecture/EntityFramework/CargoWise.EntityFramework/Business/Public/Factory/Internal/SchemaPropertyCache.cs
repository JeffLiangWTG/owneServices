using System;
using System.Collections.Generic;
using System.Reflection;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework
{
	class SchemaInfo
	{
		public SchemaInfo(Type bizOType)
		{
			schemaProperties = new Dictionary<string, string>(2);
			this.bizOType = bizOType;
		}

		public string this[string propertyName]
		{
			get
			{
				string result;
				if (!schemaProperties.TryGetValue(propertyName, out result))
				{
					result = GetValue(propertyName);
					schemaProperties.Add(propertyName, result);
				}
				return result;
			}
		}

		string GetValue(string propertyName)
		{
#if DEBUG
			TimesOfRunGetValue++;
#endif
			string result = null;

			if (schemaType == null)
			{
				Type typeWithSchema = bizOType;
				while (typeWithSchema != null && (schemaType = typeWithSchema.GetNestedType((NoResString)"Schema")) == null)
				{
					typeWithSchema = typeWithSchema.BaseType;
				}
			}

			if (schemaType != null)
			{
				FieldInfo field = schemaType.GetField(propertyName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
				if (field != null)
				{
					result = (string)field.GetValue(null);
				}
			}

			return result;
		}
#if DEBUG
		public int TimesOfRunGetValue { get; private set; }
#endif

		readonly Type bizOType;
		readonly Dictionary<string, string> schemaProperties;

		Type schemaType;
	}

	internal class BizOTypeHash
	{
		public BizOTypeHash()
		{
			hash = new Dictionary<Type, SchemaInfo>();
		}

		public SchemaInfo this[Type bizOType]
		{
			get
			{
				SchemaInfo result;
				if (!hash.TryGetValue(bizOType, out result))
				{
					result = new SchemaInfo(bizOType);
					hash[bizOType] = result;
				}

				return result;
			}
		}

		readonly Dictionary<Type, SchemaInfo> hash;
	}
}
