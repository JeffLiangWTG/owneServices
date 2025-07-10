using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class ShipsAgencyPrincipalProvider : CollectionProvider
	{
		public ShipsAgencyPrincipalProvider(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new ShipsAgencyPrincipalCollectionWithSecurityCheck(BusinessObjectFactory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.Organisation;
	}
}
