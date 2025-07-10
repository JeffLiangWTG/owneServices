using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using Moq;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;

namespace ServiceManager.Common.Testing
{
	sealed class JobObjectTest
	{
		[Test]
		public void TestJobKillsProcess()
		{
			// Arrange
			var fileName = Path.Combine(ExecutableDirectory!, "ServiceManager.Common.Test.TestProcessAnyCpu.exe");
			var processStartInfo = new ProcessStartInfo()
			{
				FileName = fileName,
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardInput = true,
			};

			using var process = Process.Start(processStartInfo);
			process!.WaitForExit(5000);

			using (var job = new JobObject(Mock.Of<IHostRegistrySettings>(r => r.ServiceTaskMemoryConstraint == 0)))
			{
				// Act
				job.AddProcess(process.Handle);
				Assert.That(process.HasExited, Is.False);
			}

			// Assert
			Thread.Sleep(2000);
			Assert.That(process.HasExited, Is.True);
		}

		[Test]
		public void TestJobCustomizedMemoryConstraints()
		{
			// Arrange
			const int mb = 1024 * 1024;
			const int mb_16 = 16 * mb;
			const int mb_64 = 64 * mb;
			const int mb_128 = 128 * mb;

			var fileName64bit = Path.Combine(ExecutableDirectory!, "ServiceManager.Common.Test.TestProcessAnyCpu.exe");

			// Act and Assert
			// 64 bit process
			var mem64bit = GetPrivateMemoryUsedByProcess(fileName64bit, 64);
			Assert.That(mem64bit, Is.GreaterThan(mb_16).And.LessThanOrEqualTo(mb_64), $@"Expected mem64bit > 16MB && mem64bit <= 64MB but was {mem64bit} byte");

			mem64bit = GetPrivateMemoryUsedByProcess(fileName64bit, 128);
			Assert.That(mem64bit, Is.GreaterThan(mb_64).And.LessThanOrEqualTo(mb_128), $@"Expected mem64bit > 64MB && mem64bit <= 128MB but was {mem64bit} byte");

			mem64bit = GetPrivateMemoryUsedByProcess(fileName64bit, 128);
			Assert.That(mem64bit, Is.GreaterThan(mb_64).And.LessThanOrEqualTo(mb_128), $@"Expected mem64bit > 64MB && mem64bit <= 128MB but was {mem64bit} byte");
		}

		// return: private memory used by the process (megabytes)
		int GetPrivateMemoryUsedByProcess(string fileName, int processMemoryLimit = 0)
		{
			var registryMock = Mock.Of<IHostRegistrySettings>(r => r.ServiceTaskMemoryConstraint == processMemoryLimit);
			var processStartInfo = new ProcessStartInfo()
			{
				FileName = fileName,
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardInput = true,
			};

			using var process = Process.Start(processStartInfo);
			using var job = new JobObject(registryMock);

			job.AddProcess(process!.Handle);
			process.StandardInput.WriteLine("Ready");
			process.WaitForExit();

			return process.ExitCode;
		}

		string ExecutableDirectory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
	}
}
