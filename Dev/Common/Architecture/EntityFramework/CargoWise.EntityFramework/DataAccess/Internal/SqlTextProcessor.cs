using System.Text.RegularExpressions;

namespace CargoWise.EntityFramework
{
	class SqlTextProcessor
	{
		public static bool HasLike(string sqlText)
		{
			if (sqlText.IndexOf("like", System.StringComparison.OrdinalIgnoreCase) < 0)
			{
				return false;
			}
			else
			{
				return LikeRegex.IsMatch(sqlText);
			}
		}

		static Regex LikeRegex
		{
			get
			{
				if (likeRegex == null)
				{
					likeRegex = new Regex(@"\slike\s+", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
				}

				return likeRegex;
			}
		}

		static Regex likeRegex;
	}
}
