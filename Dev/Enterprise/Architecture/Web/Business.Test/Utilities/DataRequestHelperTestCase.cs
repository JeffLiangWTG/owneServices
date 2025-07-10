using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Business.Utilities
{
	[HttpContextEnabledTest]
	[TestsSubclassesOf(typeof(DataRequestHelper), ExcludePrivate = true)]
	public abstract class DataRequestHelperTestCase : TransactionedTestCase
	{
		#region DataRequestHelper

		protected DataRequestHelper RequestHelper
		{
			get
			{
				if (fRequestHelper == null)
				{
					fRequestHelper = GetRequestHelper();
				}
				return fRequestHelper;
			}
		}

		protected abstract DataRequestHelper GetRequestHelper();
		DataRequestHelper fRequestHelper;

		#endregion

		#region TestBaseUrl

		public void TestBaseUrl()
		{
			Assert("BaseUrl must not contain a querystring", !RequestHelper.BaseUrl.Contains("?"));
			AssertEquals("BaseUrl must be correct", ExpectedBaseUrl, RequestHelper.BaseUrl);
		}

		protected abstract string ExpectedBaseUrl { get; }

		#endregion

		#region TestUseSecureQueryString / TestEnableCache

		public void TestUseSecureQueryString()
		{
			AssertEquals("Secure query strings should be " + ExpectedUseSecureQueryString.ToString(), ExpectedUseSecureQueryString, RequestHelper.UseSecureQueryString);
		}

		public void TestEnableCache()
		{
			AssertEquals("Caching should be " + (ExpectedEnableCache ? "enabled" : "disabled"), ExpectedEnableCache, RequestHelper.EnableCache);
		}

		protected abstract bool ExpectedUseSecureQueryString { get; }

		protected abstract bool ExpectedEnableCache { get; }

		#endregion
	}
}
