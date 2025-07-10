using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AirCargoOutturnWorkflowDescriptor))]
	class AirCargoOutturnWorkflowDescriptorTest : WorkflowDescriptorTestCase<AirCargoOutturnWorkflowDescriptor>
	{
		public override void TestID()
		{
			AssertEquals("Correct Code", JobInvoicingConsumerTypes.CusUnderbond.Code, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", JobInvoicingConsumerTypes.CusUnderbond.Description, WorkflowDescriptor.Description);
		}

		public void TestSupportsBufferManagement_Does()
		{
			Assert(new AirCargoOutturnWorkflowDescriptor().SupportsBufferManagement);
		}

		public void TestControllerID()
		{
			AssertEquals(ControllerIDs.Customs.AU.AirCargoOutturnBillsController, new AirCargoOutturnWorkflowDescriptor().ControllerID);
		}

		[StressTest] // AirlineList loads many RefAirline objects
		public override void TestSubTypes()
		{
			AssertEquals("1 sub type", 1, WorkflowDescriptor.SubTypeInformation.Length);

			AssertEquals("Sub Type 1 is Airline", "Airline", WorkflowDescriptor.SubTypeInformation[0].Description);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[0].List);
		}

		[StressTest] // AirlineList loads many RefAirline objects
		public new void TestArrayPropertiesDoNotReturnNull()
		{
			base.TestArrayPropertiesDoNotReturnNull();
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(true, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var provider = Factory.New<CusUnderbond>();

			JobHeader.Loader jobLoader = new JobHeader.Loader(provider);
			JobHeader job = jobLoader.TryCreate();
			job.JH_OA_LocalChargesAddr = BillToPartyOrg.MainAddress.PK;

			return new[] { provider };
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return MessageRecipientPartyType.OrgProxy;
			}
		}
	}
}
