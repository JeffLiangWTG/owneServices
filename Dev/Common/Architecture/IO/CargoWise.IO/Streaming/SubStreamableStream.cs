using System.IO;
using CargoWise.Common.Testing;

namespace CargoWise.IO.Shim
{
	/// <summary>
	/// A stream that can provide segments of itself as a separate stream without copying
	/// </summary>
	public class SubStreamableStream : IO.SubStreamableStream
	{
		/// <summary>
		/// Default constructor mimics VirtualMemoryStream in that the first DEFAULT_MEMORY_STREAM_CHUNK_SIZE bytes 
		/// are a memory stream, but after that we use the file stream.
		/// </summary>
		public SubStreamableStream() : base(DisposableLeakListener.Instance)
		{
		}

		/// <summary>
		/// Create a SubStreamable stream provided a stream creation factory and a chunk size
		/// </summary>
		/// <param name="streamFactory">A factory that can create a stream</param>
		/// <param name="streamChunkSize">The size of each stream chunk to create </param>
		public SubStreamableStream(StreamFactory streamFactory, int streamChunkSize) : base(streamFactory, streamChunkSize, DisposableLeakListener.Instance)
		{
		}

		/// <summary>
		/// Wrap an existing Memory stream, extensions will also be memory streams
		/// </summary>
		/// <param name="stream">A memory stream</param>
		public SubStreamableStream(MemoryStream stream) : base(stream, DisposableLeakListener.Instance)
		{
		}

		/// <summary>
		/// Wrap an existing file stream, extensions will also be file streams
		/// </summary>
		/// <param name="stream">File stream</param>
		public SubStreamableStream(FileStream stream) : base(stream, DisposableLeakListener.Instance)
		{
		}

		/// <summary>
		/// Wrap an existing seekable stream using FileStreams for any extension
		/// </summary>
		/// <param name="stream">A seekable stream</param>
		public SubStreamableStream(Stream stream) : base(stream, DisposableLeakListener.Instance)
		{
		}

		/// <summary>
		/// Wrap an UnclosableStreamWrapper, extensions will be file streams
		/// </summary>
		/// <param name="stream">Unclosable stream</param>
		public SubStreamableStream(UnclosableStreamWrapper stream) : base(stream, DisposableLeakListener.Instance)
		{
		}
	}
}
