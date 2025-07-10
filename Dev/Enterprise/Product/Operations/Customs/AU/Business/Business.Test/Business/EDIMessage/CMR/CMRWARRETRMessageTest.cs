using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRWARRETRMessage))]
	sealed class CMRWARRETRMessageTest : CMRCUSRESMessageTest
	{
		public void TestEM_MessageInterpretation()
		{
			var message = Factory.New<CMRWARRETRMessage>();
			message.EM_MessageText = CMRExportMessagesTestData.WARRETFails;
			Assert("Report body contains error text", message.EM_MessageInterpretation.Contains("DECLARATION IS NOT IN THE CORRECT STATE TO ALLOW IT TO BE RETURNED TO THE WAREHOUSE. HOLD FOR CUSTOMS"));
		}
	}
}
