using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business.RexOwnershipSoap;
using Enterprise.Edifact;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class NEXDOCMessageHelperTest : TestCaseWithFactory
	{
		public void TestTryDeserialize()
		{
			var message = CreateNEXDOCMessage(
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
</UniversalInterchange>");

			var interchangeDoc = NEXDOCMessageHelper.TryDeserialize<NEXDOCAcknowledgeInterchange>(message.EM_MessageText, "UniversalInterchange");
			AssertType<NEXDOCAcknowledgeInterchange>(interchangeDoc);
		}

		public void TestTryDeserializeThrowsOnFailure()
		{
			var message = CreateNEXDOCMessage(
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<n1:UniversalInterchange>
</n1:UniversalInterchange>");

			var exception = AssertExceptionThrown<InvalidFormatException>(() => NEXDOCMessageHelper.TryDeserialize<NEXDOCAcknowledgeInterchange>(message.EM_MessageText, "UniversalInterchange"));
			AssertContains("Exception Message",
@"Corrupted or Malformed Response Message. Cannot Process.
'n1' is an undeclared prefix. Line 2, position 2.", exception.Message);
		}

		[ExpectNoExceptions]
		public void TestSerializeShouldNotCauseStackOverflowExceptionWhenReflectingEnums()
		{
			var forwardOwnership = new RexForwardOwnership
			{
				holdUntilStatus = ForwardCompletionStatusType.COMP
			};
			forwardOwnership.Serialize();
		}

		EDIMessage CreateNEXDOCMessage(ZString messageText)
		{
			var message = Factory.New<EDIMessage>();
			message.EM_MessageType = EDIMessage.ApplicationCodes.NEXDOCS;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = messageText;
			return message;
		}
	}
}
