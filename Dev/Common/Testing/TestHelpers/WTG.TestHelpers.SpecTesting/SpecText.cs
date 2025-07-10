using System.Linq;

namespace WTG.TestHelpers.SpecTesting
{
	public static class SpecText
	{
		public static string NormalizeSpecText(string text)
		{
			// Normalize windows line endings away
			text = text.Replace("\r\n", "\n");

			// Remove comment lines
			var nonCommentLines = text.Split('\n')
				.Where(line => !line.StartsWith("//") && !line.StartsWith("#"));
			text = string.Join("\n", nonCommentLines);

			// Normalize trailing/ending whitespace and newline characters
			text = text.Trim();

			// Normalize line gaps more than 1 line long
			while (text.Contains("\n\n\n"))
			{
				text = text.Replace("\n\n\n", "\n\n");
			}

			return text;
		}
	}
}
