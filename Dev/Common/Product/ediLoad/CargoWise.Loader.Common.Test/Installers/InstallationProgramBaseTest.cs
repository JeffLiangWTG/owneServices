using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using NUnit.Framework;

namespace CargoWise.Loader.Common.Testing.Installers
{
	class InstallationProgramBaseTest : TestCase
	{
		class InstallationProgramForTest : InstallationProgramBase
		{
			public InstallationProgramForTest(Installation installation) : base(installation)
			{
			}

			public new InstallationResult CheckAvailabilityExcludingDependencies()
			{
				return base.CheckAvailabilityExcludingDependencies();
			}

			public new InstallationResult InstallExcludingDependencies()
			{
				return base.InstallExcludingDependencies();
			}

			public override string FullPathOfProgramToRun
			{
				get
				{
					return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Filename);
				}
			}

			public new Process Process => base.Process;

			public new Thread WaitThread => base.WaitThread;

			public string Filename = "MockProgram.exe";
		}

		Installation Installation;
		InstallationProgramForTest InstallationProgram;
		int currentTaskDescriptionChangedCount;
		string expectedTaskDescription;

		protected override void SetUp()
		{
			base.SetUp();
			SetUp(new Configuration());
		}

		void SetUp(Configuration configuration)
		{
			Installation = new Installation(configuration);
			InstallationProgram = new InstallationProgramForTest(Installation);
			Installation.CurrentTaskDescriptionChanged += new EventHandler<TaskDescriptionChangedEventArgs>(Installation_CurrentTaskDescriptionChanged);
		}

		public void TestCheckAvailabilityExcludingDependencies()
		{
			Assert(InstallationProgram.CheckAvailabilityExcludingDependencies().IsOK);
		}

		public void TestNeedsToInstall()
		{
			Assert("Must always install as wrapper really decides this", InstallationProgram.NeedsToInstall());
		}

		public void TestErrorMessageShowsErrorAndProgramName()
		{
			InstallationProgram.Filename = "foobar.invalid.exe";
			InstallationResult result = InstallationProgram.InstallExcludingDependencies();
			Assert(result.IsError);
			Assert("Message should contain error, but was: " + result.Message, result.Message.IndexOf("cannot find the file") >= 0);
			Assert("Message should contain filename, but was: " + result.Message, result.Message.IndexOf("foobar.invalid.exe") >= 0);
		}

		public void TestRunAndWaitForInputIdle()
		{
			InstallationProgram.Arguments = "-WaitTwoSecondsAndStartMessageLoop";
			int startTicks = Environment.TickCount;
			InstallationProgram.WaitForInputIdle = true;
			expectedTaskDescription = "Launching ...";
			try
			{
				Assert("Should run OK", InstallationProgram.InstallExcludingDependencies().IsOK);
				int endTicks = Environment.TickCount;
				Assert("Should have waited about 2 seconds for input idle", endTicks - startTicks > 1600 /* fudge factor */);
				AssertEquals("Should have changed task description", 1, currentTaskDescriptionChangedCount);
			}
			finally
			{
				InstallationProgram.Process.Kill();
			}
		}

		public void TestRunAndWaitForExit()
		{
			InstallationProgram.Arguments = "-WaitTwoSecondsAndExit";
			int startTicks = Environment.TickCount;
			InstallationProgram.WaitForExit = true;
			expectedTaskDescription = "Waiting for MockProgram";
			Assert("Should run OK", InstallationProgram.InstallExcludingDependencies().IsOK);
			int endTicks = Environment.TickCount;
			Assert("Should have waited about 2 seconds for exit", endTicks - startTicks > 1600 /* fudge factor */);
			Assert("Process should have exited", InstallationProgram.Process.HasExited);
			AssertEquals("Should have changed task description", 1, currentTaskDescriptionChangedCount);
		}

		public void TestRunAndReturnImmediately()
		{
			using (TempFile outputFile = TempFile.New())
			{
				InstallationProgram.Arguments = "\"" + outputFile + "\" arg2 arg3";
				File.Delete(outputFile);
				Assert(!File.Exists(outputFile));
				AssertEquals("Process should not be set until program is run", null, InstallationProgram.Process);
				InstallationProgram.Install(new InstallationResultCollection());
				using (Process startedProcess = InstallationProgram.Process)
				{
					try
					{
						int startTickCount = Environment.TickCount;
						while (!File.Exists(outputFile))
						{
							Thread.Sleep(100);
							if (Environment.TickCount - startTickCount > 5000)
							{
								Fail("Timed out waiting for program output");
							}
						}

						bool fileOpened = false;
						while (!fileOpened)
						{
							try
							{
								using (StreamReader reader = File.OpenText(outputFile))
								{
									fileOpened = true;
									string line;
									int lineNumber = 0;
									while ((line = reader.ReadLine()) != null)
									{
										switch (lineNumber)
										{
											case 0:
												AssertEquals("Working directory", AppDomain.CurrentDomain.BaseDirectory, line);
												break;
											case 1:
												Assert(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MockProgram.exe").Equals(line, StringComparison.OrdinalIgnoreCase));
												break;
											case 2:
												AssertEquals(outputFile.FileName, line);
												break;
											case 3:
												AssertEquals("arg2", line);
												break;
											case 4:
												AssertEquals("arg3", line);
												break;
											default:
												Fail("Too many lines in file");
												break;
										}
										lineNumber++;
									}
								}
							}
							catch (IOException ex)
							{
								var errorCode = Marshal.GetHRForException(ex) & ((1 << 16) - 1);
								if (errorCode == 32 || errorCode == 33) // File is locked
								{
									Thread.Sleep(100);
								}
								else
								{
									throw;
								}
							}
						}
					}
					finally
					{
						startedProcess.Kill();
						startedProcess.WaitForExit();
					}
				}
			}
		}

		public void TestWaitForExitInBackground()
		{
			InstallationProgram.Arguments = "-WaitTwoSecondsAndExit";
			var stopWatch = new Stopwatch();
			InstallationProgram.WaitForExitInBackground = true;
			expectedTaskDescription = "Waiting for MockProgram";
			stopWatch.Start();
			Assert("Should run OK", InstallationProgram.InstallExcludingDependencies().IsOK);
			stopWatch.Stop();
			CombineAssertions(() =>
			{
				Assert($"Should have exited immediatly. Instead took {stopWatch.Elapsed}ms", stopWatch.Elapsed < TimeSpan.FromMilliseconds(1000));
				Assert("Process should not have exited", !InstallationProgram.Process.HasExited);
				AssertNotNull("Background thread should have been created", InstallationProgram.WaitThread);
				Assert("Background thread should be running", InstallationProgram.WaitThread.IsAlive);
				Assert("Background thread should finish", InstallationProgram.WaitThread.Join(5000));
				Assert("Process should have exited", InstallationProgram.Process.HasExited);
			});
		}

		void Installation_CurrentTaskDescriptionChanged(object sender, TaskDescriptionChangedEventArgs e)
		{
			currentTaskDescriptionChangedCount++;
			AssertEquals(expectedTaskDescription, e.TaskDescription);
		}
	}
}
