using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using BaseEDIMessage = Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.IE.Business.Testing
{
	public class BaseMessageInterpreterBaseOnlyTest : TestCaseWithFactory
	{
		public void TestMessageCreatedDate()
		{
			var interpreter = CreateInterpreter();
			ediMessage.EM_SystemCreateTimeUtc = ZDateTime.Empty;
			AssertEquals("Default today", ZDate.Today, interpreter.MessageCreatedDateExposed);

			ediMessage.EM_SystemCreateTimeUtc = new ZDateTime(2023, 1, 4, 12, 35, 0);
			interpreter = CreateInterpreter();
			AssertEquals("SystemCreateTimeUTC", new ZDate(2023, 1, 4), interpreter.MessageCreatedDateExposed);
		}

		protected override void SetUp()
		{
			base.SetUp();
			ediMessage = Factory.New<BaseEDIMessage>();
		}
		BaseEDIMessage ediMessage;

		BaseMessageInterpreterForTesting CreateInterpreter() => new BaseMessageInterpreterForTesting(ediMessage, new object());
	}

	class BaseMessageInterpreterForTesting : BaseMessageInterpreter<object>
	{
		public BaseMessageInterpreterForTesting(BaseEDIMessage message, object provider) : base(message, provider)
		{
		}

		internal ZDate MessageCreatedDateExposed => MessageCreatedDate;
	}
}
