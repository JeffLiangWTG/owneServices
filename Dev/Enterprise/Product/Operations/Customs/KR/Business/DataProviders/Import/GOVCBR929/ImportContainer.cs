using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.KR.Business.XmlSerializers")]
	public class ImportContainer : IImportContainer
	{
		public int SequenceNo { get; set; }
		public string ContainerNo { get; set; }

		ZInt IImportContainer.SequenceNo => SequenceNo;
		ZString IImportContainer.ContainerNo => ContainerNo;
	}
}
