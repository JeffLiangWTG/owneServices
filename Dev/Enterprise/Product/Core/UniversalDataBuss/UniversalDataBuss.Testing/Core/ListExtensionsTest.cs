using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.UniversalDataBuss.DataObjects.Core.Testing
{
	class ListExtensionsTest : TestCaseWithFactory
	{
		public void TestAddSafeItem()
		{
			List<string> value = null;
			AssertNull(value);
			value = value.AddSafe((string)null);
			AssertNull(value);

			value = value.AddSafe("Something");
			AssertEquals("Something", string.Join(", ", value));

			value = value.AddSafe((string)null);
			AssertEquals("Something", string.Join(", ", value));

			value = value
				.AddSafe("AWESOME!!")
				.AddSafe("Today.");
			AssertEquals("Something, AWESOME!!, Today.", string.Join(", ", value));
		}

		public void TestAddSafeArray()
		{
			List<string> value = null;
			AssertNull(value);
			value = value.AddSafe((IEnumerable<string>)null);
			AssertNull(value);

			string[] arrayToAdd = new[] { "One", "Two", "Three" };
			value = value.AddSafe(arrayToAdd);
			AssertEquals("One, Two, Three", string.Join(", ", value));

			value = value.AddSafe((IEnumerable<string>)null);
			AssertEquals("One, Two, Three", string.Join(", ", value));

			string[] arrayToAdd2 = new[] { "Four", "Five", "Six" };

			value = value
				.AddSafe(arrayToAdd2);
			AssertEquals("One, Two, Three, Four, Five, Six", string.Join(", ", value));
		}
	}
}
