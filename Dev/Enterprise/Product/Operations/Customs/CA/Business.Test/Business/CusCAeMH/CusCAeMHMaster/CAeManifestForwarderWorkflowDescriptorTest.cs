using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CAeManifestForwarderWorkflowDescriptor))]
	sealed class CAeManifestForwarderWorkflowDescriptorTest : WorkflowDescriptorTestCase<CAeManifestForwarderWorkflowDescriptor>
	{
		public void TestDocumentBusinessContext()
		{
			AssertEquals(BusinessContext.CAeManifest, WorkflowDescriptor.DocumentBusinessContext[0]);
		}

		public void TestGetWorkflowTriggerActionCore()
		{
			var descriptor = new CAeManifestForwarderWorkflowDescriptor();
			var cusCAeMHMaster = Factory.New<CusCAeMHMaster>();
			ProcessTask processTask = cusCAeMHMaster.WorkflowItems.Triggers.AddNew();
			ProcessTaskNotification action = Factory.NewWithValidTestData<ProcessTaskNotification>();
			processTask.ProcessTaskNotifications.Add(action);
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendEmanifestCloseMessage;
			IProcessor resultProcessor = descriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));
			Assert("Processor for Send Close message should be SendCAeManifestCloseMessageProcessor", resultProcessor is SendCAeManifestCloseMessageProcessor);
		}

		protected override CodeDescriptionPair[] ExpectedAdditionalWorkflowTriggerActionTypes
		{
			get
			{
				List<CodeDescriptionPair> list = new List<CodeDescriptionPair>();
				list.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendEmanifestCloseMessage, WorkflowTriggerActionTypeConstants.Descriptions.SendEmanifestCloseMessage));
				list.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendAllEmanifestHouseBills, WorkflowTriggerActionTypeConstants.Descriptions.SendAllEmanifestHouseBills));
				return list.ToArray();
			}
		}

		public override void TestID()
		{
			AssertEquals("Correct Code", WorkflowDescriptors.CAeManifestWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "CA eManifest Forwarder", WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			AssertEquals("0 sub type", 0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals("SupportsEventTracking", true, WorkflowDescriptor.SupportsEventTracking);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[] { Factory.NewWithValidTestData<CusCAeMHMaster>() };
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Email;
			}
		}
	}
}
