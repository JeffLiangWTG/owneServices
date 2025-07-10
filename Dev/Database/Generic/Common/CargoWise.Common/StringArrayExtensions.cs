using System;
using System.Diagnostics.CodeAnalysis;

namespace CargoWise.Common
{
	public static class StringArrayExtensions
	{
		[SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "strings")]
		public static string[] OrderLongToShort(this string[] strings)
		{
			Argument.NotNull(strings, nameof(strings)); // Suggested By ReviewBot 
			var result = new string[strings.Length];

			strings.CopyTo(result, 0);
			Array.Sort(result, (lhs, rhs) => rhs.Length - lhs.Length);

			return result;
		}
	}
}
