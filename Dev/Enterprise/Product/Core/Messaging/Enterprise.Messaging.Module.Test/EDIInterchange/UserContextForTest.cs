using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Messaging.Module.Testing
{
	sealed class UserContextForTest : UserContext
	{
		internal UserContextForTest(IUser user, ICompany company)
		{
			this.User = user;
			this.Company = company;
		}
	}
}
