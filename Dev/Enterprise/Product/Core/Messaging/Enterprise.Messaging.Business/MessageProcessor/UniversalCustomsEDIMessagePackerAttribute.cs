using System;
using System.Xml.Serialization;
using CargoWise.Definitions;

namespace Enterprise.Messaging.Business.MessageProcessor
{
	[Serializable]
	[XmlSerializerAssembly("Enterprise.Messaging.Business.XmlSerializers")]
	public sealed class UniversalCustomsEDIMessagePackerAttribute
		: AssemblyMetaDataAttributeWithTypeAndApplicationCode
	{
		public UniversalCustomsEDIMessagePackerAttribute()
		{
		}

		public UniversalCustomsEDIMessagePackerAttribute(string applicationCode, Type type)
			: base(applicationCode, type)
		{
		}
	}
}
