using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.Management.EventProcessing.Testing
{
	sealed class EventXmlDeserializerTest : TestCase
	{
		public void TestParseNullElement()
		{
			TextReader messageText = null;
			var valueObject = parser.Parse(messageText, new DummyLogger());
			AssertNotNull("parser should always return object", valueObject);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCanParseAllTestFilesWithoutExceptions()
		{
			var sampleFiles = new string[]
			{
				"6.1.xml", "6.2.xml", "6.3.xml", "6.4.xml", "6.5.xml", "6.6.xml", "6.7.xml", "6.8.xml", "6.9.xml", "6.10.xml",
				"6.11.xml", "6.12.xml", "6.13.xml", "6.14.xml", "6.15-1.xml", "6.15-2.xml", "6.16.xml", "6.17-1.xml", "6.17-2.xml", "6.17-3.xml", "6.18.xml", "6.19-1.xml", "6.19-2.xml",
				"6.20.xml", "6.21.xml", "6.22.xml", "6.23.xml", "6.24-1.xml", "6.24-2.xml", "6.25.xml", "6.26.xml", "6.27.xml", "6.28.xml",
			};

			CombineAssertions(delegate
			{
				foreach (var sampleFile in sampleFiles)
				{
					using (var stream = File.OpenRead(Path.Combine(SampleXMLFolder.Path, sampleFile)))
					{
						AssertNoExceptionThrown("Parsing: " + sampleFile, () => ((SubStreamableStream)stream).Parse<Event>());
					}
				}
			});
		}

		public void TestParseBasicEvent()
		{
			var messageText = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <EventType>OCR</EventType>
    <EventTime>2010-07-10T18:00:00</EventTime>
    <EventReference>DummyDescription</EventReference>
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
    </ContextCollection>
  </Event>
</UniversalEvent>
".Trim();

			using (var messageReader = new StringReader(messageText))
			{
				var valueObject = parser.Parse(messageReader, new DummyLogger());

				AssertNotNull("Should not be null when xml is valid", valueObject);
				CombineAssertions(delegate
				{
					AssertEquals("Event Type should be OCR", "OCR", valueObject.EventType);
					AssertEquals("Event Description should be 'DummyDescription'", "DummyDescription", valueObject.EventReference);
					AssertEquals("Event Type should be the same", new ZDateTimeOffset(2010, 07, 10, 18, 00, 00), valueObject.EventTime);
					AssertEquals("Should have two context item", 2, valueObject.Context.Values.Count());
					AssertEquals("Context.MAWBNumber", "020-12345675", valueObject.Context.Values.FirstOrDefault(o => o.Key.Type == "MAWBNumber").Value);
					AssertEquals("Context.MAWBOriginIATAAirportCode", "JFK", valueObject.Context.Values.FirstOrDefault(o => o.Key.Type == "MAWBOriginIATAAirportCode").Value);
				});
			}
		}

		public void TestParseOnMoreCompleteObject()
		{
			var messageText = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <EventType>ILB</EventType>
    <EventReference>palma non sine pulvere</EventReference>
    <EventTime>2010-09-08T07:00:00</EventTime>
    <CreatedTime>2010-09-08T06:05:00</CreatedTime>
    <DataProvider>MyProvider</DataProvider>
    <IsEstimate>true</IsEstimate>
    <IsPartial>false</IsPartial>
    <IsCancelled>true</IsCancelled>
    <ContextCollection>
      <Context>
        <Type>MAWBNumber</Type>
        <Value>123-12345678</Value>
      </Context>
      <Context>
        <Type>MAWBOriginIATAAirportCode</Type>
        <Value>JFK</Value>
      </Context>
      <Context>
        <Type>MAWBDestinationIATAAirportCode</Type>
        <Value>LAX</Value>
      </Context>
      <Context>
        <Type>MAWBNumberOfPieces</Type>
        <Value>3</Value>
      </Context>
      <Context>
        <Type>OtherServiceInformation</Type>
        <Value>Cause we can</Value>
      </Context>
      <Context>
        <Type>MessageIsPartial</Type>
        <Value>true</Value>
      </Context>
      <Context>
        <Type>MessageNumberOfPieces</Type>
        <Value>42</Value>
      </Context>
      <Context>
        <Type>MessageWeightOfGoods</Type>
        <Value>123 KG</Value>
      </Context>
      <Context>
        <Type>IATACarrierCode</Type>
        <Value>QF</Value>
      </Context>
      <Context>
        <Type>IATAAirportCode</Type>
        <Value>ABQ</Value>
      </Context>
      <Context>
        <Type>NumberOfPieces</Type>
        <Value>66</Value>
      </Context>
      <Context>
        <Type>PackageType</Type>
        <Value>PLT</Value>
      </Context>
      <Context>
        <Type>WeightOfGoods</Type>
        <Value>565 LB</Value>
      </Context>
      <Context>
        <Type>ReceivedFromName</Type>
        <Value>Fred Nerks</Value>
      </Context>
      <Context>
        <Type>VolumeOfGoods</Type>
        <Value>4.2 M3</Value>
      </Context>
      <Context>
        <Type>DensityGroup</Type>
        <Value>12</Value>
      </Context>
      <Context>
        <Type>FlightNumber</Type>
        <Value>QF123</Value>
      </Context>
      <Context>
        <Type>FlightDate</Type>
        <Value>2010-07-02</Value>
      </Context>
      <Context>
        <Type>TimeOfDeparture</Type>
        <Value>2010-07-02T01:02:03</Value>
      </Context>
      <Context>
        <Type>TimeOfArrival</Type>
        <Value>2010-07-03T04:05:06</Value>
      </Context>
      <Context>
        <Type>OriginIATAAirportCode</Type>
        <Value>ABC</Value>
      </Context>
      <Context>
        <Type>DestinationIATAAirportCode</Type>
        <Value>XYZ</Value>
      </Context>
      <Context>
        <Type>RecipientName</Type>
        <Value>Johnny Lightfoot</Value>
      </Context>
      <Context>
        <Type>DiscrepancyCode</Type>
        <Value>FRQ</Value>
      </Context>
      <Context>
        <Type>ULDIdentification</Type>
        <Value>AA212N</Value>
      </Context>
      <Context>
        <Type>ULDIdentification</Type>
        <Value>AA215N</Value>
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
".Trim();

			var xmlEvent = parser.Parse(messageText);

			AssertNotNull("Should not be null when xml is valid", xmlEvent);

			CombineAssertions(delegate
			{
				AssertEquals("xmlEvent.EventType", new ZString("ILB"), xmlEvent.EventType);
				AssertEquals("xmlEvent.EventDescription", new ZString("palma non sine pulvere"), xmlEvent.EventReference);
				AssertEquals("xmlEvent.EventTime", new ZDateTimeOffset(2010, 9, 8, 7, 0, 0), xmlEvent.EventTime);
				AssertEquals("xmlEvent.CreatedTime", new ZDateTimeOffset(2010, 9, 8, 6, 5, 0), xmlEvent.CreatedTime);
				AssertEquals("xmlEvent.IsEstimate", ZBool.True, xmlEvent.IsEstimate);
				AssertEquals("xmlEvent.IsCancelled", ZBool.True, xmlEvent.IsCancelled);
			});

			CombineAssertions(delegate
			{
				AssertEquals("xmlEvent.Context.MAWBNumber", new ZString("123-12345678"), xmlEvent.Context.MAWBNumber);
				AssertEquals("xmlEvent.Context.MAWBOriginIATAAirportCode", new ZString("JFK"), xmlEvent.Context.MAWBOriginIATAAirportCode);
				AssertEquals("xmlEvent.Context.MAWBDestinationIATAAirportCode", new ZString("LAX"), xmlEvent.Context.MAWBDestinationIATAAirportCode);
				AssertEquals("xmlEvent.Context.MAWBNumberOfPieces", new ZInt(3), xmlEvent.Context.MAWBNumberOfPieces);
				AssertEquals("xmlEvent.Context.OtherServiceInformation", new ZString("Cause we can"), xmlEvent.Context.OtherServiceInformation);
				AssertEquals("xmlEvent.Context.MessageIsPartial", new ZBool(true), xmlEvent.Context.MessageIsPartial);
				AssertEquals("xmlEvent.Context.MessageNumberOfPieces", new ZInt(42), xmlEvent.Context.MessageNumberOfPieces);
				AssertEquals("xmlEvent.Context.MessageWeightOfGoods", new ZString("123 KG"), xmlEvent.Context.MessageWeightOfGoods);
				AssertEquals("xmlEvent.Context.IATACarrierCode", new ZString("QF"), xmlEvent.Context.IATACarrierCode);
				AssertEquals("xmlEvent.Context.IATAAirportCode", new ZString("ABQ"), xmlEvent.Context.IATAAirportCode);
				AssertEquals("xmlEvent.Context.NumberOfPieces", new ZInt(66), xmlEvent.Context.NumberOfPieces);
				AssertEquals("xmlEvent.Context.PackageType", new ZString("PLT"), xmlEvent.Context.PackageType);
				AssertEquals("xmlEvent.Context.WeightOfGoods", new ZString("565 LB"), xmlEvent.Context.WeightOfGoods);
				AssertEquals("xmlEvent.Context.ReceivedFromName", new ZString("Fred Nerks"), xmlEvent.Context.ReceivedFromName);
				AssertEquals("xmlEvent.Context.VolumeOfGoods", new ZString("4.2 M3"), xmlEvent.Context.VolumeOfGoods);
				AssertEquals("xmlEvent.Context.DensityGroup", new ZString("12"), xmlEvent.Context.DensityGroup);
				AssertEquals("xmlEvent.Context.FlightNumber", new ZString("QF123"), xmlEvent.Context.FlightNumber);
				AssertEquals("xmlEvent.Context.FlightDate", new ZDateTime(2010, 7, 2), xmlEvent.Context.FlightDate);
				AssertEquals("xmlEvent.Context.TimeOfDeparture", new ZDateTime(2010, 7, 2, 1, 2, 3), xmlEvent.Context.TimeOfDeparture);
				AssertEquals("xmlEvent.Context.TimeOfArrival", new ZDateTime(2010, 7, 3, 4, 5, 6), xmlEvent.Context.TimeOfArrival);
				AssertEquals("xmlEvent.Context.OriginIATAAirportCode", new ZString("ABC"), xmlEvent.Context.OriginIATAAirportCode);
				AssertEquals("xmlEvent.Context.DestinationIATAAirportCode", new ZString("XYZ"), xmlEvent.Context.DestinationIATAAirportCode);
				AssertEquals("xmlEvent.Context.RecipientName", new ZString("Johnny Lightfoot"), xmlEvent.Context.RecipientName);
				AssertEquals("xmlEvent.Context.DiscrepancyCode", new ZString("FRQ"), xmlEvent.Context.DiscrepancyCode);
				AssertContainsExactElementsInAnyOrder("xmlEvent.Context.ULDIdentifications", new List<ZString> { new ZString("AA212N"), new ZString("AA215N") }, xmlEvent.Context.ULDIdentifications);
				AssertEquals("AdditionalReference", new ZString("134FREGT"), xmlEvent.Context.Values.Single(reference => reference.Key.Type == "AMS" && reference.Key.Description == "AMS Number").Value);
				AssertEquals("AdditionalReferenceWithNoDescription", new ZString("267AIRGT"), xmlEvent.Context.Values.Single(reference => reference.Key.Type == "COC").Value);
			});
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			parser = new XmlEventDeserializer();
		}
		IXmlEventDeserializer parser;

		#endregion
	}
}

