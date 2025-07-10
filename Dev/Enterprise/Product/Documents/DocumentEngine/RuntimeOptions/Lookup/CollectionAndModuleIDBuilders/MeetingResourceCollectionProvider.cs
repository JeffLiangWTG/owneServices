using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class MeetingResourceCollectionProvider : CollectionProviderWithCodeSupport
	{
		public MeetingResourceCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new GlbResourceCollection(BusinessObjectFactory, new AdhocCollectionRelationship(typeof(GlbStaff)));
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			var meetingResourcesQuery = new ZQuery(GlbStaffSchema.GS_ResourceType, SystemDataRegistry.Instance.ResourceTypes.Value.GetActiveCodeDescriptionPairList().Cast<ICodeDescription>().Select(x => x.Code));
			return new GlbResourceCollection(BusinessObjectFactory, meetingResourcesQuery);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.GlbStaff;

		public override int MaxLength => GlbStaffSchema.GS_Code.MaxLength;
	}
}
