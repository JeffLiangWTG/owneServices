using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(ManifestForwardEDIMessageCollection))]
	sealed class ManifestForwardEDIMessageCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCreateAdditionalQuery()
		{
			AddMessage(MessageTypeList.Codes.ACIHouseBill, EDIMessage.Direction.Transmit);
			var message = AddMessage(MessageTypeList.Codes.ACIHouseBill, EDIMessage.Direction.Receive);
			message.EM_MessageSubType = MessageTypeList.Codes.ManifestForwardHouse;

			var coll = new ManifestForwardEDIMessageCollection(Factory);
			coll.LoadWithMoreFiltering(new ZQuery(EDIMessageSchema.EM_Status, "!"));

			AssertEquals(1, coll.Count);
			AssertEquals(true, coll.Contains(message));
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new ManifestForwardEDIMessageCollection(Factory);

		EDIMessage AddMessage(string messageType, string direction)
		{
			var message = Factory.New<EDIMessage>();
			message.EM_MessageType = messageType;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAACI;
			message.EM_ReceiveTransmit = direction;
			message.EM_Status = "!";
			return message;
		}
	}
}
