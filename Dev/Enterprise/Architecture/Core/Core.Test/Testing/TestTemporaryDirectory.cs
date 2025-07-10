using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using CargoWise.Common.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public class TestTemporaryDirectory : IDisposable
	{
		public TestTemporaryDirectory() : this(false)
		{ }

		/// <summary>
		/// Creates a TestTemporaryDirectory and optionally runs procmon to log file system access.
		/// </summary>
		/// <param name="runProcmon">Requires local admin permissions. To help debug errors deleting temporary directories, run Sysinternals Process Monitor and upload logs to DAT in the event of failure.</param>
		public TestTemporaryDirectory(bool runProcmon)
		{
			DisposableLeakListener.Instance.RegisterDisposable(this);
			this.runProcmon = runProcmon;
		}

		public DirectoryInfo Directory
		{
			get
			{
				if (fDisposed)
				{
					throw new ObjectDisposedException(GetType().FullName);
				}

				if (fDirectory == null)
				{
					if (runProcmon)
					{ StartProcmon(); }
					string tempDirectoryName = EnvProxy.Instance.GetTempFileName();
					File.Delete(tempDirectoryName);
					fDirectory = new DirectoryInfo(tempDirectoryName);
					fDirectory.Create();
				}
				return fDirectory;
			}
		}

		void StartProcmon()
		{
			// Procmon has command line flags to drive it programatically.
			// The main procmon process needs time to start up and runs in the background.
			// Separate processes are launched to wait for the main process to start (/waitforidle) and tell it to exit (/terminate)
			// Example from the docs:
			// set PM=C:\sysint\procmon.exe
			// start %PM% /quiet /minimized /backingfile C:\temp\notepad.pml
			// %PM% /waitforidle
			// start /wait notepad.exe
			// %PM% /terminate

			if (!IsAdministrator())
			{ throw new Exception("Procmon tracking requires admin rights. For DAT, add DAT capabilities Admin and VM to your test."); }

			Process.Start(ProcmonPath, "/terminate /accepteula").WaitForExit(120_000); // Any existing instances need to be stopped
			procmonLogFilename = EnvProxy.Instance.GetTempFileName(EnvProxy.Instance.TempPath, "pml"); // Must end in PML else procmon will add it
			mainProcmonProcess = Process.Start(ProcmonPath, "/quiet /minimized /accepteula /backingfile " + procmonLogFilename);
			procmonIdleSuccessful = Process.Start(ProcmonPath, "/waitforidle /accepteula").WaitForExit(120_000);
		}

		static bool IsAdministrator()
		{
			var identity = WindowsIdentity.GetCurrent();
			WindowsPrincipal principal = new WindowsPrincipal(identity);
			return principal.IsInRole(WindowsBuiltInRole.Administrator);
		}

		void StopProcmon()
		{
			Process.Start(ProcmonPath, "/terminate /accepteula").WaitForExit(120_000);
			mainProcmonProcess.WaitForExit(120_000);
		}

		string ProcmonPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Procmon.exe");

		DirectoryInfo fDirectory;
		string procmonLogFilename;
		string ProcmonZipFilename => Path.ChangeExtension(procmonLogFilename, "zip");
		readonly bool runProcmon;
		bool fDisposed;
		bool procmonIdleSuccessful;
		Process mainProcmonProcess;

		public void Dispose()
		{
			try
			{
				if (fDirectory != null)
				{
					Exception exception = null;
					try
					{
						fDirectory.Delete(true);
					}
					catch (Exception ex)
					{
						exception = ex;
					}
					finally
					{
						if (runProcmon)
						{ StopProcmon(); }
					}

					if (exception != null)
					{
						var message = "Error deleting temporary test directory.";
						if (runProcmon)
						{
							if (!procmonIdleSuccessful)
							{
								message += " Procmon launch waitforidle did not succeed, log file may be missing or incomplete.";
							}

							try
							{
								// This Procmon logging was added to diagnose errors installing an MSI file.
								// The resulting Procmon log file was ~600-1000 MB, which timed out uploading to DAT.
								// Zipping the log gets it down to ~70-100 MB which works better.
								var zipCreator = new ZipCreator();
								zipCreator.CreateZipFile(procmonLogFilename, ProcmonZipFilename);
								var uri = new WTG.DevTools.Common.TestFailureDataClient().Upload(ProcmonZipFilename, "application/zip", ".zip").Result;
								message += " Procmon log zip: " + uri;
							}
							catch (Exception ex)
							{
								message += " Error uploading procmon log: " + ex.Message;
							}
						}
						else
						{
							message += " Procmon logging is disabled.";
						}

						throw new Exception(message, exception);
					}
				}
				fDisposed = true;
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
			}
			finally
			{
				if (procmonLogFilename != null)
				{
					File.Delete(procmonLogFilename);
					File.Delete(ProcmonZipFilename);
				}
			}
		}
	}
}
