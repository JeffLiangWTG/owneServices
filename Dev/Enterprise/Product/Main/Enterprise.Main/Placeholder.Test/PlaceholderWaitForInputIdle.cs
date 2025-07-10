#pragma warning disable IDE0005
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Reflection;
using NUnit.Framework;

namespace Placeholder.Test
{
	public class PlaceholderWaitForInputIdle
	{
		[CancelAfter(20000)]
		[Property("DAT:CapabilityRequirements", "GUI")]
		[TestCase("CargoWise.exe")]
		[TestCase("CargoWiseOne.exe")]
		[TestCase("CargoWiseOneAnyCpu.exe")]
		public void Test_Placeholders_WaitForInputIdle(string testExecutable)
		{
			var installationPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var fullPathOfProgramToRun = Path.Combine(installationPath, testExecutable);
			var arguments = "";

			var startInfo = new ProcessStartInfo(fullPathOfProgramToRun, arguments)
			{
				WorkingDirectory = installationPath
			};

			Process startedProcess;
			try
			{
				startedProcess = Process.Start(startInfo);
				if (startedProcess == null)
				{
					Assert.Fail("Unable to start " + fullPathOfProgramToRun + ".");
					return;
				}
			}
			catch (Win32Exception ex)
			{
				Assert.Fail("Win32Exception when starting " + fullPathOfProgramToRun + ". " + ex.Message);
				return;
			}

			// Wait for the process form to be created and go idle. This tests that the placeholder emulates the behavior of the CargoWise Main exec correctly.
			var waitedForInputIdle = startedProcess.WaitForInputIdle(15000);

			// Kill all processes... we're going for max speed here to keep the unit test running fast.
			CloseMainWindowOnProcessAndChildren(startedProcess.Id, true);

			Assert.Multiple(() =>
			{
				Assert.That(waitedForInputIdle, Is.True);   // We got the idle event.
			});
		}

		static int CloseMainWindowOnProcessAndChildren(int pid, bool kill)
		{
			// Cannot close 'system idle process'.
			if (pid == 0)
			{
				return 0;
			}
			int processCount = 0;
			ManagementObjectSearcher searcher = new ManagementObjectSearcher($"Select * From Win32_Process Where ParentProcessID={pid}");
			ManagementObjectCollection moc = searcher.Get();
			foreach (var mo in moc)
			{
				CloseMainWindowOnProcessAndChildren(Convert.ToInt32(mo["ProcessID"]), kill);
			}
			try
			{
				Process proc = Process.GetProcessById(pid);
				if (!proc.HasExited)
				{
					if (kill)
					{
						proc.Kill();
					}
					else
					{
						proc.CloseMainWindow();
					}

					processCount++;
				}
			}
			catch (ArgumentException)
			{
				// Process already exited.
			}
			catch (InvalidOperationException)
			{
				// Process already exited.
			}
			catch (Win32Exception)
			{
				// Process already exited.
			}

			return processCount;
		}
	}
}
