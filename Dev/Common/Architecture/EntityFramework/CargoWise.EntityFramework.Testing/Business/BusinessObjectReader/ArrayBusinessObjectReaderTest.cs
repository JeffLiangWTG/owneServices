namespace CargoWise.EntityFramework.Testing
{
	sealed class ArrayBusinessObjectReaderTest : TestCaseWithFactory
	{
		public void TestEnumeration()
		{
			AssertEquals("Precondition - there must be more than one dummy in the list to test enumerating.", true, Dummies.Count > 0);

			ArrayBusinessObjectReader reader = new ArrayBusinessObjectReader(Dummies);

			int dummiesEnumerated = 0;
			foreach (DummyBusinessObject dummy in reader)
			{
				dummiesEnumerated++;
				AssertNotNull(dummy);
			}

			AssertEquals("Should have enumerated " + dummiesEnumerated + " objects.", Dummies.Count, dummiesEnumerated);
		}

		public void TestApproxCount()
		{
			ArrayBusinessObjectReader reader = new ArrayBusinessObjectReader(Dummies);
			AssertEquals(2, reader.ApproximateCount);
		}

		public void TestHasRecords()
		{
			DummyBusinessObjectCollection dummyCollection = new DummyBusinessObjectCollection(Factory);
			ArrayBusinessObjectReader reader = new ArrayBusinessObjectReader(dummyCollection);
			AssertEquals(false, reader.HasRecords);

			dummyCollection = new DummyBusinessObjectCollection(Factory);
			dummyCollection.AddNew();
			reader = new ArrayBusinessObjectReader(dummyCollection);
			AssertEquals(true, reader.HasRecords);
		}

		public void TestBusinessObjectType()
		{
			AssertEquals(typeof(DummyBusinessObject), new ArrayBusinessObjectReader(Dummies).BusinessObjectType);
			AssertEquals(typeof(DummyBusinessObject), new ArrayBusinessObjectReader(new DummyBusinessObjectCollectionView(Dummies)).BusinessObjectType);
		}

		#region Test Classes

		class DummyBusinessObjectCollectionView : BusinessObjectCollectionView<DummyBusinessObject>
		{
			public DummyBusinessObjectCollectionView(BusinessObjectCollection collectionToFilter)
				: base(collectionToFilter)
			{
			}

			protected override bool IsThisPartOfTheCollection(BusinessObject element)
			{
				return false;
			}
		}

		#endregion

		#region Implementation

		DummyBusinessObjectCollection Dummies
		{
			get
			{
				if (fDummies == null)
				{
					fDummies = new DummyBusinessObjectCollection(Factory);
					fDummies.AddNew();
					fDummies.AddNew();
				}

				return fDummies;
			}
		}

		DummyBusinessObjectCollection fDummies;

		#endregion
	}
}
