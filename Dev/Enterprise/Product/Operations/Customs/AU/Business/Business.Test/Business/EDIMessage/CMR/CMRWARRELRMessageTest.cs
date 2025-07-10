using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRWARRELRMessage))]
	sealed class CMRWARRELRMessageTest : CMRCUSRESMessageTest
	{
		public void TestEM_MessageInterpretation()
		{
			var message = Factory.New<CMRWARRELRMessage>();
			message.EM_MessageText = CMRExportMessagesTestData.WARRELFails;
			Assert("Report body contains error text", message.EM_MessageInterpretation.Contains("RELEASE CANNOT BE REQUESTED MORE THAN 24 HOURS IN ADVANCE"));
		}
	}
}
