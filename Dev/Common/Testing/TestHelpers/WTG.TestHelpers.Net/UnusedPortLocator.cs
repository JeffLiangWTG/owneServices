using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace WTG.TestHelpers.Net
{
	public static class UnusedPortLocator
	{
		public const int DefaultPortScanStart = 32000;
		public const int PortScanRange = 100;
		public const int MaxValidPort = 0xFFFF;

		public static Port Find() => Find(DefaultPortScanStart);

		public static Port Find(int startScanAt)
		{
			var port = FindCore(startScanAt);
			if (port < 0)
			{
				throw new InvalidOperationException("Cannot acquire a port.");
			}
			else
			{
				return new Port((ushort)port);
			}
		}

		static int FindCore(int startScanAt)
		{
			if (startScanAt > MaxValidPort - PortScanRange || startScanAt <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(startScanAt));
			}

			var reservedPorts = GetReservedPorts().ToList();

			for (var i = startScanAt; i < startScanAt + PortScanRange; i++)
			{
				if (reservedPorts.Contains(i))
				{
					continue;
				}

				if (portsSharedFile.AcquirePort((ushort)i))
				{
					var release = true;
					try
					{
						var endPoint = new IPEndPoint(IPAddress.Loopback, i);

						using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
						socket.Connect(endPoint);
					}
					catch (SocketException ex)
					{
						if (ex.SocketErrorCode == SocketError.ConnectionRefused)
						{
							release = false;
							return i;
						}
					}
					finally
					{
						if (release)
						{
							portsSharedFile.ReleasePort((ushort)i);
						}
					}
				}
			}

			return -1;
		}

		static IEnumerable<int> GetReservedPorts()
		{
			using var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\HTTP\Parameters\UrlAclInfo", writable: false);
			foreach (var values in key?.GetValueNames() ?? Array.Empty<string>())
			{
				var match = new Regex(@"http(s)?:\/\/[^:]+:(?<port>\d+)\/.*").Match(values);
				var reservedPort = match.Groups["port"].Value;
				if (int.TryParse(reservedPort, out var port))
				{
					yield return port;
				}
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
		public sealed class Port : IDisposable
		{
			public Port(ushort port)
			{
				this.port = port;
			}

			public int Num => port;
			public void Dispose() => portsSharedFile.ReleasePort(port);

			readonly ushort port;
		}

		static readonly TestServicePortsMemoryMappedFile portsSharedFile = new TestServicePortsMemoryMappedFile();
	}
}
