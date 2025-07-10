using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class CommissionAgreementCollectionProvider : CollectionProvider
	{
		public CommissionAgreementCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new OrgCommissionAgreementCollection(BusinessObjectFactory, new AdhocCollectionRelationship(typeof(OrgCommissionAgreement)));
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			return new OrgCommissionAgreementCollection(BusinessObjectFactory, Filter);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.OrgCommissionAgreement;
	}
}
