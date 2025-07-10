using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;
using static System.FormattableString;

namespace Enterprise.Customs.IL.Business
{
	public abstract class ILEDIMessagePrettierBase
	{
		protected ILEDIMessagePrettierBase(MessageDataObjectBase messageDataObject)
		{
			MessageDataObject = messageDataObject;
		}

		public MessageDataObjectBase MessageDataObject { get; }

		public BusinessObjectFactory Factory => MessageDataObject.Factory;

		public abstract ZString GetMessageInterpretation();

		protected Builder BuildMessageInterpretation() => new Builder(this);

		protected ZString ToBaseInformationPart(Dictionary<ZString, ZString> dictionary)
		{
			var stringBuilder = new ZStringBuilder();
			stringBuilder.Append($"<div>");

			foreach (var element in dictionary)
			{
				if (!element.Value.IsEmpty)
				{
					stringBuilder.Append($"<strong>{element.Key}:</strong> {element.Value}<br>");
				}
			}
			stringBuilder.Append($"</div>");
			return stringBuilder.ToString();
		}

		internal ZString ToH1IfNotEmpty(ZString h1)
		{
			return !h1.IsEmpty
				? (ZString)Invariant($"<H1>{h1}</H1>")
				: ZString.Empty;
		}

		internal ZString ToTable<T>(IEnumerable<T> input, params TableColumn<T>[] cols)
		{
			var creator = GetHtmlTableCreator();

			creator.WriteRowWithFormatting(cols.Select(col => new CellWithFormatting(col.Header, true)).ToArray());

			foreach (var item in input)
			{
				creator.WriteRowWithFormatting(cols.Select(col => new CellWithFormatting(col.GetValue(item))).ToArray());
			}

			return creator.ToHtml();
		}

		protected HtmlTableCreator GetHtmlTableCreator(string width = "100%")
		{
			return new HtmlTableCreator(new NameValueCollection
			{
				{ (NoResString)"border", "1" },
				{ (NoResString)"cellpadding", "1" },
				{ (NoResString)"cellspacing", "0" },
				{ (NoResString)"width", width },
				{ (NoResString)"class", (NoResString)"table" }
			})
			{
				EnableHTMLEncoding = false
			};
		}

		protected ZString ToPointTablePart<T>(ZString sectionCaption, IEnumerable<T> input, params TableColumn<T>[] cols)
		{
			var stringBuilder = new ZStringBuilder();
			stringBuilder.Append($"<div>");
			stringBuilder.Append($"<strong>{sectionCaption}:</strong><br>");
			stringBuilder.Append(ToTable(input, cols));
			stringBuilder.Append($"</div>");
			return stringBuilder.ToString();
		}
	}

	public abstract class ILEDIMessagePrettierBase<TMessage> : ILEDIMessagePrettierBase
			where TMessage : class
	{
		protected ILEDIMessagePrettierBase(MessageDataObject<TMessage> messageDataObject) : base(messageDataObject)
		{
		}

		public new MessageDataObject<TMessage> MessageDataObject => (MessageDataObject<TMessage>)base.MessageDataObject;

		public TMessage MessageData => MessageDataObject.MessageData;

		public override ZString GetMessageInterpretation()
		{
			return MessageDataObject.MessageText;
		}

		protected string ConvertNullableIntToString(int? nullableInt)
			=> nullableInt.HasValue ? nullableInt.ToString() : ZString.Empty;

		protected string ConvertNullableDateTimeToString(DateTime? deliveryOrderDate)
		{
			if (!deliveryOrderDate.HasValue)
			{
				return string.Empty;
			}
			var dateTime = deliveryOrderDate.Value;
			var formattedDate = dateTime.ToLongDateString();
			var formattedTime = dateTime.ToLongTimeString();

			var result = formattedDate;
			return dateTime.TimeOfDay.TotalSeconds > 0
				? formattedDate + " " + formattedTime
				: formattedDate;
		}

		protected CellWithFormatting CreateCell(string cellValue, bool isTitle, int widthInPixel, TextAlign textAlign = TextAlign.left)
		{
			var cell1 = new CellWithFormatting(cellValue, isTitle);
			cell1.HtmlAttributes.Add((NoResString)"style", $"min-width: {widthInPixel}px;text-align: {textAlign};");
			return cell1;
		}

		public enum TextAlign
		{
			center,
			left,
			right
		}
	}
}
