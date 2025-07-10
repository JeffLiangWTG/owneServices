using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.CN.Business.Testing
{
	public class InboundInterchangeProcessorTest : TestCaseWithFactory
	{
		public void TestProcessor()
		{
			var factoryForReload = new BusinessObjectFactory { NameForDebugging = "Enterprise.Customs.CN.Business.Testing.InboundInterchangeProcessorTest.factoryForReload" };

			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Queued;
			Factory.Save();
			var processor = new InboundInterchangeProcessor();
			processor.ExecuteBatch();
			var processedInterchange = factoryForReload.Load<EDIInterchange>(interchange.PK);
			AssertEquals("EDIInterchange not fit filter, should not be processed.", EDIInterchangeStatusList.Codes.Queued, processedInterchange.EI_Status);

			var interchangeCSW = Factory.NewWithValidTestData<EDIInterchange>();
			interchangeCSW.EI_Status = EDIInterchangeStatusList.Codes.Queued;
			interchangeCSW.EI_InterchangeType = "CSW";
			Factory.Save();
			var processor2 = new InboundInterchangeProcessor();
			processor2.ExecuteBatch();
			var processedInterchangeCSW = factoryForReload.Load<EDIInterchange>(interchangeCSW.PK);
			AssertEquals("EDIInterchange not fit filter, should not be processed.", EDIInterchangeStatusList.Codes.Queued, processedInterchangeCSW.EI_Status);

			var interchangeValid = Factory.NewWithValidTestData<EDIInterchange>();
			interchangeValid.EI_Status = EDIInterchangeStatusList.Codes.Queued;
			interchangeValid.EI_InterchangeType = "CSW";
			interchangeValid.EI_ApplicationCode = EDIMessage.ApplicationCodes.GenericMessageDelivery;
			interchangeValid.EI_IsActive = true;
			interchangeValid.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			Factory.Save();
			var processor3 = new InboundInterchangeProcessor();
			processor3.ExecuteBatch();
			var processedInterchangeValid = factoryForReload.Load<EDIInterchange>(interchangeValid.PK);
			AssertEquals("EDIInterchange fit filter with empty text, should be processed and set ERR.", EDIInterchangeStatusList.Codes.Error, processedInterchangeValid.EI_Status);
		}
	}
}
