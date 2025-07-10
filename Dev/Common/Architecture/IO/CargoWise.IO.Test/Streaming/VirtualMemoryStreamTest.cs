using System;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace CargoWise.IO.Testing
{
	class TestVirtualMemoryStream : TestCase
	{
		const string TestString = "This is a test string";

		readonly byte[] InputByteArray = new UTF8Encoding().GetBytes(TestString);

		public void TestVirtualMemoryStreamConstructors()
		{
			AssertMemoryStream(new VirtualMemoryStream());
			AssertMemoryStream(new VirtualMemoryStream(InputByteArray.Length));
			AssertMemoryStream(new VirtualMemoryStream(InputByteArray.Length, InputByteArray.Length));
		}

		public void TestSwitchToFileStream()
		{
			string tempFilePath = GetTempFilePathString();
			var content = new byte[InputByteArray.Length * 2];
			var stream = new MockVirtualStream(InputByteArray.Length + 10, tempFilePath);

			stream.Write(InputByteArray, 0, InputByteArray.Length);
			Assert("virtual file shouldn't be created", !File.Exists(tempFilePath));
			System.Buffer.BlockCopy(InputByteArray, 0, content, 0, InputByteArray.Length);

			stream.Write(InputByteArray, 0, InputByteArray.Length);
			Assert("virtual file should be created", File.Exists(tempFilePath));
			System.Buffer.BlockCopy(InputByteArray, 0, content, InputByteArray.Length, InputByteArray.Length);

			AssertEquals("Stream Length: ", content.Length, stream.Length);
			AssertStream(content, stream);

			stream.Dispose();
			Assert("virtual file should be deleted", !File.Exists(tempFilePath));
		}

		public void TestSwitchToFileStream_FromInitialMemoryStreamSize()
		{
			string tempFilePath = GetTempFilePathString();
			var content = new byte[InputByteArray.Length * 2];
			var stream = new MockVirtualStream(InputByteArray.Length + 10, 10, tempFilePath);

			stream.Write(InputByteArray, 0, InputByteArray.Length);
			Assert("virtual file shouldn't be created", !File.Exists(tempFilePath));
			System.Buffer.BlockCopy(InputByteArray, 0, content, 0, InputByteArray.Length);

			stream.Write(InputByteArray, 0, InputByteArray.Length);
			Assert("virtual file should be created", File.Exists(tempFilePath));
			System.Buffer.BlockCopy(InputByteArray, 0, content, InputByteArray.Length, InputByteArray.Length);

			AssertEquals("Stream Length: ", content.Length, stream.Length);
			AssertStream(content, stream);

			stream.Dispose();
			Assert("virtual file should be deleted", !File.Exists(tempFilePath));
		}

		public void TestClone()
		{
			var stream = new VirtualMemoryStream();
			stream.Write(InputByteArray, 0, InputByteArray.Length);
			stream.Flush();

			var clone = stream.Clone();
			AssertNotNull("Clone", clone);
			Assert("Clone should be an instance of VirtualMemoryStream", clone is VirtualMemoryStream);

			var streamClone = clone as VirtualMemoryStream;
			Assert("Clone should be a different isntance", streamClone != stream);
			AssertEquals("Can Read: ", stream.CanRead, streamClone.CanRead);
			AssertEquals("Can Seek: ", stream.CanSeek, streamClone.CanSeek);
			AssertEquals("Can Write: ", stream.CanWrite, streamClone.CanWrite);
			AssertEquals("Position: ", stream.Position, streamClone.Position);
			AssertStream(InputByteArray, streamClone);
		}

		public void TestFailedSwitchToFile()
		{
			string tempFilePath = GetTempFilePathString();
			var stream = new MockVirtualStream(InputByteArray.Length, tempFilePath);
			stream.ThrowIOExceptionOnGetTempFilePath = true;

			stream.Write(InputByteArray, 0, InputByteArray.Length);
			Assert("File is yet to be created", !File.Exists(tempFilePath));
			AssertExceptionThrown<IOException>(() => stream.Write(InputByteArray, 0, InputByteArray.Length));
		}

		public void TestFinalizerOnIOExceptionDuringSwitch()
		{
			string tempFilePath = GetTempFilePathString();
			AssertExceptionThrown<IOException>(() => MakeVirtualMemoryStreamThrowIOExceptionOnSwitch(tempFilePath));
			AssertGCCollect(tempFilePath);
		}

		void MakeVirtualMemoryStreamThrowIOExceptionOnSwitch(string tempFilePath)
		{
			var stream = new MockVirtualStream(InputByteArray.Length + 10, tempFilePath);
			stream.ThrowIOExceptionOnSwitch = true;
			stream.Write(InputByteArray, 0, InputByteArray.Length);
			Assert("virtual file shouldn't be created", !File.Exists(tempFilePath));
			stream.Write(InputByteArray, 0, InputByteArray.Length);
		}

		public void TestDestructorOnIOExceptionDuringWrite()
		{
			string tempFilePath = GetTempFilePathString();
			MakeVirtualMemorySwitchAndThrowIOExceptionOnWrite(tempFilePath);
			AssertGCCollect(tempFilePath);
		}

		void MakeVirtualMemorySwitchAndThrowIOExceptionOnWrite(string tempFilePath)
		{
			var stream = new MockVirtualStream(InputByteArray.Length + 10, tempFilePath);
			stream.Write(InputByteArray, 0, InputByteArray.Length);
			Assert("virtual file shouldn't be created", !File.Exists(tempFilePath));
			stream.Write(InputByteArray, 0, InputByteArray.Length);
			Assert("virtual file should be created", File.Exists(tempFilePath));
			stream.ThrowIOExceptionOnWrite = true;
			AssertExceptionThrown<IOException>(() => stream.Write(InputByteArray, 0, InputByteArray.Length));
		}

		public void TestDeleteFileGarbageCollectWithoutDisposing()
		{
			string tempFilePath = GetTempFilePathString();
			MakeVirtualMemoryStreamToSwitchForFileStreamWithoutDisposing(tempFilePath);
			AssertGCCollect(tempFilePath);
		}

		void MakeVirtualMemoryStreamToSwitchForFileStreamWithoutDisposing(string tempFilePath)
		{
			var stream = new MockVirtualStream(InputByteArray.Length + 10, tempFilePath);
			stream.Write(InputByteArray, 0, InputByteArray.Length);
			Assert("virtual file shouldn't be created", !File.Exists(tempFilePath));
			stream.Write(InputByteArray, 0, InputByteArray.Length);
			Assert("virtual file should be created", File.Exists(tempFilePath));
		}

		public void TestWriteWithExceptionDoesNotCloseStream()
		{
			var tempFilePath = GetTempFilePathString();
			var stream = new MockVirtualStream(InputByteArray.Length + 10, tempFilePath);
			stream.ThrowIOExceptionOnWrite = true;
			AssertExceptionThrown<IOException>(() => stream.Write(InputByteArray, 0, InputByteArray.Length));
			AssertNoExceptionThrown(() =>
			{
				long position = stream.Position;
			});
		}

		#region Asserts

		void AssertMemoryStream(Stream stream)
		{
			AssertEquals(true, stream.CanRead);
			AssertEquals(true, stream.CanSeek);
			AssertEquals(true, stream.CanWrite);

			stream.Write(InputByteArray, 0, InputByteArray.Length);
			stream.Flush();
			AssertEquals(InputByteArray.Length, stream.Position);
			AssertEquals(InputByteArray.Length, stream.Length);
			stream.Position = 0;
			var outputByteArray = new byte[32000];
			int read = stream.Read(outputByteArray, 0, outputByteArray.Length);
			AssertEquals(read, InputByteArray.Length);
		}

		void AssertGCCollect(string tempFilePath)
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			Assert("virtual file should be deleted", !File.Exists(tempFilePath));
		}

		void AssertStream(byte[] expected, Stream stream)
		{
			var originalPosition = stream.Position;
			stream.Position = 0;

			var buffer = new byte[stream.Length];
			stream.Read(buffer, 0, (int)stream.Length);
			AssertEquals("Content Length: ", expected.Length, buffer.Length);

			for (int i = 0; i < buffer.Length; i++)
			{
				AssertEquals("Stream content: ", expected[i], buffer[i]);
			}

			stream.Position = originalPosition;
		}

		#endregion

		#region Test Helper

		static string GetTempFilePathString()
		{
			string tempFilePath = Temp.GetTempFileName();
			File.Delete(tempFilePath);
			return tempFilePath;
		}

		class MockVirtualStream : VirtualMemoryStream
		{
			readonly string tempFilePath;

			static Exception ExceptionToThrow { get { return new IOException("There is not enough space on the disk."); } }
			public bool ThrowIOExceptionOnSwitch { get; set; }
			public bool ThrowIOExceptionOnWrite { get; set; }
			public bool ThrowIOExceptionOnGetTempFilePath { get; set; }

			public MockVirtualStream(int switchToFileLimitInBytes, string tempFilePath)
				: base(switchToFileLimitInBytes)
			{
				this.tempFilePath = tempFilePath;
			}

			public MockVirtualStream(int switchToFileLimitInBytes, int initialMemoryStreamSizeInBytes, string tempFilePath)
				: base(switchToFileLimitInBytes, initialMemoryStreamSizeInBytes)
			{
				this.tempFilePath = tempFilePath;
			}

			protected override string GetTempFilePath()
			{
				if (ThrowIOExceptionOnGetTempFilePath)
				{
					throw ExceptionToThrow;
				}

				return tempFilePath;
			}

			protected override void WriteToMainStream(byte[] buffer, int offset, int count)
			{
				if (offset < 0)
				{
					throw new ArgumentException("Invalid argument.", nameof(offset));
				}

				if (count < 0)
				{
					throw new ArgumentException("Invalid argument.", nameof(count));
				}

				if (count > (buffer.Length - offset))
				{
					throw new ArgumentException("Invalid argument.", nameof(count));
				}

				if (ThrowIOExceptionOnWrite)
				{
					throw ExceptionToThrow;
				}

				base.WriteToMainStream(buffer, offset, count);
			}

			protected override void CopyStream(Stream writer, Stream reader)
			{
				if (ThrowIOExceptionOnSwitch)
				{
					throw ExceptionToThrow;
				}

				base.CopyStream(writer, reader);
			}

			protected override long MemoryUsage
			{
				get { return memoryUsage; }
			}
			long memoryUsage = 50;

			public void SetMemoryUsage(long value)
			{
				memoryUsage = value;
			}
		}

		#endregion
	}
}
