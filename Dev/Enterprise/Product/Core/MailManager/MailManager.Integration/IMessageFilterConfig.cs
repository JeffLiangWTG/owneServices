namespace Enterprise.MailManager.Integration
{
	public interface IMessageFilterConfig
	{
		string ServiceTaskCode { get; }
		string TableName { get; }
		string TypeName { get; }
		string TypeAssemblyName { get; }
	}
}
