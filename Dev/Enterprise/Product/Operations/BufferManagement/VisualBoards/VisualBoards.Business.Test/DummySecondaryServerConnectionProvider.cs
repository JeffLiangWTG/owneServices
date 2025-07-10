using System;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.VisualBoards.Business.Test
{
	public class DummySecondaryServerConnectionProvider : ISecondaryServerConnectionProvider
	{
		public static IDisposable TemporarilyEnableDummyProvider(DbConnection connectionToUseForSecondaryServer = null, bool forceOverrideProvider = false)
		{
			if (!forceOverrideProvider && SecondaryServerConnectionProviderProvider.GetProvider() is DummySecondaryServerConnectionProvider)
			{
				return DisposableAction.NoAction;
			}

			return SecondaryServerConnectionProviderProvider.OverrideProvider(new DummySecondaryServerConnectionProvider(connectionToUseForSecondaryServer));
		}

		DummySecondaryServerConnectionProvider(DbConnection connectionToUseForSecondaryServer = null)
		{
			this.connectionToUseForSecondaryServer = connectionToUseForSecondaryServer;
		}

		readonly DbConnection connectionToUseForSecondaryServer;

		bool ISecondaryServerConnectionProvider.IsReportingDbEnabled => true;

		bool ISecondaryServerConnectionProvider.NeedUsePrimaryServer => true;

		SecondaryServerConnectionDetailsProvider ISecondaryServerConnectionProvider.SecondaryServerConnectionDetails => new SecondaryServerConnectionDetailsProvider();

		IDbConnectionForReportingWrapper IConnectionProvider.GetNewConnectionWrapper(string dbUserName, string applicationNameSuffix)
		{
			GetNewConnectionWrapperExecutedCount++;
			return new DummyDbConnectionForReportingWrapper(connectionToUseForSecondaryServer ?? Db.Connection, shouldDisposeConnection: false);
		}

		public int GetNewConnectionWrapperExecutedCount { get; set; }

		class DummyDbConnectionForReportingWrapper : IDbConnectionForReportingWrapper
		{
			public DummyDbConnectionForReportingWrapper(DbConnection connection, bool shouldDisposeConnection)
			{
				this.connection = connection;
				this.shouldDisposeConnection = shouldDisposeConnection;
			}

			readonly DbConnection connection;
			readonly bool shouldDisposeConnection;

			DbConnection IDbConnectionForReportingWrapper.Connection => connection;

			bool IDbConnectionForReportingWrapper.IsMainServer => true;

			void IDisposable.Dispose()
			{
				if (shouldDisposeConnection)
				{
					connection.Dispose();
				}
			}
		}
	}
}
