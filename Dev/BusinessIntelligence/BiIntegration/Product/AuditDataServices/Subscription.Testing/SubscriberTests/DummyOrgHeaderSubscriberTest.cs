namespace Enterprise.AuditDataServices.Subscription.Testing
{
	using System.Data;
	using Enterprise.AuditDataServices.Subscription.Subscribers;
	using NUnit.Framework;

	[TestedType(typeof(DummyOrgHeaderSubscriber))]
	class DummyOrgHeaderSubscriberTest : ActualDataChangesAuditSubscriberTest
	{
		public override void TestCustomFilter()
		{
			var subscriber = new DummyOrgHeaderSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		protected override DataTable GetTestDataTable() => null;
	}
}
