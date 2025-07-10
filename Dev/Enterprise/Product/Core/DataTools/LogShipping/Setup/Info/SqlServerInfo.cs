using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Common;

namespace Enterprise.LogShipping.Setup
{
	public class SqlServerInfo
	{
		public SqlServerInfo(string serverName, string instance, string version)
		{
			Argument.NotNullOrEmpty(serverName, nameof(serverName));
			Argument.NotNullOrEmpty(version, nameof(version));

			this.ServerName = GetLocalMachineNameForLocalhost(serverName).ToUpper();
			this.InstanceName = !string.IsNullOrEmpty(instance) ? instance.ToUpper() : DefaultSqlServerInstanceName;
			this.Version = new Version(version);
		}

		public static SqlServerInfo[] GetLocalSqlServerInstances()
		{
			return GetSqlServersInstances(Environment.MachineName);
		}

		public static SqlServerInfo[] GetSqlServersInstances(string serverName)
		{
			Argument.NotNull(serverName, nameof(serverName));

			var servers = new List<SqlServerInfo>();

			try
			{
				serverName = GetLocalMachineNameForLocalhost(serverName);

				string[] instanceAttributesDataArray = GetSqlServersInstancesData(serverName).Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries);

				foreach (string data in instanceAttributesDataArray)
				{
					Match match = Regex.Match(data, @"^ServerName;([-\w]+);.*InstanceName;([-\w]+);.*Version;(\d+.\d+.\d+.\d+).*$");
					if (match.Success)
					{
						servers.Add(new SqlServerInfo(match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value));
					}
				}
				if (servers.Count == 0)
				{
					throw new SqlServerInfoException("The server is unavailable or it does not have available instances of SQL Server.");
				}
			}
			catch (SocketException ex)
			{
				throw new SqlServerInfoException(ex);
			}

			return servers.ToArray();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public override string ToString()
		{
			string instance = (IsDefaultInstance) ? InstanceName + " (default instance)" : InstanceName;
			string releaseName;

			switch (Version.Major)
			{
				case 16:
					releaseName = " - SQL 2022";
					break;
				case 15:
					releaseName = " - SQL 2019";
					break;
				case 14:
					releaseName = " - SQL 2017";
					break;
				case 13:
					releaseName = " - SQL 2016";
					break;
				case 12:
					releaseName = " - SQL 2014";
					break;
				case 11:
					releaseName = " - SQL 2012";
					break;
				case 10:
					releaseName = " - SQL 2008" + ((Version.Minor == 50) ? " R2" : "");
					break;
				case 9:
					releaseName = " - SQL 2005";
					break;
				default:
					releaseName = "";
					break;
			}

			return instance + releaseName;
		}

		public string FullInstanceName
		{
			get
			{
				string result = ServerName;
				if (!IsDefaultInstance)
				{
					result = string.Format(@"{0}\{1}", result, InstanceName);
				}
				return result;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public string SQLServerAgentName
		{
			get
			{
				string result = "SQLSERVERAGENT";
				if (!IsDefaultInstance)
				{
					result = "SQLAgent$" + InstanceName;
				}

				return result;
			}
		}

		public string ServerName { get; private set; }
		public string InstanceName { get; private set; }
		public Version Version { get; private set; }

		#region Implementation

		static string GetSqlServersInstancesData(string serverName)
		{
			Argument.NotNull(serverName, nameof(serverName));

			StringBuilder response = new StringBuilder();
			Socket socket = NewConnectedSocket(serverName, 1434);

			if (socket != null)
			{
				try
				{
					byte[] message = new byte[] { 0x02 };
					socket.Send(message);

					int bytesCount = 0;
					byte[] buffer = new byte[1024];
					do
					{
						bytesCount = socket.Receive(buffer, SocketFlags.Partial);
						var count = BitConverter.ToInt16(buffer, 1);
						response.Append(Encoding.ASCII.GetString(buffer, 3, count));
						socket.ReceiveTimeout = 300;
					}
					while (bytesCount > 0);
				}
				catch (SocketException ex)
				{
					if (ex.SocketErrorCode != SocketError.TimedOut)
					{
						throw;
					}
				}
				finally
				{
					socket.Close();
				}
			}

			return response.ToString();
		}

		static Socket NewConnectedSocket(string server, int port)
		{
			Argument.NotNull(server, nameof(server));

			Socket socket = null;
			IPHostEntry hostEntry = Dns.GetHostEntry(server);

			if (hostEntry != null && hostEntry.AddressList != null)
			{
				foreach (IPAddress address in hostEntry.AddressList)
				{
					IPEndPoint ipe = new IPEndPoint(address, port);
					Socket tempSocket = new Socket(ipe.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

					tempSocket.Connect(ipe);
					if (tempSocket.Connected)
					{
						socket = tempSocket;
						break;
					}

					tempSocket.Close();
				}
			}

			if (socket != null)
			{
				socket.ReceiveTimeout = ReceiveTimeout;
			}

			return socket;
		}

		bool IsDefaultInstance
		{
			get
			{
				return InstanceName == DefaultSqlServerInstanceName;
			}
		}

		static string GetLocalMachineNameForLocalhost(string serverName)
		{
			Argument.NotNull(serverName, nameof(serverName));

			return Regex.Replace(serverName, @"^(localhost|\(local\)|\.)", Environment.MachineName, RegexOptions.IgnoreCase);
		}

		const int ReceiveTimeout = 3000;
		const string DefaultSqlServerInstanceName = "MSSQLSERVER";

		#endregion
	}

	[Serializable]
	public class SqlServerInfoException : Exception
	{
		public SqlServerInfoException(SocketException exception)
			: base(GetExceptionMessage(exception), exception)
		{
			Argument.NotNull(exception, nameof(exception));
		}

		public SqlServerInfoException(string message)
			: base(message)
		{ }

#if NETFRAMEWORK
		protected SqlServerInfoException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		static string GetExceptionMessage(SocketException exception)
		{
			Argument.NotNull(exception, nameof(exception));

			string result = null;
			if (exception.SocketErrorCode == SocketError.AccessDenied)
			{
				result = "Access to the server is denied.";
			}
			else if (exception.SocketErrorCode == SocketError.ConnectionReset)
			{
				result = "SQL Server Browser service does not respond, probably it's disabled on target server.";
			}
			else if (exception.SocketErrorCode == SocketError.HostNotFound
			|| exception.SocketErrorCode == SocketError.HostUnreachable
			|| exception.SocketErrorCode == SocketError.NetworkUnreachable)
			{
				result = "The server is unavailable.";
			}
			else
			{
				result = exception.Message;
			}
			return result;
		}
	}
}
