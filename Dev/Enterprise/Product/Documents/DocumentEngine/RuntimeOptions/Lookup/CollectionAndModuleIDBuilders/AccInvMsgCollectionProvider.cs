using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class AccInvMsgCollectionProvider : CollectionProviderWithCodeSupport
	{
		public AccInvMsgCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new AccInvMsgCollection(BusinessObjectFactory, new AdhocCollectionRelationship(typeof(AccInvMsg)));
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			return new AccInvMsgCollection(BusinessObjectFactory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.AccInvMsg;

		public override int MaxLength => AccInvMsgSchema.A9_Code.MaxLength;
	}
}
