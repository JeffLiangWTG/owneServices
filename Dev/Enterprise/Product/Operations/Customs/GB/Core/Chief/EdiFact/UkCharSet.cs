using System.Linq;
using Enterprise.Edifact;

namespace Enterprise.Customs.GB.Chief.EdiFact
{
	public class UkCharSet : UNOACharacterSet
	{
		public UkCharSet(char subElement, char element, char escape, char segment)
			: base(subElement, element, escape, segment)
		{
			this.replaceEscapedCharactersWithSpace = false;
		}

		public string RemoveIllegalCharacters(string element)
		{
			return KeepChars(element, ValidUNOACharacters, "");
		}

		protected override string ReplaceIllegalCharacters(string element)
		{
			return KeepChars(element, ValidUNOACharacters, " ");
		}

		protected string ValidUNOACharacters
		{   // We need to be able to send MUCR="A:12345678" for example.
			get { return ValidCharacters + SegmentDelimiter + SubElementDelimiter + ElementDelimiter + EscapeCharacter + ":"; }
		}

		public UkCharSet()
			: base()
		{
			this.replaceEscapedCharactersWithSpace = false;
		}

		public bool ContainsValidUNOACharacters(char keyChar)
		{
			return ValidUNOACharacters.Any(character => character == keyChar);
		}
	}
}
