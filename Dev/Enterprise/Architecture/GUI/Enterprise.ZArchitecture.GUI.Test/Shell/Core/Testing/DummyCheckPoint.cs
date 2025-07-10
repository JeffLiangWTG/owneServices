using Enterprise.Core.Environment;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class DummyCheckPoint : SecurityCheckpoint
	{
		public static bool SecurityAllowed = true;

		public static IZSecurity SecurityInstance
		{
			get
			{
				var testSecurity = new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
				return testSecurity.ZSecurityInstance;
			}
		}

		DummyCheckPoint() : base("ZUBS", (NoResString)"TESTING 1 2 3", null, SecurityInstance) { }

		static DummyCheckPoint fInstance;
		public static DummyCheckPoint Instance
		{
			get
			{
				if (fInstance == null)
				{
					fInstance = new DummyCheckPoint();
				}

				return fInstance;
			}
		}
		public override bool IsAllowed => SecurityAllowed; //new SecurityCheckpoint(,,,,,,
	}
}
