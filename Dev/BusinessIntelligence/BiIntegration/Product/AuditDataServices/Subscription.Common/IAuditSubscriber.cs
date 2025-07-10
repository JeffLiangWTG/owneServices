namespace Enterprise.AuditDataServices.Subscription.Common
{
	using CargoWise.Data;
	using Enterprise.Integration;

	public interface IAuditSubscriber
	{
		string Code { get; }
		string Description { get; }
		bool IsRequired();
		IAuditSubscriberWrapper GetWrapper(DbConnection auditConnection, ILogger logger);
	}
}
