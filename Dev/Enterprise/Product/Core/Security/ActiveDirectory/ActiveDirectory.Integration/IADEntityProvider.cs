using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Security.ActiveDirectory
{
	public interface IADEntityProvider
	{
		IADUser GetADUser(IGlbStaff staff);
		IADEntity GetADGroup(IGlbGroup group);
	}
}
