namespace Enterprise.Customs.CH.Business;

public class ResponseParsingResult
{
	public string Description { get; set; }
	public string BodyText { get; set; }
	public byte[] BodyData { get; set; }

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	const string ContentTypeTextXml = "text/xml";
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	const string ContentTypeApplicationXml = "application/xml";
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	const string ContentTypeApplicationXopXml = "application/xop+xml";
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	const string ContentTypeApplicationPdf = "application/pdf";
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	const string ContentTypeApplicationJson = "application/json";

	public string Type
	{
		get => fType;
		set
		{
			fType = value;
			IsXml = fType.StartsWith(ContentTypeTextXml) || fType.StartsWith(ContentTypeApplicationXml) || fType.StartsWith(ContentTypeApplicationXopXml);
			IsPdf = fType.StartsWith(ContentTypeApplicationPdf);
			IsJson = fType.StartsWith(ContentTypeApplicationJson);
		}
	}
	string fType;

	public bool IsXml { get; set; }
	public bool IsPdf { get; set; }
	public bool IsJson { get; set; }
	public bool IsEmpty => string.IsNullOrEmpty(BodyText) && BodyData == null;
}
