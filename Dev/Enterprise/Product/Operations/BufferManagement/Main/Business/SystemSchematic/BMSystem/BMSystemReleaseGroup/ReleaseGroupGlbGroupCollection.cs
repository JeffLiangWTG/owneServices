using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	[ModuleID(ModuleId.GlbGroup)]
	public class ReleaseGroupGlbGroupCollection : ActiveBusinessObjectCollection<GlbGroup>
	{
		public ReleaseGroupGlbGroupCollection(BMSystem system)
			: base(system, typeof(BMSystemReleaseGroup), new ZQuery(), BMSystemReleaseGroupSchema.FSG_FS_System, BMSystemReleaseGroupSchema.FSG_GG_Group)
		{
		}
	}
}
