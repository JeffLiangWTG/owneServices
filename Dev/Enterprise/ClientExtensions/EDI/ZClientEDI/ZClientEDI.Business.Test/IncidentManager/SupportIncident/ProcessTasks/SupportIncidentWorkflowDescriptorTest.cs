using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;
using ZClientEDI.Business.Test;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(SupportIncidentWorkflowDescriptor))]
	class SupportIncidentWorkflowDescriptorTest : WorkflowDescriptorTestCase<SupportIncidentWorkflowDescriptor>
	{
		public void TestBusinessContext()
		{
			AssertEquals(1, WorkflowDescriptor.DocumentBusinessContext.Length);
			AssertEquals(BusinessContext.SupportIncident, WorkflowDescriptor.DocumentBusinessContext[0]);
		}

		public override void TestID()
		{
			AssertEquals("Correct Code", EDIJobInvoicingConsumerTypes.Incident.Code, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", EDIJobInvoicingConsumerTypes.Incident.Description, WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			AssertEquals("5 sub types", 5, WorkflowDescriptor.SubTypeInformation.Length);

			AssertEquals("Sub Type 1", "Product", WorkflowDescriptor.SubTypeInformation[0].Description);
			AssertEquals("Sub Type 2", "Stage", WorkflowDescriptor.SubTypeInformation[1].Description);
			AssertEquals("Sub Type 3", "Reported By", WorkflowDescriptor.SubTypeInformation[2].Description);
			AssertEquals("Sub Type 4", "Product Area", WorkflowDescriptor.SubTypeInformation[3].Description);
			AssertEquals("Sub Type 5", "Language", WorkflowDescriptor.SubTypeInformation[4].Description);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[0].List);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[1].List);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[2].List);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[3].List);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[4].List);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals("RequiresPort1", false, WorkflowDescriptor.RequiresPort1);
			AssertEquals("RequiresPort2", false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresClient()
		{
			AssertEquals("RequiresClient", true, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals("RequiresBranch", false, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals("RequiresDepartment", false, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals("SupportsEventTracking", true, WorkflowDescriptor.SupportsEventTracking);
		}

		protected override void SetUp()
		{
			base.SetUp();

			EDIClientDbSchemaUpgradeForTest.UpgradeViewClientProcessHeader(TestConnection);
		}

		protected override bool ExpectingTasksToBeCompanySpecific
		{
			get { return false; }
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get { return MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Client | MessageRecipientPartyType.Email; }
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_OH_Client = ClientOrg.PK;
			return new IWorkflowProvider[] { incident };
		}

		protected override void SetDateTimeSourcePropertyValue(IWorkflowProvider workflowProvider, string dateTimeSourceType, ZDateTime localTime)
		{
			if (dateTimeSourceType == SupportIncidentEstimateDefaultedFromList.Codes.TimeOfTaskCopiedFromTemplate)
			{
				// Nothing to set - this information comes from the ADD event of the task or workflow.
			}
		}

		protected override ZDateTime GetLocalTimeForDateTimeOffsetTest(IWorkflowProvider workflowProvider)
		{
			var task = workflowProvider.WorkflowItems.Where(t => t.P9_Description == "FOR TEST").First();
			var result = task.Logs.AddedLog?.SL_EventTime ?? task.P9_SystemCreateTimeUtc; // The only datetime relevant is an ADD event timestamp
			if (!result.IsValid)
			{
				result = ZDateTime.UtcNow;
			}
			return result.Kind == DateTimeKind.Utc ? result.ToLocalBranchTime().ToSmallDateTimeFloor() : result.ToSmallDateTimeFloor();
		}

		protected override TimeSpan GetUtcOffsetForDateTimeSourceType(IWorkflowProvider workflowProvider, string dateTimeSourceType)
		{
			//Always use the branch responsible for the ADD event to calculate the estimate time
			return ZDateTime.UtcNow.ToDateTimeOffset(((GlbBranch)Env.CurrentBranch).HomePort).Offset;
		}
	}
}
