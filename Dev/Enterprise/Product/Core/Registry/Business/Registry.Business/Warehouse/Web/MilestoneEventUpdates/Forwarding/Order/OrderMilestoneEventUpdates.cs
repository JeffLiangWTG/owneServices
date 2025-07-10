using System.Xml;
using System.Xml.Serialization;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class OrderMilestoneEventUpdates : MilestoneEventUpdates
	{
		#region Schema

		public new class Schema : MilestoneEventUpdates.Schema
		{
			public const string IsOrderedByUpdateAllowed = "IsOrderedByUpdateAllowed";
			public const string IsSupplierUpdateAllowed = "IsSupplierUpdateAllowed";
			public const string IsSendingAgentUpdateAllowed = "IsSendingAgentUpdateAllowed";
			public const string IsReceivingAgentUpdateAllowed = "IsReceivingAgentUpdateAllowed";
		}

		#endregion

		#region Constructors

		public OrderMilestoneEventUpdates() : base() { }

		public OrderMilestoneEventUpdates(ZString eventType)
			: base(eventType)
		{
		}

		#endregion

		#region Properties

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

		#region IsOrderedByUpdateAllowed

		public ZBool IsOrderedByUpdateAllowed
		{
			get { return GetValue(WebPartyType.OrderedBy); }
			set
			{
				SetValue(WebPartyType.OrderedBy, value);
				IsOrderedByUpdateAllowedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsOrderedByUpdateAllowedInfo
		{
			get { return GetZPropertyInfo(Schema.IsOrderedByUpdateAllowed); }
		}

		#endregion

		#endregion

		protected override ZString GetWorkflowType()
		{
			return Constants.WebWorkflowType.Order;
		}

		protected override MilestoneEventUpdates GetNewMilestoneEventUpdatesCopy()
		{
			return new OrderMilestoneEventUpdates(EventType);
		}

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.IsOrderedByUpdateAllowed, IsOrderedByUpdateAllowed.ToString());
			writer.WriteElementString(Schema.IsSupplierUpdateAllowed, IsSupplierUpdateAllowed.ToString());
			writer.WriteElementString(Schema.IsSendingAgentUpdateAllowed, IsSendingAgentUpdateAllowed.ToString());
			writer.WriteElementString(Schema.IsReceivingAgentUpdateAllowed, IsReceivingAgentUpdateAllowed.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			base.ReadElements(reader);
			IsOrderedByUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsOrderedByUpdateAllowed);
			IsSupplierUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsSupplierUpdateAllowed);
			IsSendingAgentUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsSendingAgentUpdateAllowed);
			IsReceivingAgentUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsReceivingAgentUpdateAllowed);
		}

		#endregion
	}
}
