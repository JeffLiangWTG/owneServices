using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.Testing
{
	public class RateCodeDescriptionPairListTest : TestCase
	{
		public void TestInsertInSortOrder()
		{
			var list = new RateCodeDescriptionPairList();
			list.AddPair("333", "333");
			list.AddPair("111", "111");
			list.AddPair("GHI", "GHI");
			list.AddPair("ABC", "ABC");
			list.Sort();
			list.InsertInSortOrder(new CodeDescriptionPair("222", "222"));
			list.InsertInSortOrder(new CodeDescriptionPair("DEF", "DEF"));
			AssertEquals("ABC", list[0].Code);
			AssertEquals("DEF", list[1].Code);
			AssertEquals("GHI", list[2].Code);
			AssertEquals("111", list[3].Code);
			AssertEquals("222", list[4].Code);
			AssertEquals("333", list[5].Code);
		}

		public void TestSort()
		{
			var list = new RateCodeDescriptionPairList();
			list.AddPair("111", "111");
			list.AddPair("333", "333");
			list.AddPair("DEF", "DEF");
			list.AddPair("ABC", "ABC");
			list.AddPair("222", "222");
			list.Sort();
			AssertEquals("ABC", list[0].Code);
			AssertEquals("DEF", list[1].Code);
			AssertEquals("111", list[2].Code);
			AssertEquals("222", list[3].Code);
			AssertEquals("333", list[4].Code);
		}
	}
}
