using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class EndpointNameRegistryItem : StringRegistryItem
	{
		public EndpointNameRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, string defaultValue)
			: base(name, category, caption, hint, storage, options, defaultValue)
		{
		}

		protected override object ValueCore
		{
			get
			{
				var siteName = Regex.Match(GetHostName(), @"^[\w]{0,3}").Value;
				return string.Format(CultureInfo.InvariantCulture, (string)base.ValueCore, siteName);
			}
		}

		[SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
		public static string GetHostName()
		{
			var hostname = Dns.GetHostName();
			try
			{
				return Dns.GetHostEntry(hostname).HostName.ToLowerInvariant();
			}
			catch (SocketException)
			{
				return hostname.ToLowerInvariant();
			}
		}
	}
}
