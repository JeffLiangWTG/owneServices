namespace CargoWise.EntityFramework.Testing
{
	sealed class BusinessObjectEqualityComparerTest : TestCaseWithFactory
	{
		public void TestPKOnlyComparer()
		{
			SetupDummies();
			Comparer = BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer;

			AssertEqual("null && null", null, null);
			AssertNotEqual("Dummy && null", Dummy, null);
			AssertEqual("Dummy && Dummy", Dummy, Dummy);
			AssertNotEqual("Dummy && AnotherDummy", Dummy, AnotherDummy);
			AssertEqual("Dummy && DummyAsDifferentType", Dummy, DummyAsDifferentType);
			AssertEqual("Dummy && DummyInAnotherFactory", Dummy, DummyInAnotherFactory);
		}

		public void TestIgnoreFactoryComparer()
		{
			SetupDummies();
			Comparer = BusinessObjectEqualityComparer<BusinessObject>.IgnoreFactoryComparer;

			AssertEqual("null && null", null, null);
			AssertNotEqual("Dummy && null", Dummy, null);
			AssertEqual("Dummy && Dummy", Dummy, Dummy);
			AssertNotEqual("Dummy && AnotherDummy", Dummy, AnotherDummy);
			AssertNotEqual("Dummy && DummyAsDifferentType", Dummy, DummyAsDifferentType);
			AssertEqual("Dummy && DummyInAnotherFactory", Dummy, DummyInAnotherFactory);
		}

		public void TestInstanceComparer()
		{
			SetupDummies();
			Comparer = BusinessObjectEqualityComparer<BusinessObject>.InstanceComparer;

			AssertEqual("null && null", null, null);
			AssertNotEqual("Dummy && null", Dummy, null);
			AssertEqual("Dummy && Dummy", Dummy, Dummy);
			AssertNotEqual("Dummy && AnotherDummy", Dummy, AnotherDummy);
			AssertNotEqual("Dummy && DummyAsDifferentType", Dummy, DummyAsDifferentType);
			AssertNotEqual("Dummy && DummyInAnotherFactory", Dummy, DummyInAnotherFactory);
		}

		#region Implementation

		void AssertEqual(string message, BusinessObject bizo1, BusinessObject bizo2)
		{
			AssertEquals(message + ": compare hashes", Comparer.GetHashCode(bizo1), Comparer.GetHashCode(bizo2));
			AssertEquals(message + ": bizo1 == bizo2", true, Comparer.Equals(bizo1, bizo2));
			AssertEquals(message + ": bizo2 == bizo1", true, Comparer.Equals(bizo2, bizo1));
		}

		void AssertNotEqual(string message, BusinessObject bizo1, BusinessObject bizo2)
		{
			AssertEquals(message + ": bizo1 != bizo2", false, Comparer.Equals(bizo1, bizo2));
			AssertEquals(message + ": bizo2 != bizo1", false, Comparer.Equals(bizo2, bizo1));
		}

		void SetupDummies()
		{
			Dummy = Factory.New<DummyBusinessObject>();
			AnotherDummy = Factory.New<DummyBusinessObject>();
			DummyAsDifferentType = Factory.Load<DummyChildBusinessObject>(Dummy.PK);
			Factory.Save();

			BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
			DummyInAnotherFactory = anotherFactory.Load<DummyBusinessObject>(Dummy.PK);
		}

		BusinessObject Dummy;
		BusinessObject AnotherDummy;
		BusinessObject DummyAsDifferentType;
		BusinessObject DummyInAnotherFactory;
		BusinessObjectEqualityComparer<BusinessObject> Comparer;

		#endregion
	}
}
