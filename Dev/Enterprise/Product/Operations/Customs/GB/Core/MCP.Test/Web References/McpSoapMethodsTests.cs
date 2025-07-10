using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Chief;
using Enterprise.Customs.GB.Registry;

namespace Enterprise.Customs.GB.MCP.SoapMethods.Testing
{
	class McpSoapMethodsTests : TestCaseWithFactory
	{
		public void TestSignatureOfSoapMethods_ProcessEdiMessage()
		{
			var allMethods = typeof(IGbCspUploaderInterface).GetMethods();
			var processEdiMessageMethod = allMethods.FirstOrDefault(m => !m.IsSpecialName);
			AssertEquals("[Precondition] Method Name : processEDIMessage", "processEDIMessage", processEdiMessageMethod.Name);
			var runtimeMethod = typeof(CusDec.destin8WebService.ChiefEDIPortClient).GetMethod(processEdiMessageMethod.Name + "Async");
			var parameters = runtimeMethod.GetParameters();
			AssertEquals("EDI", parameters[0].Name);
			AssertEquals("CompanyCode", parameters[1].Name);
			AssertEquals("Operational", parameters[2].Name);
		}

		public void TestSignatureOfSoapMethods_Download()
		{
			var allMethods = typeof(ICspPrintsMailBoxProvider).GetMethods();
			foreach (var method in allMethods.Where(m => !m.IsSpecialName && !m.Name.Contains("checkCds")))
			{
				var runtimeMethod = typeof(CusDec.destin8WebService.ChiefEDIPortClient).GetMethod(method.Name + "Async");
				var parameters = runtimeMethod.GetParameters();
				AssertEquals("CompanyCode", parameters[0].Name);
				AssertEquals("Device", parameters[1].Name);
				if (parameters.Length >= 3)
				{
					AssertEquals("BatchID", parameters[2].Name);
				}
			}
		}
	}
}
