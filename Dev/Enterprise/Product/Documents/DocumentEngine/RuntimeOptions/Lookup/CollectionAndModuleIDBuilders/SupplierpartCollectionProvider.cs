using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class OrgSupplierPartCollectionProvider : CollectionProvider
	{
		public OrgSupplierPartCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			OrgHeader owner = Filter != null && !Filter.IsEmpty ? BusinessObjectFactory.LoadTop1<OrgHeader>(Filter) : null;
			return GetOrgSupplierPartCollection(BusinessObjectFactory, null, owner, false);
		}

		protected virtual OrgSupplierPartCollection GetOrgSupplierPartCollection(BusinessObjectFactory factory, OrgHeader supplier, OrgHeader owner, bool isExport)
		{
			return new OrgSupplierPartCollection(factory, supplier, owner, isExport);
		}

		public override sealed ModuleIdentifier ModuleID => GetModuleID();

		protected virtual ModuleIdentifier GetModuleID() => ModuleIDs.SupplierPart;

		public override string GetFilterDescription()
		{
			return Res.GetString("858d2bbd-20ae-4a01-a56d-e5e9ae37e210", "Client Required");
		}
	}
}
