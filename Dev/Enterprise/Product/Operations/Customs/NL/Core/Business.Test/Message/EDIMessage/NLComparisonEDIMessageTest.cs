using System;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(NLComparisonEDIMessage))]
class NLComparisonEDIMessageTests : EnterpriseBusinessObjectTestCase
{
	public void TestSetDefaultValues()
	{
		var message = Factory.New<NLComparisonEDIMessage>();
		AssertEquals(EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
		AssertEquals(EDIMessageStatusList.Codes.Acknowledged, message.EM_Status);
	}

	public void TestEM_MessageInterpretation()
	{
		var message = Factory.New<NLComparisonEDIMessage>();
		AssertEquals(FormattableString.Invariant($"This is a snapshot of the state of your entry at {message.EM_SystemCreateTimeUtc} UTC, which was used to create an amendment/CRI request.  The details of what was sent to DMS should be viewed on the AMD/CRI message."), message.EM_MessageInterpretation);
	}
}
