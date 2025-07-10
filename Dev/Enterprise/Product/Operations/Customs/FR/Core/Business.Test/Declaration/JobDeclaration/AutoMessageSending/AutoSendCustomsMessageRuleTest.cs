using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Interfaces;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public abstract class AutoSendCustomsMessageRuleTest<T> : TestCaseWithFactory where T : IAutoSendCustomsMessageRule, new()
	{
		public void TestSendCustomsMessageRule()
		{
			var rule = new T();
			AssertEquals("NewMessageType", ExpectedMessageType, rule.newMessageType);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			entry.CH_EntryStatus = EntryStatusThatCanSendMessage;
			Assert(rule.CanSendMessage(entry));

			entry.CH_EntryStatus = EntryStatusThatCanNotSendMessage;
			Assert(!rule.CanSendMessage(entry));
		}

		protected abstract ZString ExpectedMessageType { get; }

		protected abstract ZString EntryStatusThatCanSendMessage { get; }

		protected abstract ZString EntryStatusThatCanNotSendMessage { get; }
	}
}
