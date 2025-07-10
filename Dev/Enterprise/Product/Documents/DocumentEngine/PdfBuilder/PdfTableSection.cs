using System.Collections.Generic;
using System.Linq;

namespace Enterprise.DocumentEngine.PdfBuilder
{
	public enum ColumnAlignment
	{
		Left = 0,
		Right = 1,
		Center = 2,
	}

	public class PdfTableSection : PdfElement
	{
		public List<string> Headers { get; private set; } = new List<string>();

		public List<int> Widths { get; private set; } = new List<int>();

		public List<ColumnAlignment> Alignments { get; private set; } = new List<ColumnAlignment>();

		public List<string[]> Rows { get; private set; } = new List<string[]>();

		public override bool Equals(object obj)
		{
			if (obj is PdfTableSection other)
			{
				if (Rows.Count != other.Rows.Count)
				{
					return false;
				}

				var objRows = other.Rows;
				for (var i = 0; i < Rows.Count; i++)
				{
					if (!Rows[i].SequenceEqual(objRows[i]))
					{
						return false;
					}
				}

				return Headers.SequenceEqual(other.Headers)
						&& Widths.SequenceEqual(other.Widths)
						&& (Alignments.SequenceEqual(other.Alignments))
						&& base.Equals(obj);
			}

			return false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Required because Equals is overridden")]
		public override int GetHashCode() => base.GetHashCode();
	}
}
