using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.CusStatement;

namespace Enterprise.Customs.FR.Business.MessageSending.Testing
{
	public class StatementMessageSendingObjectTest : TestCaseWithFactory
	{
		public void TestMessageOwner()
		{
			AssertSame(statement, ((IMessageSendingObject)objectToSend).MessagesOwner);
		}

		public void TestDataSource()
		{
			AssertSame(statement, ((IMessageSendingObject)objectToSend).DataSource);
		}

		public void TestMessageType()
		{
			AssertEquals(MessageTypeList.Codes.DCG, ((IMessageSendingObject)objectToSend).MessageType);
		}

		protected override void SetUp()
		{
			base.SetUp();
			statement = Factory.New<CusStatementHeader>();
			objectToSend = new StatementMessageSendingObject(statement);
		}

		CusStatementHeader statement;
		StatementMessageSendingObject objectToSend;
	}
}
