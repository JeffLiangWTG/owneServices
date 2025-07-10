using System;
using System.Linq;
using System.Security.Principal;
using NUnit.Framework;

namespace Enterprise.Dat.Implementation.Testing
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public sealed class RequiresAdminPrivilegesAttribute : TestSetupAttribute, ITestAction
	{
		public RequiresAdminPrivilegesAttribute()
		{
		}

		public ActionTargets Targets => ActionTargets.Test | ActionTargets.Suite;

		public override void SetUp(TestCase testCase)
		{
			using (var identity = WindowsIdentity.GetCurrent())
			{
				var principal = new WindowsPrincipal(identity);
				if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
				{
					throw new NotSupportedException($"The principal: {principal} does not have admin privileges to run the test: {testCase.Name}.");
				}
			}
		}

		public void BeforeTest(NUnit.Framework.Interfaces.ITest test)
		{
			if (!test.Properties.ContainsKey("DAT:CapabilityRequirements"))
			{
				test.Properties.Add("DAT:CapabilityRequirements", "ADMIN");
			}
			else
			{
				var caps = test.Properties["DAT:CapabilityRequirements"].Cast<string>().ToList();
				if (!caps.Contains("ADMIN"))
				{
					caps.Add("ADMIN");
					test.Properties.Set("DAT:CapabilityRequirements", caps);
				}
			}
		}

		public void AfterTest(NUnit.Framework.Interfaces.ITest test)
		{
		}

		public override void TearDown(TestCase testCase)
		{
		}
	}
}
