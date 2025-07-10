using FlexCel.Core;

namespace Enterprise.DocumentEngine.FlexCelInterface
{
	public struct ExcelHyperlink
	{
		public ExcelHyperlink(THyperLinkType type, string textToShow, string linkLocation, string targetFrame, string textMark, string tooltip)
		{
			this.Type = type;
			this.TextToShow = textToShow;
			this.LinkLocation = linkLocation;
			this.TargetFrame = targetFrame;
			this.TextMark = textMark;
			this.Tooltip = tooltip;
		}

		public ExcelHyperlink(THyperLinkType type, string linkLocation)
			: this(type, linkLocation, linkLocation, "", "", "")
		{
		}

		public readonly THyperLinkType Type;
		public readonly string TextToShow;
		public readonly string LinkLocation;
		public readonly string TargetFrame;
		public readonly string TextMark;
		public readonly string Tooltip;

		public override string ToString() => LinkLocation;
	}
}
