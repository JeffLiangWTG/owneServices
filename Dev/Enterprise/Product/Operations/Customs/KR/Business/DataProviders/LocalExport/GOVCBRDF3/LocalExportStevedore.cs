using System;
using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.KR.Business.XmlSerializers")]
	public class LocalExportStevedore : ILocalExportStevedore
	{
		public int SequenceNo { get; set; }
		public string CompanyName { get; set; }
		public DateTime Birthday { get; set; }
		public string FullName { get; set; }
		public string RoadNameCode { get; set; }
		public string BuildingNumber { get; set; }
		public string Postcode { get; set; }
		public string AddressLine1 { get; set; }
		public string AddressLine2 { get; set; }
		public string PhoneNumber { get; set; }
		public string MobileNumber { get; set; }

		ZInt ILocalExportStevedore.SequenceNo => SequenceNo;
		ZString ILocalExportStevedore.CompanyName => CompanyName;
		ZDate ILocalExportStevedore.Birthday => new ZDate(Birthday);
		ZString ILocalExportStevedore.FullName => FullName;
		ZString ILocalExportStevedore.RoadNameCode => RoadNameCode;
		ZString ILocalExportStevedore.BuildingNumber => BuildingNumber;
		ZString ILocalExportStevedore.Postcode => Postcode;
		ZString ILocalExportStevedore.AddressLine1 => AddressLine1;
		ZString ILocalExportStevedore.AddressLine2 => AddressLine2;
		ZString ILocalExportStevedore.PhoneNumber => PhoneNumber;
		ZString ILocalExportStevedore.MobileNumber => MobileNumber;
	}
}
