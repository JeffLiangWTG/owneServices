using CargoWiseOne.ResourceStrings;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1178:Use source generated static methods", Justification = "The test of the ResourceString")]
	public class MultilingualStringTest : TestCase
	{
		public void TestReplace()
		{
			using (var mockData = Res.UseMockData())
			{
				MultilingualString str = ResString.GetMultilingualString("0", "zoo bar").Replace("zoo", "goo");
				AssertEquals("goo bar", (string)str);

				mockData.Put("0", new ResourceStringData("", "zoo zoo"));
				AssertEquals("goo goo", (string)str);
			}
		}

		public void TestJoin()
		{
			using (var mockData = Res.UseMockData())
			{
				MultilingualString str = MultilingualString.Join(" ", ResString.GetMultilingualString("0", "zoo"), ResString.GetMultilingualString("1", "bar"));
				AssertEquals("zoo bar", (string)str);

				mockData.Put("0", new ResourceStringData("0", "goo"));
				mockData.Put("1", new ResourceStringData("1", "gar"));
				AssertEquals("goo gar", (string)str);
			}
		}

		public void TestEquals()
		{
			const string key = "e322a64b-009a-4c50-bd38-224129fb7733";
			using (var mockData = Res.UseMockData())
			{
				mockData.Put(key, new ResourceStringData(key, "Test"));

				var chsRes = Res.GetLanguageInstance(Enterprise.Core.SharedConstants.Languages.ChineseSimplified);
				using (var chsMockData = Res.GetLanguageInstance(Enterprise.Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
				{
					chsMockData.Put(key, new ResourceStringData(key, "测试"));
					var str = ResString.GetMultilingualString(key, "Test");

					Assert(str.Equals("Test"));
					Assert(!str.Equals("测试"));

					Assert(str.EqualsUnresolvedOrLocalized("Test", ignoreCase: false));
					Assert(!str.EqualsUnresolvedOrLocalized("测试", ignoreCase: false));

					Assert(str.EqualsUnresolvedOrLocalized("tEst", ignoreCase: true));
					Assert(!str.EqualsUnresolvedOrLocalized("测试", ignoreCase: true));

					using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.ChineseSimplified))
					{
						Assert(!str.Equals("Test"));
						Assert(str.Equals("测试"));

						Assert(str.EqualsUnresolvedOrLocalized("Test", ignoreCase: false));
						Assert(str.EqualsUnresolvedOrLocalized("测试", ignoreCase: false));

						Assert(str.EqualsUnresolvedOrLocalized("tEst", ignoreCase: true));
						Assert(str.EqualsUnresolvedOrLocalized("测试", ignoreCase: true));
					}
				}
			}
		}
	}
}
