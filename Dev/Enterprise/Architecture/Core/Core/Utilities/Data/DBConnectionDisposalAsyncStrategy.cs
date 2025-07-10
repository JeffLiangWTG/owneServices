using System;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.Data;

namespace Enterprise.ZArchitecture.Core
{
	public class DBConnectionDisposalAsyncStrategy : DefaultAsyncStrategy
	{
		#region Construction

		public new static IAsyncStrategy Get()
		{
			return new DBConnectionDisposalAsyncStrategy();
		}

		protected DBConnectionDisposalAsyncStrategy()
		{
		}

		#endregion

		protected override IDisposable OnBackgroundThreadStarting(ThreadHandoverToken token, int? callerThreadID)
		{
			return new DisposableList(new[] { base.OnBackgroundThreadStarting(token, callerThreadID), Db.DisposableActionForDbConnection() });
		}
	}
}
