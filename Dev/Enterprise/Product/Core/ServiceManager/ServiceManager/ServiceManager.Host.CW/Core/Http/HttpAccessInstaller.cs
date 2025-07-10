using System.Collections;
using System.Configuration.Install;
using System.Security.Principal;
using CargoWise.Interop;

namespace ServiceManager.Host.CW
{
	public class HttpAccessInstaller : Installer
	{
		public override void Install(IDictionary stateSaver)
		{
			base.Install(stateSaver);

			using (var httpApi = new HttpApi())
			{
				var oldDescriptor = httpApi.GetHttpServiceConfigUrlAclInfo(Url);
				if (oldDescriptor != null)
				{
					stateSaver["oldDescriptor"] = oldDescriptor;
					httpApi.DeleteHttpServiceConfigUrlAclInfo(Url);
				}
				if (Sid != null)
				{
					var securityDescriptor = string.Format("D:(A;;GX;;;{0})", Sid.Value);
					httpApi.SetHttpServiceConfigUrlAclInfo(Url, securityDescriptor);
				}
			}
		}

		public override void Rollback(IDictionary savedState)
		{
			base.Rollback(savedState);

			using (var httpApi = new HttpApi())
			{
				if (httpApi.GetHttpServiceConfigUrlAclInfo(Url) != null)
				{
					httpApi.DeleteHttpServiceConfigUrlAclInfo(Url);
				}
				if (savedState.Contains("oldDescriptor"))
				{
					httpApi.SetHttpServiceConfigUrlAclInfo(Url, (string)savedState["oldDescriptor"]);
				}
			}
		}

		public override void Uninstall(IDictionary savedState)
		{
			base.Uninstall(savedState);

			using (var httpApi = new HttpApi())
			{
				if (httpApi.GetHttpServiceConfigUrlAclInfo(Url) != null)
				{
					httpApi.DeleteHttpServiceConfigUrlAclInfo(Url);
				}
			}
		}

		public string Url
		{
			get { return url; }
			set { url = value; }
		}

		string url;

		public SecurityIdentifier Sid
		{
			get { return sid; }
			set { sid = value; }
		}

		SecurityIdentifier sid;
	}
}
