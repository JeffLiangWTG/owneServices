using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Security.ActiveDirectory
{
	public class ADEntityProvider : IADEntityProvider
	{
		public IADUser GetADUser(IGlbStaff staff)
		{
			return new ADUser((GlbStaff)staff);
		}

		public IADEntity GetADGroup(IGlbGroup group)
		{
			return new ADGroup((GlbGroup)group);
		}
	}
}
