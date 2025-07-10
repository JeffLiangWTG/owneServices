using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class EnterpriseBusinessObjectExtensionsTest : TestCaseWithFactory
	{
		public void TestDeleteChildren()
		{
			var bizObj = Factory.New<DummyWithDependentsEnterpriseBusinessObject>();
			var childBizObj1 = bizObj.Dependents.AddNew();
			var childBizObj2 = bizObj.Dependents.AddNew();
			childBizObj1.ZD1_Code = "TEST1";
			childBizObj2.ZD1_Code = "TEST2";

			bizObj.DeleteChildren<DummyDependantBusinessObject>(DummyDependentBizoSchema.ZD1_Z0, additionalQuery: new(DummyDependentBizoSchema.ZD1_Code, "TEST2"));
			Assert(!childBizObj1.IsDeleted);
			Assert(childBizObj2.IsDeleted);

			bizObj.DeleteChildren<DummyDependantBusinessObject>(DummyDependentBizoSchema.ZD1_Z0);
			Assert(childBizObj1.IsDeleted);
		}

		public void TestLoadChildren()
		{
			var bizObj = Factory.New<DummyWithDependentsEnterpriseBusinessObject>();
			var childBizObj1 = bizObj.Dependents.AddNew();
			var childBizObj2 = bizObj.Dependents.AddNew();
			childBizObj1.ZD1_Code = "TEST1";
			childBizObj2.ZD1_Code = "TEST2";

			var actualLoadedChildren = bizObj.LoadChildren<DummyDependantBusinessObject>(DummyDependentBizoSchema.ZD1_Z0, additionalQuery: new(DummyDependentBizoSchema.ZD1_Code, "TEST1"));
			AssertSequencesEqual(new[] { childBizObj1 }, actualLoadedChildren);

			actualLoadedChildren = bizObj.LoadChildren<DummyDependantBusinessObject>(DummyDependentBizoSchema.ZD1_Z0);
			AssertSequencesEqual(new[] { childBizObj1, childBizObj2 }, actualLoadedChildren);
		}
	}
}
