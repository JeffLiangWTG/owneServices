using System;
using System.Text;
using System.Threading;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public class ORtfTextUtilTest : TestCase
	{
		public void TestGeneratorInfoRegex()
		{
			var partRtf1 = @"{\*\generator Msftedit 5.41.15.1507;}\viewkind4\uc1";
			var partRtf2 = @"{\*\generator Riched20 10.0.17763}\viewkind4\uc1";
			var partRtf3 = @"{\*\generator Riched20 10.0.17763}\viewkind4\uc1}\viewkind4\uc1";
			Assert(ORtfTextUtil.GeneratorInfoRegex_Exposed.IsMatch(partRtf1));
			Assert(ORtfTextUtil.GeneratorInfoRegex_Exposed.IsMatch(partRtf2));
			var matches = ORtfTextUtil.GeneratorInfoRegex_Exposed.Matches(partRtf3);
			AssertEquals(1, matches.Count);
			AssertEquals(@"{\*\generator Riched20 10.0.17763}\viewkind4\uc1", matches[0].Value);
		}

		public void TestEqualsExceptGeneratorInfo()
		{
			var rtf1 = "{\\rtf1\\ansi\\ansicpg1252\\deff0\\nouicompat\\deflang3081{\\fonttbl{\\f0\\fswiss\\fprq2\\fcharset0 Tahoma;}{\\f1\\fnil\\fcharset0 Microsoft Sans Serif;}}{\\*\\generator Riched20 6.3.9600}\\viewkind4\\uc1\\pard\\cf1\\f0\\fs-28\\lang2057 SHIPMENT : S00002843\\parETA : 06 / 03\\parIT WAS PG's AND IT'S NOT ON THE SPREADSHEET WE GOT:) PLEASE ASSIGN!19 / 02\\cf0\\f1\\fs20\\lang3081\\par}";

			var rtf2 = "{\\rtf1\\ansi\\ansicpg1252\\deff0\\nouicompat\\deflang3081{\\fonttbl{\\f0\\fswiss\\fprq2\\fcharset0 Tahoma;}{\\f1\\fnil\\fcharset0 Microsoft Sans Serif;}}{\\*\\generator Riched20 10.0.17763}\\viewkind4\\uc1\\pard\\cf1\\f0\\fs-28\\lang2057 SHIPMENT : S00002843\\parETA : 06 / 03\\parIT WAS PG's AND IT'S NOT ON THE SPREADSHEET WE GOT:) PLEASE ASSIGN!19 / 02\\cf0\\f1\\fs20\\lang3081\\par}";

			Assert(ORtfTextUtil.EqualsExceptGeneratorInfo(rtf1, rtf2));
		}

		#region AssertRtfTextsEqualLanguageIndependent

		public static void AssertRtfTextsEqualLanguageIndependent(string expectedRtf, string actualRtf)
		{
			AssertRtfTextsEqualLanguageIndependent(string.Empty, expectedRtf, actualRtf);
		}

		public static void AssertRtfTextsEqualLanguageIndependent(string message, string expectedRtf, string actualRtf)
		{
			AssertEquals(message, GetRtfStringIndependentOfMachineSettings(expectedRtf), GetRtfStringIndependentOfMachineSettings(actualRtf));
		}

		public static string GetRtfStringIndependentOfMachineSettings(string rtf)
		{
			var result = rtf;
			result = ReplaceWithCultureIndependentString(result, "lang");
			result = ReplaceWithCultureIndependentString(result, "charset");
			result = ReplaceWithCultureIndependentString(result, "ansicpg");
			result = ReplaceWithCultureIndependentString(result, "fs");
			result = ReplaceWithCultureIndependentString(result, "deflang");
			result = RemoveGenerator(result);
			return result;
		}
		static string RemoveGenerator(string rtf)
		{
			var startIndex = rtf.IndexOf("generator");
			if (startIndex < 0)
			{
				return rtf;
			}
			var endIndex = rtf.IndexOf("}", startIndex);
			if (endIndex < 0)
			{
				return rtf;
			}
			return rtf.Replace(rtf.Substring(startIndex, endIndex - startIndex), "generator Riched20 ZZZ");
		}

		static string ReplaceWithCultureIndependentString(string rtf, string stringToReplace)
		{
			var result = rtf;
			var startIndex = rtf.IndexOf(stringToReplace);
			while (startIndex != -1)
			{
				var endIndex = startIndex + stringToReplace.Length;
				while (char.IsDigit(result[endIndex]))
				{
					endIndex++;
				}
				if (endIndex > startIndex + stringToReplace.Length)
				{
					string fullStringToReplace = result.Substring(startIndex, (endIndex - startIndex));
					result = result.Replace(fullStringToReplace, stringToReplace + "ZZZ");
					startIndex = result.IndexOf(stringToReplace, startIndex + stringToReplace.Length + 3);
				}
				else
				{
					startIndex = result.IndexOf(stringToReplace, startIndex + stringToReplace.Length);
				}
			}
			return result;
		}

		#endregion

		public void TestEmptyRtf()
		{
			AssertNotNull(ORtfTextUtil.EmptyRtf);
			AssertNotNull(ORtfTextUtil.EmptyRtfByteArray);

			var rtf = ORtfTextUtil.EmptyRtf;
			AssertContains("Empty font table should exist using the empty rtf string", @"{\fonttbl}", rtf);
			AssertContains("Empty color table should exist using the empty rtf string", @"{\colortbl}", rtf);
		}

		public void TestIsRtf()
		{
			var rtf = ORtfTextUtil.TextToRtf(string.Empty);
			AssertEquals("IsRtf=true", true, ORtfTextUtil.IsRtf(rtf));
			AssertEquals("IsRtf=true", true, ORtfTextUtil.IsRtf(Encoding.UTF8.GetBytes(rtf)));
			rtf = ORtfTextUtil.TextToRtf("XXX");
			AssertEquals("IsRtf=true", true, ORtfTextUtil.IsRtf(rtf));
			AssertEquals("IsRtf=true", true, ORtfTextUtil.IsRtf(Encoding.UTF8.GetBytes(rtf)));
			rtf = "blah";
			AssertEquals("IsRtf=false", false, ORtfTextUtil.IsRtf(rtf));
			AssertEquals("IsRtf=false", false, ORtfTextUtil.IsRtf(Encoding.UTF8.GetBytes(rtf)));
		}

		public void TestRtfToText()
		{
			string text = ORtfTextUtil.RtfToText("{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang3081{\\fonttbl{\\f0\\fnil\\fcharset0 Microsoft Sans Serif;}}\r\n\\viewkind4\\uc1\\pard\\f0\\fs17 x\\par\r\n}\r\n\0");
			AssertEquals("RtfToText", "x", text);
		}

		public void TestRtfToTextUndefinedFont()
		{
			var rtf = "{\\rtf1\\ansi\\ansicpg1252\\deff0\\nouicompat\\deflang3081{\\fonttbl{\\f0\\fnil\\fcharset0 Microsoft Sans Serif;}}\\viewkind4\\uc1\\pard\\f0\\fs20 >>>>> Concurrency Error Current Value\\par \\par\\par\\cf1\\f0\\fs20 JMS 11 - Dec - 18 08:16:  AVA 09 DEC // STG 11 DEC\\par	 CW 10 - Dec - 18 13:55: Landed 09 / 12, not out-turned at AMI yet\\par		   CW 10 - Dec - 18 16:03: requested AMI for out-turn\\par\\par\\par\f0\fs20 >>>>> Concurrency Error Old Value\\par \\par\\par\\cf1\f0\fs20 CW 10 - Dec - 18 13:55: Landed 09 / 12, not out-turned at AMI yet\\par			   CW 10 - Dec - 18 16:03: requested AMI for out-turn, Per out-turn report, storage strarts 11 / 12, requested AMI to confirm no storage will apply(out -turn just recieved 11 / 12 8am).\\par\\pard\\sb100\\sa100 CW 11 - Dec - 18 08:16: CLEAR, BOOKED WITH XTREME / MEL TO DELIVER 11 / 12 - BOOKING # \\cf0\\f1\\fs24 196523\\cf1\\f0\\fs20\\par\\pard\\par}";
			var plainText = ORtfTextUtil.RtfToText(rtf);
			AssertNotEquals("Undefined Font", rtf, plainText);
		}

		public void TestRtfToTextInvalidFontSize()
		{
			var rtf = "{\\rtf1\\ansi\\ansicpg1252\\deff0\\nouicompat\\deflang3081{\\fonttbl{\\f0\\fswiss\\fprq2\\fcharset0 Tahoma;}{\\f1\\fnil\\fcharset0 Microsoft Sans Serif;}}{\\*\\generator Riched20 6.3.9600}\\viewkind4\\uc1\\pard\\cf1\\f0\\fs-28\\lang2057 SHIPMENT : S00002843\\parETA : 06 / 03\\parIT WAS PG's AND IT'S NOT ON THE SPREADSHEET WE GOT:) PLEASE ASSIGN!19 / 02\\cf0\\f1\\fs20\\lang3081\\par}";
			var plainText = ORtfTextUtil.RtfToText(rtf);
			AssertNotEquals("Invalid Font Size", rtf, plainText);
		}

		public void TestRtfToTextDuplicatedFont()
		{
			var rtf = "{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang3081{\\fonttbl{\\f0\\fbidi\\froman\\fcharset0\\fprq2{\\*\\panose 02020603050405020304}Times New Roman;}{\\f0\\fbidi\\froman\\fcharset0\\fprq2{\\*\\panose 02020603050405020304}Times New Roman;}\r\n\\viewkind4\\uc1\\pard\\f0\\fs17 x\\par\r\n}\r\n\0";
			var plainText = ORtfTextUtil.RtfToText(rtf);
			AssertNotEquals("Duplicated Font", rtf, plainText);
		}

		public void TestRtfToTextInvalidCodePage()
		{
			var rtf = "{\\rtf1\\ansi\\ansicpg1252\\deff0\\nouicompat\\deflang3081{\\fonttbl{\\f0\\fswiss\\fprq2\\fcharset0 Tahoma;}{\\f1\\fnil\\fcharset0 Microsoft Sans Serif;}{\\f2\\fswiss\\fprq2\\fcharset0\\cpg-8532 Vijaya;}}{\\*\\generator Riched20 6.3.9600}\\viewkind4\\uc1\\pard\\cf1\\f0\\fs-28\\lang2057 SHIPMENT : S00002843\\parETA : 06 / 03\\parIT WAS PG's AND IT'S NOT ON THE SPREADSHEET WE GOT:) PLEASE ASSIGN!19 / 02\\cf0\\f1\\fs20\\lang3081\\par}";
			var text = "SHIPMENT : S00002843: 06 / 03WAS PG's AND IT'S NOT ON THE SPREADSHEET WE GOT:) PLEASE ASSIGN!19 / 02";
			AssertEquals("Text should be extracted as we can now deal with invalid code pages.", text, ORtfTextUtil.RtfToText(rtf));
		}

		public void TestRtfToTextHiddenTextDefaultsToBeingShown()
		{
			string text = ORtfTextUtil.RtfToText("{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang3081{\\fonttbl{\\f0\\fnil\\fcharset0 Microsoft Sans Serif;}}\r\n\\viewkind4\\uc1\\pard\\f0\\fs17 x{\\v\\insrsid2294299\\charrsid2294299  hidden}\\par\r\n}  \r\n\0");
			AssertEquals("RtfToText doesn't show hidden text by default", "x hidden", text);
		}

		public void TestRtfToTextHiddenTextCanBeHidden()
		{
			string text = ORtfTextUtil.RtfToText("{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang3081{\\fonttbl{\\f0\\fnil\\fcharset0 Microsoft Sans Serif;}}\r\n\\viewkind4\\uc1\\pard\\f0\\fs17 x{\\v\\insrsid2294299\\charrsid2294299  hidden}\\par\r\n}  \r\n\0", false);
			AssertEquals("RtfToText can't hide hidden text when specified to do so", "x", text);
		}

		public void TestTextToRtf()
		{
			string text = "hi angie";
			string rtf = ORtfTextUtil.TextToRtf(text);
			AssertEquals("TextToRtf", true, ORtfTextUtil.IsRtf(rtf));
		}

		public void TestBase64RtfToText()
		{
			string text1 = "e1xydGYxXGFuc2lcZGVmZjB7XGZvbnR0Ymx7XGYwXGZuaWxcZmNoYXJzZXQwIE1pY3Jvc29mdCBTYW5zIFNlcmlmO319DQpcdmlld2tpbmQ0XHVjMVxwYXJkXGxhbmczMDgxXGYwXGZzMjAgU3RyaW5nIE9mIERhdGEgc29tZWhvdyByZWxhdGluZyB0byB0aGlzIHRlc3RccGFyDQp9DQo=";
			string text2 = "e1xydGYxXGFuc2lcZGVmZjB7XGZvbnR0Ymx7XGYwXGZuaWxcZmNoYXJzZXQwIE1pY3Jvc29mdCBTYW5zIFNlcmlmO319DQpcdmlld2tpbmQ0XHVjMVxwYXJkXGxhbmcxMDMzXGYwXGZzMjAgU3RyaW5nIE9mIERhdGEgc29tZWhvdyByZWxhdGluZyB0byB0aGlzIHRlc3RccGFyDQp9DQo=";
			var expected = "String Of Data somehow relating to this test";

			AssertEquals(expected, ORtfTextUtil.Base64RtfToText(text1));
			AssertEquals(expected, ORtfTextUtil.Base64RtfToText(text2));
		}

		public void TestGetRtfFirstTextLine()
		{
			string firstLine = ORtfTextUtil.GetRtfFirstTextLine("blahblah", -1);
			AssertEquals("Full first line", "blahblah", firstLine);
			string partFirstLine = ORtfTextUtil.GetRtfFirstTextLine("blahblah", 5);
			AssertEquals("First line with MaxLength", "blahb", partFirstLine);
		}

		public void TestGetRtfFirstTextLineWithMultipleLines()
		{
			string firstLine = ORtfTextUtil.GetRtfFirstTextLine("E 26-Jul-24 14:57: xxxxxxxxxxx\nyyy", -1);
			AssertEquals("Full first line", "E 26-Jul-24 14:57: xxxxxxxxxxx", firstLine);
			string partFirstLine = ORtfTextUtil.GetRtfFirstTextLine("E 26-Jul-24 14:57: xxxxxxxxxxx\nyyy", 5);
			AssertEquals("First line with MaxLength", "E 26-", partFirstLine);
		}

		public void TestCallingOnOtherThreads()
		{
			Thread newThread = new Thread(new ThreadStart(AssertRtf_ThreadStart));
			newThread.Name = "ORtfTextUtilTest_HelperThread_1";
			newThread.Start();
			newThread.Join();
			Thread newThread2 = new Thread(new ThreadStart(AssertRtf_ThreadStart));
			newThread2.Name = "ORtfTextUtilTest_HelperThread_2";
			newThread2.Start();
			newThread2.Join();
		}

		public void TestDefaultFontSize()
		{
			AssertEquals(OFont.GetRichTextBoxFont().Size, ORtfTextUtil.DefaultFontSize);
		}

		public void TestAppendRtfStrings()
		{
			FormattedRtfString string1 = new FormattedRtfString();
			string1 = string1.Add("Hello this is line 1 in bold", System.Drawing.FontStyle.Bold);
			string rawString1 = string1.ToRtf();

			FormattedRtfString string2 = new FormattedRtfString();
			string2 = string2.Add("Hmmmmmmmmm line 2?", System.Drawing.FontStyle.Italic);
			string rawString2 = string2.ToRtf();

			var result = ORtfTextUtil.AppendRtfStrings(rawString1, rawString2);

			var expectedRtf1 = @"{\rtf1\ansi\ansicpg1252\deflang3081\nouicompat\uc0{\fonttbl{\f0\fnil Microsoft Sans Serif;}}{\colortbl}{\*\generator RTFConverter RTFConverter 2.0.0.0}{{\f0\fs20\b";
			var expectedRtf2 = @" Hello this is line 1 in bold}{\f0\fs20\i Hmmmmmmmmm line 2?}\par}{\par}}";
			var concatedExpectedRtf = expectedRtf1 + expectedRtf2;
			AssertRtfTextsEqualLanguageIndependent("2 RTF Strings appended", concatedExpectedRtf, result);

			AssertEquals("Text", "Hello this is line 1 in boldHmmmmmmmmm line 2?\r\n", ORtfTextUtil.RtfToText(result));
		}

		public void TestAppendRtfStrings_PlainText()
		{
			var result = ORtfTextUtil.AppendRtfStrings("hello", "byebye");
			AssertEquals("Should handle plain text appends", ORtfTextUtil.AppendRtfStrings(ORtfTextUtil.TextToRtf("hello", 0), ORtfTextUtil.TextToRtf("byebye", 0)), result);
		}

		public void TestConcatRtfString_OnAnEmptyString()
		{
			var existingNote = ZBlob.Empty;
			var toConcat = "Hello world!";
			var expected = "Hello world!";

			var concatenatedBlob = ORtfTextUtil.ConcatRtfString(existingNote, toConcat);
			AssertEquals(ORtfTextUtil.RtfToText(concatenatedBlob.ToUTF8()), expected);
		}

		public void TestConcatRtfString_OnANonEmptyString()
		{
			var existingNote = ZBlob.FromAscii("This is a non empty string");
			var toConcat = "And this should append to the existingNote after two new lines";
			var expected = "This is a non empty string" + System.Environment.NewLine + System.Environment.NewLine + "And this should append to the existingNote after two new lines" + System.Environment.NewLine;

			var concatenatedBlob = ORtfTextUtil.ConcatRtfString(existingNote, toConcat);
			AssertEquals(ORtfTextUtil.RtfToText(concatenatedBlob.ToUTF8()), expected);
		}

		public void TestRtfToHtml()
		{
			var expected = "<p>1234</p>";
			var htmlBlob = ORtfTextUtil.RtfToHtml(ZBlob.FromUTF8("1234"));
			var htmlBlob2 = ORtfTextUtil.RtfToHtml(ZBlob.FromUTF8("{\\rtf1\\ansi\\ansicpg1252\\nouicompat\\deflang3081\r\n{\\*\\generator Riched20 10.0.19041}\\viewkind4\\uc1 \\pard 1234\\par\r\n}\r\n"));
			AssertEquals(ORtfTextUtil.RtfToText(htmlBlob.ToUTF8()), expected);
			AssertEquals(ORtfTextUtil.RtfToText(htmlBlob2.ToUTF8()), expected);
		}

		public void TestRtfToHtmlString()
		{
			var expected = "<p>1234</p>";
			var htmlBlob = ORtfTextUtil.RtfToHtml("1234");
			var htmlBlob2 = ORtfTextUtil.RtfToHtml("{\\rtf1\\ansi\\ansicpg1252\\nouicompat\\deflang3081\r\n{\\*\\generator Riched20 10.0.19041}\\viewkind4\\uc1 \\pard 1234\\par\r\n}\r\n");
			AssertEquals(ORtfTextUtil.RtfToText(htmlBlob), expected);
			AssertEquals(ORtfTextUtil.RtfToText(htmlBlob2), expected);
		}

		void AssertRtf_ThreadStart()
		{
			try
			{
				string rtf = ORtfTextUtil.TextToRtf("splaty");
				Assert("Rtf converted ok", rtf.Length > 50);
			}
			catch (Exception ex)
			{
				Globals.Message.ShowDeveloperException(ex);
			}
		}
	}
}
