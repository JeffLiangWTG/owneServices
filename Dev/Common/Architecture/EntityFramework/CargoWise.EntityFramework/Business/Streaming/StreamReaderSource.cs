using System.IO;
using CargoWise.Common;
using CargoWise.IO;

namespace CargoWise.EntityFramework
{
	public class StreamReaderSource : ITextReaderSource
	{
		public StreamReaderSource(Stream stream)
		{
			this.stream = Argument.NotNull(stream, "stream");
		}
		readonly Stream stream;

		public TextReader GetReader(bool closeUnderlyingStream)
		{
			stream.Position = 0;
			return new StreamReader(closeUnderlyingStream ? stream : new UnclosableStreamWrapper(stream));
		}
	}
}
