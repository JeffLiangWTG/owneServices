using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Messaging;

public class ExportLineSadFieldDescriptionsProvider : CustomsFieldDescriptionsProviderWithSequenceNumber<SadExportFieldDescriptionList>
{
	public ExportLineSadFieldDescriptionsProvider(BusinessObjectFactory factory) : base(factory) { }

	protected override IEnumerable<(string, string)> GetCorrelations()
	{
		#region Correlations

		yield return ("7", SadExportFieldDescriptionList.Codes.TypeOfDeclarationNctsEcs);
		yield return ("8", SadExportFieldDescriptionList.Codes.ConsignorEoriCode);
		yield return ("9", SadExportFieldDescriptionList.Codes.ConsignorName);
		yield return ("10", SadExportFieldDescriptionList.Codes.ConsignorStreetAndNumber);
		yield return ("11", SadExportFieldDescriptionList.Codes.ConsignorPostalCode);
		yield return ("12", SadExportFieldDescriptionList.Codes.ConsignorCity);
		yield return ("13", SadExportFieldDescriptionList.Codes.ConsignorCountryCode);
		yield return ("14", SadExportFieldDescriptionList.Codes.ConsignorNadLng);
		yield return ("15", SadExportFieldDescriptionList.Codes.ConsigneeEoriCode);
		yield return ("16", SadExportFieldDescriptionList.Codes.ConsigneeName);
		yield return ("17", SadExportFieldDescriptionList.Codes.ConsigneeStreetAndNumber);
		yield return ("18", SadExportFieldDescriptionList.Codes.ConsigneePostalCode);
		yield return ("19", SadExportFieldDescriptionList.Codes.ConsigneeCity);
		yield return ("20", SadExportFieldDescriptionList.Codes.ConsigneeCountryCode);
		yield return ("21", SadExportFieldDescriptionList.Codes.ConsigneeNadLng);
		yield return ("22", SadExportFieldDescriptionList.Codes.CountryOfDispatch);
		yield return ("23", SadExportFieldDescriptionList.Codes.CountryOfDestinationCode);
		yield return ("24", SadExportFieldDescriptionList.Codes.ConsignorSecurityTraderEoriCode);
		yield return ("25", SadExportFieldDescriptionList.Codes.ConsignorSecurityTraderName);
		yield return ("26", SadExportFieldDescriptionList.Codes.ConsignorSecurityTraderStreetAndNumber);
		yield return ("27", SadExportFieldDescriptionList.Codes.ConsignorSecurityTraderPostalCode);
		yield return ("28", SadExportFieldDescriptionList.Codes.ConsignorSecurityTraderCity);
		yield return ("29", SadExportFieldDescriptionList.Codes.ConsignorSecurityTraderCountryCode);
		yield return ("30", SadExportFieldDescriptionList.Codes.ConsignorSecurityTraderNadLng);
		yield return ("31", SadExportFieldDescriptionList.Codes.ConsigneeSecurityTraderEoriCode);
		yield return ("32", SadExportFieldDescriptionList.Codes.ConsigneeSecurityTraderName);
		yield return ("33", SadExportFieldDescriptionList.Codes.ConsigneeSecurityTraderStreetAndNumber);
		yield return ("34", SadExportFieldDescriptionList.Codes.ConsigneeSecurityTraderPostalCode);
		yield return ("35", SadExportFieldDescriptionList.Codes.ConsigneeSecurityTraderCity);
		yield return ("36", SadExportFieldDescriptionList.Codes.ConsigneeSecurityTraderCountryCode);
		yield return ("37", SadExportFieldDescriptionList.Codes.ConsigneeSecurityTraderNadLng);
		yield return ("38", SadExportFieldDescriptionList.Codes.TransportChargesMethodOfPayment);
		yield return ("39", SadExportFieldDescriptionList.Codes.CommercialReferenceNumber);
		yield return ("40", SadExportFieldDescriptionList.Codes.PackageDeclaredNumberOfPackages);
		yield return ("40.1", SadExportFieldDescriptionList.Codes.PackageNumberOfPackages);
		yield return ("40.2", SadExportFieldDescriptionList.Codes.PackageMarksAndNumbersOfPackages);
		yield return ("40.3", SadExportFieldDescriptionList.Codes.PackageMarksAndNumbersOfPackagesLng);
		yield return ("40.4", SadExportFieldDescriptionList.Codes.PackageKindOfPackages);
		yield return ("40.5", SadExportFieldDescriptionList.Codes.PackageNumberOfPieces);
		yield return ("41.1", SadExportFieldDescriptionList.Codes.ContainerContainerNumber);
		yield return ("41.2", SadExportFieldDescriptionList.Codes.ContainerIndicationOfPartialTotalContainerUnload);
		yield return ("42", SadExportFieldDescriptionList.Codes.ContainerGoodsDescription);
		yield return ("43", SadExportFieldDescriptionList.Codes.ContainerGoodsDescriptionLng);
		yield return ("44", SadExportFieldDescriptionList.Codes.SgiCodesSensitiveGoodsCode);
		yield return ("45", SadExportFieldDescriptionList.Codes.SgiCodesSensitiveQuantity);
		yield return ("46", SadExportFieldDescriptionList.Codes.SgiCodesUnDangerousGoodsCode);
		yield return ("47", SadExportFieldDescriptionList.Codes.ItemNumber);
		yield return ("48", SadExportFieldDescriptionList.Codes.CombinedNomenclature);
		yield return ("49", SadExportFieldDescriptionList.Codes.CommodityCodeTaricCode);
		yield return ("50", SadExportFieldDescriptionList.Codes.AdditionalCode);
		yield return ("50.1", SadExportFieldDescriptionList.Codes.CommodityCodeTaricFirstAdditionalCode);
		yield return ("51", SadExportFieldDescriptionList.Codes.CountryOfOrigin);
		yield return ("52", SadExportFieldDescriptionList.Codes.GrossMass);
		yield return ("53", SadExportFieldDescriptionList.Codes.ProcedureCode);
		yield return ("54", SadExportFieldDescriptionList.Codes.NationalProceduresNumberOfOccurrences);
		yield return ("54.1", SadExportFieldDescriptionList.Codes.CommunityNationalProcedure);
		yield return ("55", SadExportFieldDescriptionList.Codes.NetMass);
		yield return ("56", SadExportFieldDescriptionList.Codes.PreviousAdministrativeDocumentCategory);
		yield return ("57", SadExportFieldDescriptionList.Codes.PreviousAdministrativeDocumentType);
		yield return ("58", SadExportFieldDescriptionList.Codes.PreviousAdministrativeDocumentRegister);
		yield return ("59", SadExportFieldDescriptionList.Codes.PreviousAdministrativeDocumentReference);
		yield return ("61", SadExportFieldDescriptionList.Codes.PreviousAdministrativeDocumentReferenceCin);
		yield return ("62", SadExportFieldDescriptionList.Codes.PreviousAdministrativeDocumentDate);
		yield return ("63", SadExportFieldDescriptionList.Codes.TransportChargesMethodOfPayment);
		yield return ("64", SadExportFieldDescriptionList.Codes.PreviousAdministrativeDocumentCustomsOffice);
		yield return ("65", SadExportFieldDescriptionList.Codes.PreviousAdministrativeDocumentItemNumber);
		yield return ("66", SadExportFieldDescriptionList.Codes.PreviousAdministrativeDocumentMrn);
		yield return ("67", SadExportFieldDescriptionList.Codes.PreviousAdministrativeDocumentComplementOfInformation);
		yield return ("68", SadExportFieldDescriptionList.Codes.PreviousAdministrativeReferenceComplementOfInformationLng);
		yield return ("69", SadExportFieldDescriptionList.Codes.SupplementaryUnit);
		yield return ("70", SadExportFieldDescriptionList.Codes.SpecialMentionFirstEoriCode);
		yield return ("71", SadExportFieldDescriptionList.Codes.SpecialMentionSecondEoriCode);
		yield return ("72", SadExportFieldDescriptionList.Codes.SpecialMentionPreviousInvoiceAmount);
		yield return ("73", SadExportFieldDescriptionList.Codes.SpecialMentionUnloadingDataCommodityCode);
		yield return ("74", SadExportFieldDescriptionList.Codes.SpecialMentionUnloadingDataQuantity);
		yield return ("75", SadExportFieldDescriptionList.Codes.SpecialMentionUnloadingDataSupplementaryUnit);
		yield return ("76", SadExportFieldDescriptionList.Codes.ProductDocumentCertificatesDeclaredNumberOfDocuments);
		yield return ("76.1", SadExportFieldDescriptionList.Codes.ProductDocumentCertificatesDocumentType);
		yield return ("76.2", SadExportFieldDescriptionList.Codes.ProductDocumentCertificatesDocumentCountryOfIssue);
		yield return ("76.3", SadExportFieldDescriptionList.Codes.ProductDocumentCertificatesDocumentYearOfIssue);
		yield return ("76.4", SadExportFieldDescriptionList.Codes.ProductDocumentCertificatesDocumentReference);
		yield return ("76.5", SadExportFieldDescriptionList.Codes.ProductDocumentCertificatesDocumentReferenceLng);
		yield return ("76.6", SadExportFieldDescriptionList.Codes.ProductDocumentCertificatesDocumentReferredQuantity);
		yield return ("76.7", SadExportFieldDescriptionList.Codes.ProductDocumentCertificatesDocumentReferredUnitOfMeasure);
		yield return ("76.8", SadExportFieldDescriptionList.Codes.ProductDocumentCertificatesDerogationFlag);
		yield return ("76.9", SadExportFieldDescriptionList.Codes.ProductDocumentCertificatesLatePresentationFlag);
		yield return ("77", SadExportFieldDescriptionList.Codes.SpecialMentionsAdditionalInformation);
		yield return ("78", SadExportFieldDescriptionList.Codes.SpecialMentionsAdditionalInformationLng);
		yield return ("79", SadExportFieldDescriptionList.Codes.SpecialMentionsAdditionalInformationCoded);
		yield return ("80", SadExportFieldDescriptionList.Codes.SpecialMentionsExportFromEc);
		yield return ("81", SadExportFieldDescriptionList.Codes.SpecialMentionsExportFromCountry);
		yield return ("82", SadExportFieldDescriptionList.Codes.SpecialMentionsComplementOfInformation);
		yield return ("83", SadExportFieldDescriptionList.Codes.SpecialMentionsComplementOfInformationLng);
		yield return ("84", SadExportFieldDescriptionList.Codes.SpecialMentionsNotes);
		yield return ("85", SadExportFieldDescriptionList.Codes.StatisticalValueAmount);
		yield return ("86", SadExportFieldDescriptionList.Codes.StatisticalValueCurrency);
		yield return ("87", SadImportFieldDescriptionList.Codes.TaxesCalculationNumberOfOccurrences);
		yield return ("87.1", SadImportFieldDescriptionList.Codes.TaxesCalculationType);
		yield return ("87.2", SadImportFieldDescriptionList.Codes.TaxesCalculationBase);
		yield return ("87.3", SadImportFieldDescriptionList.Codes.TaxesCalculationCalculationFactor1);
		yield return ("87.4", SadImportFieldDescriptionList.Codes.TaxesCalculationRate1);
		yield return ("87.5", SadImportFieldDescriptionList.Codes.TaxesCalculationCalculationFactor2);
		yield return ("87.6", SadImportFieldDescriptionList.Codes.TaxesCalculationRate2);
		yield return ("87.7", SadImportFieldDescriptionList.Codes.TaxesCalculationCalculationFactor3);
		yield return ("87.8", SadImportFieldDescriptionList.Codes.TaxesCalculationRate3);
		yield return ("87.9", SadImportFieldDescriptionList.Codes.TaxesCalculationCalculationFactor4);
		yield return ("87.10", SadImportFieldDescriptionList.Codes.TaxesCalculationAmount);
		yield return ("87.11", SadImportFieldDescriptionList.Codes.TaxesCalculationMethodOfPayment);
		yield return ("88", SadImportFieldDescriptionList.Codes.TotalItemTaxedAmount);
		yield return ("89", SadImportFieldDescriptionList.Codes.GrandTotalTaxedAmount);

		#endregion
	}
}
