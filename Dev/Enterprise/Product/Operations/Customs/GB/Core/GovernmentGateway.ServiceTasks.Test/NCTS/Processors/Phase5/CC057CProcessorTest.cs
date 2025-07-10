using System.Collections.Generic;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.cc057c;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.ctypes;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.tcl;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.NCTS;
using Moq;
using Moq.Protected;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.Processors.Phase5.Testing
{
	sealed class CC057CProcessorTest : NctsBaseProcessorTest<Cc057CType>
	{
		protected override IEnumerable<MessageProcessorTestCase> ProcessorTestCases
		{
			get
			{
				yield return new MessageProcessorTestCase
				{
					SetUpHeader = header =>
					{
						header.BH_HeaderType = NctsMovementType.Codes.Arrival;
						header.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
					},
					MRN = "23GB000060KNEJKEJ3",
					CorrelationIdentifier = "52865659620996",
					IncomingMessageText = responseHelper.GetEmbeddedResourceFile("CC057C_Message.xml"),
					MessageSubType = "57C",
					ExpectedNewMessageStatus = LogicalStatusList.Codes.Invalid,
					ExpectedNewMessageInterpretation = @"Declaration received an error for type 007 on 02/08/2023 10:52:38</br>
Reason: 12 </br>
</br>
Functional error code: 92</br>
Reason: N/A</br>
Attribute: /CC007C</br>
Element in declaration now contains the value:"
					,
					ExpectedMovementType = NctsMovementType.Codes.Arrival,
					ExpectedAcceptMovementType = NctsMovementType.Codes.Arrival,
				};
			}
		}

		public new void TestMessageShouldBeDiscarded()
		{
			var header = responseHelper.SetupMessagesForTest(Factory, NctsMovementType.Codes.DepartureAndArrival, string.Empty, ZString.Empty);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.BH_HeaderType = NctsMovementType.Codes.Arrival;
			var ediMessage = Factory.New<NCTSInboundEDIMessage>();
			ediMessage.EM_MessageSubType = "057";
			Factory.Save();

			var messageObject = new Cc057CType();
			messageObject.MessageType = MessageTypes.Cc057C;
			messageObject.TransitOperation = new TransitOperationType21();

			var mockProcessor = new Mock<CC057CProcessor>(new TestServiceLogger(), new BatchProcessor.LoggingInformation());
			mockProcessor.CallBase = true;
			mockProcessor.Protected().Setup<Cc057CType>("DeserializeMessage", ItExpr.IsAny<string>()).Returns(messageObject);
			var processor = mockProcessor.Object;
			processor.ProcessMessage(ediMessage);

			messageObject.TransitOperation.BusinessRejectionType = "007";
			header.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
			header.ArrivalMovementHeader.BM_CustomsStatus = ZString.Empty;
			var result = processor.MessageShouldBeDiscarded(header, out _);
			AssertEquals("Rejection 007 BH_MessageStatus=SNT: ShouldBeDiscarded", expected: false, result);

			header.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Acknowledged;
			result = processor.MessageShouldBeDiscarded(header, out _);
			AssertEquals("Rejection 007 BH_MessageStatus=ACK: ShouldBeDiscarded", expected: false, result);

			header.ArrivalMovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.Acknowledged;
			result = processor.MessageShouldBeDiscarded(header, out var reason);
			AssertEquals("Rejection 007 BM_CustomsStatus=ACK: ShouldBeDiscarded", expected: true, result);
			AssertContains("Rejection 007 BM_CustomsStatus=ACK: Reason", "The message was discarded", reason);

			messageObject.TransitOperation.BusinessRejectionType = "044";
			header.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
			header.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
			result = processor.MessageShouldBeDiscarded(header, out _);
			AssertEquals("Rejection 044 BH_MessageStatus=SNT/BM_CustomsStatus=UAP: ShouldBeDiscarded", expected: false, result);

			header.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Acknowledged;
			result = processor.MessageShouldBeDiscarded(header, out _);
			AssertEquals("Rejection 044 BH_MessageStatus=ACK/BM_CustomsStatus=UAP: ShouldBeDiscarded", expected: false, result);

			header.ArrivalMovementHeader.BM_MessageStatus = NctsMessageStatusList.Codes.Ok;
			result = processor.MessageShouldBeDiscarded(header, out _);
			AssertEquals("Rejection 044 BH_MessageStatus=MOK/BM_CustomsStatus=UAP: ShouldBeDiscarded", expected: false, result);

			header.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks;
			result = processor.MessageShouldBeDiscarded(header, out _);
			AssertEquals("Rejection 044 BH_MessageStatus=ACK/BM_CustomsStatus=ULR: ShouldBeDiscarded", expected: false, result);

			header.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
			result = processor.MessageShouldBeDiscarded(header, out _);
			AssertEquals("Rejection 044 BH_MessageStatus=SNT/BM_CustomsStatus=ULR: ShouldBeDiscarded", expected: false, result);

			header.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
			result = processor.MessageShouldBeDiscarded(header, out reason);
			AssertEquals("Rejection 044 BH_MessageStatus=SNT/BM_CustomsStatus=CL1: ShouldBeDiscarded", expected: true, result);
			AssertContains("Rejection 044 BH_MessageStatus=SNT/BM_CustomsStatus=CL1: Reason", "The message was discarded", reason);

			header.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Failed;
			header.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
			result = processor.MessageShouldBeDiscarded(header, out reason);
			AssertEquals("Rejection 044 BH_MessageStatus=FAL/BM_CustomsStatus=UAP: ShouldBeDiscarded", expected: true, result);
			AssertContains("Rejection 044 BH_MessageStatus=FAL/BM_CustomsStatus=UAP: Reason", "The message was discarded", reason);
		}
	}
}
