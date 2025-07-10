using System.Text.RegularExpressions;

namespace Enterprise.ZArchitecture.Business.Utilities
{
	public class CapitaliseApostropheSuffix : IWordConverter
	{
		public CapitaliseApostropheSuffix(int minSuffixLengthBeforeCapitalise)
		{
			this.apostropheSuffixRegex = new Regex(@"([a-zA-Z]+')([a-z])([a-z]{" + (minSuffixLengthBeforeCapitalise - 1) + ",})");
		}

		public string Convert(string word)
		{
			return apostropheSuffixRegex.Replace(word, match => match.Groups[1].Value + match.Groups[2].Value.ToUpper() + match.Groups[3].Value);
		}

		readonly Regex apostropheSuffixRegex;
	}
}
