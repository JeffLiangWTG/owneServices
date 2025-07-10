using System.Net;

namespace Enterprise.Customs.GB.Chief.CusRes
{
	class ErrorFromChiefHumanReadable
	{
		public string Line;
		public string ErrorText;
		public string Original;
		public string Box;

		public ErrorFromChiefHumanReadable(string line, string errorText, string original, string box)
		{
			Box = WebUtility.HtmlDecode(box);
			Line = WebUtility.HtmlDecode(line);
			ErrorText = WebUtility.HtmlDecode(errorText);
			Original = WebUtility.HtmlDecode(original);
		}
	}
}
