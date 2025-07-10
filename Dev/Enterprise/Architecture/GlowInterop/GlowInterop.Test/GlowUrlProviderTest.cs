using System;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace Enterprise.ZArchitecture.GlowInterop.Test
{
	class GlowUrlProviderTest : TestCaseWithFactory
	{
		public void TestConstructor_Null()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new GlowUrlProvider(null));
		}

		public void TestRegistry_Empty() => TestRegistry_NullOrWhiteSpace(string.Empty);
		public void TestRegistry_Space() => TestRegistry_NullOrWhiteSpace(" ");
		public void TestRegistry_Tab() => TestRegistry_NullOrWhiteSpace("\t");
		public void TestRegistry_NewLine() => TestRegistry_NullOrWhiteSpace("\r\n");

		void TestRegistry_NullOrWhiteSpace(string baseUrl)
		{
			var notificationsMock = new Mock<INotifications>();
			try
			{
				GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, baseUrl);
			}
			catch (RegistryValidationException)
			{
				// Swallow exception to enable us to bypass validation test whitespace URLs in the registry to emulate bad data in the database.
			}

			AssertNull(new GlowUrlProvider(notificationsMock.Object).TryGenerateUrl("/PRE/Desktop"));
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(e => e.Type.EnumValueName == (NotificationType.Error.EnumValueName) && e.Message == @"This module cannot be opened in a browser as GLOW has not been configured for this client.
Registry: GLOW/Services/GLOW Portals Root URL")));
		}

		public void TestTryGenerateUrl_ParametersNotNull()
		{
			var notificationsMock = new Mock<INotifications>();
			var helper = new GlowUrlProvider(notificationsMock.Object);

			AssertExceptionThrown<ArgumentNullException>(() => helper.TryGenerateUrl(null));
			AssertExceptionThrown<ArgumentNullException>(() => helper.TryGenerateUrl("/PRE/Desktop", null));
		}

		public void TestTryGenerateUrl()
		{
			var singleSignOnHelperMock = new Mock<IGlowSingleSignOnTokenProvider>();
			singleSignOnHelperMock
				.Setup(h => h.CreateLimitedToken(Env.CurrentUserPK, "GS", It.IsAny<GlowSingleSignOnTokenOptions>()))
				.Returns("sometoken");

			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var notificationsMock = new Mock<INotifications>();

			using (ObjectFactory.Substitute(singleSignOnHelperMock.Object))
			{
				var helper = new GlowUrlProvider(notificationsMock.Object).TryGenerateUrl("/PRE/Desktop");
				AssertEquals("Launched uri is correct.", "https", helper.Scheme);
				AssertEquals("Launched uri is correct.", "address", helper.Host);
				AssertEquals("Launched uri is correct.", "/PRE/Desktop", helper.AbsolutePath);
				AssertEquals("Launched uri is correct.", string.Empty, helper.Fragment);
				AssertEquals("Launched uri is correct.", "https://address/PRE/Desktop?sso_otp=sometoken", helper.AbsoluteUri);
			}

			notificationsMock.Verify(n => n.Add(It.IsAny<INotification>()), Times.Never);
		}

		public void TestTryGenerateUrl_Endpoint_Empty()
		{
			var notificationsMock = new Mock<INotifications>();
			var helper = new GlowUrlProvider(notificationsMock.Object);

			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var ex = AssertExceptionThrown<ArgumentException>(() => new GlowUrlProvider(notificationsMock.Object).TryGenerateUrl("", "Production Rules Engine portal"));

#if NETFRAMEWORK
			AssertEquals(@"Value cannot be empty string ("""").
Parameter name: endpoint", ex.Message);

#else
			AssertEquals(@"Value cannot be empty string (""""). (Parameter 'endpoint')", ex.Message);
#endif
		}

		public void TestTryGenerateUrl_Endpoint_Null()
		{
			var notificationsMock = new Mock<INotifications>();
			var helper = new GlowUrlProvider(notificationsMock.Object);

			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var ex = AssertExceptionThrown<ArgumentException>(() => new GlowUrlProvider(notificationsMock.Object).TryGenerateUrl(null, "Production Rules Engine portal"));
#if NETFRAMEWORK
			AssertEquals(@"Value cannot be null.
Parameter name: endpoint", ex.Message);

#else
			AssertEquals(@"Value cannot be null. (Parameter 'endpoint')", ex.Message);
#endif
		}

		public void TestTryGenerateUrl_ReturnErrorWhenGlowIsNotConfiguredInRegistry_WithJobDescription()
		{
			var notificationsMock = new Mock<INotifications>();
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);

			AssertNull(new GlowUrlProvider(notificationsMock.Object).TryGenerateUrl("/PRE/Desktop", "Production Rules Engine portal"));
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(e => e.Type.EnumValueName == (NotificationType.Error.EnumValueName) && e.Message == @"This Production Rules Engine portal cannot be opened in a browser as GLOW has not been configured for this client.
Registry: GLOW/Services/GLOW Portals Root URL")));
		}

		public void TestHandleLinkClick_ReturnErrorWhenGlowIsNotConfiguredInRegistry_WithoutJobDescription()
		{
			var notificationsMock = new Mock<INotifications>();
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);

			AssertNull(new GlowUrlProvider(notificationsMock.Object).TryGenerateUrl("/PRE/Desktop"));
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(e => e.Type.EnumValueName == (NotificationType.Error.EnumValueName) && e.Message == @"This module cannot be opened in a browser as GLOW has not been configured for this client.
Registry: GLOW/Services/GLOW Portals Root URL")));
		}

		public void TestTryGenerateUrl_WithAdditionalQueryStrings()
		{
			var singleSignOnHelperMock = new Mock<IGlowSingleSignOnTokenProvider>();
			singleSignOnHelperMock
				.Setup(h => h.CreateLimitedToken(Env.CurrentUserPK, "GS", It.IsAny<GlowSingleSignOnTokenOptions>()))
				.Returns("sometoken");

			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var notificationsMock = new Mock<INotifications>();

			using (ObjectFactory.Substitute(singleSignOnHelperMock.Object))
			{
				var helper = new GlowUrlProvider(notificationsMock.Object).TryGenerateUrl("/PRE/Desktop", "", additionalQueryStrings: new[] { ("entityPK", "dummy123") });
				AssertEquals("Launched uri is correct.", "https", helper.Scheme);
				AssertEquals("Launched uri is correct.", "address", helper.Host);
				AssertEquals("Launched uri is correct.", "/PRE/Desktop", helper.AbsolutePath);
				AssertEquals("Launched uri is correct.", string.Empty, helper.Fragment);
				Assert("Launched uri is correct.", helper.Query.Contains("entityPK=dummy123"));
				AssertEquals("Launched uri is correct.", "https://address/PRE/Desktop?sso_otp=sometoken&entityPK=dummy123", helper.AbsoluteUri);
			}

			notificationsMock.Verify(n => n.Add(It.IsAny<INotification>()), Times.Never);
		}
	}
}
