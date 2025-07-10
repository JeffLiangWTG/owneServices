using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.eHub.Common.Extensions;
using CargoWise.IO;
using Enterprise.Client.EDI.ServiceTasks.EventLogs;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Client.EDI.ServiceTask.EventLogs.Testing
{
	class EventLogRetrieverTest : TransactionedTestCase
	{
		public void TestEndToEnd_CannotReturnXMLByWEVTUTIL()
		{
			string machinename = "Wrong-Name-Or-Offline-Server";
			var retriever = new EventLogRetriever();
			var logger = new Mock<ILogger>();
			var result = retriever.Retrieve(machinename, new EventLogDataTransmissionHandler(), logger.Object);
			AssertNull("Expect return null when there is invalid machine", result);
		}

		public void TestReturnCorrectXElementRoot()
		{
			using (var stream = new MemoryStream(new EmbeddedResourceRetriever(GetType().Assembly).GetBytes("ZClientEDI.Test.ServiceTasks.EventLogs.TestFiles.TestEventLogsInput1.txt")))
			{
				var machinename = System.Environment.MachineName;
				var retriever = new Mock<EventLogRetriever>() { CallBase = true };
				retriever.Setup(x => x.RetreiveEventLogs(It.IsAny<string>(), It.IsAny<ILogger>(), It.IsAny<System.Diagnostics.Process>()))
					.Returns(new StringBuilder(stream.ReadToEnd()));
				var logger = new Mock<ILogger>();
				var result = retriever.Object.Retrieve(machinename, new EventLogDataTransmissionHandler(), logger.Object);
				AssertEquals("Right root element of XML file", "EventLogs", result.Name.ToString());
				AssertNotEquals("Good information in XML retriever", string.Empty, result.Value);
			}
		}

		public void TestGetUpdatedRetriever_HighWaterMark()
		{
			EDIDataRegistry.Instance.EvenLogTraceLevel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, EDIDataRegistry.EvenLogTraceLevelOptions.CallStackOnly);
			using (var stream1 = new MemoryStream(new EmbeddedResourceRetriever(GetType().Assembly).GetBytes("ZClientEDI.Test.ServiceTasks.EventLogs.TestFiles.TestEventsInput.txt")))
			using (var stream2 = new MemoryStream(new EmbeddedResourceRetriever(GetType().Assembly).GetBytes("ZClientEDI.Test.ServiceTasks.EventLogs.TestFiles.TestEventsInput1.txt")))
			{
				var machinename = System.Environment.MachineName;
				var eventLogDataTransmissionHandler = new EventLogDataTransmissionHandler();
				var retriever = new Mock<EventLogRetriever>() { CallBase = true };
				var logger = new Mock<ILogger>();
				retriever.Setup(m => m.RetreiveEventLogs(It.IsAny<string>(), It.IsAny<ILogger>(), It.IsAny<System.Diagnostics.Process>()))
					.Returns(new StringBuilder(stream1.ReadToEnd()));
				var result = retriever.Object.Retrieve(machinename, eventLogDataTransmissionHandler, logger.Object);
				AssertEquals("Right root element of XML file", "EventLogs", result.Name.ToString());
				AssertNotEquals("Good information in XML retriever", string.Empty, result.Value);
				retriever.VerifyAll();
				logger.VerifyAll();
				XNamespace xmlPath = "http://schemas.microsoft.com/win/2004/08/events/event";
				string lastEventRecordID = result.Element(xmlPath + "Event").Element(xmlPath + "System").Element(xmlPath + "EventRecordID").Value;
				RegistryMachinesHandler.Sync(string.Format("{0}+{1}", machinename, lastEventRecordID));
				eventLogDataTransmissionHandler.SyncEventLogHighWaterMarkRegistryAndListProvidersAndTheirCapacityRegistry();
				retriever.Setup(m => m.RetreiveEventLogs(It.IsAny<string>(), It.IsAny<ILogger>(), It.IsAny<System.Diagnostics.Process>()))
					.Returns((StringBuilder)null);
				result = retriever.Object.Retrieve(machinename, eventLogDataTransmissionHandler, logger.Object);
				AssertNull("Should return empty because of highWaterMark", result);
				AssertEquals("3966032", eventLogDataTransmissionHandler.FindHighWaterMark(machinename));
				retriever.VerifyAll();
				logger.VerifyAll();
				// Create new EventLogDataTransmissionHandler to try to load new registry HighWaterMark value
				retriever.Setup(m => m.RetreiveEventLogs(It.IsAny<string>(), It.IsAny<ILogger>(), It.IsAny<System.Diagnostics.Process>()))
					.Returns(new StringBuilder(stream2.ReadToEnd()));
				eventLogDataTransmissionHandler = new EventLogDataTransmissionHandler();
				result = retriever.Object.Retrieve(machinename, eventLogDataTransmissionHandler, logger.Object);
				AssertEquals("Should have four XElements return", 4, result.Elements().Count());
				AssertEquals("Should collect CallStackOnly and return 3966032 instead of 4087442", "3966032", eventLogDataTransmissionHandler.FindHighWaterMark(machinename));
				retriever.VerifyAll();
				logger.VerifyAll();
			}
		}
	}
}
