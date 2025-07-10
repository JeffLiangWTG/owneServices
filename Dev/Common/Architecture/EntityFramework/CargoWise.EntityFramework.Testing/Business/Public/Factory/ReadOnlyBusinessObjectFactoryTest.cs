using CargoWise.Common;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ReadOnlyBusinessObjectFactoryTest : TestCaseWithFactory
	{
		public void TestSave()
		{
			ReadOnlyBusinessObjectFactory factory = new ReadOnlyBusinessObjectFactory();
			DummyBusinessObject dummy = factory.NewWithValidTestData<DummyBusinessObject>();
			factory.Save();
			AssertNull("Business object shouldn't be saved to database.", new BusinessObjectFactory().Load<DummyBusinessObject>(dummy.PK));
			AssertEquals("Incorrect saving action is logged.", "SavingReadOnlyFactory", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestRelinquishThreadOwnershipShouldReportWhenChangingOwnershipIsNotAllowed()
		{
			var factory = new ReadOnlyBusinessObjectFactory(allowChangingThreadOwnership: false);
			factory.RelinquishThreadOwnership();

			AssertContains("Attempted to call RelinquishThreadOwnership() on an object that does now allow changing thread ownership.", ErrorReporter.LastExceptionReported.Message);
			AssertEquals(1, ErrorReporter.TotalErrorCount);

			ErrorReporter.Clear();
		}
	}
}
