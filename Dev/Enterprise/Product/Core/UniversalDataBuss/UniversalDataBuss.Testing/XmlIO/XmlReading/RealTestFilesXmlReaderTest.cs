using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.Testing.XmlIO.XmlReading
{
	sealed class RealTestFilesXmlReaderTest : TestCaseWithFactory
	{
		public void TestCanReadUniversalShipmentWithSpacesInAttributesAndCharacterSetPrefix()
		{
			var logger = new TestErrorLogger();
			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			using (var embeddedResourceStream = resourceRetriever.Value.GetStream("Enterprise.UniversalDataBuss.Testing.XmlIO.XmlReading.RealTestFiles.UniversalShipmentWithSpacesInAttributesAndCharacterSetPrefix.xml"))
			using (var stream = new SubStreamableStream(embeddedResourceStream, EmptyDisposableLeakListener.Instance))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(universalShipment, stream, logger);
			}

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("logger.Errors", "", string.Join("\r\n", logger.Errors.ToArray()));
				AssertMultilineASCIIEquals("logger.Warnings", "", string.Join("\r\n", logger.Warnings.ToArray()));
				AssertEquals("LocalTransportJobType.Code", "DRFC", universalShipment.LocalTransportJobType.Code);
			});
		}

		public void TestCanReadWithNamespacePrefixes()
		{
			var logger = new TestErrorLogger();
			var universalEvent = new Event();

			using (var embeddedResourceStream = resourceRetriever.Value.GetStream("Enterprise.UniversalDataBuss.Testing.XmlIO.XmlReading.RealTestFiles.UniversalEventWithNamespacePrefixes.xml"))
			using (var stream = new SubStreamableStream(embeddedResourceStream, EmptyDisposableLeakListener.Instance))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(universalEvent, stream, logger);
			}

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("logger.Errors", "", string.Join("\r\n", logger.Errors.ToArray()));
				AssertMultilineASCIIEquals("logger.Warnings", "", string.Join("\r\n", logger.Warnings.ToArray()));
				AssertEquals("universalEvent.EventReference", "Location: YUSTERMIN CA", universalEvent.EventReference);
				AssertEquals("universalEvent.EventType", "FUL", universalEvent.EventType);
				AssertEquals("universalEvent.EventTime", new ZDateTimeOffset(2011, 7, 8), universalEvent.EventTime);
				AssertNotEquals("Data Target", null,
					universalEvent.DataContext.DataTargetCollection.FirstOrDefault(dataTarget => dataTarget.Key.Value == "C00001091" && dataTarget.Type.Value == "ForwardingConsol"));

				AssertNotEquals("ContainerNumber", null, universalEvent.ContextCollection.Find(contextItem => contextItem.Type.Type.Value == "ContainerNumber" && contextItem.Value.Value == "CVHU123456"));
				AssertNotEquals("SightingDateTime", null, universalEvent.ContextCollection.Find(contextItem => contextItem.Type.Type.Value == "SightingDateTime" && contextItem.Value.Value == "201107080115"));
				AssertNotEquals("SightingEventCode", null, universalEvent.ContextCollection.Find(contextItem => contextItem.Type.Type.Value == "SightingEventCode" && contextItem.Value.Value == "W"));
				AssertNotEquals("SightingCity", null, universalEvent.ContextCollection.Find(contextItem => contextItem.Type.Type.Value == "SightingCity" && contextItem.Value.Value == "YUSTERMIN"));
				AssertNotEquals("SightingState", null, universalEvent.ContextCollection.Find(contextItem => contextItem.Type.Type.Value == "SightingState" && contextItem.Value.Value == "CA"));
				AssertNotEquals("SightingSPLC", null, universalEvent.ContextCollection.Find(contextItem => contextItem.Type.Type.Value == "SightingSPLC" && contextItem.Value.Value == "883215000"));
				AssertNotEquals("LoadEmptyStatus", null, universalEvent.ContextCollection.Find(contextItem => contextItem.Type.Type.Value == "LoadEmptyStatus" && contextItem.Value.Value == "L"));
				AssertNotEquals("SentDateHour", null, universalEvent.ContextCollection.Find(contextItem => contextItem.Type.Type.Value == "SentDateHour" && contextItem.Value.Value == "2011071922"));
				AssertNotEquals("HeaderRailroadSCAC", null, universalEvent.ContextCollection.Find(contextItem => contextItem.Type.Type.Value == "HeaderRailroadSCAC" && contextItem.Value.Value == "RRDC"));
				AssertNotEquals("ReportingRailroadSCAC", null, universalEvent.ContextCollection.Find(contextItem => contextItem.Type.Type.Value == "ReportingRailroadSCAC" && contextItem.Value.Value == "UP"));
				AssertNotEquals("AEIReidInd", null, universalEvent.ContextCollection.Find(contextItem => contextItem.Type.Type.Value == "AEIReidInd" && contextItem.Value.Value == ""));
				AssertNotEquals("DestinationCity", null, universalEvent.ContextCollection.Find(contextItem => contextItem.Type.Type.Value == "DestinationCity" && contextItem.Value.Value == "AUSTELL"));
				AssertNotEquals("DestinationState", null, universalEvent.ContextCollection.Find(contextItem => contextItem.Type.Type.Value == "DestinationState" && contextItem.Value.Value == "GA"));
				AssertNotEquals("DestinationSPLC", null, universalEvent.ContextCollection.Find(contextItem => contextItem.Type.Type.Value == "DestinationSPLC" && contextItem.Value.Value == "456495000"));
				AssertNotEquals("ETADestinationCity", null, universalEvent.ContextCollection.Find(contextItem => contextItem.Type.Type.Value == "ETADestinationCity" && contextItem.Value.Value == "AUSTELL"));
				AssertNotEquals("ETADestinationState", null, universalEvent.ContextCollection.Find(contextItem => contextItem.Type.Type.Value == "ETADestinationState" && contextItem.Value.Value == "GA"));
				AssertNotEquals("ETADestinationSPLC", null, universalEvent.ContextCollection.Find(contextItem => contextItem.Type.Type.Value == "ETADestinationSPLC" && contextItem.Value.Value == "456495000"));
				AssertNotEquals("ETADestinationEventCode", null, universalEvent.ContextCollection.Find(contextItem => contextItem.Type.Type.Value == "ETADestinationEventCode" && contextItem.Value.Value == "V"));
				AssertNotEquals("ETADateHour", null, universalEvent.ContextCollection.Find(contextItem => contextItem.Type.Type.Value == "ETADateHour" && contextItem.Value.Value == "2011072000"));
			});
		}

		public void TestCanReadRealShipmentFile()
		{
			var logger = new TestErrorLogger();
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			using (var embeddedResourceStream = resourceRetriever.Value.GetStream("Enterprise.UniversalDataBuss.Testing.XmlIO.XmlReading.RealTestFiles.ShipmentSample1.xml"))
			using (var stream = new SubStreamableStream(embeddedResourceStream, EmptyDisposableLeakListener.Instance))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
			}

			AssertMultilineASCIIEquals("logger.Errors", "", string.Join("\r\n", logger.Errors.ToArray()));
			AssertMultilineASCIIEquals("logger.Warnings", "", string.Join("\r\n", logger.Warnings.ToArray()));
		}

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		Lazy<EmbeddedResourceRetriever> resourceRetriever;
	}
}
