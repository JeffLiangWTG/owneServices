namespace Enterprise.Registry.Business.Testing
{
	sealed class CommissionPeriodCollectionToTVPTestCase : RegistryCollectionToTVPTestCase<CommissionPeriodCollection>
	{
		protected override CommissionPeriodCollection GetCollectionToTest()
		{
			return new CommissionPeriodCollection();
		}

		protected override CommissionPeriodCollection PopulateCollection()
		{
			var collection = new CommissionPeriodCollection();
			collection.Add(new CommissionPeriod() { Code = "NEW", EnglishDescription = "New Client", Start = 0, End = 12, IsEnabled = true });
			collection.Add(new CommissionPeriod() { Code = "EXS", EnglishDescription = "Existing Client", Start = 12, End = 24, IsEnabled = true });
			return collection;
		}
	}
}
