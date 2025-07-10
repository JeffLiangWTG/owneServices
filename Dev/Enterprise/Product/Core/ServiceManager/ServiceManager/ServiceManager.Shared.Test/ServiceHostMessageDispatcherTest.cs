using System;
using System.IO;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Shared.Testing
{
	class ServiceHostMessageDispatcherTest
	{
		[Test]
		public void TestSendErrorReport()
		{
			ServiceHostMessageDispatcher serviceHostMessageDispatcher = new();

			Assert.Multiple(() =>
			{
				Test("ASD", $"STER:ASD");
				Test("DSA", $"STER:DSA");
			});

			void Test(string taskCode, string expectedResult)
			{
				// Arrange
				using (var consoleOutput = new ConsoleOutput())
				{
					// Act
					serviceHostMessageDispatcher.SendErrorReport(taskCode);

					// Assert
					Assert.That(consoleOutput.GetOuput(), Is.EqualTo(expectedResult));
				}
			}
		}

		class ConsoleOutput : IDisposable
		{
			public ConsoleOutput()
			{
				stringWriter = new StringWriter();
				originalOutput = Console.Out;
				Console.SetOut(stringWriter);
			}

			public string GetOuput()
			{
				return stringWriter.ToString().Trim();
			}

			public void Dispose()
			{
				Console.SetOut(originalOutput);
				stringWriter.Dispose();
			}

			readonly StringWriter stringWriter;
			readonly TextWriter originalOutput;
		}
	}
}
