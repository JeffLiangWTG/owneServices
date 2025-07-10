namespace Enterprise.AlwaysOn.Setup
{
	public interface IDbServerInstance : IValidationStatus
	{
		void Load();
		SqlServerInfo ServerInfo { get; }
		IFailoverCluster FailoverCluster { get; }
		int AlwaysOnEndpointPort { get; }
		bool IsFailoverClusterInstance { get; }
		DbLoginInfo OdysseyAdminLogin { get; }
	}
}
