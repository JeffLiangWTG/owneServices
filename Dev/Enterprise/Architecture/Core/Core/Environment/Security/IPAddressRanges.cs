using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;

namespace Enterprise.ZArchitecture.Environment
{
	public class IPAddressRange
	{
		public IPAddressRange(string cidrNotation)
		{
			this.cidrNotation = cidrNotation;
			var parts = cidrNotation.Split('/');
			if (parts.Length > 2)
			{
				throw new FormatException(cidrNotation + " is not a valid IP address or IP address range");
			}
			if (string.Compare("LOCALHOST", cidrNotation, StringComparison.OrdinalIgnoreCase) == 0)
			{
				IsLocalhost = true;
			}
			else
			{
				cidrAddress = BitConverter.ToInt32(IPAddress.Parse(parts[0]).GetAddressBytes(), 0);
				int cidrMaskPart = parts.Length > 1 ? int.Parse(parts[1], CultureInfo.InvariantCulture) : 32;
				if (cidrMaskPart < 1 || cidrMaskPart > 32)
				{
					throw new FormatException(cidrMaskPart + " is not a valid subnet length, valid lengths are from 1 to 32.");
				}
				cidrMask = IPAddress.HostToNetworkOrder(-1 << (32 - cidrMaskPart));
			}
		}

		public bool IsInRange(IPAddress ipAddress)
		{
			return !IsLocalhost &&
				(BitConverter.ToInt32(ipAddress.GetAddressBytes(), 0) & cidrMask) == (cidrAddress & cidrMask);
		}

		public override string ToString()
		{
			return cidrNotation;
		}

		public bool IsLocalhost { get; }

		readonly string cidrNotation;
		readonly int cidrAddress;
		readonly int cidrMask;
	}

	public static class IPAddressRanges
	{
		public static bool IsInRange(this IEnumerable<IPAddressRange> ranges, IPAddress address)
		{
			return ranges.Any(range => range.IsInRange(address));
		}

		public static IEnumerable<IPAddressRange> Parse(string rangesValue)
		{
			return rangesValue.Split(';', ',').Select(o => new IPAddressRange(o.Trim()));
		}
	}
}
