using NUnit.Framework;

namespace Enterprise.Barcode.Business.Symbologies.Code128.Testing
{
	sealed class Code128ValuesTest : TestCase
	{
		public void TestCharToCodeForCharacterSetA()
		{
			int code = Code128Values.CharToCode("[FNC4]", Code128CharacterSet.CodeA);
			AssertEquals(101, code);

			code = Code128Values.CharToCode("A", Code128CharacterSet.CodeA);
			AssertEquals(33, code);

			code = Code128Values.CharToCode(" ", Code128CharacterSet.CodeA);
			AssertEquals(0, code);

			code = Code128Values.CharToCode("a", Code128CharacterSet.CodeA);
			AssertEquals(-1, code);
		}

		public void TestCharToCodeForCharacterSetB()
		{
			int code = Code128Values.CharToCode("[FNC4]", Code128CharacterSet.CodeB);
			AssertEquals(100, code);

			code = Code128Values.CharToCode("A", Code128CharacterSet.CodeB);
			AssertEquals(33, code);

			code = Code128Values.CharToCode(" ", Code128CharacterSet.CodeB);
			AssertEquals(0, code);

			code = Code128Values.CharToCode(Code128Values.CodeAShiftValue, Code128CharacterSet.CodeB);
			AssertEquals(101, code);

			code = Code128Values.CharToCode("AA", Code128CharacterSet.CodeB);
			AssertEquals(-1, code);

			code = Code128Values.CharToCode("[FNC8]", Code128CharacterSet.CodeB);
			AssertEquals(-1, code);
		}

		public void TestCharToCodeForCharacterSetC()
		{
			int code = Code128Values.CharToCode("20", Code128CharacterSet.CodeC);
			AssertEquals(20, code);

			code = Code128Values.CharToCode("00", Code128CharacterSet.CodeC);
			AssertEquals(0, code);

			code = Code128Values.CharToCode(Code128Values.CodeAShiftValue, Code128CharacterSet.CodeC);
			AssertEquals(101, code);

			code = Code128Values.CharToCode(Code128Values.CodeBShiftValue, Code128CharacterSet.CodeC);
			AssertEquals(100, code);

			code = Code128Values.CharToCode("A", Code128CharacterSet.CodeC);
			AssertEquals(-1, code);
		}

		public void TestCodeToCharForSetA()
		{
			Code128CharacterSet set = Code128CharacterSet.CodeA;
			string @char = Code128Values.CodeToChar(33, ref set);
			AssertEquals("Common letter in CodeA/CodeB set", "A", @char);

			@char = Code128Values.CodeToChar(98, ref set);
			AssertEquals("Special encoding in CodeA/CodeB set", "", @char);

			@char = Code128Values.CodeToChar(70, ref set);
			AssertEquals("Different letter in CodeA/CodeB set", "f", @char);
		}

		public void TestCodeToCharForSetB()
		{
			Code128CharacterSet set = Code128CharacterSet.CodeB;
			string @char = Code128Values.CodeToChar(33, ref set);
			AssertEquals("Common letter in CodeA/CodeB set", "A", @char);

			@char = Code128Values.CodeToChar(98, ref set);
			AssertEquals("Special encoding in CodeA/CodeB set", "", @char);

			@char = Code128Values.CodeToChar(70, ref set);
			AssertEquals("Different letter in CodeA/CodeB set", "[ACK]", @char);
		}

		public void TestCodeToCharForSetC()
		{
			Code128CharacterSet set = Code128CharacterSet.CodeC;
			string @char = Code128Values.CodeToChar(33, ref set);
			AssertEquals("33", @char);

			@char = Code128Values.CodeToChar(98, ref set);
			AssertEquals("98", @char);

			@char = Code128Values.CodeToChar(100, ref set);
			AssertEquals("", @char);

			@char = Code128Values.CodeToChar(102, ref set);
			AssertEquals("[FNC1]", @char);
		}

		public void TestCharacterSetToIndex()
		{
			int index = Code128Values.CharacterSetToIndex(Code128CharacterSet.CodeB);
			AssertEquals("b start code", 104, index);

			index = Code128Values.CharacterSetToIndex(Code128CharacterSet.CodeAShift);
			AssertEquals("A shift code", 101, index);

			index = Code128Values.CharacterSetToIndex(Code128CharacterSet.CodeAOneCharacterShift);
			AssertEquals("A one character shift code", 98, index);
		}

		public void TestIndexToCharacterSet()
		{
			Code128CharacterSet set = Code128Values.IndexToCharacterSet(99);
			AssertEquals("Code C Shift", Code128CharacterSet.CodeCShift, set);

			set = Code128Values.IndexToCharacterSet(101);
			AssertEquals("Code A Shift", Code128CharacterSet.CodeAShift, set);

			set = Code128Values.IndexToCharacterSet(104);
			AssertEquals("Code B start", Code128CharacterSet.CodeB, set);

			set = Code128Values.IndexToCharacterSet(103);
			AssertEquals("Code A start", Code128CharacterSet.CodeA, set);
		}
	}
}
