using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImportFTALine
	{
		[ID()]
		[DataItemID(FTAAmendmentDataItemIDList.Codes._10)]
		ZInt EntryLineNo { get; }
		ZInt SequenceNo { get; }
		[DataItemID(FTAAmendmentDataItemIDList.Codes._19)]
		ZString HSCode { get; }
		[DataItemID(FTAAmendmentDataItemIDList.Codes._21)]
		ZString CertificateOfOriginProductType { get; }
		[DataItemID(FTAAmendmentDataItemIDList.Codes._28)]
		ZString AdditionalInvoiceIssuedInThirdCountryYN { get; }
		[DataItemID(FTAAmendmentDataItemIDList.Codes._22)]
		ZString AdditionalInvoiceIssuingThirdCountryCode { get; }
		[DataItemID(FTAAmendmentDataItemIDList.Codes._23)]
		ZString CertificateOfOriginExporterNumber { get; }
		[DataItemID(FTAAmendmentDataItemIDList.Codes._17)]
		ZString AssociatedCOOIssuingCountryCode { get; }
		[DataItemID(FTAAmendmentDataItemIDList.Codes._24)]
		ZString CountryOfOrigin { get; }
		[DataItemID(FTAAmendmentDataItemIDList.Codes._11F)]
		ZDate CertificateOfOriginIssueDate { get; }
		[DataItemID(FTAAmendmentDataItemIDList.Codes._12)]
		ZDecimal CertificateOfOriginTotalNetWeight { get; }
		[DataItemID(FTAAmendmentDataItemIDList.Codes._18B)]
		ZInt CertificateOfOriginSplitOrder { get; }
		[DataItemID(FTAAmendmentDataItemIDList.Codes._25)]
		ZDecimal NetWeight { get; }
		[DataItemID(FTAAmendmentDataItemIDList.Codes._11E)]
		ZString CertificateOfOriginNo { get; }
		[DataItemID(FTAAmendmentDataItemIDList.Codes._11D)]
		ZString CertifiticateOfOriginIssuingAgencyType { get; }
		[DataItemID(FTAAmendmentDataItemIDList.Codes._11C)]
		ZString CertificateOfOriginAgencyName { get; }
		[DataItemID(FTAAmendmentDataItemIDList.Codes._11A)]
		ZString CountryOfOriginSupportingDocType { get; }
		[DataItemID(FTAAmendmentDataItemIDList.Codes._11B)]
		ZString CertifiticateOfOriginIssuerType { get; }
		[DataItemID(FTAAmendmentDataItemIDList.Codes._20A)]
		ZString DutyRateCode { get; }
		[DataItemID(FTAAmendmentDataItemIDList.Codes._20B)]
		ZDecimal TariffRate { get; }
		IOrganization Manufacturer { get; }
	}
}
