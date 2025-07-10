using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	sealed class NctsEdiMessagePrettierTest : TestCaseWithFactory
	{
		public void TestOutboundPretty()
		{
			var expected = @"<tr><td>Message Recipient</td><td>NTA.XX</td></tr>";
			AssertContains(expected, new NctsEdiMessagePrettier(message).MakeOutboundPrettyForInterpretation(header));
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = Factory.New<EDIMessage>();
			message.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
										<q1:CC015C xmlns:q1=""http://ncts.dgtaxud.ec"">
										  <messageRecipient>NTA.XX</messageRecipient>
										</q1:CC015C>";
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
		}
		EDIMessage message;
		NctsHeader header;
	}
}
