using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class WarehouseProductCollectionProvider : OrgSupplierPartCollectionProvider
	{
		public WarehouseProductCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.WhsConfigProduct;

		protected override OrgSupplierPartCollection GetOrgSupplierPartCollection(BusinessObjectFactory factory, OrgHeader supplier, OrgHeader owner, bool isExport)
		{
			return (OrgSupplierPartCollection)Activator.CreateInstance(WhsOrgSupplierPartCollectionType, new object[] { factory, supplier, owner, isExport });
		}

		Type WhsOrgSupplierPartCollectionType
		{
			get { return whsOrgSupplierPartCollectionType ?? (whsOrgSupplierPartCollectionType = ObjectFactory.GetType<IWhsOrgSupplierPartCollection>()); }
		}
		Type whsOrgSupplierPartCollectionType;
	}
}
