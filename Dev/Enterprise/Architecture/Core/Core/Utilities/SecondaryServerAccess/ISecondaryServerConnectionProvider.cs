namespace Enterprise.ZArchitecture.Core
{
	public interface ISecondaryServerConnectionProvider : IConnectionProvider
	{
		SecondaryServerConnectionDetailsProvider SecondaryServerConnectionDetails { get; }

		bool IsReportingDbEnabled { get; }
		bool NeedUsePrimaryServer { get; }
	}

	public interface IConnectionProvider
	{
		IDbConnectionForReportingWrapper GetNewConnectionWrapper(string dbUserName = null, string applicationNameSuffix = null);
	}
}
