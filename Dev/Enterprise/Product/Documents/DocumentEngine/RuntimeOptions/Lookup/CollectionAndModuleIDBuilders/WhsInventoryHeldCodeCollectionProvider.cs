using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class WhsInventoryHeldCodeCollectionProvider : CollectionProvider
	{
		public WhsInventoryHeldCodeCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
			HoldCodeCollectionType = ObjectFactory.GetType<IWhsInventoryHeldCodeCollection>();
			HoldCodeType = ObjectFactory.GetType<IWhsInventoryHeldCode>();
		}

		protected readonly Type HoldCodeCollectionType;
		protected readonly Type HoldCodeType;

		protected override IBusinessObjectCollection CreateCollection()
		{
			return (IBusinessObjectCollection)Activator.CreateInstance(HoldCodeCollectionType, BusinessObjectFactory, new AdhocCollectionRelationship(HoldCodeType));
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			return (IBusinessObjectCollection)Activator.CreateInstance(HoldCodeCollectionType, BusinessObjectFactory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.WhsInventoryHeldCodes;
	}
}
