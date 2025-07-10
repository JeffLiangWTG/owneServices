using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class BMSystemWorkflowDeterminerCollection : ActiveBusinessObjectCollection<BMSystemWorkflowDeterminer>
	{
		public BMSystemWorkflowDeterminerCollection(BMSystem system)
			: base(system.Factory, system, new ZQuery(), BMSystemWorkflowDeterminerSchema.FSW_FS_System)
		{
		}
	}
}
