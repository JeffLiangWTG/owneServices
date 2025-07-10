namespace Enterprise.Customs.AE.Business;

public class AECharacterSet : Edifact.UNOBCharacterSet
{
	AECharacterSet()
	{
		SubElementDelimiterChar = ':';
		ElementDelimiterChar = '+';
		EscapeCharacterChar = '?';
		SegmentDelimiterChar = '\'';

		validCharacters = ValidCharacters;
		ReplaceEscapedCharactersWithSpace = true;
	}

	public static AECharacterSet New() => new();
}
