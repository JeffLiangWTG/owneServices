using System;

namespace Enterprise.Barcode.Business.Symbologies.Code128
{
	/// <summary>
	/// Following Code128 specification at : http://www.adams1.com/pub/russadam/128code.html
	/// </summary>
	internal sealed class Code128Values
	{
		public const int ModulesPerCharacter = 11;
		public const int BarsPerCharacter = 6;

		#region Bar Width Data

		public const int StopIndex = 106;
		public const int CodeAStartIndex = 103;
		public const int CodeBStartIndex = 104;
		public const int CodeCStartIndex = 105;
		public const int CodeAShiftIndex = 101;
		public const int CodeBShiftIndex = 100;
		public const int CodeCShiftIndex = 99;
		public const int ShiftOneIndex = 98;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public const string ShiftOneValue = "Â";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public const string CodeCShiftValue = "Ã";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public const string CodeBShiftValue = "Ä";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public const string CodeAShiftValue = "Å";
		public const int UpsideDownStartIndex = 107;

		public const int DataCharacters = 108;

		[ThreadStatic]
		static int[][] fData;

		public int[][] Data
		{
			get
			{
				if (fData == null)
				{
					fData = new int[108][];

					// Black Stipe, White Stripe, BS, WS, BS, WS
					Data[0] = new int[] { 2, 1, 2, 2, 2, 2 };  // 0
					Data[1] = new int[] { 2, 2, 2, 1, 2, 2 };
					Data[2] = new int[] { 2, 2, 2, 2, 2, 1 };
					Data[3] = new int[] { 1, 2, 1, 2, 2, 3 };
					Data[4] = new int[] { 1, 2, 1, 3, 2, 2 };
					Data[5] = new int[] { 1, 3, 1, 2, 2, 2 };
					Data[6] = new int[] { 1, 2, 2, 2, 1, 3 };
					Data[7] = new int[] { 1, 2, 2, 3, 1, 2 };
					Data[8] = new int[] { 1, 3, 2, 2, 1, 2 };
					Data[9] = new int[] { 2, 2, 1, 2, 1, 3 };
					Data[10] = new int[] { 2, 2, 1, 3, 1, 2 };   // 10
					Data[11] = new int[] { 2, 3, 1, 2, 1, 2 };
					Data[12] = new int[] { 1, 1, 2, 2, 3, 2 };
					Data[13] = new int[] { 1, 2, 2, 1, 3, 2 };
					Data[14] = new int[] { 1, 2, 2, 2, 3, 1 };
					Data[15] = new int[] { 1, 1, 3, 2, 2, 2 };
					Data[16] = new int[] { 1, 2, 3, 1, 2, 2 };
					Data[17] = new int[] { 1, 2, 3, 2, 2, 1 };
					Data[18] = new int[] { 2, 2, 3, 2, 1, 1 };
					Data[19] = new int[] { 2, 2, 1, 1, 3, 2 };
					Data[20] = new int[] { 2, 2, 1, 2, 3, 1 };   // 20
					Data[21] = new int[] { 2, 1, 3, 2, 1, 2 };
					Data[22] = new int[] { 2, 2, 3, 1, 1, 2 };
					Data[23] = new int[] { 3, 1, 2, 1, 3, 1 };
					Data[24] = new int[] { 3, 1, 1, 2, 2, 2 };
					Data[25] = new int[] { 3, 2, 1, 1, 2, 2 };
					Data[26] = new int[] { 3, 2, 1, 2, 2, 1 };
					Data[27] = new int[] { 3, 1, 2, 2, 1, 2 };
					Data[28] = new int[] { 3, 2, 2, 1, 1, 2 };
					Data[29] = new int[] { 3, 2, 2, 2, 1, 1 };
					Data[30] = new int[] { 2, 1, 2, 1, 2, 3 };   // 30
					Data[31] = new int[] { 2, 1, 2, 3, 2, 1 };
					Data[32] = new int[] { 2, 3, 2, 1, 2, 1 };
					Data[33] = new int[] { 1, 1, 1, 3, 2, 3 };
					Data[34] = new int[] { 1, 3, 1, 1, 2, 3 };
					Data[35] = new int[] { 1, 3, 1, 3, 2, 1 };
					Data[36] = new int[] { 1, 1, 2, 3, 1, 3 };
					Data[37] = new int[] { 1, 3, 2, 1, 1, 3 };
					Data[38] = new int[] { 1, 3, 2, 3, 1, 1 };
					Data[39] = new int[] { 2, 1, 1, 3, 1, 3 };
					Data[40] = new int[] { 2, 3, 1, 1, 1, 3 };   // 40
					Data[41] = new int[] { 2, 3, 1, 3, 1, 1 };
					Data[42] = new int[] { 1, 1, 2, 1, 3, 3 };
					Data[43] = new int[] { 1, 1, 2, 3, 3, 1 };
					Data[44] = new int[] { 1, 3, 2, 1, 3, 1 };
					Data[45] = new int[] { 1, 1, 3, 1, 2, 3 };
					Data[46] = new int[] { 1, 1, 3, 3, 2, 1 };
					Data[47] = new int[] { 1, 3, 3, 1, 2, 1 };
					Data[48] = new int[] { 3, 1, 3, 1, 2, 1 };
					Data[49] = new int[] { 2, 1, 1, 3, 3, 1 };
					Data[50] = new int[] { 2, 3, 1, 1, 3, 1 };  // 50
					Data[51] = new int[] { 2, 1, 3, 1, 1, 3 };
					Data[52] = new int[] { 2, 1, 3, 3, 1, 1 };
					Data[53] = new int[] { 2, 1, 3, 1, 3, 1 };
					Data[54] = new int[] { 3, 1, 1, 1, 2, 3 };
					Data[55] = new int[] { 3, 1, 1, 3, 2, 1 };
					Data[56] = new int[] { 3, 3, 1, 1, 2, 1 };
					Data[57] = new int[] { 3, 1, 2, 1, 1, 3 };
					Data[58] = new int[] { 3, 1, 2, 3, 1, 1 };
					Data[59] = new int[] { 3, 3, 2, 1, 1, 1 };
					Data[60] = new int[] { 3, 1, 4, 1, 1, 1 };  // 60
					Data[61] = new int[] { 2, 2, 1, 4, 1, 1 };
					Data[62] = new int[] { 4, 3, 1, 1, 1, 1 };
					Data[63] = new int[] { 1, 1, 1, 2, 2, 4 };
					Data[64] = new int[] { 1, 1, 1, 4, 2, 2 };
					Data[65] = new int[] { 1, 2, 1, 1, 2, 4 };
					Data[66] = new int[] { 1, 2, 1, 4, 2, 1 };
					Data[67] = new int[] { 1, 4, 1, 1, 2, 2 };
					Data[68] = new int[] { 1, 4, 1, 2, 2, 1 };
					Data[69] = new int[] { 1, 1, 2, 2, 1, 4 };
					Data[70] = new int[] { 1, 1, 2, 4, 1, 2 };  // 70
					Data[71] = new int[] { 1, 2, 2, 1, 1, 4 };
					Data[72] = new int[] { 1, 2, 2, 4, 1, 1 };
					Data[73] = new int[] { 1, 4, 2, 1, 1, 2 };
					Data[74] = new int[] { 1, 4, 2, 2, 1, 1 };
					Data[75] = new int[] { 2, 4, 1, 2, 1, 1 };
					Data[76] = new int[] { 2, 2, 1, 1, 1, 4 };
					Data[77] = new int[] { 4, 1, 3, 1, 1, 1 };
					Data[78] = new int[] { 2, 4, 1, 1, 1, 2 };
					Data[79] = new int[] { 1, 3, 4, 1, 1, 1 };
					Data[80] = new int[] { 1, 1, 1, 2, 4, 2 };  // 80
					Data[81] = new int[] { 1, 2, 1, 1, 4, 2 };
					Data[82] = new int[] { 1, 2, 1, 2, 4, 1 };
					Data[83] = new int[] { 1, 1, 4, 2, 1, 2 };
					Data[84] = new int[] { 1, 2, 4, 1, 1, 2 };
					Data[85] = new int[] { 1, 2, 4, 2, 1, 1 };
					Data[86] = new int[] { 4, 1, 1, 2, 1, 2 };
					Data[87] = new int[] { 4, 2, 1, 1, 1, 2 };
					Data[88] = new int[] { 4, 2, 1, 2, 1, 1 };
					Data[89] = new int[] { 2, 1, 2, 1, 4, 1 };
					Data[90] = new int[] { 2, 1, 4, 1, 2, 1 };  // 90
					Data[91] = new int[] { 4, 1, 2, 1, 2, 1 };
					Data[92] = new int[] { 1, 1, 1, 1, 4, 3 };
					Data[93] = new int[] { 1, 1, 1, 3, 4, 1 };
					Data[94] = new int[] { 1, 3, 1, 1, 4, 1 };
					Data[95] = new int[] { 1, 1, 4, 1, 1, 3 };
					Data[96] = new int[] { 1, 1, 4, 3, 1, 1 };
					Data[97] = new int[] { 4, 1, 1, 1, 1, 3 };
					Data[98] = new int[] { 4, 1, 1, 3, 1, 1 };   // SHIFT (just the next character, A to B or B to A)
					Data[99] = new int[] { 1, 1, 3, 1, 4, 1 };   // CodeC Shift
					Data[100] = new int[] { 1, 1, 4, 1, 3, 1 };  // CodeB Shift
					Data[101] = new int[] { 3, 1, 1, 1, 4, 1 };  // CodeA Shift
					Data[102] = new int[] { 4, 1, 1, 1, 3, 1 };

					Data[103] = new int[] { 2, 1, 1, 4, 1, 2 };  // Code A start
					Data[104] = new int[] { 2, 1, 1, 2, 1, 4 };  // Code B start
					Data[105] = new int[] { 2, 1, 1, 2, 3, 2 };  // Code C start
					Data[106] = new int[] { 2, 3, 3, 1, 1, 1, 2 };  // STOP - 106
					Data[107] = new int[] { 2, 1, 1, 1, 3, 3 };  // Backwards Stop Character (ie, Upside-down Barcode)
				}
				return fData;
			}
		}
		#endregion

		#region CodeA, CodeB and CodeC Character Codes

		public readonly static string[] CodeACharacters = { " ", "!", "\"", "#", "$", "%", "&", "'", "(", ")", "*", "+", ",", "-", ".", "/", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", ":", ";", "<", "=", ">", "?", "@", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "[", "\\", "]", "^", "_", "[NUL]", "[SOH]", "[STX]", "[ETX]", "[EOT]", "[ENQ]", "[ACK]", "[BEL]", "[BS]", "[HT]", "[LF]", "[VT]", "[FF]", "[CR]", "[SO]", "[SI]", "[DLE]", "[DC1]", "[DC2]", "[DC3]", "[DC4]", "[NAK]", "[SYN]", "[ETB]", "[CAN]", "[EM]", "[SUB]", "[ESC]", "[FS]", "[GS]", "[RS]", "[US]", "[FNC3]", "[FNC2]", ShiftOneValue, CodeCShiftValue, CodeBShiftValue, "[FNC4]", "[FNC1]" }; // Hard-coded constant
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "String is empty or contains only symbols.")]
		public readonly static string[] CodeBCharacters = { " ", "!", "\"", "#", "$", "%", "&", "'", "(", ")", "*", "+", ",", "-", ".", "/", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", ":", ";", "<", "=", ">", "?", "@", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "[", "\\", "]", "^", "_", "'", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "{", "|", "}", "~", "[DEL]", "[FNC3]", "[FNC2]", ShiftOneValue, CodeCShiftValue, "[FNC4]", CodeAShiftValue, "[FNC1]" };
		public readonly static string[] CodeCCharacters = { "00", "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "60", "61", "62", "63", "64", "65", "66", "67", "68", "69", "70", "71", "72", "73", "74", "75", "76", "77", "78", "79", "80", "81", "82", "83", "84", "85", "86", "87", "88", "89", "90", "91", "92", "93", "94", "95", "96", "97", "98", "99", CodeBShiftValue, CodeAShiftValue, "[FNC1]" }; // Hard-coded character codes

		#endregion

		#region Number Code to Character Translation Functions

		static public string CodeToChar(int code128, ref Code128CharacterSet characterSet)
		{
			string result;

			switch (characterSet)
			{
				case Code128CharacterSet.CodeA:
					return Code128AToChar(code128, ref characterSet);
				case Code128CharacterSet.CodeB:
					return Code128BToChar(code128, ref characterSet);
				case Code128CharacterSet.CodeC:
					return Code128CToChar(code128, ref characterSet);
				case Code128CharacterSet.CodeAOneCharacterShift:
					result = Code128BToChar(code128, ref characterSet);
					characterSet = Code128CharacterSet.CodeA;
					return result;
				case Code128CharacterSet.CodeBOneCharacterShift:
					result = Code128AToChar(code128, ref characterSet);
					characterSet = Code128CharacterSet.CodeB;
					return result;
				default:
					return String.Empty;
			}
		}

		static public int CharToCode(string characterCode, Code128CharacterSet characterSet)
		{
			switch (characterSet)
			{
				case Code128CharacterSet.CodeA:
					return CharToCode128A(characterCode);
				case Code128CharacterSet.CodeB:
					return CharToCode128B(characterCode);
				case Code128CharacterSet.CodeC:
					return CharToCode128C(characterCode);
				default:
					return -1;
			}
		}

		#region Implementation

		#region Code 128A functions
		static string Code128AToChar(int code128, ref Code128CharacterSet characterSet)
		{
			switch (code128)
			{
				case 98:
					characterSet = Code128CharacterSet.CodeAOneCharacterShift;
					return "";
				case 99:
					characterSet = Code128CharacterSet.CodeC;
					return "";
				case 100:
					characterSet = Code128CharacterSet.CodeB;
					return "";
				default:
					if (Code128Values.CodeACharacters.Length >= code128)
					{
						return Code128Values.CodeACharacters[code128];
					}
					else
					{
						return "";
					}
			}
		}

		static int CharToCode128A(string characterCode)
		{
			for (int index = 0; index < Code128.Code128Values.CodeACharacters.Length; index++)
			{
				if (characterCode == Code128.Code128Values.CodeACharacters[index])
				{
					return index;
				}
			}
			return -1;
		}
		#endregion

		#region Code 128B functions
		static string Code128BToChar(int code128, ref Code128CharacterSet characterSet)
		{
			switch (code128)
			{
				case 98:
					characterSet = Code128CharacterSet.CodeBOneCharacterShift;
					return "";
				case 99:
					characterSet = Code128CharacterSet.CodeC;
					return "";
				case 101:
					characterSet = Code128CharacterSet.CodeA;
					return "";
				default:
					if (Code128Values.CodeBCharacters.Length >= code128)
					{
						return Code128Values.CodeBCharacters[code128];
					}
					else
					{
						return "";
					}
			}
		}

		static int CharToCode128B(string characterCode)
		{
			for (int index = 0; index < Code128.Code128Values.CodeBCharacters.Length; index++)
			{
				if (characterCode == Code128.Code128Values.CodeBCharacters[index])
				{
					return index;
				}
			}
			return -1;
		}
		#endregion

		#region Code 128C functions
		static string Code128CToChar(int code128, ref Code128CharacterSet characterSet)
		{
			switch (code128)
			{
				case 100:
					characterSet = Code128CharacterSet.CodeB;
					return "";
				case 101:
					characterSet = Code128CharacterSet.CodeA;
					return "";
				case 102:
					return "[FNC1]"; // Hard-coded character codes
				default:
					if (Code128Values.CodeCCharacters.Length >= code128)
					{
						return Code128Values.CodeCCharacters[code128];
					}
					else
					{
						return "";
					}
			}
		}

		static int CharToCode128C(string characters)
		{
			for (int index = 0; index < Code128.Code128Values.CodeCCharacters.Length; index++)
			{
				if (characters == Code128.Code128Values.CodeCCharacters[index])
				{
					return index;
				}
			}
			return -1;
		}
		#endregion

		#endregion

		#endregion

		#region Data Validation Functions

		/// <summary>
		/// given the number of stripes in a potential barcode image region, return whether
		/// this is a valid number of stripes for a barcode or not.
		/// </summary>
		/// <param name="numberOfStripes"></param>
		/// <returns></returns>
		public static bool IsValidNumberOfStripes(int numberOfStripes)
		{
			// Start + Data + Checksum + Stop + 1
			return (numberOfStripes >= 4 * Code128Values.BarsPerCharacter + 1);
		}

		/// <remarks>
		/// All numbers in this function are magic, having been pulled out
		/// from the air. Confidence is value between 0 and 1.
		/// </remarks>
		public static double GetConfidence(double error)
		{
			if (error < 0.1)
			{
				return 1;
			}

			double confidence = (1 / (error * 3));
			if (confidence > 1)
			{
				confidence = 1;
			}

			return confidence;
		}

		public static int CharacterSetToIndex(Code128CharacterSet charSet)
		{
			switch (charSet)
			{
				case Code128CharacterSet.CodeA:
					return CodeAStartIndex;
				case Code128CharacterSet.CodeB:
					return CodeBStartIndex;
				case Code128CharacterSet.CodeC:
					return CodeCStartIndex;
				case Code128CharacterSet.CodeAShift:
					return CodeAShiftIndex;
				case Code128CharacterSet.CodeBShift:
					return CodeBShiftIndex;
				case Code128CharacterSet.CodeCShift:
					return CodeCShiftIndex;
				case Code128CharacterSet.CodeAOneCharacterShift:
				case Code128CharacterSet.CodeBOneCharacterShift:
					return ShiftOneIndex;
				case Code128CharacterSet.UpsideDown:
					return UpsideDownStartIndex;
				default:
					return -1;
			}
		}

		public static Code128CharacterSet IndexToCharacterSet(int index)
		{
			switch (index)
			{
				case CodeAStartIndex:
					return Code128CharacterSet.CodeA;
				case CodeBStartIndex:
					return Code128CharacterSet.CodeB;
				case CodeCStartIndex:
					return Code128CharacterSet.CodeC;
				case CodeAShiftIndex:
					return Code128CharacterSet.CodeAShift;
				case CodeBShiftIndex:
					return Code128CharacterSet.CodeBShift;
				case CodeCShiftIndex:
					return Code128CharacterSet.CodeCShift;
				case UpsideDownStartIndex:
					return Code128CharacterSet.UpsideDown;
				default:
					return Code128CharacterSet.InvalidCharacterSet;
			}
		}

		#endregion

	}

	public enum Code128CharacterSet
	{
		InvalidCharacterSet,

		CodeA,
		CodeB,
		CodeC,

		CodeAShift,
		CodeBShift,
		CodeCShift,

		UpsideDown,

		CodeAOneCharacterShift,
		CodeBOneCharacterShift,
	}
}
