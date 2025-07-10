namespace CargoWise.EntityFramework.Testing
{
	sealed class TypedEnumeratorTest : TestCaseWithFactory
	{
		public void TestGetTypedEnumerator()
		{
			DummyBusinessObjectCollection coll = new DummyBusinessObjectCollection(Factory);
			DummyBusinessObject bizObj1 = coll.AddNew();
			DummyBusinessObject bizObj2 = coll.AddNew();

			using (TypedEnumerator<DummyBusinessObject> enumerator = new TypedEnumerator<DummyBusinessObject>(coll))
			{
				Assert(enumerator.MoveNext());
				AssertEquals(bizObj1, enumerator.Current);

				Assert(enumerator.MoveNext());
				AssertEquals(bizObj2, enumerator.Current);

				enumerator.Reset();
				Assert(enumerator.MoveNext());
				AssertEquals(bizObj1, enumerator.Current);
			}
		}
	}
}
