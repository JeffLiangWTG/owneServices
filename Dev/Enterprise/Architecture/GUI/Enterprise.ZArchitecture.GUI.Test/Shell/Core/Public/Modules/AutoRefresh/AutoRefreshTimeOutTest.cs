using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.AutoRefresh
{
	sealed class AutoRefreshTimeOutTest : TestCase
	{
		#region TestConstructorWithParameters

		public void TestConstructorWithParameters()
		{
			var timeOut = new AutoRefreshTimeOut(true, 30);
			AssertEquals(true, timeOut.IsEnabled);
			AssertEquals((byte)30, timeOut.RefreshTimeInMinutes);
		}

		#endregion

		#region TestAutoRefreshTimeOutDefaults

		public void TestAutoRefreshTimeOutDefaults()
		{
			var timeOut = new AutoRefreshTimeOut();
			AssertEquals(false, timeOut.IsEnabled);
			AssertEquals((byte)5, timeOut.RefreshTimeInMinutes);
		}

		#endregion

		#region TestIsEnabled

		public void TestIsEnabled()
		{
			var timeOut = new AutoRefreshTimeOut();
			timeOut.IsEnabled = true;
			AssertEquals(true, timeOut.IsEnabled);
		}

		#endregion

		#region TestRefreshTimeInMinutes

		public void TestRefreshTimeInMinutes()
		{
			var timeOut = new AutoRefreshTimeOut();
			timeOut.RefreshTimeInMinutes = 15;
			AssertEquals((byte)15, timeOut.RefreshTimeInMinutes);
		}

		#endregion
	}
}
