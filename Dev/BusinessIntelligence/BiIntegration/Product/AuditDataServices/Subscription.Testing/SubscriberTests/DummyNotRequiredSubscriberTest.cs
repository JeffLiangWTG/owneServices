namespace Enterprise.AuditDataServices.Subscription.Testing
{
	using System.Data;
	using Enterprise.AuditDataServices.Subscription.Subscribers;
	using NUnit.Framework;

	[TestedType(typeof(DummyNotRequiredSubscriber))]
	class DummyNotRequiredSubscriberTest : ActualDataChangesAuditSubscriberTest
	{
		public override void TestCustomFilter()
		{
			var subscriber = new DummyNotRequiredSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		protected override DataTable GetTestDataTable() => null;
	}
}
