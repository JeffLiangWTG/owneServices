using System.Xml;
using System.Xml.Serialization;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ISFMilestoneEventUpdates : MilestoneEventUpdates
	{
		#region Schema

		public new class Schema : MilestoneEventUpdates.Schema
		{
			public const string IsImporterUpdateAllowed = "IsImporterUpdateAllowed";
			public const string IsShipToLocationUpdateAllowed = "IsShipToLocationUpdateAllowed";
			public const string IsBuyingPartyUpdateAllowed = "IsBuyingPartyUpdateAllowed";
			public const string IsSellingPartyUpdateAllowed = "IsSellingPartyUpdateAllowed";
			public const string IsStuffingLocationUpdateAllowed = "IsStuffingLocationUpdateAllowed";
			public const string IsConsolidatorUpdateAllowed = "IsConsolidatorUpdateAllowed";
			public const string IsManufacturerUpdateAllowed = "IsManufacturerUpdateAllowed";
			public const string IsSendingAgentUpdateAllowed = "IsSendingAgentUpdateAllowed";
			public const string IsBookingPartyUpdateAllowed = "IsBookingPartyUpdateAllowed";
		}

		#endregion

		#region Constructors

		public ISFMilestoneEventUpdates() : base() { }

		public ISFMilestoneEventUpdates(ZString eventType)
			: base(eventType)
		{
		}

		#endregion

		#region Properties

		#region New

		public new ISFMilestoneEventUpdatesCollection Parent
		{
			get
			{
				return (ISFMilestoneEventUpdatesCollection)base.Parent;
			}
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

		#region IsManufacturerUpdateAllowed

		public ZBool IsManufacturerUpdateAllowed
		{
			get { return GetValue(WebPartyType.Manufacturer); }
			set
			{
				SetValue(WebPartyType.Manufacturer, value);
				IsManufacturerUpdateAllowedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsManufacturerUpdateAllowedInfo
		{
			get { return GetZPropertyInfo(Schema.IsManufacturerUpdateAllowed); }
		}

		#endregion

		#region IsConsolidatorUpdateAllowed

		public ZBool IsConsolidatorUpdateAllowed
		{
			get { return GetValue(WebPartyType.Consolidator); }
			set
			{
				SetValue(WebPartyType.Consolidator, value);
				IsConsolidatorUpdateAllowedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsConsolidatorUpdateAllowedInfo
		{
			get { return GetZPropertyInfo(Schema.IsConsolidatorUpdateAllowed); }
		}

		#endregion

		#region IsStuffingLocationUpdateAllowed

		public ZBool IsStuffingLocationUpdateAllowed
		{
			get { return GetValue(WebPartyType.StuffingLocation); }
			set
			{
				SetValue(WebPartyType.StuffingLocation, value);
				IsStuffingLocationUpdateAllowedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsStuffingLocationUpdateAllowedInfo
		{
			get { return GetZPropertyInfo(Schema.IsStuffingLocationUpdateAllowed); }
		}

		#endregion

		#region IsSellingPartyUpdateAllowed

		public ZBool IsSellingPartyUpdateAllowed
		{
			get { return GetValue(WebPartyType.SellingParty); }
			set
			{
				SetValue(WebPartyType.SellingParty, value);
				IsSellingPartyUpdateAllowedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsSellingPartyUpdateAllowedInfo
		{
			get { return GetZPropertyInfo(Schema.IsSellingPartyUpdateAllowed); }
		}

		#endregion

		#region IsImporterUpdateAllowed

		public ZBool IsImporterUpdateAllowed
		{
			get { return GetValue(WebPartyType.Importer); }
			set
			{
				SetValue(WebPartyType.Importer, value);
				IsImporterUpdateAllowedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsImporterUpdateAllowedInfo
		{
			get { return GetZPropertyInfo(Schema.IsImporterUpdateAllowed); }
		}

		#endregion

		#region IsShipToLocationUpdateAllowed

		public ZBool IsShipToLocationUpdateAllowed
		{
			get { return GetValue(WebPartyType.ShipToLocation); }
			set
			{
				SetValue(WebPartyType.ShipToLocation, value);
				IsShipToLocationUpdateAllowedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsShipToLocationUpdateAllowedInfo
		{
			get { return GetZPropertyInfo(Schema.IsShipToLocationUpdateAllowed); }
		}

		#endregion

		#region IsBuyingPartyUpdateAllowed

		public ZBool IsBuyingPartyUpdateAllowed
		{
			get { return GetValue(WebPartyType.BuyingParty); }
			set
			{
				SetValue(WebPartyType.BuyingParty, value);
				IsBuyingPartyUpdateAllowedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsBuyingPartyUpdateAllowedInfo
		{
			get { return GetZPropertyInfo(Schema.IsBuyingPartyUpdateAllowed); }
		}

		#endregion

		#endregion

		protected override ZString GetWorkflowType()
		{
			return Constants.WebWorkflowType.ISF;
		}

		protected override MilestoneEventUpdates GetNewMilestoneEventUpdatesCopy()
		{
			return new ISFMilestoneEventUpdates(EventType);
		}

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.IsImporterUpdateAllowed, IsImporterUpdateAllowed.ToString());
			writer.WriteElementString(Schema.IsShipToLocationUpdateAllowed, IsShipToLocationUpdateAllowed.ToString());
			writer.WriteElementString(Schema.IsBuyingPartyUpdateAllowed, IsBuyingPartyUpdateAllowed.ToString());
			writer.WriteElementString(Schema.IsSellingPartyUpdateAllowed, IsSellingPartyUpdateAllowed.ToString());
			writer.WriteElementString(Schema.IsStuffingLocationUpdateAllowed, IsStuffingLocationUpdateAllowed.ToString());
			writer.WriteElementString(Schema.IsConsolidatorUpdateAllowed, IsConsolidatorUpdateAllowed.ToString());
			writer.WriteElementString(Schema.IsManufacturerUpdateAllowed, IsManufacturerUpdateAllowed.ToString());
			writer.WriteElementString(Schema.IsSendingAgentUpdateAllowed, IsSendingAgentUpdateAllowed.ToString());
			writer.WriteElementString(Schema.IsBookingPartyUpdateAllowed, IsBookingPartyUpdateAllowed.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			base.ReadElements(reader);
			IsImporterUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsImporterUpdateAllowed);
			IsShipToLocationUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsShipToLocationUpdateAllowed);
			IsBuyingPartyUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsBuyingPartyUpdateAllowed);
			IsSellingPartyUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsSellingPartyUpdateAllowed);
			IsStuffingLocationUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsStuffingLocationUpdateAllowed);
			IsConsolidatorUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsConsolidatorUpdateAllowed);
			IsManufacturerUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsManufacturerUpdateAllowed);
			IsSendingAgentUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsSendingAgentUpdateAllowed);
			IsBookingPartyUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsBookingPartyUpdateAllowed);
		}

		#endregion
	}
}
