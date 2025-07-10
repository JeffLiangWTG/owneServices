using Enterprise.Messaging.Integration;

namespace Enterprise.UniversalDataBuss.Management.Testing
{
	class ScheduleProcessingTest : TestCaseWithFactoryAndMessagingHelpers
	{
//        public void TestProcessScheduleData_Recognised()
//        {
//            var message = GetQueuedUniversalScheduleMessage(@"<UniversalSchedule xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
//      <Schedule>
//        <Carrier>
//          <AddressType>Carrier</AddressType>
//          <OrganizationCode>OFR</OrganizationCode>
//          <CompanyName>CMA-CGM</CompanyName>
//        </Carrier>
//        <DataProvider>DAK</DataProvider>
//        <IsCancellation>false</IsCancellation>
//        <Transport>
//          <Sea>
//            <Vessel>
//              <VesselName>GRANDE NIGERIA</VesselName>
//              <LloydsNumber>9130937</LloydsNumber>
//            </Vessel>
//            <VoyageNumber>OFR1125</VoyageNumber>
//          </Sea>
//        </Transport>
//        <DischargeCollection />
//        <LoadingCollection />
//      </Schedule>
//    </UniversalSchedule>");

//            var serviceTaskLog = new ServiceTaskLogForTesting();
//            var manager = new UniversalMessageProcessingManager(serviceTaskLog);
//            manager.Process(message);

//            AssertEquals(EDIMessageStatusList.Codes.Recognised, message.EM_Status);
//        }

		public void TestProcessScheduleData_Failed()
		{
			var message = GetQueuedUniversalScheduleMessage(@"<UniversalSchedule xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
      <Schedule>
        <Carrier>
          <AddressType>Carrier</AddressType>
          <OrganizationCode>OFR</OrganizationCode>
          <CompanyName>CMA-CGM</CompanyName>
        </Carrier>
        <DataProvider>DAK</DataProvider>
        <IsCancellation>false</IsCancellation>
        <Transport>
          <Sea>
            <Vessel>
              <VesselName>GRANDE NIGERIA</VesselName>
              <LloydsNumber>9130937</LloydsNumber>
            </Vessel>
            <VoyageNumber>OFR1125</VoyageNumber>
          </Sea>
          <Air>
            <FlightNumber>4258785</FlightNumber>
          </Air>
        </Transport>
        <DischargeCollection />
        <LoadingCollection />
      </Schedule>
    </UniversalSchedule>");

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals(EDIMessageStatusList.Codes.Rejected, message.EM_Status);
		}
	}
}
