using System;
namespace CargoWise.Data.Testing
{
	sealed class MainConnectionForTest : MainConnection
	{
		public MainConnectionForTest(TimeSpan maximumTimeSpentTryingToReconnect) : base()
		{
			hasConnectedBefore = true;
			this.maximumTimeSpentTryingToReconnect = maximumTimeSpentTryingToReconnect;
		}

		public bool ConfirmReopenConnection_Exposed()
		{
			return ConfirmReopenConnection();
		}

		readonly TimeSpan maximumTimeSpentTryingToReconnect;
		protected override TimeSpan MaximumTimeSpentTryingToReconnect
		{
			get { return maximumTimeSpentTryingToReconnect; }
		}

		protected override void OpenConnectionWithSplashInfo()
		{
#if NETFRAMEWORK
			throw (SqlException)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(SqlException));
#else
			throw (SqlException)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(SqlException));
#endif
		}

		protected override DbErrorType GetErrorType(System.Data.Common.DbException sqlEx)
		{
			return DbErrorType.ServerDoesNotExist;
		}
	}
}
