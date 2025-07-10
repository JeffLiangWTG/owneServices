using Enterprise.Core.Environment;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	public sealed class DummyCheckPointWithSecuritySet : SecurityCheckpoint
	{
		public static IZSecurity SecurityInstance
		{
			get
			{
				var testSecurity = new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
				return testSecurity.ZSecurityInstance;
			}
		}

		public DummyCheckPointWithSecuritySet(bool isAllowed) : base("Dummy", (NoResString)"TESTING 1 2 3", null, SecurityInstance)
		{
			this.IsAllowed = isAllowed;
		}
	}
}
