using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test;

[TestedType(typeof(Q05OutboundEDIMessage))]
public class Q05OutboundEDIMessageTest : EDIMessageTest
{
	public void TestMessageDefaults()
	{
		var message = Factory.New<Q05OutboundEDIMessage>();
		CombineAssertions(() =>
		{
			AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.IC2, message.EM_ApplicationCode);
			AssertEquals("EM_MessageType", MessageTypes.Codes.Q05, message.EM_MessageType);
		});
	}

	[TestDate(2024, 12, 1)]
	public void TestReplaceLRNPlaceholders()
	{
		var manifest = Factory.New<AsycudaManifestHeader>();
		var message = Factory.New<Q05OutboundEDIMessage>();

		message.EM_MessageText = $"<message>{ICS2OutboundEDIMessage.LRNPlaceHolderHtml}<message>";
		message.EM_LinkedObject = manifest;
		manifest.Messages.Add(message);

		message.Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("Generated reference should be populated in EM_ExternalReferenceNumber.", "EDIBNE2400000000001", message.EM_ExternalReferenceNumber);
			AssertContains("Generated reference should be populated in message, replacing placeholder.", message.EM_ExternalReferenceNumber, message.EM_MessageText);
			AssertEquals("LRN should not be updated for manifest.", string.Empty, manifest.LocalReferenceNumber);
		});
	}
}
