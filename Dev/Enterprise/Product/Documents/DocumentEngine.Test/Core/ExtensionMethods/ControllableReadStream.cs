using System;
using System.IO;

namespace Enterprise.DocumentEngine.Testing
{
	public class ControllableReadStream : Stream
	{
		public ControllableReadStream(int bytesToReturnInEachChunk, byte[] input)
		{
			this.buffer = input;
			this.bytesToReturnInEachChunk = bytesToReturnInEachChunk;
			this.position = 0;
		}
		readonly byte[] buffer;
		readonly int bytesToReturnInEachChunk;
		int position;

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
		}

		public override long Length
		{
			get { return buffer.Length; }
		}

		public override long Position
		{
			get { return position; }
			set { position = (int)value; }
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			int bytesRead = 0;
			int length = (int)this.Length;
			for (int counter = 0; counter < bytesToReturnInEachChunk && position < length; counter++)
			{
				buffer[offset] = this.buffer[position];
				position++;
				offset++;
				bytesRead++;
			}
			return bytesRead;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			switch (origin)
			{
				case SeekOrigin.Begin:
					position = (int)offset;
					break;
				case SeekOrigin.Current:
					position = (int)(position + offset);
					break;
				case SeekOrigin.End:
					position = (int)(Length - offset - 1);
					break;
			}
			return position;
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException();
		}
	}
}
