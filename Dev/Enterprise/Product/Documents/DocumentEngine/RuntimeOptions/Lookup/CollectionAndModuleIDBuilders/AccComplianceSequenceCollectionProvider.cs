using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class AccComplianceSequenceCollectionProvider : CollectionProviderWithCodeSupport
	{
		public AccComplianceSequenceCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new AccComplianceSequenceCollection(BusinessObjectFactory, new AdhocCollectionRelationship(typeof(AccComplianceSequence)));
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			return new AccComplianceSequenceCollection(BusinessObjectFactory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.AccComplianceSequence;

		public override int MaxLength => AccComplianceSequenceSchema.XD_Code.MaxLength;
	}
}
