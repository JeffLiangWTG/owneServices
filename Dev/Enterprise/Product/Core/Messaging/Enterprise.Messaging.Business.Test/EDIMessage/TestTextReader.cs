using System.IO;

namespace Enterprise.Messaging.Testing
{
	public class TestTextReader : TextReader
	{
		public bool WasDisposed { get; private set; }

		public TestTextReader(string fileName)
		{
			WasDisposed = false;
		}

		public override void Close()
		{
			WasDisposed = true;
		}

		protected override void Dispose(bool disposing)
		{
			WasDisposed = true;
		}
	}
}
