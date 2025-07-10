using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ShippingPortsMessagingEHubIDCollection : RegistryBusinessObjectCollectionTemplate
	{
		[System.Diagnostics.DebuggerStepThrough]
		public new ShippingPortsMessagingEHubID AddNew()
		{
			return (ShippingPortsMessagingEHubID)base.AddNew();
		}

		public new ShippingPortsMessagingEHubID this[int index]
		{
			get { return (ShippingPortsMessagingEHubID)Elements[index]; }
		}

		#region NewWithDefaultValues

		public static ShippingPortsMessagingEHubIDCollection NewWithDefaultValues()
		{
			var result = new ShippingPortsMessagingEHubIDCollection();

			AddValue(result, "ESBCN", ModuleTypes.Codes.xHub, ShippingPortsMessagingEHubID.ShippingPortMessagingForXHub);
			AddValue(result, "ESGAN", ModuleTypes.Codes.xHub, ShippingPortsMessagingEHubID.ShippingPortMessagingForXHub);
			AddValue(result, "ESPDS", ModuleTypes.Codes.xHub, ShippingPortsMessagingEHubID.ShippingPortMessagingForXHub);
			AddValue(result, "ESVLC", ModuleTypes.Codes.xHub, ShippingPortsMessagingEHubID.ShippingPortMessagingForXHub);
			AddValue(result, ZString.Empty, ModuleTypes.Codes.eHub, ShippingPortsMessagingEHubID.ShippingPortMessaging);

			return result;
		}

		static void AddValue(ShippingPortsMessagingEHubIDCollection collection, ZString port, ZString module, ZString recipientID)
		{
			var value = collection.AddNew();
			value.Port = port;
			value.Module = module;
			value.RecipientID = recipientID;
		}

		#endregion

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ShippingPortsMessagingEHubID();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ShippingPortsMessagingEHubIDCollection();
		}

		#endregion
	}
}
