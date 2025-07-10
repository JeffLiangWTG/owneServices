using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace AppDomainWrappers.Net.Test
{
	public class AppDomainWrapperTests : TestCase
	{
		#region RunActionInAppDomain Tests
		public void TestRunActionInAppDomainWithSetDataWithExeption()
		{
			var domainData = new Dictionary<string, object>
			{
				{ "ExceptionMessage", "Something went wrong" }
			};

			using (var appDomainWrapper = new AppDomainWrapper("MyTestAppDomain"))
			{
				AnonymousMethod method = () =>
				{
					appDomainWrapper.RunActionInAppDomain(() =>
					{
						var errorMessage = (string)AppDomain.CurrentDomain.GetData("ExceptionMessage");
						throw new Exception(errorMessage);
					}, domainData);
				};
				AssertExceptionThrown(typeof(Exception), "Something went wrong", method);
			}
		}

		public void TestRunActionInTestAppDomainWithSetDataWithExeption()
		{
			var domainData = new Dictionary<string, object>
			{
				{ "ExceptionMessage", "Something went wrong" }
			};

			using (var appDomainWrapper = new AppDomainWrapper("MyTestAppDomain", true))
			{
				AnonymousMethod method = () =>
				{
					appDomainWrapper.RunActionInAppDomain(() =>
					{
						var errorMessage = (string)AppDomain.CurrentDomain.GetData("ExceptionMessage");
						throw new Exception(errorMessage);
					}, domainData);
				};
				AssertExceptionThrown(typeof(Exception), "Something went wrong", method);
			}
		}

		public void TestRunActionInAppDomainWithReturnData()
		{
			var returnData = new Dictionary<string, object>
			{
				{ "AppDomainName", null }
			};

			using (var appDomainWrapper = new AppDomainWrapper("MyTestAppDomain"))
			{
				var result = appDomainWrapper.RunActionInAppDomain(() =>
				{
					var ad = AppDomain.CurrentDomain;
					ad.SetData("AppDomainName", ad.FriendlyName);
				}, null, returnData);

				AssertEquals("MyTestAppDomain", result["AppDomainName"]);
			}
		}

		public void TestRunActionInTestAppDomainWithReturnData()
		{
			var returnData = new Dictionary<string, object>
			{
				{ "AppDomainName", null }
			};

			using (var appDomainWrapper = new AppDomainWrapper("MyTestAppDomain", true))
			{
				var result = appDomainWrapper.RunActionInAppDomain(() =>
				{
					var ad = AppDomain.CurrentDomain;
					ad.SetData("AppDomainName", ad.FriendlyName);
				}, null, returnData);

				AssertEquals("MyTestAppDomain", result["AppDomainName"]);
			}
		}

		public void TestRunActionInAppDomainWithSetDataAndReturnData()
		{
			var domainData = new Dictionary<string, object>
			{
				{ "DataOne", "First" },
				{ "DataTwo", "Second" }
			};

			var returnData = new Dictionary<string, object>
			{
				{ "Message", null }
			};

			using (var appDomainWrapper = new AppDomainWrapper("MyTestAppDomain"))
			{
				var result = appDomainWrapper.RunActionInAppDomain(() =>
				{
					var ad = AppDomain.CurrentDomain;
					ad.SetData("Message", $"{ad.GetData("DataOne")}-{ad.GetData("DataTwo")}");
				}, domainData, returnData);

				AssertEquals("First-Second", result["Message"]);
			}
		}

		public void TestRunActionInTestAppDomainWithSetDataAndReturnData()
		{
			var domainData = new Dictionary<string, object>
			{
				{ "DataOne", "First" },
				{ "DataTwo", "Second" }
			};

			var returnData = new Dictionary<string, object>
			{
				{ "Message", null }
			};

			using (var appDomainWrapper = new AppDomainWrapper("MyTestAppDomain", true))
			{
				var result = appDomainWrapper.RunActionInAppDomain(() =>
				{
					var ad = AppDomain.CurrentDomain;
					ad.SetData("Message", $"{ad.GetData("DataOne")}-{ad.GetData("DataTwo")}");
				}, domainData, returnData);

				AssertEquals("First-Second", result["Message"]);
			}
		}

		public void TestRunActionInAppDomain()
		{
			using (var appDomainWrapper = new AppDomainWrapper("MyTestAppDomain"))
			{
				AnonymousMethod method = () =>
				{
					appDomainWrapper.RunActionInAppDomain(() =>
					{
#pragma warning disable CW1106 // Do Not Leave In Debug Messages
						Console.WriteLine("Custom Code Line 1");
						Console.WriteLine("Custom Code Line 2");
						//throw new System.Exception(""Some error occurred"");

						Console.WriteLine("Custom Code Line 3");
						Console.WriteLine("Custom Code Line 4");
						throw new Exception("Some error occurred");
#pragma warning restore CW1106 // Do Not Leave In Debug Messages
					});
				};
				AssertExceptionThrown(typeof(Exception), "Some error occurred", method);
			}
		}

		public void TestRunActionInTestAppDomain()
		{
			using (var appDomainWrapper = new AppDomainWrapper("MyTestAppDomain", true))
			{
				AnonymousMethod method = () =>
				{
					appDomainWrapper.RunActionInAppDomain(() =>
					{
#pragma warning disable CW1106 // Do Not Leave In Debug Messages
						Console.WriteLine("Custom Code Line 1");
						Console.WriteLine("Custom Code Line 2");
						//throw new System.Exception(""Some error occurred"");

						Console.WriteLine("Custom Code Line 3");
						Console.WriteLine("Custom Code Line 4");
						throw new Exception("Some error occurred");
#pragma warning restore CW1106 // Do Not Leave In Debug Messages
					});
				};
				AssertExceptionThrown(typeof(Exception), "Some error occurred", method);
			}
		}
		#endregion

		#region RunActionInTask Tests
		public void TestRunActionInTask()
		{
			using (var appDomainWrapper = new AppDomainWrapper("MyTestAppDomain"))
			{
				AnonymousMethod method = () =>
				{
					var cts = new CancellationTokenSource();

					appDomainWrapper.RunActionInTask(() =>
					{
#pragma warning disable CW1106 // Do Not Leave In Debug Messages
						Console.WriteLine("Custom Code Line 1");
						Console.WriteLine("Custom Code Line 2");
						//throw new System.Exception(""Some error occurred"");

						Console.WriteLine("Custom Code Line 3");
						Console.WriteLine("Custom Code Line 4");
						throw new Exception("Some error occurred");
#pragma warning restore CW1106 // Do Not Leave In Debug Messages
					}, cts.Token);
				};
				AssertExceptionThrown(typeof(Exception), "Some error occurred", method);
			}
		}
		#endregion

		#region RunCodeInProcess Tests
		public void TestRunCodeInProcess()
		{
			var codeToRun = @"
				System.Console.WriteLine(""Custom Code Line 1"");
				System.Console.WriteLine(""Custom Code Line 2"");
				//throw new System.Exception(""Some error occurred"");

				System.Console.WriteLine(""Custom Code Line 3"");
				System.Console.WriteLine(""Custom Code Line 4"");
				throw new System.Exception(""Some error occurred"");
				";

			var appDomainWrapper = new AppDomainWrapper("MyTestAppDomain");
			var result = appDomainWrapper.RunCodeInProcess(codeToRun);

			var expectedResult =
@"Custom Code Line 1
Custom Code Line 2
Custom Code Line 3
Custom Code Line 4
Some error occurred
";
			AssertEquals(expectedResult, result);
		}

		public void TestRunCodeInProcessWithGetSet()
		{
			var codeToRun = @"
				DataStore.SetData(""temp"", ""Some ERROR occurred"");
				System.Console.WriteLine(""Custom Code Line 1"");
				System.Console.WriteLine(""Custom Code Line 2"");
				//throw new System.Exception(""Some error occurred"");

				System.Console.WriteLine(""Custom Code Line 3"");
				System.Console.WriteLine(""Custom Code Line 4"");
				throw new System.Exception((string)DataStore.GetData(""temp""));
				";

			var appDomainWrapper = new AppDomainWrapper("MyTestAppDomain");
			var result = appDomainWrapper.RunCodeInProcess(codeToRun);

			var expectedResult =
@"Custom Code Line 1
Custom Code Line 2
Custom Code Line 3
Custom Code Line 4
Some ERROR occurred
";
			AssertEquals(expectedResult, result);
		}

		public void TestRunCodeInProcess48Long()
		{
			var codeToRun = @"
				System.Console.WriteLine(""Custom Code Line 1"");
				System.Console.WriteLine(""Custom Code Line 2"");
				//throw new System.Exception(""Some error occurred"");

				System.Threading.Thread.Sleep(System.TimeSpan.FromSeconds(1));
				System.Console.WriteLine(""Custom Code Line 3"");
				System.Console.WriteLine(""Custom Code Line 4"");
				throw new System.Exception(""Some error occurred"");
				";

			var appDomainWrapper = new AppDomainWrapper("MyTestAppDomain");
			var result = appDomainWrapper.RunCodeInProcess48(codeToRun);

			var expectedResult =
@"Custom Code Line 1
Custom Code Line 2
Custom Code Line 3
Custom Code Line 4
Some error occurred
";
			AssertEquals(expectedResult, result);
		}

		public void TestRunCodeInProcess48()
		{
			var codeToRun = @"
				System.Console.WriteLine(""Custom Code Line 1"");
				System.Console.WriteLine(""Custom Code Line 2"");
				//throw new System.Exception(""Some error occurred"");

				System.Console.WriteLine(""Custom Code Line 3"");
				System.Console.WriteLine(""Custom Code Line 4"");
				throw new System.Exception(""Some error occurred"");
				";

			var appDomainWrapper = new AppDomainWrapper("MyTestAppDomain");
			var result = appDomainWrapper.RunCodeInProcess48(codeToRun);

			var expectedResult =
@"Custom Code Line 1
Custom Code Line 2
Custom Code Line 3
Custom Code Line 4
Some error occurred
";
			AssertEquals(expectedResult, result);
		}

		public void TestRunCodeInProcessWithGetSet48()
		{
			var codeToRun = @"
				DataStore.SetData(""temp"", ""Some ERROR occurred"");
				System.Console.WriteLine(""Custom Code Line 1"");
				System.Console.WriteLine(""Custom Code Line 2"");
				//throw new System.Exception(""Some error occurred"");

				System.Console.WriteLine(""Custom Code Line 3"");
				System.Console.WriteLine(""Custom Code Line 4"");
				throw new System.Exception((string)DataStore.GetData(""temp""));
				";

			var appDomainWrapper = new AppDomainWrapper("MyTestAppDomain");
			var result = appDomainWrapper.RunCodeInProcess48(codeToRun);

			var expectedResult =
@"Custom Code Line 1
Custom Code Line 2
Custom Code Line 3
Custom Code Line 4
Some ERROR occurred
";
			AssertEquals(expectedResult, result);
		}

		public void TestRunStaticMethodInProcess48()
		{
			var appDomainWrapper = new AppDomainWrapper("MyTestAppDomain");
			var config = new ProcessConfig
			{
				AssemblyDependancies = new List<string>() { "NUnitCore.dll" },
				NamespacePath = "AppDomainWrappers.Net.Test",
				ClassName = nameof(AppDomainWrapperTests),
				MethodName = nameof(StaticMethodForTesting),
			};

			config.AssemblyFile = Path.Combine(config.BinFolder, "AppDomainWrappers.Net.Test.dll");

			var result = appDomainWrapper.RunMethodInProcess48(config);
			var expectedResult =
@"Custom Code Line 1
Custom Code Line 2
Custom Code Line 3
Custom Code Line 4
Some error occurred
";
			AssertEquals(expectedResult, result);
		}

		public void TestRunMethodInProcess48()
		{
			var appDomainWrapper = new AppDomainWrapper("MyTestAppDomain");
			var config = new ProcessConfig
			{
				AssemblyDependancies = new List<string>() { "NUnitCore.dll" },
				NamespacePath = "AppDomainWrappers.Net.Test",
				ClassName = nameof(AppDomainWrapperTests),
				MethodName = nameof(MethodForTesting),
			};

			config.AssemblyFile = Path.Combine(config.BinFolder, "AppDomainWrappers.Net.Test.dll");

			var result = appDomainWrapper.RunMethodInProcess48(config);
			var expectedResult =
@"Custom Code Line 1
Custom Code Line 2
Custom Code Line 3
Custom Code Line 4
Some error occurred
";
			AssertEquals(expectedResult, result);
		}

		public void TestMultipleProcessesRunningMethod()
		{
			var appDomainWrapper = new AppDomainWrapper("MyTestAppDomain");
			var config = new ProcessConfig
			{
				AssemblyDependancies = new List<string>() { "NUnitCore.dll" },
				NamespacePath = "AppDomainWrappers.Net.Test",
				ClassName = nameof(AppDomainWrapperTests),
				MethodName = nameof(LongRunningMethodForTesting),
			};

			config.AssemblyFile = Path.Combine(config.BinFolder, "AppDomainWrappers.Net.Test.dll");
			config.MethodParameters = new string[] { "2" };

			var result1 = "";
			var task = new Task(() =>
			{
				result1 = appDomainWrapper.RunMethodInProcess48(config);
			});
			task.Start();

			var expectedResult =
@"Custom Code Line 1
Custom Code Line 2
";

			var result2 = appDomainWrapper.RunMethodInProcess48(config);
			task.Wait();

			AssertEquals(expectedResult, result1);
			AssertEquals(expectedResult, result2);
		}

		public void TestRunStaticMethodThrowsException()
		{
			var ex = AssertExceptionThrown<Exception>(() => StaticMethodForTesting());
			Assert(ex.Message.Equals("Some error occurred"));
		}

		[SuppressMessage("CargoWiseOne", "CW1106:Do Not Leave In Debug Messages", Justification = "Used in unit testing")]
		static void StaticMethodForTesting()
		{
			Console.WriteLine("Custom Code Line 1");
			Console.WriteLine("Custom Code Line 2");

			Console.WriteLine("Custom Code Line 3");
			Console.WriteLine("Custom Code Line 4");
			throw new Exception("Some error occurred");
		}

		public void TestRunMethodThrowsException()
		{
			var ex = AssertExceptionThrown<Exception>(() => MethodForTesting());
			Assert(ex.Message.Equals("Some error occurred"));
		}

		[SuppressMessage("CargoWiseOne", "CW1106:Do Not Leave In Debug Messages", Justification = "Used in unit testing")]
		static void MethodForTesting()
		{
			Console.WriteLine("Custom Code Line 1");
			Console.WriteLine("Custom Code Line 2");

			Console.WriteLine("Custom Code Line 3");
			Console.WriteLine("Custom Code Line 4");
			throw new Exception("Some error occurred");
		}

		[SuppressMessage("CargoWiseOne", "CW1106:Do Not Leave In Debug Messages", Justification = "Used in unit testing")]
		static void LongRunningMethodForTesting(string sleepTimeInSecounds)
		{
			Console.WriteLine("Custom Code Line 1");
			Thread.Sleep(TimeSpan.FromSeconds(int.Parse(sleepTimeInSecounds)));
			Console.WriteLine("Custom Code Line 2");
		}

		public void TestDisposeCalledTwiceThrowsException()
		{
			var target = new AppDomainWrapper("TestDomain");
			target.Dispose();

			var ex = AssertExceptionThrown<Exception>(() => target.Dispose());
			Assert(ex is ObjectDisposedException);
		}
		#endregion
	}
}
