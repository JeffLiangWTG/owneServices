using CargoWise.Types;
using NUnit.Framework;
namespace Enterprise.Client.YAS.Testing
{
	public class YasExtensionsTest : TestCase
	{
		public void TestMatch()
		{
			AssertMatch("*ABC39%", "blahblahabc39%", true);
			AssertMatch("*ABC39b", "ABC39", false);
			AssertMatch("9%*[ABC]*", "9%HOHO[abc]aaaa", true);
			AssertMatch("|ABC|*|111|[ABC=T]*000", "ABC|48483|111|[ABC=T]111000", false);
			AssertMatch("|ABC|*|111|[ABC=^T]*000", "|ABC|48483|111|[ABC=^T]111000", true);
		}

		void AssertMatch(ZString searchPattern, ZString matchingStr, bool shouldMatch)
		{
			AssertEquals(ZString.Format("{0} should match search pattern {1}.)", matchingStr, searchPattern), shouldMatch, YASExtentions.Match(searchPattern, matchingStr));
		}
	}
}
