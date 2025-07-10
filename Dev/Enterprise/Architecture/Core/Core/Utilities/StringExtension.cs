using System;

namespace Enterprise.ZArchitecture.Core
{
	public static class StringExtension
	{
		public static int CountMatches(this string str, char value)
		{
			int count = 0;
			foreach (char c in str) // could be LINQ expression, but foreach is faster
			{
				if (c == value)
				{
					count++;
				}
			}

			return count;
		}

		public static int CountMatches(this string str, string value)
		{
			int count = 0;
			int start = 0;
			int match = 0;
			while (start < str.Length)
			{
				match = str.IndexOf(value, start);
				if (match != -1)
				{
					count++;
					start = match + value.Length;
				}
				else
				{
					break;
				}
			}

			return count;
		}

		public static bool Contains(this string source, string value, StringComparison comparison)
		{
			return source.IndexOf(value, comparison) >= 0;
		}
	}
}
