using System;
using System.Collections.Generic;

namespace CargoWise.EntityFramework
{
	class ActiveBusinessObjectCollectionIndexNotifier : IService
	{
		readonly Dictionary<Guid, IActiveBusinessObjectCollectionIndex> store = new Dictionary<Guid, IActiveBusinessObjectCollectionIndex>();

		public ActiveBusinessObjectCollectionIndexNotifier(BusinessObjectFactory factory)
		{
			factory.Saved += Factory_Saved;
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			var indexes = new IActiveBusinessObjectCollectionIndex[store.Count];
			store.Values.CopyTo(indexes, 0 );

			foreach (var index in indexes)
			{
				index.OnFactorySaved(savedSuccessfully);
			}
		}

		public void Add(IActiveBusinessObjectCollectionIndex index)
		{
			store[index.ID] = index;
		}

		public void Remove(IActiveBusinessObjectCollectionIndex index)
		{
			store.Remove(index.ID);
		}

		public void Clear()
		{
			store.Clear();
		}

		public static ActiveBusinessObjectCollectionIndexNotifier For(BusinessObjectFactory factory)
		{
			return factory.ServiceContainer.GetService<ActiveBusinessObjectCollectionIndexNotifier>() ??
				factory.ServiceContainer.AddService(new ActiveBusinessObjectCollectionIndexNotifier(factory));
		}
	}
}
