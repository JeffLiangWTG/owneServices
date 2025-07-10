using System;
using System.Net;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Web
{
	[TestedType(typeof(eDocsRequestHelper))]
	sealed class eDocsRequestHelperTest : DataRequestHelperTestCase
	{
		public void TestGetHandlerUrl()
		{
			eDocsRequestHelper requestHelper = new eDocsRequestHelper();

			ZGuid documentPK = ZGuid.NewZGuid();
			ZGuid parentPK = ZGuid.NewZGuid();

			string actualHandlerUrl = requestHelper.GetHandlerUrl(documentPK, parentPK);
			string expectedHandlerUrl = String.Format("{0}?{1}={2}&{3}={4}", requestHelper.BaseUrl, eDocsRequestHelper.DocumentKey,
				WebUtility.UrlEncode(documentPK.ToString()), eDocsRequestHelper.ParentKey, WebUtility.UrlEncode(parentPK.ToString()));

			AssertEquals("The handler URL must contain the document and parent PKs.", expectedHandlerUrl, actualHandlerUrl);
		}

		protected override DataRequestHelper GetRequestHelper()
		{
			return new eDocsRequestHelper();
		}

		protected override bool ExpectedEnableCache
		{
			get { return true; }
		}

		protected override bool ExpectedUseSecureQueryString
		{
			get { return false; }
		}

		protected override string ExpectedBaseUrl
		{
			get { return "eDocsRequestHandler.axd"; }
		}
	}
}
