namespace Enterprise.AuditDataServices.Subscription.Testing
{
	using Enterprise.AuditDataServices.Subscription.Common;
	using NUnit.Framework;

	public abstract class AuditSubscriberTest : TransactionedTestCase
	{
		public void TestCodeIs3CharacterLong()
		{
			AssertNotNull("Code", TestSubscriber.Code);
			AssertEquals("Code length", 3, TestSubscriber.Code.Trim().Length);
		}

		protected abstract IAuditSubscriber TestSubscriber { get; }
	}
}
