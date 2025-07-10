namespace Enterprise.ZArchitecture.Environment
{
	public enum DocumentTextType { Open, Close }

	public struct DocumentOpenCloseText
	{
		public DocumentOpenCloseText(string openingText, string closingText)
		{
			OpeningText = openingText;
			ClosingText = closingText;
		}

		public string Text(DocumentTextType textType)
		{
			return (textType == DocumentTextType.Open) ? OpeningText : ClosingText;
		}

		public readonly string OpeningText;
		public readonly string ClosingText;
	}
}
