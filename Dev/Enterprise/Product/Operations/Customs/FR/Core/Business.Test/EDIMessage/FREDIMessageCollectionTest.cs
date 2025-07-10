using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	[TestedType(typeof(FREDIMessageCollection))]
	public class FREDIMessageCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<FREDIMessage>();
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			return new FREDIMessageCollection(entryHeader);
		}

		public void TestLastIncomingNotEmptyEntryStatusMessage()
		{
			var collection = (FREDIMessageCollection)GetCollectionToTest();
			
			var errorMessage = Factory.New<DeltaCImportFREDIMessage>();
			errorMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			errorMessage.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.ErrorResponseMessageWithoutNodeEtat.xml");
			errorMessage.EM_SystemCreateTimeUtc = new ZDateTime(2024, 03, 11, DateTimeKind.Utc);
			collection.Add(errorMessage);

			AssertNull(collection.LastIncomingMessageWithValuedEntryStatus);

			var message = Factory.New<DeltaCImportFREDIMessage>();
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessage.xml");
			message.EM_SystemCreateTimeUtc = new ZDateTime(2024, 03, 12, DateTimeKind.Utc);
			collection.Add(message);

			AssertNotNull(collection.LastIncomingMessageWithValuedEntryStatus);

			var lastMessage = Factory.New<DeltaCImportFREDIMessage>();
			lastMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			lastMessage.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessage.xml");
			lastMessage.EM_SystemCreateTimeUtc = new ZDateTime(2024, 03, 13, DateTimeKind.Utc);
			collection.Add(lastMessage);

			AssertEquals("LastIncomingMessageWithValuedEntryStatus must return the latest received message with non empty entry status", lastMessage.PK, collection.LastIncomingMessageWithValuedEntryStatus.PK);
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
