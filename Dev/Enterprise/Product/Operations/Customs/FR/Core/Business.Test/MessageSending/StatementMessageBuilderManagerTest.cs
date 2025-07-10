using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.CusStatement;
using Enterprise.Customs.FR.Messaging.MessageBuilders.DCG;

namespace Enterprise.Customs.FR.Business.MessageSending.Testing
{
	public class StatementMessageBuilderManagerTest : TestCaseWithFactory
	{
		public void TestNewMessageBuilder()
		{
			var statement = Factory.New<CusStatementHeader>();
			var objectToSend = new StatementMessageSendingObject(statement);
			var errorCollector = new ErrorCollector();
			var messageBuilderManager = new StatementMessageBuilderManager(errorCollector);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(objectToSend);
			AssertType<DCGSendMessageBuilder>(messageBuilder);
		}
	}
}
