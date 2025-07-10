using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class LicenceEnterpriseCollectionProvider : CollectionProviderWithCodeSupport
	{
		public LicenceEnterpriseCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new LicenceEnterpriseCollection(this.BusinessObjectFactory);
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return Modules.ClientModuleRegistration.LicenceEnterprise;
			}
		}

		public override int MaxLength => LicenceEnterpriseSchema.LE_EnterpriseCode.MaxLength;
	}
}
