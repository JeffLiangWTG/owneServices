using System.IO;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class ZipStreamTest : TestCase
	{
		public void TestFilenameAndStream()
		{
			using (MemoryStream stream = new MemoryStream())
			{
				ZipStream zipStream = new ZipStream("hello.txt", stream);
				AssertEquals("Filename", "hello.txt", zipStream.Filename);
				AssertEquals("Stream", stream, zipStream.Stream);
			}
		}
	}
}
