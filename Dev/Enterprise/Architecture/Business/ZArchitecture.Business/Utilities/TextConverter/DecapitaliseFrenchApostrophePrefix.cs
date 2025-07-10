using System.Text.RegularExpressions;

namespace Enterprise.ZArchitecture.Business.Utilities
{
	public class DecapitaliseFrenchApostrophePrefix : IWordConverter
	{
		public string Convert(string word)
		{
			return frenchApostrophePrefixRegex.Replace(word, match => match.Groups[1].Value.ToLower() + match.Groups[2].Value);
		}

		static readonly Regex frenchApostrophePrefixRegex = new Regex(@"^([LD])('[a-zA-Z]+)");
	}
}
