using System.Collections.Generic;
using System.IO;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(CustomsMessageExporter))]
sealed class CustomsMessageExporterTest : TestCaseWithFactory
{
	public void TestSaveToFile()
	{
		var message = CreateTestMessage();
		var tempPath = EnvProxy.Instance.TempPath;
		var expectedFileName = Path.Combine(tempPath, "123.txt");
		var createdFile = CustomsMessageExporter.SaveToFile(message, tempPath);

		try
		{
			AssertEquals("File path", expectedFileName, createdFile);
			Assert(File.Exists(expectedFileName));
			AssertEquals("File content", "Test message", File.ReadAllText(expectedFileName));
		}
		finally
		{
			if (File.Exists(createdFile))
			{
				File.Delete(createdFile);
			}
			if (File.Exists(expectedFileName))
			{
				File.Delete(expectedFileName);
			}
		}
	}

	public void TestSaveToFileWhenSameFileExists()
	{
		var message = CreateTestMessage();
		var tempPath = EnvProxy.Instance.TempPath;
		var filesToDelete = new List<string>();

		try
		{
			var expectedFileName = Path.Combine(tempPath, "123.txt");
			var createdFile = CustomsMessageExporter.SaveToFile(message, tempPath);
			AssertEquals("File path", expectedFileName, createdFile);
			filesToDelete.Add(createdFile);
			filesToDelete.Add(expectedFileName);

			expectedFileName = Path.Combine(tempPath, "123 (1).txt");
			createdFile = CustomsMessageExporter.SaveToFile(message, tempPath);
			AssertEquals("After same file download", expectedFileName, createdFile);
			filesToDelete.Add(createdFile);
			filesToDelete.Add(expectedFileName);
		}
		finally
		{
			filesToDelete.ForEach(File.Delete);
		}
	}

	public void TestFileExtensionForGoodsRegistration()
	{
		var message = CreateTestMessage();
		message.EM_MessageSubType = IN.Business.EDIMessageSubTypeList.Codes.GoodsRegistration;
		var tempPath = EnvProxy.Instance.TempPath;
		var expectedFileName = Path.Combine(tempPath, "123.gr");
		var createdFile = CustomsMessageExporter.SaveToFile(message, tempPath);
		try
		{
			AssertEquals("File path", expectedFileName, createdFile);
			Assert(File.Exists(expectedFileName));
			AssertEquals("File content", "Test message", File.ReadAllText(expectedFileName));
		}
		finally
		{
			if (File.Exists(createdFile))
			{
				File.Delete(createdFile);
			}
			if (File.Exists(expectedFileName))
			{
				File.Delete(expectedFileName);
			}
		}
	}

	EDIMessage CreateTestMessage()
	{
		var message = Factory.New<EDIMessage>();
		message.EM_MessageText = "Test message";
		message.EM_MessageNum = "123";
		message.EM_MessageType = "txt";
		return message;
	}
}
