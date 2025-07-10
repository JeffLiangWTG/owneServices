using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.CH.Business.Testing;

class OutboundMessageProcessorTest : TestCaseWithFactory
{
	public void TestProcessMessagesWithTokenCredentials() => CombineAssertions(() =>
	{
		CredentialsTestHelper.CreateCurrentCompanyTokenCredential();
		AssertProcessMessages(new[]
		{
				(true, ApplicationCodeList.Codes.CHCustomsEdec),
				(true, ApplicationCodeList.Codes.CHCustomsPassar),
				(true, ApplicationCodeList.Codes.CHCustomsCharteraOutput),
		});
	});

	public void TestProcessMessagesWithoutTokenCredentials() => CombineAssertions(() =>
	{
		AssertProcessMessages(new[]
		{
				(true, ApplicationCodeList.Codes.CHCustomsEdec),
				(false, ApplicationCodeList.Codes.CHCustomsPassar),
				(false, ApplicationCodeList.Codes.CHCustomsCharteraOutput),
		});
	});

	void AssertProcessMessages((bool expectedInvocation, string applicationCode)[] tests)
	{
		var ediMessages = new EDIMessage[tests.Length];
		for (var t = 0; t < tests.Length; t++)
		{
			ediMessages[t] = CreateOutboundMessage(tests[t].applicationCode);
		}

		Factory.Save();
		var logger = new LoggingInformationForTesting();
		OutboundMessageProcessor.ProcessMessages(logger, new CancellationToken());

		for (var t = 0; t < tests.Length; t++)
		{
			ediMessages[t].Reload();
			if (tests[t].expectedInvocation)
			{
				AssertNotEquals($"{tests[t].applicationCode}: EM_Status", EDIMessage.Status.Queued, ediMessages[t].EM_Status);
			}
			else
			{
				AssertEquals($"{tests[t].applicationCode}: EM_Status", EDIMessage.Status.Queued, ediMessages[t].EM_Status);
			}
		}

		EDIMessage CreateOutboundMessage(ZString applicationCode)
		{
			var message = Factory.New<CHEDIMessage>();
			message.EM_ApplicationCode = applicationCode;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Queued;
			return message;
		}
	}
}
