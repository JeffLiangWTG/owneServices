using System.Net;
using System.Net.Sockets;
using System.Text;
using static System.FormattableString;

namespace CargoWise.Data.SqlProxy.Interface;

public static class SqlProxyNamingConvention
{
	public static string SqlProxyServiceNamedPipeName(string serverName, string databaseName)
	{
		var base64String = GetBase64String(Invariant($"{NormalizedInstanceName(serverName)}_{databaseName}"));
		var namedPipeName = Invariant($"SqlProxyServiceNamedPipe_{base64String}");
		return namedPipeName;
	}

	public static string GlobalSqlProxyServiceMutexName(string serverName, string databaseName)
	{
		var base64String = GetBase64String(Invariant($"{NormalizedInstanceName(serverName)}_{databaseName}"));
		var mutexName = Invariant($"Global\\SqlProxyServiceMutex_{base64String}");
		return mutexName;
	}

	public static string GetBase64String(string name) =>
		Convert.ToBase64String(Encoding.UTF8.GetBytes(name)).Replace("=", "").Replace("/", "_");

	public static string NormalizedInstanceName(string serverName)
	{
		if (string.IsNullOrWhiteSpace(serverName))
		{
			return serverName;
		}

		var normalized = serverName.Trim().Replace('/', '\\');
		if (IsLocalMachineReference(normalized))
		{
			return GetFullQualifiedLocalName(normalized);
		}

		if (TryResolveToFqdn(normalized, out var fqdn))
		{
			return fqdn;
		}

		return GetFullQualifiedLocalName(normalized);
	}

	public const string SqlProxyServerExeName = "CargoWise.Data.SqlProxy.Server.exe";

	static string GetFullQualifiedLocalName(string input)
	{
		var machineName = GetFullQualifiedDomainName();
		var instanceName = input.Contains("\\") ? input.Substring(input.IndexOf('\\')) : "";
		return $"{machineName}{instanceName}";
	}

	static bool TryResolveToFqdn(string name, out string result)
	{
		result = name;
		var serverPart = name.Split('\\').First();
		if (!IPAddress.TryParse(serverPart, out var address))
		{
			return false;
		}

		try
		{
			var localIPs = Dns.GetHostEntry(Dns.GetHostName())
				.AddressList
				.Where(a => a.AddressFamily == AddressFamily.InterNetwork)
				.ToList();

			if (localIPs.Contains(address))
			{
				var instancePart = name.Contains("\\") ? name.Substring(name.IndexOf('\\')) : "";
				result = $"{GetFullQualifiedDomainName()}{instancePart}";
				return true;
			}
		}
		catch
		{
			// Fallback to original IP if resolution fails
		}

		return false;
	}

	static string GetFullQualifiedDomainName()
	{
		try
		{
			return Dns.GetHostEntry("").HostName;
		}
		catch
		{
			return Environment.MachineName;
		}
	}

	static bool IsLocalMachineReference(string name)
	{
		return
			name.StartsWith(".\\", StringComparison.Ordinal)
			|| name.StartsWith("localhost\\", StringComparison.OrdinalIgnoreCase)
			|| name.Equals("(local)", StringComparison.OrdinalIgnoreCase)
			|| name.Equals(".", StringComparison.Ordinal)
			|| name.Equals("localhost", StringComparison.OrdinalIgnoreCase)
			|| name.StartsWith("@", StringComparison.Ordinal);
	}
}
