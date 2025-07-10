using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(StatusRequestCollection))]
	class StatusRequestCollectionTest : ActiveBusinessObjectCollectionTestCase<StatusRequestCollection>
	{
		public void TestFilteredBranch()
		{
			var currentBranchStatusRequest = Factory.New<StatusRequest>();
			currentBranchStatusRequest.EM_ApplicationCode = ApplicationCodes.DECustomsAtlasSystem;
			currentBranchStatusRequest.EM_MessageType = EDIMessageTypeList.Codes.NCTS;

			var differentBranchStatusRequest = Factory.New<StatusRequest>();
			differentBranchStatusRequest.EM_ApplicationCode = ApplicationCodes.DECustomsAtlasSystem;
			differentBranchStatusRequest.EM_MessageType = EDIMessageTypeList.Codes.NCTS;
			differentBranchStatusRequest.EM_GB = GlbCompany.CurrentCompany.Branches.AddNew().PK;

			CombineAssertions(() =>
			{
				AssertEquals("Current Branch", true, currentBranchStatusRequest.MatchesFilter(statusRequestCollection.CompleteFilter));
				AssertEquals("Different Branch", false, differentBranchStatusRequest.MatchesFilter(statusRequestCollection.CompleteFilter));
			});
		}

		public void TestDifferentAtlasMessageFiltered()
		{
			var temporaryStorageMessage = Factory.New<AtlasEDIMessage>();
			temporaryStorageMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			temporaryStorageMessage.EM_MessageType = EDIMessageTypeList.Codes.TemporaryStorage;
			AssertEquals(false, temporaryStorageMessage.MatchesFilter(statusRequestCollection.CompleteFilter));
		}

		public void TestExportNonRequestMessageFiltered()
		{
			var exportMessage = Factory.New<AesEDIMessage>();
			exportMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			exportMessage.EM_MessageType = EDIMessageTypeList.Codes.AES;
			exportMessage.EM_MessageSubType = ExportMessageSubTypeList.Codes.EXP;
			AssertEquals(false, exportMessage.MatchesFilter(statusRequestCollection.CompleteFilter));
		}

		public void TestExportRequestMessageFiltered()
		{
			var exportMessage = Factory.New<AesEDIMessage>();
			exportMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			exportMessage.EM_MessageType = EDIMessageTypeList.Codes.AES;
			exportMessage.EM_MessageSubType = ExportMessageSubTypeList.Codes.EXQ;
			AssertEquals(true, exportMessage.MatchesFilter(statusRequestCollection.CompleteFilter));
		}

		public void TestNctsNonRequestMessageFiltered()
		{
			var atlasEDIMessage = Factory.New<AtlasEDIMessage>();
			atlasEDIMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			atlasEDIMessage.EM_MessageType = EDIMessageTypeList.Codes.NCTS;
			atlasEDIMessage.EM_MessageSubType = NctsMessageSubTypeList.Codes.DestinationMessage;
			AssertEquals(false, atlasEDIMessage.MatchesFilter(statusRequestCollection.CompleteFilter));
		}

		public void TestNctsRequestMessageFiltered()
		{
			var atlasEDIMessage = Factory.New<AtlasEDIMessage>();
			atlasEDIMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			atlasEDIMessage.EM_MessageType = EDIMessageTypeList.Codes.NCTS;
			atlasEDIMessage.EM_MessageSubType = NctsMessageSubTypeList.Codes.StatusRequestMessage;
			AssertEquals(true, atlasEDIMessage.MatchesFilter(statusRequestCollection.CompleteFilter));
		}

		public void TestNCTSRequestMessages()
		{
			var nctsStatusRequestOutbound = Factory.New<StatusRequest>();
			nctsStatusRequestOutbound.EM_ApplicationCode = ApplicationCodes.DECustomsAtlasSystem;
			nctsStatusRequestOutbound.EM_MessageType = EDIMessageTypeList.Codes.NCTS;

			var nctsStatusRequestInbound = Factory.New<StatusRequest>();
			nctsStatusRequestInbound.EM_ReceiveTransmit = Direction.Receive;
			nctsStatusRequestInbound.EM_ApplicationCode = ApplicationCodes.DECustomsAtlasSystem;
			nctsStatusRequestInbound.EM_MessageType = EDIMessageTypeList.Codes.NCTS;

			CombineAssertions(() =>
			{
				AssertEquals("Outbound", true, nctsStatusRequestOutbound.MatchesFilter(statusRequestCollection.CompleteFilter));
				AssertEquals("Inbound", false, nctsStatusRequestInbound.MatchesFilter(statusRequestCollection.CompleteFilter));
			});
		}

		public void TestAESRequestMessages()
		{
			var aesStatusRequestOutbound = Factory.New<StatusRequest>();
			aesStatusRequestOutbound.EM_ApplicationCode = ApplicationCodes.DECustomsAesSystem;
			aesStatusRequestOutbound.EM_MessageType = EDIMessageTypeList.Codes.AES;

			var aesStatusRequestInbound = Factory.New<StatusRequest>();
			aesStatusRequestInbound.EM_ReceiveTransmit = Direction.Receive;
			aesStatusRequestInbound.EM_ApplicationCode = ApplicationCodes.DECustomsAesSystem;
			aesStatusRequestInbound.EM_MessageType = EDIMessageTypeList.Codes.AES;

			CombineAssertions(() =>
			{
				AssertEquals("Outbound", true, aesStatusRequestOutbound.MatchesFilter(statusRequestCollection.CompleteFilter));
				AssertEquals("Inbound", false, aesStatusRequestInbound.MatchesFilter(statusRequestCollection.CompleteFilter));
			});
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var statusRequest = Factory.New<StatusRequest>();
			statusRequest.EM_ApplicationCode = ApplicationCodes.DECustomsAesSystem;
			statusRequest.EM_MessageType = EDIMessageTypeList.Codes.AES;
			return statusRequest;
		}

		protected override StatusRequestCollection GetCollectionToTest() => statusRequestCollection;

		protected override void SetUp()
		{
			base.SetUp();
			statusRequestCollection = new StatusRequestCollection(Factory, GlbBranch.CurrentBranch);
		}
		StatusRequestCollection statusRequestCollection;
	}
}
