using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CargoWise.Common
{
	public static class Diff
	{
		public static Info<string> CompareByTokens(string x, string y)
		{
			Argument.NotNull(x, nameof(x)); // Suggested By ReviewBot 
			Argument.NotNull(y, nameof(y)); // Suggested By ReviewBot 
			return Compare(Tokenize(x), Tokenize(y));
		}

		// This is the classic Longest Common Subsequence solution by dynamic programing
		// See http://en.wikipedia.org/wiki/Longest_common_subsequence_problem
		public static Info<T> Compare<T>(T[] x, T[] y)
		{
			Argument.NotNull(x, nameof(x)); // Suggested By ReviewBot 
			Argument.NotNull(y, nameof(y)); // Suggested By ReviewBot 
			int[][] c = new int[x.Length + 1][];
			c[0] = new int[y.Length + 1];

			for (int i = 1; i <= x.Length; i++)
			{
				c[i] = new int[y.Length + 1];
				for (int j = 1; j <= y.Length; j++)
				{
					c[i][j] =
						x[i - 1].Equals(y[j - 1]) ?
						c[i - 1][j - 1] + 1 :
						Math.Max(c[i][j - 1], c[i - 1][j]);
				}
			}

			Info<T> info = new Info<T>(x, y, c[x.Length][y.Length]);
			SetChanged(c, ref info);
			return info;
		}

		static void SetChanged<T>(int[][] c, ref Info<T> info)
		{
			Argument.NotNull(c, nameof(c));
			Argument.NotNull(info.x, nameof(info.x));
			Argument.NotNull(info.y, nameof(info.y));

			int i = info.x.Length;
			int j = info.y.Length;
			while (true)
			{
				if (i > 0 && j > 0 && info.x[i - 1].Equals(info.y[j - 1]))
				{
					i--;
					j--;
				}
				else if (j > 0 && (i == 0 || c[i][j - 1] >= c[i - 1][j]))
				{
					info.yChanged[j - 1] = true;
					j--;
				}
				else if (i > 0 && (j == 0 || c[i][j - 1] < c[i - 1][j]))
				{
					info.xChanged[i - 1] = true;
					i--;
				}
				else
				{
					break;
				}
			}
		}

		public struct Info<T>
		{
			public Info(T[] x, T[] y, int lcsLength)
				: this(x, new bool[x.Length], y, new bool[y.Length], lcsLength)
			{
				Argument.NotNull(x, nameof(x)); // Suggested By ReviewBot 
				Argument.NotNull(y, nameof(y)); // Suggested By ReviewBot 
			}

			internal Info(T[] x, bool[] xChanged, T[] y, bool[] yChanged, int lcsLength)
			{
				Argument.NotNull(x, nameof(x)); // Suggested By ReviewBot 
				Argument.NotNull(y, nameof(y)); // Suggested By ReviewBot 
				Argument.NotNull(xChanged, nameof(xChanged));
				Argument.NotNull(yChanged, nameof(yChanged));
				this.x = x;
				this.y = y;
				this.xChanged = xChanged;
				this.yChanged = yChanged;
				this.lcsLength = lcsLength;
			}
			public readonly T[] x;
			public readonly bool[] xChanged;
			public readonly T[] y;
			public readonly bool[] yChanged;
			public readonly int lcsLength;
		}

		public static Info<string> Simplify(Info<string> diff)
		{
			Argument.NotNull(diff.xChanged, nameof(diff.xChanged));
			Argument.NotNull(diff.yChanged, nameof(diff.yChanged));
			Argument.NotNull(diff.x, nameof(diff.x)); // Suggested By ReviewBot 
			Argument.NotNull(diff.y, nameof(diff.y)); // Suggested By ReviewBot 
			if (!((1 < diff.x.Length || 1 >= diff.y.Length) || diff.y.Length <= diff.yChanged.Length))
			{
				throw new ArgumentException("Invalid argument.", nameof(diff));
			}

			if (!(1 >= diff.x.Length || diff.x.Length <= diff.xChanged.Length))
			{
				throw new ArgumentException("Invalid argument.", nameof(diff));
			}

			if (!(1 >= diff.y.Length || diff.y.Length <= diff.yChanged.Length))
			{
				throw new ArgumentException("Invalid argument.", nameof(diff));
			}

			string[] simpleX;
			string[] simpleY;
			bool[] simpleXChanged;
			bool[] simpleYChanged;
			Simplify(diff.x, diff.xChanged, out simpleX, out simpleXChanged);
			Simplify(diff.y, diff.yChanged, out simpleY, out simpleYChanged);
			return new Info<string>(simpleX, simpleXChanged, simpleY, simpleYChanged, diff.lcsLength);
		}

		static void Simplify(string[] v, bool[] vChanged, out string[] simpleV, out bool[] simpleVChanged)
		{
			Argument.NotNull(v, nameof(v)); // Suggested By ReviewBot 
			Argument.NotNull(vChanged, nameof(vChanged)); // Suggested By ReviewBot 
			if (v.Length == 0)
			{
				simpleV = v;
				simpleVChanged = vChanged;
			}
			else
			{
				List<string> newV = new List<string>();
				List<bool> newVChanged = new List<bool>();
				newV.Add(v[0]);
				if (vChanged.Length > 0)
				{
					newVChanged.Add(vChanged[0]);
				}
				for (int i = 1; i < v.Length; i++)
				{
					if (newVChanged[newVChanged.Count - 1] == vChanged[i])
					{
						newV[newV.Count - 1] += v[i];
					}
					else
					{
						newV.Add(v[i]);
						newVChanged.Add(vChanged[i]);
					}
				}
				simpleV = newV.ToArray();
				simpleVChanged = newVChanged.ToArray();
			}
		}
#pragma warning disable CA1502
		public static string[] Tokenize(string str)
		{
			Argument.NotNull(str, nameof(str)); // Suggested By ReviewBot 
			List<string> tokens = new List<string>();
			StringBuilder current = null;
			bool currentIsNumber = false;
			bool currentIsWord = false;

			for (int i = 0; i < str.Length; i++)
			{
				switch (char.GetUnicodeCategory(str, i))
				{
					// IsPunctuation
					case UnicodeCategory.ClosePunctuation:
					case UnicodeCategory.ConnectorPunctuation:
					case UnicodeCategory.DashPunctuation:
					case UnicodeCategory.OpenPunctuation:
					case UnicodeCategory.InitialQuotePunctuation:
					case UnicodeCategory.FinalQuotePunctuation:
					case UnicodeCategory.OtherPunctuation:

					// IsSymbole
					case UnicodeCategory.MathSymbol:
					case UnicodeCategory.CurrencySymbol:
					case UnicodeCategory.ModifierSymbol:
					case UnicodeCategory.OtherSymbol:

					// Non-Token characters
					case UnicodeCategory.Control:
					case UnicodeCategory.EnclosingMark:
					case UnicodeCategory.Format:
					case UnicodeCategory.LineSeparator:
					case UnicodeCategory.OtherNotAssigned:
					case UnicodeCategory.ParagraphSeparator:
					case UnicodeCategory.PrivateUse:
					case UnicodeCategory.SpaceSeparator:
					case UnicodeCategory.SpacingCombiningMark:

					// ??
					case UnicodeCategory.NonSpacingMark:
					case UnicodeCategory.Surrogate:
						if (current != null)
						{
							tokens.Add(current.ToString());
							current = null;
						}
						tokens.Add(str[i].ToString());
						break;

					// IsNumber
					case UnicodeCategory.DecimalDigitNumber:
					case UnicodeCategory.LetterNumber:
					case UnicodeCategory.OtherNumber:
						if (current != null && currentIsNumber)
						{
							current.Append(str[i]);
						}
						else
						{
							if (current != null)
							{
								tokens.Add(current.ToString());
							}
							current = new StringBuilder();
							current.Append(str[i]);
							currentIsNumber = true;
							currentIsWord = false;
						}
						break;

					// IsLetter
					case UnicodeCategory.LowercaseLetter:
					case UnicodeCategory.ModifierLetter:
					case UnicodeCategory.TitlecaseLetter:
					case UnicodeCategory.UppercaseLetter:
					case UnicodeCategory.OtherLetter:
						if (current != null && currentIsWord)
						{
							current.Append(str[i]);
						}
						else
						{
							if (current != null)
							{
								tokens.Add(current.ToString());
							}
							current = new StringBuilder();
							current.Append(str[i]);
							currentIsWord = !str[i].IsCjk();
							currentIsNumber = false;
						}
						break;
				}
			}

			if (current != null)
			{
				tokens.Add(current.ToString());
				current = null;
			}

			return tokens.ToArray();
		}
#pragma warning restore CA1502
	}
}
