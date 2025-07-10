using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(MessageValueObject))]
	sealed class MessageValueObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetContextList()
		{
			var message = Factory.New<IXmlEDIMessage>();
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message.Content = XElement.Parse(@"
			<UniversalEvent>
				<Event>
				  <EventType>OCR</EventType>
				  <EventTime>10-JUL-2010 18:00</EventTime>
				  <EventDescription>Dummy Description</EventDescription>
				  <DataProvider>Dummy</DataProvider>
				  <ContextCollection>
					<Context>
					  <Type>MAWBNumber</Type>
					  <Value>020-12345675</Value>
					</Context>
					<Context>
					  <Type>MAWBOriginIATAAirportCode</Type>
					  <Value>JFK</Value>
					</Context>
					<Context>
					  <Type>MAWBDestinationIATAAirportCode</Type>
					  <Value>BKK</Value>
					</Context>
					<Context>
					  <Type>MAWBNumberOfPieces</Type>
					  <Value>20</Value>
					</Context>
					<Context>
					  <Type>NativeEventCode</Type>
					  <Value>RCS</Value>
					</Context>
					<Context>
					  <Type>NumberOfPieces</Type>
					  <Value>20</Value>
					</Context>
					<Context>
					  <Type>IATAAirportCode</Type>
					  <Value>JFK</Value>
					</Context>
					<Context>
					  <Type>ReceivedFromName</Type>
					  <Value>SHIPPERNAME</Value>
					</Context>
          <Context>
            <Type Description=""AMS Number"">AMS</Type>
            <Value>134FREGT</Value>
          </Context>
          <Context>
            <Type Description="""">COC</Type>
            <Value>267AIRGT</Value>
          </Context>
				  </ContextCollection>
				</Event>
			</UniversalEvent>
				".Trim());
			var valueObject = new MessageValueObject(message);
			AssertNotNull("Content List should not be null", valueObject.ContextList);
			AssertEquals("valueObject.ContextList.MAWBNumber", "020-12345675", valueObject.ContextList.MAWBNumber);
			AssertEquals("AdditionalReferenceNumber", "134FREGT", valueObject.ContextList.Values.FirstOrDefault(reference => reference.Key.Description == "AMS Number").Value);
			AssertEquals("AdditionalReferenceNumberWithEmptyDescription", "267AIRGT", valueObject.ContextList.Values.FirstOrDefault(reference => reference.Key.Type == "COC").Value);
		}

		public void TestGetDataContextAndOtherContextValues()
		{
			var message = Factory.New<IXmlEDIMessage>();
			var interchange = Factory.New<IEDIInterchange>();
			interchange.EI_From = "God";
			interchange.EI_To = "All those developers";

			((BusinessObject)message)[EDIMessageSchema.EM_EI] = interchange.PK;
			message.EM_MessageType = "XDC";
			message.EM_MessageSubType = "XUS";
			message.Content = XElement.Parse(@"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <Shipment>
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
      <EventUser>
        <Code>GOD</Code>
        <Name>The One And Only God</Name>
      </EventUser>
      <ServerID>GOD</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2011-04-21T12:06:00</TriggerDate>
      <TriggerDescription>Answer To All</TriggerDescription>
      <TriggerReference>The Father</TriggerReference>
      <TriggerType>Trigger</TriggerType>
    </DataContext>

    <WayBillNumber>ONMYWAY</WayBillNumber>
    <WayBillType>
      <Code>MWB</Code>
      <Description>Master Waybill</Description>
    </WayBillType>
  </Shipment>
</UniversalShipment>
".Trim());

			var valueObject = new MessageValueObject(message);

			AssertEquals("valueObject.Sender", "God", valueObject.Sender);
			AssertEquals("valueObject.Recipient", "All those developers", valueObject.Recipient);
			AssertEquals("valueObject.Message.MessageSubTypeWithDescription", "XUS - XML Universal Shipment", valueObject.Message.MessageSubTypeWithDescription);

			var dataContext = valueObject.DataContext;
			AssertNotNull("Data Context should not be null", dataContext);

			CombineAssertions(delegate
			{
				AssertPairExists(dataContext, "Data Source Action Purpose", "MFA - Message From Above");
				AssertPairExists(dataContext, "Data Source Company", "HVN - Heaven");
				AssertPairExists(dataContext, "Data Source Enterprise ID", "GOD");
				AssertPairExists(dataContext, "Data Source Server ID", "GOD");
				AssertPairExists(dataContext, "Data Source Trigger Count", "1");
				AssertPairExists(dataContext, "Data Source Trigger Date", "21-Apr-11 12:06:00 +10:00");
				AssertPairExists(dataContext, "Data Source Trigger Description", "Answer To All");
				AssertPairExists(dataContext, "Data Source Trigger Event", "DIV - Divine");
				AssertPairExists(dataContext, "Data Source Trigger Event User", "GOD - The One And Only God");
				AssertPairExists(dataContext, "Data Source Trigger Reference", "The Father");
				AssertPairExists(dataContext, "Data Source Trigger Type", "Trigger");
			});
		}

		static void AssertPairExists(IDataContextDataObject dataContext, ZString pairName, ZString pairValue)
		{
			var pair = dataContext.ContextKeyValuePairs.FirstOrDefault(o => o.Key == pairName);
			AssertNotNull("Pair should exist for Name: " + pairName, pair);
			AssertEquals("Pair value for Name: " + pairName, pairValue, pair.Value);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var message = Factory.New<IXmlEDIMessage>();
			return new MessageValueObject(message);
		}

		#endregion
	}
}
