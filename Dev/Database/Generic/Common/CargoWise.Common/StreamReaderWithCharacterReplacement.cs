using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CargoWise.Common
{
	public class StreamReaderWithCharacterReplacement : StreamReader
	{
		public StreamReaderWithCharacterReplacement(string filePath, Encoding encoding)
			: base(filePath, encoding)
		{
			Argument.NotNullOrEmpty(filePath, nameof(filePath)); // Suggested By ReviewBot 
			Argument.NotNull(encoding, nameof(encoding)); // Suggested By ReviewBot 
		}

		public StreamReaderWithCharacterReplacement(Stream stream)
			: base(stream)
		{
			Argument.NotNull(stream, nameof(stream)); // Suggested By ReviewBot 
		}

		readonly Dictionary<char, char> replacements = new Dictionary<char, char>();

		public void AddReplacement(char characterToReplace, char replacementCharacter)
		{
			replacements[characterToReplace] = replacementCharacter;
		}

		public override int Read()
		{
			return Replace(base.Read());
		}

		public override int Read(char[] buffer, int index, int count)
		{
			var charsRead = base.Read(buffer, index, count);
			for (int i = index; i < index + charsRead; i++)
			{
				buffer[i] = Replace(buffer[i]);
			}
			return charsRead;
		}

		public override string ReadLine()
		{
			var line = base.ReadLine();
			return string.IsNullOrEmpty(line) ? line : Replace(line);
		}

		public override string ReadToEnd()
		{
			return Replace(base.ReadToEnd());
		}

		string Replace(string source)
		{
			Argument.NotNull(source, nameof(source));
			var result = new StringBuilder(source.Length);
			foreach (var c in source)
			{
				result.Append(Replace(c));
			}
			return result.ToString();
		}

		int Replace(int source)
		{
			return Replace((char)source);
		}

		char Replace(char source)
		{
			char replacement;
			return replacements.TryGetValue(source, out replacement) ? replacement : source;
		}
	}
}
