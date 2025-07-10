namespace Enterprise.AuditDataServices.Subscription.Common
{
	public interface IChangedTableSchema
	{
		string SchemaName { get; }
		string TableName { get; }
		string PkName { get; }
	}
}
