using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Data.HttpClient;
using CargoWise.Data.SqlProxy.Interface;

namespace CargoWise.Data;

public static class SqlProxyClientProvider
{
	[SuppressMessage("CargoWiseOne", "CW1055:DoNotUseProcessGetProcesses", Justification = "Debug code")]
	public static string MiddleTierAddress
	{
		set
		{
			try
			{
				HttpLoaderFactory.SetGlowLoaderServerListeningPort(value);
			}
			catch (Exception ex)
			{
				throw new CommunicationException($"Set {nameof(MiddleTierAddress)} error", ex);
			}
		}
	}

	public static bool IsHttpEnabled => HttpLoaderFactory.IsHttpServerConfigured;

	[SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.",
		Justification = "Startup GlowServer process")]
	public static Process StartSqlProxyServerProcess(string serverName, string databaseName, CancellationToken cancellationToken)
	{
		if (TryConnectToExistRunningSqlProxyServerProcess(serverName, databaseName, cancellationToken))
		{
			return null;
		}

		var binDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		var exeFilePath = Path.Combine(binDir!, "net8.0", SqlProxyNamingConvention.SqlProxyServerExeName);
		if (!File.Exists(exeFilePath))
		{
			throw new FileNotFoundException($"Glow server executable not found at {exeFilePath}");
		}

		var port = FindNextAvailablePort(out var listener);
		var args = FormattableString.Invariant($"{port} {serverName} {databaseName}");
		var processStartInfo = new ProcessStartInfo(exeFilePath)
		{
			WindowStyle = ProcessWindowStyle.Hidden,
			CreateNoWindow = true,
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			RedirectStandardInput = true,
			Arguments = args,
		};

		listener.Stop();
		var process = Process.Start(processStartInfo);
		if (process == null)
		{
			return null;
		}

		var serviceStarted = false;

		process.OutputDataReceived += OnProcessOnOutputDataReceived;
		process.ErrorDataReceived += OnProcessOnErrorDataReceived;
		process.BeginOutputReadLine();
		process.BeginErrorReadLine();

		try
		{
			while (!cancellationToken.IsCancellationRequested && !serviceStarted)
			{
				Thread.Sleep(100);
			}

			if (cancellationToken.IsCancellationRequested)
			{
				KillGlowServerProcess(null, EventArgs.Empty);
				return null;
			}
		}
		finally
		{
			if (process != null)
			{
				process.OutputDataReceived -= OnProcessOnOutputDataReceived;
				process.ErrorDataReceived -= OnProcessOnErrorDataReceived;
			}
		}

		return process;

		void KillGlowServerProcess(object sender, EventArgs e)
		{
			HttpLoaderFactory.ResetGlowLoaderService();

			if (process != null)
			{
				process.StandardInput.Write("exit");
				if (!process.HasExited)
				{
					process.Kill();
				}

				process = null;
			}
		}

		void OnProcessOnErrorDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (e.Data == null)
			{
				return;
			}

			KillGlowServerProcess(null, EventArgs.Empty);
			throw new Exception($"GlowServer exception: {e.Data}");
		}

		void OnProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (e.Data == null)
			{
				return;
			}

			if (string.Equals(e.Data.Trim(), args) && !serviceStarted)
			{
				MiddleTierAddress = FormattableString.Invariant($"{port}");
				serviceStarted = true;
			}
		}

		static int FindNextAvailablePort(out TcpListener listener, int startingPort = 7070)
		{
			var port = startingPort;

			while (true)
			{
				try
				{
					listener = new TcpListener(IPAddress.IPv6Any, port);
					listener.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
					listener.Start();
					return port;
				}
				catch (SocketException)
				{
					port++;
				}
			}
		}
	}

	static bool TryConnectToExistRunningSqlProxyServerProcess(string serverName, string databaseName, CancellationToken cancellationToken)
	{
		var pipeName = SqlProxyNamingConvention.SqlProxyServiceNamedPipeName(serverName, databaseName);
		var pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);

		try
		{
			var task = ConnectAndGetResponseAsync(pipeClient, cancellationToken);
			task.Wait(cancellationToken);
			var result = task.Result;

			if (!string.IsNullOrWhiteSpace(result) && int.TryParse(result, out var port))
			{
				MiddleTierAddress = FormattableString.Invariant($"{port}");
				return true;
			}
		}
		finally
		{
			pipeClient.Close();
			pipeClient.Dispose();
		}

		return false;

		static async Task<string> ConnectAndGetResponseAsync(NamedPipeClientStream clientStream, CancellationToken cancellationToken)
		{
			const int maxRetries = 3;
			const int retryDelayMs = 1000;

			for (var i = 0; i < maxRetries; i++)
			{
				try
				{
					await clientStream.ConnectAsync(2000, cancellationToken);

					var buffer = new byte[1024];
#pragma warning disable CA1835
					var bytesRead = await clientStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
#pragma warning restore CA1835
					return Encoding.UTF8.GetString(buffer, 0, bytesRead);
				}
				catch (IOException)
				{
					// Ignore the exception and retry
				}
				catch (TimeoutException)
				{
					await Task.Delay(retryDelayMs, cancellationToken);
				}
			}

			return string.Empty;
		}
	}
}
