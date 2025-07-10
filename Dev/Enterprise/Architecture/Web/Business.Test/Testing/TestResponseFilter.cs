#if DEBUG

using System;
using System.IO;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	#region BaseTestHttpFilter

	public abstract class BaseTestHttpFilter : Stream
	{
		protected BaseTestHttpFilter(Stream baseStream)
		{
			this.fBaseStream = baseStream;
		}

		protected Stream BaseStream
		{
			get { return fBaseStream; }
		}

		public override bool CanRead
		{
			get { return false; }
		}

		public override bool CanWrite
		{
			get { return !fClosed; }
		}

		public override bool CanSeek
		{
			get { return false; }
		}

		public override void Close()
		{
			fClosed = true;
			fBaseStream.Close();
		}

		protected bool Closed
		{
			get { return fClosed; }
		}

		public override void Flush()
		{
			fBaseStream.Flush();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}

		public override long Length
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override long Position
		{
			get
			{
				throw new NotSupportedException();
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		readonly Stream fBaseStream;
		bool fClosed;
	}

	#endregion

	#region TestReponseFilter

	public class TestResponseFilter : BaseTestHttpFilter
	{
		public TestResponseFilter(Stream baseStream, MemoryStream memoryStream)
			: base(baseStream)
		{
			fMemoryStream = memoryStream;
		}

		public MemoryStream MemoryStream
		{
			get
			{
				return fMemoryStream;
			}
		}
		readonly MemoryStream fMemoryStream;

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (Closed)
			{
				throw new ObjectDisposedException("PassThroughFilter");
			}

			MemoryStream.Write(buffer, offset, count);
			BaseStream.Write(buffer, offset, count);
		}
	}
	#endregion
}
#endif
