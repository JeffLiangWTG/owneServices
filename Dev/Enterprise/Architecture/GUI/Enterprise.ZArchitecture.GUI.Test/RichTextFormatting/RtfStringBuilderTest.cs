using System;
using System.Drawing;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class RtfStringBuilderTest : TestCaseWithFactory
	{
		public void TestPlainText()
		{
			builder.Append("Just some boring text\n");
			AssertText(@"{\rtf1\ansi\ansicpg936\deff0\deflang1033\deflangfe2052 Just some boring text\par }");
			AssertLinks("No Links");

			builder.AppendFormat("Some {1} {0} Text", "Boring", "More");
			AssertText(@"{\rtf1\ansi\ansicpg936\deff0\deflang1033\deflangfe2052 Just some boring text\par Some More Boring Text}");
			AssertLinks("No Links");

			builder.AppendNewLine();
			AssertText(@"{\rtf1\ansi\ansicpg936\deff0\deflang1033\deflangfe2052 Just some boring text\par Some More Boring Text\par }");
		}

		public void TestPlainTextNewLine()
		{
			builder.Append("Just some boring text\r\n");
			AssertText(@"{\rtf1\ansi\ansicpg936\deff0\deflang1033\deflangfe2052 Just some boring text\par }");
			AssertLinks("No Links");

			builder.Append("Just some boring \r\n text");
			AssertText(@"{\rtf1\ansi\ansicpg936\deff0\deflang1033\deflangfe2052 Just some boring text\par Just some boring \par  text}");
			AssertLinks("No Links");

			builder.AppendNewLine();
			AssertText(@"{\rtf1\ansi\ansicpg936\deff0\deflang1033\deflangfe2052 Just some boring text\par Just some boring \par  text\par }");
		}

		public void TestEscaping()
		{
			builder.Append("{foo}\n");
			builder.AppendFormat("{{bar}}\r\n{0}", "{baz\\}");
			AssertText(@"{\rtf1\ansi\ansicpg936\deff0\deflang1033\deflangfe2052 \{foo\}\par \{bar\}\par \{baz\\\}}");
			AssertLinks("No Links");
		}

		public void TestFormatText()
		{
			builder.AppendFormat("Decimal: {0:0.000}", 5.1234567m);
			AssertText(@"{\rtf1\ansi\ansicpg936\deff0\deflang1033\deflangfe2052 Decimal: 5.123}");
			AssertLinks("No Links");
		}

		public void TestHyperlinks()
		{
			builder.AppendFormat("Link1: {0}\n", new LogUrlLink("Help", new Uri("http://www.cargowise.com")));
#if !WINZOR
			AssertText(@"{\rtf1\ansi\ansicpg936\deff0\deflang1033\deflangfe2052 Link1: Help\v #KEY0000\v0 \par }");
			AssertLinks("1 Link Region",
				new RtfStringBuilder.LinkRegion(7, 12)
			);
#else
			AssertText(@"{\rtf1\ansi\ansicpg936\deff0\deflang1033\deflangfe2052 Link1: {{\field{\*\fldinst{HYPERLINK http://www.cargowise.com/ }}{\fldrslt{Help\ulnone\cf0}}}}\par }");
#endif

			builder.AppendFormat("Link2: {0} ({1})\n", new LogUrlLink("DoStuff", new Uri("http://www.cargowise.com")), new LogUrlLink("Help", new Uri("http://www.cargowise.com")));
			//                       0    0    1       1             2    2    3       3    4        4       5        5
			//                       0    5    0       5             0    5    0       5    0        5       0        5
			//                       ...........   ........         ..............   ........    ......   ........    .
#if !WINZOR
			AssertText(@"{\rtf1\ansi\ansicpg936\deff0\deflang1033\deflangfe2052 Link1: Help\v #KEY0000\v0 \par Link2: DoStuff\v #KEY0001\v0  (Help\v #KEY0000\v0 )\par }");
			// Link1: Help#KEY0000\nLink2: DoStuff#KEY0001 (Help#KEY0000)\n"
			// 01234567890123456789 0123456789012345678901234
			//        7                   27               44
			AssertLinks("3 Link Regions",
				new RtfStringBuilder.LinkRegion(7, 12),
				new RtfStringBuilder.LinkRegion(27, 15),
				new RtfStringBuilder.LinkRegion(44, 12)
			);
#else
			AssertText(@"{\rtf1\ansi\ansicpg936\deff0\deflang1033\deflangfe2052 Link1: {{\field{\*\fldinst{HYPERLINK http://www.cargowise.com/ }}{\fldrslt{Help\ulnone\cf0}}}}\par " +
				@"Link2: {{\field{\*\fldinst{HYPERLINK http://www.cargowise.com/ }}{\fldrslt{DoStuff\ulnone\cf0}}}} ({{\field{\*\fldinst{HYPERLINK http://www.cargowise.com/ }}{\fldrslt{Help\ulnone\cf0}}}})\par }");
#endif
		}

		public void TestHyperlinkLocationCorrectAfterNewlineOrdinaryAppend()
		{
			builder.Append("\nLink1: ");
			builder.AppendFormat("{0}\n", new LogUrlLink("Help", new Uri("http://www.cargowise.com")));
#if !WINZOR
			AssertText(@"{\rtf1\ansi\ansicpg936\deff0\deflang1033\deflangfe2052 \par Link1: Help\v #KEY0000\v0 \par }");
			AssertLinks("Link region after newline in ordinary append",
				new RtfStringBuilder.LinkRegion(8, 12)
			);
#else
			AssertText(@"{\rtf1\ansi\ansicpg936\deff0\deflang1033\deflangfe2052 \par Link1: {{\field{\*\fldinst{HYPERLINK http://www.cargowise.com/ }}{\fldrslt{Help\ulnone\cf0}}}}\par }");
#endif
		}

		public void TestHyperlinkLocationCorrectAfterNewlineAppendFormat()
		{
			builder.AppendFormat("\nLink1: {0}\n", new LogUrlLink("Help", new Uri("http://www.cargowise.com")));
#if !WINZOR
			AssertText(@"{\rtf1\ansi\ansicpg936\deff0\deflang1033\deflangfe2052 \par Link1: Help\v #KEY0000\v0 \par }");
			AssertLinks("Link region after newline in append format",
				new RtfStringBuilder.LinkRegion(8, 12)
			);
#else
			AssertText(@"{\rtf1\ansi\ansicpg936\deff0\deflang1033\deflangfe2052 \par Link1: {{\field{\*\fldinst{HYPERLINK http://www.cargowise.com/ }}{\fldrslt{Help\ulnone\cf0}}}}\par }");
#endif
		}

		public void TestHyperlinkLocationCorrectAfterAppendNewLine()
		{
			builder.AppendNewLine();
			builder.AppendFormat("Link1: {0}\n", new LogUrlLink("Help", new Uri("http://www.cargowise.com")));
#if !WINZOR
			AssertText(@"{\rtf1\ansi\ansicpg936\deff0\deflang1033\deflangfe2052 \par Link1: Help\v #KEY0000\v0 \par }");
			AssertLinks("Link region after calling AppendNewLine",
				new RtfStringBuilder.LinkRegion(8, 12)
			);
#else
			AssertText(@"{\rtf1\ansi\ansicpg936\deff0\deflang1033\deflangfe2052 \par Link1: {{\field{\*\fldinst{HYPERLINK http://www.cargowise.com/ }}{\fldrslt{Help\ulnone\cf0}}}}\par }");
#endif
		}

		public void TestSetForgroundColor()
		{
			var colour = Color.FromArgb(50, 100, 150);

			builder.Append("Normal ");
			builder.SetForgroundColour(colour);
			builder.Append("Woot!");
			builder.SetForgroundColour(null);
			builder.Append(" Normal");

			AssertText(@"{\rtf1\ansi\ansicpg936\deff0\deflang1033\deflangfe2052 {\colortbl;\red50\green100\blue150;}Normal \cf1 Woot!\cf0  Normal}");
			AssertLinks("No Links");
		}

		public void TestChineseText()
		{
			builder.Append("Just some boring text 一些中文汉字\n");
			AssertText(@"{\rtf1\ansi\ansicpg936\deff0\deflang1033\deflangfe2052 Just some boring text \u19968?\u20123?\u20013?\u25991?\u27721?\u23383?\par }");
			AssertLinks("No Links");

			builder.AppendFormat("Some {1} {0} Text 一些中文汉字", "Boring", "More");
			AssertText(@"{\rtf1\ansi\ansicpg936\deff0\deflang1033\deflangfe2052 Just some boring text \u19968?\u20123?\u20013?\u25991?\u27721?\u23383?\par Some More Boring Text \u19968?\u20123?\u20013?\u25991?\u27721?\u23383?}");
			AssertLinks("No Links");

			builder.AppendNewLine();
			AssertText(@"{\rtf1\ansi\ansicpg936\deff0\deflang1033\deflangfe2052 Just some boring text \u19968?\u20123?\u20013?\u25991?\u27721?\u23383?\par Some More Boring Text \u19968?\u20123?\u20013?\u25991?\u27721?\u23383?\par }");
		}

		public void TestLinkTypes()
		{
			var logUrlLink = new LogUrlLink("Help", new Uri("http://www.cargowise.com"));
			builder.AppendArg(logUrlLink, "logUrlLink");

#if !WINZOR
			AssertText(@"{\rtf1\ansi\ansicpg936\deff0\deflang1033\deflangfe2052 Help\v #KEY0000\v0 }");
#else
			AssertText(@"{\rtf1\ansi\ansicpg936\deff0\deflang1033\deflangfe2052 {{\field{\*\fldinst{HYPERLINK http://www.cargowise.com/ }}{\fldrslt{Help\ulnone\cf0}}}}}");
#endif

			var typeOfOrgHeader = ObjectFactory.GetType<IOrgHeader>();
			var org = Factory.NewWithValidTestData(typeOfOrgHeader);
			Factory.Save();
			
			var logControllerLink = new LogControllerLink("text", ControllerIDs.Organisation, org.PK);
			builder.AppendArg(logControllerLink, "logControllerLink");
			var internalUrl = ShowEditFormUrlHandler.Instance.Create(logControllerLink.Controller, logControllerLink.PK);

#if !WINZOR
			AssertText(@"{\rtf1\ansi\ansicpg936\deff0\deflang1033\deflangfe2052 Help\v #KEY0000\v0 text\v #KEY0001\v0 }");
#else
			AssertText($@"{{\rtf1\ansi\ansicpg936\deff0\deflang1033\deflangfe2052 {{{{\field{{\*\fldinst{{HYPERLINK http://www.cargowise.com/ }}}}{{\fldrslt{{Help\ulnone\cf0}}}}}}}}{{{{\field{{\*\fldinst{{HYPERLINK {internalUrl} }}}}{{\fldrslt{{text\ulnone\cf0}}}}}}}}}}");
#endif
		}

			#region Implementation

			void AssertText(string expected)
		{
			AssertText("", expected);
		}
		void AssertText(string message, string expected)
		{
			AssertMultilineASCIIEquals(message, expected.Replace("\\par ", "\\par \n"), builder.ToString().Replace("\\par ", "\\par \n"));
		}
		void AssertLinks(string message, params RtfStringBuilder.LinkRegion[] expected)
		{
			AssertContainsExactElementsInAnyOrder(message, FormatRegion, expected, builder.LinkRegions);
		}
		string FormatRegion(RtfStringBuilder.LinkRegion region)
		{
			return string.Format("{0}, {1}", region.Start, region.Length);
		}
		readonly RtfStringBuilder builder = new RtfStringBuilder(new HyperlinkActionCollection());

		#endregion
	}
}
