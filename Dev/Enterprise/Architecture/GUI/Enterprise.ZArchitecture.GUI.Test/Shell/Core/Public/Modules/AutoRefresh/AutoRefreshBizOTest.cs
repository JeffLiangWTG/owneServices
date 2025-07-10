using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.AutoRefresh.Testing
{
	[TestedType(typeof(AutoRefreshBizO))]
	sealed class AutoRefreshBizOTest : NonPersistentBusinessObjectTestCase
	{
		#region TestConstructor

		public void TestConstructor()
		{
			var bizO = new AutoRefreshBizO(15);
			AssertEquals(false, bizO.HasChanges);
			AssertEquals((byte)15, bizO.AutoRefreshTimeOut);
		}

		#endregion

		#region TestAutoRefreshTimeOut

		public void TestAutoRefreshTimeOutGet()
		{
			BizO.AutoRefreshTimeOutDescription = "15 Minutes";
			AssertEquals((byte)15, BizO.AutoRefreshTimeOut);

			BizO.AutoRefreshTimeOutDescription = "crap";
			AssertEquals((byte)0, BizO.AutoRefreshTimeOut);

			BizO.AutoRefreshTimeOutDescription = "Hour";
			AssertEquals((byte)60, BizO.AutoRefreshTimeOut);

			BizO.AutoRefreshTimeOutDescription = "-1";
			AssertEquals((byte)0, BizO.AutoRefreshTimeOut);
		}

		public void TestAutoRefreshTimeOutSet()
		{
			BizO.AutoRefreshTimeOut = 15;
			AssertEquals("15 Minutes", BizO.AutoRefreshTimeOutDescription);

			BizO.AutoRefreshTimeOut = 0;
			AssertEquals("", BizO.AutoRefreshTimeOutDescription);

			BizO.AutoRefreshTimeOut = 60;
			AssertEquals("Hour", BizO.AutoRefreshTimeOutDescription);

			BizO.AutoRefreshTimeOut = 120;
			AssertEquals("2 Hours", BizO.AutoRefreshTimeOutDescription);
		}

		#endregion

		#region TestAutoRefreshTimeOutDescription_List

		public void TestAutoRefreshTimeOutDescription_List()
		{
			AssertEquals("Property should be cached.", BizO.AutoRefreshTimeOutDescription_List, BizO.AutoRefreshTimeOutDescription_List);
			AssertEquals(true, BizO.AutoRefreshTimeOutDescription_List.ContainsOnly(
				"1 Minute", "5 Minutes", "10 Minutes", "15 Minutes", "30 Minutes", "45 Minutes", "Hour", "2 Hours", "3 Hours", "4 Hours"));
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AutoRefreshBizO(5);
		}

		AutoRefreshBizO BizO
		{
			get { return fBizO ?? (fBizO = new AutoRefreshBizO(5)); }
		}

		AutoRefreshBizO fBizO;

		#endregion
	}
}
