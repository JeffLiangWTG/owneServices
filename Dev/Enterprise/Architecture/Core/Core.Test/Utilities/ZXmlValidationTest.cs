using NUnit.Framework;

namespace Enterprise.ZArchitecture.Xml.Testing
{
	sealed class ZXmlValidationTest : TestCase
	{
		public void TestEscapeInvalidXmlCharacters()
		{
			int removed;

			var valid = "<apple>Banana</apple>";
			AssertEquals(valid, ZXmlValidation.EscapeInvalidXmlCharacters(valid));
			AssertEquals(valid, ZXmlValidation.EscapeInvalidXmlCharacters(valid, out removed));
			AssertEquals(0, removed);

			var invalid = string.Format("<apple>{0}Banana{1}</apple>", (char)0x02, (char)0x1F);
			var escapedInvalid = "<apple>Banana</apple>";
			AssertEquals(escapedInvalid, ZXmlValidation.EscapeInvalidXmlCharacters(invalid));
			AssertEquals(escapedInvalid, ZXmlValidation.EscapeInvalidXmlCharacters(invalid, out removed));
			AssertEquals(2, removed);

			var invalid2 = string.Format("<apple>\tBanana{0}Orange\n</apple>", (char)0xD800);
			var escapedInvalid2 = "<apple>\tBananaOrange\n</apple>";
			AssertEquals(escapedInvalid2, ZXmlValidation.EscapeInvalidXmlCharacters(invalid2));
			AssertEquals(escapedInvalid2, ZXmlValidation.EscapeInvalidXmlCharacters(invalid2, out removed));
			AssertEquals(1, removed);
		}

		public void TestIsLegalXmlCharacter()
		{
			AssertEquals(true, ZXmlValidation.IsLegalXmlCharacter('a'));
			AssertEquals(true, ZXmlValidation.IsLegalXmlCharacter('9'));
			AssertEquals(true, ZXmlValidation.IsLegalXmlCharacter('@'));

			AssertEquals(true, ZXmlValidation.IsLegalXmlCharacter('\t'));
			AssertEquals(true, ZXmlValidation.IsLegalXmlCharacter('\n'));
			AssertEquals(true, ZXmlValidation.IsLegalXmlCharacter('\r'));

			AssertEquals(true, ZXmlValidation.IsLegalXmlCharacter('<'));
			AssertEquals(true, ZXmlValidation.IsLegalXmlCharacter('>'));
			AssertEquals(true, ZXmlValidation.IsLegalXmlCharacter('&'));
			AssertEquals(true, ZXmlValidation.IsLegalXmlCharacter('"'));
			AssertEquals(true, ZXmlValidation.IsLegalXmlCharacter('/'));

			AssertEquals(false, ZXmlValidation.IsLegalXmlCharacter(0x02));
			AssertEquals(false, ZXmlValidation.IsLegalXmlCharacter(0x1F));
			AssertEquals(false, ZXmlValidation.IsLegalXmlCharacter(0xD800));
			AssertEquals(false, ZXmlValidation.IsLegalXmlCharacter(0xDFFF));
			AssertEquals(false, ZXmlValidation.IsLegalXmlCharacter(0xFFFE));
			AssertEquals(false, ZXmlValidation.IsLegalXmlCharacter(0xFFFF));
			AssertEquals(false, ZXmlValidation.IsLegalXmlCharacter(0x110000));
		}
	}
}
