using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(StmData))]
	sealed class StmDataTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPreventDelete()
		{
			StmData stmData = Factory.New<StmData>();
			AssertEquals("PreventDelete", false, PreventDeleteAttribute.IsTrue(stmData.GetType()));
		}

		#region TestSupportsClone

		public void TestSupportsClone()
		{
			StmData userData = Factory.New<StmData>();
			Assert(userData.SupportsClone());
		}

		#endregion
	}
}
