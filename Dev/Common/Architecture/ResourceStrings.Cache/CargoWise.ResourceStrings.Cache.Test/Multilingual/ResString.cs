using System;
using CargoWiseOne.ResourceStrings;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	[TestedType(typeof(ResourceString))]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1178:Use source generated static methods", Justification = "The test of the ResourceString")]
	public class ResStringTest : ZMultilingualTest
	{
		public void TestResStringIsLocalizedReturnsTrueForEng()
		{
			string test = "Test";
			ResourceString res = ResString.GetMultilingualString("0", test);
			Assert("IsLocalized", res.IsLocalized("EN"));
		}

		public void TestResStringWithoutData()
		{
			using (Res.UseMockData())
			using (Res.GetLanguageInstance("DE-DE").UseMockData())
			{
				string str;
				string test = "Test";
				ResourceString res = ResString.GetMultilingualString("0", test);
				str = res;
				AssertEquals("Test", str);
				AssertEquals("Test", res.ToString());
				Assert("!IsLocalized", !res.IsLocalized("DE-DE"));
				AssertEquals("Test", res.ToString("DE-DE"));

				test = "Test {0}";
				res = ResString.GetMultilingualString("1", test, 2);
				str = res;
				AssertEquals("Test 2", str);
				AssertEquals("Test 2", res.ToString());
				Assert("!IsLocalized", !res.IsLocalized("DE-DE"));
				AssertEquals("Test 2", res.ToString("DE-DE"));

				test = "Test {0} {1}";
				res = ResString.GetMultilingualString("2", test, 3, "should pass");
				str = res;
				AssertEquals("Test 3 should pass", str);
				AssertEquals("Test 3 should pass", res.ToString());
				Assert("!IsLocalized", !res.IsLocalized("DE-DE"));
				AssertEquals("Test 3 should pass", res.ToString("DE-DE"));
			}
		}

		public void TestWithData()
		{
			using (var mockData = Res.UseMockData())
			using (var grmMockData = Res.GetLanguageInstance("DE-DE").UseMockData())
			{
				mockData.Put("0", new ResourceStringData("0", "Testing"));
				grmMockData.Put("1", new ResourceStringData("1", "Prufung {0}"));

				string str;
				string test = "Test";
				ResourceString res = ResString.GetMultilingualString("0", test);
				str = res;
				AssertEquals("Testing", str);
				AssertEquals("Testing", res.ToString());
				Assert("!IsLocalized", !res.IsLocalized("DE-DE"));

				string test0 = "Test {0}";
				res = ResString.GetMultilingualString("1", test0, 2);
				str = res;
				AssertEquals("Test 2", str);
				AssertEquals("Test 2", res.ToString());
				Assert("IsLocalized", res.IsLocalized("DE-DE"));
				AssertEquals("Prufung 2", res.ToString("DE-DE"));
			}
		}

		public void TestCompareTo()
		{
			string[] stringList = new string[] { "111", "AAA", "ZZZ", "bbb", "yyy" };

			using (var mockData = Res.UseMockData())
			{
				foreach (string a in stringList)
				{
					mockData.Put(a, new ResourceStringData(a, a));
				}

				foreach (string a in stringList)
				{
					foreach (string b in stringList)
					{
						ResourceString aRes = ResString.GetMultilingualString(a, a);
						ResourceString bRes = ResString.GetMultilingualString(b, b);
						AssertEquals(a, aRes.GetLocalizedValue(Res.CurrentLanguage));
						AssertEquals(b, bRes.GetLocalizedValue(Res.CurrentLanguage));
						AssertEquals(a + ".CompareTo(" + b + ")", a.CompareTo(b), aRes.CompareTo(bRes));
					}
				}
			}
		}

		public void TestWithRealData()
		{
			const string key = "0274bace-f9ad-46fb-8e3e-4d13858ecdba";
			UInt16 asmid;
			unchecked
			{
				asmid = ResTest.GetAsmid("Enterprise.ZArchitecture.GUI");
			}
			AssertEquals("Packs", ResString._GetMultilingualString(asmid, key, "Packs"));
			using (Res.TemporarilySwitchLanguage("DE-DE"))
			{
				AssertEquals("&Speichern", ResString._GetMultilingualString(asmid, key, "Packs"));
			}
		}

		protected override ZMultilingual GetZMultilingualConcrete()
		{
			return ResString.GetMultilingualString("key", "We are diabolically sober");
		}
	}
}
