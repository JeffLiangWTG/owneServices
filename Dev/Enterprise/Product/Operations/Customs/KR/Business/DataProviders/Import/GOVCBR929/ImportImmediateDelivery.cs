using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.KR.Business.XmlSerializers")]
	public class ImportImmediateDelivery : IImportImmediateDelivery
	{
		public int SequenceNo { get; set; }

		public string ImmediateDeliveryNo { get; set; }

		ZInt IImportImmediateDelivery.SequenceNo => SequenceNo;

		ZString IImportImmediateDelivery.ImmediateDeliveryNo => ImmediateDeliveryNo;
	}
}
