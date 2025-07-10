using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentCollectionProvider : CollectionProviderWithCodeSupport
	{
		public IncidentCollectionProvider(BusinessObjectFactory businessObjectFactory) : base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new SupportIncidentCollection(BusinessObjectFactory);
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			return new SupportIncidentCollection(BusinessObjectFactory, Filter);
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return Modules.ClientModuleRegistration.SupportIncident;
			}
		}

		public override int MaxLength => IncidentMainSchema.IM_IncidentNumber.MaxLength;
	}
}
