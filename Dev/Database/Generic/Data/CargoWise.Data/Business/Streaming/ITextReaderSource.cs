using System.IO;

namespace CargoWise.EntityFramework
{
	public interface ITextReaderSource
	{
		TextReader GetReader(bool closeUnderlyingStream = true);
	}
}
