namespace CargoWise.EntityFramework.Testing
{
	sealed class DeleteCheckerTest : TestCaseWithFactory
	{
		public void TestCheckPluginsCanDeleteIsIgnoredDuringDataRefreshDelete()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			Factory.Save();

			dummy.SetDeleteCheckers(new DummyDeleteChecker());

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObject dummyCopy = factory2.Load<DummyBusinessObject>(dummy.PK);

			dummyCopy.Delete();
			factory2.Save();

			AssertEquals("Dummy.IsDeleted", true, dummy.IsDeleted);
		}

		public void TestDeleteCheckerStopsDelete()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			dummy.SetDeleteCheckers(new DummyDeleteChecker());

			bool gotException = false;
			try
			{
				dummy.Delete();
			}
			catch (CannotDeleteException)
			{
				gotException = true;
				AssertEquals("Not deleted", false, dummy.IsDeleted);
			}
			Assert("Delete checker Exception thrown", gotException);
		}
	}
}
