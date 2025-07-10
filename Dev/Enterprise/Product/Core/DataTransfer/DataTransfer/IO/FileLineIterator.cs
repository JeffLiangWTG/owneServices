using System.Collections;
using System.IO;

namespace Enterprise.DataTransfer.IO
{
	public class FileLineIterator : StreamReader, IEnumerable, IEnumerator
	{
		public FileLineIterator(string uri)
			: base(uri)
		{
		}

		public FileLineIterator(Stream stream)
			: base(stream)
		{
		}

		public IEnumerator GetEnumerator()
		{
			return this;
		}

		public void Reset()
		{
			BaseStream.Position = 0;
		}

		public object Current
		{
			get { return lineFromFile; }
		}

		public bool MoveNext()
		{
			lineFromFile = ReadLine();
			return lineFromFile != null;
		}

		string lineFromFile;
	}
}
