using System;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using NctsHeaderMessageSendingObjectParent = Enterprise.Customs.GB.Business.NCTS.NctsHeaderMessageSendingObjectParent;

namespace Enterprise.Customs.GB.Business.Testing
{
	sealed class MessageSendingConfigurationTest : EU.NCTS.Business.Testing.MessageSendingConfigurationAbstractTest<MessageSendingConfiguration>
	{
		public override void TestGetNewNctsMessageSendingObjectParent()
		{
			AssertType<NctsHeaderMessageSendingObjectParent>(configuration.GetNewNctsHeaderMessageSendingObjectParent(departureHeader));
		}

		public override void TestGetShouldSendDefault() => CombineAssertions(() =>
		{
			var departureSendingObject = new NctsHeaderMessageSendingObject(departureHeader);
			AssertEquals("Departure|SNT", expected: true, configuration.GetShouldSendDefault(departureSendingObject));
			departureHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
			AssertEquals("Departure|SNT", expected: true, configuration.GetShouldSendDefault(departureSendingObject));
			departureHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			AssertEquals("Departure|SNT", expected: true, configuration.GetShouldSendDefault(departureSendingObject));

			var arrivalHeaderSendingObject = new NctsHeaderMessageSendingObject(arrivalHeader);
			AssertEquals("Arrival|SNT", expected: true, configuration.GetShouldSendDefault(arrivalHeaderSendingObject));
			arrivalHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
			AssertEquals("Arrival|SNT", expected: true, configuration.GetShouldSendDefault(arrivalHeaderSendingObject));
			arrivalHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			AssertEquals("Arrival|SNT", expected: true, configuration.GetShouldSendDefault(arrivalHeaderSendingObject));
		});

		public override void TestMessageTypeList()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Departure", "013, 014, 015, 170", configuration.MessageTypeList(departureHeader).CodesAsString);
				AssertEquals("Arrival", "007, 044", configuration.MessageTypeList(arrivalHeader).CodesAsString);
			});
		}

		public override void TestSetDefaultMessageType()
		{
			var departureMoveHeader = departureHeader.MovementHeader;
			AssertEquals("Departure|SubApplicationCode", NctsMoveHeaderType.Codes.Departure, departureMoveHeader.BM_SubApplicationCode);

			var departureSendingObject = new NctsHeaderMessageSendingObject(departureHeader);
			configuration.SetDefaultMessageType(departureSendingObject);
			AssertEquals("Departure|MessageType", "015", departureSendingObject.MessageType);

			departureMoveHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.PreLodged;
			departureMoveHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Amendment;
			configuration.SetDefaultMessageType(departureSendingObject);
			AssertEquals("Departure|MessageType", "170", departureSendingObject.MessageType);

			departureMoveHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
			configuration.SetDefaultMessageType(departureSendingObject);
			AssertEquals("Departure|MessageType", "013", departureSendingObject.MessageType);

			var arrivalMoveHeader = arrivalHeader.ArrivalMovementHeader;
			AssertEquals("Arrival|SubApplicationCode", NctsMoveHeaderType.Codes.Arrival, arrivalMoveHeader.BM_SubApplicationCode);

			var arrivalSendingObject = new NctsHeaderMessageSendingObject(arrivalHeader);
			configuration.SetDefaultMessageType(arrivalSendingObject);
			AssertEquals("Arrival|MessageType", "007", arrivalSendingObject.MessageType);

			arrivalMoveHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.UnloadingPermissionGranted;
			configuration.SetDefaultMessageType(arrivalSendingObject);
			AssertEquals("Arrival|MessageType", ZString.Empty, arrivalSendingObject.MessageType);

			arrivalMoveHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
			arrivalMoveHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Arrival;
			configuration.SetDefaultMessageType(arrivalSendingObject);
			AssertEquals("Arrival|MessageType", "044", arrivalSendingObject.MessageType);
		}

		public override void TestShowJustification()
		{
			AssertEquals(expected: true, configuration.ShowJustification(departureHeader));
		}

		protected override Type ExpectedNctsHeaderMessageSendingObjectValidationDeciderType => typeof(NctsHeaderMessageSendingObjectValidationDecider);

		protected override void SetUp()
		{
			base.SetUp();
			departureHeader = Factory.New<NctsHeader>();
			departureHeader.SetMovementType(NctsMovementType.Codes.Departure);
			arrivalHeader = Factory.New<NctsHeader>();
			arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		}

		NctsHeader departureHeader, arrivalHeader;
	}
}
