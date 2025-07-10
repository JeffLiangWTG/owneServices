using System.Xml;
using System.Xml.Serialization;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CartageMilestoneEventUpdates : MilestoneEventUpdates
	{
		#region Schema

		public new class Schema : MilestoneEventUpdates.Schema
		{
			public const string IsShipperUpdateAllowed = "IsShipperUpdateAllowed";
			public const string IsConsigneeUpdateAllowed = "IsConsigneeUpdateAllowed";
			public const string IsCTOUpdateAllowed = "IsCTOUpdateAllowed";
			public const string IsCFSUpdateAllowed = "IsCFSUpdateAllowed";
			public const string IsLocalClientUpdateAllowed = "IsLocalClientUpdateAllowed";
		}

		#endregion

		#region Constructors

		public CartageMilestoneEventUpdates() : base() { }

		public CartageMilestoneEventUpdates(ZString eventType)
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

		#region IsCFSUpdateAllowed

		public ZBool IsCFSUpdateAllowed
		{
			get { return GetValue(WebPartyType.CFS); }
			set
			{
				SetValue(WebPartyType.CFS, value);
				IsCFSUpdateAllowedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsCFSUpdateAllowedInfo
		{
			get { return GetZPropertyInfo(Schema.IsCFSUpdateAllowed); }
		}

		#endregion

		#region IsCTOUpdateAllowed

		public ZBool IsCTOUpdateAllowed
		{
			get { return GetValue(WebPartyType.CTO); }
			set
			{
				SetValue(WebPartyType.CTO, value);
				IsCTOUpdateAllowedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsCTOUpdateAllowedInfo
		{
			get { return GetZPropertyInfo(Schema.IsCTOUpdateAllowed); }
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

		#endregion

		protected override ZString GetWorkflowType()
		{
			return Constants.WebWorkflowType.Cartage;
		}

		protected override MilestoneEventUpdates GetNewMilestoneEventUpdatesCopy()
		{
			return new CartageMilestoneEventUpdates(EventType);
		}

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.IsShipperUpdateAllowed, IsShipperUpdateAllowed.ToString());
			writer.WriteElementString(Schema.IsConsigneeUpdateAllowed, IsConsigneeUpdateAllowed.ToString());
			writer.WriteElementString(Schema.IsCTOUpdateAllowed, IsCTOUpdateAllowed.ToString());
			writer.WriteElementString(Schema.IsCFSUpdateAllowed, IsCFSUpdateAllowed.ToString());
			writer.WriteElementString(Schema.IsLocalClientUpdateAllowed, IsLocalClientUpdateAllowed.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			base.ReadElements(reader);
			IsShipperUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsShipperUpdateAllowed);
			IsConsigneeUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsConsigneeUpdateAllowed);
			IsCTOUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsCTOUpdateAllowed);
			IsCFSUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsCFSUpdateAllowed);
			IsLocalClientUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsLocalClientUpdateAllowed);
		}

		#endregion
	}
}
