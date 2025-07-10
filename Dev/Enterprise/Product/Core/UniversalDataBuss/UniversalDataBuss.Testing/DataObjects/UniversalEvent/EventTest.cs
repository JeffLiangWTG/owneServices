using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	[TestedType(typeof(Event))]
	sealed class EventTest : TopLevelDataObjectTestCase<Event>
	{
		public void TestObjectWritesFineThroughXmlWriter_2011_11()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				var eventData = GetSampleEvent();

				using (var stream = (SubStreamableStream)new MemoryStream())
				{
					var xmlWriter = ObjectFactory.Get<IXmlWriter>();
					xmlWriter.WriteXML(eventData, stream);

					using (var reader = new StreamReader(stream))
					{
						var result = reader.ReadToEnd();
						var expectedXML = resourceRetriever.Value.GetString("Enterprise.UniversalDataBuss.Testing.DataObjects.UniversalEvent.UniversalEventSample_2011_11.xml");
						AssertMultilineASCIIEquals("Serialized UniversalEvent", expectedXML, result);
					}
				}
			}
		}

		public void TestObjectWritesFineThroughXmlWriter_2012_11()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				using (var eventData = GetSampleEventWithAttachedDocuments())
				{
					using (var stream = (SubStreamableStream)new MemoryStream())
					{
						var xmlWriter = ObjectFactory.Get<IXmlWriter>();
						xmlWriter.WriteXML(eventData, stream);

						using (var reader = new StreamReader(stream))
						{
							var result = reader.ReadToEnd();
							var expectedXML = resourceRetriever.Value.GetString("Enterprise.UniversalDataBuss.Testing.DataObjects.UniversalEvent.UniversalEventSample_2012_11.xml");
							AssertMultilineASCIIEquals("Serialized UniversalEvent", expectedXML, result);
						}
					}
				}
			}
		}

		public void TestObjectWritesFineThroughXmlWriter_ValidationRuleCollection()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				using (var eventData = GetSampleEvent())
				{
					eventData.ValidationRuleCollection = new List<ValidationRule> { new ValidationRule { Code = "R001", Sequence = 1, MessageLog = "DingDing", Result = "WARNING", } };
					using (var stream = (SubStreamableStream)new MemoryStream())
					{
						var xmlWriter = ObjectFactory.Get<IXmlWriter>();
						xmlWriter.WriteXML(eventData, stream);

						using (var reader = new StreamReader(stream))
						{
							var result = reader.ReadToEnd();
							var expectedXML = resourceRetriever.Value.GetString("Enterprise.UniversalDataBuss.Testing.DataObjects.UniversalEvent.UniversalEventSample_ValidationRuleCollection.xml");
							AssertMultilineASCIIEquals("Serialized UniversalEvent", expectedXML, result);
						}
					}
				}
			}
		}

		static Event GetSampleEventWithAttachedDocuments()
		{
			var eventData = GetSampleEvent();
			eventData.AttachedDocumentCollection = new List<AttachedDocument>();
			var attachedDocument = new AttachedDocument();
			var type = new DocumentType();
			type.Code = "CAD";
			type.Description = "Cartage Advice";
			attachedDocument.Type = type;
			attachedDocument.FileName = "TestCartageAdviceFile";
			attachedDocument.IsPublished = true;
			attachedDocument.ImageData = (SubStreamableStream)new MemoryStream(new byte[] { 1, 2, 3 });
			eventData.AttachedDocumentCollection.Add(attachedDocument);

			attachedDocument = new AttachedDocument();
			type = new DocumentType();
			type.Code = "MSC";
			type.Description = "Miscellaneous Document";
			attachedDocument.Type = type;
			attachedDocument.FileName = "TestMiscellaneousFile";
			attachedDocument.IsPublished = false;
			attachedDocument.ImageData = (SubStreamableStream)new MemoryStream(new byte[] { 4, 5, 6 });
			eventData.AttachedDocumentCollection.Add(attachedDocument);

			return eventData;
		}

		static Event GetSampleEvent()
		{
			var eventData = new Event();
			eventData.EventTime = new ZDateTimeOffset(2010, 2, 15, 18, 0, 0);
			eventData.EventType = "ATH";
			eventData.EventReference = "Open Job";

			var dataSource = DataContextFactory.New();
			eventData.DataContext = dataSource;
			dataSource.AddDataSource(DataContextType.ForwardingShipment, "S00001010");
			dataSource.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataSource.SetWorkflowInfo(new WorkflowInfo()
			{
				EventType = new CodeDescriptionPair() { Code = "ATH", Description = "Action Authorized" },
				EventUser = null,
				EventBranch = null,
				EventDepartment = null,
				ActionPurpose = new CodeDescriptionPair() { Code = "APP", Description = "As Per Payload" },
				TriggerDescription = "Job Started",
				TriggerCount = 1,
				TriggerDate = new ZDateTimeOffset(2010, 2, 12),
				TriggerReference = "Open Job",
				TriggerType = TriggerType.Trigger,
				RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.FOR } }
			});

			eventData.ContextCollection = new List<Context>
			{
				new Context { Type = "MAWBNumber", Value = "125-22230003" },
				new Context { Type = "MAWBOriginIATAAirportCode", Value = "BHX" },
				new Context { Type = "MAWBDestinationIATAAirportCode", Value = "TUL" },
				new Context { Type = "MAWBNumberOfPieces", Value = "13" },
				new Context { Type = "NativeEventCode", Value = "RCS" },
				new Context { Type = "NumberOfPieces", Value = "13" },
				new Context { Type = "IATAAirportCode", Value = "BHX" },
				new Context { Type = "ReceivedFromName", Value = "SHIPPERNAME" }
			};

			eventData.AdditionalContextCollection = new List<AdditionalContext>
			{
				new AdditionalContext
				{
					DataContext = DataContextFactory.New(),
					ContextCollection = new List<Context> { new Context { Type = "OrderTrackingNumber", Value = "1" } }
				},
				new AdditionalContext
				{
					DataContext = DataContextFactory.New(),
					ContextCollection = new List<Context> { new Context { Type = "OrderTrackingNumber", Value = "2" } }
				}
			};
			eventData.AdditionalContextCollection[0].DataContext.AddDataSource(DataContextType.eManifestLine, "1~1");
			eventData.AdditionalContextCollection[1].DataContext.AddDataSource(DataContextType.eManifestLine, "1~2");

			eventData.AdditionalFieldsToUpdateCollection = new List<AdditionalFieldToUpdate>();
			eventData.AdditionalFieldsToUpdateCollection.Add(new AdditionalFieldToUpdate() { Type = "JobShipment.JS_Something", Value = "Gimme a Reason" });
			eventData.AdditionalFieldsToUpdateCollection.Add(new AdditionalFieldToUpdate() { Type = "JobShipment.JS_SomethingElse", Value = "2010-02-01T00:00:00" });
			return eventData;
		}

		public void TestUniversalEventReadsFineThroughXmlReader_2012_11()
		{
			TestObjectReadsFineThroughXmlReader("Enterprise.UniversalDataBuss.Testing.DataObjects.UniversalEvent.UniversalEventSample_2012_11.xml");
		}

		public void TestUniversalEventReadsFineThroughXmlReader_2011_11()
		{
			TestObjectReadsFineThroughXmlReader("Enterprise.UniversalDataBuss.Testing.DataObjects.UniversalEvent.UniversalEventSample_2011_11.xml");
		}

		public void TestUniversalEventReadsFineThroughXmlReader_Unversioned()
		{
			TestObjectReadsFineThroughXmlReader("Enterprise.UniversalDataBuss.Testing.DataObjects.UniversalEvent.UniversalEventSample_UnversionedNamespace.xml");
		}

		public void TestUniversalEventReadsFineThroughXmlReader_TimeZone()
		{
			TestObjectReadsFineThroughXmlReader("Enterprise.UniversalDataBuss.Testing.DataObjects.UniversalEvent.UniversalEventSample_UsingTimeZone.xml");
		}

		public void TestUniversalEvent_MasterHouseBillIsCorrectlyPopulated()
		{
			var xml = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
      </Company>
    </DataContext>

    <EventTime>2013-03-20T09:49:21.9</EventTime>
    <EventType>CAD</EventType>

    <ContextCollection>
      <Context>
        <Type>MAWBNumber</Type>
        <Value>08112302135</Value>
      </Context>
      <Context>
        <Type>MasterHouseBill</Type>
        <Value>MHB</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";
			using (var stream = (SubStreamableStream)new MemoryStream(Encoding.ASCII.GetBytes(xml)))
			{
				var xmlReader = ObjectFactory.Get<IXmlReader>();
				var eventData = new Event();
				xmlReader.ReadXML(eventData, stream, new TestErrorLogger());
				IXmlEventValueObject eventValueObject = eventData;
				var context = eventValueObject.Context;
				AssertEquals(true, context.MasterHouseBill.HasValue);
				AssertEquals("MHB", context.MasterHouseBill.Value);
			}

			xml = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
      </Company>
    </DataContext>

    <EventTime>2013-03-20T09:49:21.9</EventTime>
    <EventType>CAD</EventType>

    <ContextCollection>
      <Context>
        <Type>MAWBNumber</Type>
        <Value>08112302135</Value>
      </Context>
      <Context>
        <Type>MasterHouseBill</Type>
        <Value></Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";
			using (var stream = (SubStreamableStream)new MemoryStream(Encoding.ASCII.GetBytes(xml)))
			{
				var xmlReader = ObjectFactory.Get<IXmlReader>();
				var eventData = new Event();
				xmlReader.ReadXML(eventData, stream, new TestErrorLogger());
				IXmlEventValueObject eventValueObject = eventData;
				var context = eventValueObject.Context;
				AssertEquals(true, context.MasterHouseBill.HasValue);
				AssertEquals("", context.MasterHouseBill.Value);
			}

			xml = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
      </Company>
    </DataContext>

    <EventTime>2013-03-20T09:49:21.9</EventTime>
    <EventType>CAD</EventType>

    <ContextCollection>
      <Context>
        <Type>MAWBNumber</Type>
        <Value>08112302135</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";
			using (var stream = (SubStreamableStream)new MemoryStream(Encoding.ASCII.GetBytes(xml)))
			{
				var xmlReader = ObjectFactory.Get<IXmlReader>();
				var eventData = new Event();
				xmlReader.ReadXML(eventData, stream, new TestErrorLogger());
				IXmlEventValueObject eventValueObject = eventData;
				var context = eventValueObject.Context;
				AssertEquals(false, context.MasterHouseBill.HasValue);
			}
		}

		void TestObjectReadsFineThroughXmlReader(string embeddedResourceName)
		{
			using (var embeddedResourceStream = resourceRetriever.Value.GetStream(embeddedResourceName))
			using (var stream = new SubStreamableStream(embeddedResourceStream, EmptyDisposableLeakListener.Instance))
			{
				var xmlReader = ObjectFactory.Get<IXmlReader>();
				var logger = new TestErrorLogger();
				using (var eventData = new Event())
				{
					xmlReader.ReadXML(eventData, stream, logger);

					CombineAssertions(delegate
					{
						AssertEquals("eventData.EventTime", new ZDateTimeOffset(2010, 2, 15, 18, 0, 0), eventData.EventTime);
						AssertEquals("eventData.EventType", "ATH", eventData.EventType);
						AssertEquals("eventData.EventReference", "Open Job", eventData.EventReference);
						AssertEquals("eventData.CreatedTime", null, eventData.CreatedTime);
						AssertEquals("eventData.IsEstimate", null, eventData.IsEstimate);
						AssertEquals("eventData.IsCancelled", null, eventData.IsCancelled);

						AssertNotNull("eventData.ContextCollection", eventData.ContextCollection);
						if (eventData.ContextCollection != null)
						{
							AssertMultilineASCIIEquals("eventData.ContextCollection", @"
MAWBNumber - 125-22230003
MAWBOriginIATAAirportCode - BHX
MAWBDestinationIATAAirportCode - TUL
MAWBNumberOfPieces - 13
NativeEventCode - RCS
NumberOfPieces - 13
IATAAirportCode - BHX
ReceivedFromName - SHIPPERNAME
".Trim(), string.Join("\r\n", eventData.ContextCollection.Select(o => o.Type.Type + " - " + o.Value).ToArray()));
						}

						AssertNotNull("eventData.AdditionalContextCollection", eventData.AdditionalContextCollection);
						if (eventData.AdditionalContextCollection != null)
						{
							AssertMultilineASCIIEquals("eventData.AdditionalContextCollection", @"
eManifestLine - 1~1 : OrderTrackingNumber - 1
eManifestLine - 1~2 : OrderTrackingNumber - 2
".Trim(), string.Join("\r\n", eventData.AdditionalContextCollection.Select(o => o.DataContext.DataSourceCollection.Single().Type + " - " + o.DataContext.DataSourceCollection.Single().Key + " : " + o.ContextCollection.Single().Type + " - " + o.ContextCollection.Single().Value)));
						}

						AssertNotNull("eventData.AdditionalFieldsToUpdateCollection", eventData.AdditionalFieldsToUpdateCollection);
						if (eventData.AdditionalFieldsToUpdateCollection != null)
						{
							AssertMultilineASCIIEquals("eventData.AdditionalFieldsToUpdateCollection", @"
JobShipment.JS_Something - Gimme a Reason
JobShipment.JS_SomethingElse - 2010-02-01T00:00:00
".Trim(), string.Join("\r\n", eventData.AdditionalFieldsToUpdateCollection.Select(o => { return o.Type + " - " + o.Value; }).ToArray()));
						}

						if (eventData.AttachedDocumentCollection != null)
						{
							AssertMultilineASCIIEquals("eventData.AttachedDocumentCollection", @"
CAD - Cartage Advice - TestCartageAdviceFile - Y
MSC - Miscellaneous Document - TestMiscellaneousFile - N
".Trim(), string.Join("\r\n", eventData.AttachedDocumentCollection.Select(doc => { return doc.Type.Code + " - " + doc.Type.Description + " - " + doc.FileName + " - " + doc.IsPublished; }).ToArray()));
							AssertArrayEqualsByElements(new byte[] { 1, 2, 3 }, eventData.AttachedDocumentCollection[0].ImageData.ConvertToByteArrayAndCloseStream());
							AssertArrayEqualsByElements(new byte[] { 4, 5, 6 }, eventData.AttachedDocumentCollection[1].ImageData.ConvertToByteArrayAndCloseStream());
						}
					});

					AssertMultilineASCIIEquals("logger.Logs", @"".Trim(), logger.Logs);
				}
			}
		}

		public void TestUniversalEvent_ContainerCollectionCorrectlyPopulated()
		{
			var xml = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
      </Company>
    </DataContext>

    <EventTime>2013-03-20T09:49:21.9</EventTime>
    <EventType>CAD</EventType>

    <ContextCollection>
      <Context>
        <Type>ContainerNumber</Type>
        <Value>1</Value>
      </Context>
      <Context>
        <Type>ContainerNumber</Type>
        <Value>2</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";
			using (var stream = (SubStreamableStream)new MemoryStream(Encoding.ASCII.GetBytes(xml)))
			{
				var xmlReader = ObjectFactory.Get<IXmlReader>();
				var eventData = new Event();
				xmlReader.ReadXML(eventData, stream, new TestErrorLogger());
				IXmlEventValueObject eventValueObject = eventData;
				var context = eventValueObject.Context;
				var containerNumbers = context.ContainerNumbers;
				AssertContainsExactElementsInAnyOrder(new ZString[] { "1", "2" }, containerNumbers);
			}
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

