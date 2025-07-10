using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Testing
{
	sealed class CodeDescriptionWithThreeGroupsCollectionTestCase : TestCaseWithFactory
	{
		public void TestGetClone()
		{
			FallbackLevel testFallBack = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var coll = new CodeDescriptionWithThreeGroupsCollectionForTest(55);
			var coll2 = (CodeDescriptionWithThreeGroupsCollection)coll.GetCloneForTesting(testFallBack, Factory);
			AssertEquals(testFallBack, coll2.CurrentFallbackLevel);
			AssertEquals(55, coll2.CodeMaxLength);
		}
	}
}
