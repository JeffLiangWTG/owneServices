using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.GraphEngine.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.Scheduler.GraphEngine;
using Enterprise.Scheduler.GraphEngine.Test;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.UniversalDataBuss.ServiceTasks.Testing
{
	[TestedType(typeof(UMIServiceTaskKeyGen))]
	[UseSnapshotProtection]
	class UMIServiceTaskKeyGenTest : ServiceTaskTestCase<UMIServiceTaskKeyGen>
	{
		#region Setup

		BusinessObjectFactory factory;

		protected override void SetUpCore()
		{
			base.SetUpCore();
			eAdaptorRegistry.Instance.AllowParallelUMI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			eAdaptorRegistry.Instance.ParallelUMIQueueHistoryInHours.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 24);
			eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Db.ConnectionOverrideForTest = Db.NewExtraConnectionToMainDb();
			factory = new BusinessObjectFactory();
		}

		protected override void TearDownCore()
		{
			base.TearDownCore();
			Db.ConnectionOverrideForTest.Dispose();
			Db.ConnectionOverrideForTest = null;
		}

		#endregion

		#region Collision test

		public void TestGetKeysFromUniversalEventHandleNullDataSourceInCollection()
		{
			#region inboundMessageText

			const string inboundMessageText = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
  <Event>
    <DataContext>
      <DocumentaryOverride>
        <DocumentName>Booking Request</DocumentName>
      </DocumentaryOverride>
      <DataTargetCollection>
        <DataTarget>
          <Key>C1800101819</Key>
          <Type>ForwardingConsol</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <EventTime>2018-07-31T13:27:00</EventTime>
    <EventType />
    <EventParameters>
      <Department>Carrier</Department>
      <MessageType>Booking Request</MessageType>
    </EventParameters>
    <EventReference />
    <ContextCollection>
      <Context>
        <Type>MBOLNumber</Type>
        <Value>HLCURTM180724430</Value>
      </Context>
      <Context>
        <Type>CarriersBookingReference</Type>
        <Value>50057478</Value>
      </Context>
      <Context>
        <Type>CarrierCode</Type>
        <Value>HLCU</Value>
      </Context>
      <Context>
        <Type>MessageReference</Type>
        <Value>C1800101819</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>";

			#endregion

			TestCaseHelper.ClearTable(JobShipmentSchema.Constants.TableName);
			var message = MakeUniversalEvent(inboundMessageText);

			factory.Save();

			var logger = new TestServiceLogger();
			var keyGenWorker = new UMIServiceTaskKeyGen { ServiceLogger = logger };
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				AssertNoExceptionThrown(() =>
				{
					keyGenWorker.RunTask();
					message.Reload();
				});
			}
		}

		public void TestNoExceptionFromKeyGenerationWithMultipleDataTargetElements()
		{
			#region inboundMessageText
			const string inboundMessageText = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <CodesMappedToTarget>true</CodesMappedToTarget>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingContainer</Type>
          <Key>MORU0714690</Key>
        </DataTarget>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>SCZA00015489</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <ContainerCollection>
      <Container>
        <ContainerNumber>MORU0714690</ContainerNumber>
        <LCLUnpack>2018-03-13</LCLUnpack>
      </Container>
    </ContainerCollection>
  </Shipment>
</UniversalShipment>";
			#endregion

			TestCaseHelper.ClearTable(JobShipmentSchema.Constants.TableName);
			var message = MakeUniversalShipment(inboundMessageText);

			factory.Save();

			var logger = new TestServiceLogger();
			var keyGenWorker = new UMIServiceTaskKeyGen { ServiceLogger = logger };
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				keyGenWorker.RunTask();
			}

			message.Reload();
			CombineAssertions(delegate
			{
				AssertEquals("Should be left Queued if Key Gen successful.", EDIMessageStatusList.Codes.Queued, message.EM_Status);

				AssertContains($"Information|Starting processing Message #{message.EM_MessageNum}", logger.ToString());
				AssertContains($"Information|Calculating Keys for {message.EM_MessageNum}", logger.ToString());
				AssertContains("Error|Match couldn't be found for ForwardingShipment with Key SCZA00015489", logger.ToString());
				AssertContains("Information|No changes were made due to the above errors. Please fix the errors and try again.", logger.ToString());

				AssertMultilineASCIIEquals("Message Log Note won't exist unless a fatal error occurs", "No Notes Found matching [Data Import Log Text]", message.GetLogNoteText());
			});
		}

		[TestDate(2012, 12, 12)]
		public void TestKeyGen()
		{
			// Test that keys are generated by this service task
			eAdaptorRegistry.Instance.MessagesPerBatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);
			eAdaptorRegistry.Instance.UMIMessagesPerBatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);
			eAdaptorRegistry.Instance.MessagesPerExecution.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			eAdaptorRegistry.Instance.UMIMessagesPerExecution.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 15);
			eAdaptorRegistry.Instance.MessageQueueCapacity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 40);

			var messages = Enumerable.Range(0, 40).Select(i => UMIServiceTaskTest.GetMessageRowWithRandomMessageContent(factory, i)).ToArray();
			for (var i = 0; i < messages.Length; i++)
			{
				messages[i].EM_MessageNum = i.ToString("D20");
				messages[i].EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(i);
			}
			factory.Save();
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var logger = new TestServiceLogger();
				var keyGenWorker1 = new UMIServiceTaskKeyGen { ServiceLogger = logger };
				var keyGenWorker2 = new UMIServiceTaskKeyGen { ServiceLogger = logger };

				var masterTask = new UMIServiceTask { ServiceLogger = logger };

				var worker1 = new UMIServiceTaskWorker { ServiceLogger = logger };
				var worker2 = new UMIServiceTaskWorker { ServiceLogger = logger };

				masterTask.RunTask();
				AssertEquals(0, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.Queued));
				AssertEquals(0, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.PreKey));

				keyGenWorker1.RunTask();
				AssertEquals(10, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.PreKey));
				keyGenWorker1.RunTask();
				AssertEquals(20, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.PreKey));

				masterTask.RunTask();
				AssertEquals(15, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.Queued));
				masterTask.RunTask();
				AssertEquals(20, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.Queued));

				worker1.RunTask();
				AssertEquals(10, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.Notify));
				worker2.RunTask();
				AssertEquals(20, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.Notify));
				worker2.RunTask();
				AssertEquals(20, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.Notify));

				masterTask.RunTask();

				AssertEquals(20, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.Processed));
			}
		}

		public void TestKeyGenDoesNotSave()
		{
			eAdaptorRegistry.Instance.MessagesPerBatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);
			eAdaptorRegistry.Instance.MessagesPerExecution.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			eAdaptorRegistry.Instance.MessageQueueCapacity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 40);

			var messages = Enumerable.Range(0, 10).Select(i => UMIServiceTaskTest.GetMessageRowWithRandomMessageContent(factory, i)).ToArray();

			factory.Save();
			int factorySaves = 0;
			BusinessObjectFactory.SetOnFactorySaveHookForTest(_ => ++factorySaves);
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var logger = new TestServiceLogger();
				var umk = new UMIServiceTaskKeyGen { ServiceLogger = logger };
				var umi = new UMIServiceTask { ServiceLogger = logger };

				umk.RunTask();
				AssertEquals(0, factorySaves);
				AssertEquals(10, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.PreKey));

				umi.RunTask();
				AssertEquals(10, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.Queued));
				AssertEquals(0, factorySaves);
			}
		}

		[TestDate(2012, 12, 12)]
		public void TestOutOfOrderProcessing()
		{
			// Test that keys are generated by this service task
			eAdaptorRegistry.Instance.MessagesPerBatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);
			eAdaptorRegistry.Instance.UMIMessagesPerBatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 7);
			eAdaptorRegistry.Instance.MessagesPerExecution.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			eAdaptorRegistry.Instance.UMIMessagesPerExecution.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 15);
			eAdaptorRegistry.Instance.MessageQueueCapacity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 40);

			var messages = Enumerable.Range(0, 40).Select(i => UMIServiceTaskTest.GetMessageRowWithRandomMessageContent(factory, 1)).ToArray();

			for (var i = 0; i < 40; i++)
			{
				if (i % 2 == 0)
				{
					messages[i].EM_MessageNum = (messages.Length - i).ToString("D20");
				}
				else
				{
					messages[i].EM_MessageNum = i.ToString("D20");
				}

				messages[i].EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(i);
			}

			factory.Save();

			var logger = new TestServiceLogger();
			var keyGenWorker1 = new UMIServiceTaskKeyGen { ServiceLogger = logger };
			var keyGenWorker2 = new UMIServiceTaskKeyGen { ServiceLogger = logger };
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var masterTask = new UMIServiceTask { ServiceLogger = logger };

				masterTask.RunTask();
				AssertEquals(0, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.Queued));
				AssertEquals(0, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.PreKey));

				keyGenWorker1.RunTask();
				AssertEquals(10, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.PreKey));

				keyGenWorker2.RunTask();
				AssertEquals(20, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.PreKey));

				keyGenWorker1.RunTask();
				AssertEquals(30, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.PreKey));

				using (var manager = new UniversalProcessingManager(masterTask.SupportedMessageSubtypes, masterTask.ExcludedMessageSubtypes, GrEngineServiceSetting.Flipper))
				{
					var entitiesResult = manager.GrEngine.Enqueuer.Enqueue(factory, manager.GetGrEngineWatermarkQueryForTest());
					AssertEquals(7, entitiesResult.NewEntities.Count);

					for (int i = 0; i < 7; ++i)
					{
						var message = messages.First(x => x.PK.ToGuid() == entitiesResult.NewEntities[i].ParentID);
						AssertEquals((i + 1).ToString("D20"), message.EM_MessageNum);
					}
				}
			}
		}

		public void TestGetGrEngineWatermarkQuery()
		{
			var messages = Enumerable.Range(0, 20).Select(i => UMIServiceTaskTest.GetMessageRowWithRandomMessageContent(factory, 1)).ToArray();
			messages[10].EM_IsActive = false;
			factory.Save();
			var masterTask = new UMIServiceTask();
			using var manager = new UniversalProcessingManager(masterTask.SupportedMessageSubtypes, masterTask.ExcludedMessageSubtypes, GrEngineServiceSetting.Flipper);

			var query = manager.GetGrEngineWatermarkQueryForTest().Value;
			AssertEquals(query.Params[0].Value, messages[0].EM_MessageNum);

			List<StmQueueState> states = new List<StmQueueState>();
			var stateFactory = new StmQueueStateFactory(() => new GrEngineLogOptions());
			for (int i = 0; i < 10; i++)
			{
				var state = stateFactory.NewQueueState(messages[i], new[] { "" }, messages[i].EM_MessageNum);
				state.UpdateStatus(QueueStatusCodes.Codes.Queued);
				states.Add(state);
			}
			stateFactory.InsertAsQueued(states);

			query = manager.GetGrEngineWatermarkQueryForTest().Value;
			AssertEquals(query.Params[0].Value, messages[11].EM_MessageNum);

			messages[10].EM_IsActive = true;
			factory.Save();
			query = manager.GetGrEngineWatermarkQueryForTest().Value;
			AssertEquals(query.Params[0].Value, messages[10].EM_MessageNum);
		}

		public void TestGetGrEngineWatermarkQueryOnlySelectsMessageNum()
		{
			UMIServiceTaskTest.GetMessageRowWithRandomMessageContent(factory, 1);
			factory.Save();

			var masterTask = new UMIServiceTask();
			using var manager = new UniversalProcessingManager(masterTask.SupportedMessageSubtypes, masterTask.ExcludedMessageSubtypes, GrEngineServiceSetting.Flipper);

			using (Db.Connection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: false))
			{
				var query = manager.GetGrEngineWatermarkQueryForTest().Value;
				var waterMarkQuery = Db.Connection.ExecutedCommandsAndQueryPlans.Last().Item1;
				Assert(waterMarkQuery.StartsWith($"select TOP 1 {EDIMessage.Schema.EM_MessageNum} from dbo.{EDIMessage.Schema.TableName}", StringComparison.OrdinalIgnoreCase));
				Assert(waterMarkQuery.Contains($"ORDER BY {EDIMessage.Schema.EM_MessageNum}, {EDIMessage.Schema.EM_SystemCreateTimeUtc}", StringComparison.OrdinalIgnoreCase));
			}
		}

		[TestDate(2021, 03, 15)]
		public void TestGetGrEngineWatermarkQueryIsParameterised()
		{
			UMIServiceTaskTest.GetMessageRowWithRandomMessageContent(factory, 1);
			factory.Save();

			var masterTask = new UMIServiceTask();
			using var manager = new UniversalProcessingManager(masterTask.SupportedMessageSubtypes, masterTask.ExcludedMessageSubtypes, GrEngineServiceSetting.Flipper);

			using (Db.Connection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: false))
			{
				var query = manager.GetGrEngineWatermarkQueryForTest().Value;
				var waterMarkQuery = Db.Connection.ExecutedCommandsAndQueryPlans.Last().Item1;
				AssertContains("Params", waterMarkQuery, true);
				AssertContains($"{ParameterNameFactory.GetParameterName(1)}: '{QueueStatusCodes.Codes.Queued}'", waterMarkQuery, true);
				AssertContains($"{ParameterNameFactory.GetParameterName(2)}: '{ReceiveTransmitList.Codes.Receive}'", waterMarkQuery, true);
				AssertContains($"{ParameterNameFactory.GetParameterName(3)}: 1", waterMarkQuery, true);
				AssertContains($"{ParameterNameFactory.GetParameterName(4)}: '03/15/2021 00:00:00'", waterMarkQuery, true);
				AssertContains($"{ParameterNameFactory.GetParameterName(5)}: '{ApplicationCodeList.Codes.UniversalDataMessaging}'", waterMarkQuery, true);
			}
		}

		public void TestKeyGen_ReferenceIgnored()
		{
			// Test that keys are generated by this service task
			eAdaptorRegistry.Instance.MessagesPerBatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);
			eAdaptorRegistry.Instance.MessagesPerExecution.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			eAdaptorRegistry.Instance.MessageQueueCapacity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 40);

			#region inboundMessageText

			var messageWithBadReference = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent>
  <Event>
    <EventType>ATH</EventType>
    <EventTime>07-NOV-2010 05:47</EventTime>
    <EventReference>Dummy Description</EventReference>
    <ContextCollection>
      <Context>
        <Type>MAWBNumber</Type>
        <Value>{0}</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>";
			var itemsToExclude = @"!@#$%^*()-_=+[]{}\|;:'"" /?,.<>";
			#endregion

			var logger = new TestServiceLogger();
			var keyGenWorker1 = new UMIServiceTaskKeyGen { ServiceLogger = logger };
			var keyGenWorker2 = new UMIServiceTaskKeyGen { ServiceLogger = logger };

			var masterTask = new UMIServiceTask { ServiceLogger = logger };

			var worker1 = new UMIServiceTaskWorker { ServiceLogger = logger };
			var worker2 = new UMIServiceTaskWorker { ServiceLogger = logger };

			var messages = Enumerable.Range(0, itemsToExclude.Length).Select(i =>
				MakeUniversalEvent(string.Format(messageWithBadReference, itemsToExclude[i]))).ToArray();
			factory.Save();
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				masterTask.RunTask();
				AssertEquals(0, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.Queued));
				AssertEquals(0, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.PreKey));

				keyGenWorker1.RunTask();
				AssertEquals(0, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.PreKey));
				AssertEquals(10, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.Queued));
				keyGenWorker1.RunTask();
				AssertEquals(0, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.PreKey));
				AssertEquals(20, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.Queued));

				masterTask.RunTask();
				AssertEquals(20, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.Queued));
				masterTask.RunTask();
				AssertEquals(20, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.Queued));

				worker1.RunTask();
				AssertEquals(10, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.Notify));
				worker2.RunTask();
				AssertEquals(20, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.Notify));
				worker2.RunTask();
				AssertEquals(20, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.Notify));

				masterTask.RunTask();

				AssertEquals(20, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.Processed));
			}
		}

		[TestDate(2016, 11, 12)]
		public void TestMessageOrderTest_EventDiscard()
		{
			var eventMessage = Event();
			var shipmentHatMessage = HatRackShipment();
			var shipmentShoesMessage = ShoesShipment();

			eventMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddSeconds(1);
			shipmentHatMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddSeconds(2);
			shipmentShoesMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddSeconds(3);

			factory.Save();
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var logger = new TestServiceLogger();
				var keyGenWorker1 = new UMIServiceTaskKeyGen { ServiceLogger = logger };
				var masterTask = new UMIServiceTask { ServiceLogger = logger };
				var worker1 = new UMIServiceTaskWorker { ServiceLogger = logger };

				keyGenWorker1.RunTask();
				masterTask.RunTask();
				worker1.RunTask();

				factory.ReloadAll<EDIMessage>();
				AssertEquals(eventMessage.EM_Status, XmlEDIMessage.Status.Discarded);
				AssertEquals("Chains are a fantastic beast", XmlEDIMessage.Status.ProcessedOK, shipmentHatMessage.EM_Status);
				AssertEquals("Everything just gets nom nom nommed.", XmlEDIMessage.Status.ProcessedOK, shipmentShoesMessage.EM_Status);
			}
		}

		[TestDate(2023, 12, 12)]
		public void TestKeyGenRejectsMessageOnFail()
		{
			AssertKeyGenRejectException(XmlEDIMessage.Status.Rejected, new ArgumentException("Something happened..."));
			AssertKeyGenRejectException(XmlEDIMessage.Status.Rejected, new InvalidOperationException("Something happened..."));
			AssertKeyGenRejectException(XmlEDIMessage.Status.Rejected, new InvalidCastException("Something happened..."));
			AssertKeyGenRejectException(XmlEDIMessage.Status.Rejected, new ArgumentException("Something happened..."));
		}

		public void AssertKeyGenRejectException(string messageStatus, Exception ex)
		{
			var eventMessage = Event();
			eventMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddSeconds(1);

			factory.Save();

			UniversalProcessingManager.SetMessageProcessingHookForTest((debugName, message) =>
			{
				if (debugName == "PreProcessing")
				{
					throw ex;
				}
			});

			var logger = new TestServiceLogger();
			var keyGenWorker = new UMIServiceTaskKeyGen { ServiceLogger = logger };

			keyGenWorker.RunTask();

			factory.ReloadAll<EDIMessage>();
			AssertEquals(eventMessage.EM_Status, messageStatus);
			AssertContains($"Warning|Failed Key Gen Message #{eventMessage.EM_MessageNum}", logger.ToString());
			AssertEquals((byte)12, eventMessage.EM_RetryCount);
			AssertStartsWith("Error Message", $"KeyGen Exception occured.", ErrorReporter.LastMessageReported);
			AssertEquals(ex, ErrorReporter.LastExceptionReported);
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		[TestDate(2016, 11, 12)]
		public void TestMessageOrderTest_KeyGenRaceCondition()
		{
			var eventMessage = Event();
			var shipmentShoesMessage = ShoesShipment();

			shipmentShoesMessage.EM_MessageNum = 1.ToString("D20");
			eventMessage.EM_MessageNum = 3.ToString("D20");

			factory.Save();
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var logger = new TestServiceLogger();
				var keyGenWorker1 = new UMIServiceTaskKeyGen { ServiceLogger = logger };
				var masterTask = new UMIServiceTask { ServiceLogger = logger };
				var worker1 = new UMIServiceTaskWorker { ServiceLogger = logger };

				keyGenWorker1.RunTask();

				var shipmentHatMessage = HatRackShipment();
				shipmentHatMessage.EM_MessageNum = 2.ToString("D20");
				factory.Save();

				masterTask.RunTask();
				worker1.RunTask();

				factory.ReloadAll<EDIMessage>();
				AssertEquals("It is done", XmlEDIMessage.Status.ProcessedOK, shipmentShoesMessage.EM_Status);
				AssertEquals("Shipment is blocking", XmlEDIMessage.Status.Queued, eventMessage.EM_Status);
				AssertEquals(XmlEDIMessage.Status.Queued, shipmentHatMessage.EM_Status);

				masterTask.RunTask();
				worker1.RunTask();

				factory.ReloadAll<EDIMessage>();
				AssertEquals("SHould be in same position because keygen of hat is blocking.", XmlEDIMessage.Status.Queued, eventMessage.EM_Status);
				AssertEquals(XmlEDIMessage.Status.Queued, shipmentHatMessage.EM_Status);

				keyGenWorker1.RunTask();
				masterTask.RunTask();
				worker1.RunTask();

				factory.ReloadAll<EDIMessage>();
				AssertEquals(XmlEDIMessage.Status.ProcessedOK, shipmentHatMessage.EM_Status);
				AssertEquals("Chaining means this gets processed too.", XmlEDIMessage.Status.ProcessedOK, eventMessage.EM_Status);
			}
		}

		[TestDate(2016, 11, 12)]
		public void TestMessageOrderTest_KeyGen_NotBlockedByUnrelatedMessages()
		{
			var eventMessage = Event();
			var shipmentShoesMessage = ShoesShipment();

			shipmentShoesMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddSeconds(2);
			eventMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddSeconds(1);

			factory.Save();
			eventMessage.EM_ReceiveTransmit = "TRX";
			factory.Save();
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var logger = new TestServiceLogger();
				var keyGenWorker1 = new UMIServiceTaskKeyGen { ServiceLogger = logger };
				var masterTask = new UMIServiceTask { ServiceLogger = logger };
				var worker1 = new UMIServiceTaskWorker { ServiceLogger = logger };

				keyGenWorker1.RunTask();
				masterTask.RunTask();
				worker1.RunTask();

				factory.ReloadAll<EDIMessage>();
				AssertEquals(XmlEDIMessage.Status.ProcessedOK, shipmentShoesMessage.EM_Status);
			}
		}

		[TestDate(2016, 11, 12)]
		public void TestBillingHeaderChain_NoLock()
		{
			var shipmentShoesMessage = ShoesShipment();

			factory.Save();

			var logger = new TestServiceLogger();
			var keyGenWorker1 = new UMIServiceTaskKeyGen { ServiceLogger = logger };
			var masterTask = new UMIServiceTask { ServiceLogger = logger };
			var worker1 = new UMIServiceTaskWorker { ServiceLogger = logger };
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				keyGenWorker1.RunTask();
				masterTask.RunTask();
				worker1.RunTask();
				var id = factory.LoadTop1<IForwardingShipment>(new ZQuery()).JS_UniqueConsignRef;

				factory.ReloadAll<EDIMessage>();

				var message1 = BillingShipment(id);
				var message2 = BillingShipment(id);
				var message3 = BillingShipment(id);

				factory.Save();

				keyGenWorker1.RunTask();

				factory.ReloadAll<EDIMessage>();

				AssertEquals("No reject", XmlEDIMessage.Status.Queued, message1.EM_Status);
				AssertEquals("No reject", XmlEDIMessage.Status.Queued, message2.EM_Status);
				AssertEquals("No reject", XmlEDIMessage.Status.Queued, message3.EM_Status);
			}
		}

		[TestDate(2016, 11, 12)]
		public void TestMessageOrderTest_EventBetween()
		{
			var shipmentHatMessage = HatRackShipment();
			var eventMessage = Event();
			var shipmentShoesMessage = ShoesShipment();

			shipmentHatMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddSeconds(1);
			eventMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddSeconds(2);
			shipmentShoesMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddSeconds(3);

			factory.Save();
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var logger = new TestServiceLogger();
				var keyGenWorker1 = new UMIServiceTaskKeyGen { ServiceLogger = logger };
				var masterTask = new UMIServiceTask { ServiceLogger = logger };
				var worker1 = new UMIServiceTaskWorker { ServiceLogger = logger };

				keyGenWorker1.RunTask();
				masterTask.RunTask();
				worker1.RunTask();

				factory.ReloadAll<EDIMessage>();
				AssertEquals("Chaining makes everything happen.", XmlEDIMessage.Status.ProcessedOK, shipmentHatMessage.EM_Status);
				AssertEquals(XmlEDIMessage.Status.ProcessedOK, eventMessage.EM_Status);
				AssertEquals(XmlEDIMessage.Status.ProcessedOK, shipmentShoesMessage.EM_Status);
			}
		}

		public void TestConsolContainerEventMessage()
		{
			AssertEventMessageDoesNotCrash(containerEventMessage);
		}

		public void TestCommercialInvoiceMessage()
		{
			AssertShipmentMessageDoesNotCrash(commercialInvoiceLine);
		}

		public void TestPackingLineShipmentMessage()
		{
			AssertShipmentMessageDoesNotCrash(packingLineShipment);
		}

		public void TestBillingMessageDoesNotCrash()
		{
			AssertShipmentMessageDoesNotCrash(billingHeaderMessage);
		}

		public void TestManifestMessagetDoesNotCrash()
		{
			AssertShipmentMessageDoesNotCrash(manifest);
		}

		public void TestMessagesCollide_AsExpected()
		{
			var messages = new[] { MakeUniversalShipment(SampleDsvMessage1), MakeUniversalShipment(sampleDsvMessage2) };
			var branch = GlbCompany.CurrentCompany.Branches.AddNew();
			branch.GB_Code = "NAJ";
			branch.GB_IsActive = true;
			branch.Factory.Save();
			factory.Save();
			AssertKeysCollide(messages);
		}

		public void TestYusenEventMessagesCollide()
		{
			var messages = new[] { MakeUniversalEvent(YusenEvent), MakeUniversalEvent(YusenEvent) };
			factory.Save();
			AssertKeysCollide(messages);
		}

		public void TestCusEntryNum()
		{
			var list = FreightDataRegistry.Instance.CustomsAdditionalReferenceNumbers.Value;
			var newList = (CustomsReferenceNumberTypeCollection)list.Clone(null, null);
			newList.Add("SSN", (NoResString)"imr test");
			FreightDataRegistry.Instance.CustomsAdditionalReferenceNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newList);

			var messages = new[] { MakeUniversalShipment(CusEntryNumShipment), MakeUniversalEvent(CusEntryNumEvent) };
			var branch = GlbCompany.CurrentCompany.Branches.AddNew();
			branch.GB_Code = "NAJ";
			branch.GB_IsActive = true;
			branch.Factory.Save();
			factory.Save();
			AssertKeysCollide(messages);
		}

		void AssertEventMessageDoesNotCrash(string message) => AssertMessageDoesNotCrash(message, EDIMessageSubTypeList.Codes.XmlUniversalEvent);

		void AssertShipmentMessageDoesNotCrash(string message) => AssertMessageDoesNotCrash(message, EDIMessageSubTypeList.Codes.XmlUniversalShipment);

		void AssertMessageDoesNotCrash(string message, string code)
		{
			var eventMessage = UMITestHelper.GetMessage(factory,
				XmlEDIMessage.ApplicationCodes.UniversalDataMessaging,
				EDIMessageTypeList.Codes.XDC,
				code,
				XmlEDIMessage.Status.Queued, message);

			factory.Save();

			var logger = new TestServiceLogger();
			var keyGenWorker1 = new UMIServiceTaskKeyGen { ServiceLogger = logger };
			var masterTask = new UMIServiceTask { ServiceLogger = logger };
			var worker1 = new UMIServiceTaskWorker { ServiceLogger = logger };

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				keyGenWorker1.RunTask();
				masterTask.RunTask();
				worker1.RunTask();
				eventMessage.Reload();
				AssertNotEquals("eventMessage.EM_Status", XmlEDIMessage.Status.Queued, eventMessage.EM_Status);
			}
		}

		void AssertKeysCollide(params EDIMessage[] messages)
		{
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var logger = new TestServiceLogger();
				var keyGenWorker1 = new UMIServiceTaskKeyGen { ServiceLogger = logger };
				var masterTask = new UMIServiceTask { ServiceLogger = logger };
				var worker1 = new UMIServiceTaskWorker { ServiceLogger = logger };

				keyGenWorker1.RunTask();
				masterTask.RunTask();

				AssertEquals("They are created ya?", messages.Length, GraphEngineTestExtensions.CountInQueue(messages));
				AssertEquals("One fellow ought to be queued", 1, GraphEngineTestExtensions.CountInQueue(messages, status: QueueStatusCodes.Codes.Queued));
				AssertEquals("Everything else ought to be blocked", messages.Length - 1, GraphEngineTestExtensions.CountInQueue(messages, status: QueueStatusCodes.Codes.Blocked));
			}
		}

		#endregion

		public void TestMinimumPeriod()
		{
			AssertEquals("1minute", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"Universal Shipment Key Generator",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.UniversalDataMessaging,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_MessageType + "=" + EDIMessageTypeList.Codes.XDC,
						EDIMessageSchema.Constants.EM_MessageSubType + "=" + EDIMessageSubTypeList.Codes.XmlUniversalShipment),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"Universal Event Key Generator",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.UniversalDataMessaging,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_MessageType + "=" + EDIMessageTypeList.Codes.XDC,
						EDIMessageSchema.Constants.EM_MessageSubType + "=" + EDIMessageSubTypeList.Codes.XmlUniversalEvent),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"Universal Transaction Key Generator",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.UniversalDataMessaging,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_MessageType + "=" + EDIMessageTypeList.Codes.XDC,
						EDIMessageSchema.Constants.EM_MessageSubType + "=" + EDIMessageSubTypeList.Codes.XmlUniversalTransaction),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"Universal Transaction Batch Key Generator",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.UniversalDataMessaging,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_MessageType + "=" + EDIMessageTypeList.Codes.XDC,
						EDIMessageSchema.Constants.EM_MessageSubType + "=" + EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch),
				};
			}
		}

		#region Giant XML Blobs

		EDIMessage MakeUniversalShipment(string xml)
		{
			return UMITestHelper.GetMessage(factory,
				XmlEDIMessage.ApplicationCodes.UniversalDataMessaging,
				EDIMessageTypeList.Codes.XDC,
				EDIMessageSubTypeList.Codes.XmlUniversalShipment,
				XmlEDIMessage.Status.Queued, xml);
		}

		EDIMessage MakeUniversalEvent(string content)
		{
			return UMITestHelper.GetMessage(factory,
				XmlEDIMessage.ApplicationCodes.UniversalDataMessaging,
				EDIMessageTypeList.Codes.XDC,
				EDIMessageSubTypeList.Codes.XmlUniversalEvent,
				XmlEDIMessage.Status.Queued, content);
		}

		EDIMessage Event() => MakeUniversalEvent(big_eventMessage);

		EDIMessage BillingShipment(string shipmentID) => MakeUniversalShipment(string.Format(billingHeaderMessage, shipmentID));

		EDIMessage HatRackShipment() => MakeUniversalShipment(big_shipmentHatRack);

		EDIMessage ShoesShipment() => MakeUniversalShipment(big_shipmentUsedShoes);

		const string billingHeaderMessage = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<UniversalShipment>
   <Shipment>
      <DataContext>
         <CodesMappedToTarget>true</CodesMappedToTarget>
         <Company>
            <Code>EDI</Code>
         </Company>
         <DataTargetCollection>
            <DataTarget>
               <Type>ForwardingShipment</Type>
               <Key>{0}</Key>
            </DataTarget>
         </DataTargetCollection>
      </DataContext>
      <JobCosting>
         <ChargeLineCollection>
            <ChargeLine>
               <ChargeCode>
                  <Code>GTAXGDS</Code>
               </ChargeCode>
               <CostLocalAmount>8242.00</CostLocalAmount>
               <Creditor>
                  <Type>Organization</Type>
                  <Key>FRREREPANTE</Key>
               </Creditor>
               <ImportMetaData>
                  <Instruction>Insert</Instruction>
               </ImportMetaData>
               <SupplierReference>175526237</SupplierReference>
            </ChargeLine>
            <ChargeLine>
               <ChargeCode>
                  <Code>PRODTAX</Code>
               </ChargeCode>
               <CostLocalAmount>78.00</CostLocalAmount>
               <Creditor>
                  <Type>Organization</Type>
                  <Key>FRREREPANTE</Key>
               </Creditor>
               <ImportMetaData>
                  <Instruction>Insert</Instruction>
               </ImportMetaData>
               <SupplierReference>175526237</SupplierReference>
            </ChargeLine>
         </ChargeLineCollection>
      </JobCosting>
      <EntryNumberCollection>
         <EntryNumber>
            <Number>175526237</Number>
            <Type>
               <Code>IMP</Code>
            </Type>
            <IssueDate>2017-07-24</IssueDate>
         </EntryNumber>
      </EntryNumberCollection>
   </Shipment>
</UniversalShipment>";

		const string containerEventMessage = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingConsol</Type>
          <Key>C00001089</Key>
        </DataTarget>
      </DataTargetCollection>

      <Company>
        <Code>BEN</Code>
      </Company>
      <EnterpriseID>HYE</EnterpriseID>
      <EventType>
        <Code>SBR</Code>
        <Description>Subscription Requested</Description>
      </EventType>
      <ServerID>BEN</ServerID>
    </DataContext>

    <EventTime>2017-01-24T15:39:02.99</EventTime>
    <EventType>SBR</EventType>
    <IsEstimate>false</IsEstimate>

    <ContextCollection>
      <Context>
        <Type>ContainerNumber</Type>
        <Value>WWCD2948252</Value>
      </Context>
      <Context>
        <Type>MBOLNumber</Type>
        <Value>CWISETEST-7</Value>
      </Context>
      <Context>
        <Type>MBOLOriginUNLOCO</Type>
        <Value>AUSYD</Value>
      </Context>
      <Context>
        <Type>MBOLDestinationUNLOCO</Type>
        <Value>USLAX</Value>
      </Context>
      <Context>
        <Type>CarriersBookingReference</Type>
        <Value>CWISETESTBOOK-7</Value>
      </Context>
      <Context>
        <Type>CarrierCode</Type>
        <Value>HLCU</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>";

		const string big_eventMessage = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
		<DataContext>
			<Company>
				<Code>EDI</Code>
			</Company>
			<DataProvider>EDIDATEDI</DataProvider>
			<EnterpriseID>EDI</EnterpriseID>
			<EventType>
				<Code>ATH</Code>
				<Description>Authorized</Description>
			</EventType>
			<ServerID>DAT</ServerID>
		</DataContext>

		<EventTime>2016-12-21T05:28:54.147</EventTime>
		<EventType>ATH</EventType>
		<IsEstimate>false</IsEstimate>

		<ContextCollection>
			<Context>
				<Type>HAWBNumber</Type>
				<Value>HB0000001010101</Value>
			</Context>
			<Context>
				<Type>HAWBOriginIATAAirportCode</Type>
				<Value>FRA</Value>
			</Context>
			<Context>
				<Type>HAWBDestinationIATAAirportCode</Type>
				<Value>HKG</Value>
			</Context>
			<Context>
				<Type>HBOLOriginUNLOCO</Type>
				<Value>DEFRA</Value>
			</Context>
			<Context>
				<Type>HBOLDestinationUNLOCO</Type>
				<Value>HKHKG</Value>
			</Context>
			<Context>
				<Type>ShippersReference</Type>
				<Value>DF123456</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		const string big_shipmentHatRack = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Shipment>
		<DataContext>
			<Company>
				<Code>EDI</Code>
			</Company>
			<DataProvider>EDIDATEDI</DataProvider>
			<EnterpriseID>EDI</EnterpriseID>
			<ServerID>DAT</ServerID>

			<RecipientRoleCollection>
				<RecipientRole>
					<Code>RAG</Code>
					<Description>Receiving Agent</Description>
				</RecipientRole>
			</RecipientRoleCollection>
		</DataContext>

		<BookingConfirmationReference>DF123456</BookingConfirmationReference>
		<ContainerMode>
			<Code>LSE</Code>
			<Description>Loose</Description>
		</ContainerMode>
		<GoodsDescription>Hat Racks</GoodsDescription>
		<OuterPacksPackageType>
			<Code>PLT</Code>
			<Description>Pallet</Description>
		</OuterPacksPackageType>
		<PortOfDestination>
			<Code>HKHKG</Code>
			<Name>Hong Kong</Name>
		</PortOfDestination>
		<PortOfOrigin>
			<Code>DEFRA</Code>
			<Name>Frankfurt am Main</Name>
		</PortOfOrigin>
		<ServiceLevel>
			<Code>STD</Code>
			<Description>Standard</Description>
		</ServiceLevel>
		<ShipmentIncoTerm>
			<Code>FOB</Code>
			<Description>Free On Board</Description>
		</ShipmentIncoTerm>
		<TransportMode>
			<Code>AIR</Code>
			<Description>Air Freight</Description>
		</TransportMode>
		<WayBillNumber>HB0000001010101</WayBillNumber>
		<WayBillType>
			<Code>HWB</Code>
			<Description>House Waybill</Description>
		</WayBillType>

		<OrganizationAddressCollection>
			<OrganizationAddress>
				<AddressType>ConsignorDocumentaryAddress</AddressType>
				<Address1>DIESLSTR 11</Address1>
				<Address2>57439 ATTENDORN, GERMANY</Address2>
				<AddressOverride>false</AddressOverride>
				<AddressShortCode>PST: DIESLSTR 11</AddressShortCode>
				<City></City>
				<CompanyName>ABA BEUL</CompanyName>
				<Country>
					<Code>DE</Code>
					<Name>Germany</Name>
				</Country>
				<Email></Email>
				<Fax></Fax>
				<OrganizationCode>ABABEU</OrganizationCode>
				<Phone></Phone>
				<Port>
					<Code>DEFRA</Code>
					<Name>Frankfurt am Main</Name>
				</Port>
				<Postcode></Postcode>
				<State></State>
			</OrganizationAddress>
			<OrganizationAddress>
				<AddressType>ConsignorPickupDeliveryAddress</AddressType>
				<Address1>DIESLSTR 11</Address1>
				<Address2></Address2>
				<AddressOverride>false</AddressOverride>
				<AddressShortCode>Pick Up Address</AddressShortCode>
				<City>ATTENDORN?, GERMANY</City>
				<CompanyName>ABA BEUL</CompanyName>
				<Country>
					<Code>DE</Code>
					<Name>Germany</Name>
				</Country>
				<Email></Email>
				<Fax></Fax>
				<OrganizationCode>ABABEU</OrganizationCode>
				<Phone></Phone>
				<Port>
					<Code></Code>
				</Port>
				<Postcode>57439</Postcode>
				<State></State>
			</OrganizationAddress>
			<OrganizationAddress>
				<AddressType>ConsigneeDocumentaryAddress</AddressType>
				<Address1>UNIT A1, 4TH FLOOR, PIONEER IND BLDG</Address1>
				<Address2>213 WAI YIP STREET, KWUN TONG, KOWLOON  HONG KONG</Address2>
				<AddressOverride>false</AddressOverride>
				<AddressShortCode>PST: UNIT A1, 4TH FLOOR,</AddressShortCode>
				<City></City>
				<CompanyName>A&amp;S FURNISHING CO LTD</CompanyName>
				<Country>
					<Code>HK</Code>
					<Name>Hong Kong</Name>
				</Country>
				<Email></Email>
				<Fax></Fax>
				<OrganizationCode>AASDRA</OrganizationCode>
				<Phone></Phone>
				<Port>
					<Code>HKHKG</Code>
					<Name>Hong Kong</Name>
				</Port>
				<Postcode></Postcode>
				<State></State>
			</OrganizationAddress>
			<OrganizationAddress>
				<AddressType>ConsigneePickupDeliveryAddress</AddressType>
				<Address1>UNIT A1, 4TH FLOOR, PIONEER IND BLDG</Address1>
				<Address2>213 WAI YIP STREET, KWUN TONG, KOWLOON  HONG KONG</Address2>
				<AddressOverride>false</AddressOverride>
				<AddressShortCode>PST: UNIT A1, 4TH FLOOR,</AddressShortCode>
				<City></City>
				<CompanyName>A&amp;S FURNISHING CO LTD</CompanyName>
				<Country>
					<Code>HK</Code>
					<Name>Hong Kong</Name>
				</Country>
				<Email></Email>
				<Fax></Fax>
				<OrganizationCode>AASDRA</OrganizationCode>
				<Phone></Phone>
				<Port>
					<Code>HKHKG</Code>
					<Name>Hong Kong</Name>
				</Port>
				<Postcode></Postcode>
				<State></State>
			</OrganizationAddress>
		</OrganizationAddressCollection>

		<PackingLineCollection>
			<PackingLine>
				<Commodity>
					<Code>GEN</Code>
					<Description>General</Description>
				</Commodity>
				<GoodsDescription>Hat Racks</GoodsDescription>
				<PackQty>12</PackQty>
				<PackType>
					<Code>PLT</Code>
					<Description>Pallet</Description>
				</PackType>
			</PackingLine>
		</PackingLineCollection>
	</Shipment>
</UniversalShipment>";

		const string big_shipmentUsedShoes = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Shipment>
		<DataContext>
			<Company>
				<Code>EDI</Code>
			</Company>
			<DataProvider>EDIDATEDI</DataProvider>
			<EnterpriseID>EDI</EnterpriseID>
			<ServerID>DAT</ServerID>

			<RecipientRoleCollection>
				<RecipientRole>
					<Code>RAG</Code>
					<Description>Receiving Agent</Description>
				</RecipientRole>
			</RecipientRoleCollection>
		</DataContext>

		<BookingConfirmationReference>DF123456</BookingConfirmationReference>
		<ContainerMode>
			<Code>LSE</Code>
			<Description>Loose</Description>
		</ContainerMode>
		<GoodsDescription>Used Shoes</GoodsDescription>
		<OuterPacksPackageType>
			<Code>PLT</Code>
			<Description>Pallet</Description>
		</OuterPacksPackageType>
		<PortOfDestination>
			<Code>HKHKG</Code>
			<Name>Hong Kong</Name>
		</PortOfDestination>
		<PortOfOrigin>
			<Code>DEFRA</Code>
			<Name>Frankfurt am Main</Name>
		</PortOfOrigin>
		<ServiceLevel>
			<Code>STD</Code>
			<Description>Standard</Description>
		</ServiceLevel>
		<ShipmentIncoTerm>
			<Code>FOB</Code>
			<Description>Free On Board</Description>
		</ShipmentIncoTerm>
		<TransportMode>
			<Code>AIR</Code>
			<Description>Air Freight</Description>
		</TransportMode>
		<WayBillNumber>HB0000001010101</WayBillNumber>
		<WayBillType>
			<Code>HWB</Code>
			<Description>House Waybill</Description>
		</WayBillType>

		<OrganizationAddressCollection>
			<OrganizationAddress>
				<AddressType>ConsignorDocumentaryAddress</AddressType>
				<Address1>DIESLSTR 11</Address1>
				<Address2>57439 ATTENDORN, GERMANY</Address2>
				<AddressOverride>false</AddressOverride>
				<AddressShortCode>PST: DIESLSTR 11</AddressShortCode>
				<City></City>
				<CompanyName>ABA BEUL</CompanyName>
				<Country>
					<Code>DE</Code>
					<Name>Germany</Name>
				</Country>
				<Email></Email>
				<Fax></Fax>
				<OrganizationCode>ABABEU</OrganizationCode>
				<Phone></Phone>
				<Port>
					<Code>DEFRA</Code>
					<Name>Frankfurt am Main</Name>
				</Port>
				<Postcode></Postcode>
				<State></State>
			</OrganizationAddress>
			<OrganizationAddress>
				<AddressType>ConsignorPickupDeliveryAddress</AddressType>
				<Address1>DIESLSTR 11</Address1>
				<Address2></Address2>
				<AddressOverride>false</AddressOverride>
				<AddressShortCode>Pick Up Address</AddressShortCode>
				<City>ATTENDORN?, GERMANY</City>
				<CompanyName>ABA BEUL</CompanyName>
				<Country>
					<Code>DE</Code>
					<Name>Germany</Name>
				</Country>
				<Email></Email>
				<Fax></Fax>
				<OrganizationCode>ABABEU</OrganizationCode>
				<Phone></Phone>
				<Port>
					<Code></Code>
				</Port>
				<Postcode>57439</Postcode>
				<State></State>
			</OrganizationAddress>
			<OrganizationAddress>
				<AddressType>ConsigneeDocumentaryAddress</AddressType>
				<Address1>UNIT A1, 4TH FLOOR, PIONEER IND BLDG</Address1>
				<Address2>213 WAI YIP STREET, KWUN TONG, KOWLOON  HONG KONG</Address2>
				<AddressOverride>false</AddressOverride>
				<AddressShortCode>PST: UNIT A1, 4TH FLOOR,</AddressShortCode>
				<City></City>
				<CompanyName>A&amp;S FURNISHING CO LTD</CompanyName>
				<Country>
					<Code>HK</Code>
					<Name>Hong Kong</Name>
				</Country>
				<Email></Email>
				<Fax></Fax>
				<OrganizationCode>AASDRA</OrganizationCode>
				<Phone></Phone>
				<Port>
					<Code>HKHKG</Code>
					<Name>Hong Kong</Name>
				</Port>
				<Postcode></Postcode>
				<State></State>
			</OrganizationAddress>
			<OrganizationAddress>
				<AddressType>ConsigneePickupDeliveryAddress</AddressType>
				<Address1>UNIT A1, 4TH FLOOR, PIONEER IND BLDG</Address1>
				<Address2>213 WAI YIP STREET, KWUN TONG, KOWLOON  HONG KONG</Address2>
				<AddressOverride>false</AddressOverride>
				<AddressShortCode>PST: UNIT A1, 4TH FLOOR,</AddressShortCode>
				<City></City>
				<CompanyName>A&amp;S FURNISHING CO LTD</CompanyName>
				<Country>
					<Code>HK</Code>
					<Name>Hong Kong</Name>
				</Country>
				<Email></Email>
				<Fax></Fax>
				<OrganizationCode>AASDRA</OrganizationCode>
				<Phone></Phone>
				<Port>
					<Code>HKHKG</Code>
					<Name>Hong Kong</Name>
				</Port>
				<Postcode></Postcode>
				<State></State>
			</OrganizationAddress>
		</OrganizationAddressCollection>

		<PackingLineCollection>
			<PackingLine>
				<Commodity>
					<Code>GEN</Code>
					<Description>General</Description>
				</Commodity>
				<GoodsDescription>Hat Racks</GoodsDescription>
				<PackQty>12</PackQty>
				<PackType>
					<Code>PLT</Code>
					<Description>Pallet</Description>
				</PackType>
			</PackingLine>
		</PackingLineCollection>
	</Shipment>
</UniversalShipment>";

		const string commercialInvoiceLine = @"<UniversalShipment>
  <Shipment>
    <DataContext>
      <ActionPurpose>
        <Code>YUS</Code>
      </ActionPurpose>
      <Company>
        <Code>YUS</Code>
        <Country>
          <Code>US</Code>
        </Country>
      </Company>
      <EnterpriseID>YAS</EnterpriseID>
      <EventBranch>
        <Code>CHI</Code>
      </EventBranch>
      <ServerID>PRD</ServerID>
      <DataTargetCollection>
        <DataTarget>
          <Type>CustomsCommercialInvoice</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <CommercialInfo>
      <CommercialInvoiceCollection>
        <CommercialInvoice>
          <InvoiceNumber>N61030EI</InvoiceNumber>
          <InvoiceAmount>8873.52</InvoiceAmount>
          <InvoiceCurrency>
            <Code>USD</Code>
          </InvoiceCurrency>
          <InvoiceDate>2016-12-22T00:00:00</InvoiceDate>
          <Supplier>
            <AddressType>Supplier</AddressType>
            <OrganizationCode>OMRONUKY</OrganizationCode>
          </Supplier>
          <CommercialInvoiceLineCollection>
            <CommercialInvoiceLine>
              <LineNo>1</LineNo>
              <Description>E2B-M12KS04-WP-C1 2M OMI</Description>
              <EntryNumber>9X0392017001</EntryNumber>
              <InvoiceQuantity>100</InvoiceQuantity>
              <InvoiceQuantityUnit>
                <Code>PCS</Code>
              </InvoiceQuantityUnit>
              <LinePrice>528</LinePrice>
              <NetWeight>7.26</NetWeight>
              <OrderNumber>024392A</OrderNumber>
              <PartNo>E2B 2148B</PartNo>
              <Weight>8.115</Weight>
              <AddInfoCollection>
                <AddInfo>
                  <Key>UC_NKCountryOfOrigin</Key>
                  <Value>ID</Value>
                </AddInfo>
                <AddInfo>
                  <Key>UC_NKCountryOfExport</Key>
                  <Value>ID</Value>
                </AddInfo>
              </AddInfoCollection>
              <AddInfoGroupCollection>
                <AddInfoGroup>
                  <Type>
                    <Code />
                  </Type>
                  <AddInfoCollection>
                    <AddInfo>
                      <Key>UnitPrice</Key>
                      <Value>5.28</Value>
                    </AddInfo>
                  </AddInfoCollection>
                </AddInfoGroup>
              </AddInfoGroupCollection>
              <OrganizationAddressCollection>
                <OrganizationAddress>
                  <AddressType>UltimateConsignee</AddressType>
                  <OrganizationCode>OMRELEJMH</OrganizationCode>
                </OrganizationAddress>
              </OrganizationAddressCollection>
            </CommercialInvoiceLine>
          </CommercialInvoiceLineCollection>
          <OrganizationAddressCollection>
            <OrganizationAddress>
              <AddressType>Importer</AddressType>
              <OrganizationCode>OMRELEJMH</OrganizationCode>
            </OrganizationAddress>
          </OrganizationAddressCollection>
        </CommercialInvoice>
      </CommercialInvoiceCollection>
    </CommercialInfo>
    <MessageType>
      <Code>IMP</Code>
    </MessageType>
  </Shipment>
</UniversalShipment>
";

		const string packingLineShipment = @"<UniversalShipment>
      <Shipment>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>ForwardingShipment</Type>
            </DataTarget>
          </DataTargetCollection>
          <ActionPurpose>
            <Code>YJP</Code>
          </ActionPurpose>
          <Company>
            <Country>
              <Code>JP</Code>
            </Country>
            <Code>YJP</Code>
          </Company>
          <EnterpriseID>YAS</EnterpriseID>
          <ServerID>PRD</ServerID>
        </DataContext>
        <GoodsDescription>AUTO PARTS</GoodsDescription>
        <ContainerMode>
          <Code>LSE</Code>
        </ContainerMode>
        <TransportMode>
          <Code>AIR</Code>
        </TransportMode>
        <Branch>
          <Code>NAJ</Code>
        </Branch>
        <PortOfDestination>
          <Code>WUH</Code>
        </PortOfDestination>
        <PortOfOrigin>
          <Code>JPNGO</Code>
        </PortOfOrigin>
        <ShipmentIncoTerm>
          <Code>CIF</Code>
        </ShipmentIncoTerm>
        <ShipmentType>
          <Code>STD</Code>
        </ShipmentType>
        <TotalNoOfPacks>0</TotalNoOfPacks>
        <TotalNoOfPacksPackageType>
          <Code>CTN</Code>
        </TotalNoOfPacksPackageType>
        <OuterPacks>1</OuterPacks>
        <OuterPacksPackageType>
          <Code>PCE</Code>
        </OuterPacksPackageType>
        <TotalVolume>0.033000</TotalVolume>
        <TotalVolumeUnit>
          <Code>M3</Code>
        </TotalVolumeUnit>
        <TotalWeight>5.700</TotalWeight>
        <TotalWeightUnit>
          <Code>KG</Code>
        </TotalWeightUnit>
        <WayBillNumber>IC011ZN0AY</WayBillNumber>
        <WayBillType>
          <Code>HWB</Code>
        </WayBillType>
        <LocalProcessing>
          <OrderNumberCollection>
            <OrderNumber>
              <OrderReference>IC011ZN0AY</OrderReference>
              <Sequence>1</Sequence>
            </OrderNumber>
          </OrderNumberCollection>
        </LocalProcessing>
        <CustomizedFieldCollection>
          <CustomizedField>
            <Key>DENSO-6) Sell-Rate</Key>
            <DataType>Decimal</DataType>
            <Value>3000</Value>
          </CustomizedField>
          <CustomizedField>
            <Key>AE11.Consol Section</Key>
            <DataType>String</DataType>
            <Value>XXX</Value>
          </CustomizedField>
        </CustomizedFieldCollection>
        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>ConsigneeDocumentaryAddress</AddressType>
            <OrganizationCode>DENINVBJS</OrganizationCode>
            <AddressShortCode>A_BC</AddressShortCode>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty</AddressType>
            <OrganizationCode>DENINVBJS</OrganizationCode>
            <AddressShortCode>A_WUH</AddressShortCode>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsignorDocumentaryAddress</AddressType>
            <OrganizationCode>DENSONGO501</OrganizationCode>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ExportBroker</AddressType>
            <OrganizationCode>SANKAINGO1</OrganizationCode>
          </OrganizationAddress>
        </OrganizationAddressCollection>
        <PackingLineCollection>
          <PackingLine>
            <GoodsDescription>AUTO PARTS</GoodsDescription>
            <PackQty>1</PackQty>
            <PackType>
              <Code>PCE</Code>
            </PackType>
            <Volume>0.033000</Volume>
            <VolumeUnit>
              <Code>M3</Code>
            </VolumeUnit>
            <Weight>5.700</Weight>
            <WeightUnit>
              <Code>KG</Code>
            </WeightUnit>
            <Height>27.000</Height>
            <Length>40.000</Length>
            <LengthUnit>
              <Code>CM</Code>
            </LengthUnit>
            <Width>31.000</Width>
          </PackingLine>
        </PackingLineCollection>
      </Shipment>
    </UniversalShipment>
";

		const string CusEntryNumShipment = @"
<?xml version=""1.0"" encoding=""UTF-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
   <Shipment>
      <DataContext>
         <CodesMappedToTarget>true</CodesMappedToTarget>
         <EnterpriseID>DFD</EnterpriseID>
         <ServerID>DQA</ServerID>
         <Company>
            <Code>EDI</Code>
         </Company>
         <DataProvider>DSV-Eservices</DataProvider>
         <DataTargetCollection>
            <DataTarget>
               <Type>ForwardingShipment</Type>
            </DataTarget>
         </DataTargetCollection>
      </DataContext>
      <AdditionalTerms>Algiers Port</AdditionalTerms>
      <AWBServiceLevel>
         <Code>STD</Code>
         <Description>Standard</Description>
      </AWBServiceLevel>
      <BookingConfirmationReference>E-REF:7597063333</BookingConfirmationReference>
      <CFSReference />
      <ContainerCount>0</ContainerCount>
      <ContainerMode>
         <Code>FCL</Code>
      </ContainerMode>
      <GoodsDescription>Systeme De Ventillat</GoodsDescription>
      <JobCosting>
         <Department>
            <Code>FES</Code>
         </Department>
         <ChargeLineCollection />
      </JobCosting>
      <OuterPacks>18</OuterPacks>
      <OuterPacksPackageType>
         <Code>PKG</Code>
      </OuterPacksPackageType>
      <PortOfOrigin>
         <Code>DKHOR</Code>
      </PortOfOrigin>
      <ReleaseType>
         <Code>OBR</Code>
      </ReleaseType>
      <ServiceLevel>
         <Code>CLA</Code>
      </ServiceLevel>
      <ShipmentIncoTerm>
         <Code>CFR</Code>
      </ShipmentIncoTerm>
      <TotalWeight>2781</TotalWeight>
      <TotalWeightUnit>
         <Code>KG</Code>
      </TotalWeightUnit>
      <TransportMode>
         <Code>SEA</Code>
      </TransportMode>
      <LocalProcessing>
         <EstimatedPickup>2017-10-27T00:00:00</EstimatedPickup>
         <InsuranceRequired>false</InsuranceRequired>
         <OrderNumberCollection>
            <OrderNumber>
               <OrderReference>64123</OrderReference>
               <Sequence>1</Sequence>
            </OrderNumber>
         </OrderNumberCollection>
      </LocalProcessing>
      <AdditionalReferenceCollection Content=""Partial"">
         <AdditionalReference>
            <Type>
               <Code>SID</Code>
            </Type>
            <ReferenceNumber>Eservices</ReferenceNumber>
         </AdditionalReference>
         <AdditionalReference>
            <Type>
               <Code>SSN</Code>
            </Type>
            <ReferenceNumber>7597063333</ReferenceNumber>
         </AdditionalReference>
         <AdditionalReference>
            <Type>
               <Code>STP</Code>
            </Type>
            <ReferenceNumber>DSV</ReferenceNumber>
         </AdditionalReference>
         <AdditionalReference>
            <Type>
               <Code>OSR</Code>
            </Type>
            <ReferenceNumber>64123</ReferenceNumber>
         </AdditionalReference>
      </AdditionalReferenceCollection>
      <CustomizedFieldCollection />
      <DateCollection />
      <EntryNumberCollection />
      <NoteCollection>
         <Note>
            <Description>Export Pickup Instructions</Description>
            <IsCustomDescription>false</IsCustomDescription>
            <NoteText>Oplys ref nr. ved afhentning 64123-mns

Mandag - torsdag 06.00 - 16.30

LC vedhæftet</NoteText>
         </Note>
         <Note>
            <Description>Special Instructions</Description>
            <IsCustomDescription>false</IsCustomDescription>
            <NoteText>COL CONTACT:CHRISTIAN SMED

72175555

DEL CONTACT:

550440306</NoteText>
         </Note>
         <Note>
            <Description>BarcodeNote</Description>
            <IsCustomDescription>true</IsCustomDescription>
            <NoteText>--Start Barcodes--
Pack_1 [373323997412654309;373323997412654316;373323997412654323;373323997412654330;373323997412654347;373323997412654354;373323997412654361;373323997412654378;373323997412654385;373323997412654392;373323997412654408;373323997412654415;373323997412654422;373323997412654439;373323997412654446;373323997412654453;373323997412654460;373323997412654477]
--Stop Barcodes--</NoteText>
            <Visibility>
               <Code>PUB</Code>
            </Visibility>
         </Note>
         <Note>
            <Description>Internal Work Notes</Description>
            <IsCustomDescription>false</IsCustomDescription>
            <NoteText>CLIENT HAS BOOKED CONTAINER(S).
	Container Id: 
	Container Type: 20ftDC</NoteText>
         </Note>
         <Note>
            <Description>Booking Contact Details</Description>
            <IsCustomDescription>true</IsCustomDescription>
            <NoteText>Sender (Shipper):
SKOV A/S,HEDELUND 4, GLYNGØRE,7870 ROSLEV,DK
Contact name: CHRISTIAN SMED / Phone: 72175555 / E-mail: cts@skov.dk

Pickup Address:
SKOV A/S,HEDELUND 4, GLYNGØRE,7870 ROSLEV,DK
Contact name: CHRISTIAN SMED / Phone: 72175555 / E-mail: cts@skov.dk

Receiver (Consignee):
SARL OUCIF AVICOLA,Cite Ouled Amrane Hachem,No 139 Ain,000 Soltane Ain Defla,DZ
Contact name:  / Phone: 550440306 / E-mail: avicolam.cu@gmail.com

Delivery Address:
SARL OUCIF AVICOLA,Cite Ouled Amrane Hachem,No 139 Ain,000 Soltane Ain Defla,DZ
Contact name:  / Phone: 550440306 / E-mail: avicolam.cu@gmail.com</NoteText>
         </Note>
      </NoteCollection>
      <OrganizationAddressCollection>
         <OrganizationAddress>
            <AddressType>ConsignorDocumentaryAddress</AddressType>
            <OrganizationCode>6402099538</OrganizationCode>
            <Address1>HEDELUND 4, GLYNGØRE</Address1>
            <City>ROSLEV</City>
            <CompanyName>SKOV A/S</CompanyName>
            <Contact>CHRISTIAN SMED</Contact>
            <Country>
               <Code>DK</Code>
            </Country>
            <Email>cts@skov.dk</Email>
            <Phone>72175555</Phone>
            <Postcode>7870</Postcode>
         </OrganizationAddress>
         <OrganizationAddress>
            <AddressType>ConsigneeDocumentaryAddress</AddressType>
            <OrganizationCode>17541SCCtdemASe50</OrganizationCode>
            <Address1>Cite Ouled Amrane Hachem</Address1>
            <Address2>No 139 Ain</Address2>
            <City>Soltane Ain Defla</City>
            <CompanyName>SARL OUCIF AVICOLA</CompanyName>
            <Country>
               <Code>DZ</Code>
            </Country>
            <Email>avicolam.cu@gmail.com</Email>
            <Phone>550440306</Phone>
            <Postcode>000</Postcode>
         </OrganizationAddress>
         <OrganizationAddress>
            <AddressType>ConsigneePickupDeliveryAddress</AddressType>
            <OrganizationCode>17541SCCtdemASe50</OrganizationCode>
            <Address1>Cite Ouled Amrane Hachem</Address1>
            <Address2>No 139 Ain</Address2>
            <City>Soltane Ain Defla</City>
            <CompanyName>SARL OUCIF AVICOLA</CompanyName>
            <Country>
               <Code>DZ</Code>
            </Country>
            <Email>avicolam.cu@gmail.com</Email>
            <Phone>550440306</Phone>
            <Postcode>000</Postcode>
         </OrganizationAddress>
         <OrganizationAddress>
            <AddressType>ConsignorPickupDeliveryAddress</AddressType>
            <OrganizationCode>0D07SEoSVHENGNE32</OrganizationCode>
            <Address1>HEDELUND 4, GLYNGØRE</Address1>
            <City>ROSLEV</City>
            <CompanyName>SKOV A/S</CompanyName>
            <Contact>CHRISTIAN SMED</Contact>
            <Country>
               <Code>DK</Code>
            </Country>
            <Email>cts@skov.dk</Email>
            <Phone>72175555</Phone>
            <Postcode>7870</Postcode>
         </OrganizationAddress>
      </OrganizationAddressCollection>
      <PackingLineCollection>
         <PackingLine>
            <CountryOfOrigin>
               <Code>DK</Code>
            </CountryOfOrigin>
            <DetailedDescription>Systeme De Ventillat</DetailedDescription>
            <GoodsDescription>Systeme De Ventillat</GoodsDescription>
            <HarmonisedCode>84369900</HarmonisedCode>
            <LengthUnit>
               <Code />
            </LengthUnit>
            <LinePrice>37865</LinePrice>
            <MarksAndNos>80299126, 80300292</MarksAndNos>
            <PackQty>18</PackQty>
            <PackType>
               <Code>PKG</Code>
            </PackType>
            <Weight>2781</Weight>
            <WeightUnit>
               <Code>KG</Code>
            </WeightUnit>
            <CustomizedFieldCollection />
         </PackingLine>
      </PackingLineCollection>
      <RelatedShipmentCollection />
   </Shipment>
</UniversalShipment>";

		const string CusEntryNumEvent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
   <Event>
      <DataContext>
         <CodesMappedToTarget>true</CodesMappedToTarget>
         <EnterpriseID>DFD</EnterpriseID>
         <ServerID>DQA</ServerID>
         <Company>
            <Code>EDI</Code>
         </Company>
         <DataTargetCollection />
      </DataContext>
      <EventTime>2017-10-26T13:01:54</EventTime>
      <EventType>WEM</EventType>
      <EventReference>Trigger for shipment confirmation</EventReference>
      <IsEstimate>false</IsEstimate>
      <ContextCollection>
         <Context>
            <Type>SSN</Type>
            <Value>7597063333</Value>
         </Context>
      </ContextCollection>
   </Event>
</UniversalEvent>";

		const string SampleDsvMessage1 = @"

<?xml version=""1.0"" encoding=""UTF-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
   <Shipment>
      <DataContext>
         <CodesMappedToTarget>true</CodesMappedToTarget>
         <EnterpriseID>DFD</EnterpriseID>
         <ServerID>DQA</ServerID>
         <Company>
            <Code>DE1</Code>
         </Company>
         <DataProvider>DSVCTW-DChain</DataProvider>
         <DataTargetCollection>
            <DataTarget>
               <Type>ForwardingConsol</Type>
            </DataTarget>
         </DataTargetCollection>
      </DataContext>
      <AgentsReference>125990</AgentsReference>
      <Branch>
         <Code>NAJ</Code>
      </Branch>
      <ContainerMode>
         <Code>BCN</Code>
      </ContainerMode>
      <PortFirstForeign>
         <Code>KRPUS</Code>
      </PortFirstForeign>
      <PortOfLoading>
         <Code>DEBRV</Code>
      </PortOfLoading>
      <PortOfDischarge>
         <Code>KRPUS</Code>
      </PortOfDischarge>
      <TransportMode>
         <Code>SEA</Code>
      </TransportMode>
      <WayBillNumber>MAEU963313512</WayBillNumber>
      <WayBillType>
         <Code>MWB</Code>
      </WayBillType>
      <AdditionalReferenceCollection Content=""Partial"">
         <AdditionalReference>
            <Type>
               <Code>SID</Code>
            </Type>
            <ReferenceNumber>DChain</ReferenceNumber>
         </AdditionalReference>
         <AdditionalReference>
            <Type>
               <Code>SCN</Code>
            </Type>
            <ReferenceNumber>125990</ReferenceNumber>
         </AdditionalReference>
         <AdditionalReference>
            <Type>
               <Code>STP</Code>
            </Type>
            <ReferenceNumber>DSVCTW</ReferenceNumber>
         </AdditionalReference>
      </AdditionalReferenceCollection>
      <CustomizedFieldCollection />
      <ContainerCollection>
         <Container>
            <ContainerCount>1</ContainerCount>
            <ContainerNumber>MSKU9552032</ContainerNumber>
            <ContainerType>
               <Code>40H</Code>
            </ContainerType>
            <FCL_LCL_AIR>
               <Code>GRP</Code>
            </FCL_LCL_AIR>
            <Link>1</Link>
            <Seal>CZ0229367</Seal>
         </Container>
      </ContainerCollection>
      <DateCollection />
      <NoteCollection />
      <OrganizationAddressCollection>
         <OrganizationAddress>
            <AddressType>SendingForwarderAddress</AddressType>
            <Address1>Ludwig-Erhard-Strasse 3</Address1>
            <City>Bremen</City>
            <CompanyName>DSV Air &amp; Sea GMBH</CompanyName>
            <Postcode>28195</Postcode>
         </OrganizationAddress>
         <OrganizationAddress>
            <AddressType>ReceivingForwarderAddress</AddressType>
            <OrganizationCode>BORYEONG</OrganizationCode>
            <Address1>256, GWANCHANGGONGDAN-GIL, JUGYO-MYEON</Address1>
            <City>BORYEONG-SI</City>
            <CompanyName>GM KOREA BORYEONG</CompanyName>
            <Postcode>33448</Postcode>
         </OrganizationAddress>
         <OrganizationAddress>
            <AddressType>DepartureCTOAddress</AddressType>
            <OrganizationCode>360561125</OrganizationCode>
            <Address1>Logisticka 100</Address1>
            <City>Pavlov</City>
            <CompanyName>DSV Road A.S.</CompanyName>
            <Postcode>27351</Postcode>
         </OrganizationAddress>
         <OrganizationAddress>
            <AddressType>ShippingLineAddress</AddressType>
            <OrganizationCode>65140630</OrganizationCode>
         </OrganizationAddress>
      </OrganizationAddressCollection>
      <SubShipmentCollection>
         <SubShipment>
            <DataContext>
               <CodesMappedToTarget>true</CodesMappedToTarget>
               <EnterpriseID>DFD</EnterpriseID>
               <ServerID>DQA</ServerID>
               <Company>
                  <Code>DE1</Code>
               </Company>
               <DataProvider>DSVCTW-DChain</DataProvider>
               <DataTargetCollection>
                  <DataTarget>
                     <Type>ForwardingShipment</Type>
                  </DataTarget>
               </DataTargetCollection>
            </DataContext>
            <BookingConfirmationReference>MAEU963313512</BookingConfirmationReference>
            <CFSReference />
            <ContainerMode>
               <Code>BCN</Code>
            </ContainerMode>
            <GoodsDescription>Component Parts</GoodsDescription>
            <JobCosting>
               <Branch>
                  <Code>NAJ</Code>
               </Branch>
               <Department>
                  <Code>P01</Code>
               </Department>
               <ChargeLineCollection />
            </JobCosting>
            <PortOfDestination>
               <Code>KRPUS</Code>
            </PortOfDestination>
            <PortOfOrigin>
               <Code>DEBRV</Code>
            </PortOfOrigin>
            <ReleaseType>
               <Code>EBL</Code>
            </ReleaseType>
            <ShipmentType>
               <Code>ASM</Code>
            </ShipmentType>
            <ServiceLevel>
               <Code>Door / Port</Code>
            </ServiceLevel>
            <ShipmentIncoTerm>
               <Code>FCA</Code>
            </ShipmentIncoTerm>
            <TransportMode>
               <Code>SEA</Code>
            </TransportMode>
            <WayBillNumber>MAEU963313512</WayBillNumber>
            <WayBillType>
               <Code>HWB</Code>
            </WayBillType>
            <LocalProcessing>
               <InsuranceRequired>false</InsuranceRequired>
               <OrderNumberCollection>
                  <OrderNumber>
                     <OrderReference>1207120657</OrderReference>
                     <Sequence>1</Sequence>
                  </OrderNumber>
               </OrderNumberCollection>
            </LocalProcessing>
            <AdditionalReferenceCollection Content=""Partial"">
               <AdditionalReference>
                  <Type>
                     <Code>SID</Code>
                  </Type>
                  <ReferenceNumber>DChain</ReferenceNumber>
               </AdditionalReference>
               <AdditionalReference>
                  <Type>
                     <Code>SSN</Code>
                  </Type>
                  <ReferenceNumber>MAEU963313512</ReferenceNumber>
               </AdditionalReference>
               <AdditionalReference>
                  <Type>
                     <Code>STP</Code>
                  </Type>
                  <ReferenceNumber>DSVCTW</ReferenceNumber>
               </AdditionalReference>
            </AdditionalReferenceCollection>
            <CustomizedFieldCollection />
            <DateCollection>
               <Date>
                  <Type>Departure</Type>
                  <IsEstimate>true</IsEstimate>
                  <Value>2017-12-31T23:30:00</Value>
               </Date>
               <Date>
                  <Type>Arrival</Type>
                  <IsEstimate>true</IsEstimate>
                  <Value>2018-02-04T23:30:00</Value>
               </Date>
            </DateCollection>
            <EntryNumberCollection />
            <NoteCollection>
               <Note>
                  <Description>Detailed Goods Description</Description>
                  <IsCustomDescription>false</IsCustomDescription>
                  <NoteText>Component Parts</NoteText>
               </Note>
            </NoteCollection>
            <OrganizationAddressCollection>
               <OrganizationAddress>
                  <AddressType>ConsigneeDocumentaryAddress</AddressType>
                  <OrganizationCode>BORYEONG</OrganizationCode>
                  <Address1>256, GWANCHANGGONGDAN-GIL, JUGYO-MYEON</Address1>
                  <City>BORYEONG-SI</City>
                  <CompanyName>GM KOREA BORYEONG</CompanyName>
                  <Postcode>33448</Postcode>
               </OrganizationAddress>
               <OrganizationAddress>
                  <AddressType>ConsignorDocumentaryAddress</AddressType>
                  <Address1>Ludwig-Erhard-Strasse 3</Address1>
                  <City>Bremen</City>
                  <CompanyName>DSV Air &amp; Sea GMBH</CompanyName>
                  <Postcode>28195</Postcode>
               </OrganizationAddress>
               <OrganizationAddress>
                  <AddressType>ConsignorPickupDeliveryAddress</AddressType>
                  <Address1>Ludwig-Erhard-Strasse 3</Address1>
                  <City>Bremen</City>
                  <CompanyName>DSV Air &amp; Sea GMBH</CompanyName>
                  <Postcode>28195</Postcode>
               </OrganizationAddress>
               <OrganizationAddress>
                  <AddressType>ConsigneePickupDeliveryAddress</AddressType>
                  <OrganizationCode>BORYEONG</OrganizationCode>
                  <Address1>256, GWANCHANGGONGDAN-GIL, JUGYO-MYEON</Address1>
                  <City>BORYEONG-SI</City>
                  <CompanyName>GM KOREA BORYEONG</CompanyName>
                  <Postcode>33448</Postcode>
               </OrganizationAddress>
            </OrganizationAddressCollection>
            <PackingLineCollection />
            <SubShipmentCollection>
               <SubShipment>
                  <DataContext>
                     <CodesMappedToTarget>true</CodesMappedToTarget>
                     <EnterpriseID>DFD</EnterpriseID>
                     <ServerID>DQA</ServerID>
                     <Company>
                        <Code>DE1</Code>
                     </Company>
                     <DataProvider>DSVCTW-DChain</DataProvider>
                     <DataTargetCollection>
                        <DataTarget>
                           <Type>ForwardingShipment</Type>
                        </DataTarget>
                     </DataTargetCollection>
                  </DataContext>
                  <BookingConfirmationReference>1207120657</BookingConfirmationReference>
                  <CFSReference />
                  <ContainerMode>
                     <Code>LCL</Code>
                  </ContainerMode>
                  <GoodsDescription>Component Parts</GoodsDescription>
                  <InterimReceiptNumber>MSKU9552032</InterimReceiptNumber>
                  <JobCosting>
                     <Branch>
                        <Code>NAJ</Code>
                     </Branch>
                     <Department>
                        <Code>P01</Code>
                     </Department>
                     <ChargeLineCollection />
                  </JobCosting>
                  <OuterPacks>1</OuterPacks>
                  <OuterPacksPackageType>
                     <Code>PKG</Code>
                  </OuterPacksPackageType>
                  <PortOfDestination>
                     <Code>KRPUS</Code>
                  </PortOfDestination>
                  <PortOfOrigin>
                     <Code>DEBRV</Code>
                  </PortOfOrigin>
                  <ReleaseType>
                     <Code>EBL</Code>
                  </ReleaseType>
                  <ServiceLevel>
                     <Code>Door / Port</Code>
                  </ServiceLevel>
                  <ShipmentIncoTerm>
                     <Code>FCA</Code>
                  </ShipmentIncoTerm>
                  <TotalVolume>1.056</TotalVolume>
                  <TotalVolumeUnit>
                     <Code>M3</Code>
                  </TotalVolumeUnit>
                  <TotalWeight>339.21</TotalWeight>
                  <TotalWeightUnit>
                     <Code>KG</Code>
                  </TotalWeightUnit>
                  <TransportMode>
                     <Code>SEA</Code>
                  </TransportMode>
                  <WayBillNumber>1207120657</WayBillNumber>
                  <WayBillType>
                     <Code>HWB</Code>
                  </WayBillType>
                  <LocalProcessing>
                     <DeliveryRequiredBy>2017-12-19T00:00:00</DeliveryRequiredBy>
                     <EstimatedPickup>2017-12-15T10:00:00</EstimatedPickup>
                     <InsuranceRequired>false</InsuranceRequired>
                     <PickupCartageCompleted>2017-12-15T00:00:00</PickupCartageCompleted>
                     <PickupRequiredBy>2017-12-15T12:00:00</PickupRequiredBy>
                     <OrderNumberCollection>
                        <OrderNumber>
                           <OrderReference>1207120657</OrderReference>
                           <Sequence>1</Sequence>
                        </OrderNumber>
                     </OrderNumberCollection>
                  </LocalProcessing>
                  <AdditionalReferenceCollection Content=""Partial"">
                     <AdditionalReference>
                        <Type>
                           <Code>SID</Code>
                        </Type>
                        <ReferenceNumber>DChain</ReferenceNumber>
                     </AdditionalReference>
                     <AdditionalReference>
                        <Type>
                           <Code>SSN</Code>
                        </Type>
                        <ReferenceNumber>1207120657</ReferenceNumber>
                     </AdditionalReference>
                     <AdditionalReference>
                        <Type>
                           <Code>STP</Code>
                        </Type>
                        <ReferenceNumber>DSVCTW</ReferenceNumber>
                     </AdditionalReference>
                  </AdditionalReferenceCollection>
                  <CustomizedFieldCollection />
                  <DateCollection>
                     <Date>
                        <Type>Departure</Type>
                        <IsEstimate>true</IsEstimate>
                        <Value>2017-12-31T23:30:00</Value>
                     </Date>
                     <Date>
                        <Type>Arrival</Type>
                        <IsEstimate>true</IsEstimate>
                        <Value>2018-02-04T23:30:00</Value>
                     </Date>
                  </DateCollection>
                  <EntryNumberCollection />
                  <NoteCollection>
                     <Note>
                        <Description>Export Pickup Instructions</Description>
                        <IsCustomDescription>false</IsCustomDescription>
                        <NoteText>Pickup BONTAZ CENTRE CZ SRO LESNI 401 27361 VELKA DOBRA CZ, Czech Republic</NoteText>
                     </Note>
                     <Note>
                        <Description>Detailed Goods Description</Description>
                        <IsCustomDescription>false</IsCustomDescription>
                        <NoteText>Component Parts</NoteText>
                     </Note>
                  </NoteCollection>
                  <OrganizationAddressCollection>
                     <OrganizationAddress>
                        <AddressType>ConsigneeDocumentaryAddress</AddressType>
                        <OrganizationCode>BORYEONG</OrganizationCode>
                        <Address1>256, GWANCHANGGONGDAN-GIL, JUGYO-MYEON</Address1>
                        <City>BORYEONG-SI</City>
                        <CompanyName>GM KOREA BORYEONG</CompanyName>
                        <Postcode>33448</Postcode>
                     </OrganizationAddress>
                     <OrganizationAddress>
                        <AddressType>ConsignorDocumentaryAddress</AddressType>
                        <OrganizationCode>360549539</OrganizationCode>
                        <Address1>LESNI 401</Address1>
                        <City>VELKA DOBRA</City>
                        <CompanyName>BONTAZ CENTRE CZ SRO</CompanyName>
                        <Postcode>27361</Postcode>
                     </OrganizationAddress>
                     <OrganizationAddress>
                        <AddressType>PickupLocalCartage</AddressType>
                        <OrganizationCode>GMPUCOMPANY</OrganizationCode>
                     </OrganizationAddress>
                     <OrganizationAddress>
                        <AddressType>ConsignorPickupDeliveryAddress</AddressType>
                        <OrganizationCode>360549539</OrganizationCode>
                        <Address1>LESNI 401</Address1>
                        <City>VELKA DOBRA</City>
                        <CompanyName>BONTAZ CENTRE CZ SRO</CompanyName>
                        <Postcode>27361</Postcode>
                     </OrganizationAddress>
                     <OrganizationAddress>
                        <AddressType>ConsigneePickupDeliveryAddress</AddressType>
                        <OrganizationCode>BORYEONG</OrganizationCode>
                        <Address1>256, GWANCHANGGONGDAN-GIL, JUGYO-MYEON</Address1>
                        <City>BORYEONG-SI</City>
                        <CompanyName>GM KOREA BORYEONG</CompanyName>
                        <Postcode>33448</Postcode>
                     </OrganizationAddress>
                  </OrganizationAddressCollection>
                  <PackingLineCollection>
                     <PackingLine>
                        <ContainerLink>1</ContainerLink>
                        <ContainerNumber>MSKU9552032</ContainerNumber>
                        <LengthUnit>
                           <Code />
                        </LengthUnit>
                        <PackQty>1</PackQty>
                        <PackType>
                           <Code>PKG</Code>
                        </PackType>
                        <Volume>1.056</Volume>
                        <VolumeUnit>
                           <Code>M3</Code>
                        </VolumeUnit>
                        <Weight>339.21</Weight>
                        <WeightUnit>
                           <Code>KG</Code>
                        </WeightUnit>
                        <CustomizedFieldCollection>
                           <CustomizedField>
                              <Key>DOCREM2</Key>
                              <DataType>String</DataType>
                              <Value>MSKU9552032</Value>
                           </CustomizedField>
                           <CustomizedField>
                              <Key>DOCREM3</Key>
                              <DataType>String</DataType>
                              <Value />
                           </CustomizedField>
                        </CustomizedFieldCollection>
                     </PackingLine>
                  </PackingLineCollection>
                  <SubShipmentCollection />
               </SubShipment>
            </SubShipmentCollection>
         </SubShipment>
      </SubShipmentCollection>
      <TransportLegCollection>
         <TransportLeg>
            <PortOfLoading>
               <Code>DEBRV</Code>
            </PortOfLoading>
            <PortOfDischarge>
               <Code>KRPUS</Code>
            </PortOfDischarge>
            <LegOrder>1</LegOrder>
            <TransportMode>Sea</TransportMode>
            <EstimatedArrival>2018-02-04T23:30:00</EstimatedArrival>
            <EstimatedDeparture>2017-12-31T23:30:00</EstimatedDeparture>
            <LegType>Main</LegType>
            <VesselName>MARGRETHE MAERSK</VesselName>
            <VoyageFlightNo>752E</VoyageFlightNo>
         </TransportLeg>
      </TransportLegCollection>
   </Shipment>
</UniversalShipment>";

		const string sampleDsvMessage2 = @"

<?xml version=""1.0"" encoding=""UTF-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
   <Shipment>
      <DataContext>
         <CodesMappedToTarget>true</CodesMappedToTarget>
         <EnterpriseID>DFD</EnterpriseID>
         <ServerID>DQA</ServerID>
         <Company>
            <Code>DE1</Code>
         </Company>
         <DataProvider>DSVCTW-DChain</DataProvider>
         <DataTargetCollection>
            <DataTarget>
               <Type>ForwardingConsol</Type>
            </DataTarget>
         </DataTargetCollection>
      </DataContext>
      <AgentsReference>125987</AgentsReference>
      <Branch>
         <Code>NAJ</Code>
      </Branch>
      <ContainerMode>
         <Code>BCN</Code>
      </ContainerMode>
      <PortFirstForeign>
         <Code>KRPUS</Code>
      </PortFirstForeign>
      <PortOfLoading>
         <Code>DEBRV</Code>
      </PortOfLoading>
      <PortOfDischarge>
         <Code>KRPUS</Code>
      </PortOfDischarge>
      <TransportMode>
         <Code>SEA</Code>
      </TransportMode>
      <WayBillNumber>MAEU963313512</WayBillNumber>
      <WayBillType>
         <Code>MWB</Code>
      </WayBillType>
      <AdditionalReferenceCollection Content=""Partial"">
         <AdditionalReference>
            <Type>
               <Code>SID</Code>
            </Type>
            <ReferenceNumber>DChain</ReferenceNumber>
         </AdditionalReference>
         <AdditionalReference>
            <Type>
               <Code>SCN</Code>
            </Type>
            <ReferenceNumber>125987</ReferenceNumber>
         </AdditionalReference>
         <AdditionalReference>
            <Type>
               <Code>STP</Code>
            </Type>
            <ReferenceNumber>DSVCTW</ReferenceNumber>
         </AdditionalReference>
      </AdditionalReferenceCollection>
      <CustomizedFieldCollection />
      <ContainerCollection>
         <Container>
            <ContainerCount>1</ContainerCount>
            <ContainerNumber>MSKU9552032</ContainerNumber>
            <ContainerType>
               <Code>40H</Code>
            </ContainerType>
            <FCL_LCL_AIR>
               <Code>GRP</Code>
            </FCL_LCL_AIR>
            <Link>1</Link>
            <Seal>CZ0229367</Seal>
         </Container>
      </ContainerCollection>
      <DateCollection />
      <NoteCollection />
      <OrganizationAddressCollection>
         <OrganizationAddress>
            <AddressType>SendingForwarderAddress</AddressType>
            <Address1>Ludwig-Erhard-Strasse 3</Address1>
            <City>Bremen</City>
            <CompanyName>DSV Air &amp; Sea GMBH</CompanyName>
            <Postcode>28195</Postcode>
         </OrganizationAddress>
         <OrganizationAddress>
            <AddressType>ReceivingForwarderAddress</AddressType>
            <OrganizationCode>BUPYEONG</OrganizationCode>
            <Address1>233, BUPYEONG-DAERO</Address1>
            <City>BUPYEONG-GU</City>
            <CompanyName>GM KOREA BUPYEONG</CompanyName>
            <Postcode>21334</Postcode>
         </OrganizationAddress>
         <OrganizationAddress>
            <AddressType>DepartureCTOAddress</AddressType>
            <OrganizationCode>360561125</OrganizationCode>
            <Address1>Logisticka 100</Address1>
            <City>Pavlov</City>
            <CompanyName>DSV Road A.S.</CompanyName>
            <Postcode>27351</Postcode>
         </OrganizationAddress>
         <OrganizationAddress>
            <AddressType>ShippingLineAddress</AddressType>
            <OrganizationCode>65140630</OrganizationCode>
         </OrganizationAddress>
      </OrganizationAddressCollection>
      <SubShipmentCollection>
         <SubShipment>
            <DataContext>
               <CodesMappedToTarget>true</CodesMappedToTarget>
               <EnterpriseID>DFD</EnterpriseID>
               <ServerID>DQA</ServerID>
               <Company>
                  <Code>DE1</Code>
               </Company>
               <DataProvider>DSVCTW-DChain</DataProvider>
               <DataTargetCollection>
                  <DataTarget>
                     <Type>ForwardingShipment</Type>
                  </DataTarget>
               </DataTargetCollection>
            </DataContext>
            <BookingConfirmationReference>MAEU963313512</BookingConfirmationReference>
            <CFSReference />
            <ContainerMode>
               <Code>BCN</Code>
            </ContainerMode>
            <GoodsDescription>Component Parts</GoodsDescription>
            <JobCosting>
               <Branch>
                  <Code>NAJ</Code>
               </Branch>
               <Department>
                  <Code>P01</Code>
               </Department>
               <ChargeLineCollection />
            </JobCosting>
            <PortOfDestination>
               <Code>KRPUS</Code>
            </PortOfDestination>
            <PortOfOrigin>
               <Code>DEBRV</Code>
            </PortOfOrigin>
            <ReleaseType>
               <Code>EBL</Code>
            </ReleaseType>
            <ShipmentType>
               <Code>ASM</Code>
            </ShipmentType>
            <ServiceLevel>
               <Code>Door / Port</Code>
            </ServiceLevel>
            <ShipmentIncoTerm>
               <Code>FCA</Code>
            </ShipmentIncoTerm>
            <TransportMode>
               <Code>SEA</Code>
            </TransportMode>
            <WayBillNumber>MAEU963313512</WayBillNumber>
            <WayBillType>
               <Code>HWB</Code>
            </WayBillType>
            <LocalProcessing>
               <InsuranceRequired>false</InsuranceRequired>
               <OrderNumberCollection>
                  <OrderNumber>
                     <OrderReference>1207120858</OrderReference>
                     <Sequence>1</Sequence>
                  </OrderNumber>
               </OrderNumberCollection>
            </LocalProcessing>
            <AdditionalReferenceCollection Content=""Partial"">
               <AdditionalReference>
                  <Type>
                     <Code>SID</Code>
                  </Type>
                  <ReferenceNumber>DChain</ReferenceNumber>
               </AdditionalReference>
               <AdditionalReference>
                  <Type>
                     <Code>SSN</Code>
                  </Type>
                  <ReferenceNumber>MAEU963313512</ReferenceNumber>
               </AdditionalReference>
               <AdditionalReference>
                  <Type>
                     <Code>STP</Code>
                  </Type>
                  <ReferenceNumber>DSVCTW</ReferenceNumber>
               </AdditionalReference>
            </AdditionalReferenceCollection>
            <CustomizedFieldCollection />
            <DateCollection>
               <Date>
                  <Type>Departure</Type>
                  <IsEstimate>true</IsEstimate>
                  <Value>2017-12-31T23:30:00</Value>
               </Date>
               <Date>
                  <Type>Arrival</Type>
                  <IsEstimate>true</IsEstimate>
                  <Value>2018-02-04T23:30:00</Value>
               </Date>
            </DateCollection>
            <EntryNumberCollection />
            <NoteCollection>
               <Note>
                  <Description>Detailed Goods Description</Description>
                  <IsCustomDescription>false</IsCustomDescription>
                  <NoteText>Component Parts</NoteText>
               </Note>
            </NoteCollection>
            <OrganizationAddressCollection>
               <OrganizationAddress>
                  <AddressType>ConsigneeDocumentaryAddress</AddressType>
                  <OrganizationCode>BUPYEONG</OrganizationCode>
                  <Address1>233, BUPYEONG-DAERO</Address1>
                  <City>BUPYEONG-GU</City>
                  <CompanyName>GM KOREA BUPYEONG</CompanyName>
                  <Postcode>21334</Postcode>
               </OrganizationAddress>
               <OrganizationAddress>
                  <AddressType>ConsignorDocumentaryAddress</AddressType>
                  <Address1>Ludwig-Erhard-Strasse 3</Address1>
                  <City>Bremen</City>
                  <CompanyName>DSV Air &amp; Sea GMBH</CompanyName>
                  <Postcode>28195</Postcode>
               </OrganizationAddress>
               <OrganizationAddress>
                  <AddressType>ConsignorPickupDeliveryAddress</AddressType>
                  <Address1>Ludwig-Erhard-Strasse 3</Address1>
                  <City>Bremen</City>
                  <CompanyName>DSV Air &amp; Sea GMBH</CompanyName>
                  <Postcode>28195</Postcode>
               </OrganizationAddress>
               <OrganizationAddress>
                  <AddressType>ConsigneePickupDeliveryAddress</AddressType>
                  <OrganizationCode>BUPYEONG</OrganizationCode>
                  <Address1>233, BUPYEONG-DAERO</Address1>
                  <City>BUPYEONG-GU</City>
                  <CompanyName>GM KOREA BUPYEONG</CompanyName>
                  <Postcode>21334</Postcode>
               </OrganizationAddress>
            </OrganizationAddressCollection>
            <PackingLineCollection />
            <SubShipmentCollection>
               <SubShipment>
                  <DataContext>
                     <CodesMappedToTarget>true</CodesMappedToTarget>
                     <EnterpriseID>DFD</EnterpriseID>
                     <ServerID>DQA</ServerID>
                     <Company>
                        <Code>DE1</Code>
                     </Company>
                     <DataProvider>DSVCTW-DChain</DataProvider>
                     <DataTargetCollection>
                        <DataTarget>
                           <Type>ForwardingShipment</Type>
                        </DataTarget>
                     </DataTargetCollection>
                  </DataContext>
                  <BookingConfirmationReference>1207120858</BookingConfirmationReference>
                  <CFSReference />
                  <ContainerMode>
                     <Code>LCL</Code>
                  </ContainerMode>
                  <GoodsDescription>Component Parts</GoodsDescription>
                  <InterimReceiptNumber>MSKU9552032</InterimReceiptNumber>
                  <JobCosting>
                     <Branch>
                        <Code>NAJ</Code>
                     </Branch>
                     <Department>
                        <Code>P01</Code>
                     </Department>
                     <ChargeLineCollection />
                  </JobCosting>
                  <OuterPacks>1</OuterPacks>
                  <OuterPacksPackageType>
                     <Code>PKG</Code>
                  </OuterPacksPackageType>
                  <PortOfDestination>
                     <Code>KRPUS</Code>
                  </PortOfDestination>
                  <PortOfOrigin>
                     <Code>DEBRV</Code>
                  </PortOfOrigin>
                  <ReleaseType>
                     <Code>EBL</Code>
                  </ReleaseType>
                  <ServiceLevel>
                     <Code>Door / Port</Code>
                  </ServiceLevel>
                  <ShipmentIncoTerm>
                     <Code>FCA</Code>
                  </ShipmentIncoTerm>
                  <TotalVolume>1.14</TotalVolume>
                  <TotalVolumeUnit>
                     <Code>M3</Code>
                  </TotalVolumeUnit>
                  <TotalWeight>128</TotalWeight>
                  <TotalWeightUnit>
                     <Code>KG</Code>
                  </TotalWeightUnit>
                  <TransportMode>
                     <Code>SEA</Code>
                  </TransportMode>
                  <WayBillNumber>1207120858</WayBillNumber>
                  <WayBillType>
                     <Code>HWB</Code>
                  </WayBillType>
                  <LocalProcessing>
                     <DeliveryRequiredBy>2017-12-22T00:00:00</DeliveryRequiredBy>
                     <EstimatedPickup>2017-12-15T14:00:00</EstimatedPickup>
                     <InsuranceRequired>false</InsuranceRequired>
                     <PickupCartageCompleted>2017-12-15T00:00:00</PickupCartageCompleted>
                     <PickupRequiredBy>2017-12-15T16:00:00</PickupRequiredBy>
                     <OrderNumberCollection>
                        <OrderNumber>
                           <OrderReference>1207120858</OrderReference>
                           <Sequence>1</Sequence>
                        </OrderNumber>
                     </OrderNumberCollection>
                  </LocalProcessing>
                  <AdditionalReferenceCollection Content=""Partial"">
                     <AdditionalReference>
                        <Type>
                           <Code>SID</Code>
                        </Type>
                        <ReferenceNumber>DChain</ReferenceNumber>
                     </AdditionalReference>
                     <AdditionalReference>
                        <Type>
                           <Code>SSN</Code>
                        </Type>
                        <ReferenceNumber>1207120858</ReferenceNumber>
                     </AdditionalReference>
                     <AdditionalReference>
                        <Type>
                           <Code>STP</Code>
                        </Type>
                        <ReferenceNumber>DSVCTW</ReferenceNumber>
                     </AdditionalReference>
                  </AdditionalReferenceCollection>
                  <CustomizedFieldCollection />
                  <DateCollection>
                     <Date>
                        <Type>Departure</Type>
                        <IsEstimate>true</IsEstimate>
                        <Value>2017-12-31T23:30:00</Value>
                     </Date>
                     <Date>
                        <Type>Arrival</Type>
                        <IsEstimate>true</IsEstimate>
                        <Value>2018-02-04T23:30:00</Value>
                     </Date>
                  </DateCollection>
                  <EntryNumberCollection />
                  <NoteCollection>
                     <Note>
                        <Description>Export Pickup Instructions</Description>
                        <IsCustomDescription>false</IsCustomDescription>
                        <NoteText>Pickup Grupo Antolin-Sibiu Str. Europa Unita Nr. 7 550018 SIBIU RO, Romania</NoteText>
                     </Note>
                     <Note>
                        <Description>Detailed Goods Description</Description>
                        <IsCustomDescription>false</IsCustomDescription>
                        <NoteText>Component Parts</NoteText>
                     </Note>
                  </NoteCollection>
                  <OrganizationAddressCollection>
                     <OrganizationAddress>
                        <AddressType>ConsigneeDocumentaryAddress</AddressType>
                        <OrganizationCode>BUPYEONG</OrganizationCode>
                        <Address1>233, BUPYEONG-DAERO</Address1>
                        <City>BUPYEONG-GU</City>
                        <CompanyName>GM KOREA BUPYEONG</CompanyName>
                        <Postcode>21334</Postcode>
                     </OrganizationAddress>
                     <OrganizationAddress>
                        <AddressType>ConsignorDocumentaryAddress</AddressType>
                        <OrganizationCode>000179614</OrganizationCode>
                        <Address1>Str. Europa Unita Nr. 7</Address1>
                        <City>SIBIU</City>
                        <CompanyName>Grupo Antolin-Sibiu</CompanyName>
                        <Postcode>550018</Postcode>
                     </OrganizationAddress>
                     <OrganizationAddress>
                        <AddressType>PickupLocalCartage</AddressType>
                        <OrganizationCode>GMPUCOMPANY</OrganizationCode>
                     </OrganizationAddress>
                     <OrganizationAddress>
                        <AddressType>ConsignorPickupDeliveryAddress</AddressType>
                        <OrganizationCode>000179614</OrganizationCode>
                        <Address1>Str. Europa Unita Nr. 7</Address1>
                        <City>SIBIU</City>
                        <CompanyName>Grupo Antolin-Sibiu</CompanyName>
                        <Postcode>550018</Postcode>
                     </OrganizationAddress>
                     <OrganizationAddress>
                        <AddressType>ConsigneePickupDeliveryAddress</AddressType>
                        <OrganizationCode>BUPYEONG</OrganizationCode>
                        <Address1>233, BUPYEONG-DAERO</Address1>
                        <City>BUPYEONG-GU</City>
                        <CompanyName>GM KOREA BUPYEONG</CompanyName>
                        <Postcode>21334</Postcode>
                     </OrganizationAddress>
                  </OrganizationAddressCollection>
                  <PackingLineCollection>
                     <PackingLine>
                        <ContainerLink>1</ContainerLink>
                        <ContainerNumber>MSKU9552032</ContainerNumber>
                        <LengthUnit>
                           <Code />
                        </LengthUnit>
                        <PackQty>1</PackQty>
                        <PackType>
                           <Code>PKG</Code>
                        </PackType>
                        <Volume>1.14</Volume>
                        <VolumeUnit>
                           <Code>M3</Code>
                        </VolumeUnit>
                        <Weight>128</Weight>
                        <WeightUnit>
                           <Code>KG</Code>
                        </WeightUnit>
                        <CustomizedFieldCollection>
                           <CustomizedField>
                              <Key>DOCREM2</Key>
                              <DataType>String</DataType>
                              <Value>MSKU9552032</Value>
                           </CustomizedField>
                           <CustomizedField>
                              <Key>DOCREM3</Key>
                              <DataType>String</DataType>
                              <Value />
                           </CustomizedField>
                        </CustomizedFieldCollection>
                     </PackingLine>
                  </PackingLineCollection>
                  <SubShipmentCollection />
               </SubShipment>
            </SubShipmentCollection>
         </SubShipment>
      </SubShipmentCollection>
      <TransportLegCollection>
         <TransportLeg>
            <PortOfLoading>
               <Code>DEBRV</Code>
            </PortOfLoading>
            <PortOfDischarge>
               <Code>KRPUS</Code>
            </PortOfDischarge>
            <LegOrder>1</LegOrder>
            <TransportMode>Sea</TransportMode>
            <EstimatedArrival>2018-02-04T23:30:00</EstimatedArrival>
            <EstimatedDeparture>2017-12-31T23:30:00</EstimatedDeparture>
            <LegType>Main</LegType>
            <VesselName>MARGRETHE MAERSK</VesselName>
            <VoyageFlightNo>752E</VoyageFlightNo>
         </TransportLeg>
      </TransportLegCollection>
   </Shipment>
</UniversalShipment>";

		const string manifest = @"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>AsycudaManifest</Type>
        </DataTarget>
      </DataTargetCollection>

      <ActionPurpose>
        <Code>EVT</Code>
        <Description>Event</Description>
      </ActionPurpose>
      <Company>
        <Code>SOA</Code>
        <Country>
          <Code>ZA</Code>
          <Name>South Africa</Name>
        </Country>
        <Name>CEVA Logistics South Africa Pty Ltd. Johannesburg</Name>
      </Company>
      <DataProvider>CEVTSTSOA</DataProvider>
      <EnterpriseID>CEV</EnterpriseID>
      <EventBranch>
        <Code>050</Code>
        <Name>JNB - Johannesburg</Name>
      </EventBranch>
      <EventDepartment>
        <Code>ALL</Code>
        <Name>All Modes</Name>
      </EventDepartment>
      <EventType>
        <Code>ATH</Code>
        <Description>Authorised</Description>
      </EventType>
      <EventUser>
        <Code>AS</Code>
        <Name>Atul Shukla</Name>
      </EventUser>
      <ServerID>TST</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2018-12-26T18:58:47.157</TriggerDate>
      <TriggerDescription>TEST</TriggerDescription>
      <TriggerType>Manual</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ORP</Code>
          <Description>Organisation Proxy</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <Branch>
      <Code>050</Code>
      <Name>JNB - Johannesburg</Name>
    </Branch>
    <CustomsBroker>
      <Code>AS</Code>
      <Name>Atul Shukla</Name>
    </CustomsBroker>
    <CustomsOffice>
      <Code>DBN</Code>
      <Description>DURBAN</Description>
    </CustomsOffice>
    <LloydsIMO></LloydsIMO>
    <PortOfDischarge>
      <Code>ZADUR</Code>
      <Name>Durban</Name>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>DEBRV</Code>
      <Name>Bremerhaven</Name>
    </PortOfLoading>
    <TransportMode>
      <Code>SEA</Code>
      <Description>SEA</Description>
    </TransportMode>
    <VesselName>MOL Proficiency</VesselName>
    <VoyageFlightNo>190A</VoyageFlightNo>
    <WayBillNumber>DUR102905</WayBillNumber>

    <AddInfoCollection>
      <AddInfo>
        <Key>ConveyanceNationality</Key>
        <Value>DE</Value>
      </AddInfo>
      <AddInfo>
        <Key>ManifestNumber</Key>
        <Value>DUR102905</Value>
      </AddInfo>
      <AddInfo>
        <Key>MasterInformation</Key>
        <Value>DAL</Value>
      </AddInfo>
      <AddInfo>
        <Key>CustomsOffice</Key>
        <Value>DBN</Value>
      </AddInfo>
      <AddInfo>
        <Key>VesselCarrierCode</Key>
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>VesselRadioCallSign</Key>
        <Value>V7NH8</Value>
      </AddInfo>
      <AddInfo>
        <Key>VesselScreeningStatus</Key>
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>VesselVesselType</Key>
        <Value>CV</Value>
      </AddInfo>
      <AddInfo>
        <Key>VesselYearOfConstruction</Key>
        <Value>0</Value>
      </AddInfo>
      <AddInfo>
        <Key>VesselNetTonnage</Key>
        <Value>0</Value>
      </AddInfo>
    </AddInfoCollection>

    <ContainerCollection Content=""Complete"">
      <Container>
        <Commodity>
          <Code>PLT</Code>
          <Description></Description>
        </Commodity>
        <ContainerNumber>CAIU9003147</ContainerNumber>
        <ContainerType>
          <Code>40HC</Code>
          <Category>
            <Code>DRY</Code>
            <Description>Dry Storage</Description>
          </Category>
          <Description>Forty foot high cube</Description>
          <ISOCode>45G0</ISOCode>
        </ContainerType>
        <GrossWeight>350</GrossWeight>
        <Seal>WWA032497A</Seal>
        <SecondSeal></SecondSeal>
        <StowagePosition></StowagePosition>
        <ThirdSeal></ThirdSeal>
        <TotalHeight>9.500</TotalHeight>
        <TotalLength>40.000</TotalLength>
        <TotalWidth>8.000</TotalWidth>
        <WeightUnit>
          <Code>KG</Code>
        </WeightUnit>

        <AddInfoCollection>
          <AddInfo>
            <Key>ACN_EmptyFullIndicator</Key>
            <Value>LCL</Value>
          </AddInfo>
          <AddInfo>
            <Key>ACN_SealingPartyName</Key>
            <Value>SHIPCO</Value>
          </AddInfo>
          <AddInfo>
            <Key>ACN_SealingPartyType</Key>
            <Value>CAR</Value>
          </AddInfo>
          <AddInfo>
            <Key>ACN_NumberOfPackages</Key>
            <Value>1</Value>
          </AddInfo>
        </AddInfoCollection>
      </Container>
    </ContainerCollection>

    <DateCollection>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2018-12-20T00:00:00</Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2019-01-11T20:43:00</Value>
      </Date>
    </DateCollection>

    <EntryHeaderCollection>
      <EntryHeader>
        <Type>
          <Code>ZA</Code>
          <Description>South Africa</Description>
        </Type>
        <EntryInstructionLink>1</EntryInstructionLink>
      </EntryHeader>
    </EntryHeaderCollection>

    <EntryInstructionCollection>
      <EntryInstruction>
        <CustomsOffice>
          <Code>DBN</Code>
          <Description>DURBAN</Description>
        </CustomsOffice>
        <DateAtCustomsOffice></DateAtCustomsOffice>
        <FirstArrival>
          <Code>ZADUR</Code>
          <Name>Durban</Name>
        </FirstArrival>
        <Link>1</Link>
        <Style>ALH</Style>

        <AddInfoCollection>
          <AddInfo>
            <Key>AHC_Nature</Key>
            <Value>IMP</Value>
          </AddInfo>
          <AddInfo>
            <Key>PlaceOfEntry</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>EXI</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>TR1</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>TR2</Key>
            <Value></Value>
          </AddInfo>
        </AddInfoCollection>
      </EntryInstruction>
    </EntryInstructionCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>Carrier</AddressType>
        <Address1>SHARAF HOUSE 1ST FL LA LUCIA RIDGE OFFICE ESTATE</Address1>
        <Address2>2 SINEMBE CRESCENT</Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>SHARAF HOUSE 1ST FL LA LU</AddressShortCode>
        <City>DURBAN</City>
        <CompanyName>DAL AGENCY (PTY) LTD</CompanyName>
        <Country>
          <Code>ZA</Code>
          <Name>South Africa</Name>
        </Country>
        <Email></Email>
        <Fax>+27315829401</Fax>
        <OrganizationCode>ZADAYU</OrganizationCode>
        <Phone>+27315829400</Phone>
        <Port>
          <Code>ZADUR</Code>
          <Name>Durban</Name>
        </Port>
        <Postcode>4051</Postcode>
        <ScreeningStatus>
          <Code>NOT</Code>
          <Description>Not Screened</Description>
        </ScreeningStatus>
        <State></State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>CCC</Code>
              <Description>Customs Carrier Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>ZA</Code>
              <Name>South Africa</Name>
            </CountryOfIssue>
            <Value>DAYU</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>Branch</AddressType>
        <Address1>23 POMONA RD</Address1>
        <Address2>POMONA</Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>ONEID</AddressShortCode>
        <City>KEMPTON PARK</City>
        <CompanyName>CEVA LOGISTICS SOUTH AFRICA PTY LTD</CompanyName>
        <Country>
          <Code>ZA</Code>
          <Name>South Africa</Name>
        </Country>
        <Email>VICKY.DE.BRUYN@CEVALOGISTICS.COM</Email>
        <Fax></Fax>
        <OrganizationCode>595693</OrganizationCode>
        <Phone></Phone>
        <Port>
          <Code>ZAKMP</Code>
          <Name>Kempton Park</Name>
        </Port>
        <Postcode>1620</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State></State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>AGT</Code>
              <Description>Agent Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>ZA</Code>
              <Name>South Africa</Name>
            </CountryOfIssue>
            <Value>01871019</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>CDP</Code>
              <Description>Customs Dual Profile Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>ZA</Code>
              <Name>South Africa</Name>
            </CountryOfIssue>
            <Value>WTG</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>

    <SubShipmentCollection>
      <SubShipment>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>AsycudaBill</Type>
              <Key>FRA186402104</Key>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>

        <CommercialInfo>
          <CommercialChargeCollection>
            <CommercialCharge>
              <ChargeType>
                <Code>CDU</Code>
                <Description>Customs Duty Payable</Description>
              </ChargeType>
              <Amount>0</Amount>
              <Currency>
                <Code>ZAR</Code>
                <Description>South African Rand</Description>
              </Currency>
            </CommercialCharge>
            <CommercialCharge>
              <ChargeType>
                <Code>CUS</Code>
                <Description>Customs Value</Description>
              </ChargeType>
              <Amount>0.0000</Amount>
            </CommercialCharge>
            <CommercialCharge>
              <ChargeType>
                <Code>DIS</Code>
                <Description>Discount</Description>
              </ChargeType>
              <Amount>0</Amount>
            </CommercialCharge>
            <CommercialCharge>
              <ChargeType>
                <Code>EXW</Code>
                <Description>Ex-Works Amount</Description>
              </ChargeType>
              <Amount>0.0000</Amount>
            </CommercialCharge>
            <CommercialCharge>
              <ChargeType>
                <Code>GST</Code>
                <Description>Goods and Services Tax</Description>
              </ChargeType>
              <Amount>0</Amount>
              <Currency>
                <Code>ZAR</Code>
                <Description>South African Rand</Description>
              </Currency>
            </CommercialCharge>
            <CommercialCharge>
              <ChargeType>
                <Code>OFT</Code>
                <Description>International Freight</Description>
              </ChargeType>
              <Amount>0.0000</Amount>
            </CommercialCharge>
            <CommercialCharge>
              <ChargeType>
                <Code>ONS</Code>
                <Description>International Insurance</Description>
              </ChargeType>
              <Amount>0.0000</Amount>
            </CommercialCharge>
            <CommercialCharge>
              <ChargeType>
                <Code>OTH</Code>
                <Description>Other Charges</Description>
              </ChargeType>
              <Amount>0</Amount>
            </CommercialCharge>
          </CommercialChargeCollection>
        </CommercialInfo>
        <GoodsDescription>PON-ATLAS-LOCKNIT</GoodsDescription>
        <OuterPacks>1</OuterPacks>
        <OuterPacksPackageType>
          <Code>PLT</Code>
          <Description>Pallet</Description>
        </OuterPacksPackageType>
        <PortOfDestination>
          <Code>ZADUR</Code>
          <Name>Durban</Name>
        </PortOfDestination>
        <PortOfOrigin>
          <Code>DEBRV</Code>
          <Name>Bremerhaven</Name>
        </PortOfOrigin>
        <TotalVolume>5.44</TotalVolume>
        <TotalVolumeUnit>
          <Code>M3</Code>
          <Description>Cubic metre</Description>
        </TotalVolumeUnit>
        <TotalWeight>350</TotalWeight>
        <TotalWeightUnit>
          <Code>KG</Code>
          <Description>kilogram</Description>
        </TotalWeightUnit>
        <WayBillNumber>FRA186402104</WayBillNumber>
        <WayBillType>
          <Code>HWB</Code>
          <Description>House Waybill</Description>
        </WayBillType>

        <AddInfoCollection>
          <AddInfo>
            <Key>ABL_BolType</Key>
            <Value>STD</Value>
          </AddInfo>
          <AddInfo>
            <Key>ABL_LocationOfGoods</Key>
            <Value>J7</Value>
          </AddInfo>
          <AddInfo>
            <Key>ABL_ShipmentType</Key>
            <Value>IMP</Value>
          </AddInfo>
          <AddInfo>
            <Key>ABL_MarksAndNumbers</Key>
            <Value>785/105</Value>
          </AddInfo>
        </AddInfoCollection>

        <EntryHeaderCollection>
          <EntryHeader>
            <Type>
              <Code>ZA</Code>
              <Description>South Africa</Description>
            </Type>
            <EntryInstructionLink>1</EntryInstructionLink>


            <CustomsReferenceCollection>
              <CustomsReference>
                <Type>
                  <Code>SRF</Code>
                </Type>
                <Reference></Reference>
              </CustomsReference>
            </CustomsReferenceCollection>

            <EntryNumberCollection>
              <EntryNumber>
                <Type>
                  <Code>ASY</Code>
                </Type>
                <Number></Number>
                <EntryStatus>
                  <Code></Code>
                </EntryStatus>
                <IssueDate>2018-12-21T22:30:00</IssueDate>
              </EntryNumber>
            </EntryNumberCollection>
          </EntryHeader>
        </EntryHeaderCollection>

        <EntryInstructionCollection>
          <EntryInstruction>
            <Link>1</Link>
            <LocationAtClearance>
              <Code>J7</Code>
              <Description>ZACPACK Durban Depot (Pty) Ltd</Description>
            </LocationAtClearance>
            <Style>IMP</Style>

            <AddInfoCollection>
              <AddInfo>
                <Key>ABC_LocationInformation</Key>
                <Value></Value>
              </AddInfo>
              <AddInfo>
                <Key>DutyAmount</Key>
                <Value>0</Value>
              </AddInfo>
              <AddInfo>
                <Key>TaxAmount</Key>
                <Value>0</Value>
              </AddInfo>
              <AddInfo>
                <Key>PayeeIndicator</Key>
                <Value></Value>
              </AddInfo>
              <AddInfo>
                <Key>PartyStatus</Key>
                <Value></Value>
              </AddInfo>
              <AddInfo>
                <Key>PartyIndicator</Key>
                <Value></Value>
              </AddInfo>
              <AddInfo>
                <Key>CycleDate</Key>
                <Value></Value>
              </AddInfo>
              <AddInfo>
                <Key>CycleNumber</Key>
                <Value></Value>
              </AddInfo>
            </AddInfoCollection>

            <OrganizationAddressCollection>
              <OrganizationAddress>
                <AddressType>HouseBillIssuingParty</AddressType>

                <RegistrationNumberCollection>
                  <RegistrationNumber>
                    <Type>
                      <Code>BIL</Code>
                      <Description>Bill Issuer</Description>
                    </Type>
                    <CountryOfIssue>
                      <Code>ZA</Code>
                      <Name>South Africa</Name>
                    </CountryOfIssue>
                    <Value>01871019</Value>
                  </RegistrationNumber>
                </RegistrationNumberCollection>
              </OrganizationAddress>
            </OrganizationAddressCollection>
          </EntryInstruction>
        </EntryInstructionCollection>

        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>NotifyParty</AddressType>
            <Address1></Address1>
            <Address2></Address2>
            <City></City>
            <CompanyName></CompanyName>
            <Phone></Phone>
            <Postcode></Postcode>
            <State></State>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsignorDocumentaryAddress</AddressType>
            <Address1>AM WALDRAND 29</Address1>
            <Address2></Address2>
            <AddressOverride>false</AddressOverride>
            <AddressShortCode>ONEID</AddressShortCode>
            <City>NUERNBERG</City>
            <CompanyName>GEORG A. STEINMANN LEDERWARENFABRIK GMBH &amp; CO.KG</CompanyName>
            <Country>
              <Code>DE</Code>
              <Name>Germany</Name>
            </Country>
            <Email>SIMON.ATZEI@STEINMANN-LEDERWAREN.DE</Email>
            <Fax></Fax>
            <OrganizationCode>1000341764</OrganizationCode>
            <Phone>+4991227960</Phone>
            <Port>
              <Code>DENUE</Code>
              <Name>Nurnberg</Name>
            </Port>
            <Postcode>90455</Postcode>
            <ScreeningStatus>
              <Code>UNK</Code>
              <Description>Unknown</Description>
            </ScreeningStatus>
            <State>BY</State>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsigneeDocumentaryAddress</AddressType>
            <Address1>291 PAISLEY ROAD</Address1>
            <Address2>JACOBS</Address2>
            <AddressOverride>false</AddressOverride>
            <AddressShortCode>ONEID</AddressShortCode>
            <City>DURBAN</City>
            <CompanyName>FELTEX FEHRER PTY LTD (DURBAN)</CompanyName>
            <Country>
              <Code>ZA</Code>
              <Name>South Africa</Name>
            </Country>
            <Email>CECILLAP@FELTEX.CO.ZA</Email>
            <Fax>002731 4604303</Fax>
            <OrganizationCode>1000057878</OrganizationCode>
            <Phone>+27314604200</Phone>
            <Port>
              <Code>ZADUR</Code>
              <Name>Durban</Name>
            </Port>
            <Postcode>4052</Postcode>
            <ScreeningStatus>
              <Code>UNK</Code>
              <Description>Unknown</Description>
            </ScreeningStatus>
            <State></State>
          </OrganizationAddress>
        </OrganizationAddressCollection>

        <PackingLineCollection Content=""Complete"">
          <PackingLine>
            <Commodity>
              <Code></Code>
            </Commodity>
            <ContainerNumber>CAIU9003147</ContainerNumber>
            <CustomsOuterPacks>1</CustomsOuterPacks>
            <CustomsPackType>
              <Code>PLT</Code>
            </CustomsPackType>
            <GoodsDescription>PON-ATLAS-LOCKNIT</GoodsDescription>
            <LinePrice>0</LinePrice>
            <MarksAndNos>785/104</MarksAndNos>
            <PackQty>1</PackQty>
            <PackType>
              <Code>PLT</Code>
              <Description>Pallet</Description>
            </PackType>
            <Volume>5.44</Volume>
            <VolumeUnit>
              <Code>M3</Code>
              <Description>Cubic metre</Description>
            </VolumeUnit>
            <Weight>350</Weight>
            <WeightUnit>
              <Code>KG</Code>
              <Description>kilogram</Description>
            </WeightUnit>

            <AddInfoGroupCollection>
              <AddInfoGroup>
                <Type>
                  <Code>PAC</Code>
                  <Description>Pack Lines</Description>
                </Type>

                <AddInfoCollection>
                  <AddInfo>
                    <Key>ConsignmentReference</Key>
                    <Value>1</Value>
                  </AddInfo>
                  <AddInfo>
                    <Key>MatchingReference</Key>
                    <Value></Value>
                  </AddInfo>
                </AddInfoCollection>
              </AddInfoGroup>
            </AddInfoGroupCollection>

            <PackingLineCollection>
              <PackingLine>
                <GoodsDescription></GoodsDescription>
                <HarmonisedCode></HarmonisedCode>
                <PackQty>0</PackQty>
                <PackType>
                  <Code></Code>
                </PackType>

                <AddInfoGroupCollection>
                  <AddInfoGroup>
                    <Type>
                      <Code>PAC</Code>
                      <Description>Asycuda Country Specific Packing Item Details</Description>
                    </Type>

                    <AddInfoCollection>
                      <AddInfo>
                        <Key>Country</Key>
                        <Value>ZA</Value>
                      </AddInfo>
                      <AddInfo>
                        <Key>CustomsValue</Key>
                        <Value>0.0000</Value>
                      </AddInfo>
                      <AddInfo>
                        <Key>CustomsQty</Key>
                        <Value>0.00000</Value>
                      </AddInfo>
                      <AddInfo>
                        <Key>CustomsUQ</Key>
                        <Value></Value>
                      </AddInfo>
                      <AddInfo>
                        <Key>DutyValue</Key>
                        <Value>0.0000</Value>
                      </AddInfo>
                      <AddInfo>
                        <Key>TaxValue</Key>
                        <Value>0.0000</Value>
                      </AddInfo>
                      <AddInfo>
                        <Key>CountryOfDestination</Key>
                        <Value>ZA</Value>
                      </AddInfo>
                    </AddInfoCollection>
                  </AddInfoGroup>
                </AddInfoGroupCollection>
              </PackingLine>
            </PackingLineCollection>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>";

		const string YusenEvent = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Event>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">YASPRDYAU</DataProvider>
        <Key>C05915180</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>
      <Workflow>
        <Company>
          <Code>YAU</Code>
          <Country Name=""Australia"">AU</Country>
          <Name>Yusen Logistics (Australia) Pty Ltd</Name>
        </Company>
        <EventBranch Name=""Sydney Branch"">SYD</EventBranch>
        <EventDepartment Name=""Branch"">BRN</EventDepartment>
        <EventType Description=""Subscription Requested"">SBR</EventType>
        <EventUser Name=""CargoWise One Service"">~BP</EventUser>
        <TriggerCount>1</TriggerCount>
        <TriggerDate>2019-02-18T13:40:16.92</TriggerDate>
        <TriggerDescription></TriggerDescription>
        <TriggerReference>|TYP=Container Tracking</TriggerReference>
        <TriggerType>Manual</TriggerType>
      </Workflow>
    </DataContext>
    <EventTime>2019-02-18T13:40:16.92</EventTime>
    <EventType>SBR</EventType>
    <EventReference>|TYP=Container Tracking</EventReference>
    <IsEstimate>false</IsEstimate>
    <AdditionalContextCollection>
      <AdditionalContext>
        <DataContext>
          <DataSource>
            <Key>D03397056</Key>
            <Type>ForwardingContainer</Type>
          </DataSource>
        </DataContext>
        <ContextCollection>
          <Context>
            <Type>ContainerISOCode</Type>
            <Value>45G0</Value>
          </Context>
        </ContextCollection>
      </AdditionalContext>
      <AdditionalContext>
        <DataContext>
          <DataSource>
            <Key>D03397089</Key>
            <Type>ForwardingContainer</Type>
          </DataSource>
        </DataContext>
        <ContextCollection>
          <Context>
            <Type>ContainerNumber</Type>
            <Value>BEAU4558683</Value>
          </Context>
          <Context>
            <Type>ContainerISOCode</Type>
            <Value>45G0</Value>
          </Context>
        </ContextCollection>
      </AdditionalContext>
      <AdditionalContext>
        <DataContext>
          <DataSource>
            <Key>D03397096</Key>
            <Type>ForwardingContainer</Type>
          </DataSource>
        </DataContext>
        <ContextCollection>
          <Context>
            <Type>ContainerNumber</Type>
            <Value>BEAU4558683</Value>
          </Context>
          <Context>
            <Type>ContainerISOCode</Type>
            <Value>45G0</Value>
          </Context>
        </ContextCollection>
      </AdditionalContext>
      <AdditionalContext>
        <DataContext>
          <DataSource>
            <Key>D03397085</Key>
            <Type>ForwardingContainer</Type>
          </DataSource>
        </DataContext>
        <ContextCollection>
          <Context>
            <Type>ContainerNumber</Type>
            <Value>DFSU6611124</Value>
          </Context>
          <Context>
            <Type>ContainerISOCode</Type>
            <Value>45G0</Value>
          </Context>
        </ContextCollection>
      </AdditionalContext>
      <AdditionalContext>
        <DataContext>
          <DataSource>
            <Key>D03397094</Key>
            <Type>ForwardingContainer</Type>
          </DataSource>
        </DataContext>
        <ContextCollection>
          <Context>
            <Type>ContainerNumber</Type>
            <Value>DRYU9267731</Value>
          </Context>
          <Context>
            <Type>ContainerISOCode</Type>
            <Value>45G0</Value>
          </Context>
        </ContextCollection>
      </AdditionalContext>
      <AdditionalContext>
        <DataContext>
          <DataSource>
            <Key>D03397097</Key>
            <Type>ForwardingContainer</Type>
          </DataSource>
        </DataContext>
        <ContextCollection>
          <Context>
            <Type>ContainerNumber</Type>
            <Value>EGSU9044346</Value>
          </Context>
          <Context>
            <Type>ContainerISOCode</Type>
            <Value>45G0</Value>
          </Context>
        </ContextCollection>
      </AdditionalContext>
      <AdditionalContext>
        <DataContext>
          <DataSource>
            <Key>D03397091</Key>
            <Type>ForwardingContainer</Type>
          </DataSource>
        </DataContext>
        <ContextCollection>
          <Context>
            <Type>ContainerNumber</Type>
            <Value>EGSU9044346</Value>
          </Context>
          <Context>
            <Type>ContainerISOCode</Type>
            <Value>45G0</Value>
          </Context>
        </ContextCollection>
      </AdditionalContext>
      <AdditionalContext>
        <DataContext>
          <DataSource>
            <Key>D03397099</Key>
            <Type>ForwardingContainer</Type>
          </DataSource>
        </DataContext>
        <ContextCollection>
          <Context>
            <Type>ContainerNumber</Type>
            <Value>EISU9247820</Value>
          </Context>
          <Context>
            <Type>ContainerISOCode</Type>
            <Value>45G0</Value>
          </Context>
        </ContextCollection>
      </AdditionalContext>
      <AdditionalContext>
        <DataContext>
          <DataSource>
            <Key>D03397090</Key>
            <Type>ForwardingContainer</Type>
          </DataSource>
        </DataContext>
        <ContextCollection>
          <Context>
            <Type>ContainerNumber</Type>
            <Value>EITU1810735</Value>
          </Context>
          <Context>
            <Type>ContainerISOCode</Type>
            <Value>45G0</Value>
          </Context>
        </ContextCollection>
      </AdditionalContext>
      <AdditionalContext>
        <DataContext>
          <DataSource>
            <Key>D03397095</Key>
            <Type>ForwardingContainer</Type>
          </DataSource>
        </DataContext>
        <ContextCollection>
          <Context>
            <Type>ContainerNumber</Type>
            <Value>FCIU9958155</Value>
          </Context>
          <Context>
            <Type>ContainerISOCode</Type>
            <Value>45G0</Value>
          </Context>
        </ContextCollection>
      </AdditionalContext>
      <AdditionalContext>
        <DataContext>
          <DataSource>
            <Key>D03397087</Key>
            <Type>ForwardingContainer</Type>
          </DataSource>
        </DataContext>
        <ContextCollection>
          <Context>
            <Type>ContainerNumber</Type>
            <Value>HMCU9018361</Value>
          </Context>
          <Context>
            <Type>ContainerISOCode</Type>
            <Value>45G0</Value>
          </Context>
        </ContextCollection>
      </AdditionalContext>
      <AdditionalContext>
        <DataContext>
          <DataSource>
            <Key>D03397092</Key>
            <Type>ForwardingContainer</Type>
          </DataSource>
        </DataContext>
        <ContextCollection>
          <Context>
            <Type>ContainerNumber</Type>
            <Value>SEGU5990517</Value>
          </Context>
          <Context>
            <Type>ContainerISOCode</Type>
            <Value>45G0</Value>
          </Context>
        </ContextCollection>
      </AdditionalContext>
      <AdditionalContext>
        <DataContext>
          <DataSource>
            <Key>D03397093</Key>
            <Type>ForwardingContainer</Type>
          </DataSource>
        </DataContext>
        <ContextCollection>
          <Context>
            <Type>ContainerNumber</Type>
            <Value>TEMU8962048</Value>
          </Context>
          <Context>
            <Type>ContainerISOCode</Type>
            <Value>45G0</Value>
          </Context>
        </ContextCollection>
      </AdditionalContext>
      <AdditionalContext>
        <DataContext>
          <DataSource>
            <Key>D03397098</Key>
            <Type>ForwardingContainer</Type>
          </DataSource>
        </DataContext>
        <ContextCollection>
          <Context>
            <Type>ContainerNumber</Type>
            <Value>TEMU8962048</Value>
          </Context>
          <Context>
            <Type>ContainerISOCode</Type>
            <Value>45G0</Value>
          </Context>
        </ContextCollection>
      </AdditionalContext>
      <AdditionalContext>
        <DataContext>
          <DataSource>
            <Key>D03397088</Key>
            <Type>ForwardingContainer</Type>
          </DataSource>
        </DataContext>
        <ContextCollection>
          <Context>
            <Type>ContainerNumber</Type>
            <Value>TGHU6936648</Value>
          </Context>
          <Context>
            <Type>ContainerISOCode</Type>
            <Value>45G0</Value>
          </Context>
        </ContextCollection>
      </AdditionalContext>
      <AdditionalContext>
        <DataContext>
          <DataSource>
            <Key>D03397086</Key>
            <Type>ForwardingContainer</Type>
          </DataSource>
        </DataContext>
        <ContextCollection>
          <Context>
            <Type>ContainerNumber</Type>
            <Value>TLLU4136650</Value>
          </Context>
          <Context>
            <Type>ContainerISOCode</Type>
            <Value>45G0</Value>
          </Context>
        </ContextCollection>
      </AdditionalContext>
    </AdditionalContextCollection>
    <ContextCollection>
      <Context>
        <Type>MBOLNumber</Type>
        <Value>080900037219</Value>
      </Context>
      <Context>
        <Type>MBOLOriginUNLOCO</Type>
        <Value>IDJKT</Value>
      </Context>
      <Context>
        <Type>MBOLDestinationUNLOCO</Type>
        <Value>COCTG</Value>
      </Context>
      <Context>
        <Type>CarriersBookingReference</Type>
        <Value>080900037219</Value>
      </Context>
      <Context>
        <Type>CarrierCode</Type>
        <Value>EGLV</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>";

		#endregion
	}
}
