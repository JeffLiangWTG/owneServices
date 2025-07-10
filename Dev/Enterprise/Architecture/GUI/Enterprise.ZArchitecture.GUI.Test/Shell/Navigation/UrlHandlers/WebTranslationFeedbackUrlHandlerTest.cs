using System;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	class WebTranslationFeedbackUrlHandlerTest : TestCaseWithFactory
	{
		public void TestHandleUrlsWithinTrustedDomains()
		{
			var handler = new WebTranslationFeedbackUrlHandler();
			var queryString = new QueryString("l=ZH-CN&c=状态");
			AssertNull("Pre-condition, url should be null.", queryString["u"]);
			AssertEquals("The null request should be handled.", true, handler.Handle(queryString));

			queryString = new QueryString("l=ZH-CN&c=状态&u=https://host/route/subRoute");
			AssertEquals("Pre-condition, Trusted domain list should be empty by default.", 0, Env.Registry.ResourceStringUsageTrustedDomains.Length);

			handler.Handle(queryString);
			AssertEquals("Invalid parameter, Detect untrusted domain https://host.", UnitTestUserNotification.Instance.LastMessage.Text);

			using (Env.Registry.RawRegistry.ResourceStringUsageTrustedDomains.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new string[] { "https://host" }))
			{
				AssertEquals("The null request should be handled.", true, handler.Handle(queryString));
			}
		}
	}
}
