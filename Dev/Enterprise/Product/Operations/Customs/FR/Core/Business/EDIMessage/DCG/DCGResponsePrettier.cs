using System.Globalization;
using CargoWise.Customs.FR.MessageDefinitions.DCG.Response;
using CargoWise.Types;
using static System.FormattableString;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Response Html strings")]
	public class DCGResponsePrettier : FREDIMessagePrettier<TMessage>
	{
		public DCGResponsePrettier(DCGResponseMessageDataObject messageDataObject)
			: base(messageDataObject)
		{
		}

		public new DCGResponseMessageDataObject MessageDataObject => (DCGResponseMessageDataObject)base.MessageDataObject;

		public override ZString GetMessageInterpretation()
		{
			var frCultureInfo = CultureInfo.GetCultureInfo("fr-FR");
			var stringBuilder = new ZStringBuilder();
			if (ResponseMessage != null)
			{
				stringBuilder.Append(ToH1IfNotEmpty("DCG Status"));
				stringBuilder.Append(NewLine);
				stringBuilder.Append(HTMLTBoldString(Invariant($"Reference: ")) + Invariant($"{MessageDataObject.EntryNum} / {MessageDataObject.DCGReference}"));
				stringBuilder.Append(NewLine);
				stringBuilder.Append(HTMLTBoldString(Invariant($"Period: ")) + Invariant($"{MessageDataObject.DateFrom} - {MessageDataObject.DateTo}"));

				if (MessageDataObject.HasTaxes)
				{
					stringBuilder.Append(NewLine);
					stringBuilder.Append(NewLine);
					stringBuilder.Append(HTMLTBoldString("Taxes:"));

					foreach (var tax in MessageDataObject.Taxes)
					{
						stringBuilder.Append(NewLine);
						stringBuilder.Append($"{Tab}{tax.Codtax} / {tax.Codtaxeeu} = {tax.Montanttax.ToString("C", frCultureInfo)}");
					}

					stringBuilder.Append(NewLine);
					stringBuilder.Append(NewLine);
					stringBuilder.Append(HTMLTBoldString("Entries:"));

					foreach (var entry in MessageDataObject.Entries)
					{
						stringBuilder.Append(NewLine);
						stringBuilder.Append(HTMLColourString(Invariant($"{Tab}{entry.Refdec} / {entry.Refdos}"), Green));
					}
				}

				if (MessageDataObject.HasAnomalies)
				{
					stringBuilder.Append(NewLine);
					stringBuilder.Append(NewLine);
					stringBuilder.Append(HTMLTBoldString("Anomalies:"));

					foreach (var anomalie in MessageDataObject.Anomalies)
					{
						stringBuilder.Append(NewLine);
						stringBuilder.Append(HTMLColourString(Invariant($"{Tab}{anomalie.Anomaliecode} : {anomalie.Anomaliedescr}"), Orange));
					}
				}

				if (MessageDataObject.HasErrors)
				{
					stringBuilder.Append(NewLine);
					stringBuilder.Append(NewLine);
					stringBuilder.Append(HTMLTBoldString("Errors:"));

					foreach (var error in MessageDataObject.Errors)
					{
						stringBuilder.Append(NewLine);
						stringBuilder.Append(HTMLColourString(Invariant($"{Tab}{error.ErreurCode} : {error.ErreurDescription}"), Red));
					}
				}
			}

			return stringBuilder.ToString();
		}

		ZString HTMLColourString(ZString text, ZString colour)
		{
			return Invariant($@"<font color=""{colour}"">{text}</font>");
		}

		ZString HTMLTBoldString(ZString text)
		{
			return Invariant($"<b>{text}</b>");
		}

		const string Red = "red";
		const string Green = "green";
		const string Orange = "orange";
		const string Tab = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
		const string NewLine = "<br />";
	}
}
