using System.Collections.Generic;
using System.Net;
using System.Web;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public static class RequestAuthorisationHelper
	{
		public static bool IsRequestPermitted(HttpRequest request)
		{
			return IPAddress.TryParse(request.UserHostAddress, out var address) && IsRequestPermittedFromAddress(address);
		}

		public static bool IsRequestPermittedFromAddress(IPAddress address)
		{
			return InternalTestAllowedIpRanges.IsInRange(address);
		}

		static IEnumerable<IPAddressRange> InternalTestAllowedIpRanges
		{
			get { yield return new IPAddressRange("10.0.0.0/8"); }
		}
	}
}
