using NUnit.Framework;

namespace Enterprise.Customs.CA.Messaging.Testing
{
	sealed class CACharSetTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestEscapeDelimiterChars()
		{
			NUnit.Framework.Assert.That(charSet.FormatElement("A'B+C:D?E@F"), NUnit.Framework.Is.EqualTo("A?'B?+C?:D??E@F"));
			NUnit.Framework.Assert.That(charSet.FormatElement("ABC + XYZ Company Int'l"), NUnit.Framework.Is.EqualTo("ABC ?+ XYZ COMPANY INT?'L"));
		}

		[ExpectNoExceptions]
		public void TestReplaceExclamationMarkWithSpace()
		{
			NUnit.Framework.Assert.That(charSet.FormatElement("ABC!"), NUnit.Framework.Is.EqualTo("ABC "));
		}

		[ExpectNoExceptions]
		public void TestFormatSingleChar()
		{
			string cAValidCharacters = charSet.GetValidNonDelimiterCharacters() + EscapedCharacters;
			for (int currentCharacterIndex = 0; currentCharacterIndex < cAValidCharacters.Length - 1; currentCharacterIndex++)
			{
				string aValidCharacter = cAValidCharacters[currentCharacterIndex].ToString();
				if (IsAnEscapedCharacter(aValidCharacter))
				{
					NUnit.Framework.Assert.That(charSet.FormatElement(aValidCharacter), NUnit.Framework.Is.EqualTo(charSet.EscapeCharacterChar + aValidCharacter), "EscapeCharacterChar + EscapedCharacter");
				}
				else
				{
					NUnit.Framework.Assert.That(charSet.FormatElement(aValidCharacter), NUnit.Framework.Is.EqualTo(aValidCharacter), "Non EscapedCharacter");
				}
			}
		}

		protected override void SetUp()
		{
			charSet = new CACharSet();
		}
		CACharSet charSet;

		bool IsAnEscapedCharacter(string validCharacter)
		{
			bool escapedCharacter = false;
			if (EscapedCharacters.IndexOf(validCharacter) > -1)
			{
				escapedCharacter = true;
			}

			return escapedCharacter;
		}

		const string EscapedCharacters = @"'+:?";
	}
}
