using System;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class StringExtensionTest : NUnit.Framework.TestCase
	{
		public void TestCountChar()
		{
			string sourceStr = "abc.de.f.";
			AssertEquals(3, sourceStr.CountMatches('.'));

			sourceStr = ".abc.de.f";
			AssertEquals(3, sourceStr.CountMatches('.'));

			sourceStr = ".abc.abcdef.abc.";
			AssertEquals(4, sourceStr.CountMatches('.'));

			sourceStr = "\"hello\",\"you \"\"legend\"\"\"";
			AssertEquals(8, sourceStr.CountMatches('"'));
		}

		public void TestCountString()
		{
			string sourceStr = "abc.abb.abc";
			AssertEquals(2, sourceStr.CountMatches("abc"));

			sourceStr = ".abc.abb.abc.";
			AssertEquals(4, sourceStr.CountMatches("."));

			sourceStr = "fdjklfjdkaluriewaol;nflcam.jfekwlcmjfkela;mcjfkel";
			AssertEquals(3, sourceStr.CountMatches("jf"));

			sourceStr = "\"hello\",\"you \"\"legend\"\"\"";
			AssertEquals(8, sourceStr.CountMatches("\""));

			sourceStr = "aaaaaa";
			AssertEquals(3, sourceStr.CountMatches("aa"));

			sourceStr = "aaaaaaa";
			AssertEquals(3, sourceStr.CountMatches("aa"));
		}

		public void TestContains()
		{
			string sourceStr = "AbcdEFgH";

			Assert(sourceStr.Contains("abc", StringComparison.OrdinalIgnoreCase));
			Assert(sourceStr.Contains("Abc", StringComparison.OrdinalIgnoreCase));
			Assert(sourceStr.Contains("aBc", StringComparison.OrdinalIgnoreCase));

			Assert(!sourceStr.Contains("abc", StringComparison.Ordinal));
			Assert(sourceStr.Contains("Abc", StringComparison.Ordinal));
		}
	}
}
