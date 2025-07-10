using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace CargoWise.EntityFramework
{
	public delegate IBusinessObjectCollection BusinessObjectCollectionProvider(BusinessObjectFactory factory);

	public class BusinessObjectCollectionProviderFactory
	{
		// Generated Delegate:
		//
		// BusinessObjectCollection Create_<Collection>(BusinessObjectFactory factory)
		// {
		//     return new <Collection>(factory);
		// }

		public BusinessObjectCollectionProvider Get(Type collectionType)
		{
			BusinessObjectCollectionProvider result;

			if (!cache.TryGetValue(collectionType, out result))
			{
				result = New(collectionType);
				cache.Add(collectionType, result);
			}

			return result;
		}

		static BusinessObjectCollectionProvider New(Type collectionType)
		{
			if (collectionType == null)
			{
				throw new ArgumentNullException(nameof(collectionType));
			}

			if (!(typeof(IBusinessObjectCollection)).IsAssignableFrom(collectionType))
			{
				throw new ArgumentOutOfRangeException(nameof(collectionType), collectionType, "type must be a BusinessObjectCollection");
			}

			if (collectionType.IsAbstract)
			{
				throw new ArgumentOutOfRangeException(nameof(collectionType), collectionType, "type must not be abstract");
			}

			ConstructorInfo constructor = collectionType.GetConstructor(new Type[] { typeof(BusinessObjectFactory) });
			if (constructor == null)
			{
				new InvalidOperationException("cant find a factory only constructor");
			}

			DynamicMethod dm = new DynamicMethod(
				"Create_" + collectionType.Name,
				typeof(IBusinessObjectCollection),
				new Type[] { typeof(BusinessObjectFactory) },
				typeof(BusinessObjectCollectionProviderFactory));

			ILGenerator g = dm.GetILGenerator();
			g.Emit(OpCodes.Ldarg_0);
			g.Emit(OpCodes.Newobj, constructor);
			g.Emit(OpCodes.Ret);

			return (BusinessObjectCollectionProvider)dm.CreateDelegate(typeof(BusinessObjectCollectionProvider));
		}

		readonly Dictionary<Type, BusinessObjectCollectionProvider> cache = new Dictionary<Type, BusinessObjectCollectionProvider>();
	}
}
