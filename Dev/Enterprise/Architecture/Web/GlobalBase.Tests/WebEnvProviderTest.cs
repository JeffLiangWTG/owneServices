using System.Web;
using AppDomainWrappers.Net;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Web.Shared.Test;
using NUnit.Framework;
using static System.FormattableString;

namespace Enterprise.ZArchitecture.Web.GlobalBase.Tests
{
	class WebEnvProviderTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestGetDbEnvironmentInstance()
		{
			using (var appDomainWrapper = new AppDomainWrapper(Invariant($@"/LM/W3SVC/3/ROOT-1-123456789012345678")))
			{
				appDomainWrapper.RunActionInAppDomain(() =>
				{
					using (var provider = new WebEnvProvider())
					{
						provider.Enable();
						AssertType<BaseWebDbEnvironment>(DbEnv.Instance);
					}
				});
			}
		}

		public void TestWebEnvironmentWithoutHttpContextOrSession()
		{
			using (var provider = new WebEnvProvider())
			using (var webEnvironment = provider.Instance)
			{
				AssertNull(HttpContext.Current);
				Assert(webEnvironment is WebEnvironment);
			}

			Assert(AssemblyLoader.Instance is DefaultAssemblyLoader);
		}

		[TestRequiresAdministrativePrivileges("Admin privilege is required to edit the Registry")]
		public void TestWebEnvironmentWithHttpContextSession()
		{
			using (TestHttpContextHelper.DisposableSession(out _))
			using (var provider = new WebEnvProvider())
			using (var webEnvironment = provider.Instance)
			{
				AssertNotNull(HttpContext.Current);
				AssertNotNull(HttpContext.Current.Session);
				Assert(webEnvironment is WebEnvironment);
				AssertNotNull(HttpContext.Current?.Session?[WebEnvProvider.EnvironmentSessionKey]);
			}

			Assert(AssemblyLoader.Instance is DefaultAssemblyLoader);
		}

		[TestRequiresAdministrativePrivileges("Admin privilege is required to edit the Registry")]
		public void TestWebEnvironmentWithHttpContext()
		{
			using (TestHttpContextHelper.DisposableHttpContext(out _, out _))
			using (var provider = new WebEnvProvider())
			using (var webEnvironment = provider.Instance)
			{
				AssertNotNull(HttpContext.Current);
				AssertNull(HttpContext.Current.Session);
				Assert(webEnvironment is WebEnvironment);
				AssertNotNull(HttpContext.Current.Items[WebEnvProvider.EnvironmentSessionKey]);
			}

			Assert(AssemblyLoader.Instance is DefaultAssemblyLoader);
		}

		public void TestWebEnvironmentWithContextManager()
		{
			using (TestHttpContextHelper.DisposableHttpContext(out _, out _))
			using (var provider = new WebEnvProvider(() => new SessionUserContextManager()))
			using (var webEnvironment = provider.Instance)
			{
				AssertNotNull(HttpContext.Current);
				AssertNull(HttpContext.Current.Session);
				Assert(webEnvironment is WebEnvironment);
				AssertNotNull(HttpContext.Current.Items[WebEnvProvider.EnvironmentSessionKey]);
			}

			Assert(AssemblyLoader.Instance is DefaultAssemblyLoader);
		}
	}
}
