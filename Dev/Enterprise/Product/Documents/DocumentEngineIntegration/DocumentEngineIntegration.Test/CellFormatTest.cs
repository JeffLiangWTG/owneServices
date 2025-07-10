using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;

namespace Enterprise.DocumentEngineIntegration.Testing
{
	sealed class CellFormatTest : TestCase
	{
		public void TestConstructorDefaults()
		{
			var cellFormat = new CellFormat();

			AssertNotNull(cellFormat.Borders);
			AssertEquals(FillPatternStyle.None, cellFormat.FillPattern);
			AssertEquals(CellFormat.DefaultFontName, cellFormat.FontName);
			AssertEquals(CellFormat.DefaultFontSize, cellFormat.FontSize);
			AssertEquals(CellFormat.DefaultFontStyle, cellFormat.FontStyle);
			AssertEquals(string.Empty, cellFormat.FormatPattern);
			AssertEquals(Color.Black, cellFormat.TextColor);
		}

		public void TestGetFontNoRegular()
		{
			if (FontFamily.Families.Any(f => f.Name.Equals("Monotype Corsiva")))
			{
				var cellFormat = new CellFormat { FontName = "Monotype Corsiva", FontSize = 10, FontStyle = FontStyle.Italic };
				AssertEquals("Monotype Corsiva", cellFormat.GetFont().Name);

				cellFormat.FontStyle = FontStyle.Regular;
				AssertEquals("Monotype Corsiva", cellFormat.GetFont().Name);
			}
			else
			{
				Assert("There is not test font installed on this mashine, do nothing", true);
			}
		}

		[ExpectNoExceptions]
		public void TestGetFontNotInstallFallBack()
		{
			var cellFormat = new CellFormat
			{
				FontName = "NotExistFont",
				FontSize = 10,
			};

			var font = cellFormat.GetFont();
			AssertNotNull(font);
			AssertNotEquals("NotExistFont", font.Name);
		}

		public void TestGetFontThatNotInstallAndSizeEqualZero()
		{
			var cellFormat = new CellFormat { FontName = "NotExistFont", FontSize = 0, FontStyle = FontStyle.Regular };
			var ex = AssertExceptionThrown<Exception>("GetFont() will throw exception", () => cellFormat.GetFont());
			var expectedMessage = @"FontName:NotExistFont, FontSize:0, FontStyle:Regular, Font Installed:False,
Pervious Exception:Value of '0' is not valid for 'emSize'. 'emSize' should be greater than 0 and less than or equal to System.Single.MaxValue. (Parameter 'emSize')
Now Exception:Font 'NotExistFont' cannot be found.";
#if NETFRAMEWORK
			expectedMessage = @"FontName:NotExistFont, FontSize:0, FontStyle:Regular, Font Installed:False,
Pervious Exception:Value of '0' is not valid for 'emSize'. 'emSize' should be greater than 0 and less than or equal to System.Single.MaxValue.
Parameter name: emSize
Now Exception:Font 'NotExistFont' cannot be found.";
#endif
			AssertMultilineASCIIEquals(expectedMessage,ex.Message);
		}
	}
}
