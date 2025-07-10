using System;
using System.IO;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.Core
{
	public class UndisposableSteam : Stream
	{
		Stream Stream;

		public UndisposableSteam(Stream stream)
		{
			Argument.NotNull(stream, nameof(stream));
			Stream = stream;
		}

		public override bool CanRead => Stream.CanRead;

		public override bool CanSeek => Stream.CanSeek;

		public override bool CanWrite => Stream.CanWrite;

		public override void Flush()
		{
			CheckDisposed();
			Stream.Flush();
		}

		public override long Length => Stream.Length;

		public override long Position
		{
			get
			{
				return Stream.Position;
			}
			set
			{
				Stream.Position = value;
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			CheckDisposed();
			return Stream.Read(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			CheckDisposed();
			return Stream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			CheckDisposed();
			Stream.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			CheckDisposed();
			Stream.Write(buffer, offset, count);
		}

		public void ShouldDispose()
		{
			shouldDispose = true;
		}
		bool shouldDispose;

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (shouldDispose)
			{
				Stream.Dispose();
			}
		}

		public override void Close()
		{
			base.Close();
			if (shouldDispose)
			{
				Stream = null;
			}
		}

		void CheckDisposed()
		{
			if (Stream == null)
			{
				throw new ObjectDisposedException("UndisposableSteam");
			}
		}
	}
}
