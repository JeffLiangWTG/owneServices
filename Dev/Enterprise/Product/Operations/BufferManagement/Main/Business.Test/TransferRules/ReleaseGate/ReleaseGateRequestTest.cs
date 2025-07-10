using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	class ReleaseGateRequestTest : BMSTestCaseWithFactory
	{
		public void TestComparingWithNull_ShouldNotThrowException()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory), "STAHP");

			var request = new ReleaseGateRequest(workflow, config.ComponentLink);

			AssertEquals("The operator overloads should not throw null reference exceptions", false, request.Equals(null));

			AssertEquals("The operator overloads should not throw null reference exceptions", false, request == null);
			AssertEquals("The operator overloads should not throw null reference exceptions", false, null == request);
			AssertEquals("The operator overloads should not throw null reference exceptions", true, request != null);
			AssertEquals("The operator overloads should not throw null reference exceptions", true, null != request);
			AssertEquals("The operator overloads should not throw null reference exceptions", true, null == (ReleaseGateRequest)null);
			AssertEquals("The operator overloads should not throw null reference exceptions", false, null != (ReleaseGateRequest)null);
		}
	}
}
