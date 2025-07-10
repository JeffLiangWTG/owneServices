using System.IO;
using CargoWise.Types;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Barcode.Business.Testing
{
	sealed class TextBarcodeTest : TestCase
	{
		#region TestTextToEncode

		public void TestTextToEncode()
		{
			var barcode = new TextBarcode("Text To Encode");
			AssertEquals("barcode.TextToEncode", "Text To Encode", barcode.TextToEncode);
		}

		#endregion

		#region TestBarcodeOptimizationWithOddDigitChunkGreaterThan5WithOrWithoutFollowingText

		public void TestBarcodeOptimizationWithOddDigitChunkGreaterThan5WithOrWithoutFollowingText()
		{
			TextBarcode barcode = new TextBarcode("ABCDEFG1234567", true);
			AssertEquals("First digit is rendered as 128B, following digits are rendered as 128C", "ÈABCDEFG1Ã7Mc{Ê", barcode.TextAs128sFontString);

			TextBarcode barcode2 = new TextBarcode("ABCDEFG1234567HIJKLMNOP", true);
			AssertEquals("First digit is rendered as 128B, following digits are rendered as 128C", "ÈABCDEFG1Ã7McÄHIJKLMNOP>Ê", barcode2.TextAs128sFontString);

			TextBarcode barcode3 = new TextBarcode("1234567ABCDEFG", true);
			AssertEquals("First digit is rendered as 128B, following digits are rendered as 128C", "È1Ã7McÄABCDEFGBÊ", barcode3.TextAs128sFontString);
		}

		#endregion

		#region TestBarcodeOptimizationWithEvenDigitChunkGreaterThan5WithOrWithoutFollowingText

		public void TestBarcodeOptimizationWithEvenDigitChunkGreaterThan5WithOrWithoutFollowingText()
		{
			TextBarcode barcode = new TextBarcode("ABCDEFG12345678", true);
			AssertEquals("All digits are rendered as 128C", "ÈABCDEFGÃ,BXn2Ê", barcode.TextAs128sFontString);

			TextBarcode barcode2 = new TextBarcode("ABCDEFG12345678HIJKLMNOP", true);
			AssertEquals("All digits are rendered as 128C", "ÈABCDEFGÃ,BXnÄHIJKLMNOP\\Ê", barcode2.TextAs128sFontString);

			TextBarcode barcode3 = new TextBarcode("12345678ABCDEFG", true);
			AssertEquals("All digits are rendered as 128C", "É,BXnÄABCDEFG^Ê", barcode3.TextAs128sFontString);
		}

		#endregion

		#region TestBarcodeOptimizationWithOddDigitChunkLessThan5WithOrWithoutFollowingText

		public void TestBarcodeOptimizationWithOddDigitChunkLessThan5WithOrWithoutFollowingText()
		{
			TextBarcode barcode = new TextBarcode("ABCDEFG123", true);
			AssertEquals("All digits are rendered as 128B", "ÈABCDEFG123sÊ", barcode.TextAs128sFontString);

			TextBarcode barcode2 = new TextBarcode("ABCDEFG123HIJKLMNOP", true);
			AssertEquals("All digits are rendered as 128B", "ÈABCDEFG123HIJKLMNOP&Ê", barcode2.TextAs128sFontString);
		}

		#endregion

		#region TestBarcodeOptimizationWithEvenDigitChunkExactly4DigitsLongWithoutFollowingText

		public void TestBarcodeOptimizationWithEvenDigitChunkExactly4DigitsLongWithoutFollowingText()
		{
			TextBarcode barcode = new TextBarcode("ABCDEFG1234", true);
			AssertEquals("All digits are rendered as 128C", "ÈABCDEFGÃ,B+Ê", barcode.TextAs128sFontString);
		}

		#endregion

		#region TestBarcodeOptimizationWithEvenDigitChunkExactly4DigitsLongWithFollowingText

		public void TestBarcodeOptimizationWithEvenDigitChunkExactly4DigitsLongWithFollowingText()
		{
			TextBarcode barcode = new TextBarcode("ABCDEFG1234HIJKLMNOP", true);
			AssertEquals("All digits are rendered as 128B", "ÈABCDEFG1234HIJKLMNOP$Ê", barcode.TextAs128sFontString);
		}

		#endregion

		#region TestBarcodeOptimizationWithEvenDigitChunkLessThan4DigitsLongWithOrWithoutFollowingText

		public void TestBarcodeOptimizationWithEvenDigitChunkLessThan4DigitsLongWithOrWithoutFollowingText()
		{
			TextBarcode barcode = new TextBarcode("ABCDEFG12", true);
			AssertEquals("All digits are rendered as 128B", "ÈABCDEFG12ÃÊ", barcode.TextAs128sFontString);

			TextBarcode barcode2 = new TextBarcode("ABCDEFG12HIJKLMNOP", true);
			AssertEquals("All digits are rendered as 128B", "ÈABCDEFG12HIJKLMNOPFÊ", barcode2.TextAs128sFontString);
		}

		#endregion

		#region TestBarcodeOptimizationWithStringContainingEvenDigitsAndOnlyDigits

		public void TestBarcodeOptimizationWithStringContainingEvenDigitsAndOnlyDigits()
		{
			TextBarcode barcode = new TextBarcode("12345678", true);
			AssertEquals("All digits are rendered as 128C", "É,BXnOÊ", barcode.TextAs128sFontString);
		}

		#endregion

		#region TestBarcodeOptimizationWithStringContainingOddDigitsAndOnlyDigits

		public void TestBarcodeOptimizationWithStringContainingOddDigitsAndOnlyDigits()
		{
			TextBarcode barcode = new TextBarcode("1234567", true);
			AssertEquals("First digit is rendered as 128B, following digits are rendered as 128C", "È1Ã7McoÊ", barcode.TextAs128sFontString);
		}

		#endregion

		#region TestTextAs128sFontString

		public void TestTextAs128sFontString()
		{
			TextBarcode barcode = new TextBarcode("ABCDEFG");
			AssertEquals("string for FONT - Check start digit, check digit and stop bit", "ÈABCDEFG'Ê", barcode.TextAs128sFontString);

			barcode = new TextBarcode("[ROHCIVSYDMELH1234567890]");
			AssertEquals("string for FONT - Check start digit, check digit and stop bit", "È[ROHCIVSYDMELH1234567890]nÊ", barcode.TextAs128sFontString);
		}

		#endregion

		#region TestTextAs128sFontStringWithEncoding

		public void TestTextAs128sFontStringWithEncoding()
		{
			TextBarcode barcode = new TextBarcode("ABCDEFG", true);
			AssertEquals("string for FONT - Check start digit, check digit and stop bit, same as ordinary without encoding", "ÈABCDEFG'Ê", barcode.TextAs128sFontString);

			barcode = new TextBarcode("[ROHCIVSYDMELH1234567890]", true);
			AssertEquals("string for FONT - Check start digit, check digit and stop bit", "È[ROHCIVSYDMELHÃ,BXnzÄ]lÊ", barcode.TextAs128sFontString);
		}

		#endregion

		#region TestTextAs128sFont_WithOptimisationAndGS1128Encoding

		public void TestTextAs128sFont_WithOptimisationAndGS1128Encoding()
		{
			var barcode1 = new TextBarcode("00393123450000000013", true, true);
			AssertEquals("ÉÆ¯G?7M¯¯¯¯-QÊ", barcode1.TextAs128sFontString);

			var barcode2 = new TextBarcode("0209312345000005", true, true);
			AssertEquals("ÉÆ\")?7M¯¯%gÊ", barcode2.TextAs128sFontString);
		}

		#endregion

		#region TestChecksumThatIsASpaceRendersAsABarcodeCharacter

		public void TestChecksumThatIsASpaceRendersAsABarcodeCharacter()
		{
			TextBarcode barcode = new TextBarcode("^DOC=PKD|", true);
			AssertEquals("String for font - space replaced with special char", "È^DOC=PKD|¯Ê", barcode.TextAs128sFontString);
		}

		#endregion

		#region TestEncodeTo128ValuesWithoutOptimumEncoding

		public void TestEncodeTo128ValuesWithoutOptimumEncoding()
		{
			TextBarcode barcode = new TextBarcode("12345678901234abcdefg", false);
			int[] codesExpected = new int[] { 104, 17, 18, 19, 20, 21, 22, 23, 24, 25, 16, 17, 18, 19, 20, 65, 66, 67, 68, 69, 70, 71, 83, 106 };
			string generatedString = barcode.TextAs128sFontString;

			AssertEquals("Compression off should return a fully CodeB formatted string", "È12345678901234abcdefgsÊ", generatedString);
		}

		#endregion

		#region TestCalculateChecksum

		public void TestCalculateChecksum()
		{
			TextBarcode barcode = new TextBarcode("12345678901234abcdefg", false);
			string generatedString = barcode.TextAs128sFontString;
			AssertEquals("uncompressed - Checksum character should be s", 's', generatedString[generatedString.Length - 2]);

			barcode = new TextBarcode("12345678901234abcdefg", true);
			generatedString = barcode.TextAs128sFontString;
			AssertEquals("using compression, checksum character should be B", 'B', generatedString[generatedString.Length - 2]);
		}

		#endregion

		#region TestIsValidInternal

		public void TestIsValidInternal()
		{
			string emptyFilename = Env.GetTempFileName();
			Assert(new FileInfo(emptyFilename).Length == 0);

			Assert(File.Exists(emptyFilename));

			File.Delete(emptyFilename);
		}

		#endregion

		#region TestGS1EndCharacter

		public void TestGS1EndCharacter()
		{
			var part1 = "1234567890";
			var part2 = "09876543210";
			var part3 = "112233445566";

			var barcode = new TextBarcode(new ZString[] { part1, part2, part3 }, true, true);
			AssertEquals("ÉÆ,BXnzÆ)waK5Ä0ÆÃ+6ALWbcÊ", barcode.TextAs128sFontString);
		}

		#endregion

		#region TestEmptyBarcodeReturnsEmptyFontString

		public void TestEmptyBarcodeReturnsEmptyFontString()
		{
			AssertEquals("128s Font String should be empty on an empty barcode", ZString.Empty, new TextBarcode(ZString.Empty).TextAs128sFontString);
		}

		#endregion
	}
}
