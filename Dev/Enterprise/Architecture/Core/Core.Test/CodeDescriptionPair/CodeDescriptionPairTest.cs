using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public class CodeDescriptionPairTest : TestCase
	{
		public CodeDescriptionPairTest()
			: base()
		{
		}

		public void TestCodeAndDescriptionAreDifferent()
		{
			CodeDescriptionPair pair = new CodeDescriptionPair("TEST", "test3");
			AssertEquals("Different code and description", "TEST - test3", pair.CodeAndDescription);
		}

		[ExpectException(typeof(InvalidCodeDescriptionPairException))]
		public void TestWhenCodeIsGuid()
		{
			Guid code = new Guid();
			string description = "test3";
			CodeDescriptionPair pair = new CodeDescriptionPair(code, description);
		}

		public void TestEquality()
		{
			CodeDescriptionPair pair = new CodeDescriptionPair("FOO", "bar");
			CodeDescriptionPair pair2 = new CodeDescriptionPair("FOO", "bar");
			CodeDescriptionPair pair3 = new CodeDescriptionPair("FOO", "harbl");
			CodeDescriptionPair pair4 = new CodeDescriptionPair("FOOSH", "bar");

			AssertEquals(pair, pair);
			AssertEquals(pair, pair2);
			AssertNotEquals(pair, pair3);
			AssertNotEquals(pair, pair4);
			AssertNotEquals(pair3, pair4);

			AssertEquals(pair.GetHashCode(), pair2.GetHashCode());
			AssertEquals("FOO".GetHashCode() ^ "bar".GetHashCode(), pair.GetHashCode());
		}
	}
}
