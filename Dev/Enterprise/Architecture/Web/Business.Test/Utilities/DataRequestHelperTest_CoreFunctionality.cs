using System;
using System.Collections.Generic;
using System.Net;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Business.Test.Utilities
{
	sealed class DataRequestHelperTest_CoreFunctionality : TestCase
	{
		public void TestGetHandlerUrlWithOnePK()
		{
			DataRequestHelperDummy dummyRequestHelper = new DataRequestHelperDummy();
			dummyRequestHelper.SetUseSecureQueryString(false);

			ZGuid pK = ZGuid.NewZGuid();
			string actualHandlerUrl = dummyRequestHelper.GetHandlerUrl(pK);
			string expectedHandlerUrl = dummyRequestHelper.BaseUrl + "?Data=" + WebUtility.UrlEncode(pK.ToString());

			AssertEquals("The handler URL must be URL encoded and contain the PK.", expectedHandlerUrl, actualHandlerUrl);

			dummyRequestHelper.SetUseSecureQueryString(true);
			actualHandlerUrl = dummyRequestHelper.GetHandlerUrl(pK);

			SecureQueryString queryString = new SecureQueryString();
			queryString.Add(DataRequestHelper.DataKey, pK.ToString());

			expectedHandlerUrl = String.Format("{0}?{1}={2}", dummyRequestHelper.BaseUrl, SecureQueryString.QueryStringKey, WebUtility.UrlEncode(queryString.ToString()));

			AssertEquals("The handler URL must be encrypted and URL encoded, and contain the PK.", expectedHandlerUrl, actualHandlerUrl);
		}

		public void TestGetHandlerUrlWithTwoKeys()
		{
			DataRequestHelperDummy dummyRequestHelper = new DataRequestHelperDummy();
			dummyRequestHelper.SetUseSecureQueryString(false);

			Dictionary<string, ZGuid> keys = new Dictionary<string, ZGuid>();
			keys.Add("PK1", ZGuid.NewZGuid());
			keys.Add("PK2", ZGuid.NewZGuid());

			string actualHandlerUrl = dummyRequestHelper.GetHandlerUrl(keys);
			string expectedHandlerUrl = dummyRequestHelper.BaseUrl + "?PK1=" + WebUtility.UrlEncode(keys["PK1"].ToString()) + "&PK2=" + WebUtility.UrlEncode(keys["PK2"].ToString());

			AssertEquals("The handler URL must be URL encoded and contain the two Keys.", expectedHandlerUrl, actualHandlerUrl);

			dummyRequestHelper.SetUseSecureQueryString(true);
			actualHandlerUrl = dummyRequestHelper.GetHandlerUrl(keys);

			SecureQueryString qs = new SecureQueryString();

			foreach (string key in keys.Keys)
			{
				qs.Add(key, keys[key].ToString());
			}

			expectedHandlerUrl = String.Format("{0}?{1}={2}", dummyRequestHelper.BaseUrl, SecureQueryString.QueryStringKey, WebUtility.UrlEncode(qs.ToString()));
			AssertEquals("The handler URL must be encrypted and URL encoded, and contain the two Keyss.", expectedHandlerUrl, actualHandlerUrl);
		}

		public void TestGetHandlerUrlWithThreeKeys()
		{
			DataRequestHelperDummy dummyRequestHelper = new DataRequestHelperDummy();
			dummyRequestHelper.SetUseSecureQueryString(false);

			Dictionary<string, ZGuid> keys = new Dictionary<string, ZGuid>();
			keys.Add("PK1", ZGuid.NewZGuid());
			keys.Add("PK2", ZGuid.NewZGuid());
			keys.Add("PK3", ZGuid.NewZGuid());

			string actualHandlerUrl = dummyRequestHelper.GetHandlerUrl(keys);
			string expectedHandlerUrl = dummyRequestHelper.BaseUrl + "?PK1=" + WebUtility.UrlEncode(keys["PK1"].ToString()) + "&PK2=" + WebUtility.UrlEncode(keys["PK2"].ToString()) +
				"&PK3=" + WebUtility.UrlEncode(keys["PK3"].ToString());

			AssertEquals("The handler URL must be URL encoded and contain the three Keys.", expectedHandlerUrl, actualHandlerUrl);

			dummyRequestHelper.SetUseSecureQueryString(true);
			actualHandlerUrl = dummyRequestHelper.GetHandlerUrl(keys);

			SecureQueryString qs = new SecureQueryString();

			foreach (string key in keys.Keys)
			{
				qs.Add(key, keys[key].ToString());
			}

			expectedHandlerUrl = String.Format("{0}?{1}={2}", dummyRequestHelper.BaseUrl, SecureQueryString.QueryStringKey, WebUtility.UrlEncode(qs.ToString()));
			AssertEquals("The handler URL must be encrypted and URL encoded, and contain the three  Keys.", expectedHandlerUrl, actualHandlerUrl);
		}

		public void TestGetHandlerUrlWithTwoKeysAndThreePKs()
		{
			DataRequestHelperDummy dummyRequestHelper = new DataRequestHelperDummy();
			dummyRequestHelper.SetUseSecureQueryString(false);

			Dictionary<string, ZGuid[]> keys = new Dictionary<string, ZGuid[]>();
			keys.Add("Key1", new ZGuid[] { ZGuid.NewZGuid() });
			keys.Add("Key2", new ZGuid[] { ZGuid.NewZGuid(), ZGuid.NewZGuid() });

			string actualHandlerUrl = dummyRequestHelper.GetHandlerUrl(keys);
			string expectedHandlerUrl = dummyRequestHelper.BaseUrl + "?Key1=" + WebUtility.UrlEncode(keys["Key1"][0].ToString()) + "&Key2=" + WebUtility.UrlEncode(keys["Key2"][0].ToString()) + "," + WebUtility.UrlEncode(keys["Key2"][1].ToString());

			AssertEquals("The handler URL must be URL encoded and contain two Keys and three PKs.", expectedHandlerUrl, actualHandlerUrl);

			dummyRequestHelper.SetUseSecureQueryString(true);
			actualHandlerUrl = dummyRequestHelper.GetHandlerUrl(keys);

			SecureQueryString qs = new SecureQueryString();

			foreach (string key in keys.Keys)
			{
				qs.Add(key, DataRequestHelper.GetGuidsAsString(keys[key]));
			}

			expectedHandlerUrl = String.Format("{0}?{1}={2}", dummyRequestHelper.BaseUrl, SecureQueryString.QueryStringKey, WebUtility.UrlEncode(qs.ToString()));
			AssertEquals("The handler URL must be encrypted and URL encoded, and contain the three  PKs.", expectedHandlerUrl, actualHandlerUrl);
		}

		class DataRequestHelperDummy : DataRequestHelper
		{
			public override string BaseUrl
			{
				get { return "TestHandler.axd"; }
			}

			public override bool EnableCache
			{
				get { return true; }
			}

			public override bool UseSecureQueryString
			{
				get { return fUseSecureQueryString; }
			}
			bool fUseSecureQueryString = true;

			public void SetUseSecureQueryString(bool value)
			{
				fUseSecureQueryString = value;
			}
		}
	}
}
