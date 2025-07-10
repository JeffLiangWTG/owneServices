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
	public class AccEPaymentBeneficiaryRequestEventParentFinderTest : TestCaseWithFactory
	{
		public void TestGetLogParents_WrongXMLFormat()
		{
			var xml = string.Format(XMLBaseFormat, InvalidXMLFormat, ZString.Empty, nameof(DataContextType.AccEPaymentBeneficiaryRequest), ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			var objects = ProcessEventXml(xml, out var logger);
			AssertEquals("Error - Unsupported XML format encountered: Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11.DataContext.", logger.ToString());
			AssertNull("Expect no Beneficiary Request is found", objects);
		}

		public void TestGetLogParents_NoProviderCode()
		{
			var xml = string.Format(XMLBaseFormat, ValidXMLFormat, ZString.Empty, nameof(DataContextType.AccEPaymentBeneficiaryRequest), Events.InterchangeAcknowledgedCode, ZString.Empty, AccEPaymentBeneficiaryRequestMessageConstants.MessageSubTypes.SearchBeneficiary, ZString.Empty);
			var objects = ProcessEventXml(xml, out var logger);
			AssertEquals("Error - Could not find a valid Provider Code in the Event's <MessageType> parameter. Value found: ''", logger.ToString());
			AssertNull("Expect no Beneficiary Request is found", objects);
		}

		public void TestGetLogParents_NoDealFound()
		{
			var xml = string.Format(XMLBaseFormat, ValidXMLFormat, ZString.Empty, nameof(DataContextType.AccEPaymentBeneficiaryRequest), Events.InterchangeAcknowledgedCode, ProviderCodes.OFX, AccEPaymentBeneficiaryRequestMessageConstants.MessageSubTypes.SearchBeneficiary, ZString.Empty);
			var objects = ProcessEventXml(xml, out var logger);
			AssertEquals("Error - Could not find a valid Beneficiary Request Reference in the <DataTarget> Key parameter. Value found: ''", logger.ToString());
			AssertNull("Expect no Beneficiary Request is found", objects);
		}

		public void TestGetLogParents_NoContextCollection()
		{
			var xml = string.Format(XMLBaseFormat, ValidXMLFormat, "00001000", nameof(DataContextType.AccEPaymentBeneficiaryRequest), Events.InterchangeAcknowledgedCode, ProviderCodes.OFX, AccEPaymentBeneficiaryRequestMessageConstants.MessageSubTypes.SearchBeneficiary, ZString.Empty);
			var objects = ProcessEventXml(xml, out var logger);
			AssertEquals("Error - No Context Collection found.", logger.ToString());
			AssertNull("Expect no Beneficiary Request is found", objects);
		}

		public void TestGetLogParents()
		{
			var xml = string.Format(XMLBaseFormat, ValidXMLFormat, "00001000", nameof(DataContextType.AccEPaymentBeneficiaryRequest), Events.InterchangeAcknowledgedCode, ProviderCodes.OFX, AccEPaymentBeneficiaryRequestMessageConstants.MessageSubTypes.SearchBeneficiary, ContextCollection);

			var correctRequest = Factory.NewWithValidTestData<AccEPaymentBeneficiaryRequest>();
			correctRequest.ABR_InternalReference = "00001000";
			correctRequest.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Partial;
			correctRequest.ABR_LastResponseReceivedUtc = ZDateTime.UtcNow;

			var wrongRefRequest = Factory.NewWithValidTestData<AccEPaymentBeneficiaryRequest>();
			wrongRefRequest.ABR_InternalReference = "00001111";
			wrongRefRequest.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Partial;
			wrongRefRequest.ABR_LastResponseReceivedUtc = ZDateTime.UtcNow;

			var otherCompany = TestObjectCreator.CreateNewCompany("NEW");
			var wrongCompanyRequest = Factory.NewWithValidTestData<AccEPaymentBeneficiaryRequest>();
			wrongCompanyRequest.ABR_InternalReference = "00001000";
			wrongCompanyRequest.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Partial;
			wrongCompanyRequest.ABR_LastResponseReceivedUtc = ZDateTime.UtcNow;
			wrongCompanyRequest.ABR_GC_Company = otherCompany.PK;

			Factory.Save();

			var objects = ProcessEventXml(xml, out var logger);
			AssertEquals(ZString.Empty, logger.ToString());
			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { correctRequest }, objects);
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
			var subscriber = new AccEPaymentBeneficiaryRequestEventParentFinder(Factory, new AccEPaymentBeneficiaryRequestDataContextManager(), logger);

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlMessage);
			return subscriber.GetLogParentsForEvent(xmlEvent);
		}

		protected TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
