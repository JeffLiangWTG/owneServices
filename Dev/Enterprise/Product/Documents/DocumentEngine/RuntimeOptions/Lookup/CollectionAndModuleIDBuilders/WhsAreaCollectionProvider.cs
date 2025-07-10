using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class WhsAreaCollectionProvider : CollectionProvider
	{
		public WhsAreaCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
			AreaCollectionType = ObjectFactory.GetType<IWhsAreaCollection>();
		}

		protected readonly Type AreaCollectionType;

		protected override IBusinessObjectCollection CreateCollection()
		{
			return (IBusinessObjectCollection)Activator.CreateInstance(AreaCollectionType, BusinessObjectFactory, new AdhocCollectionRelationship(AreaCollectionType));
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			IBusinessObjectCollection result = null;

			if (Filter != null && !Filter.IsEmpty)
			{
				var owner = BusinessObjectFactory.LoadTop1<IWhsWarehouse>(Filter);
				if (owner != null)
				{
					result = GetAreaCollection(owner);
				}
			}

			if (result == null)
			{
				result = (IBusinessObjectCollection)Activator.CreateInstance(AreaCollectionType, BusinessObjectFactory, ZQuery.NoResultQuery); // we do not want to return any areas.
			}

			return result;
		}

		protected virtual IWhsAreaCollection GetAreaCollection(IWhsWarehouse owner)
		{
			return (IWhsAreaCollection)Activator.CreateInstance(AreaCollectionType, BusinessObjectFactory, owner);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.WhsConfigArea;
	}
}
