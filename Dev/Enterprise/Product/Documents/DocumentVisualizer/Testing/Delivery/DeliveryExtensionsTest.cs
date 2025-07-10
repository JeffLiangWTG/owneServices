using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Delivery;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DeliveryExtensionsTest : TestCaseWithFactory
	{
		#region TestCreateDeliveryGroup

		public void TestCreateDeliveryGroup_UseDeliveryGroup_MatchExisting()
		{
			var forwardingShipmentType = ObjectFactory.GetType<IForwardingShipment>();
			var shipment = (IForwardingShipment)Factory.NewWithValidTestData(forwardingShipmentType);
			shipment.JS_UniqueConsignRef = "STST0001000";

			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "Test DocPack";
			menuItem.SU_MenuType = Enterprise.Core.Constants.StmMenuItemTypes.Forms;
			menuItem.SU_BusinessContext = nameof(BusinessContext.Shipment);
			menuItem.SU_EmailSubjectLine = "my email subject: <JobNumber>";

			Factory.Save();

			var instructions = new DeliveryInstructions();
			instructions.DeliveryGroups[0].SB_EmailSubjectLine = "my email subject: STST0001000";
			instructions.Destination = DeliveryInstructionDestination.TakenFromContact;

			var documentPack = new DocumentPack(menuItem);
			documentPack.DocumentSupporter = ((IDocumentSupportable)shipment).DocumentSupporter;
			instructions.DocPack = documentPack;

			var contact = new DocDeliveryContact(Factory);
			contact.EmailCarbonCopyRecipientsAsString = "mr.robot@email.me";

			Assert("prerequisite: use delivery group", instructions.UsesDeliveryGroup);

			var deliveryGroup = instructions.GetOrCreateDeliveryGroup(contact);

			AssertNotNull("delivery group was returned", deliveryGroup);
			AssertEquals("delivery group is marked as unprocessed", false, deliveryGroup.SB_IsProcessed);
			AssertEquals("no additional delivery groups were created", 1, instructions.DeliveryGroups.Count);
			AssertEquals("delivery group was correctly matched by email subject", instructions.DeliveryGroups[0], deliveryGroup);
		}

		public void TestCreateDeliveryGroup_UseDeliveryGroup_UseOneWithEmptyEmailSubject()
		{
			var forwardingShipmentType = ObjectFactory.GetType<IForwardingShipment>();
			var shipment = (IForwardingShipment)Factory.NewWithValidTestData(forwardingShipmentType);
			shipment.JS_UniqueConsignRef = "STST0001000";

			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "Test DocPack";
			menuItem.SU_MenuType = Enterprise.Core.Constants.StmMenuItemTypes.Forms;
			menuItem.SU_BusinessContext = nameof(BusinessContext.Shipment);
			menuItem.SU_EmailSubjectLine = "my email subject: <JobNumber>";

			Factory.Save();

			var instructions = new DeliveryInstructions();
			instructions.DeliveryGroups[0].SB_EmailSubjectLine = "   ";
			instructions.Destination = DeliveryInstructionDestination.TakenFromContact;

			var documentPack = new DocumentPack(menuItem);
			documentPack.DocumentSupporter = ((IDocumentSupportable)shipment).DocumentSupporter;
			instructions.DocPack = documentPack;

			var contact = new DocDeliveryContact(Factory);
			contact.EmailCarbonCopyRecipientsAsString = "mr.robot@email.me";

			Assert("prerequisite: use delivery group", instructions.UsesDeliveryGroup);

			var deliveryGroup = instructions.GetOrCreateDeliveryGroup(contact);

			AssertNotNull("delivery group was returned", deliveryGroup);
			AssertEquals("delivery group is marked as unprocessed", false, deliveryGroup.SB_IsProcessed);
			AssertEquals("no additional delivery groups were created", 1, instructions.DeliveryGroups.Count);
			AssertEquals("delivery group was correctly matched by email subject", instructions.DeliveryGroups[0], deliveryGroup);
			AssertEquals("email subject was populated", "my email subject: STST0001000", deliveryGroup.SB_EmailSubjectLine);
		}

		public void TestCreateDeliveryGroup_UseDeliveryGroup_UseExistingRegardlessOfEmailSubject()
		{
			var forwardingShipmentType = ObjectFactory.GetType<IForwardingShipment>();
			var shipment = (IForwardingShipment)Factory.NewWithValidTestData(forwardingShipmentType);
			shipment.JS_UniqueConsignRef = "STST0001000";

			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "Test DocPack";
			menuItem.SU_MenuType = Enterprise.Core.Constants.StmMenuItemTypes.Forms;
			menuItem.SU_BusinessContext = nameof(BusinessContext.Shipment);
			menuItem.SU_EmailSubjectLine = "my email subject: <JobNumber>";

			Factory.Save();

			var instructions = new DeliveryInstructions();
			instructions.DeliveryGroups[0].SB_EmailSubjectLine = "subject";
			instructions.Destination = DeliveryInstructionDestination.TakenFromContact;

			var documentPack = new DocumentPack(menuItem);
			documentPack.DocumentSupporter = ((IDocumentSupportable)shipment).DocumentSupporter;
			instructions.DocPack = documentPack;

			var contact = new DocDeliveryContact(Factory);
			contact.EmailCarbonCopyRecipientsAsString = "mr.robot@email.me";

			Assert("prerequisite: use delivery group", instructions.UsesDeliveryGroup);

			var deliveryGroup = instructions.GetOrCreateDeliveryGroup(contact, DeliveryExtensions.DeliveryGroupMatchStrategies.MatchAny);

			AssertNotNull("delivery group was returned", deliveryGroup);
			AssertEquals("delivery group is marked as unprocessed", false, deliveryGroup.SB_IsProcessed);
			AssertEquals("no additional delivery groups were created", 1, instructions.DeliveryGroups.Count);
		}

		public void TestCreateDeliveryGroup_UseDeliveryGroup_CreateNew()
		{
			var forwardingShipmentType = ObjectFactory.GetType<IForwardingShipment>();
			var shipment = (IForwardingShipment)Factory.NewWithValidTestData(forwardingShipmentType);
			shipment.JS_UniqueConsignRef = "STST0001000";

			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "Test DocPack";
			menuItem.SU_MenuType = Enterprise.Core.Constants.StmMenuItemTypes.Forms;
			menuItem.SU_BusinessContext = nameof(BusinessContext.Shipment);
			menuItem.SU_EmailSubjectLine = "my email subject: <JobNumber>";

			Factory.Save();

			var instructions = new DeliveryInstructions();
			instructions.DeliveryGroups[0].SB_EmailSubjectLine = "some email subject";
			instructions.Destination = DeliveryInstructionDestination.TakenFromContact;

			var documentPack = new DocumentPack(menuItem);
			documentPack.DocumentSupporter = ((IDocumentSupportable)shipment).DocumentSupporter;
			instructions.DocPack = documentPack;

			var contact = new DocDeliveryContact(Factory);
			contact.EmailCarbonCopyRecipientsAsString = "mr.robot@email.me";

			Assert("prerequisite: use delivery group", instructions.UsesDeliveryGroup);

			var deliveryGroup = instructions.GetOrCreateDeliveryGroup(contact);

			AssertNotNull("delivery group was returned", deliveryGroup);
			AssertEquals("delivery group is marked as unprocessed", false, deliveryGroup.SB_IsProcessed);
			AssertEquals("new additional delivery group was created", 2, instructions.DeliveryGroups.Count);
			AssertEquals("delivery group was added to instuctions", instructions.DeliveryGroups[1], deliveryGroup);
			AssertEquals("added delivery group is in correct factory", deliveryGroup.Factory._Instance, instructions.Factory._Instance);
			AssertEquals("created delivery group email subject", "my email subject: STST0001000", deliveryGroup.SB_EmailSubjectLine);
			Assert("delivery group is in db", deliveryGroup.IsInDatabase);
		}

		public void TestCreateDeliveryGroup_DoNotUseDeliveryGroup()
		{
			var instructions = new DeliveryInstructions();
			instructions.Destination = DeliveryInstructionDestination.Preview;

			var contact = new DocDeliveryContact(Factory);
			contact.EmailCarbonCopyRecipientsAsString = "mr.robot@email.me";

			Assert("prerequisite: do not use delivery group", !instructions.UsesDeliveryGroup);

			var deliveryGroup = instructions.GetOrCreateDeliveryGroup(contact);

			AssertNull("delivery group was not created", deliveryGroup);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			TestCaseHelper.ClearTable(StmPrintJobCopyRecipientSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmDeliveryGroupSchema.Constants.TableName);
		}

		#endregion
	}
}
