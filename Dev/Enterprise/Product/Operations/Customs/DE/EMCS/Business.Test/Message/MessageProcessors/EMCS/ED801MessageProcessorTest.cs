using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	class ED801MessageProcessorTest : TestCaseWithFactory
	{
		public void TestGetProcessor_Consignor()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_MessageSubType = Messaging.EmcsMessageSubTypeList.Codes.Eme;

			AssertType<ED801ConsignorMessageProcessor>(provider.GetProcessor(message));
		}

		public void TestGetProcessor_Consignee()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_MessageSubType = Messaging.EmcsMessageSubTypeList.Codes.Emb;

			AssertType<ED801ConsigneeMessageProcessor>(provider.GetProcessor(message));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var logger = new LoggingInformation();
			provider = new ED801MessageProcessor(logger);
		}
		ED801MessageProcessor provider;
	}
}
