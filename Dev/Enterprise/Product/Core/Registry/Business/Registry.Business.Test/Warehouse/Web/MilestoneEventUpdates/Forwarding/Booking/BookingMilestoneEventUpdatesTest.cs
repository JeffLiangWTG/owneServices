using CargoWise.Definitions;
using CargoWise.Types;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(BookingMilestoneEventUpdates))]
	sealed class BookingMilestoneEventUpdatesTest : MilestoneEventUpdatesTest
	{
		public void TestIsLocalClientUpdateAllowed()
		{
			AssertEquals(false, BookingMilestoneEvents.IsLocalClientUpdateAllowed);
			AssertEquals(false, BookingMilestoneEvents.GetValue(WebPartyType.LocalClient));

			BookingMilestoneEvents.IsLocalClientUpdateAllowed = true;
			AssertEquals(true, BookingMilestoneEvents.GetValue(WebPartyType.LocalClient));

			BookingMilestoneEvents.IsLocalClientUpdateAllowed = false;
			AssertEquals(false, BookingMilestoneEvents.GetValue(WebPartyType.LocalClient));
		}

		public void TestIsConsigneeUpdateAllowed()
		{
			AssertEquals(false, BookingMilestoneEvents.IsConsigneeUpdateAllowed);
			AssertEquals(false, BookingMilestoneEvents.GetValue(WebPartyType.Consignee));

			BookingMilestoneEvents.IsConsigneeUpdateAllowed = true;
			AssertEquals(true, BookingMilestoneEvents.GetValue(WebPartyType.Consignee));

			BookingMilestoneEvents.IsConsigneeUpdateAllowed = false;
			AssertEquals(false, BookingMilestoneEvents.GetValue(WebPartyType.Consignee));
		}

		public void TestIsShipperUpdateAllowed()
		{
			AssertEquals(false, BookingMilestoneEvents.IsShipperUpdateAllowed);
			AssertEquals(false, BookingMilestoneEvents.GetValue(WebPartyType.Shipper));

			BookingMilestoneEvents.IsShipperUpdateAllowed = true;
			AssertEquals(true, BookingMilestoneEvents.GetValue(WebPartyType.Shipper));

			BookingMilestoneEvents.IsShipperUpdateAllowed = false;
			AssertEquals(false, BookingMilestoneEvents.GetValue(WebPartyType.Shipper));
		}

		public void TestIsSendingAgentUpdateAllowed()
		{
			AssertEquals(false, BookingMilestoneEvents.IsSendingAgentUpdateAllowed);
			AssertEquals(false, BookingMilestoneEvents.GetValue(WebPartyType.SendingAgent));

			BookingMilestoneEvents.IsSendingAgentUpdateAllowed = true;
			AssertEquals(true, BookingMilestoneEvents.GetValue(WebPartyType.SendingAgent));

			BookingMilestoneEvents.IsSendingAgentUpdateAllowed = false;
			AssertEquals(false, BookingMilestoneEvents.GetValue(WebPartyType.SendingAgent));
		}

		public void TestIsReceivingAgentUpdateAllowed()
		{
			AssertEquals(false, BookingMilestoneEvents.IsReceivingAgentUpdateAllowed);
			AssertEquals(false, BookingMilestoneEvents.GetValue(WebPartyType.ReceivingAgent));

			BookingMilestoneEvents.IsReceivingAgentUpdateAllowed = true;
			AssertEquals(true, BookingMilestoneEvents.GetValue(WebPartyType.ReceivingAgent));

			BookingMilestoneEvents.IsReceivingAgentUpdateAllowed = false;
			AssertEquals(false, BookingMilestoneEvents.GetValue(WebPartyType.ReceivingAgent));
		}

		public void TestIsDeliveryAgentUpdateAllowed()
		{
			AssertEquals(false, BookingMilestoneEvents.IsDeliveryAgentUpdateAllowed);
			AssertEquals(false, BookingMilestoneEvents.GetValue(WebPartyType.DeliveryAgent));

			BookingMilestoneEvents.IsDeliveryAgentUpdateAllowed = true;
			AssertEquals(true, BookingMilestoneEvents.GetValue(WebPartyType.DeliveryAgent));

			BookingMilestoneEvents.IsDeliveryAgentUpdateAllowed = false;
			AssertEquals(false, BookingMilestoneEvents.GetValue(WebPartyType.DeliveryAgent));
		}

		public void TestIsImportBrokerUpdateAllowed()
		{
			AssertEquals(false, BookingMilestoneEvents.IsImportBrokerUpdateAllowed);
			AssertEquals(false, BookingMilestoneEvents.GetValue(WebPartyType.ImportBroker));

			BookingMilestoneEvents.IsImportBrokerUpdateAllowed = true;
			AssertEquals(true, BookingMilestoneEvents.GetValue(WebPartyType.ImportBroker));

			BookingMilestoneEvents.IsImportBrokerUpdateAllowed = false;
			AssertEquals(false, BookingMilestoneEvents.GetValue(WebPartyType.ImportBroker));
		}

		public void TestIsExportBrokerUpdateAllowed()
		{
			AssertEquals(false, BookingMilestoneEvents.IsExportBrokerUpdateAllowed);
			AssertEquals(false, BookingMilestoneEvents.GetValue(WebPartyType.ExportBroker));

			BookingMilestoneEvents.IsExportBrokerUpdateAllowed = true;
			AssertEquals(true, BookingMilestoneEvents.GetValue(WebPartyType.ExportBroker));

			BookingMilestoneEvents.IsExportBrokerUpdateAllowed = false;
			AssertEquals(false, BookingMilestoneEvents.GetValue(WebPartyType.ExportBroker));
		}

		BookingMilestoneEventUpdates BookingMilestoneEvents => (BookingMilestoneEventUpdates)TestObjectTemplate;

		#region Overrides

		protected override MilestoneEventUpdatesCollection GetNewObjectTemplateCollection()
		{
			return new BookingMilestoneEventUpdatesCollection();
		}

		protected override MilestoneEventUpdates GetNewObjectTemplate()
		{
			return new BookingMilestoneEventUpdates();
		}

		protected override MilestoneEventUpdates GetNewObjectTemplate(ZString eventType)
		{
			return new BookingMilestoneEventUpdates(eventType);
		}

		protected override ZString GetExpectedWorkflowType()
		{
			return Constants.WebWorkflowType.ForwardingBooking;
		}

		#endregion
	}
}
