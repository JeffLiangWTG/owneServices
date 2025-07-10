using System;
using CargoWise.Application;
using Enterprise.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GlowInterop.Test
{
	class UrlBuilderTest : TestCase
	{
		public void TestGenerateURL_WithBaseURLLocalPath()
		{
			var accessToken = "sometoken";
			var baseUri = "http://address.com/woop/";
			var relativePath = "ABC";
			var fragment = "/foo/bar";

			var tokenProvider = CreateAndSetupTokenProvider(accessToken, Env.CurrentUserPK, UserType.Staff);
			ObjectFactory.Substitute(tokenProvider);

			var actualUri = UrlBuilder.GenerateURL(new Uri(baseUri), relativePath, fragment);
			const string ExpectedUri = "http://address.com/woop/ABC?sso_otp=sometoken#/foo/bar";
			AssertEquals("Url should be constructed in defined format", ExpectedUri, actualUri.ToString());
		}

		public void TestGenerateURL_WithBaseURLLocalPath_NoSlash()
		{
			var accessToken = "sometoken";
			var baseUri = "http://address.com/woop";
			var relativePath = "ABC";
			var fragment = "/foo/bar";
			var tokenProvider = CreateAndSetupTokenProvider(accessToken, Env.CurrentUserPK, UserType.Staff);
			ObjectFactory.Substitute(tokenProvider);

			var actualUri = UrlBuilder.GenerateURL(new Uri(baseUri), relativePath, fragment);
			const string ExpectedUri = "http://address.com/woop/ABC?sso_otp=sometoken#/foo/bar";
			AssertEquals("Url should be constructed in defined format", ExpectedUri, actualUri.ToString());
		}

		public void TestGenerateURL_WithoutBaseURLLocalPath()
		{
			var accessToken = "sometoken";
			var baseUri = "http://address.com/";
			var relativePath = "ABC";
			var fragment = "/foo/bar";
			var tokenProvider = CreateAndSetupTokenProvider(accessToken, Env.CurrentUserPK, UserType.Staff);
			ObjectFactory.Substitute(tokenProvider);

			var actualUri = UrlBuilder.GenerateURL(new Uri(baseUri), relativePath, fragment);
			const string ExpectedUri = "http://address.com/ABC?sso_otp=sometoken#/foo/bar";
			AssertEquals("Url should be constructed in defined format", ExpectedUri, actualUri.ToString());
		}

		public void TestGenerateURL_MultiSegmentsRelativePath()
		{
			var accessToken = "sometoken";
			var baseUri = "http://address.com/woop";
			var relativePath = "ABC/" + FormFactor.Desktop;
			var fragment = "/foo/bar";
			var tokenProvider = CreateAndSetupTokenProvider(accessToken, Env.CurrentUserPK, UserType.Staff);
			ObjectFactory.Substitute(tokenProvider);

			var actualUri = UrlBuilder.GenerateURL(new Uri(baseUri), relativePath, fragment);
			const string ExpectedUri = "http://address.com/woop/ABC/Desktop?sso_otp=sometoken#/foo/bar";
			AssertEquals("Url should be constructed in defined format", ExpectedUri, actualUri.ToString());
		}

		public void TestGenerateCaptiveSessionURL()
		{
			var accessToken = "sometoken";
			var baseUri = "http://address.com/woop/";
			var relativePath = "ABC";
			var fragment = "/foo/bar";
			var tokenProvider = CreateAndSetupTokenProvider(accessToken, Env.CurrentUserPK, UserType.Staff, isCaptiveSession: true);
			ObjectFactory.Substitute(tokenProvider);

			var actualUri = UrlBuilder.GenerateCaptiveSessionURL(new Uri(baseUri), relativePath, fragment);

			const string ExpectedUri = "http://address.com/woop/ABC?sso_otp=sometoken#/foo/bar";
			AssertEquals("Url should be constructed in defined format", ExpectedUri, actualUri.ToString());
		}

		public void TestGenerateURLForContact_WithBaseURLLocalPath()
		{
			var contactPK = Guid.NewGuid();
			var accessToken = "sometoken";
			var baseUri = "http://address.com/woop/";
			var relativePath = "ABC";
			var fragment = "/foo/bar";
			var tokenProvider = CreateAndSetupTokenProvider(accessToken, contactPK, UserType.Contact);
			ObjectFactory.Substitute(tokenProvider);

			var actualUri = UrlBuilder.GenerateURLForContact(contactPK, new Uri(baseUri), relativePath, fragment);
			const string ExpectedUri = "http://address.com/woop/ABC?sso_otp=sometoken#/foo/bar";
			AssertEquals("Url should be constructed in defined format", ExpectedUri, actualUri.ToString());
		}

		public void TestGenerateURLForContact_WithBaseURLLocalPath_NoSlash()
		{
			var contactPK = Guid.NewGuid();
			var accessToken = "sometoken";
			var baseUri = "http://address.com/woop";
			var relativePath = "ABC";
			var fragment = "/foo/bar";
			var tokenProvider = CreateAndSetupTokenProvider(accessToken, contactPK, UserType.Contact);
			ObjectFactory.Substitute(tokenProvider);

			var actualUri = UrlBuilder.GenerateURLForContact(contactPK, new Uri(baseUri), relativePath, fragment);
			const string ExpectedUri = "http://address.com/woop/ABC?sso_otp=sometoken#/foo/bar";
			AssertEquals("Url should be constructed in defined format", ExpectedUri, actualUri.ToString());
		}

		public void TestGenerateURLForContact_WithoutBaseURLLocalPath()
		{
			var contactPK = Guid.NewGuid();
			var accessToken = "sometoken";
			var baseUri = "http://address.com/";
			var relativePath = "ABC";
			var fragment = "/foo/bar";
			var tokenProvider = CreateAndSetupTokenProvider(accessToken, contactPK, UserType.Contact);
			ObjectFactory.Substitute(tokenProvider);

			var actualUri = UrlBuilder.GenerateURLForContact(contactPK, new Uri(baseUri), relativePath, fragment);
			const string ExpectedUri = "http://address.com/ABC?sso_otp=sometoken#/foo/bar";
			AssertEquals("Url should be constructed in defined format", ExpectedUri, actualUri.ToString());
		}

		public void TestGenerateURLForContact_MultiSegmentsRelativePath()
		{
			var contactPK = Guid.NewGuid();
			var accessToken = "sometoken";
			var baseUri = "http://address.com/woop";
			var relativePath = "ABC/" + FormFactor.Mobile;
			var fragment = "/foo/bar";
			var tokenProvider = CreateAndSetupTokenProvider(accessToken, contactPK, UserType.Contact);
			ObjectFactory.Substitute(tokenProvider);

			var actualUri = UrlBuilder.GenerateURLForContact(contactPK, new Uri(baseUri), relativePath, fragment);
			const string ExpectedUri = "http://address.com/woop/ABC/Mobile?sso_otp=sometoken#/foo/bar";
			AssertEquals("Url should be constructed in defined format", ExpectedUri, actualUri.ToString());
		}

		public void TestGenerateURL_WithExistingQueryStrings()
		{
			var accessToken = "sometoken";
			var baseUri = "http://address.com/woop?baz=123";
			var relativePath = "ABC";
			var fragment = "/foo/bar";
			var tokenProvider = CreateAndSetupTokenProvider(accessToken, Env.CurrentUserPK, UserType.Staff);
			ObjectFactory.Substitute(tokenProvider);

			var actualUri = UrlBuilder.GenerateURL(new Uri(baseUri), relativePath, fragment);
			const string ExpectedUri = "http://address.com/woop/ABC?baz=123&sso_otp=sometoken#/foo/bar";
			AssertEquals("Url should be constructed in defined format", ExpectedUri, actualUri.ToString());
		}

		public void TestGenerateURL_WithExistingAndAdditionalQueryStrings()
		{
			var accessToken = "sometoken";
			var baseUri = "http://address.com/woop?baz=123";
			var relativePath = "ABC";
			var fragment = "/foo/bar";
			var tokenProvider = CreateAndSetupTokenProvider(accessToken, Env.CurrentUserPK, UserType.Staff);
			ObjectFactory.Substitute(tokenProvider);

			var actualUri = UrlBuilder.GenerateURL(new Uri(baseUri), relativePath, fragment, new[] { ("firstName", "Ana"), ("lastName", "De Armas") });
			const string ExpectedUri = "http://address.com/woop/ABC?baz=123&sso_otp=sometoken&firstName=Ana&lastName=De+Armas#/foo/bar";
			AssertEquals("Url should be constructed in defined format", ExpectedUri, actualUri.ToString());
		}

		IGlowSingleSignOnTokenProvider CreateAndSetupTokenProvider(string expectedReturnedToken, Guid userPK, UserType userType, bool isCaptiveSession = false)
		{
			var singleSignOnHelperMock = new Mock<IGlowSingleSignOnTokenProvider>();
			var tablePrefix = userType == UserType.Contact ? "OC" : "GS";
			singleSignOnHelperMock
				.Setup(h => h.CreateLimitedToken(userPK, tablePrefix, It.Is<GlowSingleSignOnTokenOptions>(o => o.IsCaptiveSession == isCaptiveSession)))
				.Returns(expectedReturnedToken);

			return singleSignOnHelperMock.Object;
		}

		enum UserType
		{
			Contact,
			Staff,
		}
	}
}
