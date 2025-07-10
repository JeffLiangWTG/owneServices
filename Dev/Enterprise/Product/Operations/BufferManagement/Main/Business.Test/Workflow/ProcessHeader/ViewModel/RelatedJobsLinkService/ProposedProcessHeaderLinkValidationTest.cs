using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	class ProposedProcessHeaderLinkValidationTest : BusinessObjectValidationTestCase
	{
		public void TestWorkflowFKs_ShouldFilterByJobHeader()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "ENFB");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "INFJ");

			var link = new ProposedProcessHeaderLink(jobHeader1, jobHeader2);

			link.Validation.ValidateAll();

			AssertMandatoryValidationError(link.FP_FH_HeaderFromInfo, isExpectingError: true);
			AssertMandatoryValidationError(link.FP_FH_HeaderToInfo, isExpectingError: true);

			link.FP_FH_HeaderFrom = workflow2.PK;
			link.FP_FH_HeaderTo = workflow1.PK;

			AssertNoErrors("Links should work either way around (job1 -> job2 or job2 -> job1)", link);

			link.FP_FH_HeaderFrom = workflow1.PK;
			link.FP_FH_HeaderTo = workflow2.PK;

			AssertNoErrors(link);
		}

		public void TestLinkType()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var link = new ProposedProcessHeaderLink(jobHeader1, jobHeader2);

			AssertEquals(ProcessHeaderLinkTypeList.Codes.Dependency, link.FP_LinkType);
			AssertNoErrors(link.FP_LinkTypeInfo);

			link.FP_LinkType = "ZZZ";

			AssertListValidationInvalidCodeError(link.FP_LinkTypeInfo, isExpectingError: true);

			foreach (ICodeDescription cdp in new ProcessHeaderLinkTypeList())
			{
				link.FP_LinkType = cdp.Code;
				AssertNoErrors(link.FP_LinkTypeInfo);
			}
		}
	}
}
