using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.PdfBuilder
{
	public class PdfElement
	{
		public int MarginLeft { get; set; } = 10;

		public int MarginTop { get; set; } = 10;

		public bool IsCentered { get; set; }

		public string FontName { get; set; } = (NoResString)"Times New Roman";

		public int FontHeight { get; set; } = 10;

		public int LineHeight { get; set; } = 14;

		public int Padding { get; set; } = 2;

		public override bool Equals(object obj)
		{
			if (obj is PdfElement other)
			{
				return MarginLeft == other.MarginLeft
						&& MarginTop == other.MarginTop
						&& IsCentered == other.IsCentered
						&& FontName == other.FontName
						&& FontHeight == other.FontHeight
						&& LineHeight == other.LineHeight
						&& Padding == other.Padding;
			}

			return base.Equals(obj);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Required because Equals is overridden")]
		public override int GetHashCode() => base.GetHashCode();
	}
}
