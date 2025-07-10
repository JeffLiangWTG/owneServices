using System.Text.RegularExpressions;

namespace Enterprise.ZArchitecture.Business.Utilities
{
	public class DecapitaliseNumberSuffix : IWordConverter
	{
		public string Convert(string word)
		{
			return numberSuffixRegex.Replace(word, match => match.Groups[1].Value + match.Groups[2].Value.ToLower());
		}

		static readonly Regex numberSuffixRegex = new Regex(@"([0-9]+)([a-zA-Z]+)");
	}
}
