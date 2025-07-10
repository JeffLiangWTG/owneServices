using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class MessageBlockGeneratorTest : TestCaseWithFactory
	{
		public void TestSerialiseEmptyGenerator()
		{
			AssertEquals("", new MessageBlockGenerator().Serialise());
		}

		public void TestSerialiseWithElements()
		{
			MessageBlockGenerator generator = new MessageBlockGenerator();
			DLMPermit permit = new DLMPermit();
			permit.PermitNumber = "PE342342";
			string permitSerialisation = permit.Serialise();
			generator.AddMessageBlock(permit);
			AssertEquals(permitSerialisation, generator.Serialise());

			DLMReference reference = new DLMReference();
			reference.ReferenceNumber = "REF23423";
			string referenceSerialisation = reference.Serialise();
			generator.AddMessageBlock(reference);
			AssertMultilineASCIIEquals("generator.Serialise()", permitSerialisation + System.Environment.NewLine + referenceSerialisation, generator.Serialise());
		}

		public void TestCreateMessage()
		{
			MessageBlockGenerator generator = new MessageBlockGenerator();
			EDIMessage message = generator.CreateMessage(Factory);
			AssertEquals(EDIMessage.ApplicationCodes.CACustoms, message.EM_ApplicationCode);
			AssertEquals(EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
			AssertEquals(EDIMessage.Status.Queued, message.EM_Status);
			AssertEquals(generator.Serialise().Trim(), message.EM_MessageText);
		}

		public void TestSerialiseHumanFriendly()
		{
			MessageBlockGenerator generator = new MessageBlockGenerator();
			DLMPermit permit = new DLMPermit();
			permit.PermitNumber = "PE342342";
			generator.AddMessageBlock(permit);
			ZString result = generator.Serialise(true);
			Assert(result, result.Replace(" ", "").Replace("\t", "").Contains("PermitNumber(2-36):PE342342"));
		}
	}
}
