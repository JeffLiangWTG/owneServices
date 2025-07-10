using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Testing
{
	public sealed class CodeDescriptionBoolCollectionTest : TestCaseWithFactory
	{
		public void TestGetNewCollection()
		{
			var testFallBack = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var coll = new CodeDescriptionBoolCollectionForTest(testFallBack);
			var coll2 = coll.GetNewCollectionForTesting();
			AssertEquals(testFallBack, coll2.CurrentFallbackLevel);
		}

		public void TestGetClone()
		{
			var testFallBack = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var coll = new CodeDescriptionBoolCollectionForTest(testFallBack);
			coll.CodeMaxLength = 55;
			var coll2 = (CodeDescriptionBoolCollection)coll.GetCloneForTesting(testFallBack, Factory);
			AssertEquals(testFallBack, coll2.CurrentFallbackLevel);
			AssertEquals(55, coll2.CodeMaxLength);
		}
	}
}
