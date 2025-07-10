namespace Enterprise.Customs.AE.Manifest.Business;

sealed class FreeTextProvider : IFreeTextProvider
{
	public FreeTextProvider(string subjectCode, string text)
	{
		SubjectCode = subjectCode;
		Text = text;
	}

	public string SubjectCode { get; }

	public string Text { get; }
}
