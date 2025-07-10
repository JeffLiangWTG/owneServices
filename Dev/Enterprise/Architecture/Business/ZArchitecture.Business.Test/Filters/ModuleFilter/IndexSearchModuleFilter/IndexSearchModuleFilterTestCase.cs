namespace Enterprise.ZArchitecture.Business.Testing
{
	public abstract class IndexSearchModuleFilterTestCase<T> : ModuleFilterTestCase<ModuleFilter> where T : IIndexSearchModuleFilter
	{
		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		public void TestZQueryIsEmpty()
		{
			AssertEquals(true, Filter.Query.IsEmpty);
		}

		abstract public void TestGetGlowIndexQuery();
	}
}
