using System.Linq;
using System.Web.Script.Services;
using Enterprise.RemotePrinting.Server.Support;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Server.Testing
{
	class SupportServiceTest : TestCase
	{
		public void TestShouldEnableScriptService()
		{
			var services = typeof(SupportService).GetCustomAttributes(typeof(ScriptServiceAttribute), false);

			AssertNotNull("SupportService should add ScriptServiceAttribute", services);
		}

		public void TestShouldEnableScriptMethod()
		{
			var methods = typeof(SupportService).GetMethods().Where(m => m.GetCustomAttributes(typeof(ScriptMethodAttribute), false).Length > 0).ToArray();

			AssertEquals(2, methods.Length);
			AssertEquals("GetSignalRClients should add ScriptMethodAttribute", "GetSignalRClients", methods[0].Name);
			AssertEquals("RequestClientLogs should add ScriptMethodAttribute", "RequestClientLogs", methods[1].Name);
		}
	}
}
