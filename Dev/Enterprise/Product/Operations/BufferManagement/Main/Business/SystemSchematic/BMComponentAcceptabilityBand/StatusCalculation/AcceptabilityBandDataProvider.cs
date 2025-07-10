using System;
using System.Threading;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public class AcceptabilityBandDataProvider
	{
		public AcceptabilityBandDataProvider(IConnectionProvider connectionProvider)
		{
			this.connectionProvider = connectionProvider;
		}

		readonly IConnectionProvider connectionProvider;

		public IDbConnectionForReportingWrapper ConnectionWrapper => connectionWrapper ?? (connectionWrapper = connectionProvider.GetNewConnectionWrapper());
		IDbConnectionForReportingWrapper connectionWrapper;

#if DEBUG
		public event EventHandler BeforeScalarResultCommandExecuted_ForTest;

		public void OnBeforeReaderCommandExecuted_ForTest()
		{
			Interlocked.Increment(ref OnBeforeReaderCommandExecutedCount_ForTest);
			BeforeScalarResultCommandExecuted_ForTest?.Invoke(this, EventArgs.Empty);
		}

#pragma warning disable CW1021 // Static Fields Are Thread Static Rule
		public static int OnBeforeReaderCommandExecutedCount_ForTest;
#pragma warning restore CW1021 // Static Fields Are Thread Static Rule
#endif
	}
}
