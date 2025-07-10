using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(CAReleaseNotificationsCollection))]
	sealed class CAReleaseNotificationsCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCreateAdditionalQuery()
		{
			var entryHeader = Factory.New<CusEntryHeader>();

			var message1 = Factory.New<EDIMessage>();
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message1.EM_MessageType = MessageTypeList.Codes.RNSRequest;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_MessageNum = "1";// yes
			message1.EM_SystemCreateTimeUtc = ZDateTime.Now;

			var message2 = Factory.New<EDIMessage>();
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message2.EM_MessageType = MessageTypeList.Codes.RNSRequest;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message2.EM_MessageNum = "2";
			message2.EM_LinkedObject = entryHeader;
			message2.EM_SystemCreateTimeUtc = ZDateTime.Now;

			var message3 = Factory.New<EDIMessage>();
			message3.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message3.EM_MessageType = MessageTypeList.Codes.EDIRelease;
			message3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message3.EM_MessageNum = "3";// yes
			message3.EM_SystemCreateTimeUtc = ZDateTime.Now;

			var message4 = Factory.New<EDIMessage>();
			message4.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message4.EM_MessageType = MessageTypeList.Codes.EDIRelease;
			message4.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message4.EM_MessageNum = "4";
			message4.EM_SystemCreateTimeUtc = ZDateTime.Now;

			var message5 = Factory.New<EDIMessage>();
			message5.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message5.EM_MessageType = MessageTypeList.Codes.EDIRelease;
			message5.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message5.EM_MessageSubType = EDIReleaseImportEntryStatusList.Codes.Error;
			message5.EM_MessageNum = "5";// yes
			message5.EM_SystemCreateTimeUtc = ZDateTime.Now;

			var message6 = Factory.New<EDIMessage>();
			message6.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message6.EM_MessageType = MessageTypeList.Codes.EDIRelease;
			message6.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message6.EM_MessageSubType = EDIReleaseImportEntryStatusList.Codes.GoodsReleased;
			message6.EM_MessageNum = "6";//yes
			message6.EM_LinkedObject = entryHeader;
			message6.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-10);

			var message7 = Factory.New<EDIMessage>();
			message7.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message7.EM_MessageType = MessageTypeList.Codes.EDIRelease;
			message7.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message7.EM_MessageSubType = EDIReleaseImportEntryStatusList.Codes.Error;
			message7.EM_MessageNum = "7";
			message7.EM_LinkedObject = entryHeader;
			message7.EM_SystemCreateTimeUtc = ZDateTime.Now;

			var message8 = Factory.New<EDIMessage>();
			message8.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message8.EM_MessageType = MessageTypeList.Codes.EDIRelease;
			message8.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message8.EM_MessageSubType = EDIReleaseImportEntryStatusList.Codes.MessageContentRejected;
			message8.EM_MessageNum = "8";
			message8.EM_LinkedObject = entryHeader;
			message8.EM_SystemCreateTimeUtc = ZDateTime.Now;

			var message9 = Factory.New<EDIMessage>();
			message9.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message9.EM_MessageType = MessageTypeList.Codes.EDIRelease;
			message9.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message9.EM_MessageSubType = EDIReleaseImportEntryStatusList.Codes.SyntaxError;
			message9.EM_MessageNum = "9";
			message9.EM_LinkedObject = entryHeader;
			message9.EM_SystemCreateTimeUtc = ZDateTime.Now;

			var coll = new CAReleaseNotificationsCollection(Factory);
			coll.Load();
			AssertEquals(4, coll.Count);
			Assert(coll.Contains(message1));
			Assert(coll.Contains(message3));
			Assert(coll.Contains(message5));
			Assert(coll.Contains(message6));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CAReleaseNotificationsCollection(Factory);
		}
	}
}
