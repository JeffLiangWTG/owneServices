using System.Linq;
using CargoWise.Customs.JP.MessageContracts;
using CargoWise.Customs.JP.MessageDefinitions.Inbound;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using static Enterprise.Customs.JP.Common.JPOutputInformationCodeList;

namespace Enterprise.Customs.JP.Common
{
	public sealed class EACNoticeMessageDocumentWrapper(IJPInboundMessageParseResult parseResult, BusinessObjectFactory factory) : InboundMessageDocumentWrapper<IEACNoticeResponse>(parseResult, factory)
	{
		public ZString H_2 => messageProvider?.Reserved ?? ZString.Empty;

		public ZString H_3 => messageProvider?.ValueType ?? ZString.Empty;

		public ZString H_4 => messageProvider?.DeclarationType ?? ZString.Empty;

		public ZString H_5 => messageProvider?.DeclarationSubType ?? ZString.Empty;

		public ZString H_6 => messageProvider?.CargoType ?? ZString.Empty;

		public ZString H_7 => messageProvider?.MainPartyType ?? ZString.Empty;

		public ZString H_8 => messageProvider?.InspectionType ?? ZString.Empty;

		public ZString H_9 => messageProvider?.CustomsOffice ?? ZString.Empty;

		public ZString H_10 => messageProvider?.CustomsOfficeDepartment ?? ZString.Empty;

		public ZString H_11 => messageProvider?.ApprovalAmendmentDate?.ToNACCSDate() ?? ZString.Empty;

		public ZString H_12 => messageProvider?.ApprovalAmendmentDeclarationNumber ?? ZString.Empty;

		public ZString H_13 => messageProvider?.ApplicantCode ?? ZString.Empty;

		public ZString H_14 => messageProvider?.ApplicantName ?? ZString.Empty;

		public ZString H_15 => messageProvider?.ApplicantCustomsBrokerCode ?? ZString.Empty;

		public ZString H_16 => messageProvider?.OriginalApplicantCode ?? ZString.Empty;

		public ZString H_17 => messageProvider?.OriginalApplicantName ?? ZString.Empty;

		public ZString H_18 => messageProvider?.InspectionWitness ?? ZString.Empty;

		public ZString H_19 => messageProvider?.Exporter?.Code ?? ZString.Empty;

		public ZString H_20 => messageProvider?.Exporter?.Name ?? ZString.Empty;

		public ZString H_21 => messageProvider?.Exporter?.PostCode ?? ZString.Empty;

		public ZString H_22 => messageProvider?.Exporter?.Prefecture ?? ZString.Empty;

		public ZString H_23 => messageProvider?.Exporter?.City ?? ZString.Empty;

		public ZString H_24 => messageProvider?.Exporter?.Street ?? ZString.Empty;

		public ZString H_25 => messageProvider?.Exporter?.AdditionalInformation ?? ZString.Empty;

		public ZString H_26 => messageProvider?.Exporter?.Phone ?? ZString.Empty;

		public ZString H_27 => messageProvider?.AttorneyforCustomsProcedure ?? ZString.Empty;

		public ZString H_28 => messageProvider?.AttorneyforCustomsProcedureAcceptanceNumber ?? ZString.Empty;

		public ZString H_29 => messageProvider?.AttorneyforCustomsProcedureName ?? ZString.Empty;

		public ZString H_30 => messageProvider?.Consignee?.Code ?? ZString.Empty;

		public ZString H_31 => messageProvider?.Consignee?.Name ?? ZString.Empty;

		public ZString H_32 => messageProvider?.Consignee?.PostCode ?? ZString.Empty;

		public ZString H_33 => messageProvider?.Consignee?.Street1 ?? ZString.Empty;

		public ZString H_34 => messageProvider?.Consignee?.Street2 ?? ZString.Empty;

		public ZString H_35 => messageProvider?.Consignee?.City ?? ZString.Empty;

		public ZString H_36 => messageProvider?.Consignee?.State ?? ZString.Empty;

		public ZString H_37 => messageProvider?.Consignee?.CountryCode ?? ZString.Empty;

		public ZString H_38 => messageProvider?.ApprovalDeclarationNumber ?? ZString.Empty;

		public ZString H_39 => messageProvider?.ApprovalDate?.ToNACCSDate() ?? ZString.Empty;

		public ZString H_40 => messageProvider?.MarksNumbers ?? ZString.Empty;

		public ZString H_41 => messageProvider?.ChangeType ?? ZString.Empty;

		public ZString H_42 => messageProvider?.ChangeReason ?? ZString.Empty;

		public ZString H_43 => messageProvider?.BondedTransportationApprovalStartDate?.ToNACCSDate() ?? ZString.Empty;

		public ZString H_44 => messageProvider?.BondedTransportationApprovalEndDate?.ToNACCSDate() ?? ZString.Empty;

		public ZString H_45 => messageProvider?.OriginalVesselCode ?? ZString.Empty;

		public ZString H_46 => messageProvider?.OriginalScheduledVesselName ?? ZString.Empty;

		public ZString H_47 => messageProvider?.AmendedVesselCode ?? ZString.Empty;

		public ZString H_48 => messageProvider?.AmendedScheduledVesselName ?? ZString.Empty;

		public ZString H_49 => messageProvider?.OriginalExportControlNumber ?? ZString.Empty;

		public ZString H_50 => messageProvider?.AmendedExportControlNumber ?? ZString.Empty;

		public ZString H_51 => messageProvider?.OriginalDateofDeparture?.ToNACCSDate() ?? ZString.Empty;

		public ZString H_52 => messageProvider?.AmendedDateofDeparture?.ToNACCSDate() ?? ZString.Empty;

		public ZString H_53 => messageProvider?.OriginalPortofLoading ?? ZString.Empty;

		public ZString H_54 => messageProvider?.OriginalPortofLoadingName ?? ZString.Empty;

		public ZString H_55 => messageProvider?.AmendedPortofLoading ?? ZString.Empty;

		public ZString H_56 => messageProvider?.AmendedPortofLoadingName ?? ZString.Empty;

		public ZString H_57 => messageProvider?.OriginalFinalDestinationCode ?? ZString.Empty;

		public ZString H_58 => messageProvider?.OriginalFinalDestinationName ?? ZString.Empty;

		public ZString H_59 => messageProvider?.AmendedFinalDestinationCode ?? ZString.Empty;

		public ZString H_60 => messageProvider?.AmendedFinalDestinationName ?? ZString.Empty;

		public ZString H_61 => messageProvider?.OriginalVanningLocationCode ?? ZString.Empty;

		public ZString H_62 => messageProvider?.AmendedVanningLocationCode ?? ZString.Empty;

		public ZString H_63 => messageProvider?.OriginalVanningLocationName ?? ZString.Empty;

		public ZString H_64 => messageProvider?.AmendedVanningLocationName ?? ZString.Empty;

		public ZString H_65_1 => OriginalVanningLocationCodeArray.ElementAtOrDefault(0);

		public ZString H_65_2 => OriginalVanningLocationCodeArray.ElementAtOrDefault(1);

		public ZString H_65_3 => OriginalVanningLocationCodeArray.ElementAtOrDefault(2);

		public ZString H_65_4 => OriginalVanningLocationCodeArray.ElementAtOrDefault(3);

		public ZString H_66_1 => AmendedVanningLocationCodeArray.ElementAtOrDefault(0);

		public ZString H_66_2 => AmendedVanningLocationCodeArray.ElementAtOrDefault(1);

		public ZString H_66_3 => AmendedVanningLocationCodeArray.ElementAtOrDefault(2);

		public ZString H_66_4 => AmendedVanningLocationCodeArray.ElementAtOrDefault(3);

		public ZString H_67 => messageProvider?.OriginalCargoQuantity?.Quantity.FormatNumberInDocument() ?? ZString.Empty;

		public ZString H_68 => messageProvider?.OriginalCargoQuantity?.Unit ?? ZString.Empty;

		public ZString H_69 => messageProvider?.AmendedCargoQuantity?.Quantity.FormatNumberInDocument() ?? ZString.Empty;

		public ZString H_70 => messageProvider?.AmendedCargoQuantity?.Unit ?? ZString.Empty;

		public ZString H_71 => messageProvider?.OriginalGrossWeight?.Quantity.FormatNumberInDocument() ?? ZString.Empty;

		public ZString H_72 => messageProvider?.OriginalGrossWeight?.Unit ?? ZString.Empty;

		public ZString H_73 => messageProvider?.AmendedGrossWeight?.Quantity.FormatNumberInDocument() ?? ZString.Empty;

		public ZString H_74 => messageProvider?.AmendedGrossWeight?.Unit ?? ZString.Empty;

		public ZString H_75 => messageProvider?.OriginalInvoiceIncoterm ?? ZString.Empty;

		public ZString H_76 => messageProvider?.OriginalInvoiceCurrency ?? ZString.Empty;

		public ZString H_77 => messageProvider?.OriginalInvoicePrice.FormatNumberInDocument() ?? ZString.Empty;

		public ZString H_78 => messageProvider?.OriginalInvoiceValuationType ?? ZString.Empty;

		public ZString H_79 => messageProvider?.AmendedInvoiceIncoterm ?? ZString.Empty;

		public ZString H_80 => messageProvider?.AmendedInvoiceCurrency ?? ZString.Empty;

		public ZString H_81 => messageProvider?.AmendedInvoicePrice.FormatNumberInDocument() ?? ZString.Empty;

		public ZString H_82 => messageProvider?.AmendedInvoiceValuationType ?? ZString.Empty;

		public ZString H_83 => messageProvider?.OriginalFOBCurrency ?? ZString.Empty;

		public ZString H_84 => messageProvider?.OriginalFOBPrice.FormatNumberInDocument() ?? ZString.Empty;

		public ZString H_85 => messageProvider?.AmendedFOBCurrency ?? ZString.Empty;

		public ZString H_86 => messageProvider?.AmendedFOBPrice.FormatNumberInDocument() ?? ZString.Empty;

		public ZString H_87 => messageProvider?.StorageCustoms ?? ZString.Empty;

		public ZString H_88 => messageProvider?.StorageCustomsDepartment ?? ZString.Empty;

		public ZString H_89 => messageProvider?.CustomsNotes ?? ZString.Empty;

		public ZString H_90 => messageProvider?.CustomsBrokerNotes ?? ZString.Empty;

		public ZString H_91 => messageProvider?.OwnerNotes ?? ZString.Empty;

		public ZString H_92 => messageProvider?.OwnerSectionCode ?? ZString.Empty;

		public ZString H_93 => messageProvider?.OwnerReferenceNumber ?? ZString.Empty;

		public ZString H_94 => messageProvider?.InternalReferenceNumber ?? ZString.Empty;

		public ZString H_95 => messageProvider?.UserReferenceNumber ?? ZString.Empty;

		public ZString H_96 => messageProvider?.ExporterImporter ?? ZString.Empty;

		public ZString H_97 => messageProvider?.ApprovalDate2?.ToNACCSDate() ?? ZString.Empty;

		public ZString H_98 => messageProvider?.ApprovalDeclarationNumber2 ?? ZString.Empty;

		public ZString H_99 => messageProvider?.AmendmentApplicationDeclarationNumber ?? ZString.Empty;

		public ZString H_100 => messageProvider?.CustomsOfficeDirectorName ?? ZString.Empty;

		public ZString H_101 => messageProvider?.ApprovalDateforAmendment?.ToNACCSDate() ?? ZString.Empty;

		protected override ZString GetTemplateTypeCore() => EACNoticeMessageTemplateCodes.GetDescriptionFromCode(TemplateCode);

		public ICodeDescriptionPairList EACNoticeMessageTemplateCodes => Factory.GetCachedValue<EACNoticeMessageTemplateCodeList>();

		public ZString TemplateCode => Factory.GetCachedValue($"EACNoticeMessageDocumentWrapper_TemplateCode_{OutputInformationCode}", () =>
		{
			return (string)OutputInformationCode switch
			{
				JPEACNoticeInformationCodeList.Codes.SAE4431 or JPEACNoticeInformationCodeList.Codes.SAY4431 or JPEACNoticeInformationCodeList.Codes.AAE4431 or JPEACNoticeInformationCodeList.Codes.AAY4431 => EACNoticeMessageTemplateCodeList.Codes.T4431,
				JPEACNoticeInformationCodeList.Codes.SAE4451 or JPEACNoticeInformationCodeList.Codes.SAY4451 or JPEACNoticeInformationCodeList.Codes.AAE4451 or JPEACNoticeInformationCodeList.Codes.AAY4451 => EACNoticeMessageTemplateCodeList.Codes.T4451,
				JPEACNoticeInformationCodeList.Codes.SAE4471 or JPEACNoticeInformationCodeList.Codes.SAY4471 or JPEACNoticeInformationCodeList.Codes.AAE4472 or JPEACNoticeInformationCodeList.Codes.AAY4472 => EACNoticeMessageTemplateCodeList.Codes.T4472,
				JPEACNoticeInformationCodeList.Codes.SAE4491 or JPEACNoticeInformationCodeList.Codes.SAY4491 or JPEACNoticeInformationCodeList.Codes.AAE4491 or JPEACNoticeInformationCodeList.Codes.AAY4491 => EACNoticeMessageTemplateCodeList.Codes.T4491,
				_ => string.Empty,
			};
		});

		protected override ZString GetShipmentTypeCore() => Constants.DocumentMessageCodes.ShipmentTypes.Export;

		string[] OriginalVanningLocationCodeArray => messageProvider?.OriginalVanningLocationCodeList?.ToArray() ?? [];

		string[] AmendedVanningLocationCodeArray => messageProvider?.AmendedVanningLocationCodeList?.ToArray() ?? [];
	}
}
