using System.Xml;
using System.Xml.Serialization;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class DeclarationMilestoneEventUpdates : MilestoneEventUpdates
	{
		#region Schema

		public new class Schema : MilestoneEventUpdates.Schema
		{
			public const string IsSupplierUpdateAllowed = "IsSupplierUpdateAllowed";
			public const string IsForwarderUpdateAllowed = "IsForwarderUpdateAllowed";
			public const string IsCarrierUpdateAllowed = "IsCarrierUpdateAllowed";
			public const string IsImporterUpdateAllowed = "IsImporterUpdateAllowed";
			public const string IsUltimateConsigneeUpdateAllowed = "IsUltimateConsigneeUpdateAllowed";
			public const string IsExternalBrokerUpdateAllowed = "IsExternalBrokerUpdateAllowed";
		}

		#endregion

		#region Constructors

		public DeclarationMilestoneEventUpdates() : base() { }

		public DeclarationMilestoneEventUpdates(ZString eventType)
			: base(eventType)
		{
		}

		#endregion

		#region Properties

		#region New

		public new DeclarationMilestoneEventUpdatesCollection Parent
		{
			get
			{
				return (DeclarationMilestoneEventUpdatesCollection)base.Parent;
			}
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

		#region IsForwarderUpdateAllowed

		public ZBool IsForwarderUpdateAllowed
		{
			get { return GetValue(WebPartyType.Forwarder); }
			set
			{
				SetValue(WebPartyType.Forwarder, value);
				IsForwarderUpdateAllowedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsForwarderUpdateAllowedInfo
		{
			get { return GetZPropertyInfo(Schema.IsForwarderUpdateAllowed); }
		}

		#endregion

		#region IsExternalBrokerUpdateAllowed

		public ZBool IsExternalBrokerUpdateAllowed
		{
			get { return GetValue(WebPartyType.ExternalBroker); }
			set
			{
				SetValue(WebPartyType.ExternalBroker, value);
				IsExternalBrokerUpdateAllowedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsExternalBrokerUpdateAllowedInfo
		{
			get { return GetZPropertyInfo(Schema.IsExternalBrokerUpdateAllowed); }
		}

		#endregion

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

		#region IsUltimateConsigneeUpdateAllowed

		public ZBool IsUltimateConsigneeUpdateAllowed
		{
			get { return GetValue(WebPartyType.UltimateConsignee); }
			set
			{
				SetValue(WebPartyType.UltimateConsignee, value);
				IsUltimateConsigneeUpdateAllowedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsUltimateConsigneeUpdateAllowedInfo
		{
			get { return GetZPropertyInfo(Schema.IsUltimateConsigneeUpdateAllowed); }
		}

		#endregion

		#endregion

		protected override ZString GetWorkflowType()
		{
			return Constants.WebWorkflowType.Declaration;
		}

		protected override MilestoneEventUpdates GetNewMilestoneEventUpdatesCopy()
		{
			return new DeclarationMilestoneEventUpdates(EventType);
		}

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.IsSupplierUpdateAllowed, IsSupplierUpdateAllowed.ToString());
			writer.WriteElementString(Schema.IsForwarderUpdateAllowed, IsForwarderUpdateAllowed.ToString());
			writer.WriteElementString(Schema.IsCarrierUpdateAllowed, IsCarrierUpdateAllowed.ToString());
			writer.WriteElementString(Schema.IsImporterUpdateAllowed, IsImporterUpdateAllowed.ToString());
			writer.WriteElementString(Schema.IsUltimateConsigneeUpdateAllowed, IsUltimateConsigneeUpdateAllowed.ToString());
			writer.WriteElementString(Schema.IsExternalBrokerUpdateAllowed, IsExternalBrokerUpdateAllowed.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			base.ReadElements(reader);
			IsSupplierUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsSupplierUpdateAllowed);
			IsForwarderUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsForwarderUpdateAllowed);
			IsCarrierUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsCarrierUpdateAllowed);
			IsImporterUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsImporterUpdateAllowed);
			IsUltimateConsigneeUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsUltimateConsigneeUpdateAllowed);
			IsExternalBrokerUpdateAllowed = reader.ReadElementStringAsZBool(Schema.IsExternalBrokerUpdateAllowed);
		}

		#endregion
	}
}
