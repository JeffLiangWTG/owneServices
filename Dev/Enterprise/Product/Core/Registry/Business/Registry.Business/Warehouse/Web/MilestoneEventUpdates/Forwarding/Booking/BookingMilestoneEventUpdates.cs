using System.Xml;
using System.Xml.Serialization;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class BookingMilestoneEventUpdates : MilestoneEventUpdates
	{
		#region Schema

		public new class Schema : MilestoneEventUpdates.Schema
		{
			public const string IsExportBrokerUpdateAllowed = "IsExportBrokerUpdateAllowed";
			public const string IsImportBrokerUpdateAllowed = "IsImportBrokerUpdateAllowed";
			public const string IsDeliveryAgentUpdateAllowed = "IsDeliveryAgentUpdateAllowed";
			public const string IsReceivingAgentUpdateAllowed = "IsReceivingAgentUpdateAllowed";
			public const string IsSendingAgentUpdateAllowed = "IsSendingAgentUpdateAllowed";
			public const string IsShipperUpdateAllowed = "IsShipperUpdateAllowed";
			public const string IsConsigneeUpdateAllowed = "IsConsigneeUpdateAllowed";
			public const string IsLocalClientUpdateAllowed = "IsLocalClientUpdateAllowed";
		}

		#endregion

		#region Constructors

		public BookingMilestoneEventUpdates() : base() { }

		public BookingMilestoneEventUpdates(ZString eventType)
			: base(eventType)
		{
		}

		#endregion

		#region Properties

		#region IsLocalClientUpdateAllowed

		public ZBool IsLocalClientUpdateAllowed
		{
			get { return GetValue(WebPartyType.LocalClient); }
			set
			{
				SetValue(WebPartyType.LocalClient, value);
				IsLocalClientUpdateAllowedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsLocalClientUpdateAllowedInfo
		{
			get { return GetZPropertyInfo(Schema.IsLocalClientUpdateAllowed); }
		}

		#endregion

		#region IsConsigneeUpdateAllowed

		public ZBool IsConsigneeUpdateAllowed
		{
			get { return GetValue(WebPartyType.Consignee); }
			set
			{
				SetValue(WebPartyType.Consignee, value);
				IsConsigneeUpdateAllowedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsConsigneeUpdateAllowedInfo
		{
			get { return GetZPropertyInfo(Schema.IsConsigneeUpdateAllowed); }
		}

		#endregion

		#region IsShipperUpdateAllowed

		public ZBool IsShipperUpdateAllowed
		{
			get { return GetValue(WebPartyType.Shipper); }
			set
			{
				SetValue(WebPartyType.Shipper, value);
				IsShipperUpdateAllowedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsShipperUpdateAllowedInfo
		{
			get { return GetZPropertyInfo(Schema.IsShipperUpdateAllowed); }
		}

		#endregion

		#region IsSendingAgentUpdateAllowed

		public ZBool IsSendingAgentUpdateAllowed
		{
			get { return GetValue(WebPartyType.SendingAgent); }
			set
			{
				SetValue(WebPartyType.SendingAgent, value);
				IsSendingAgentUpdateAllowedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsSendingAgentUpdateAllowedInfo
		{
			get { return GetZPropertyInfo(Schema.IsSendingAgentUpdateAllowed); }
		}

		#endregion

		#region IsReceivingAgentUpdateAllowed

		public ZBool IsReceivingAgentUpdateAllowed
		{
			get { return GetValue(WebPartyType.ReceivingAgent); }
			set
			{
				SetValue(WebPartyType.ReceivingAgent, value);
				IsReceivingAgentUpdateAllowedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsReceivingAgentUpdateAllowedInfo
		{
			get { return GetZPropertyInfo(Schema.IsReceivingAgentUpdateAllowed); }
		}

		#endregion

		#region IsDeliveryAgentUpdateAllowed

		public ZBool IsDeliveryAgentUpdateAllowed
		{
			get { return GetValue(WebPartyType.DeliveryAgent); }
			set
			{
				SetValue(WebPartyType.DeliveryAgent, value);
				IsDeliveryAgentUpdateAllowedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsDeliveryAgentUpdateAllowedInfo
		{
			get { return GetZPropertyInfo(Schema.IsDeliveryAgentUpdateAllowed); }
		}

		#endregion

		#region IsImportBrokerUpdateAllowed

		public ZBool IsImportBrokerUpdateAllowed
		{
			get { return GetValue(WebPartyType.ImportBroker); }
			set
			{
				SetValue(WebPartyType.ImportBroker, value);
				IsImportBrokerUpdateAllowedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsImportBrokerUpdateAllowedInfo
		{
			get { return GetZPropertyInfo(Schema.IsImportBrokerUpdateAllowed); }
		}

		#endregion

		#region IsExportBrokerUpdateAllowed

		public ZBool IsExportBrokerUpdateAllowed
		{
			get { return GetValue(WebPartyType.ExportBroker); }
			set
			{
				SetValue(WebPartyType.ExportBroker, value);
				IsExportBrokerUpdateAllowedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsExportBrokerUpdateAllowedInfo
		{
			get { return GetZPropertyInfo(Schema.IsExportBrokerUpdateAllowed); }
		}

		#endregion

		#endregion

		protected override ZString GetWorkflowType()
		{
			return Constants.WebWorkflowType.ForwardingBooking;
		}

		protected override MilestoneEventUpdates GetNewMilestoneEventUpdatesCopy()
		{
			return new BookingMilestoneEventUpdates(EventType);
		}

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.IsExportBrokerUpdateAllowed, IsExportBrokerUpdateAllowed.ToString());
			writer.WriteElementString(Schema.IsImportBrokerUpdateAllowed, IsImportBrokerUpdateAllowed.ToString());
			writer.WriteElementString(Schema.IsDeliveryAgentUpdateAllowed, IsDeliveryAgentUpdateAllowed.ToString());
			writer.WriteElementString(Schema.IsReceivingAgentUpdateAllowed, IsReceivingAgentUpdateAllowed.ToString());
			writer.WriteElementString(Schema.IsSendingAgentUpdateAllowed, IsSendingAgentUpdateAllowed.ToString());
			writer.WriteElementString(Schema.IsShipperUpdateAllowed, IsShipperUpdateAllowed.ToString());
			writer.WriteElementString(Schema.IsConsigneeUpdateAllowed, IsConsigneeUpdateAllowed.ToString());
			writer.WriteElementString(Schema.IsLocalClientUpdateAllowed, IsLocalClientUpdateAllowed.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			base.ReadElements(reader);
			IsExportBrokerUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsExportBrokerUpdateAllowed);
			IsImportBrokerUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsImportBrokerUpdateAllowed);
			IsDeliveryAgentUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsDeliveryAgentUpdateAllowed);
			IsReceivingAgentUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsReceivingAgentUpdateAllowed);
			IsSendingAgentUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsSendingAgentUpdateAllowed);
			IsShipperUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsShipperUpdateAllowed);
			IsConsigneeUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsConsigneeUpdateAllowed);
			IsLocalClientUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsLocalClientUpdateAllowed);
		}

		#endregion
	}
}
