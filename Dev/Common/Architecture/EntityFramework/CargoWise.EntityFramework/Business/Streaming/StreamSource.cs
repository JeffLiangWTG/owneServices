using System;
using System.IO;
using CargoWise.Common;

namespace CargoWise.EntityFramework
{
	public class StreamSource : IStreamSource, IDisposable
	{
		Stream stream;
		bool disposed;

		public StreamSource(Stream stream)
		{
			Argument.NotNull(stream, nameof(stream));
			this.stream = stream;
		}

		public Stream GetStream()
		{
			return new DisposeProtectedStream(stream);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					stream.Dispose();
					stream = null;
				}

				disposed = true;
			}
		}
	}
}
