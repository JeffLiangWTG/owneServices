using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.ElectronicPayment.Universal;
using Enterprise.Environment;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using ProviderCodes = Enterprise.MasterFiles.Business.EPaymentProviderCodes.Codes;

namespace Enterprise.Accounting.ElectronicPayment.Testing.Universal
{
	public class AccEPaymentQuoteEventParentFinderTest : TestCaseWithFactory
	{
		public void TestGetLogParents_WrongXMLFormat()
		{
			var xml = string.Format(XMLBaseFormat, InvalidXMLFormat, ZString.Empty, nameof(DataContextType.AccEPaymentQuote), ZString.Empty, ZString.Empty, ZString.Empty);
			var objects = ProcessEventXml(xml, out var logger);
			AssertEquals("Error - Unsupported XML format encountered: Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11.DataContext.", logger.ToString());
			AssertNull("Expect no quote is found", objects);
		}

		public void TestGetLogParents_NoProviderCode()
		{
			var xml = string.Format(XMLBaseFormat, ValidXMLFormat, ZString.Empty, nameof(DataContextType.AccEPaymentQuote), Events.InterchangeAcknowledgedCode, ZString.Empty, ZString.Empty);
			var objects = ProcessEventXml(xml, out var logger);
			AssertEquals("Error - Could not find a valid Provider Code in the Event's <MessageType> parameter. Value found: ''", logger.ToString());
			AssertNull("Expect no quote is found", objects);
		}

		public void TestGetLogParents_NoQuoteFound()
		{
			var xml = string.Format(XMLBaseFormat, ValidXMLFormat, ZString.Empty, nameof(DataContextType.AccEPaymentQuote), Events.InterchangeAcknowledgedCode, ProviderCodes.OFX, ZString.Empty);
			var objects = ProcessEventXml(xml, out var logger);
			AssertEquals("Error - Could not find a valid Quote Number in the <DataTarget> Key parameter. Value found: ''", logger.ToString());
			AssertNull("Expect no quote is found", objects);
		}

		public void TestGetLogParents_NoContextCollection()
		{
			var xml = string.Format(XMLBaseFormat, ValidXMLFormat, "00001000", nameof(DataContextType.AccEPaymentQuote), Events.InterchangeAcknowledgedCode, ProviderCodes.OFX, ZString.Empty);
			var objects = ProcessEventXml(xml, out var logger);
			AssertEquals("Error - No Context Collection found.", logger.ToString());
			AssertNull("Expect no quote is found", objects);
		}

		public void TestGetLogParents()
		{
			var xml = string.Format(XMLBaseFormat, ValidXMLFormat, "00001000", nameof(DataContextType.AccEPaymentQuote), Events.InterchangeAcknowledgedCode, ProviderCodes.OFX, ContextCollection);

			var correctQuote = Factory.NewWithValidTestData<AccEPaymentQuote>();
			correctQuote.QU_InternalReference = "00001000";

			var wrongRefQuote = Factory.NewWithValidTestData<AccEPaymentQuote>();
			wrongRefQuote.QU_InternalReference = "00001111";

			var otherCompany = TestObjectCreator.CreateNewCompany("NEW");
			var wrongCompanyQuote = Factory.NewWithValidTestData<AccEPaymentQuote>();
			wrongCompanyQuote.QU_InternalReference = "00001000";
			wrongCompanyQuote.QU_GC = otherCompany.PK;

			Factory.Save();

			var objects = ProcessEventXml(xml, out var logger);
			AssertEquals(ZString.Empty, logger.ToString());
			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { correctQuote }, objects);
		}

		readonly string ValidXMLFormat = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">";
		readonly string InvalidXMLFormat = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11""> ";
		readonly string ContextCollection = $@"<ContextCollection> 
           <Context>             
             <Type>ProviderRef</Type>
            <Value>abcd1234-ab12-cd34-ef56-abcd1234efgh5678</Value>
          </Context> 
           <Context>             
             <Type>Ledger</Type>
            <Value>AP</Value>
          </Context> 
           <Context>             
             <Type>CompanyCode</Type>
            <Value>{Env.CurrentCompany.Code}</Value>
          </Context> 
          <Context> 
            <Type>FromCurrency</Type>
            <Value>AUD</Value>
          </Context> 
          <Context> 
            <Type>FromAmount</Type>
            <Value>500.0</Value> 
          </Context> 
         <Context> 
            <Type>ToCurrency</Type>
            <Value>USD</Value>
          </Context> 
          <Context> 
            <Type>ToAmount</Type>
            <Value>1000.0</Value> 
          </Context> 
          <Context> 
            <Type>ExchangeRate</Type>
            <Value>2.0</Value> 
          </Context> 
          <Context> 
            <Type>ExchangeRateInverted</Type>
            <Value>0.5</Value> 
          </Context> 
          <Context> 
            <Type>FeeCurrency</Type>
            <Value>AUD</Value>
          </Context> 
          <Context> 
            <Type>FeeAmount</Type>
            <Value>10.0</Value> 
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
          <MessageSubType>GAQ</MessageSubType>
        </EventParameters> 
        {5}
      </Event> 
    </UniversalEvent>";

		BusinessObject[] ProcessEventXml(string eventXmlMessage, out XmlSessionTracker logger)
		{
			logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var subscriber = new AccEPaymentQuoteEventParentFinder(Factory, new AccEPaymentQuoteDataContextManager(), logger);

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlMessage);
			return subscriber.GetLogParentsForEvent(xmlEvent);
		}

		protected TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
