using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.EU.NCTS.DataTransfer.Testing
{
	class NctsEventParentFinderTests : TestCaseWithFactory
	{
		public void TestProcessUniveralEvent()
		{
			var decOne = Factory.New<NctsHeader>();
			var decTwo = Factory.New<NctsHeader>();
			var decThree = Factory.New<NctsHeader>();
			Factory.Save();
			decTwo.BH_JobReference = "NCT00000049";
			Factory.Save();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(testEventMessage);
			var logger = new TestErrorLogger();
			var subscriber = new NctsEventParentFinder(Factory, new NctsHeaderDataContextManager(), logger);
			var eventDataObject = xmlEvent as UniversalEvent;
			var headersFound = subscriber.GetLogParentsForEvent(eventDataObject);

			AssertEquals(1, headersFound.Length);
			AssertEquals(decTwo.PK, headersFound[0].PK);
		}

		const string testEventMessage = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>NctsHeader</Type>
          <Key>NCT00000049</Key>
        </DataTarget>
      </DataTargetCollection> 
      <EventType>
        <Code>CES</Code>
        <Description>Daniel Test</Description>
      </EventType>
    </DataContext>

    <EventTime>2020-08-26T13:31:10.62</EventTime>
    <EventType>CES</EventType>
    <IsEstimate>false</IsEstimate>
 	<EventReference>Yo mama so fat</EventReference>
  </Event>
</UniversalEvent>";
	}
}
