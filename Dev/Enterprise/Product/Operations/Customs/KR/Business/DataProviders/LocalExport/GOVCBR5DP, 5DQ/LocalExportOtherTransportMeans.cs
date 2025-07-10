using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.KR.Business.XmlSerializers")]
	public class LocalExportOtherTransportMeans : ILocalExportOtherTransportMeans
	{
		public int SequenceNo { get; set; }
		public string WorkingVesselName { get; set; }
		public string WorkingVesselLloydsNumber { get; set; }
		public string TransportVehicleRegNo { get; set; }

		ZInt ILocalExportOtherTransportMeans.SequenceNo => SequenceNo;
		ZString ILocalExportOtherTransportMeans.WorkingVesselName => WorkingVesselName;
		ZString ILocalExportOtherTransportMeans.WorkingVesselLloydsNumber => WorkingVesselLloydsNumber;
		ZString ILocalExportOtherTransportMeans.TransportVehicleRegNo => TransportVehicleRegNo;
	}
}
