using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(UpdateJobOverseasAgentActionMethodApplicator))]
	public class UpdateJobOverseasAgentActionMethodApplicatorTest : UpdateJobActionMethodApplicatorBaseTest
	{
		public void TestOverseasAgent()
		{
			Applicator.OverseasAgent = TestObjectCreator.Debtor.MainAddress.PK;

			var collection = new OrganisationsFindBoxCollection(Factory);
			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property0", ZBool.True));

			AssertEquals(TestObjectCreator.Debtor.MainAddress.PK, Applicator.OverseasAgent);
			AssertNotNull(Applicator.OverseasAgentInfo);
			AssertEquals(collection.Count, Applicator.OverseasAgentList.Count);
		}

		public void TestValidateOverseasAgent()
		{
			Applicator.OverseasAgent = ZGuid.Invalid;

			AssertHasError(Applicator.OverseasAgentInfo, "Enter a valid selection.");
		}

		public void TestAgentCollectAddr_ZAddress()
		{
			Applicator.OverseasAgent = TestObjectCreator.Debtor.MainAddress.PK;
			AssertNotNull(Applicator.AgentCollectAddr_ZAddress);
		}

		protected override void SetupJobPropertySameWithApplicatorValue()
		{
			Applicator.OverseasAgent = TestObjectCreator.Debtor.MainAddress.PK;
			Shipment1.Job.JH_OA_AgentCollectAddr = TestObjectCreator.Debtor.MainAddress.PK;
		}

		protected override void SetupJobPropertyInfoReadOnly()
		{
			Applicator.OverseasAgent = TestObjectCreator.Debtor.MainAddress.PK;
			Shipment1.Job.SetReadOnlyIncludingChildren(true);
		}

		protected override void SetupApplicatorPropertyForValidation()
		{
			Applicator.OverseasAgent = ZGuid.Invalid;
		}

		protected override void AssertUpdateJobPropertySuccessful()
		{
			AssertNotEquals(TestObjectCreator.Debtor.MainAddress.PK, Shipment1.Job.JH_OA_AgentCollectAddr);
			AssertNotEquals(TestObjectCreator.Debtor.MainAddress.PK, Shipment2.Job.JH_OA_AgentCollectAddr);
			Applicator.OverseasAgent = TestObjectCreator.Debtor.MainAddress.PK;
			var jobNumber1 = Shipment1.Job.JH_JobNum;
			var jobNumber2 = Shipment2.Job.JH_JobNum;

			ApplyApplicator(new BusinessObject[] { Shipment1, Shipment2 }, $@"INFO: Job {jobNumber1}: Start Process.
INFO: Job {jobNumber1}: Processed.

INFO: Job {jobNumber2}: Start Process.
INFO: Job {jobNumber2}: Processed.");

			AssertEquals(TestObjectCreator.Debtor.MainAddress.PK, Shipment1.Job.JH_OA_AgentCollectAddr);
			AssertEquals(TestObjectCreator.Debtor.MainAddress.PK, Shipment2.Job.JH_OA_AgentCollectAddr);
		}

		public override string ExpectedValidationErrorLog => $@"INFO: Job {Shipment1.Job.JH_JobNum}: Start Process.
ERROR: Job has errors: Enter a valid Overseas Agent.
";

		protected override BusinessObject GetNewBusinessObject()
		{
			return new UpdateJobOverseasAgentActionMethodApplicator(Factory);
		}

		new UpdateJobOverseasAgentActionMethodApplicator Applicator => (UpdateJobOverseasAgentActionMethodApplicator)base.Applicator;
	}
}
