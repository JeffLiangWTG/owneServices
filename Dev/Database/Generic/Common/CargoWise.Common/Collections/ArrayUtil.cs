using System;
using System.Diagnostics.CodeAnalysis;

namespace CargoWise.Common
{
	/// <summary>
	/// Utility methods for detailing with Array.
	/// </summary>
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Util")]
	public static class ArrayUtil
	{
		/// <summary>
		/// Do the two arrays equal, including their elements?
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs")]
		public static bool ArrayEquals<T>(T[] lhs, T[] rhs)
		{
			bool result = (lhs == null && rhs == null) || (lhs != null && rhs != null && lhs.Length == rhs.Length);
			if (result && lhs != null)
			{
				for (int i = 0; i < lhs.Length; i++)
				{
					if (!object.Equals(lhs[i], rhs[i]))
					{
						result = false;
						break;
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Combine two arrays togethers.
		/// </summary>
		public static T[] Combine<T>(T[] array1, params T[] array2)
		{
			Argument.NotNull(array1, nameof(array1));
			Argument.NotNull(array2, nameof(array2));

			T[] result = new T[array1.Length + array2.Length];
			array1.CopyTo(result, 0);
			array2.CopyTo(result, array1.Length);
			return result;
		}

		public static T[] Append<T>(T[] array, T value)
		{
			Argument.NotNull(array, nameof(array));

			var result = new T[array.Length + 1];
			Array.Copy(array, result, array.Length);
			result[array.Length] = value;
			return result;
		}
	}
}
