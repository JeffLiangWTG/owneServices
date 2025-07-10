namespace CargoWise.EntityFramework.Testing
{
	sealed class ValidationCache_Test : TestCaseWithFactory
	{
		public void TestHashTableIsCleared()
		{
			ValidationCache cache = new ValidationCache();
			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
			cache.BeginValidation(bizO.Z0_CodeInfo);

			cache.SetHasValidationBeenRun(bizO.Z0_CodeInfo);
			AssertEquals(1, cache.validatedPropertyInfosCacheQueue.Count);

			cache.EndValidation(bizO.Z0_CodeInfo);
			AssertEquals(0, cache.validatedPropertyInfosCacheQueue.Count);
		}

		public void TestBusinessObjectIsCleared()
		{
			ValidationCache cache = new ValidationCache();
			BusinessObject bizO = Factory.New(typeof(DummyBusinessObject));

			cache.BeginValidation(bizO);
			AssertEquals("Precondition", bizO, cache.topLevelBusinessObject);

			cache.EndValidation(bizO);
			AssertEquals(null, cache.topLevelBusinessObject);
		}
	}
}
