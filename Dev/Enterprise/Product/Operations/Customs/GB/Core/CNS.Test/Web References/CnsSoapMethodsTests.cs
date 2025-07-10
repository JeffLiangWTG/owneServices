using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Chief;

namespace Enterprise.Customs.GB.CNS.SoapMethods.Testing
{
	class CnsSoapMethodsTests : TestCaseWithFactory
	{
		public void TestSignatureOfSoapMethods_ProcessEdiMessage()
		{
			var allMethods = typeof(IGbCspUploaderInterface).GetMethods();
			var processEdiMessageMethod = allMethods.Where(m => !m.IsSpecialName).FirstOrDefault();
			AssertEquals("[Precondition] Method Name : processEDIMessage", "processEDIMessage", processEdiMessageMethod.Name);
			var runtimeMethod = typeof(WebServices.CnsChiefEDI.ChiefEDIPortQSService).GetMethod(processEdiMessageMethod.Name);
			var parameters = runtimeMethod.GetParameters();
			AssertEquals("EDI", parameters[0].Name);
			AssertEquals("CompanyCode", parameters[1].Name);
			AssertEquals("Operational", parameters[2].Name);
		}
	}
}
