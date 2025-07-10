using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microsoft.Win32;

namespace WTG.TestHelpers.IISExpress
{
	public abstract class IISExpressHostedBase : IDisposable
	{
		protected IISExpressHostedBase(int port)
		{
			Port = port;
			jobObject = new JobObject();
		}

		public int Port { get; }
		public virtual Uri ApplicationUri => new Uri("http://localhost:" + Port + "/", UriKind.Absolute);

		public void Start()
		{
			if (process != null)
			{
				throw new InvalidOperationException("IIS Express has already been started.");
			}

			var processStartInfo = GetProcessStartInfo(LocateIISExpressInstallPath());
			process = new Process { StartInfo = processStartInfo };
			var started = process.Start();
			if (!started)
			{
				throw new CouldNotStartProcessException();
			}

			// Technically, there is a race here. The child process could spawn a child of it's own before it's added to the JobObject.
			// The only reliable solution to this is to start the process in the suspended state and then resume it only after adding it
			// to the job object. That has it's own issues, so for now we will just have to assume that IIS is too slow to start up for this
			// to be a real issue.
			jobObject.AddProcess(process);

			// Give IIS Express a moment to start up.
			// It can be slow to start, but if we hit the site before IIS starts then
			// we will probably hit the dreaded "connection refused" error.
			WaitUntilServiceIsReady(ApplicationUri, TimeSpan.FromSeconds(20));
		}

		public void Stop()
		{
			if (process != null)
			{
				OnStop();

				IISExpressStopper.SendStopMessage(process);
				var exited = process.WaitForExit((int)TimeSpan.FromSeconds(5).TotalMilliseconds);
				if (!exited)
				{
					try
					{
						process.Kill();
					}
					catch (InvalidOperationException)
					{
						// The process has already exited. (Race condition.)
					}
					catch (Win32Exception)
					{
						// The associated process could not be terminated. (Doesn't say _why_, though)
						// -or-
						// The process is terminating. (Most likely scenario here)
						// -or-
						// The associated process is a Win16 executable. (Highly unlikely)
					}
				}

				process.WaitForExit(); // Waits for I/O to flush.

				process.Dispose();
				process = null;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected abstract ProcessStartInfo GetProcessStartInfo(string iisProcessPath);

		protected static ProcessStartInfo GetProcessStartInfo(string iisProcessPath, params string[] arguments)
		{
			var builder = new StringBuilder("/systray:false /trace:info");

			if (arguments != null)
			{
				foreach (var argument in arguments)
				{
					builder.Append(" \"").Append(argument).Append('"');
				}
			}

			return new ProcessStartInfo()
			{
				FileName = iisProcessPath,
				Arguments = builder.ToString(),
				UseShellExecute = false,
				WindowStyle = ProcessWindowStyle.Hidden,
				CreateNoWindow = true,
			};
		}

		protected virtual void OnStop()
		{
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				try
				{
					Stop();
				}
				finally
				{
					jobObject.Dispose();
				}
			}
		}

		static void WaitUntilServiceIsReady(Uri uri, TimeSpan timeout)
		{
			var waited = TimeSpan.Zero;
			var sleepMilliseconds = 50;

			using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			while (waited < timeout && !TryConnectSocket(socket, uri))
			{
				Thread.Sleep(sleepMilliseconds);
				sleepMilliseconds = sleepMilliseconds * 2 <= 1000 ? sleepMilliseconds * 2 : 1000;
				waited.Add(TimeSpan.FromMilliseconds(sleepMilliseconds));
			}
		}

		static bool TryConnectSocket(Socket socket, Uri uri)
		{
			try
			{
				socket.Connect(uri.Host, uri.Port);
				return socket.Connected;
			}
			catch (SocketException)
			{
				return false;
			}
		}

		readonly JobObject jobObject;
		Process process;

		public static string LocateIISExpressInstallPath()
		{
			using var view64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
			using var registryKey = view64.OpenSubKey(@"SOFTWARE\Microsoft\IISExpress\10.0");
			var installPath = (string)registryKey?.GetValue("InstallPath")
				?? throw new CouldNotLocateIISExpressException();

			return Path.Combine(installPath, "iisexpress.exe");
		}
	}
}
