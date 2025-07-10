using System;
using System.IO;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentEngineCore.Registry.LogDocumentRenderer;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class DocumentRendererDiskLoggerTest : TransactionedTestCase
	{
		public void TestLog()
		{
			using (var tempFile = TempFile.New())
			{
				DocumentsDataRegistry.Instance.LogDocumentRenderer.SetValue(
					Guid.Empty,
					Guid.Empty,
					Guid.Empty,
					new LogDocumentRendererRegistry() { LogFilePath = tempFile.Filename, GenerateCallStacks = ZBool.False });

				using (var logger = new DocumentRendererDiskLogger())
				{
					logger.Log("Hello World");
					logger.Log("Goodbye World");
				}

				var logOutput = File.ReadAllText(tempFile.Filename);

				Assert("Log should contain the message 'Hello World'.", logOutput.Contains("Hello World"));
				Assert("Log should contain the message 'Goodbye World'.", logOutput.Contains("Hello World"));
			}
		}

		public void TestLogWithCallStacks()
		{
			using (var tempFile = TempFile.New())
			{
				DocumentsDataRegistry.Instance.LogDocumentRenderer.SetValue(
					Guid.Empty,
					Guid.Empty,
					Guid.Empty,
					new LogDocumentRendererRegistry() { LogFilePath = tempFile.Filename, GenerateCallStacks = ZBool.True });

				using (var logger = new DocumentRendererDiskLogger())
				{
					logger.Log("Hello World");
					logger.Log("Goodbye World");
				}

				var logOutput = File.ReadAllText(tempFile.Filename);

				Assert("Log should contain the message 'Hello World'.", logOutput.Contains("Hello World"));
				Assert("Log should contain the message 'Goodbye World'.", logOutput.Contains("Hello World"));
				Assert("Log should contain call stacks.", logOutput.Contains("Callstack="));
			}
		}
	}
}
