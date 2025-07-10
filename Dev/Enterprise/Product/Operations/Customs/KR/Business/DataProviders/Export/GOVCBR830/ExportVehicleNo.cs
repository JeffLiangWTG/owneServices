using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.KR.Business.XmlSerializers")]
	public class ExportVehicleNo : IExportVehicleNo
	{
		public string SequenceNo { get; set; }
		public string VIN { get; set; }

		ZString IExportVehicleNo.SequenceNo => SequenceNo;
		ZString IExportVehicleNo.VIN => VIN;
	}
}
