using System;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Moq;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class GlowLinksHelperTest : TestCaseWithFactory
	{
		public void TestOpenEnityInGlow_NullBusinessObject()
		{
			AssertExceptionThrown<ArgumentNullException>(() => GlowLinksHelper.OpenEnityInGlow(new Mock<INotifications>().Object, "goto/testAlias", null));
		}

		public void TestOpenEnityInGlow()
		{
			var singleSignOnHelperMock = new Mock<IGlowSingleSignOnTokenProvider>();
			singleSignOnHelperMock
				.Setup(h => h.CreateLimitedToken(Env.CurrentUserPK, "GS", It.IsAny<GlowSingleSignOnTokenOptions>()))
				.Returns("sometoken");

			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var notificationsMock = new Mock<INotifications>();

			var dummyBizO = Factory.New<DummyBusinessObject>();
			using (ObjectFactory.Substitute(singleSignOnHelperMock.Object))
			{
				GlowLinksHelper.OpenEnityInGlow(notificationsMock.Object, "goto/testAlias", dummyBizO);
				var launchedUrl = WebUrlLauncher.LastUrlLaunched;
				var uri = new Uri(launchedUrl, UriKind.Absolute);

				AssertEquals("Launched uri is correct.", "https", uri.Scheme);
				AssertEquals("Launched uri is correct.", "address", uri.Host);
				AssertEquals("Launched uri is correct.", "/goto/testAlias", uri.AbsolutePath);
				AssertEquals("Launched uri is correct.", string.Empty, uri.Fragment);
				Assert("Launched uri is correct.", uri.Query.Contains("entityPK=" + dummyBizO.PK.ToString()));
			}

			notificationsMock.Verify(n => n.Add(It.IsAny<INotification>()), Times.Never);
		}

		public void TestOpenEntityInGlow_WithAdditionalQueryStrings()
		{
			var singleSignOnHelperMock = new Mock<IGlowSingleSignOnTokenProvider>();
			singleSignOnHelperMock
				.Setup(h => h.CreateLimitedToken(Env.CurrentUserPK, "GS", It.IsAny<GlowSingleSignOnTokenOptions>()))
				.Returns("sometoken");

			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var notificationsMock = new Mock<INotifications>();

			var dummyBizO = Factory.New<DummyBusinessObject>();
			var additionalQueryStrings = new[]
			{
				("testParam1", "value1"),
				("testParam2", "value2")
			};

			using (ObjectFactory.Substitute(singleSignOnHelperMock.Object))
			{
				GlowLinksHelper.OpenEnityInGlow(notificationsMock.Object, "goto/testAlias", dummyBizO, additionalQueryStrings);
				var launchedUrl = WebUrlLauncher.LastUrlLaunched;
				var uri = new Uri(launchedUrl, UriKind.Absolute);

				AssertEquals("Launched uri is correct.", "https", uri.Scheme);
				AssertEquals("Launched uri is correct.", "address", uri.Host);
				AssertEquals("Launched uri is correct.", "/goto/testAlias", uri.AbsolutePath);
				AssertEquals("Launched uri is correct.", string.Empty, uri.Fragment);

				Assert("Launched uri is correct.", uri.Query.Contains("testParam1=value1"));
				Assert("Launched uri is correct.", uri.Query.Contains("testParam2=value2"));
			}

			notificationsMock.Verify(n => n.Add(It.IsAny<INotification>()), Times.Never);
		}

		public void TestOpenEnityInGlow_ReturnErrorWhenGlowIsNotConfiguredInRegistry()
		{
			var notificationsMock = new Mock<INotifications>();
			var dummyBizO = Factory.New<DummyBusinessObject>();
			WebUrlLauncher.ClearLastUrlLaunched();

			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			GlowLinksHelper.OpenEnityInGlow(notificationsMock.Object, "goto/testAlias", dummyBizO);

			AssertNullOrEmpty(WebUrlLauncher.LastUrlLaunched);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(e => e.Type.EnumValueName == (NotificationType.Error.EnumValueName) && e.Message == @"This DummyBizo cannot be opened in a browser as GLOW has not been configured for this client.
Registry: GLOW/Services/GLOW Portals Root URL")));
		}
	}
}
