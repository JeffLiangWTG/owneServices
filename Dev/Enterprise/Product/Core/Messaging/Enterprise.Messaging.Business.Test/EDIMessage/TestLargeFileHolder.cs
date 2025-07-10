using System.IO;
using CargoWise.EntityFramework;

namespace Enterprise.Messaging.Testing
{
	public class TestLargeFileHolder : ITextReaderSource
	{
		readonly string fileName;
		public TestTextReader Reader;

		public TestLargeFileHolder(string fileName)
		{
			this.fileName = fileName;
		}

		public TextReader GetReader(bool closeUnderlyingStream = true)
		{
			Reader = string.IsNullOrEmpty(fileName) ? null : new TestTextReader(fileName);
			return Reader;
		}
	}
}
