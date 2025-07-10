using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentEngine.DeliveryMethods.Testing
{
	sealed class FactoryStrategyTest : TestCaseWithFactory
	{
		public void TestSaveInChunksReturnsANewFactoryEveryTime()
		{
			FactoryStrategy strategy = new FactoryStrategy.SaveInChunks();
			BusinessObjectFactory factory1 = strategy.GetFactory();
			BusinessObjectFactory factory2 = strategy.GetFactory();

			AssertNotNull("Factory returned is never null", factory1);
			AssertNotNull("Factory returned is never null", factory2);
			Assert("Different factory every time", factory1 != factory2);
		}

		public void TestPopulateButDoNotSaveGetFactory()
		{
			FactoryStrategy strategy = new FactoryStrategy.PopulateButDoNotSave(Factory);
			BusinessObjectFactory factory1 = strategy.GetFactory();
			BusinessObjectFactory factory2 = strategy.GetFactory();

			AssertNotNull("Factory returned is never null", factory1);
			AssertNotNull("Factory returned is never null", factory2);
			AssertEquals("Same factory every time", factory1, factory2);
		}

		public void TestSaveInChunksDoesSave()
		{
			FactoryStrategy strategy = new FactoryStrategy.SaveInChunks();
			BusinessObjectFactory factory1 = strategy.GetFactory();
			factory1.Saved += new BusinessObjectFactory.SavedEventHandler(Factory1_Saved);
			strategy.SaveChunk(factory1);
			Assert("Saved", HasSaved);
		}

		bool HasSaved;
		void Factory1_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			HasSaved = true;
		}

		public void TestPopulateButDoNotSaveDoesNotSave()
		{
			FactoryStrategy strategy = new FactoryStrategy.PopulateButDoNotSave(Factory);
			BusinessObjectFactory factory1 = strategy.GetFactory();
			factory1.Saved += new BusinessObjectFactory.SavedEventHandler(Factory1_Saved);
			strategy.SaveChunk(factory1);
			Assert("Didn't Saved", !HasSaved);
		}
	}
}
