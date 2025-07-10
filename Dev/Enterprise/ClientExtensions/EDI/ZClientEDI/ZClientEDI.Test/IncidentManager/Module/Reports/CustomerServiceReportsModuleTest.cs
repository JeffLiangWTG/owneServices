using Enterprise.Client.EDI;
using Enterprise.Client.EDI.IncidentManager.Module;
using NUnit.Framework;

namespace ZClientEDI.Test.IncidentManager.Module.Reports
{
	internal class CustomerServiceReportsModuleTest : TestCase
	{
		public void TestSecurityCheckpoint()
		{
			using (var module = new CustomerServiceReports())
			{
				AssertEquals(EDISecurityCheckpoints.CustomerServiceReports, module.SecurityCheckpoint);
			}
		}
	}
}
