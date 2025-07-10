using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Web;
using System.Web.Configuration;
using CargoWise.Common;
using Enterprise.Client.EDI;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class RequestIPValidationHelper
	{
		public Tuple<bool, string> IsCurrentRequestFromValidIP()
		{
			var userHostAddress = HttpContext.Current.Request.UserHostAddress;
			if (EDIDataRegistry.Instance.DisableMyAccountWhitelisting.Value)
			{
				return Tuple.Create(true, userHostAddress);
			}

			var ipAddresses = new List<IPAddress>();

			var hostnames = GetHostnames() ?? Array.Empty<string>();
			foreach (var hostname in hostnames)
			{
				try
				{
					ipAddresses.AddRange(Dns.GetHostAddresses(hostname));
				}
				catch (SocketException ex)
				{
					ErrorReporter.ReportOnce(FormattableString.Invariant($"WebServiceAccessList has an invalid hostname: {hostname}. It should be removed from web.config."), ex);
				}
			}

			return Tuple.Create(ipAddresses.Any(ip => ip.ToString() == userHostAddress), userHostAddress);
		}

		protected virtual string[] GetHostnames()
		{
			var webConfig = WebConfigurationManager.OpenWebConfiguration("~");
			if (webConfig.AppSettings.Settings.Count > 0)
			{
				var accessListSetting = webConfig.AppSettings.Settings["WebServiceAccessList"];
				return accessListSetting?.Value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			}

			return null;
		}
	}
}
