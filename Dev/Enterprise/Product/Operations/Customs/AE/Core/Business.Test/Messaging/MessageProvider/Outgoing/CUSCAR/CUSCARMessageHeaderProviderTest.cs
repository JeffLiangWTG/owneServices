using Enterprise.Edifact.D23A.Elements;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class CUSCARMessageHeaderProviderTest : TestCase
{
	public void TestProperties()
	{
		var provider = new CUSCARMessageHeaderProvider(MessageTypeList.CustomsCargoReportMessage);
		CombineAssertions(() =>
		{
			AssertEquals("ReferenceNumber", AEConstants.Messaging.Placeholders.MessageNumber, provider.ReferenceNumber);
			AssertEquals("MessageType", MessageTypeList.CustomsCargoReportMessage.ToString(), provider.MessageType);
			AssertEquals("MessageVersion", MessageVersionNumberList.DraftVersionUnEdifactDirectory.ToString(), provider.MessageVersion);
			AssertEquals("MessageReleaseNumber", AEConstants.Messaging.EDIFACT.MessageReleaseNumber, provider.MessageReleaseNumber);
			AssertEquals("ControllingAgency", ControllingAgencyCodedList.UnCefact.ToString(), provider.ControllingAgency);
		});
	}
}
