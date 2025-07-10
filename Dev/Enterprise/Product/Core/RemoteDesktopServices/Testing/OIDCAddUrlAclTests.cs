using System;
using CargoWise.Interop;
using Enterprise.RemoteDesktopServices.Server;
using NUnit.Framework;

namespace Enterprise.RemoteDesktopServices.Testing
{
	class OIDCAddUrlAclTests : TestCase
	{
		[TestRequiresAdministrativePrivileges("UrlAcl cleanup, need to run in CW1 app otherwise AddUrlAcl.EnsureCallbackUrlConfigured will fail due to unloaded dll.")]
		public void TestAclCanBeAdded()
		{
			var url = $"http://127.0.0.1:80/CargowiseOne/{Guid.NewGuid().ToString("N")}/";
			try
			{
				AddUrlAcl.EnsureCallbackUrlConfigured(url);

				using (var httpApi = new HttpApi())
				{
					AssertNotNull(httpApi.GetHttpServiceConfigUrlAclInfo(url));
				}
			}
			finally
			{
				using (var httpApi = new HttpApi())
				{
					httpApi.DeleteHttpServiceConfigUrlAclInfo(url);
				}
			}
		}
	}
}
