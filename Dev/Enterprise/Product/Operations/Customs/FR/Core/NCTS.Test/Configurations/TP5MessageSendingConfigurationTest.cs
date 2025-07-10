using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.FR.NCTS.Configurations;
using Enterprise.Customs.FR.NCTS.Messaging;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	sealed class TP5MessageSendingConfigurationTest : EU.NCTS.Business.Testing.MessageSendingConfigurationAbstractTest<TP5MessageSendingConfiguration>
	{
		public override void TestGetNewNctsMessageSendingObjectParent()
		{
			var nctsHeader = GetNewDepartureHeader();
			AssertType<TP5MessageSendingObjectParent>(configuration.GetNewNctsHeaderMessageSendingObjectParent(nctsHeader));
		}

		public override void TestMessageTypeList()
		{
			var configuration = new TP5MessageSendingConfiguration();

			AssertMessageTypeList(ZString.Empty,  new string[] { TP5MessageTypeList.Codes.CC015C }, configuration);
			AssertMessageTypeList(NCTS5DepartureCustomsStatusList.Codes.Acknowledged, new string[] { TP5MessageTypeList.Codes.CC013C, TP5MessageTypeList.Codes.CC014C }, configuration);
			AssertMessageTypeList(NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested, new string[] { TP5MessageTypeList.Codes.CC013C, TP5MessageTypeList.Codes.CC014C, }, configuration);
			AssertMessageTypeList(NCTS5DepartureCustomsStatusList.Codes.DecisionToControl, new string[] { ZString.Empty }, configuration);
			AssertMessageTypeList(NCTS5DepartureCustomsStatusList.Codes.AdditionalDocumentsRequest, new string[] { TP5MessageTypeList.Codes.CC015C }, configuration);
			AssertMessageTypeList(NCTS5DepartureCustomsStatusList.Codes.IntentionToControl, new string[] { ZString.Empty }, configuration);
			AssertMessageTypeList(NCTS5DepartureCustomsStatusList.Codes.Cancelled, new string[] { ZString.Empty }, configuration);
			AssertMessageTypeList(NCTS5DepartureCustomsStatusList.Codes.CancellationRequestedByCustoms, new string[] { ZString.Empty }, configuration);
			AssertMessageTypeList(NCTS5DepartureCustomsStatusList.Codes.DiscrepanciesAtDestination, new string[] { ZString.Empty }, configuration);
			AssertMessageTypeList(NCTS5DepartureCustomsStatusList.Codes.UnderEnquiry, new string[] { TP5MessageTypeList.Codes.CC141C }, configuration);
			AssertMessageTypeList(NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid, new string[] { TP5MessageTypeList.Codes.CC013C, TP5MessageTypeList.Codes.CC014C }, configuration);
			AssertMessageTypeList(NCTS5DepartureCustomsStatusList.Codes.IncidentRegistered, new string[] { ZString.Empty }, configuration);
			AssertMessageTypeList(NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, new string[] { TP5MessageTypeList.Codes.CC013C, TP5MessageTypeList.Codes.CC014C, TP5MessageTypeList.Codes.CC034C }, configuration);
			AssertMessageTypeList(NCTS5DepartureCustomsStatusList.Codes.PreLodged, new string[] { TP5MessageTypeList.Codes.CC013C, TP5MessageTypeList.Codes.CC014C, TP5MessageTypeList.Codes.CC034C, TP5MessageTypeList.Codes.CC170C }, configuration);
			AssertMessageTypeList(NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit, new string[] { TP5MessageTypeList.Codes.CC014C, TP5MessageTypeList.Codes.CC034C }, configuration);
			AssertMessageTypeList(NCTS5DepartureCustomsStatusList.Codes.RequestForAdvice, new string[] { TP5MessageTypeList.Codes.CC015C }, configuration);
			AssertMessageTypeList(NCTS5DepartureCustomsStatusList.Codes.ReleaseRequestHasBeenRequested, new string[] { TP5MessageTypeList.Codes.CC015C }, configuration);
			AssertMessageTypeList(NCTS5DepartureCustomsStatusList.Codes.RejectedAtOrigin, new string[] { TP5MessageTypeList.Codes.CC015C }, configuration);
			AssertMessageTypeList(NCTS5DepartureCustomsStatusList.Codes.UnderRecoveryProcedure, new string[] { ZString.Empty }, configuration);
			AssertMessageTypeList(NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed, new string[] { ZString.Empty }, configuration);
			AssertMessageTypeList("OTH", new string[] { ZString.Empty }, configuration);

			AssertMessageTypeList(ZString.Empty, new string[] { TP5MessageTypeList.Codes.CC007C }, configuration, isArrival: true);
			AssertMessageTypeList(NCTS5ArrivalCustomsStatusList.Codes.Canceled, new string[] { ZString.Empty }, configuration, isArrival: true);
			AssertMessageTypeList(NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease , new string[] { ZString.Empty }, configuration, isArrival: true);
			AssertMessageTypeList(NCTS5ArrivalCustomsStatusList.Codes.ClosedPartialRelease, new string[] { TP5MessageTypeList.Codes.CC007C }, configuration, isArrival: true);
			AssertMessageTypeList(NCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionPartialRelease, new string[] { TP5MessageTypeList.Codes.CC007C }, configuration, isArrival: true);
			AssertMessageTypeList(NCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease, new string[] { TP5MessageTypeList.Codes.CC007C }, configuration, isArrival: true);
			AssertMessageTypeList(NCTS5ArrivalCustomsStatusList.Codes.RequestForAdvice, new string[] { TP5MessageTypeList.Codes.CC007C }, configuration, isArrival: true);
			AssertMessageTypeList(NCTS5ArrivalCustomsStatusList.Codes.ReleaseRequestHasBeenRequested, new string[] { TP5MessageTypeList.Codes.CC007C }, configuration, isArrival: true);
			AssertMessageTypeList(NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted, new string[] { TP5MessageTypeList.Codes.CC044C }, configuration, isArrival: true);
			AssertMessageTypeList(NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks, new string[] { TP5MessageTypeList.Codes.CC044C }, configuration, isArrival: true);
			AssertMessageTypeList(NCTS5ArrivalCustomsStatusList.Codes.UnderRecoveryProcedure, new string[] { ZString.Empty }, configuration, isArrival: true);
			AssertMessageTypeList("OTH", new string[] { TP5MessageTypeList.Codes.CC007C }, configuration, isArrival: true);
		}

		void AssertMessageTypeList(string status, string[] messageAvailable, TP5MessageSendingConfiguration configuration, bool isArrival = false)
		{
			var nctsHeader = isArrival ? GetNewArrivalHeader() : GetNewDepartureHeader();
			var statusToShow = string.IsNullOrEmpty(status) ? "empty" : status;

			if (isArrival)
			{
				nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = status;
			}
			else
			{
				nctsHeader.MovementHeader.BM_CustomsStatus = status;
			}

			var messageTypeList = configuration.MessageTypeList(nctsHeader).GetAllCodes();
			if (messageAvailable[0] == ZString.Empty)
			{
				AssertEquals($"MessageType list should return no result when BM_CustomsStatus is {statusToShow}.", 0, messageTypeList.Length);
			}
			else
			{
				AssertContainsExactElementsInAnyOrder($"MessageType list should contain {string.Join(", ", messageAvailable)} when BM_CustomsStatus is {statusToShow}.", messageAvailable, messageTypeList);
			}
		}

		public override void TestSetDefaultMessageType()
		{
			var nctsHeader = GetNewDepartureHeader();
			var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
			configuration.SetDefaultMessageType(sendingObject);
			AssertEquals("Default MessageType should be CC015C for departure.", TP5MessageTypeList.Codes.CC015C, sendingObject.MessageType);

			nctsHeader.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.Cancelled;
			configuration.SetDefaultMessageType(sendingObject);
			AssertEquals("Default MessageType should be empty for departure if customs status is equal to Cancelled as MessageTypeList is empty.", ZString.Empty, sendingObject.MessageType);

			nctsHeader.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.PreLodged;
			configuration.SetDefaultMessageType(sendingObject);
			AssertEquals("Default MessageType should be empty for departure if customs status is equal to PreLodged as MessageTypeList has more than 1 element.", ZString.Empty, sendingObject.MessageType);

			nctsHeader = GetNewArrivalHeader();
			sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
			configuration.SetDefaultMessageType(sendingObject);
			AssertEquals("Default MessageType should be CC007C for arrival.", TP5MessageTypeList.Codes.CC007C, sendingObject.MessageType);
		}

		public override void TestGetShouldSendDefault()
		{
			var nctsHeader = GetNewDepartureHeader();
			AssertEquals("Base implementation", true, configuration.GetShouldSendDefault(new NctsHeaderMessageSendingObject(nctsHeader)));
		}

		public override void TestShowJustification()
		{
			var nctsHeader = GetNewDepartureHeader();
			AssertEquals("Base implementation", expected: false, configuration.ShowJustification(nctsHeader));
		}

		NctsHeader GetNewDepartureHeader()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			return nctsHeader;
		}

		NctsHeader GetNewArrivalHeader()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			return nctsHeader;
		}

		protected override Type ExpectedNctsHeaderMessageSendingObjectValidationDeciderType => typeof(TP5MessageSendingObjectValidationDecider);
	}
}
