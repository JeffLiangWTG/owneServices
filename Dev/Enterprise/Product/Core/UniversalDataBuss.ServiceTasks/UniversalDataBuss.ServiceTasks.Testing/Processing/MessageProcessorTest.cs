using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.GraphEngine.ServiceTasks;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.DataTransfer.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.UniversalDataBuss.ServiceTasks.Testing
{
	sealed class MessageProcessorTest : TestCaseWithFactory
	{
		public void TestHandleNull()
		{
			var (group, staff) = CreateGroupWithUser("rejected@test.com");
			eServicesRegistry.Instance.ImportRejectedNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			var postMastersGroup = Factory.Load<GlbGroup>(Enterprise.Core.Constants.Groups.PostMastersGroupPK);
			staff.Groups.Add(postMastersGroup);

			var message = GetNewQueuedMessage(EDIMessageSubTypeList.Codes.XmlUniversalEvent, @"
<UniversalEvent>	
	<Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>DummyBusinessObject</Type>
          <Key>TOAST!!!</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
		<EventTime>2017-07-01T20:17:41</EventTime>
		<EventType>CES</EventType>
		<ContextCollection>
			<Context>
				<Type>EntryNumber</Type>
				<Value>More TOAST!!!</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>");
			message.EM_MessageNum = "00001";

			var dummyBO = Factory.New<DummyWithWorkflow>();
			dummyBO.Z0_Description = "TOAST!!!";

			Factory.Save();

			DummyWithWorkflowDataContextManager.LoadDummy.Value = (f, q) => f.Load<DummyWithWorkflow>(q).Concat(new DummyWithWorkflow[] { null }).ToArray();
			var logger = new TestServiceLogger();
			var serviceTask = new UMIServiceTask() { ServiceLogger = logger };
			serviceTask.RunTask();

			Factory.Save();
			AssertContains("Starting processing Message #00001", logger.ToString());
			AssertContains("Linked Event to Dummy Business Object TOAST!!!.", logger.ToString());
			AssertContains("Finished processing Message #00001", logger.ToString());

			AssertContains("returned an enumerable containing null", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestNotificationsSentToCorrectGroupForRetries()
		{
			var (group, staff) = CreateGroupWithUser("rejected@test.com");

			eServicesRegistry.Instance.ImportRejectedNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			var postMastersGroup = Factory.Load<GlbGroup>(Enterprise.Core.Constants.Groups.PostMastersGroupPK);
			staff.Groups.Add(postMastersGroup);

			var message = GetNewQueuedMessage(EDIMessageSubTypeList.Codes.XmlUniversalEvent, @"
<UniversalEvent>	
	<Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>DummyBusinessObject</Type>
          <Key>TOAST!!!</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
		<EventTime>2017-07-01T20:17:41</EventTime>
		<EventType>CES</EventType>
		<ContextCollection>
			<Context>
				<Type>EntryNumber</Type>
				<Value>More TOAST!!!</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>");
			message.EM_MessageNum = "00001";

			var dummyBO = Factory.New<DummyWithWorkflow>();
			dummyBO.Z0_Description = "TOAST!!!";

			Factory.Save();

			var logger = new MockRepository(MockBehavior.Strict).Create<ILogger>();
			var logString = string.Empty;
			var count = 0;
			logger.Setup(l => l.Log(It.IsAny<LogType>(), It.Is<string>(s => s.Contains("No Module found a Business Entity to link this Universal Event to.")))).Callback<LogType, string>((log, s) =>
			{
				if (count++ < 3)
				{
					throw new MessageProcessingBusinessFailureException("This message failed because I wanted it to fail.", "Reasons", true);
				}
			});
			logger.Setup(l => l.Log(It.IsAny<LogType>(), It.Is<string>(s => !s.Contains("No Module found a Business Entity to link this Universal Event to.")))).Callback<LogType, string>((log, s) => logString += $"{log.ToString()}|{s}\r\n");

			int CountEmailsInDB() => Db.Connection.ExecuteScalar<int>($"SELECT COUNT(*) FROM {MailDBItemsSchema.Constants.SqlSchemaName}.{MailDBItemsSchema.Constants.TableName}");
			var initialEmailCount = CountEmailsInDB();

			var serviceTask = new UMIServiceTask() { ServiceLogger = logger.Object };
			MasterFiles.DataTransfer.Testing.DummyWithWorkflowDataContextManager.SetThrowExceptionWithDataContextKeyMatchingCount(3);
			serviceTask.RunTask();
			serviceTask.RunTask();
			serviceTask.RunTask();
			serviceTask.RunTask();

			Factory.Save();

			AssertContains("logger", "Information|Starting processing Message #00001", logString);
			AssertContains("logger", "Warning|Exception processing message 00001: [This message failed because I wanted it to fail.]", logString);
			AssertContains("logger", "Information|Finished processing Message #00001", logString);
			AssertContains("logger", "Information|Starting processing Message #00001", logString);
			AssertContains("logger", "Warning|Exception processing message 00001: [This message failed because I wanted it to fail.]", logString);
			AssertContains("logger", "Information|Finished processing Message #00001", logString);

			var registrationKey = ObjectFactory.Get<Enterprise.Integration.Licensing.IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "WTL";
			registrationKey.ServerCodeForTest = "NMO";

			var factory = new BusinessObjectFactory();

			var ediMesssage = factory.Load<EDIMessage>(message.PK);
			AssertEquals("00001", ediMesssage.EM_MessageNum);
			AssertEquals("EDI Message should be rejected.", EDIMessage.Status.Rejected, ediMesssage.EM_Status);

			var email = Env.OutgoingMailManager.EmailsCreated;
			AssertEquals(1, email.Count);
			AssertEquals("Email should be saved", 1, CountEmailsInDB() - initialEmailCount);
			AssertNotContains("Exception occurred 3 times whilst processing a message individually. The message's status has been set to 'Rejected'", email.First().Body);
			AssertEquals("email is sent to group set in registry", "rejected@test.com", email.First().Recipients[0].Email);
			AssertNotContains("at Enterprise.MasterFiles.DataTransfer.Testing.DummyWithWorkflowDataContextManager.GetDataContextKeyMatchingQuery", email.First().Body);

			ErrorReporter.Clear();
		}

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new UniversalMessageProcessor(new LoggingInformation(), null, new FactoryService()));
			AssertExceptionThrown<ArgumentNullException>(() => new UniversalMessageProcessor(new LoggingInformation(), new[] { "XXX" }, null));
			AssertNoExceptionThrown(() => new UniversalMessageProcessor(new LoggingInformation(), new[] { "XXX" }.AsEnumerable(), new FactoryService()));
		}

		public void TestUniversalDataMessagingOnlyRetriesOnce()
		{
			var message = GetNewQueuedMessage(EDIMessageSubTypeList.Codes.XmlUniversalEvent, @"
<UniversalEvent>
  <Event>
    <EventType>OCR</EventType>
    <EventTime>10-JUL-2010 18:00</EventTime>
    <EventReference>Dummy Description</EventReference>
    <DataProvider>Dummy</DataProvider>
    <ContextCollection>
      <Context>
        <Type>MAWBNumber</Type>
        <Value>1211234567</Value>
      </Context>
      <Context>
        <Type>HAWBNumber</Type>
        <Value>AAAA1003248</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>");
			message.EM_MessageNum = "00001";

			Factory.Save();

			var processor = new ProcessingManagerThatThrowsExceptionOnSave(new string[] { message.EM_MessageSubType }, new Exception("Exception Thrown during ProcessMessage(EDIMessage message)"));
			processor.ExecuteBatch(CancellationToken.None);
			processor.Logger.Log(".");

			var reloadedMessage = new BusinessObjectFactory().Load<EDIMessage>(message.PK);
			AssertEquals("reloadedMessage.EM_Status", EDIMessage.Status.Rejected, reloadedMessage.EM_Status);

			AssertEquals("ErrorReporter.LastMessageReported", "Error Processing Incoming EDI Message: UDM-XDC-00001", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			var log = string.Join("\r\n", processor.Logger.UserLogStrings.Cast<string>());
			AssertContains("Exception processing message #00001 individually: [Exception Thrown during ProcessMessage(EDIMessage message)]", log);
			AssertContains("Exception occurred 1 times whilst processing a message individually. The message's status has been set to 'Rejected'.", log);
		}

		public void TestRetryCountOnLockTimeOutException()
		{
			var exception = SqlExceptionBuilder.CreateSqlException(
				SqlExceptionBuilder.CreateSqlErrorCollection(
					SqlExceptionBuilder.CreateSqlError(1222, byte.MaxValue, byte.MinValue, "", "Lock request time out period exceeded.", string.Empty, 0)
				));
			AssertExceptionRetryCount(exception);
		}

		public void TestRetryCountOnConcurrencyException()
		{
			var exception = new ZSaveConcurrencyException(new ZDataConcurrencyException(null, null, null), Factory);
			AssertRetryOnException(exception);
		}

		public void TestShouldRetryOnceOnException()
		{
			var exception = new ZException("Message");
			AssertRetryOnException(exception, retryAttempts: 1);
		}

		void AssertExceptionRetryCount(Exception exception)
		{
			var message = GetNewQueuedMessage(EDIMessageSubTypeList.Codes.XmlUniversalEvent, @"
<UniversalEvent>	
	<Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>DummyBusinessObject</Type>
          <Key>Officer K</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
		<EventTime>2049-07-01T20:17:41</EventTime>
		<EventType>CES</EventType>
		<ContextCollection>
			<Context>
				<Type>EntryNumber</Type>
				<Value>Interlinked</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>");
			message.EM_MessageNum = "00001";

			Factory.Save();

			var processor = new ProcessingManagerThatThrowsExceptionOnSave(new string[] { message.EM_MessageSubType }, exception);
			processor.ExecuteBatch(CancellationToken.None);

			AssertEquals(eAdaptorRegistry.Instance.RetryAttemptsOnUniversalXMLProcessingRecoverableErrors.Value, processor.exceptionThrownCount);
			ErrorReporter.Clear();
		}

		public void TestCodesMappedToTargetUsesMessageBranch()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var message = GetNewQueuedMessage(EDIMessageSubTypeList.Codes.XmlUniversalShipment, @"<UniversalShipment>
   <Shipment>
      <DataContext>
         <CodesMappedToTarget>true</CodesMappedToTarget>
         <DataTargetCollection>
            <DataTarget>
               <Type>ForwardingShipment</Type>
               <Key>SDFW0055942</Key>
            </DataTarget>
         </DataTargetCollection>
      </DataContext>
      <JobCosting>
         <ChargeLineCollection>
            <ChargeLine>
               <ChargeCode>
                  <Code>BAF</Code>
               </ChargeCode>
               <ImportMetaData>
                  <Instruction>UpdateAndInsertIfNotFound</Instruction>
                  <MatchingCriteriaCollection>
                     <MatchingCriteria>
                        <FieldName>ChargeCode</FieldName>
                        <Value>BAF</Value>
                     </MatchingCriteria>
                     <MatchingCriteria>
                        <FieldName>SellOSCurrency</FieldName>
                        <Value>JPY</Value>
                     </MatchingCriteria>
                  </MatchingCriteriaCollection>
               </ImportMetaData>
               <SellOSAmount>00000001418</SellOSAmount>
               <SellOSCurrency>
                  <Code>JPY</Code>
               </SellOSCurrency>
            </ChargeLine>
         </ChargeLineCollection>
      </JobCosting>
   </Shipment>
</UniversalShipment>");
				message.EM_MessageNum = "00001";

				var shipment = (IForwardingShipment)Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
				shipment.JS_UniqueConsignRef = "SDFW0055942";
				var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				job.JH_ParentID = shipment.PK;
				job.JH_GE = GetFirstValidDepartmentForChargeCode("BAF");

				Factory.Save();
			}

			var logger = new TestServiceLogger();
			var serviceTask = new UMIServiceTask { ServiceLogger = logger };
			serviceTask.RunTask();

			CombineAssertions(() =>
			{
				AssertContains("Information|Starting processing Message #00001", logger.ToString());
				AssertContains("Information|Updated Shipment SDFW0055942 from UniversalShipment.", logger.ToString());
				AssertContains("Information|Successfully saved Shipment SDFW0055942.", logger.ToString());
				AssertContains("Information|Finished processing Message #00001", logger.ToString());
			});

			CombineAssertions("When the message does not specify a company the company/branch should be taken using EM_GB from the message", () =>
			{
				var chargeCode = Factory.LoadTop1<JobCharge>(new ZQuery());
				AssertNotNull(chargeCode);
				AssertEquals(branch.PK, chargeCode.Branch.PK);
				AssertEquals(company.PK, chargeCode.CalculatedCompany.PK);
			});
		}

		public void TestInactiveBranchDefaultsToCompanyFirstActiveBranch()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "ABC";
			branch.GB_GC = company.PK;
			branch.GB_IsActive = false;

			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "DEF";
			branch2.GB_GC = company.PK;

			var branch3 = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "GHI";
			branch3.GB_GC = company.PK;

			Factory.Save();

			var logger = new TestServiceLogger();

			using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUser, branch.PK.ToGuid(), Env.CurrentDepartmentPK)))
			{
				var message = GetNewQueuedMessage(EDIMessageSubTypeList.Codes.XmlUniversalEvent, @"
<UniversalEvent>	
	<Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>DummyBusinessObject</Type>
          <Key>TOAST!!!</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
		<EventTime>2017-07-01T20:17:41</EventTime>
		<EventType>CES</EventType>
		<ContextCollection>
			<Context>
				<Type>EntryNumber</Type>
				<Value>More TOAST!!!</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>");
				message.EM_MessageNum = "00001";

				var dummyBO = Factory.New<DummyWithWorkflow>();
				dummyBO.Z0_Description = "TOAST!!!";

				Factory.Save();

				var serviceTask = new UMIServiceTask { ServiceLogger = logger };
				serviceTask.RunTask();

				var reloadedMessage = new BusinessObjectFactory().Load<EDIMessage>(message.PK);
				AssertEquals(EDIMessage.Status.ProcessedOK, reloadedMessage.EM_Status);
			}

			CombineAssertions(() =>
			{
				AssertContains("Information|Starting processing Message #00001", logger.ToString());
				AssertContains("Information|Finished processing Message #00001", logger.ToString());
			});
		}

		public void TestMessageWithAttachmentAndParentsFromMultipleContexts()
		{
			var shipment = Factory.New(ObjectFactory.GetType<IForwardingShipment>());
			shipment[JobShipmentSchema.JS_UniqueConsignRef] = "S00001286";
			shipment[JobShipmentSchema.JS_TransportMode] = "SEA";
			shipment[JobShipmentSchema.JS_HouseBill] = "HOUSE";

			var order = Factory.New(ObjectFactory.GetType<IOrder>());
			order[JobOrderHeaderSchema.JD_OrderNumber] = "AA-100";
			order[JobOrderHeaderSchema.JD_OA_BuyerAddress.Name] = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			order[JobOrderHeaderSchema.JD_JS] = shipment.PK;

			var message = EDIMessageTestFactory.New(Factory);

			message.EM_ApplicationCode = XmlEDIMessage.ApplicationCodes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message.EM_Status = XmlEDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText =
			#region Message Text
			$@"<UniversalEvent>
  <Event>
    <DataContext>
		<Company>
        <Code>EDI</Code>
        <Name>Eagle Datamation International</Name>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingShipment</Type>
          <Key>S00053321</Key>
        </DataSource>
      </DataSourceCollection>
      <DataProvider>{Enterprise.Core.Constants.ProductName}</DataProvider>
      <EventType>
        <Code>DDI</Code>
        <Description>Document Imported</Description>
      </EventType>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2020-05-12T11:49:50.243</TriggerDate>
      <TriggerDescription>Send CIV to Receiving Agent</TriggerDescription>
      <TriggerReference>*CIV*</TriggerReference>
      <TriggerType>Trigger</TriggerType>
    </DataContext>
    <EventTime>2020-05-12T11:49:50.243</EventTime>
    <EventType>DDI</EventType>
    <CreatedTime>2020-05-12T04:25:56.32</CreatedTime>
    <EventReference>CIV</EventReference>
    <IsEstimate>false</IsEstimate>
    <AttachedDocumentCollection>
      <AttachedDocument>
        <FileName>hello.txt</FileName>
        <ImageData>SGVsbG8sIFdvcmxkIQ==</ImageData>
		<Type>
          <Code>CIV</Code>
          <Description>Commercial Invoice</Description>
        </Type>
        <IsPublished>true</IsPublished>
        <SaveDateUTC>2020-05-12T03:49:00</SaveDateUTC>
        <SavedBy>
          <Code>SH0</Code>
          <Name>Test</Name>
        </SavedBy>
        <Source>
          <Code></Code>
          <Description></Description>
        </Source>
        <VisibleBranchCode></VisibleBranchCode>
        <VisibleCompanyCode></VisibleCompanyCode>
        <VisibleDepartmentCode></VisibleDepartmentCode>
      </AttachedDocument>
    </AttachedDocumentCollection>
    <ContextCollection>
	  <Context>
        <Type>HBOLNumber</Type>
        <Value>HOUSE</Value>
      </Context>
	  <Context>
        <Type>OrderNumber</Type>
        <Value>AA-100</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>";
			#endregion // Message Text

			Factory.Save();

			var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			AssertEquals("eDoc is attached to shipment", 1, ((IDocManagerSupport)shipment).DocManagerInfo.Files.Count);
			AssertEquals("eDoc is attached to order", 1, ((IDocManagerSupport)order).DocManagerInfo.Files.Count);

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, "DDI");
			var ddiLogs = shipment.GetLogs().Find(query);
			AssertEquals("Shipment should have DDI event", 1, ddiLogs.Length);

			var newFactory = new BusinessObjectFactory();

			message = newFactory.Load<EDIMessage>(message.PK);
			AssertEquals(EDIMessage.Status.ProcessedOK, message.EM_Status);
		}

		public void TestUniversalDataMessagingExitsAndStatusQueuedOnNonTimeoutOrConcurrentSQLErrors()
		{
			var sqlException = SqlExceptionBuilder.CreateSqlException(
					SqlExceptionBuilder.CreateSqlErrorCollection(
						SqlExceptionBuilder.CreateSqlError(983, 1, 1, "", "Unable to access database because its replica role is RESOLVING.", "", 1)));

			AssertNoRetryLeaveStatusQueuedOnException(sqlException);

			AssertNoRetryLeaveStatusQueuedOnException(SqlExceptionBuilder.CreateSqlException(
						SqlExceptionBuilder.CreateSqlErrorCollection(
							SqlExceptionBuilder.CreateSqlError(1105, 1, 1, "",
								"Could Not Allocate Space for Object Name in Database 'DB' Because the 'PRIMARY' Filegroup is Full.", "", 1)))
			);

			AssertNoRetryLeaveStatusQueuedOnException(SqlExceptionBuilder.CreateSqlException(
						SqlExceptionBuilder.CreateSqlErrorCollection(
							SqlExceptionBuilder.CreateSqlError(3958, 1, 1, "",
								"Transaction aborted when accessing versioned row in table 'dbo.Table' in database 'DB'.", "", 1)))
			);
		}

		public void TestUniversalDataMessagingRetriesOnSQLTimeoutError()
		{
			var sqlException = SqlExceptionBuilder.CreateSqlException(
					SqlExceptionBuilder.CreateSqlErrorCollection(
						SqlExceptionBuilder.CreateSqlError(-2, 0, 11, Db.ServerName, "Timeout expired.  The timeout period elapsed prior to completion of the operation or the server is not responding.", "", 0)));

			var exception = new ZSaveException(new ZDataException(sqlException, null, Db.Connection), Factory);

			AssertEquals(true, exception.IsExceptionPresentIncludingInner<System.Data.Common.DbException>(s => s.IsTimeoutExpired(), matchExactType: false));

			AssertRetryOnException(exception);
		}

		public void TestUniversalDataMessagingRetriesOnSQLLockTimeoutError()
		{
			var sqlException = SqlExceptionBuilder.CreateSqlException(
					SqlExceptionBuilder.CreateSqlErrorCollection(
						SqlExceptionBuilder.CreateSqlError(1222, 0, 11, Db.ServerName, "Lock request time out period exceeded.", "", 0)));

			var exception = new ZSaveException(new ZDataException(sqlException, null, Db.Connection), Factory);

			AssertEquals(true, exception.IsExceptionPresentIncludingInner<System.Data.Common.DbException>(s => s.IsLockTimeoutExpired(), matchExactType: false));

			AssertRetryOnException(exception, null, null, 0);
		}

		public void TestUniversalDataMessagingExitsAndStatusQueuedOnGeneralNetworkError()
		{
			var sqlException = SqlExceptionBuilder.CreateSqlException(64, 0, 11, Db.ServerName, "A connection was successfully established with the server, but then an error occurred during the pre-login handshake.", "", 0);

			AssertNoRetryLeaveStatusQueuedOnException(sqlException);
		}

		public void TestUniversalDataMessagingExitsAndStatusQueuedOnSevereError()
		{
			var sqlException = SqlExceptionBuilder.CreateSqlException(596, 0, 11, Db.ServerName, "Cannot continue the execution because the session is in the kill state.\r\nA severe error occurred on the current command.  The results, if any, should be discarded.", "", 0);

			AssertNoRetryLeaveStatusQueuedOnException(sqlException);
		}

		public void TestUniversalProcessingManagerDoesNotThrowNullReferenceExceptionOnDispose()
		{
			AssertNoExceptionThrown(new UniversalProcessingManager(new string[] { EDIMessageSubTypeList.Codes.XmlUniversalEvent }, Enumerable.Empty<string>()).Dispose);
		}

		public void TestUniversalProcessingManagerErrorReportIfIllegalSaveToMessageFactory()
		{
			var shipment = Factory.New(ObjectFactory.GetType<IForwardingShipment>());
			shipment[JobShipmentSchema.JS_UniqueConsignRef] = "S00001323";

			var message = GetNewQueuedMessage(EDIMessageSubTypeList.Codes.XmlUniversalEvent, @"<UniversalEvent>
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>S00001323</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <EventTime>2020-05-05T15:15:15.000</EventTime>
    <EventType>Z00</EventType>
    <CreatedTime>2020-05-05T15:15:15.000</CreatedTime>
    <IsEstimate>false</IsEstimate>
  </Event>
</UniversalEvent>");
			Factory.Save();

			var processor = new UniversalProcessingManager(new string[] { EDIMessageSubTypeList.Codes.XmlUniversalEvent }, Enumerable.Empty<string>(), GrEngineServiceSetting.Disabled);
			var firstSave = true;
			BusinessObjectFactory.SetOnFactorySaveHookForTest((thefactory) =>
			{
				if (thefactory.NameForDebugging == "Universal Message Logging")
				{
					if (!firstSave)
					{
						var ship = thefactory.Load(ObjectFactory.GetType<IForwardingShipment>(), shipment.PK) as IForwardingShipment;
						ship.JS_UniqueConsignRef = "SHIPMENT";
					}
					else
					{
						firstSave = false;
					}
				}
			});
			var topLevelProcessor = message.GetUniversalDataMessageFactory().TopLevelDataObjectProcessor;
			processor.ExecuteBatch(CancellationToken.None);
			AssertEquals(
@$"Found BusinessObject from invalid table to be saved in Universal Message Logging factory
Tables: [JobShipment]
Top Level Processor: {topLevelProcessor.GetType()}", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		delegate void AdditionalLogAssertions(string log);

		void AssertRetryOnException(Exception exception, int? retryAttempts = null, AdditionalLogAssertions assertions = null, int errorMessages = 1)
		{
			var message = GetNewQueuedMessage(EDIMessageSubTypeList.Codes.XmlUniversalEvent, @"
<UniversalEvent>
  <Event>
    <EventType>OCR</EventType>
    <EventTime>10-JUL-2010 18:00</EventTime>
    <EventReference>Dummy Description</EventReference>
    <DataProvider>Dummy</DataProvider>
    <ContextCollection>
      <Context>
        <Type>MAWBNumber</Type>
        <Value>1211234567</Value>
      </Context>
      <Context>
        <Type>HAWBNumber</Type>
        <Value>AAAA1003248</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>");
			message.EM_MessageNum = "00001";

			Factory.Save();

			var processor = new ProcessingManagerThatThrowsExceptionOnSave(new string[] { message.EM_MessageSubType }, exception);
			processor.ExecuteBatch(CancellationToken.None);
			processor.Logger.Log(".");

			var reloadedMessage = new BusinessObjectFactory().Load<EDIMessage>(message.PK);

			retryAttempts = retryAttempts ?? eAdaptorRegistry.Instance.RetryAttemptsOnUniversalXMLProcessingRecoverableErrors.Value;

			var log = string.Join("\r\n", processor.Logger.UserLogStrings.Cast<string>());
			AssertContains("Exception processing message #00001 individually", log);
			AssertEquals("Data Notes Added", retryAttempts, reloadedMessage.DataImportLogNoteCount);
			AssertEquals("EM_RetryCount", (byte)retryAttempts, reloadedMessage.EM_RetryCount);
			AssertEquals("Retry didn't work so message is rejected", EDIMessage.Status.Rejected, reloadedMessage.EM_Status);
			assertions?.Invoke(log);

			AssertEquals(errorMessages, ExceptionReporterTestListener.Instance.Count);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestUniversalDataMessagingRetriesOnSQLDeadlockError()
		{
			var sqlException = SqlExceptionBuilder.CreateSqlException(
				SqlExceptionBuilder.CreateSqlErrorCollection(
					SqlExceptionBuilder.CreateSqlError(1205, 1, 1, "", "A deadlock error", "", 1)));

			AssertRetryOnException(sqlException);
		}

		public void TestUniversalDataMessagingRetriesOnCannotSaveExceptionMarkedAsRetriable()
		{
			var exception = new ZCannotSaveException("Save failed, but retry should work.", "Save error", shouldReprocess: true);
			AssertRetryOnException(exception, assertions: (log) =>
			{
				AssertContains("Save failed, but retry should work.", log);
				AssertNotContains("Should not log call stack", nameof(TestUniversalDataMessagingRetriesOnCannotSaveExceptionMarkedAsRetriable), log);
			});
		}

		public void TestUniversalDataMessagingRetriesOnTransactionException()
		{
			var exception = new TransactionException("Save failed, but retry should work.", OdysseyDataErrorType.TransactionRolledBack);
			AssertRetryOnException(exception, assertions: (log) =>
			{
				AssertContains("Save failed, but retry should work.", log);
				AssertNotContains("Should not log call stack", nameof(TestUniversalDataMessagingRetriesOnCannotSaveExceptionMarkedAsRetriable), log);
			});
		}

		public void TestUniversalDataMessagingRetriesOnTransactionExceptionInsideSaveException()
		{
			var row = (Factory.New<DummyBusinessObject>() as INeedRow).Row;
			var exception = new ZSaveException(new ZDataException(new TransactionException("Save failed, but retry should work.", OdysseyDataErrorType.TransactionRolledBack), row, Db.Connection), Factory);
			AssertRetryOnException(exception, assertions: (log) =>
			{
				AssertContains("Save failed, but retry should work.", log);
				AssertNotContains("Should not log call stack", nameof(TestUniversalDataMessagingRetriesOnCannotSaveExceptionMarkedAsRetriable), log);
			});
		}

		public void TestUniversalDataMessagingRejectOnCannotSaveExceptionNotMarkedAsRetriable()
		{
			var message = GetNewQueuedMessage(EDIMessageSubTypeList.Codes.XmlUniversalEvent, @"
<UniversalEvent>
  <Event>
    <EventType>OCR</EventType>
    <EventTime>10-JUL-2010 18:00</EventTime>
    <EventReference>Dummy Description</EventReference>
    <DataProvider>Dummy</DataProvider>
    <ContextCollection>
      <Context>
        <Type>MAWBNumber</Type>
        <Value>1211234567</Value>
      </Context>
      <Context>
        <Type>HAWBNumber</Type>
        <Value>AAAA1003248</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>");
			message.EM_MessageNum = "00001";

			Factory.Save();

			var exception = new ZCannotSaveException("Save failed. Retry won't work.", "Save error", shouldReprocess: false);

			var processor = new ProcessingManagerThatThrowsExceptionOnSave(new string[] { message.EM_MessageSubType }, exception);
			processor.ExecuteBatch(CancellationToken.None);
			processor.Logger.Log(".");

			var reloadedMessage = new BusinessObjectFactory().Load<EDIMessage>(message.PK);
			AssertEquals("reloadedMessage.EM_Status", EDIMessage.Status.Rejected, reloadedMessage.EM_Status);

			var log = string.Join("\r\n", processor.Logger.UserLogStrings.Cast<string>());
			AssertContains("Exception processing message #00001 individually", log);
			AssertContains("Save failed. Retry won't work.", log);
			AssertNotContains("Should not log call stack", nameof(TestUniversalDataMessagingRejectOnCannotSaveExceptionNotMarkedAsRetriable), log);
		}

		public void TestUniversalDataMessagingRetriesMoreThanOnceOnConcurrencyError()
		{
			var exception = new ZSaveConcurrencyException(new ZDataConcurrencyException(new InvalidOperationException("Exception Thrown during ProcessMessage(EDIMessage message)"), null, null), Factory);
			AssertRetryOnException(exception);
		}

		public void TestExceptionNotReportedToErrorRerporterOnMessageProcessingBusinessFailureException()
		{
			ErrorReporter.Clear();
			AssertEquals("Precondition: ErrorReporter error count is 0.", 0, ErrorReporter.TotalErrorCount);

			var message = GetNewQueuedMessage(EDIMessageSubTypeList.Codes.XmlUniversalEvent, @"
<UniversalEvent>
  <Event>
    <EventType>OCR</EventType>
    <EventTime>10-JUL-2010 18:00</EventTime>
    <EventReference>Dummy Description</EventReference>
    <DataProvider>Dummy</DataProvider>
    <ContextCollection>
      <Context>
        <Type>MAWBNumber</Type>
        <Value>1211234567</Value>
      </Context>
      <Context>
        <Type>HAWBNumber</Type>
        <Value>AAAA1003248</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>");
			message.EM_MessageNum = "00001";
			Factory.Save();

			var exception = new MessageProcessingBusinessFailureException("Business failure exception.", "Save error", false);
			var processor = new ProcessingManagerThatThrowsExceptionOnSave(new string[] { message.EM_MessageSubType }, exception);
			processor.ExecuteBatch(CancellationToken.None);

			var messageAfterExecute = new BusinessObjectFactory().Load<EDIMessage>(message.PK);
			AssertEquals("Message status should be rejected.", EDIMessage.Status.Rejected, messageAfterExecute.EM_Status);

			var log = string.Join("\r\n", processor.Logger.UserLogStrings.Cast<string>());
			AssertContains("Exception processing message #00001 individually", log);
			AssertContains("Business failure exception.", log);
			AssertEquals("MessageProcessingBusinessFailureException is not reported to ErrorReporter.", 0, ErrorReporter.TotalErrorCount);
		}

		public void TestExceptionWithSpecialCharInMessage()
		{
			var message = GetNewQueuedMessage(EDIMessageSubTypeList.Codes.XmlUniversalEvent, @"
<UniversalEvent>
  <Event>
    <EventType>OCR</EventType>
    <EventTime>10-JUL-2010 18:00</EventTime>
    <EventReference>Dummy Description</EventReference>
    <DataProvider>Dummy</DataProvider>
    <ContextCollection>
      <Context>
        <Type>MAWBNumber</Type>
        <Value>1211234567</Value>
      </Context>
      <Context>
        <Type>HAWBNumber</Type>
        <Value>AAAA1003248</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>");
			message.EM_MessageNum = "00001";
			var exception = new ZCannotSaveException("Save failed.\uD82BX\uDC00", "Save error", shouldReprocess: true);
			var processor = new ProcessingManagerThatThrowsExceptionOnSave(new string[] { message.EM_MessageSubType }, exception);
			var retryAttempts = eAdaptorRegistry.Instance.RetryAttemptsOnUniversalXMLProcessingRecoverableErrors.Value;

			using (new DisposableAction(ExceptionReporterTestListener.Instance.Clear))
			{
				Factory.Save();

				AssertNoExceptionThrown(() => processor.ExecuteBatch(CancellationToken.None));

				var reloadedMessage = new BusinessObjectFactory().Load<EDIMessage>(message.PK);

				var log = string.Join("\r\n", processor.Logger.UserLogStrings.Cast<string>());
				AssertContains("Exception processing message #00001 individually", log);
				AssertContains("Save failed.\uD82BX\uDC00", log);
				AssertNotContains("Should not log call stack", nameof(TestUniversalDataMessagingRetriesOnCannotSaveExceptionMarkedAsRetriable), log);
				AssertEquals("Data notes added", retryAttempts, reloadedMessage.DataImportLogNoteCount);
				var logNoteCount = reloadedMessage.Notes.GetAllNotes().Count(bizo => bizo is StmNote stmNote && stmNote.ST_NoteDataAsText.Contains("Save failed.�X�"));
				AssertEquals("Data notes has correct log added", retryAttempts, logNoteCount);
				AssertEquals("EM_RetryCount", (byte)retryAttempts, reloadedMessage.EM_RetryCount);
				AssertEquals("Retry didn't work so message is rejected", EDIMessage.Status.Rejected, reloadedMessage.EM_Status);

				AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
			}
		}

		public void TestDuplicateExceptionToDuplicateMessageException()
		{
			var (group, staff) = CreateGroupWithUser("rejected@test.com");

			eServicesRegistry.Instance.ImportDiscardedNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			var postMastersGroup = Factory.Load<GlbGroup>(Enterprise.Core.Constants.Groups.PostMastersGroupPK);
			staff.Groups.Add(postMastersGroup);

			var message = GetNewQueuedMessage(EDIMessageSubTypeList.Codes.XmlUniversalEvent, @"
<UniversalEvent>	
	<Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>DummyBusinessObject</Type>
          <Key>TOAST!!!</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
		<EventTime>2017-07-01T20:17:41</EventTime>
		<EventType>CES</EventType>
		<ContextCollection>
			<Context>
				<Type>EntryNumber</Type>
				<Value>More TOAST!!!</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>");
			message.EM_MessageNum = "00001";

			var dummyBO = Factory.New<DummyWithWorkflow>();
			dummyBO.Z0_Description = "TOAST!!!";

			Factory.Save();

			var logger = new MockRepository(MockBehavior.Strict).Create<ILogger>();
			var logString = string.Empty;
			var count = 0;
			logger.Setup(l => l.Log(It.IsAny<LogType>(), It.Is<string>(s => s.Contains("No Module found a Business Entity to link this Universal Event to.")))).Callback<LogType, string>((log, s) =>
			{
				if (count++ < 3)
				{
					throw new DuplicateMessageException("1234");
				}
			});
			logger.Setup(l => l.Log(It.IsAny<LogType>(), It.Is<string>(s => !s.Contains("No Module found a Business Entity to link this Universal Event to.")))).Callback<LogType, string>((log, s) => logString += $"{log.ToString()}|{s}\r\n");

			var serviceTask = new UMIServiceTask() { ServiceLogger = logger.Object };
			MasterFiles.DataTransfer.Testing.DummyWithWorkflowDataContextManager.SetThrowExceptionWithDataContextKeyMatchingCount(3);
			serviceTask.RunTask();
			serviceTask.RunTask();
			serviceTask.RunTask();
			serviceTask.RunTask();

			Factory.Save();
			var messageAfterExecute = new BusinessObjectFactory().Load<EDIMessage>(message.PK);
			AssertEquals("Message status should be discarded.", EDIMessage.Status.Discarded, messageAfterExecute.EM_Status);

			AssertEquals("No email expected for discarded XUE", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertContains("Information|Starting processing Message #00001\r\n".Trim(), logString);
			AssertContains("Information|Universal Event with ApplicationReference 1234 ignored, as it is a duplicate\r\n".Trim(), logString);
			AssertContains("Information|Finished processing Message #00001\r\n".Trim(), logString);
		}

		public void TestDisposableServiceIsDisposedAfterSavingWhenUnhandledException()
		{
			var message = GetNewQueuedMessage(EDIMessageSubTypeList.Codes.XmlUniversalEvent, @"
<UniversalEvent>	
	<Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>DummyBusinessObject</Type>
          <Key>TOAST!!!</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
		<EventTime>2017-07-01T20:17:41</EventTime>
		<EventType>CES</EventType>
		<ContextCollection>
			<Context>
				<Type>EntryNumber</Type>
				<Value>More TOAST!!!</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>");
			message.EM_MessageNum = "00001";

			var dummyBO = Factory.New<DummyWithWorkflow>();
			dummyBO.Z0_Description = "TOAST!!!";

			Factory.Save();

			var isDisposed = false;
			var isHooked = false;
			var disposedTooEarly = false;
			BusinessObjectFactory.SetOnFactorySaveHookForTest(f =>
			{
				if (isHooked)
				{
					disposedTooEarly = isDisposed;
				}
				else if (f.TryGetDisposableManager(out DisposableManager manager))
				{
					isHooked = true;
					manager.Subscribe(new DisposableAction(() => isDisposed = true));
					throw new Exception("Unhandled Exception");
				}
			});

			var logger = new TestServiceLogger();
			var serviceTask = new UMIServiceTask() { ServiceLogger = logger };
			serviceTask.RunTask();

			AssertEquals("Dispose should not happpen until message result is saved", false, disposedTooEarly);
			AssertEquals("Disposable manager should be disposed", true, isDisposed);
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertEquals("Unhandled Exception", ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();
		}

		public void TestPrcoessDuplicateMessageWhenGrEngineServiceSettingIsDisabled()
		{
			var messageBody = @"
<UniversalEvent>
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>CAeManifestStatusNotice</Type>
          <Key>0123456789ABCDEFG</Key>
        </DataTarget>
      </DataTargetCollection>
      <RecipientRoleCollection>
        <RecipientRole>
          <Code>CD4</Code>
          <Description>CA Customs IID/D4 Status Notice</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>
    <EventTime>2022-11-15T06:35:00</EventTime>
    <EventType>CMS</EventType>
  </Event>
</UniversalEvent>
";

			var message1 = GetNewQueuedMessage(EDIMessageSubTypeList.Codes.XmlUniversalEvent, messageBody);
			var message2 = GetNewQueuedMessage(EDIMessageSubTypeList.Codes.XmlUniversalEvent, messageBody);
			message1.EM_MessageNum = "00001";
			message2.EM_MessageNum = "00002";

			message1.EM_MessageSubType = "D4";
			message1.EM_Status = "PRS";
			message1.EM_ApplicationReference = "15-Nov-22 06:3500000000";
			Factory.Save();

			var universalProcessingManager = new UniversalProcessingManager(new UMIServiceTaskWorker().SupportedMessageSubtypes, new UMIServiceTaskWorker().ExcludedMessageSubtypes, GrEngineServiceSetting.Disabled);
			AssertNoExceptionThrown(() =>
			{
				universalProcessingManager.ExecuteBatch(CancellationToken.None);
			});
		}

		public void TestPrcoessDuplicateMessageWhenGrEngineServiceSettingIsWorker()
		{
			var messageBody = @"
<UniversalEvent>
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>CAeManifestStatusNotice</Type>
          <Key>0123456789ABCDEFG</Key>
        </DataTarget>
      </DataTargetCollection>
      <RecipientRoleCollection>
        <RecipientRole>
          <Code>CD4</Code>
          <Description>CA Customs IID/D4 Status Notice</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>
    <EventTime>2022-11-15T06:35:00</EventTime>
    <EventType>CMS</EventType>
  </Event>
</UniversalEvent>
";

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var message1 = GetNewQueuedMessage(EDIMessageSubTypeList.Codes.XmlUniversalEvent, messageBody);
				var message2 = GetNewQueuedMessage(EDIMessageSubTypeList.Codes.XmlUniversalEvent, messageBody);
				message1.EM_MessageNum = "00001";
				message2.EM_MessageNum = "00002";
				Factory.Save();

				var keyGen = new UniversalProcessingManager(new UMIServiceTaskKeyGen().SupportedMessageSubtypes, new UMIServiceTaskKeyGen().ExcludedMessageSubtypes, GrEngineServiceSetting.KeyGen);
				keyGen.ExecuteBatch(CancellationToken.None);

				message1.EM_MessageSubType = "D4";
				message1.EM_Status = "PRS";
				message1.EM_ApplicationReference = "15-Nov-22 06:3500000000";
				Factory.Save();

				var worker = new UniversalProcessingManager(new UMIServiceTaskWorker().SupportedMessageSubtypes, new UMIServiceTaskWorker().ExcludedMessageSubtypes, GrEngineServiceSetting.Worker);
				AssertNoExceptionThrown(() =>
				{
					worker.ExecuteBatch(CancellationToken.None);
				});
			}
		}

		public void TestPrcoessDuplicateMessageWhenGrEngineServiceSettingIsKeyGen()
		{
			var messageBody = @"
<UniversalEvent>
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>CAeManifestStatusNotice</Type>
          <Key>0123456789ABCDEFG</Key>
        </DataTarget>
      </DataTargetCollection>
      <RecipientRoleCollection>
        <RecipientRole>
          <Code>CD4</Code>
          <Description>CA Customs IID/D4 Status Notice</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>
    <EventTime>2022-11-15T06:35:00</EventTime>
    <EventType>CMS</EventType>
  </Event>
</UniversalEvent>
";

			var message1 = GetNewQueuedMessage(EDIMessageSubTypeList.Codes.XmlUniversalEvent, messageBody);
			var message2 = GetNewQueuedMessage(EDIMessageSubTypeList.Codes.XmlUniversalEvent, messageBody);
			message1.EM_MessageNum = "00001";
			message2.EM_MessageNum = "00002";

			message1.EM_MessageSubType = "D4";
			message1.EM_Status = "PRS";
			message1.EM_ApplicationReference = "15-Nov-22 06:3500000000";
			Factory.Save();

			var keyGen = new UniversalProcessingManager(new UMIServiceTaskWorker().SupportedMessageSubtypes, new UMIServiceTaskWorker().ExcludedMessageSubtypes, GrEngineServiceSetting.KeyGen);
			AssertNoExceptionThrown(() =>
			{
				keyGen.ExecuteBatch(CancellationToken.None);
			});
		}

		public void TestUniversalShipmentWithBadXMLGetsMarkedAsFailed()
		{
			var message = GetNewQueuedMessage(EDIMessageSubTypeList.Codes.XmlUniversalShipment, @"<UniversalShipment></UniversalShipment>");
			Factory.Save();

			var logger = new TestServiceLogger();
			var serviceTask = new UMIServiceTask() { ServiceLogger = logger };
			serviceTask.RunTask();

			var newFactory = new BusinessObjectFactory();

			CombineAssertions(delegate
			{
				message = newFactory.Load<XmlEDIMessage>(message.PK);
				AssertEquals("Message Status should be changed to Failed", EDIMessage.Status.Rejected, message.EM_Status);

				AssertContains("Information|Starting processing Message #00", logger.ToString());
				AssertContains("Error|Failed to parse XML. Errors found:-", logger.ToString());
				AssertContains("Error - Line 1: Root element <UniversalShipment> must contain one <Shipment> element. </UniversalShipment> is not valid in this scope.", logger.ToString());
				AssertContains("Warning|Failed processing Message #00", logger.ToString());
			});
		}

		public void TestDisposableServiceWhenRejectingMessage()
		{
			var message = GetNewQueuedMessage(EDIMessageSubTypeList.Codes.XmlUniversalShipment, @"<UniversalShipment></UniversalShipment>");
			Factory.Save();

			var disposed = false;
			BusinessObjectFactory.SetOnFactorySaveHookForTest(f =>
			{
				if (f.NameForDebugging == "Rejected Message Factory")
				{
					f.SubscribeForDispose(new DisposableAction(() => disposed = true));
				}
			});

			var serviceTask = new UMIServiceTask() { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			message = new BusinessObjectFactory().Load<XmlEDIMessage>(message.PK);
			AssertEquals("Message Status should be changed to Rejected", EDIMessage.Status.Rejected, message.EM_Status);
			AssertEquals("Factory for rejecting message should have a disposable service", true, disposed);
		}

		public void TestValidMessageDoesNotCreateDuplicates()
		{
			var message1 = GetNewQueuedMessage(EDIMessageSubTypeList.Codes.XmlUniversalEvent, @"
<UniversalEvent>
  <Event>
    <EventType>OCR</EventType>
    <EventTime>10-JUL-2010 18:00</EventTime>
    <EventReference>Dummy Description</EventReference>
    <DataProvider>Dummy</DataProvider>
    <ContextCollection>
      <Context>
        <Type>MAWBNumber</Type>
        <Value>1211234567</Value>
      </Context>
      <Context>
        <Type>HAWBNumber</Type>
        <Value>AAAA1003248</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>");

			var message2 = GetNewQueuedMessage(EDIMessageSubTypeList.Codes.XmlUniversalEvent, @"
<UniversalEvent>
  <Event>
    <EventType>OCR</EventType>
    <EventTime>10-JUL-2010 18:00</EventTime>
    <EventReference>Dummy Description</EventReference>
    <DataProvider>Dummy</DataProvider>
    <ContextCollection>
      <Context>
        <Type>HAWBNumber</Type>
        <Value>AAAA1003248</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>");

			var message3 = GetNewQueuedMessage(EDIMessageSubTypeList.Codes.XmlUniversalEvent, @"
<UniversalEvent>
  <Event>
    <EventType>OCR</EventType>
    <EventTime>10-JUL-2010 18:00</EventTime>
    <EventReference>Dummy Description</EventReference>
    <DataProvider>Dummy</DataProvider>
    <ContextCollection>
      <Context>
        <Type>MAWBNumber</Type>
        <Value>1211234567</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>");

			var consol = Factory.New(ObjectFactory.GetType<IForwardingConsol>());
			consol[JobConsolSchema.JK_TransportMode] = Constants.TransportModes.Air;
			consol[JobConsolSchema.JK_MasterBillNum] = "1211234567";

			var shipment = Factory.New(ObjectFactory.GetType<IForwardingShipment>());
			shipment[JobShipmentSchema.JS_TransportMode] = Constants.TransportModes.Air;
			shipment[JobShipmentSchema.JS_HouseBill] = "AAAA1003248";

			var pivot = Factory.New(ObjectFactory.GetType<IJobConShipLink>());
			pivot[JobConShipLinkSchema.JN_JK] = consol.PK;
			pivot[JobConShipLinkSchema.JN_JS] = shipment.PK;

			var processTask1 = ((IWorkflowProvider)consol).WorkflowItems.AddNew();
			processTask1.P9_Type = Constants.Workflow.WorkflowTriggerType;
			processTask1.TriggerConditions.TriggerEventCode = "OCR";

			var processTask2 = ((IWorkflowProvider)shipment).WorkflowItems.AddNew();
			processTask2.P9_Type = Constants.Workflow.WorkflowTriggerType;
			processTask2.TriggerConditions.TriggerEventCode = "OCR";

			Factory.Save();

			var ocrEventFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, "OCR");
			var ocrLogsConsol = consol.GetLogs().Find(ocrEventFilter);
			AssertEquals("Precondition: Should not have OCR log for consol", 0, ocrLogsConsol.Length);

			var ocrLogsShipment = shipment.GetLogs().Find(ocrEventFilter);
			AssertEquals("Precondition: Should not have OCR log for shipment", 0, ocrLogsShipment.Length);

			var serviceTask = new UMIServiceTask() { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			var newFactory = new BusinessObjectFactory();

			CombineAssertions(delegate
			{
				consol = newFactory.Load(ObjectFactory.GetType<IForwardingConsol>(), consol.PK);
				ocrLogsConsol = consol.GetLogs().Find(ocrEventFilter);
				AssertEquals("OCR log should be added to consol", 2, ocrLogsConsol.Length);

				shipment = newFactory.Load(ObjectFactory.GetType<IForwardingShipment>(), shipment.PK);
				ocrLogsShipment = shipment.GetLogs().Find(ocrEventFilter);
				AssertEquals("Two OCR logs should be added to shipment", 2, ocrLogsShipment.Length);

				message1 = newFactory.Load<XmlEDIMessage>(message1.PK);
				message2 = newFactory.Load<XmlEDIMessage>(message2.PK);
				message3 = newFactory.Load<XmlEDIMessage>(message3.PK);
				AssertEquals("Message Status should be changed to Recognised", EDIMessage.Status.Warning, message1.EM_Status);
				AssertEquals("Message Status should be changed to Recognised", EDIMessage.Status.Warning, message2.EM_Status);
				AssertEquals("Message Status should be changed to Recognised", EDIMessage.Status.Warning, message3.EM_Status);

				var logger = (TestServiceLogger)serviceTask.ServiceLogger;

				AssertNotEquals(message1.EM_MessageNum, message2.EM_MessageNum);
				AssertNotEquals(message2.EM_MessageNum, message3.EM_MessageNum);
				AssertContains($"Information|Starting processing Message #{message1.EM_MessageNum}", logger.ToString());
				AssertContains("Information|Linked Event to Shipment S00001000 (House Bill='AAAA1003248').", logger.ToString());
				AssertContains($"Information|Finished processing Message #{message1.EM_MessageNum}", logger.ToString());

				AssertContains($"Information|Starting processing Message #{message2.EM_MessageNum}", logger.ToString());
				AssertContains("Linked Event to Shipment S00001000 (House Bill='AAAA1003248').", logger.ToString());
				AssertContains($"Information|Finished processing Message #{message2.EM_MessageNum}", logger.ToString());

				AssertContains($"Information|Starting processing Message #{message3.EM_MessageNum}", logger.ToString());
				AssertContains("Information|Linked Event to Consol C00001000 (Master Bill='1211234567').", logger.ToString());
				AssertContains($"Information|Finished processing Message #{message3.EM_MessageNum}", logger.ToString());
			});
		}

		public void TestErrorInParsingShowsOnEDIMessageNoteAsWellAsServiceTask()
		{
			var message = Factory.New<XmlEDIMessage>();
			message.EM_ApplicationCode = XmlEDIMessage.ApplicationCodes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_Status = XmlEDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = "RCV";
			message.EM_MessageText = @"<UniversalEvent>
</UniversalEvent>";
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			Factory.Save();

			var serviceTask = new UMIServiceTask() { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			message.Reload();
			AssertEquals("Message Status should be changed to Failed", EDIMessage.Status.Rejected, message.EM_Status);
			var note = ((StmNoteCollection)message.Notes.GetAllNotes())[0].ST_NoteDataAsText;

			var logger = (TestServiceLogger)serviceTask.ServiceLogger;

			CombineAssertions(delegate
			{
				AssertContains($"Information|Starting processing Message #{message.EM_MessageNum}", logger.ToString());
				AssertContains("Error|Failed to parse XML. Errors found:-", logger.ToString());
				AssertContains("Error - Line 2: Root element <UniversalEvent> must contain one <Event> element. </UniversalEvent> is not valid in this scope.", logger.ToString());
				AssertContains($"Warning|Failed processing Message #{message.EM_MessageNum}", logger.ToString());

				AssertMultilineASCIIEquals("noteAsText", @"
Error - Line 2: Root element <UniversalEvent> must contain one <Event> element. </UniversalEvent> is not valid in this scope.
Message Rejected.
				".Trim(), note);
			});
		}

		public void TestSuccessfulImportShowsCorrectMessagesOnEDIMessageNoteAsWellAsServiceTask()
		{
			var message = Factory.New<XmlEDIMessage>();
			message.EM_ApplicationCode = XmlEDIMessage.ApplicationCodes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_Status = XmlEDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = "RCV";
			message.EM_MessageText = @"
<UniversalEvent>
  <Event>
    <EventType>OCR</EventType>
    <EventTime>10-JUL-2010 18:00</EventTime>
    <EventReference>Dummy Description</EventReference>
    <ContextCollection>
      <Context>
        <Type>MAWBNumber</Type>
        <Value>02012345675</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
".Trim();
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;

			var consol = PrepareConsol("02012345675");

			var processTask = ((IWorkflowProvider)consol).WorkflowItems.AddNew();
			processTask.P9_Type = Constants.Workflow.WorkflowTriggerType;
			processTask.TriggerConditions.TriggerEventCode = "OCR";

			Factory.Save();

			var ocrEventFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, "OCR");
			var ocrLogs = consol.GetLogs().Find(ocrEventFilter);
			AssertEquals("Precondition: Should not have OCR log for Consol", 0, ocrLogs.Length);

			var serviceTask = new UMIServiceTask() { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			var newFactory = new BusinessObjectFactory();
			consol = newFactory.Load(ObjectFactory.GetType<IForwardingConsol>(), consol.PK);
			ocrLogs = consol.GetLogs().Find(ocrEventFilter);
			AssertEquals("One OCR log should be added to consol", 1, ocrLogs.Length);

			message = newFactory.Load<XmlEDIMessage>(message.PK);
			AssertEquals("Message Status should be changed to Recognised", EDIMessage.Status.ProcessedOK, message.EM_Status);
			var note = ((StmNoteCollection)message.Notes.GetAllNotes())[0].ST_NoteDataAsText;

			var logger = (TestServiceLogger)serviceTask.ServiceLogger;

			CombineAssertions(delegate
			{
				AssertContains($"Information|Starting processing Message #{message.EM_MessageNum}", logger.ToString());
				AssertContains("Information|Linked Event to Consol C00001000 (Master Bill='02012345675').", logger.ToString());
				AssertContains($"Information|Finished processing Message #{message.EM_MessageNum}", logger.ToString());

				AssertMultilineASCIIEquals("noteAsText", @"
Linked Event to Consol C00001000 (Master Bill='02012345675').
".Trim(), note);
			});
		}

		public void TestInvalidEventCodeMessageShowsOnEDIMessageNoteAsWellAsServiceTask()
		{
			var message = Factory.New<XmlEDIMessage>();
			message.EM_ApplicationCode = XmlEDIMessage.ApplicationCodes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_Status = XmlEDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = "RCV";
			message.EM_MessageText = @"<UniversalEvent>
  <Event>
    <EventType>ZZZ</EventType>
    <EventTime>10-JUL-2010 18:00</EventTime>
    <EventReference>Dummy Description</EventReference>
    <ContextCollection>
      <Context>
        <Type>MAWBNumber</Type>
        <Value>02012345675</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>";
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;

			var consol = PrepareConsol("02012345675");

			var processTask = ((IWorkflowProvider)consol).WorkflowItems.AddNew();
			processTask.P9_Type = Constants.Workflow.WorkflowTriggerType;
			processTask.TriggerConditions.TriggerEventCode = "OCR";

			Factory.Save();

			var ocrEventFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, "OCR");
			var ocrLogs = consol.GetLogs().Find(ocrEventFilter);
			AssertEquals("Precondition: Should not have OCR log for Consol", 0, ocrLogs.Length);

			var serviceTask = new UMIServiceTask() { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			var newFactory = new BusinessObjectFactory();
			consol = newFactory.Load(ObjectFactory.GetType<IForwardingConsol>(), consol.PK);
			ocrLogs = consol.GetLogs().Find(ocrEventFilter);
			AssertEquals("No OCR log should be added to consol", 0, ocrLogs.Length);

			message = newFactory.Load<XmlEDIMessage>(message.PK);
			AssertEquals("Message Status should be changed to Regected", EDIMessage.Status.Rejected, message.EM_Status);
			var note = ((StmNoteCollection)message.Notes.GetAllNotes())[0].ST_NoteDataAsText;

			var logger = (TestServiceLogger)serviceTask.ServiceLogger;

			CombineAssertions(delegate
			{
				AssertContains($"Information|Starting processing Message #{message.EM_MessageNum}", logger.ToString());
				AssertContains($"Error|[ZZZ] is not a valid {Enterprise.Core.Constants.ProductName} Event Code. Cannot import XML Event unless it has a valid code.", logger.ToString());
				AssertContains($"Information|Finished processing Message #{message.EM_MessageNum}", logger.ToString());

				AssertMultilineASCIIEquals("noteAsText", @$"
Error - [ZZZ] is not a valid {Enterprise.Core.Constants.ProductName} Event Code. Cannot import XML Event unless it has a valid code.
Message Rejected.
".Trim(), note);
			});
		}

		public void TestImport_EventContainsDuplicatedParametersInReference_Reject()
		{
			var message = Factory.New<XmlEDIMessage>();
			message.EM_ApplicationCode = XmlEDIMessage.ApplicationCodes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_Status = XmlEDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = "RCV";
			message.EM_MessageText = @"<UniversalEvent>
  <Event>
    <EventType>DEP</EventType>
    <EventTime>10-JUL-2010 18:00</EventTime>
    <EventReference>McLaren|LOC=UAIEV|FAC=CTO|LOC=AUSYD</EventReference>
  </Event>
</UniversalEvent>";
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;

			Factory.Save();

			var serviceTask = new UMIServiceTask() { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			var logger = (TestServiceLogger)serviceTask.ServiceLogger;

			AssertContains($"Information|Starting processing Message #{message.EM_MessageNum}", logger.ToString());
			AssertContains($"Information|Finished processing Message #{message.EM_MessageNum}", logger.ToString());
		}

		public void TestImport_EventContainsDuplicatedParametersExplicitly_Reject()
		{
			var message = Factory.New<XmlEDIMessage>();
			message.EM_ApplicationCode = XmlEDIMessage.ApplicationCodes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_Status = XmlEDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = "RCV";
			message.EM_MessageText = @"<UniversalEvent>
  <Event>
    <EventType>DEP</EventType>
    <EventTime>10-JUL-2010 18:00</EventTime>
    <EventReference>McLaren|LOC=UAIEV|FAC=CTO</EventReference>
	<EventParameters>
		<Location>AUSYD</Location>
	</EventParameters>
  </Event>
</UniversalEvent>";
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;

			Factory.Save();

			var serviceTask = new UMIServiceTask() { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			var logger = (TestServiceLogger)serviceTask.ServiceLogger;

			AssertContains($"Information|Starting processing Message #{message.EM_MessageNum}", logger.ToString());
			AssertContains("Error|Event contains duplicated parameters. Cannot import XML Event unless all parameters are unique.", logger.ToString());
			AssertContains($"Information|Finished processing Message #{message.EM_MessageNum}", logger.ToString());
		}

		public void TestImportInvalidEventTimeIsRejected()
		{
			var message1 = GetNewQueuedMessage(EDIMessageSubTypeList.Codes.XmlUniversalEvent, @"
<UniversalEvent>
  <Event>
    <EventType>OCR</EventType>
    <EventTime>10-JUL-0010 18:00</EventTime>
    <EventReference>Dummy Description</EventReference>
    <ContextCollection>
      <Context>
        <Type>MAWBNumber</Type>
        <Value>1211234567</Value>
      </Context>
      <Context>
        <Type>HAWBNumber</Type>
        <Value>AAAA1003248</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>");

			var consol = Factory.New(ObjectFactory.GetType<IForwardingConsol>());
			consol[JobConsolSchema.JK_TransportMode] = Constants.TransportModes.Air;
			consol[JobConsolSchema.JK_MasterBillNum] = "1211234567";

			var shipment = Factory.New(ObjectFactory.GetType<IForwardingShipment>());
			shipment[JobShipmentSchema.JS_TransportMode] = Constants.TransportModes.Air;
			shipment[JobShipmentSchema.JS_HouseBill] = "AAAA1003248";

			var pivot = Factory.New(ObjectFactory.GetType<IJobConShipLink>());
			pivot[JobConShipLinkSchema.JN_JK] = consol.PK;
			pivot[JobConShipLinkSchema.JN_JS] = shipment.PK;
			Factory.Save();

			var serviceTask = new UMIServiceTask() { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			AssertContains("Information|Starting processing Message #00", serviceTask.ServiceLogger.ToString());
			AssertContains("Error|Failed to parse XML. Errors found:-", serviceTask.ServiceLogger.ToString());
			AssertContains("Warning - Line 4: <Event>.<EventTime> - Invalid value [10-JUL-0010 18:00]. Value must be a valid date", serviceTask.ServiceLogger.ToString());
			AssertContains("Error - Line 16: Top Level Element <Event> opened at line 2 cannot be imported as it is missing mandatory elements. Missing: EventTime.", serviceTask.ServiceLogger.ToString());
			AssertContains("Warning|Failed processing Message #00", serviceTask.ServiceLogger.ToString());
		}

		public void TestUnableToLinkMessageShowsOnEDIMessageNoteAsWellAsServiceTask()
		{
			var message = Factory.New<XmlEDIMessage>();
			message.EM_ApplicationCode = XmlEDIMessage.ApplicationCodes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_Status = XmlEDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = "RCV";
			message.EM_MessageText = @"<UniversalEvent>
  <Event>
    <EventType>OCR</EventType>
    <EventTime>10-JUL-2010 18:00</EventTime>
    <EventReference>Dummy Description</EventReference>
    <ContextCollection>
      <Context>
        <Type>MAWBNumber</Type>
        <Value>11111111111</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>";
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;

			var consol = PrepareConsol("02012345675");

			var processTask = ((IWorkflowProvider)consol).WorkflowItems.AddNew();
			processTask.P9_Type = Constants.Workflow.WorkflowTriggerType;
			processTask.TriggerConditions.TriggerEventCode = "OCR";

			Factory.Save();

			var ocrEventFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, "OCR");
			var ocrLogs = consol.GetLogs().Find(ocrEventFilter);
			AssertEquals("Precondition: Should not have OCR log for Consol", 0, ocrLogs.Length);

			var serviceTask = new UMIServiceTask() { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			var newFactory = new BusinessObjectFactory();
			consol = newFactory.Load(ObjectFactory.GetType<IForwardingConsol>(), consol.PK);
			ocrLogs = consol.GetLogs().Find(ocrEventFilter);
			AssertEquals("No OCR log should be added to consol", 0, ocrLogs.Length);

			message = newFactory.Load<XmlEDIMessage>(message.PK);
			AssertEquals("Message Status should be changed to Discarded", EDIMessage.Status.Discarded, message.EM_Status);
			var note = ((StmNoteCollection)message.Notes.GetAllNotes())[0].ST_NoteDataAsText;

			var logger = (TestServiceLogger)serviceTask.ServiceLogger;

			CombineAssertions(delegate
			{
				AssertContains($"Information|Starting processing Message #{message.EM_MessageNum}", logger.ToString());
				AssertContains("Warning|No Module found a Business Entity to link this Universal Event to.", logger.ToString());
				AssertContains($"Information|Finished processing Message #{message.EM_MessageNum}", logger.ToString());

				AssertMultilineASCIIEquals("noteAsText", @"
Warning - No Module found a Business Entity to link this Universal Event to.
Message Discarded.
".Trim(), note);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRunTask_CIMP_6_1()
		{
			var message = SetupIncomingEdiMessageWithInterchangeFrom("6.1.xml"
				, EDIMessageSubTypeList.Codes.XmlUniversalEvent);

			var consol = PrepareConsol("02012345675");

			var processTask = ((IWorkflowProvider)consol).WorkflowItems.AddNew();
			processTask.P9_Type = Constants.Workflow.WorkflowTriggerType;
			processTask.TriggerConditions.TriggerEventCode = "OCR";

			Factory.Save();

			var ocrEventFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, "OCR");
			var ocrLogs = consol.GetLogs().Find(ocrEventFilter);
			AssertEquals("Precondition: Should not have OCR log for Consol", 0, ocrLogs.Length);

			var serviceTask = new UMIServiceTask() { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			var newFactory = new BusinessObjectFactory();
			consol = newFactory.Load(ObjectFactory.GetType<IForwardingConsol>(), consol.PK);
			ocrLogs = consol.GetLogs().Find(ocrEventFilter);
			AssertEquals("One OCR log should be added to consol", 1, ocrLogs.Length);
			var log = ocrLogs[0];

			processTask = newFactory.Load<ProcessTask>(processTask.PK);
			AssertEquals("Should trigger process tasks for OCR", log.SL_EventTime, processTask.P9_ActualDate.ToZDateTime());

			message = newFactory.Load<XmlEDIMessage>(message.PK);
			AssertEquals("Message Status should be changed to Recognised", EDIMessage.Status.Warning, message.EM_Status);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRunTask_CIMP_6_2()
		{
			var message = SetupIncomingEdiMessageWithInterchangeFrom("6.2.xml"
				, EDIMessageSubTypeList.Codes.XmlUniversalEvent);

			var consol = PrepareConsol("05712345675");

			Factory.Save();

			var filter = new ZQuery(StmALogSchema.SL_SE_NKEvent, "TRF");
			var consolLogs = consol.GetLogs().Find(filter);
			AssertEquals("Precondition: Should not have TRF log for Consol", 0, consolLogs.Length);

			var serviceTask = new UMIServiceTask() { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			var newFactory = new BusinessObjectFactory();
			consol = newFactory.Load(ObjectFactory.GetType<IForwardingConsol>(), consol.PK);
			consolLogs = consol.GetLogs().Find(filter);
			AssertEquals("TRF log should be added to consol", 1, consolLogs.Length);
			var consolLog = consolLogs[0];
			AssertEquals("consolLog.SL_Reference", "Dummy Description", consolLog.SL_Reference);

			var messageAfter = newFactory.Load<XmlEDIMessage>(message.PK);
			AssertEquals("Message Status should be changed to Recognised", EDIMessage.Status.Warning, messageAfter.EM_Status);
		}

		[UseSnapshotProtection]
		public void TestCodesMappedToTargetThrowsExceptionAndNotifyUser()
		{
			eServicesRegistry.Instance.ImportRejectedNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, CreateGroupWithUser("rejected@test.com").group.PK.ToGuid());
			UniversalXmlWorkflowProcessor.PreventTemporarilyResumeValidation_ForTestOnly.Value = true;

			AssertEquals("Precondition: Env.OutgoingMailManager.EmailsCreated.Count", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = @"betty.boop";
			staff.GS_Code = "B.B";
			staff.GS_EmailAddress = "betty.boop@cargowise.com";
			var postMastersGroup = Factory.Load<GlbGroup>(Enterprise.Core.Constants.Groups.PostMastersGroupPK);
			staff.Groups.Add(postMastersGroup);
			var message = GetNewQueuedMessage(EDIMessageSubTypeList.Codes.XmlUniversalShipment, CodesMappedToTargetXML);
			message.EM_MessageNum = "00001";

			Factory.Save();
			var registrationKey = ObjectFactory.Get<Enterprise.Integration.Licensing.IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "WTL";
			registrationKey.ServerCodeForTest = "NMO";

			var processor = new UniversalProcessingManager(new string[] { message.EM_MessageSubType }, Array.Empty<string>());

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				processor.ExecuteBatch();
			}
			processor.Logger.Log(".");
			var email = Env.OutgoingMailManager.EmailsCreated;
			AssertEquals(1, email.Count);
			AssertEquals($"Error Processing Incoming EDI Message: UDM-XDC-{message.EM_MessageNum}", email[0].Subject);
			ErrorReporter.Clear();
		}

		[UseSnapshotProtection]
		public void TestSaveMessageFactoryIfFailed_RunsBeforeStreamsAreDisposed()
		{
			ErrorReporter.Clear();
			AssertEquals("Precondition: ErrorReporter error count is 0.", 0, ErrorReporter.TotalErrorCount);
			var pk = ZGuid.Empty;
			using (Factory.AddDisposableService())
			{
				var interchange = Factory.New<EDIInterchange>();
				interchange.EI_InterchangeType = "XMS";
				interchange.EI_From = "God";
				interchange.EI_To = "InfinityAndBeyond";
				interchange.EI_HeaderText = @"
<InterchangeInfo xmlns=""http://www.edi.com.au/EnterpriseService/"">
	<Date>2013-11-28T15:37:56.893+11:00</Date>
	<XmlType>Verbose</XmlType>
	<Source/>
	<Target />
	<Acknowledgement>
      <Required>OnAll</Required>
      <Channel>eAdaptor</Channel>
      <RecipientID>Whom_It_May_Concern</RecipientID>
      <ContextCollection>
        <Context>
          <Type>ORG</Type>
          <Value>INBORG</Value>
        </Context>
      </ContextCollection>
    </Acknowledgement>
  </InterchangeInfo>";

				var message = Factory.New<XmlEDIMessage>();
				((BusinessObject)message)[EDIMessageSchema.EM_EI] = interchange.PK;
				message.EM_MessageType = "XDC";
				message.EM_MessageSubType = "XUS";

				var source = Factory.SubscribeForDispose(new MemoryStream());
				StreamWriter strW = new StreamWriter(source);
				strW.Write(@"
<UniversalEvent>
  <Event>
    <EventType>OCR</EventType>
    <EventTime>10-JUL-2010 18:00</EventTime>
    <EventReference>Dummy Description</EventReference>
    <DataProvider>Dummy</DataProvider>
    <ContextCollection>
      <Context>
        <Type>MAWBNumber</Type>
        <Value>1211234567</Value>
      </Context>
      <Context>
        <Type>HAWBNumber</Type>
        <Value>AAAA1003248</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>");
				strW.Flush();
				source.Position = 0;
				message.EM_ApplicationCode = XmlEDIMessage.ApplicationCodes.UniversalDataMessaging;
				message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
				message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
				message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
				message.EM_Status = XmlEDIMessage.Status.Queued;
				message.EM_MessageNum = "00001";
				message.SetEM_MessageTextOrDataSource(source);

				Factory.Save();
				pk = message.PK;
			}

			var processor = new UniversalProcessingManager(new string[] { EDIMessageSubTypeList.Codes.XmlUniversalEvent }, Enumerable.Empty<string>(), GrEngineServiceSetting.Disabled);
			processor.ExecuteBatch(CancellationToken.None);

			var messageAfterExecute = new BusinessObjectFactory().Load<EDIMessage>(pk);
			var acknowledgeAfterExecute = new BusinessObjectFactory().LoadTop1<EDIInterchange>(new ZQueryFactory().Create(EDIInterchangeSchema.EI_To, "Whom_It_May_Concern"));
			var log = string.Join("\r\n", processor.Logger.UserLogStrings.Cast<string>());

			CombineAssertions(() =>
			{
				AssertNotNull("Acknowledgement did not create. Test is invalid.", acknowledgeAfterExecute);
				AssertEquals("Message status should be set to Discarded.", EDIMessage.Status.Discarded, messageAfterExecute.EM_Status);
				AssertNotContains("Cannot access a disposed object.", log);
			});
		}

		public void TestTriggerFiredByImportedUniversalEvent()
		{
			var shipment = Factory.New(ObjectFactory.GetType<IForwardingShipment>());
			shipment[JobShipmentSchema.JS_UniqueConsignRef] = "S00001323";

			var trigger = ((IWorkflowProvider)shipment).WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "MCR";
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;

			var message = GetNewQueuedMessage(EDIMessageSubTypeList.Codes.XmlUniversalEvent, @"<UniversalEvent>
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>S00001323</Key>
        </DataTarget>
      </DataTargetCollection>
      <RecipientRoleCollection>
        <RecipientRole>
          <Code>BRO</Code>
          <Description>Broker</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>
    <EventTime>2020-05-05T15:15:15.000</EventTime>
    <EventType>Z00</EventType>
    <CreatedTime>2020-05-05T15:15:15.000</CreatedTime>
    <EventReference>|RES=Y</EventReference>
    <IsEstimate>false</IsEstimate>
  </Event>
</UniversalEvent>");
			message.EM_MessageNum = "00000000000000000001";

			Factory.Save();

			var serviceTask = new UMIServiceTask() { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			var logger = (TestServiceLogger)serviceTask.ServiceLogger;

			Factory.Save();

			AssertContains($"Information|Starting processing Message #{message.EM_MessageNum}", logger.ToString());
			AssertContains("Information|Linked Event to Shipment S00001323.", logger.ToString());
			AssertContains($"Information|Finished processing Message #{message.EM_MessageNum}", logger.ToString());

			var logResult = MasterFilesTestHelper.RunLogWalker();

			trigger.Reload();

			AssertEquals("Trigger Fired", 99, ((int)trigger.P9_TriggerFiredCountdown));
		}

		ProcessTaskTemplate WorkflowTemplateWithCustomFieldCodeDescriptionAddonRule()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "SHP";
			template.P0_CustomFieldFallback = FallbackTypeList.Codes.NeverFallback;

			var addOnRule = Factory.New<GenCustomAddOnRule>();
			#region SourceCode
			addOnRule.XR_SourceCode =
	@"<sourceCode>
  <rules>
    <rule code=""InvalidCode"" enabled=""true"">
      <details>
        <codeDescriptionList>
          <codeDescription code=""Airport to Door"" description=""1"" />
          <codeDescription code=""Door to Airport"" description=""2"" />
        </codeDescriptionList>
      </details>
    </rule>
    <rule code=""CreateEvent"" enabled=""false"">
      <details />
    </rule>
    <rule code=""DateTimeFormat"" enabled=""false"">
      <details>
        <format>Short</format>
      </details>
    </rule>
    <rule code=""CheckEntered"" enabled=""false"">
      <details />
    </rule>
  </rules>
</sourceCode>";
			#endregion

			var def = template.GenCustomColumnDefinitions.AddNew();
			def.XC_Name = "Custom Field";
			def.XC_Type = "STR";
			def.XC_XR = addOnRule.PK;

			return template;
		}

		public void TestCustomFieldCodeDescriptionAddonRule_CodeCapitalized_Import()
		{
			WorkflowTemplateWithCustomFieldCodeDescriptionAddonRule();
			Factory.Save();

			var shipment = Factory.New<IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001000";

			Factory.Save();

			#region xmlMessage (CustomField)
			var xmlMessage =
	@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>S00001000</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
	<CustomizedFieldCollection>
		<CustomizedField>
			<DataType>String</DataType>
			<Key>Custom Field</Key>
			<Value>Airport to Door</Value>
		</CustomizedField>
	</CustomizedFieldCollection>
  </Shipment>
</UniversalShipment>";
			#endregion

			var messageUpdate = GetNewQueuedMessage(EDIMessageSubTypeList.Codes.XmlUniversalShipment, xmlMessage);
			messageUpdate.EM_MessageNum = "00000000000000000001";

			Factory.Save();

			var serviceTask = new UMIServiceTask() { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			var shipmentCustomField = Factory.Load<GenCustomAddOnValue>(new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, shipment.PK)).Single();
			AssertContains("CodeDescriptionList - Code should be capitalized", "AIRPORT TO DOOR", shipmentCustomField.XV_Data);
		}

		public void TestCustomFieldAddonRuleForUpdateXML()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "SHP";
			template.P0_CustomFieldFallback = FallbackTypeList.Codes.NeverFallback;

			var addOnRule = Factory.New<GenCustomAddOnRule>();
			#region SourceCode
			addOnRule.XR_SourceCode =
	@"<sourceCode>
	<rules>
		<rule code=""CheckEntered"" enabled=""false"">
			<details />
		</rule>
		<rule code=""DateTimeFormat"" enabled=""false"">
			<details>
			<format>Short</format>
			</details>
		</rule>
		<rule code=""CreateEvent"" enabled=""true"">
			<details>
				<CreateEventRuleCode>Z00</CreateEventRuleCode>
			</details>
		</rule>
		<rule code=""InvalidCode"" enabled=""false"">
			<details />
		</rule>
	</rules>
</sourceCode>";
			#endregion

			var def = template.GenCustomColumnDefinitions.AddNew();
			def.XC_Name = "cf1";
			def.XC_Type = "STR";
			def.XC_XR = addOnRule.PK;

			var trigger = template.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = "Z00";

			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce;
			triggerAction.PQ_P0_WorkflowTemplate = template.PK;

			#region xmlMessage (Create)
			var xmlMessage =
	@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key />
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
  </Shipment>
</UniversalShipment>";
			#endregion

			var message = GetNewQueuedMessage(EDIMessageSubTypeList.Codes.XmlUniversalShipment, xmlMessage);
			message.EM_MessageNum = "00000000000000000001";

			Factory.Save();

			var serviceTask = new UMIServiceTask() { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			Factory.Save();

			#region xmlMessage (CustomField)
			xmlMessage =
	@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>S00001000</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
	<CustomizedFieldCollection>
		<CustomizedField>
			<DataType>String</DataType>
			<Key>cf1</Key>
			<Value>asd</Value>
		</CustomizedField>
	</CustomizedFieldCollection>
  </Shipment>
</UniversalShipment>";
			#endregion

			var messageUpdate = GetNewQueuedMessage(EDIMessageSubTypeList.Codes.XmlUniversalShipment, xmlMessage);
			messageUpdate.EM_MessageNum = "00000000000000000002";

			Factory.Save();

			serviceTask.RunTask();

			var logger = (TestServiceLogger)serviceTask.ServiceLogger;

			Factory.Save();

			var shipment = Factory.LoadTop1<IForwardingShipment>(new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, "S00001000"));

			var shipmentLogs = ((BusinessObject)shipment).GetLogs();

			AssertEquals("Z00 Not Created", true, shipmentLogs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == "Z00"));
		}

		public void TestCustomFieldAddonRuleForImportedXML()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "SHP";
			template.P0_CustomFieldFallback = FallbackTypeList.Codes.NeverFallback;

			var addOnRule = Factory.New<GenCustomAddOnRule>();
			#region SourceCode
			addOnRule.XR_SourceCode =
	@"<sourceCode>
	<rules>
		<rule code=""CheckEntered"" enabled=""false"">
			<details />
		</rule>
		<rule code=""DateTimeFormat"" enabled=""false"">
			<details>
			<format>Short</format>
			</details>
		</rule>
		<rule code=""CreateEvent"" enabled=""true"">
			<details>
				<CreateEventRuleCode>Z00</CreateEventRuleCode>
			</details>
		</rule>
		<rule code=""InvalidCode"" enabled=""false"">
			<details />
		</rule>
	</rules>
</sourceCode>";
			#endregion

			var def = template.GenCustomColumnDefinitions.AddNew();
			def.XC_Name = "cf1";
			def.XC_Type = "STR";
			def.XC_XR = addOnRule.PK;

			Factory.Save();

			#region xmlMessage (CustomField)
			var xmlMessage =
	@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key />
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
	<CustomizedFieldCollection>
		<CustomizedField>
			<DataType>String</DataType>
			<Key>cf1</Key>
			<Value>asd</Value>
		</CustomizedField>
	</CustomizedFieldCollection>
  </Shipment>
</UniversalShipment>";
			#endregion

			var message = GetNewQueuedMessage(EDIMessageSubTypeList.Codes.XmlUniversalShipment, xmlMessage);
			message.EM_MessageNum = "00000000000000000001";

			Factory.Save();

			var serviceTask = new UMIServiceTask() { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			var logger = (TestServiceLogger)serviceTask.ServiceLogger;

			Factory.Save();

			var shipment = Factory.LoadTop1<IForwardingShipment>(new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, "S00001000"));

			var shipmentLogs = ((BusinessObject)shipment).GetLogs();

			AssertEquals("Z00 Not Created", true, shipmentLogs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == "Z00"));
		}

		public void TestImportedCustomFieldsDIMTriggerConditions()
		{
			var shipment = Factory.New<IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001000";

			var trigger1 = ((IWorkflowProvider)shipment).WorkflowItems.Triggers.AddNew();
			trigger1.TriggerConditions.TriggerEventCode = Events.DataImportCode;
			trigger1.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.UserDefined;
			trigger1.TriggerConditions.TriggerConditionValue = "\"<GetCustomField(CF)>\"!=\"\"";

			var trigger2 = ((IWorkflowProvider)shipment).WorkflowItems.Triggers.AddNew();
			trigger2.TriggerConditions.TriggerEventCode = Events.DataImportCode;
			trigger2.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.UserDefined;
			trigger2.TriggerConditions.TriggerConditionValue = "\"<GetCustomField(CF)>\"==\"\"";

			Factory.Save();

			#region xmlMessage (CustomField)
			var xmlMessage =
			@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
			  <Shipment>
			    <DataContext>
			      <DataTargetCollection>
			        <DataTarget>
			          <Type>ForwardingShipment</Type>
			          <Key>S00001000</Key>
			        </DataTarget>
			      </DataTargetCollection>
			    </DataContext>
				<CustomizedFieldCollection>
					<CustomizedField>
						<DataType>String</DataType>
						<Key>CF</Key>
						<Value>new_value</Value>
					</CustomizedField>
				</CustomizedFieldCollection>
			  </Shipment>
			</UniversalShipment>";
			#endregion

			var messageUpdate = GetNewQueuedMessage(EDIMessageSubTypeList.Codes.XmlUniversalShipment, xmlMessage);

			Factory.Save();

			var serviceTask = new UMIServiceTask() { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			var newFactory = new BusinessObjectFactory();
			var triggerShouldHaveFired = newFactory.Load<ProcessTask>(trigger1.PK);
			var triggerShouldNotHaveFired = newFactory.Load<ProcessTask>(trigger2.PK);
			var reloadedShipment = newFactory.Load(ObjectFactory.GetType<IForwardingShipment>(), shipment.PK) as IStmALogParent;

			CombineAssertions(() =>
			{
				Assert(reloadedShipment.Logs.Find(l => l.SL_SE_NKEvent == Events.DataImportCode).ToArray().Length == 1);
				AssertEquals((short)99, triggerShouldHaveFired.P9_TriggerFiredCountdown);
				AssertEquals((short)100, triggerShouldNotHaveFired.P9_TriggerFiredCountdown);
			});
		}

		public void TestCustomFieldValuesSet()
		{
			#region xmlMessage (CustomField)
			var xmlMessage =
	@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key />
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
	<CustomizedFieldCollection>
		<CustomizedField>
			<DataType>String</DataType>
			<Key>cf1</Key>
			<Value>asd</Value>
		</CustomizedField>
	</CustomizedFieldCollection>
  </Shipment>
</UniversalShipment>";
			#endregion

			var message = GetNewQueuedMessage(EDIMessageSubTypeList.Codes.XmlUniversalShipment, xmlMessage);
			message.EM_MessageNum = "00000000000000000001";

			Factory.Save();

			var serviceTask = new UMIServiceTask() { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			var logger = (TestServiceLogger)serviceTask.ServiceLogger;

			Factory.Save();

			var shipment = Factory.LoadTop1<IForwardingShipment>(new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, "S00001000"));
			var customFields = ((ICustomFieldProvider)shipment).GetCustomBusinessObject();
			var propertyNames = ((IDynamicBusinessObject)customFields).PropertyNames;

			AssertEquals(2, propertyNames.Length);

			AssertEquals("__CF1__prop__ZString", propertyNames[0]);
			AssertEquals("asd", customFields[propertyNames[0]]);
		}

		#region TestUniversalShipmentWithAction

		public void TestUniversalShipmentWithAction_LinkOnly()
		{
			var shipment = (IForwardingShipment)Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			shipment.JS_UniqueConsignRef = "SXX001001";
			shipment.JS_GoodsDescription = "goods description";

			Factory.Save();

			var usxml = GetShipmentWithImportAction(ImportAction.LinkOnly, shipment.JS_UniqueConsignRef);

			var message = GetNewQueuedMessage(EDIMessageSubTypeList.Codes.XmlUniversalShipment, usxml);
			Factory.Save();

			var logger = new TestServiceLogger();
			var serviceTask = new UMIServiceTask() { ServiceLogger = logger };
			serviceTask.RunTask();

			var newFactory = new BusinessObjectFactory();

			void Assert()
			{
				message = newFactory.Load<XmlEDIMessage>(message.PK);
				AssertEquals("Message Status should be changed to Failed", EDIMessage.Status.Linked, message.EM_Status);

				shipment = (IForwardingShipment)newFactory.Load(ObjectFactory.GetType<IForwardingShipment>(), shipment.PK);
				AssertEquals("Shipment was not updated from UXml", "goods description", shipment.JS_GoodsDescription);

				var dataImportEvents = ((IStmALogParent)shipment)
					.Logs
					.Find(l => l.SL_SE_NKEvent == Events.DataLinkedCode || l.SL_SE_NKEvent == Events.DataImportCode)
					.ToArray();

				AssertContainsExactElementsInAnyOrder("found exactly 1 DLI event",
					new[]
					{
						Events.DataLinkedCode
					},
					dataImportEvents.Select(l => l.SL_SE_NKEvent));

				AssertEquals("message was linked to the DLI event",
					message, dataImportEvents[0].RelatedEDIMessage.Message);
				AssertContains($"Information|Starting processing Message #{message.EM_MessageNum}", logger.ToString());
				AssertContains("Information|Universal Shipment data was linked to Shipment SXX001001.", logger.ToString());
				AssertContains("Information|Successfully saved, but nothing was reported as being updated.", logger.ToString());
				AssertContains($"Information|Finished processing Message #{message.EM_MessageNum}", logger.ToString());
			}

			CombineAssertions(Assert);
		}

		public void TestUniversalShipmentWithAction_Merge()
		{
			var shipment = (IForwardingShipment)Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			shipment.JS_UniqueConsignRef = "SXX001001";
			shipment.JS_GoodsDescription = "goods description";

			Factory.Save();

			var usxml = GetShipmentWithImportAction(ImportAction.Merge, shipment.JS_UniqueConsignRef);

			var message = GetNewQueuedMessage(EDIMessageSubTypeList.Codes.XmlUniversalShipment, usxml);
			Factory.Save();

			var logger = new TestServiceLogger();
			var serviceTask = new UMIServiceTask() { ServiceLogger = logger };
			serviceTask.RunTask();

			var newFactory = new BusinessObjectFactory();

			void Assert()
			{
				message = newFactory.Load<XmlEDIMessage>(message.PK);
				AssertEquals("Message Status should be changed to Failed", EDIMessage.Status.ProcessedOK, message.EM_Status);

				shipment = (IForwardingShipment)newFactory.Load(ObjectFactory.GetType<IForwardingShipment>(), shipment.PK);
				AssertEquals("Shipment was updated from UXml", "goods description in xml", shipment.JS_GoodsDescription);

				var dataImportEvents = ((IStmALogParent)shipment)
					.Logs
					.Find(l => l.SL_SE_NKEvent == Events.DataLinkedCode || l.SL_SE_NKEvent == Events.DataImportCode)
					.ToArray();

				AssertContainsExactElementsInAnyOrder("found exactly 1 DIM event",
					new[]
					{
						Events.DataImportCode
					},
					dataImportEvents.Select(l => l.SL_SE_NKEvent));

				AssertEquals("message was linked to the DIM event",
					message, dataImportEvents[0].RelatedEDIMessage.Message);

				AssertContains($"Information|Starting processing Message #{message.EM_MessageNum}", logger.ToString());
				AssertContains("Information|Updated Shipment SXX001001 (House Bill='HBL00001001') from UniversalShipment.", logger.ToString());
				AssertContains("Information|Successfully saved Shipment SXX001001 (House Bill='HBL00001001').", logger.ToString());
				AssertContains($"Information|Finished processing Message #{message.EM_MessageNum}", logger.ToString());
			}

			CombineAssertions(Assert);
		}

		string GetShipmentWithImportAction(ImportAction importAction, string key = null) => $@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <Action>{importAction}</Action>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>{key}</Key>
        </DataTarget>
      </DataTargetCollection>

      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <DataProvider>EDIDATEDI</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <EventBranch>
        <Code>BNE</Code>
        <Name>BN - AUBNE</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code></Code>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>{Enterprise.Core.Constants.ProductSupportName}</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2021-04-08T16:24:07.52</TriggerDate>
      <TriggerDescription></TriggerDescription>
      <TriggerType>Manual</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ORP</Code>
          <Description>Organization Proxy</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <GoodsDescription>goods description in xml</GoodsDescription>
    <WayBillNumber>HBL00001001</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>

  </Shipment>
</UniversalShipment>";

		#endregion

		#region CodesMappedToTargetXML
		const string CodesMappedToTargetXML = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
      <Shipment>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>BillOfLading</Type>
              <Key />
            </DataTarget>
          </DataTargetCollection>
          <Company>
            <Code>EDI</Code>
          </Company>
          <DataProvider>JGSHULHUL</DataProvider>
          <EnterpriseID>WTL</EnterpriseID>
          <ServerID>NMO</ServerID>
        </DataContext>
        <AgentsReference />
        <BookingConfirmationReference>ORDER NO 355130</BookingConfirmationReference>
        <NoCopyBills>1</NoCopyBills>
        <NoOriginalBills>0</NoOriginalBills>
        <PortOfDestination>
          <Code>GBTEE</Code>
          <Name />
        </PortOfDestination>
        <PortOfDischarge>
          <Code>GBTEE</Code>
          <Name />
        </PortOfDischarge>
        <PortOfLoading>
          <Code>PLGDY</Code>
          <Name />
        </PortOfLoading>
        <PortOfOrigin>
          <Code>PLGDY</Code>
          <Name />
        </PortOfOrigin>
        <VesselName>JSP ROVER</VesselName>
        <VoyageFlightNo>2229</VoyageFlightNo>
        <ReleaseType>
          <Code>SWB</Code>
          <Description />
        </ReleaseType>
        <TransportLegCollection>
          <TransportLeg>
            <PortOfDischarge>
              <Code>GBTEE</Code>
              <Name />
            </PortOfDischarge>
            <PortOfLoading>
              <Code>PLGDY</Code>
              <Name />
            </PortOfLoading>
            <LegOrder>0</LegOrder>
            <Carrier>
              <AddressType>Carrier</AddressType>
              <OrganizationCode>EURSHILMS</OrganizationCode>
            </Carrier>
            <CarrierBookingReference />
            <CarrierServiceLevel>
              <Code />
            </CarrierServiceLevel>
            <EstimatedArrival />
            <EstimatedDeparture>2016-06-19T00:00:00</EstimatedDeparture>
            <IsCargoOnly>true</IsCargoOnly>
            <LegNotes />
            <LegType>Main</LegType>
            <TransportMode>Sea</TransportMode>
            <VesselName>JSP ROVER</VesselName>
            <VoyageFlightNo>2229</VoyageFlightNo>
          </TransportLeg>
        </TransportLegCollection>
        <ContainerMode>
          <Code>FCL</Code>
          <Description>Full Container Load</Description>
        </ContainerMode>
        <GoodsDescription>APPLE JUICE</GoodsDescription>
        <HBLAWBChargesDisplay>
          <Code>NON</Code>
          <Description>No Charges Showing</Description>
        </HBLAWBChargesDisplay>
        <ContainerCollection Content=""Complete"">
          <Container>
            <ContainerNumber>SBKU0003857</ContainerNumber>
            <ContainerType>
              <Code>22T0</Code>
              <ISOCode>2T86</ISOCode>
            </ContainerType>
            <GoodsWeight />
            <GrossWeight />
            <TareWeight>3500.000</TareWeight>
            <IsDamaged>false</IsDamaged>
            <IsEmptyContainer>false</IsEmptyContainer>
            <IsSealOk>true</IsSealOk>
            <IsShipperOwned>true</IsShipperOwned>
            <ReleaseNum />
            <Seal>0107541-544 </Seal>
            <SecondSeal />
            <VolumeUnit>
              <Code>M3</Code>
              <Description>Cubic Metres</Description>
            </VolumeUnit>
            <WeightUnit>
              <Code>KG</Code>
              <Description>Kilograms</Description>
            </WeightUnit>
          </Container>
        </ContainerCollection>
        <IsShipping>true</IsShipping>
        <JobCosting>
          <Branch>
            <Code>BNE</Code>
          </Branch>
          <Department>
            <Code>SDC</Code>
          </Department>
          <OperationsStaff>
            <Code>YF</Code>
            <Name>Yvonne Fewster</Name>
          </OperationsStaff>
          <ChargeLineCollection>
            <ChargeLine>
              <ImportMetaData>
                <Instruction>UpdateAndInsertIfNotFound</Instruction>
                <MatchingCriteriaCollection>
                  <MatchingCriteria>
                    <FieldName>ChargeCode</FieldName>
                    <Value>CFSCUST</Value>
                  </MatchingCriteria>
                </MatchingCriteriaCollection>
              </ImportMetaData>
              <Branch>
                <Code>BNE</Code>
                <Type>Organization</Type>
              </Branch>
              <ChargeCode>
                <Code>CFSCUST</Code>
              </ChargeCode>
              <Debtor>
                <Type>Organization</Type>
                <Key />
              </Debtor>
              <Department>
                <Code>XXX</Code>
              </Department>
              <Description>Origin Fuel Surcharge</Description>
              <SellInvoiceType>LCO</SellInvoiceType>
              <SellOSAmount>30.0000</SellOSAmount>
              <SellOSCurrency>
                <Code>GBP</Code>
                <Description />
              </SellOSCurrency>
            </ChargeLine>
          </ChargeLineCollection>
        </JobCosting>
        <PaymentMethod>
          <Code>CLT</Code>
        </PaymentMethod>
        <ServiceLevel>
          <Code>STD</Code>
          <Description>Standard</Description>
        </ServiceLevel>
        <ShipmentStatus>
          <Code>CNF</Code>
          <Description>Confirmed</Description>
        </ShipmentStatus>
        <ShippedOnBoard>
          <Code>SHP</Code>
          <Description>Shipped</Description>
        </ShippedOnBoard>
        <TotalWeight />
        <TotalWeightUnit>
          <Code>KG</Code>
          <Description />
        </TotalWeightUnit>
        <TransportMode>
          <Code>SEA</Code>
          <Description>Sea Freight</Description>
        </TransportMode>
        <WayBillNumber>F527</WayBillNumber>
        <WayBillType>
          <Code>MWB</Code>
          <Description>Master Waybill</Description>
        </WayBillType>
        <DateCollection>
          <Date>
            <Type>Departure</Type>
            <IsEstimate>true</IsEstimate>
            <Value>2016-06-19T00:00:00</Value>
          </Date>
          <Date>
            <Type>Arrival</Type>
            <IsEstimate>true</IsEstimate>
            <Value />
          </Date>
          <Date>
            <Type>ShippedOnBoard</Type>
            <IsEstimate>false</IsEstimate>
            <Value />
          </Date>
        </DateCollection>
        <EntryNumberCollection>
          <EntryNumber>
            <Number />
            <Type>
              <Code>C</Code>
              <Description />
            </Type>
            <CountryOfIssue>
              <Code>GB</Code>
              <Name />
            </CountryOfIssue>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
            <EntryLineReference />
            <ExpiryDate />
            <IssueDate />
          </EntryNumber>
        </EntryNumberCollection>
        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>ConsigneeDocumentaryAddress</AddressType>
            <OrganizationCode />
            <Address1>       53-55 MERSEY VIEW</Address1>
            <Address2>       BRIGHTON-LE-SANDS</Address2>
            <AddressOverride>false</AddressOverride>
            <City />
            <CompanyName>       SEABROOK TANK SERVICES LTD</CompanyName>
            <Country>
              <Code />
            </Country>
            <Postcode />
            <State />
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsigneePickupDeliveryAddress</AddressType>
            <OrganizationCode />
            <Address1>       53-55 MERSEY VIEW</Address1>
            <Address2>       BRIGHTON-LE-SANDS</Address2>
            <AddressOverride>false</AddressOverride>
            <City />
            <CompanyName>       SEABROOK TANK SERVICES LTD</CompanyName>
            <Country>
              <Code />
              <Name />
            </Country>
            <Postcode />
            <State />
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsignorDocumentaryAddress</AddressType>
            <OrganizationCode />
            <Address1>       ON BEHALF OF</Address1>
            <Address2>       SALES &amp; CUSTOMER SERVICE GMBH</Address2>
            <AddressOverride>false</AddressOverride>
            <City>       AT-8200 GLEISDORF,</City>
            <CompanyName>P AGEN POL-AGENT SZCZECIN</CompanyName>
            <Country>
              <Code />
              <Name />
            </Country>
            <Postcode />
            <State>       AUSTRIA</State>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsignorPickupDeliveryAddress</AddressType>
            <OrganizationCode />
            <Address1>       ON BEHALF OF</Address1>
            <Address2>       SALES &amp; CUSTOMER SERVICE GMBH</Address2>
            <AddressOverride>false</AddressOverride>
            <City>       AT-8200 GLEISDORF,</City>
            <CompanyName>P AGEN POL-AGENT SZCZECIN</CompanyName>
            <Country>
              <Code />
            </Country>
            <Postcode />
            <State>       AUSTRIA</State>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty</AddressType>
            <OrganizationCode>JOHGOOHUL</OrganizationCode>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ShippingLineAddress</AddressType>
            <AddressShortCode />
            <OrganizationCode>EURSHILMS</OrganizationCode>
            <Address1>ARCH.MAKARIOU III,</Address1>
            <Address2>4TH FLOOR 229 MELIZA COURT,</Address2>
            <AddressOverride>false</AddressOverride>
            <City>LIMASSOL</City>
            <CompanyName>EUROAFRICA SHIPPING LINES CYPRUS LIMITED</CompanyName>
            <Contact>.</Contact>
            <Country>
              <Code>CY</Code>
              <Name>Cyprus</Name>
            </Country>
            <Postcode>3105</Postcode>
            <State />
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>Principal</AddressType>
            <AddressShortCode />
            <OrganizationCode>EURSHILMS</OrganizationCode>
            <Address1>ARCH.MAKARIOU III,</Address1>
            <Address2>4TH FLOOR 229 MELIZA COURT,</Address2>
            <AddressOverride>false</AddressOverride>
            <City>LIMASSOL</City>
            <CompanyName>EUROAFRICA SHIPPING LINES CYPRUS LIMITED</CompanyName>
            <Contact>.</Contact>
            <Country>
              <Code>CY</Code>
              <Name>Cyprus</Name>
            </Country>
            <Port>
              <Code>CYLMS</Code>
              <Name>Limassol</Name>
            </Port>
            <Postcode>3105</Postcode>
            <State />
          </OrganizationAddress>
        </OrganizationAddressCollection>
        <PackingLineCollection>
          <PackingLine>
            <ContainerNumber>SBKU0003857</ContainerNumber>
            <DetailedDescription>APPLE JUICE</DetailedDescription>
            <GoodsDescription />
            <PackQty>1</PackQty>
            <PackType>
              <Code>VL</Code>
              <Description />
            </PackType>
            <ReferenceNumber />
            <Volume />
            <VolumeUnit>
              <Code />
              <Description />
            </VolumeUnit>
            <Weight>22900.000</Weight>
            <WeightUnit>
              <Code>KG</Code>
              <Description />
            </WeightUnit>
          </PackingLine>
        </PackingLineCollection>
      </Shipment>
    </UniversalShipment>";
		#endregion

		//#region Sample 6.2
		///// <summary>
		///// *****************************************
		///// 6.2: Consignment received from another airline
		///// *****************************************
		///// FSA/12
		///// 057-12345675IAHJED/T20
		///// RCT/TW/12NOV1900/CDG/T20 
		///// *****************************************
		///// </summary>
		//public void TestRunTask_CIMP_6_2_TransportNotFound()
		//{
		//  //Setup
		//  message.Content = LoadTestXml("6.2.xml");
		//  var consol = SetupConsolFor6_2_TransportNotFound();
		//  Factory.Save();

		//  //PreCondition
		//  var filter = new ZQuery(StmALogSchema.SL_SE_NKEvent, "TRF");
		//  var logs = consol.Logs.Find(filter);
		//  AssertEquals("Precondition: Should not have TRF log for Consol", 0, logs.Length);

		//  //Run Method
		//  target.RunTask();

		//  //Verify
		//  consol.Logs.Refresh();
		//  logs = consol.Logs.Find(filter);
		//  AssertEquals("One TRF log should be added to consol", 1, logs.Length);

		//  var log = logs[0];
		//  var processTask = ((IWorkflowProvider)consol).WorkflowItems.Triggers[0];

		//  AssertEquals("Should trigger process tasks for TRF", log.SL_EventTime, processTask.P9_ActualDate);

		//  var messageAfter = Factory.Load<IXmlEDIMessage>(message.PK);
		//  AssertEquals("Message Status should be changed to Received", EDIMessage.Status.Received, messageAfter.EM_Status);
		//}

		//ForwardingConsol SetupConsolFor6_2_TransportNotFound()
		//{
		//  var consol = PrepareConsol("05712345675");

		//  var workflowProvider = (IWorkflowProvider)consol;
		//  var processTask = workflowProvider.WorkflowItems.AddNew();
		//  processTask.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
		//  processTask.TriggerConditions.TriggerEventCode = "TRF";
		//  return consol;
		//}

		//#endregion

		//#region Sample 6.3

		//public void TestRunTask_CIMP_6_3()
		//{
		//  //Setup
		//  message.Content = LoadTestXml("6.3.xml");
		//  Factory.Save();

		//  //Pre Condition

		//  //Run Method
		//  target.RunTask();

		//  //Verify
		//  var messageAfter = Factory.Load<IXmlEDIMessage>(message.PK);
		//  AssertEquals("Message Status should be changed to Received", EDIMessage.Status.Received, messageAfter.EM_Status);
		//}

		//#endregion

		//#region Sample 6.4
		//public void TestRunTask_CIMP_6_4()
		//{
		//  //Setup
		//  message.Content = LoadTestXml("6.4.xml");
		//  Factory.Save();

		//  //Pre Condition

		//  //Run Method
		//  target.RunTask();

		//  //Verify
		//  var messageAfter = Factory.Load<IXmlEDIMessage>(message.PK);
		//  AssertEquals("Message Status should be changed to Received", EDIMessage.Status.Received, messageAfter.EM_Status);
		//}

		//#endregion

		//#region Sample 6.5

		//public void TestRunTask_CIMP_6_5()
		//{
		//  //Setup
		//  message.Content = LoadTestXml("6.5.xml");
		//  Factory.Save();

		//  //Pre Condition

		//  //Run Method
		//  target.RunTask();

		//  //Verify
		//  var messageAfter = Factory.Load<IXmlEDIMessage>(message.PK);
		//  AssertEquals("Message Status should be changed to Received", EDIMessage.Status.Received, messageAfter.EM_Status);
		//}

		//#endregion

		//#region Sample 6.6

		//public void TestRunTask_CIMP_6_6()
		//{
		//  //Setup
		//  message.Content = LoadTestXml("6.6.xml");
		//  Factory.Save();

		//  //Pre Condition

		//  //Run Method
		//  target.RunTask();

		//  //Verify
		//  var messageAfter = Factory.Load<IXmlEDIMessage>(message.PK);
		//  AssertEquals("Message Status should be changed to Received", EDIMessage.Status.Received, messageAfter.EM_Status);
		//}

		//#endregion

		//#region Sample 6.7

		//public void TestRunTask_CIMP_6_7()
		//{
		//  //Setup
		//  message.Content = LoadTestXml("6.7.xml");
		//  Factory.Save();

		//  //Pre Condition

		//  //Run Method
		//  target.RunTask();

		//  //Verify
		//  var messageAfter = Factory.Load<IXmlEDIMessage>(message.PK);
		//  AssertEquals("Message Status should be changed to Received", EDIMessage.Status.Received, messageAfter.EM_Status);
		//}

		//#endregion

		//#region Sample 6.8

		//public void TestRunTask_CIMP_6_8()
		//{
		//  //Setup
		//  message.Content = LoadTestXml("6.8.xml");
		//  var consol = SetupConsolData_6_8();
		//  Factory.Save();

		//  //Pre Condition
		//  var filter = new ZQuery(StmALogSchema.SL_SE_NKEvent, "TRF");
		//  var logs = consol.Logs.Find(filter);
		//  AssertEquals("Precondition: Should not have TRF log for Consol", 0, logs.Length);

		//  //Run Method
		//  target.RunTask();

		//  //Verify
		//  //TODO: Should handle IsPartial Message
		//  consol.Logs.Refresh();
		//  logs = consol.Logs.Find(filter);
		//  AssertEquals("One TRF log should be added to consol", 1, logs.Length);

		//  var log = logs[0];
		//  var processTask = ((IWorkflowProvider)consol).WorkflowItems.Triggers[0];

		//  AssertEquals("Should trigger process tasks for TRF", log.SL_EventTime, processTask.P9_ActualDate);

		//  var messageAfter = Factory.Load<IXmlEDIMessage>(message.PK);
		//  AssertEquals("Message Status should be changed to Received", EDIMessage.Status.Received, messageAfter.EM_Status);
		//}

		//ForwardingConsol SetupConsolData_6_8()
		//{
		//  var consol = PrepareConsol("05712345675");
		//  var workflowProvider = (IWorkflowProvider)consol;
		//  var processTask = workflowProvider.WorkflowItems.AddNew();
		//  processTask.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
		//  processTask.TriggerConditions.TriggerEventCode = "TRF";
		//  Factory.Save();
		//  return consol;
		//}

		//#endregion

		//#region Sample 6.9

		//public void TestRunTask_CIMP_6_9()
		//{
		//  //Setup
		//  message.Content = LoadTestXml("6.9.xml");
		//  var consol = SetupConsolData_6_9();
		//  Factory.Save();

		//  //Pre Condition
		//  var filter = new ZQuery(StmALogSchema.SL_SE_NKEvent, "TRF");
		//  var logs = consol.Logs.Find(filter);
		//  AssertEquals("Precondition: Should not have OCR log for Consol", 0, logs.Length);

		//  //Run Method
		//  target.RunTask();

		//  //Verify
		//  consol.Logs.Refresh();
		//  logs = consol.Logs.Find(filter);
		//  AssertEquals("One TRF log should be added to consol", 1, logs.Length);

		//  var log = logs[0];
		//  var processTask = ((IWorkflowProvider)consol).WorkflowItems.Triggers[0];

		//  AssertEquals("Should trigger process tasks for TRF", log.SL_EventTime, processTask.P9_ActualDate);

		//  var messageAfter = Factory.Load<IXmlEDIMessage>(message.PK);
		//  AssertEquals("Message Status should be changed to Received", EDIMessage.Status.Received, messageAfter.EM_Status);
		//}

		//ForwardingConsol SetupConsolData_6_9()
		//{
		//  var consol = PrepareConsol("02012345675");
		//  var workflowProvider = (IWorkflowProvider)consol;
		//  var processTask = workflowProvider.WorkflowItems.AddNew();
		//  processTask.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
		//  processTask.TriggerConditions.TriggerEventCode = "TRF";
		//  Factory.Save();
		//  return consol;
		//}

		//#endregion

		//#region Sample 6.10

		//public void TestRunTask_CIMP_6_10()
		//{
		//  //Setup
		//  message.Content = LoadTestXml("6.10.xml");
		//  Factory.Save();

		//  //Pre Condition

		//  //Run Method
		//  target.RunTask();

		//  //Verify
		//  var messageAfter = Factory.Load<IXmlEDIMessage>(message.PK);
		//  AssertEquals("Message Status should be changed to Received", EDIMessage.Status.Received, messageAfter.EM_Status);
		//}

		//#endregion

		//#region Sample 6.11

		//public void TestRunTask_CIMP_6_11()
		//{
		//  //Setup
		//  message.Content = LoadTestXml("6.11.xml");
		//  Factory.Save();

		//  //Pre Condition

		//  //Run Method
		//  target.RunTask();

		//  //Verify
		//  var messageAfter = Factory.Load<IXmlEDIMessage>(message.PK);
		//  AssertEquals("Message Status should be changed to Received", EDIMessage.Status.Received, messageAfter.EM_Status);
		//}

		//#endregion

		//#region Sample 6.12

		//public void TestRunTask_CIMP_6_12()
		//{
		//  //Setup
		//  message.Content = LoadTestXml("6.12.xml");
		//  Factory.Save();

		//  //Pre Condition

		//  //Run Method
		//  target.RunTask();

		//  //Verify
		//  var messageAfter = Factory.Load<IXmlEDIMessage>(message.PK);
		//  AssertEquals("Message Status should be changed to Received", EDIMessage.Status.Received, messageAfter.EM_Status);
		//}

		//#endregion

		//#region Sample 6.13

		//public void TestRunTask_CIMP_6_13()
		//{
		//  //Setup
		//  message.Content = LoadTestXml("6.13.xml");
		//  Factory.Save();

		//  //Pre Condition

		//  //Run Method
		//  target.RunTask();

		//  //Verify
		//  var messageAfter = Factory.Load<IXmlEDIMessage>(message.PK);
		//  AssertEquals("Message Status should be changed to Received", EDIMessage.Status.Received, messageAfter.EM_Status);
		//}

		//#endregion

		//#region Sample 6.14

		//public void TestRunTask_CIMP_6_14()
		//{
		//  //Setup
		//  message.Content = LoadTestXml("6.14.xml");
		//  Factory.Save();

		//  //Pre Condition

		//  //Run Method
		//  target.RunTask();

		//  //Verify
		//  var messageAfter = Factory.Load<IXmlEDIMessage>(message.PK);
		//  AssertEquals("Message Status should be changed to Received", EDIMessage.Status.Received, messageAfter.EM_Status);
		//}

		//#endregion

		//#region Sample 6.15-2

		//public void TestRunTask_CIMP_6_15_2()
		//{
		//  //Setup
		//  message.Content = LoadTestXml("6.15-2.xml");
		//  Factory.Save();

		//  //Pre Condition

		//  //Run Method
		//  target.RunTask();

		//  //Verify
		//  var messageAfter = Factory.Load<IXmlEDIMessage>(message.PK);
		//  AssertEquals("Message Status should be changed to Received", EDIMessage.Status.Received, messageAfter.EM_Status);
		//}

		//#endregion

		//#region Sample 6.16

		//public void TestRunTask_CIMP_6_16()
		//{
		//  //Setup
		//  message.Content = LoadTestXml("6.16.xml");
		//  Factory.Save();

		//  //Pre Condition

		//  //Run Method
		//  target.RunTask();

		//  //Verify
		//  var messageAfter = Factory.Load<IXmlEDIMessage>(message.PK);
		//  AssertEquals("Message Status should be changed to Received", EDIMessage.Status.Received, messageAfter.EM_Status);
		//}

		//#endregion

		//#region Sample 6.17-1

		//public void TestRunTask_CIMP_6_17_1()
		//{
		//  //Setup
		//  message.Content = LoadTestXml("6.17-1.xml");
		//  Factory.Save();

		//  //Pre Condition

		//  //Run Method
		//  target.RunTask();

		//  //Verify
		//  var messageAfter = Factory.Load<IXmlEDIMessage>(message.PK);
		//  AssertEquals("Message Status should be changed to Received", EDIMessage.Status.Received, messageAfter.EM_Status);
		//}

		//#endregion

		//#region Sample 6.17-2

		//public void TestRunTask_CIMP_6_17_2()
		//{
		//  //Setup
		//  message.Content = LoadTestXml("6.17-2.xml");
		//  Factory.Save();

		//  //Pre Condition

		//  //Run Method
		//  target.RunTask();

		//  //Verify
		//  var messageAfter = Factory.Load<IXmlEDIMessage>(message.PK);
		//  AssertEquals("Message Status should be changed to Received", EDIMessage.Status.Received, messageAfter.EM_Status);
		//}

		//#endregion

		//#region Sample 6.17-3

		//public void TestRunTask_CIMP_6_17_3()
		//{
		//  //Setup
		//  message.Content = LoadTestXml("6.17-3.xml");
		//  Factory.Save();

		//  //Pre Condition

		//  //Run Method
		//  target.RunTask();

		//  //Verify
		//  var messageAfter = Factory.Load<IXmlEDIMessage>(message.PK);
		//  AssertEquals("Message Status should be changed to Received", EDIMessage.Status.Received, messageAfter.EM_Status);
		//}

		//#endregion

		//#region Sample 6.18

		//public void TestRunTask_CIMP_6_18()
		//{
		//  //Setup
		//  message.Content = LoadTestXml("6.18.xml");
		//  Factory.Save();

		//  //Pre Condition

		//  //Run Method
		//  target.RunTask();

		//  //Verify
		//  var messageAfter = Factory.Load<IXmlEDIMessage>(message.PK);
		//  AssertEquals("Message Status should be changed to Received", EDIMessage.Status.Received, messageAfter.EM_Status);
		//}

		//#endregion

		//#region Sample 6.19-1

		//public void TestRunTask_CIMP_6_19_1()
		//{
		//  //Setup
		//  message.Content = LoadTestXml("6.19-1.xml");
		//  Factory.Save();

		//  //Pre Condition

		//  //Run Method
		//  target.RunTask();

		//  //Verify
		//  var messageAfter = Factory.Load<IXmlEDIMessage>(message.PK);
		//  AssertEquals("Message Status should be changed to Received", EDIMessage.Status.Received, messageAfter.EM_Status);
		//}

		//#endregion

		//#region Sample 6.20

		//public void TestRunTask_CIMP_6_20()
		//{
		//  //Setup
		//  message.Content = LoadTestXml("6.20.xml");
		//  Factory.Save();

		//  //Pre Condition

		//  //Run Method
		//  target.RunTask();

		//  //Verify
		//  var messageAfter = Factory.Load<IXmlEDIMessage>(message.PK);
		//  AssertEquals("Message Status should be changed to Received", EDIMessage.Status.Received, messageAfter.EM_Status);
		//}

		//#endregion

		//#region Sample 6.21

		//public void TestRunTask_CIMP_6_21()
		//{
		//  //Setup
		//  message.Content = LoadTestXml("6.21.xml");
		//  Factory.Save();

		//  //Pre Condition

		//  //Run Method
		//  target.RunTask();

		//  //Verify
		//  var messageAfter = Factory.Load<IXmlEDIMessage>(message.PK);
		//  AssertEquals("Message Status should be changed to Received", EDIMessage.Status.Received, messageAfter.EM_Status);
		//}

		//#endregion

		//#region Sample 6.22

		//public void TestRunTask_CIMP_6_22()
		//{
		//  //Setup
		//  message.Content = LoadTestXml("6.22.xml");
		//  Factory.Save();

		//  //Pre Condition

		//  //Run Method
		//  target.RunTask();

		//  //Verify
		//  var messageAfter = Factory.Load<IXmlEDIMessage>(message.PK);
		//  AssertEquals("Message Status should be changed to Received", EDIMessage.Status.Received, messageAfter.EM_Status);
		//}

		//#endregion

		//#region Sample 6.23

		//public void TestRunTask_CIMP_6_23()
		//{
		//  //Setup
		//  message.Content = LoadTestXml("6.23.xml");
		//  Factory.Save();

		//  //Pre Condition

		//  //Run Method
		//  target.RunTask();

		//  //Verify
		//  var messageAfter = Factory.Load<IXmlEDIMessage>(message.PK);
		//  AssertEquals("Message Status should be changed to Received", EDIMessage.Status.Received, messageAfter.EM_Status);
		//}

		//#endregion

		//#region Sample 6.24-1

		//public void TestRunTask_CIMP_6_24_1()
		//{
		//  //Setup
		//  message.Content = LoadTestXml("6.24-1.xml");
		//  Factory.Save();

		//  //Pre Condition

		//  //Run Method
		//  target.RunTask();

		//  //Verify
		//  var messageAfter = Factory.Load<IXmlEDIMessage>(message.PK);
		//  AssertEquals("Message Status should be changed to Received", EDIMessage.Status.Received, messageAfter.EM_Status);
		//}

		//#endregion

		//#region Sample 6.24-2

		//public void TestRunTask_CIMP_6_24_2()
		//{
		//  //Setup
		//  message.Content = LoadTestXml("6.24-2.xml");
		//  Factory.Save();

		//  //Pre Condition

		//  //Run Method
		//  target.RunTask();

		//  //Verify
		//  var messageAfter = Factory.Load<IXmlEDIMessage>(message.PK);
		//  AssertEquals("Message Status should be changed to Received", EDIMessage.Status.Received, messageAfter.EM_Status);
		//}

		//#endregion

		//#region Sample 6.25

		//public void TestRunTask_CIMP_6_25()
		//{
		//  //Setup
		//  message.Content = LoadTestXml("6.25.xml");
		//  Factory.Save();

		//  //Pre Condition

		//  //Run Method
		//  target.RunTask();

		//  //Verify
		//  var messageAfter = Factory.Load<IXmlEDIMessage>(message.PK);
		//  AssertEquals("Message Status should be changed to Received", EDIMessage.Status.Received, messageAfter.EM_Status);
		//}

		//#endregion

		//#region Sample 6.26

		//public void TestRunTask_CIMP_6_26()
		//{
		//  //Setup
		//  message.Content = LoadTestXml("6.26.xml");
		//  Factory.Save();

		//  //Pre Condition

		//  //Run Method
		//  target.RunTask();

		//  //Verify
		//  var messageAfter = Factory.Load<IXmlEDIMessage>(message.PK);
		//  AssertEquals("Message Status should be changed to Received", EDIMessage.Status.Received, messageAfter.EM_Status);
		//}

		//#endregion

		//#region Sample 6.27

		//public void TestRunTask_CIMP_6_27()
		//{
		//  //Setup
		//  message.Content = LoadTestXml("6.27.xml");
		//  Factory.Save();

		//  //Pre Condition

		//  //Run Method
		//  target.RunTask();

		//  //Verify
		//  var messageAfter = Factory.Load<IXmlEDIMessage>(message.PK);
		//  AssertEquals("Message Status should be changed to Received", EDIMessage.Status.Received, messageAfter.EM_Status);
		//}

		//#endregion

		//#region Sample 6.28

		//public void TestRunTask_CIMP_6_28()
		//{
		//  //Setup
		//  message.Content = LoadTestXml("6.28.xml");
		//  Factory.Save();

		//  //Pre Condition

		//  //Run Method
		//  target.RunTask();

		//  //Verify
		//  var messageAfter = Factory.Load<IXmlEDIMessage>(message.PK);
		//  AssertEquals("Message Status should be changed to Received", EDIMessage.Status.Received, messageAfter.EM_Status);
		//}

		//#endregion

		XmlEDIMessage SetupIncomingEdiMessageWithInterchangeFrom(string sampleFileName, string messageSubType)
		{
			var interchange = Factory.New<XmlEDIInterchange>();
			interchange.EI_ApplicationCode = EDIMessage.ApplicationCodes.UniversalDataMessaging;
			interchange.EI_InterchangeType = EDIMessageTypeList.Codes.XDC;
			interchange.EI_ReceiveTransmit = XmlEDIInterchange.Direction.Receive;
			interchange.EI_From = "EHUB";
			interchange.EI_To = "Dummy";

			var message = interchange.AddNeweHubMessage();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = messageSubType;
			message.EM_Status = XmlEDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = XmlEDIMessage.Direction.Receive;
			message.Content = LoadTestXml(sampleFileName);
			return (XmlEDIMessage)message;
		}

		XmlEDIMessage GetNewQueuedMessage(string messageSubType, string messageText)
		{
			var message = Factory.New<XmlEDIMessage>();
			message.EM_ApplicationCode = XmlEDIMessage.ApplicationCodes.UniversalDataMessaging;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = messageSubType;
			message.EM_MessageText = messageText.Trim();
			message.EM_Status = XmlEDIMessage.Status.Queued;
			return message;
		}

		BusinessObject PrepareConsol(string masterBillNum)
		{
			var consol = Factory.New(ObjectFactory.GetType<IForwardingConsol>());
			// JK_TransportMode will reset MasterBillNum
			consol[JobConsolSchema.JK_TransportMode] = Constants.TransportModes.Air;
			consol[JobConsolSchema.JK_MasterBillNum] = masterBillNum;
			return consol;
		}

		XElement LoadTestXml(string fileName)
		{
			var filePath = Path.Combine(SampleXMLFolder.Path, fileName);
			return XElement.Load(filePath);
		}

		(GlbGroup group, GlbStaff staff) CreateGroupWithUser(string userEmail)
		{
			var group = CreateGroup("SGR");
			var staff = CreateStaff("SUR", "Test user", userEmail);
			CreateGroupLink(group, staff);
			return (group, staff);
		}

		GlbGroup CreateGroup(string groupName)
		{
			var result = Factory.New<GlbGroup>();
			result.GG_Code = groupName;
			result.GG_Desc = "Call Me " + groupName;
			Factory.Save();
			return result;
		}

		GlbStaff CreateStaff(string code, string userName, string userEmail)
		{
			var result = Factory.New<GlbStaff>();
			result.GS_Code = code;
			result.GS_LoginName = userName;
			result.GS_IsSystemAccount = false;
			result.GS_EmailAddress = userEmail;
			Factory.Save();
			return result;
		}

		void CreateGroupLink(GlbGroup group, GlbStaff staff)
		{
			var result = Factory.New<GlbGroupLink>();
			result.GK_GG = group.PK;
			result.GK_GS = staff.PK;
			Factory.Save();
		}

		ZGuid GetFirstValidDepartmentForChargeCode(ZString chargeCode)
		{
			var code = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, chargeCode));
			var departmentCode = code?.AC_DepartmentFilterList.Split(",").FirstOrDefault() ?? ZString.Empty;
			var departmentQuery = departmentCode.IsEmpty || departmentCode == "ALL"
				? new ZQuery(GlbDepartmentSchema.GE_Misc, ZBool.False)
				: new ZQuery(GlbDepartmentSchema.GE_Code, departmentCode);

			return Factory.LoadTop1<GlbDepartment>(departmentQuery)?.PK ?? GlbDepartment.CurrentDepartment.PK;
		}

		void AssertNoRetryLeaveStatusQueuedOnException(Exception exception)
		{
			var message = GetNewQueuedMessage(EDIMessageSubTypeList.Codes.XmlUniversalEvent, @"
<UniversalEvent>
  <Event>
    <EventType>OCR</EventType>
    <EventTime>10-JUL-2010 18:00</EventTime>
    <EventReference>Dummy Description</EventReference>
    <DataProvider>Dummy</DataProvider>
    <ContextCollection>
      <Context>
        <Type>MAWBNumber</Type>
        <Value>1211234567</Value>
      </Context>
      <Context>
        <Type>HAWBNumber</Type>
        <Value>AAAA1003248</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>");
			message.EM_MessageNum = "00001";

			Factory.Save();

			var zSaveException = new ZSaveException(new ZDataException(exception, null, Db.Connection), Factory);

			var processor = new ProcessingManagerThatThrowsExceptionOnSave(new string[] { message.EM_MessageSubType }, zSaveException);
			AssertExceptionThrown(zSaveException.GetType(), () => processor.ExecuteBatch(CancellationToken.None));
			processor.Logger.Log(".");

			var reloadedMessage = new BusinessObjectFactory { RefreshEnabled = false }.Load<EDIMessage>(message.PK);
			AssertEquals("reloadedMessage.EM_Status", EDIMessage.Status.Queued, reloadedMessage.EM_Status);

			var log = string.Join("\r\n", processor.Logger.UserLogStrings.Cast<string>());
			AssertContains("Exception processing message #00001 individually", log);
		}
	}

	sealed class ProcessingManagerThatThrowsExceptionOnSave : IUniversalProcessingManager
	{
		readonly IUniversalProcessingManager universalProcessingManager;

		public ProcessingManagerThatThrowsExceptionOnSave(IEnumerable<string> messageSubTypes, Exception exceptionToThrow, GrEngineServiceSetting settings = GrEngineServiceSetting.Disabled)
		{
			universalProcessingManager = new UniversalProcessingManager(messageSubTypes, Enumerable.Empty<string>(), settings);
			var firstSave = true;
			exceptionThrownCount = 0;
			BusinessObjectFactory.SetOnFactorySaveHookForTest((thefactory) =>
			{
				if (thefactory.NameForDebugging == "Universal Message Processing" || thefactory.NameForDebugging == "Universal Message Logging")
				{
					if (!firstSave && exceptionToThrow != null)
					{
						firstSave = true;
						exceptionThrownCount++;
						throw exceptionToThrow;
					}
					else
					{
						firstSave = false;
					}
				}
			});
		}

		public int exceptionThrownCount { get; private set; }

		public XmlEDIGrEngine GrEngine => universalProcessingManager.GrEngine;

		public LoggingInformation Logger { get => universalProcessingManager.Logger; }

		public void ProcessBatch(BaseMessageProcessor<XmlEDIMessage>.DisposableBatch messages, CancellationToken token, FailedMessagesManager failedMessagesManager)
		{
			universalProcessingManager.ProcessBatch(messages, token, failedMessagesManager);
		}

		public void ExecuteBatch(CancellationToken token)
		{
			universalProcessingManager.ExecuteBatch(token);
		}

		public void FlipGrEngine()
		{
			universalProcessingManager.FlipGrEngine();
		}

		public void Dispose()
		{
			universalProcessingManager.Dispose();
		}
	}
}
