using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentEngineCore.DocWrappers.Testing
{
	sealed class EmptyGenericWrapperLoaderTest : TestCaseWithFactory
	{
		public void TestGetWrappers()
		{
			EmptyGenericWrapperLoader loader = new EmptyGenericWrapperLoader();

			DocumentWrapper[] wrappers = loader.GetWrappers(null, null);
			AssertNull(wrappers);

			wrappers = loader.GetWrappers(null, null, null);
			AssertNull(wrappers);
		}

		public void GetWrapperType()
		{
			EmptyGenericWrapperLoader loader = new EmptyGenericWrapperLoader();
			AssertNull(loader.GetWrapperType());
		}
	}
}
