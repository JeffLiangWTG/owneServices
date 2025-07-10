using System.Xml;
using System.Xml.Serialization;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class BillOfLadingMilestoneEventUpdates : MilestoneEventUpdates
	{
		#region Schema

		public new class Schema : MilestoneEventUpdates.Schema
		{
			public const string IsBookingPartyUpdateAllowed = "IsBookingPartyUpdateAllowed";
			public const string IsConsignorUpdateAllowed = "IsConsignorUpdateAllowed";
			public const string IsConsigneeUpdateAllowed = "IsConsigneeUpdateAllowed";
		}

		#endregion

		#region Constructors

		public BillOfLadingMilestoneEventUpdates() : base() { }

		public BillOfLadingMilestoneEventUpdates(ZString eventType)
			: base(eventType)
		{
		}

		#endregion

		#region Properties

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

		#region IsConsignorUpdateAllowed

		public ZBool IsConsignorUpdateAllowed
		{
			get { return GetValue(WebPartyType.Consignor); }
			set
			{
				SetValue(WebPartyType.Consignor, value);
				IsConsignorUpdateAllowedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsConsignorUpdateAllowedInfo
		{
			get { return GetZPropertyInfo(Schema.IsConsignorUpdateAllowed); }
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
			return Constants.WebWorkflowType.BillOfLading;
		}

		protected override MilestoneEventUpdates GetNewMilestoneEventUpdatesCopy()
		{
			return new BillOfLadingMilestoneEventUpdates(EventType);
		}

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.IsBookingPartyUpdateAllowed, IsBookingPartyUpdateAllowed.ToString());
			writer.WriteElementString(Schema.IsConsignorUpdateAllowed, IsConsignorUpdateAllowed.ToString());
			writer.WriteElementString(Schema.IsConsigneeUpdateAllowed, IsConsigneeUpdateAllowed.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			base.ReadElements(reader);
			IsBookingPartyUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsBookingPartyUpdateAllowed);
			IsConsignorUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsConsignorUpdateAllowed);
			IsConsigneeUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsConsigneeUpdateAllowed);
		}

		#endregion
	}
}
