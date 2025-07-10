using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.ElectronicPayment.Universal;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using ProviderCodes = Enterprise.MasterFiles.Business.EPaymentProviderCodes.Codes;

namespace Enterprise.Accounting.ElectronicPayment.Testing.Universal
{
	public class AccEPaymentDealEventParentFinderTest : TestCaseWithFactory
	{
		public void TestGetLogParents_WrongXMLFormat()
		{
			var xml = string.Format(XMLBaseFormat, InvalidXMLFormat, ZString.Empty, nameof(DataContextType.AccEPaymentDeal), ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			var objects = ProcessEventXml(xml, out var logger);
			AssertEquals("Error - Unsupported XML format encountered: Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11.DataContext.", logger.ToString());
			AssertNull("Expect no deal is found", objects);
		}

		public void TestGetLogParents_NoProviderCode()
		{
			var xml = string.Format(XMLBaseFormat, ValidXMLFormat, ZString.Empty, nameof(DataContextType.AccEPaymentDeal), Events.InterchangeAcknowledgedCode, ZString.Empty, Events.StatusUpdatedCode, ZString.Empty);
			var objects = ProcessEventXml(xml, out var logger);
			AssertEquals("Error - Could not find a valid Provider Code in the Event's <MessageType> parameter. Value found: ''", logger.ToString());
			AssertNull("Expect no deal is found", objects);
		}

		public void TestGetLogParents_NoDealFound()
		{
			var xml = string.Format(XMLBaseFormat, ValidXMLFormat, ZString.Empty, nameof(DataContextType.AccEPaymentDeal), Events.InterchangeAcknowledgedCode, ProviderCodes.OFX, Events.StatusUpdatedCode, ZString.Empty);
			var objects = ProcessEventXml(xml, out var logger);
			AssertEquals("Error - Could not find a valid Deal Reference in the <DataTarget> Key parameter. Value found: ''", logger.ToString());
			AssertNull("Expect no deal is found", objects);
		}

		public void TestGetLogParents_NoContextCollection()
		{
			var xml = string.Format(XMLBaseFormat, ValidXMLFormat, "00001000", nameof(DataContextType.AccEPaymentDeal), Events.InterchangeAcknowledgedCode, ProviderCodes.OFX, Events.StatusUpdatedCode, ZString.Empty);
			var objects = ProcessEventXml(xml, out var logger);
			AssertEquals("Error - No Context Collection found.", logger.ToString());
			AssertNull("Expect no deal is found", objects);
		}

		public void TestGetLogParents()
		{
			var xml = string.Format(XMLBaseFormat, ValidXMLFormat, "00001000", nameof(DataContextType.AccEPaymentDeal), Events.InterchangeAcknowledgedCode, ProviderCodes.OFX, Events.StatusUpdatedCode, ContextCollection);

			var correctDeal = Factory.NewWithValidTestData<AccEPaymentDeal>();
			correctDeal.AED_InternalReference = "00001000";
			correctDeal.AED_Status = EPaymentStatusCodes.Deal.InProgress;
			correctDeal.AED_LastResponseReceivedUtc = ZDateTime.UtcNow;
			correctDeal.AED_ProviderReference = "C55F5AA6-B7EA-4B05-BC2B-3EC51D52DF9D";

			var wrongRefDeal = Factory.NewWithValidTestData<AccEPaymentDeal>();
			wrongRefDeal.AED_InternalReference = "00001111";
			wrongRefDeal.AED_Status = EPaymentStatusCodes.Deal.InProgress;
			wrongRefDeal.AED_LastResponseReceivedUtc = ZDateTime.UtcNow;
			wrongRefDeal.AED_ProviderReference = "040D07EC-10B8-42C5-B781-C1B7DDCF7A2A";

			var otherCompany = TestObjectCreator.CreateNewCompany("NEW");
			var wrongCompanyDeal = Factory.NewWithValidTestData<AccEPaymentDeal>();
			wrongCompanyDeal.AED_InternalReference = "00001000";
			wrongCompanyDeal.AED_Status = EPaymentStatusCodes.Deal.InProgress;
			wrongCompanyDeal.AED_LastResponseReceivedUtc = ZDateTime.UtcNow;
			wrongCompanyDeal.AED_ProviderReference = "08EDB83F-5437-4E81-AB97-2E20A9B5D6CD";
			wrongCompanyDeal.AED_GC_Company = otherCompany.PK;

			Factory.Save();

			var objects = ProcessEventXml(xml, out var logger);
			AssertEquals(ZString.Empty, logger.ToString());
			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { correctDeal }, objects);
		}

		readonly string ValidXMLFormat = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">";
		readonly string InvalidXMLFormat = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11""> ";
		readonly string ContextCollection = $@"<ContextCollection>
           <Context>             
             <Type>StatusCode</Type>
            <Value>Paid</Value>
          </Context> 
           <Context>             
             <Type>CompanyCode</Type>
            <Value>{Env.CurrentCompany.Code}</Value>
          </Context>
        </ContextCollection>";

		readonly ZString XMLBaseFormat = @"
{0}
      <Event> 
        <DataContext> 
          <DataTargetCollection> 
            <DataTarget> 
              <Key>{1}</Key> 
              <Type>{2}</Type> 
            </DataTarget> 
          </DataTargetCollection> 
        </DataContext> 
        <EventTime>2021-02-10T07:27:21</EventTime> 
        <EventType>{3}</EventType> 
        <EventParameters> 
		  <MessageType>{4}</MessageType>
		  <MessageSubType>{5}</MessageSubType>
        </EventParameters> 
        {6}
      </Event> 
    </UniversalEvent>";

		BusinessObject[] ProcessEventXml(string eventXmlMessage, out XmlSessionTracker logger)
		{
			logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var subscriber = new AccEPaymentDealEventParentFinder(Factory, new AccEPaymentDealDataContextManager(), logger);

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlMessage);
			return subscriber.GetLogParentsForEvent(xmlEvent);
		}

		protected TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
