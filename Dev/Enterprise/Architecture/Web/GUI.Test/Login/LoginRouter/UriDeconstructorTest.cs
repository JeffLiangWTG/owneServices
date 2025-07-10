using System;
using System.Net;
using System.Web;
using CargoWise.Common;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.Login.Testing
{
	sealed class UriDeconstructorTest : TestCase
	{
		public void TestPropertiesAbsoluteUri()
		{
			var uriBuilder = new UriBuilder("http://google.com.au/");
			const string originalUrl = "https://yahoo.com.au/";
			var secureQueryString = new SecureQueryString
			{
				[LoginRouter.OriginalUrlQueryStringKey] = originalUrl
			};

			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			var query = HttpUtility.ParseQueryString(uriBuilder.Uri.Query);
			query.Add("blah", encodedQueryString);
			uriBuilder.Query = query.ToString();

			var uriDeconstructor = new UriDeconstructor(uriBuilder.Uri);
			AssertEquals(uriBuilder.Uri.AbsoluteUri, uriDeconstructor.OriginalUri.AbsoluteUri);
			AssertEquals("http://google.com.au/", uriDeconstructor.BaseUrl);

			var deconstructedQuery = HttpUtility.ParseQueryString(uriDeconstructor.Query);
			AssertEquals(encodedQueryString, deconstructedQuery["blah"]);
		}

		public void TestPropertiesRelativeUri()
		{
			const string originalUrl = "https://yahoo.com.au/";
			var secureQueryString = new SecureQueryString
			{
				[LoginRouter.OriginalUrlQueryStringKey] = originalUrl
			};

			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			var uri = new Uri(FormattableString.Invariant($"/webapp/Login/Login.aspx?blah={encodedQueryString}"), UriKind.Relative);

			var uriDeconstructor = new UriDeconstructor(uri);
			AssertEquals(uri.OriginalString, uriDeconstructor.OriginalUri.OriginalString);
			AssertEquals("/webapp/Login/Login.aspx", uriDeconstructor.BaseUrl);

			var deconstructedQuery = HttpUtility.ParseQueryString(uriDeconstructor.Query);
			AssertEquals(secureQueryString.ToString(), deconstructedQuery["blah"]);
		}

		public void TestGetUriWithNewQuery()
		{
			const string originalUrl = "https://yahoo.com.au/";
			var secureQueryString = new SecureQueryString
			{
				[LoginRouter.OriginalUrlQueryStringKey] = originalUrl
			};

			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			var uri = new Uri(FormattableString.Invariant($"/webapp/Login/Login.aspx?blah={encodedQueryString}"), UriKind.Relative);

			var uriDeconstructor = new UriDeconstructor(uri);
			var deconstructedQuery = HttpUtility.ParseQueryString(uriDeconstructor.Query);
			deconstructedQuery.Add("snap", "cracklePop");

			var newUri = uriDeconstructor.GetUriWithNewQuery(deconstructedQuery.ToString());
			var newUriDeconstructor = new UriDeconstructor(newUri);
			AssertEquals(newUri.OriginalString, newUriDeconstructor.OriginalUri.OriginalString);
			AssertEquals("/webapp/Login/Login.aspx", newUriDeconstructor.BaseUrl);

			var deconstructedQuery2 = HttpUtility.ParseQueryString(newUriDeconstructor.Query);
			AssertEquals(secureQueryString.ToString(), deconstructedQuery2["blah"]);
			AssertEquals("cracklePop", deconstructedQuery2["snap"]);
		}

		public void TestGetUriPathAndQueryWithRelativeUri()
		{
			var uri = new Uri("/some/relative/path/page.aspx?old1=oldValue1&old2=oldValue2", UriKind.Relative);
			var deconstructor = new UriDeconstructor(uri);

			AssertEquals("/some/relative/path/page.aspx?new1=newValue1&new2=newValue2", deconstructor.GetUriPathAndQuery("new1=newValue1&new2=newValue2"));
		}

		public void TestGetUriPathAndQueryWithAbsoluteUri()
		{
			var uri = new Uri("http://www.test.com/some/absolute/path/page.aspx?old1=oldValue1&old2=oldValue2", UriKind.Absolute);
			var deconstructor = new UriDeconstructor(uri);

			AssertEquals("http://www.test.com/some/absolute/path/page.aspx?new1=newValue1&new2=newValue2", deconstructor.GetUriPathAndQuery("new1=newValue1&new2=newValue2"));
		}
	}
}
