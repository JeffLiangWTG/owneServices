using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class StringWithLanguageTestCase : TestCase
	{
		public void TestStringWithLanguage()
		{
			List<StringWithLanguage> testList = new List<StringWithLanguage>();
			testList.Add(new StringWithLanguage("test name", "ENG"));

			Assert("Contains", testList.Contains(new StringWithLanguage("test name", "ENG")));
			Assert("Not Contains", !testList.Contains(new StringWithLanguage("test name", "TST")));
			AssertEquals("Value", "test name", testList[0].Value);
			AssertEquals("LanguageCode", "ENG", testList[0].LanguageCode);
			Assert("IsEmpty", !testList[0].IsEmpty);
			AssertEquals("Empty Value", "", StringWithLanguage.Empty.Value);
		}
	}
}
