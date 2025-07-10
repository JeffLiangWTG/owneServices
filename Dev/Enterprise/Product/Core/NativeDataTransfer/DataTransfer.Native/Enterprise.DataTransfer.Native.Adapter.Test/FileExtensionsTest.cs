using System.IO;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Adapter
{
	public class FileExtensionsTest : TestCase
	{
		public void TestWriteToFile()
		{
			using (var file = TempFile.New())
			{
				const string content = "This is a test stream";
				var stream = new MemoryStream();
				var writer = new StreamWriter(stream);
				writer.Write(content);
				writer.Flush();

				FileExtensions.WriteToFile(stream, file.Filename);
				Assert(File.Exists(file.Filename));

				using (var reader = new StreamReader(File.OpenRead(file.Filename)))
				{
					AssertEquals(content, reader.ReadToEnd());
				}

				FileExtensions.DeleteFileIfExists(file.Filename);
			}
		}
	}
}
