namespace TestRunner
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;
	using System.Threading;
	using CargoWise.Definitions;
	using CargoWise.Types;
	using Enterprise.Startup;
	using Enterprise.ZArchitecture.Core.Testing;
	using NUnit.Framework;

	class TestRunnerInAnotherProcess
	{
		[STAThread]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFrom")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This is essentially a top level exception handler")]
		static int Main(string[] args)
		{
			try
			{
				var argLength = args.Length;
				CrossPlatformTest.RunningOnOriginalPlatform = false;

				var localPath = Assembly.GetExecutingAssembly().Location;
				var localDirectory = Path.GetDirectoryName(localPath);
				var filename = args[argLength - 3];
				var assemblyFile = Path.Combine(localDirectory, filename);

				var typeName = args[argLength - 2];
				var methodName = args[argLength - 1];

				var list = new List<string>(args);
				list.RemoveRange(argLength - 3, 3);
				var mainArgs = list.ToArray();

				//System.Diagnostics.Debugger.Launch(); // leave it here for debugging when needed

				Exception applicationStartupDirectorException = null;
				var thread = new Thread(() =>
				{
					try
					{
						ApplicationStartupDirector.Main(mainArgs);
					}
					catch (Exception ex)
					{
						applicationStartupDirectorException = ex;
					}
				});

				thread.SetApartmentState(ApartmentState.STA);
				thread.Start();

				DatForm.InitializedEvent.WaitOne();

				if (applicationStartupDirectorException != null)
				{
					ConsoleWriteLines(new[]
					{
						$"ApplicationStartupDirector.Main failed under {(Environment.Is64BitProcess ? "64" : "32")} bits environment. Main arguments are: '{string.Join(" ", args)}'. The exception is: ",
						applicationStartupDirectorException.ToString(),
						ExceptionSerializer.SerializeToExceptionLine(applicationStartupDirectorException),
					});
					return ExitCodes.ApplicationStartupDirectorFailedExitCode;
				}

				Exception datFormInstanceCloseException = null;
				try
				{
					DatForm.Instance.InvokeEx(() =>
					{
						var testListerners = new List<ITestListener>();

						foreach (var listener in UnitTestListenersFactory.GetTestListeners())
						{
							testListerners.Add(listener);
						}

						using (var results = new TestResult(testListerners.ToArray()))
						{
							try
							{
								results.AddTest();
								results.BeforeTestInstantiated();

								RunTest(assemblyFile, typeName, methodName, results);

								foreach (var listener in testListerners)
								{
									listener.AfterEachTest(ZDateTime.Now.ToDateTime());
								}
							}
							finally
							{
								TestCase.RunFinalTearDown();
							}
						}
					});
				}
				catch (AggregateException aggregateException)
				{
					var unitTestException = aggregateException.InnerException;
					unitTestException = unitTestException is TargetInvocationException && unitTestException.InnerException != null
						? unitTestException.InnerException
						: unitTestException;

					ConsoleWriteLines(new[]
					{
						$"The unit test fails under {(Environment.Is64BitProcess ? "64" : "32")} bits environment. Main arguments are: '{string.Join(" ", args)}'. The exception is: ",
						unitTestException.ToString(),
						ExceptionSerializer.SerializeToExceptionLine(unitTestException),
					});

					return ExitCodes.UnitTestFailedExitCode;
				}
				finally
				{
					try
					{
						DatForm.Instance.InvokeEx(DatForm.Instance.Close);
					}
					catch (AggregateException ex)
					{
						datFormInstanceCloseException = ex.InnerException;
					}
				}

				if (datFormInstanceCloseException != null)
				{
					ConsoleWriteLines(new[]
					{
						$"DatForm.Instance.Close failed under {(Environment.Is64BitProcess ? "64" : "32")} bits environment. Main arguments are: '{string.Join(" ", args)}'. The exception is: ",
						datFormInstanceCloseException.ToString(),
						ExceptionSerializer.SerializeToExceptionLine(datFormInstanceCloseException),
					});

					return ExitCodes.DatFormInstanceCloseFailedExitCode;
				}
			}
			catch (Exception ex)
			{
				ConsoleWriteLines(new[]
				{
					$"TestRunnerInAnotherProcess.Main fails under {(Environment.Is64BitProcess ? "64" : "32")} bits environment. Main arguments are: '{string.Join(" ", args)}'. The exception is: ",
					ex.ToString(),
					ExceptionSerializer.SerializeToExceptionLine(ex),
				});

				return ExitCodes.TestRunnerInAnotherProcessFailedExitCode;
			}

			return ExitCodes.Success;
		}

		static void ConsoleWriteLines(IEnumerable<string> lines)
		{
			foreach (var line in lines)
			{
				Console.WriteLine(line); // these Console output is used for testing on 32/b4 bit platforms
			}
		}

		static void RunTest(string assemblyFile, string typeName, string methodName, TestResult results)
		{
			var assembly = Assembly.Load(AssemblyName.GetAssemblyName(assemblyFile));
			var classType = assembly.GetType(typeName);
			var constructor = classType.GetConstructor(Type.EmptyTypes);
			var testCase = (TestCase)((ITest)constructor.Invoke(null));
			testCase.Name = methodName;
			testCase.Run(results);
		}
	}
}
