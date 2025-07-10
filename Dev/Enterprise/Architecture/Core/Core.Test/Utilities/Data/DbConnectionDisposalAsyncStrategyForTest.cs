using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class DbConnectionDisposalAsyncStrategyForTest : DBConnectionDisposalAsyncStrategy
	{
		public DbConnectionDisposalAsyncStrategyForTest()
		{
		}

		public new static DbConnectionDisposalAsyncStrategyForTest Get()
		{
			return new DbConnectionDisposalAsyncStrategyForTest();
		}

		protected override IDisposable OnBackgroundThreadStarting(ThreadHandoverToken token, int? callerThreadID)
		{
			return new DisposableList(new[] { base.OnBackgroundThreadStarting(token, callerThreadID), new DisposableAction(() => ewh?.Set()) });
		}

		protected override void TakeOwnershipAndExecute(Action action, ThreadHandoverToken threadTokenHandover, string threadName, int callerThreadID)
		{
			base.TakeOwnershipAndExecute(action, threadTokenHandover, threadName, callerThreadID);

			using (Db.DisposableActionForDbConnection())
			{
				ConnectionState = Db.Connection.State;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule", Justification = "Unit test only case")]
		public ConnectionState ConnectionState;

		[SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule", Justification = "Unit test only case")]
		public AutoResetEvent ewh;
	}
}
