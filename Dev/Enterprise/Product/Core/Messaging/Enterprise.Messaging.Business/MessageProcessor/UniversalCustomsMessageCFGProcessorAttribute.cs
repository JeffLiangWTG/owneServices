using System;
using System.Xml.Serialization;
using CargoWise.Definitions;

namespace Enterprise.Messaging.Business.MessageProcessor
{
	[Serializable]
	[XmlSerializerAssembly("Enterprise.Messaging.Business.XmlSerializers")]
	public sealed class UniversalCustomsMessageCFGProcessorAttribute
		: AssemblyMetaDataAttributeWithTypeAndMessageType
	{
		public UniversalCustomsMessageCFGProcessorAttribute()
		{
		}

		public UniversalCustomsMessageCFGProcessorAttribute(string messageType, Type type)
			: base(messageType, type)
		{
		}
	}
}
