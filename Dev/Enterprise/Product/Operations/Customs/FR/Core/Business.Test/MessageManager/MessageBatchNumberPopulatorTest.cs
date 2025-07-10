using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.EdiMessages;

namespace Enterprise.Customs.FR.Business.MessageManager.Testing
{
	class MessageBatchNumberPopulatorTest : TestCaseWithFactory
	{
		public void TestPopulateBatchNumber()
		{
			var messageBatchNumberPopulator = new MessageBatchNumberPopulator(Factory);
			var message = Factory.New<FREDIMessage>();
			message.FillWithValidTestData();
			var message2 = Factory.New<FREDIMessage>();
			message.FillWithValidTestData();

			messageBatchNumberPopulator.PopulateBatchNumber(message);
			AssertEquals("message EM_ApplicationReference should be equal FRMessageFountainNumber value", "0000000000000001", message.EM_ApplicationReference);

			messageBatchNumberPopulator.PopulateBatchNumber(message);
			AssertEquals("message EM_ApplicationReference should have not change", "0000000000000001", message.EM_ApplicationReference);

			messageBatchNumberPopulator.PopulateBatchNumber(message2);
			AssertEquals("message2 EM_ApplicationReference should have same value as message EM_ApplicationReference as there are in the same batch ", "0000000000000001", message2.EM_ApplicationReference);

			messageBatchNumberPopulator = new MessageBatchNumberPopulator(Factory);
			messageBatchNumberPopulator.PopulateBatchNumber(message2);
			AssertEquals("message2 EM_ApplicationReference should be now equal FRMessageFountainNumber next value", "0000000000000002", message2.EM_ApplicationReference);
		}
	}
}
