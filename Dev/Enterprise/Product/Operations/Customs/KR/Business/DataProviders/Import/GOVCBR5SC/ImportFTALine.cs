using System;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.KR.Business.XmlSerializers")]
	public class ImportFTALine : IImportFTALine
	{
		public int EntryLineNo { get; set; }
		public int SequenceNo { get; set; }
		public string HSCode { get; set; }
		public string CertificateOfOriginProductType { get; set; }
		public string AdditionalInvoiceIssuedInThirdCountryYN { get; set; }
		public string AdditionalInvoiceIssuingThirdCountryCode { get; set; }
		public string CertificateOfOriginExporterNumber { get; set; }
		public string AssociatedCOOIssuingCountryCode { get; set; }
		public string CountryOfOrigin { get; set; }
		public DateTime CertificateOfOriginIssueDate { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.Weight)]
		public decimal CertificateOfOriginTotalNetWeight { get; set; }
		public int CertificateOfOriginSplitOrder { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.Weight)]
		public decimal NetWeight { get; set; }
		public string CertificateOfOriginNo { get; set; }
		public string CertifiticateOfOriginIssuingAgencyType { get; set; }
		public string CertificateOfOriginAgencyName { get; set; }
		public string CountryOfOriginSupportingDocType { get; set; }
		public string CertifiticateOfOriginIssuerType { get; set; }
		public string DutyRateCode { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.DutyRate)]
		public decimal TariffRate { get; set; }
		public Organisation Manufacturer { get; set; }

		ZInt IImportFTALine.EntryLineNo => EntryLineNo;
		ZInt IImportFTALine.SequenceNo => SequenceNo;
		ZString IImportFTALine.HSCode => HSCode;
		ZString IImportFTALine.CertificateOfOriginProductType => CertificateOfOriginProductType;
		ZString IImportFTALine.AdditionalInvoiceIssuedInThirdCountryYN => AdditionalInvoiceIssuedInThirdCountryYN;
		ZString IImportFTALine.AdditionalInvoiceIssuingThirdCountryCode => AdditionalInvoiceIssuingThirdCountryCode;
		ZString IImportFTALine.CertificateOfOriginExporterNumber => CertificateOfOriginExporterNumber;
		ZString IImportFTALine.AssociatedCOOIssuingCountryCode => AssociatedCOOIssuingCountryCode;
		ZString IImportFTALine.CountryOfOrigin => CountryOfOrigin;
		ZDate IImportFTALine.CertificateOfOriginIssueDate => (ZDate)CertificateOfOriginIssueDate;
		ZDecimal IImportFTALine.CertificateOfOriginTotalNetWeight => CertificateOfOriginTotalNetWeight;
		ZInt IImportFTALine.CertificateOfOriginSplitOrder => CertificateOfOriginSplitOrder;
		ZDecimal IImportFTALine.NetWeight => NetWeight;
		ZString IImportFTALine.CertificateOfOriginNo => CertificateOfOriginNo;
		ZString IImportFTALine.CertifiticateOfOriginIssuingAgencyType => CertifiticateOfOriginIssuingAgencyType;
		ZString IImportFTALine.CertificateOfOriginAgencyName => CertificateOfOriginAgencyName;
		ZString IImportFTALine.CountryOfOriginSupportingDocType => CountryOfOriginSupportingDocType;
		ZString IImportFTALine.CertifiticateOfOriginIssuerType => CertifiticateOfOriginIssuerType;
		ZString IImportFTALine.DutyRateCode => DutyRateCode;
		ZDecimal IImportFTALine.TariffRate => TariffRate;
		IOrganization IImportFTALine.Manufacturer => Manufacturer;
	}
}
