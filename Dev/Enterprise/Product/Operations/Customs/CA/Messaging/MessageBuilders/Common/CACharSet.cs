using Enterprise.Edifact;

namespace Enterprise.Customs.CA.Messaging
{
	public class CACharSet : UNOACharacterSet
	{
		public const string CAValidCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 .,-()/=\"%&*;<>@";

		public CACharSet(char subElement, char element, char escape, char segment)
			: base(subElement, element, escape, segment)
		{
			EscapeDelimiterChars();
		}

		public CACharSet()
			: base()
		{
			EscapeDelimiterChars();
		}

		void EscapeDelimiterChars()
		{
			SetStandardDelimiters();
			validCharacters = CAValidCharacters + AllDelimiterCharacters();
			replaceEscapedCharactersWithSpace = false;
		}

		protected override string ReplaceIllegalCharacters(string element)
		{
			return KeepChars(element, validCharacters, " ");
		}

		protected override string KeepCharsWithoutDiacritics(string value)
		{
			return base.KeepCharsWithoutDiacritics(FormatElement(value));
		}
	}
}
