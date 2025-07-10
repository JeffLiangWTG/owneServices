using System.IO;
using System.Text;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	public class FileTextReaderSource : ITextReaderSource
	{
		public FileTextReaderSource(string filePath, Encoding encoding)
		{
			this.filePath = filePath;
			this.encoding = encoding;
		}

		readonly string filePath;
		readonly Encoding encoding;

		public TextReader GetReader(bool closeUnderlyingStream = true)
		{
			return new StreamReader(filePath, encoding);
		}
	}
}
