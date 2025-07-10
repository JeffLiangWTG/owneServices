using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class WhsLocationTypeCollectionProvider : CollectionProvider
	{
		public WhsLocationTypeCollectionProvider(BusinessObjectFactory factory)
			: base(factory)
		{
			CollectionType = ObjectFactory.GetType<IWhsLocationTypeCollection>();
		}

		readonly Type CollectionType;

		protected override IBusinessObjectCollection CreateCollection()
		{
			return (IBusinessObjectCollection)Activator.CreateInstance(CollectionType, BusinessObjectFactory, new AdhocCollectionRelationship(CollectionType));
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			return (IBusinessObjectCollection)Activator.CreateInstance(CollectionType, BusinessObjectFactory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.WhsConfigLocationType;

		public override string GetFilterDescription()
		{
			return Res.GetString("1b7eb057-b171-4e62-b6ad-a0722c7c182e", "Select Location Type.");
		}
	}
}
