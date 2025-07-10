using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.UniversalDataBuss.Management.EventProcessing.Testing
{
	class DataContextXmlDeserializerTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestDeserializingDataContextDoesntBarfWhenItHitsTheRegistryInsideARead()
		{
			var message = GetQueuedUniversalShipmentMessage(MessageWithDataContextIn2012_11Namespace);
			Factory.SaveForTesting();

			var reloadedMessage = new BusinessObjectFactory().Load<IEDIMessage>(message.PK);

			var deserializer = ObjectFactory.GetDesignerSafe<IXmlDataContextDeserializer>();

			using (var messageTextReader = reloadedMessage.GetEM_MessageTextReader())
			{
				AssertNoExceptionThrown(() => { deserializer.Parse(messageTextReader, new DummyLogger()); });
			}
		}

		public void TestDataContextDeserializationWith2012_11Namespace()
		{
			var shipmentMessage = GetQueuedUniversalShipmentMessage(MessageWithDataContextIn2012_11Namespace);
			var reader = shipmentMessage.GetEM_MessageTextReader();
			var logger = new TestErrorLogger();
			var deserializer = ObjectFactory.GetDesignerSafe<IXmlDataContextDeserializer>();
			var dataContext = deserializer.Parse(reader, logger);

			AssertNotNull(dataContext);
			CombineAssertions(delegate
			{
				AssertEquals("logger.Logs", "", logger.Logs);
				AssertPairExists(dataContext, "Data Source Action Purpose", "MFA - Message From Above");
				AssertPairExists(dataContext, "Data Source Company", "HVN - Heaven");
				AssertPairExists(dataContext, "Data Source Data Provider", "GODGODHVN (EnterpriseID)");
				AssertPairExists(dataContext, "Data Source Trigger Event", "DIV - Divine");
				AssertPairExists(dataContext, "Data Source Trigger Count", "1");
				AssertPairExists(dataContext, "Data Source Trigger Date", "21-Apr-11 12:06:00 +10:00");
				AssertPairExists(dataContext, "Data Source Trigger Description", "Answer To All");
				AssertPairExists(dataContext, "Data Source Trigger Reference", "The Father");
				AssertPairExists(dataContext, "Data Source Trigger Type", "Trigger");
				AssertEquals("dataContext.CodesMappedToTarget", false, dataContext.CodesMappedToTarget);
			});

			var concreteDataContext = dataContext as DataObjects.Universal._2012_11.DataContext;
			AssertNotNull("dataContext should be a _2012_11.DataContext", concreteDataContext);

			AssertEquals("concreteDataContext.Workflow.TriggerCount", 1, concreteDataContext.Workflow.TriggerCount);
			AssertEquals("concreteDataContext.Workflow.TriggerDate", new ZDateTimeOffset(2011, 4, 21, 12, 6, 0), concreteDataContext.Workflow.TriggerDate);
			AssertEquals("concreteDataContext.Workflow.TriggerDescription", "Answer To All", concreteDataContext.Workflow.TriggerDescription);
			AssertEquals("concreteDataContext.Workflow.TriggerReference", "The Father", concreteDataContext.Workflow.TriggerReference);
			AssertEquals("concreteDataContext.Workflow.TriggerType", TriggerType.Trigger, concreteDataContext.Workflow.TriggerType);
		}

		public void TestDataContextDeserializationWith2011_11Namespace()
		{
			var shipmentMessage = GetQueuedUniversalShipmentMessage(string.Format(MessageWithDataContextPre2012_11Namespace, "http://www.cargowise.com/Schemas/Universal/2011/11"));
			var reader = shipmentMessage.GetEM_MessageTextReader();
			var logger = new TestErrorLogger();
			var deserializer = ObjectFactory.GetDesignerSafe<IXmlDataContextDeserializer>();
			var dataContext = deserializer.Parse(reader, logger);
			AssertDataContextContents(logger, dataContext);

			var concreteDataContext = dataContext as DataObjects.Universal._2011_11.DataContext;
			AssertNotNull("dataContext should be a _2011_11.DataContext", concreteDataContext);

			AssertEquals("concreteDataContext.TriggerCount", 1, concreteDataContext.TriggerCount);
			AssertEquals("concreteDataContext.TriggerDate", new ZDateTimeOffset(2011, 4, 21, 12, 6, 0), concreteDataContext.TriggerDate);
			AssertEquals("concreteDataContext.TriggerDescription", "Answer To All", concreteDataContext.TriggerDescription);
			AssertEquals("concreteDataContext.TriggerReference", "The Father", concreteDataContext.TriggerReference);
			AssertEquals("concreteDataContext.TriggerType", TriggerType.Trigger, concreteDataContext.TriggerType);
		}

		public void TestDataContextDeserializationWithNamespaceWithoutDateSuffix()
		{
			var shipmentMessage = GetQueuedUniversalShipmentMessage(string.Format(MessageWithDataContextPre2012_11Namespace, "http://www.cargowise.com/Schemas/Universal"));
			var reader = shipmentMessage.GetEM_MessageTextReader();
			var logger = new TestErrorLogger();
			var deserializer = ObjectFactory.GetDesignerSafe<IXmlDataContextDeserializer>();
			var dataContext = deserializer.Parse(reader, logger);
			AssertDataContextContents(logger, dataContext);
			Assert("dataContext should be a _2011_11.DataContext", dataContext is DataObjects.Universal._2011_11.DataContext);
		}

		public void TestDataContextDeserializationWithRandomNamespace()
		{
			var shipmentMessage = GetQueuedUniversalShipmentMessage(string.Format(MessageWithDataContextPre2012_11Namespace, "http://wegottaqgetoutofthisplace/if/its/the/last/thing/we/ever/do"));
			var reader = shipmentMessage.GetEM_MessageTextReader();
			var logger = new TestErrorLogger();
			var deserializer = ObjectFactory.GetDesignerSafe<IXmlDataContextDeserializer>();
			var dataContext = deserializer.Parse(reader, logger);
			AssertDataContextContents(logger, dataContext);
			Assert("dataContext should be a _2011_11.DataContext", dataContext is DataObjects.Universal._2011_11.DataContext);
		}

		public void TestDataContextDeserializationWithEmptyNamespace()
		{
			var shipmentMessage = GetQueuedUniversalShipmentMessage(string.Format(MessageWithDataContextPre2012_11Namespace, ""));
			var reader = shipmentMessage.GetEM_MessageTextReader();
			var logger = new TestErrorLogger();
			var deserializer = ObjectFactory.GetDesignerSafe<IXmlDataContextDeserializer>();
			var dataContext = deserializer.Parse(reader, logger);
			AssertDataContextContents(logger, dataContext);
			Assert("dataContext should be a _2011_11.DataContext", dataContext is DataObjects.Universal._2011_11.DataContext);
		}

		static void AssertDataContextContents(TestErrorLogger logger, IDataContextDataObject dataContext)
		{
			AssertNotNull(dataContext);
			CombineAssertions(delegate
			{
				AssertEquals("logger.Logs", "", logger.Logs);
				AssertPairExists(dataContext, "Data Source Action Purpose", "MFA - Message From Above");
				AssertPairExists(dataContext, "Data Source Company", "HVN - Heaven");
				AssertPairExists(dataContext, "Data Source Enterprise ID", "GOD");
				AssertPairExists(dataContext, "Data Source Trigger Event", "DIV - Divine");
				AssertPairExists(dataContext, "Data Source Server ID", "GOD");
				AssertPairExists(dataContext, "Data Source Trigger Count", "1");
				AssertPairExists(dataContext, "Data Source Trigger Date", "21-Apr-11 12:06:00 +10:00");
				AssertPairExists(dataContext, "Data Source Trigger Description", "Answer To All");
				AssertPairExists(dataContext, "Data Source Trigger Reference", "The Father");
				AssertPairExists(dataContext, "Data Source Trigger Type", "Trigger");
				AssertEquals("dataContext.CodesMappedToTarget", false, dataContext.CodesMappedToTarget);
			});
		}

		static void AssertPairExists(IDataContextDataObject dataContext, ZString pairName, ZString pairValue)
		{
			var pair = dataContext.ContextKeyValuePairs.FirstOrDefault(o => o.Key == pairName);
			AssertNotNull("Pair should exist for Name: " + pairName, pair);
			if (pair != null)
			{
				AssertEquals("Pair value for Name: " + pairName, pairValue, pair.Value);
			}
		}

		#region const string MessageWithDataContextPre2012_11Namespace

		const string MessageWithDataContextPre2012_11Namespace = @"<?xml version=""1.0"" encoding=""utf-8""?>
<FooBarShipment xmlns=""{0}"" version=""1.1"">
  <ShipThat>

I can feel a <<XXXX>> coming on.
Will that work?
It has to be properly formatted before...
No, No, now that I think about it anything should be fine.

		<DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingPrayer</Type>
          <Key>P00001043</Key>
        </DataSource>
      </DataSourceCollection>

      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingPrayer</Type>
          <Key>P00001043</Key>
        </DataTarget>
      </DataTargetCollection>

      <ActionPurpose>
        <Code>MFA</Code>
        <Description>Message From Above</Description>
      </ActionPurpose>
      <Company>
        <Code>HVN</Code>
        <Name>Heaven</Name>
      </Company>
      <EnterpriseID>GOD</EnterpriseID>
      <EventType>
        <Code>DIV</Code>
        <Description>Divine</Description>
      </EventType>
      <ServerID>GOD</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2011-04-21T12:06:00</TriggerDate>
      <TriggerDescription>Answer To All</TriggerDescription>
      <TriggerReference>The Father</TriggerReference>
      <TriggerType>Trigger</TriggerType>
    </DataContext>

    <InvalidXML>
    <WayBillNumber>ONM>>SLS<<D
<F><F><F>F<E>
  </ShipThat>
</FooBarShipment>";

		#endregion

		#region const string MessageWithDataContextIn2012_11Namespace

		const string MessageWithDataContextIn2012_11Namespace = @"<?xml version=""1.0"" encoding=""utf-8""?>
<FooBarShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <ShipThat>

I can feel a <<XXXX>> coming on.
Will that work?
It has to be properly formatted before...
No, No, now that I think about it anything should be fine.

		<DataContext>
      <DataSource>
        <Type>ForwardingPrayer</Type>
        <Key>P00001043</Key>
				<DataProvider Type=""EnterpriseID"">GODGODHVN</DataProvider>
      </DataSource>

      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingPrayer</Type>
          <Key>P00001043</Key>
        </DataTarget>
      </DataTargetCollection>

			<Workflow>
				<ActionPurpose Description=""Message From Above"">MFA</ActionPurpose>
				<Company>
					<Code>HVN</Code>
					<Name>Heaven</Name>
				</Company>
				<EventType Description=""Divine"">DIV</EventType>
				<TriggerCount>1</TriggerCount>
				<TriggerDate>2011-04-21T12:06:00</TriggerDate>
				<TriggerDescription>Answer To All</TriggerDescription>
				<TriggerReference>The Father</TriggerReference>
				<TriggerType>Trigger</TriggerType>
			</Workflow>
    </DataContext>

    <InvalidXML>
    <WayBillNumber>ONM>>SLS<<D
<F><F><F>F<E>
  </ShipThat>
</FooBarShipment>";

		#endregion
	}
}
