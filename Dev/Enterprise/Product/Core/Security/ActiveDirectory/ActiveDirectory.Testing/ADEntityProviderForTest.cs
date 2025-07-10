using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Moq;

namespace Enterprise.Security.ActiveDirectory.Test
{
	public class ADEntityProviderForTest : IADEntityProvider
	{
		public IADEntity GetADGroup(IGlbGroup group) => ADGroup;

		public IADUser GetADUser(IGlbStaff staff) => ADUser;

		public IADUser ADUser
		{
			get => adUser ?? (adUser = new Mock<IADUser>().Object);
			set => adUser = value;
		}
		IADUser adUser;

		public IADEntity ADGroup
		{
			get => adGroup ?? (adGroup = new Mock<IADEntity>().Object);
			set => adGroup = value;
		}
		IADEntity adGroup;
	}
}
