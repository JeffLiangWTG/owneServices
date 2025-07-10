using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Messaging.Business.Testing
{
	[TestedType(typeof(EDIMessageInterpreter))]
	class EDIMessageInterpreterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLists()
		{
			var interpreter = new EDIMessageInterpreter();
			AssertEquals("ApplicationCodes", Factory.GetCachedValue<ApplicationCodeList>(), interpreter.ApplicationCodes);
			AssertEquals("DirectionList", Factory.GetCachedValue<ReceiveTransmitList>(), interpreter.DirectionList);
		}

		public void TestGenerateMessageTextInterpretation()
		{
			var interpreter = new EDIMessageInterpreter();
			interpreter.GenerateMessageTextInterpretation();
			AssertEquals("MessageInterpretation", "No text to interpret.\r\nPlease enter Message Text.", interpreter.MessageInterpretation);
			interpreter.MessageText = "B013901sv9nt                                               HYEDUSCMT_188928     FDF266BAS05                                                                     FDF366BAS05CAFFEINE (STIMULANT);HUMAN - NON/RX SINGLE INGREDIENT;ACTIVE         Y  3901SV9NT00002";
			interpreter.GenerateMessageTextInterpretation();
			AssertEquals("MessageInterpretation", "Could not interpret message text.\r\nPlease fix all errors before trying again.", interpreter.MessageInterpretation);
			interpreter.ApplicationCode = ApplicationCodeList.Codes.USCustomsImport;
			interpreter.GenerateMessageTextInterpretation();
			AssertMultilineASCIIEquals("MessageInterpretation", @"------------------APLB------------------
 Block Number (2-3)                  :1
 Processing District Port Code (4-7) :3901
 Entry Filer Code (8-10)             :SV9
 Application Identifier (11-12)      :NT
 User Data (60-80)                   :HYEDUSCMT_188928

----------UnknownMessageBlock-----------
 Data (1-80) :FDF266BAS05

----------UnknownMessageBlock-----------
 Data (1-80) :FDF366BAS05CAFFEINE (STIMULANT);HUMAN - NON/RX SINGLE INGREDIENT;ACTIVE

------------------APLY------------------
 Processing District Port Code (4-7)                       :3901
 Entry Filer Code (8-10)                                   :SV9
 Application Identifier (11-12)                            :NT
 Number Of Transaction Detail Records In The Block (13-17) :2", interpreter.MessageInterpretation.TrimEnd());
			interpreter.MessageText = "B013901SV9NT                                               HYEDUSCMT_188928     Y   3901SV9NT00002";
			interpreter.GenerateMessageTextInterpretation();

#if NETFRAMEWORK
			var expected = @"Data Mismatch in Message.

Error reading NumberOfTransactionDetailRecordsInTheBlock from block APLY.
Specified argument was out of the range of valid values.
Parameter name: value
Deserialising to Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common.APLY.NumberOfTransactionDetailRecordsInTheBlock
Offset=12, Length=5, Value='T0000'
BlockData='Y   3901SV9NT00002                                                              '
MessageType=
";
#else
			var expected = @"Data Mismatch in Message.

Error reading NumberOfTransactionDetailRecordsInTheBlock from block APLY.
Specified argument was out of the range of valid values. (Parameter 'value')
Deserialising to Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common.APLY.NumberOfTransactionDetailRecordsInTheBlock
Offset=12, Length=5, Value='T0000'
BlockData='Y   3901SV9NT00002                                                              '
MessageType=
";
#endif

			AssertMultilineASCIIEquals("MessageInterpretation", expected, interpreter.MessageInterpretation.TrimEnd());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new EDIMessageInterpreter();
		}
	}
}
