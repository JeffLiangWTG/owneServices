using System;
using CargoWise.Types;
using Type1 = Enterprise.DataTransfer.Business.Testing.FixedWidthFlatFileFormatTest.FixedWidthFlatFileFormatTestClass.Constants.LineType1;
using Type2 = Enterprise.DataTransfer.Business.Testing.FixedWidthFlatFileFormatTest.FixedWidthFlatFileFormatTestClass.Constants.LineType2;
using Type3 = Enterprise.DataTransfer.Business.Testing.FixedWidthFlatFileFormatTest.FixedWidthFlatFileFormatTestClass.Constants.LineType3;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class FixedWidthFlatFileFormatTest : FlatFileFormatTestCase
	{
		#region Test Convert Lines

		public void TestConvertType1()
		{
			const string Type1Line = "AAAABBBBBBCCCCCDDDDDDDEEEEFFFFFFFGGGGGG";
			ZString[] type1Seperated = {
												new ZString("AAAA"),
												new ZString("BBBBBB"),
												new ZString("CCCCC"),
												new ZString("DDDDDDD"),
												new ZString("EEEE"),
												new ZString("FFFFFFF"),
												new ZString("GGGGGG")
											};
			FlatFileDataRow result = FileFormat.ConvertToRow(Type1Line);

			for (int i = 0; i < result.FieldCount; i++)
			{
				AssertEquals(type1Seperated[i], result.GetField(i));
			}
		}

		public void TestConvertType2()
		{
			const string Type2Line = "OOOOOOOOOOOOIIIIIKKK";
			ZString[] type2Seperated = {
												new ZString("OOOOOOOOOOOO"),
												new ZString("IIIII"),
												new ZString("KKK")
											};
			FlatFileDataRow result = FileFormat.ConvertToRow(Type2Line);

			for (int i = 0; i < result.FieldCount; i++)
			{
				AssertEquals(type2Seperated[i], result.GetField(i));
			}
		}

		public void TestConvertType3()
		{
			const string Type3Line = "RRRTTTUUU";
			ZString[] type3Seperated = {
												new ZString("RRR"),
												new ZString("TTT"),
												new ZString("UUU")
											};
			FlatFileDataRow result = FileFormat.ConvertToRow(Type3Line);

			for (int i = 0; i < result.FieldCount; i++)
			{
				AssertEquals(type3Seperated[i], result.GetField(i));
			}
		}

		#endregion

		#region Test Generate Whitespace
		public void TestGenerateWhitespace()
		{
			char[] charToPass;
			string[] returnChars;
			int[] numberOfChars;

			charToPass = new char[] { ' ', '1', '@' };
			numberOfChars = new int[] { 1, 2, 3 };
			returnChars = new string[] { " ", "11", "@@@" };

			for (int i = 0; i < charToPass.Length; i++)
			{
				AssertEquals("Charcters do not match", returnChars[i], FileFormat.GenerateWhitespace(numberOfChars[i], charToPass[i]));
				AssertEquals("Number of characters do not match", numberOfChars[i], FileFormat.GenerateWhitespace(numberOfChars[i], charToPass[i]).Length);
			}
		}
		#endregion

		#region TestFillFixedLengthField
		public void TestFillFixedLengthField()
		{
			ZStringBuilder lineBuilder = new ZStringBuilder();
			ZString value1 = "ThisIsATest";
			ZString value2 = "Snow";

			FileFormat.FillFixedLengthField(value1, 11, lineBuilder, true, ' ');
			AssertEquals("Strings do not match.", value1, lineBuilder.ToString());

			FileFormat.FillFixedLengthField(value2, 6, lineBuilder, true, '@');
			AssertEquals("Strings do not match.", "ThisIsATest@@Snow", lineBuilder.ToString());

			FileFormat.FillFixedLengthField(value2, 8, lineBuilder, false, '#');
			AssertEquals("Strings do not match.", "ThisIsATest@@SnowSnow####", lineBuilder.ToString());
		}
		#endregion

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			FileFormat = new FixedWidthFlatFileFormatTestClass();
		}

		protected override FlatFileFormat GetFlatFileFormat()
		{
			return new FixedWidthFlatFileFormatTestClass();
		}

		FixedWidthFlatFileFormatTestClass FileFormat;

		public class FixedWidthFlatFileFormatTestClass : FixedWidthFlatFileFormat
		{
			#region Overrides

			public override FileExtensionType FileExtensionForExport
			{
				get { return FileExtensionType.Txt; }
			}

			public override FileExtensionType FileExtensionForImport
			{
				get { return FileExtensionType.Txt; }
			}

			public override ZString ConvertToLine(FlatFileDataRow row)
			{
				throw new NotImplementedException();
			}

			public override FlatFileDataRow ConvertToRow(ZString rawRow)
			{
				FlatFileDataRow row;

				switch (rawRow.Length)
				{
					case Constants.LineType1.NumberOfCharacters:
						row = ConvertLineType1(rawRow);
						break;

					case Constants.LineType2.NumberOfCharacters:
						row = ConvertLineType2(rawRow);
						break;

					case Constants.LineType3.NumberOfCharacters:
						row = ConvertLineType3(rawRow);
						break;

					default:
						row = new FlatFileDataRow(0);
						break;
				}

				return row;
			}

			public new void FillFixedLengthField(ZString value, int lengthOfFileField, ZStringBuilder lineBuilder, bool padToLeft, char whiteSpaceChar)
			{
				base.FillFixedLengthField(value, lengthOfFileField, lineBuilder, padToLeft, whiteSpaceChar);
			}

			public new string GenerateWhitespace(int numberOfWhitespaceChars, char whiteSpaceChar)
			{
				return base.GenerateWhitespace(numberOfWhitespaceChars, whiteSpaceChar);
			}
			#endregion

			#region Implementation

			public FlatFileDataRow ConvertLineType1(string rawRow)
			{
				FlatFileDataRow result = new FlatFileDataRow(Constants.LineType1.RowLength);

				result.SetField(0, GetValue(Type1.Position.A, Type1.Length.A, rawRow));
				result.SetField(1, GetValue(Type1.Position.B, Type1.Length.B, rawRow));
				result.SetField(2, GetValue(Type1.Position.C, Type1.Length.C, rawRow));
				result.SetField(3, GetValue(Type1.Position.D, Type1.Length.D, rawRow));
				result.SetField(4, GetValue(Type1.Position.E, Type1.Length.E, rawRow));
				result.SetField(5, GetValue(Type1.Position.F, Type1.Length.F, rawRow));
				result.SetField(6, GetValue(Type1.Position.G, Type1.Length.G, rawRow));

				return result;
			}

			public FlatFileDataRow ConvertLineType2(string rawRow)
			{//OOOOOOOOOOOOIIIIIKKK
				FlatFileDataRow result = new FlatFileDataRow(Constants.LineType2.RowLength);

				result.SetField(0, GetValue(Type2.Position.O, Type2.Length.O, rawRow));
				result.SetField(1, GetValue(Type2.Position.I, Type2.Length.I, rawRow));
				result.SetField(2, GetValue(Type2.Position.K, Type2.Length.K, rawRow));

				return result;
			}

			public FlatFileDataRow ConvertLineType3(string rawRow)
			{
				FlatFileDataRow result = new FlatFileDataRow(Constants.LineType3.RowLength);

				result.SetField(0, GetValue(Type3.Position.R, Type3.Length.R, rawRow));
				result.SetField(1, GetValue(Type3.Position.T, Type3.Length.T, rawRow));
				result.SetField(2, GetValue(Type3.Position.U, Type3.Length.U, rawRow));

				return result;
			}

			#endregion

			#region Constants

			public static class Constants
			{
				public static class LineType1
				{
					public const int RowLength = 7;
					public const int NumberOfCharacters = 39;

					public static class Position
					{
						public const int A = 0;
						public const int B = 4;
						public const int C = 10;
						public const int D = 15;
						public const int E = 22;
						public const int F = 26;
						public const int G = 33;
					}

					public static class Length
					{
						public const int A = 4;
						public const int B = 6;
						public const int C = 5;
						public const int D = 7;
						public const int E = 4;
						public const int F = 7;
						public const int G = 6;
					}
				}

				public static class LineType2
				{   //OOOOOOOOOOOOIIIIIKKK
					public const int RowLength = 3;
					public const int NumberOfCharacters = 20;

					public static class Position
					{
						public const int O = 0;
						public const int I = 12;
						public const int K = 17;
					}

					public static class Length
					{
						public const int O = 12;
						public const int I = 5;
						public const int K = 3;
					}
				}

				public static class LineType3
				{
					public const int RowLength = 3;
					public const int NumberOfCharacters = 9;

					public static class Position
					{
						public const int R = 0;
						public const int T = 3;
						public const int U = 6;
					}

					public static class Length
					{
						public const int R = 3;
						public const int T = 3;
						public const int U = 3;
					}
				}
			}

			#endregion
		}

		#endregion
	}
}
