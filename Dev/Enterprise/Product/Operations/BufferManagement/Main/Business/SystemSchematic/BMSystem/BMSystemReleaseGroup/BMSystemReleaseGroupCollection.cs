using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class BMSystemReleaseGroupCollection : ActiveBusinessObjectCollection<BMSystemReleaseGroup>
	{
		public BMSystemReleaseGroupCollection(BMSystem parentSystem)
			: base(parentSystem.Factory, parentSystem, new ZQuery(), BMSystemReleaseGroupSchema.FSG_FS_System)
		{
			this.parentSystem = parentSystem;
		}

		readonly BMSystem parentSystem;

		public ActiveBusinessObjectCollection<GlbGroup> ToGroupCollection()
		{
			return new ReleaseGroupGlbGroupCollection(parentSystem);
		}
	}
}
