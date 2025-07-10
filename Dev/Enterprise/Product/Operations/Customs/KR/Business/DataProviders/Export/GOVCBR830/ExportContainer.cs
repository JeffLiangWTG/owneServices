using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.KR.Business.XmlSerializers")]
	public class ExportContainer : IExportContainer
	{
		public string SequenceNo { get; set; }
		public string ContainerNo { get; set; }

		ZString IExportContainer.SequenceNo => SequenceNo;
		ZString IExportContainer.ContainerNo => ContainerNo;
	}
}
