using System;
using System.Web;
using CargoWise.IO;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZPageNoUserTest : ZPageTestCase
	{
		[HttpContextEnabledTest]
		public void TestResolveUnauthenticatedUser()
		{
			using (var temp = new TempDirectory())
			{
				Page.SetServerMappedPathForTest(temp.DirectoryName);
				AssertNull("Precondition", Page.SiteUser);
				Assert("Precondition", !SessionStateContainer.IsAbandoned);

				Page.OnInitInternal(EventArgs.Empty);

				Assert(SessionStateContainer.IsAbandoned);
				Assert(HttpContext.Current.Response.IsRequestBeingRedirected);
				AssertEquals("/Login.aspx", HttpContext.Current.Response.RedirectLocation);
			}
		}

		[ExpectNoExceptions("TranslationFeedbackManager should not blow up")]
		public void TestTranslationFeedbackManagerWhenSiteUserNull()
		{
			using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.French))
			{
				Page.OnPreLoadInternal(EventArgs.Empty);

				Page.IsPostBack = true;

				Page.OnPreLoadInternal(EventArgs.Empty);
			}
		}

		protected override ZPage GetNewZPage()
		{
			return new ZTestPageNoUser();
		}

		class ZTestPageNoUser : ZTestPage
		{
			protected override ZGlobal GetNewTestGlobal()
			{
				return new ZGlobalNoUser();
			}

			protected override bool PageRequiresLogin(Uri url)
			{
				return true;
			}
		}

		class ZGlobalNoUser : ZTestGlobal
		{
			public override WebUser SiteUser => null;
		}
	}
}
