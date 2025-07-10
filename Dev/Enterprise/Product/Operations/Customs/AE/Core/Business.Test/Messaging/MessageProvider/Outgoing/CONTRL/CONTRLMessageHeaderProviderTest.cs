using Enterprise.Edifact.D23A.Elements;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class CONTRLMessageHeaderProviderTest : TestCase
{
	public void TestProperties() => CombineAssertions(() =>
	{
		var provider = new CONTRLMessageHeaderProvider();
		AssertEquals("ReferenceNumber", AEConstants.Messaging.Placeholders.MessageNumber, provider.ReferenceNumber);
		AssertEquals("MessageType", MessageTypeList.SyntaxAndServiceReportMessage.ToString(), provider.MessageType);
		AssertEquals("MessageVersion", MessageVersionNumberList.ServiceMessageVersion4Note.ToString(), provider.MessageVersion);
		AssertEquals("MessageReleaseNumber", AEConstants.Messaging.EDIFACT.CharacterEncoding, provider.MessageReleaseNumber);
		AssertEquals("ControllingAgency", ControllingAgencyCodedList.UnCefact.ToString(), provider.ControllingAgency);
	});
}
