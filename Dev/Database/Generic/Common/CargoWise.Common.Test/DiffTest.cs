using System;
using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	class DiffTest : TestCase
	{
		public void TestDiff()
		{
			for (int xLength = 0; xLength < 10; xLength++)
			{
				for (int yLength = 0; yLength < 10; yLength++)
				{
					for (int sameStart = 0; sameStart <= Math.Min(xLength, yLength); sameStart++)
					{
						for (int sameEnd = sameStart + 1; sameEnd <= Math.Min(xLength, yLength) + 1; sameEnd++)
						{
							short[] x = new short[xLength];
							bool[] xChanged = new bool[xLength];
							short[] y = new short[yLength];
							bool[] yChanged = new bool[yLength];
							int i;
							for (i = 0; i < sameStart; i++)
							{
								x[i] = 1;
								y[i] = 2;
								xChanged[i] = yChanged[i] = true;
							}

							for (; i < sameEnd && i < Math.Min(xLength, yLength); i++)
							{
								x[i] = y[i] = 0;
								xChanged[i] = yChanged[i] = false;
							}

							for (int j = i; j < xLength; j++)
							{
								x[j] = 1;
								xChanged[j] = true;
							}

							for (int j = i; j < yLength; j++)
							{
								y[j] = 2;
								yChanged[j] = true;
							}

							Diff.Info<short> info = Diff.Compare(x, y);
							AssertEquals(Math.Min(sameEnd, Math.Min(xLength, yLength)) - sameStart, info.lcsLength);
							AssertArrayEqualsByElements("xChanged", xChanged, info.xChanged);
							AssertArrayEqualsByElements("yChanged", yChanged, info.yChanged);
						}
					}
				}
			}
		}

		public void TestCompareByTokens()
		{
			Diff.Info<string> info = Diff.CompareByTokens("This is a test", "This is the test.");
			AssertArrayEqualsByElements(new string[] { "This", " ", "is", " ", "a", " ", "test" }, info.x);
			AssertArrayEqualsByElements(new string[] { "This", " ", "is", " ", "the", " ", "test", "." }, info.y);
			AssertArrayEqualsByElements(new bool[] { false, false, false, false, true, false, false }, info.xChanged);
			AssertArrayEqualsByElements(new bool[] { false, false, false, false, true, false, false, true }, info.yChanged);
			info = Diff.Simplify(info);
			AssertArrayEqualsByElements(new string[] { "This is ", "a", " test" }, info.x);
			AssertArrayEqualsByElements(new string[] { "This is ", "the", " test", "." }, info.y);
			AssertArrayEqualsByElements(new bool[] { false, true, false, }, info.xChanged);
			AssertArrayEqualsByElements(new bool[] { false, true, false, true }, info.yChanged);
			info = Diff.CompareByTokens("魚與熊掌不可兼得", "蝶與魚不可兼得");
			AssertArrayEqualsByElements(new string[] { "魚", "與", "熊", "掌", "不", "可", "兼", "得" }, info.x);
			AssertArrayEqualsByElements(new string[] { "蝶", "與", "魚", "不", "可", "兼", "得" }, info.y);
			AssertArrayEqualsByElements(new bool[] { true, false, true, true, false, false, false, false }, info.xChanged);
			AssertArrayEqualsByElements(new bool[] { true, false, true, false, false, false, false }, info.yChanged);
		}
	}
}