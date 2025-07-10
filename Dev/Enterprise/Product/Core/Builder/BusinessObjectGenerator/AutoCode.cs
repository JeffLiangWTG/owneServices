using System.Collections;
using System.Text;

namespace Enterprise.BusinessObjectGenerator
{
	public class AutoCode
	{
		public string LinesOfCode(params string[] lines)
		{
			return NewLineSeparatedText(lines);
		}

		protected string NewLineSeparatedText(IEnumerable text)
		{
			var result = new StringBuilder();
			var hasValue = false;
			foreach (string line in text)
			{
				if (line != null)
				{
					if (hasValue)
					{
						result.AppendLine();
					}
					result.Append(line);
					hasValue = true;
				}
			}

			return hasValue ? result.ToString() : null;
		}

		protected char[] Whitespace
		{
			get { return fWhitespace; }
		}

		readonly char[] fWhitespace = { '\r', '\n', ' ', '	' };
	}
}
