using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ImmediateColumnFetchHintTest : TestCaseWithFactory
	{
		public void TestHintForcesBusinessObjectGeneration()
		{
			BusinessObjectFactory factoryForInsert = new BusinessObjectFactory();
			DummyBusinessObject bizO = DummyBusinessObject.New(factoryForInsert);
			bizO.Z0_Code = "123";
			factoryForInsert.Save();

			FetchHint hint = new ImmediateFetchHint(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Code, new ZString("123"));
			Factory.AddFetchHint(hint);
			AssertEquals("Precondition", 1, Factory.RowFactory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));
			AssertEquals("Precondition", 1, Factory.ActiveTableFetchHints);

			AssertEquals("Precondition", 0, Factory.GetBizOsForPK(bizO.PK.ToGuid()).Length);

			Factory.Load(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Code, "124"));
			AssertEquals(0, Factory.RowFactory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));
			AssertEquals(0, Factory.ActiveTableFetchHints);
			AssertEquals(1, Factory.GetBizOsForPK(bizO.PK.ToGuid()).Length);
		}
	}
}
