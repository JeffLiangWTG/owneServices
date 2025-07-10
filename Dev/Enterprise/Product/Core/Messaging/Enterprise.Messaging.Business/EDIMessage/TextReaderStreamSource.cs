using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Messaging.Integration;

namespace Enterprise.Messaging.Business
{
	class TextReaderStreamSource : IStreamSource
	{
		internal TextReaderStreamSource(ITextReaderSource source)
		{
			this.source = Argument.NotNull(source, "source");
		}
		readonly ITextReaderSource source;

		Stream IStreamSource.GetStream()
		{
			Stream result = null;
			var textReader = source.GetReader();
			if (textReader != null)
			{
				var streamReader = textReader as StreamReader;
				if (streamReader != null)
				{
					result = streamReader.BaseStream;
				}
				else	// I would like this to go away, ideally everything should move to MessageData then this can be removed. There is a potential we are going from
				{			// Stream to TextReader and back which will be dealt with by the "if" above. Possibly the "else" should be an exception. Safety first for now.
					result = new VirtualMemoryStream();
					var writer = new StreamWriter(result, MessageEncoding.UTF8WithoutBOM);
					writer.AddStream(textReader);
				}
				result.Position = 0;
			}
			return result;
		}
	}
}
