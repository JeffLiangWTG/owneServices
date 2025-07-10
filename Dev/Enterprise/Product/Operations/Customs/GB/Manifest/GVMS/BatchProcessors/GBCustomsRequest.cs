using System;
using System.Xml.Serialization;
using CargoWise.Customs.GB.MessageDefinitions;

namespace Enterprise.Customs.GB.GVMS
{
	[Serializable]
	[XmlSerializerAssembly("Enterprise.Customs.GB.GVMS.XmlSerializers")]
	public partial class GBCustomsRequest
	{
		public ProviderType Provider { get; set; }

		public ServiceType Service { get; set; }

		public Credentials Credentials { get; set; }

		[XmlElement(IsNullable = true)]
		public string JobNumber { get; set; }

		public string ServiceReference { get; set; }

		public string Version { get; set; }

		public string ContentType { get; set; }
	}

	[Serializable]
	public enum ServiceType
	{
		Create,
		Update,
		Delete,
		Finalise
	}
}
