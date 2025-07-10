using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(K84MessageCollection))]
	sealed class K84MessageCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCreateAdditionalQuery()
		{
			AddMessage(MessageTypeList.Codes.B3CUSDEC, EDIMessage.Direction.Transmit);
			var k84Message = AddMessage(MessageTypeList.Codes.K84Report, EDIMessage.Direction.Receive);
			var arlMessage = AddARLMessage();
			Factory.Save();

			var coll = new K84MessageCollection(Factory);
			coll.LoadWithMoreFiltering(new ZQuery(EDIMessageSchema.EM_Status, "!"));

			AssertEquals(2, coll.Count);
			AssertNotNull(coll.FindByPK(k84Message.PK));
			AssertNotNull(coll.FindByPK(arlMessage.PK));
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

		EDIMessage AddARLMessage()
		{
			var message = Factory.New<ARLMessage>();
			message.EM_Status = "!";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.SetSystemDefinedValue(EDIMessage.Schema.XMLCustomsMessageType, new ZString("DN"));

			return message;
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new K84MessageCollection(Factory);
	}
}
