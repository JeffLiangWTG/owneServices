using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation;
using Enterprise.Messaging.MessageProcessors;
using static System.FormattableString;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Html strings")]
	public abstract class UCCMessagePrettier<TMessage> : FREDIMessagePrettier<TMessage>
		where TMessage : class
	{
		protected UCCMessagePrettier(MessageDataObject<TMessage> messageDataObject) : base(messageDataObject)
		{
		}

		public override ZString GetMessageInterpretation()
		{
			var prettiedMessage = ZString.Empty;

			try
			{
				prettiedMessage = GetMessageInterpretationStyle + GetMessageInterpretationCore(ResponseMessage) + GetErrorsTableIfNeeded(ResponseMessage);
			}
			catch (Exception ex)
			{
				var messageText = MessageDataObject.MessageText;
				prettiedMessage = $@"{formatError}: {ex.Message}<br><br>{messageText}";
				ErrorReporter.ReportOnce($"{formatError}\r\n{ex.Message}\r\nMessage Text:\r\n{messageText}", ex);
			}

			return prettiedMessage;
		}

		protected abstract ZString GetMessageInterpretationCore(TMessage messageObject);

		protected virtual ZString GetErrorsTableIfNeeded(TMessage messageObject) => ZString.Empty;

		protected virtual ZString GetMessageInterpretationStyle => "<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style>";

		protected string GetFieldCode(string path)
		{
			return path.Split(pathDelimiters.ToArray()).LastOrDefault() ?? string.Empty;
		}

		protected string AppendWordBreaksToPath(string path)
		{
			var result = path;
			foreach (var delimiter in pathDelimiters)
			{
				result = result.Replace(delimiter.ToString(), "<wbr>" + delimiter);
			}
			return result;
		}

		protected HtmlTableCreator GetHtmlTableCreator(string width = "100%")
		{
			return new HtmlTableCreator(new NameValueCollection
			{
				{ "border", "1" },
				{ "cellpadding", "1" },
				{ "cellspacing", "0" },
				{ "width", width },
				{ "class", "table" }
			})
			{
				EnableHTMLEncoding = false
			};
		}

		protected ZString ToTableSection(ZString caption, HtmlTableCreator creator, bool toP = true)
		{
			var result = ZString.Empty;

			ZString tableString = creator?.ToHtml() ?? ZString.Empty;
			if (!tableString.IsEmpty)
			{
				var content = new ZString(ToStrongIfNotEmpty($"{caption}: ") + Invariant($"{tableString}")).Trim(' ', ':');
				result = toP ? ToPIfNotEmpty(content) : content;
			}

			return result;
		}

		protected ZString ToTr(ZString caption)
		{
			return (ZString)Invariant($"<tr>{caption}</tr>");
		}

		protected void EndOfARow(ref ZString result, ref ZString content)
		{
			result += ToTr(content);
			content = ZString.Empty;
		}

		protected ZString ToTd(ZString caption, int rowspan, int colspan)
		{
			var rowspanString = rowspan > 0 ? $" rowspan=\"{rowspan.ToString()}\"" : "";
			var colspanString = colspan > 0 ? $" colspan=\"{colspan.ToString()}\"" : "";
			return (ZString)Invariant($"<td{rowspanString}{colspanString}>{caption}</td>");
		}

		protected ZString ToTd(ZString caption, int rowspan, int colspan, ZString backgroundColor)
		{
			var rowspanString = rowspan > 0 ? $" rowspan=\"{rowspan.ToString()}\"" : "";
			var colspanString = colspan > 0 ? $" colspan=\"{colspan.ToString()}\"" : "";
			var backgroundColorString = !backgroundColor.IsEmpty ? $" bgcolor=\"{backgroundColor}\"" : "";
			return (ZString)Invariant($"<td{backgroundColorString}{rowspanString}{colspanString}>{caption}</td>");
		}

		protected ZString ToKeyValuePairSection(IEnumerable<(ZString key, ZString value)> pairs, bool allowEmpties = false, bool toLargerP = true, string title = "")
		{
			var content = pairs
				.Where(p => allowEmpties || !p.value.IsEmpty)
				.Select(p => new ZString(ToStrongIfNotEmpty(p.key + ": ") + p.value).Trim(' ', ':'))
				.Where(x => allowEmpties || !x.IsEmpty)
				.JoinAsString("<br>");

			if (!title.IsEmpty())
			{
				content = ToStrongIfNotEmpty(title) + "<br>" + content;
			}

			return toLargerP ? ToLargerPIfNotEmpty(content) : content;
		}

		protected ZString ToStrongIfNotEmpty(ZString strong, string emptyTemplate = ": ")
		{
			return !(strong.Replace(emptyTemplate, "")).IsEmpty
				? (ZString)Invariant($"<strong>{strong}</strong>")
				: ZString.Empty;
		}

		protected ZString ToPIfNotEmpty(ZString p)
		{
			return !p.IsEmpty
				? (ZString)Invariant($"<p>{p}</p>")
				: ZString.Empty;
		}

		protected ZString ToLargerPIfNotEmpty(ZString h)
		{
			return !h.IsEmpty
				? (ZString)Invariant($"<p style=\"font-size: 120%\">{h}</p>")
				: ZString.Empty;
		}

		readonly ImmutableArray<char> pathDelimiters = new char[]
		{
			'.', '/'
		}.ToImmutableArray();

		readonly ZString formatError = ResString.GetMultilingualString("7E8F36A1-B876-47BE-B2B6-720E7C5E0211", "Message content is badly formatted.");
	}
}
