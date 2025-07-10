using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	sealed class DocDataObjectDynamicDataFactory : IDynamicDataFactory
	{
		readonly IDictionary<string, IDynamicData> cache = new Dictionary<string, IDynamicData>();

		#region Create (with IDynamicDataManager)

		public IDynamicData Create(object obj, Type type, IDynamicDataManager manager) => TryGetFromCacheOrCreate(obj, type, manager);

		IDynamicData TryGetFromCacheOrCreate(object obj, Type type, IDynamicDataManager manager)
		{
			var id = GetId(obj);

			if (!string.IsNullOrWhiteSpace(id))
			{
				if (!cache.TryGetValue(id, out var res))
				{
					res = CreateNew(obj, type, manager);
					cache[id] = res;
				}

				return res;
			}

			return CreateNew(obj, type, manager);
		}

		IDynamicData CreateNew(object obj, Type type, IDynamicDataManager manager)
		{
			if (IsCollection(type))
			{
				return new DynamicDataCollection((IEnumerable)obj, type, manager);
			}

			return new DocDataObjectDynamicData(obj, type, manager);
		}

		#endregion

		#region Create (with IDynamicData)

		public IDynamicData Create(object obj, Type type, IDynamicData parent) => TryGetFromCacheOrCreate(obj, type, parent);

		IDynamicData TryGetFromCacheOrCreate(object obj, Type type, IDynamicData parent)
		{
			var id = GetId(obj);

			if (!string.IsNullOrWhiteSpace(id))
			{
				if (!cache.TryGetValue(id, out var res))
				{
					res = CreateNew(obj, type, parent);
					cache[id] = res;
				}

				return res;
			}

			return CreateNew(obj, type, parent);
		}

		IDynamicData CreateNew(object obj, Type type, IDynamicData parent)
		{
			if (IsCollection(type))
			{
				return new DynamicDataCollection((IEnumerable)obj, type, parent);
			}

			return new DocDataObjectDynamicData(obj, type, parent);
		}

		#endregion

		#region Implementation

		static string GetId(object obj)
		{
			if (obj is DocDataObject docDataObject)
			{
				return Convert.ToString(docDataObject.Identifier, CultureInfo.InvariantCulture);
			}

			return null;
		}

		static bool IsCollection(Type type)
		{
			return ImplementsIEnumerable(type)
				|| type.GetInterfaces().Any(ImplementsIEnumerable);
		}

		static bool ImplementsIEnumerable(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);

		#endregion
	}
}
