using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.IO;
using CargoWise.Shared;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	public class ShortcutCreator
	{
		const int MaxSymbolsCount = 1033; //found in experimental way
		static Regex UnreliableHtmlHyperlinkExpression { get; } = new Regex("<a\\s+(?:[^>]*\\s+)?href=\"([^\"]*)\"", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		static Regex MarkdownHyperlinkExpression { get; } = new Regex("\\[(.*)\\]\\((https://.*\\/link\\/ShowEditForm\\/.*)\\)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		public virtual void CopyHyperlinksToClipboard(params Tuple<ZString, ZString>[] hyperlinks) =>
			CreateHyperlinkCore(true, hyperlinks);

		public static void CopyWebHyperlinkToClipboard(string caption, string url)
		{
			var data = new DataObject();

			var hyperlinks = Tuple.Create((ZString)caption, (ZString)url);
			var htmlFragment = string.Format(CultureInfo.InvariantCulture, @"<html><body>{0}</body></html>", GetHyperLinks([hyperlinks]));

			data.SetData(DataFormats.Text, true, url);
			data.SetData(DataFormats.Html, true, HtmlClipboardHelper.GetHtmlDataString(htmlFragment));

			if (!SetDataObjectToClipboard(data))
			{
				Globals.Message.Show(SafeClipboard.ClipboardNotAccessibleWarning);
			}
		}

		public virtual void CopyHyperlinkToClipboard(string caption, string url) =>
			CopyHyperlinksToClipboard(Tuple.Create((ZString)caption, (ZString)url));

		public static bool DoesUseWebHyperlinks()
		{
			var sessionBrokerInstalled = !ZFormUtilities.WebVersionLaunchUrl.IsNullOrEmpty();
			var enterpriseServicesInstalled = !ZFormUtilities.EnterpriseServicesUrl.IsNullOrEmpty();

			return sessionBrokerInstalled || enterpriseServicesInstalled;
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1089:DoNotUseAssemblyGetEntryAssemblyAnalyzer", Justification = "Baseline")]
		public virtual void CreateDesktopShortcut(string caption, string url)
		{
			try
			{
				var fileName = PathValidation.GetSafeFilename(caption, '_') + ".url";
				var entryAssembly = Assembly.GetEntryAssembly(); // In the rare case no .exe is available from GetEntryAssembly, no icon is used which is ok
				var iconFile = (entryAssembly == null) ? "" : ("IconFile=" + new Uri(entryAssembly.Location).LocalPath);
				var content =
					@"[InternetShortcut]
URL=" + url + @"
IconIndex=0
" + iconFile + "\r\n"; // HTML For Email
				SaveShortcutFile(fileName, content);
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

		public DataObject FormatHyperlink(string caption, string url)
		{
			return CreateHyperlinkCore(false, Tuple.Create((ZString)caption, (ZString)url));
		}

		DataObject CreateHyperlinkCore(bool copyToClipboard, params Tuple<ZString, ZString>[] hyperlinks)
		{
			var data = new DataObject();
			var htmlFragment = string.Format(CultureInfo.InvariantCulture, @"<html><body>{0}</body></html>", GetHyperLinks(hyperlinks)); // HTML For Email

			data.SetData(DataFormats.Text, true, string.Join(System.Environment.NewLine, hyperlinks.Select(t => t.Item2)));
			data.SetData(DataFormats.Html, true, HtmlClipboardHelper.GetHtmlDataString(htmlFragment));

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

		static string GetHyperLinks(IEnumerable<Tuple<ZString, ZString>> hyperlinks)
		{
			return string.Join(@"<br>", hyperlinks.Select(t => GetHyperlink(t.Item1, t.Item2)));  // Html for Email
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Html for Email")]
		static string GetHyperlink(string caption, string url)
		{
			return "<a href=\"" + url + "\">" + WebUtility.HtmlEncode(caption) + "</a>";
		}

		static bool SetDataObjectToClipboard(IDataObject dataObject)
		{
			try
			{
				SafeClipboard.SetDataObject(dataObject, true);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public string FormatHyperLinkRtf(string caption, string url) => null;

		protected virtual void SaveShortcutFile(string fileName, string content)
		{
			Form.SaveShortcutFile(fileName, content);
		}
#if DEBUG
		protected virtual void WriteAllText(string fileName, string content)
		{
			File.WriteAllText(fileName, content);
		}
#endif
	}
}
