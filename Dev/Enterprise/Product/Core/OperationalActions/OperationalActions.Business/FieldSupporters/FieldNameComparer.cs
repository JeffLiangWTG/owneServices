using System.Collections;
using System.Collections.Generic;

namespace Enterprise.Services.OperationalActions.Business
{
	sealed class FieldNameComparer : IComparer<string>, IEqualityComparer<string>, IComparer, IEqualityComparer
	{
		public static int Compare(string key1, string key2)
		{
			int i = 0;
			do
			{
				bool hasKey1 = i < key1.Length;
				bool hasKey2 = i < key2.Length;

				if (hasKey1)
				{
					if (hasKey2)
					{
						int diff = Translate(key1[i]) - Translate(key2[i]);
						if (diff != 0)
						{
							return diff;
						}
					}
					else
					{
						return 1;
					}
				}
				else
				{
					if (hasKey2)
					{
						return -1;
					}
					else
					{
						return 0;
					}
				}

				i++;
			}
			while (true);
		}

		public static int GetHashCode(string key)
		{
			int result = 0x5a5a5a5a;

			for (int i = 0; i < key.Length; i++)
			{
				result = RotateLeft5(result) ^ Translate(key[i]);
			}

			return result;
		}

		static char Translate(char c)
		{
			if (c == '+')
			{
				return '.';
			}
			else
			{
				return char.ToLower(c);
			}
		}

		static int RotateLeft5(int value)
		{
			unchecked
			{
				uint val = (uint)value;
				return (int)((val << 5) | (val >> 27));
			}
		}

		#region IComparer<string> Members

		int IComparer<string>.Compare(string x, string y)
		{
			return Compare(x, y);
		}

		#endregion

		#region IEqualityComparer<string> Members

		bool IEqualityComparer<string>.Equals(string x, string y)
		{
			return Compare(x, y) == 0;
		}

		int IEqualityComparer<string>.GetHashCode(string obj)
		{
			return GetHashCode(obj);
		}

		#endregion

		#region IComparer Members

		int IComparer.Compare(object x, object y)
		{
			return Compare((string)x, (string)y);
		}

		#endregion

		#region IEqualityComparer Members

		bool IEqualityComparer.Equals(object x, object y)
		{
			return Compare((string)x, (string)y) == 0;
		}

		int IEqualityComparer.GetHashCode(object obj)
		{
			return GetHashCode((string)obj);
		}

		#endregion
	}
}
