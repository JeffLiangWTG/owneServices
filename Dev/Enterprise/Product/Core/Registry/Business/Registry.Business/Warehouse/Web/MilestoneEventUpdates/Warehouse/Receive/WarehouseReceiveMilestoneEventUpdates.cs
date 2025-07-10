using System.Xml;
using System.Xml.Serialization;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class WarehouseReceiveMilestoneEventUpdates : MilestoneEventUpdates
	{
		#region Schema

		public new class Schema : MilestoneEventUpdates.Schema
		{
			public const string IsClientUpdateAllowed = "IsClientUpdateAllowed";
			public const string IsTransportUpdateAllowed = "IsTransportUpdateAllowed";
			public const string IsSupplierUpdateAllowed = "IsSupplierUpdateAllowed";
		}

		#endregion

		#region Constructors

		public WarehouseReceiveMilestoneEventUpdates() : base() { }

		public WarehouseReceiveMilestoneEventUpdates(ZString eventType)
			: base(eventType)
		{
		}

		#endregion

		#region Properties

		#region IsSupplierUpdateAllowed

		public ZBool IsSupplierUpdateAllowed
		{
			get { return GetValue(WebPartyType.Supplier); }
			set
			{
				SetValue(WebPartyType.Supplier, value);
				IsSupplierUpdateAllowedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsSupplierUpdateAllowedInfo
		{
			get { return GetZPropertyInfo(Schema.IsSupplierUpdateAllowed); }
		}

		#endregion

		#region IsTransportUpdateAllowed

		public ZBool IsTransportUpdateAllowed
		{
			get { return GetValue(WebPartyType.Transport); }
			set
			{
				SetValue(WebPartyType.Transport, value);
				IsTransportUpdateAllowedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsTransportUpdateAllowedInfo
		{
			get { return GetZPropertyInfo(Schema.IsTransportUpdateAllowed); }
		}

		#endregion

		#region IsClientUpdateAllowed

		public ZBool IsClientUpdateAllowed
		{
			get { return GetValue(WebPartyType.Client); }
			set
			{
				SetValue(WebPartyType.Client, value);
				IsClientUpdateAllowedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsClientUpdateAllowedInfo
		{
			get { return GetZPropertyInfo(Schema.IsClientUpdateAllowed); }
		}

		#endregion

		#endregion

		protected override ZString GetWorkflowType()
		{
			return Constants.WebWorkflowType.WarehouseReceive;
		}

		protected override MilestoneEventUpdates GetNewMilestoneEventUpdatesCopy()
		{
			return new WarehouseReceiveMilestoneEventUpdates(EventType);
		}

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.IsClientUpdateAllowed, IsClientUpdateAllowed.ToString());
			writer.WriteElementString(Schema.IsTransportUpdateAllowed, IsTransportUpdateAllowed.ToString());
			writer.WriteElementString(Schema.IsSupplierUpdateAllowed, IsSupplierUpdateAllowed.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			base.ReadElements(reader);
			IsClientUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsClientUpdateAllowed);
			IsTransportUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsTransportUpdateAllowed);
			IsSupplierUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsSupplierUpdateAllowed);
		}

		#endregion
	}
}
