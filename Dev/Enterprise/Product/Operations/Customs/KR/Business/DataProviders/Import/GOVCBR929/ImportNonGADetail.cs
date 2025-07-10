using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.KR.Business.XmlSerializers")]
	public class ImportNonGADetail : IImportNonGADetail
	{
		public int SequenceNo { get; set; }
		public string ReasonType { get; set; }
		public string RegulationCategoryCode { get; set; }
		public string NonGAReasonType { get; set; }
		public string Reason { get; set; }

		ZInt IImportNonGADetail.SequenceNo => SequenceNo;
		ZString IImportNonGADetail.ReasonType => ReasonType;
		ZString IImportNonGADetail.RegulationCategoryCode => RegulationCategoryCode;
		ZString IImportNonGADetail.NonGAReasonType => NonGAReasonType;
		ZString IImportNonGADetail.Reason => Reason;
	}
}
