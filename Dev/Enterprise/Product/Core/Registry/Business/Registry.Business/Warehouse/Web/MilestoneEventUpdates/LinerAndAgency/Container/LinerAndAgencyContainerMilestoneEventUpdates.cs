using System.Xml;
using System.Xml.Serialization;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class LinerAndAgencyContainerMilestoneEventUpdates : MilestoneEventUpdates
	{
		#region Schema

		public new class Schema : MilestoneEventUpdates.Schema
		{
			public const string IsCarrierUpdateAllowed = "IsCarrierUpdateAllowed";
			public const string IsConsigneeUpdateAllowed = "IsConsigneeUpdateAllowed";
			public const string IsDeliveryAgentUpdateAllowed = "IsDeliveryAgentUpdateAllowed";
			public const string IsLocalClientUpdateAllowed = "IsLocalClientUpdateAllowed";
			public const string IsReceivingAgentUpdateAllowed = "IsReceivingAgentUpdateAllowed";
			public const string IsSendingAgentUpdateAllowed = "IsSendingAgentUpdateAllowed";
			public const string IsBookingPartyUpdateAllowed = "IsBookingPartyUpdateAllowed";
		}

		#endregion

		#region Constructors

		public LinerAndAgencyContainerMilestoneEventUpdates() : base() { }

		public LinerAndAgencyContainerMilestoneEventUpdates(ZString eventType)
			: base(eventType)
		{
		}

		#endregion

		#region Properties

		#region IsCarrierUpdateAllowed

		public ZBool IsCarrierUpdateAllowed
		{
			get { return GetValue(WebPartyType.Carrier); }
			set
			{
				SetValue(WebPartyType.Carrier, value);
				IsCarrierUpdateAllowedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsCarrierUpdateAllowedInfo
		{
			get { return GetZPropertyInfo(Schema.IsCarrierUpdateAllowed); }
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

		#region IsBookingPartyUpdateAllowed

		public ZBool IsBookingPartyUpdateAllowed
		{
			get { return GetValue(WebPartyType.BookingParty); }
			set
			{
				SetValue(WebPartyType.BookingParty, value);
				IsBookingPartyUpdateAllowedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsBookingPartyUpdateAllowedInfo
		{
			get { return GetZPropertyInfo(Schema.IsBookingPartyUpdateAllowed); }
		}

		#endregion

		#endregion

		protected override ZString GetWorkflowType()
		{
			return Constants.WebWorkflowType.Container;
		}

		protected override MilestoneEventUpdates GetNewMilestoneEventUpdatesCopy()
		{
			return new LinerAndAgencyContainerMilestoneEventUpdates(EventType);
		}

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.IsCarrierUpdateAllowed, IsCarrierUpdateAllowed.ToString());
			writer.WriteElementString(Schema.IsConsigneeUpdateAllowed, IsConsigneeUpdateAllowed.ToString());
			writer.WriteElementString(Schema.IsDeliveryAgentUpdateAllowed, IsDeliveryAgentUpdateAllowed.ToString());
			writer.WriteElementString(Schema.IsLocalClientUpdateAllowed, IsLocalClientUpdateAllowed.ToString());
			writer.WriteElementString(Schema.IsReceivingAgentUpdateAllowed, IsReceivingAgentUpdateAllowed.ToString());
			writer.WriteElementString(Schema.IsSendingAgentUpdateAllowed, IsSendingAgentUpdateAllowed.ToString());
			writer.WriteElementString(Schema.IsBookingPartyUpdateAllowed, IsBookingPartyUpdateAllowed.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			base.ReadElements(reader);
			IsCarrierUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsCarrierUpdateAllowed);
			IsConsigneeUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsConsigneeUpdateAllowed);
			IsDeliveryAgentUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsDeliveryAgentUpdateAllowed);
			IsLocalClientUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsLocalClientUpdateAllowed);
			IsReceivingAgentUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsReceivingAgentUpdateAllowed);
			IsSendingAgentUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsSendingAgentUpdateAllowed);
			IsBookingPartyUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsBookingPartyUpdateAllowed);
		}

		#endregion
	}
}
