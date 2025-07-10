using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Services.OperationalActions.Support.Testing;

namespace Enterprise.Accounting.GUI.Testing
{
	public abstract class UpdateJobActionMethodApplicatorBaseTest : OperationalActionMethodApplicatorTest
	{
		public void TestUpdateProperty()
		{
			AssertUpdateJobPropertySuccessful();
		}

		public void TestUpdateProperty_SameWithPrevious()
		{
			SetupJobPropertySameWithApplicatorValue();
			var jobNumber1 = Shipment1.Job.JH_JobNum;
			var jobNumber2 = Shipment2.Job.JH_JobNum;
			ApplyApplicator(new BusinessObject[] { Shipment1, Shipment2 }, $@"INFO: Job {jobNumber1}: Start Process.
INFO: Skipped, because the value you want to set is same with previous.

INFO: Job {jobNumber2}: Start Process.
INFO: Job {jobNumber2}: Processed.");
		}

		public void TestUpdateProperty_PropertyInfoReadOnly()
		{
			SetupJobPropertyInfoReadOnly();
			var jobNumber1 = Shipment1.Job.JH_JobNum;
			ApplyApplicator(new BusinessObject[] { Shipment1, Shipment2 }, $@"INFO: Job {jobNumber1}: Start Process.
ERROR: The field is read-only in this job.");
		}

		public void TestUpdateProperty_ValidationError()
		{
			SetupApplicatorPropertyForValidation();
			ApplyApplicator(new BusinessObject[] { Shipment1, Shipment2 }, ExpectedValidationErrorLog);
		}

		protected abstract void SetupJobPropertySameWithApplicatorValue();

		protected abstract void SetupJobPropertyInfoReadOnly();

		protected abstract void SetupApplicatorPropertyForValidation();

		protected abstract void AssertUpdateJobPropertySuccessful();

		public abstract string ExpectedValidationErrorLog { get; }

		protected override void SetUp()
		{
			base.SetUp();

			Shipment1 = TestObjectCreator.CreateAndSaveTestForwardingShipmentJob("WRK");
			Shipment2 = TestObjectCreator.CreateAndSaveTestForwardingShipmentJob("WRK");
		}

		protected ForwardingShipment Shipment1;
		protected ForwardingShipment Shipment2;

		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}

				return fTestObjectCreator;
			}
		}

		TestObjectCreator fTestObjectCreator;
	}
}
