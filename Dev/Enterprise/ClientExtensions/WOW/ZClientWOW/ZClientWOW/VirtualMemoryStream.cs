using System.IO;
using CargoWise.IO;

namespace Enterprise.Client.Wow
{
	public class VirtualMemoryStream : Stream
	{
		Stream mainStream;
		string tempFilePath;

		public VirtualMemoryStream(int switchToFileLimitInBytes = 32000)
		{
			SwitchToFileLimitInBytes = switchToFileLimitInBytes;
			mainStream = new MemoryStream();
			tempFilePath = "";
		}

		public int SwitchToFileLimitInBytes
		{
			get;
			private set;
		}

		#region Stream Overrides

		public override bool CanRead
		{
			get { return true; }
		}

		public override bool CanSeek
		{
			get { return true; }
		}

		public override bool CanWrite
		{
			get { return true; }
		}

		public override void Flush()
		{
			mainStream.Flush();
		}

		public override long Length
		{
			get { return mainStream.Length; }
		}

		public override long Position
		{
			get
			{
				return mainStream.Position;
			}
			set
			{
				mainStream.Position = value;
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return mainStream.Read(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return mainStream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			mainStream.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (string.IsNullOrEmpty(tempFilePath))
			{
				if (mainStream.Position + count > SwitchToFileLimitInBytes)
				{
					SwitchMainStreamToFile();
				}
			}

			mainStream.Write(buffer, offset, count);
		}

		#endregion

		protected virtual string GetTempFilePath()
		{
			return Temp.GetTempFileName();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (!string.IsNullOrEmpty(tempFilePath))
				{
					mainStream.Close();
					mainStream.Dispose();
					File.Delete(tempFilePath);
				}
				else
				{
					mainStream.Dispose();
				}
			}
		}

		#region Implementation

		void SwitchMainStreamToFile()
		{
			tempFilePath = GetTempFilePath();
			Stream fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.ReadWrite);
			mainStream.Position = 0;
			AddStream(fileStream, mainStream);
			mainStream.Close();
			mainStream.Dispose();
			mainStream = fileStream;
		}

		void AddStream(Stream writer, Stream reader)
		{
			byte[] buffer = new byte[32000];

			while (true)
			{
				int readCount = reader.Read(buffer, 0, buffer.Length);
				if (readCount == 0)
				{
					break;
				}

				writer.Write(buffer, 0, readCount);
			}

			writer.Flush();
		}

		#endregion
	}
}
