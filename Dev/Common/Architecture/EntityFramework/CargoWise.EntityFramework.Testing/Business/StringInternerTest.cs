using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	sealed class StringInternerTest : NUnit.Framework.TestCase
	{
		public void TestStringInterning()
		{
			StringInterner interner = new StringInterner();

			string string1 = string.Format("my {0} string", "test");
			string string2 = string.Format("my {0} string", "test");
			Assert(!ReferenceEquals(string1, string2));

			string result1 = interner.InternValue(string1);
			string result2 = interner.InternValue(string2);

			Assert(ReferenceEquals(result1, result2));
		}

		public void TestNullStringInterning()
		{
			StringInterner interner = new StringInterner();

			string result1 = interner.InternValue(null);

			AssertNull(result1);
		}

		public void TestZStringInterning()
		{
			StringInterner interner = new StringInterner();

			ZString string3 = new ZString(string.Format("my {0} string", "test"));
			ZString string4 = new ZString(string.Format("my {0} string", "test"));

			Assert(!ReferenceEquals(string3.ToString(), string4.ToString()));

			ZString result3 = interner.InternValue(string3);
			ZString result4 = interner.InternValue(string4);

			Assert(ReferenceEquals(result3.ToString(), result4.ToString()));
		}

		public void TestNullZStringInterning()
		{
			StringInterner interner = new StringInterner();

			ZString result1 = interner.InternValue((ZString)null);

			Assert(ReferenceEquals(ZString.Empty.ToString(), result1.ToString()));
		}
	}
}
