using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class CONTRLDataProviderTest : Customs.Business.Testing.DataProviderTestCase<CONTRLDataProvider, ICONTRLDataProvider>
{
	[ExpectNoExceptions]
	public void TestConstructor()
	{
		NUnit.Framework.Assert.Throws<ArgumentException>(() => new CONTRLDataProvider(ZString.Empty));
	}

	public void TestOutgoingAccessReference()
	{
		AssertEquals(ExpectedOutgoingAccessReference, Provider.OutgoingAccessReference);
	}

	public void TestInterchangeResponse() => AssertType<CONTRLInterchangeResponseProvider>(Provider.InterchangeResponse);

	public void TestMessageResponse() => AssertType<CONTRLMessageResponseProvider>(Provider.MessageResponse);

	protected override CONTRLDataProvider GetProvider()
	{
		var messageText = "UNH+102931H0000001+CONTRL:4:2:UN'"
			+ $"UCI+{ExpectedOutgoingAccessReference}+++4+2+UNB+2:5'"
			+ "UCM+083500H2222222+CUSCAR:D:23A+4+12+UNB+3:4'"
			+ "UNT+3+102931H0000001'";
		return new CONTRLDataProvider(messageText);
	}

	const string ExpectedOutgoingAccessReference = "062813B0000010";
}
