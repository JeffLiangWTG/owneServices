using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Integration
{
	public interface IBMSystem : IBusiness
	{
		ZGuid PK { get; }
		ZString FS_Name { get; set; }
		ZString FS_Description { get; set; }

		ZBool FS_IsLive { get; set; }

		IBusinessObjectCollection Components { get; }

		bool IsForWorkflowType(string workflowType);
	}
}
