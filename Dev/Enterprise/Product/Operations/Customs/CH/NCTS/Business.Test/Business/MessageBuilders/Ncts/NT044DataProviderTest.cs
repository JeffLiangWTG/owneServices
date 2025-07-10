using System;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class NT044DataProviderTest : BaseNctsArrivalMessageDataProviderTest<NT044DataProvider, NctsHeaderArrivalMessageSendingObject>
{
	protected override NT044DataProvider CreateDataProvider() => new NT044DataProvider(MessageSendingObject);

	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Argument == null", () => new NC123DataProvider(null));
	}

	public void TestProperties() => CombineAssertions(() =>
	{
		NctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = true;
		NctsHeader.ArrivalMovementHeader.BM_UnloadingDate = new ZDateTimeOffset(2023, 5, 20);
		NctsHeader.ArrivalMovementHeader.BM_UnloadingRemarks = "remarks";

		NctsHeader.DestinationTrader.OrganisationPK = Factory.New<OrgHeader>().PK;
		NctsHeader.DestinationTrader.Organisation.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, "IMDBID123");

		AssertEquals("UnloadingConform", true, DataProvider.UnloadingConform);
		AssertEquals("UnloadingDate", new DateTime(2023, 5, 20), DataProvider.UnloadingDate);
		AssertEquals("UnloadingRemarkText", "remarks", DataProvider.UnloadingRemarkText);
		AssertEquals("UnloadingRemarkText", "IMDBID123", DataProvider.TraderAtDestinationIdentificationNumber);
	});

	public void TestTransitOperation() => CombineAssertions(() =>
	{
		AssertNotNull("Instance provided", DataProvider.TransitOperation);
		AssertSame("Instance cached", DataProvider.TransitOperation, DataProvider.TransitOperation);
	});

	public void TestTransportEquipment() => CombineAssertions(() =>
	{
		NctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = false;
		NctsHeader.ArrivalHeaderContainers.AddNew();
		AssertNotNull("Instance provided", DataProvider.TransportEquipments);
		AssertSame("Instance cached", DataProvider.TransportEquipments, DataProvider.TransportEquipments);
		AssertEquals("Count", 1, DataProvider.TransportEquipments.Count);

		ResetDataProvider();
		NctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = true;
		AssertNull("BM_NoChangesToReport = true", DataProvider.TransportEquipments);
	});

	public void TestHouseConsignment() => CombineAssertions(() =>
	{
		NctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = false;
		var bill = NctsHeader.Bills.AddNew();
		bill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		AssertNotNull("Instance provided", DataProvider.HouseConsignments);
		AssertSame("Instance cached", DataProvider.HouseConsignments, DataProvider.HouseConsignments);
		AssertEquals("Count", 1, DataProvider.HouseConsignments.Count);

		ResetDataProvider();
		NctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = true;
		AssertNull("BM_NoChangesToReport = true", DataProvider.HouseConsignments);
	});

	[TestDateIncremental(seconds: 1)]
	public void TestCorrelationIdentifier() => CombineAssertions(() =>
	{
		const string applicationReference = "Hit";

		CreateAndPopulateMessage("To early",ApplicationCodeList.Codes.CHCustomsPassar, EDIInterchange.Direction.Receive, MessageTypeCodeList.Codes.MSG, MessageSubTypeCodeList.Codes.PassarInventoryRequest, EDIMessage.Status.ProcessedOK, NctsHeader);
		CreateAndPopulateMessage(applicationReference, ApplicationCodeList.Codes.CHCustomsPassar, EDIInterchange.Direction.Receive, MessageTypeCodeList.Codes.MSG, MessageSubTypeCodeList.Codes.PassarInventoryRequest, EDIMessage.Status.ProcessedOK, NctsHeader);
		CreateAndPopulateMessage("ApplicationCode", ApplicationCodeList.Codes.CHCustomsCharteraOutput, EDIInterchange.Direction.Receive, MessageTypeCodeList.Codes.MSG, MessageSubTypeCodeList.Codes.PassarInventoryRequest, EDIMessage.Status.ProcessedOK, NctsHeader);
		CreateAndPopulateMessage("Direction", ApplicationCodeList.Codes.CHCustomsPassar, EDIInterchange.Direction.Transmit, MessageTypeCodeList.Codes.MSG, MessageSubTypeCodeList.Codes.PassarInventoryRequest, EDIMessage.Status.ProcessedOK, NctsHeader);
		CreateAndPopulateMessage("MessageType", ApplicationCodeList.Codes.CHCustomsPassar, EDIInterchange.Direction.Receive, MessageTypeCodeList.Codes.EVV, MessageSubTypeCodeList.Codes.PassarInventoryRequest, EDIMessage.Status.ProcessedOK, NctsHeader);
		CreateAndPopulateMessage("MessageSubType", ApplicationCodeList.Codes.CHCustomsPassar, EDIInterchange.Direction.Receive, MessageTypeCodeList.Codes.MSG, MessageSubTypeCodeList.Codes.PassarArrivalNotification, EDIMessage.Status.ProcessedOK, NctsHeader);
		CreateAndPopulateMessage("Status", ApplicationCodeList.Codes.CHCustomsPassar, EDIInterchange.Direction.Receive, MessageTypeCodeList.Codes.MSG, MessageSubTypeCodeList.Codes.PassarInventoryRequest, EDIMessage.Status.Error, NctsHeader);
		CreateAndPopulateMessage("LinkedObject", ApplicationCodeList.Codes.CHCustomsPassar, EDIInterchange.Direction.Receive, MessageTypeCodeList.Codes.MSG, MessageSubTypeCodeList.Codes.PassarInventoryRequest, EDIMessage.Status.ProcessedOK, Factory.New<NctsHeader>());
		Factory.Save();

		AssertEquals("CorrelationIdentifier", applicationReference, DataProvider.CorrelationIdentifier);
		AssertSame("cached", DataProvider.CorrelationIdentifier, DataProvider.CorrelationIdentifier);
	});

	EDIMessage CreateAndPopulateMessage(ZString applicationReference, ZString applicationCode, ZString receiveTransmit, ZString messageType, ZString messageSubType, ZString status, NctsHeader linkedObject)
	{
		var message = Factory.New<EDIMessage>();
		message.EM_ApplicationCode = applicationCode;
		message.EM_ReceiveTransmit = receiveTransmit;
		message.EM_MessageType = messageType;
		message.EM_GB = GlbBranch.CurrentBranch.PK;
		message.EM_MessageSubType = messageSubType;
		message.EM_Status = status;
		message.EM_LinkedObject = linkedObject;
		message.EM_LinkTable = NctsHeader.Schema.TableName;
		message.EM_IsTestMessage = true;
		message.EM_ApplicationReference = applicationReference;
		message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
		message.MessageNumberStrategy = new MessageNumberStrategyForTesting(applicationReference);
		return message;
	}
}
