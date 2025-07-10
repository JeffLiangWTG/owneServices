using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class AddressCollectionProvider : CollectionProviderWithCodeSupport
	{
		public AddressCollectionProvider(BusinessObjectFactory businessObjectFactory) : base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			//You need to derive from collection and set RelationShipFileter on it to use Filter property
			return new OrgAddressCollection(BusinessObjectFactory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.OrgAddresses;

		public override int MaxLength => OrgAddressSchema.OA_Code.MaxLength;
	}
}
