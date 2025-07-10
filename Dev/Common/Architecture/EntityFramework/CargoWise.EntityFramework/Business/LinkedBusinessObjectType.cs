using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Schema;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public class TypeLoader
	{
		public TypeLoader(Type type)
			: this(type, ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(BusinessObjectFactory.GetTableNameFromType(type)))
		{
		}

		public TypeLoader(Type type, ZString tablePrefix)
		{
			this.Type = type;
			this.TablePrefix = tablePrefix;
		}

		public readonly Type Type;
		public readonly ZString TablePrefix;

		public BusinessObject Load(BusinessObjectFactory factory, ZGuid pK)
		{
			return LoadCore(factory, pK);
		}

		protected virtual BusinessObject LoadCore(BusinessObjectFactory factory, ZGuid pK)
		{
			return factory.Load(Type, pK);
		}
	}

	public class TypeLoaderCollection
	{
		public TypeLoaderCollection()
			: this(null)
		{
		}

		public TypeLoaderCollection(params Type[] types)
		{
			InternalCollection = new Dictionary<ZString, TypeLoader>();
			if (types != null)
			{
				Add(types);
			}
		}

		public void Add(params Type[] types)
		{
			foreach (Type type in types)
			{
				Add(new TypeLoader(type));
			}
		}

		public void Add(TypeLoader loader)
		{
			InternalCollection.Add(loader.TablePrefix, loader);
		}

		public BusinessObject LoadBusinessObject(BusinessObjectFactory factory, ZString tablePrefix, ZGuid pK)
		{
			BusinessObject result = null;

			TypeLoader loader = null;
			if (InternalCollection.TryGetValue(tablePrefix, out loader))
			{
				result = loader.Load(factory, pK);
			}

			return result;
		}

		public bool HasLoaderFor(ZString tablePrefix)
		{
			return InternalCollection.ContainsKey(tablePrefix);
		}

		public bool HasLoaderFor(Type bizObjType)
		{
			return HasLoaderFor(ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(BusinessObjectFactory.GetTableNameFromType(bizObjType)));
		}

		public void SetTablePrefixAndPK(ILinkable bizObj, ZPropertyInfo tablePrefixInfo, ZPropertyInfo pkInfo)
		{
			if (bizObj == null)
			{
				tablePrefixInfo.Value = ZString.Empty;
				pkInfo.Value = ZGuid.Empty;
			}
			else
			{
				ZString tablePrefix = bizObj.LinkTablePrefix;
				tablePrefixInfo.Value = tablePrefix;
				pkInfo.Value = bizObj.LinkPK;
			}
		}

		public IEnumerable<Type> GetSupportedTypes()
		{
			return InternalCollection.Values.Select(x => x.Type);
		}

		readonly Dictionary<ZString, TypeLoader> InternalCollection;
	}
}
