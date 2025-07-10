using System.IO;
using System.Text;
using CargoWise.IO;
using NUnit.Framework;

namespace Enterprise.Client.Wow.Testing
{
	class TestVirtualMemoryStream : TestCase
	{
		const string testString = "This is a test string";
		public void TestVitualMemoryStream()
		{
			VirtualMemoryStream stream = new VirtualMemoryStream();
			AssertEquals(true, stream.CanRead);
			AssertEquals(true, stream.CanSeek);
			AssertEquals(true, stream.CanWrite);
			var encoder = new UTF8Encoding();
			byte[] inputByteArray = encoder.GetBytes(testString);
			stream.Write(inputByteArray, 0, inputByteArray.Length);
			stream.Flush();
			AssertEquals(inputByteArray.Length, stream.Position);
			AssertEquals(inputByteArray.Length, stream.Length);
			stream.Position = 0;
			byte[] ouputByteArray = new byte[32000];
			int read = stream.Read(ouputByteArray, 0, ouputByteArray.Length);
			AssertEquals(read, inputByteArray.Length);
		}

		public void TestSwitchToFileStream()
		{
			var encoder = new UTF8Encoding();
			byte[] inputByteArray = encoder.GetBytes(testString);
			string tempFilePath = Temp.GetTempFileName();
			File.Delete(tempFilePath);
			TestVirtualStream stream = new TestVirtualStream(inputByteArray.Length + 10, tempFilePath);
			stream.Write(inputByteArray, 0, inputByteArray.Length);
			Assert("virtual file shouldn't be created", !File.Exists(tempFilePath));
			stream.Write(inputByteArray, 0, inputByteArray.Length);
			Assert("virtual file should be created", File.Exists(tempFilePath));
			stream.Position = 0;
			Assert("Streams have different length", stream.Length == inputByteArray.Length * 2);
			for (int i = 0; i < stream.Length; i++)
			{
				if (stream.ReadByte() != inputByteArray[i % inputByteArray.Length])
				{
					Assert("Streams are different", stream.ReadByte() == inputByteArray[i % inputByteArray.Length]);
				}
			}

			stream.Dispose();
			Assert("virtual file should be deleted", !File.Exists(tempFilePath));
		}

		class TestVirtualStream : VirtualMemoryStream
		{
			readonly string tempFilePath;
			public TestVirtualStream(int switchToFileLimitInBytes, string tempFilePath) : base(switchToFileLimitInBytes)
			{
				this.tempFilePath = tempFilePath;
			}

			protected override string GetTempFilePath()
			{
				return tempFilePath;
			}
		}
	}
}
