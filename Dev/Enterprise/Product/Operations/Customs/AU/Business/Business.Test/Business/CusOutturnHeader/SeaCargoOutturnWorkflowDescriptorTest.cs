using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.Integration.DataObjects;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(SeaCargoOutturnWorkflowDescriptor))]
	sealed class SeaCargoOutturnWorkflowDescriptorTest : WorkflowDescriptorTestCase<SeaCargoOutturnWorkflowDescriptor>
	{
		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		public override void TestIEventPublisherPerformance()
		{
			Assert(true);
		}

		public override void TestID()
		{
			AssertEquals("Correct Code", WorkflowDescriptors.SeaCargoOutturnWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "Sea Cargo Outturn", WorkflowDescriptor.Description);
		}

		public void TestControllerID()
		{
			AssertEquals(ControllerIDs.Customs.AU.SeaCargoDepotStandAloneController, new SeaCargoOutturnWorkflowDescriptor().ControllerID);
		}

		[StressTest]
		public override void TestSubTypes()
		{
			AssertEquals("No sub type", 0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresPort1);
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
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest() => new[] { (IWorkflowProvider)Factory.New<CusOutturnHeader>() };

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties => MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.ArrivalTransitWarehouse;

		protected override ZString[] ExpectedSupportedTriggerPartyServices(ZString recipient)
		{
			switch (recipient)
			{
				case MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse:
					return new ZString[] { ServiceCodesList.Codes.TransitWarehouseReceive };
				default:
					return base.ExpectedSupportedTriggerPartyServices(recipient);
			}
		}

		protected override void SetAdditionalPropertiesForTestGetWorkflowTriggerAction_ForNotificationEmailDelivery(IWorkflowProvider workflowProvider, string partyTypeCode)
		{
			base.SetAdditionalPropertiesForTestGetWorkflowTriggerAction_ForNotificationEmailDelivery(workflowProvider, partyTypeCode);
			var header = (CusOutturnHeader)workflowProvider;
			if (partyTypeCode == MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse)
			{
				header.C6_OA_OutturningPremise = CreateOrgHeaderAndSetupEDICommunications().MainAddress.PK;
			}
		}

		OrgHeader CreateOrgHeaderAndSetupEDICommunications()
		{
			var org = Factory.New<OrgHeader>();
			var orgMode = org.EDICommunicationsModes.AddNew();
			orgMode.EK_Module = WorkflowDescriptors.SeaCargoOutturnWorkflowDescriptorCode;
			orgMode.EK_FileFormat = "NTF";
			orgMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			orgMode.EK_Destination = "@notificationemail.cargowise.com";
			return org;
		}
	}
}
