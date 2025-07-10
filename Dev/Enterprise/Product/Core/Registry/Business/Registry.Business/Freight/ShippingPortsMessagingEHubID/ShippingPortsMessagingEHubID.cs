using System.Linq;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ShippingPortsMessagingEHubID : AutoShippingPortsMessagingEHubID
	{
		#region Properties

		[List("Lookups.PortList")]
		public override ZString Port { get => base.Port; set => base.Port = value; }

		[List("Lookups.ModuleTypeList")]
		public override ZString Module
		{
			get
			{
				return base.Module;
			}
			set
			{
				base.Module = value;
				if (RecipientID.IsEmpty)
				{
					RecipientID = Module == ModuleTypes.Codes.eHub ? ShippingPortMessaging : ShippingPortMessagingForXHub;
				}
			}
		}

		public const string ShippingPortMessagingForXHub = "SHIPPING_PORT_MESSAGING";
		public const string ShippingPortMessaging = "ShippingPortMessaging";

		#endregion

		#region Parent

		[BusinessObjectTestExclude]
		public ShippingPortsMessagingEHubIDCollection Parent
		{
			get { return (ShippingPortsMessagingEHubIDCollection)GetParentCollection(this, typeof(ShippingPortsMessagingEHubIDCollection)); }
		}

		#endregion

		#region Validation

		public override void ValidatePort()
		{
			base.ValidatePort();
			ListValidation.ErrorIfInvalidCode(PortInfo, Lookups.PortList);
			CheckUniqueness(PortInfo);
		}

		public override void ValidateModule()
		{
			base.ValidateModule();
			MandatoryValidation.CheckEntered(ModuleInfo);
			ListValidation.ErrorIfInvalidCode(ModuleInfo, Lookups.ModuleTypeList);
			CheckUniqueness(ModuleInfo);
		}

		public override void ValidateRecipientID()
		{
			base.ValidateRecipientID();
			MandatoryValidation.CheckEntered(RecipientIDInfo);
		}

		void CheckUniqueness(ZPropertyInfo property)
		{
			if (Parent != null && Parent.Cast<ShippingPortsMessagingEHubID>().Count(x => x.Port == Port) >= 2)
			{
				property.AddError(Res.GetString("aa884341-44ca-401d-bf89-026094aa9d89", "The same Port cannot be duplicated."));
			}
		}

		#endregion

		#region Lookups

		public ShippingPortsMessagingEHubIDLookups Lookups
		{
			get { return lookups ?? (lookups = new ShippingPortsMessagingEHubIDLookups(this, CurrentFactory)); }
		}
		ShippingPortsMessagingEHubIDLookups lookups;

		#endregion

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ShippingPortsMessagingEHubID();
		}

		#endregion
	}
}
