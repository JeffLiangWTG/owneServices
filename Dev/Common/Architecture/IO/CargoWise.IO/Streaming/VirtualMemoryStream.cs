using System;
using System.IO;
using CargoWise.Common;

namespace CargoWise.IO
{
	public class VirtualMemoryStream : Stream, ICloneable
	{
		Stream mainStream;
		string tempFilePath;
		bool disposed;
		int switchToFileLimitInBytes;
		int initialMemoryStreamSizeInBytes;

		const int DefaultBufferSize = 0x1000;
		const int DefaultSwitchToFileLimitInBytes = 32 * 1024;

		public VirtualMemoryStream()
			: this(DefaultSwitchToFileLimitInBytes, DefaultSwitchToFileLimitInBytes)
		{ }

		public VirtualMemoryStream(int switchToFileLimitInBytes)
			: this(switchToFileLimitInBytes, switchToFileLimitInBytes)
		{
			if (switchToFileLimitInBytes < 0)
			{
				throw new ArgumentException("Invalid argument.", nameof(switchToFileLimitInBytes));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public VirtualMemoryStream(int switchToFileLimitInBytes, int initialMemoryStreamSizeInBytes)
		{
			if (switchToFileLimitInBytes < 0)
			{
				throw new ArgumentException("Invalid argument.", nameof(switchToFileLimitInBytes));
			}

			if (initialMemoryStreamSizeInBytes < 0)
			{
				throw new ArgumentException("Invalid argument.", nameof(initialMemoryStreamSizeInBytes));
			}

			if (initialMemoryStreamSizeInBytes > switchToFileLimitInBytes)
			{
				throw new ArgumentException("Invalid argument.", nameof(initialMemoryStreamSizeInBytes));
			}

			InitialMemoryStreamSizeInBytes = initialMemoryStreamSizeInBytes;
			SwitchToFileLimitInBytes = switchToFileLimitInBytes;
			mainStream = new MemoryStream(InitialMemoryStreamSizeInBytes);
			tempFilePath = "";
		}

		protected int SwitchToFileLimitInBytes
		{
			get
			{
				return switchToFileLimitInBytes;
			}
			private set
			{
				switchToFileLimitInBytes = value;
			}
		}

		protected int InitialMemoryStreamSizeInBytes
		{
			get
			{
				return initialMemoryStreamSizeInBytes;
			}
			private set
			{
				initialMemoryStreamSizeInBytes = value;
			}
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
			get
			{
				return mainStream.Length;
			}
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
			return mainStream.Read(buffer, offset, count);  // Simply call base Read Method
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
			if (disposed)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			if (!IsSwitchedToFile && mainStream.Position + count > SwitchToFileLimitInBytes)
			{
				SwitchMemoryStreamToFileStream();
			}

			try
			{
				WriteToMainStream(buffer, offset, count);
			}
			catch (IOException exception)
			{
				throw new IOException(string.Format("VirtualMemoryStream unable to write to a file {0} on {1} machine - {2}", tempFilePath, Environment.MachineName, exception.Message), exception);
			}
		}

		#endregion

		#region IClonable

		public object Clone()
		{
			var clone = new VirtualMemoryStream(SwitchToFileLimitInBytes);
			CopyStream(clone, this);
			return clone;
		}

		#endregion

		public bool IsSwitchedToFile
		{
			get { return !string.IsNullOrEmpty(tempFilePath); }
		}

		protected virtual string GetTempFilePath()
		{
			return Temp.GetTempFileName();
		}

		protected virtual void WriteToMainStream(byte[] buffer, int offset, int count)
		{
			Argument.NotNull(buffer, nameof(buffer)); // Suggested By ReviewBot 
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

			mainStream.Write(buffer, offset, count);
		}

		#region Clean Up

		protected override void Dispose(bool disposing)
		{
			if (disposed)
			{
				return;
			}

			if (disposing)
			{
				mainStream.Dispose();
			}
			disposed = true;
			base.Dispose(disposing);
		}

		#endregion

		#region Implementation

#if DEBUG
		protected virtual
#else
		static
#endif
		long MemoryUsage
		{
			get { return GC.GetTotalMemory(false); }
		}

		protected virtual void SwitchMemoryStreamToFileStream()
		{
			Stream fileStream = null;

			try
			{
				tempFilePath = GetTempFilePath();
				fileStream = File.Create(tempFilePath, DefaultBufferSize, FileOptions.DeleteOnClose);
				CopyStream(fileStream, mainStream);
				mainStream.Close();
				mainStream = fileStream;
			}
			catch (IOException exception)
			{
				if (fileStream != null)
				{
					fileStream.Close();
				}

				throw new IOException(string.Format("VirtualMemoryStream unable to switch to a file {0} on {1} machine - {2}", tempFilePath, Environment.MachineName, exception.Message), exception);
			}
		}

		protected virtual void CopyStream(Stream destination, Stream source)
		{
			Argument.NotNull(destination, nameof(destination));
			Argument.NotNull(source, nameof(source));
			var buffer = new byte[DefaultSwitchToFileLimitInBytes];
			var originalPosition = source.Position;

			try
			{
				source.Position = 0;

				while (true)
				{
					int readCount = source.Read(buffer, 0, buffer.Length);
					if (readCount == 0)
					{
						break;
					}

					destination.Write(buffer, 0, readCount);
				}

				destination.Flush();
				destination.Position = originalPosition;
			}
			finally
			{
				source.Position = originalPosition;
			}
		}

		#endregion
	}
}
