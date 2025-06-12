using System;
using System.Linq;
using System.Net;
using System.Web;

public class IPNetworking
{
	public static string GetRequesterIP4Address()
	{
		var hostAddresses = new IPAddress[0];

		var currentHttpContext = HttpContext.Current;
		if (currentHttpContext != null)
		{
			var request = currentHttpContext.Request;
			if (request != null)
			{
				var userHostAddress = request.UserHostAddress;
				if (userHostAddress != null) hostAddresses = Dns.GetHostAddresses(userHostAddress);
			}
		}

		var ip4Address = FindIP4Address(hostAddresses);
		return string.IsNullOrEmpty(ip4Address) ? unknown : ip4Address;
	}

	public static string GetLocalIP4Address()
	{
		var hostAddresses = Dns.GetHostAddresses(Dns.GetHostName());
		var ip4Address = FindIP4Address(hostAddresses);
		return string.IsNullOrEmpty(ip4Address) ? unknown : ip4Address;
	}

	static string FindIP4Address(IPAddress[] ipAddresses)
	{
		foreach (var ipAddress in ipAddresses)
		{
			if (ipAddress.AddressFamily.ToString() == ip4AddressFamily)
			{
				return ipAddress.ToString();
			}
		}

		return string.Empty;
	}

	const string ip4AddressFamily = "InterNetwork";
	const string unknown = "<UNKNOWN>";
}