using System;
using System.Collections.Generic;
using CargoWiseOne.ResourceStrings;

namespace CargoWise.EntityFramework
{
	internal static class SchemaBoundResourceStrings
	{
		internal static ResourceStringData Get(Type type, string propertyName)
		{
			ResourceStringData data = null;
			var tableName = GetTableName(type);
			if (tableName != null)
			{
				UInt16 asmid = EnsureManifestData(tableName, type);
				cache.TryGetValue(tableName + "|" + propertyName, out data);
				if (data != null)
				{
					data = Res._GetData(asmid, data);
				}
			}
			return data;
		}

		internal static ResourceStringData Get(Type type)
		{
			ResourceStringData data = null;
			var tableName = GetTableName(type);
			if (tableName != null)
			{
				UInt16 asmid = EnsureManifestData(tableName, type);
				cache.TryGetValue(tableName, out data);
				if (data != null)
				{
					data = Res._GetData(asmid, data);
				}
			}
			return data;
		}

		static UInt16 EnsureManifestData(string tableName, Type type)
		{
			lock (loadedTables)
			{
				UInt16 asmid;
				if (!loadedTables.TryGetValue(tableName, out asmid) && !unboundTypes.Contains(type))
				{
					var manifestData = GetManifestData(tableName, type, out asmid);
					if (manifestData != null)
					{
						foreach (var resourceStringDefinition in manifestData)
						{
							cache[resourceStringDefinition.Key] = resourceStringDefinition;
						}
						loadedTables.Add(tableName, asmid);
					}
					else
					{
						unboundTypes.Add(type);
					}
				}
				return asmid;
			}
		}

		internal static IEnumerable<ResourceStringData> GetManifestData(string tableName, Type type, out UInt16 asmid)
		{
			asmid = 0;
			while (type != null)
			{
				if (!type.Assembly.IsDynamic)
				{
					string resourceName = Array.Find(type.Assembly.GetManifestResourceNames(), item => item.EndsWith("." + tableName + "ResourceStrings.xml"));
					if (resourceName != null)
					{
						using (var stream = type.Assembly.GetManifestResourceStream(resourceName))
						{
							asmid = ResourceStringAssemblyIdAttribute.GetAsmid(type);
							return XmlResourceStringSource.ReadAll(stream);
						}
					}
				}
				type = type.BaseType;
			}
			return null;
		}

		internal static string GetTableName(Type type)
		{
			if (type != null &&
				typeof(BusinessObject).IsAssignableFrom(type) &&
				BusinessObjectFactory.HasTableName(type))
			{
				return BusinessObjectFactory.GetTableNameFromType(type);
			}
			return null;
		}

		static readonly Dictionary<string, ResourceStringData> cache = new Dictionary<string, ResourceStringData>();
		static readonly Dictionary<string, UInt16> loadedTables = new Dictionary<string, UInt16>();
		static readonly HashSet<Type> unboundTypes = new HashSet<Type>();
	}
}
