using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	public static class RichTextActionManager
	{
		public static void SetTextFromMarkdown(RichTextBox box, string markdown, RtfStringBuilder builder = null)
		{
			builder ??= new RtfStringBuilder(new HyperlinkActionCollection());
			AppendMarkdownContent(builder, markdown);
			AppendBuilderContent(box, builder);
			box.LinkClicked += CreateClickHandler(builder.HyperlinkActions, false);
		}

		static void AppendMarkdownContent(RtfStringBuilder builder, string markdown)
		{
			foreach (var textOrLink in ParseMarkdown(markdown.Replace("\n", "\r\n").Replace("\r\r\n", "\r\n")))
			{
				builder.AppendFormat("{0}", textOrLink);
			}
		}

		public static void FixCustomisedHyperlinks(RichTextBox textBox, string markdown, int startingTextLength)
		{
#if !WINZOR
			var builder = new RtfStringBuilder(new HyperlinkActionCollection());
			AppendMarkdownContent(builder, markdown);

			foreach (var region in builder.LinkRegions)
			{
				textBox.BeginInvoke(() =>
				{
					if (!textBox.IsDisposed)
					{
						textBox.Select(region.Start + startingTextLength, region.Length);
						textBox.SetSelectionLink(true);
					}
				});
			}
#endif
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Html element")]
		public static void AppendBuilderContent(RichTextBox textBox, RtfStringBuilder builder)
		{
			var startingTextLength = textBox.TextLength;
			var selectionStart = textBox.SelectionStart;
			var selectionLength = textBox.SelectionLength;
			var follow = selectionStart == startingTextLength;

			try
			{
#if !WINZOR
				textBox.SelectionStart = startingTextLength;
				textBox.SelectedRtf = builder.ToString();

				foreach (var region in builder.LinkRegions)
				{
					textBox.BeginInvoke(() =>
					{
						if (!textBox.IsDisposed)
						{
							textBox.Select(region.Start + startingTextLength, region.Length);
							textBox.SetSelectionLink(true);
						}
					});
				}
#else
				textBox.Html += ORtfTextUtil.RtfToHtml(builder.ToString());
#endif
			}
			finally
			{
				if (follow)
				{
					textBox.SelectionStart = textBox.TextLength;
					textBox.ScrollToCaret();
				}
				else
				{
					textBox.SelectionStart = selectionStart;
					textBox.SelectionLength = selectionLength;
				}
			}
		}

		public static IEnumerable<object> ParseMarkdown(string markdown)
		{
			// Expected format for a hyperlink:
			// "[I'm an inline-style link](https://www.google.com)"
			// Regular expression:
			//   1) left square bracket, 2) anything that's not a square bracket or a parenthesis, 3) right square bracket,
			//   4) left parenthesis, 5) Enterprise.URLHandler.EdiUrlPrefix.Value or https: or http:,
			//   6) anything not a space, 7) right parenthesis
			//   Item (2) makes up the link text, and items (6) and (7) make up the URI

			var hyperlinkRegex = new Regex(@"\[(?<linkText>[^\]]+)\]\((?<uri>(https|http|edient):\S+)\)");
			var matches = hyperlinkRegex.Matches(markdown);
			if (matches.Count > 0)
			{
				for (var i = 0; i < matches.Count; i++)
				{
					var match = matches[i];
					string preMatchString;
					if (i == 0)
					{
						preMatchString = markdown.Substring(0, match.Index);
					}
					else
					{
						var previousMatch = matches[i - 1];
						preMatchString = markdown.Substring(previousMatch.Index + previousMatch.Length, match.Index - (previousMatch.Index + previousMatch.Length));
					}

					if (!preMatchString.IsNullOrEmpty())
					{
						yield return preMatchString;
					}

					var uriSuccess = Uri.TryCreate(match.Groups["uri"].Value, UriKind.Absolute, out var uri);
					if (uriSuccess)
					{
						yield return new LogUrlLink(match.Groups["linkText"].Value, uri);
					}
					else
					{
						yield return markdown.Substring(match.Index, match.Length);
					}
				}
				var lastMatch = matches[matches.Count - 1];
				var postMatchString = markdown.Substring(lastMatch.Index + lastMatch.Length);
				if (!postMatchString.IsNullOrEmpty())
				{
					yield return postMatchString;
				}
			}
			else
			{
				if (!markdown.IsNullOrEmpty())
				{
					yield return markdown;
				}
			}
		}

		public static LinkClickedEventHandler CreateClickHandler(HyperlinkActionCollection actions, bool reportOnFailure)
		{
			return (object sender, LinkClickedEventArgs e) =>
			{
				var link = e.LinkText;

				if (!HyperlinkActionCollection.IsHyperlinkActionCollectionLink(e))
				{
					try
					{
						WebUrlLauncher.Launch(link);
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						Globals.Message.ShowError(ex.Message, Res.GetString("249b1d5a-72b4-4e9e-94a8-7386179303fa", "Could not open link"));
					}
				}
				else
				{
					/*
					 * Explanation:
					 * After Work Item WI00218111 changed to the new Rich Edit Control, it stopped displaying hidden text in its Text property.
					 * For KRichTextBox/ZRichTextBox, this is easy to fix - we already have an RTF text parser that outputs text with hidden text, and its behaviour
					 * is identical to how old RichTextBox.Text worked. But there is a problem with hyperlinks - text parsing to see what text is in that hyperlink
					 * is done in RichTextBox itself, and thus hidden text is ignored. In such cases, we need to examine the Z/K RichTextBox directly, find the link
					 * and scrape the # and following information ourselves. (It's worth considering if we even want the new Rich Edit Control that badly, since we
					 * apparently relied on quirks of its behaviour that it no longer does.)
					 * (Note: LinkClickedEventArgs.LinkText comes from RichTextBox private void EnLinkMsgHandler(ref Message m), which calls native methods. It lacks
					 * everything 'hidden' in the RTF, same as RichTextBox.Text, after the RichTextControl changes, which is the specific reason why stuff after #
					 * is no longer in the link.)
					 * */
					if (!link.Contains("#"))
					{
						var richTextBox = sender as RichTextBox;
						if (richTextBox != null)
						{
							var richText = richTextBox.Text;
							var indexInRichTextBox = richText.IndexOf(link, StringComparison.Ordinal);
							if (indexInRichTextBox != -1)
							{
								var indexOfNextHash = richText.IndexOf('#', indexInRichTextBox);
								if (indexOfNextHash != -1)
								{
									link = richText.Substring(indexInRichTextBox, indexOfNextHash - indexInRichTextBox + 1 + HyperlinkActionCollection.Key.Length + HyperlinkActionCollection.KeyLength);
								}
							}
						}
					}

					var lastHash = link.LastIndexOf('#');

					if (lastHash < 0)
					{
						if (reportOnFailure)
						{
							ErrorReporter.ReportOnce("ActionLog.logTextBox_LinkClicked1", string.Format(CultureInfo.InvariantCulture, "cant find the '#' in '{0}'", link));
							Globals.Message.Show(Res.GetString("OperationalActionLog|BrokenLink", "Broken Link"));
						}
					}
					else
					{
						var key = link.Substring(lastHash + 1, link.Length - lastHash - 1);

						var action = actions[key];

						if (action == null)
						{
							if (!Uri.IsWellFormedUriString(link, UriKind.RelativeOrAbsolute))
							{
								ErrorReporter.ReportOnce("ActionLog.logTextBox_LinkClicked2", string.Format(CultureInfo.InvariantCulture, "dont know anything about the key '{0}' (from: {1})", key, link));
								Globals.Message.Show(Res.GetString("OperationalActionLog|BrokenLink", "Broken Link"));
							}
						}
						else
						{
							action.DoAction();
						}
					}
				}
			};
		}
	}
}
