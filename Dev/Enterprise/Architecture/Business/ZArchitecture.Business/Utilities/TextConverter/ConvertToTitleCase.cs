using System.Globalization;

namespace Enterprise.ZArchitecture.Business.Utilities
{
	public class ConvertToTitleCase : IWordConverter
	{
		public string Convert(string word)
		{
			return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(word.ToLower());
		}
	}
}
