using System.Drawing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class FormattedRtfStringTest : TestCase
	{
		public void TestFormattedRtfString_WithDefaultConstructor()
		{
			TestFormattedRtfString(new FormattedRtfString());
		}

		public void TestFormattedRtfString_With1ArgConstructor()
		{
			TestFormattedRtfString(new FormattedRtfString(ORtfTextUtil.DefaultFont));
		}

		void TestFormattedRtfString(FormattedRtfString rtf)
		{
			using (Font font1 = new Font(System.Drawing.FontFamily.GenericSansSerif, 10))
			using (Font font2 = new Font(System.Drawing.FontFamily.GenericSansSerif, 15))
			{
				rtf += new FormattedRtfPart("splaty", font1);
				rtf += new FormattedRtfPart("splatty knows everything", font2);
				rtf += "normal text";
				rtf = rtf.Add("styled text", FontStyle.Bold | FontStyle.Italic);

				string rtfString = rtf.ToRtf();

				var expectedRtf1 = @"{\rtf1\ansi\ansicpg1252\deflang3081\nouicompat\uc0{\fonttbl{\f0\fnil Microsoft Sans Serif;}}{\colortbl}{\*\generator RTFConverter RTFConverter 2.0.0.0}{{\f0\fs20 splaty}{\f0\fs30 splatty knows everything}"; // AU = lang3081
				var expectedRtf2 = @"{\f0\fs20 normal text}{\f0\fs20\b\i styled text}\par}}";
				var concatedExpectedRtf = $"{expectedRtf1}{expectedRtf2}";

				ORtfTextUtilTest.AssertRtfTextsEqualLanguageIndependent("Should have some different fonts etc", concatedExpectedRtf, rtfString);
			}
		}

		public void TestFormattedRtfString_NewlineFormattedAsParagraph()
		{
			var rtf = new FormattedRtfString();
			using (Font font1 = new Font(System.Drawing.FontFamily.GenericSansSerif, 10))
			{
				rtf += new FormattedRtfPart("Line One \r\nLine Two ", font1);
				rtf += new FormattedRtfPart("Still Line Two \r\nLine Three", font1);
				rtf += new FormattedRtfPart("\r\n\r\n", font1);
				rtf += new FormattedRtfPart("An Extra Line", font1);

				string rtfString = rtf.ToRtf();

				var expectedRtf = @"{\rtf1\ansi\ansicpg1252\deflang3081\nouicompat\uc0{\fonttbl{\f0\fnil Microsoft Sans Serif;}}{\colortbl}{\*\generator RTFConverter RTFConverter 2.0.0.0}{{\f0\fs20 Line One }\par}{{\f0\fs20 Line Two Still Line Two }\par}{{\f0\fs20 Line Three}\par}{\par}{{\f0\fs20 An Extra Line}\par}}";
				ORtfTextUtilTest.AssertRtfTextsEqualLanguageIndependent("Newlines shouldbe par tokens", expectedRtf, rtfString);
			}
		}
	}
}
