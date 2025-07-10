using System.Text;

namespace CargoWise.Main.Extensions;

public static class StringExtensions
{
	public static string ReplaceWhitespaces(this string input, string replacement = "")
	{
		var sb = new StringBuilder();

		foreach (var c in input)
		{
			sb.Append(char.IsWhiteSpace(c) ? replacement : c);
		}

		return sb.ToString();
	}
}
