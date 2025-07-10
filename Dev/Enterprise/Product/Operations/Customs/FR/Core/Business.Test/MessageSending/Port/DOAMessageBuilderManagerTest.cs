using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Documents;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Customs.FR.Messaging.MessageBuilders.DOA;

namespace Enterprise.Customs.FR.Business.MessageSending.Testing
{
	public class DOAMessageBuilderManagerTest : TestCaseWithFactory
	{
		public void TestBuilderType()
		{
			AssertEquals(MessageTypeList.Codes.POR, manager.BuilderType);
		}

		public void TestMessageBuilder()
		{
			var header = Factory.New<NctsHeader>();
			var dataObject = new DOADataObject();

			var sendingObject = new DOAMessageSendingObject(header, dataObject);
			AssertType<DOASendMessageBuilder>(manager.NewMessageBuilder(sendingObject));
		}

		protected override void SetUp()
		{
			base.SetUp();
			errorCollector = new ErrorCollector();
			manager = new DOAMessageBuilderManager(errorCollector);
		}
		ErrorCollector errorCollector;
		DOAMessageBuilderManager manager;
	}
}
