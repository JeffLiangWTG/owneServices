using System;
using System.Globalization;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Core
{
	public static class UriRegistryTypeValidator
	{
		public static void ValidateUri(string proposedValue, string protocol, bool shouldAllowAutoProtocolPrefixing)
		{
			ValidateUri(proposedValue, protocol, shouldAllowAutoProtocolPrefixing, true);
		}

		public static void ValidateUri(string proposedValue, string protocol, bool shouldAllowAutoProtocolPrefixing, bool shouldAllowQueryString)
		{
			if (!string.IsNullOrEmpty(proposedValue))
			{
				string serverName = AffixProtocolToServerNameIfNeeded(proposedValue, protocol, shouldAllowAutoProtocolPrefixing);
				if (!Uri.IsWellFormedUriString(serverName, UriKind.Absolute))
				{
					throw new RegistryValidationException(Res.GetString("24e4ad9d-6a04-4ab2-acfa-82f19ea00c2e", "{0} Address is not well formed.", protocol.ToUpper(CultureInfo.InvariantCulture)));
				}
				if (!shouldAllowQueryString && proposedValue.Contains("?"))
				{
					throw new RegistryValidationException(Res.GetString("290C8B5D-7E63-4DB3-8681-6FA9CB284CB4", "Address cannot contain query string ({0}).", proposedValue.Substring(proposedValue.IndexOf("?"))));
				}
			}
		}

		static string AffixProtocolToServerNameIfNeeded(string serverName, string protocol, bool shouldAllowAutoProtocolPrefixing)
		{
			var protocolWithDelimiter = protocol + Uri.SchemeDelimiter;

			if (serverName.StartsWith(protocolWithDelimiter, StringComparison.OrdinalIgnoreCase))
			{
				return serverName;
			}
			else if (serverName.Contains(Uri.SchemeDelimiter))
			{
				throw new RegistryValidationException(Res.GetString("6f3c6248-4af1-41e5-aa3a-bd361c90d4bb", "Only {0} protocol is supported.", protocol.ToUpper(CultureInfo.InvariantCulture)));
			}
			else if (shouldAllowAutoProtocolPrefixing)
			{
				return protocolWithDelimiter + serverName;
			}
			else
			{
				return serverName;
			}
		}
	}
}
