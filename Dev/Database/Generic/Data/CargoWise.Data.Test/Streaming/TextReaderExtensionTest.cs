using System.IO;
using NUnit.Framework;

namespace CargoWise.IO.Testing
{
	public class TextReaderExtensionTest : TestCase
	{
		public void TestContainsTheSameDataAs()
		{
			AssertEquals(true, new StringReader("").ContainsTheSameDataAs(new StringReader("")));
			AssertEquals(false, new StringReader("").ContainsTheSameDataAs(new StringReader("a")));
			AssertEquals(false, new StringReader("a").ContainsTheSameDataAs(new StringReader("ab")));
			AssertEquals(true, new StringReader("abc").ContainsTheSameDataAs(new StringReader("abc")));
			AssertEquals(false, new StringReader("abc").ContainsTheSameDataAs(new StringReader("cba")));
			AssertEquals(false, new StringReader("abc").ContainsTheSameDataAs(new StringReader("azc")));
			AssertEquals(false, new StringReader("abc").ContainsTheSameDataAs(new StringReader("abd")));
		}
	}
}
