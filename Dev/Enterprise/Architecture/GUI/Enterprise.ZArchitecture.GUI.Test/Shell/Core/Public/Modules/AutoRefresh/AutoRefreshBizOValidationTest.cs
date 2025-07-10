using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Modules.AutoRefresh.Testing
{
	sealed class AutoRefreshBizOValidationTest : BusinessObjectValidationTestCase
	{
		#region TestValidateAutoRefreshTimeOutDescription

		public void TestValidateAutoRefreshTimeOutDescription()
		{
			AssertNoErrors(BizO.AutoRefreshTimeOutDescriptionInfo);

			BizO.AutoRefreshTimeOutDescription = "15 MiNUteS";
			AssertNoErrors(BizO.AutoRefreshTimeOutDescriptionInfo);

			BizO.AutoRefreshTimeOutDescription = "";
			AssertHasErrors(BizO.AutoRefreshTimeOutDescriptionInfo);

			BizO.AutoRefreshTimeOutDescription = "Hour";
			AssertNoErrors(BizO.AutoRefreshTimeOutDescriptionInfo);

			BizO.AutoRefreshTimeOutDescription = "crap";
			AssertHasErrors(BizO.AutoRefreshTimeOutDescriptionInfo);
		}

		#endregion

		#region Implementation

		AutoRefreshBizO BizO
		{
			get { return fBizO ?? (fBizO = new AutoRefreshBizO(5)); }
		}

		AutoRefreshBizO fBizO;

		#endregion
	}
}
