using System.IO;
using CargoWise.Common;

namespace CargoWise.EntityFramework
{
	public class DisposeProtectedStream : Stream
	{
		readonly Stream stream;
		internal DisposeProtectedStream(Stream stream)
		{
			Argument.NotNull(stream, nameof(stream));
			this.stream = stream;
			this.stream.Position = 0;
		}

		public override bool CanRead => stream.CanRead;
		public override bool CanSeek => stream.CanSeek;
		public override bool CanWrite => stream.CanWrite;
		public override long Length => stream.Length;
		public override long Position
		{
			get => stream.Position;
			set => stream.Position = value;
		}

		public override void Flush()
		{
			stream.Flush();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return stream.Read(buffer, offset, count); // pass through to the underlying stream
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return stream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			stream.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			stream.Write(buffer, offset, count);
		}

		protected override void Dispose(bool disposing)
		{
			// do not dispose the underlying stream, the StreamSource will dispose that on their disposal
		}
	}
}
