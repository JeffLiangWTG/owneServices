using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using NUnit.Framework;

namespace GenerateDbUpgraderResources.Tests
{
	[GuiTest]
	public class SetupStartupDirectorTest : TestCase
	{
		public void TestStdOutputMessageWhenNoArgs()
		{
			// Arrange
			const string expectMessage = "Invalid arguments in GenerateDbUpgraderResources";
			var args = new[] { "-InvalidArgs" };
			TestStdOutputMessage(expectMessage, args);
		}

		public void TestStdOutputMessageWhenCWSharedIsEmpty()
		{
			// Arrange
			const string expectMessage = "No valid CWShared path specified.";
			var noCwSharedArgs = new[] { "-MERGE" };
			TestStdOutputMessage(expectMessage, noCwSharedArgs);
		}

		void TestStdOutputMessage(string expectMessage, string[] arguments)
		{
			// Arrange
			Process process = null;
			var messages = new ConcurrentBag<string>();
			var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new NullReferenceException("Assembly location not found");
#if NET
			Path.Combine(exePath, CommonAssemblyInfo.CWNetCoreSubfolder);
#endif
			exePath = Path.Combine(exePath, "GenerateDbUpgraderResources.exe");
			if (!File.Exists(exePath))
			{
				throw new FileNotFoundException($"Executable not found at {exePath}. Ensure the project is built.");
			}

			var startInfo = new ProcessStartInfo()
			{
				FileName = exePath,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				CreateNoWindow = true,
				UseShellExecute = false,
				Arguments = string.Join(" ", arguments),
			};

			try
			{
				// Act
				process = Process.Start(startInfo);
				process.OutputDataReceived += (s, e) => messages.Add(e.Data);
				process.ErrorDataReceived += (s, e) => messages.Add(e.Data);
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();

				// Assert
				Thread.Sleep(1000);
				Assert(messages.Any(x => x.Contains(expectMessage)));
			}
			finally
			{
				const int retries = 3;
				for (var i = 0; i < retries; i++)
				{
					process?.Kill();
					Thread.Sleep(5000);
					if (process?.HasExited ?? true)
					{
						break;
					}
				}
			}
		}
	}
}
