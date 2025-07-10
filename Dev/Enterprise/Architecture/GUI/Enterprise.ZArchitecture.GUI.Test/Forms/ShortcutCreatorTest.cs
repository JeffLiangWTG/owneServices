using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ShortcutCreatorTest : TestCaseWithFactory
	{
		[DeveloperOnlyTest]
		public void TestGetHumanReadableNameFromClipboard_Success()
		{
			// Setup
			var caption = "Unable to process binding \"component: \\function(){return { name:}}";
			var sanitisedCaption = "Unable to process binding \"component: \\\\function()\\{return \\{ name:\\}\\}";

			// Act
			ShortcutCreator.CopyTextToClipboard(caption);
			var text = (string)SafeClipboard.GetData(DataFormats.Text);

			// Assert
			AssertEquals("Text caption should be sanitised", true, text.Contains(sanitisedCaption));

			// Cleanup
			SafeClipboard.Clear();
		}

		[DeveloperOnlyTest]
		public void TestCopyHyperlinkToClipboardWhenCaptionContainsCurlyBrackets()
		{
			var url = "edient:Command=SomeURL";
			var caption = "Unable to process binding \"component: \\function(){return { name:}}";
			var sanitisedCaption = "Unable to process binding \"component: \\\\function()\\{return \\{ name:\\}\\}";

			// Hyperlink - Setup
			ShortcutCreatorForTest.CopyHyperlinkToClipboard(caption, url);

			// Hyperlink - Execute
			var rtf = (string)SafeClipboard.GetData(DataFormats.Rtf);
			var plain = (string)SafeClipboard.GetData(DataFormats.Text);

			// Hyperlink - Assert
			AssertEquals("Rtf - plain link", url, plain);
			AssertEquals("Rtf link", true, rtf.Contains(url));
			AssertEquals("Rtf caption should be sanitised", true, rtf.Contains(sanitisedCaption));

			// Web Hyperlink - Setup
			ShortcutCreator.CopyWebHyperlinkToClipboard(caption, url);

			// Web Hyperlink - Execute
			rtf = (string)SafeClipboard.GetData(DataFormats.Rtf);
			plain = (string)SafeClipboard.GetData(DataFormats.Text);

			// Web Hyperlink - Assert
			AssertEquals("Web hyperlink-plain link", url, plain);
			AssertEquals("Web hyperlink-Rtf link", true, rtf.Contains(url));
			AssertEquals("Web hyperlink-Rtf caption should be sanitised", true, rtf.Contains(sanitisedCaption));

			SafeClipboard.Clear();
		}

		[DeveloperOnlyTest]
		public void TestCopyHyperlinkToClipboardWhenCaptionContainsUnicodeCharacters()
		{
			var url = "edient:Command=SomeURL";
			var caption = "1‐2‒3–4—5―测试用例";

			// Hyperlink - Setup
			ShortcutCreatorForTest.CopyHyperlinkToClipboard(caption, url);

			// Hyperlink - Execute
			var rtf = (string)SafeClipboard.GetData(DataFormats.Rtf);
			var plain = (string)SafeClipboard.GetData(DataFormats.Text);

			// Hyperlink - Assert
			AssertEquals("Rtf - plain link", url, plain);
			AssertEquals("Rtf link", true, rtf.Contains(url));
			AssertEquals("Rtf caption should be transformed", true, rtf.Contains("1\\u8208?2\\u8210?3\\u8211?4\\u8212?5\\u8213?\\u27979?\\u35797?\\u29992?\\u20363?"));

			// Web Hyperlink - Setup
			ShortcutCreator.CopyWebHyperlinkToClipboard(caption, url);

			// Web Hyperlink - Execute
			rtf = (string)SafeClipboard.GetData(DataFormats.Rtf);
			plain = (string)SafeClipboard.GetData(DataFormats.Text);

			// Web Hyperlink - Assert
			AssertEquals("Web hyperlink-plain link", url, plain);
			AssertEquals("Web hyperlink-Rtf link", true, rtf.Contains(url));
			AssertEquals("Web hyperlink-Rtf caption should be transformed", true, rtf.Contains("1\\u8208?2\\u8210?3\\u8211?4\\u8212?5\\u8213?\\u27979?\\u35797?\\u29992?\\u20363?"));

			SafeClipboard.Clear();
		}

		[DeveloperOnlyTest]
		public void TestCopyLongLinkToClipboard()
		{
			DoCopyHyperlinkToClipboard("aaa");
			Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);

			DoCopyHyperlinkToClipboard("aaa".PadLeft(1033, 'q'));
			Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);

			DoCopyHyperlinkToClipboard("aaa".PadLeft(1034, 'w'));
			Assert(UnitTestUserNotification.Instance.LastMessage.WasWarning);
			AssertMultilineASCIIEquals("Should show warning message",
				@"The hyperlink exceeds the limit of 1033 characters. It has been copied to clipboard but will not work if you try to paste it into Microsoft Office Application due to known issue with RTF limitations. As a workaround you may create Desktop Shortcut and copy (or drag and drop) it into MS Outlook's email or MS Word document.",
				UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[DeveloperOnlyTest]
		[ExpectNoExceptions]
		public void TestCopyHyperlinkToClipboard()
		{
			DoCopyHyperlinkToClipboard(SampleUrl);
		}

		void DoCopyHyperlinkToClipboard(string url)
		{
			var action = new Action(() => ShortcutCreatorForTest.CopyHyperlinkToClipboard(SampleCaption, url));
			action.Invoke();

			var clipboardData = ClipboardTestHelper.RetryIfCopyOrCutFailed<object>(action, DataFormats.Text);
			AssertEquals("Text", SampleCaption, clipboardData);
			AssertContains("<html><body><!--StartFragment--><a href=\"" + url + "\">" + SampleCaptionHtmlEncoded + "</a><!--EndFragment--></body></html>", SafeClipboard.GetData(DataFormats.Html).ToString());

			var rtf = (string)SafeClipboard.GetData(DataFormats.Rtf);
			AssertEquals("Rtf link", true, rtf.Contains(url));
			AssertEquals("Rtf caption", true, rtf.Contains(SampleCaptionRtfEncoded));
		}

		[DeveloperOnlyTest]
		public void TestGetUrlFromClipboard_Success()
		{
			var action = new Action(() => ShortcutCreatorForTest.CopyHyperlinkToClipboard(SampleCaption, SampleUrl));
			action.Invoke();

			var data = ClipboardTestHelper.RetryIfCopyOrCutFailed<object>(action, DataFormats.Html);
			var url = ShortcutCreatorForTest.TryGetUrlsFromHyperlinkBadly((string)data).Single();
			AssertEquals(SampleUrl, url);
		}

		[DeveloperOnlyTest]
		public void TestGetUrlFromClipboard_Multiple_Success()
		{
			var sampleUrls = new[]
				{
				new [] { "Smangos", "https://WeLikeMangosHere" },
				new [] { "Weanus", "https://WeLikeWeanusHere" },
				new [] { "ChocolateCake", "https://WeLikeChocoCakeHere" },
			};

			ShortcutCreatorForTest.CopyHyperlinksToClipboard(sampleUrls.Select(pair => Tuple.Create((ZString)pair[0], (ZString)pair[1])).ToArray());
			AssertArrayEqualsByElements(sampleUrls.Select(p => p[1]).ToArray(), ShortcutCreatorForTest.TryGetUrlsFromHyperlinkBadly((string)SafeClipboard.GetData(DataFormats.Html)).ToArray());
		}

		[DeveloperOnlyTest]
		public void TestGetUrlFromClipboard_Multiple_Rtf()
		{
			var sampleUrls = new[]
			{
				new [] { SampleCaption, SampleUrl },
				new [] { "ChocolateCake1", "ChocolateCake2" },
			};

			ShortcutCreatorForTest.CopyHyperlinksToClipboard(sampleUrls.Select(pair => Tuple.Create((ZString)pair[0], (ZString)pair[1])).ToArray());

			var rtfText = (string)SafeClipboard.GetData(DataFormats.Rtf);

			AssertEquals(true, rtfText.Contains(SampleCaptionRtfEncoded));
			AssertEquals(true, rtfText.Contains("ChocolateCake1"));
			AssertEquals(true, rtfText.Contains("ChocolateCake2"));
		}

		[DeveloperOnlyTest]
		public void TestGetUrlFromClipboard_Multiple_Text()
		{
			var sampleUrls = new[]
			{
				new [] { SampleCaption, SampleUrl },
				new [] { "ChocolateCake1", "wertyuioghjksdfukhdf11.>1/<" },
			};

			ShortcutCreatorForTest.CopyHyperlinksToClipboard(sampleUrls.Select(pair => Tuple.Create((ZString)pair[0], (ZString)pair[1])).ToArray());

			var text = (string)SafeClipboard.GetData(DataFormats.Text);

			AssertEquals(SampleCaption + System.Environment.NewLine + "ChocolateCake1", text);
		}

		public void TestGetUrlFromClipboard_FailsQuietly_WhenHtmlInvalid()
		{
			ShortcutCreatorForTest.CopyHyperlinkToClipboard(SampleCaption, SampleUrl);

			SafeClipboard.SetData(DataFormats.Html, "This string represents invalid html <q />");

			AssertEquals(false, ShortcutCreatorForTest.TryGetUrlsFromHyperlinkBadly((string)SafeClipboard.GetData(DataFormats.Html)).Any());
		}

		public void TestGetUrlFromClipboard_FailsQuietly_WhenHtmlPartiallyValid()
		{
			ShortcutCreatorForTest.CopyHyperlinkToClipboard(SampleCaption, SampleUrl);

			SafeClipboard.SetData(DataFormats.Html, "<html>Beware, shocking test data below!</html>");

			AssertEquals(false, ShortcutCreatorForTest.TryGetUrlsFromHyperlinkBadly((string)SafeClipboard.GetData(DataFormats.Html)).Any());
		}

#if !WINZOR
// Equivalent version of this test for Winzor is TestCreateDesktopShortcut in Dev\Winzor\Enterprise.Winzor.Architecture.Test\ZFormMenuStrategyTest.cs 
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1089:DoNotUseAssemblyGetEntryAssemblyAnalyzer", Justification = "Baseline")]
		public void TestCreateDesktopShortcut()
		{
			var shortcutFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory), SampleCaptionAsFileName + " (2)" + ".url");
			if (File.Exists(shortcutFile))
			{
				File.Delete(shortcutFile);
			}

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			ShortcutCreatorForTest.CreateDesktopShortcut(SampleCaption, SampleUrl);
			AssertEquals("A shortcut shouldn't be created unless the user clicks OK", false, File.Exists(shortcutFile));

			var existingShortcutFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory), SampleCaptionAsFileName + ".url");
			File.WriteAllText(existingShortcutFile, "");

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			try
			{
				ShortcutCreatorForTest.CreateDesktopShortcut(SampleCaption, SampleUrl);
				AssertEquals("A shortcut file should be created", true, File.Exists(shortcutFile));
				var codeBase = Assembly.GetEntryAssembly().Location;
				AssertEquals("Shortcut file content",
	@"[InternetShortcut]
URL=" + SampleUrl + @"
IconIndex=0
IconFile=" + new Uri(codeBase).LocalPath + "\r\n",
File.ReadAllText(shortcutFile));
			}
			finally
			{
				File.Delete(existingShortcutFile);
				File.Delete(shortcutFile);
			}
		}

// Winzor uses browser-based download to save Shortcut, so UnauthorizedAccessException and DirectoryNotFoundException cannot occur.
		public void TestCreateDesktopShortcut_WithError()
		{
			var shortcutFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory), SampleCaptionAsFileName + ".url");
			try
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				ShortcutCreatorForTest.ExceptionToThrowInWriteAllText = new UnauthorizedAccessException();
				ShortcutCreatorForTest.CreateDesktopShortcut(SampleCaption, SampleUrl);
				AssertEquals("Attempted to perform an unauthorized operation.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			finally
			{
				File.Delete(shortcutFile);
			}

			try
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				ShortcutCreatorForTest.ExceptionToThrowInWriteAllText = new DirectoryNotFoundException("Desktop directory not found");
				ShortcutCreatorForTest.CreateDesktopShortcut(SampleCaption, SampleUrl);
				AssertEquals("Desktop directory not found", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			finally
			{
				File.Delete(shortcutFile);
			}
		}
#endif
		public void TestFormatHyperlink()
		{
			const string linkCaption = "Link caption";
			var hyperlinkDataObject = ShortcutCreatorForTest.FormatHyperlink(linkCaption, SampleUrl);
			using (var data = CargoWise.Interop.DataObjects.ZDataObject.FromData(hyperlinkDataObject))
			{
				var htmlText = (string)data.GetData(DataFormats.Html);
				Assert("Hyperlink should contain the caption", htmlText.Contains(linkCaption));
				Assert("Hyperlink should contain the url", htmlText.Contains(SampleUrl));
			}
		}

#if !WINZOR
		// In SetDataObject(object, bool), the `copy` parameter in SetDataObject, which determines if data is persistent after the application exits, is only applicable in Winforms.
		// For Winzor, since we use navigator.clipboard, the data will be persistent regardless.
		public void TestCopyTextAndHyperlink_DataShouldBePersistent()
		{
			var helper = new ClipboardTestHelper();
			using (helper.MockClipboard())
			{
				var clipboardMock = Mock.Get(ObjectFactory.Get<IClipboard>());

				ShortcutCreator.CopyTextToClipboard("Hello world!!!");
				clipboardMock.Verify(c => c.SetDataObject(It.IsAny<object>(), true), Times.Once);

				ShortcutCreator.CopyWebHyperlinkToClipboard("Hello world!!!", "http://www.google.com");
				clipboardMock.Verify(c => c.SetDataObject(It.IsAny<object>(), true), Times.Exactly(2));
			}

			Assert(true); // this line is here to avoid Empty Test error. It's a quirk in Nunit that a test must have at least one Assert in it.
		}
#endif

		#region Test Classes

		class TestShortcutCreator : ShortcutCreator
		{
			public Exception ExceptionToThrowInWriteAllText;

			protected override void WriteAllText(string fileName, string content)
			{
				base.WriteAllText(fileName, content);
				if (ExceptionToThrowInWriteAllText != null)
				{
					throw ExceptionToThrowInWriteAllText;
				}
			}
		}

		#endregion

		#region Implementation

		readonly TestShortcutCreator ShortcutCreatorForTest = new TestShortcutCreator();
		const string SampleCaption = @"pink/fluffy<>\unicorns";
		const string SampleCaptionHtmlEncoded = @"pink/fluffy&lt;&gt;\unicorns";
		const string SampleCaptionRtfEncoded = @"pink/fluffy<>\\unicorns";
		const string SampleUrl = "https://www.youtube.com/watch?v=eWM2joNb9NE";
#if !WINZOR
		const string SampleCaptionAsFileName = @"pink_fluffy___unicorns";
#endif
#endregion
	}
}
