using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(UpdateJobOperatorActionMethodApplicator))]
	public class UpdateJobOperatorActionMethodApplicatorTest : UpdateJobActionMethodApplicatorBaseTest
	{
		public void TestOperator()
		{
			Applicator.Operator = TestObjectCreator.Staff.GS_Code;

			AssertEquals(TestObjectCreator.Staff.GS_Code, Applicator.Operator);
			AssertNotNull(Applicator.OperatorInfo);

			AssertEquals(Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, Applicator.Operator), Applicator.RepOps);
			AssertEquals((new GlbStaffCollection(Factory)).Count, Applicator.OperatorList.Count);
		}

		public void TestValidateOperator()
		{
			Applicator.Operator = string.Empty;
			AssertHasError(Applicator.OperatorInfo, "Please enter a value.");

			Applicator.Operator = "xxx";
			AssertHasError(Applicator.OperatorInfo, "Enter a valid selection.");
		}

		protected override void SetupJobPropertySameWithApplicatorValue()
		{
			Applicator.Operator = TestObjectCreator.Staff.GS_Code;
			Shipment1.Job.JH_GS_NKRepOps = TestObjectCreator.Staff.GS_Code;
		}

		protected override void SetupJobPropertyInfoReadOnly()
		{
			Applicator.Operator = TestObjectCreator.Staff.GS_Code;
			Shipment1.Job.SetReadOnlyIncludingChildren(true);
		}

		protected override void SetupApplicatorPropertyForValidation()
		{
			Applicator.Operator = "XXX";
		}

		protected override void AssertUpdateJobPropertySuccessful()
		{
			AssertNotEquals(TestObjectCreator.Staff.GS_Code, Shipment1.Job.JH_GS_NKRepOps);
			AssertNotEquals(TestObjectCreator.Staff.GS_Code, Shipment2.Job.JH_GS_NKRepOps);
			Applicator.Operator = TestObjectCreator.Staff.GS_Code;
			var jobNumber1 = Shipment1.Job.JH_JobNum;
			var jobNumber2 = Shipment2.Job.JH_JobNum;

			ApplyApplicator(new BusinessObject[] { Shipment1, Shipment2 }, $@"INFO: Job {jobNumber1}: Start Process.
INFO: Job {jobNumber1}: Processed.

INFO: Job {jobNumber2}: Start Process.
INFO: Job {jobNumber2}: Processed.");

			AssertEquals(TestObjectCreator.Staff.GS_Code, Shipment1.Job.JH_GS_NKRepOps);
			AssertEquals(TestObjectCreator.Staff.GS_Code, Shipment2.Job.JH_GS_NKRepOps);
		}

		public override string ExpectedValidationErrorLog => $@"INFO: Job {Shipment1.Job.JH_JobNum}: Start Process.
ERROR: Job has errors: Enter a valid Operations Rep.
";

		protected override BusinessObject GetNewBusinessObject()
		{
			return new UpdateJobOperatorActionMethodApplicator(Factory);
		}

		new UpdateJobOperatorActionMethodApplicator Applicator => (UpdateJobOperatorActionMethodApplicator)base.Applicator;
	}
}
