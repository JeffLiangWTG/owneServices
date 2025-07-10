using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class DictionaryExtensionTest : TestCaseWithFactory
	{
		public void TestAddIfNotExists()
		{
			var dict = new Dictionary<string, decimal>();
			CombineAssertions(() =>
			{
				AssertEquals("The dictionary is empty", 0, dict.Count);

				dict.AddIfNotExists("key", 1);
				AssertEquals("The dictionary has a new key/value pair", 1, dict.Count);

				dict.AddIfNotExists("key2", 2);
				AssertEquals("The dictionary has a new key/value pair", 2, dict.Count);

				dict.AddIfNotExists("key", 3);
				AssertEquals("The dictionary has a no new key/value pair because the key is already in the dictionay", 2, dict.Count);
			});
		}
	}
}
