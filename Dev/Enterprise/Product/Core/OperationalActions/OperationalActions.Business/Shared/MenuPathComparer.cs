using System.Collections;
using System.Collections.Generic;
using System.Text;
using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class MenuPathComparer :
		IComparer<string>, IEqualityComparer<string>,
		IComparer<ZString>, IEqualityComparer<ZString>,
		IComparer, IEqualityComparer
	{
		public const char Separator = '/';
		public const char Space = ' ';

		public static string NormalisePath(string path)
		{
			StringBuilder builder = new StringBuilder(path.Length);

			foreach (char c in SignificantChars(path))
			{
				builder.Append(c);
			}

			return builder.ToString();
		}

		public static int ComparePath(string path1, string path2)
		{
			using (IEnumerator<char> chars1 = SignificantChars(path1).GetEnumerator())
			using (IEnumerator<char> chars2 = SignificantChars(path2).GetEnumerator())
			{
				do
				{
					bool hasChar1 = chars1.MoveNext();
					bool hasChar2 = chars2.MoveNext();

					if (!hasChar1 && !hasChar2)
					{
						return 0;
					}

					if (hasChar1 && !hasChar2)
					{
						return -1;
					}

					if (hasChar2 && !hasChar1)
					{
						return 1;
					}

					char char1 = (chars1.Current == Separator ? (char)0 : char.ToLower(chars1.Current));
					char char2 = (chars2.Current == Separator ? (char)0 : char.ToLower(chars2.Current));
					int diff = char1.CompareTo(char2);

					if (diff != 0)
					{
						return diff;
					}
				} while (true);
			}
		}

		public static bool Equals(string path1, string path2)
		{
			return ComparePath(path1, path2) == 0;
		}

		public static int HashCode(string path)
		{
			int result = 0x5a5a5a5a;

			foreach (char c in SignificantChars(path))
			{
				result = RotateLeft5(result) ^ char.ToLower(c);
			}

			return result;
		}

		#region IComparer<string> Members

		int IComparer<string>.Compare(string x, string y)
		{
			return ComparePath(x, y);
		}

		#endregion

		#region IEqualityComparer<string> Members

		bool IEqualityComparer<string>.Equals(string x, string y)
		{
			return Equals(x, y);
		}

		int IEqualityComparer<string>.GetHashCode(string obj)
		{
			return HashCode(obj);
		}

		#endregion

		#region IComparer<ZString> Members

		int IComparer<ZString>.Compare(ZString x, ZString y)
		{
			return ComparePath(x, y);
		}

		#endregion

		#region IEqualityComparer<ZString> Members

		bool IEqualityComparer<ZString>.Equals(ZString x, ZString y)
		{
			return Equals(x, y);
		}

		int IEqualityComparer<ZString>.GetHashCode(ZString obj)
		{
			return HashCode(obj);
		}

		#endregion

		#region IComparer Members

		int IComparer.Compare(object x, object y)
		{
			return ComparePath((string)x, (string)y);
		}

		#endregion

		#region IEqualityComparer Members

		bool IEqualityComparer.Equals(object x, object y)
		{
			return Equals((string)x, (string)y);
		}

		int IEqualityComparer.GetHashCode(object obj)
		{
			return HashCode((string)obj);
		}

		#endregion

		#region Implementation

		static int RotateLeft5(int value)
		{
			unchecked
			{
				uint val = (uint)value;
				return (int)((val << 5) | (val >> 27));
			}
		}

		static IEnumerable<char> SignificantChars(string path)
		{
			const int Start = 0;
			const int FoundContent = 1;
			const int FoundWhiteSpace = 2;
			const int FoundSeparator = 3;

			int state = Start;

			for (int i = 0; i < path.Length; i++)
			{
				char c = path[i];

				if (c == Separator)
				{
					if (state != Start)
					{
						state = FoundSeparator;
					}
				}
				else if (char.IsWhiteSpace(c))
				{
					if (state == FoundContent)
					{
						state = FoundWhiteSpace;
					}
				}
				else
				{
					if (state == FoundSeparator)
					{
						yield return Separator;
					}
					else if (state == FoundWhiteSpace)
					{
						yield return Space;
					}

					state = FoundContent;

					yield return c;
				}
			}
		}

		#endregion
	}
}
