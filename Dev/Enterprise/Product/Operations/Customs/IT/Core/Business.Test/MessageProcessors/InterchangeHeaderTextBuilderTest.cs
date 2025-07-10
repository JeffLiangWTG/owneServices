using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class InterchangeHeaderTextBuilderTest : TestCase
{
	public void TestBuilder()
	{
		var interchangeHeader = new InterchangeHeaderTextBuilder()
			.AppendAccountNumber("123456")
			.AppendFileName("12345678.R01")
			.AppendHeader("HEADER TEXT")
			.AppendMessageType("MSG TYPE")
			.AppendNode("845A")
			.AppendStaff("RFI")
			.AppendTrackingID(new ZGuid("8C2900CD-E6F1-482E-9D4A-0D4C60BA1292"))
			.Build();
		AssertEquals(
			"<ITMessage>" +
				"<AccountNumber>123456</AccountNumber>" +
				"<FileName>12345678.R01</FileName>" +
				"<Header>HEADER TEXT</Header>" +
				"<MessageType>MSG TYPE</MessageType>" +
				"<Node>845A</Node>" +
				"<Staff>RFI</Staff>" +
				"<eHubTrackingIDFromSentInterchange>8c2900cd-e6f1-482e-9d4a-0d4c60ba1292</eHubTrackingIDFromSentInterchange>" +
			"</ITMessage>", interchangeHeader);
	}

	public void TestBuilderDuplicatedEntries()
	{
		var interchangeHeader = new InterchangeHeaderTextBuilder()
			.AppendAccountNumber("123456")
			.AppendAccountNumber("654321")
			.Build();
		AssertEquals(
			"<ITMessage>" +
				"<AccountNumber>123456</AccountNumber>" +
				"<AccountNumber>654321</AccountNumber>" +
			"</ITMessage>", interchangeHeader);
	}

	public void TestBuilderEmptyValueEntry()
	{
		var interchangeHeader = new InterchangeHeaderTextBuilder()
			.AppendAccountNumber("")
			.Build();
		AssertEquals(
			"<ITMessage>" +
				"<AccountNumber />" +
			"</ITMessage>", interchangeHeader);
	}
}
