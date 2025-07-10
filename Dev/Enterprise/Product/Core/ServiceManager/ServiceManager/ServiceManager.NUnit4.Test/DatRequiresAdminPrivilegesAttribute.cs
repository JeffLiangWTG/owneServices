using System;
using System.Linq;
using System.Security.Principal;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace Enterprise.Dat.Implementation.Testing
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public sealed class DatRequiresAdminPrivilegesAttribute : Attribute, ITestAction, IApplyToContext
	{
		public DatRequiresAdminPrivilegesAttribute(string reason)
		{ }

		public ActionTargets Targets => ActionTargets.Test | ActionTargets.Suite;

		public void ApplyToContext(TestExecutionContext context)
		{
			using var identity = WindowsIdentity.GetCurrent();
			var principal = new WindowsPrincipal(identity);
			if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
			{
				throw new NotSupportedException($"The principal: {principal} does not have admin privileges to run the test: {context.CurrentTest.Name}.");
			}
		}

		public void BeforeTest(ITest test)
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

		public void AfterTest(ITest test)
		{
		}
	}
}
