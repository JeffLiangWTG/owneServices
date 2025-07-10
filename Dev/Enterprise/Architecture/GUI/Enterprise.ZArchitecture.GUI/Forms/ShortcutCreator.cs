using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.IO;
using CargoWise.Shared;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices.MessageElements;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	public class ShortcutCreator
	{
		public virtual void CopyHyperlinksToClipboard(params Tuple<ZString, ZString>[] hyperlinks)
		{
			CreateHyperlinkCore(true, hyperlinks);
		}

		[SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings")]
		public static void CopyWebHyperlinkToClipboard(string caption, string url)
		{
			var data = new DataObject();

			// Due to a bug in RDP that Microsoft will not fix, if a clipboard item contains both RTF and HTML, the HTML will be stripped away.
			// On the other hand, Winzor being web-browser based only supports HTML, so we need to provide both. In the event that neither HTML
			// or RTF works, there is still the plain-text version of the URL which should work everywhere.

			var hyperlinks = new [] { Tuple.Create((ZString)caption, (ZString)url) };
			var htmlFragment = string.Format(CultureInfo.InvariantCulture, @"<html><body>{0}</body></html>", GetHyperLinks(hyperlinks));

			data.SetData(DataFormats.Text, true, url);
			data.SetData(DataFormats.Rtf, true, GetRawRtfText(new[] { (caption, url) }));
			data.SetData(DataFormats.Html, true, HtmlClipboardHelper.GetHtmlDataString(htmlFragment));

			if (!SetDataObjectToClipboard(data))
			{
				Globals.Message.Show(SafeClipboard.ClipboardNotAccessibleWarning);
			}
		}

		public static bool DoesUseWebHyperlinks()
		{
			return ZFormUtilities.WebHyperlinksEnabled;
		}

		public static void CopyTextToClipboard(string text)
		{
			var data = new DataObject();
			data.SetData(DataFormats.Text, true, text);
			if (!SetDataObjectToClipboard(data))
			{
				Globals.Message.Show(SafeClipboard.ClipboardNotAccessibleWarning);
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings")]
		public DataObject FormatHyperlink(string caption, string url)
		{
			return CreateHyperlinkCore(false, Tuple.Create((ZString)caption, (ZString)url));
		}

		DataObject CreateHyperlinkCore(bool copyToClipboard, params Tuple<ZString, ZString>[] hyperlinks)
		{
			var data = new DataObject();
			data.SetData(DataFormats.Text, true, string.Join(System.Environment.NewLine, hyperlinks.Select(t => t.Item1)));

			var htmlFragment = string.Format(CultureInfo.InvariantCulture, @"<html><body>{0}</body></html>", GetHyperLinks(hyperlinks)); // HTML For Email
			data.SetData(DataFormats.Html, true, HtmlClipboardHelper.GetHtmlDataString(htmlFragment));
			data.SetData(DataFormats.Rtf, true, GetRawRtfText(hyperlinks.Select(t => (t.Item1.ToString(), t.Item2.ToString()))));

			if (copyToClipboard && !SetDataObjectToClipboard(data))
			{
				Globals.Message.Show(SafeClipboard.ClipboardNotAccessibleWarning);
			}
			else if (hyperlinks.Any(t => t.Item2.Length > MaxSymbolsCount))
			{
				Globals.Message.ShowWarning(Res.GetString("b7f88550-c605-4931-8d29-d393e94615ff", "The hyperlink exceeds the limit of 1033 characters. It has been copied to clipboard but will not work if you try to paste it into Microsoft Office Application due to known issue with RTF limitations. As a workaround you may create Desktop Shortcut and copy (or drag and drop) it into MS Outlook's email or MS Word document."));
			}

			return data;
		}

		public string FormatHyperLinkRtf(string caption, string url)
		{
			return GetRawRtfText(new[] { (caption, url) });
		}

		static bool SetDataObjectToClipboard(IDataObject dataObject) => ObjectFactory.Get<IClipboard>().SetDataObject(dataObject, true);

		static string GetRawRtfText(IEnumerable<(string caption, string link)> hyperlinks)
		{
			return @"{\rtf1\ansi\ansicpg1252\deff0\deflang1033
{\colortbl ;\red0\green0\blue255;}
\viewkind4\uc1\pard\sb100\sa100\lang3081\f0\fs24(*HYPERLINK*)
}"
				.Replace("(*HYPERLINK*)", GetRtfHyperlinks(hyperlinks)); // Rtf formatting
		}

		static string GetRtfHyperlinks(IEnumerable<(string caption, string link)> hyperlinks)
		{
			return string.Join(@"\line", hyperlinks.Select(s => GetRtfHyperLink(s.caption, s.link))); // Rtf formatting
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Rtf formatting")]
		static string GetRtfHyperLink(string caption, string url)
		{
			caption = SpecialCharactersExpression.Replace(caption, @"\$1");
			caption = BMPCharacterExpression.Replace(caption, GetUnicodeEscaped);

			return @"{\field{\*\fldinst{HYPERLINK ""(*URL*)""}}{\fldrslt{\cf1\ul (*TEXT*)}}}".Replace("(*URL*)", url).Replace("(*TEXT*)", caption);
		}

		static string GetUnicodeEscaped(Capture m)
		{
			return $"\\u{(ushort)m.Value[0]}?"; // Escape RTF Unicode
		}

		static readonly Regex BMPCharacterExpression = new Regex("[\u0080-\uFFFF]", RegexOptions.Compiled);

		static readonly Regex SpecialCharactersExpression = new Regex(@"([\\\{\}])", RegexOptions.Compiled);

		static string GetHyperLinks(IEnumerable<Tuple<ZString, ZString>> hyperlinks)
		{
			return string.Join(@"<br>", hyperlinks.Select(t => GetHyperlink(t.Item1, t.Item2)));  // Html for Email
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Html for Email")]
		static string GetHyperlink(string caption, string url)
		{
			return "<a href=\"" + url + "\">" + WebUtility.HtmlEncode(caption) + "</a>";
		}

		public virtual void CopyHyperlinkToClipboard(string caption, string url)
		{
			CopyHyperlinksToClipboard(Tuple.Create((ZString)caption, (ZString)url));
		}

		public virtual IEnumerable<string> TryGetUrlsFromHyperlinkBadly(string text)
		{
			try
			{
				var htmlMatches = UnreliableHtmlHyperlinkExpression.Matches(text).Cast<Match>().Select(m => m.Value).Select(match => match.Substring(9, match.Length - 10));
				var markdownMatches = MarkdownHyperlinkExpression.Matches(text).Cast<Match>().Select(m => m.Groups[2].Value);

				return htmlMatches.Concat(markdownMatches);
			}
			catch (Exception e)
			{
				if (e.IsCriticalException())
				{
					throw;
				}
				else
				{
					return Enumerable.Empty<string>();
				}
			}
		}

		// It's bad because regex can't parse html very well. Only use this when you have control over the relevant html being parsed.
		static Regex UnreliableHtmlHyperlinkExpression { get; } = new Regex("<a\\s+(?:[^>]*\\s+)?href=\"([^\"]*)\"", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		static Regex MarkdownHyperlinkExpression { get; } = new Regex(@"\[(.*)\]\((https://.*\/link\/ShowEditForm\/.*)\)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		const int MaxSymbolsCount = 1033; //found in experimental way

		[SuppressMessage("CargoWiseOne", "CW1089:DoNotUseAssemblyGetEntryAssemblyAnalyzer", Justification = "Baseline")]
		public virtual void CreateDesktopShortcut(string caption, string url)
		{
			if (ObjectFactory.Get<TerminalService>().IsWTSSession && ObjectFactory.Get<TerminalService>().IsRemoteAppSession)
			{
				if (Array.IndexOf(InitializationMessageHandler.RegisteredRemoteMessageTypes, EnterpriseChannelMessageTypes.CreateDesktopShortcut) > -1)
				{
					var message = Res.GetString("47ff4574-0f51-4ef5-8bb9-6759d70603cd", "A shortcut to this record will be placed on your desktop.");
					var response = Globals.Message.Show(message, BrandingFactory.Instance.ProductName, MessageBoxButtons.OKCancel, DialogResult.OK);
					if (response == DialogResult.OK)
					{
						EnterpriseChannel.Instance.SendMessage(EnterpriseChannelMessageTypes.CreateDesktopShortcut, new CreateDesktopShortcutMessage(caption, url));
					}
				}
				else
				{
					var message = Res.GetString("6ab30b09-4ffc-4b89-ab17-2dfda9996658", "To save a shortcut to your desktop while running {0} as a Remote Application, you must install the latest version of {0} Remote Desktop Services", Constants.ProductName);
					Globals.Message.ShowError(message);
				}
			}
			else
			{
				var message = Res.GetString("af70ab5b-539b-43d7-8015-29ee7728e093", "A shortcut to this record will be placed on your desktop.\r\nIf you are running {0} on a Terminal Server or Citrix client, the shortcut will be placed on the terminal server desktop.", Constants.ProductName);
				var response = Globals.Message.Show(message, BrandingFactory.Instance.ProductName, MessageBoxButtons.OKCancel, DialogResult.OK);
				if (response == DialogResult.OK)
				{
					try
					{
						var name = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory), PathValidation.GetSafeFilename(caption, '_'));
						var fileName = name;
						for (var i = 2; File.Exists(fileName + ".url"); i++)
						{
							fileName = name + " (" + i + ")";
						}
						fileName += ".url";

						var entryAssembly = Assembly.GetEntryAssembly(); // In the rare case no .exe is available from GetEntryAssembly, no icon is used which is ok
						var iconFile = (entryAssembly == null) ? "" : ("IconFile=" + new Uri(entryAssembly.Location).LocalPath);
						var content =
	@"[InternetShortcut]
URL=" + url + @"
IconIndex=0
" + iconFile + "\r\n"; // HTML For Email
						WriteAllText(fileName, content);
					}
					catch (IOException ex)
					{
						Globals.Message.ShowError(ex.Message);
					}
					catch (UnauthorizedAccessException ex)
					{
						Globals.Message.ShowError(ex.Message);
					}
				}
			}
		}

#if DEBUG
		protected virtual
#endif
 void WriteAllText(string fileName, string content)
		{
			File.WriteAllText(fileName, content);
		}
	}
}
