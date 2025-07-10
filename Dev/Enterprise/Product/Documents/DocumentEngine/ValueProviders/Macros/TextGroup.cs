using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	public class TextGroup : ValueProvider
	{
		public override Regex Regex
		{
			get { return regex; }
		}

		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<TextGroup(\"{text}\",{length}[,\"{delimiter}\"[,{direction: L or R}]])>",
				ResString.GetMultilingualString("666f784c-d24c-4f2a-9aee-cd4682e5be33", @"Returns a string where characters have been grouped into readable chunks.
- {0}: The text to be chunked.
- {1}: The length of each chunk.
- {2}: One or more characters to insert between each chunk. If no {2} is specified, a space will be used.
- {3} is either {4} or {5}. This specifies which direction the text is grouped,. This is relevant if {0} cannot be split into a whole multiple of {1}. If no {3} is specified, text will be grouped from the left.",
"text", "length", "delimiter", "direction", "L", "R"),
				new List<(string example, object expectedResult)> {
					((NoResString)"<TextGroup(\"0001000200\",4)>", "0001 0002 00"),
					((NoResString)"<TextGroup(\"0001000200\",4,\"-\",L)>", "0001-0002-00"),
					((NoResString)"<TextGroup(\"0001000200\",4,\"-\",R)>", "00-0100-0200")
				});
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var result = string.Empty;
			var match = Regex.Match(macro);

			if (match.Success)
			{
				var text = match.Groups["text"].Value;
				if (!int.TryParse(match.Groups["length"].Value, out int length) || length <= 0)
				{
					ReportMacroError(report, Res.GetString("c6b64280-b77c-46b3-a11c-4755b29d0b83", "Invalid length parameters provided: {0}", macro));

					return string.Empty;
				}

				string delimiter = " ";
				if (match.Groups["delimiter"].Success)
				{
					delimiter = match.Groups["delimiter"].Value;
				}

				string direction = "L";
				if (match.Groups["direction"].Success)
				{
					direction = match.Groups["direction"].Value;
				}

				var output = new StringBuilder(text.Length + (text.Length / length + 1) * delimiter.Length);
				if (direction == "R")
				{
					for (int i = text.Length - 1; i >= 0; i--)
					{
						output.Insert(0, text[i]);
						if (i > 0 && ((text.Length - i) % length == 0))
						{
							output.Insert(0, delimiter);
						}
					}
				}
				else
				{
					for (int i = 0; i < text.Length; i++)
					{
						if (i > 0 && (i % length == 0))
						{
							output.Append(delimiter);
						}
						output.Append(text[i]);
					}
				}

				result = output.ToString();
			}

			return result;
		}

		static readonly Regex regex = new Regex(@"^<\s*TextGroup\s*\(\s*""(?<text>.*)"",\s*(?<length>\d+)(,\s*""(?<delimiter>[^""\n\r]*)"")?(,\s*(?<direction>[LR]))?\s*\)\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
