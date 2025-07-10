using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Messaging;

public class ImportHeaderSadFieldDescriptionsProvider : CustomsFieldDescriptionsProviderWithSequenceNumber<SadImportFieldDescriptionList>
{
	public ImportHeaderSadFieldDescriptionsProvider(BusinessObjectFactory factory) : base(factory) { }

	protected override IEnumerable<(string, string)> GetCorrelations()
	{
		#region Correlations

		yield return ("1", SadImportFieldDescriptionList.Codes.RecordType);
		yield return ("2", SadImportFieldDescriptionList.Codes.MessageCode);
		yield return ("5", SadImportFieldDescriptionList.Codes.AnnualSequenceNumber);
		yield return ("6", SadImportFieldDescriptionList.Codes.SequenceNumber);
		yield return ("7", SadImportFieldDescriptionList.Codes.AuthorizationNumber);
		yield return ("8", SadImportFieldDescriptionList.Codes.AuthorizationCin);
		yield return ("9", SadImportFieldDescriptionList.Codes.CompanyRegisterNumber);
		yield return ("10", SadImportFieldDescriptionList.Codes.CompanyRegisterSeries);
		yield return ("11", SadImportFieldDescriptionList.Codes.CompanyRegisterDate);
		yield return ("12", SadImportFieldDescriptionList.Codes.NoticeNumber);
		yield return ("13", SadImportFieldDescriptionList.Codes.NoticeOrderNumber);
		yield return ("14", SadImportFieldDescriptionList.Codes.NoticeDate);
		yield return ("15", SadImportFieldDescriptionList.Codes.PreClearing);
		yield return ("16", SadImportFieldDescriptionList.Codes.DeclarationFirstSubdivision);
		yield return ("17", SadImportFieldDescriptionList.Codes.DeclarationSecondSubdivision);
		yield return ("18", SadImportFieldDescriptionList.Codes.DeclarationThirdSubdivision);
		yield return ("19", SadImportFieldDescriptionList.Codes.AcceptanceDate);
		yield return ("20", SadImportFieldDescriptionList.Codes.HeaderDataDeclaredAtArticleLevel);
		yield return ("21", SadImportFieldDescriptionList.Codes.TotalItems);
		yield return ("22", SadImportFieldDescriptionList.Codes.TotalNumberOfPacks);
		yield return ("23", SadImportFieldDescriptionList.Codes.ConsignorCountryOfFiscalCodeVatNumber);
		yield return ("24", SadImportFieldDescriptionList.Codes.ConsignorFiscalCodeVatNumber);
		yield return ("25", SadImportFieldDescriptionList.Codes.ConsignorName);
		yield return ("26", SadImportFieldDescriptionList.Codes.ConsignorStreetAndNumber);
		yield return ("27", SadImportFieldDescriptionList.Codes.ConsignorPostalCode);
		yield return ("28", SadImportFieldDescriptionList.Codes.ConsignorCity);
		yield return ("29", SadImportFieldDescriptionList.Codes.ConsignorCountryCode);
		yield return ("30", SadImportFieldDescriptionList.Codes.ReferenceNumber);
		yield return ("31", SadImportFieldDescriptionList.Codes.ConsigneeCountryOfFiscalCodeVatNumber);
		yield return ("32", SadImportFieldDescriptionList.Codes.ConsigneeFiscalCodeVatNumber);
		yield return ("33", SadImportFieldDescriptionList.Codes.ConsigneeName);
		yield return ("34", SadImportFieldDescriptionList.Codes.ConsigneeStreetAndNumber);
		yield return ("35", SadImportFieldDescriptionList.Codes.ConsigneePostalCode);
		yield return ("36", SadImportFieldDescriptionList.Codes.ConsigneeCity);
		yield return ("37", SadImportFieldDescriptionList.Codes.ConsigneeCountryCode);
		yield return ("38", SadImportFieldDescriptionList.Codes.DeliveryCosts);
		yield return ("39", SadImportFieldDescriptionList.Codes.DeclarantTraderRepresentativeStatusCode);
		yield return ("40", SadImportFieldDescriptionList.Codes.DeclarantTraderCountryOfFiscalCodeVatNumber);
		yield return ("41", SadImportFieldDescriptionList.Codes.DeclarantTraderFiscalCodeVatNumber);
		yield return ("42", SadImportFieldDescriptionList.Codes.DeclarantTraderName);
		yield return ("43", SadImportFieldDescriptionList.Codes.DeclarantTraderStreetAndNumber);
		yield return ("44", SadImportFieldDescriptionList.Codes.DeclarantTraderPostalCode);
		yield return ("45", SadImportFieldDescriptionList.Codes.DeclarantTraderCity);
		yield return ("46", SadImportFieldDescriptionList.Codes.DeclarantTraderCountryCode);
		yield return ("47", SadImportFieldDescriptionList.Codes.CountryOfDispatch);
		yield return ("48", SadImportFieldDescriptionList.Codes.CountryOfDestinationCode);
		yield return ("49", SadImportFieldDescriptionList.Codes.CountryOfDestinationProvince);
		yield return ("50", SadImportFieldDescriptionList.Codes.MeansOfTransportOnArrivalNationality);
		yield return ("51", SadImportFieldDescriptionList.Codes.MeansOfTransportOnArrivalIdentity);
		yield return ("52", SadImportFieldDescriptionList.Codes.ContainerizedIndicator);
		yield return ("53", SadImportFieldDescriptionList.Codes.TermsOfDeliveryIncotermCode);
		yield return ("54", SadImportFieldDescriptionList.Codes.TermsOfDeliveryComplementOfInfo);
		yield return ("55", SadImportFieldDescriptionList.Codes.TermsOfDeliveryComplementaryCode);
		yield return ("56", SadImportFieldDescriptionList.Codes.MeansOfTransportCrossingBorderNationality);
		yield return ("57", SadImportFieldDescriptionList.Codes.MeansOfTransportCrossingBorderIdentity);
		yield return ("58", SadImportFieldDescriptionList.Codes.DataTransactionCurrency);
		yield return ("59", SadImportFieldDescriptionList.Codes.DataTransactionTotalAmountInvoiced);
		yield return ("60", SadImportFieldDescriptionList.Codes.DataTransactionExchangeRate);
		yield return ("61", SadImportFieldDescriptionList.Codes.DataTransactionNatureOfTransactionCode);
		yield return ("62", SadImportFieldDescriptionList.Codes.TransportModeAtBorder);
		yield return ("63", SadImportFieldDescriptionList.Codes.InlandTransportMode);
		yield return ("64", SadImportFieldDescriptionList.Codes.PlaceOfLoadingCode);
		yield return ("65", SadImportFieldDescriptionList.Codes.EntryCustomsOfficeNationality);
		yield return ("66", SadImportFieldDescriptionList.Codes.EntryCustomsOfficeReferenceNumber);
		yield return ("67", SadImportFieldDescriptionList.Codes.EntryCustomsOfficeName);
		yield return ("68", SadImportFieldDescriptionList.Codes.LocationOfGoodsCodeAndCinPlaceOfExamination);
		yield return ("69", SadImportFieldDescriptionList.Codes.LocationOfGoodsCodeAndCinPlaceOfUnloading);
		yield return ("70", SadImportFieldDescriptionList.Codes.DeferredPaymentAuthorizationReference);
		yield return ("71", SadImportFieldDescriptionList.Codes.DeferredPaymentCinOfAuthorizationReference);
		yield return ("72", SadImportFieldDescriptionList.Codes.WarehouseIdentificationAssessmentProcedure);
		yield return ("73", SadImportFieldDescriptionList.Codes.WarehouseIdentificationType);
		yield return ("74", SadImportFieldDescriptionList.Codes.WarehouseIdentificationIdentification);
		yield return ("75", SadImportFieldDescriptionList.Codes.WarehouseIdentificationCinIdentification);
		yield return ("76", SadImportFieldDescriptionList.Codes.WarehouseIdentificationAuthorizingCountry);
		yield return ("77", SadImportFieldDescriptionList.Codes.WarehouseIdentificationControlCustomsOffice);
		yield return ("78", SadImportFieldDescriptionList.Codes.DateLimitOfTemporaryOperation);
		yield return ("79", SadImportFieldDescriptionList.Codes.PrincipalTraderCountryOfFiscalCodeVatNumber);
		yield return ("80", SadImportFieldDescriptionList.Codes.PrincipalTraderFiscalCodeVatNumber);
		yield return ("81", SadImportFieldDescriptionList.Codes.PrincipalTraderName);
		yield return ("82", SadImportFieldDescriptionList.Codes.PrincipalTraderStreetAndNumber);
		yield return ("83", SadImportFieldDescriptionList.Codes.PrincipalTraderPostalCode);
		yield return ("84", SadImportFieldDescriptionList.Codes.PrincipalTraderCity);
		yield return ("85", SadImportFieldDescriptionList.Codes.PrincipalTraderCountryCode);
		yield return ("86", SadImportFieldDescriptionList.Codes.PrincipalTraderRepresentativeName);
		yield return ("87", SadImportFieldDescriptionList.Codes.PrincipalTraderRepresentativeType);
		yield return ("88", SadImportFieldDescriptionList.Codes.TransitCustomsOfficeNumberOfOccurrences);
		yield return ("88.1", SadImportFieldDescriptionList.Codes.TransitCustomsOfficeReferenceNumber);
		yield return ("89", SadImportFieldDescriptionList.Codes.GuaranteeType);
		yield return ("89.1", SadImportFieldDescriptionList.Codes.GuaranteeGrn);
		yield return ("89.2", SadImportFieldDescriptionList.Codes.GuaranteeOtherReference);
		yield return ("89.3", SadImportFieldDescriptionList.Codes.GuaranteeAccessCode);
		yield return ("89.4", SadImportFieldDescriptionList.Codes.GuaranteeOffice);
		yield return ("89.5", SadImportFieldDescriptionList.Codes.GuaranteeAmount);
		yield return ("89.6", SadImportFieldDescriptionList.Codes.GuaranteeNotValidForEc);
		yield return ("89.7", SadImportFieldDescriptionList.Codes.GuaranteeNotValidForOtherContractingParties1);
		yield return ("89.8", SadImportFieldDescriptionList.Codes.GuaranteeNotValidForOtherContractingParties2);
		yield return ("90", SadImportFieldDescriptionList.Codes.DestinationCustomsOfficeReferenceNumber);
		yield return ("91", SadImportFieldDescriptionList.Codes.AuthorizedConsigneeFiscalCode);
		yield return ("92", SadImportFieldDescriptionList.Codes.SealsNumberOfOccurrences);
		yield return ("92.1", SadImportFieldDescriptionList.Codes.SealsIdentity);
		yield return ("93", SadImportFieldDescriptionList.Codes.DateLimitOfArrivalNotification);
		yield return ("94", SadImportFieldDescriptionList.Codes.DateLimitForTheExitFromEc);

		#endregion
	}
}
