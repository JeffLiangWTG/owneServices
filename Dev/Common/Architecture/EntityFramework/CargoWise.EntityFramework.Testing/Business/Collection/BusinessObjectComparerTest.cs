namespace CargoWise.EntityFramework.Testing
{
	sealed class BusinessObjectComparerTest : TestCaseWithFactory
	{
		public void TestEquals()
		{
			BusinessObject bizO1 = DummyBusinessObject.New(Factory);
			BusinessObject bizO2 = DummyBusinessObject.New(Factory);

			Assert(BusinessObjectComparer.Instance.Equals(bizO1, bizO1));
			Assert(!BusinessObjectComparer.Instance.Equals(bizO1, bizO2));

			BusinessObject bizO3 = DummyBusinessObject.New(Factory);
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			BusinessObject bizO4 = DummyBusinessObject.New(newFactory);

			Assert(!BusinessObjectComparer.Instance.Equals(bizO3, bizO4));
		}

		public void TestGetHashCode()
		{
			BusinessObject bizO1 = DummyBusinessObject.New(Factory);
			BusinessObject bizO2 = DummyBusinessObject.New(Factory);

			AssertEquals(bizO1.GetHashCode(), BusinessObjectComparer.Instance.GetHashCode(bizO1));
			AssertEquals(bizO2.GetHashCode(), BusinessObjectComparer.Instance.GetHashCode(bizO2));
			AssertNotEquals(bizO1.GetHashCode(), BusinessObjectComparer.Instance.GetHashCode(bizO2));
		}
	}
}
