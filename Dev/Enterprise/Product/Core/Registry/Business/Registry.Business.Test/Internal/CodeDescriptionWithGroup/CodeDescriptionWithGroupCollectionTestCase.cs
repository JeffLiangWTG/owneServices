using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Testing
{
	sealed class CodeDescriptionWithGroupCollectionTestCase : TestCaseWithFactory
	{
		public void TestGetClone()
		{
			FallbackLevel testFallBack = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var coll = new CodeDescriptionWithGroupCollectionForTest(testFallBack);
			coll.CodeMaxLength = 55;
			var coll2 = (CodeDescriptionWithGroupCollection)coll.GetCloneForTesting(testFallBack, Factory);
			AssertEquals(testFallBack, coll2.CurrentFallbackLevel);
			AssertEquals(55, coll2.CodeMaxLength);
		}
	}
}
