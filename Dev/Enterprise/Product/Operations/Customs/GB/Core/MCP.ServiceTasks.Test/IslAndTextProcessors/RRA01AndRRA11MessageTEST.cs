using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.GB.MCP.ServiceTasks.RRA01AndRRA11.Testing
{
	public sealed class RRA01AndRRA11MessageTEST : TestCaseWithFactory
	{
		public void TestGetMessageIdentifierString()
		{
			var message = new RRAMessageForTest("=RRA01");
			message.SetValidMessageIdentifiers(new List<ZString> { "RRA01", "RRA02", "RRA03" });
			AssertEquals(message.InvalidMessageException, "This is the RRA01, RRA02 and RRA03 processor, and you passed me something that is not an RRA01, RRA02 or RRA03. It did not start with '=RRA01', '=RRA02' or '=RRA03'. It was [=RRA01]");
			AssertEquals(message.MissingDelimiterException, "This is the RRA01, RRA02 and RRA03 processor. The message that I was asked to parse did not contain any delimiters.");
			AssertEquals(message.MessageLengthException, "This is the RRA01, RRA02 and RRA03 processor. The message that I was asked to parse did not contain the expected number of elements.");

			message.SetValidMessageIdentifiers(new List<ZString> { "RRA01", "RRA02" });
			AssertEquals(message.InvalidMessageException, "This is the RRA01 and RRA02 processor, and you passed me something that is not an RRA01 or RRA02. It did not start with '=RRA01' or '=RRA02'. It was [=RRA01]");
			AssertEquals(message.MissingDelimiterException, "This is the RRA01 and RRA02 processor. The message that I was asked to parse did not contain any delimiters.");
			AssertEquals(message.MessageLengthException, "This is the RRA01 and RRA02 processor. The message that I was asked to parse did not contain the expected number of elements.");

			message.SetValidMessageIdentifiers(new List<ZString> { "RRA01" });
			AssertEquals(message.InvalidMessageException, "This is the RRA01 processor, and you passed me something that is not an RRA01. It did not start with '=RRA01'. It was [=RRA01]");
			AssertEquals(message.MissingDelimiterException, "This is the RRA01 processor. The message that I was asked to parse did not contain any delimiters.");
			AssertEquals(message.MessageLengthException, "This is the RRA01 processor. The message that I was asked to parse did not contain the expected number of elements.");
		}
	}

	class RRAMessageForTest : RRA01AndRRA11Message
	{
		public RRAMessageForTest(ZString messageTextAsReceivedFromCustoms) : base(messageTextAsReceivedFromCustoms)
		{
		}

		public void SetValidMessageIdentifiers(List<ZString> messageIdentifiers) => ValidMessageIdentifiers = messageIdentifiers;

		protected override List<ZString> ValidMessageIdentifiers { get; set; }

		public new ZString InvalidMessageException => base.InvalidMessageException;

		public new ZString MissingDelimiterException => base.MissingDelimiterException;

		public new ZString MessageLengthException => base.MessageLengthException;
	}
}
