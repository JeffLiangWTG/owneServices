using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Documents;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Customs.FR.Messaging.MessageBuilders.CAED;

namespace Enterprise.Customs.FR.Business.MessageSending.Testing
{
	public class CAEDMessageBuilderManagerTest : TestCaseWithFactory
	{
		public void TestBuilderType()
		{
			AssertEquals(MessageTypeList.Codes.POR, manager.BuilderType);
		}

		public void TestMessageBuilder()
		{
			var header = Factory.New<NctsHeader>();
			var dataObject = new CAEDDataObject();

			var sendingObject = new CAEDMessageSendingObject(header, dataObject);
			AssertType<CAEDSendMessageBuilder>(manager.NewMessageBuilder(sendingObject));
		}

		protected override void SetUp()
		{
			base.SetUp();
			errorCollector = new ErrorCollector();
			manager = new CAEDMessageBuilderManager(errorCollector);
		}
		ErrorCollector errorCollector;
		CAEDMessageBuilderManager manager;
	}
}
