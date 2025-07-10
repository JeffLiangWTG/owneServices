namespace Enterprise.AuditDataServices.Subscription.Common
{
	public interface IBacklogCountOverridable
	{
		string EffectiveTableName { get; }
		bool CountInsert { get; }
		bool CountUpdate { get; }
		bool CountDelete { get; }
	}
}
