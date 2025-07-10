using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Messaging;

public class ImportLineSadFieldDescriptionsProvider : CustomsFieldDescriptionsProviderWithSequenceNumber<SadImportFieldDescriptionList>
{
	public ImportLineSadFieldDescriptionsProvider(BusinessObjectFactory factory) : base(factory) { }

	protected override IEnumerable<(string, string)> GetCorrelations()
	{
		#region Correlations

		yield return ("1", SadImportFieldDescriptionList.Codes.RecordType);
		yield return ("2", SadImportFieldDescriptionList.Codes.MessageCode);
		yield return ("5", SadImportFieldDescriptionList.Codes.AnnualSequenceNumber);
		yield return ("6", SadImportFieldDescriptionList.Codes.SequenceNumber);
		yield return ("8", SadImportFieldDescriptionList.Codes.ConsignorCountryOfFiscalCodeVatNumber);
		yield return ("9", SadImportFieldDescriptionList.Codes.ConsignorFiscalCodeVatNumber);
		yield return ("10", SadImportFieldDescriptionList.Codes.ConsignorName);
		yield return ("11", SadImportFieldDescriptionList.Codes.ConsignorStreetAndNumber);
		yield return ("12", SadImportFieldDescriptionList.Codes.ConsignorPostalCode);
		yield return ("13", SadImportFieldDescriptionList.Codes.ConsignorCity);
		yield return ("14", SadImportFieldDescriptionList.Codes.ConsignorCountryCode);
		yield return ("15", SadImportFieldDescriptionList.Codes.ConsigneeCountryOfFiscalCodeVatNumber);
		yield return ("16", SadImportFieldDescriptionList.Codes.ConsigneeFiscalCodeVatNumber);
		yield return ("17", SadImportFieldDescriptionList.Codes.ConsigneeName);
		yield return ("18", SadImportFieldDescriptionList.Codes.ConsigneeStreetAndNumber);
		yield return ("19", SadImportFieldDescriptionList.Codes.ConsigneePostalCode);
		yield return ("20", SadImportFieldDescriptionList.Codes.ConsigneeCity);
		yield return ("21", SadImportFieldDescriptionList.Codes.ConsigneeCountryCode);
		yield return ("22", SadImportFieldDescriptionList.Codes.CountryOfDispatch);
		yield return ("23", SadImportFieldDescriptionList.Codes.CountryOfDestinationCode);
		yield return ("24", SadImportFieldDescriptionList.Codes.PackageNumberOfPacks);
		yield return ("25", SadImportFieldDescriptionList.Codes.ContainersNumberOfOccurrences);
		yield return ("25.1", SadImportFieldDescriptionList.Codes.ContainerNumber);
		yield return ("25.2", SadImportFieldDescriptionList.Codes.ContainersTotalPartialUnloadContainerIndicator);
		yield return ("26", SadImportFieldDescriptionList.Codes.GoodsDescription);
		yield return ("27", SadImportFieldDescriptionList.Codes.PackageMarksAndNumbers);
		yield return ("28", SadImportFieldDescriptionList.Codes.PackagePackageKind);
		yield return ("29", SadImportFieldDescriptionList.Codes.PackageNumberOfPieces);
		yield return ("30", SadImportFieldDescriptionList.Codes.SgiCodesGoodsCode);
		yield return ("31", SadImportFieldDescriptionList.Codes.SgiCodesQuantity);
		yield return ("32", SadImportFieldDescriptionList.Codes.ItemNumber);
		yield return ("33", SadImportFieldDescriptionList.Codes.CombinedNomenclature);
		yield return ("34", SadImportFieldDescriptionList.Codes.AdditionalCodesNumberOfOccurrences);
		yield return ("34.1", SadImportFieldDescriptionList.Codes.AdditionalCode);
		yield return ("35", SadImportFieldDescriptionList.Codes.CountryOfOrigin);
		yield return ("36", SadImportFieldDescriptionList.Codes.GrossMass);
		yield return ("37", SadImportFieldDescriptionList.Codes.Preferences);
		yield return ("38", SadImportFieldDescriptionList.Codes.ProcedureCode);
		yield return ("39", SadImportFieldDescriptionList.Codes.NationalProceduresNumberOfOccurrences);
		yield return ("39.1", SadImportFieldDescriptionList.Codes.NationalProcedure);
		yield return ("40", SadImportFieldDescriptionList.Codes.NetMass);
		yield return ("41", SadImportFieldDescriptionList.Codes.QuotasNumberOfOccurrences);
		yield return ("41.1", SadImportFieldDescriptionList.Codes.Quota);
		yield return ("42", SadImportFieldDescriptionList.Codes.PreviousAdministrativeDocumentCategory);
		yield return ("43", SadImportFieldDescriptionList.Codes.PreviousAdministrativeDocumentType);
		yield return ("44", SadImportFieldDescriptionList.Codes.PreviousAdministrativeDocumentRegister);
		yield return ("45", SadImportFieldDescriptionList.Codes.PreviousAdministrativeDocumentReference);
		yield return ("46", SadImportFieldDescriptionList.Codes.PreviousAdministrativeDocumentReferenceCin);
		yield return ("47", SadImportFieldDescriptionList.Codes.PreviousAdministrativeDocumentDate);
		yield return ("48", SadImportFieldDescriptionList.Codes.PreviousAdministrativeDocumentSeries);
		yield return ("49", SadImportFieldDescriptionList.Codes.PreviousAdministrativeDocumentCustomsOffice);
		yield return ("50", SadImportFieldDescriptionList.Codes.PreviousAdministrativeDocumentItemNumber);
		yield return ("51", SadImportFieldDescriptionList.Codes.PreviousAdministrativeDocumentMrn);
		yield return ("52", SadImportFieldDescriptionList.Codes.PreviousAdministrativeDocumentComplementOfInformation);
		yield return ("53", SadImportFieldDescriptionList.Codes.SupplementaryUnit);
		yield return ("54", SadImportFieldDescriptionList.Codes.ItemPriceEuro);
		yield return ("55", SadImportFieldDescriptionList.Codes.EvaluationMethod);
		yield return ("56", SadImportFieldDescriptionList.Codes.SpecialMentionFirstEoriCode);
		yield return ("57", SadImportFieldDescriptionList.Codes.SpecialMentionSecondEoriCode);
		yield return ("58", SadImportFieldDescriptionList.Codes.SpecialMentionPreviousInvoiceAmount);
		yield return ("59", SadImportFieldDescriptionList.Codes.SpecialMentionUnloadingDataCommodityCode);
		yield return ("60", SadImportFieldDescriptionList.Codes.SpecialMentionUnloadingDataQuantity);
		yield return ("61", SadImportFieldDescriptionList.Codes.SpecialMentionUnloadingDataSupplementaryUnit);
		yield return ("62", SadImportFieldDescriptionList.Codes.SpecialMentionPreviousProcedureRegister);
		yield return ("63", SadImportFieldDescriptionList.Codes.SpecialMentionPreviousProcedureReference);
		yield return ("64", SadImportFieldDescriptionList.Codes.SpecialMentionPreviousProcedureReferenceCin);
		yield return ("65", SadImportFieldDescriptionList.Codes.SpecialMentionPreviousProcedureDate);
		yield return ("66", SadImportFieldDescriptionList.Codes.SpecialMentionPreviousProcedureSeries);
		yield return ("67", SadImportFieldDescriptionList.Codes.SpecialMentionPreviousProcedureCustomsOffice);
		yield return ("68", SadImportFieldDescriptionList.Codes.SpecialMentionPreviousProcedureItemNumber);
		yield return ("69", SadImportFieldDescriptionList.Codes.SpecialMentionSteelType);
		yield return ("70", SadImportFieldDescriptionList.Codes.CertificatesNumberOfOccurrences);
		yield return ("70.1", SadImportFieldDescriptionList.Codes.CertificatesDocumentType);
		yield return ("70.2", SadImportFieldDescriptionList.Codes.CertificatesCountryOfIssue);
		yield return ("70.3", SadImportFieldDescriptionList.Codes.CertificatesIssuingYear);
		yield return ("70.4", SadImportFieldDescriptionList.Codes.CertificatesReference);
		yield return ("70.5", SadImportFieldDescriptionList.Codes.CertificatesQuantity);
		yield return ("70.6", SadImportFieldDescriptionList.Codes.CertificatesUnitOfMeasurement);
		yield return ("70.7", SadImportFieldDescriptionList.Codes.CertificatesDerogationFlag);
		yield return ("70.8", SadImportFieldDescriptionList.Codes.CertificatesRetrospectiveDerogationFlag);
		yield return ("71", SadImportFieldDescriptionList.Codes.UnitsOfMeasureNumberOfOccurrences);
		yield return ("71.1", SadImportFieldDescriptionList.Codes.UnitsOfMeasureLiquidationUnitOfMeasure);
		yield return ("71.2", SadImportFieldDescriptionList.Codes.UnitsOfMeasureLiquidationQuantity);
		yield return ("71.3", SadImportFieldDescriptionList.Codes.UnitsOfMeasureLiquidationQualifier);
		yield return ("71.4", SadImportFieldDescriptionList.Codes.UnitsOfMeasureAdditionalLiquidationQualifier);
		yield return ("71.5", SadImportFieldDescriptionList.Codes.UnitsOfMeasureAdditionalLiquidationQuantity);
		yield return ("72", SadImportFieldDescriptionList.Codes.EntryPrice);
		yield return ("73", SadImportFieldDescriptionList.Codes.Notes);
		yield return ("74", SadImportFieldDescriptionList.Codes.AdjustmentInEuro);
		yield return ("75", SadImportFieldDescriptionList.Codes.StatisticalValueAmount);
		yield return ("76", SadImportFieldDescriptionList.Codes.SpecialMentionExportFromCe);
		yield return ("77", SadImportFieldDescriptionList.Codes.SpecialMentionExportFromCountry);
		yield return ("78", SadImportFieldDescriptionList.Codes.SpecialMentionExportFromCe);
		yield return ("79", SadImportFieldDescriptionList.Codes.SpecialMentionExportFromCountry);
		yield return ("80", SadImportFieldDescriptionList.Codes.TaxesCalculationNumberOfOccurrences);
		yield return ("80.1", SadImportFieldDescriptionList.Codes.TaxesCalculationType);
		yield return ("80.2", SadImportFieldDescriptionList.Codes.TaxesCalculationBase);
		yield return ("80.3", SadImportFieldDescriptionList.Codes.TaxesCalculationCalculationFactor1);
		yield return ("80.4", SadImportFieldDescriptionList.Codes.TaxesCalculationRate1);
		yield return ("80.5", SadImportFieldDescriptionList.Codes.TaxesCalculationCalculationFactor2);
		yield return ("80.6", SadImportFieldDescriptionList.Codes.TaxesCalculationRate2);
		yield return ("80.7", SadImportFieldDescriptionList.Codes.TaxesCalculationCalculationFactor3);
		yield return ("80.8", SadImportFieldDescriptionList.Codes.TaxesCalculationRate3);
		yield return ("80.9", SadImportFieldDescriptionList.Codes.TaxesCalculationCalculationFactor4);
		yield return ("80.10", SadImportFieldDescriptionList.Codes.TaxesCalculationAmount);
		yield return ("80.11", SadImportFieldDescriptionList.Codes.TaxesCalculationMethodOfPayment);
		yield return ("81", SadImportFieldDescriptionList.Codes.TotalItemTaxedAmount);
		yield return ("82", SadImportFieldDescriptionList.Codes.GrandTotalTaxedAmount);

		#endregion
	}
}
