using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSOutboundEDIMessage))]
	public class EMCSOutboundEDIMessageTest : EDIMessageTest
	{
		public void TestMessageDefaults()
		{
			var message = Factory.New<EMCSOutboundEDIMessage>();
			AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
			AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.IECustomsEMCS, message.EM_ApplicationCode);
		}

		public void TestGetMessageReferenceNumber()
		{
			var message1 = Factory.New<EMCSOutboundEDIMessage>();
			Factory.Save();
			AssertEquals("Should have created MessageNumber(001) from IEMessageControlNumber sequense", "IEM00000000000001", message1.EM_MessageNum);

			var message2 = Factory.New<EMCSOutboundEDIMessage>();
			Factory.Save();
			AssertEquals("Should have created MessageNumber(002) from IEMessageControlNumber sequense", "IEM00000000000002", message2.EM_MessageNum);
		}

		public void TestMessageInterpretation()
		{
			var message = Factory.New<EMCSOutboundEDIMessage>();
			message.EM_MessageText = "<Node>Test Text</Node>";
			Factory.Save();

			var expectedText = @"
						<html>
							</head>
							<body>
								<pre>&lt;Node&gt;Test Text&lt;/Node&gt;<pre>
							</body>
						</html>";
			AssertEquals("EM_MessageInterpretation", expectedText, message.EM_MessageInterpretation);
		}
	}
}
