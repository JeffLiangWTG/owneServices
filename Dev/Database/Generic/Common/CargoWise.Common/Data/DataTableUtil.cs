using System.Diagnostics.CodeAnalysis;

namespace CargoWise.Common
{
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Util")]
	public static class DataTableUtil
	{
		[SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Tokenised")]
		public static string GetRowValueAsTokenisedAdoFilterStringValue_ForLikeStartsWith(string startsWith)
		{
			Argument.NotNull(startsWith, nameof(startsWith)); // Suggested By ReviewBot 
			string result = TokeniseStringForLike(startsWith);
			return "'" + result + "%'";
		}

		static string TokeniseStringForLike(string str)
		{
			Argument.NotNull(str, nameof(str)); // Suggested By ReviewBot 
			string result = str;
			result = result.Replace("'", "''");
			result = result.Replace("[", "[[]");
			result = result.Replace("]", "[]]");
			result = result.Replace("[[[]]", "[[]");
			result = result.Replace("*", "[*]");
			result = result.Replace("%", "[%]");
			return result;
		}
	}
}
