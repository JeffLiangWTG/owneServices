using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(CAQueryMessageCollection))]
	sealed class CAQueryMessageCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCreateAdditionalQuery()
		{
			AddMessage(MessageTypeList.Codes.B3CUSDEC, EDIMessage.Direction.Transmit);
			AddMessage(MessageTypeList.Codes.EDIRelease, EDIMessage.Direction.Transmit);
			var message = AddMessage(MessageTypeList.Codes.Query, EDIMessage.Direction.Transmit);
			var message1 = AddMessage(MessageTypeList.Codes.SyntaxError, EDIMessage.Direction.Transmit);

			var coll = new CAQueryMessageCollection(Factory);
			coll.LoadWithMoreFiltering(new ZQuery(EDIMessageSchema.EM_Status, "!"));

			AssertEquals(2, coll.Count);
			AssertEquals(true, coll.Contains(message));
			AssertEquals(true, coll.Contains(message1));
		}

		EDIMessage AddMessage(string messageType, string direction)
		{
			var message = Factory.New<EDIMessage>();
			message.EM_MessageType = messageType;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message.EM_ReceiveTransmit = direction;
			message.EM_Status = "!";
			return message;
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new CAQueryMessageCollection(Factory);
	}
}
