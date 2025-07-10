using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Barcode.Business.Symbologies.Code128;

namespace Enterprise.Barcode.Business
{
	/// <summary>
	/// Represents the text of a barcode. 
	/// Use this in conjunction with the AbriBar128s font.
	/// There are two steps - get the original text you want to encode -> Convert it to 128s 
	/// indexes for each letter you want to encode -> convert it to appropriate ASCII values
	/// for this specific font.
	/// </summary>
	public class TextBarcode
	{
		public TextBarcode()
		{
		}

		public TextBarcode(ZString textToEncode, bool useOptimisedEncoding = false, bool isGS1128Barcode = false)
			: this(new[] { textToEncode }, useOptimisedEncoding, isGS1128Barcode)
		{
		}

		public TextBarcode(ZString[] textsToConcatenateAndEncode, bool useOptimisedEncoding = false, bool isGS1128Barcode = false)
		{
			this.textPartsToEncode = textsToConcatenateAndEncode;
			this.IsGS1128Barcode = isGS1128Barcode;
			this.UseOptimisedEncoding = useOptimisedEncoding;
		}

		public ZString TextToEncode
		{
			get { return new ZStringBuilder(TextPartsToEncode).ToString(); }
		}

		protected IReadOnlyList<ZString> TextPartsToEncode
		{
			get { return textPartsToEncode; }
		}

		readonly ZString[] textPartsToEncode;
		readonly bool UseOptimisedEncoding;
		readonly bool IsGS1128Barcode;

		/// <summary>
		/// Returns the GS1 End Character to use for Variable Length Strings
		/// </summary>
		int GetGS1EndCharacterInt(Code128CharacterSet characterSet)
		{
			return Code128Values.CharToCode("[FNC1]", characterSet); // Barcode constant
		}

		/// <summary>
		/// Adapted from the foxpro function supplied by AbriBar.
		/// Returns data needed in the barcode, including start digit for 128B,
		/// encoded string, check digit and stop digit.
		/// </summary>
		public ZString TextAs128sFontString
		{
			get
			{
				ZString allText = "";
				if (!TextToEncode.IsEmpty)
				{
					foreach (int codeValue in TextAs128Values)
					{
						allText += ConvertToFont(codeValue);
					}
				}
				return allText;
			}
		}

		Code128CharacterSet StartCharacterSet
		{
			get { return TextPartsToEncode.Count > 0 && UseOptimisedEncoding && ShouldConvertToCharacterSetC(TextPartsToEncode[0]) ? Code128CharacterSet.CodeC : Code128CharacterSet.CodeB; }
		}

		#region Implementation

		/// <summary>
		/// Takes a string and converts it to an array of 128 values 
		/// as per the lookup table in Code128Values class
		/// </summary>
		/// <returns></returns>
		protected IReadOnlyList<int> TextAs128Values
		{
			get
			{
				var encodedCharacters = new ArrayList();
				currentCharacterSet = StartCharacterSet;
				encodedCharacters.Add(Code128Values.CharacterSetToIndex(currentCharacterSet));

				for (int t = 0; t < TextPartsToEncode.Count; t++)
				{
					var text = TextPartsToEncode[t];

					if (IsGS1128Barcode)
					{
						encodedCharacters.Add(GetGS1EndCharacterInt(currentCharacterSet));
					}

					encodedCharacters.AddRange(GetTextAs128Values(text));
				}

				encodedCharacters.Add(CalculateChecksum(encodedCharacters));
				encodedCharacters.Add(Code128Values.StopIndex);

				var codes = new int[encodedCharacters.Count];
				encodedCharacters.CopyTo(codes, 0);
				return codes;
			}
		}

		ArrayList GetTextAs128Values(ZString text)
		{
			var encodedCharacters = new ArrayList();

			for (int i = 0; i < text.Length; i++)
			{
				int code;
				if (currentCharacterSet == Code128CharacterSet.CodeC)
				{
					if ((UseOptimisedEncoding || MustConvertToCharacterSetB(text.Substring(i)))
						&& ShouldConvertToCharacterSetB(text.Substring(i)))
					{
						currentCharacterSet = Code128CharacterSet.CodeB;
						code = Code128Values.CharacterSetToIndex(Code128CharacterSet.CodeBShift);
						i--;
					}
					else // set C takes TWO characters
					{
						code = EncodeCharacter(text.Substring(i, 2), currentCharacterSet);
						i++;
					}
				}
				else
				{
					if (UseOptimisedEncoding && ShouldConvertToCharacterSetC(text.Substring(i)))
					{
						currentCharacterSet = Code128CharacterSet.CodeC;
						code = Code128Values.CharacterSetToIndex(Code128CharacterSet.CodeCShift);
						i--;
					}
					else
					{
						code = EncodeCharacter(text[i].ToString(), currentCharacterSet);
					}
				}

				encodedCharacters.Add(code);
			}

			return encodedCharacters;
		}

		Code128CharacterSet currentCharacterSet;

		int EncodeCharacter(string charactersToAdd, Code128CharacterSet characterSet)
		{
			return Code128Values.CharToCode(charactersToAdd, characterSet);
		}

		protected int CalculateChecksum(ArrayList encodedCharacters)
		{
			int checkSum = Code128Values.CharacterSetToIndex(StartCharacterSet);

			// ignore the first character, we already included the start bit
			for (int i = 1; i < encodedCharacters.Count; i++)
			{
				checkSum += (int)encodedCharacters[i] * i;
			}

			return checkSum % 103;
		}

		/// <summary>
		/// Whether or not to convert to Character set B when currently in Set C encoding
		/// - if you have only one character left to encode, or the next two characters aren't numbers
		/// </summary>
		bool ShouldConvertToCharacterSetB(string textRemaining)
		{
			return MustConvertToCharacterSetB(textRemaining) || !(Char.IsDigit(textRemaining[0]) && Char.IsDigit(textRemaining[1]));
		}

		/// <summary>
		/// Whether or not we must convert to Character Set B from set C.
		/// (i.e. if we have only 1 character left)
		/// </summary>
		bool MustConvertToCharacterSetB(string textRemaining)
		{
			return textRemaining.Length < 2;
		}

		/// <summary>
		/// Whether or not to convert to Character set C when currently in Set B encoding.
		/// It becomes more space-effective to switch to character set C if you have
		/// more than 5 numbers in a row.
		/// </summary>
		bool ShouldConvertToCharacterSetC(string textRemaining)
		{
			int digitCount = 0;

			for (int i = 0; i < textRemaining.Length; i++)
			{
				if (Char.IsDigit(textRemaining[i]))
				{
					digitCount++;
				}
				else
				{
					break;
				}
			}

			return ((digitCount > 5 && digitCount % 2 == 0) || (digitCount == 4 && textRemaining.Length == 4));
		}

		/// <summary>
		/// Converts a 128s Code index to the correct ascii character used in the AbriBar font.
		/// See foxpro functions supplied with the font. The font uses ascii chars 32-126 and 191-202.
		/// Space (ascii 32) is replaced with ascii 175.
		/// </summary>
		char ConvertToFont(int codeValue)
		{
			int offsetCode = codeValue + ((codeValue < 95) ? 32 : 96);
			return (char)((offsetCode == 32) ? 175 : offsetCode);
		}

		#endregion
	}
}
