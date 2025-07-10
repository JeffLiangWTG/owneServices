namespace Enterprise.ZArchitecture.Core
{
	public static class StringTrimmer
	{
		public static string TrimWithUnicodeWhitespace(this string stringToTrim)
		{
			return stringToTrim.Trim().Trim(new char[] { '\uFEFF', '\u200B' });
		}
	}
}
