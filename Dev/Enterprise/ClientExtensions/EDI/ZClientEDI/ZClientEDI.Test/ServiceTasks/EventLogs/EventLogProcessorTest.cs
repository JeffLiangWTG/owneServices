using System;
using System.IO;
using System.Xml.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.Client.EDI.ServiceTasks.EventLogs;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Client.EDI.ServiceTask.EventLogs.Testing
{
	class EventLogProcessorTest : TestCaseWithFactory
	{
		[UseSnapshotProtection]
		[TestDate(2016, 03, 23, 0, 0, 0)]
		public void TestEndToEndProcessEvenLog_SaveDatabaseSuccess()
		{
			int beforeCount = Factory.GetDatabaseCount(typeof(EdiHelpErrorLog));
			using (var stream = new MemoryStream(new EmbeddedResourceRetriever(GetType().Assembly).GetBytes("ZClientEDI.Test.ServiceTasks.EventLogs.TestFiles.TestEventLogsInput1.txt")))
			{
				var eventLogProcessor = new EventLogProcessor(new LoggerForTest());
				EDIDataRegistry.Instance.EvenLogTraceLevel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Enterprise.Client.EDI.EDIDataRegistry.EvenLogTraceLevelOptions.CallStackOnly);
				var eventLogDataTransmissionHandler = new EventLogDataTransmissionHandler();
				eventLogProcessor.Process(XElement.Load(stream), "hostname", eventLogDataTransmissionHandler);
			}

			var query = new ZQuery(HelpErrorLogSchema.HE_ExceptionType, "TestingException.CargoWise");
			query.AddToFilter(HelpErrorLogSchema.HE_ExceptionSource, "ConsoleApplication1.exe");
			query.OrderBy = HelpErrorLogSchema.Constants.HE_LastReported + " desc";
			var log = Factory.LoadTop1<EdiHelpErrorLog>(query);
			AssertEquals("Correct Exception Message", new ZString("The process was terminated due to an unhandled exception."), log.HE_ExceptionMessage);
			AssertEquals(new ZDateTime(2014, 7, 8, 22, 27, 0), log.HE_LastReported);
			AssertEquals("Rows in HelpErrorLog", beforeCount + 1, Factory.GetDatabaseCount(typeof(EdiHelpErrorLog)));
		}

		public void TestUpdateListProvidersAndTheirCapacityCache()
		{
			EDIDataRegistry.Instance.EvenLogTraceLevel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Enterprise.Client.EDI.EDIDataRegistry.EvenLogTraceLevelOptions.All);
			var scalableHelpErrorLogCollection = new Mock<IHelpErrorLogCollection>();
			var eventLogProcessor = new Mock<EventLogProcessor>(new LoggerForTest());
			eventLogProcessor.Setup(m => m.ProcessExceptionXml(It.IsAny<string>(), It.IsAny<IHelpErrorLogCollection>())).Returns((EdiHelpErrorLog)null);
			eventLogProcessor.Setup(m => m.CreateNewScalableHelpErrorLogCollection()).Returns(scalableHelpErrorLogCollection.Object);
			var eventLogDataTransmissionHandler = new EventLogDataTransmissionHandler();
			using (var stream = new MemoryStream(new EmbeddedResourceRetriever(GetType().Assembly).GetBytes("ZClientEDI.Test.ServiceTasks.EventLogs.TestFiles.TestEventLogsInput1.txt")))
			{
				eventLogProcessor.Object.Process(XElement.Load(stream), "hostname", eventLogDataTransmissionHandler);
			}

			AssertEquals(3, eventLogDataTransmissionHandler.ListProvidersAndTheirCapacityLocalCache.Count);
		}
	}
}
