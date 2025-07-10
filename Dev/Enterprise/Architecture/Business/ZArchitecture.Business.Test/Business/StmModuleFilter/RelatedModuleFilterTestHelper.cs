using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public static class RelatedModuleFilterTestHelper
	{
		public static void AssertModuleFilterDeepClone(StmModuleFilter original, StmModuleFilter clone, BusinessObject cloneParent)
		{
			Assertion.AssertNotEquals("Cloning should have created a new StmModuleFilter rather than keeping a reference to the original, and yet... Try using args.AddExcludedColumns in CloneInternal.", original.PK, clone.PK);
			Assertion.AssertEquals(original.S9_FilterName, clone.S9_FilterName);

			Assertion.AssertEquals(original.S9_ModuleID, clone.S9_ModuleID);
			Assertion.AssertEquals(original.S9_FilterName, clone.S9_FilterName);

			Assertion.AssertEquals("Filter data on cloned filters should have same content", original.S9_FilterData, clone.S9_FilterData);
			Assertion.AssertEquals("Layout data on cloned filters should have same content", original.S9_ColumnLayoutData, clone.S9_ColumnLayoutData);

			Assertion.AssertNotEquals("Should create a new StmModuleFilterUserData record", original.GetOrCreateLayoutUserData(new EmptyLayoutsHelper()).PK, clone.GetOrCreateLayoutUserData(new EmptyLayoutsHelper()).PK);
			Assertion.AssertEquals("StmModuleFilterUserData values should be the same", original.GetOrCreateLayoutUserData(new EmptyLayoutsHelper()).S0_FilterDataValues, clone.GetOrCreateLayoutUserData(new EmptyLayoutsHelper()).S0_FilterDataValues);
		}
	}
}
