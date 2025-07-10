using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Text;
using Enterprise.ZArchitecture.Core;

// Do not make changes to this file unless you have a reasionable understanding of the RTF format.
// You can find a copy of the specification in \\corporate.cargowise.com\globaldata\Development\Documents\Standards and Procedures\Word2007RTFSpec9.docx
// or you can download a copy from the microsoft website.

namespace Enterprise.ZArchitecture.GUI
{
	public sealed class RtfStringBuilder
	{
		#region SuppressResourceStringsCheckRegion

		const string DocumentLeadIn = @"{\rtf1\ansi\ansicpg936\deff0\deflang1033\deflangfe2052 ";
		const string DocumentLeadOut = "}";

		const string ColourTableLeadIn = @"{\colortbl;";
		const string ColourTableRecord = @"\red{0}\green{1}\blue{2};";
		const string ColourTableLeadOut = "}";

		const string RtfOpenBrace = @"\{";
		const string RtfCloseBrace = @"\}";
		const string RtfBackslash = @"\\";
		const string RtfParagraph = @"\par ";
		const string RtfStartColour = @"\cf";
		const string RtfEndColor = @"\cf0 ";
#if !WINZOR
		const string RtfStartHidden = @"\v ";
		const string RtfEndHidden = @"\v0 ";
#endif

#endregion

		public RtfStringBuilder(HyperlinkActionCollection hyperlinkActions)
		{
			if (hyperlinkActions == null)
			{
				throw new ArgumentNullException(nameof(hyperlinkActions));
			}

			this.hyperlinkActions = hyperlinkActions;
			this.innerBuilder = new StringBuilder();
			this.linkRegions = new List<LinkRegion>();
			this.colourTable = new List<Color>();
		}

		[SuppressMessage("CargoWiseOne", "CW1066:DoNotUseGoToCaseOrDefault", Justification = "Baseline")]
		public void AppendFormat(string format, params object[] args)
		{
			const int READING_TEXT = 0;
			const int READING_INDEX = 1;
			const int READING_FORMAT = 2;

			if (format == null)
			{
				throw new ArgumentNullException(nameof(format));
			}

			var openBrace = -1;
			var colon = -1;
			var state = READING_TEXT;

			for (var i = 0; i < format.Length; i++)
			{
				switch (format[i])
				{
					case '{':
						if (state == READING_TEXT)
						{
							openBrace = i;

							if (i + 1 < format.Length && format[i + 1] == '{')
							{
								innerBuilder.Append(RtfOpenBrace);
#if !WINZOR
								unformattedLength++;
#endif
								i++;
							}
							else
							{
								state = READING_INDEX;
							}
							break;
						}
						goto default;

					case ':':
						if (state == READING_INDEX)
						{
							colon = i;
							state = READING_FORMAT;
							break;
						}
						goto default;

					case '}':
						if (state == READING_INDEX)
						{
							state = READING_TEXT;
							AppendArg(format, args, openBrace, i);
						}
						else if (state == READING_FORMAT)
						{
							state = READING_TEXT;
							AppendArg(format, args, openBrace, colon, i);
						}
						else if (i + 1 < format.Length && format[i + 1] == '}')
						{
							innerBuilder.Append(RtfCloseBrace);
#if !WINZOR
							unformattedLength++;
#endif
							i++;
						}
						else
						{
							goto default;
						}
						break;

					case '\r':
						if (i + 1 < format.Length && format[i + 1] == '\n')
						{
							i++;
						}
						goto case '\n';

					case '\n':
						if (state == READING_TEXT)
						{
							innerBuilder.Append(RtfParagraph);
#if !WINZOR
							unformattedLength++;
#endif
							break;
						}
						goto default;

					default:
						if (state == READING_TEXT && format[i] != '}')
						{
							innerBuilder.Append(format[i] == '\\' ? RtfBackslash : GetRtfUnicodeEscapedString(format[i]));
#if !WINZOR
							unformattedLength++;
#endif
						}
						else
						{
							FormatExceptionGenerator(format[i], state);
						}
						break;
				}
			}

			if (state != READING_TEXT)
			{
				throw new FormatException("unclosed token");
			}
		}

		void FormatExceptionGenerator(char c, int state)
		{
			if (c == '\n' || c == '\r')
			{
				throw new FormatException("newline is not valid inside a token.");
			}
			else if (c == '{' || c == '}')
			{
				throw new FormatException("badly placed '" + c + "'");
			}
			else if (state == 1 && !char.IsNumber(c))
			{
				throw new FormatException("encountered an invalid character while reading an index.");
			}
		}

		public void Append(string text)
		{
			AppendPlainText(text);
		}
		public void AppendNewLine()
		{
			innerBuilder.Append(RtfParagraph);
#if !WINZOR
			unformattedLength++;
#endif
		}

		public void SetForgroundColour(Color? colour)
		{
			if (colour.HasValue)
			{
				var index = GetColorIndex(colour.Value) + 1;
				innerBuilder.Append(RtfStartColour);
				innerBuilder.Append(index);
				innerBuilder.Append(' ');
			}
			else
			{
				innerBuilder.Append(RtfEndColor);
			}
		}

		public IEnumerable<LinkRegion> LinkRegions
		{
			get { return linkRegions; }
		}

		public HyperlinkActionCollection HyperlinkActions => hyperlinkActions;

		public override string ToString()
		{
			var length = DocumentLeadIn.Length + DocumentLeadOut.Length + innerBuilder.Length;

			if (colourTable.Count > 0)
			{
				length += ColourTableLeadIn.Length + ColourTableLeadOut.Length + ColourTableRecord.Length * colourTable.Count;
			}

			var builder = new StringBuilder(length);
			builder.Append(DocumentLeadIn);

			if (colourTable.Count > 0)
			{
				builder.Append(ColourTableLeadIn);
				foreach (var colour in colourTable)
				{
					builder.AppendFormat(CultureInfo.InvariantCulture, ColourTableRecord, colour.R, colour.G, colour.B);
				}
				builder.Append(ColourTableLeadOut);
			}

			builder.Append(innerBuilder.ToString());
			builder.Append(DocumentLeadOut);
			return builder.ToString();
		}

		void AppendArg(string format, object[] args, int startBrace, int endBrace)
		{
			var value = FindObject(format, args, startBrace, endBrace);
			AppendArg(value, null);
		}
		void AppendArg(string format, object[] args, int startBrace, int colon, int endBrace)
		{
			var value = FindObject(format, args, startBrace, colon);
			var formatText = format.Substring(colon + 1, endBrace - colon - 1);
			AppendArg(value, formatText);
		}

#if DEBUG
		public
#endif
		void AppendArg(object value, string formatText)
		{
			IFormattable formattable;

			if (value == null)
			{
				// do nothing.
			}
			else if (value is LogHyperlink link)
			{
#if !WINZOR
				var action = hyperlinkActions[link];
				AppendHyperlink(link.Text, action.Key);
#else
				switch (link)
				{
					case LogUrlLink urlLink:
						AppendHyperlink(link.Text, urlLink.Url.ToString());
						break;
					case LogControllerLink controllerLink:
						AppendHyperlink(link.Text, ShowEditFormUrlHandler.Instance.Create(controllerLink.Controller, controllerLink.PK));
						break;
					default:
						throw new InvalidOperationException("Unknown hyperlink type");
				}
#endif
			}
			else if (formatText != null && (formattable = value as IFormattable) != null)
			{
				AppendPlainText(formattable.ToString(formatText, null));
			}
			else
			{
				AppendPlainText(value.ToString());
			}
		}

#if !WINZOR
		void AppendHyperlink(string text, string key)
		{
			linkRegions.Add(new LinkRegion(unformattedLength, text.Length + key.Length + 1));

			AppendPlainText(text);
			innerBuilder.Append(RtfStartHidden);
			innerBuilder.Append('#');
			unformattedLength++;
			AppendPlainText(key);
			innerBuilder.Append(RtfEndHidden);
		}
#else
		void AppendHyperlink(string text, string url)
		{
#pragma warning disable CW1161 // Res.GetString Analyzer
			innerBuilder.Append(@"{{\field{\*\fldinst{HYPERLINK " + url + @" }}{\fldrslt{" + text + @"\ulnone\cf0}}}}");
#pragma warning restore CW1161 // Res.GetString Analyzer
		}
#endif

		[SuppressMessage("CargoWiseOne", "CW1066:DoNotUseGoToCaseOrDefault", Justification = "Baseline")]
		void AppendPlainText(string text)
		{
			if (!string.IsNullOrEmpty(text))
			{
				innerBuilder.EnsureCapacity(innerBuilder.Length + text.Length);

				for (var i = 0; i < text.Length; i++)
				{
					switch (text[i])
					{
						case '{':
							innerBuilder.Append(RtfOpenBrace);
#if !WINZOR
							unformattedLength++;
#endif
							break;

						case '}':
							innerBuilder.Append(RtfCloseBrace);
#if !WINZOR
							unformattedLength++;
#endif
							break;

						case '\\':
							innerBuilder.Append(RtfBackslash);
#if !WINZOR
							unformattedLength++;
#endif
							break;

						case '\r':
							if (i + 1 < text.Length && text[i + 1] == '\n')
							{
								i++;
							}
							goto case '\n';

						case '\n':
							innerBuilder.Append(RtfParagraph);
#if !WINZOR
							unformattedLength++;
#endif
							break;

						default:
							innerBuilder.Append(GetRtfUnicodeEscapedString(text[i]));
#if !WINZOR
							unformattedLength++;
#endif
							break;
					}
				}
			}
		}

		string GetRtfUnicodeEscapedString(char input)
		{
			if (input <= 0x7f)
			{
				return input.ToString();
			}
			else
			{
				return "\\u" + Convert.ToUInt32(input) + "?";
			}
		}

		object FindObject(string format, object[] args, int indexLBound, int indexRBound)
		{
			int index;
			var indexStr = format.Substring(indexLBound + 1, indexRBound - indexLBound - 1);

			if (!int.TryParse(indexStr, out index))
			{
				throw new FormatException("error parsing index");
			}

			if (index < 0 || index >= args.Length)
			{
				throw new FormatException("index out of range");
			}

			return args[index];
		}
		int GetColorIndex(Color colour)
		{
			var result = colourTable.IndexOf(colour);

			if (result < 0)
			{
				result = colourTable.Count;
				colourTable.Add(colour);
			}

			return result;
		}

		#region LinkRegion

		[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Only used in this file, not compared with each other except in tests")]
		[System.Diagnostics.DebuggerDisplay("{Start} ({Length})")]
		public struct LinkRegion
		{
			public LinkRegion(int start, int length)
			{
				this.start = start;
				this.length = length;
			}

			[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
			public int Start
			{
				[System.Diagnostics.DebuggerStepThrough]
				get { return start; }
			}
			[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
			public int Length
			{
				[System.Diagnostics.DebuggerStepThrough]
				get { return length; }
			}

			[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
			readonly int start;
			[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
			readonly int length;
		}

		#endregion

#if !WINZOR
		int unformattedLength;
#endif
		readonly StringBuilder innerBuilder;
		readonly IList<LinkRegion> linkRegions;
		readonly List<Color> colourTable;
		readonly HyperlinkActionCollection hyperlinkActions;
	}
}
