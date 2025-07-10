using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRDEPRECRMessage))]
	sealed class CMRDEPRECRMessageTest : CMRCUSRESMessageTest
	{
		public void TestEM_MessageInterpretation()
		{
			var message = Factory.New<CMRDEPRECRMessage>();
			message.EM_MessageText = CMRExportMessagesTestData.DEPRECFails;
			Assert("Report body contains error text", message.EM_MessageInterpretation.Contains("REPORTING PARTY Reporting Party Id ABN=41065894724 IS NOT LINKED WITH REPORTED DEPOT ESTABLISHMENT Establishment ID=9172E"));
		}
	}
}
