namespace Enterprise.AuditDataServices.Subscription.Testing
{
	using Enterprise.AuditDataServices.Subscription.Common;
	using Enterprise.AuditDataServices.Subscription.Subscribers;
	using NUnit.Framework;

	[TestedType(typeof(DummyChangedTableListSubscriber))]
	class DummyChangedTableListSubscriberTest : ChangedTableListOnlyAuditSubscriberTest
	{
		protected override ChangedTableListOnlyAuditSubscriber NewChangedTableListSubscriber() => new DummyChangedTableListSubscriber();
	}
}
