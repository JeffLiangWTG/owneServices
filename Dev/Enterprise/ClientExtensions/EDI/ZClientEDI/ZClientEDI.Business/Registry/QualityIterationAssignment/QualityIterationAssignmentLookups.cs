using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class QualityIterationAssignmentLookups : ZLookups
	{
		public QualityIterationAssignmentLookups(BusinessObject parent)
			: base(parent)
		{
		}

		public ActiveBusinessObjectCollection<GlbGroup> ReleaseGroupList
		{
			get { return releaseGroupList ?? (releaseGroupList = new ActiveBusinessObjectCollection<GlbGroup>(Factory)); }
		}
		ActiveBusinessObjectCollection<GlbGroup> releaseGroupList;

		protected override BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;
	}
}

