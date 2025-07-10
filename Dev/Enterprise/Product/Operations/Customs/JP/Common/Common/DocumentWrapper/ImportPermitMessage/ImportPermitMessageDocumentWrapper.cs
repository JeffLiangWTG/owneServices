using System.Linq;
using CargoWise.Customs.JP.MessageContracts;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.Customs.JP.MessageDefinitions.Inbound;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Customs.JP.Common;

public sealed class ImportPermitMessageDocumentWrapper(IJPInboundMessageParseResult parseResult, BusinessObjectFactory factory)
	: InboundMessageIncludingItemsDocumentWrapper<IImportClearanceNotice, ImportClearancePermitItemDocumentWrapper, ImportClearanceItemProvider>(parseResult, factory)
{
	#region Header Fields
	public ZString H_2_1 => messageProvider?.ReportTitles?.ElementAtOrDefault(0) ?? ZString.Empty;

	public ZString H_2_2 => messageProvider?.ReportTitles?.ElementAtOrDefault(1) ?? ZString.Empty;

	public ZString H_3 => messageProvider?.MainHSCode ?? ZString.Empty;

	public ZString H_4 => messageProvider?.ValueType ?? ZString.Empty;

	public ZString H_5 => messageProvider?.DeclarationType ?? ZString.Empty;

	public ZString H_6 => messageProvider?.DeclarationSubType ?? ZString.Empty;

	public ZString H_7 => messageProvider?.CargoType ?? ZString.Empty;

	public ZString H_8 => messageProvider?.MainPartyType ?? ZString.Empty;

	public ZString H_9 => messageProvider?.InspectionType ?? ZString.Empty;

	public ZString H_10 => messageProvider?.CustomsOffice ?? ZString.Empty;

	public ZString H_11 => messageProvider?.CustomsOfficeDepartment ?? ZString.Empty;

	public ZString H_12 => messageProvider?.DeclarationDate?.ToNACCSDate() ?? ZString.Empty;

	public ZString H_13 => messageProvider?.CustomsOfficeForSpecialDeclaration ?? ZString.Empty;

	public ZString H_14 => messageProvider?.CustomsOfficeDepartmentForSpecialDeclaration ?? ZString.Empty;

	public ZString H_15 => messageProvider?.SpecialDeclarationDate?.ToNACCSDate() ?? ZString.Empty;

	public ZString H_16 => messageProvider?.DeclarationNumber ?? ZString.Empty;

	public ZString H_17 => messageProvider?.DeclarationCondition ?? ZString.Empty;

	public ZString H_18 => messageProvider?.Reserved ?? ZString.Empty;

	public ZString H_19 => messageProvider?.ScheduledDeclarationDate?.ToNACCSDate() ?? ZString.Empty;

	public ZString H_20 => messageProvider?.IsMainDeclaration ?? ZString.Empty;

	public ZString H_21 => messageProvider?.Importer?.Code ?? ZString.Empty;

	public ZString H_22 => messageProvider?.Importer?.Name ?? ZString.Empty;

	public ZString H_23 => messageProvider?.Importer?.PostCode ?? ZString.Empty;

	public ZString H_24 => messageProvider?.Importer?.Prefecture ?? ZString.Empty;

	public ZString H_25 => messageProvider?.Importer?.City ?? ZString.Empty;

	public ZString H_26 => messageProvider?.Importer?.Street ?? ZString.Empty;

	public ZString H_27 => messageProvider?.Importer?.AdditionalInformation ?? ZString.Empty;

	public ZString H_28 => messageProvider?.Importer?.Phone ?? ZString.Empty;

	public ZString H_29 => messageProvider?.AttorneyForCustomsProceduresCode ?? ZString.Empty;

	public ZString H_30 => messageProvider?.AttorneyForCustomsProceduresAcceptanceNumber ?? ZString.Empty;

	public ZString H_31 => messageProvider?.AttorneyForCustomsProceduresName ?? ZString.Empty;

	public ZString H_32 => messageProvider?.ConsigneeCode ?? ZString.Empty;

	public ZString H_33 => messageProvider?.ConsigneeName ?? ZString.Empty;

	public ZString H_34 => messageProvider?.Shipper?.Code ?? ZString.Empty;

	public ZString H_35 => messageProvider?.Shipper?.Name ?? ZString.Empty;

	public ZString H_36 => messageProvider?.Shipper?.PostCode ?? ZString.Empty;

	public ZString H_37 => messageProvider?.Shipper?.Street1 ?? ZString.Empty;

	public ZString H_38 => messageProvider?.Shipper?.Street2 ?? ZString.Empty;

	public ZString H_39 => messageProvider?.Shipper?.City ?? ZString.Empty;

	public ZString H_40 => messageProvider?.Shipper?.State ?? ZString.Empty;

	public ZString H_41 => messageProvider?.Shipper?.CountryCode ?? ZString.Empty;

	public ZString H_42 => messageProvider?.ConsignorName ?? ZString.Empty;

	public ZString H_43 => messageProvider?.AgentCode ?? ZString.Empty;

	public ZString H_44 => messageProvider?.AgentName ?? ZString.Empty;

	public ZString H_45 => messageProvider?.CustomsBrokerCode ?? ZString.Empty;

	public ZString H_46 => messageProvider?.InspectionWitness ?? ZString.Empty;

	public ZString H_47_1 => messageProvider?.AWBNumbers?.ElementAtOrDefault(0) ?? ZString.Empty;

	public ZString H_47_2 => messageProvider?.AWBNumbers?.ElementAtOrDefault(1) ?? ZString.Empty;

	public ZString H_47_3 => messageProvider?.AWBNumbers?.ElementAtOrDefault(2) ?? ZString.Empty;

	public ZString H_47_4 => messageProvider?.AWBNumbers?.ElementAtOrDefault(3) ?? ZString.Empty;

	public ZString H_47_5 => messageProvider?.AWBNumbers?.ElementAtOrDefault(4) ?? ZString.Empty;

	public ZString H_48 => messageProvider?.BLNumber ?? ZString.Empty;

	public ZString H_49 => messageProvider?.PortOfUnloadingCode ?? ZString.Empty;

	public ZString H_50 => messageProvider?.PortOfUnloadingName ?? ZString.Empty;

	public ZString H_51 => messageProvider?.PortOfLoadingCode ?? ZString.Empty;

	public ZString H_52 => messageProvider?.PortOfLoadingName ?? ZString.Empty;

	public ZString H_53 => messageProvider?.VesselCode ?? ZString.Empty;

	public ZString H_54 => messageProvider?.VesselName ?? ZString.Empty;

	public ZString H_55 => messageProvider?.ArrivalDate?.ToNACCSDate() ?? ZString.Empty;

	public ZString H_56 => messageProvider?.StorageCustoms ?? ZString.Empty;

	public ZString H_57 => messageProvider?.StorageCustomsDepartment ?? ZString.Empty;

	public ZString H_58 => messageProvider?.Quantity?.Quantity.FormatNumberInDocument() ?? ZString.Empty;

	public ZString H_59 => messageProvider?.Quantity?.Unit ?? ZString.Empty;

	public ZString H_60 => messageProvider?.CustomsDepotCode ?? ZString.Empty;

	public ZString H_61 => messageProvider?.CustomsDepotName ?? ZString.Empty;

	public ZString H_62 => messageProvider?.GrossWeight?.Quantity.FormatNumberInDocument() ?? ZString.Empty;

	public ZString H_63 => messageProvider?.GrossWeight?.Unit ?? ZString.Empty;

	public ZString H_64 => messageProvider?.BondedLocationCode ?? ZString.Empty;

	public ZString H_65 => messageProvider?.BondedLocationName ?? ZString.Empty;

	public ZInt H_66 => messageProvider?.ContainerCount ?? ZInt.Zero;

	public ZString H_67 => messageProvider?.IntoBondedAreaFirstApprovalDate?.ToNACCSDate() ?? ZString.Empty;

	public ZString H_68 => messageProvider?.ComprehensiveDeclarationType ?? ZString.Empty;

	public ZString H_69 => messageProvider?.TradeType ?? ZString.Empty;

	public ZString H_70 => messageProvider?.CustomsInspectionCode ?? ZString.Empty;

	public ZString H_71 => messageProvider?.MarksAndNumbers ?? ZString.Empty;

	public ZString H_72 => messageProvider?.ImportTradeControlOrderArticle3 ?? ZString.Empty;

	public ZString H_73 => messageProvider?.ImportApprovalCertificateAdditionalType ?? ZString.Empty;

	public ZString H_74_1 => messageProvider?.CodeForVerificationBasedOnOtherLawsAndRegulations?.ElementAtOrDefault(0) ?? ZString.Empty;

	public ZString H_74_2 => messageProvider?.CodeForVerificationBasedOnOtherLawsAndRegulations?.ElementAtOrDefault(1) ?? ZString.Empty;

	public ZString H_74_3 => messageProvider?.CodeForVerificationBasedOnOtherLawsAndRegulations?.ElementAtOrDefault(2) ?? ZString.Empty;

	public ZString H_74_4 => messageProvider?.CodeForVerificationBasedOnOtherLawsAndRegulations?.ElementAtOrDefault(3) ?? ZString.Empty;

	public ZString H_74_5 => messageProvider?.CodeForVerificationBasedOnOtherLawsAndRegulations?.ElementAtOrDefault(4) ?? ZString.Empty;

	public ZString H_75 => messageProvider?.CommonControlNumber ?? ZString.Empty;

	public ZString H_76 => messageProvider?.FoodHygineCertificateType ?? ZString.Empty;

	public ZString H_77 => messageProvider?.FoodImportNotificationReceiptNumber ?? ZString.Empty;

	public ZString H_78 => messageProvider?.PlantProtectionCertificateType ?? ZString.Empty;

	public ZString H_79 => messageProvider?.PlantImportInspectionApplicationNumber ?? ZString.Empty;

	public ZString H_80 => messageProvider?.AnimalQuarantineCertificateType ?? ZString.Empty;

	public ZString H_81 => messageProvider?.AnimalProductImportInspectionApplicationNumber ?? ZString.Empty;

	public ZString H_82_1 => messageProvider?.ImportApprovalCertificates?.ElementAtOrDefault(0)?.Type ?? ZString.Empty;

	public ZString H_83_1 => messageProvider?.ImportApprovalCertificates?.ElementAtOrDefault(0)?.Number ?? ZString.Empty;

	public ZString H_82_2 => messageProvider?.ImportApprovalCertificates?.ElementAtOrDefault(1)?.Type ?? ZString.Empty;

	public ZString H_83_2 => messageProvider?.ImportApprovalCertificates?.ElementAtOrDefault(1)?.Number ?? ZString.Empty;

	public ZString H_82_3 => messageProvider?.ImportApprovalCertificates?.ElementAtOrDefault(2)?.Type ?? ZString.Empty;

	public ZString H_83_3 => messageProvider?.ImportApprovalCertificates?.ElementAtOrDefault(2)?.Number ?? ZString.Empty;

	public ZString H_82_4 => messageProvider?.ImportApprovalCertificates?.ElementAtOrDefault(3)?.Type ?? ZString.Empty;

	public ZString H_83_4 => messageProvider?.ImportApprovalCertificates?.ElementAtOrDefault(3)?.Number ?? ZString.Empty;

	public ZString H_82_5 => messageProvider?.ImportApprovalCertificates?.ElementAtOrDefault(4)?.Type ?? ZString.Empty;

	public ZString H_83_5 => messageProvider?.ImportApprovalCertificates?.ElementAtOrDefault(4)?.Number ?? ZString.Empty;

	public ZString H_82_6 => messageProvider?.ImportApprovalCertificates?.ElementAtOrDefault(5)?.Type ?? ZString.Empty;

	public ZString H_83_6 => messageProvider?.ImportApprovalCertificates?.ElementAtOrDefault(5)?.Number ?? ZString.Empty;

	public ZString H_82_7 => messageProvider?.ImportApprovalCertificates?.ElementAtOrDefault(6)?.Type ?? ZString.Empty;

	public ZString H_83_7 => messageProvider?.ImportApprovalCertificates?.ElementAtOrDefault(6)?.Number ?? ZString.Empty;

	public ZString H_82_8 => messageProvider?.ImportApprovalCertificates?.ElementAtOrDefault(7)?.Type ?? ZString.Empty;

	public ZString H_83_8 => messageProvider?.ImportApprovalCertificates?.ElementAtOrDefault(7)?.Number ?? ZString.Empty;

	public ZString H_82_9 => messageProvider?.ImportApprovalCertificates?.ElementAtOrDefault(8)?.Type ?? ZString.Empty;

	public ZString H_83_9 => messageProvider?.ImportApprovalCertificates?.ElementAtOrDefault(8)?.Number ?? ZString.Empty;

	public ZString H_82_10 => messageProvider?.ImportApprovalCertificates?.ElementAtOrDefault(9)?.Type ?? ZString.Empty;

	public ZString H_83_10 => messageProvider?.ImportApprovalCertificates?.ElementAtOrDefault(9)?.Number ?? ZString.Empty;

	public ZString H_84 => messageProvider?.Invoice?.Type ?? ZString.Empty;

	public ZString H_85 => messageProvider?.Invoice?.Number ?? ZString.Empty;

	public ZString H_86 => messageProvider?.Invoice?.ElectronicReceiptNumber ?? ZString.Empty;

	public ZString H_87 => messageProvider?.Invoice?.PriceTypeCode ?? ZString.Empty;

	public ZString H_88 => messageProvider?.Invoice?.Incoterm ?? ZString.Empty;

	public ZString H_89 => messageProvider?.Invoice?.Price?.CurrencyCode ?? ZString.Empty;

	public ZString H_90 => messageProvider?.Invoice?.Price?.Amount.FormatNumberInDocument() ?? ZString.Empty;

	public ZString H_91 => messageProvider?.Freight?.Type ?? ZString.Empty;

	public ZString H_92 => messageProvider?.Freight?.CurrencyCode ?? ZString.Empty;

	public ZString H_93 => messageProvider?.Freight?.Amount.FormatNumberInDocument() ?? ZString.Empty;

	public ZString H_94 => messageProvider?.Insurance?.Type ?? ZString.Empty;

	public ZString H_95 => messageProvider?.Insurance?.CurrencyCode ?? ZString.Empty;

	public ZString H_96 => messageProvider?.Insurance?.Amount.FormatNumberInDocument() ?? ZString.Empty;

	public ZString H_97 => messageProvider?.ComprehensiveInsuranceNumber ?? ZString.Empty;

	public ZString H_98 => messageProvider?.Clearance?.CurrencyCode ?? ZString.Empty;

	public ZString H_99 => messageProvider?.Clearance?.Amount.FormatNumberInDocument() ?? ZString.Empty;

	public ZString H_100 => messageProvider?.ValuationType ?? ZString.Empty;

	public ZString H_101_1 => messageProvider?.ComprehensiveValuationDeclarationAcceptanceNumber?.ElementAtOrDefault(0) ?? ZString.Empty;

	public ZString H_101_2 => messageProvider?.ComprehensiveValuationDeclarationAcceptanceNumber?.ElementAtOrDefault(1) ?? ZString.Empty;

	public ZString H_101_3 => messageProvider?.ComprehensiveValuationDeclarationAcceptanceNumber?.ElementAtOrDefault(2) ?? ZString.Empty;

	public ZString H_102 => messageProvider?.ValuationCorrectionType ?? ZString.Empty;

	public ZString H_103 => messageProvider?.ValuationCorrectionBase?.CurrencyCode ?? ZString.Empty;

	public ZString H_104 => messageProvider?.ValuationCorrectionBase?.Amount.FormatNumberInDocument() ?? ZString.Empty;

	public ZString H_105 => messageProvider?.ValuationCorrectionFormula ?? ZString.Empty;

	public ZString H_106_1 => messageProvider?.ComprehensiveValuations?.ElementAtOrDefault(0)?.ComprehensiveValuationStandardFormulaType ?? ZString.Empty;

	public ZString H_107_1 => messageProvider?.ComprehensiveValuations?.ElementAtOrDefault(0)?.ComprehensiveValuationCorrectionType ?? ZString.Empty;

	public ZString H_108_1 => messageProvider?.ComprehensiveValuations?.ElementAtOrDefault(0)?.ComprehensiveValuationCorrectionFormula ?? ZString.Empty;

	public ZString H_106_2 => messageProvider?.ComprehensiveValuations?.ElementAtOrDefault(1)?.ComprehensiveValuationStandardFormulaType ?? ZString.Empty;

	public ZString H_107_2 => messageProvider?.ComprehensiveValuations?.ElementAtOrDefault(1)?.ComprehensiveValuationCorrectionType ?? ZString.Empty;

	public ZString H_108_2 => messageProvider?.ComprehensiveValuations?.ElementAtOrDefault(1)?.ComprehensiveValuationCorrectionFormula ?? ZString.Empty;

	public ZString H_106_3 => messageProvider?.ComprehensiveValuations?.ElementAtOrDefault(2)?.ComprehensiveValuationStandardFormulaType ?? ZString.Empty;

	public ZString H_107_3 => messageProvider?.ComprehensiveValuations?.ElementAtOrDefault(2)?.ComprehensiveValuationCorrectionType ?? ZString.Empty;

	public ZString H_108_3 => messageProvider?.ComprehensiveValuations?.ElementAtOrDefault(2)?.ComprehensiveValuationCorrectionFormula ?? ZString.Empty;

	public ZString H_109_1 => messageProvider?.AdvancedRulingOnValuation?.ElementAtOrDefault(0) ?? ZString.Empty;

	public ZString H_109_2 => messageProvider?.AdvancedRulingOnValuation?.ElementAtOrDefault(1) ?? ZString.Empty;

	public ZString H_110 => messageProvider?.TotalCustomsValueApportionmentCoefficient.FormatNumberInDocument() ?? ZString.Empty;

	public ZString H_111 => messageProvider?.TotalCustomsValueApportionmentCoefficientType ?? ZString.Empty;

	public ZString H_112 => messageProvider?.CalculationType ?? ZString.Empty;

	public ZString H_113 => messageProvider?.CertificateOfOrigin ?? ZString.Empty;

	public ZString H_114 => messageProvider?.DutyDrawback ?? ZString.Empty;

	public ZString H_115 => messageProvider?.ContentInspectionResult ?? ZString.Empty;

	public ZString H_116_1 => messageProvider?.Taxes?.ElementAtOrDefault(0)?.TaxType ?? ZString.Empty;

	public ZString H_117_1 => messageProvider?.Taxes?.ElementAtOrDefault(0)?.TaxName ?? ZString.Empty;

	public ZString H_118_1 => messageProvider?.Taxes?.ElementAtOrDefault(0)?.TaxAmount.FormatNumberInDocument() ?? ZString.Empty;

	public ZInt H_119_1 => messageProvider?.Taxes?.ElementAtOrDefault(0)?.TaxColumnCount ?? ZInt.Zero;

	public ZString H_116_2 => messageProvider?.Taxes?.ElementAtOrDefault(1)?.TaxType ?? ZString.Empty;

	public ZString H_117_2 => messageProvider?.Taxes?.ElementAtOrDefault(1)?.TaxName ?? ZString.Empty;

	public ZString H_118_2 => messageProvider?.Taxes?.ElementAtOrDefault(1)?.TaxAmount.FormatNumberInDocument() ?? ZString.Empty;

	public ZInt H_119_2 => messageProvider?.Taxes?.ElementAtOrDefault(1)?.TaxColumnCount ?? ZInt.Zero;

	public ZString H_116_3 => messageProvider?.Taxes?.ElementAtOrDefault(2)?.TaxType ?? ZString.Empty;

	public ZString H_117_3 => messageProvider?.Taxes?.ElementAtOrDefault(2)?.TaxName ?? ZString.Empty;

	public ZString H_118_3 => messageProvider?.Taxes?.ElementAtOrDefault(2)?.TaxAmount.FormatNumberInDocument() ?? ZString.Empty;

	public ZInt H_119_3 => messageProvider?.Taxes?.ElementAtOrDefault(2)?.TaxColumnCount ?? ZInt.Zero;

	public ZString H_116_4 => messageProvider?.Taxes?.ElementAtOrDefault(3)?.TaxType ?? ZString.Empty;

	public ZString H_117_4 => messageProvider?.Taxes?.ElementAtOrDefault(3)?.TaxName ?? ZString.Empty;

	public ZString H_118_4 => messageProvider?.Taxes?.ElementAtOrDefault(3)?.TaxAmount.FormatNumberInDocument() ?? ZString.Empty;

	public ZInt H_119_4 => messageProvider?.Taxes?.ElementAtOrDefault(3)?.TaxColumnCount ?? ZInt.Zero;

	public ZString H_116_5 => messageProvider?.Taxes?.ElementAtOrDefault(4)?.TaxType ?? ZString.Empty;

	public ZString H_117_5 => messageProvider?.Taxes?.ElementAtOrDefault(4)?.TaxName ?? ZString.Empty;

	public ZString H_118_5 => messageProvider?.Taxes?.ElementAtOrDefault(4)?.TaxAmount.FormatNumberInDocument() ?? ZString.Empty;

	public ZInt H_119_5 => messageProvider?.Taxes?.ElementAtOrDefault(4)?.TaxColumnCount ?? ZInt.Zero;

	public ZString H_116_6 => messageProvider?.Taxes?.ElementAtOrDefault(5)?.TaxType ?? ZString.Empty;

	public ZString H_117_6 => messageProvider?.Taxes?.ElementAtOrDefault(5)?.TaxName ?? ZString.Empty;

	public ZString H_118_6 => messageProvider?.Taxes?.ElementAtOrDefault(5)?.TaxAmount.FormatNumberInDocument() ?? ZString.Empty;

	public ZInt H_119_6 => messageProvider?.Taxes?.ElementAtOrDefault(5)?.TaxColumnCount ?? ZInt.Zero;

	public ZString H_116_7 => messageProvider?.Taxes?.ElementAtOrDefault(6)?.TaxType ?? ZString.Empty;

	public ZString H_117_7 => messageProvider?.Taxes?.ElementAtOrDefault(6)?.TaxName ?? ZString.Empty;

	public ZString H_118_7 => messageProvider?.Taxes?.ElementAtOrDefault(6)?.TaxAmount.FormatNumberInDocument() ?? ZString.Empty;

	public ZInt H_119_7 => messageProvider?.Taxes?.ElementAtOrDefault(6)?.TaxColumnCount ?? ZInt.Zero;

	public ZString H_120 => messageProvider?.TotalTaxAmount.FormatNumberInDocument() ?? ZString.Empty;

	public ZString H_121 => messageProvider?.GuaranteedAmount.FormatNumberInDocument() ?? ZString.Empty;

	public ZString H_122_1 => messageProvider?.ExchangeRates?.ElementAtOrDefault(0)?.ExchangeRateCurrency ?? ZString.Empty;

	public ZString H_123_1 => messageProvider?.ExchangeRates?.ElementAtOrDefault(0)?.ExchangeRate.FormatNumberInDocument() ?? ZString.Empty;

	public ZString H_122_2 => messageProvider?.ExchangeRates?.ElementAtOrDefault(1)?.ExchangeRateCurrency ?? ZString.Empty;

	public ZString H_123_2 => messageProvider?.ExchangeRates?.ElementAtOrDefault(1)?.ExchangeRate.FormatNumberInDocument() ?? ZString.Empty;

	public ZString H_122_3 => messageProvider?.ExchangeRates?.ElementAtOrDefault(2)?.ExchangeRateCurrency ?? ZString.Empty;

	public ZString H_123_3 => messageProvider?.ExchangeRates?.ElementAtOrDefault(2)?.ExchangeRate.FormatNumberInDocument() ?? ZString.Empty;

	public ZString H_124 => messageProvider?.PaymentDeadlineExtensionCode ?? ZString.Empty;

	public ZString H_125 => messageProvider?.PrefectureCode ?? ZString.Empty;

	public ZString H_126 => messageProvider?.BeforePermitApplicationCode ?? ZString.Empty;

	public ZString H_127 => messageProvider?.PetroleumAndCoalTaxExceptionalPaymentApprovalNumber ?? ZString.Empty;

	public ZString H_128 => messageProvider?.BankAccountType ?? ZString.Empty;

	public ZString H_129 => messageProvider?.HasConsumptionTax ?? ZString.Empty;

	public ZString H_130 => messageProvider?.TobaccoDealerRegistrationNumber ?? ZString.Empty;

	public ZString H_131 => messageProvider?.PaymentMethod ?? ZString.Empty;

	public ZInt H_132 => messageProvider?.MessageCount ?? ZInt.Zero;

	public ZInt H_133 => messageProvider?.ColumnCount ?? ZInt.Zero;

	public ZString H_134 => messageProvider?.CustomsNotes ?? ZString.Empty;

	public ZString H_135 => messageProvider?.ImporterInput ?? ZString.Empty;

	public ZString H_136 => messageProvider?.CustomsBrokerNotes ?? ZString.Empty;

	public ZString H_137 => messageProvider?.ConsigneeInput ?? ZString.Empty;

	public ZString H_138 => messageProvider?.OwnerNotes ?? ZString.Empty;

	public ZString H_139 => messageProvider?.InternalReferenceNumber ?? ZString.Empty;

	public ZString H_140 => messageProvider?.OwnerSectionCode ?? ZString.Empty;

	public ZString H_141 => messageProvider?.OwnerReferenceNumber ?? ZString.Empty;

	public ZString H_142 => messageProvider?.UserReferenceNumber ?? ZString.Empty;

	public ZString H_143 => messageProvider?.CustomsNotificationColumn ?? ZString.Empty;

	public ZString H_144 => messageProvider?.CustomsOfficeDirectorName ?? ZString.Empty;

	public ZString H_145 => messageProvider?.ApprovalDate?.ToNACCSDate() ?? ZString.Empty;

	public ZString H_146 => messageProvider?.InspectionCompleteDate?.ToNACCSDate() ?? ZString.Empty;

	public ZString H_147 => messageProvider?.PostClearanceInspection ?? ZString.Empty;

	public ZString H_148 => messageProvider?.BPApprovalDate?.ToNACCSDate() ?? ZString.Empty;

	public ZString H_149 => messageProvider?.BPInspectionCompleteDate?.ToNACCSDate() ?? ZString.Empty;

	public ZInt H_150 => messageProvider?.DaysUntilImportationApproval ?? ZInt.Zero;

	public ZString H_151 => messageProvider?.SpecialDeclarationDeadline?.ToNACCSDate() ?? ZString.Empty;

	public ZString H_152 => messageProvider?.BondedTransportationApprovalStartDate?.ToNACCSDate() ?? ZString.Empty;

	public ZString H_153 => messageProvider?.BondedTransportationApprovalEndDate?.ToNACCSDate() ?? ZString.Empty;

	public ZString H_154 => messageProvider?.DeadlineExtensionCitedLawTitle ?? ZString.Empty;

	public ZString H_155 => messageProvider?.OverdueTaxTotal.FormatNumberInDocument() ?? ZString.Empty;

	public ZString H_156_1 => messageProvider?.PaymentDeadlineExtensionLegalBasis?.ElementAtOrDefault(0) ?? ZString.Empty;

	public ZString H_156_2 => messageProvider?.PaymentDeadlineExtensionLegalBasis?.ElementAtOrDefault(1) ?? ZString.Empty;

	public ZString H_156_3 => messageProvider?.PaymentDeadlineExtensionLegalBasis?.ElementAtOrDefault(2) ?? ZString.Empty;

	public ZString H_156_4 => messageProvider?.PaymentDeadlineExtensionLegalBasis?.ElementAtOrDefault(3) ?? ZString.Empty;

	public ZString H_156_5 => messageProvider?.PaymentDeadlineExtensionLegalBasis?.ElementAtOrDefault(4) ?? ZString.Empty;

	public ZString H_156_6 => messageProvider?.PaymentDeadlineExtensionLegalBasis?.ElementAtOrDefault(5) ?? ZString.Empty;

	public ZString H_156_7 => messageProvider?.PaymentDeadlineExtensionLegalBasis?.ElementAtOrDefault(6) ?? ZString.Empty;

	public ZString H_157 => messageProvider?.PaymentDeadlineTitle ?? ZString.Empty;

	public ZString H_158_1 => messageProvider?.PaymentDeadlines?.ElementAtOrDefault(0)?.PaymentDeadlineExtensionTaxType ?? ZString.Empty;

	public ZString H_159_1 => messageProvider?.PaymentDeadlines?.ElementAtOrDefault(0)?.PaymentDeadlineExtensionTaxName ?? ZString.Empty;

	public ZString H_160_1 => messageProvider?.PaymentDeadlines?.ElementAtOrDefault(0)?.PaymentDeadline?.ToNACCSDate() ?? ZString.Empty;

	public ZString H_158_2 => messageProvider?.PaymentDeadlines?.ElementAtOrDefault(1)?.PaymentDeadlineExtensionTaxType ?? ZString.Empty;

	public ZString H_159_2 => messageProvider?.PaymentDeadlines?.ElementAtOrDefault(1)?.PaymentDeadlineExtensionTaxName ?? ZString.Empty;

	public ZString H_160_2 => messageProvider?.PaymentDeadlines?.ElementAtOrDefault(1)?.PaymentDeadline?.ToNACCSDate() ?? ZString.Empty;

	public ZString H_158_3 => messageProvider?.PaymentDeadlines?.ElementAtOrDefault(2)?.PaymentDeadlineExtensionTaxType ?? ZString.Empty;

	public ZString H_159_3 => messageProvider?.PaymentDeadlines?.ElementAtOrDefault(2)?.PaymentDeadlineExtensionTaxName ?? ZString.Empty;

	public ZString H_160_3 => messageProvider?.PaymentDeadlines?.ElementAtOrDefault(2)?.PaymentDeadline?.ToNACCSDate() ?? ZString.Empty;

	public ZString H_158_4 => messageProvider?.PaymentDeadlines?.ElementAtOrDefault(3)?.PaymentDeadlineExtensionTaxType ?? ZString.Empty;

	public ZString H_159_4 => messageProvider?.PaymentDeadlines?.ElementAtOrDefault(3)?.PaymentDeadlineExtensionTaxName ?? ZString.Empty;

	public ZString H_160_4 => messageProvider?.PaymentDeadlines?.ElementAtOrDefault(3)?.PaymentDeadline?.ToNACCSDate() ?? ZString.Empty;

	public ZString H_158_5 => messageProvider?.PaymentDeadlines?.ElementAtOrDefault(4)?.PaymentDeadlineExtensionTaxType ?? ZString.Empty;

	public ZString H_159_5 => messageProvider?.PaymentDeadlines?.ElementAtOrDefault(4)?.PaymentDeadlineExtensionTaxName ?? ZString.Empty;

	public ZString H_160_5 => messageProvider?.PaymentDeadlines?.ElementAtOrDefault(4)?.PaymentDeadline?.ToNACCSDate() ?? ZString.Empty;

	public ZString H_158_6 => messageProvider?.PaymentDeadlines?.ElementAtOrDefault(5)?.PaymentDeadlineExtensionTaxType ?? ZString.Empty;

	public ZString H_159_6 => messageProvider?.PaymentDeadlines?.ElementAtOrDefault(5)?.PaymentDeadlineExtensionTaxName ?? ZString.Empty;

	public ZString H_160_6 => messageProvider?.PaymentDeadlines?.ElementAtOrDefault(5)?.PaymentDeadline?.ToNACCSDate() ?? ZString.Empty;

	public ZString H_158_7 => messageProvider?.PaymentDeadlines?.ElementAtOrDefault(6)?.PaymentDeadlineExtensionTaxType ?? ZString.Empty;

	public ZString H_159_7 => messageProvider?.PaymentDeadlines?.ElementAtOrDefault(6)?.PaymentDeadlineExtensionTaxName ?? ZString.Empty;

	public ZString H_160_7 => messageProvider?.PaymentDeadlines?.ElementAtOrDefault(6)?.PaymentDeadline?.ToNACCSDate() ?? ZString.Empty;

	public ZString H_161 => messageProvider?.ReturnToCustomsOfficePostcode ?? ZString.Empty;

	public ZString H_162 => messageProvider?.ReturnToCustomsOfficeAddress ?? ZString.Empty;

	public ZString H_163 => messageProvider?.ReturnToCustomsOfficeName ?? ZString.Empty;

	public ZString H_164 => DocumentWrapperHelper.FormatBarCode(messageProvider?.IntoBondedAreaApprovalApplicationNumberBarcode ?? ZString.Empty);

	public ZString H_165 => messageProvider?.OtherCustomsNotificationColumn ?? ZString.Empty;

	public ZString H_166_1 => messageProvider?.OtherCitedLawCustomsNotificationColumns?.ElementAtOrDefault(0) ?? ZString.Empty;

	public ZString H_166_2 => messageProvider?.OtherCitedLawCustomsNotificationColumns?.ElementAtOrDefault(1) ?? ZString.Empty;

	public ZString H_167 => messageProvider?.CustomsNotificationColumnDeferralPeriodTitle ?? ZString.Empty;

	public ZInt H_168 => messageProvider?.CustomsNotificationColumnDeferralPeriod ?? ZInt.Zero;

	public ZString H_169 => messageProvider?.CustomsNotificationColumnDeferralPeriodUnit ?? ZString.Empty;
	#endregion

	protected override DocumentWrapperCollection<ImportClearancePermitItemDocumentWrapper, ImportClearanceItemProvider> GetItemsCore() => new(messageProvider.ImportClearanceItems, Factory);

	public DocumentWrapperCollection<ImportClearancePermitItemDocumentWrapper, ImportClearanceItemProvider> FirstPageItems => firstPageItems ??= new(messageProvider.ImportClearanceItems.Take(FirstPageItemMaxNumber), Factory);
	DocumentWrapperCollection<ImportClearancePermitItemDocumentWrapper, ImportClearanceItemProvider> firstPageItems;

	public DocumentWrapperCollection<ImportClearancePermitItemDocumentWrapper, ImportClearanceItemProvider> RemainingItems => remainingItems ??= new(messageProvider.ImportClearanceItems.Skip(FirstPageItemMaxNumber), Factory);
	DocumentWrapperCollection<ImportClearancePermitItemDocumentWrapper, ImportClearanceItemProvider> remainingItems;

	ZInt FirstPageItemMaxNumber
	{
		get
		{
			var count = 0;
			var groupType = GroupType;
			var templateCode = TemplateCode;
			if (groupType == Constants.DocumentMessageCodes.ImportPermitMessageGroupTypes.GISTH)
			{
				count = 5;
			}
			else if (templateCode == ImportPermitTemplateCodeList.Codes.NA || templateCode == ImportPermitTemplateCodeList.Codes.ISTA || groupType == Constants.DocumentMessageCodes.ImportPermitMessageGroupTypes.GBPS)
			{
				count = 1;
			}
			return count;
		}
	}

	protected override ZString GetTemplateTypeCore() => ImportPermitTemplateCodes.GetDescriptionFromCode(TemplateCode);

	public ICodeDescriptionPairList ImportPermitTemplateCodes => Factory.GetCachedValue<ImportPermitTemplateCodeList>();

	protected override ZString GetShipmentTypeCore() => Constants.DocumentMessageCodes.ShipmentTypes.Import;

	public ZString TemplateCode => Factory.GetCachedValue($"ImportPermitMessageDocumentWrapper_TemplateCode_{OutputInformationCode}", () =>
	{
		var result = ZString.Empty;
		if (IsImportPermitMessageNACode)
		{
			result = ImportPermitTemplateCodeList.Codes.NA;
		}
		else if (IsImportPermitMessageNBCode)
		{
			result = ImportPermitTemplateCodeList.Codes.NB;
		}
		else if (IsImportPermitMessageNCCode)
		{
			result = ImportPermitTemplateCodeList.Codes.NC;
		}
		else if (IsImportPermitMessageBPBCode)
		{
			result = ImportPermitTemplateCodeList.Codes.BPB;
		}
		else if (IsImportPermitMessageBPCCode)
		{
			result = ImportPermitTemplateCodeList.Codes.BPC;
		}
		else if (IsImportPermitMessageISTACode)
		{
			result = ImportPermitTemplateCodeList.Codes.ISTA;
		}
		else if (IsImportPermitMessageISTBCode)
		{
			result = ImportPermitTemplateCodeList.Codes.ISTB;
		}
		else if (IsImportPermitMessageISTCCode)
		{
			result = ImportPermitTemplateCodeList.Codes.ISTC;
		}
		else if (IsImportPermitMessageNHCode)
		{
			result = ImportPermitTemplateCodeList.Codes.NH;
		}
		else if (IsImportPermitMessageISTHCode)
		{
			result = ImportPermitTemplateCodeList.Codes.ISTH;
		}
		else if (IsImportPermitMessageNSCode)
		{
			result = ImportPermitTemplateCodeList.Codes.NS;
		}
		else if (IsImportPermitMessageBPSCode)
		{
			result = ImportPermitTemplateCodeList.Codes.BPS;
		}

		return result;
	});

	public ZString GroupType => Factory.GetCachedValue($"ImportPermitMessageDocumentWrapper_GroupType_{OutputInformationCode}", () =>
	{
		var result = ZString.Empty;
		if (IsImportPermitMessageNACode || IsImportPermitMessageISTACode)
		{
			result = Constants.DocumentMessageCodes.ImportPermitMessageGroupTypes.GA;
		}
		else if (IsImportPermitMessageNBCode || IsImportPermitMessageBPBCode || IsImportPermitMessageISTBCode)
		{
			result = Constants.DocumentMessageCodes.ImportPermitMessageGroupTypes.GB;
		}
		else if (IsImportPermitMessageNCCode || IsImportPermitMessageBPCCode || IsImportPermitMessageISTCCode)
		{
			result = Constants.DocumentMessageCodes.ImportPermitMessageGroupTypes.GC;
		}
		else if (IsImportPermitMessageNHCode || IsImportPermitMessageISTHCode)
		{
			result = Constants.DocumentMessageCodes.ImportPermitMessageGroupTypes.GISTH;
		}
		else if (IsImportPermitMessageNSCode || IsImportPermitMessageBPSCode)
		{
			result = Constants.DocumentMessageCodes.ImportPermitMessageGroupTypes.GBPS;
		}

		return result;
	});

	public ZString SubGroupType => Factory.GetCachedValue($"ImportPermitMessageDocumentWrapper_SubGroupType_{OutputInformationCode}", () =>
	{
		var result = ZString.Empty;
		if (IsImportPermitMessageNACode || IsImportPermitMessageNBCode || IsImportPermitMessageNCCode)
		{
			result = Constants.DocumentMessageCodes.ImportPermitMessageSubGroupTypes.GNABC;
		}
		else if (IsImportPermitMessageBPBCode || IsImportPermitMessageBPCCode)
		{
			result = Constants.DocumentMessageCodes.ImportPermitMessageSubGroupTypes.GBPBC;
		}
		else if (IsImportPermitMessageISTACode || IsImportPermitMessageISTBCode || IsImportPermitMessageISTCCode)
		{
			result = Constants.DocumentMessageCodes.ImportPermitMessageSubGroupTypes.GISTABC;
		}

		return result;
	});

	bool IsImportPermitMessageNACode => Constants.DocumentMessageCodes.ImportPermitMessage_NA_Codes.Contains(OutputInformationCode);

	bool IsImportPermitMessageNBCode => Constants.DocumentMessageCodes.ImportPermitMessage_NB_Codes.Contains(OutputInformationCode);

	bool IsImportPermitMessageNCCode => Constants.DocumentMessageCodes.ImportPermitMessage_NC_Codes.Contains(OutputInformationCode);

	bool IsImportPermitMessageBPBCode => Constants.DocumentMessageCodes.ImportPermitMessage_BPB_Codes.Contains(OutputInformationCode);

	bool IsImportPermitMessageBPCCode => Constants.DocumentMessageCodes.ImportPermitMessage_BPC_Codes.Contains(OutputInformationCode);

	bool IsImportPermitMessageISTACode => Constants.DocumentMessageCodes.ImportPermitMessage_ISTA_Codes.Contains(OutputInformationCode);

	bool IsImportPermitMessageISTBCode => Constants.DocumentMessageCodes.ImportPermitMessage_ISTB_Codes.Contains(OutputInformationCode);

	bool IsImportPermitMessageISTCCode => Constants.DocumentMessageCodes.ImportPermitMessage_ISTC_Codes.Contains(OutputInformationCode);

	bool IsImportPermitMessageNHCode => Constants.DocumentMessageCodes.ImportPermitMessage_NH_Codes.Contains(OutputInformationCode);

	bool IsImportPermitMessageISTHCode => Constants.DocumentMessageCodes.ImportPermitMessage_ISTH_Codes.Contains(OutputInformationCode);

	bool IsImportPermitMessageNSCode => Constants.DocumentMessageCodes.ImportPermitMessage_NS_Codes.Contains(OutputInformationCode);

	bool IsImportPermitMessageBPSCode => Constants.DocumentMessageCodes.ImportPermitMessage_BPS_Codes.Contains(OutputInformationCode);
}
