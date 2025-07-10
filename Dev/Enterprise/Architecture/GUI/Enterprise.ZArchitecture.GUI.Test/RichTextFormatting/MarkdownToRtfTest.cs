using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class MarkdownToRtfTest : TestCase
	{
		public void TestParseText_WhenNoHyperlinks_ShouldLeaveUnchanged()
		{
			var textToParse = "Hi there, this is a message without a hyperlink";
			var expectedObjects = new List<object> { textToParse };
			AssertSequencesEqual("Failed to leave message as is", expectedObjects, RichTextActionManager.ParseMarkdown(textToParse));
		}

		public void TestParseText_WhenHyperLinkExists_ReturnsLink()
		{
			var textToParse = "[I'm an inline-style link](https://www.google.com)";
			var expectedObjects = new List<object> { new LogUrlLink("I'm an inline-style link", new Uri("https://www.google.com")) };
			var actualObjects = RichTextActionManager.ParseMarkdown(textToParse);
			AssertSequencesEqual("Doesn't include hyperlink", expectedObjects, actualObjects);
		}

		public void TestParseText_WhenHyperLinkExistsAndNonASCII_ReturnsLink()
		{
			var textToParse = "[漢字](https://www.google.com)";
			var expectedObjects = new List<object> { new LogUrlLink("漢字", new Uri("https://www.google.com")) };
			var actualObjects = RichTextActionManager.ParseMarkdown(textToParse);
			AssertSequencesEqual("Doesn't include hyperlink", expectedObjects, actualObjects);
		}

		public void TestParseText_WhenTextAndLinksExist_ReturnsThem()
		{
			var textToParse = "Before text ending with whitespace [I'm an inline-style link](https://www.google.com) after text starting with whitespace";
			var expectedObjects = new List<object> { "Before text ending with whitespace ", new LogUrlLink("I'm an inline-style link", new Uri("https://www.google.com")), " after text starting with whitespace" };
			var actualObjects = RichTextActionManager.ParseMarkdown(textToParse);
			AssertSequencesEqual("Doesn't include text", expectedObjects, actualObjects);
		}

		public void TestParseText_WhenEmpty_ShouldReturnEmpty()
		{
			var textToParse = "";
			var expectedObjects = new List<object> { };
			AssertSequencesEqual("Doesn't return an empty list", expectedObjects, RichTextActionManager.ParseMarkdown(textToParse));
		}

		public void TestParseText_WithInvalidHyperlink_ShouldReturnText()
		{
			var textToParse = "[I forgot my second square bracket(https://www.google.com)";
			var expectedObjects = new List<object> { "[I forgot my second square bracket(https://www.google.com)" };
			var actualObjects = RichTextActionManager.ParseMarkdown(textToParse);
			AssertSequencesEqual("Doesn't include hyperlink", expectedObjects, actualObjects);
		}

		public void TestParseText_WithMultilineLinks_ShouldReturnText()
		{
			var textToParse = "[I have newlines in my url](https\n://www.google.com)";
			IEnumerable<object> expectedObjects = new List<object> { "[I have newlines in my url](https\n://www.google.com)" };
			var actualObjects = RichTextActionManager.ParseMarkdown(textToParse);
			AssertSequencesEqual("Doesn't return text when encountering multiline links", expectedObjects, actualObjects);
		}

		public void TestParseText_WithHyperlinkAndAlsoUnrelatedLinebreaks_ShouldKeepLineBreaks()
		{
			var textToParse = "[I'm an inline-style link](https://www.google.com)\n\nHere is some more text";
			var expectedObjects = new List<object> { new LogUrlLink("I'm an inline-style link", new Uri("https://www.google.com")), "\n\nHere is some more text" };
			var actualObjects = RichTextActionManager.ParseMarkdown(textToParse);
			AssertSequencesEqual("Doesn't keep line breaks", expectedObjects, actualObjects);
		}

		public void TestParseText_WithTextBetweenLinks_ReturnsThatText()
		{
			var textToParse = "[I'm an inline-style link](https://www.google.com) Hey I want some more text here [This one links to Wikipedia](https://en.wikipedia.org/wiki/Main_Page)";
			var expectedObjects = new List<object> { new LogUrlLink("I'm an inline-style link", new Uri("https://www.google.com")), " Hey I want some more text here ", new LogUrlLink("This one links to Wikipedia", new Uri("https://en.wikipedia.org/wiki/Main_Page")) };
			var actualObjects = RichTextActionManager.ParseMarkdown(textToParse);
			AssertSequencesEqual("Doesn't return text between links", expectedObjects, actualObjects);
		}

		public void TestParseText_WithPathologicallyLongFormattedHyperlink_ShouldNotTakeTooLong()
		{
			var textToParse = "[I'm a very looooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooong link](https://www.example.com/looooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooong)";

			AssertNoExceptionThrown(() => RichTextActionManager.ParseMarkdown(textToParse));
		}

		public void TestParseText_WithInvalidMarkdown()
		{
			var textToParse = "[I'm an inline-style link](https://www.google.com)[I'm another inline-style link](https://www.google.com<>)";
			var expectedObjects = new List<object> { new LogUrlLink("I'm an inline-style link", new Uri("https://www.google.com")), "[I'm another inline-style link](https://www.google.com<>)" };
			var actualObjects = RichTextActionManager.ParseMarkdown(textToParse);
			AssertSequencesEqual("Doesn't return text when encountering invalid links", expectedObjects, actualObjects);
		}

#if !WINZOR
		// TestWinzorMarkdownHyperLinkIsCreated below is enough to cover this test in Winzor
		public void TestParseText_HyperLinkIsCreated()
		{
			using (var form = new Form())
			using (var box = new RichTextBox())
			{
				form.Controls.Add(box);
				box.CreateControl();
				form.Show();

				var textToParse = "[I'm an inline-style link](https://www.google.com)";
				RichTextActionManager.SetTextFromMarkdown(box, textToParse);
				AssertEquals("Doesn't include hyperlink", false, box.GetSelectionLink());

				Application.DoEvents();
				AssertEquals("Include hyperlink", true, box.GetSelectionLink());
			}
		}
#else
		public void TestWinzorMarkdownHyperLinkIsCreated()
		{
			using (var form = new Form())
			using (var box = new RichTextBox() { DetectUrls = true, IsToolBarVisible = true, Font = new System.Drawing.Font("Tahoma", 20, System.Drawing.GraphicsUnit.Pixel) })
			{
				form.Controls.Add(box);
				box.CreateControl();
				form.Show();
				var textToParse = "[I'm an inline-style link](https://www.wisetechglobal.com) Hey I want some more text here [This one links to Wikipedia](https://en.wikipedia.org/wiki/Main_Page)";
				RichTextActionManager.SetTextFromMarkdown(box, textToParse);
				AssertContains("<a href=\"https://www.wisetechglobal.com/\" target=\"_blank\" style=\"font-family: Tahoma, sans-serif; font-size: 20px;\">", box.Html);
				AssertContains("I&#39;m an inline-style link", box.Html);
				AssertContains("This one links to Wikipedia", box.Html);
			}
		}

		public void TestWinzorMarkdownHyperLinkNotBlockWhenUrlText()
		{
			using (var form = new Form())
			using (var box = new RichTextBox())
			{
				form.Controls.Add(box);
				box.CreateControl();
				form.Show();

				var textToParse = "[http://www.google.com](https://www.google.com)";
				RichTextActionManager.SetTextFromMarkdown(box, textToParse);
				AssertContains("https://www.google.com/", box.Html);
			}
		}
#endif
	}
}
