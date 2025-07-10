using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class WarehouseClientCollectionProvider : CollectionProvider
	{
		public WarehouseClientCollectionProvider(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new WarehouseClientCollectionWithSecurityCheck(BusinessObjectFactory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.Organisation;

		public override string GetFilterDescription()
		{
			return Res.GetString("8f6df78b-b79a-4575-b981-17c48fdb4600", "The organization should be a warehouse client.");
		}
	}
}
