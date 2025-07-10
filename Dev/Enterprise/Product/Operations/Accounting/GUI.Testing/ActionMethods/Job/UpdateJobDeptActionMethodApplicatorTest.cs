using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(UpdateJobDeptActionMethodApplicator))]
	public class UpdateJobDeptActionMethodApplicatorTest : UpdateJobActionMethodApplicatorBaseTest
	{
		public void TestDepartment()
		{
			Applicator.Department = TestObjectCreator.FEADepartment.PK;
			AssertEquals(TestObjectCreator.FEADepartment.PK, Applicator.Department);
			AssertEquals(FindboxLookupCollections.GetDepartmentCollection_ActiveOnly(Factory), Applicator.DepartmentList);
			AssertNotNull(Applicator.DepartmentInfo);
		}

		public void TestValidateDepartment()
		{
			Applicator.Department = ZGuid.Empty;
			AssertHasError(Applicator.DepartmentInfo, "Please enter a value.");

			Applicator.Department = ZGuid.Invalid;
			AssertHasError(Applicator.DepartmentInfo, "Enter a valid selection.");
		}

		protected override void SetupJobPropertySameWithApplicatorValue()
		{
			Applicator.Department = TestObjectCreator.FEADepartment.PK;
			Shipment1.Job.JH_GE = TestObjectCreator.FEADepartment.PK;
		}

		protected override void SetupJobPropertyInfoReadOnly()
		{
			Applicator.Department = TestObjectCreator.FEADepartment.PK;
			Shipment1.Job.SetReadOnlyIncludingChildren(true);
		}

		protected override void SetupApplicatorPropertyForValidation()
		{
			Applicator.Department = ZGuid.Invalid;
		}

		protected override void AssertUpdateJobPropertySuccessful()
		{
			AssertNotEquals(TestObjectCreator.FEADepartment.PK, Shipment1.Job.JH_GE);
			AssertNotEquals(TestObjectCreator.FEADepartment.PK, Shipment2.Job.JH_GE);
			Applicator.Department = TestObjectCreator.FEADepartment.PK;
			var jobNumber1 = Shipment1.Job.JH_JobNum;
			var jobNumber2 = Shipment2.Job.JH_JobNum;

			ApplyApplicator(new BusinessObject[] { Shipment1, Shipment2 }, $@"INFO: Job {jobNumber1}: Start Process.
INFO: Job {jobNumber1}: Processed.

INFO: Job {jobNumber2}: Start Process.
INFO: Job {jobNumber2}: Processed.");

			AssertEquals(TestObjectCreator.FEADepartment.PK, Shipment1.Job.JH_GE);
			AssertEquals(TestObjectCreator.FEADepartment.PK, Shipment2.Job.JH_GE);
		}

		public override string ExpectedValidationErrorLog => $@"INFO: Job {Shipment1.Job.JH_JobNum}: Start Process.
ERROR: Job has errors: Enter a valid Department.";

		protected override BusinessObject GetNewBusinessObject()
		{
			return new UpdateJobDeptActionMethodApplicator(Factory);
		}

		new UpdateJobDeptActionMethodApplicator Applicator => (UpdateJobDeptActionMethodApplicator)base.Applicator;
	}
}
