using System;
using System.IO;
using NUnit.Framework;
using SysEnv = System.Environment;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	class FileContentHelperTests : TestCase
	{
		public void TestFileContentHelper_WhenFileContentMatches_ReturnsFalse()
		{
			var path = Path.GetTempFileName();
			var content = "test content";
			File.WriteAllText(path, content);

			var result = FileContentHelper.FileContentsDiffer(path, content);

			Assert($"Differing file content should return true.{SysEnv.NewLine}Content: {content}", !result);
			File.Delete(path);
		}

		public void TestFileContentHelper_WhenFileContentDiffers_ReturnsTrue()
		{
			var originalText = "original";
			var newText = "new";
			var path = Path.GetTempFileName();
			File.WriteAllText(path, originalText);

			var result = FileContentHelper.FileContentsDiffer(path, newText);

			Assert($"Differing file content should return true.{SysEnv.NewLine}Original: {originalText}{SysEnv.NewLine}New Text: {newText}", result);
			File.Delete(path);
		}

		public void TestFileContentHelper_WhenFileDoesNotExist_ReturnsTrue()
		{
			var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".txt");

			var result = FileContentHelper.FileContentsDiffer(path, "anything");

			Assert("Non-existant file paths are considered as differing as content, and thus should return true.", result);
		}

		public void TestFileContentHelper_WhenPathIsNull_ThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() =>
				FileContentHelper.FileContentsDiffer(null, "content"));
		}

		public void TestFileContentHelper_WhenPathIsEmpty_ThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() =>
				FileContentHelper.FileContentsDiffer(string.Empty, "content"));
		}

		public void TestFileContentHelper_WhenPathIsWhiteSpace_ThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() =>
				FileContentHelper.FileContentsDiffer("  ", "content"));
		}

		public void TestFileContentHelper_WhenNewTextIsNull_ThrowsArgumentNullException()
		{
			var path = Path.GetTempFileName();

			AssertExceptionThrown<ArgumentNullException>(() =>
				FileContentHelper.FileContentsDiffer(path, null));

			File.Delete(path);
		}
	}
}
