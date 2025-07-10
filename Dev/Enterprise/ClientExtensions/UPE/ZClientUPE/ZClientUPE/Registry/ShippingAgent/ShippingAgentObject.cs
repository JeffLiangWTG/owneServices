using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.UPE.Registry.Business
{
	[XmlSerializerAssembly("ZClientUPE.XmlSerializers")]
	public class ShippingAgentObject : RegistryBusinessObjectTemplate
	{
		public ShippingAgentObject()
			: base()
		{
		}

		public static class Schema
		{
			public const string ShippingAgentAddress = "ShippingAgentAddress";
		}

		public ShippingAgentObjectWrapper Wrapper
		{
			get
			{
				if (wrapper == null)
				{
					wrapper = new ShippingAgentObjectWrapper(this);
					RegisterEditableChildObject(wrapper);
				}
				return wrapper;
			}
		}
		ShippingAgentObjectWrapper wrapper;

		public ZGuid ShippingAgentAddress
		{
			get => shippingAgentAddress;
			set => SetNonPersistentPropertyValue(ShippingAgentAddressInfo, ref shippingAgentAddress, value);
		}
		ZGuid shippingAgentAddress;

		public ZPropertyInfo ShippingAgentAddressInfo => GetZPropertyInfo(Schema.ShippingAgentAddress);

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new ShippingAgentObject();

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ShippingAgentAddress = new ZGuid(reader.ReadElementString(Schema.ShippingAgentAddress));
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.ShippingAgentAddress, ShippingAgentAddress.ToString());
		}
	}
}
