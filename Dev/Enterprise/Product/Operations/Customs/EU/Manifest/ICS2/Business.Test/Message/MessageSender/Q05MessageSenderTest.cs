using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test;

public class Q05MessageSenderTest : TestCaseWithFactory
{
	[TestDate(2024, 2, 11)]
	public void TestSendQ05Message()
	{
		var manifestHeader = Factory.New<AsycudaManifestHeader>();
		manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F24; // value doesn't matter

		Factory.Save();

		var messageSender = new Q05MessageSender(manifestHeader);
		messageSender.SendMessage();

		var message = (EDIMessage)manifestHeader.Messages.Single();

		CombineAssertions(() =>
		{
			AssertEquals("Correct message type created", "Q05", message.EM_MessageType);
			AssertEquals("AMA_MessageStatus should not be set/updated.", string.Empty, manifestHeader.AMA_MessageStatus);
		});
	}
}
