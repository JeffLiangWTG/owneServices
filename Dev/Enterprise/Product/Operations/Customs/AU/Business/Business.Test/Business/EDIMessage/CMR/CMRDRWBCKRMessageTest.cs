using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRDRWBCKRMessage))]
	sealed class CMRDRWBCKRMessageTest : CMRCUSRESMessageTest
	{
		public void TestEM_MessageInterpretation()
		{
			var message = Factory.New<CMRDRWBCKRMessage>();
			message.EM_MessageText = CMRDrawbackMessageTestData.DRWBCKFails;
			Assert("Report body contains error text", message.EM_MessageInterpretation.Contains("AMBER RSN TYPE CANNOT BE POPULATE AT BOTH HDR AND LINE LEVEL"));
		}
	}
}
