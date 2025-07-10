using System.IO;
using CargoWise.Common;

namespace CargoWise.IO
{
	public static class TextReaderExtension
	{
		public static void CopyTo(this TextReader source, TextWriter destination)
		{
			Argument.NotNull(source, nameof(source)); // Suggested By ReviewBot 
			Argument.NotNull(destination, nameof(destination)); // Suggested By ReviewBot 
			var baseStream = source is StreamReader ? ((StreamReader)source).BaseStream : null;
			long initialPosition = 0;
			if (baseStream != null)
			{
				initialPosition = baseStream.Position;
			}
			char[] buffer = new char[2048];
			int length;
			do
			{
				length = source.Read(buffer, 0, buffer.Length);
				if (length > 0 && buffer.Length >= length)
				{
					destination.Write(buffer, 0, length);
				}
			}
			while (length > 0);

			if (baseStream != null && baseStream.CanSeek)
			{
				baseStream.Position = initialPosition;
			}
		}

		public static bool ContainsTheSameDataAs(this TextReader s1, TextReader s2)
		{
			Argument.NotNull(s1, nameof(s1)); // Suggested By ReviewBot 
			Argument.NotNull(s2, nameof(s2)); // Suggested By ReviewBot 
			Stream baseS1 = s1 is StreamReader ? ((StreamReader)s1).BaseStream : null;
			Stream baseS2 = s2 is StreamReader ? ((StreamReader)s2).BaseStream : null;

			long s1InitialPosition = 0;
			long s2InitialPosition = 0;

			if (baseS1 != null)
			{
				s1InitialPosition = baseS1.Position;
			}
			if (baseS2 != null)
			{
				s2InitialPosition = baseS2.Position;
			}

			bool result = true;
			int b1;
			do
			{
				b1 = s1.Read();
				if (s2.Read() != b1)
				{
					result = false;
					break;
				}
			}
			while (b1 != -1);

			if (baseS1 != null && baseS1.CanSeek)
			{
				baseS1.Position = s1InitialPosition;
				((StreamReader)s1).DiscardBufferedData();
			}
			if (baseS2 != null && baseS2.CanSeek)
			{
				baseS2.Position = s2InitialPosition;
				((StreamReader)s2).DiscardBufferedData();
			}
			return result;
		}
	}
}
