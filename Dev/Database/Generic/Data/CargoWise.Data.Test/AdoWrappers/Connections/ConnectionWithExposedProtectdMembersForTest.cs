using System;
using CargoWise.DataProtection;

namespace CargoWise.Data.Testing
{
	sealed class ConnectionWithExposedProtectdMembersForTest : DbConnection<RestrictedWriterLoginCredentials>
	{
		internal ConnectionWithExposedProtectdMembersForTest()
			: base()
		{
		}

		public void OpenConnectionWithSplashInfo_Exposed()
		{
			OpenConnectionWithSplashInfo();
		}

		protected override IDisposable NewConnectingSplashFormManager()
		{
			NewConnectingSplashFormManagerWasAccessed = true;
			return base.NewConnectingSplashFormManager();
		}

		public bool NewConnectingSplashFormManagerWasAccessed { get; set; }

		public override string UserLogin => RestrictedWriterLoginCredentials.UserNameFor(fInitialDatabaseName);
	}
}
