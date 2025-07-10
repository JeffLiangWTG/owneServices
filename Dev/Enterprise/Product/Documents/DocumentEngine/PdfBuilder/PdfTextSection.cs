using System.Collections.Generic;
using System.Linq;

namespace Enterprise.DocumentEngine.PdfBuilder
{
	public class PdfTextSection : PdfElement
	{
		public List<string> Texts { get; private set; } = new List<string>();

		public override bool Equals(object obj)
		{
			if (obj is PdfTextSection other)
			{
				return Texts.SequenceEqual(other.Texts) && base.Equals(obj);
			}

			return false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Required because Equals is overridden")]
		public override int GetHashCode() => base.GetHashCode();
	}
}
