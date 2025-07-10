namespace CargoWise.EntityFramework.Testing
{
	sealed class UntypedHashtableKeyedByZPropertyInfoTest : TestCaseWithFactory
	{
		public void TestTryGetValueAndReturnValue()
		{
			DictionaryKeyedByZPropertyInfo<string> hash = new DictionaryKeyedByZPropertyInfo<string>();
			BusinessObject bizO1 = DummyBusinessObject.New(Factory);
			ZPropertyInfo info1 = new ZPropertyInfo(bizO1, "Info", null);
			string result;
			AssertEquals(false, hash.TryGetValue(info1, out result));

			BusinessObject bizO2 = DummyBusinessObject.New(Factory);
			ZPropertyInfo info2 = new ZPropertyInfo(bizO2, "Info", null);
			AssertEquals(false, hash.TryGetValue(info2, out result));
			hash[info2] = "X";
			AssertEquals(true, hash.TryGetValue(info2, out result));
			AssertEquals("X", result);
		}

		public void TestContainsKeyAndRemove()
		{
			DictionaryKeyedByZPropertyInfo<bool> hash = new DictionaryKeyedByZPropertyInfo<bool>();
			BusinessObject bizO1 = DummyBusinessObject.New(Factory);
			ZPropertyInfo info1 = new ZPropertyInfo(bizO1, "Info", null);
			AssertEquals(false, hash.ContainsKey(info1));

			hash[info1] = false;
			AssertEquals(true, hash.ContainsKey(info1));

			hash[info1] = true;
			AssertEquals(true, hash.ContainsKey(info1));

			hash.Remove(info1);
			AssertEquals(false, hash.ContainsKey(info1));

			BusinessObject bizO2 = DummyBusinessObject.New(Factory);
			ZPropertyInfo info2 = new ZPropertyInfo(bizO2, "Info", null);
			bool result;
			AssertEquals(false, hash.TryGetValue(info2, out result));
			hash[info2] = true;
			AssertEquals(true, hash.TryGetValue(info2, out result));
			AssertEquals(true, result);

			AssertEquals(false, hash.ContainsKey(info1));
			AssertEquals(true, hash.ContainsKey(info2));
		}
	}
}
