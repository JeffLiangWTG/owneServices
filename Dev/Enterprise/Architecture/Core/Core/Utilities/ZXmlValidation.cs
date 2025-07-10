using System.Text;

namespace Enterprise.ZArchitecture.Xml
{
	public static class ZXmlValidation
	{
		public static string EscapeInvalidXmlCharacters(string original)
		{
			var result = new StringBuilder(original.Length);
			foreach (var character in original)
			{
				if (IsLegalXmlCharacter(character))
				{
					result.Append(character);
				}
			}
			return result.ToString();
		}

		public static string EscapeInvalidXmlCharacters(string original, out int removed)
		{
			var removedCounter = 0;
			var result = new StringBuilder(original.Length);
			foreach (var character in original)
			{
				if (IsLegalXmlCharacter(character))
				{
					result.Append(character);
				}
				else
				{
					removedCounter++;
				}
			}

			removed = removedCounter;
			return (removedCounter > 0) ? result.ToString() : original;
		}

		/// <summary>
		/// Checks whether the specified character is a legal xml character (as specified in the Extensible Markup Language (XML) 1.0 (Fifth Edition) document)
		/// 
		/// The valid character range is:
		///		Char ::= #x9 | #xA | #xD | [#x20-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF]
		/// </summary>
		/// <param name="character"></param>
		/// <returns></returns>
		public static bool IsLegalXmlCharacter(int character)
		{
			return
			(
				character == 0x9 /* == '\t' == 9   */ ||
				character == 0xA /* == '\n' == 10  */ ||
				character == 0xD /* == '\r' == 13  */ ||
				(character >= 0x20 && character <= 0xD7FF) ||
				(character >= 0xE000 && character <= 0xFFFD) ||
				(character >= 0x10000 && character <= 0x10FFFF)
			);
		}
	}
}
