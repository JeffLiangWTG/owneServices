using Enterprise.Edifact;

namespace Enterprise.Customs.ES.Messaging
{
	public class UNOAESCharacterSet : UNOACharacterSet
	{
		/// <summary>
		/// For use with Spain EdiFact
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public const string SpainValidCharacters = @"AÁBCDEÉFGHIÍJKLMNÑOÓPQRSTUÚVWXYZaábcdeéfghiíjklmnñoópqrstuúvwxyz0123456789~`!@#$%^&*()-_=+[{]}\|:""<.,>/?";

		public UNOAESCharacterSet()
		{
			SetStandardDelimiters();
			validCharacters = SpainValidCharacters + AllDelimiterCharacters();
			replaceEscapedCharactersWithSpace = false;
		}

		protected override string EnforceCase(string dataPiece)
		{
			return dataPiece;
		}

		protected override string ReplaceIllegalCharacters(string element)
		{
			return KeepChars(element, validCharacters, " ");
		}
	}
}
