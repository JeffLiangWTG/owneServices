using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Messaging.Business;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.FR.NCTS.ServiceTask.Testing
{
	abstract class DTBaseProcessorTest<T> : TestCaseWithFactory
		where T : CargoWise.Customs.FR.MessageDefinitions.DeltaT.INctsXmlMessage
	{
		public void TestProcessMessage()
		{
			var jobNum = 1001;
			foreach (var processorTestCase in DTMessageProcessorTestCases)
			{
				jobNum++;
				var header = frNctsReponseHelper.SetupMessagesForTest(Factory, EU.NCTS.Business.NctsMovementType.Codes.DepartureAndArrival, processorTestCase.IncomingMessageText, initialDeclarationStatus, true, EU.NCTS.Business.NctsMessageStatusList.Codes.DepartureDeclarationSent, jobNo: "BH_" + jobNum);
				processorTestCase.SetUpHeader?.Invoke(header);
				var oldDetailedDepartureStatus = header.DetailedDepartureStatusCode;
				var oldDetailedArrivalStatus = header.DetailedArrivalStatusCode;
				Factory.Save();
				var serviceLogger = new TestServiceLogger();
				var processor = new FRNctsResponseMessageProcessor(serviceLogger);
				processor.ExecuteBatch();
				var reloadedHeader = new BusinessObjectFactory().Load<NctsHeader>(header.PK);
				AssertEquals(2, reloadedHeader.Messages.Count);

				var lastIncomingMessage = reloadedHeader.Messages.LastIncomingMessage;
				AssertEquals(ExpectedMessageType, lastIncomingMessage.EM_MessageType);

				AssertEquals("Message status", processorTestCase.ExpectedNewMessageStatus, lastIncomingMessage.EM_Status);
				if (reloadedHeader.IsDepartureMovement)
				{
					AssertEquals("Declaration status", processorTestCase.ExpectedNewDepartureStatus, reloadedHeader.MovementHeader.BM_CustomsStatus);
					if (!processorTestCase.ExpectedNewDetailedDepartureStatus.IsEmpty)
					{
						AssertEquals("Detailed status", processorTestCase.ExpectedNewDetailedDepartureStatus, reloadedHeader.DetailedDepartureStatusCode);
					}
					else
					{
						AssertEquals("Detailed status", oldDetailedDepartureStatus, reloadedHeader.DetailedDepartureStatusCode);
					}
				}

				if (reloadedHeader.IsArrivalMovement)
				{
					AssertEquals("Arrival status", processorTestCase.ExpectedNewArrivalStatus, reloadedHeader.ArrivalMovementHeader.BM_CustomsStatus);

					if (!processorTestCase.ExpectedNewDetailedArrivalStatus.IsEmpty)
					{
						AssertEquals("ExpectedNewDetailedArrivalStatus not empty status", processorTestCase.ExpectedNewDetailedArrivalStatus, reloadedHeader.DetailedArrivalStatusCode);
					}
					else
					{
						AssertEquals("ExpectedNewDetailedArrivalStatus empty status", oldDetailedArrivalStatus, reloadedHeader.DetailedArrivalStatusCode);
					}
				}

				if (!string.IsNullOrEmpty(processorTestCase.ExpectedNewMessageInterpretation))
				{
					AssertXMLEquals("Message interpretation", processorTestCase.ExpectedNewMessageInterpretation.Replace("\r\n", ""), lastIncomingMessage.EM_MessageInterpretation.Replace("\r\n", ""));
				}
				processorTestCase.HeaderAssertion?.Invoke(reloadedHeader);
				processorTestCase.MessageAssertion?.Invoke(lastIncomingMessage);
				AssertEquals(ExpectedMessageType, lastIncomingMessage.EM_MessageType);

				var outgoingMessage = reloadedHeader.GetOutgoingMessage(lastIncomingMessage);
				AssertEquals(outgoingMessage.EM_Status, EDIMessage.Status.Acknowledged);
			}
		}

		protected virtual ZString ExpectedMessageType => new ZString(typeof(T).Name).KeepNumericCharacters();

		protected readonly FRNctsReponseHelper frNctsReponseHelper = new FRNctsReponseHelper();

		protected abstract IEnumerable<DTMessageProcessorTestCase> DTMessageProcessorTestCases { get; }

		protected virtual ZString initialDeclarationStatus => FR.Business.NctsTransitStatusList.Codes.Unknown;
	}
}
