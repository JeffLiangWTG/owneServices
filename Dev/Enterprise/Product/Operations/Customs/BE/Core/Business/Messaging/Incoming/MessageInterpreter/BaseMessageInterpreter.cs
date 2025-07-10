using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BE.Business;

public abstract class BaseMessageInterpreter<T> where T : class
{
	#region Constants

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "html interpretation.")]
	public const string HtmlTableHeader = "<table cellpadding=\"3\" cellspacing=\"0\" width=\"100%\" border=\"1\" style=\"font-size: 14px; border: 1px solid gray; border-collapse: collapse; font-family: Arial, sans-serif;\">";
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "html interpretation.")]
	public const string HtmlTableTail = "</table>";
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "html interpretation.")]
	public const string HtmlTableRow = @"<tr><td width=""{0}"">{1}</td><td>{2}</td></tr>";
	public const string HtmlLineChange = "<br />";

	#endregion

	public abstract string Interpret(T dataProvider, EDIMessage ediMessage);

	public string InterpretDiscardedMessage(T dataProvider, EDIMessage ediMessage)
	{
		var result = string.Empty;

		if (ediMessage != null && ediMessage.EM_Status == EDIMessage.Status.Discarded)
		{
			result = ediMessage.Notes.FindByDescription(Constants.MessageProcessingNotes.ProcessingLog).LastOrDefault()?.ST_NoteDataAsText;
		}

		return result;
	}

	protected void AddHtmlTableInterpretation(ZStringBuilder noteHtmlInterpretation, string keypxWidth, IEnumerable<KeyValuePair<string, string>> items)
	{
		noteHtmlInterpretation.Append(HtmlTableHeader);
		foreach (var item in items)
		{
			noteHtmlInterpretation.Append(string.Format(CultureInfo.InvariantCulture, HtmlTableRow, keypxWidth, item.Key, item.Value));
		}
		noteHtmlInterpretation.Append(HtmlTableTail);
		noteHtmlInterpretation.Append(HtmlLineChange);
	}

	protected void AddHTMLNoteTextLineInterpretation(ZStringBuilder noteHtmlInterpretation, string newlineNote)
	{
		noteHtmlInterpretation.Append(newlineNote);
		noteHtmlInterpretation.Append(HtmlLineChange);
	}
}
